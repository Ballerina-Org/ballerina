namespace Ballerina.DSL.Next.Types.TypeChecker

module Eval =
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Object
  open Ballerina.State.WithError
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
  open System
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Patterns

  type TypeExpr with
    static member EvalAsSymbol: TypeExprSymbolEval =
      fun loc0 t ->
        state {
          let (!) = TypeExpr.EvalAsSymbol loc0
          let (!!) = TypeExpr.Eval None loc0
          let! ctx = state.GetContext()

          let error e = Errors.Singleton(loc0, e)

          let ofSum (p: Sum<'a, Ballerina.Errors.Errors>) =
            p |> Sum.mapRight (Errors.FromErrors loc0) |> state.OfSum

          match t with
          | TypeExpr.NewSymbol name -> return TypeSymbol.Create(Identifier.LocalScope name)
          | TypeExpr.Lookup v ->

            return!
              reader.Any(
                NonEmptyList.OfList(
                  TypeExprEvalState.tryFindTypeSymbol (v |> ctx.Scope.Resolve, loc0),
                  [ TypeExprEvalState.tryFindRecordFieldSymbol (v |> ctx.Scope.Resolve, loc0)
                    TypeExprEvalState.tryFindUnionCaseSymbol (v |> ctx.Scope.Resolve, loc0) ]
                )
              )
              |> state.OfStateReader
          | TypeExpr.Apply(f, a) ->
            let! f, f_k = !!f
            do! Kind.AsArrow f_k |> ofSum |> state.Ignore
            let! a, a_k = !!a

            let! param, body =
              f
              |> TypeValue.AsLambda
              |> ofSum
              |> state.Map WithTypeExprSourceMapping.Getters.Value

            do! TypeExprEvalState.bindType (param.Name |> Identifier.LocalScope |> ctx.Scope.Resolve) (a, a_k)

            return! !body
          | _ ->
            return!
              $"Error: invalid type expression when evaluating for symbol, got {t}"
              |> error
              |> state.Throw
        }

    static member Eval: TypeExprEval =
      fun n loc0 t ->
        state {
          let (!) = TypeExpr.Eval None loc0
          let (!!) = TypeExpr.EvalAsSymbol loc0
          let! ctx = state.GetContext()

          let error e = Errors.Singleton(loc0, e)

          let ofSum (p: Sum<'a, Ballerina.Errors.Errors>) =
            p |> Sum.mapRight (Errors.FromErrors loc0) |> state.OfSum

          let source =
            match n with
            | Some name -> TypeExprSourceMapping.OriginExprTypeLet(name, t)
            | None -> TypeExprSourceMapping.OriginTypeExpr t

          match t with
          | TypeExpr.Imported i ->
            let! parameters =
              i.Parameters
              |> List.map (fun p -> !(TypeExpr.Lookup(Identifier.LocalScope p.Name)) |> state.Map fst)
              |> state.All

            let! args = i.Arguments |> List.map (fun p -> !p.AsExpr |> state.Map fst) |> state.All

            return
              TypeValue.Imported
                { i with
                    Parameters = []
                    Arguments = parameters @ args },
              Kind.Star
          | TypeExpr.NewSymbol _ -> return! $"Errors cannot evaluate {t} as a type" |> error |> state.Throw
          | TypeExpr.Primitive p -> return TypeValue.Primitive { value = p; source = source }, Kind.Star
          | TypeExpr.Lookup v ->
            return!
              TypeExprEvalState.tryFindType (v |> TypeCheckScope.Empty.Resolve, loc0)
              |> state.OfStateReader
          | TypeExpr.LetSymbols(xts, symbolsKind, rest) ->
            do!
              xts
              |> Seq.map (fun x ->
                state {
                  let x0 = Identifier.LocalScope x
                  // match ctx.Scope.Type with
                  // | Some t -> Identifier.FullyQualified([ t ], x)
                  // | None -> Identifier.LocalScope x

                  let s_x = TypeSymbol.Create(x0)
                  let x = x0 |> ctx.Scope.Resolve

                  do! TypeExprEvalState.bindIdentifierToResolvedIdentifier x x0

                  match ctx.Scope.Type with
                  | Some t ->
                    do!
                      TypeExprEvalState.bindIdentifierToResolvedIdentifier
                        x
                        (Identifier.FullyQualified([ t ], x0.LocalName))
                  | None -> ()

                  // do Console.WriteLine($"Binding symbol {s_x.Name.ToString()} to {x.ToString()}")

                  match symbolsKind with
                  | RecordFields -> do! TypeExprEvalState.bindRecordFieldSymbol x s_x
                  | UnionConstructors -> do! TypeExprEvalState.bindUnionCaseSymbol x s_x

                // do! TypeExprEvalState.bindTypeSymbol x s_x
                })
              |> state.All
              |> state.Ignore

            let! resultValue, resultKind = !rest
            return TypeValue.SetSourceMapping(resultValue, source), resultKind
          | TypeExpr.Let(x, t_x, rest) ->
            let x = Identifier.LocalScope x |> ctx.Scope.Resolve

            return!
              state.Either3
                (state {
                  let! t_x = !t_x
                  do! TypeExprEvalState.bindType x t_x
                  let! resultValue, resultKind = !rest
                  return TypeValue.SetSourceMapping(resultValue, source), resultKind
                })
                (state {
                  let! s_x = !!t_x
                  do! TypeExprEvalState.bindTypeSymbol x s_x
                  let! resultValue, resultKind = !rest
                  return TypeValue.SetSourceMapping(resultValue, source), resultKind
                })
                (state { return! $"Error: cannot evaluate let binding {x}" |> error |> state.Throw }
                 |> state.MapError(Errors.SetPriority ErrorPriority.High))

          | TypeExpr.Apply(f, a) ->
            let! f, f_k = !f
            let! f_k_i, f_k_o = f_k |> Kind.AsArrow |> ofSum

            return!
              state.Either3
                (state {
                  let! param, body =
                    f
                    |> TypeValue.AsLambda
                    |> ofSum
                    |> state.Map WithTypeExprSourceMapping.Getters.Value

                  match param.Kind with
                  | Kind.Symbol ->
                    let! a = !!a

                    do!
                      TypeExprEvalState.bindTypeSymbol
                        (param.Name |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve)
                        a

                    let! resultValue, resultKind = !body
                    return TypeValue.SetSourceMapping(resultValue, source), resultKind
                  | _ ->
                    let! a = !a

                    do!
                      TypeExprEvalState.bindType (param.Name |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve) a

                    let! resultValue, resultKind = !body
                    return TypeValue.SetSourceMapping(resultValue, source), resultKind
                })
                (state {
                  let! f_var = f |> TypeValue.AsVar |> ofSum

                  let! a, a_k = !a

                  if f_k_i <> a_k then
                    return!
                      $"Error: mismatched kind, expected {f_k_i} but got {a_k}"
                      |> error
                      |> state.Throw
                  else
                    return TypeValue.Apply { value = (f_var, a); source = source }, f_k_o
                })
                (state { return! $"Error: cannot evaluate application " |> error |> state.Throw }
                 |> state.MapError(Errors.SetPriority ErrorPriority.High))
          | TypeExpr.Lambda(param, bodyExpr) ->
            let fresh_var_t =
              TypeValue.Var(
                { TypeVar.Name = param.Name
                  Guid = Guid.CreateVersion7() }
              )

            do!
              TypeExprEvalState.bindType
                (param.Name |> Identifier.LocalScope |> ctx.Scope.Resolve)
                (fresh_var_t, param.Kind)

            let! _body, body_k = !bodyExpr
            do! TypeExprEvalState.unbindType (param.Name |> Identifier.LocalScope |> ctx.Scope.Resolve)

            return
              TypeValue.Lambda
                { value = (param, bodyExpr)
                  source = source },
              Kind.Arrow(param.Kind, body_k)
          | TypeExpr.Arrow(input, output) ->
            let! input, input_k = !input
            let! output, output_k = !output
            do! input_k |> Kind.AsStar |> ofSum |> state.Ignore
            do! output_k |> Kind.AsStar |> ofSum |> state.Ignore

            return
              TypeValue.Arrow
                { value = (input, output)
                  source = source },
              Kind.Star
          | TypeExpr.Record(fields) ->
            let! fields =
              fields
              |> Seq.map (fun (k, v) ->
                state {
                  let! k = !!k
                  let! v, v_k = !v
                  do! v_k |> Kind.AsStar |> ofSum |> state.Ignore
                  return (k, v)
                })
              |> state.All
              |> state.Map(OrderedMap.ofSeq)

            return TypeValue.Record { value = fields; source = source }, Kind.Star
          | TypeExpr.Tuple(items) ->
            let! items =
              items
              |> List.map (fun i ->
                state {
                  let! i, i_k = !i
                  do! i_k |> Kind.AsStar |> ofSum |> state.Ignore
                  return i
                })
              |> state.All

            return TypeValue.Tuple { value = items; source = source }, Kind.Star
          | TypeExpr.Union(cases) ->
            let! cases =
              cases
              |> Seq.map (fun (k, v) ->
                state {
                  let! k = !!k
                  let! v, v_k = !v
                  do! v_k |> Kind.AsStar |> ofSum |> state.Ignore
                  return (k, v)
                })
              |> state.All
              |> state.Map(OrderedMap.ofSeq)

            return TypeValue.Union { value = cases; source = source }, Kind.Star
          | TypeExpr.Set(element) ->
            let! element, element_k = !element
            do! element_k |> Kind.AsStar |> ofSum |> state.Ignore
            return TypeValue.Set { value = element; source = source }, Kind.Star
          | TypeExpr.Map(key, value) ->
            let! key, key_k = !key
            let! value, value_k = !value
            do! key_k |> Kind.AsStar |> ofSum |> state.Ignore
            do! value_k |> Kind.AsStar |> ofSum |> state.Ignore

            return
              TypeValue.Map
                { value = (key, value)
                  source = source },
              Kind.Star
          | TypeExpr.KeyOf(arg) ->
            let! arg, arg_k = !arg
            do! arg_k |> Kind.AsStar |> ofSum |> state.Ignore

            let! cases =
              arg
              |> TypeValue.AsRecord
              |> ofSum
              |> state.Map WithTypeExprSourceMapping.Getters.Value

            let mappedCasesFixThis =
              cases
              |> OrderedMap.map (fun _ _ -> TypeValue.CreatePrimitive PrimitiveType.Unit)

            return
              TypeValue.Union
                { value = mappedCasesFixThis
                  source = source },
              Kind.Star
          | TypeExpr.Sum(variants) ->
            let! variants =
              variants
              |> List.map (fun i ->
                state {
                  let! i, i_k = !i
                  do! i_k |> Kind.AsStar |> ofSum |> state.Ignore
                  return i
                })
              |> state.All

            return TypeValue.Sum { value = variants; source = source }, Kind.Star
          | TypeExpr.Flatten(type1, type2) ->
            let! type1, type1_k = !type1
            let! type2, type2_k = !type2
            do! type1_k |> Kind.AsStar |> ofSum |> state.Ignore
            do! type2_k |> Kind.AsStar |> ofSum |> state.Ignore

            return!
              state.Either3
                (state {
                  let! cases1 =
                    type1
                    |> TypeValue.AsUnion
                    |> ofSum
                    |> state.Map WithTypeExprSourceMapping.Getters.Value

                  let! cases2 =
                    type2
                    |> TypeValue.AsUnion
                    |> ofSum
                    |> state.Map WithTypeExprSourceMapping.Getters.Value

                  let cases1 = cases1 |> OrderedMap.toSeq
                  let keys1 = cases1 |> Seq.map fst |> Set.ofSeq

                  let cases2 = cases2 |> OrderedMap.toSeq
                  let keys2 = cases2 |> Seq.map fst |> Set.ofSeq

                  if keys1 |> Set.intersect keys2 |> Set.isEmpty then
                    let cases = cases1 |> Seq.append cases2 |> OrderedMap.ofSeq
                    return TypeValue.Union { value = cases; source = source }, Kind.Star
                  else
                    return!
                      $"Error: cannot flatten types with overlapping keys: {keys1} and {keys2}"
                      |> error
                      |> state.Throw
                })
                (state {
                  let! fields1 =
                    type1
                    |> TypeValue.AsRecord
                    |> ofSum
                    |> state.Map WithTypeExprSourceMapping.Getters.Value

                  let! fields2 =
                    type2
                    |> TypeValue.AsRecord
                    |> ofSum
                    |> state.Map WithTypeExprSourceMapping.Getters.Value

                  let fields1 = fields1 |> OrderedMap.toSeq
                  let keys1 = fields1 |> Seq.map fst |> Set.ofSeq

                  let fields2 = fields2 |> OrderedMap.toSeq
                  let keys2 = fields2 |> Seq.map fst |> Set.ofSeq

                  if keys1 |> Set.intersect keys2 |> Set.isEmpty then
                    let fields = fields1 |> Seq.append fields2 |> OrderedMap.ofSeq
                    return TypeValue.Record { value = fields; source = source }, Kind.Star
                  else
                    return!
                      $"Error: cannot flatten types with overlapping keys: {keys1} and {keys2}"
                      |> error
                      |> state.Throw
                })
                (state { return! $"Error: cannot evaluate flattening " |> error |> state.Throw }
                 |> state.MapError(Errors.SetPriority ErrorPriority.High))
          | TypeExpr.Exclude(type1, type2) ->
            let! type1, type1_k = !type1
            let! type2, type2_k = !type2
            do! type1_k |> Kind.AsStar |> ofSum |> state.Ignore
            do! type2_k |> Kind.AsStar |> ofSum |> state.Ignore

            return!
              state.Either3
                (state {
                  let! cases1 =
                    type1
                    |> TypeValue.AsUnion
                    |> ofSum
                    |> state.Map WithTypeExprSourceMapping.Getters.Value

                  let! cases2 =
                    type2
                    |> TypeValue.AsUnion
                    |> ofSum
                    |> state.Map WithTypeExprSourceMapping.Getters.Value

                  let keys2 = cases2 |> OrderedMap.keys |> Set.ofSeq
                  let cases = cases1 |> OrderedMap.filter (fun k _ -> keys2 |> Set.contains k |> not)
                  return TypeValue.Union { value = cases; source = source }, Kind.Star
                })
                (state {
                  let! fields1 =
                    type1
                    |> TypeValue.AsRecord
                    |> ofSum
                    |> state.Map WithTypeExprSourceMapping.Getters.Value

                  let! fields2 =
                    type2
                    |> TypeValue.AsRecord
                    |> ofSum
                    |> state.Map WithTypeExprSourceMapping.Getters.Value

                  let keys2 = fields2 |> OrderedMap.keys |> Set.ofSeq

                  let fields =
                    fields1 |> OrderedMap.filter (fun k _ -> keys2 |> Set.contains k |> not)

                  return TypeValue.Record { value = fields; source = source }, Kind.Star
                })
                (state { return! $"Error: cannot evaluate exclude " |> error |> state.Throw }
                 |> state.MapError(Errors.SetPriority ErrorPriority.High))
          | TypeExpr.Rotate(t) ->
            let! t, t_k = !t
            do! t_k |> Kind.AsStar |> ofSum |> state.Ignore

            return!
              state.Either3
                (state {
                  let! cases = t |> TypeValue.AsUnion |> ofSum

                  return TypeValue.Record { value = cases.value; source = source }, Kind.Star
                })
                (state {
                  let! fields = t |> TypeValue.AsRecord |> ofSum

                  return
                    TypeValue.Union
                      { value = fields.value
                        source = source },
                    Kind.Star
                })
                (state { return! $"Error: cannot evaluate rotation" |> error |> state.Throw }
                 |> state.MapError(Errors.SetPriority ErrorPriority.High))
        }

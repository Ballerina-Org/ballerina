namespace Ballerina.DSL.Next.Types.TypeChecker

module Eval =
  open System
  open Ballerina.Fun
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Object
  open Ballerina.State.WithError
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.Collections.Map
  open System
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.KindEval
  open Ballerina.DSL.Next.Unification

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
                  TypeCheckState.tryFindTypeSymbol (v |> ctx.Scope.Resolve, loc0),
                  [ TypeCheckState.tryFindRecordFieldSymbol (v |> ctx.Scope.Resolve, loc0)
                    TypeCheckState.tryFindUnionCaseSymbol (v |> ctx.Scope.Resolve, loc0) ]
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

            let! ctx = state.GetContext()
            let closure = ctx.TypeVariables |> Map.add (param.Name) (a, a_k)

            return!
              !body
              |> state.MapContext(TypeCheckContext.Updaters.TypeVariables(replaceWith closure))
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
          | TypeExpr.FromTypeValue tv ->
            // do Console.WriteLine($"Instantiating type value {tv}")
            let! ctx = state.GetContext()
            let! s = state.GetState()
            let scope = ctx.TypeVariables |> Map.map (fun _ (_, k) -> k)
            let scope = Map.merge (fun _ -> id) scope ctx.TypeParameters
            let! k = TypeValue.KindEval n loc0 tv |> state.MapContext(fun _ -> scope)

            // do Console.WriteLine($"Instantiating type value {tv} with kind {k}")

            // do
            //   Console.WriteLine(
            //     $"Eval context is {ctx.TypeVariables.ToFSharpString}, {ctx.TypeParameters.ToFSharpString}"
            //   )

            let! tv =
              tv
              |> TypeValue.Instantiate TypeExpr.Eval loc0
              |> State.Run(TypeInstantiateContext.FromEvalContext(ctx), s)
              |> sum.Map fst
              |> sum.MapError fst
              |> state.OfSum

            // do Console.WriteLine($"Instantiated type value to {tv}")

            return TypeValue.SetSourceMapping(tv, source), k
          | TypeExpr.Imported i ->
            // let! ctx = state.GetContext()

            // do
            //   Console.WriteLine(
            //     $"Importing type {i} with context {ctx.TypeVariables.ToFSharpString} and {ctx.TypeParameters.ToFSharpString}"
            //   )

            // do Console.ReadLine() |> ignore

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
            let! res =
              state.Any(
                NonEmptyList.OfList(
                  TypeCheckContext.tryFindTypeVariable (v.LocalName, loc0) |> state.OfReader,
                  [ TypeCheckContext.tryFindTypeParameter (v.LocalName, loc0)
                    |> reader.Map(fun k -> TypeValue.Lookup v, k)
                    |> state.OfReader
                    TypeCheckState.tryFindType (v |> TypeCheckScope.Empty.Resolve, loc0)
                    |> state.OfStateReader

                    state {
                      let! c = state.GetContext()

                      return!
                        $"Error: cannot find type for {v} with context ({c.TypeVariables.AsFSharpString})"
                        |> error
                        |> state.Throw
                        |> state.MapError(Errors.SetPriority(ErrorPriority.High))
                    } ]
                )
              )
              |> state.MapError(Errors.FilterHighestPriorityOnly)

            // do Console.WriteLine($"Lookup {v} resolved to {res}")

            return res

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

                  do! TypeCheckState.bindIdentifierToResolvedIdentifier x x0

                  match ctx.Scope.Type with
                  | Some t ->
                    do!
                      TypeCheckState.bindIdentifierToResolvedIdentifier
                        x
                        (Identifier.FullyQualified([ t ], x0.LocalName))
                  | None -> ()

                  // do Console.WriteLine($"Binding symbol {s_x.Name.ToString()} to {x.ToString()}")

                  match symbolsKind with
                  | RecordFields -> do! TypeCheckState.bindRecordFieldSymbol x s_x
                  | UnionConstructors -> do! TypeCheckState.bindUnionCaseSymbol x s_x

                })
              |> state.All
              |> state.Ignore

            let! resultValue, resultKind = !rest
            return TypeValue.SetSourceMapping(resultValue, source), resultKind
          | TypeExpr.Let(x, t_x, rest) ->

            return!
              state.Either3
                (state {
                  let! t_x = !t_x

                  let! resultValue, resultKind =
                    !rest
                    |> state.MapContext(TypeCheckContext.Updaters.TypeVariables(Map.add x t_x))

                  return TypeValue.SetSourceMapping(resultValue, source), resultKind
                })
                (state {
                  let! s_x = !!t_x
                  let x = Identifier.LocalScope x |> ctx.Scope.Resolve
                  do! TypeCheckState.bindTypeSymbol x s_x
                  let! resultValue, resultKind = !rest
                  return TypeValue.SetSourceMapping(resultValue, source), resultKind
                })
                (state { return! $"Error: cannot evaluate let binding {x}" |> error |> state.Throw }
                 |> state.MapError(Errors.SetPriority ErrorPriority.High))

          | TypeExpr.Apply(f, a) ->
            // do Console.WriteLine($"Evaluating type application of {f} to {a}")
            // do Console.WriteLine($"Evaluating type application of {f.ToFSharpString}")
            // do Console.ReadLine() |> ignore
            let! f, f_k = !f
            // do Console.WriteLine($"Evaluated function part to {f.ToFSharpString}\n{f_k}")
            // do Console.ReadLine() |> ignore
            let! f_k_i, f_k_o = f_k |> Kind.AsArrow |> ofSum

            return!
              state.Either5
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
                      TypeCheckState.bindTypeSymbol
                        (param.Name |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve)
                        a

                    let! resultValue, resultKind = !body
                    return TypeValue.SetSourceMapping(resultValue, source), resultKind
                  | _ ->
                    let! a = !a

                    let! resultValue, resultKind =
                      !body
                      |> state.MapContext(TypeCheckContext.Updaters.TypeVariables(Map.add param.Name a))

                    return TypeValue.SetSourceMapping(resultValue, source), resultKind
                })
                (state {
                  let! f_i = f |> TypeValue.AsImported |> ofSum

                  // let! ctx = state.GetContext()
                  // do Console.WriteLine($"Evaluating argument part {a.ToFSharpString}")
                  // do Console.WriteLine($"Context is {ctx.TypeParameters.ToFSharpString}")
                  // do Console.ReadLine() |> ignore
                  let! a, a_k = !a
                  // do Console.WriteLine($"aka {a.ToFSharpString}")
                  // do Console.ReadLine() |> ignore

                  if a_k <> f_k_i then
                    return!
                      $"Error: cannot apply type {f} of input kind {f_k_i} to argument of kind {a_k}"
                      |> error
                      |> state.Throw
                  else
                    match f_i.Parameters with
                    | [] ->
                      return!
                        $"Error: cannot apply imported type {f_i.Id.Name} with no parameters"
                        |> error
                        |> state.Throw
                    | _ :: ps ->
                      return
                        TypeValue.Imported
                          { f_i with
                              Parameters = ps
                              Arguments = f_i.Arguments @ [ a ] },
                        f_k_o
                })
                (state {
                  let! f_l = f |> TypeValue.AsLookup |> ofSum

                  let! a, a_k = !a

                  if a_k <> f_k_i then
                    return!
                      $"Error: cannot apply type {f} of input kind {f_k_i} to argument of kind {a_k}"
                      |> error
                      |> state.Throw
                  else
                    return TypeValue.CreateApplication(SymbolicTypeApplication.Lookup(f_l, a)), f_k_o
                })
                (state {
                  let! { value = f_app } = f |> TypeValue.AsApplication |> ofSum

                  let! a, a_k = !a

                  if a_k <> f_k_i then
                    return!
                      $"Error: cannot apply type {f} of input kind {f_k_i} to argument of kind {a_k}"
                      |> error
                      |> state.Throw
                  else
                    return TypeValue.CreateApplication(SymbolicTypeApplication.Application(f_app, a)), f_k_o
                })
                (state { return! $"Error: cannot evaluate type application {t}" |> error |> state.Throw }
                 |> state.MapError(Errors.SetPriority ErrorPriority.High))
              |> state.MapError(Errors.FilterHighestPriorityOnly)
          | TypeExpr.Lambda(param, bodyExpr) ->
            // let fresh_var_t =
            //   TypeValue.Var(
            //     { TypeVar.Name = param.Name
            //       Guid = Guid.CreateVersion7() }
            //   )

            // let! ctx = state.GetContext()
            // let closure = ctx.TypeVariables
            // let closure = ctx.TypeVariables |> Map.add param.Name (fresh_var_t, param.Kind)
            // do Console.WriteLine($"Type lambda closure: {closure.ToFSharpString}")
            // do Console.ReadLine() |> ignore

            // do Console.WriteLine($"Evaluating type lambda with param {param} and body {bodyExpr}")

            let! body_t, body_k =
              !bodyExpr
              // |> TypeExpr.KindEval n loc0
              |> state.MapContext(
                TypeCheckContext.Updaters.TypeParameters(Map.add param.Name param.Kind)
                >> TypeCheckContext.Updaters.TypeVariables(Map.remove param.Name)
              )

            // do Console.WriteLine($"Evaluated type lambda body to {body_t}")
            // do Console.ReadLine() |> ignore

            // |> state.MapContext(TypeExprEvalContext.Updaters.TypeVariables(replaceWith closure))

            return
              TypeValue.Lambda
                { value = param, body_t.AsExpr
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
                  // do! v_k |> Kind.AsStar |> ofSum |> state.Ignore
                  return (k, (v, v_k))
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

            let! cases = arg |> TypeValue.AsRecord |> ofSum

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
                  let! cases1 = type1 |> TypeValue.AsUnion |> sum.Map snd |> ofSum

                  let! cases2 = type2 |> TypeValue.AsUnion |> sum.Map snd |> ofSum

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
                  let! fields1 = type1 |> TypeValue.AsRecord |> ofSum

                  let! fields2 = type2 |> TypeValue.AsRecord |> ofSum

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
                  let! cases1 = type1 |> TypeValue.AsUnion |> sum.Map snd |> ofSum

                  let! cases2 = type2 |> TypeValue.AsUnion |> sum.Map snd |> ofSum

                  let keys2 = cases2 |> OrderedMap.keys |> Set.ofSeq
                  let cases = cases1 |> OrderedMap.filter (fun k _ -> keys2 |> Set.contains k |> not)
                  return TypeValue.Union { value = cases; source = source }, Kind.Star
                })
                (state {
                  let! fields1 = type1 |> TypeValue.AsRecord |> ofSum

                  let! fields2 = type2 |> TypeValue.AsRecord |> ofSum

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
                  let! cases = t |> TypeValue.AsUnion |> sum.Map snd |> ofSum
                  let cases = cases |> OrderedMap.map (fun _ v -> v, Kind.Star)

                  return TypeValue.Record { value = cases; source = source }, Kind.Star
                })
                (state {
                  let! fields = t |> TypeValue.AsRecord |> ofSum

                  let! _ =
                    fields
                    |> OrderedMap.map (fun _ (_, k) -> k |> Kind.AsStar |> ofSum |> state.Ignore)
                    |> OrderedMap.toList
                    |> List.map snd
                    |> state.All

                  return
                    TypeValue.Union
                      { value = fields |> OrderedMap.map (fun _ (v, _) -> v)
                        source = source },
                    Kind.Star
                })
                (state { return! $"Error: cannot evaluate rotation" |> error |> state.Throw }
                 |> state.MapError(Errors.SetPriority ErrorPriority.High))
        }

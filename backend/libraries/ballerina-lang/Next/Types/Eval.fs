namespace Ballerina.DSL.Next.Types

module Eval =
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Object
  open Ballerina.State.WithError
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open System
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns

  type TypeBindings = Map<Identifier, TypeValue * Kind>

  type TypeSymbols = Map<Identifier, TypeSymbol>

  type TypeExprEvalContext = { Scope: List<string> }

  type TypeExprEvalState =
    { Bindings: TypeBindings
      Symbols: TypeSymbols }

  type TypeExprEvalResult = State<TypeValue * Kind, TypeExprEvalContext, TypeExprEvalState, Errors>
  type TypeExprEval = TypeExpr -> TypeExprEvalResult
  type TypeExprSymbolEvalResult = State<TypeSymbol, TypeExprEvalContext, TypeExprEvalState, Errors>
  type TypeExprSymbolEval = TypeExpr -> TypeExprSymbolEvalResult

  type TypeExprEvalContext with
    static member Empty: TypeExprEvalContext = { Scope = [] }

    static member Updaters =
      {| Scope = fun u (c: TypeExprEvalContext) -> { c with Scope = c.Scope |> u } |}

  type TypeExprEvalState with
    static member Empty: TypeExprEvalState =
      { Bindings = Map.empty
        Symbols = Map.empty }

    static member Create(bindings: TypeBindings, symbols: TypeSymbols) : TypeExprEvalState =
      { TypeExprEvalState.Empty with
          Bindings = bindings
          Symbols = symbols }

    static member CreateFromSymbols(symbols: TypeSymbols) : TypeExprEvalState =
      { TypeExprEvalState.Empty with
          Symbols = symbols }

    static member tryFindType(v: Identifier) : Reader<TypeValue * Kind, TypeExprEvalState, Errors> =
      reader {
        let! s = reader.GetContext()
        return! s.Bindings |> Map.tryFindWithError v "bindings" v.ToFSharpString |> reader.OfSum
      }

    static member tryFindSymbol(v: Identifier) : Reader<TypeSymbol, TypeExprEvalState, Errors> =
      reader {
        let! s = reader.GetContext()
        return! s.Symbols |> Map.tryFindWithError v "symbols" v.ToFSharpString |> reader.OfSum
      }

    static member Updaters =
      {| Bindings = fun u (c: TypeExprEvalState) -> { c with Bindings = c.Bindings |> u }
         Symbols = fun u (c: TypeExprEvalState) -> { c with Symbols = c.Symbols |> u } |}

    static member unbindType x =
      state {
        let! ctx = state.GetContext()
        do! state.SetState(TypeExprEvalState.Updaters.Bindings(Map.remove (Identifier.LocalScope x)))
        do! state.SetState(TypeExprEvalState.Updaters.Bindings(Map.remove (Identifier.FullyQualified(ctx.Scope, x))))
      }

    static member bindType x t_x =
      state {
        let! ctx = state.GetContext()
        do! state.SetState(TypeExprEvalState.Updaters.Bindings(Map.add (Identifier.LocalScope x) t_x))

        do! state.SetState(TypeExprEvalState.Updaters.Bindings(Map.add (Identifier.FullyQualified(ctx.Scope, x)) t_x))
      }

    static member bindSymbol x t_x =
      state {
        let! ctx = state.GetContext()
        do! state.SetState(TypeExprEvalState.Updaters.Symbols(Map.add (Identifier.LocalScope x) t_x))

        do! state.SetState(TypeExprEvalState.Updaters.Symbols(Map.add (Identifier.FullyQualified(ctx.Scope, x)) t_x))
      }



  type TypeExpr with
    static member EvalAsSymbol: TypeExprSymbolEval =
      fun t ->
        state {
          let (!) = TypeExpr.EvalAsSymbol
          let (!!) = TypeExpr.Eval

          match t with
          | TypeExpr.NewSymbol name -> return TypeSymbol.Create(Identifier.LocalScope name)
          | TypeExpr.Lookup v -> return! TypeExprEvalState.tryFindSymbol v |> state.OfStateReader
          | TypeExpr.Apply(f, a) ->
            let! f, f_k = !!f
            do! Kind.AsArrow f_k |> state.OfSum |> state.Ignore
            let! a, a_k = !!a
            let! param, body = f |> TypeValue.AsLambda |> state.OfSum
            do! TypeExprEvalState.bindType param.Name (a, a_k)

            return! !body
          | _ ->
            return!
              $"Error: invalid type expression when evaluating for symbol, got {t}"
              |> Errors.Singleton
              |> state.Throw
        }

    static member Eval: TypeExprEval =
      fun t ->
        state {
          let (!) = TypeExpr.Eval
          let (!!) = TypeExpr.EvalAsSymbol

          match t with
          | TypeExpr.NewSymbol _ -> return! $"Errors cannot evaluate {t} as a type" |> Errors.Singleton |> state.Throw
          | TypeExpr.Primitive p -> return TypeValue.Primitive p, Kind.Star
          | TypeExpr.Lookup v -> return! TypeExprEvalState.tryFindType v |> state.OfStateReader
          | TypeExpr.Let(x, t_x, rest) ->
            return!
              state.Either
                (state {
                  let! t_x = !t_x
                  do! TypeExprEvalState.bindType x t_x
                  return! !rest
                })
                (state {
                  let! s_x = !!t_x
                  do! TypeExprEvalState.bindSymbol x s_x
                  return! !rest
                })

          | TypeExpr.Apply(f, a) ->
            let! f, f_k = !f
            let! f_k_i, f_k_o = f_k |> Kind.AsArrow |> state.OfSum

            return!
              state.Either
                (state {
                  let! param, body = f |> TypeValue.AsLambda |> state.OfSum

                  match param.Kind with
                  | Kind.Symbol ->
                    let! a = !!a
                    do! TypeExprEvalState.bindSymbol param.Name a

                    return! !body
                  | _ ->
                    let! a = !a

                    do! TypeExprEvalState.bindType param.Name a

                    return! !body
                })
                (state {
                  let! f_var = f |> TypeValue.AsVar |> state.OfSum

                  let! a, a_k = !a

                  if f_k_i <> a_k then
                    return!
                      $"Error: mismatched kind, expected {f_k_i} but got {a_k}"
                      |> Errors.Singleton
                      |> state.Throw
                  else
                    return TypeValue.Apply(f_var, a), f_k_o

                })
          | TypeExpr.Lambda(param, body) ->
            let fresh_var_t =
              TypeValue.Var(
                { TypeVar.Name = param.Name
                  Guid = Guid.CreateVersion7() }
              )

            do! TypeExprEvalState.bindType param.Name (fresh_var_t, param.Kind)
            let! body, body_k = !body
            do! TypeExprEvalState.unbindType param.Name
            return TypeValue.Lambda(param, body.AsExpr), Kind.Arrow(param.Kind, body_k)
          | TypeExpr.Arrow(input, output) ->
            let! input, input_k = !input
            let! output, output_k = !output
            do! input_k |> Kind.AsStar |> state.OfSum |> state.Ignore
            do! output_k |> Kind.AsStar |> state.OfSum |> state.Ignore
            return TypeValue.Arrow(input, output), Kind.Star
          | TypeExpr.Record(fields) ->
            let! fields =
              fields
              |> Seq.map (fun (k, v) ->
                state {
                  let! k = !!k
                  let! v, v_k = !v
                  do! v_k |> Kind.AsStar |> state.OfSum |> state.Ignore
                  return (k, v)
                })
              |> state.All
              |> state.Map(Map.ofSeq)

            return TypeValue.Record(fields), Kind.Star
          | TypeExpr.Tuple(items) ->
            let! items =
              items
              |> List.map (fun i ->
                state {
                  let! i, i_k = !i
                  do! i_k |> Kind.AsStar |> state.OfSum |> state.Ignore
                  return i
                })
              |> state.All

            return TypeValue.Tuple(items), Kind.Star
          | TypeExpr.Union(cases) ->
            let! cases =
              cases
              |> Seq.map (fun (k, v) ->
                state {
                  let! k = !!k
                  let! v, v_k = !v
                  do! v_k |> Kind.AsStar |> state.OfSum |> state.Ignore
                  return (k, v)
                })
              |> state.All
              |> state.Map(Map.ofSeq)

            return TypeValue.Union(cases), Kind.Star
          | TypeExpr.List(element) ->
            let! element, element_k = !element
            do! element_k |> Kind.AsStar |> state.OfSum |> state.Ignore
            return TypeValue.List(element), Kind.Star
          | TypeExpr.Set(element) ->
            let! element, element_k = !element
            do! element_k |> Kind.AsStar |> state.OfSum |> state.Ignore
            return TypeValue.Set(element), Kind.Star
          | TypeExpr.Map(key, value) ->
            let! key, key_k = !key
            let! value, value_k = !value
            do! key_k |> Kind.AsStar |> state.OfSum |> state.Ignore
            do! value_k |> Kind.AsStar |> state.OfSum |> state.Ignore
            return TypeValue.Map(key, value), Kind.Star
          | TypeExpr.KeyOf(arg) ->
            let! arg, arg_k = !arg
            do! arg_k |> Kind.AsStar |> state.OfSum |> state.Ignore
            let! cases = arg |> TypeValue.AsRecord |> state.OfSum

            return
              (cases
               |> Map.map (fun _ _ -> TypeValue.Primitive(PrimitiveType.Unit))
               |> TypeValue.Union,
               Kind.Star)
          | TypeExpr.Sum(variants) ->
            let! variants =
              variants
              |> List.map (fun i ->
                state {
                  let! i, i_k = !i
                  do! i_k |> Kind.AsStar |> state.OfSum |> state.Ignore
                  return i
                })
              |> state.All

            return TypeValue.Sum(variants), Kind.Star
          | TypeExpr.Flatten(type1, type2) ->
            let! type1, type1_k = !type1
            let! type2, type2_k = !type2
            do! type1_k |> Kind.AsStar |> state.OfSum |> state.Ignore
            do! type2_k |> Kind.AsStar |> state.OfSum |> state.Ignore

            return!
              state.Either
                (state {
                  let! cases1 = type1 |> TypeValue.AsUnion |> state.OfSum
                  let! cases2 = type2 |> TypeValue.AsUnion |> state.OfSum

                  let cases1 = cases1 |> Map.toSeq
                  let keys1 = cases1 |> Seq.map fst |> Set.ofSeq

                  let cases2 = cases2 |> Map.toSeq
                  let keys2 = cases2 |> Seq.map fst |> Set.ofSeq

                  if keys1 |> Set.intersect keys2 |> Set.isEmpty then
                    return cases1 |> Seq.append cases2 |> Map.ofSeq |> TypeValue.Union, Kind.Star
                  else
                    return!
                      $"Error: cannot flatten types with overlapping keys: {keys1} and {keys2}"
                      |> Errors.Singleton
                      |> state.Throw
                })
                (state {
                  let! fields1 = type1 |> TypeValue.AsRecord |> state.OfSum
                  let! fields2 = type2 |> TypeValue.AsRecord |> state.OfSum

                  let fields1 = fields1 |> Map.toSeq
                  let keys1 = fields1 |> Seq.map fst |> Set.ofSeq

                  let fields2 = fields2 |> Map.toSeq
                  let keys2 = fields2 |> Seq.map fst |> Set.ofSeq

                  if keys1 |> Set.intersect keys2 |> Set.isEmpty then
                    return fields1 |> Seq.append fields2 |> Map.ofSeq |> TypeValue.Record, Kind.Star
                  else
                    return!
                      $"Error: cannot flatten types with overlapping keys: {keys1} and {keys2}"
                      |> Errors.Singleton
                      |> state.Throw
                })
          | TypeExpr.Exclude(type1, type2) ->
            let! type1, type1_k = !type1
            let! type2, type2_k = !type2
            do! type1_k |> Kind.AsStar |> state.OfSum |> state.Ignore
            do! type2_k |> Kind.AsStar |> state.OfSum |> state.Ignore

            return!
              state.Either
                (state {
                  let! cases1 = type1 |> TypeValue.AsUnion |> state.OfSum
                  let! cases2 = type2 |> TypeValue.AsUnion |> state.OfSum
                  let keys2 = cases2 |> Map.keys |> Set.ofSeq

                  return
                    cases1
                    |> Map.filter (fun k _ -> keys2 |> Set.contains k |> not)
                    |> TypeValue.Union,
                    Kind.Star
                })
                (state {
                  let! fields1 = type1 |> TypeValue.AsRecord |> state.OfSum
                  let! fields2 = type2 |> TypeValue.AsRecord |> state.OfSum
                  let keys2 = fields2 |> Map.keys |> Set.ofSeq

                  return
                    fields1
                    |> Map.filter (fun k _ -> keys2 |> Set.contains k |> not)
                    |> TypeValue.Record,
                    Kind.Star
                })
          | TypeExpr.Rotate(t) ->
            let! t, t_k = !t
            do! t_k |> Kind.AsStar |> state.OfSum |> state.Ignore

            return!
              state.Either
                (state {
                  let! cases = t |> TypeValue.AsUnion |> state.OfSum

                  return cases |> TypeValue.Record, Kind.Star
                })
                (state {
                  let! fields = t |> TypeValue.AsRecord |> state.OfSum

                  return fields |> TypeValue.Union, Kind.Star
                })
        }

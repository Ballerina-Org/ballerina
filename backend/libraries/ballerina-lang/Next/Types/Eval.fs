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

  type TypeBindings = Map<Identifier, TypeValue>

  type TypeSymbols = Map<Identifier, TypeSymbol>

  type TypeExprEvalContext = { Scope: List<string> }

  type TypeExprEvalState =
    { Bindings: TypeBindings
      Symbols: TypeSymbols }

  type TypeExprEvalResult = State<TypeValue, TypeExprEvalContext, TypeExprEvalState, Errors>
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

    static member tryFindType(v: Identifier) : Reader<TypeValue, TypeExprEvalState, Errors> =
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
          | TypeExpr.NewSymbol name -> return TypeSymbol.Create name
          | TypeExpr.Lookup v -> return! TypeExprEvalState.tryFindSymbol v |> state.OfStateReader
          | TypeExpr.Apply(f, a) ->
            let! f = !!f
            let! a = !!a
            let! param, body = f |> TypeValue.AsLambda |> state.OfSum
            do! TypeExprEvalState.bindType param.Name a

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
          | TypeExpr.Primitive p -> return TypeValue.Primitive p
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
            let! f = !f

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

                  let! a = !a

                  return TypeValue.Apply(f_var, a)

                })
          | TypeExpr.Lambda(param, body) -> return TypeValue.Lambda(param, body)
          | TypeExpr.Arrow(input, output) ->
            let! input = !input
            let! output = !output
            return TypeValue.Arrow(input, output)
          | TypeExpr.Record(fields) ->
            let! fields =
              fields
              |> Seq.map (fun (k, v) ->
                state {
                  let! k = !!k
                  let! v = !v
                  return (k, v)
                })
              |> state.All
              |> state.Map(Map.ofSeq)

            return TypeValue.Record(fields)
          | TypeExpr.Tuple(items) ->
            let! items = items |> List.map (!) |> state.All
            return TypeValue.Tuple(items)
          | TypeExpr.Union(cases) ->
            let! cases =
              cases
              |> Seq.map (fun (k, v) ->
                state {
                  let! k = !!k
                  let! v = !v
                  return (k, v)
                })
              |> state.All
              |> state.Map(Map.ofSeq)

            return TypeValue.Union(cases)
          | TypeExpr.List(element) ->
            let! element = !element
            return TypeValue.List(element)
          | TypeExpr.Set(element) ->
            let! element = !element
            return TypeValue.Set(element)
          | TypeExpr.Map(key, value) ->
            let! key = !key
            let! value = !value
            return TypeValue.Map(key, value)
          | TypeExpr.KeyOf(arg) ->
            let! arg = !arg
            let! cases = arg |> TypeValue.AsRecord |> state.OfSum

            return
              cases
              |> Map.map (fun _ _ -> TypeValue.Primitive(PrimitiveType.Unit))
              |> TypeValue.Union
          | TypeExpr.Sum(variants) ->
            let! variants = variants |> List.map (!) |> state.All

            return TypeValue.Sum(variants)
          | TypeExpr.Flatten(type1, type2) ->
            let! type1 = !type1
            let! type2 = !type2

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
                    return cases1 |> Seq.append cases2 |> Map.ofSeq |> TypeValue.Union
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
                    return fields1 |> Seq.append fields2 |> Map.ofSeq |> TypeValue.Record
                  else
                    return!
                      $"Error: cannot flatten types with overlapping keys: {keys1} and {keys2}"
                      |> Errors.Singleton
                      |> state.Throw
                })
          | TypeExpr.Exclude(type1, type2) ->
            let! type1 = !type1
            let! type2 = !type2

            return!
              state.Either
                (state {
                  let! cases1 = type1 |> TypeValue.AsUnion |> state.OfSum
                  let! cases2 = type2 |> TypeValue.AsUnion |> state.OfSum
                  let keys2 = cases2 |> Map.keys |> Set.ofSeq

                  return
                    cases1
                    |> Map.filter (fun k _ -> keys2 |> Set.contains k |> not)
                    |> TypeValue.Union
                })
                (state {
                  let! fields1 = type1 |> TypeValue.AsRecord |> state.OfSum
                  let! fields2 = type2 |> TypeValue.AsRecord |> state.OfSum
                  let keys2 = fields2 |> Map.keys |> Set.ofSeq

                  return
                    fields1
                    |> Map.filter (fun k _ -> keys2 |> Set.contains k |> not)
                    |> TypeValue.Record
                })
          | TypeExpr.Rotate(t) ->
            let! t = !t

            return!
              state.Either
                (state {
                  let! cases = t |> TypeValue.AsUnion |> state.OfSum

                  return cases |> TypeValue.Record
                })
                (state {
                  let! fields = t |> TypeValue.AsRecord |> state.OfSum

                  return fields |> TypeValue.Union
                })
        }

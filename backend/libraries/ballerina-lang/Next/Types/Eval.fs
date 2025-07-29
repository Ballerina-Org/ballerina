namespace Ballerina.DSL.Next.Types

module Eval =
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open System
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns

  type TypeBindings = Map<string, TypeValue>
  type TypeSymbols = Map<string, TypeSymbol>

  type TypeExprEvalContext =
    { Bindings: TypeBindings
      Symbols: TypeSymbols }

  type TypeExprEvalResult = ReaderWithError<TypeExprEvalContext, TypeValue, Errors>
  type TypeExprEval = TypeExpr -> TypeExprEvalResult
  type TypeExprSymbolEvalResult = ReaderWithError<TypeExprEvalContext, TypeSymbol, Errors>
  type TypeExprSymbolEval = TypeExpr -> TypeExprSymbolEvalResult

  type TypeExprEvalContext with
    static member Empty: TypeExprEvalContext =
      { Bindings = Map.empty
        Symbols = Map.empty }

    static member Create(bindings: TypeBindings, symbols: TypeSymbols) : TypeExprEvalContext =
      { TypeExprEvalContext.Empty with
          Bindings = bindings
          Symbols = symbols }

    static member CreateFromSymbols(symbols: TypeSymbols) : TypeExprEvalContext =
      { TypeExprEvalContext.Empty with
          Symbols = symbols }

    static member tryFindType(v: string) : ReaderWithError<TypeExprEvalContext, TypeValue, Errors> =
      ReaderWithError(fun ctx -> ctx.Bindings |> Map.tryFindWithError v "bindings" v)

    static member tryFindSymbol(v: string) : ReaderWithError<TypeExprEvalContext, TypeSymbol, Errors> =
      ReaderWithError(fun ctx -> ctx.Symbols |> Map.tryFindWithError v "symbols" v)

    static member Updaters =
      {| Bindings = fun u (c: TypeExprEvalContext) -> { c with Bindings = c.Bindings |> u }
         Symbols = fun u (c: TypeExprEvalContext) -> { c with Symbols = c.Symbols |> u } |}

  type TypeExpr with
    static member EvalAsSymbol: TypeExprSymbolEval =
      fun t ->
        reader {
          let (!) = TypeExpr.EvalAsSymbol
          let (!!) = TypeExpr.Eval

          match t with
          | TypeExpr.Lookup v -> return! TypeExprEvalContext.tryFindSymbol v
          | TypeExpr.Apply(f, a) ->
            let! f = !!f
            let! a = !!a
            let! param, body = f |> TypeValue.AsLambda |> reader.OfSum

            return!
              !body
              |> reader.MapContext(Map.add param.Name a |> TypeExprEvalContext.Updaters.Bindings)
          | _ ->
            return!
              $"Error: invalid type expression when evaluating for symbol, got {t}"
              |> Errors.Singleton
              |> reader.Throw
        }

    static member Eval: TypeExprEval =
      fun t ->
        reader {
          let (!) = TypeExpr.Eval
          let (!!) = TypeExpr.EvalAsSymbol

          match t with
          | TypeExpr.Primitive p -> return TypeValue.Primitive p
          | TypeExpr.Lookup v -> return! TypeExprEvalContext.tryFindType v
          | TypeExpr.Apply(f, a) ->
            let! f = !f
            let! param, body = f |> TypeValue.AsLambda |> reader.OfSum

            match param.Kind with
            | Kind.Symbol ->
              let! a = !!a

              return!
                !body
                |> reader.MapContext(Map.add param.Name a |> TypeExprEvalContext.Updaters.Symbols)
            | _ ->
              let! a = !a

              return!
                !body
                |> reader.MapContext(Map.add param.Name a |> TypeExprEvalContext.Updaters.Bindings)
          | TypeExpr.Lambda(param, body) -> return TypeValue.Lambda(param, body)
          | TypeExpr.Arrow(input, output) ->
            let! input = !input
            let! output = !output
            return TypeValue.Arrow(input, output)
          | TypeExpr.Record(fields) ->
            let! fields =
              fields
              |> Seq.map (fun (k, v) ->
                reader {
                  let! k = !!k
                  let! v = !v
                  return (k, v)
                })
              |> reader.All
              |> reader.Map(Map.ofSeq)

            return TypeValue.Record(fields)
          | TypeExpr.Tuple(items) ->
            let! items = items |> List.map (!) |> reader.All
            return TypeValue.Tuple(items)
          | TypeExpr.Union(cases) ->
            let! cases =
              cases
              |> Seq.map (fun (k, v) ->
                reader {
                  let! k = !!k
                  let! v = !v
                  return (k, v)
                })
              |> reader.All
              |> reader.Map(Map.ofSeq)

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
            let! cases = arg |> TypeValue.AsRecord |> reader.OfSum

            return
              cases
              |> Map.map (fun _ _ -> TypeValue.Primitive(PrimitiveType.Unit))
              |> TypeValue.Union
          | TypeExpr.Sum(variants) ->
            let! variants = variants |> List.map (!) |> reader.All

            return TypeValue.Sum(variants)
          | TypeExpr.Flatten(type1, type2) ->
            let! type1 = !type1
            let! type2 = !type2

            return!
              reader.Any2
                (reader {
                  let! cases1 = type1 |> TypeValue.AsUnion |> reader.OfSum
                  let! cases2 = type2 |> TypeValue.AsUnion |> reader.OfSum

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
                      |> reader.Throw
                })
                (reader {
                  let! fields1 = type1 |> TypeValue.AsRecord |> reader.OfSum
                  let! fields2 = type2 |> TypeValue.AsRecord |> reader.OfSum

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
                      |> reader.Throw
                })
          | TypeExpr.Exclude(type1, type2) ->
            let! type1 = !type1
            let! type2 = !type2

            return!
              reader.Any2
                (reader {
                  let! cases1 = type1 |> TypeValue.AsUnion |> reader.OfSum
                  let! cases2 = type2 |> TypeValue.AsUnion |> reader.OfSum
                  let keys2 = cases2 |> Map.keys |> Set.ofSeq

                  return
                    cases1
                    |> Map.filter (fun k _ -> keys2 |> Set.contains k |> not)
                    |> TypeValue.Union
                })
                (reader {
                  let! fields1 = type1 |> TypeValue.AsRecord |> reader.OfSum
                  let! fields2 = type2 |> TypeValue.AsRecord |> reader.OfSum
                  let keys2 = fields2 |> Map.keys |> Set.ofSeq

                  return
                    fields1
                    |> Map.filter (fun k _ -> keys2 |> Set.contains k |> not)
                    |> TypeValue.Record
                })
          | TypeExpr.Rotate(t) ->
            let! t = !t

            return!
              reader.Any2
                (reader {
                  let! cases = t |> TypeValue.AsUnion |> reader.OfSum

                  return cases |> TypeValue.Record
                })
                (reader {
                  let! fields = t |> TypeValue.AsRecord |> reader.OfSum

                  return fields |> TypeValue.Union
                })
        }

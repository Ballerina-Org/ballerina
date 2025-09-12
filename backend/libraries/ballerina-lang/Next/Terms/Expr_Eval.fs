namespace Ballerina.DSL.Next.Terms

[<AutoOpen>]
module Eval =
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Map
  open Ballerina.Coroutines.Model
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open Ballerina.StdLib.Object
  open System
  open Ballerina.DSL.Next.Unification
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Types.TypeCheck
  open Ballerina.DSL.Next.Types.Eval

  type ExprEvalContext<'valueExtension> =
    { Values: Map<Identifier, Value<TypeValue, 'valueExtension>>
      ExtensionOps: ValueExtensionOps<'valueExtension>
      Symbols: Map<Identifier, TypeSymbol> }

  and ExtEvalResult<'valueExtension> =
    | Result of Value<TypeValue, 'valueExtension>
    | Async of Coroutine<ExtEvalResult<'valueExtension>, Unit, Unit, Unit, Errors>
    | Applicable of
      (Value<TypeValue, 'valueExtension> -> ExprEvaluator<'valueExtension, Value<TypeValue, 'valueExtension>>)
    | Matchable of
      (Map<Identifier, CaseHandler<TypeValue>> -> ExprEvaluator<'valueExtension, Value<TypeValue, 'valueExtension>>)

  and ValueExtensionOps<'valueExtension> =
    { Eval: 'valueExtension -> ExprEvaluator<'valueExtension, ExtEvalResult<'valueExtension>> }

  and ExprEvaluator<'valueExtension, 'res> = Reader<'res, ExprEvalContext<'valueExtension>, Errors>

  type ExprEvalContext<'valueExtension> with
    static member Empty: ExprEvalContext<'valueExtension> =
      { Values = Map.empty
        ExtensionOps = { Eval = fun _ -> $"Error: cannot evaluate empty extension" |> Errors.Singleton |> reader.Throw }
        Symbols = Map.empty }

    static member Getters =
      {| Values = fun (c: ExprEvalContext<'valueExtension>) -> c.Values
         ExtensionOps = fun (c: ExprEvalContext<'valueExtension>) -> c.ExtensionOps
         Symbols = fun (c: ExprEvalContext<'valueExtension>) -> c.Symbols |}

    static member Updaters =
      {| Values = fun u (c: ExprEvalContext<'valueExtension>) -> { c with Values = u (c.Values) }
         ExtensionOps =
          fun u (c: ExprEvalContext<'valueExtension>) ->
            { c with
                ExtensionOps = u (c.ExtensionOps) }
         Symbols = fun u (c: ExprEvalContext<'valueExtension>) -> { c with Symbols = u (c.Symbols) } |}

  type Expr<'T> with

    static member EvalApply(fV, argV) =
      reader {
        let! fVVar, fvBody = fV |> Value.AsLambda |> reader.OfSum

        return!
          fvBody
          |> Expr.Eval
          |> reader.MapContext(ExprEvalContext.Updaters.Values(Map.add (Identifier.LocalScope fVVar.Name) argV))
      }

    static member Eval<'valueExtension>
      (e: Expr<TypeValue>)
      : ExprEvaluator<'valueExtension, Value<TypeValue, 'valueExtension>> =
      let (!) = Expr.Eval<'valueExtension>

      reader {
        match e with
        | Expr.Primitive v -> return Value.Primitive v
        | Expr.If(cond, thenBody, elseBody) ->
          let! condV = cond |> Expr.Eval

          match condV with
          | Value.Primitive(PrimitiveValue.Bool true) -> return! thenBody |> Expr.Eval
          | Value.Primitive(PrimitiveValue.Bool false) -> return! elseBody |> Expr.Eval
          | v -> return! $"expected boolean in if condition, got {v}" |> Errors.Singleton |> reader.Throw
        | Expr.Let(var, valueExpr, body) ->
          let! value = valueExpr |> Expr.Eval

          return!
            !body
            |> reader.MapContext(ExprEvalContext.Updaters.Values(Map.add (Identifier.LocalScope var.Name) value))
        | Expr.Lookup id ->
          let! ctx = reader.GetContext()

          return!
            ctx.Values
            |> Map.tryFindWithError id "variables" (id.ToFSharpString)
            |> reader.OfSum
        | Expr.RecordCons fields ->
          let! ctx = reader.GetContext()

          let! fields =
            fields
            |> List.map (fun (id, field) ->
              reader {
                let! v = !field

                let! id =
                  ctx.Symbols
                  |> Map.tryFindWithError id "record field id" (id.ToFSharpString)
                  |> reader.OfSum

                return id, v
              })
            |> reader.All
            |> reader.Map Map.ofList

          return Value.Record(fields)
        | Expr.RecordDes(recordExpr, fieldId) ->
          let! recordV = !recordExpr
          let! recordV = recordV |> Value.AsRecord |> reader.OfSum
          let! ctx = reader.GetContext()

          let! fieldId =
            ctx.Symbols
            |> Map.tryFindWithError fieldId "record field id" (fieldId.ToFSharpString)
            |> reader.OfSum

          return!
            recordV
            |> Map.tryFindWithError fieldId "record field" (fieldId.ToFSharpString)
            |> reader.OfSum

        | Expr.TupleCons fields ->
          let! fields = fields |> List.map (!) |> reader.All

          return Value.Tuple(fields)
        | Expr.TupleDes(recordExpr, fieldId) ->
          let! recordV = !recordExpr
          let! recordV = recordV |> Value.AsTuple |> reader.OfSum

          return!
            recordV
            |> List.tryItem fieldId.Index
            |> sum.OfOption(
              $"Error: tuple index {fieldId.Index} out of bounds, size {List.length recordV}"
              |> Errors.Singleton
            )
            |> reader.OfSum
        | Expr.SumCons(tag, expr) ->
          let! v = !expr
          return Value.Sum(tag.Case, v)
        | Expr.UnionCons(tag, expr) ->
          let! v = !expr
          let! ctx = reader.GetContext()

          let! tag =
            ctx.Symbols
            |> Map.tryFindWithError tag "record field id" (tag.ToFSharpString)
            |> reader.OfSum

          return Value.UnionCase(tag, v)
        | Expr.Apply(Expr.UnionDes cases, unionE) ->
          let! unionV = !unionE

          return!
            reader.Any2
              (reader {
                let! unionVCase, unionV = unionV |> Value.AsUnion |> reader.OfSum

                return!
                  reader {
                    let! caseHandler =
                      cases
                      |> Map.tryFindWithError (unionVCase.Name) "union case" (unionVCase.ToFSharpString)
                      |> reader.OfSum

                    let caseVar, caseBody = caseHandler

                    return!
                      !caseBody
                      |> reader.MapContext(
                        ExprEvalContext.Updaters.Values(Map.add (Identifier.LocalScope caseVar.Name) unionV)
                      )
                  }
                  |> reader.MapError(Errors.WithPriority ErrorPriority.High)
              })
              (reader {
                let! unionV = unionV |> Value.AsExt |> reader.OfSum

                return!
                  reader {
                    let! ctx = reader.GetContext()
                    let unionV = ctx.ExtensionOps.Eval unionV

                    match! unionV with
                    | Matchable f -> return! f cases
                    | _ ->
                      return!
                        "Expected an applicable or matchable extension function"
                        |> Errors.Singleton
                        |> reader.Throw
                  }
                  |> reader.MapError(Errors.WithPriority ErrorPriority.High)
              })
            |> reader.MapError(Errors.HighestPriority)

        | Expr.Apply(Expr.SumDes cases, sumE) ->
          let! sumV = !sumE
          let! sumVCase, sumV = sumV |> Value.AsSum |> reader.OfSum

          let! caseHandler =
            cases
            |> List.tryItem (sumVCase)
            |> sum.OfOption($"Error: sum case {sumVCase} not found" |> Errors.Singleton)
            |> reader.OfSum

          let caseVar, caseBody = caseHandler

          return!
            !caseBody
            |> reader.MapContext(ExprEvalContext.Updaters.Values(Map.add (Identifier.LocalScope caseVar.Name) sumV))

        | Expr.Apply(f, argE) ->
          let! fV = !f
          let! argV = !argE

          return!
            reader.Any(
              reader {
                let! fVVar, fvBody = fV |> Value.AsLambda |> reader.OfSum

                return!
                  !fvBody
                  |> reader.MapContext(ExprEvalContext.Updaters.Values(Map.add (Identifier.LocalScope fVVar.Name) argV))
              },
              [ (reader {
                  let! fExt = fV |> Value.AsExt |> reader.OfSum
                  let! ctx = reader.GetContext()
                  let fExt = ctx.ExtensionOps.Eval fExt

                  match! fExt with
                  | Applicable f -> return! f argV
                  | _ ->
                    return!
                      "Expected an applicable or matchable extension function"
                      |> Errors.Singleton
                      |> reader.Throw
                }) ]
            )

        | Expr.Lambda(var, _, body) -> return Value.Lambda(var, body)
        | Expr.TypeLambda(_, body)
        | Expr.TypeApply(body, _)
        | Expr.TypeLet(_, _, body) -> return! !body
        | _ -> return! $"Cannot eval expression {e}" |> Errors.Singleton |> reader.Throw
      }

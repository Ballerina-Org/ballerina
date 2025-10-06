namespace Ballerina.DSL.Next.Terms

[<AutoOpen>]
module Eval =
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Map
  open Ballerina.Coroutines.Model
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.StdLib.Object
  open System
  open Ballerina.DSL.Next.Unification
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Types.TypeCheck
  open Ballerina.DSL.Next.Types.Model

  type ExprEvalContext<'valueExtension> =
    { Values: Map<Identifier, Value<TypeValue, 'valueExtension>>
      ExtensionOps: ValueExtensionOps<'valueExtension>
      Symbols: Map<Identifier, TypeSymbol> }

  and ExtEvalResult<'valueExtension> =
    | Result of Value<TypeValue, 'valueExtension>
    | Async of Coroutine<ExtEvalResult<'valueExtension>, Unit, Unit, Unit, Errors>
    | Applicable of
      (Value<TypeValue, 'valueExtension> -> ExprEvaluator<'valueExtension, Value<TypeValue, 'valueExtension>>)
    | TypeApplicable of (TypeValue -> ExprEvaluator<'valueExtension, Value<TypeValue, 'valueExtension>>)
    | Matchable of
      (Map<Identifier, CaseHandler<TypeValue>> -> ExprEvaluator<'valueExtension, Value<TypeValue, 'valueExtension>>)

  and ExtensionEvaluator<'valueExtension> =
    Location -> 'valueExtension -> ExprEvaluator<'valueExtension, ExtEvalResult<'valueExtension>>

  and ValueExtensionOps<'valueExtension> =
    { Eval: ExtensionEvaluator<'valueExtension> }

  and ExprEvaluator<'valueExtension, 'res> = Reader<'res, ExprEvalContext<'valueExtension>, Errors>

  type ExprEvalContext<'valueExtension> with
    static member Empty: ExprEvalContext<'valueExtension> =
      { Values = Map.empty
        ExtensionOps =
          { Eval =
              fun loc0 _ ->
                (loc0, $"Error: cannot evaluate empty extension")
                |> Errors.Singleton
                |> reader.Throw }
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

    static member EvalApply (loc0: Location) (fV, argV) =
      reader {
        let! fVVar, fvBody = fV |> Value.AsLambda |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

        return!
          fvBody
          |> Expr.Eval
          |> reader.MapContext(ExprEvalContext.Updaters.Values(Map.add (Identifier.LocalScope fVVar.Name) argV))
      }

    static member Eval<'valueExtension>
      (e: Expr<TypeValue>)
      : ExprEvaluator<'valueExtension, Value<TypeValue, 'valueExtension>> =
      let (!) = Expr.Eval<'valueExtension>
      let loc0 = e.Location

      reader {
        match e.Expr with
        | ExprRec.Primitive v -> return Value.Primitive v
        | ExprRec.If(cond, thenBody, elseBody) ->
          let! condV = cond |> Expr.Eval

          match condV with
          | Value.Primitive(PrimitiveValue.Bool true) -> return! thenBody |> Expr.Eval
          | Value.Primitive(PrimitiveValue.Bool false) -> return! elseBody |> Expr.Eval
          | v ->
            return!
              (loc0, $"expected boolean in if condition, got {v}")
              |> Errors.Singleton
              |> reader.Throw
        | ExprRec.Let(var, _varType, valueExpr, body) ->
          let! value = valueExpr |> Expr.Eval

          return!
            !body
            |> reader.MapContext(ExprEvalContext.Updaters.Values(Map.add (Identifier.LocalScope var.Name) value))
        | ExprRec.Lookup id ->
          let! ctx = reader.GetContext()

          return!
            ctx.Values
            |> Map.tryFindWithError id "variables" (id.ToFSharpString) loc0
            |> reader.OfSum
        | ExprRec.RecordCons fields ->
          let! ctx = reader.GetContext()

          let! fields =
            fields
            |> List.map (fun (id, field) ->
              reader {
                let! v = !field

                let! id =
                  ctx.Symbols
                  |> Map.tryFindWithError id "record field id" (id.ToFSharpString) loc0
                  |> reader.OfSum

                return id, v
              })
            |> reader.All
            |> reader.Map Map.ofList

          return Value.Record(fields)
        | ExprRec.RecordDes(recordExpr, fieldId) ->
          let! recordV = !recordExpr

          let! recordV =
            recordV
            |> Value.AsRecord
            |> sum.MapError(Errors.FromErrors loc0)
            |> reader.OfSum

          let! ctx = reader.GetContext()

          let! fieldId =
            ctx.Symbols
            |> Map.tryFindWithError fieldId "record field id" (fieldId.ToFSharpString) loc0
            |> reader.OfSum

          return!
            recordV
            |> Map.tryFindWithError fieldId "record field" (fieldId.ToFSharpString) loc0
            |> reader.OfSum

        | ExprRec.TupleCons fields ->
          let! fields = fields |> List.map (!) |> reader.All

          return Value.Tuple(fields)
        | ExprRec.TupleDes(recordExpr, fieldId) ->
          let! recordV = !recordExpr
          let! recordV = recordV |> Value.AsTuple |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

          return!
            recordV
            |> List.tryItem fieldId.Index
            |> sum.OfOption(
              (loc0, $"Error: tuple index {fieldId.Index} out of bounds, size {List.length recordV}")
              |> Errors.Singleton
            )
            |> reader.OfSum
        | ExprRec.SumCons(tag, expr) ->
          let! v = !expr
          return Value.Sum(tag.Case, v)
        | ExprRec.UnionCons(tag, expr) ->
          let! v = !expr
          let! ctx = reader.GetContext()

          let! tag =
            ctx.Symbols
            |> Map.tryFindWithError tag "record field id" (tag.ToFSharpString) loc0
            |> reader.OfSum

          return Value.UnionCase(tag, v)
        | ExprRec.Apply({ Expr = ExprRec.UnionDes(cases, fallback) }, unionE) ->
          let! unionV = !unionE

          return!
            reader.Any2
              (reader {
                let! unionVCase, unionV =
                  unionV |> Value.AsUnion |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                return!
                  reader {

                    let! caseHandler =
                      cases
                      |> Map.tryFindWithError (unionVCase.Name) "union case" (unionVCase.ToFSharpString) loc0
                      |> reader.OfSum
                      |> reader.Catch
                      |> reader.Map(Sum.toOption)

                    match caseHandler with
                    | Some caseHandler ->
                      let caseVar, caseBody = caseHandler

                      return!
                        !caseBody
                        |> reader.MapContext(
                          ExprEvalContext.Updaters.Values(Map.add (Identifier.LocalScope caseVar.Name) unionV)
                        )
                    | None ->
                      match fallback with
                      | Some fallback -> return! !fallback
                      | None ->
                        return!
                          (loc0, $"Error: cannot find case handler for union case {unionVCase.Name}")
                          |> Errors.Singleton
                          |> reader.Throw
                  }
                  |> reader.MapError(Errors.SetPriority ErrorPriority.High)
              })
              (reader {
                let! unionV = unionV |> Value.AsExt |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                return!
                  reader {
                    let! ctx = reader.GetContext()
                    let unionV = ctx.ExtensionOps.Eval loc0 unionV

                    match! unionV with
                    | Matchable f -> return! f cases
                    | _ ->
                      return!
                        (loc0, "Expected an applicable or matchable extension function")
                        |> Errors.Singleton
                        |> reader.Throw
                  }
                  |> reader.MapError(Errors.SetPriority ErrorPriority.High)
              })
            |> reader.MapError(Errors.FilterHighestPriorityOnly)

        | ExprRec.Apply({ Expr = ExprRec.SumDes cases }, sumE) ->
          let! sumV = !sumE
          let! sumVCase, sumV = sumV |> Value.AsSum |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

          let! caseHandler =
            cases
            |> List.tryItem (sumVCase)
            |> sum.OfOption((loc0, $"Error: sum case {sumVCase} not found") |> Errors.Singleton)
            |> reader.OfSum

          let caseVar, caseBody = caseHandler

          return!
            !caseBody
            |> reader.MapContext(ExprEvalContext.Updaters.Values(Map.add (Identifier.LocalScope caseVar.Name) sumV))

        | ExprRec.Apply(f, argE) ->
          let! fV = !f
          let! argV = !argE

          return!
            reader.Any(
              reader {
                let! fVVar, fvBody = fV |> Value.AsLambda |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                return!
                  !fvBody
                  |> reader.MapContext(ExprEvalContext.Updaters.Values(Map.add (Identifier.LocalScope fVVar.Name) argV))
              },
              [ (reader {
                  let! fExt = fV |> Value.AsExt |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum
                  let! ctx = reader.GetContext()
                  let! fExt = ctx.ExtensionOps.Eval loc0 fExt

                  match fExt with
                  | Applicable f -> return! f argV
                  | _ ->
                    return!
                      (loc0, "Expected an applicable or matchable extension function")
                      |> Errors.Singleton
                      |> reader.Throw
                }) ]
            )

        | ExprRec.Lambda(var, _, body) -> return Value.Lambda(var, body)
        | ExprRec.TypeLambda(_, body)
        | ExprRec.TypeLet(_, _, body) -> return! !body
        | ExprRec.TypeApply(typeLambda, typeArg) ->
          return!
            reader.Any(
              // Note: ordering matters here, the most specific branch needs to be evaluated first
              reader {
                let! typeLambda = !typeLambda

                let! ext =
                  typeLambda
                  |> Value.AsExt
                  |> sum.MapError(Errors.FromErrors loc0)
                  |> reader.OfSum

                let! ctx = reader.GetContext()
                let! ext = ctx.ExtensionOps.Eval loc0 ext

                match ext with
                | TypeApplicable f -> return! f typeArg
                | _ ->
                  return!
                    (loc0, $"Expected extension to be type applicable, got {ext}")
                    |> Errors.Singleton
                    |> reader.Throw
              },
              // this says we do not care about the type info
              [ !typeLambda ]
            )
        | _ -> return! (loc0, $"Cannot eval expression {e}") |> Errors.Singleton |> reader.Throw
      }

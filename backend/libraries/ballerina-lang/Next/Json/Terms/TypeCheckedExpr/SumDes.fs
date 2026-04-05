namespace Ballerina.DSL.Next.Terms.Json.TypeCheckedExpr

[<AutoOpen>]
module SumDes =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.Errors
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.DSL.Next.Terms.Patterns

  let private discriminator = "sum-des"

  type TypeCheckedExpr<'valueExt> with
    static member FromJsonSumDes
      (fromRootJson: TypeCheckedExprParser<'valueExt>)
      (value: JsonValue)
      : TypeCheckedExprParserReader<'valueExt> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator value (fun sumDesJson ->
        reader {
          let! caseHandlers = sumDesJson |> JsonValue.AsArray |> reader.OfSum

          let! caseHandlers =
            caseHandlers
            |> Seq.map (fun caseHandler ->
              reader {
                let! handlerCase, handlerCount, handlerVar, handlerBody =
                  caseHandler |> JsonValue.AsQuadruple |> reader.OfSum

                let! handlerCase = handlerCase |> JsonValue.AsInt |> reader.OfSum
                let! handlerCount = handlerCount |> JsonValue.AsInt |> reader.OfSum
                let! handlerVar = handlerVar |> JsonValue.AsString |> reader.OfSum
                let handlerVar = Var.Create handlerVar
                let! handlerBody = handlerBody |> fromRootJson

                return
                  ({ Case = handlerCase
                     Count = handlerCount }),
                  (Some handlerVar, handlerBody)
              })
            |> reader.All
            |> reader.Map(Map.ofSeq)

          return TypeCheckedExpr.SumDes(caseHandlers)
        })

    static member ToJsonSumDes
      (rootToJson: TypeCheckedExprEncoder<'valueExt>)
      (caseHandlers: Map<SumConsSelector, TypeCheckedCaseHandler<'valueExt>>)
      : TypeCheckedExprEncoderReader<'valueExt> =
      caseHandlers
      |> Map.toList
      |> List.map (fun (k, (v, h)) ->
        reader {
          let i = k.Case |> decimal |> JsonValue.Number
          let n = k.Count |> decimal |> JsonValue.Number

          let v =
            v
            |> Option.map (fun var -> var.Name |> JsonValue.String)
            |> Option.defaultValue (JsonValue.String "()")

          let! h = h |> rootToJson
          return [| i; n; v; h |] |> JsonValue.Array
        })
      |> reader.All
      |> reader.Map(Array.ofList >> JsonValue.Array >> Json.discriminator discriminator)

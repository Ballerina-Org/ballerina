namespace Ballerina.DSL.Next.Terms.Json.Expr

[<AutoOpen>]
module SumDes =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.Errors
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.DSL.Next.Terms.Patterns

  let private discriminator = "sum-des"

  type Expr<'T> with
    static member FromJsonSumDes (fromRootJson: ExprParser<'T>) (value: JsonValue) : ExprParserReader<'T> =
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
                  (handlerVar, handlerBody)
              })
            |> reader.All
            |> reader.Map(Map.ofSeq)

          return Expr.SumDes(caseHandlers)
        })

    static member ToJsonSumDes
      (rootToJson: ExprEncoder<'T>)
      (caseHandlers: Map<SumConsSelector, CaseHandler<'T>>)
      : ExprEncoderReader<'T> =
      caseHandlers
      |> Map.toList
      |> List.map (fun (k, (v, h)) ->
        reader {
          let i = k.Case |> decimal |> JsonValue.Number
          let n = k.Count |> decimal |> JsonValue.Number
          let v = v.Name |> JsonValue.String
          let! h = h |> rootToJson
          return [| i; n; v; h |] |> JsonValue.Array
        })
      |> reader.All
      |> reader.Map(Array.ofList >> JsonValue.Array >> Json.discriminator discriminator)

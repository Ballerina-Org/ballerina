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

  let private kindKey = "sum-des"
  let private fieldKey = "sum-des"

  type Expr<'T> with
    static member FromJsonSumDes (fromRootJson: ExprParser<'T>) (value: JsonValue) : ExprParserReader<'T> =
      reader.AssertKindAndContinueWithField value kindKey fieldKey (fun sumDesJson ->
        reader {
          let! caseHandlers = sumDesJson |> JsonValue.AsArray |> reader.OfSum

          let! caseHandlers =
            caseHandlers
            |> Seq.map (fun caseHandler ->
              reader {
                let! handlerVar, handlerBody = caseHandler |> JsonValue.AsPair |> reader.OfSum
                let! handlerVar = handlerVar |> JsonValue.AsString |> reader.OfSum
                let handlerVar = Var.Create handlerVar
                let! handlerBody = handlerBody |> fromRootJson
                return (handlerVar, handlerBody)
              })
            |> reader.All

          return Expr.SumDes(caseHandlers)
        })

    static member ToJsonSumDes
      (rootToJson: ExprEncoder<'T>)
      (caseHandlers: List<CaseHandler<'T>>)
      : ExprEncoderReader<'T> =
      caseHandlers
      |> List.map (fun (v, c) ->
        reader {
          let v = v.Name |> JsonValue.String
          let! c = c |> rootToJson
          return [| v; c |] |> JsonValue.Array
        })
      |> reader.All
      |> reader.Map(Array.ofList >> JsonValue.Array >> Json.kind kindKey fieldKey)

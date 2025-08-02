namespace Ballerina.DSL.Next.Terms.Json.Expr

[<AutoOpen>]
module SumDes =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Json

  type Expr<'T> with
    static member FromJsonSumDes(fromRootJson: JsonValue -> ExprParser<'T>) : JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "sum-des" "sum-des" (fun sumDesJson ->
        reader {
          let! caseHandlers = sumDesJson |> JsonValue.AsArray |> reader.OfSum

          let! caseHandlers =
            caseHandlers
            |> Seq.map (fun caseHandler ->
              reader {
                let! (caseIndex, handler) = caseHandler |> JsonValue.AsPair |> reader.OfSum
                let! caseIndex = caseIndex |> JsonValue.AsInt |> reader.OfSum
                let! handlerVar, handlerBody = handler |> JsonValue.AsPair |> reader.OfSum
                let! handlerVar = handlerVar |> JsonValue.AsString |> reader.OfSum
                let handlerVar = Var.Create handlerVar
                let! handlerBody = handlerBody |> fromRootJson
                return (caseIndex, (handlerVar, handlerBody))
              })
            |> reader.All
            |> reader.Map Map.ofSeq

          return Expr.SumDes(caseHandlers)
        })

    static member ToJsonSumDes(rootToJson: Expr<'T> -> JsonValue) : Map<int, CaseHandler<'T>> -> JsonValue =
      Map.toArray
      >> Array.map (fun (i, (v, c)) ->
        let i = i |> decimal |> JsonValue.Number
        let v = v.Name |> JsonValue.String
        let c = c |> rootToJson
        [| i; [| v; c |] |> JsonValue.Array |] |> JsonValue.Array)
      >> JsonValue.Array
      >> Json.kind "sum-des" "sum-des"

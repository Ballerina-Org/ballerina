namespace Ballerina.DSL.Next.Terms.Json.Expr

[<AutoOpen>]
module UnionDes =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Json

  type Expr<'T> with
    static member FromJsonUnionDes(fromRootJson: JsonValue -> ExprParser<'T>) : JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "union-match" "union-match" (fun unionDesJson ->
        reader {
          let! caseHandlers = unionDesJson |> JsonValue.AsArray |> reader.OfSum

          let! caseHandlers =
            caseHandlers
            |> Seq.map (fun caseHandler ->
              reader {
                let! (caseName, handler) = caseHandler |> JsonValue.AsPair |> reader.OfSum
                let! caseName = caseName |> JsonValue.AsString |> reader.OfSum
                let! handlerVar, handlerBody = handler |> JsonValue.AsPair |> reader.OfSum
                let! handlerVar = handlerVar |> JsonValue.AsString |> reader.OfSum
                let handlerVar = Var.Create handlerVar
                let! handlerBody = handlerBody |> fromRootJson
                return (caseName, (handlerVar, handlerBody))
              })
            |> reader.All
            |> reader.Map Map.ofSeq

          return Expr.UnionDes(caseHandlers)
        })

    static member ToJsonUnionDes(rootToJson: Expr<'T> -> JsonValue) : Map<string, CaseHandler<'T>> -> JsonValue =
      fun union ->
        let cases =
          union
          |> Map.toList
          |> List.map (fun (caseName, (handlerVar, handlerExpr)) ->
            let caseNameJson = JsonValue.String caseName

            let handlerJson =
              JsonValue.Array [| JsonValue.String handlerVar.Name; rootToJson handlerExpr |]

            JsonValue.Array [| caseNameJson; handlerJson |])

        JsonValue.Array(List.toArray cases) |> Json.kind "union-match" "union-match"

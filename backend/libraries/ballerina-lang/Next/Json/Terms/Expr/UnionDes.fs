namespace Ballerina.DSL.Next.Terms.Json.Expr

[<AutoOpen>]
module UnionDes =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Json
  open Ballerina.Errors

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
                let! caseName = caseName |> Identifier.FromJson |> reader.OfSum
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

    static member ToJsonUnionDes
      : ExprEncoder<'T> -> Map<Identifier, CaseHandler<'T>> -> Reader<JsonValue, JsonEncoder<'T>, Errors> =
      fun rootToJson union ->
        reader {
          let! cases =
            union
            |> Map.toList
            |> List.map (fun (caseName, (handlerVar, handlerExpr)) ->
              reader {
                let caseNameJson = caseName |> Identifier.ToJson
                let! handlerExpr = rootToJson handlerExpr

                let handlerJson =
                  JsonValue.Array [| JsonValue.String handlerVar.Name; handlerExpr |]

                return JsonValue.Array [| caseNameJson; handlerJson |]
              })
            |> reader.All

          return JsonValue.Array(List.toArray cases) |> Json.kind "union-match" "union-match"
        }

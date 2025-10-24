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
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.DSL.Next.Terms.Patterns

  let private discriminator = "union-match"

  type Expr<'T, 'Id when 'Id: comparison> with
    static member FromJsonUnionDes (fromRootJson: ExprParser<'T, 'Id>) (value: JsonValue) : ExprParserReader<'T, 'Id> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator value (fun unionDesJson ->
        reader {
          let! (_, idFromJson) = reader.GetContext()
          let! caseHandlers = unionDesJson |> JsonValue.AsArray |> reader.OfSum

          let! caseHandlers =
            caseHandlers
            |> Seq.map (fun caseHandler ->
              reader {
                let! (caseName, handler) = caseHandler |> JsonValue.AsPair |> reader.OfSum
                let! caseName = caseName |> idFromJson |> reader.OfSum
                let! handlerVar, handlerBody = handler |> JsonValue.AsPair |> reader.OfSum
                let! handlerVar = handlerVar |> JsonValue.AsString |> reader.OfSum
                let handlerVar = Var.Create handlerVar
                let! handlerBody = handlerBody |> fromRootJson
                return (caseName, (handlerVar, handlerBody))
              })
            |> reader.All
            |> reader.Map Map.ofSeq

          return Expr.UnionDes(caseHandlers, None)
        })

    static member ToJsonUnionDes
      (rootToJson: ExprEncoder<'T, 'Id>)
      (union: Map<'Id, CaseHandler<'T, 'Id>>)
      (_fallback: Option<Expr<'T, 'Id>>)
      : ExprEncoderReader<'T, 'Id> =
      reader {
        let! cases =
          union
          |> Map.toList
          |> List.map (fun (caseName, (handlerVar, handlerExpr)) ->
            reader {
              let! _, ctx = reader.GetContext()
              let caseNameJson = caseName |> ctx
              let! handlerExpr = rootToJson handlerExpr

              let handlerJson =
                JsonValue.Array [| JsonValue.String handlerVar.Name; handlerExpr |]

              return JsonValue.Array [| caseNameJson; handlerJson |]
            })
          |> reader.All

        return JsonValue.Array(List.toArray cases) |> Json.discriminator discriminator
      }

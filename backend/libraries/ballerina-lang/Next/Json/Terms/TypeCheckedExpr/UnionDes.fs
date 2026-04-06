namespace Ballerina.DSL.Next.Terms.Json.TypeCheckedExpr

[<AutoOpen>]
module UnionDes =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Json
  open Ballerina.Errors
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.DSL.Next.Terms.Patterns

  let private discriminator = "union-match"

  type TypeCheckedExpr<'valueExt> with
    static member FromJsonUnionDes
      (fromRootJson: TypeCheckedExprParser<'valueExt>)
      (value: JsonValue)
      : TypeCheckedExprParserReader<'valueExt> =
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
                let handlerVar = handlerVar |> Var.Create |> Some
                let! handlerBody = handlerBody |> fromRootJson
                return (caseName, (handlerVar, handlerBody))
              })
            |> reader.All
            |> reader.Map Map.ofSeq

          return TypeCheckedExpr.UnionDes(caseHandlers, None, TypeValue.CreateUnit(), Kind.Star)
        })

    static member ToJsonUnionDes
      (rootToJson: TypeCheckedExprEncoder<'valueExt>)
      (union: Map<ResolvedIdentifier, TypeCheckedCaseHandler<'valueExt>>)
      (_fallback: Option<TypeCheckedExpr<'valueExt>>)
      : TypeCheckedExprEncoderReader<'valueExt> =
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
                JsonValue.Array
                  [| handlerVar
                     |> Option.map (fun v -> v.Name)
                     |> Option.defaultValue "@anoynomous"
                     |> JsonValue.String
                     handlerExpr |]

              return JsonValue.Array [| caseNameJson; handlerJson |]
            })
          |> reader.All

        return JsonValue.Array(List.toArray cases) |> Json.discriminator discriminator
      }

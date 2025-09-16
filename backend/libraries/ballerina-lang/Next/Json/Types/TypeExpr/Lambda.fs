namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module LambdaTypeExpr =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model

  let private kindKey = "lambda"
  let private fieldKey = "lambda"

  type TypeExpr with
    static member FromJsonLambda(fromJsonRoot: TypeExprParser) : TypeExprParser =
      sum.AssertKindAndContinueWithField kindKey fieldKey (fun lambdaFields ->
        sum {
          let! param, body = lambdaFields |> JsonValue.AsPair
          let! param = param |> TypeParameter.FromJson
          let! body = body |> fromJsonRoot

          return TypeExpr.Lambda(param, body)
        })

    static member ToJsonLambda(rootToJson: TypeExpr -> JsonValue) : TypeParameter * TypeExpr -> JsonValue =
      fun (param, body) ->
        let param = param |> TypeParameter.ToJson
        let body = body |> rootToJson
        JsonValue.Array [| param; body |] |> Json.kind kindKey fieldKey

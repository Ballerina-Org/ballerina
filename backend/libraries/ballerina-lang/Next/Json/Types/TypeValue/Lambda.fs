namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module Lambda =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Json
  open Ballerina.Collections.Sum.Operators
  open Ballerina.DSL.Next.Json

  type TypeValue with
    static member FromJsonLambda(fromExpr: JsonValue -> Sum<TypeExpr, Errors>) : JsonValue -> Sum<TypeValue, Errors> =
      sum.AssertKindAndContinueWithField "lambda" "lambda" (fun lambdaFields ->
        sum {
          let! lambdaFields = lambdaFields |> JsonValue.AsRecordMap

          let! param =
            lambdaFields
            |> (Map.tryFindWithError "param" "lambda" "param" >>= TypeParameter.FromJson)

          let! body = lambdaFields |> (Map.tryFindWithError "body" "lambda" "body" >>= fromExpr)

          return TypeValue.Lambda(param, body)
        })

    static member ToJsonLambda(rootToJson: TypeExpr -> JsonValue) : TypeParameter * TypeExpr -> JsonValue =
      fun (param, body) ->
        let paramJson = TypeParameter.ToJson param
        let bodyJson = body |> rootToJson

        JsonValue.Record [| ("param", paramJson); ("body", bodyJson) |]
        |> Json.kind "lambda" "lambda"

namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module Lambda =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Json
  open Ballerina
  open Ballerina.Collections.Sum.Operators
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "lambda"

  type TypeValue<'valueExt> with
    static member FromJsonLambda
      (fromExpr: JsonValue -> Sum<TypeExpr<'valueExt>, Errors<unit>>)
      : JsonValue -> Sum<TypeParameter * TypeExpr<'valueExt>, Errors<unit>> =
      Sum.assertDiscriminatorAndContinueWithValue discriminator (fun lambdaFields ->
        sum {
          let! lambdaFields = lambdaFields |> JsonValue.AsRecordMap

          let! param =
            lambdaFields
            |> (Map.tryFindWithError "param" "lambda" (fun () -> "param") ()
                >>= TypeParameter.FromJson)

          let! body =
            lambdaFields
            |> (Map.tryFindWithError "body" "lambda" (fun () -> "body") () >>= fromExpr)

          return param, body
        })

    static member ToJsonLambda
      (rootToJson: TypeExpr<'valueExt> -> JsonValue)
      : TypeParameter * TypeExpr<'valueExt> -> JsonValue =
      fun (param, body) ->
        let paramJson = TypeParameter.ToJson param
        let bodyJson = body |> rootToJson

        JsonValue.Record [| ("param", paramJson); ("body", bodyJson) |]
        |> Json.discriminator discriminator

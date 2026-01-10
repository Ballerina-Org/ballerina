namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module LambdaTypeExpr =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "lambda"

  type TypeExpr<'valueExt> with
    static member FromJsonLambda(fromJsonRoot: TypeExprParser<'valueExt>) : TypeExprParser<'valueExt> =
      Sum.assertDiscriminatorAndContinueWithValue discriminator (fun lambdaFields ->
        sum {
          let! param, body = lambdaFields |> JsonValue.AsPair
          let! param = param |> TypeParameter.FromJson
          let! body = body |> fromJsonRoot

          return TypeExpr.Lambda(param, body)
        })

    static member ToJsonLambda
      (rootToJson: TypeExpr<'valueExt> -> JsonValue)
      : TypeParameter * TypeExpr<'valueExt> -> JsonValue =
      fun (param, body) ->
        let param = param |> TypeParameter.ToJson
        let body = body |> rootToJson
        JsonValue.Array [| param; body |] |> Json.discriminator discriminator

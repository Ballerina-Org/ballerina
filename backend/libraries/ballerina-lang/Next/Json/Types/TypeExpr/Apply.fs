namespace Ballerina.DSL.Next.Types.Json

open Ballerina.DSL.Next.Types.Model

[<AutoOpen>]
module Apply =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "apply"

  type TypeExpr<'valueExt> with
    static member FromJsonApply(fromJsonRoot: JsonParser<TypeExpr<'valueExt>>) : JsonParser<TypeExpr<'valueExt>> =
      Sum.assertDiscriminatorAndContinueWithValue discriminator (fun applyFields ->
        sum {
          let! (functionField, argumentField) = applyFields |> JsonValue.AsPair

          let! functionType = functionField |> fromJsonRoot
          let! argumentType = argumentField |> fromJsonRoot

          return TypeExpr.Apply(functionType, argumentType)
        })

    static member ToJsonApply
      (rootToJson: TypeExpr<'valueExt> -> JsonValue)
      : TypeExpr<'valueExt> * TypeExpr<'valueExt> -> JsonValue =
      fun (functionType, argumentType) ->
        let functionJson = functionType |> rootToJson
        let argumentJson = argumentType |> rootToJson

        JsonValue.Array [| functionJson; argumentJson |]
        |> Json.discriminator discriminator

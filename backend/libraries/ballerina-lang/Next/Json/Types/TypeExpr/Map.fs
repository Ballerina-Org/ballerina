namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module MapTypeExpr =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "map"

  type TypeExpr<'valueExt> with
    static member FromJsonMap(fromJsonRoot: TypeExprParser<'valueExt>) : TypeExprParser<'valueExt> =
      Sum.assertDiscriminatorAndContinueWithValue discriminator (fun mapFields ->
        sum {
          let! (key, value) = mapFields |> JsonValue.AsPair
          let! keyType = key |> fromJsonRoot
          let! valueType = value |> fromJsonRoot
          return TypeExpr.Map(keyType, valueType)
        })

    static member ToJsonMap
      (rootToJson: TypeExpr<'valueExt> -> JsonValue)
      : TypeExpr<'valueExt> * TypeExpr<'valueExt> -> JsonValue =
      fun (keyType, valueType) ->
        let keyJson = keyType |> rootToJson
        let valueJson = valueType |> rootToJson
        JsonValue.Array [| keyJson; valueJson |] |> Json.discriminator discriminator

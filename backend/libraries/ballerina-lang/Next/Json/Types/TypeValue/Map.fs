namespace Ballerina.DSL.Next.Types.Json

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module Map =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "map"

  type TypeValue<'valueExt> with
    static member FromJsonMap
      (fromRootJson: JsonValue -> Sum<TypeValue<'valueExt>, Errors<unit>>)
      : JsonValue -> Sum<TypeValue<'valueExt> * TypeValue<'valueExt>, Errors<unit>> =
      Sum.assertDiscriminatorAndContinueWithValue discriminator (fun mapFields ->
        sum {
          let! (key, value) = mapFields |> JsonValue.AsPair
          let! keyType = key |> fromRootJson
          let! valueType = value |> fromRootJson
          return keyType, valueType
        })

    static member ToJsonMap
      (toRootJson: TypeValue<'valueExt> -> JsonValue)
      : TypeValue<'valueExt> * TypeValue<'valueExt> -> JsonValue =
      fun (keyType, valueType) ->
        let keyJson = keyType |> toRootJson
        let valueJson = valueType |> toRootJson
        JsonValue.Array [| keyJson; valueJson |] |> Json.discriminator discriminator

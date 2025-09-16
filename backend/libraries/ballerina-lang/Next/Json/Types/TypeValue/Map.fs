namespace Ballerina.DSL.Next.Types.Json

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module Map =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Types.Model

  let private kindKey = "map"
  let private fieldKey = "map"

  type TypeValue with
    static member FromJsonMap(fromRootJson: JsonValue -> Sum<TypeValue, Errors>) : JsonValue -> Sum<TypeValue, Errors> =
      sum.AssertKindAndContinueWithField kindKey fieldKey (fun mapFields ->
        sum {
          let! (key, value) = mapFields |> JsonValue.AsPair
          let! keyType = key |> fromRootJson
          let! valueType = value |> fromRootJson
          return TypeValue.Map(keyType, valueType)
        })

    static member ToJsonMap(toRootJson: TypeValue -> JsonValue) : TypeValue * TypeValue -> JsonValue =
      fun (keyType, valueType) ->
        let keyJson = keyType |> toRootJson
        let valueJson = valueType |> toRootJson
        JsonValue.Array [| keyJson; valueJson |] |> Json.kind kindKey fieldKey

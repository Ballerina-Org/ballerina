namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module Record =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Json
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap

  let private discriminator = "record"

  type TypeValue with
    static member FromJsonRecord
      (fromRootJson: JsonValue -> Sum<TypeValue, Errors>)
      : JsonValue -> Sum<TypeValue, Errors> =
      Sum.assertDiscriminatorAndContinueWithValue discriminator (fun recordFields ->
        sum {
          let! fields = recordFields |> JsonValue.AsArray

          let! fieldTypes =
            fields
            |> Array.map (fun field ->
              sum {
                let! fieldKey, fieldValueAndKind = field |> JsonValue.AsPair
                let! fieldValue, fieldKind = fieldValueAndKind |> JsonValue.AsPair
                let! fieldType = fromRootJson fieldValue
                let! fieldKind = fieldKind |> Kind.FromJson
                let! fieldKey = fieldKey |> TypeSymbol.FromJson
                return fieldKey, (fieldType, fieldKind)
              })
            |> sum.All
            |> sum.Map OrderedMap.ofSeq

          return TypeValue.CreateRecord(fieldTypes) // FIXME: origin should be serialized and parsed
        })

    static member ToJsonRecord
      (rootToJson: TypeValue -> JsonValue)
      : OrderedMap<TypeSymbol, TypeValue * Kind> -> JsonValue =
      OrderedMap.toArray
      >> Array.map (fun (fieldKey, (fieldValue, fieldKind)) ->
        let fieldKeyJson = fieldKey |> TypeSymbol.ToJson
        let fieldValueJson = fieldValue |> rootToJson
        let fieldKindJson = fieldKind |> Kind.ToJson
        JsonValue.Array [| fieldKeyJson; JsonValue.Array [| fieldValueJson; fieldKindJson |] |])
      >> JsonValue.Array
      >> Json.discriminator discriminator

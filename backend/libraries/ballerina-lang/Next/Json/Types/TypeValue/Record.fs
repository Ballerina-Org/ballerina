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
                let! (fieldKey, fieldValue) = field |> JsonValue.AsPair
                let! fieldType = fromRootJson fieldValue
                let! fieldKey = fieldKey |> TypeSymbol.FromJson
                return (fieldKey, fieldType)
              })
            |> sum.All
            |> sum.Map Map.ofSeq

          return TypeValue.CreateRecord(fieldTypes) // FIXME: origin should be serialized and parsed
        })

    static member ToJsonRecord(rootToJson: TypeValue -> JsonValue) : Map<TypeSymbol, TypeValue> -> JsonValue =
      Map.toArray
      >> Array.map (fun (fieldKey, fieldValue) ->
        let fieldKey = fieldKey |> TypeSymbol.ToJson
        let fieldValue = rootToJson fieldValue
        JsonValue.Array [| fieldKey; fieldValue |])
      >> JsonValue.Array
      >> Json.discriminator discriminator

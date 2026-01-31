namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module Record =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina
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

  type TypeValue<'valueExt> with
    static member FromJsonRecord
      (fromRootJson: JsonValue -> Sum<TypeValue<'valueExt>, Errors<unit>>)
      : JsonValue -> Sum<OrderedMap<TypeSymbol, TypeValue<'valueExt> * Kind>, Errors<unit>> =
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

          return fieldTypes
        })

    static member ToJsonRecord
      (rootToJson: TypeValue<'valueExt> -> JsonValue)
      : OrderedMap<TypeSymbol, TypeValue<'valueExt> * Kind> -> JsonValue =


      (OrderedMap.toArray
       >> Array.map (fun (fieldKey, (fieldValue, fieldKind)) ->
         let fieldKeyJson = fieldKey |> TypeSymbol.ToJson
         let fieldValueJson = fieldValue |> rootToJson
         let fieldKindJson = fieldKind |> Kind.ToJson
         JsonValue.Array [| fieldKeyJson; JsonValue.Array [| fieldValueJson; fieldKindJson |] |])
       >> JsonValue.Array)
      >> Json.discriminator discriminator

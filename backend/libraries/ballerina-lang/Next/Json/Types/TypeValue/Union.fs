namespace Ballerina.DSL.Next.Types.Json

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module Union =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Json
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap

  let private discriminator = "union"

  type TypeValue with
    static member FromJsonUnion
      (fromRootJson: JsonValue -> Sum<TypeValue, Errors>)
      : JsonValue -> Sum<TypeValue, Errors> =
      Sum.assertDiscriminatorAndContinueWithValue discriminator (fun unionFields ->
        sum {
          let! cases = unionFields |> JsonValue.AsArray

          let! caseTypes =
            cases
            |> Array.map (fun case ->
              sum {
                let! (caseKey, caseValue) = case |> JsonValue.AsPair
                let! caseType = fromRootJson caseValue
                let! caseKey = caseKey |> TypeSymbol.FromJson
                return (caseKey, caseType)
              })
            |> sum.All
            |> sum.Map OrderedMap.ofSeq

          return TypeValue.CreateUnion(caseTypes) // FIXME: origin should be serialized and parsed
        })

    static member ToJsonUnion(rootToJson: TypeValue -> JsonValue) : OrderedMap<TypeSymbol, TypeValue> -> JsonValue =
      OrderedMap.toArray
      >> Array.map (fun (symbol, value) -> JsonValue.Array [| TypeSymbol.ToJson symbol; rootToJson value |])
      >> JsonValue.Array
      >> Json.discriminator discriminator

namespace Ballerina.DSL.Next.Types.Json

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module Tuple =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "tuple"

  type TypeValue with
    static member FromJsonTuple
      (fromRootJson: JsonValue -> Sum<TypeValue, Errors>)
      : JsonValue -> Sum<TypeValue, Errors> =
      Sum.assertDiscriminatorAndContinueWithValue discriminator (fun tupleFields ->
        sum {
          let! elements = tupleFields |> JsonValue.AsArray
          let! elementTypes = elements |> Array.map (fun element -> element |> fromRootJson) |> sum.All
          return TypeValue.CreateTuple(elementTypes) // FIXME: origin should be serialized and parsed
        })

    static member ToJsonTuple(rootToJson: TypeValue -> JsonValue) : List<TypeValue> -> JsonValue =
      List.toArray
      >> Array.map rootToJson
      >> JsonValue.Array
      >> Json.discriminator discriminator

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

  type TypeValue<'valueExt> with
    static member FromJsonTuple
      (fromRootJson: JsonValue -> Sum<TypeValue<'valueExt>, Errors>)
      : JsonValue -> Sum<List<TypeValue<'valueExt>>, Errors> =
      Sum.assertDiscriminatorAndContinueWithValue discriminator (fun tupleFields ->
        sum {
          let! elements = tupleFields |> JsonValue.AsArray
          let! elementTypes = elements |> Array.map (fun element -> element |> fromRootJson) |> sum.All
          return elementTypes
        })

    static member ToJsonTuple(rootToJson: TypeValue<'valueExt> -> JsonValue) : List<TypeValue<'valueExt>> -> JsonValue =
      List.toArray
      >> Array.map rootToJson
      >> JsonValue.Array
      >> Json.discriminator discriminator

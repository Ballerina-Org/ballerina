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

  type TypeValue with
    static member FromJsonTuple
      (fromRootJson: JsonValue -> Sum<TypeValue, Errors>)
      : JsonValue -> Sum<TypeValue, Errors> =
      sum.AssertKindAndContinueWithField "tuple" "tuple" (fun tupleFields ->
        sum {
          let! elements = tupleFields |> JsonValue.AsArray
          let! elementTypes = elements |> Array.map (fun element -> element |> fromRootJson) |> sum.All
          return TypeValue.Tuple(elementTypes)
        })

    static member ToJsonTuple(rootToJson: TypeValue -> JsonValue) : List<TypeValue> -> JsonValue =
      List.toArray
      >> Array.map rootToJson
      >> JsonValue.Array
      >> Json.kind "tuple" "tuple"

namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module Lookup =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model

  type TypeValue with
    static member FromJsonLookup: JsonValue -> Sum<TypeValue, Errors> =
      sum.AssertKindAndContinueWithField "lookup" "lookup" (fun lookupFields ->
        sum {
          let! name = lookupFields |> JsonValue.AsString
          return TypeValue.Lookup { Name = name }
        })

    static member ToJsonLookup: TypeIdentifier -> JsonValue =
      fun id -> JsonValue.String id.Name |> Json.kind "lookup" "lookup"

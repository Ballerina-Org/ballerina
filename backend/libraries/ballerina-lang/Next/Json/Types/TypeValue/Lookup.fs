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
          return TypeValue.Lookup(Identifier.LocalScope name)
        })

    static member ToJsonLookup(id: Identifier) : JsonValue =
      match id with
      | Identifier.LocalScope name -> name |> JsonValue.String |> Json.kind "lookup" "lookup"
      | Identifier.FullyQualified(scope, name) ->
        (name :: scope |> Seq.map JsonValue.String |> Seq.toArray)
        |> JsonValue.Array
        |> Json.kind "lookup" "lookup"

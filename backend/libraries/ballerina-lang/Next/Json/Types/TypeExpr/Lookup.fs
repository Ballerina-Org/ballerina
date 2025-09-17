namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module LookupTypeExpr =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Sum.Operators
  open Ballerina.StdLib.Json.Sum
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model

  let private kindKey = "lookup"
  let private fieldKey = "lookup"

  type TypeExpr with
    static member FromJsonLookup: TypeExprParser =
      sum.AssertKindAndContinueWithField
        kindKey
        fieldKey
        (JsonValue.AsString >>= (Identifier.LocalScope >> TypeExpr.Lookup >> sum.Return))

    static member ToJsonLookup(id: Identifier) : JsonValue =
      match id with
      | Identifier.LocalScope name -> name |> JsonValue.String |> Json.kind kindKey fieldKey
      | Identifier.FullyQualified(scope, name) ->
        (name :: scope |> Seq.map JsonValue.String |> Seq.toArray)
        |> JsonValue.Array
        |> Json.kind kindKey fieldKey

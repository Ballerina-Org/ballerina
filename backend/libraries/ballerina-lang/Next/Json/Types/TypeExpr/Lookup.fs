namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module LookupTypeExpr =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina
  open Ballerina.Collections.Sum.Operators
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "lookup"

  type TypeExpr<'valueExt> with
    static member FromJsonLookup: TypeExprParser<'valueExt> =
      Sum.assertDiscriminatorAndContinueWithValue
        discriminator
        (JsonValue.AsString >>= (Identifier.LocalScope >> TypeExpr.Lookup >> sum.Return))

    static member ToJsonLookup(id: Identifier) : JsonValue =
      match id with
      | Identifier.LocalScope name -> name |> JsonValue.String |> Json.discriminator discriminator
      | Identifier.FullyQualified(scope, name) ->
        (name :: scope |> Seq.map JsonValue.String |> Seq.toArray)
        |> JsonValue.Array
        |> Json.discriminator discriminator

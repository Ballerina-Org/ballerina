namespace Ballerina.DSL.Next.Terms.Json

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module Lookup =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns

  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.Reader.WithError

  type Expr<'T> with
    static member FromJsonLookup: JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "lookup" "name" (fun nameJson ->
        reader {
          let! name = nameJson |> JsonValue.AsString |> reader.OfSum
          return Expr.Lookup(name |> Identifier.LocalScope)
        })

    static member ToJsonLookup(id: Identifier) : JsonValue =
      match id with
      | Identifier.LocalScope name -> name |> JsonValue.String |> Json.kind "lookup" "name"
      | Identifier.FullyQualified(scope, name) ->
        (name :: scope |> Seq.map JsonValue.String |> Seq.toArray)
        |> JsonValue.Array
        |> Json.kind "lookup" "name"

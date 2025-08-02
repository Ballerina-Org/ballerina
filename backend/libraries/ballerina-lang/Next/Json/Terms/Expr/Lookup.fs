namespace Ballerina.DSL.Next.Terms.Json

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module Lookup =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns

  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.Reader.WithError

  type Expr<'T> with
    static member FromJsonLookup: JsonValue -> ExprParser<'T> =
      reader.AssertKindAndContinueWithField "lookup" "name" (fun nameJson ->
        reader {
          let! name = nameJson |> JsonValue.AsString |> reader.OfSum
          return Expr.Lookup name
        })

    static member ToJsonLookup: string -> JsonValue =
      JsonValue.String >> Json.kind "lookup" "name"

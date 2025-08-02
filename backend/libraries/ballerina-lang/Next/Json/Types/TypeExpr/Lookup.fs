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

  type TypeExpr with
    static member FromJsonLookup: TypeExprParser =
      sum.AssertKindAndContinueWithField "lookup" "lookup" (JsonValue.AsString >>= (TypeExpr.Lookup >> sum.Return))

    static member ToJsonLookup: string -> JsonValue =
      JsonValue.String >> Json.kind "lookup" "lookup"

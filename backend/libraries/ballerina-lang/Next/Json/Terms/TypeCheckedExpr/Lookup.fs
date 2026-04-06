namespace Ballerina.DSL.Next.Terms.Json.TypeCheckedExpr

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module Lookup =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.Reader.WithError
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.DSL.Next.Terms.Patterns

  let private discriminator = "lookup"

  type TypeCheckedExpr<'valueExt> with
    static member FromJsonLookup(value: JsonValue) : TypeCheckedExprParserReader<'valueExt> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator value (fun nameJson ->
        reader {
          let! _, ctx = reader.GetContext()
          let! (res: ResolvedIdentifier) = nameJson |> ctx |> reader.OfSum
          return TypeCheckedExpr.Lookup(res, TypeValue.CreateUnit(), Kind.Star)
        })

    static member ToJsonLookup(id: TypeCheckedExprLookup<'valueExt>) : TypeCheckedExprEncoderReader<'valueExt> =
      reader {
        let! _, ctx = reader.GetContext()
        return id.Id |> ctx |> Json.discriminator discriminator
      }

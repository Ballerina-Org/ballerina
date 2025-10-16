namespace Ballerina.DSL.Next.Terms.Json

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module Lookup =
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.Reader.WithError
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.DSL.Next.Terms.Patterns

  let private discriminator = "lookup"

  type Expr<'T, 'Id when 'Id: comparison> with
    static member FromJsonLookup(value: JsonValue) : ExprParserReader<'T, 'Id> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator value (fun nameJson ->
        reader {
          let! _, ctx = reader.GetContext()
          let! res = nameJson |> ctx |> reader.OfSum
          return Expr.Lookup res
        })

    static member ToJsonLookup(id: 'Id) : ExprEncoderReader<'T, 'Id> =
      reader {
        let! _, ctx = reader.GetContext()
        return id |> ctx |> Json.discriminator discriminator
      }

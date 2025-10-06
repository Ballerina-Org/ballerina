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

  type Expr<'T> with
    static member FromJsonLookup(value: JsonValue) : ExprParserReader<'T> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator value (fun nameJson ->
        reader.Any2
          (reader {
            let! name = nameJson |> JsonValue.AsString |> reader.OfSum
            return Expr.Lookup(name |> Identifier.LocalScope)
          })
          (reader {
            let! path = nameJson |> JsonValue.AsArray |> reader.OfSum
            let! path = path |> Seq.map (JsonValue.AsString >> reader.OfSum) |> reader.All

            match path |> List.rev with
            | [] -> return! Errors.Singleton "Empty path in fully qualified identifier" |> reader.Throw
            | x :: xs -> return Expr.Lookup(Identifier.FullyQualified(xs, x))
          }))

    static member ToJsonLookup(id: Identifier) : ExprEncoderReader<'T> =
      (match id with
       | Identifier.LocalScope name -> name |> JsonValue.String |> Json.discriminator discriminator
       | Identifier.FullyQualified(scope, name) ->
         (name :: scope |> List.rev |> Seq.map JsonValue.String |> Seq.toArray)
         |> JsonValue.Array
         |> Json.discriminator discriminator)
      |> reader.Return

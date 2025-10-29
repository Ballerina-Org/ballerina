namespace Ballerina.DSL.Next.Terms.Json

[<AutoOpen>]
module Tuple =
  open Ballerina.Reader.WithError
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.DSL.Next.Types

  let private discriminator = "tuple"

  type Value<'T, 'valueExtension> with
    static member FromJsonTuple
      (fromJsonRoot: ValueParser<'T, ResolvedIdentifier, 'valueExtension>)
      (json: JsonValue)
      : ValueParserReader<'T, ResolvedIdentifier, 'valueExtension> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator json (fun elementsJson ->
        reader {
          let! elements = elementsJson |> JsonValue.AsArray |> reader.OfSum
          let! elements = elements |> Seq.map fromJsonRoot |> reader.All
          return Value.Tuple elements
        })

    static member ToJsonTuple
      (rootToJson: ValueEncoder<'T, 'valueExtension>)
      (elements: List<Value<'T, 'valueExtension>>)
      : ValueEncoderReader<'T> =
      reader {
        let! elements = elements |> List.map rootToJson |> reader.All
        return elements |> List.toArray |> JsonValue.Array |> Json.discriminator discriminator
      }

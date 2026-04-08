namespace Ballerina.DSL.Next.Terms.Json

[<AutoOpen>]
module Sum =
  open Ballerina.Reader.WithError
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "sum"

  type Value<'T, 'valueExtension> with
    static member FromJsonSum
      (fromJsonRoot: ValueParser<'T, ResolvedIdentifier, 'valueExtension>)
      (json: JsonValue)
      : ValueParserReader<'T, ResolvedIdentifier, 'valueExtension> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator json (fun elementsJson ->
        reader {
          let! k, n, v = elementsJson |> JsonValue.AsTriple |> reader.OfSum
          let! k = k |> JsonValue.AsInt |> reader.OfSum
          let! n = n |> JsonValue.AsInt |> reader.OfSum
          let! v = fromJsonRoot v
          return Value.Sum({ Case = k; Count = n }, v)
        })

    static member ToJsonSum
      (rootToJson: ValueEncoder<'T, 'valueExtension>)
      (selector: SumConsSelector)
      (v: Value<'T, 'valueExtension>)
      : ValueEncoderReader<'T, 'valueExtension> =
      reader {
        let i = selector.Case |> decimal |> JsonValue.Number
        let n = selector.Count |> decimal |> JsonValue.Number
        let! v = rootToJson v
        return [| i; n; v |] |> JsonValue.Array |> Json.discriminator discriminator
      }

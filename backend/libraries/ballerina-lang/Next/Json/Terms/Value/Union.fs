namespace Ballerina.DSL.Next.Terms.Json

[<AutoOpen>]
module Union =

  open FSharp.Data
  open Ballerina.Errors
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Types.Json.TypeSymbolJson
  open Ballerina.DSL.Next.Types.Json.ResolvedTypeIdentifier
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "union-case"
  let private discriminator_cons = "union-cons"

  type Value<'T, 'valueExtension> with
    static member FromJsonUnion
      (fromJsonRoot: ValueParser<'T, ResolvedIdentifier, 'valueExtension>)
      (json: JsonValue)
      : ValueParserReader<'T, ResolvedIdentifier, 'valueExtension> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator json (fun caseJson ->
        reader {
          let! k, v = caseJson |> JsonValue.AsPair |> reader.OfSum
          let! k = k |> ResolvedIdentifier.FromJson |> reader.OfSum
          let! v = fromJsonRoot v
          return Value.UnionCase(k, v)
        })

    static member ToJsonUnion
      (rootToJson: ValueEncoder<'T, 'valueExtension>)
      (k: ResolvedIdentifier)
      (v: Value<'T, 'valueExtension>)
      : ValueEncoderReader<'T> =
      reader {
        let k = ResolvedIdentifier.ToJson k
        let! v = rootToJson v
        return [| k; v |] |> JsonValue.Array |> Json.discriminator discriminator
      }

    static member FromJsonUnionCons
      (_fromJsonRoot: ValueParser<'T, ResolvedIdentifier, 'valueExtension>)
      (json: JsonValue)
      : ValueParserReader<'T, ResolvedIdentifier, 'valueExtension> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator_cons json (fun caseJson ->
        reader {
          let! k = caseJson |> ResolvedIdentifier.FromJson |> reader.OfSum
          return Value.UnionCons(k)
        })

    static member ToJsonUnionCons
      (_rootToJson: ValueEncoder<'T, 'valueExtension>)
      (k: ResolvedIdentifier)
      : ValueEncoderReader<'T> =
      reader {
        let k = ResolvedIdentifier.ToJson k
        return k |> Json.discriminator discriminator_cons
      }

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
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "union-case"
  let private discriminator_cons = "union-cons"

  type Value<'T, 'valueExtension> with
    static member FromJsonUnion
      (fromJsonRoot: ValueParser<'T, 'valueExtension>)
      (json: JsonValue)
      : ValueParserReader<'T, 'valueExtension> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator json (fun caseJson ->
        reader {
          let! k, v = caseJson |> JsonValue.AsPair |> reader.OfSum
          let! k = TypeSymbol.FromJson k |> reader.OfSum
          let! v = fromJsonRoot v
          return Value.UnionCase(k, v)
        })

    static member ToJsonUnion
      (rootToJson: ValueEncoder<'T, 'valueExtension>)
      (k: TypeSymbol)
      (v: Value<'T, 'valueExtension>)
      : ValueEncoderReader<'T> =
      reader {
        let k = TypeSymbol.ToJson k
        let! v = rootToJson v
        return [| k; v |] |> JsonValue.Array |> Json.discriminator discriminator
      }

    static member FromJsonUnionCons
      (_fromJsonRoot: ValueParser<'T, 'valueExtension>)
      (json: JsonValue)
      : ValueParserReader<'T, 'valueExtension> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator_cons json (fun caseJson ->
        reader {
          let! k = caseJson |> TypeSymbol.FromJson |> reader.OfSum
          return Value.UnionCons(k)
        })

    static member ToJsonUnionCons
      (_rootToJson: ValueEncoder<'T, 'valueExtension>)
      (k: TypeSymbol)
      : ValueEncoderReader<'T> =
      reader {
        let k = TypeSymbol.ToJson k
        return k |> Json.discriminator discriminator_cons
      }

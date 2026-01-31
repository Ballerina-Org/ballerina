namespace Ballerina.DSL.Next.StdLib.List.Json

[<AutoOpen>]
module Extension =

  open Ballerina.Reader.WithError
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types
  open FSharp.Data
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.DSL.Next.StdLib.List.Patterns
  open Ballerina.DSL.Next.StdLib.List.Model
  open Ballerina.Lenses
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Errors

  let parser
    (lens: PartialLens<'ext, ListValues<'ext>>)
    (rootValueParser: ValueParser<TypeValue<'ext>, ResolvedIdentifier, 'ext>)
    (v: JsonValue)
    : ValueParserReader<TypeValue<'ext>, ResolvedIdentifier, 'ext> =
    Reader.assertDiscriminatorAndContinueWithValue "list" v (fun elementsJson ->
      reader {
        let! elements = elementsJson |> JsonValue.AsArray |> reader.OfSum

        let! elements = elements |> Seq.map rootValueParser |> reader.All
        return (ListValues.List elements |> lens.Set, None) |> Ext
      })

  let encoder
    (lens: PartialLens<'ext, ListValues<'ext>>)
    (rootValueEncoder: ValueEncoder<TypeValue<'ext>, 'ext>)
    (v: Value<TypeValue<'ext>, 'ext>)
    : ValueEncoderReader<TypeValue<'ext>, 'ext> =

    reader {
      let! v, _ = Value.AsExt v |> reader.OfSum

      let! v =
        lens.Get v
        |> sum.OfOption((fun () -> "cannot get list value") |> Errors<Unit>.Singleton())
        |> reader.OfSum

      let! v = v |> ListValues.AsList |> reader.OfSum
      let! elements = v |> List.map rootValueEncoder |> reader.All

      return elements |> List.toArray |> JsonValue.Array |> Json.discriminator "list"
    }

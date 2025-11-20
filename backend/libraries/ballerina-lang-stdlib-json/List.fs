namespace Ballerina.DSL.Next.StdLib.List.Json

open Ballerina.Collections.Sum
open Ballerina.DSL.Next.Terms.Patterns
open Ballerina.DSL.Next.Json
open Ballerina.DSL.Next.Json.Keys
open Ballerina.DSL.Next.StdLib
open Ballerina.DSL.Next.StdLib.List.Patterns
open Ballerina.DSL.Next.StdLib.List.Model
open Ballerina.Lenses
open Ballerina.StdLib.Json.Patterns

[<AutoOpen>]
module Extension =

  open Ballerina.Reader.WithError
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types
  open FSharp.Data

  let parser
    (lens: PartialLens<'ext, ListValues<'ext>>)
    (rootValueParser: ValueParser<TypeValue, ResolvedIdentifier, 'ext>)
    (v: JsonValue)
    : ValueParserReader<TypeValue, ResolvedIdentifier, 'ext> =
    Reader.assertDiscriminatorAndContinueWithValue "list" v (fun elementsJson ->
      reader {
        let! elements = elementsJson |> JsonValue.AsArray |> reader.OfSum

        let! elements = elements |> Seq.map rootValueParser |> reader.All
        return ListValues.List elements |> lens.Set |> Ext
      })

  let encoder
    (lens: PartialLens<'ext, ListValues<'ext>>)
    (rootValueEncoder: ValueEncoder<TypeValue, 'ext>)
    (v: Value<TypeValue, 'ext>)
    : ValueEncoderReader<TypeValue, 'ext> =

    reader {
      let! v = Value.AsExt v |> reader.OfSum

      let! v =
        lens.Get v
        |> sum.OfOption("cannot get list value" |> Ballerina.Errors.Errors.Singleton)
        |> reader.OfSum

      let! v = v |> ListValues.AsList |> reader.OfSum
      let! elements = v |> List.map rootValueEncoder |> reader.All

      return elements |> List.toArray |> JsonValue.Array |> Json.discriminator "list"
    }

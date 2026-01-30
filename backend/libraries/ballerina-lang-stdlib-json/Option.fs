namespace Ballerina.DSL.Next.StdLib.Option.Json

[<AutoOpen>]
module Extension =

  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Json.Keys
  open Ballerina.DSL.Next.StdLib.Option.Patterns
  open Ballerina.DSL.Next.StdLib.Option.Model
  open Ballerina.Lenses
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types
  open Ballerina.Reader.WithError
  open FSharp.Data
  open Ballerina.Errors

  let parser<'ext>
    (lens: PartialLens<'ext, OptionValues<'ext>>)
    (rootValueParser: ValueParser<TypeValue<'ext>, ResolvedIdentifier, 'ext>)
    (v: JsonValue)
    : ValueParserReader<TypeValue<'ext>, ResolvedIdentifier, 'ext> =
    Reader.assertDiscriminatorAndContinueWithValue "option" v (fun elementJson ->
      reader {
        let opt =
          match elementJson with
          | JsonValue.Null -> None
          | jsonValue -> Some jsonValue

        let! opt = opt |> Option.map rootValueParser |> reader.RunOption
        return (OptionValues.Option opt |> lens.Set, None) |> Ext
      })

  let encoder
    (lens: PartialLens<'ext, OptionValues<'ext>>)
    (rootValueEncoder: ValueEncoder<TypeValue<'ext>, 'ext>)
    (v: Value<TypeValue<'ext>, 'ext>)
    : ValueEncoderReader<TypeValue<'ext>, 'ext> =
    reader {
      let! v, _ = Value.AsExt v |> reader.OfSum

      let! v =
        lens.Get v
        |> sum.OfOption((fun () -> "cannot get option value") |> Errors<Unit>.Singleton())
        |> reader.OfSum

      let! v = v |> OptionValues.AsOption |> reader.OfSum
      let! element = v |> Option.map rootValueEncoder |> reader.RunOption

      return
        match element with
        | Some jsonValue -> jsonValue
        | None -> JsonValue.Null
        |> Json.discriminator "option"
    }

namespace Ballerina.DSL.Next.StdLib.Option.Json

[<AutoOpen>]
module Extension =

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

  let parser<'ext>
    (lens: PartialLens<'ext, OptionValues<'ext>>)
    (rootValueParser: ValueParser<TypeValue, ResolvedIdentifier, 'ext>)
    (v: JsonValue)
    : ValueParserReader<TypeValue, ResolvedIdentifier, 'ext> =
    Reader.assertDiscriminatorAndContinueWithValue "option" v (fun elementJson ->
      reader {
        let opt =
          match elementJson with
          | JsonValue.Null -> None
          | jsonValue -> Some jsonValue

        let! opt = opt |> Option.map rootValueParser |> reader.RunOption
        return OptionValues.Option opt |> lens.Set |> Ext
      })

  let encoder
    (lens: PartialLens<'ext, OptionValues<'ext>>)
    (rootValueEncoder: ValueEncoder<TypeValue, 'ext>)
    (v: Value<TypeValue, 'ext>)
    : ValueEncoderReader<TypeValue, 'ext> =
    reader {
      let! v = Value.AsExt v |> reader.OfSum

      let! v =
        lens.Get v
        |> sum.OfOption("cannot get option value" |> Ballerina.Errors.Errors.Singleton)
        |> reader.OfSum

      let! v = v |> OptionValues.AsOption |> reader.OfSum
      let! element = v |> Option.map rootValueEncoder |> reader.RunOption

      return
        match element with
        | Some jsonValue -> jsonValue
        | None -> JsonValue.Null
        |> Json.discriminator "option"
    }

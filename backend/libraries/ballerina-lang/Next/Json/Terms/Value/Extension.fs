namespace Ballerina.DSL.Next.Terms.Json

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module Extensions =
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open Ballerina.StdLib.String
  open Ballerina.StdLib.Object
  open FSharp.Data
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Json

  type Value<'T, 'valueExtension> with
    static member FromJsonExt
      (listFromJson: ValueParser<'T, 'valueExtension>)
      (optionFromJson: ValueParser<'T, 'valueExtension>)
      (json: JsonValue)
      : ValueParserReader<'T, 'valueExtension> =
      reader.Any(
        listFromJson json,
        [ optionFromJson json
          $"FromJsonExt: Unknown Value JSON: {json.ToFSharpString.ReasonablyClamped}"
          |> Errors.Singleton
          |> Errors.WithPriority ErrorPriority.Medium
          |> reader.Throw ]
      )
      |> reader.MapError(Errors.HighestPriority)

    static member ToJsonExt
      (listToJson: ValueEncoder<'T, 'valueExtension>)
      (optionToJson: ValueEncoder<'T, 'valueExtension>)
      (value: Value<'T, 'valueExtension>)
      : ValueEncoderReader<'T> =
      reader.Any(
        listToJson value,
        [ optionToJson value
          $"ToJsonExt: Unknown Value: {value.ToFSharpString.ReasonablyClamped}"
          |> Errors.Singleton
          |> Errors.WithPriority ErrorPriority.Medium
          |> reader.Throw ]
      )
      |> reader.MapError(Errors.HighestPriority)

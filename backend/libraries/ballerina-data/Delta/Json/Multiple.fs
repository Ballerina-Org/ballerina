namespace Ballerina.DSL.Next.Delta.Json

open Ballerina.DSL.Next.Types.Model

[<AutoOpen>]
module Multiple =
  open Ballerina.Errors
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Json
  open Ballerina.Data.Delta.Model
  open FSharp.Data

  type Delta<'valueExtension> with
    static member FromJsonMultiple
      (fromJsonRoot: DeltaParser<'valueExtension>)
      (json: JsonValue)
      : DeltaParserReader<'valueExtension> =
      reader.AssertKindAndContinueWithField json "multiple" "multiple" (fun json ->
        reader {
          let! deltas = json |> JsonValue.AsArray |> reader.OfSum
          let! deltas = deltas |> Seq.map fromJsonRoot |> Seq.toList |> reader.All
          return deltas |> Seq.toList |> Delta.Multiple
        })

    static member ToJsonMultiple
      (rootToJson: DeltaEncoder<'valueExtension>)
      (deltas: List<Delta<'valueExtension>>)
      : DeltaEncoderReader<'valueExtension> =
      reader {
        let! jsonDeltas = deltas |> List.map rootToJson |> reader.All
        return jsonDeltas |> List.toArray |> JsonValue.Array |> Json.kind "multiple" "multiple"
      }

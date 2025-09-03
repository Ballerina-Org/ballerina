namespace Ballerina.DSL.Next.Delta.Json

[<AutoOpen>]
module Multiple =
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Json
  open Ballerina.Data.Delta.Model
  open FSharp.Data

  type Delta with
    static member FromJsonMultiple(fromJsonRoot: DeltaParser) : DeltaParser =
      reader.AssertKindAndContinueWithField "multiple" "multiple" (fun json ->
        reader {
          let! deltas = json |> JsonValue.AsArray |> reader.OfSum
          let! deltas = deltas |> Seq.map fromJsonRoot |> Seq.toList |> reader.All
          return deltas |> Seq.toList |> Delta.Multiple
        })

    static member ToJsonMultiple(rootToJson: Delta -> JsonValue) : List<Delta> -> JsonValue =
      fun deltas ->
        let jsonDeltas = deltas |> List.map rootToJson
        JsonValue.Array(jsonDeltas |> List.toArray) |> Json.kind "multiple" "multiple"

namespace Ballerina.DSL.Next.Delta.Json

open Ballerina.DSL.Next.Types.Model

[<AutoOpen>]
module Tuple =
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Json
  open Ballerina.Data.Delta.Model
  open FSharp.Data

  type Delta<'valueExtension> with
    static member FromJsonTuple
      (fromJsonRoot: DeltaParser<'valueExtension>)
      (json: JsonValue)
      : DeltaParserReader<'valueExtension> =
      reader.AssertKindAndContinueWithField json "tuple" "tuple" (fun json ->
        reader {
          let! fieldIndex, fieldDelta = json |> JsonValue.AsPair |> reader.OfSum
          let! fieldIndex = fieldIndex |> JsonValue.AsInt |> reader.OfSum
          let! fieldDelta = fieldDelta |> fromJsonRoot
          return Delta.Tuple(fieldIndex, fieldDelta)
        })

    static member ToJsonTuple
      (rootToJson: DeltaEncoder<'valueExtension>)
      (i: int)
      (v: Delta<'valueExtension>)
      : DeltaEncoderReader<'valueExtension> =
      reader {
        let i = i |> decimal |> JsonValue.Number
        let! v = v |> rootToJson
        return [| i; v |] |> JsonValue.Array |> Json.kind "tuple" "tuple"
      }

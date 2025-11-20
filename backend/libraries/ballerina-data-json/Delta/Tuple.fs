namespace Ballerina.DSL.Next.Delta.Json

open Ballerina.DSL.Next.Types.Model

[<AutoOpen>]
module Tuple =
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Json
  open Ballerina.Data.Delta.Model
  open Ballerina.DSL.Next.Json.Keys
  open FSharp.Data

  type Delta<'valueExtension, 'deltaExtension> with
    static member FromJsonTuple
      (fromJsonRoot: DeltaParser<'valueExtension, 'deltaExtension>)
      (json: JsonValue)
      : DeltaParserReader<'valueExtension, 'deltaExtension> =
      Reader.assertDiscriminatorAndContinueWithValue "tuple" json (fun json ->
        reader {
          let! fieldIndex, fieldDelta = json |> JsonValue.AsPair |> reader.OfSum
          let! fieldIndex = fieldIndex |> JsonValue.AsInt |> reader.OfSum
          let! fieldDelta = fieldDelta |> fromJsonRoot
          return Delta.Tuple(fieldIndex, fieldDelta)
        })

    static member ToJsonTuple
      (rootToJson: DeltaEncoder<'valueExtension, 'deltaExtension>)
      (i: int)
      (v: Delta<'valueExtension, 'deltaExtension>)
      : DeltaEncoderReader<'valueExtension, 'deltaExtension> =
      reader {
        let i = i |> decimal |> JsonValue.Number
        let! v = v |> rootToJson
        return [| i; v |] |> JsonValue.Array |> Json.discriminator "tuple"
      }

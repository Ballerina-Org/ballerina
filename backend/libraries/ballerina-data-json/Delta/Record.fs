namespace Ballerina.DSL.Next.Delta.Json

open Ballerina.DSL.Next.Types.Model

[<AutoOpen>]
module Record =
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Json
  open Ballerina.Data.Delta.Model
  open Ballerina.DSL.Next.Json.Keys
  open FSharp.Data

  type Delta<'valueExtension, 'deltaExtension> with
    static member FromJsonRecord
      (fromJsonRoot: DeltaParser<'valueExtension, 'deltaExtension>)
      (json: JsonValue)
      : DeltaParserReader<'valueExtension, 'deltaExtension> =
      Reader.assertDiscriminatorAndContinueWithValue "record" json (fun json ->
        reader {
          let! fieldName, fieldDelta = json |> JsonValue.AsPair |> reader.OfSum
          let! fieldName = fieldName |> JsonValue.AsString |> reader.OfSum
          let! fieldDelta = fieldDelta |> fromJsonRoot
          return Delta.Record(fieldName, fieldDelta)
        })

    static member ToJsonRecord
      (rootToJson: DeltaEncoder<'valueExtension, 'deltaExtension>)
      (name: string)
      (delta: Delta<'valueExtension, 'deltaExtension>)
      : DeltaEncoderReader<'valueExtension, 'deltaExtension> =
      reader {
        let name = name |> JsonValue.String
        let! delta = delta |> rootToJson
        return [| name; delta |] |> JsonValue.Array |> Json.discriminator "record"
      }

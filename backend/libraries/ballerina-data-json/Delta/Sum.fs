namespace Ballerina.DSL.Next.Delta.Json

open Ballerina.DSL.Next.Types.Model

[<AutoOpen>]
module Sum =
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Json
  open Ballerina.Data.Delta.Model
  open Ballerina.DSL.Next.Json.Keys
  open FSharp.Data

  type Delta<'valueExtension, 'deltaExtension> with
    static member FromJsonSum
      (fromJsonRoot: DeltaParser<'valueExtension, 'deltaExtension>)
      (json: JsonValue)
      : DeltaParserReader<'valueExtension, 'deltaExtension> =
      Reader.assertDiscriminatorAndContinueWithValue "sum" json (fun json ->
        reader {
          let! caseIndex, caseDelta = json |> JsonValue.AsPair |> reader.OfSum
          let! caseIndex = caseIndex |> JsonValue.AsInt |> reader.OfSum
          let! caseDelta = caseDelta |> fromJsonRoot
          return Delta.Sum(caseIndex, caseDelta)
        })

    static member ToJsonSum
      (rootToJson: DeltaEncoder<'valueExtension, 'deltaExtension>)
      (i: int)
      (v: Delta<'valueExtension, 'deltaExtension>)
      : DeltaEncoderReader<'valueExtension, 'deltaExtension> =
      reader {
        let i = i |> decimal |> JsonValue.Number
        let! v = v |> rootToJson
        return [| i; v |] |> JsonValue.Array |> Json.discriminator "sum"
      }

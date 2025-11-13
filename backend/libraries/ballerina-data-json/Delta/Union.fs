namespace Ballerina.DSL.Next.Delta.Json

open Ballerina.DSL.Next.Types.Model

[<AutoOpen>]
module Union =
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Json
  open Ballerina.Data.Delta.Model
  open Ballerina.Errors
  open Ballerina.DSL.Next.Json.Keys
  open FSharp.Data

  type Delta<'valueExtension, 'deltaExtension> with
    static member FromJsonUnion
      (fromJsonRoot: DeltaParser<'valueExtension, 'deltaExtension>)
      (json: JsonValue)
      : DeltaParserReader<'valueExtension, 'deltaExtension> =
      Reader.assertDiscriminatorAndContinueWithValue "union" json (fun json ->
        reader {
          let! caseName, caseDelta = json |> JsonValue.AsPair |> reader.OfSum
          let! caseName = caseName |> JsonValue.AsString |> reader.OfSum
          let! caseDelta = caseDelta |> fromJsonRoot
          return Delta.Union(caseName, caseDelta)
        })

    static member ToJsonUnion
      (rootToJson: DeltaEncoder<'valueExtension, 'deltaExtension>)
      (caseName: string)
      (caseDelta: Delta<'valueExtension, 'deltaExtension>)
      : DeltaEncoderReader<'valueExtension, 'deltaExtension> =
      reader {
        let caseName = caseName |> JsonValue.String
        let! caseDelta = caseDelta |> rootToJson

        return [| caseName; caseDelta |] |> JsonValue.Array |> Json.discriminator "union"
      }

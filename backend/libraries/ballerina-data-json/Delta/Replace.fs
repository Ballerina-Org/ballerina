namespace Ballerina.DSL.Next.Delta.Json

[<AutoOpen>]
module Replace =
  open Ballerina.Errors
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Json
  open Ballerina.Data.Delta.Model
  open Ballerina.DSL.Next.Terms.Json
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Json.Keys
  open FSharp.Data

  type Delta<'valueExtension, 'deltaExtension> with
    static member FromJsonReplace(json: JsonValue) : DeltaParserReader<'valueExtension, 'deltaExtension> =
      Reader.assertDiscriminatorAndContinueWithValue "replace" json (fun json ->
        reader {
          let! ctx, _ = reader.GetContext()
          let! value = ctx json |> reader.OfSum
          return value |> Delta.Replace
        })

    static member ToJsonReplace
      (value: Value<TypeValue<'valueExtension>, 'valueExtension>)
      : DeltaEncoderReader<'valueExtension, 'deltaExtension> =
      reader {
        let! rootToJson, _ = reader.GetContext()
        let! value = value |> rootToJson |> reader.OfSum
        return value |> Json.discriminator "replace"
      }

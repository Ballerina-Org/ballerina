namespace Ballerina.DSL.Next.Delta.Json

[<AutoOpen>]
module Replace =
  open Ballerina.Errors
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Json
  open Ballerina.Data.Delta.Model
  open Ballerina.DSL.Next.Terms.Json
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Types.Model
  open FSharp.Data

  type Delta<'valueExtension> with
    static member FromJsonReplace(json: JsonValue) : DeltaParserReader<'valueExtension> =
      reader.AssertKindAndContinueWithField json "replace" "replace" (fun json ->
        reader {
          let! ctx = reader.GetContext()
          let! value = ctx json |> reader.OfSum
          return value |> Delta.Replace
        })

    static member ToJsonReplace(value: Value<TypeValue, 'valueExtension>) : DeltaEncoderReader<'valueExtension> =
      reader {
        let! rootToJson = reader.GetContext()
        let! value = value |> rootToJson |> reader.OfSum
        return value |> Json.kind "replace" "replace"
      }

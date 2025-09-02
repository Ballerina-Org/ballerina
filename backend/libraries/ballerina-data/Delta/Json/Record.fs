namespace Ballerina.DSL.Next.Delta.Json

[<AutoOpen>]
module Record =
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Json
  open Ballerina.Data.Delta.Model
  open FSharp.Data

  type Delta with
    static member FromJsonRecord(fromJsonRoot: DeltaParser) : DeltaParser =
      reader.AssertKindAndContinueWithField "record" "record" (fun json ->
        reader {
          let! fieldName, fieldDelta = json |> JsonValue.AsPair |> reader.OfSum
          let! fieldName = fieldName |> JsonValue.AsString |> reader.OfSum
          let! fieldDelta = fieldDelta |> fromJsonRoot
          return Delta.Record(fieldName, fieldDelta)
        })

    static member ToJsonRecord(rootToJson: Delta -> JsonValue) : string * Delta -> JsonValue =
      fun (caseName, caseDelta) ->
        let caseName = caseName |> JsonValue.String
        let caseDelta = caseDelta |> rootToJson
        [| caseName; caseDelta |] |> JsonValue.Array |> Json.kind "record" "record"

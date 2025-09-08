namespace Ballerina.DSL.Next.Delta.Json

[<AutoOpen>]
module Tuple =
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Json
  open Ballerina.Data.Delta.Model
  open FSharp.Data

  type Delta<'valueExtension> with
    static member FromJsonTuple(fromJsonRoot: DeltaParser<'valueExtension>) : DeltaParser<'valueExtension> =
      reader.AssertKindAndContinueWithField "tuple" "tuple" (fun json ->
        reader {
          let! fieldIndex, fieldDelta = json |> JsonValue.AsPair |> reader.OfSum
          let! fieldIndex = fieldIndex |> JsonValue.AsInt |> reader.OfSum
          let! fieldDelta = fieldDelta |> fromJsonRoot
          return Delta.Tuple(fieldIndex, fieldDelta)
        })

    static member ToJsonTuple
      (rootToJson: Delta<'valueExtension> -> JsonValue)
      : int * Delta<'valueExtension> -> JsonValue =
      fun (caseName, caseDelta) ->
        let caseName = caseName |> decimal |> JsonValue.Number
        let caseDelta = caseDelta |> rootToJson
        [| caseName; caseDelta |] |> JsonValue.Array |> Json.kind "tuple" "tuple"

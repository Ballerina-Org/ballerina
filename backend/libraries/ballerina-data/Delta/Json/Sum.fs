namespace Ballerina.DSL.Next.Delta.Json

[<AutoOpen>]
module Sum =
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Json
  open Ballerina.Data.Delta.Model
  open FSharp.Data

  type Delta with
    static member FromJsonSum(fromJsonRoot: DeltaParser) : DeltaParser =
      reader.AssertKindAndContinueWithField "sum" "sum" (fun json ->
        reader {
          let! caseIndex, caseDelta = json |> JsonValue.AsPair |> reader.OfSum
          let! caseIndex = caseIndex |> JsonValue.AsInt |> reader.OfSum
          let! caseDelta = caseDelta |> fromJsonRoot
          return Delta.Sum(caseIndex, caseDelta)
        })

    static member ToJsonSum(rootToJson: Delta -> JsonValue) : int * Delta -> JsonValue =
      fun (caseName, caseDelta) ->
        let caseName = caseName |> decimal |> JsonValue.Number
        let caseDelta = caseDelta |> rootToJson
        [| caseName; caseDelta |] |> JsonValue.Array |> Json.kind "sum" "sum"

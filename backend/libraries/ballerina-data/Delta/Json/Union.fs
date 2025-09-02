namespace Ballerina.DSL.Next.Delta.Json

[<AutoOpen>]
module Union =
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Json
  open Ballerina.Data.Delta.Model

  open FSharp.Data

  type Delta with
    static member FromJsonUnion(fromJsonRoot: DeltaParser) : DeltaParser =
      reader.AssertKindAndContinueWithField "union" "union" (fun json ->
        reader {
          let! caseName, caseDelta = json |> JsonValue.AsPair |> reader.OfSum
          let! caseName = caseName |> JsonValue.AsString |> reader.OfSum
          let! caseDelta = caseDelta |> fromJsonRoot
          return Delta.Union(caseName, caseDelta)
        })

    static member ToJsonUnion(rootToJson: Delta -> JsonValue) : string * Delta -> JsonValue =
      fun (caseName, caseDelta) ->
        let caseName = caseName |> JsonValue.String
        let caseDelta = caseDelta |> rootToJson
        [| caseName; caseDelta |] |> JsonValue.Array |> Json.kind "union" "union"

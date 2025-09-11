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

  open FSharp.Data

  type Delta<'valueExtension> with
    static member FromJsonUnion(fromJsonRoot: DeltaParser<'valueExtension>) : DeltaParser<'valueExtension> =
      reader.AssertKindAndContinueWithField "union" "union" (fun json ->
        reader {
          let! caseName, caseDelta = json |> JsonValue.AsPair |> reader.OfSum
          let! caseName = caseName |> JsonValue.AsString |> reader.OfSum
          let! caseDelta = caseDelta |> fromJsonRoot
          return Delta.Union(caseName, caseDelta)
        })

    static member ToJsonUnion
      : DeltaEncoder<'valueExtension> -> string -> Delta<'valueExtension> -> JsonEncoder<TypeValue, 'valueExtension> =
      fun rootToJson caseName caseDelta ->
        reader {
          let caseName = caseName |> JsonValue.String
          let! caseDelta = caseDelta |> rootToJson
          return [| caseName; caseDelta |] |> JsonValue.Array |> Json.kind "union" "union"
        }

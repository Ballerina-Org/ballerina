namespace Ballerina.DSL.Next.Delta.Json

open Ballerina.DSL.Next.Types.Model

[<AutoOpen>]
module Record =
  open Ballerina.Reader.WithError
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Json
  open Ballerina.Data.Delta.Model
  open FSharp.Data

  type Delta<'valueExtension> with
    static member FromJsonRecord(fromJsonRoot: DeltaParser<'valueExtension>) : DeltaParser<'valueExtension> =
      reader.AssertKindAndContinueWithField "record" "record" (fun json ->
        reader {
          let! fieldName, fieldDelta = json |> JsonValue.AsPair |> reader.OfSum
          let! fieldName = fieldName |> JsonValue.AsString |> reader.OfSum
          let! fieldDelta = fieldDelta |> fromJsonRoot
          return Delta.Record(fieldName, fieldDelta)
        })

    static member ToJsonRecord
      : DeltaEncoder<'valueExtension> -> string -> Delta<'valueExtension> -> JsonEncoder<TypeValue, 'valueExtension> =
      fun rootToJson name delta ->
        reader {
          let name = name |> JsonValue.String
          let! delta = delta |> rootToJson
          return [| name; delta |] |> JsonValue.Array |> Json.kind "record" "record"
        }

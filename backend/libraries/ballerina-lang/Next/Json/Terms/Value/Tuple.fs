namespace Ballerina.DSL.Next.Terms.Json

[<AutoOpen>]
module Tuple =
  open Ballerina.Reader.WithError
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Json

  type FromJsonRoot<'T, 'valueExtension> = JsonValue -> Reader<Value<'T, 'valueExtension>, JsonParser<'T>, Errors>

  type Value<'T, 'valueExtension> with
    static member FromJsonTuple
      (fromJsonRoot: FromJsonRoot<'T, 'valueExtension>)
      : JsonValue -> ValueParser<'T, 'valueExtension> =
      fun json ->
        reader {
          return!
            reader.AssertKindAndContinueWithField
              "tuple"
              "elements"
              (fun elementsJson ->
                reader {
                  let! elements = elementsJson |> JsonValue.AsArray |> reader.OfSum
                  let! elements = elements |> Seq.map fromJsonRoot |> reader.All
                  return Value.Tuple elements
                })
              (json)
        }

    static member ToJsonTuple
      (rootToJson: Value<'T, 'valueExtension> -> JsonValue)
      : Value<'T, 'valueExtension> list -> JsonValue =
      fun elements ->
        let elementsJson = elements |> Seq.map rootToJson
        elementsJson |> Seq.toArray |> JsonValue.Array |> Json.kind "tuple" "elements"

namespace Ballerina.DSL.Next.Terms.Json

[<AutoOpen>]
module Sum =
  open Ballerina.Reader.WithError
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Json

  let private kindKey = "sum"
  let private fieldKey = "case"

  type Value<'T, 'valueExtension> with
    static member FromJsonSum
      (fromJsonRoot: ValueParser<'T, 'valueExtension>)
      (json: JsonValue)
      : ValueParserReader<'T, 'valueExtension> =
      reader.AssertKindAndContinueWithField json kindKey fieldKey (fun elementsJson ->
        reader {
          let! k, v = elementsJson |> JsonValue.AsPair |> reader.OfSum
          let! k = k |> JsonValue.AsInt |> reader.OfSum
          let! v = fromJsonRoot v
          return Value.Sum(k, v)
        })

    static member ToJsonSum
      (rootToJson: ValueEncoder<'T, 'valueExtension>)
      (i: int)
      (v: Value<'T, 'valueExtension>)
      : ValueEncoderReader<'T> =
      reader {
        let i = i |> decimal |> JsonValue.Number
        let! v = rootToJson v
        return [| i; v |] |> JsonValue.Array |> Json.kind kindKey fieldKey
      }

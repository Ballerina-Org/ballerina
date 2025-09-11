namespace Ballerina.DSL.Next.Terms.Json

[<AutoOpen>]
module Sum =
  open Ballerina.Reader.WithError
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Json

  type Value<'T, 'valueExtension> with
    static member FromJsonSum
      (fromJsonRoot: FromJsonRoot<'T, 'valueExtension>)
      : JsonValue -> ValueParser<'T, 'valueExtension> =
      fun json ->
        reader {
          return!
            reader.AssertKindAndContinueWithField
              "sum"
              "case"
              (fun elementsJson ->
                reader {
                  let! k, v = elementsJson |> JsonValue.AsPair |> reader.OfSum
                  let! k = k |> JsonValue.AsInt |> reader.OfSum
                  let! v = (fromJsonRoot v)
                  return Value.Sum(k, v)
                })
              json
        }

    static member ToJsonSum
      : ValueEncoder<'T, 'valueExtension> -> int -> Value<'T, 'valueExtension> -> JsonEncoder<'T, 'valueExtension> =
      fun rootToJson i v ->
        reader {
          let i = JsonValue.Number(decimal i)
          let! v = rootToJson v
          return [| i; v |] |> JsonValue.Array |> Json.kind "sum" "case"
        }

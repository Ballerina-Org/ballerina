namespace Ballerina.DSL.Next.Terms.Json

[<AutoOpen>]
module Sum =
  open Ballerina.Reader.WithError
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Json

  type Value<'T> with
    static member FromJsonSum(fromJsonRoot: FromJsonRoot<'T>) : JsonValue -> ValueParser<'T> =
      fun json ->
        reader {
          return!
            reader.AssertKindAndContinueWithField
              "sum"
              "case"
              (fun elementsJson ->
                reader {
                  let! (k, v) = elementsJson |> JsonValue.AsPair |> reader.OfSum
                  let! k = k |> JsonValue.AsInt |> reader.OfSum
                  let! v = (fromJsonRoot v)
                  return Value.Sum(k, v)
                })
              (json)
        }

    static member ToJsonSum(rootToJson: Value<'T> -> JsonValue) : int * Value<'T> -> JsonValue =
      fun (i, v) ->
        let i = JsonValue.Number(decimal i)
        let v = rootToJson v
        [| i; v |] |> JsonValue.Array |> Json.kind "sum" "case"

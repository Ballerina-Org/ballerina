namespace Ballerina.DSL.Next.Terms.Json

[<AutoOpen>]
module Union =
  open Ballerina.Reader.WithError
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Types.Json.TypeSymbolJson
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Types.Model

  type Value<'T, 'valueExtension> with
    static member FromJsonUnion
      (fromJsonRoot: JsonValue -> ValueParser<'T, 'valueExtension>)
      : JsonValue -> ValueParser<'T, 'valueExtension> =
      fun json ->
        reader {
          return!
            reader.AssertKindAndContinueWithField
              "union-case"
              "union-case"
              (fun caseJson ->
                reader {
                  let! (k, v) = caseJson |> JsonValue.AsPair |> reader.OfSum
                  let! k = TypeSymbol.FromJson k |> reader.OfSum
                  let! v = (fromJsonRoot v)
                  return Value.UnionCase(k, v)
                })
              (json)
        }

    static member ToJsonUnion(rootToJson) : TypeSymbol * Value<'T, 'valueExtension> -> JsonValue =
      fun (k, v) ->
        let k = TypeSymbol.ToJson k
        let v = rootToJson v
        [| k; v |] |> JsonValue.Array |> Json.kind "union-case" "union-case"

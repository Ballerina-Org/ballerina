namespace Ballerina.DSL.Next.Terms.Json

[<AutoOpen>]
module Record =
  open Ballerina.Reader.WithError
  open FSharp.Data
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.StdLib.Json.Reader
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Json
  open Ballerina.DSL.Next.Json

  type FromJsonRoot<'T, 'valueExtension> = JsonValue -> Reader<Value<'T, 'valueExtension>, JsonParser<'T>, Errors>

  type Value<'T, 'valueExtension> with
    static member FromJsonRecord
      (fromJsonRoot: FromJsonRoot<'T, 'valueExtension>)
      : JsonValue -> ValueParser<'T, 'valueExtension> =
      fun json ->
        reader {

          return!
            reader.AssertKindAndContinueWithField
              "record"
              "fields"
              (fun fieldsJson ->
                reader {
                  let! fields = fieldsJson |> JsonValue.AsArray |> reader.OfSum

                  let! fields =
                    fields
                    |> Seq.map (fun field ->
                      reader {
                        let! (k, v) = field |> JsonValue.AsPair |> reader.OfSum
                        let! k = TypeSymbol.FromJson k |> reader.OfSum
                        let! v = (fromJsonRoot v)
                        return (k, v)
                      })
                    |> reader.All
                    |> reader.Map Map.ofSeq

                  return Value.Record(fields)
                })
              (json)
        }

    static member ToJsonRecord
      (toRootJson: Value<'T, 'valueExtension> -> JsonValue)
      : Map<TypeSymbol, Value<'T, 'valueExtension>> -> JsonValue =
      fun fields ->
        let fieldsJson =
          fields
          |> Map.toList
          |> List.map (fun (ts, v) ->

            let k = TypeSymbol.ToJson ts
            let v = toRootJson v
            [| k; v |] |> JsonValue.Array)

        JsonValue.Array(fieldsJson |> Array.ofSeq) |> Json.kind "record" "fields"

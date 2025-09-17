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

  let private kindKey = "record"
  let private fieldKey = "fields"

  type Value<'T, 'valueExtension> with
    static member FromJsonRecord
      (fromJsonRoot: ValueParser<'T, 'valueExtension>)
      (json: JsonValue)
      : ValueParserReader<'T, 'valueExtension> =
      reader.AssertKindAndContinueWithField json kindKey fieldKey (fun fieldsJson ->
        reader {
          let! fields = fieldsJson |> JsonValue.AsArray |> reader.OfSum

          let! fields =
            fields
            |> Seq.map (fun field ->
              reader {
                let! k, v = field |> JsonValue.AsPair |> reader.OfSum
                let! k = TypeSymbol.FromJson k |> reader.OfSum
                let! v = fromJsonRoot v
                return k, v
              })
            |> reader.All
            |> reader.Map Map.ofSeq

          return Value.Record(fields)
        })

    static member ToJsonRecord
      (rootToJson: ValueEncoder<'T, 'valueExtension>)
      (fields: Map<TypeSymbol, Value<'T, 'valueExtension>>)
      : ValueEncoderReader<'T> =
      reader {
        let! fields =
          fields
          |> Map.toList
          |> List.map (fun (ts, v) ->
            reader {
              let k = TypeSymbol.ToJson ts
              let! v = rootToJson v
              return [| k; v |] |> JsonValue.Array
            })
          |> reader.All

        return JsonValue.Array(fields |> List.toArray) |> Json.kind kindKey fieldKey
      }

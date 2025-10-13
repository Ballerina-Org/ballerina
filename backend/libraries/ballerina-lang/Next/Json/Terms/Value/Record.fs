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
  open Ballerina.DSL.Next.Json.Keys

  let private discriminator = "record"
  let private discriminator_des = "record_des"

  type Value<'T, 'valueExtension> with
    static member FromJsonRecord
      (fromJsonRoot: ValueParser<'T, 'valueExtension>)
      (json: JsonValue)
      : ValueParserReader<'T, 'valueExtension> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator json (fun fieldsJson ->
        reader {
          let! fields = fieldsJson |> JsonValue.AsArray |> reader.OfSum

          let! fields =
            fields
            |> Seq.map (fun field ->
              reader {
                let! k, v = field |> JsonValue.AsPair |> reader.OfSum
                let! k = Identifier.FromJson k |> reader.OfSum
                let! v = fromJsonRoot v
                return k, v
              })
            |> reader.All
            |> reader.Map Map.ofSeq

          return Value.Record(fields)
        })

    static member ToJsonRecord
      (rootToJson: ValueEncoder<'T, 'valueExtension>)
      (fields: Map<Identifier, Value<'T, 'valueExtension>>)
      : ValueEncoderReader<'T> =
      reader {
        let! fields =
          fields
          |> Map.toList
          |> List.map (fun (ts, v) ->
            reader {
              let k = Identifier.ToJson ts
              let! v = rootToJson v
              return [| k; v |] |> JsonValue.Array
            })
          |> reader.All

        return JsonValue.Array(fields |> List.toArray) |> Json.discriminator discriminator
      }


    static member FromJsonRecordDes
      (_fromJsonRoot: ValueParser<'T, 'valueExtension>)
      (json: JsonValue)
      : ValueParserReader<'T, 'valueExtension> =
      Reader.assertDiscriminatorAndContinueWithValue discriminator_des json (fun caseJson ->
        reader {
          let! k = caseJson |> TypeSymbol.FromJson |> reader.OfSum
          return Value.UnionCons(k)
        })

    static member ToJsonRecordDes
      (_rootToJson: ValueEncoder<'T, 'valueExtension>)
      (k: TypeSymbol)
      : ValueEncoderReader<'T> =
      reader {
        let k = TypeSymbol.ToJson k
        return k |> Json.discriminator discriminator_des
      }

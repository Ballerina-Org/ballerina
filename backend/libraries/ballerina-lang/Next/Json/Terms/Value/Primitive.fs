namespace Ballerina.DSL.Next.Terms.Json

[<AutoOpen>]
module PrimitiveValue =
  open Ballerina.Reader.WithError
  open Ballerina.Reader.WithError.Operators
  open FSharp.Data
  open Ballerina.DSL.Next.Json
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Json.Primitive

  type Value<'T, 'valueExtension> with
    static member FromJsonPrimitive(json: JsonValue) : ValueParserReader<'T, 'valueExtension> =
      reader {
        let! primitive = PrimitiveValue.FromJson json |> reader.OfSum
        return Value.Primitive primitive
      }

    static member ToJsonPrimitive(primitive: PrimitiveValue) : ValueEncoderReader<'T> =
      let primitive = PrimitiveValue.ToJson primitive
      primitive |> reader.Return

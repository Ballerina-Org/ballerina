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
    static member FromJsonPrimitive: JsonValue -> ValueParser<'T, 'valueExtension> =
      (PrimitiveValue.FromJson >> reader.OfSum)
      >>= (fun primitive -> reader.Return(Value.Primitive primitive))

    static member ToJsonPrimitive: PrimitiveValue -> JsonEncoder<'T, 'valueExtension> =
      PrimitiveValue.ToJson >> reader.Return

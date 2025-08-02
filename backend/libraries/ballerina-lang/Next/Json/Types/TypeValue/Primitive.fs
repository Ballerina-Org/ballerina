namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module PrimitiveTypeValue =
  open FSharp.Data
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Sum.Operators
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Json

  type TypeValue with
    static member FromJsonPrimitive =
      PrimitiveType.FromJson >>= (TypeValue.Primitive >> sum.Return)

    static member ToJsonPrimitive: PrimitiveType -> JsonValue = PrimitiveType.ToJson

namespace Ballerina.DSL.Next.Types.Json

open Ballerina.Errors

[<AutoOpen>]
module PrimitiveTypeValue =
  open FSharp.Data
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina
  open Ballerina.Collections.Sum.Operators
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Json
  open Ballerina.DSL.Next.Types.Patterns

  type TypeValue<'valueExt> with
    static member FromJsonPrimitive: JsonValue -> Sum<TypeValue<'valueExt>, Errors<_>> =
      PrimitiveType.FromJson >>= (TypeValue.CreatePrimitive >> sum.Return)

    static member ToJsonPrimitive: PrimitiveType -> JsonValue = PrimitiveType.ToJson

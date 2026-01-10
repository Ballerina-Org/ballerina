namespace Ballerina.DSL.Next.Types.Json

open Ballerina.Errors

[<AutoOpen>]
module PrimitiveTypeValue =
  open FSharp.Data
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Sum.Operators
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Json
  open Ballerina.DSL.Next.Types.Patterns

  type TypeValue<'valueExt> with
    static member FromJsonPrimitive: JsonValue -> Sum<TypeValue<'valueExt>, Errors> =
      PrimitiveType.FromJson >>= (TypeValue.CreatePrimitive >> sum.Return)

    static member ToJsonPrimitive: PrimitiveType -> JsonValue = PrimitiveType.ToJson

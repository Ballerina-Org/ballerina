namespace Ballerina.DSL.Next.Types.Json

[<AutoOpen>]
module PrimitiveTypeExpr =
  open FSharp.Data
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Sum.Operators
  open Ballerina.DSL.Next.Types.Model

  type TypeExpr with
    static member FromJsonPrimitive =
      PrimitiveType.FromJson >>= (TypeExpr.Primitive >> sum.Return)

    static member ToJsonPrimitive: PrimitiveType -> JsonValue = PrimitiveType.ToJson

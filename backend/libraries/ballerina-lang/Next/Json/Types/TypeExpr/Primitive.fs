namespace Ballerina.DSL.Next.Types.Json

open Ballerina.Errors

[<AutoOpen>]
module PrimitiveTypeExpr =
  open FSharp.Data
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Sum.Operators
  open Ballerina.DSL.Next.Types.Model

  type TypeExpr<'valueExt> with
    static member FromJsonPrimitive: JsonValue -> Sum<TypeExpr<'valueExt>, Errors> =
      PrimitiveType.FromJson >>= (TypeExpr.Primitive >> sum.Return)

    static member ToJsonPrimitive: PrimitiveType -> JsonValue = PrimitiveType.ToJson

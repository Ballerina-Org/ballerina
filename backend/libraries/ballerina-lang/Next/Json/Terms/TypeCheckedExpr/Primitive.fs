namespace Ballerina.DSL.Next.Terms.Json.TypeCheckedExpr

open Ballerina.DSL.Next.Json
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Terms.Json
open Ballerina.DSL.Next.Types

[<AutoOpen>]
module PrimitiveExpr =
  open FSharp.Data
  open Ballerina.Errors
  open Ballerina.Reader.WithError
  open Ballerina.Reader.WithError.Operators
  open Ballerina.DSL.Next.Terms.Json.Primitive
  open Ballerina.DSL.Next.Terms.Patterns

  type TypeCheckedExpr<'valueExt> with
    static member FromJsonPrimitive: TypeCheckedExprParser<'valueExt> =
      PrimitiveValue.FromJson
      >> reader.OfSum
      >>= fun primitive -> reader.Return(TypeCheckedExpr.Primitive(primitive, TypeValue.CreateUnit(), Kind.Star))

    static member ToJsonPrimitive: PrimitiveValue -> TypeCheckedExprEncoderReader<'valueExt> =
      PrimitiveValue.ToJson >> reader.Return

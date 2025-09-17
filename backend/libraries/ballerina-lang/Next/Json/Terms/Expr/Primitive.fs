namespace Ballerina.DSL.Next.Terms.Json

open Ballerina.DSL.Next.Json
open Ballerina.DSL.Next.Terms.Model

[<AutoOpen>]
module PrimitiveExpr =
  open FSharp.Data
  open Ballerina.Errors
  open Ballerina.Reader.WithError
  open Ballerina.Reader.WithError.Operators
  open Ballerina.DSL.Next.Terms.Json.Primitive

  type Expr<'T> with
    static member FromJsonPrimitive: ExprParser<'T> =
      PrimitiveValue.FromJson
      >> reader.OfSum
      >>= fun primitive -> reader.Return(Expr.Primitive primitive)

    static member ToJsonPrimitive: PrimitiveValue -> ExprEncoderReader<'T> =
      PrimitiveValue.ToJson >> reader.Return

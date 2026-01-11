namespace Ballerina.DSL.Next.Terms.Json

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

  type Expr<'T, 'Id, 'valueExt when 'Id: comparison> with
    static member FromJsonPrimitive: ExprParser<'T, 'Id, 'valueExt> =
      PrimitiveValue.FromJson
      >> reader.OfSum
      >>= fun primitive -> reader.Return(Expr.Primitive primitive)

    static member ToJsonPrimitive: PrimitiveValue -> ExprEncoderReader<'T, 'Id> =
      PrimitiveValue.ToJson >> reader.Return

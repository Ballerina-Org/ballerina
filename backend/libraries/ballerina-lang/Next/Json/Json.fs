namespace Ballerina.DSL.Next.Json

open Ballerina.DSL.Next.Terms.Model
open Ballerina.Errors
open Ballerina.Collections.Sum
open Ballerina.Reader.WithError
open Ballerina.DSL.Next.Types.Model
open FSharp.Data

type ValueParser<'valueExtension> = JsonValue -> Sum<Value<TypeValue, 'valueExtension>, Errors>
type JsonParser<'T> = JsonValue -> Sum<'T, Errors>

type JsonEncoder<'T> = 'T -> JsonValue

type ValueParser<'T, 'valueExtension> = Reader<Value<'T, 'valueExtension>, JsonParser<'T>, Errors>
type ExprParser<'T> = Reader<Expr<'T>, JsonParser<'T>, Errors>

type JsonEncoder<'T, 'valueExtension> = Reader<JsonValue, JsonEncoder<'T> * JsonEncoder<'valueExtension>, Errors>
type ValueEncoder<'T, 'valueExtension> = Value<'T, 'valueExtension> -> JsonEncoder<'T, 'valueExtension>
type ExprEncoder<'T> = Expr<'T> -> Reader<JsonValue, JsonEncoder<'T>, Errors>

type TypeExprParser = JsonValue -> Sum<TypeExpr, Errors>

module Json =
  let kind (kind: string) (fields: string) (value: JsonValue) =
    JsonValue.Record [| "kind", JsonValue.String kind; fields, value |]

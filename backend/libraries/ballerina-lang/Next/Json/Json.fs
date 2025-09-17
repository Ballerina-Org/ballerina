namespace Ballerina.DSL.Next.Json

open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Types.Model
open Ballerina.Errors
open Ballerina.Collections.Sum
open Ballerina.Reader.WithError
open FSharp.Data
open Ballerina.Fix
open Ballerina.Collections.NonEmptyList

// parsing
type JsonParser<'T> = JsonValue -> Sum<'T, Errors>

type ValueParserReader<'T, 'valueExtension> =
  Reader<Value<'T, 'valueExtension>, JsonParser<Expr<'T>> * JsonParser<'T>, Errors>

type ExprParserReader<'T> = Reader<Expr<'T>, JsonParser<'T>, Errors>

type ValueParser<'T, 'valueExtension> = JsonValue -> ValueParserReader<'T, 'valueExtension>
type ExprParser<'T> = JsonValue -> ExprParserReader<'T>

type TypeExprParser = JsonParser<TypeExpr>

type ValueParserLayer<'T, 'valueExtension> = ValueParser<'T, 'valueExtension> -> ValueParser<'T, 'valueExtension>

// encoding/serializing
type JsonEncoder<'T> = 'T -> JsonValue
type JsonEncoderWithError<'T> = 'T -> Sum<JsonValue, Errors>

type ExprEncoderReader<'T> = Reader<JsonValue, JsonEncoder<'T>, Errors>
type ExprEncoder<'T> = Expr<'T> -> ExprEncoderReader<'T>

type ValueEncoderReader<'T> = Reader<JsonValue, JsonEncoderWithError<Expr<'T>> * JsonEncoder<'T>, Errors>
type ValueEncoder<'T, 'valueExtension> = Value<'T, 'valueExtension> -> ValueEncoderReader<'T>

type ValueEncoderLayer<'T, 'valueExtension> = ValueEncoder<'T, 'valueExtension> -> ValueEncoder<'T, 'valueExtension>

module Json =
  let kind (kind: string) (fields: string) (value: JsonValue) =
    JsonValue.Record [| "kind", JsonValue.String kind; fields, value |]

  let buildRootParser<'T, 'valueExtension>
    (layers: NonEmptyList<ValueParserLayer<'T, 'valueExtension>>)
    : ValueParser<'T, 'valueExtension> =
    let F
      (layers: NonEmptyList<ValueParserLayer<'T, 'valueExtension>>)
      (self: ValueParser<'T, 'valueExtension>)
      : ValueParser<'T, 'valueExtension> =
      fun data -> reader.Any(layers |> NonEmptyList.map (fun layer -> layer self data))

    let parsingOperation = F layers
    fix parsingOperation

  let buildRootEncoder<'T, 'valueExtension>
    (layers: NonEmptyList<ValueEncoderLayer<'T, 'valueExtension>>)
    : ValueEncoder<'T, 'valueExtension> =
    let F
      (layers: NonEmptyList<ValueEncoderLayer<'T, 'valueExtension>>)
      (self: ValueEncoder<'T, 'valueExtension>)
      : ValueEncoder<'T, 'valueExtension> =
      fun value -> reader.Any(layers |> NonEmptyList.map (fun layer -> layer self value))

    let encodingOperation = F layers
    fix encodingOperation

namespace Ballerina.DSL.Next.Json

open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Types.Model
open Ballerina.Errors
open Ballerina.Collections.Sum
open Ballerina.Reader.WithError
open FSharp.Data
open Ballerina.Fix
open Ballerina.Collections.NonEmptyList
open Keys

// parsing
type JsonParser<'T> = JsonValue -> Sum<'T, Errors>

type ValueParserReader<'T, 'Id, 'valueExtension when 'Id: comparison> =
  Reader<Value<'T, 'valueExtension>, JsonParser<Expr<'T, 'Id>> * JsonParser<'T> * JsonParser<'Id>, Errors>

type ExprParserReader<'T, 'Id when 'Id: comparison> = Reader<Expr<'T, 'Id>, JsonParser<'T> * JsonParser<'Id>, Errors>

type ValueParser<'T, 'Id, 'valueExtension when 'Id: comparison> =
  JsonValue -> ValueParserReader<'T, 'Id, 'valueExtension>

type ExprParser<'T, 'Id when 'Id: comparison> = JsonValue -> ExprParserReader<'T, 'Id>

type TypeExprParser = JsonParser<TypeExpr>

type ValueParserLayer<'T, 'Id, 'valueExtension when 'Id: comparison> =
  ValueParser<'T, 'Id, 'valueExtension> -> ValueParser<'T, 'Id, 'valueExtension>

// encoding/serializing
type JsonEncoder<'T> = 'T -> JsonValue
type JsonEncoderWithError<'T> = 'T -> Sum<JsonValue, Errors>

type ExprEncoderReader<'T, 'Id> = Reader<JsonValue, JsonEncoder<'T> * JsonEncoder<'Id>, Errors>
type ExprEncoder<'T, 'Id when 'Id: comparison> = Expr<'T, 'Id> -> ExprEncoderReader<'T, 'Id>

type ValueEncoderReader<'T> =
  Reader<JsonValue, JsonEncoderWithError<Expr<'T, ResolvedIdentifier>> * JsonEncoder<'T>, Errors>

type ValueEncoder<'T, 'valueExtension> = Value<'T, 'valueExtension> -> ValueEncoderReader<'T>

type ValueEncoderLayer<'T, 'valueExtension> = ValueEncoder<'T, 'valueExtension> -> ValueEncoder<'T, 'valueExtension>

module Json =
  let discriminator (discriminatorValue: string) (value: JsonValue) =
    JsonValue.Record [| discriminatorKey, JsonValue.String discriminatorValue; valueKey, value |]

  let buildRootParser<'T, 'Id, 'valueExtension when 'Id: comparison>
    (layers: NonEmptyList<ValueParserLayer<'T, 'Id, 'valueExtension>>)
    : ValueParser<'T, 'Id, 'valueExtension> =
    let F
      (layers: NonEmptyList<ValueParserLayer<'T, 'Id, 'valueExtension>>)
      (self: ValueParser<'T, 'Id, 'valueExtension>)
      : ValueParser<'T, 'Id, 'valueExtension> =
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

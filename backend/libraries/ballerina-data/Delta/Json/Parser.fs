namespace Ballerina.DSL.Next.Delta.Json

open Ballerina.Errors
open Ballerina.Collections.Sum
open Ballerina.Reader.WithError
open Ballerina.Data.Delta.Model
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Types.Model

open FSharp.Data

type ValueParser = JsonValue -> Sum<Value<TypeValue>, Errors>
type DeltaParser = JsonValue -> Reader<Delta, ValueParser, Errors>
type JsonParser<'T> = JsonValue -> Sum<'T, Errors>
type ValueParser<'T> = Reader<Value<'T>, JsonParser<'T>, Errors>
type ExprParser<'T> = Reader<Expr<'T>, JsonParser<'T>, Errors>

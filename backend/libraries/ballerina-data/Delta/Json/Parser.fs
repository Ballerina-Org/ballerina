namespace Ballerina.DSL.Next.Delta.Json

open Ballerina.DSL.Next.Json
open Ballerina.Errors
open Ballerina.Collections.Sum
open Ballerina.Reader.WithError
open Ballerina.Data.Delta.Model
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Types.Model

open FSharp.Data

type ValueParser<'valueExtension> = JsonValue -> Sum<Value<TypeValue, 'valueExtension>, Errors>
type DeltaParser<'valueExtension> = JsonValue -> Reader<Delta<'valueExtension>, ValueParser<'valueExtension>, Errors>

type DeltaEncoder<'valueExtension> =
  Delta<'valueExtension> -> Reader<JsonValue, JsonEncoder<TypeValue> * JsonEncoder<'valueExtension>, Errors>

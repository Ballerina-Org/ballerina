namespace Ballerina.DSL.Next.Delta.Json

open Ballerina.DSL.Next.Json
open Ballerina.Errors
open Ballerina.Collections.Sum
open Ballerina.Reader.WithError
open Ballerina.Data.Delta.Model
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Types.Model

open FSharp.Data

type DeltaParserReader<'valueExtension> =
  Reader<Delta<'valueExtension>, JsonParser<Value<TypeValue, 'valueExtension>>, Errors>

type DeltaParser<'valueExtension> = JsonValue -> DeltaParserReader<'valueExtension>

type DeltaEncoderReader<'valueExtension> =
  Reader<JsonValue, JsonEncoderWithError<Value<TypeValue, 'valueExtension>>, Errors>

type DeltaEncoder<'valueExtension> = Delta<'valueExtension> -> DeltaEncoderReader<'valueExtension>

namespace Ballerina.DSL.Next.Delta.Json

open Ballerina.DSL.Next.Json
open Ballerina.Errors
open Ballerina.Reader.WithError
open Ballerina.Data.Delta.Model
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Types.Model

open FSharp.Data


type DeltaParserReader<'valueExtension, 'deltaExtension> =
  Reader<
    Delta<'valueExtension, 'deltaExtension>,
    JsonParser<Value<TypeValue<'valueExtension>, 'valueExtension>> * JsonParser<'deltaExtension>,
    Errors<Unit>
   >

type DeltaParser<'valueExtension, 'deltaExtension> = JsonValue -> DeltaParserReader<'valueExtension, 'deltaExtension>

type DeltaEncoderReader<'valueExtension, 'deltaExtension> =
  Reader<
    JsonValue,
    JsonEncoderWithError<Value<TypeValue<'valueExtension>, 'valueExtension>> * JsonEncoderWithError<'deltaExtension>,
    Errors<Unit>
   >

type DeltaEncoder<'valueExtension, 'deltaExtension> =
  Delta<'valueExtension, 'deltaExtension> -> DeltaEncoderReader<'valueExtension, 'deltaExtension>

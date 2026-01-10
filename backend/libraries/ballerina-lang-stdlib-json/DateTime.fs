namespace Ballerina.DSL.Next.StdLib.DateTime.Json

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module Extension =

  open Ballerina.Reader.WithError
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types
  open FSharp.Data

  let parser
    (_rootValueParser: ValueParser<TypeValue<'ext>, ResolvedIdentifier, 'ext>)
    (_v: JsonValue)
    : ValueParserReader<TypeValue<'ext>, ResolvedIdentifier, 'ext> =
    reader.Throw(Ballerina.Errors.Errors.Singleton("DateTime value parser not implemented"))

  let encoder
    (_rootValueEncoder: ValueEncoder<TypeValue<'ext>, 'ext>)
    (_v: Value<TypeValue<'ext>, 'ext>)
    : ValueEncoderReader<TypeValue<'ext>, 'ext> =
    reader.Throw(Ballerina.Errors.Errors.Singleton("DateTime value encoder not implemented"))

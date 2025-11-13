namespace Ballerina.DSL.Next.StdLib.TimeSpan.Json

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module Extension =

  open Ballerina.Reader.WithError
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types
  open FSharp.Data

  let parser
    (_rootValueParser: ValueParser<TypeValue, ResolvedIdentifier, 'ext>)
    (_v: JsonValue)
    : ValueParserReader<TypeValue, ResolvedIdentifier, 'ext> =
    reader.Throw(Ballerina.Errors.Errors.Singleton("TimeSpan value parser not implemented"))

  let encoder
    (_rootValueEncoder: ValueEncoder<TypeValue, 'ext>)
    (_v: Value<TypeValue, 'ext>)
    : ValueEncoderReader<TypeValue, 'ext> =
    reader.Throw(Ballerina.Errors.Errors.Singleton("TimeSpan value encoder not implemented"))

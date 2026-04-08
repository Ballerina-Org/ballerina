namespace Ballerina.DSL.Next.StdLib.Guid.Json

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module Extension =

  open Ballerina.Reader.WithError
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types
  open FSharp.Data
  open Ballerina.Errors

  let parser
    (_rootValueParser: ValueParser<TypeValue<'ext>, ResolvedIdentifier, 'ext>)
    (_v: JsonValue)
    : ValueParserReader<TypeValue<'ext>, ResolvedIdentifier, 'ext> =
    reader.Throw(Errors<Unit>.Singleton () (fun () -> "Guid value parser not implemented"))

  let encoder
    (_rootValueEncoder: ValueEncoder<TypeValue<'ext>, 'ext>)
    (_v: Value<TypeValue<'ext>, 'ext>)
    : ValueEncoderReader<TypeValue<'ext>, 'ext> =
    reader.Throw(Errors<Unit>.Singleton () (fun () -> "Guid value encoder not implemented"))

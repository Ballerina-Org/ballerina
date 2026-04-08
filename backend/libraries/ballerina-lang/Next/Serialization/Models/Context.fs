namespace Ballerina.DSL.Next.Serialization

open Ballerina.Reader.WithError
open PocoObjects
open Ballerina.DSL.Next.Types

type SerializationContext<'ext, 'extDTO when 'extDTO: not null and 'extDTO: not struct> =
  { ToDTO:
      'ext
        -> Option<ResolvedIdentifierDTO>
        -> Reader<ValueDTO<'extDTO>, SerializationContext<'ext, 'extDTO>, Ballerina.Errors.Errors<unit>>
    FromDTO:
      'extDTO
        -> Option<ResolvedIdentifier>
        -> Reader<Value<TypeValue<'ext>, 'ext>, SerializationContext<'ext, 'extDTO>, Ballerina.Errors.Errors<unit>> }

  static member Empty: SerializationContext<'ext, 'extDTO> =
    { FromDTO =
        fun ext _ ->
          reader {
            return!
              reader.Throw(Ballerina.Errors.Errors.Singleton () (fun () -> $"Undefined conversion from DTO for {ext}"))
          }
      ToDTO =
        fun ext _ ->
          reader {
            return!
              reader.Throw(Ballerina.Errors.Errors.Singleton () (fun () -> $"Undefined conversion from DTO for {ext}"))
          } }

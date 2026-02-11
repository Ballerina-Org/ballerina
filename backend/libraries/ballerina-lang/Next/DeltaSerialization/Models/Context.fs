namespace Ballerina.Data.Delta.Serialization

open Ballerina.DSL.Next.Serialization
open Ballerina.Reader.WithError
open Ballerina.Data.Delta
open Ballerina.Errors
open DeltaDTO

type DeltaSerializationContext<'valueExtension, 'valueExtensionDTO, 'deltaExtension, 'deltaExtensionDTO
  when 'valueExtensionDTO: not null
  and 'valueExtensionDTO: not struct
  and 'deltaExtensionDTO: not null
  and 'deltaExtensionDTO: not struct> =
  { SerializationContext: SerializationContext<'valueExtension, 'valueExtensionDTO>
    ToDTO:
      'deltaExtension
        -> Reader<
          DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>,
          DeltaSerializationContext<'valueExtension, 'valueExtensionDTO, 'deltaExtension, 'deltaExtensionDTO>,
          Errors<unit>
         >

    FromDTO:
      'deltaExtensionDTO
        -> Reader<
          Delta<'valueExtension, 'deltaExtension>,
          DeltaSerializationContext<'valueExtension, 'valueExtensionDTO, 'deltaExtension, 'deltaExtensionDTO>,
          Errors<unit>
         > }

  static member Create
    (serializationContext: SerializationContext<'valueExtension, 'valueExtensionDTO>)
    : DeltaSerializationContext<'valueExtension, 'valueExtensionDTO, 'deltaExtension, 'deltaExtensionDTO> =
    { SerializationContext = serializationContext
      ToDTO =
        fun ext ->
          reader.Throw(Errors.Singleton () (fun _ -> $"Undefined delta extension conversion to DTO for {ext}."))
      FromDTO =
        fun ext ->
          reader.Throw(Errors.Singleton () (fun _ -> $"Undefined delta extension conversion from DTO for {ext}.")) }

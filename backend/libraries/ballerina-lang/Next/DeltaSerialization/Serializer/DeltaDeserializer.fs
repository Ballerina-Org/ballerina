namespace Ballerina.Data.Delta.Serialization

module DeltaDeserializer =
  open Ballerina.Data.Delta
  open Ballerina.Reader.WithError
  open DeltaDTO
  open Ballerina.Errors
  open Ballerina.DSL.Next.Serialization.ValueDeserializer
  open Ballerina.Collections.NonEmptyList
  open System.Text.Json
  open Ballerina.DSL.Next.Serialization.SerializerConfig

  let tryGetDeltaDTOWithKind
    (expectedKind: DeltaDiscriminator)
    (deltaExtractor: DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO> -> 'dtoValue)
    (deltaDTO: DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>)
    : Reader<'dtoValue, 'context, Errors<unit>> =

    tryGetDTOWithKind expectedKind deltaDTO (fun deltaDTO -> deltaDTO.Discriminator) deltaExtractor assertValue

  let rec multipleFromDTO
    (deltaDTO: DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>)
    : Reader<
        Delta<'valueExtension, 'deltaExtension>,
        DeltaSerializationContext<'valueExtension, 'valueExtensionDTO, 'deltaExtension, 'deltaExtensionDTO>,
        Errors<unit>
       >
    =
    reader {
      let! multipleDTO = tryGetDeltaDTOWithKind DeltaDiscriminator.Multiple (fun delta -> delta.Multiple) deltaDTO
      return! multipleDTO |> Array.map deltaFromDTO |> reader.All |> reader.Map Multiple
    }

  and replaceFromDTO
    (deltaDTO: DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>)
    : Reader<
        Delta<'valueExtension, 'deltaExtension>,
        DeltaSerializationContext<'valueExtension, 'valueExtensionDTO, 'deltaExtension, 'deltaExtensionDTO>,
        Errors<unit>
       >
    =
    reader {
      let! valueDTO = tryGetDeltaDTOWithKind DeltaDiscriminator.Replace (fun delta -> delta.Replace) deltaDTO

      return!
        valueFromDTO valueDTO
        |> reader.MapContext(fun deltaContext -> deltaContext.SerializationContext)
        |> reader.Map Replace
    }

  and recordFromDTO
    (deltaDTO: DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>)
    : Reader<
        Delta<'valueExtension, 'deltaExtension>,
        DeltaSerializationContext<'valueExtension, 'valueExtensionDTO, 'deltaExtension, 'deltaExtensionDTO>,
        Errors<unit>
       >
    =
    reader {
      let! recordDTO = tryGetDeltaDTOWithKind DeltaDiscriminator.Record (fun delta -> delta.Record) deltaDTO
      let! delta = deltaFromDTO recordDTO.Delta
      return Record(recordDTO.Field, delta)
    }

  and unionFromDTO
    (deltaDTO: DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>)
    : Reader<
        Delta<'valueExtension, 'deltaExtension>,
        DeltaSerializationContext<'valueExtension, 'valueExtensionDTO, 'deltaExtension, 'deltaExtensionDTO>,
        Errors<unit>
       >
    =
    reader {
      let! unionDTO = tryGetDeltaDTOWithKind DeltaDiscriminator.Union (fun delta -> delta.Union) deltaDTO
      let! delta = deltaFromDTO unionDTO.Delta
      return Union(unionDTO.Case, delta)
    }

  and tupleFromDTO
    (deltaDTO: DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>)
    : Reader<
        Delta<'valueExtension, 'deltaExtension>,
        DeltaSerializationContext<'valueExtension, 'valueExtensionDTO, 'deltaExtension, 'deltaExtensionDTO>,
        Errors<unit>
       >
    =
    reader {
      let! tupleDTO = tryGetDeltaDTOWithKind DeltaDiscriminator.Tuple (fun delta -> delta.Tuple) deltaDTO
      let! delta = deltaFromDTO tupleDTO.Delta
      return Tuple(tupleDTO.Position, delta)
    }

  and sumFromDTO
    (deltaDTO: DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>)
    : Reader<
        Delta<'valueExtension, 'deltaExtension>,
        DeltaSerializationContext<'valueExtension, 'valueExtensionDTO, 'deltaExtension, 'deltaExtensionDTO>,
        Errors<unit>
       >
    =
    reader {
      let! sumDTO = tryGetDeltaDTOWithKind DeltaDiscriminator.Sum (fun delta -> delta.Sum) deltaDTO
      let! delta = deltaFromDTO sumDTO.Delta
      return Sum(sumDTO.CaseIndex, delta)
    }

  and extFromDTO
    (deltaDTO: DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>)
    : Reader<
        Delta<'valueExtension, 'deltaExtension>,
        DeltaSerializationContext<'valueExtension, 'valueExtensionDTO, 'deltaExtension, 'deltaExtensionDTO>,
        Errors<unit>
       >
    =
    reader {
      let! extensionDTO = tryGetDeltaDTOWithKind DeltaDiscriminator.Ext (fun delta -> delta.Ext) deltaDTO
      let! context = reader.GetContext()
      return! context.FromDTO extensionDTO
    }


  and deltaFromDTO<'valueExtension, 'valueExtensionDTO, 'deltaExtension, 'deltaExtensionDTO
    when 'valueExtensionDTO: not null
    and 'valueExtensionDTO: not struct
    and 'deltaExtensionDTO: not null
    and 'deltaExtensionDTO: not struct>
    (deltaDTO: DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>)
    : Reader<
        Delta<'valueExtension, 'deltaExtension>,
        DeltaSerializationContext<'valueExtension, 'valueExtensionDTO, 'deltaExtension, 'deltaExtensionDTO>,
        Errors<unit>
       >
    =
    reader.Any(
      (multipleFromDTO deltaDTO,
       [ replaceFromDTO deltaDTO
         recordFromDTO deltaDTO
         unionFromDTO deltaDTO
         tupleFromDTO deltaDTO
         sumFromDTO deltaDTO
         extFromDTO deltaDTO
         reader.Throw(Errors.Singleton () (fun _ -> $"The value {deltaDTO} cannot be converted from DTO.")) ])
      |> NonEmptyList
    )

  type Delta<'valueExtension, 'deltaExtension> with
    static member JsonDeserializeV2
      (json: string)
      : Reader<
          Delta<'valueExtension, 'deltaExtension>,
          DeltaSerializationContext<'valueExtension, 'valueExtensionDTO, 'deltaExtension, 'deltaExtensionDTO>,
          Errors<unit>
         >
      =
      reader {
        let deltaDTO: DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO> =
          JsonSerializer.Deserialize<DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>>(
            json,
            jsonSerializationConfiguration
          )

        return! deltaFromDTO deltaDTO
      }

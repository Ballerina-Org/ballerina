namespace Ballerina.Data.Delta.Serialization

module DeltaDeserializer =
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.Reader.WithError
  open DeltaDTO
  open Ballerina.Errors
  open Ballerina.DSL.Next.Serialization.ValueDeserializer
  open Ballerina.Collections.NonEmptyList
  open System.Text.Json
  open Ballerina.DSL.Next.Serialization.SerializerConfig
  open Ballerina.Data.Delta

  module private ResolvedIdentifierParsing =
    open Ballerina.DSL.Next.Serialization.PocoObjects

    let tryParse (s: string) = ResolvedIdentifier.TryParse s

  let rec multipleFromDTO
    (deltaDTO: DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>)
    : Reader<
        Delta<'valueExtension, 'deltaExtension>,
        DeltaSerializationContext<
          'valueExtension,
          'valueExtensionDTO,
          'deltaExtension,
          'deltaExtensionDTO
         >,
        Errors<unit>
       >
    =
    reader {
      let! multipleDTO = assertValue deltaDTO.Multiple "multiple deltas"

      return!
        multipleDTO
        |> Array.map deltaFromDTO
        |> reader.All
        |> reader.Map Multiple
    }

  and replaceFromDTO
    (deltaDTO: DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>)
    : Reader<
        Delta<'valueExtension, 'deltaExtension>,
        DeltaSerializationContext<
          'valueExtension,
          'valueExtensionDTO,
          'deltaExtension,
          'deltaExtensionDTO
         >,
        Errors<unit>
       >
    =
    reader {
      let! valueDTO = assertValue deltaDTO.Replace "replace"

      return!
        valueFromDTO valueDTO
        |> reader.MapContext(fun deltaContext ->
          deltaContext.SerializationContext)
        |> reader.Map Replace
    }

  and recordFromDTO
    (deltaDTO: DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>)
    : Reader<
        Delta<'valueExtension, 'deltaExtension>,
        DeltaSerializationContext<
          'valueExtension,
          'valueExtensionDTO,
          'deltaExtension,
          'deltaExtensionDTO
         >,
        Errors<unit>
       >
    =
    reader {
      let! recordDTO = assertValue deltaDTO.Record "record delta"

      let! field, deltaDTO =
        assertSingleElementDictionary recordDTO "record delta"

      let! fieldIdentifier =
        ResolvedIdentifierParsing.tryParse field
        |> reader.OfSum

      let! delta = deltaFromDTO deltaDTO
      return Record(fieldIdentifier, delta)
    }

  and unionFromDTO
    (deltaDTO: DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>)
    : Reader<
        Delta<'valueExtension, 'deltaExtension>,
        DeltaSerializationContext<
          'valueExtension,
          'valueExtensionDTO,
          'deltaExtension,
          'deltaExtensionDTO
         >,
        Errors<unit>
       >
    =
    reader {
      let! unionDTO = assertValue deltaDTO.Union "union delta"
      let! case, deltaDTO = assertSingleElementDictionary unionDTO "union delta"

      let! caseIdentifier =
        ResolvedIdentifierParsing.tryParse case
        |> reader.OfSum

      let! delta = deltaFromDTO deltaDTO
      return Union(caseIdentifier, delta)
    }

  and tupleFromDTO
    (deltaDTO: DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>)
    : Reader<
        Delta<'valueExtension, 'deltaExtension>,
        DeltaSerializationContext<
          'valueExtension,
          'valueExtensionDTO,
          'deltaExtension,
          'deltaExtensionDTO
         >,
        Errors<unit>
       >
    =
    reader {
      let! tupleDTO = assertValue deltaDTO.Tuple "tuple delta"

      let! position, deltaDTO =
        assertSingleElementDictionary tupleDTO "tuple delta"

      let! delta = deltaFromDTO deltaDTO
      return Tuple(position, delta)
    }

  and sumFromDTO
    (deltaDTO: DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>)
    : Reader<
        Delta<'valueExtension, 'deltaExtension>,
        DeltaSerializationContext<
          'valueExtension,
          'valueExtensionDTO,
          'deltaExtension,
          'deltaExtensionDTO
         >,
        Errors<unit>
       >
    =
    reader {
      let! sumDTO = assertValue deltaDTO.Sum "sum delta"

      let! caseIndex, deltaDTO =
        assertSingleElementDictionary sumDTO "sum delta"

      let! delta = deltaFromDTO deltaDTO
      return Sum(caseIndex, delta)
    }

  and extFromDTO
    (deltaDTO: DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>)
    : Reader<
        Delta<'valueExtension, 'deltaExtension>,
        DeltaSerializationContext<
          'valueExtension,
          'valueExtensionDTO,
          'deltaExtension,
          'deltaExtensionDTO
         >,
        Errors<unit>
       >
    =
    reader {
      let! extensionDTO = assertValue deltaDTO.Ext "extension delta"
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
        DeltaSerializationContext<
          'valueExtension,
          'valueExtensionDTO,
          'deltaExtension,
          'deltaExtensionDTO
         >,
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
         reader.Throw(
           Errors.Singleton () (fun _ ->
             $"The value {deltaDTO} cannot be converted from DTO.")
         ) ])
      |> NonEmptyList
    )

  type Delta<'valueExtension, 'deltaExtension> with
    static member JsonDeserializeV2
      (json: string)
      : Reader<
          Delta<'valueExtension, 'deltaExtension>,
          DeltaSerializationContext<
            'valueExtension,
            'valueExtensionDTO,
            'deltaExtension,
            'deltaExtensionDTO
           >,
          Errors<unit>
         >
      =
      reader {
        let deltaDTO: DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO> =
          JsonSerializer.Deserialize<
            DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>
           >(
            json,
            jsonSerializationConfiguration
          )

        return! deltaFromDTO deltaDTO
      }

namespace Ballerina.Data.Delta.Serialization

module DeltaDTO =
  open Ballerina.DSL.Next.Serialization.PocoObjects
  open System.Collections.Generic

  type RecordDeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO
    when 'valueExtensionDTO: not null
    and 'valueExtensionDTO: not struct
    and 'deltaExtensionDTO: not null
    and 'deltaExtensionDTO: not struct> =
    { Field: string
      Delta: DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO> }

  and UnionDeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO
    when 'valueExtensionDTO: not null
    and 'valueExtensionDTO: not struct
    and 'deltaExtensionDTO: not null
    and 'deltaExtensionDTO: not struct> =
    { Case: string
      Delta: DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO> }

  and TupleDeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO
    when 'valueExtensionDTO: not null
    and 'valueExtensionDTO: not struct
    and 'deltaExtensionDTO: not null
    and 'deltaExtensionDTO: not struct> =
    { Position: int
      Delta: DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO> }

  and SumDeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO
    when 'valueExtensionDTO: not null
    and 'valueExtensionDTO: not struct
    and 'deltaExtensionDTO: not null
    and 'deltaExtensionDTO: not struct> =
    { CaseIndex: int
      Delta: DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO> }

  and DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO
    when 'valueExtensionDTO: not null
    and 'valueExtensionDTO: not struct
    and 'deltaExtensionDTO: not null
    and 'deltaExtensionDTO: not struct>() =

    member val Multiple: DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>[] = null with get, set
    member val Replace: ValueDTO<'valueExtensionDTO> | null = null with get, set
    member val Record: Dictionary<string, DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>> = null with get, set
    member val Union: Dictionary<string, DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>> = null with get, set
    member val Tuple: Dictionary<int, DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>> = null with get, set
    member val Sum: Dictionary<int, DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>> = null with get, set
    member val Ext: 'deltaExtensionDTO | null = null with get, set

    new(deltas: DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>[]) as this =
      DeltaDTO()
      then this.Multiple <- deltas

    new(replace: ValueDTO<'valueExtensionDTO>) as this =
      DeltaDTO()
      then this.Replace <- replace

    new(recordOrUnion: Dictionary<string, DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>>, isRecord: bool) as this =
      DeltaDTO()

      then
        if isRecord then
          this.Record <- recordOrUnion
        else
          this.Union <- recordOrUnion

    new(tupleOrSum: Dictionary<int, DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>>, isTuple: bool) as this =
      DeltaDTO()

      then
        if isTuple then
          this.Tuple <- tupleOrSum
        else
          this.Sum <- tupleOrSum

    new(extension: 'deltaExtensionDTO) as this =
      DeltaDTO()
      then this.Ext <- extension

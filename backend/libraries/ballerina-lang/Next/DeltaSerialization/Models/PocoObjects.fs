namespace Ballerina.Data.Delta.Serialization

module DeltaDTO =
  open Ballerina.DSL.Next.Serialization.PocoObjects

  type DeltaDiscriminator =
    | Multiple = 1
    | Replace = 2
    | Record = 3
    | Union = 4
    | Tuple = 5
    | Sum = 6
    | Ext = 7

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
    when 'valueExtensionDTO: not null and 'valueExtensionDTO: not struct> =
    { Discriminator: DeltaDiscriminator
      Multiple: DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>[]
      Replace: ValueDTO<'valueExtensionDTO> | null
      Record: RecordDeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO> | null
      Union: UnionDeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO> | null
      Tuple: TupleDeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO> | null
      Sum: SumDeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO> | null
      Ext: 'deltaExtensionDTO | null }

    static member Empty =
      { Discriminator = DeltaDiscriminator.Multiple
        Multiple = null
        Replace = null
        Record = null
        Union = null
        Tuple = null
        Sum = null
        Ext = null }

    static member CreateMultiple deltas : DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO> =
      { DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>.Empty with
          Discriminator = DeltaDiscriminator.Multiple
          Multiple = deltas }

    static member CreateReplace replace : DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO> =
      { DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>.Empty with
          Discriminator = DeltaDiscriminator.Replace
          Replace = replace }

    static member CreateRecord record : DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO> =
      { DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>.Empty with
          Discriminator = DeltaDiscriminator.Record
          Record = record }

    static member CreateUnion union : DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO> =
      { DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>.Empty with
          Discriminator = DeltaDiscriminator.Union
          Union = union }

    static member CreateTuple tuple : DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO> =
      { DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>.Empty with
          Discriminator = DeltaDiscriminator.Tuple
          Tuple = tuple }

    static member CreateSum sum : DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO> =
      { DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>.Empty with
          Discriminator = DeltaDiscriminator.Sum
          Sum = sum }

    static member CreateExtension extension : DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO> =
      { DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>.Empty with
          Discriminator = DeltaDiscriminator.Ext
          Ext = extension }

namespace Ballerina.Data.Delta.Serialization

module DeltaSerializer =
  open Ballerina.Data.Delta
  open Ballerina.Reader.WithError
  open Ballerina.DSL.Next.Serialization
  open DeltaDTO
  open Ballerina.Errors
  open Ballerina.DSL.Next.Serialization.ValueSerializer
  open Ballerina.DSL.Next.Serialization.PocoObjects
  open System.Text.Json
  open Ballerina.DSL.Next.Serialization.SerializerConfig

  let rec multipleToDTO (deltas: List<Delta<'valueExtension, 'deltaExtension>>) =
    deltas
    |> List.map deltaToDTO
    |> reader.All
    |> reader.Map(List.toArray >> DeltaDTO.CreateMultiple)

  and replaceToDTO
    (value:
      Ballerina.DSL.Next.Types.Model.Value<Ballerina.DSL.Next.Types.Model.TypeValue<'valueExtension>, 'valueExtension>)
    : Reader<
        DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>,
        DeltaSerializationContext<'valueExtension, 'valueExtensionDTO, 'deltaExtension, 'deltaExtensionDTO>,
        Errors<unit>
       >
    =
    valueToDTO value
    |> reader.MapContext(fun deltaContext -> deltaContext.SerializationContext)
    |> reader.Map DeltaDTO.CreateReplace

  and recordToDTO (field: string) (delta: Delta<'valueExtension, 'deltaExtension>) =
    reader {
      let! deltaDTO = deltaToDTO delta
      return { Field = field; Delta = deltaDTO } |> DeltaDTO.CreateRecord
    }

  and unionToDTO (case: string) (delta: Delta<'valueExtension, 'deltaExtension>) =
    reader {
      let! deltaDTO = deltaToDTO delta
      return { Case = case; Delta = deltaDTO } |> DeltaDTO.CreateUnion
    }

  and tupleToDTO (position: int) (delta: Delta<'valueExtension, 'deltaExtension>) =
    reader {
      let! deltaDTO = deltaToDTO delta

      return
        { Position = position
          Delta = deltaDTO }
        |> DeltaDTO.CreateTuple
    }

  and sumDTO (caseIndex: int) (delta: Delta<'valueExtension, 'deltaExtension>) =
    reader {
      let! deltaToDTO = deltaToDTO delta

      return
        { CaseIndex = caseIndex
          Delta = deltaToDTO }
        |> DeltaDTO.CreateSum
    }

  and deltaToDTO
    (delta: Delta<'valueExtension, 'deltaExtension>)
    : Reader<
        DeltaDTO<'valueExtensionDTO, 'deltaExtensionDTO>,
        DeltaSerializationContext<'valueExtension, 'valueExtensionDTO, 'deltaExtension, 'deltaExtensionDTO>,
        Errors<unit>
       >
    =
    reader {
      match delta with
      | Multiple deltas -> return! multipleToDTO deltas
      | Replace value -> return! replaceToDTO value
      | Record(field, delta) -> return! recordToDTO field delta
      | Union(case, delta) -> return! unionToDTO case delta
      | Tuple(position, delta) -> return! tupleToDTO position delta
      | Sum(caseIndex, delta) -> return! tupleToDTO caseIndex delta
      | Ext deltaExtension ->
        let! context = reader.GetContext()
        return! context.ToDTO deltaExtension
    }

  type Delta<'valueExtension, 'deltaExtension> with
    static member JsonSerializeV2(delta: Delta<'valueExtension, 'deltaExtension>) =
      reader {
        let! deltaDTO = deltaToDTO delta
        return JsonSerializer.Serialize(deltaDTO, jsonSerializationConfiguration)
      }

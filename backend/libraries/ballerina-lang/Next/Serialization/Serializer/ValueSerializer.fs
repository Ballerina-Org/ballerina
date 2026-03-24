namespace Ballerina.DSL.Next.Serialization

open Ballerina.DSL.Next.Serialization.PocoObjects

module ValueSerializer =
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.Errors
  open System.Text.Json
  open Ballerina.Reader.WithError
  open SerializerConfig

  let optionToNullable (option: Option<'T>) =
    match option with
    | None -> null
    | Some v -> v

  let resolvedIdentifierToDTO (identifier: ResolvedIdentifier) : ResolvedIdentifierDTO =
    { Assembly = identifier.Assembly
      Module = identifier.Module
      Type = optionToNullable identifier.Type
      Name = identifier.Name }

  let rec recordToDTO (record: Map<ResolvedIdentifier, Value<'T, 'valueExt>>) =
    record
    |> Map.toList
    |> List.sortWith (fun (id1, _) (id2, _) -> ResolvedIdentifier.Compare id1 id2)
    |> List.map (fun (identifier, value) ->
      reader {
        let identifierDTO = resolvedIdentifierToDTO identifier
        let! valueDTO = valueToDTO value
        return identifierDTO, valueDTO
      })
    |> reader.All
    |> Reader.map (List.map (fun (key, value) -> { Key = key; Value = value }) >> List.toArray)
    |> Reader.map (fun r -> new ValueDTO<'valueExtDTO>(r))

  and unionCaseToDTO ((identifier, value): ResolvedIdentifier * Value<'T, 'valueExt>) =
    reader {
      let identifierDTO = resolvedIdentifierToDTO identifier
      let! valueDTO = valueToDTO value

      let unionDTO: UnionCaseDTO<'valueExtDTO> =
        { Case = identifierDTO
          Value = valueDTO }

      return new ValueDTO<'valueExtDTO>(unionDTO)
    }

  and tupleToDTO (items: List<Value<'T, 'valueExt>>) =
    items
    |> List.map valueToDTO
    |> reader.All
    |> reader.Map(List.toArray >> fun t -> new ValueDTO<'valueExtDTO>(t))

  and sumToDTO ((selector, value): SumConsSelector * Value<'T, 'valueExt>) =
    reader {
      let! valueDTO = valueToDTO value

      let sumDTO: SumDTO<'valueExtDTO> =
        { Selector = selector
          Value = valueDTO }

      return new ValueDTO<'valueExtDTO>(sumDTO)
    }

  and primitiveToDTO =
    function
    | Int32 int32 -> new PrimitiveValueDTO(int32)
    | Int64 int64 -> new PrimitiveValueDTO(int64)
    | Float32 float32 -> new PrimitiveValueDTO(float32)
    | Float64 float64 -> new PrimitiveValueDTO(float64)
    | Decimal decimal -> new PrimitiveValueDTO(decimal)
    | Bool bool -> new PrimitiveValueDTO(bool)
    | Guid guid -> new PrimitiveValueDTO(guid)
    | String string -> new PrimitiveValueDTO(string)
    | Date date -> new PrimitiveValueDTO(date)
    | DateTime dateTime -> new PrimitiveValueDTO(dateTime)
    | TimeSpan span -> new PrimitiveValueDTO(span)
    | Unit -> new PrimitiveValueDTO()

  and valueToDTO
    (value: Value<'T, 'valueExt>)
    : Reader<ValueDTO<'valueExtDTO>, SerializationContext<'valueExt, 'valueExtDTO>, Errors<unit>> =
    reader {
      match value with
      | Record record -> return! recordToDTO record
      | UnionCase(identifier, value) -> return! unionCaseToDTO (identifier, value)
      | RecordDes identifier -> return new ValueDTO<'valueExtDTO>(resolvedIdentifierToDTO identifier)
      | UnionCons(identifier: ResolvedIdentifier) ->
        return new ValueDTO<'valueExtDTO>(resolvedIdentifierToDTO identifier)
      | Tuple items -> return! tupleToDTO items
      | Sum(selector, value) -> return! sumToDTO (selector, value)
      | Primitive primitive -> return new ValueDTO<'valueExtDTO>(primitiveToDTO primitive)
      | Var var -> return new ValueDTO<'valueExtDTO>(var)
      | Ext(ext, applicableId) ->
        let! context = reader.GetContext()
        let applicableIdDTO = applicableId |> Option.map resolvedIdentifierToDTO
        return! context.ToDTO ext applicableIdDTO
      | _ -> return! reader.Throw(Errors.Singleton () (fun _ -> $"The value {value} cannot be converted to DTO."))
    }

  type Value<'T, 'valueExt> with
    static member JsonSerializeV2(value: Value<'T, 'valueExt>) =
      reader {
        let! valueDTO = valueToDTO value
        return JsonSerializer.Serialize(valueDTO, jsonSerializationConfiguration)
      }

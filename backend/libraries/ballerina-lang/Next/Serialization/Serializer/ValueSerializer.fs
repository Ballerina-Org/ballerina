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
    |> List.map (fun (identifier, value) ->
      reader {
        let identifierDTO = resolvedIdentifierToDTO identifier
        let! valueDTO = valueToDTO value
        return identifierDTO, valueDTO
      })
    |> reader.All
    |> Reader.map (List.map (fun (key, value) -> { Key = key; Value = value }) >> List.toArray)
    |> Reader.map ValueDTO<'valueExtDTO>.CreateRecord

  and unionCaseToDTO ((identifier, value): ResolvedIdentifier * Value<'T, 'valueExt>) =
    reader {
      let identifierDTO = resolvedIdentifierToDTO identifier
      let! valueDTO = valueToDTO value

      return
        { Case = identifierDTO
          Value = valueDTO }
        |> ValueDTO<'valueExtDTO>.CreateUnionCase
    }

  and tupleToDTO (items: List<Value<'T, 'valueExt>>) =
    items
    |> List.map valueToDTO
    |> reader.All
    |> reader.Map(List.toArray >> ValueDTO<'valueExtDTO>.CreateTuple)

  and sumToDTO ((selector, value): SumConsSelector * Value<'T, 'valueExt>) =
    reader {
      let! valueDTO = valueToDTO value

      return
        { Selector = selector
          Value = valueDTO }
        |> ValueDTO<'valueExtDTO>.CreateSum
    }

  and primitiveToDTO =
    function
    | Int32 int32 -> PrimitiveValueDTO.CreateInt32 int32
    | Int64 int64 -> PrimitiveValueDTO.CreateInt64 int64
    | Float32 float32 -> PrimitiveValueDTO.CreateFloat32 float32
    | Float64 float64 -> PrimitiveValueDTO.CreateFloat64 float64
    | Decimal decimal -> PrimitiveValueDTO.CreateDecimal decimal
    | Bool bool -> PrimitiveValueDTO.CreateBool bool
    | Guid guid -> PrimitiveValueDTO.CreateGuid guid
    | String string -> PrimitiveValueDTO.CreateString string
    | Date date -> PrimitiveValueDTO.CreateDate date
    | DateTime dateTime -> PrimitiveValueDTO.CreateDateTime dateTime
    | TimeSpan span -> PrimitiveValueDTO.CreateTimeSpan span
    | Unit -> PrimitiveValueDTO.Empty

  and valueToDTO
    (value: Value<'T, 'valueExt>)
    : Reader<ValueDTO<'valueExtDTO>, SerializationContext<'valueExt, 'valueExtDTO>, Errors<unit>> =
    reader {
      match value with
      | Record record -> return! recordToDTO record
      | UnionCase(identifier, value) -> return! unionCaseToDTO (identifier, value)
      | RecordDes identifier -> return resolvedIdentifierToDTO identifier |> ValueDTO<'valueExtDTO>.CreateRecordDes
      | UnionCons(identifier: ResolvedIdentifier) ->
        return resolvedIdentifierToDTO identifier |> ValueDTO<'valueExtDTO>.CreateUnionCons
      | Tuple items -> return! tupleToDTO items
      | Sum(selector, value) -> return! sumToDTO (selector, value)
      | Primitive primitive -> return primitiveToDTO primitive |> ValueDTO<'valueExtDTO>.CreatePrimitive
      | Var var -> return ValueDTO<'valueExtDTO>.CreateVar var
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

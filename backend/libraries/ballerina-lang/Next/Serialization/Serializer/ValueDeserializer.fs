namespace Ballerina.DSL.Next.Serialization

module ValueDeserializer =
  open PocoObjects
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.Errors
  open System.Text.Json
  open Ballerina.Reader.WithError
  open Ballerina.Collections.NonEmptyList
  open System
  open SerializerConfig

  let nullableToOption (value: 'T | null) : Option<'T> =
    if isNull value then None else Some value

  let assertValue (value: 'T | null) : Reader<'T, 'context, Errors<unit>> =
    if isNull value then
      reader.Throw(Errors.Singleton () (fun _ -> "Expected non nullable value"))
    else
      reader.Return value

  let assertNonNullable (nullable: Nullable<'T>) : Reader<Nullable<'T>, 'context, Errors<unit>> =
    if nullable.HasValue |> not then
      reader.Throw(Errors.Singleton () (fun _ -> "Expected non nullable value"))
    else
      reader.Return nullable

  let resolvedIdentifierFromDTO (identifier: ResolvedIdentifierDTO) : ResolvedIdentifier =
    { Assembly = identifier.Assembly
      Module = identifier.Module
      Type = nullableToOption identifier.Type
      Name = identifier.Name }

  let tryGetDTOWithKind
    (expectedKind: 'kind)
    (dto: 'valueDTO)
    (kindExtractor: 'valueDTO -> 'kind)
    (valueExtractor: 'valueDTO -> 'dtoValue)
    (assertion: 'dtoValue -> Reader<'dtoValue, 'context, Errors<unit>>)
    : Reader<'dtoValue, 'context, Errors<unit>> =
    reader {
      let kind = kindExtractor dto

      if expectedKind <> kind then
        return!
          reader.Throw(Errors.Singleton () (fun _ -> $"Error when converting {kind} from DTO. Expected {expectedKind}"))
      else
        return! valueExtractor >> assertion <| dto
    }

  let tryGetValueDTOWithKind
    (expectedKind: ValueDiscriminator)
    (valueExtractor: ValueDTO<'valueExtDTO> -> 'dtoValue)
    (valueDTO: ValueDTO<'valueExtDTO>)
    =
    tryGetDTOWithKind expectedKind valueDTO (fun valueDTO -> valueDTO.Kind) valueExtractor assertValue

  let tryGetPrimitiveDTOWithKind
    (expectedKind: PrimitiveValueDiscriminator)
    (valueExtractor: PrimitiveValueDTO -> Nullable<'a>)
    (primitive: PrimitiveValueDTO)
    =
    tryGetDTOWithKind
      expectedKind
      primitive
      (fun (primitive: PrimitiveValueDTO) -> primitive.Kind)
      valueExtractor
      assertNonNullable
    |> reader.Map(fun nullable -> nullable.Value)

  let tryGetString (valueDTO: PrimitiveValueDTO) =
    tryGetDTOWithKind
      PrimitiveValueDiscriminator.String
      valueDTO
      (fun dto -> dto.Kind)
      (fun dto -> dto.String)
      assertValue

  let tryGetUnit (valueDTO: PrimitiveValueDTO) =
    if valueDTO.Kind <> PrimitiveValueDiscriminator.Unit then
      reader.Throw(
        Errors.Singleton () (fun _ ->
          $"Error when converting {valueDTO.Kind} from DTO. Expected PrimitiveValueKind.Unit.")
      )
    else
      reader.Return PrimitiveValue.Unit


  let rec recordFromDTO (valueDTO: ValueDTO<'valueExtDTO>) =
    reader {
      let! recordDTO = tryGetValueDTOWithKind ValueDiscriminator.Record (fun dto -> dto.Record) valueDTO

      return!
        recordDTO
        |> Array.map (fun recordKV ->
          reader {
            let identifier = resolvedIdentifierFromDTO recordKV.Key
            let! value = valueFromDTO recordKV.Value
            return identifier, value
          })
        |> reader.All
        |> Reader.map (Map.ofList >> Record)
    }

  and unionCaseFromDTO (valueDTO: ValueDTO<'valueExtDTO>) =
    reader {
      let! unionCaseDTO = tryGetValueDTOWithKind ValueDiscriminator.UnionCase (fun dto -> dto.UnionCase) valueDTO
      let identifier = resolvedIdentifierFromDTO unionCaseDTO.Case
      let! value = valueFromDTO unionCaseDTO.Value
      return UnionCase(identifier, value)
    }

  and tupleFromDTO (valueDTO: ValueDTO<'valueExtDTO>) =
    reader {
      let! itemsDTO = tryGetValueDTOWithKind ValueDiscriminator.Tuple (fun dto -> dto.Tuple) valueDTO
      return! itemsDTO |> Array.map valueFromDTO |> reader.All |> reader.Map Value.Tuple
    }

  and sumFromDTO (valueDTO: ValueDTO<'valueExtDTO>) =
    reader {
      let! sumDTO = tryGetValueDTOWithKind ValueDiscriminator.Sum (fun dto -> dto.Sum) valueDTO
      let! value = valueFromDTO sumDTO.Value
      return Sum(sumDTO.Selector, value)
    }

  and primitiveValueFromDTO
    (primitive: PrimitiveValueDTO)
    : Reader<Value<TypeValue<'valueExt>, 'valueExt>, SerializationContext<'valueExt, 'valueExtDTO>, Errors<unit>> =
    reader.Any(
      NonEmptyList(
        tryGetPrimitiveDTOWithKind PrimitiveValueDiscriminator.Int32 (fun primitive -> primitive.Int32) primitive
        |> reader.Map(PrimitiveValue.Int32 >> Primitive),
        [ tryGetPrimitiveDTOWithKind PrimitiveValueDiscriminator.Int64 (fun primitive -> primitive.Int64) primitive
          |> reader.Map(PrimitiveValue.Int64 >> Primitive)
          tryGetPrimitiveDTOWithKind PrimitiveValueDiscriminator.Float32 (fun primitive -> primitive.Float32) primitive
          |> reader.Map(PrimitiveValue.Float32 >> Primitive)
          tryGetPrimitiveDTOWithKind PrimitiveValueDiscriminator.Float64 (fun primitive -> primitive.Float64) primitive
          |> reader.Map(PrimitiveValue.Float64 >> Primitive)
          tryGetPrimitiveDTOWithKind PrimitiveValueDiscriminator.Decimal (fun primitive -> primitive.Decimal) primitive
          |> reader.Map(PrimitiveValue.Decimal >> Primitive)
          tryGetPrimitiveDTOWithKind PrimitiveValueDiscriminator.Bool (fun primitive -> primitive.Bool) primitive
          |> reader.Map(PrimitiveValue.Bool >> Primitive)
          tryGetPrimitiveDTOWithKind PrimitiveValueDiscriminator.Guid (fun primitive -> primitive.Guid) primitive
          |> reader.Map(PrimitiveValue.Guid >> Primitive)
          tryGetString primitive |> reader.Map(PrimitiveValue.String >> Primitive)
          tryGetPrimitiveDTOWithKind PrimitiveValueDiscriminator.Date (fun primitive -> primitive.Date) primitive
          |> reader.Map(PrimitiveValue.Date >> Primitive)
          tryGetPrimitiveDTOWithKind
            PrimitiveValueDiscriminator.DateTime
            (fun primitive -> primitive.DateTime)
            primitive
          |> reader.Map(PrimitiveValue.DateTime >> Primitive)
          tryGetPrimitiveDTOWithKind
            PrimitiveValueDiscriminator.TimeSpan
            (fun primitive -> primitive.TimeSpan)
            primitive
          |> reader.Map(PrimitiveValue.TimeSpan >> Primitive)
          tryGetUnit primitive |> reader.Map Primitive
          reader.Throw(Errors.Singleton () (fun _ -> $"The primitive {primitive} cannot be converted from DTO.")) ]
      )
    )

  and primitiveFromDTO (valueDTO: ValueDTO<'valueExtDTO>) =
    reader {
      let! primitiveValueDTO = tryGetValueDTOWithKind ValueDiscriminator.Primitive (fun dto -> dto.Primitive) valueDTO
      return! primitiveValueFromDTO primitiveValueDTO
    }

  and varFromDTO
    (valueDTO: ValueDTO<'valueExtDTO>)
    : Reader<Value<TypeValue<'valueExt>, 'valueExt>, SerializationContext<'valueExt, 'valueExtDTO>, Errors<unit>> =
    tryGetValueDTOWithKind ValueDiscriminator.Var (fun dto -> dto.Var) valueDTO
    |> reader.Map Var

  and extFromDTO
    (valueDTO: ValueDTO<'valueExtDTO>)
    : Reader<Value<TypeValue<'valueExt>, 'valueExt>, SerializationContext<'valueExt, 'valueExtDTO>, Errors<unit>> =
    reader {
      let! extDTO = tryGetValueDTOWithKind ValueDiscriminator.Ext (fun dto -> dto.Ext) valueDTO

      let applicableId =
        if isNull extDTO.ApplicableId then
          None
        else
          resolvedIdentifierFromDTO extDTO.ApplicableId |> Some

      let! context = reader.GetContext()
      return! context.FromDTO extDTO.Value applicableId
    }

  and valueFromDTO
    (valueDTO: ValueDTO<'valueExtDTO>)
    : Reader<Value<TypeValue<'valueExt>, 'valueExt>, SerializationContext<'valueExt, 'valueExtDTO>, Errors<unit>> =
    reader.Any(
      (recordFromDTO valueDTO,
       [ unionCaseFromDTO valueDTO
         tupleFromDTO valueDTO
         sumFromDTO valueDTO
         primitiveFromDTO valueDTO
         varFromDTO valueDTO
         extFromDTO valueDTO
         reader.Throw(Errors.Singleton () (fun _ -> $"The value {valueDTO} cannot be converted from DTO.")) ])
      |> NonEmptyList
    )

  type Value<'T, 'valueExt> with
    static member JsonDeserializeV2
      (json: string)
      : Reader<Value<TypeValue<'valueExt>, 'valueExt>, SerializationContext<'valueExt, 'valueExtDTO>, Errors<unit>> =
      reader {
        let value: ValueDTO<'valueExtDTO> =
          JsonSerializer.Deserialize<ValueDTO<'valueExtDTO>>(json, jsonSerializationConfiguration)

        return! valueFromDTO value
      }

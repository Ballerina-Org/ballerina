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

  let assertValue
    (value: 'T | null)
    (category: string)
    : Reader<'T, 'context, Errors<unit>> =
    if isNull value then
      reader.Throw(
        Errors.Singleton () (fun _ ->
          $"Expected non nullable value when parsing {category}.")
      )
    else
      reader.Return value

  let assertNonNullable
    (nullable: Nullable<'T>)
    (category: string)
    : Reader<'T, 'context, Errors<unit>> =
    if nullable.HasValue |> not then
      reader.Throw(
        Errors.Singleton () (fun _ ->
          $"Expected non nullable value when parsing {category}.")
      )
    else
      reader.Return nullable.Value

  let assertSingleElementDictionary
    (dictionary: System.Collections.Generic.Dictionary<'k, 'v>)
    (category: string)
    =
    reader {
      if dictionary.Count <> 1 then
        return!
          reader.Throw(
            Errors.Singleton () (fun _ ->
              $"Invalid structure in {category} DTO. Expected 1 element but found {dictionary.Count}.")
          )

      let! dto =
        dictionary
        |> Seq.tryHead
        |> reader.OfOption(
          Errors.Singleton () (fun _ ->
            $"The {category} DTO was an empty dictionary. Expected 1 element.")
        )

      return dto.Key, dto.Value
    }


  let rec recordFromDTO (valueDTO: ValueDTO<'valueExtDTO>) =
    reader {
      let! recordDTO = assertValue valueDTO.Record "record"

      return!
        recordDTO
        |> Seq.map (fun recordKV ->
          reader {
            let! identifier =
              ResolvedIdentifier.TryParse recordKV.Key |> reader.OfSum

            let! value = valueFromDTO recordKV.Value
            return identifier, value
          })
        |> reader.All
        |> Reader.map (Map.ofList >> Record)
    }

  and unionCaseFromDTO (valueDTO: ValueDTO<'valueExtDTO>) =
    reader {
      let! unionCaseDTO = assertValue valueDTO.UnionCase "union case"

      let! identifierDTO, valueDTO =
        assertSingleElementDictionary unionCaseDTO "union case"

      let! identifier =
        ResolvedIdentifier.TryParse identifierDTO |> reader.OfSum

      let! value = valueFromDTO valueDTO
      return UnionCase(identifier, value)
    }

  and tupleFromDTO (valueDTO: ValueDTO<'valueExtDTO>) =
    reader {
      let! itemsDTO = assertValue valueDTO.Tuple "tuple"

      return!
        itemsDTO
        |> Array.map valueFromDTO
        |> reader.All
        |> reader.Map Value.Tuple
    }

  and sumFromDTO (valueDTO: ValueDTO<'valueExtDTO>) =
    reader {
      let! sumDTO = assertValue valueDTO.Sum "sum"
      let! caseDTO, valueDTO = assertSingleElementDictionary sumDTO "sum"
      let! sumSelector = SumConsSelector.TryParse caseDTO |> reader.OfSum
      let! value = valueFromDTO valueDTO
      return Sum(sumSelector, value)
    }

  and primitiveValueFromDTO
    (primitive: PrimitiveValueDTO)
    : Reader<
        Value<TypeValue<'valueExt>, 'valueExt>,
        SerializationContext<'valueExt, 'valueExtDTO>,
        Errors<unit>
       >
    =
    reader.Any(
      NonEmptyList(
        assertNonNullable primitive.Int32 "int32"
        |> reader.Map(PrimitiveValue.Int32 >> Primitive),
        [ assertNonNullable primitive.Int64 "int64"
          |> reader.Map(PrimitiveValue.Int64 >> Primitive)
          assertNonNullable primitive.Float32 "float32"
          |> reader.Map(PrimitiveValue.Float32 >> Primitive)
          assertNonNullable primitive.Float64 "float64"
          |> reader.Map(PrimitiveValue.Float64 >> Primitive)
          assertNonNullable primitive.Decimal "decimal"
          |> reader.Map(PrimitiveValue.Decimal >> Primitive)
          assertNonNullable primitive.Bool "bool"
          |> reader.Map(PrimitiveValue.Bool >> Primitive)
          assertNonNullable primitive.Guid "guid"
          |> reader.Map(PrimitiveValue.Guid >> Primitive)
          assertValue primitive.String "string"
          |> reader.Map(PrimitiveValue.String >> Primitive)
          assertNonNullable primitive.Date "date"
          |> reader.Map(PrimitiveValue.Date >> Primitive)
          assertNonNullable primitive.DateTime "date time"
          |> reader.Map(PrimitiveValue.DateTime >> Primitive)
          assertNonNullable primitive.TimeSpan "time span"
          |> reader.Map(PrimitiveValue.TimeSpan >> Primitive)
          reader.Return(PrimitiveValue.Unit |> Primitive) ]
      )
    )

  and primitiveFromDTO
    (valueDTO: ValueDTO<'valueExtDTO>)
    : Reader<
        Value<TypeValue<'valueExt>, 'valueExt>,
        SerializationContext<'valueExt, 'valueExtDTO>,
        Errors<unit>
       >
    =
    reader {
      let! primitiveValue = assertValue valueDTO.Primitive "primitive"
      return! primitiveValueFromDTO primitiveValue
    }

  and varFromDTO
    (valueDTO: ValueDTO<'valueExtDTO>)
    : Reader<
        Value<TypeValue<'valueExt>, 'valueExt>,
        SerializationContext<'valueExt, 'valueExtDTO>,
        Errors<unit>
       >
    =
    assertValue valueDTO.Var "var" |> reader.Map Var

  and extFromDTO
    (valueDTO: ValueDTO<'valueExtDTO>)
    : Reader<
        Value<TypeValue<'valueExt>, 'valueExt>,
        SerializationContext<'valueExt, 'valueExtDTO>,
        Errors<unit>
       >
    =
    reader {
      let! extDTO = assertValue valueDTO.Ext "extension"
      let! context = reader.GetContext()
      return! context.FromDTO extDTO None
    }

  and extWithIdFromDTO
    (valueDTO: ValueDTO<'valueExtDTO>)
    : Reader<
        Value<TypeValue<'valueExt>, 'valueExt>,
        SerializationContext<'valueExt, 'valueExtDTO>,
        Errors<unit>
       >
    =
    reader {
      let! extDTO = assertValue valueDTO.ExtWithId "extension with applicable"

      let! applicableIdDTO, extDTO =
        assertSingleElementDictionary extDTO "extension with applicable"

      let! applicableId =
        ResolvedIdentifier.TryParse applicableIdDTO |> reader.OfSum

      let! context = reader.GetContext()
      return! context.FromDTO extDTO (Some applicableId)
    }

  and valueFromDTO
    (valueDTO: ValueDTO<'valueExtDTO>)
    : Reader<
        Value<TypeValue<'valueExt>, 'valueExt>,
        SerializationContext<'valueExt, 'valueExtDTO>,
        Errors<unit>
       >
    =
    reader.Any(
      (recordFromDTO valueDTO,
       [ unionCaseFromDTO valueDTO
         tupleFromDTO valueDTO
         sumFromDTO valueDTO
         primitiveFromDTO valueDTO
         varFromDTO valueDTO
         extFromDTO valueDTO
         reader.Throw(
           Errors.Singleton () (fun _ ->
             $"The value {valueDTO} cannot be converted from DTO.")
         ) ])
      |> NonEmptyList
    )

  type Value<'T, 'valueExt> with
    static member JsonDeserializeV2
      (json: string)
      : Reader<
          Value<TypeValue<'valueExt>, 'valueExt>,
          SerializationContext<'valueExt, 'valueExtDTO>,
          Errors<unit>
         >
      =
      reader {
        let value: ValueDTO<'valueExtDTO> =
          JsonSerializer.Deserialize<ValueDTO<'valueExtDTO>>(
            json,
            jsonSerializationConfiguration
          )

        return! valueFromDTO value
      }

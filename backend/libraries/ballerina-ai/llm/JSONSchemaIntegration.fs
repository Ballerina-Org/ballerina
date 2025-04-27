namespace Ballerina.AI.LLM

[<RequireQualifiedAccess>]
module JSONSchemaIntegration =
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open NJsonSchema
  open FSharp.Data
  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Types.Model
  open System

  let private discriminatorFieldName = "discriminator"
  let private valueFieldName = "value"

  let private schemaToPropertySchema (schema: JsonSchema) =
    let propertySchema =
      JsonSchemaProperty(
        Type = schema.Type,
        IsRequired = true,
        Default = schema.Default,
        Format = schema.Format,
        AdditionalPropertiesSchema = schema.AdditionalPropertiesSchema,
        MinLength = schema.MinLength,
        MaxLength = schema.MaxLength,
        Minimum = schema.Minimum,
        Maximum = schema.Maximum,
        MultipleOf = schema.MultipleOf,
        Pattern = schema.Pattern,
        AllowAdditionalItems = schema.AllowAdditionalItems,
        AllowAdditionalProperties = schema.AllowAdditionalProperties,
        Item = schema.Item
      )

    for property in schema.Properties do
      propertySchema.Properties.Add(property.Key, property.Value)

    for oneOf in schema.OneOf do
      propertySchema.OneOf.Add oneOf

    propertySchema

  let private makeObjectJsonSchema (fields: (string * JsonSchema) list) =
    let schema =
      JsonSchema(Type = JsonObjectType.Object, AllowAdditionalProperties = false, AllowAdditionalItems = false)

    for fieldName, fieldSchema in fields do
      schema.Properties.Add(fieldName, schemaToPropertySchema fieldSchema)

    schema

  let private makeOneOfJsonSchema (fields: (CaseName * JsonSchema) list) =
    let schema =
      JsonSchema(Type = JsonObjectType.Object, Discriminator = discriminatorFieldName)

    let oneOfSchemas =
      fields
      |> List.map (fun (key, value) ->
        let oneOfSchema = JsonSchema(Type = JsonObjectType.Object)

        let discriminatorProperty =
          JsonSchemaProperty(Type = JsonObjectType.String, IsRequired = true)

        discriminatorProperty.Enumeration.Add key.CaseName

        oneOfSchema.Properties.Add(discriminatorFieldName, discriminatorProperty)

        oneOfSchema.Properties.Add(valueFieldName, schemaToPropertySchema value)
        oneOfSchema)

    for oneOfSchema in oneOfSchemas do
      schema.OneOf.Add oneOfSchema

    schema

  let private sumToUnion (left: ExprType) (right: ExprType) =
    Map.ofList
      [ { CaseName = "left" }, { CaseName = "left"; Fields = left }
        { CaseName = "right" }, { CaseName = "right"; Fields = right } ]

  let private optionToUnion (inner: ExprType) =
    Map.ofList
      [ { CaseName = "some" }, { CaseName = "some"; Fields = inner }
        { CaseName = "none" },
        { CaseName = "none"
          Fields = ExprType.UnitType } ]


  let private tupleKeys =
    [ "first"
      "second"
      "third"
      "fourth"
      "fifth"
      "sixth"
      "seventh"
      "eighth"
      "ninth"
      "tenth" ]

  let private tupleToRecord (items: ExprType list) =

    match items with
    | _ when List.length items > List.length tupleKeys ->
      sum.Throw(Errors.Singleton $"Error: tuple type with more than {List.length tupleKeys} items not implemented")
    | items -> Left(Map.ofList (List.zip tupleKeys[0 .. List.length items - 1] items))


  let generateJsonSchema (t: ExprType) =
    let rec eval (t: ExprType) =
      let (!) = eval

      sum {
        match t with
        | ExprType.UnitType -> JsonSchema(Type = JsonObjectType.Null)
        | ExprType.PrimitiveType p ->
          match p with
          | PrimitiveType.BoolType -> JsonSchema(Type = JsonObjectType.Boolean)
          | PrimitiveType.IntType -> JsonSchema(Type = JsonObjectType.Integer, Format = JsonFormatStrings.Integer)
          | PrimitiveType.FloatType -> JsonSchema(Type = JsonObjectType.Number, Format = JsonFormatStrings.Float)
          | PrimitiveType.GuidType -> JsonSchema(Type = JsonObjectType.String, Format = "uuid")
          | PrimitiveType.StringType -> JsonSchema(Type = JsonObjectType.String)
          | PrimitiveType.DateOnlyType -> JsonSchema(Type = JsonObjectType.String, Format = "date")
          | PrimitiveType.DateTimeType -> JsonSchema(Type = JsonObjectType.String, Format = "date-time")
          | PrimitiveType.RefType _ -> return! sum.Throw(Errors.Singleton $"Error: ref type not implemented for {p}")
        | ExprType.ListType e ->
          let! elementType = !e
          return JsonSchema(Type = JsonObjectType.Array, Item = elementType)
        | ExprType.MapType(k, v) ->
          match k with
          | ExprType.PrimitiveType PrimitiveType.StringType ->
            let! valueType = !v
            JsonSchema(Type = JsonObjectType.Object, AdditionalPropertiesSchema = valueType)
          | _ -> return! sum.Throw(Errors.Singleton $"Error: not implemented default value for map key type {k}")
        | ExprType.SumType(lt, rt) -> return! !ExprType.UnionType(sumToUnion lt rt)
        | ExprType.OptionType e -> return! !ExprType.UnionType(optionToUnion e)
        | ExprType.SetType e -> return! !ExprType.ListType(e)
        | ExprType.RecordType fields ->
          let! schemaByField =
            fields
            |> Map.toList
            |> List.map (fun (key, value) ->
              match !value with
              | Left schema -> Left(key, schema)
              | Right e -> Right e)
            |> sum.All

          schemaByField |> makeObjectJsonSchema
        | ExprType.TupleType items ->
          let! asRecord = tupleToRecord items
          return! !ExprType.RecordType(asRecord)
        | ExprType.LookupType l -> return! sum.Throw(Errors.Singleton $"Error: lookup type {l} not implemented")
        | ExprType.UnionType cs ->
          let! schemaByCase =
            cs
            |> Map.toList
            |> List.map (fun (key, case) ->
              match !case.Fields with
              | Left schema -> Left(key, schema)
              | Right e -> Right e)
            |> sum.All

          return schemaByCase |> makeOneOfJsonSchema
        | ExprType.CustomType _ -> return! sum.Throw(Errors.Singleton $"Error: custom type not implemented")
        | ExprType.VarType _ -> return! sum.Throw(Errors.Singleton $"Error: var type not implemented")
        | ExprType.SchemaLookupType _ ->
          return! sum.Throw(Errors.Singleton $"Error: schema lookup type not implemented")
        | ExprType.TableType _ -> return! sum.Throw(Errors.Singleton $"Error: table type not implemented")
        | ExprType.OneType e -> return! sum.Throw(Errors.Singleton $"Error: one type not implemented {e}")
        | ExprType.ManyType e -> return! sum.Throw(Errors.Singleton $"Error: many type not implemented {e}")
      }

    eval t



  let parseJsonResult (t: ExprType) (LLM.LLMOutput data) =
    let rec eval (t: ExprType) (data: JsonValue) : Sum<Value, Errors> =
      sum {
        match t with
        | ExprType.UnitType ->
          match data with
          | JsonValue.Null -> Value.Unit
          | unexpected -> return! sum.Throw(Errors.Singleton $"Error: null expected for unit type, got {unexpected}")
        | ExprType.PrimitiveType p ->
          match p with
          | PrimitiveType.BoolType ->
            match data with
            | JsonValue.Boolean b -> Value.ConstBool b
            | unexpected ->
              return! sum.Throw(Errors.Singleton $"Error: boolean expected for bool type, got {unexpected}")
          | PrimitiveType.IntType ->
            match data with
            | JsonValue.Number n -> Value.ConstInt(int n)
            | unexpected ->
              return! sum.Throw(Errors.Singleton $"Error: integer expected for int type, got {unexpected}")
          | PrimitiveType.FloatType ->
            match data with
            | JsonValue.Number n -> Value.ConstFloat(float n)
            | unexpected ->
              return! sum.Throw(Errors.Singleton $"Error: float expected for float type, got {unexpected}")
          | PrimitiveType.GuidType ->
            match data with
            | JsonValue.String s ->
              match Guid.TryParse s with
              | true, guid -> Value.ConstGuid guid
              | false, _ -> return! sum.Throw(Errors.Singleton $"Error: guid expected for guid type, got {s}")
            | unexpected -> return! sum.Throw(Errors.Singleton $"Error: guid expected for guid type, got {unexpected}")
          | PrimitiveType.StringType ->
            match data with
            | JsonValue.String s -> Value.ConstString s
            | unexpected ->
              return! sum.Throw(Errors.Singleton $"Error: string expected for string type, got {unexpected}")
          | PrimitiveType.DateOnlyType ->
            match data with
            | JsonValue.String s ->
              match DateOnly.TryParse s with
              | true, date -> Value.ConstString(date.ToString "yyyy-MM-dd")
              | false, _ -> return! sum.Throw(Errors.Singleton $"Error: date expected for date type, got {s}")
            | unexpected -> return! sum.Throw(Errors.Singleton $"Error: date expected for date type, got {unexpected}")
          | PrimitiveType.DateTimeType ->
            match data with
            | JsonValue.String s ->
              match DateTime.TryParse s with
              | true, date -> Value.ConstString(date.ToString "yyyy-MM-ddTHH:mm:ssZ")
              | false, _ -> return! sum.Throw(Errors.Singleton $"Error: date time expected for date time type, got {s}")
            | unexpected ->
              return! sum.Throw(Errors.Singleton $"Error: date time expected for date time type, got {unexpected}")
          | PrimitiveType.RefType _ -> return! sum.Throw(Errors.Singleton $"Error: ref type not implemented")
        | ExprType.ListType e ->
          match data with
          | JsonValue.Array arr -> return! arr |> Array.map (eval e) |> sum.All |> Sum.map Value.Tuple
          | unexpected -> return! sum.Throw(Errors.Singleton $"Error: array expected for list type, got {unexpected}")
        | ExprType.MapType(k, v) ->
          match k with
          | ExprType.PrimitiveType PrimitiveType.StringType ->
            match data with
            | JsonValue.Record obj ->
              let! fields =
                obj
                |> Array.map (fun (key, value) -> eval v value |> Sum.map (fun v -> key, v))
                |> sum.All

              return fields |> Map.ofList |> Value.Record
            | unexpected -> return! sum.Throw(Errors.Singleton $"Error: object expected for map type, got {unexpected}")
          | unexpected -> return! sum.Throw(Errors.Singleton $"Error: map keys can only be strings, got {unexpected}")
        | ExprType.SumType(lt, rt) -> return! eval (ExprType.UnionType(sumToUnion lt rt)) data
        | ExprType.OptionType e -> return! eval (ExprType.UnionType(optionToUnion e)) data
        | ExprType.SetType e -> return! eval (ExprType.ListType e) data
        | ExprType.UnionType cs ->
          match data with
          | JsonValue.Record obj ->
            let asMap = Map.ofArray obj

            match Map.tryFind discriminatorFieldName asMap with
            | Some(JsonValue.String discriminator) ->
              match Map.tryFind { CaseName = discriminator } cs with
              | Some case ->
                match Map.tryFind "value" asMap with
                | Some value -> return! eval case.Fields value |> Sum.map (fun v -> Value.CaseCons(discriminator, v))
                | None -> return! sum.Throw(Errors.Singleton $"Error: value expected, got {data}")
              | None -> return! sum.Throw(Errors.Singleton $"Error: discriminator {discriminator} not found in {cs}")
            | _ -> return! sum.Throw(Errors.Singleton $"Error: discriminator expected, got {data}")
          | _ -> return! sum.Throw(Errors.Singleton $"Error: union type expected, got {data}")
        | ExprType.RecordType l ->
          match data with
          | JsonValue.Record obj ->
            let! fields =
              l
              |> Map.toList
              |> List.map (fun (attrName, attrType) ->

                let jsonAttributeValue =
                  Array.tryFind (fun (dataAttr, _) -> dataAttr = attrName) obj

                match jsonAttributeValue with
                | Some(_, value) -> eval attrType value |> Sum.map (fun v -> attrName, v)
                | None -> sum.Throw(Errors.Singleton $"Error: attribute {attrName} not found in {data}"))
              |> sum.All

            fields |> Map.ofList |> Value.Record
          | _ -> return! sum.Throw(Errors.Singleton $"Error: object expected for record type, got {data}")
        | ExprType.CustomType _ -> return! sum.Throw(Errors.Singleton $"Error: custom type not implemented")
        | ExprType.VarType _ -> return! sum.Throw(Errors.Singleton $"Error: var type not implemented")
        | ExprType.SchemaLookupType _ ->
          return! sum.Throw(Errors.Singleton $"Error: schema lookup type not implemented")
        | ExprType.TableType _ -> return! sum.Throw(Errors.Singleton $"Error: table type not implemented")
        | ExprType.OneType e -> return! sum.Throw(Errors.Singleton $"Error: one type not implemented {e}")
        | ExprType.ManyType e -> return! sum.Throw(Errors.Singleton $"Error: many type not implemented {e}")
        | ExprType.LookupType l -> return! sum.Throw(Errors.Singleton $"Error: lookup type not implemented {l}")
        | ExprType.TupleType items ->
          let! asRecord = tupleToRecord items
          let! recordValue = eval (ExprType.RecordType asRecord) data

          match recordValue with
          | Value.Record r ->
            return!
              tupleKeys
              |> List.map (fun key ->
                match Map.tryFind key r with
                | Some v -> Left v
                | None -> sum.Throw(Errors.Singleton $"Error: key {key} not found in {r}"))
              |> sum.All
              |> Sum.map Value.Tuple
          | _ -> return! sum.Throw(Errors.Singleton $"Error: record type expected, got {recordValue}")
      }

    match data |> JsonValue.TryParse with
    | Some json -> eval t json
    | None -> sum.Throw(Errors.Singleton $"Error: invalid json {data}")

  let private promptForSchema (t: JsonSchema) =
    LLM.OutputStructureDescriptionForPrompt $"Output schema: {t.ToString()}"

  let JSONSchemaIntegration: LLM.StructuredOutputIntegration<JsonSchema> =
    LLM.StructuredOutputIntegration(fun t ->
      t
      |> (fun t ->
        match t with
        | ExprType.RecordType _ -> Left t
        | _ -> sum.Throw(Errors.Singleton $"Top level type expected to be a record type, got {t}"))
      |> Sum.bind generateJsonSchema
      |> Sum.map (fun schema -> promptForSchema schema, schema),
      parseJsonResult t)

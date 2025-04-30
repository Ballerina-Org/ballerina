namespace Ballerina.AI.LLM

module private DerivedFormsConversions =
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Types.Model

  let private indexToKey (i: int) = $"Item{i}"

  type ExprType with
    static member SumTypeToUnionType(left: ExprType, right: ExprType) =
      Map.ofList
        [ { CaseName = "left" }, { CaseName = "left"; Fields = left }
          { CaseName = "right" }, { CaseName = "right"; Fields = right } ]

    static member OptionTypeToUnionType(inner: ExprType) =
      Map.ofList
        [ { CaseName = "some" }, { CaseName = "some"; Fields = inner }
          { CaseName = "none" },
          { CaseName = "none"
            Fields = ExprType.UnitType } ]


    static member TupleTypeToRecordType(items: ExprType list) =
      items |> List.mapi (fun i item -> indexToKey i, item) |> Map.ofList

    static member RecordValueToTupleType(record: Map<string, Value>) =
      [ 0 .. record.Count - 1 ]
      |> List.map indexToKey
      |> List.map (fun i ->
        match Map.tryFind i record with
        | Some value -> Left value
        | None -> sum.Throw(Errors.Singleton $"Error: key {i} not found in {record}"))
      |> sum.All

module private Errors =
  open Ballerina.Errors

  type Errors =
    static member Singletons =
      {| NotImplemented = fun desc -> Errors.Singleton $"Error: {desc} not implemented"
         Expectation = fun expected actual -> Errors.Singleton $"Error: {expected} expected, got {actual}"
         Type = fun typeName actual -> Errors.Singleton $"Error: {typeName} type, got {actual}" |}


module private JsonSchemaExtensions =
  open NJsonSchema
  open Ballerina.DSL.Expr.Types.Model
  open Ballerina.Core.Json

  let discriminatorFieldName = "discriminator"

  let valueFieldName = "value"

  type JsonSchema with
    static member private ToPropertySchema(schema: JsonSchema) =
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
          Item = schema.Item,
          Reference = schema.Reference
        )

      // Copy collections that aren't supported in the constructor
      for property in schema.Properties do
        propertySchema.Properties.Add(property.Key, property.Value)

      for oneOf in schema.OneOf do
        propertySchema.OneOf.Add oneOf

      for allOf in schema.AllOf do
        propertySchema.AllOf.Add allOf

      for anyOf in schema.AnyOf do
        propertySchema.AnyOf.Add anyOf

      for required in schema.RequiredProperties do
        propertySchema.RequiredProperties.Add required

      propertySchema

    static member MakeObjectJsonSchema(fields: (string * JsonSchema) list) =
      let schema =
        JsonSchema(Type = JsonObjectType.Object, AllowAdditionalProperties = false, AllowAdditionalItems = false)

      for fieldName, fieldSchema in fields do
        schema.Properties.Add(fieldName, JsonSchema.ToPropertySchema fieldSchema)

      schema

    static member MakeOneOfJsonSchema(fields: (CaseName * JsonSchema) list) =
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
          oneOfSchema.Properties.Add(valueFieldName, JsonSchema.ToPropertySchema value)
          oneOfSchema)

      for oneOfSchema in oneOfSchemas do
        schema.OneOf.Add oneOfSchema

      schema

    static member WithDefinitions (definitions: Map<TypeId, JsonSchema>) (schema: JsonSchema) =
      let outputSchema =
        JsonSchema(
          Type = schema.Type,
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
          Item = schema.Item,
          Reference = schema.Reference,
          Discriminator = schema.Discriminator
        )

      for definition in definitions do
        outputSchema.Definitions.Add(definition.Key.TypeName, definition.Value)

      // Copy collections that aren't supported in the constructor
      for property in schema.Properties do
        outputSchema.Properties.Add(property.Key, property.Value)

      for oneOf in schema.OneOf do
        outputSchema.OneOf.Add oneOf

      for allOf in schema.AllOf do
        outputSchema.AllOf.Add allOf

      for anyOf in schema.AnyOf do
        outputSchema.AnyOf.Add anyOf

      for required in schema.RequiredProperties do
        outputSchema.RequiredProperties.Add required

      outputSchema

module JSONSchemaIntegration =
  open Ballerina.Collections.Sum
  open NJsonSchema
  open FSharp.Data
  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Types.Model
  open System
  open DerivedFormsConversions
  open Errors
  open Ballerina.Errors
  open JsonSchemaExtensions
  open Ballerina.Core.Json


  type ExprType with
    static member GenerateJsonSchema (otherTypes: (TypeId * ExprType) list) (t: ExprType) =
      let rec eval (context: Map<TypeId, JsonSchema>) (t: ExprType) =
        let (!) = eval context

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
          | ExprType.SumType(lt, rt) -> return! !ExprType.UnionType(ExprType.SumTypeToUnionType(lt, rt))
          | ExprType.OptionType e -> return! !ExprType.UnionType(ExprType.OptionTypeToUnionType(e))
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

            schemaByField |> JsonSchema.MakeObjectJsonSchema
          | ExprType.TupleType items -> return! items |> ExprType.TupleTypeToRecordType |> ExprType.RecordType |> (!)
          | ExprType.LookupType typeId ->
            match context |> Map.tryFind typeId with
            | Some schema -> JsonSchema(Reference = schema)
            | None -> return! sum.Throw(Errors.Singleton $"Error: lookup type {typeId} not found")
          | ExprType.UnionType cs ->
            let! schemaByCase =
              cs
              |> Map.toList
              |> List.map (fun (key, case) ->
                match !case.Fields with
                | Left schema -> Left(key, schema)
                | Right e -> Right e)
              |> sum.All

            return schemaByCase |> JsonSchema.MakeOneOfJsonSchema
          | ExprType.CustomType _ -> return! sum.Throw(Errors.Singletons.NotImplemented "custom type")
          | ExprType.VarType _ -> return! sum.Throw(Errors.Singletons.NotImplemented "var type")
          | ExprType.SchemaLookupType _ -> return! sum.Throw(Errors.Singletons.NotImplemented "schema lookup type")
          | ExprType.TableType _ -> return! sum.Throw(Errors.Singletons.NotImplemented "table type")
          | ExprType.OneType _ -> return! sum.Throw(Errors.Singletons.NotImplemented "one type")
          | ExprType.ManyType _ -> return! sum.Throw(Errors.Singletons.NotImplemented "many type")
        }

      sum {
        let! otherTypesSchemas =
          otherTypes
          |> List.fold
            (fun acc (typeId, t) ->
              Sum.bind
                (fun acc ->
                  sum {
                    let! schema = eval acc t
                    return Map.add typeId schema acc
                  })
                acc)
            (Left Map.empty)

        let! outputSchema = eval otherTypesSchemas t
        return outputSchema |> JsonSchema.WithDefinitions otherTypesSchemas
      }

    static member ParseJsonResult (outputType: LLM.TypeDeclaration) (LLM.LLMOutput data) =
      let rec eval (otherTypes: Map<TypeId, ExprType>) (t: ExprType) (data: JsonValue) : Sum<Value, Errors> =
        let (!) = eval otherTypes

        sum {
          match t with
          | ExprType.UnitType ->
            match data with
            | JsonValue.Null -> Value.Unit
            | unexpected -> return! sum.Throw(Errors.Singletons.Expectation "null" $"{unexpected}")
          | ExprType.PrimitiveType p ->
            match p with
            | PrimitiveType.BoolType ->
              match data with
              | JsonValue.Boolean b -> Value.ConstBool b
              | unexpected -> return! sum.Throw(Errors.Singletons.Type "bool" $"{unexpected}")
            | PrimitiveType.IntType ->
              match data with
              | JsonValue.Number n -> Value.ConstInt(int n)
              | unexpected -> return! sum.Throw(Errors.Singletons.Type "integer" $"{unexpected}")
            | PrimitiveType.FloatType ->
              match data with
              | JsonValue.Number n -> Value.ConstFloat(float n)
              | unexpected -> return! sum.Throw(Errors.Singletons.Type "float" $"{unexpected}")
            | PrimitiveType.GuidType ->
              match data with
              | JsonValue.String s ->
                match Guid.TryParse s with
                | true, guid -> Value.ConstGuid guid
                | false, _ -> return! sum.Throw(Errors.Singletons.Type "guid" s)
              | unexpected -> return! sum.Throw(Errors.Singletons.Type "guid" $"{unexpected}")
            | PrimitiveType.StringType ->
              match data with
              | JsonValue.String s -> Value.ConstString s
              | unexpected -> return! sum.Throw(Errors.Singletons.Type "string" $"{unexpected}")
            | PrimitiveType.DateOnlyType ->
              match data with
              | JsonValue.String s ->
                match DateOnly.TryParse s with
                | true, date -> Value.ConstString(date.ToString "yyyy-MM-dd")
                | false, _ -> return! sum.Throw(Errors.Singletons.Type "date" s)
              | unexpected -> return! sum.Throw(Errors.Singletons.Type "date" $"{unexpected}")
            | PrimitiveType.DateTimeType ->
              match data with
              | JsonValue.String s ->
                match DateTime.TryParse s with
                | true, date -> Value.ConstString(date.ToString "yyyy-MM-ddTHH:mm:ssZ")
                | false, _ -> return! sum.Throw(Errors.Singletons.Type "date time" s)
              | unexpected -> return! sum.Throw(Errors.Singletons.Type "date time" $"{unexpected}")
            | PrimitiveType.RefType _ -> return! sum.Throw(Errors.Singletons.NotImplemented "ref type")
          | ExprType.ListType e ->
            match data with
            | JsonValue.Array arr -> return! arr |> Array.map (!e) |> sum.All |> Sum.map Value.Tuple
            | unexpected -> return! sum.Throw(Errors.Singletons.Type "array" $"{unexpected}")
          | ExprType.MapType(k, v) ->
            match k with
            | ExprType.PrimitiveType PrimitiveType.StringType ->
              match data with
              | JsonValue.Record obj ->
                let! fields =
                  obj
                  |> Array.map (fun (key, value) -> ! v value |> Sum.map (fun v -> key, v))
                  |> sum.All

                return fields |> Map.ofList |> Value.Record
              | unexpected -> return! sum.Throw(Errors.Singletons.Type "object" $"{unexpected}")
            | unexpected -> return! sum.Throw(Errors.Singleton $"Error: map keys can only be strings, got {unexpected}")
          | ExprType.SumType(lt, rt) -> return! ! (ExprType.UnionType(ExprType.SumTypeToUnionType(lt, rt))) data
          | ExprType.OptionType e -> return! ! (ExprType.UnionType(ExprType.OptionTypeToUnionType(e))) data
          | ExprType.SetType e -> return! ! (ExprType.ListType e) data
          | ExprType.UnionType cs ->
            let! asRecord = data |> JsonValue.AsRecord
            let asMap = Map.ofArray asRecord

            let! jsonDiscriminator =
              asMap
              |> Map.tryFindWithError discriminatorFieldName discriminatorFieldName discriminatorFieldName

            let! discriminator = jsonDiscriminator |> JsonValue.AsString
            let! case = cs |> Map.tryFindWithError { CaseName = discriminator } "case" "case"
            let! value = asMap |> Map.tryFindWithError valueFieldName "value" "value"

            return! ! case.Fields value |> Sum.map (fun v -> Value.CaseCons(discriminator, v))

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
                  | Some(_, value) -> ! attrType value |> Sum.map (fun v -> attrName, v)
                  | None -> sum.Throw(Errors.Singleton $"Error: attribute {attrName} not found in {data}"))
                |> sum.All

              fields |> Map.ofList |> Value.Record
            | _ -> return! sum.Throw(Errors.Singletons.Type "object" $"{data}")
          | ExprType.CustomType _ -> return! sum.Throw(Errors.Singletons.NotImplemented "custom type")
          | ExprType.VarType _ -> return! sum.Throw(Errors.Singletons.NotImplemented "var type")
          | ExprType.SchemaLookupType _ -> return! sum.Throw(Errors.Singletons.NotImplemented "schema lookup type")
          | ExprType.TableType _ -> return! sum.Throw(Errors.Singletons.NotImplemented "table type")
          | ExprType.OneType _ -> return! sum.Throw(Errors.Singletons.NotImplemented "one type")
          | ExprType.ManyType _ -> return! sum.Throw(Errors.Singletons.NotImplemented "many type")
          | ExprType.LookupType typeId ->
            match otherTypes |> Map.tryFind typeId with
            | Some t -> return! ! t data
            | None -> return! sum.Throw(Errors.Singleton $"Error: lookup type {typeId} not found")
          | ExprType.TupleType items ->
            let! recordValue = ! (ExprType.RecordType(ExprType.TupleTypeToRecordType items)) data

            match recordValue with
            | Value.Record r -> return! r |> ExprType.RecordValueToTupleType |> Sum.map Value.Tuple
            | _ -> return! sum.Throw(Errors.Singletons.Type "record" $"{recordValue}")
        }

      match data |> JsonValue.TryParse with
      | Some json -> eval (outputType.Refs |> Map.ofList) outputType.OutputType json
      | None -> sum.Throw(Errors.Singleton $"Error: invalid json {data}")

  let private promptForSchema (t: JsonSchema) =
    LLM.OutputStructureDescriptionForPrompt $"Output schema: {t.ToJson()}"

  let JSONSchemaIntegration: LLM.StructuredOutputIntegration<JsonSchema> =
    LLM.StructuredOutputIntegration(fun t ->
      t
      |> (fun t ->
        match t.OutputType with
        | ExprType.RecordType _ -> Left t
        | _ -> sum.Throw(Errors.Singleton $"Top level type expected to be a record type, got {t}"))
      |> Sum.bind (fun t -> ExprType.GenerateJsonSchema t.Refs t.OutputType)
      |> Sum.map (fun schema -> promptForSchema schema, schema),
      ExprType.ParseJsonResult t)

module Ballerina.Core.Tests.JSONSchemaIntegration

open Ballerina.Collections.Sum
open NUnit.Framework
open Ballerina.DSL.Expr.Types.Model
open FSharp.Data
open Ballerina.DSL.Expr.Model
open Ballerina.Errors

module LLM = Ballerina.AI.LLM.LLM
module JSONSchemaIntegration = Ballerina.AI.LLM.JSONSchemaIntegration

module Model = Ballerina.DSL.Expr.Types.Model

let private jsonSchemaSchema = "http://json-schema.org/draft-04/schema#"

let private createEnumCase (caseName: string) : Model.CaseName * Model.UnionCase =
  { CaseName = caseName },
  { CaseName = caseName
    Fields = ExprType.UnitType }

let private createUnion (cases: Map<string, ExprType>) =
  cases
  |> Map.toList
  |> List.map (fun (caseName, unionCase) ->
    { CaseName = caseName },
    { CaseName = caseName
      Fields = unionCase })
  |> Map.ofList
  |> ExprType.UnionType

let private getJSONSchemaAsJSON exprType =
  exprType
  |> JSONSchemaIntegration.generateJsonSchema
  |> Sum.map (fun schema -> schema.ToJson())
  |> Sum.bind (fun schema ->
    match JsonValue.TryParse schema with
    | Some json -> Left json
    | None -> sum.Throw(Errors.Singleton "Failed to parse JSON schema"))

module JSONSchemaConversion =
  [<Test>]
  let TestUnitType () =
    let result = ExprType.UnitType |> getJSONSchemaAsJSON

    let expected =
      JsonValue.Record
        [| "$schema", JsonValue.String jsonSchemaSchema
           "type", JsonValue.String "null" |]

    match result with
    | Left jsonSchema -> Assert.That(jsonSchema, Is.EqualTo expected)
    | Right err -> Assert.Fail(err.ToString())

  [<Test>]
  let TestUnionType () =
    let result =
      createUnion (Map.ofList [ "option1", ExprType.PrimitiveType StringType; "option2", ExprType.UnitType ])
      |> getJSONSchemaAsJSON

    let expected =
      JsonValue.Record
        [| "$schema", JsonValue.String jsonSchemaSchema
           "type", JsonValue.String "object"
           "discriminator", JsonValue.Record [| "propertyName", JsonValue.String "discriminator" |]
           "oneOf",
           JsonValue.Array
             [| JsonValue.Record
                  [| "type", JsonValue.String "object"
                     "required", JsonValue.Array [| JsonValue.String "discriminator"; JsonValue.String "value" |]
                     "properties",
                     JsonValue.Record
                       [| "discriminator",
                          JsonValue.Record
                            [| "type", JsonValue.String "string"
                               "enum", JsonValue.Array [| JsonValue.String "option1" |] |]
                          "value", JsonValue.Record [| "type", JsonValue.String "string" |] |] |]
                JsonValue.Record
                  [| "type", JsonValue.String "object"
                     "required", JsonValue.Array [| JsonValue.String "discriminator"; JsonValue.String "value" |]
                     "properties",
                     JsonValue.Record
                       [| "discriminator",
                          JsonValue.Record
                            [| "type", JsonValue.String "string"
                               "enum", JsonValue.Array [| JsonValue.String "option2" |] |]
                          "value", JsonValue.Record [| "type", JsonValue.String "null" |] |] |] |] |]

    match result with
    | Left schema -> Assert.That(schema, Is.EqualTo expected)
    | Right err -> Assert.Fail(err.ToString())

  [<Test>]
  let TestTupleType () =
    let result =
      ExprType.TupleType [ ExprType.PrimitiveType StringType; ExprType.PrimitiveType IntType ]
      |> getJSONSchemaAsJSON

    let expected =
      JsonValue.Record
        [| "$schema", JsonValue.String jsonSchemaSchema
           "type", JsonValue.String "object"
           "additionalItems", JsonValue.Boolean false
           "additionalProperties", JsonValue.Boolean false
           "required", JsonValue.Array [| JsonValue.String "first"; JsonValue.String "second" |]
           "properties",
           JsonValue.Record
             [| "first", JsonValue.Record [| "type", JsonValue.String "string" |]
                "second", JsonValue.Record [| "type", JsonValue.String "integer"; "format", JsonValue.String "int32" |] |] |]

    match result with
    | Left schema -> Assert.That(schema, Is.EqualTo expected)
    | Right err -> Assert.Fail(err.ToString())

  [<Test>]

  let TestRecordType () =
    let result =
      ExprType.RecordType(
        Map.ofList
          [ "first", ExprType.PrimitiveType StringType
            "second", ExprType.PrimitiveType IntType ]
      )
      |> getJSONSchemaAsJSON

    let expected =
      JsonValue.Record
        [| "$schema", JsonValue.String jsonSchemaSchema
           "type", JsonValue.String "object"
           "additionalItems", JsonValue.Boolean false
           "additionalProperties", JsonValue.Boolean false
           "required", JsonValue.Array [| JsonValue.String "first"; JsonValue.String "second" |]
           "properties",
           JsonValue.Record
             [| "first", JsonValue.Record [| "type", JsonValue.String "string" |]
                "second", JsonValue.Record [| "type", JsonValue.String "integer"; "format", JsonValue.String "int32" |] |] |]

    match result with
    | Left schema -> Assert.That(schema, Is.EqualTo expected)
    | Right err -> Assert.Fail(err.ToString())

  let TestListType () =
    let result =
      ExprType.ListType(ExprType.PrimitiveType StringType) |> getJSONSchemaAsJSON

    let expected =
      JsonValue.Record
        [| "$schema", JsonValue.String jsonSchemaSchema
           "type", JsonValue.String "array"
           "items", JsonValue.Record [| "type", JsonValue.String "string" |] |]

    match result with
    | Left schema -> Assert.That(schema, Is.EqualTo expected)
    | Right err -> Assert.Fail(err.ToString())

module JSONParsing =
  [<Test>]
  let TestUnionType () =
    let typeDefinition =
      createUnion (Map.ofList [ "option1", ExprType.PrimitiveType StringType; "option2", ExprType.UnitType ])

    let data = LLM.LLMOutput """{ "discriminator": "option1", "value": "hello" }"""

    let result = data |> JSONSchemaIntegration.parseJsonResult typeDefinition

    let expected = Value.CaseCons("option1", Value.ConstString "hello")

    match result with
    | Left value -> Assert.That(value, Is.EqualTo expected)
    | Right err -> Assert.Fail(err.ToString())

  [<Test>]
  let TestListType () =
    let typeDefinition = ExprType.ListType(ExprType.PrimitiveType StringType)

    let data = LLM.LLMOutput """["hello", "world"]"""

    let result = data |> JSONSchemaIntegration.parseJsonResult typeDefinition

    let expected = Value.Tuple [ Value.ConstString "hello"; Value.ConstString "world" ]

    match result with
    | Left value -> Assert.That(value, Is.EqualTo expected)
    | Right err -> Assert.Fail(err.ToString())

  [<Test>]
  let TestRecordType () =
    let typeDefinition =
      ExprType.RecordType(Map.ofList [ "a", ExprType.PrimitiveType StringType; "b", ExprType.PrimitiveType IntType ])

    let data = LLM.LLMOutput """{ "a": "hello", "b": 100 }"""

    let result = data |> JSONSchemaIntegration.parseJsonResult typeDefinition

    let expected =
      Value.Record(Map.ofList [ "a", Value.ConstString "hello"; "b", Value.ConstInt 100 ])

    match result with
    | Left value -> Assert.That(value, Is.EqualTo expected)
    | Right err -> Assert.Fail(err.ToString())

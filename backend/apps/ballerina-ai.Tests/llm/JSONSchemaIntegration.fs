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

let private createUnion (cases: Map<string, ExprType>) =
  cases
  |> Map.toList
  |> List.map (fun (caseName, unionCase) ->
    { CaseName = caseName },
    { CaseName = caseName
      Fields = unionCase })
  |> Map.ofList
  |> ExprType.UnionType

let private assertSuccess<'T> (result: Sum<'T, Errors>) (expected: 'T) =
  match result with
  | Left value -> Assert.That(value, Is.EqualTo expected)
  | Right err -> Assert.Fail($"Expected success but got error: {err}")

let private assertError (result: Sum<'T, Errors>) (expectedError: string) =
  match result with
  | Left value -> Assert.Fail($"Expected success but got error: {value}")
  | Right err -> Assert.That(err.ToString(), Does.Contain expectedError)

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
  let ``UnitType should generate correct JSON schema`` () =
    let result = ExprType.UnitType |> getJSONSchemaAsJSON

    let expected =
      JsonValue.Record
        [| "$schema", JsonValue.String jsonSchemaSchema
           "type", JsonValue.String "null" |]

    assertSuccess result expected

  [<Test>]
  let ``UnionType should generate correct JSON schema with discriminator`` () =
    let typeDefinition =
      createUnion (Map.ofList [ "option1", ExprType.PrimitiveType StringType; "option2", ExprType.UnitType ])

    let result = typeDefinition |> getJSONSchemaAsJSON

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

    assertSuccess result expected

  [<Test>]
  let ``TupleType should generate correct JSON schema with named fields`` () =
    let typeDefinition =
      ExprType.TupleType [ ExprType.PrimitiveType StringType; ExprType.PrimitiveType IntType ]

    let result = typeDefinition |> getJSONSchemaAsJSON

    let expected =
      JsonValue.Record
        [| "$schema", JsonValue.String jsonSchemaSchema
           "type", JsonValue.String "object"
           "additionalItems", JsonValue.Boolean false
           "additionalProperties", JsonValue.Boolean false
           "required", JsonValue.Array [| JsonValue.String "Item0"; JsonValue.String "Item1" |]
           "properties",
           JsonValue.Record
             [| "Item0", JsonValue.Record [| "type", JsonValue.String "string" |]
                "Item1", JsonValue.Record [| "type", JsonValue.String "integer"; "format", JsonValue.String "int32" |] |] |]

    assertSuccess result expected

  [<Test>]
  let ``RecordType should generate correct JSON schema with required fields`` () =
    let typeDefinition =
      ExprType.RecordType(
        Map.ofList
          [ "first", ExprType.PrimitiveType StringType
            "second", ExprType.PrimitiveType IntType ]
      )

    let result = typeDefinition |> getJSONSchemaAsJSON

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

    assertSuccess result expected

  [<Test>]
  let ``ListType should generate correct JSON schema with item type`` () =
    let typeDefinition = ExprType.ListType(ExprType.PrimitiveType StringType)
    let result = typeDefinition |> getJSONSchemaAsJSON

    let expected =
      JsonValue.Record
        [| "$schema", JsonValue.String jsonSchemaSchema
           "type", JsonValue.String "array"
           "items", JsonValue.Record [| "type", JsonValue.String "string" |] |]

    assertSuccess result expected

module JSONParsing =
  [<Test>]
  let ``UnionType should parse JSON with discriminator correctly`` () =
    let typeDefinition =
      createUnion (Map.ofList [ "option1", ExprType.PrimitiveType StringType; "option2", ExprType.UnitType ])

    let data = LLM.LLMOutput """{ "discriminator": "option1", "value": "hello" }"""
    let result = data |> JSONSchemaIntegration.parseJsonResult typeDefinition
    let expected = Value.CaseCons("option1", Value.ConstString "hello")
    assertSuccess result expected

  [<Test>]
  let ``ListType should parse JSON array correctly`` () =
    let typeDefinition = ExprType.ListType(ExprType.PrimitiveType StringType)
    let data = LLM.LLMOutput """["hello", "world"]"""
    let result = data |> JSONSchemaIntegration.parseJsonResult typeDefinition
    let expected = Value.Tuple [ Value.ConstString "hello"; Value.ConstString "world" ]
    assertSuccess result expected

  [<Test>]
  let ``RecordType should parse JSON object correctly`` () =
    let typeDefinition =
      ExprType.RecordType(
        Map.ofList
          [ "first", ExprType.PrimitiveType StringType
            "second", ExprType.PrimitiveType IntType ]
      )

    let data = LLM.LLMOutput """{ "first": "hello", "second": 100 }"""
    let result = data |> JSONSchemaIntegration.parseJsonResult typeDefinition

    let expected =
      Value.Record(Map.ofList [ "first", Value.ConstString "hello"; "second", Value.ConstInt 100 ])

    assertSuccess result expected

  [<Test>]
  let ``Invalid JSON should return error`` () =
    let typeDefinition =
      ExprType.RecordType(
        Map.ofList
          [ "first", ExprType.PrimitiveType StringType
            "second", ExprType.PrimitiveType IntType ]
      )

    let data = LLM.LLMOutput """{ invalid json }"""
    let result = data |> JSONSchemaIntegration.parseJsonResult typeDefinition
    assertError result "invalid json"

  [<Test>]
  let ``Missing required field should return error`` () =
    let typeDefinition =
      ExprType.RecordType(
        Map.ofList
          [ "first", ExprType.PrimitiveType StringType
            "second", ExprType.PrimitiveType IntType ]
      )

    let data = LLM.LLMOutput """{ "first": "hello" }"""
    let result = data |> JSONSchemaIntegration.parseJsonResult typeDefinition
    assertError result "second"

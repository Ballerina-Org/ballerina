module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Term.Json.Value_TypeValue_Primitive

open System
open Ballerina.Collections.Sum
open NUnit.Framework
open FSharp.Data
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Terms.Json.Primitive

let ``Assert PrimitiveValue -> ToJson -> FromJson -> PrimitiveValue``
  (expression: PrimitiveValue)
  (expectedJson: JsonValue)
  =
  let normalize (json: JsonValue) =
    json.ToString JsonSaveOptions.DisableFormatting

  let toJson = PrimitiveValue.ToJson expression
  Assert.That(normalize toJson, Is.EqualTo(normalize expectedJson))

  let parsed = PrimitiveValue.FromJson expectedJson

  match parsed with
  | Right err -> Assert.Fail $"Parse failed: {err}"
  | Left result -> Assert.That(result, Is.EqualTo(expression))


[<Test>]
let ``Dsl:Term:Value.PrimitiveValue json round-trip`` () =
  let testCases =
    [ """{"kind": "int32", "int32":"123"}""", PrimitiveValue.Int32 123
      """{"kind": "decimal", "decimal":"123.456"}""", PrimitiveValue.Decimal 123.456M
      """{"kind": "boolean", "boolean":"true"}""", PrimitiveValue.Bool true
      """{"kind": "guid", "guid":"00000000-0000-0000-0000-000000000001"}""",
      PrimitiveValue.Guid(System.Guid("00000000-0000-0000-0000-000000000001"))
      """{"kind": "string", "string":"hello"}""", PrimitiveValue.String "hello"
      """{"kind": "date", "date":"2023-10-01"}""", PrimitiveValue.Date(System.DateOnly(2023, 10, 1))
      """{"kind": "datetime", "datetime":"2023-10-01T12:00:00Z"}""",
      PrimitiveValue.DateTime(System.DateTime(2023, 10, 1, 12, 0, 0, DateTimeKind.Utc))
      """{"kind": "unit"}""", PrimitiveValue.Unit ]

  for json, expected in testCases do
    (expected, JsonValue.Parse json)
    ||> ``Assert PrimitiveValue -> ToJson -> FromJson -> PrimitiveValue``

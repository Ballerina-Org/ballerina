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
    [ """{"discriminator": "int32", "value":"123"}""", PrimitiveValue.Int32 123
      """{"discriminator": "decimal", "value":"123.456"}""", PrimitiveValue.Decimal 123.456M
      """{"discriminator": "boolean", "value":"true"}""", PrimitiveValue.Bool true
      """{"discriminator": "guid", "value":"00000000-0000-0000-0000-000000000001"}""",
      PrimitiveValue.Guid(System.Guid("00000000-0000-0000-0000-000000000001"))
      """{"discriminator": "string", "value":"hello"}""", PrimitiveValue.String "hello"
      """{"discriminator": "date", "value":"2023-10-01"}""", PrimitiveValue.Date(System.DateOnly(2023, 10, 1))
      """{"discriminator": "datetime", "value":"2023-10-01T12:00:00.0000000Z"}""",
      PrimitiveValue.DateTime(System.DateTime(2023, 10, 1, 12, 0, 0, DateTimeKind.Utc))
      """{"discriminator": "unit"}""", PrimitiveValue.Unit ]

  for json, expected in testCases do
    (expected, JsonValue.Parse json)
    ||> ``Assert PrimitiveValue -> ToJson -> FromJson -> PrimitiveValue``

module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Type.Json.Type_TypeValue_PrimitiveType

open Ballerina.Fun
open Ballerina
open Ballerina.Collections.Sum
open NUnit.Framework
open FSharp.Data
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Json

let ``Assert PrimitiveType -> ToJson -> FromJson = PrimitiveType``
  (expression: PrimitiveType)
  (expectedJson: JsonValue)
  =
  let normalize (json: JsonValue) =
    json.ToString JsonSaveOptions.DisableFormatting

  let toJson = PrimitiveType.ToJson expression
  Assert.That(normalize toJson, Is.EqualTo(normalize expectedJson))

  let parsed = PrimitiveType.FromJson expectedJson

  match parsed with
  | Right err -> Assert.Fail $"Parse failed: {err}"
  | Left result -> Assert.That(result, Is.EqualTo(expression))

let testCases =
  [ ("""{"discriminator": "unit"}""", PrimitiveType.Unit)
    ("""{"discriminator": "guid"}""", PrimitiveType.Guid)
    ("""{"discriminator": "int32"}""", PrimitiveType.Int32)
    ("""{"discriminator": "int64"}""", PrimitiveType.Int64)
    ("""{"discriminator": "float32"}""", PrimitiveType.Float32)
    ("""{"discriminator": "float64"}""", PrimitiveType.Float64)
    ("""{"discriminator": "decimal"}""", PrimitiveType.Decimal)
    ("""{"discriminator": "string"}""", PrimitiveType.String)
    ("""{"discriminator": "bool"}""", PrimitiveType.Bool)
    ("""{"discriminator": "datetime"}""", PrimitiveType.DateTime)
    ("""{"discriminator": "dateonly"}""", PrimitiveType.DateOnly) ]

[<Test>]
let ``Dsl:Type:Value.PrimitiveType json round-trip`` () =

  for (json, expected) in testCases do
    (expected, JsonValue.Parse json)
    ||> ``Assert PrimitiveType -> ToJson -> FromJson = PrimitiveType``

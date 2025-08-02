module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Type.Json.Type

open System
open Ballerina.Collections.Sum
open NUnit.Framework
open FSharp.Data
open Ballerina.Errors
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.DSL.Next.Json

open Ballerina.DSL.Next.Types.Json.TypeExpr
open Ballerina.DSL.Next.Types.Json.TypeValue
open Ballerina.DSL.Next.Types.Json
open Ballerina.DSL.Next.KitchenSink
open Ballerina.DSL.Next.EquivalenceClasses
open Ballerina.DSL.Next.Unification
open Ballerina.State.WithError

let ``Assert Kind -> ToJson -> FromJson -> Kind`` (expression: Kind) (expectedJson: JsonValue) =
  let toJson = Kind.ToJson expression
  Assert.That(toJson, Is.EqualTo(expectedJson))

  let parsed = Kind.FromJson expectedJson

  match parsed with
  | Right err -> Assert.Fail $"Parse failed: {err}"
  | Left result -> Assert.That(result, Is.EqualTo(expression))

let ``Assert Symbol -> ToJson -> FromJson -> Symbol`` (expression: TypeSymbol) (expectedJson: JsonValue) =
  let toJson = TypeSymbol.ToJson expression
  Assert.That(toJson, Is.EqualTo(expectedJson))

  let parsed = TypeSymbol.FromJson expectedJson

  match parsed with
  | Right err -> Assert.Fail $"Parse failed: {err}"
  | Left result -> Assert.That(result, Is.EqualTo(expression))

let ``Assert Parameter -> ToJson -> FromJson -> Parameter`` (expression: TypeParameter) (expectedJson: JsonValue) =
  let toJson = TypeParameter.ToJson expression
  Assert.That(toJson, Is.EqualTo(expectedJson))

  let parsed = TypeParameter.FromJson expectedJson

  match parsed with
  | Right err -> Assert.Fail $"Parse failed: {err}"
  | Left result -> Assert.That(result, Is.EqualTo(expression))

let ``Assert Var -> ToJson -> FromJson -> Var`` (expression: TypeVar) (expectedJson: JsonValue) =
  let toJson = TypeVar.ToJson expression
  Assert.That(toJson, Is.EqualTo(expectedJson))

  let parsed = TypeVar.FromJson expectedJson

  match parsed with
  | Right err -> Assert.Fail $"Parse failed: {err}"
  | Left result -> Assert.That(result, Is.EqualTo(expression))

[<Test>]
let ``Dsl:Type.Kind json round-trip`` () =
  let testCases =
    [ ("""{"kind": "symbol"}""", Kind.Symbol)
      ("""{"kind": "star"}""", Kind.Star)
      ("""{
            "kind": "arrow",
            "arrow": {
              "param": { "kind": "star" },
              "returnType": { "kind": "symbol" }
            }
          }""",
       Kind.Arrow(Kind.Star, Kind.Symbol)) ]

  for (json, expected) in testCases do
    (expected, JsonValue.Parse json)
    ||> ``Assert Kind -> ToJson -> FromJson -> Kind``

[<Test>]
let ``Dsl:Type.Symbol json round-trip`` () =
  let guid = Guid.NewGuid()
  let json = $"""{{"name": "MyType", "guid": "{guid}"}}"""

  let expected =
    { TypeSymbol.Name = "MyType"
      TypeSymbol.Guid = guid }

  (expected, JsonValue.Parse json)
  ||> ``Assert Symbol -> ToJson -> FromJson -> Symbol``

[<Test>]
let ``Dsl:Type.Parameter json round-trip`` () =
  let json =
    """{"name": "T", "kind": { "kind":"arrow", "arrow": { "param": { "kind": "symbol" }, "returnType": { "kind": "star" } } } }"""

  let expected = TypeParameter.Create("T", Kind.Arrow(Kind.Symbol, Kind.Star))

  (expected, JsonValue.Parse json)
  ||> ``Assert Parameter -> ToJson -> FromJson -> Parameter``

[<Test>]
let ``Dsl:Type.Var json round-trip`` () =
  let guid = Guid.NewGuid()
  let json = $"""{{"name": "MyTypeVar", "guid": "{guid}"}}"""

  let expected =
    { TypeVar.Name = "MyTypeVar"
      TypeVar.Guid = guid }

  (expected, JsonValue.Parse json) ||> ``Assert Var -> ToJson -> FromJson -> Var``

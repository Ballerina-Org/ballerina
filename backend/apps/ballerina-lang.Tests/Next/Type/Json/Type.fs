module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Type.Json.Type

open System
open Ballerina
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
open Ballerina.DSL.Next.EquivalenceClasses
open Ballerina.DSL.Next.Unification
open Ballerina.State.WithError

let ``Assert Kind -> ToJson -> FromJson -> Kind`` (expression: Kind) (expectedJson: JsonValue) : unit =
  let toJson = Kind.ToJson expression
  Assert.That(toJson, Is.EqualTo(expectedJson))

  let parsed = Kind.FromJson expectedJson

  match parsed with
  | Right err -> Assert.Fail $"Parse failed: {err}"
  | Left result -> Assert.That(result, Is.EqualTo(expression))

let ``Assert Symbol -> ToJson -> FromJson -> Symbol`` (expression: TypeSymbol) (expectedJson: JsonValue) : unit =
  let toJson = TypeSymbol.ToJson expression
  Assert.That(toJson, Is.EqualTo(expectedJson))

  let parsed = TypeSymbol.FromJson expectedJson

  match parsed with
  | Right err -> Assert.Fail $"Parse failed: {err}"
  | Left result -> Assert.That(result, Is.EqualTo(expression))

let ``Assert Parameter -> ToJson -> FromJson -> Parameter``
  (expression: TypeParameter)
  (expectedJson: JsonValue)
  : unit =
  let toJson = TypeParameter.ToJson expression
  Assert.That(toJson, Is.EqualTo(expectedJson))

  let parsed = TypeParameter.FromJson expectedJson

  match parsed with
  | Right err -> Assert.Fail $"Parse failed: {err}"
  | Left result -> Assert.That(result, Is.EqualTo(expression))

let ``Assert Var -> ToJson -> FromJson -> Var`` (expression: TypeVar) (expectedJson: JsonValue) : unit =
  let toJson = TypeVar.ToJson expression
  Assert.That(toJson, Is.EqualTo(expectedJson))

  let parsed = TypeVar.FromJson expectedJson

  match parsed with
  | Right err -> Assert.Fail $"Parse failed: {err}"
  | Left result -> Assert.That(result, Is.EqualTo(expression))

let ``Assert ResolvedIdentifier -> ToJson -> FromJson -> ResolvedIdentifier``
  (expression: ResolvedIdentifier)
  (expectedJson: JsonValue)
  : unit =
  let toJson = ResolvedIdentifier.ToJson expression
  Assert.That(toJson, Is.EqualTo(expectedJson))

  let parsed = ResolvedIdentifier.FromJson expectedJson

  match parsed with
  | Right err -> Assert.Fail $"Parse failed: {err}"
  | Left result -> Assert.That(result, Is.EqualTo(expression))



[<Test>]
let ``Dsl:Type.Kind json round-trip`` () =
  let testCases =
    [ """{"discriminator": "symbol"}""", Kind.Symbol
      """{"discriminator": "star"}""", Kind.Star
      """{
            "discriminator": "arrow",
            "value": {
              "param": { "discriminator": "star" },
              "returnType": { "discriminator": "symbol" }
            }
          }""",
      Kind.Arrow(Kind.Star, Kind.Symbol) ]

  for (json, expected) in testCases do
    (expected, JsonValue.Parse json)
    ||> ``Assert Kind -> ToJson -> FromJson -> Kind``

[<Test>]
let ``Dsl:Type.Symbol json round-trip`` () =
  let guid = Guid.NewGuid()
  let json = $"""{{"name": "MyType", "guid": "{guid}"}}"""

  let expected =
    { TypeSymbol.Name = "MyType" |> Identifier.LocalScope
      TypeSymbol.Guid = guid }

  (expected, JsonValue.Parse json)
  ||> ``Assert Symbol -> ToJson -> FromJson -> Symbol``

[<Test>]
let ``Dsl:Type.Parameter json round-trip`` () =
  let json =
    """{"name": "T", "kind": { "discriminator":"arrow", "value": { "param": { "discriminator": "symbol" }, "returnType": { "discriminator": "star" } } } }"""

  let expected = TypeParameter.Create("T", Kind.Arrow(Kind.Symbol, Kind.Star))

  (expected, JsonValue.Parse json)
  ||> ``Assert Parameter -> ToJson -> FromJson -> Parameter``

[<Test>]
let ``Dsl:Type.Var json round-trip`` () =
  let guid = Guid.NewGuid()
  let json = $"""{{"name": "MyTypeVar", "guid": "{guid}"}}"""

  let expected =
    { TypeVar.Name = "MyTypeVar"
      Synthetic = false
      TypeVar.Guid = guid }

  (expected, JsonValue.Parse json) ||> ``Assert Var -> ToJson -> FromJson -> Var``

[<Test>]
let ``Dsl:Type.ResolvedIdentifier json round-trip`` () =
  let testCases =
    [ """{"discriminator": "id", "value": ["MyAssembly", "MyModule", "MyType", "MyName"]}""",
      { ResolvedIdentifier.Assembly = "MyAssembly"
        Module = "MyModule"
        Type = Some "MyType"
        Name = "MyName" }
      """{"discriminator": "id", "value": ["MyAssembly", "MyModule", null, "MyName"]}""",
      { ResolvedIdentifier.Assembly = "MyAssembly"
        Module = "MyModule"
        Type = None
        Name = "MyName" } ]

  for (json, expected) in testCases do
    (expected, JsonValue.Parse json)
    ||> ``Assert ResolvedIdentifier -> ToJson -> FromJson -> ResolvedIdentifier``

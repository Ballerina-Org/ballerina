module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Type.FromJson

open System
open Ballerina.Collections.Sum
open NUnit.Framework
open FSharp.Data
open Ballerina.Errors
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.DSL.Next.Types.Json
open Ballerina.DSL.Next.KitchenSink
open Ballerina.DSL.Next.EquivalenceClasses
open Ballerina.DSL.Next.Unification
open Ballerina.State.WithError

[<Test>]
let ``LangNext-FromJson Primitives parse`` () =
  let testCases =
    [ ("""{"kind": "unit"}""", PrimitiveType.Unit)
      ("""{"kind": "guid"}""", PrimitiveType.Guid)
      ("""{"kind": "int"}""", PrimitiveType.Int)
      ("""{"kind": "decimal"}""", PrimitiveType.Decimal)
      ("""{"kind": "string"}""", PrimitiveType.String)
      ("""{"kind": "bool"}""", PrimitiveType.Bool)
      ("""{"kind": "datetime"}""", PrimitiveType.DateTime)
      ("""{"kind": "dateonly"}""", PrimitiveType.DateOnly) ]

  for (json, expected) in testCases do
    match PrimitiveType.FromJson(JsonValue.Parse json) with
    | Sum.Left actual -> Assert.That(actual, Is.EqualTo expected)
    | Sum.Right errors -> Assert.Fail($"Expected {expected}, but got errors: {errors}")

[<Test>]
let ``LangNext-FromJson Kinds parse`` () =
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
    match Kind.FromJson(JsonValue.Parse json) with
    | Sum.Left actual -> Assert.That(actual, Is.EqualTo expected)
    | Sum.Right errors -> Assert.Fail($"Expected {expected}, got errors: {errors}")

[<Test>]
let ``LangNext-FromJson TypeSymbols parse`` () =
  let guid = Guid.NewGuid()
  let json = $"""{{"name": "MyType", "guid": "{guid}"}}"""

  let expected =
    { TypeSymbol.Name = "MyType"
      TypeSymbol.Guid = guid }

  match TypeSymbol.FromJson(JsonValue.Parse json) with
  | Sum.Left actual -> Assert.That(actual, Is.EqualTo(expected))
  | Sum.Right errors -> Assert.Fail($"Expected valid TypeSymbol, got errors: {errors}")

[<Test>]
let ``LangNext-FromJson TypeParameters parse`` () =
  let json =
    """{"name": "T", "kind": { "kind":"arrow", "arrow": { "param": { "kind": "symbol" }, "returnType": { "kind": "star" } } } }"""

  let expected = TypeParameter.Create("T", Kind.Arrow(Kind.Symbol, Kind.Star))

  match TypeParameter.FromJson(JsonValue.Parse json) with
  | Sum.Left actual -> Assert.That(actual, Is.EqualTo(expected))
  | Sum.Right errors -> Assert.Fail($"Expected valid TypeParameter, got errors: {errors}")

  Assert.Pass()

[<Test>]
let ``LangNext-FromJson TypeVars parse`` () =
  let guid = Guid.NewGuid()
  let json = $"""{{"name": "MyTypeVar", "guid": "{guid}"}}"""

  let result = TypeVar.FromJson(JsonValue.Parse json)

  let expected =
    { TypeVar.Name = "MyTypeVar"
      TypeVar.Guid = guid }

  // Assert
  match result with
  | Sum.Left actual -> Assert.That(actual, Is.EqualTo(expected))
  | Sum.Right errors -> Assert.Fail($"Expected valid TypeVar, got errors: {errors}")


type TypeValueTestCase =
  { Name: string
    Json: string
    Expected: TypeValue }

[<Test>]
let ``LangNext-FromJson TypeValues parse`` () =

  let guid = Guid.NewGuid()

  let testCases: TypeValueTestCase list =
    [ { Name = "Var"
        Json = $"""{{"kind":"var","var":{{"name":"MyTypeVar","guid":"{guid}"}}}}"""
        Expected = TypeValue.Var { Name = "MyTypeVar"; Guid = guid } }
      { Name = "Lookup"
        Json = """{"kind":"lookup","lookup":"SomeType"}"""
        Expected = TypeValue.Lookup { Name = "SomeType" } }
      { Name = "Lambda"
        Json =
          """{
              "kind":"lambda",
              "lambda":{
            "param":{"name":"T","kind":{"kind":"star"}},
            "body":{"kind":"int"}
              }
          }"""
        Expected = TypeValue.Lambda({ Name = "T"; Kind = Kind.Star }, TypeExpr.Primitive PrimitiveType.Int) }
      { Name = "Arrow"
        Json =
          """{
              "kind":"arrow",
              "arrow":{
            "param":{"kind":"int"},
            "returnType":{"kind":"string"}
              }
          }"""
        Expected = TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int, TypeValue.Primitive PrimitiveType.String) }
      { Name = "Record"
        Json =
          """{
              "kind":"record",
              "record":[
                [{"name":"foo","guid":"00000000-0000-0000-0000-000000000001"}, {"kind":"int"}],
                [{"name":"bar","guid":"00000000-0000-0000-0000-000000000002"}, {"kind":"string"}]
              ]
          }"""
        Expected =
          TypeValue.Record(
            Map.ofList
              [ { TypeSymbol.Name = "foo"
                  TypeSymbol.Guid = System.Guid("00000000-0000-0000-0000-000000000001") },
                TypeValue.Primitive PrimitiveType.Int
                { TypeSymbol.Name = "bar"
                  TypeSymbol.Guid = System.Guid("00000000-0000-0000-0000-000000000002") },
                TypeValue.Primitive PrimitiveType.String ]
          ) }
      { Name = "Union"
        Json =
          """{
          "kind":"union",
          "union":[
            [{"name":"foo","guid":"00000000-0000-0000-0000-000000000001"}, {"kind":"int"}],
            [{"name":"bar","guid":"00000000-0000-0000-0000-000000000002"}, {"kind":"string"}],
            [{"name":"baz","guid":"00000000-0000-0000-0000-000000000003"}, {"kind":"bool"}]
          ]
          }"""
        Expected =
          [ { TypeSymbol.Name = "foo"
              TypeSymbol.Guid = System.Guid("00000000-0000-0000-0000-000000000001") },
            TypeValue.Primitive PrimitiveType.Int
            { TypeSymbol.Name = "bar"
              TypeSymbol.Guid = System.Guid("00000000-0000-0000-0000-000000000002") },
            TypeValue.Primitive PrimitiveType.String
            { TypeSymbol.Name = "baz"
              TypeSymbol.Guid = System.Guid("00000000-0000-0000-0000-000000000003") },
            TypeValue.Primitive PrimitiveType.Bool ]
          |> Map.ofList
          |> TypeValue.Union }
      { Name = "Tuple"
        Json =
          """{
          "kind":"tuple",
          "tuple":[
                {"kind":"int"},
                {"kind":"string"}
            ]
          }"""
        Expected =
          TypeValue.Tuple
            [ TypeValue.Primitive PrimitiveType.Int
              TypeValue.Primitive PrimitiveType.String ] }
      { Name = "Sum"
        Json =
          """{
          "kind":"sum",
          "sum":[
            {"kind":"int"},
            {"kind":"string"},
            {"kind":"bool"}
            ]
          }"""
        Expected =
          TypeValue.Sum
            [ TypeValue.Primitive PrimitiveType.Int
              TypeValue.Primitive PrimitiveType.String
              TypeValue.Primitive PrimitiveType.Bool ] }
      { Name = "List"
        Json = """{"kind":"list","list":{"kind":"int"}}"""
        Expected = TypeValue.List(TypeValue.Primitive PrimitiveType.Int) }
      { Name = "Set"
        Json = """{"kind":"set","set":{"kind":"string"}}"""
        Expected = TypeValue.Set(TypeValue.Primitive PrimitiveType.String) }
      { Name = "Map"
        Json = """{"kind":"map","map":[{"kind":"bool"}, {"kind":"int"}]}"""
        Expected = TypeValue.Map(TypeValue.Primitive PrimitiveType.Bool, TypeValue.Primitive PrimitiveType.Int) } ]

  for testCase in testCases do
    match TypeValue.FromJson(JsonValue.Parse testCase.Json) with
    | Sum.Left actual -> Assert.That(actual, Is.EqualTo(testCase.Expected), $"Failed on {testCase.Name}")
    | Sum.Right errors -> Assert.Fail($"Expected {testCase.Expected}, got errors: {errors}")




[<Test>]
let ``LangNext-FromJson TypeExpressions parse`` () =
  let testCases =
    [ """{ "kind":"apply", "apply": [{"kind":"lookup", "lookup":"MyFunction"}, {"kind":"int"} ] }""",
      TypeExpr.Apply(TypeExpr.Lookup "MyFunction", TypeExpr.Primitive PrimitiveType.Int)
      """{ "kind":"lambda", "lambda": [{"name":"T", "kind":{"kind":"star"}}, {"kind":"int"}] }""",
      TypeExpr.Lambda({ Name = "T"; Kind = Kind.Star }, TypeExpr.Primitive PrimitiveType.Int)
      """{ "kind":"arrow", "arrow": [{"kind":"int"}, {"kind":"string"} ] }""",
      TypeExpr.Arrow(TypeExpr.Primitive PrimitiveType.Int, TypeExpr.Primitive PrimitiveType.String)

      """{ "kind":"record", "record": [[{"kind":"lookup", "lookup":"foo"}, {"kind":"int"}], [{"kind":"lookup", "lookup":"bar"}, {"kind":"string"}]] }""",
      TypeExpr.Record
        [ (TypeExpr.Lookup "foo", TypeExpr.Primitive PrimitiveType.Int)
          (TypeExpr.Lookup "bar", TypeExpr.Primitive PrimitiveType.String) ]

      """{"kind":"int"}""", TypeExpr.Primitive PrimitiveType.Int
      """{"kind":"string"}""", TypeExpr.Primitive PrimitiveType.String
      """{"kind":"bool"}""", TypeExpr.Primitive PrimitiveType.Bool
      """{"kind":"unit"}""", TypeExpr.Primitive PrimitiveType.Unit
      """{"kind":"guid"}""", TypeExpr.Primitive PrimitiveType.Guid
      """{"kind":"decimal"}""", TypeExpr.Primitive PrimitiveType.Decimal
      """{"kind":"datetime"}""", TypeExpr.Primitive PrimitiveType.DateTime
      """{"kind":"dateonly"}""", TypeExpr.Primitive PrimitiveType.DateOnly

      """{ "kind": "lookup", "lookup": "MyType" }""", TypeExpr.Lookup "MyType"
      """{ "kind": "list", "list": {"kind": "int"} }""", TypeExpr.List(TypeExpr.Primitive PrimitiveType.Int)
      """{ "kind": "set", "set": {"kind": "string"} }""", TypeExpr.Set(TypeExpr.Primitive PrimitiveType.String)
      """{ "kind": "map", "map": [{"kind": "bool"}, {"kind": "int"}] }""",
      TypeExpr.Map(TypeExpr.Primitive PrimitiveType.Bool, TypeExpr.Primitive PrimitiveType.Int)
      """{ "kind": "keyOf", "keyOf": {"kind": "record", "record": [[{"kind": "lookup", "lookup": "foo"}, {"kind": "int"}]]} }""",
      TypeExpr.KeyOf(TypeExpr.Record [ (TypeExpr.Lookup "foo", TypeExpr.Primitive PrimitiveType.Int) ])
      """{ "kind": "tuple", "tuple": [{"kind": "int"}, {"kind": "string"}] }""",
      TypeExpr.Tuple
        [ TypeExpr.Primitive PrimitiveType.Int
          TypeExpr.Primitive PrimitiveType.String ]
      """{ "kind": "union", "union": [[{"kind":"lookup", "lookup": "foo"}, {"kind": "int"}], [{"kind": "lookup", "lookup": "bar"}, {"kind": "string"}]] }""",
      TypeExpr.Union
        [ (TypeExpr.Lookup "foo", TypeExpr.Primitive PrimitiveType.Int)
          (TypeExpr.Lookup "bar", TypeExpr.Primitive PrimitiveType.String) ]
      """{ "kind": "sum", "sum": [{"kind": "int"}, {"kind": "string"}, {"kind": "bool"}] }""",
      TypeExpr.Sum
        [ TypeExpr.Primitive PrimitiveType.Int
          TypeExpr.Primitive PrimitiveType.String
          TypeExpr.Primitive PrimitiveType.Bool ]
      """{ "kind": "flatten", "flatten": [{"kind": "int"}, {"kind": "string"}] }""",
      TypeExpr.Flatten(TypeExpr.Primitive PrimitiveType.Int, TypeExpr.Primitive PrimitiveType.String)
      """{ "kind": "exclude", "exclude": [{"kind": "int"}, {"kind": "string"}] }""",
      TypeExpr.Exclude(TypeExpr.Primitive PrimitiveType.Int, TypeExpr.Primitive PrimitiveType.String)
      """{ "kind": "rotate", "rotate": {"kind": "int"} }""", TypeExpr.Rotate(TypeExpr.Primitive PrimitiveType.Int) ]

  for (actual, expected) in testCases do
    match TypeExpr.FromJson(JsonValue.Parse actual) with
    | Sum.Left actual -> Assert.That(actual, Is.EqualTo(expected))
    | Sum.Right errors -> Assert.Fail($"Expected {expected}, got errors: {errors}")

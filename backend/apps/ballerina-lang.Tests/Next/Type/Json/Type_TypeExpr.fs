module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Type.Json.Type_TypeExpr

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


type TypeValueTestCase =
  { Name: string
    Json: string
    Expected: TypeValue }

let ``Assert TypeExpr -> ToJson -> FromJson -> TypeExpr`` (expression: TypeExpr) (expectedJson: JsonValue) =
  let normalize (json: JsonValue) =
    json.ToString JsonSaveOptions.DisableFormatting

  let toJson = TypeExpr.ToJson expression
  Assert.That(normalize toJson, Is.EqualTo(normalize expectedJson))

  let parsed = TypeExpr.FromJson expectedJson

  match parsed with
  | Right err -> Assert.Fail $"Parse failed: {err}"
  | Left result -> Assert.That(result, Is.EqualTo(expression))


[<Test>]
let ``Dsl:Type:TypeExpr json round-trip`` () =
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

  for (actualJson, expected) in testCases do
    (expected, JsonValue.Parse actualJson)
    ||> ``Assert TypeExpr -> ToJson -> FromJson -> TypeExpr``

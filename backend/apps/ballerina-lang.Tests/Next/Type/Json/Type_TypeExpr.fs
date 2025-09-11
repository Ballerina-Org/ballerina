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
    [ """{ "kind":"apply", "apply": [{"kind":"lookup", "lookup":"MyFunction"}, {"kind":"int32"} ] }""",
      TypeExpr.Apply(TypeExpr.Lookup("MyFunction" |> Identifier.LocalScope), TypeExpr.Primitive PrimitiveType.Int32)
      """{ "kind":"lambda", "lambda": [{"name":"T", "kind":{"kind":"star"}}, {"kind":"int32"}] }""",
      TypeExpr.Lambda({ Name = "T"; Kind = Kind.Star }, TypeExpr.Primitive PrimitiveType.Int32)
      """{ "kind":"arrow", "arrow": [{"kind":"int32"}, {"kind":"string"} ] }""",
      TypeExpr.Arrow(TypeExpr.Primitive PrimitiveType.Int32, TypeExpr.Primitive PrimitiveType.String)

      """{ "kind":"record", "record": [[{"kind":"lookup", "lookup":"foo"}, {"kind":"int32"}], [{"kind":"lookup", "lookup":"bar"}, {"kind":"string"}]] }""",
      TypeExpr.Let(
        "foo",
        TypeExpr.NewSymbol "foo",
        TypeExpr.Let(
          "bar",
          TypeExpr.NewSymbol "bar",
          TypeExpr.Record
            [ (TypeExpr.Lookup("foo" |> Identifier.LocalScope), TypeExpr.Primitive PrimitiveType.Int32)
              (TypeExpr.Lookup("bar" |> Identifier.LocalScope), TypeExpr.Primitive PrimitiveType.String) ]
        )
      )

      """{"kind":"int32"}""", TypeExpr.Primitive PrimitiveType.Int32
      """{"kind":"int64"}""", TypeExpr.Primitive PrimitiveType.Int64
      """{"kind":"float32"}""", TypeExpr.Primitive PrimitiveType.Float32
      """{"kind":"float64"}""", TypeExpr.Primitive PrimitiveType.Float64
      """{"kind":"string"}""", TypeExpr.Primitive PrimitiveType.String
      """{"kind":"bool"}""", TypeExpr.Primitive PrimitiveType.Bool
      """{"kind":"unit"}""", TypeExpr.Primitive PrimitiveType.Unit
      """{"kind":"guid"}""", TypeExpr.Primitive PrimitiveType.Guid
      """{"kind":"decimal"}""", TypeExpr.Primitive PrimitiveType.Decimal
      """{"kind":"datetime"}""", TypeExpr.Primitive PrimitiveType.DateTime
      """{"kind":"dateonly"}""", TypeExpr.Primitive PrimitiveType.DateOnly

      """{ "kind": "lookup", "lookup": "MyType" }""", TypeExpr.Lookup("MyType" |> Identifier.LocalScope)
      """{ "kind": "list", "list": {"kind": "int32"} }""", TypeExpr.List(TypeExpr.Primitive PrimitiveType.Int32)
      """{ "kind": "set", "set": {"kind": "string"} }""", TypeExpr.Set(TypeExpr.Primitive PrimitiveType.String)
      """{ "kind": "map", "map": [{"kind": "bool"}, {"kind": "int32"}] }""",
      TypeExpr.Map(TypeExpr.Primitive PrimitiveType.Bool, TypeExpr.Primitive PrimitiveType.Int32)
      """{ "kind": "keyOf", "keyOf": {"kind": "record", "record": [[{"kind": "lookup", "lookup": "foo"}, {"kind": "int32"}]]} }""",
      TypeExpr.KeyOf(
        TypeExpr.Let(
          "foo",
          TypeExpr.NewSymbol "foo",
          TypeExpr.Record [ (TypeExpr.Lookup("foo" |> Identifier.LocalScope), TypeExpr.Primitive PrimitiveType.Int32) ]
        )
      )
      """{ "kind": "tuple", "tuple": [{"kind": "int32"}, {"kind": "string"}] }""",
      TypeExpr.Tuple
        [ TypeExpr.Primitive PrimitiveType.Int32
          TypeExpr.Primitive PrimitiveType.String ]
      """{ "kind": "union", "union": [[{"kind":"lookup", "lookup": "foo"}, {"kind": "int32"}], [{"kind": "lookup", "lookup": "bar"}, {"kind": "string"}]] }""",
      TypeExpr.Let(
        "foo",
        TypeExpr.NewSymbol "foo",
        TypeExpr.Let(
          "bar",
          TypeExpr.NewSymbol "bar",
          TypeExpr.Union
            [ (TypeExpr.Lookup("foo" |> Identifier.LocalScope), TypeExpr.Primitive PrimitiveType.Int32)
              (TypeExpr.Lookup("bar" |> Identifier.LocalScope), TypeExpr.Primitive PrimitiveType.String) ]
        )
      )
      """{ "kind": "sum", "sum": [{"kind": "int32"}, {"kind": "string"}, {"kind": "bool"}] }""",
      TypeExpr.Sum
        [ TypeExpr.Primitive PrimitiveType.Int32
          TypeExpr.Primitive PrimitiveType.String
          TypeExpr.Primitive PrimitiveType.Bool ]
      """{ "kind": "flatten", "flatten": [{"kind": "int32"}, {"kind": "string"}] }""",
      TypeExpr.Flatten(TypeExpr.Primitive PrimitiveType.Int32, TypeExpr.Primitive PrimitiveType.String)
      """{ "kind": "exclude", "exclude": [{"kind": "int32"}, {"kind": "string"}] }""",
      TypeExpr.Exclude(TypeExpr.Primitive PrimitiveType.Int32, TypeExpr.Primitive PrimitiveType.String)
      """{ "kind": "rotate", "rotate": {"kind": "int32"} }""", TypeExpr.Rotate(TypeExpr.Primitive PrimitiveType.Int32) ]

  for (actualJson, expected) in testCases do
    (expected, JsonValue.Parse actualJson)
    ||> ``Assert TypeExpr -> ToJson -> FromJson -> TypeExpr``

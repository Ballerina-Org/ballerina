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
    [ """{ "discriminator":"apply", "value": [{"discriminator":"lookup", "value":"MyFunction"}, {"discriminator":"int32"} ] }""",
      TypeExpr.Apply(TypeExpr.Lookup("MyFunction" |> Identifier.LocalScope), TypeExpr.Primitive PrimitiveType.Int32)
      """{ "discriminator":"lambda", "value": [{"name":"T", "kind":{"discriminator":"star"}}, {"discriminator":"int32"}] }""",
      TypeExpr.Lambda({ Name = "T"; Kind = Kind.Star }, TypeExpr.Primitive PrimitiveType.Int32)
      """{ "discriminator":"arrow", "value": [{"discriminator":"int32"}, {"discriminator":"string"} ] }""",
      TypeExpr.Arrow(TypeExpr.Primitive PrimitiveType.Int32, TypeExpr.Primitive PrimitiveType.String)

      """{ "discriminator":"record", "value": [[{"discriminator":"lookup", "value":"foo"}, {"discriminator":"int32"}], [{"discriminator":"lookup", "value":"bar"}, {"discriminator":"string"}]] }""",
      TypeExpr.LetSymbols(
        [ "foo"; "bar" ],
        SymbolsKind.RecordFields,
        TypeExpr.Record
          [ (TypeExpr.Lookup("foo" |> Identifier.LocalScope), TypeExpr.Primitive PrimitiveType.Int32)
            (TypeExpr.Lookup("bar" |> Identifier.LocalScope), TypeExpr.Primitive PrimitiveType.String) ]
      )

      """{"discriminator":"int32"}""", TypeExpr.Primitive PrimitiveType.Int32
      """{"discriminator":"int64"}""", TypeExpr.Primitive PrimitiveType.Int64
      """{"discriminator":"float32"}""", TypeExpr.Primitive PrimitiveType.Float32
      """{"discriminator":"float64"}""", TypeExpr.Primitive PrimitiveType.Float64
      """{"discriminator":"string"}""", TypeExpr.Primitive PrimitiveType.String
      """{"discriminator":"bool"}""", TypeExpr.Primitive PrimitiveType.Bool
      """{"discriminator":"unit"}""", TypeExpr.Primitive PrimitiveType.Unit
      """{"discriminator":"guid"}""", TypeExpr.Primitive PrimitiveType.Guid
      """{"discriminator":"decimal"}""", TypeExpr.Primitive PrimitiveType.Decimal
      """{"discriminator":"datetime"}""", TypeExpr.Primitive PrimitiveType.DateTime
      """{"discriminator":"dateonly"}""", TypeExpr.Primitive PrimitiveType.DateOnly

      """{ "discriminator": "lookup", "value": "MyType" }""", TypeExpr.Lookup("MyType" |> Identifier.LocalScope)
      """{ "discriminator": "set", "value": {"discriminator": "string"} }""",
      TypeExpr.Set(TypeExpr.Primitive PrimitiveType.String)
      """{ "discriminator": "map", "value": [{"discriminator": "bool"}, {"discriminator": "int32"}] }""",
      TypeExpr.Map(TypeExpr.Primitive PrimitiveType.Bool, TypeExpr.Primitive PrimitiveType.Int32)
      """{ "discriminator": "keyOf", "value": {"discriminator": "record", "value": [[{"discriminator": "lookup", "value": "foo"}, {"discriminator": "int32"}]]} }""",
      TypeExpr.KeyOf(
        TypeExpr.LetSymbols(
          [ "foo" ],
          SymbolsKind.RecordFields,
          TypeExpr.Record [ (TypeExpr.Lookup("foo" |> Identifier.LocalScope), TypeExpr.Primitive PrimitiveType.Int32) ]
        )
      )
      """{ "discriminator": "tuple", "value": [{"discriminator": "int32"}, {"discriminator": "string"}] }""",
      TypeExpr.Tuple
        [ TypeExpr.Primitive PrimitiveType.Int32
          TypeExpr.Primitive PrimitiveType.String ]
      """{ "discriminator": "union", "value": [[{"discriminator":"lookup", "value": "foo"}, {"discriminator": "int32"}], [{"discriminator": "lookup", "value": "bar"}, {"discriminator": "string"}]] }""",
      TypeExpr.LetSymbols(
        [ "foo"; "bar" ],
        SymbolsKind.UnionConstructors,
        TypeExpr.Union
          [ (TypeExpr.Lookup("foo" |> Identifier.LocalScope), TypeExpr.Primitive PrimitiveType.Int32)
            (TypeExpr.Lookup("bar" |> Identifier.LocalScope), TypeExpr.Primitive PrimitiveType.String) ]
      )
      """{ "discriminator": "sum", "value": [{"discriminator": "int32"}, {"discriminator": "string"}, {"discriminator": "bool"}] }""",
      TypeExpr.Sum
        [ TypeExpr.Primitive PrimitiveType.Int32
          TypeExpr.Primitive PrimitiveType.String
          TypeExpr.Primitive PrimitiveType.Bool ]
      """{ "discriminator": "flatten", "value": [{"discriminator": "int32"}, {"discriminator": "string"}] }""",
      TypeExpr.Flatten(TypeExpr.Primitive PrimitiveType.Int32, TypeExpr.Primitive PrimitiveType.String)
      """{ "discriminator": "exclude", "value": [{"discriminator": "int32"}, {"discriminator": "string"}] }""",
      TypeExpr.Exclude(TypeExpr.Primitive PrimitiveType.Int32, TypeExpr.Primitive PrimitiveType.String)
      """{ "discriminator": "rotate", "value": {"discriminator": "int32"} }""",
      TypeExpr.Rotate(TypeExpr.Primitive PrimitiveType.Int32) ]

  for (actualJson, expected) in testCases do
    (expected, JsonValue.Parse actualJson)
    ||> ``Assert TypeExpr -> ToJson -> FromJson -> TypeExpr``

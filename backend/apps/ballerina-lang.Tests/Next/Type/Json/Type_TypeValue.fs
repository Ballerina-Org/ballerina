module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Type.Json.Type_TypeValue

open Ballerina.Collections.Sum
open NUnit.Framework
open FSharp.Data
open Ballerina.DSL.Next.Types.Model
open System
open Ballerina.DSL.Next.Types.Json.TypeValue

type TypeValueTestCase =
  { Name: string
    Json: string
    Expected: TypeValue }

let private (!) = Identifier.LocalScope

let ``Assert TypeValue -> ToJson -> FromJson -> TypeValue`` (expression: TypeValue) (expectedJson: JsonValue) =
  let toStr (j: JsonValue) =
    j.ToString(JsonSaveOptions.DisableFormatting)

  let toJson = TypeValue.ToJson expression
  Assert.That(toStr toJson, Is.EqualTo(toStr expectedJson))

  let parsed = TypeValue.FromJson expectedJson

  match parsed with
  | Right err -> Assert.Fail $"Parse failed: {err}"
  | Left result -> Assert.That(result, Is.EqualTo(expression))

let testCases guid : TypeValueTestCase list =
  [ { Name = "Var"
      Json = $"""{{"kind":"var","var":{{"name":"MyTypeVar","guid":"{guid}"}}}}"""
      Expected = TypeValue.Var { Name = "MyTypeVar"; Guid = guid } }
    { Name = "Lookup"
      Json = """{"kind":"lookup","lookup":"SomeType"}"""
      Expected = TypeValue.Lookup !"SomeType" }
    { Name = "Lambda"
      Json =
        """{
              "kind":"lambda",
              "lambda":{
            "param":{"name":"T","kind":{"kind":"star"}},
            "body":{"kind":"int32"}
              }
          }"""
      Expected = TypeValue.Lambda({ Name = "T"; Kind = Kind.Star }, TypeExpr.Primitive PrimitiveType.Int32) }
    { Name = "Arrow"
      Json =
        """{
              "kind":"arrow",
              "arrow":{
            "param":{"kind":"int32"},
            "returnType":{"kind":"string"}
              }
          }"""
      Expected = TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int32, TypeValue.Primitive PrimitiveType.String) }
    { Name = "Union"
      Json =
        """{
          "kind":"union",
          "union":[
            [{"name":"bar","guid":"00000000-0000-0000-0000-000000000002"}, {"kind":"string"}],
            [{"name":"baz","guid":"00000000-0000-0000-0000-000000000003"}, {"kind":"bool"}],
            [{"name":"foo","guid":"00000000-0000-0000-0000-000000000001"}, {"kind":"int32"}]
          ]
          }"""
      Expected =
        [ { TypeSymbol.Name = "bar" |> Identifier.LocalScope
            TypeSymbol.Guid = System.Guid("00000000-0000-0000-0000-000000000002") },
          TypeValue.Primitive PrimitiveType.String
          { TypeSymbol.Name = "baz" |> Identifier.LocalScope
            TypeSymbol.Guid = System.Guid("00000000-0000-0000-0000-000000000003") },
          TypeValue.Primitive PrimitiveType.Bool
          { TypeSymbol.Name = "foo" |> Identifier.LocalScope
            TypeSymbol.Guid = System.Guid("00000000-0000-0000-0000-000000000001") },
          TypeValue.Primitive PrimitiveType.Int32 ]
        |> Map.ofList
        |> TypeValue.Union }
    { Name = "Tuple"
      Json =
        """{
          "kind":"tuple",
          "tuple":[
                {"kind":"int32"},
                {"kind":"string"}
            ]
          }"""
      Expected =
        TypeValue.Tuple
          [ TypeValue.Primitive PrimitiveType.Int32
            TypeValue.Primitive PrimitiveType.String ] }
    { Name = "Sum"
      Json =
        """{
          "kind":"sum",
          "sum":[
            {"kind":"int32"},
            {"kind":"string"},
            {"kind":"bool"}
            ]
          }"""
      Expected =
        TypeValue.Sum
          [ TypeValue.Primitive PrimitiveType.Int32
            TypeValue.Primitive PrimitiveType.String
            TypeValue.Primitive PrimitiveType.Bool ] }
    { Name = "List"
      Json = """{"kind":"list","list":{"kind":"int32"}}"""
      Expected = TypeValue.List(TypeValue.Primitive PrimitiveType.Int32) }
    { Name = "Set"
      Json = """{"kind":"set","set":{"kind":"string"}}"""
      Expected = TypeValue.Set(TypeValue.Primitive PrimitiveType.String) }
    { Name = "Map"
      Json = """{"kind":"map","map":[{"kind":"bool"}, {"kind":"int32"}]}"""
      Expected = TypeValue.Map(TypeValue.Primitive PrimitiveType.Bool, TypeValue.Primitive PrimitiveType.Int32) }
    { Name = "Record"
      Json =
        """{
              "kind":"record",
              "record":[
                [{"name":"bar","guid":"00000000-0000-0000-0000-000000000002"}, {"kind":"string"}],
                [{"name":"foo","guid":"00000000-0000-0000-0000-000000000001"}, {"kind":"int32"}]
              ]
          }"""
      Expected =
        TypeValue.Record(
          Map.ofList
            [ { TypeSymbol.Name = "bar" |> Identifier.LocalScope
                TypeSymbol.Guid = System.Guid("00000000-0000-0000-0000-000000000002") },
              TypeValue.Primitive PrimitiveType.String
              { TypeSymbol.Name = "foo" |> Identifier.LocalScope
                TypeSymbol.Guid = System.Guid("00000000-0000-0000-0000-000000000001") },
              TypeValue.Primitive PrimitiveType.Int32 ]
        ) } ]

[<Test>]
let ``Dsl:Type:TypeValue json round-trip`` () =

  let testCases = Guid.NewGuid() |> testCases

  for testCase in testCases do
    (testCase.Expected, JsonValue.Parse testCase.Json)
    ||> ``Assert TypeValue -> ToJson -> FromJson -> TypeValue``

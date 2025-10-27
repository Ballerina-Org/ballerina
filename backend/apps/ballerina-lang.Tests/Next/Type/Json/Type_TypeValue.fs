module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Type.Json.Type_TypeValue

open Ballerina.Collections.Sum
open NUnit.Framework
open FSharp.Data
open Ballerina.DSL.Next.Types.Model
open System
open Ballerina.DSL.Next.Types.Json.TypeValue
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.StdLib.OrderPreservingMap
open Ballerina.Cat.Collections.OrderedMap

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
      Json = $"""{{"discriminator":"var","value":{{"name":"MyTypeVar","guid":"{guid}"}}}}"""
      Expected = TypeValue.Var { Name = "MyTypeVar"; Guid = guid } }
    { Name = "Lookup"
      Json = """{"discriminator":"lookup","value":"SomeType"}"""
      Expected = TypeValue.Lookup !"SomeType" }
    { Name = "Lambda"
      Json =
        """{
              "discriminator":"lambda",
              "value":{
            "param":{"name":"T","kind":{"discriminator":"star"}},
            "body":{"discriminator":"int32"}
              }
          }"""
      Expected = TypeValue.CreateLambda({ Name = "T"; Kind = Kind.Star }, TypeExpr.Primitive PrimitiveType.Int32) }
    { Name = "Arrow"
      Json =
        """{
              "discriminator":"arrow",
              "value":{
            "param":{"discriminator":"int32"},
            "returnType":{"discriminator":"string"}
              }
          }"""
      Expected = TypeValue.CreateArrow(TypeValue.CreateInt32(), TypeValue.CreateString()) }
    { Name = "Union"
      Json =
        """{
          "discriminator":"union",
          "value":[
            [{"name":"bar","guid":"00000000-0000-0000-0000-000000000002"}, {"discriminator":"string"}],
            [{"name":"baz","guid":"00000000-0000-0000-0000-000000000003"}, {"discriminator":"bool"}],
            [{"name":"foo","guid":"00000000-0000-0000-0000-000000000001"}, {"discriminator":"int32"}]
          ]
          }"""
      Expected =
        [ { TypeSymbol.Name = "bar" |> Identifier.LocalScope
            TypeSymbol.Guid = System.Guid("00000000-0000-0000-0000-000000000002") },
          TypeValue.CreateString()
          { TypeSymbol.Name = "baz" |> Identifier.LocalScope
            TypeSymbol.Guid = System.Guid("00000000-0000-0000-0000-000000000003") },
          TypeValue.CreateBool()
          { TypeSymbol.Name = "foo" |> Identifier.LocalScope
            TypeSymbol.Guid = System.Guid("00000000-0000-0000-0000-000000000001") },
          TypeValue.CreateInt32() ]
        |> OrderedMap.ofList
        |> TypeValue.CreateUnion }
    { Name = "Tuple"
      Json =
        """{
          "discriminator":"tuple",
          "value":[
                {"discriminator":"int32"},
                {"discriminator":"string"}
            ]
          }"""
      Expected = TypeValue.CreateTuple [ TypeValue.CreateInt32(); TypeValue.CreateString() ] }
    { Name = "Sum"
      Json =
        """{
          "discriminator":"sum",
          "value":[
            {"discriminator":"int32"},
            {"discriminator":"string"},
            {"discriminator":"bool"}
            ]
          }"""
      Expected = TypeValue.CreateSum [ TypeValue.CreateInt32(); TypeValue.CreateString(); TypeValue.CreateBool() ] }
    { Name = "Set"
      Json = """{"discriminator":"set","value":{"discriminator":"string"}}"""
      Expected = TypeValue.CreateSet(TypeValue.CreateString()) }
    { Name = "Map"
      Json = """{"discriminator":"map","value":[{"discriminator":"bool"}, {"discriminator":"int32"}]}"""
      Expected = TypeValue.CreateMap(TypeValue.CreateBool(), TypeValue.CreateInt32()) }
    { Name = "Record"
      Json =
        """{
              "discriminator":"record",
              "value":[
                [{"name":"bar","guid":"00000000-0000-0000-0000-000000000002"}, {"discriminator":"string"}],
                [{"name":"foo","guid":"00000000-0000-0000-0000-000000000001"}, {"discriminator":"int32"}]
              ]
          }"""
      Expected =
        TypeValue.CreateRecord(
          OrderedMap.ofList
            [ { TypeSymbol.Name = "bar" |> Identifier.LocalScope
                TypeSymbol.Guid = System.Guid("00000000-0000-0000-0000-000000000002") },
              TypeValue.CreateString()
              { TypeSymbol.Name = "foo" |> Identifier.LocalScope
                TypeSymbol.Guid = System.Guid("00000000-0000-0000-0000-000000000001") },
              TypeValue.CreateInt32() ]
        ) } ]

[<Test>]
let ``Dsl:Type:TypeValue json round-trip`` () =

  let testCases = Guid.NewGuid() |> testCases

  for testCase in testCases do
    (testCase.Expected, JsonValue.Parse testCase.Json)
    ||> ``Assert TypeValue -> ToJson -> FromJson -> TypeValue``

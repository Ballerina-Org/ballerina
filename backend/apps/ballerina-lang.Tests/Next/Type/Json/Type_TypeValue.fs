module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Type.Json.Type_TypeValue

open Ballerina.Collections.Sum
open NUnit.Framework
open FSharp.Data
open Ballerina.DSL.Next.Types.Model
open System
open Ballerina.DSL.Next.Types.Json.TypeValue
open Ballerina.DSL.Next.Types.Patterns

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
      Expected = TypeValue.CreateLambda({ Name = "T"; Kind = Kind.Star }, TypeExpr.Primitive PrimitiveType.Int32) }
    { Name = "Arrow"
      Json =
        """{
              "kind":"arrow",
              "arrow":{
            "param":{"kind":"int32"},
            "returnType":{"kind":"string"}
              }
          }"""
      Expected = TypeValue.CreateArrow(TypeValue.CreateInt32(), TypeValue.CreateString()) }
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
          TypeValue.CreateString()
          { TypeSymbol.Name = "baz" |> Identifier.LocalScope
            TypeSymbol.Guid = System.Guid("00000000-0000-0000-0000-000000000003") },
          TypeValue.CreateBool()
          { TypeSymbol.Name = "foo" |> Identifier.LocalScope
            TypeSymbol.Guid = System.Guid("00000000-0000-0000-0000-000000000001") },
          TypeValue.CreateInt32() ]
        |> Map.ofList
        |> TypeValue.CreateUnion }
    { Name = "Tuple"
      Json =
        """{
          "kind":"tuple",
          "tuple":[
                {"kind":"int32"},
                {"kind":"string"}
            ]
          }"""
      Expected = TypeValue.CreateTuple [ TypeValue.CreateInt32(); TypeValue.CreateString() ] }
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
      Expected = TypeValue.CreateSum [ TypeValue.CreateInt32(); TypeValue.CreateString(); TypeValue.CreateBool() ] }
    { Name = "Set"
      Json = """{"kind":"set","set":{"kind":"string"}}"""
      Expected = TypeValue.CreateSet(TypeValue.CreateString()) }
    { Name = "Map"
      Json = """{"kind":"map","map":[{"kind":"bool"}, {"kind":"int32"}]}"""
      Expected = TypeValue.CreateMap(TypeValue.CreateBool(), TypeValue.CreateInt32()) }
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
        TypeValue.CreateRecord(
          Map.ofList
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

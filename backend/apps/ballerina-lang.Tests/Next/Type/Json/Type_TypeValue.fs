module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Type.Json.Type_TypeValue

open Ballerina
open Ballerina.Collections.Sum
open NUnit.Framework
open FSharp.Data
open Ballerina.DSL.Next.Types.Model
open System
open Ballerina.DSL.Next.Types.Json.TypeValue
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.Cat.Collections.OrderedMap

type TypeValueTestCase =
  { Name: string
    Json: string
    Expected: TypeValue<Object> }

let private (!) = Identifier.LocalScope

let toScopedJson name (str: string) =
  $"""{{
  "discriminator":"withSourceMapping","value":{{
  "typeCheckScopeSource": {{
    "assembly": "",
    "module": "",
    "type": null
  }},
  "typeExprSource": {{
    "type": "noSourceMapping",
    "value": "{name}"
  }},
  "value":{str}  }} }}"""

let ``Assert TypeValue -> ToJson -> FromJson -> TypeValue`` (expression: TypeValue<Object>) (expectedJson: JsonValue) =
  let toStr (j: JsonValue) =
    j.ToString(JsonSaveOptions.DisableFormatting)

  let toJson = TypeValue<Object>.ToJson expression
  Assert.That(toStr toJson, Is.EqualTo(toStr expectedJson))

  let parsed = TypeValue<Object>.FromJson expectedJson

  match parsed with
  | Right err -> Assert.Fail $"Parse failed: {err}"
  | Left result -> Assert.That(result, Is.EqualTo(expression))

let testCases guid : TypeValueTestCase list =
  [ { Name = "Var"
      Json = $"""{{"discriminator":"var","value":{{"name":"MyTypeVar","guid":"{guid}"}}}}"""
      Expected =
        TypeValue<Object>.Var
          { Name = "MyTypeVar"
            Guid = guid
            Synthetic = false } }
    { Name = "Lookup"
      Json = """{"discriminator":"lookup","value":"SomeType"}"""
      Expected = TypeValue<Object>.Lookup !"SomeType" }
    { Name = "Lambda"
      Json =
        """{
              "discriminator":"lambda",
              "value":{
            "param":{"name":"T","kind":{"discriminator":"star"}},
            "body":{"discriminator":"int32"}
              }
          }"""
        |> toScopedJson "Lambda"
      Expected = TypeValue<Unit>.CreateLambda({ Name = "T"; Kind = Kind.Star }, TypeExpr.Primitive PrimitiveType.Int32) }
    { Name = "Arrow"
      Json =
        """{
              "discriminator":"arrow",
              "value":{
            "param":{"discriminator":"int32"},
            "returnType":{"discriminator":"string"}
              }
          }"""
        |> toScopedJson "Arrow"
      Expected = TypeValue<Unit>.CreateArrow(TypeValue<Unit>.CreateInt32(), TypeValue<Unit>.CreateString()) }
    { Name = "Union"
      Json =
        """{
          "discriminator":"union",
          "value":[
            [{"name":"bar","guid":"00000000-0000-0000-0000-000000000002"}, {"discriminator":"string"}],
            [{"name":"baz","guid":"00000000-0000-0000-0000-000000000003"}, {"discriminator":"bool"}],
            [{"name":"foo","guid":"00000000-0000-0000-0000-000000000001"}, {"discriminator":"int32"}]
          ]}"""
        |> toScopedJson "Union"
      Expected =
        [ { TypeSymbol.Name = "bar" |> Identifier.LocalScope
            TypeSymbol.Guid = System.Guid("00000000-0000-0000-0000-000000000002") },
          TypeValue<Unit>.CreateString()
          { TypeSymbol.Name = "baz" |> Identifier.LocalScope
            TypeSymbol.Guid = System.Guid("00000000-0000-0000-0000-000000000003") },
          TypeValue<Unit>.CreateBool()
          { TypeSymbol.Name = "foo" |> Identifier.LocalScope
            TypeSymbol.Guid = System.Guid("00000000-0000-0000-0000-000000000001") },
          TypeValue<Unit>.CreateInt32() ]
        |> OrderedMap.ofList
        |> TypeValue<Unit>.CreateUnion }
    { Name = "Tuple"
      Json =
        """{
          "discriminator":"tuple",
          "value":[
                {"discriminator":"int32"},
                {"discriminator":"string"}
            ]
          }"""
        |> toScopedJson "Tuple"
      Expected = TypeValue<Unit>.CreateTuple [ TypeValue<Unit>.CreateInt32(); TypeValue<Unit>.CreateString() ] }
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
        |> toScopedJson "Sum"
      Expected =
        TypeValue<Unit>.CreateSum
          [ TypeValue<Unit>.CreateInt32()
            TypeValue<Unit>.CreateString()
            TypeValue<Unit>.CreateBool() ] }
    { Name = "Set"
      Json =
        """{"discriminator":"set","value":{"discriminator":"string"}}"""
        |> toScopedJson "Set"
      Expected = TypeValue<Unit>.CreateSet(TypeValue<Unit>.CreateString()) }
    { Name = "Map"
      Json =
        """{"discriminator":"map","value":[{"discriminator":"bool"}, {"discriminator":"int32"}]}"""
        |> toScopedJson "Map"
      Expected = TypeValue<Unit>.CreateMap(TypeValue<Unit>.CreateBool(), TypeValue<Unit>.CreateInt32()) }
    { Name = "Record"
      Json =
        """{
              "discriminator":"record",
              "value":[
                [{"name":"bar","guid":"00000000-0000-0000-0000-000000000002"}, [{"discriminator":"string"}, {"discriminator":"star"}]],
                [{"name":"foo","guid":"00000000-0000-0000-0000-000000000001"}, [{"discriminator":"int32"}, {"discriminator":"star"}]]
              ]
          }"""
        |> toScopedJson "Record"
      Expected =
        TypeValue<Unit>
          .CreateRecord(
            OrderedMap.ofList
              [ { TypeSymbol.Name = "bar" |> Identifier.LocalScope
                  TypeSymbol.Guid = System.Guid("00000000-0000-0000-0000-000000000002") },
                (TypeValue<Unit>.CreateString(), Kind.Star)
                { TypeSymbol.Name = "foo" |> Identifier.LocalScope
                  TypeSymbol.Guid = System.Guid("00000000-0000-0000-0000-000000000001") },
                (TypeValue<Unit>.CreateInt32(), Kind.Star) ]
          ) }
    { Name = "Application (Lookup)"
      Json =
        """{
          "discriminator":"application",
          "value":[
            {"discriminator":"id","value":"List"},
            {"discriminator":"int32"}
          ]
        }"""
        |> toScopedJson "Application"
      Expected =
        TypeValue<Unit>.CreateApplication(SymbolicTypeApplication.Lookup(!"List", TypeValue<Unit>.CreateInt32())) }
    { Name = "Application (Nested Application)"
      Json =
        """{
          "discriminator":"application",
          "value":[
            [
              {"discriminator":"id","value":"Map"},
              {"discriminator":"string"}
            ],
            {"discriminator":"int32"}
          ]
        }"""
        |> toScopedJson "Application"
      Expected =
        TypeValue<Unit>
          .CreateApplication(
            SymbolicTypeApplication.Application(
              SymbolicTypeApplication.Lookup(!"Map", TypeValue<Unit>.CreateString()),
              TypeValue<Unit>.CreateInt32()
            )
          ) } ]

[<Test>]
let ``Dsl:Type:TypeValue<Unit> json round-trip`` () =

  let testCases = Guid.NewGuid() |> testCases

  for testCase in testCases do
    (testCase.Expected, JsonValue.Parse testCase.Json)
    ||> ``Assert TypeValue -> ToJson -> FromJson -> TypeValue``

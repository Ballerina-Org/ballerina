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

let rec normalizeJson (json: JsonValue) : JsonValue =
  match json with
  | JsonValue.Record fields ->
    fields
    |> Array.map (fun (k, v) -> k, normalizeJson v)
    |> Array.sortBy fst
    |> JsonValue.Record

  | JsonValue.Array items ->
    let normalized = items |> Array.map normalizeJson

    let isUnionMatchCase =
      normalized
      |> Array.forall (function
        | JsonValue.Array inner when inner.Length > 0 ->
          match inner.[0] with
          | JsonValue.String _ -> true
          | _ -> false
        | _ -> false)

    let isRecordFieldArray =
      normalized
      |> Array.forall (function
        | JsonValue.Array [| JsonValue.Record symbol; _ |] -> symbol |> Array.exists (fun (k, _) -> k = "name")
        | _ -> false)

    if isUnionMatchCase then
      normalized
      |> Array.sortBy (function
        | JsonValue.Array inner ->
          match inner.[0] with
          | JsonValue.String tag -> tag
          | _ -> ""
        | _ -> "")
      |> JsonValue.Array

    elif isRecordFieldArray then
      normalized
      |> Array.sortBy (function
        | JsonValue.Array [| JsonValue.Record symbol; _ |] ->
          symbol
          |> Array.tryFind (fun (k, _) -> k = "name")
          |> Option.map (fun (_, v) -> v.ToString())
          |> Option.defaultValue ""
        | _ -> "")
      |> JsonValue.Array

    else
      JsonValue.Array normalized

  | _ -> json

let ``Assert TypeValue -> ToJson -> FromJson -> TypeValue`` (expression: TypeValue) (expectedJson: JsonValue) =
  let toStr j =
    normalizeJson j |> _.ToString(JsonSaveOptions.DisableFormatting)

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
      Expected = TypeValue.Map(TypeValue.Primitive PrimitiveType.Bool, TypeValue.Primitive PrimitiveType.Int) }
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
        ) } ]

[<Test>]
let ``Dsl:Type:TypeValue json round-trip`` () =

  let testCases = Guid.NewGuid() |> testCases

  for testCase in testCases do
    (testCase.Expected, JsonValue.Parse testCase.Json)
    ||> ``Assert TypeValue -> ToJson -> FromJson -> TypeValue``

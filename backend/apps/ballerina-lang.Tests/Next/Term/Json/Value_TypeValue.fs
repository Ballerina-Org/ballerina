module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Term.Json.Value_TypeValue

open NUnit.Framework
open FSharp.Data

open Ballerina.Collections.Sum
open Ballerina.Reader.WithError

open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Terms.Json.Value
open Ballerina.DSL.Next.Types.Json.TypeValue
open Ballerina.DSL.Next.Json
open ExtensionsBootstrapForTesting
open Ballerina.DSL.Next.StdLib.List
open Ballerina.Collections.NonEmptyList
open Ballerina.DSL.Next.Terms.Json

let ``Assert Value<TypeValue> -> ToJson -> FromJson -> Value<TypeValue>``
  (expression: Value<TypeValue, TestExt.ValueExt>)
  (expectedJson: JsonValue)
  =
  let toStr (j: JsonValue) =
    j.ToString(JsonSaveOptions.DisableFormatting)

  let rootExprEncoder = Expr.ToJson >> Reader.Run TypeValue.ToJson

  let rootToJson =
    Json.buildRootEncoder<TypeValue, TestExt.ValueExt> (
      NonEmptyList.OfList(Value.ToJson, [ TestExt.testExtensions.List.Encoder ])
    )

  let encoder = rootToJson >> Reader.Run(rootExprEncoder, TypeValue.ToJson)

  match encoder expression with
  | Right err -> Assert.Fail $"Encode failed: {err}"
  | Left json ->
    Assert.That(toStr json, Is.EqualTo(toStr expectedJson))

    let rootExprFromJson = Expr.FromJson >> Reader.Run TypeValue.FromJson

    let rootFromJson =
      Json.buildRootParser<TypeValue, TestExt.ValueExt> (
        NonEmptyList.OfList(Value.FromJson, [ TestExt.testExtensions.List.Parser ])
      )

    let parser = rootFromJson >> Reader.Run(rootExprFromJson, TypeValue.FromJson)

    let parsed = parser expectedJson

    match parsed with
    | Right err -> Assert.Fail $"Parse failed: {err}"
    | Left result -> Assert.That(result, Is.EqualTo(expression))

[<Test>]
let ``Dsl:Terms:Value:TypeValue.Rest json round-trip`` () =
  let foo =
    { TypeSymbol.Name = "foo" |> Identifier.LocalScope
      TypeSymbol.Guid = System.Guid("00000000-0000-0000-0000-000000000001") }

  let bar =
    { TypeSymbol.Name = "bar" |> Identifier.LocalScope
      TypeSymbol.Guid = System.Guid("00000000-0000-0000-0000-000000000002") }

  let testCases: List<string * Value<TypeValue, TestExt.ValueExt>> =
    [ """{"kind": "var", "name":"myVar"}""", Var.Create "myVar" |> Value.Var
      """{"kind": "int32", "int32":"123"}""", PrimitiveValue.Int32 123 |> Value.Primitive
      """{"kind": "decimal", "decimal":"123.456"}""", PrimitiveValue.Decimal 123.456M |> Value.Primitive
      """{"kind": "boolean", "boolean":"true"}""", PrimitiveValue.Bool true |> Value.Primitive
      """{"kind": "record", "fields":[[{"name":"bar","guid":"00000000-0000-0000-0000-000000000002"}, {"kind":"string","string":"baz"}],
      [{"name":"foo","guid":"00000000-0000-0000-0000-000000000001"}, {"kind":"int32","int32":"42"}] 
        ]}""",
      Value<TypeValue, TestExt.ValueExt>
        .Record(
          Map.ofList
            [ foo, PrimitiveValue.Int32 42 |> Value.Primitive
              bar, PrimitiveValue.String "baz" |> Value.Primitive ]
        )
      """{"kind": "union-case", "union-case": [{"name":"foo","guid":"00000000-0000-0000-0000-000000000001"}, {"kind":"int32","int32":"42"}]}""",
      Value.UnionCase(foo, PrimitiveValue.Int32 42 |> Value.Primitive)
      """{"kind": "tuple", "elements":[{"kind":"int32","int32":"1"},{"kind":"string","string":"two"}]}""",
      Value.Tuple(
        [ PrimitiveValue.Int32 1 |> Value.Primitive
          PrimitiveValue.String "two" |> Value.Primitive ]
      )
      """{"kind": "sum", "case": [3, {"kind":"int32","int32":"42"}]}""",
      Value.Sum(3, PrimitiveValue.Int32 42 |> Value.Primitive)
      """{"kind": "type-lambda", "type-lambda":[{"name":"T", "kind":{"kind":"star"}}, {"kind":"int32","int32":"42"}]}""",
      Value.TypeLambda({ Name = "T"; Kind = Kind.Star }, PrimitiveValue.Int32 42 |> Expr.Primitive)
      """{"kind": "lambda", "lambda": ["x", {"kind":"int32","int32":"42"}]}""",
      Value.Lambda(Var.Create "x", PrimitiveValue.Int32 42 |> Expr.Primitive)
      """{"kind": "list", "elements":[{"kind":"int32","int32":"1"},{"kind":"int32","int32":"2"}]}""",
      Value.Ext(
        TestExt.ValueExt(
          Choice1Of3(
            TestExt.ListValues(
              List
                [ PrimitiveValue.Int32 1 |> Value.Primitive
                  PrimitiveValue.Int32 2 |> Value.Primitive ]
            )
          )
        )
      ) ]

  for json, expected in testCases do
    (expected, JsonValue.Parse json)
    ||> ``Assert Value<TypeValue> -> ToJson -> FromJson -> Value<TypeValue>``

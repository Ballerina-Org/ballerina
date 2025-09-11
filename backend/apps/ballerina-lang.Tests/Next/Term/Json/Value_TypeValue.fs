module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Term.Json.Value_TypeValue

open NUnit.Framework
open FSharp.Data

open Ballerina.Collections.Sum
open Ballerina.Reader.WithError

open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Terms.Json.Value
open Ballerina.DSL.Next.Types.Json.TypeValue

let ``Assert Value<TypeValue> -> ToJson -> FromJson -> Value<TypeValue>``
  (expression: Value<TypeValue, Unit>)
  (expectedJson: JsonValue)
  =
  let toStr (j: JsonValue) =
    j.ToString(JsonSaveOptions.DisableFormatting)

  let toJson =
    Value<TypeValue, Unit>.ToJson expression
    |> Reader.Run(TypeValue.ToJson, fun _ -> JsonValue.Null)

  match toJson with
  | Right err -> Assert.Fail $"Encode failed: {err}"
  | Left json ->
    Assert.That(toStr json, Is.EqualTo(toStr expectedJson))

    let parser = Value.FromJson(fun _ -> sum.Return()) >> Reader.Run TypeValue.FromJson

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

  let testCases: List<string * Value<TypeValue, Unit>> =
    [ """{"kind": "var", "name":"myVar"}""", Var.Create "myVar" |> Value.Var
      """{"kind": "int", "int":"123"}""", PrimitiveValue.Int 123 |> Value.Primitive
      """{"kind": "decimal", "decimal":"123.456"}""", PrimitiveValue.Decimal 123.456M |> Value.Primitive
      """{"kind": "boolean", "boolean":"true"}""", PrimitiveValue.Bool true |> Value.Primitive
      """{"kind": "record", "fields":[[{"name":"bar","guid":"00000000-0000-0000-0000-000000000002"}, {"kind":"string","string":"baz"}],
      [{"name":"foo","guid":"00000000-0000-0000-0000-000000000001"}, {"kind":"int","int":"42"}] 
        ]}""",
      Value<TypeValue, Unit>
        .Record(
          Map.ofList
            [ foo, PrimitiveValue.Int 42 |> Value.Primitive
              bar, PrimitiveValue.String "baz" |> Value.Primitive ]
        )
      """{"kind": "union-case", "union-case": [{"name":"foo","guid":"00000000-0000-0000-0000-000000000001"}, {"kind":"int","int":"42"}]}""",
      Value.UnionCase(foo, PrimitiveValue.Int 42 |> Value.Primitive)
      """{"kind": "tuple", "elements":[{"kind":"int","int":"1"},{"kind":"string","string":"two"}]}""",
      Value.Tuple(
        [ PrimitiveValue.Int 1 |> Value.Primitive
          PrimitiveValue.String "two" |> Value.Primitive ]
      )
      """{"kind": "sum", "case": [3, {"kind":"int","int":"42"}]}""",
      Value.Sum(3, PrimitiveValue.Int 42 |> Value.Primitive)
      """{"kind": "type-lambda", "type-lambda":[{"name":"T", "kind":{"kind":"star"}}, {"kind":"int","int":"42"}]}""",
      Value.TypeLambda({ Name = "T"; Kind = Kind.Star }, PrimitiveValue.Int 42 |> Expr.Primitive)
      """{"kind": "lambda", "lambda": ["x", {"kind":"int","int":"42"}]}""",
      Value.Lambda(Var.Create "x", PrimitiveValue.Int 42 |> Expr.Primitive) ]

  for json, expected in testCases do
    (expected, JsonValue.Parse json)
    ||> ``Assert Value<TypeValue> -> ToJson -> FromJson -> Value<TypeValue>``

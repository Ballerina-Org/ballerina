module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Term.Json.Value_TypeValue

open NUnit.Framework
open FSharp.Data

open Ballerina
open Ballerina.Collections.Sum
open Ballerina.Reader.WithError

open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Terms
open Ballerina.DSL.Next.Terms.Patterns
open Ballerina.DSL.Next.Terms.Json.Value
open Ballerina.DSL.Next.Types.Json.TypeValue
open Ballerina.DSL.Next.Types.Json.ResolvedTypeIdentifier
open Ballerina.DSL.Next.Json
open Ballerina.DSL.Next.StdLib
open Ballerina.Collections.NonEmptyList
open Ballerina.DSL.Next.Terms.Json
open Ballerina.DSL.Next.StdLib.Extensions

let stdExtensions, languageContext = stdExtensions

let ``Assert Value<TypeValue> -> ToJson -> FromJson -> Value<TypeValue>``
  (expression: Value<TypeValue<ValueExt>, ValueExt>)
  (expectedJson: JsonValue)
  =
  let toStr (j: JsonValue) =
    j.ToString(JsonSaveOptions.DisableFormatting)

  let rootExprEncoder =
    Expr.ToJson >> Reader.Run(TypeValue.ToJson, ResolvedIdentifier.ToJson)

  let rootToJson =
    Json.buildRootEncoder<TypeValue<ValueExt>, ValueExt> (
      NonEmptyList.OfList(
        Value.ToJson,
        [ List.Json.Extension.encoder ListExt.ValueLens
          Option.Json.Extension.encoder OptionExt.ValueLens ]
      )
    )

  let encoder = rootToJson >> Reader.Run(rootExprEncoder, TypeValue.ToJson)

  match encoder expression with
  | Right err -> Assert.Fail $"Encode failed: {err}"
  | Left json ->
    Assert.That(toStr json, Is.EqualTo(toStr expectedJson))

    let rootExprFromJson =
      Expr.FromJson >> Reader.Run(TypeValue.FromJson, ResolvedIdentifier.FromJson)

    let rootFromJson =
      Json.buildRootParser<TypeValue<ValueExt>, ResolvedIdentifier, ValueExt> (
        NonEmptyList.OfList(
          Value.FromJson,
          [ List.Json.Extension.parser ListExt.ValueLens
            Option.Json.Extension.parser OptionExt.ValueLens ]
        )
      )

    let parser =
      rootFromJson
      >> Reader.Run(rootExprFromJson, TypeValue.FromJson, ResolvedIdentifier.FromJson)

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

  let testCases: List<string * Value<TypeValue<ValueExt>, ValueExt>> =
    [ """{"discriminator": "var", "value":"myVar"}""", Var.Create "myVar" |> Value.Var
      """{"discriminator": "int32", "value":"123"}""", PrimitiveValue.Int32 123 |> Value.Primitive
      """{"discriminator": "decimal", "value":"123.456"}""", PrimitiveValue.Decimal 123.456M |> Value.Primitive
      """{"discriminator": "boolean", "value":"true"}""", PrimitiveValue.Bool true |> Value.Primitive
      """{"discriminator": "record", "value":[[{"discriminator":"id","value":["","",null,"bar"]}, {"discriminator":"string","value":"baz"}],
      [{"discriminator":"id","value":["","",null,"foo"]}, {"discriminator":"int32","value":"42"}]
        ]}""",
      Value<TypeValue<ValueExt>, ValueExt>
        .Record(
          Map.ofList
            [ foo.Name |> TypeCheckScope.Empty.Resolve, PrimitiveValue.Int32 42 |> Value.Primitive
              bar.Name |> TypeCheckScope.Empty.Resolve, PrimitiveValue.String "baz" |> Value.Primitive ]
        )
      """{"discriminator": "union-case", "value": [{"discriminator":"id", "value":["","",null,"foo"]}, {"discriminator":"int32","value":"42"}]}""",
      Value.UnionCase(foo.Name |> TypeCheckScope.Empty.Resolve, PrimitiveValue.Int32 42 |> Value.Primitive)
      """{"discriminator": "tuple", "value":[{"discriminator":"int32","value":"1"},{"discriminator":"string","value":"two"}]}""",
      Value.Tuple(
        [ PrimitiveValue.Int32 1 |> Value.Primitive
          PrimitiveValue.String "two" |> Value.Primitive ]
      )
      """{"discriminator": "sum", "value": [3, 3, {"discriminator":"int32","value":"42"}]}""",
      Value.Sum({ Case = 3; Count = 3 }, PrimitiveValue.Int32 42 |> Value.Primitive)
      """{"discriminator": "type-lambda", "value":[{"name":"T", "kind":{"discriminator":"star"}}, {"discriminator":"int32","value":"42"}]}""",
      Value.TypeLambda({ Name = "T"; Kind = Kind.Star }, PrimitiveValue.Int32 42 |> Expr.Primitive)
      """{"discriminator": "lambda", "value": ["x", {"discriminator":"int32","value":"42"}]}""",
      Value.Lambda(Var.Create "x", PrimitiveValue.Int32 42 |> Expr.Primitive, Map.empty, TypeCheckScope.Empty)
      """{"discriminator": "list", "value":[{"discriminator":"int32","value":"1"},{"discriminator":"int32","value":"2"}]}""",
      Value.Ext(
        ValueExt(
          Choice1Of6(
            ListValues(
              List.Model.ListValues.List
                [ PrimitiveValue.Int32 1 |> Value.Primitive
                  PrimitiveValue.Int32 2 |> Value.Primitive ]
            )
          )
        ),
        None
      )
      """{"discriminator": "option", "value":{"discriminator":"int32","value":"1"}}""",
      Value.Ext(
        ValueExt(
          Choice2Of6(OptionValues(Option.Model.OptionValues.Option(PrimitiveValue.Int32 1 |> Value.Primitive |> Some)))
        ),
        None
      )
      """{"discriminator": "option", "value":null}""",
      Value.Ext(ValueExt(Choice2Of6(OptionValues(Option.Model.OptionValues.Option None))), None) ]

  for json, expected in testCases do
    (expected, JsonValue.Parse json)
    ||> ``Assert Value<TypeValue> -> ToJson -> FromJson -> Value<TypeValue>``

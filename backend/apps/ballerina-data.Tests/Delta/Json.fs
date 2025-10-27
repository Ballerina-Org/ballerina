module Ballerina.Data.Tests.Delta.Json

open NUnit.Framework
open FSharp.Data
open Ballerina.Reader.WithError
open Ballerina.Collections.Sum
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Terms.Json.Value
open Ballerina.Data.Delta.Model
open Ballerina.DSL.Next.Delta.Json.Model
open Ballerina.DSL.Next.Types.Json.TypeValue
open Ballerina.DSL.Next.Json
open Ballerina.Collections.NonEmptyList
open Ballerina.DSL.Next.Terms.Json
open Ballerina.DSL.Next.Types.Json.ResolvedTypeIdentifier

let ``Assert Delta -> ToJson -> FromJson -> Delta`` (expression: Delta<Unit>) (expectedJson: JsonValue) =
  let normalize (json: JsonValue) =
    json.ToString JsonSaveOptions.DisableFormatting

  let rootExprToJson =
    Expr.ToJson >> Reader.Run(TypeValue.ToJson, ResolvedIdentifier.ToJson)

  let rootValueToJson =
    Json.buildRootEncoder<TypeValue, Unit> (NonEmptyList.OfList(Value.ToJson, []))

  let valueEncoder = rootValueToJson >> Reader.Run(rootExprToJson, TypeValue.ToJson)

  let encoded = Delta.ToJson expression |> Reader.Run valueEncoder

  match encoded with
  | Right err -> Assert.Fail $"Encode failed: {err}"
  | Left json ->
    Assert.That(normalize json, Is.EqualTo(normalize expectedJson))

    let rootExprFromJson =
      Expr.FromJson >> Reader.Run(TypeValue.FromJson, ResolvedIdentifier.FromJson)

    let rootValueFromJson =
      Json.buildRootParser<TypeValue, ResolvedIdentifier, Unit> (NonEmptyList.OfList(Value.FromJson, []))

    let valueParser =
      rootValueFromJson
      >> Reader.Run(rootExprFromJson, TypeValue.FromJson, ResolvedIdentifier.FromJson)

    let parsed = Delta.FromJson expectedJson |> Reader.Run valueParser

    match parsed with
    | Right err -> Assert.Fail $"Parse failed: {err}"
    | Left result -> Assert.That(result, Is.EqualTo(expression))


[<Test>]
let ``Delta.Multiple json round-trip`` () =

  let delta = Delta<Unit>.Multiple [ Delta.Multiple [] ]

  let json =
    """ 
      {
      "discriminator":"multiple",
      "value": [
        {
        "discriminator":"multiple",
        "value": [
        ]
        }
      ]
      }
    """
    |> JsonValue.Parse

  (delta, json) ||> ``Assert Delta -> ToJson -> FromJson -> Delta``


[<Test>]
let ``Delta.Replace json round-trip`` () =
  let json =
    """ 
      {
      "discriminator":"replace",
      "value": {"discriminator":"int32","value":"99"}
      }
    """
    |> JsonValue.Parse

  let delta = Delta<Unit>.Replace(Value.Primitive(PrimitiveValue.Int32 99))

  (delta, json) ||> ``Assert Delta -> ToJson -> FromJson -> Delta``


[<Test>]
let ``Delta.Record json round-trip`` () =
  let delta =
    Delta<Unit>.Record("Foo", Delta.Replace(Value.Primitive(PrimitiveValue.Int32 99)))

  let json =
    """ 
      {
      "discriminator":"record",
      "value": ["Foo", 
        {
          "discriminator":"replace",
          "value": {"discriminator":"int32","value":"99"}
        }
      ]
      }
    """
    |> JsonValue.Parse

  (delta, json) ||> ``Assert Delta -> ToJson -> FromJson -> Delta``


[<Test>]
let ``Delta.Union json round-trip`` () =
  let delta =
    Delta.Union("Case1", Delta.Replace(Value.Primitive(PrimitiveValue.Int32 99)))

  let json =
    """ 
      {
      "discriminator":"union",
      "value": ["Case1", 
        {
          "discriminator":"replace",
          "value": {"discriminator":"int32","value":"99"}
        }
      ]
      }
    """
    |> JsonValue.Parse

  (delta, json) ||> ``Assert Delta -> ToJson -> FromJson -> Delta``


[<Test>]
let ``Delta.Tuple json round-trip`` () =
  let delta = Delta.Tuple(3, Delta.Replace(Value.Primitive(PrimitiveValue.Int32 99)))

  let json =
    """ 
      {
      "discriminator":"tuple",
      "value": [3, 
        {
          "discriminator":"replace",
          "value": {"discriminator":"int32","value":"99"}
        }
      ]
      }
    """
    |> JsonValue.Parse

  (delta, json) ||> ``Assert Delta -> ToJson -> FromJson -> Delta``


[<Test>]
let ``Delta.Sum json round-trip`` () =
  let delta = Delta.Sum(3, Delta.Replace(Value.Primitive(PrimitiveValue.Int32 99)))

  let json =
    """ 
      {
      "discriminator":"sum",
      "value": [3, 
        {
          "discriminator":"replace",
          "value": {"discriminator":"int32","value":"99"}
        }
      ]
      }
    """
    |> JsonValue.Parse

  (delta, json) ||> ``Assert Delta -> ToJson -> FromJson -> Delta``

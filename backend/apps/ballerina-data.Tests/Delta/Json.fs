module Ballerina.Data.Tests.Delta.Json

open Ballerina.DSL.Next.StdLib.Extensions
open NUnit.Framework
open FSharp.Data
open Ballerina.Reader.WithError
open Ballerina
open Ballerina.Collections.Sum
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Terms.Json.Value
open Ballerina.Data.Delta.Model
open Ballerina.DSL.Next.Delta.Json.Model
open Ballerina.DSL.Next.Delta.Json.DeltaExt
open Ballerina.DSL.Next.Types.Json.TypeValue
open Ballerina.DSL.Next.Json
open Ballerina.Collections.NonEmptyList
open Ballerina.DSL.Next.Terms.Json
open Ballerina.DSL.Next.Types.Json.ResolvedTypeIdentifier
open Ballerina.DSL.Next.StdLib.List.Model

let ``Assert Delta -> ToJson -> FromJson -> Delta``
  (expression: Delta<ValueExt<unit>, DeltaExt<unit>>)
  (expectedJson: JsonValue)
  =
  let normalize (json: JsonValue) =
    json.ToString JsonSaveOptions.DisableFormatting

  let rootExprToJson =
    Expr.ToJson >> Reader.Run(TypeValue.ToJson, ResolvedIdentifier.ToJson)

  let rootValueToJson =
    Json.buildRootEncoder<TypeValue<ValueExt<unit>>, ValueExt<unit>> (NonEmptyList.OfList(Value.ToJson, []))

  let valueEncoder = rootValueToJson >> Reader.Run(rootExprToJson, TypeValue.ToJson)

  let encoded =
    Delta.ToJson expression
    |> Reader.Run(valueEncoder, DeltaExt.ToJson valueEncoder)

  match encoded with
  | Right err -> Assert.Fail $"Encode failed: {err}"
  | Left json ->
    Assert.That(normalize json, Is.EqualTo(normalize expectedJson))

    let rootExprFromJson =
      Expr.FromJson >> Reader.Run(TypeValue.FromJson, ResolvedIdentifier.FromJson)

    let rootValueFromJson =
      Json.buildRootParser<TypeValue<ValueExt<unit>>, ResolvedIdentifier, ValueExt<unit>> (
        NonEmptyList.OfList(Value.FromJson, [])
      )

    let valueParser =
      rootValueFromJson
      >> Reader.Run(rootExprFromJson, TypeValue.FromJson, ResolvedIdentifier.FromJson)

    let parsed =
      Delta.FromJson expectedJson
      |> Reader.Run(valueParser, DeltaExt.FromJson valueParser)

    match parsed with
    | Right err -> Assert.Fail $"Parse failed: {err}"
    | Left result -> Assert.That(result, Is.EqualTo(expression))


[<Test>]
let ``Delta.Multiple json round-trip`` () =

  let delta = Delta<ValueExt<unit>, DeltaExt<unit>>.Multiple [ Delta.Multiple [] ]

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

  let delta =
    Delta<ValueExt<unit>, DeltaExt<unit>>.Replace(Value.Primitive(PrimitiveValue.Int32 99))

  (delta, json) ||> ``Assert Delta -> ToJson -> FromJson -> Delta``


[<Test>]
let ``Delta.Record json round-trip`` () =
  let delta =
    Delta<ValueExt<unit>, DeltaExt<unit>>.Record("Foo", Delta.Replace(Value.Primitive(PrimitiveValue.Int32 99)))

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


[<Test>]
let ``Delta.Ext list append json round-trip`` () =
  let str v =
    Value.Primitive(PrimitiveValue.String v)

  let delta =
    Delta.Ext(DeltaExt.DeltaExtension(Choice1Of3(ListDeltaExt.AppendElement(str "x"))))

  let json =
    """ 
      {
        "discriminator": "deltaExt",
        "value": {
          "discriminator": "list",
          "value": {
            "discriminator": "appendElement",
            "value": {
              "discriminator": "string",
              "value": "x"
            }
          }
        }
      }
    """
    |> JsonValue.Parse

  (delta, json) ||> ``Assert Delta -> ToJson -> FromJson -> Delta``


[<Test>]
let ``Delta.Ext list update json round-trip`` () =
  let str v =
    Value.Primitive(PrimitiveValue.String v)

  let delta =
    Delta.Ext(DeltaExt.DeltaExtension(Choice1Of3(ListDeltaExt.UpdateElement(11, str "x"))))

  let json =
    """ 
      {
        "discriminator": "deltaExt",
        "value": {
          "discriminator": "list",
          "value": {
            "discriminator": "updateElementAt",
            "value": {
              "index": 11,
              "value": {
                "discriminator": "string",
                "value": "x"
              }
            }
          }
        }
      }
    """
    |> JsonValue.Parse

  (delta, json) ||> ``Assert Delta -> ToJson -> FromJson -> Delta``

[<Test>]
let ``Delta.Ext list remove json round-trip`` () =

  let delta =
    Delta.Ext(DeltaExt.DeltaExtension(Choice1Of3(ListDeltaExt.RemoveElement(4))))

  let json =
    """ {
        "discriminator": "deltaExt",
        "value": {
          "discriminator": "list",
          "value": {
            "discriminator": "removeElementAt",
            "index": 4
          }
        }
      }

    """
    |> JsonValue.Parse

  (delta, json) ||> ``Assert Delta -> ToJson -> FromJson -> Delta``

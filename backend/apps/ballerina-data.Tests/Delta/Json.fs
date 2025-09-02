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

let ``Assert Delta -> ToJson -> FromJson -> Delta`` (expression: Delta) (expectedJson: JsonValue) =
  let normalize (json: JsonValue) =
    json.ToString JsonSaveOptions.DisableFormatting

  let toJson = Delta.ToJson expression
  Assert.That(normalize toJson, Is.EqualTo(normalize expectedJson))

  let parser = Value.FromJson >> Reader.Run TypeValue.FromJson

  let parsed = Delta.FromJson expectedJson |> Reader.Run parser

  match parsed with
  | Right err -> Assert.Fail $"Parse failed: {err}"
  | Left result -> Assert.That(result, Is.EqualTo(expression))


[<Test>]
let ``Delta.Multiple json round-trip`` () =

  let delta = Delta.Multiple [ Delta.Multiple [] ]

  let json =
    """ 
      {
      "kind":"multiple",
      "multiple": [
        {
        "kind":"multiple",
        "multiple": [
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
      "kind":"replace",
      "replace": {"kind":"int","int":"99"}
      }
    """
    |> JsonValue.Parse

  let delta = Delta.Replace(Value.Primitive(PrimitiveValue.Int 99))

  (delta, json) ||> ``Assert Delta -> ToJson -> FromJson -> Delta``


[<Test>]
let ``Delta.Record json round-trip`` () =
  let delta =
    Delta.Record("Foo", Delta.Replace(Value.Primitive(PrimitiveValue.Int 99)))

  let json =
    """ 
      {
      "kind":"record",
      "record": ["Foo", 
        {
          "kind":"replace",
          "replace": {"kind":"int","int":"99"}
        }
      ]
      }
    """
    |> JsonValue.Parse

  (delta, json) ||> ``Assert Delta -> ToJson -> FromJson -> Delta``


[<Test>]
let ``Delta.Union json round-trip`` () =
  let delta =
    Delta.Union("Case1", Delta.Replace(Value.Primitive(PrimitiveValue.Int 99)))

  let json =
    """ 
      {
      "kind":"union",
      "union": ["Case1", 
        {
          "kind":"replace",
          "replace": {"kind":"int","int":"99"}
        }
      ]
      }
    """
    |> JsonValue.Parse

  (delta, json) ||> ``Assert Delta -> ToJson -> FromJson -> Delta``


[<Test>]
let ``Delta.Tuple json round-trip`` () =
  let delta = Delta.Tuple(3, Delta.Replace(Value.Primitive(PrimitiveValue.Int 99)))

  let json =
    """ 
      {
      "kind":"tuple",
      "tuple": [3, 
        {
          "kind":"replace",
          "replace": {"kind":"int","int":"99"}
        }
      ]
      }
    """
    |> JsonValue.Parse

  (delta, json) ||> ``Assert Delta -> ToJson -> FromJson -> Delta``


[<Test>]
let ``Delta.Sum json round-trip`` () =
  let delta = Delta.Sum(3, Delta.Replace(Value.Primitive(PrimitiveValue.Int 99)))

  let json =
    """ 
      {
      "kind":"sum",
      "sum": [3, 
        {
          "kind":"replace",
          "replace": {"kind":"int","int":"99"}
        }
      ]
      }
    """
    |> JsonValue.Parse

  (delta, json) ||> ``Assert Delta -> ToJson -> FromJson -> Delta``

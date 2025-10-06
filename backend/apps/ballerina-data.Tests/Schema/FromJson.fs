module Ballerina.Data.Tests.Schema.FromJson

open Ballerina.Data.Schema.Model
open Ballerina.Reader.WithError
open Ballerina.Collections.Sum
open NUnit.Framework
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Json.TypeExpr
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.DSL.Next.Types.Json
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Terms.Patterns

open Ballerina.Data.Json.Schema
open FSharp.Data

[<Test>]
let ``SpecNext-Schema entity method parses`` () =
  let tests =
    [ ("get", Get); ("getMany", GetMany); ("create", Create); ("delete", Delete) ]

  for (jsonValue, expected) in tests do
    let json = jsonValue |> JsonValue.String
    let result = EntityMethod.FromJson json

    match result with
    | Right e -> Assert.Fail($"Failed to parse entity method: {e}")
    | Left actual -> Assert.That(actual, Is.EqualTo(expected))

[<Test>]
let ``SpecNext-Schema lookup method parses`` () =
  let tests =
    [ ("get", LookupMethod.Get)
      ("getMany", LookupMethod.GetMany)
      ("create", LookupMethod.Create)
      ("delete", LookupMethod.Delete)
      ("link", LookupMethod.Link)
      ("unlink", LookupMethod.Unlink) ]

  for (jsonValue, expected) in tests do
    let json = jsonValue |> JsonValue.String
    let result = LookupMethod.FromJson json

    match result with
    | Right e -> Assert.Fail($"Failed to parse lookup method: {e}")
    | Left actual -> Assert.That(actual, Is.EqualTo(expected))

[<Test>]
let ``SpecNext-Schema updater descriptor parses`` () =
  let json =
    """ 
      [[["field", "FieldName"], ["listItem", "VariableBoundToChangedItem"], ["tupleItem", 7], ["unionCase", ["CaseName","VariableBoundToChangedCase"]], ["sumCase", [3, "VariableBoundToChangedSum"]]], {"discriminator":"bool","value":"true"}, {"discriminator":"int32","value":"100"}]
    """
    |> JsonValue.Parse

  let expected: Updater<TypeExpr> =
    { Updater.Path =
        [ UpdaterPathStep.Field "FieldName"
          UpdaterPathStep.ListItem(Var.Create "VariableBoundToChangedItem")
          UpdaterPathStep.TupleItem(7)
          UpdaterPathStep.UnionCase("CaseName", Var.Create "VariableBoundToChangedCase")
          UpdaterPathStep.SumCase(3, Var.Create "VariableBoundToChangedSum") ]
      Condition = Expr.Primitive(PrimitiveValue.Bool true)
      Expr = Expr.Primitive(PrimitiveValue.Int32 100) }

  match json |> Updater.FromJson |> Reader.Run TypeExpr.FromJson with
  | Right e -> Assert.Fail($"Failed to parse updater descriptor: {e}")
  | Left actual -> Assert.That(actual, Is.EqualTo(expected))

[<Test>]
let ``SpecNext-Schema entity descriptor parses`` () =
  let json =
    """ 
    {
      "type": { "discriminator":"lookup", "value":"MyType" },
      "methods": ["get", "getMany", "create", "delete"],
      "updaters": [],
      "predicates": {}
    }
    """
    |> JsonValue.Parse

  let expected =
    { Type = "MyType" |> Identifier.LocalScope |> TypeExpr.Lookup
      Methods = Set.ofList [ Get; GetMany; Create; Delete ]
      Updaters = []
      Predicates = Map.empty }

  match json |> EntityDescriptor.FromJson |> Reader.Run TypeExpr.FromJson with
  | Right e -> Assert.Fail($"Failed to parse entity method: {e}")
  | Left actual -> Assert.That(actual, Is.EqualTo(expected))

[<Test>]
let ``SpecNext-Schema directed lookup descriptor parses`` () =
  let json =
    """ 
    {
      "arity": { "min":1, "max":2 },
      "methods": ["get", "getMany", "create", "delete", "link", "unlink"],
      "path": [["field", "FieldName"], ["listItem", "VariableBoundToChangedItem"], ["tupleItem", 7], ["unionCase", ["CaseName","VariableBoundToChangedCase"]], ["sumCase", [3, "VariableBoundToChangedSum"]]]
    }
    """
    |> JsonValue.Parse

  let expected =
    { Arity = { Min = Some 1; Max = Some 2 }
      Methods =
        Set.ofList
          [ LookupMethod.Get
            LookupMethod.GetMany
            LookupMethod.Create
            LookupMethod.Delete
            LookupMethod.Link
            LookupMethod.Unlink ]
      Path =
        [ UpdaterPathStep.Field "FieldName"
          UpdaterPathStep.ListItem(Var.Create "VariableBoundToChangedItem")
          UpdaterPathStep.TupleItem(7)
          UpdaterPathStep.UnionCase("CaseName", Var.Create "VariableBoundToChangedCase")
          UpdaterPathStep.SumCase(3, Var.Create "VariableBoundToChangedSum") ] }

  match json |> DirectedLookupDescriptor.FromJson with
  | Right e -> Assert.Fail($"Failed to parse lookup descriptor: {e}")
  | Left actual -> Assert.That(actual, Is.EqualTo(expected))

[<Test>]
let ``SpecNext-Schema lookup descriptor parses`` () =
  let json =
    """ 
    {
      "source": "SourceTable",
      "target": "TargetTable",
      "forward": {
        "arity": { "min": 1 },
        "methods": ["get", "getMany", "create", "delete", "link", "unlink"],
        "path": []
      },
      "backward": {
        "name": "TargetToSource",
        "descriptor": {
          "arity": {},
          "methods": ["get", "getMany", "create", "delete", "link", "unlink"],
          "path": []
        }
      }
    }
    """
    |> JsonValue.Parse

  let expected: LookupDescriptor =
    { Source = { EntityName = "SourceTable" }
      Target = { EntityName = "TargetTable" }
      Forward =
        { Arity = { Min = Some 1; Max = None }
          Methods =
            Set.ofList
              [ LookupMethod.Get
                LookupMethod.GetMany
                LookupMethod.Create
                LookupMethod.Delete
                LookupMethod.Link
                LookupMethod.Unlink ]
          Path = [] }
      Backward =
        Some(
          { LookupName = "TargetToSource" },
          { Arity = { Min = None; Max = None }
            Methods =
              Set.ofList
                [ LookupMethod.Get
                  LookupMethod.GetMany
                  LookupMethod.Create
                  LookupMethod.Delete
                  LookupMethod.Link
                  LookupMethod.Unlink ]
            Path = [] }
        ) }

  match json |> LookupDescriptor.FromJson with
  | Right e -> Assert.Fail($"Failed to parse lookup descriptor: {e}")
  | Left actual -> Assert.That(actual, Is.EqualTo(expected))

[<Test>]
let ``SpecNext-Schema full schema descriptor parses`` () =
  let json =
    """ 
    {
      "entities": {
        "SourceTable": {
          "type": { "discriminator":"lookup", "value":"SomeType" },
          "methods": ["get", "getMany", "create", "delete"],
          "updaters": [],
          "predicates": { "SomePredicate": {"discriminator":"bool","value":"false"} }
        },
        "TargetTable": {
          "type": { "discriminator":"lookup", "value":"AnotherType" },
          "methods": ["get", "getMany", "create", "delete"],
          "updaters": [],
          "predicates": { "AnotherPredicate": {"discriminator":"bool","value":"true"} }
        }
      },
      "lookups": {
        "SourceToTarget": {
          "source": "SourceTable",
          "target": "TargetTable",
          "forward": {
            "arity": { "min": 1 },
            "methods": ["get", "getMany", "create", "delete", "link", "unlink"],
            "path":[]
          },
          "backward": {
            "name": "TargetToSource",
            "descriptor": {
              "arity": {},
              "methods": ["link", "unlink"],
              "path":[]
            }
          }
        }
      }
    }
    """
    |> JsonValue.Parse

  let expected: Schema<TypeExpr> =
    { Entities =
        Map.ofList
          [ ({ EntityName = "SourceTable" },
             { Type = TypeExpr.Lookup("SomeType" |> Identifier.LocalScope)
               Methods = Set.ofList [ Get; GetMany; Create; Delete ]
               Updaters = []
               Predicates =
                 [ ("SomePredicate", Expr<TypeExpr>.Primitive(PrimitiveValue.Bool false)) ]
                 |> Map.ofList })
            ({ EntityName = "TargetTable" },
             { Type = TypeExpr.Lookup("AnotherType" |> Identifier.LocalScope)
               Methods = Set.ofList [ Get; GetMany; Create; Delete ]
               Updaters = []
               Predicates =
                 [ ("AnotherPredicate", Expr<TypeExpr>.Primitive(PrimitiveValue.Bool true)) ]
                 |> Map.ofList }) ]
      Lookups =
        Map.ofList
          [ ({ LookupName = "SourceToTarget" },
             { Source = { EntityName = "SourceTable" }
               Target = { EntityName = "TargetTable" }
               Forward =
                 { Arity = { Min = Some 1; Max = None }
                   Methods =
                     Set.ofList
                       [ LookupMethod.Get
                         LookupMethod.GetMany
                         LookupMethod.Create
                         LookupMethod.Delete
                         LookupMethod.Link
                         LookupMethod.Unlink ]
                   Path = [] }
               Backward =
                 Some(
                   ({ LookupName = "TargetToSource" },
                    { Arity = { Min = None; Max = None }
                      Methods = Set.ofList [ LookupMethod.Link; LookupMethod.Unlink ]
                      Path = [] })
                 ) }) ] }

  match json |> Schema.FromJson |> Reader.Run TypeExpr.FromJson with
  | Right e -> Assert.Fail($"Failed to parse schema descriptor: {e}")
  | Left actual -> Assert.That(actual, Is.EqualTo(expected))

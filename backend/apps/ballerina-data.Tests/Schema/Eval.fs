module Ballerina.Data.Tests.Schema.Eval

open Ballerina.Data.Schema.Model
open Ballerina.State.WithError
open Ballerina.Collections.Sum
open NUnit.Framework
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.TypeChecker.Eval
open Ballerina.DSL.Next.Types.TypeChecker.Model
open Ballerina.DSL.Next.Types.TypeChecker.Patterns
open Ballerina.DSL.Next.Types.TypeChecker
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Terms.Patterns
open Ballerina.StdLib.OrderPreservingMap
open Ballerina.Data.TypeEval

[<Test>]
let ``SpecNext-Schema evaluates`` () =
  let source: Schema<TypeExpr, Identifier> =
    { Types = OrderedMap.empty
      Entities =
        Map.ofList
          [ ({ EntityName = "SourceTable" },
             { Type = "SomeType" |> Identifier.LocalScope |> TypeExpr.Lookup
               Methods = Set.ofList [ Get; GetMany; Create; Delete ]
               Updaters =
                 [ { Updater.Path = []
                     Condition = Expr<TypeExpr, Identifier>.Primitive(PrimitiveValue.Bool true)
                     Expr = Expr<TypeExpr, Identifier>.Primitive(PrimitiveValue.Int32 42) } ]
               Predicates =
                 [ ("SomePredicate", Expr<TypeExpr, Identifier>.Primitive(PrimitiveValue.Bool false)) ]
                 |> Map.ofList })
            ({ EntityName = "TargetTable" },
             { Type = "AnotherType" |> Identifier.LocalScope |> TypeExpr.Lookup
               Methods = Set.ofList [ Get; GetMany; Create; Delete ]
               Updaters = []
               Predicates =
                 [ ("AnotherPredicate", Expr<TypeExpr, Identifier>.Primitive(PrimitiveValue.Bool true)) ]
                 |> Map.ofList }) ]
      Lookups = Map.empty }

  let SomeType =
    TypeValue.CreateRecord(
      OrderedMap.ofList [ ("Foo" |> Identifier.LocalScope |> TypeSymbol.Create, TypeValue.CreateInt32()) ]
    )

  let AnotherType =
    TypeValue.CreateRecord(
      OrderedMap.ofList [ ("Bar" |> Identifier.LocalScope |> TypeSymbol.Create, TypeValue.CreateString()) ]
    )

  let expected: Schema<TypeValue, ResolvedIdentifier> =
    { Types = OrderedMap.empty
      Entities =
        Map.ofList
          [ { EntityName = "SourceTable" },
            { Type = SomeType
              Methods = Set.ofList [ Get; GetMany; Create; Delete ]
              Updaters =
                [ { Updater.Path = []
                    Condition = Expr.Primitive(PrimitiveValue.Bool true)
                    Expr = Expr.Primitive(PrimitiveValue.Int32 42) } ]
              Predicates = Map.ofList [ ("SomePredicate", Expr.Primitive(PrimitiveValue.Bool false)) ] }
            { EntityName = "TargetTable" },
            { Type = AnotherType
              Methods = Set.ofList [ Get; GetMany; Create; Delete ]
              Updaters = []
              Predicates = Map.ofList [ ("AnotherPredicate", Expr.Primitive(PrimitiveValue.Bool true)) ] } ]
      Lookups = Map.empty }

  let initialState: TypeExprEvalState =
    { TypeExprEvalState.Empty with
        Bindings =
          Map.ofList
            [ ("SomeType" |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve, (SomeType, Kind.Star))
              ("AnotherType" |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve, (AnotherType, Kind.Star)) ] }

  match
    source |> Schema.SchemaEval <| OrderedMap.empty
    |> State.Run(TypeExprEvalContext.Empty("", ""), initialState)
  with
  | Right e -> Assert.Fail($"Failed to eval schema: {e}")
  | Left(actual, _) -> Assert.That(actual, Is.EqualTo(expected))

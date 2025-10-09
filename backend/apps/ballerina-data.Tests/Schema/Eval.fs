module Ballerina.Data.Tests.Schema.Eval

open Ballerina.Data.Schema.Model
open Ballerina.State.WithError
open Ballerina.Collections.Sum
open NUnit.Framework
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Eval
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.Data.TypeEval
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Terms.Patterns
open Ballerina.StdLib.OrderPreservingMap

[<Test>]
let ``SpecNext-Schema evaluates`` () =
  let source: Schema<TypeExpr> =
    { Entities =
        Map.ofList
          [ ({ EntityName = "SourceTable" },
             { Type = "SomeType" |> Identifier.LocalScope |> TypeExpr.Lookup
               Methods = Set.ofList [ Get; GetMany; Create; Delete ]
               Updaters =
                 [ { Updater.Path = []
                     Condition = Expr<TypeExpr>.Primitive(PrimitiveValue.Bool true, Location.Unknown)
                     Expr = Expr<TypeExpr>.Primitive(PrimitiveValue.Int32 42, Location.Unknown) } ]
               Predicates =
                 [ ("SomePredicate", Expr<TypeExpr>.Primitive(PrimitiveValue.Bool false, Location.Unknown)) ]
                 |> Map.ofList })
            ({ EntityName = "TargetTable" },
             { Type = "AnotherType" |> Identifier.LocalScope |> TypeExpr.Lookup
               Methods = Set.ofList [ Get; GetMany; Create; Delete ]
               Updaters = []
               Predicates =
                 [ ("AnotherPredicate", Expr<TypeExpr>.Primitive(PrimitiveValue.Bool true, Location.Unknown)) ]
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

  let expected: Schema<TypeValue> =
    { Entities =
        Map.ofList
          [ ({ EntityName = "SourceTable" },
             { Type = SomeType
               Methods = Set.ofList [ Get; GetMany; Create; Delete ]
               Updaters =
                 [ { Updater.Path = []
                     Condition = Expr<TypeValue>.Primitive(PrimitiveValue.Bool true, Location.Unknown)
                     Expr = Expr<TypeValue>.Primitive(PrimitiveValue.Int32 42, Location.Unknown) } ]
               Predicates =
                 Map.ofList
                   [ ("SomePredicate", Expr<TypeValue>.Primitive(PrimitiveValue.Bool false, Location.Unknown)) ] })
            ({ EntityName = "TargetTable" },
             { Type = AnotherType
               Methods = Set.ofList [ Get; GetMany; Create; Delete ]
               Updaters = []
               Predicates =
                 Map.ofList
                   [ ("AnotherPredicate", Expr<TypeValue>.Primitive(PrimitiveValue.Bool true, Location.Unknown)) ] }) ]
      Lookups = Map.empty }

  let initialState: TypeExprEvalState =
    { TypeExprEvalState.Empty with
        Bindings =
          Map.ofList
            [ ("SomeType" |> Identifier.LocalScope, (SomeType, Kind.Star))
              ("AnotherType" |> Identifier.LocalScope, (AnotherType, Kind.Star)) ] }

  match
    source
    |> Schema.SchemaEval
    |> State.Run(TypeExprEvalContext.Empty, initialState)
  with
  | Right e -> Assert.Fail($"Failed to eval schema: {e}")
  | Left(actual, _) -> Assert.That(actual, Is.EqualTo(expected))

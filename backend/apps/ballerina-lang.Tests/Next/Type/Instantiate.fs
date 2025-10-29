module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Type.Instantiate

open Ballerina.Collections.Sum
open NUnit.Framework
open Ballerina.Errors
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.DSL.Next.EquivalenceClasses
open Ballerina.DSL.Next.Unification
open Ballerina.State.WithError
open Ballerina.LocalizedErrors
open Ballerina.DSL.Next.Types.TypeChecker.Eval
open Ballerina.DSL.Next.Types.TypeChecker.Model
open Ballerina.DSL.Next.Types.TypeChecker.Patterns
open Ballerina.DSL.Next.Types.TypeChecker

[<Test>]
let ``LangNext-Instantiate straightforward var to primitive`` () =

  let a = TypeVar.Create("a")

  let classes: EquivalenceClasses<TypeVar, TypeValue> =
    { Classes = Map.ofList [ "a", EquivalenceClass.Create(a |> Set.singleton, TypeValue.CreateInt32() |> Some) ]
      Variables = Map.ofList [ a, a.Name ] }

  let program = TypeValue.Var a

  let actual =
    ((TypeValue.Instantiate Location.Unknown program).run (TypeInstantiateContext.Empty, classes))

  let expected = TypeValue.CreateInt32()

  match actual with
  | Sum.Left(actual, _) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"


[<Test>]
let ``LangNext-Instantiate var nested inside generics to primitive`` () =
  let a = TypeVar.Create("a")

  let classes: EquivalenceClasses<TypeVar, TypeValue> =
    { Classes = Map.ofList [ "a", EquivalenceClass.Create(a |> Set.singleton, TypeValue.CreateInt32() |> Some) ]
      Variables = Map.ofList [ a, a.Name ] }

  let program = TypeValue.Var a |> TypeValue.CreateSet

  let actual =
    ((TypeValue.Instantiate Location.Unknown program).run (TypeInstantiateContext.Empty, classes))

  let expected = TypeValue.CreateInt32() |> TypeValue.CreateSet

  match actual with
  | Sum.Left(actual, _) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-Instantiate var nested inside generics via other bound var to primitive`` () =
  let a = TypeVar.Create("a")
  let b = TypeVar.Create("b")

  let classes: EquivalenceClasses<TypeVar, TypeValue> =
    { Classes =
        Map.ofList
          [ "a", EquivalenceClass.Create(a |> Set.singleton, b |> TypeValue.Var |> TypeValue.CreateSet |> Some)
            "b", EquivalenceClass.Create(b |> Set.singleton, TypeValue.CreateString() |> Some) ]
      Variables = Map.ofList [ a, a.Name; b, b.Name ] }

  let program = TypeValue.Var a |> TypeValue.CreateSet

  let actual =
    ((TypeValue.Instantiate Location.Unknown program).run (TypeInstantiateContext.Empty, classes))

  let expected =
    TypeValue.CreateString() |> TypeValue.CreateSet |> TypeValue.CreateSet

  match actual with
  | Sum.Left(actual, _) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-Instantiate var nested inside generics via other bound and aliased var chain to primitive`` () =
  let a = TypeVar.Create("a")
  let b = TypeVar.Create("b")
  let c = TypeVar.Create("c")

  let classes: EquivalenceClasses<TypeVar, TypeValue> =
    { Classes =
        Map.ofList
          [ a.Name, EquivalenceClass.Create(a |> Set.singleton, b |> TypeValue.Var |> TypeValue.CreateSet |> Some)
            c.Name, EquivalenceClass.Create(c |> Set.singleton, TypeValue.CreateString() |> Some) ]
      Variables = Map.ofList [ a, a.Name; b, c.Name; c, c.Name ] }

  let program = TypeValue.Var a |> TypeValue.CreateSet |> TypeValue.CreateSet

  let actual =
    ((TypeValue.Instantiate Location.Unknown program).run (TypeInstantiateContext.Empty, classes))

  let expected =
    TypeValue.CreateString()
    |> TypeValue.CreateSet
    |> TypeValue.CreateSet
    |> TypeValue.CreateSet

  match actual with
  | Sum.Left(actual, _) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

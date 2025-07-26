module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Type.Instantiate

open Ballerina.Collections.Sum
open NUnit.Framework
open Ballerina.Errors
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.DSL.Next.KitchenSink
open Ballerina.DSL.Next.EquivalenceClasses
open Ballerina.DSL.Next.Unification
open Ballerina.State.WithError

[<Test>]
let ``LangNext-Instantiate straightforward var to primitive`` () =

  let a = TypeVar.Create("a")

  let classes: EquivalenceClasses<TypeVar, TypeValue> =
    { Classes =
        Map.ofList
          [ "a", EquivalenceClass.Create(a |> Set.singleton, PrimitiveType.Int |> TypeValue.Primitive |> Some) ]
      Variables = Map.ofList [ a, a.Name ] }

  let program = TypeValue.Var a

  let actual =
    (TypeValue.Instantiate(program).run (TypeInstantiateContext.Empty, classes))

  let expected = TypeValue.Primitive PrimitiveType.Int

  match actual with
  | Sum.Left(actual, _) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"


[<Test>]
let ``LangNext-Instantiate var nested inside generics to primitive`` () =
  let a = TypeVar.Create("a")

  let classes: EquivalenceClasses<TypeVar, TypeValue> =
    { Classes =
        Map.ofList
          [ "a", EquivalenceClass.Create(a |> Set.singleton, PrimitiveType.Int |> TypeValue.Primitive |> Some) ]
      Variables = Map.ofList [ a, a.Name ] }

  let program = TypeValue.Var a |> TypeValue.Set |> TypeValue.List

  let actual =
    (TypeValue.Instantiate(program).run (TypeInstantiateContext.Empty, classes))

  let expected =
    PrimitiveType.Int |> TypeValue.Primitive |> TypeValue.Set |> TypeValue.List

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
          [ "a", EquivalenceClass.Create(a |> Set.singleton, b |> TypeValue.Var |> TypeValue.List |> Some)
            "b", EquivalenceClass.Create(b |> Set.singleton, PrimitiveType.String |> TypeValue.Primitive |> Some) ]
      Variables = Map.ofList [ a, a.Name; b, b.Name ] }

  let program = TypeValue.Var a |> TypeValue.Set |> TypeValue.List

  let actual =
    (TypeValue.Instantiate(program).run (TypeInstantiateContext.Empty, classes))

  let expected =
    PrimitiveType.String
    |> TypeValue.Primitive
    |> TypeValue.List
    |> TypeValue.Set
    |> TypeValue.List

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
          [ a.Name, EquivalenceClass.Create(a |> Set.singleton, b |> TypeValue.Var |> TypeValue.List |> Some)
            c.Name, EquivalenceClass.Create(c |> Set.singleton, PrimitiveType.String |> TypeValue.Primitive |> Some) ]
      Variables = Map.ofList [ a, a.Name; b, c.Name; c, c.Name ] }

  let program = TypeValue.Var a |> TypeValue.Set |> TypeValue.List

  let actual =
    (TypeValue.Instantiate(program).run (TypeInstantiateContext.Empty, classes))

  let expected =
    PrimitiveType.String
    |> TypeValue.Primitive
    |> TypeValue.List
    |> TypeValue.Set
    |> TypeValue.List

  match actual with
  | Sum.Left(actual, _) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

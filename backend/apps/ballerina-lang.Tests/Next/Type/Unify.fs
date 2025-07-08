module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Type.Unify

open Ballerina.Collections.Sum
open NUnit.Framework
open Ballerina.Errors
open Ballerina.DSL.Next.Model
open Ballerina.DSL.Next.EquivalenceClasses
open Ballerina.DSL.Next.Unification
open Ballerina.State.WithError

let private initialClasses =
  EquivalenceClasses<string, Sum<string, PrimitiveType>>.Empty

let private valueOperations =
  { equalize =
      (fun (v1, v2) ->
        match v1, v2 with
        | Right v1, Right v2 when v1 <> v2 -> $"Error: cannot unify {v1} and {v2}" |> Errors.Singleton |> state.Throw
        | _ -> state { return () })
    asVar = Sum.mapRight (fun _ -> $"Error: not a variable" |> Errors.Singleton)
    toValue = Sum.Left }

[<Test>]
let ``LangNext-Unify binding trivial equivalence classes over primitives succeeds`` () =

  let program: State<unit, _, EquivalenceClasses<string, Sum<string, PrimitiveType>>, Errors> =
    state {
      do! EquivalenceClasses.Bind("v1", PrimitiveType.Int |> Right)
      do! EquivalenceClasses.Bind("v2", PrimitiveType.String |> Right)
      do! EquivalenceClasses.Bind("v3", PrimitiveType.Decimal |> Right)
      do! EquivalenceClasses.Bind("v4", PrimitiveType.Decimal |> Right)
    }

  let actual = program.run (valueOperations, initialClasses)

  let expected: EquivalenceClasses<string, Sum<string, PrimitiveType>> =
    { Classes =
        Map.ofList
          [ "v1", [ "v1" |> Left; PrimitiveType.Int |> Right ] |> Set.ofList
            "v2", [ "v2" |> Left; PrimitiveType.String |> Right ] |> Set.ofList
            "v3", [ "v3" |> Left; PrimitiveType.Decimal |> Right ] |> Set.ofList
            "v4", [ "v4" |> Left; PrimitiveType.Decimal |> Right ] |> Set.ofList ]
      Variables = Map.ofList [ "v1", "v1"; "v2", "v2"; "v3", "v3"; "v4", "v4" ] }

  match actual with
  | Sum.Left((), Some(actual)) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"
  | _ -> Assert.Fail $"Expected a new state but:with equivalence classes but got none"

[<Test>]
let ``LangNext-Unify binding equivalence classes over variables and primitives or variables succeeds`` () =

  let program: State<unit, _, EquivalenceClasses<string, Sum<string, PrimitiveType>>, Errors> =
    state {
      do! EquivalenceClasses.Bind("v1", PrimitiveType.Int |> Right)
      do! EquivalenceClasses.Bind("v2", PrimitiveType.String |> Right)
      do! EquivalenceClasses.Bind("v3", PrimitiveType.Decimal |> Right)
      do! EquivalenceClasses.Bind("v4", PrimitiveType.Decimal |> Right)
      do! EquivalenceClasses.Bind("v4", "v3" |> Left)
    }

  let actual = program.run (valueOperations, initialClasses)

  let expected: EquivalenceClasses<string, Sum<string, PrimitiveType>> =
    { Classes =
        Map.ofList
          [ "v1", [ "v1" |> Left; PrimitiveType.Int |> Right ] |> Set.ofList
            "v2", [ "v2" |> Left; PrimitiveType.String |> Right ] |> Set.ofList
            "v4", [ "v3" |> Left; "v4" |> Left; PrimitiveType.Decimal |> Right ] |> Set.ofList ]
      Variables = Map.ofList [ "v1", "v1"; "v2", "v2"; "v3", "v4"; "v4", "v4" ] }

  match actual with
  | Sum.Left((), Some(actual)) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"
  | _ -> Assert.Fail $"Expected a new state but:with equivalence classes but got none"

[<Test>]
let ``LangNext-Unify binding equivalence classes over variables and primitives or variables fails`` () =
  let program: State<unit, _, EquivalenceClasses<string, Sum<string, PrimitiveType>>, Errors> =
    state {
      do! EquivalenceClasses.Bind("v1", PrimitiveType.Int |> Right)
      do! EquivalenceClasses.Bind("v2", PrimitiveType.String |> Right)
      do! EquivalenceClasses.Bind("v3", PrimitiveType.Decimal |> Right)
      do! EquivalenceClasses.Bind("v4", PrimitiveType.Decimal |> Right)
      do! EquivalenceClasses.Bind("v4", "v1" |> Left)
    }

  let actual = program.run (valueOperations, initialClasses)

  match actual with
  | Sum.Right _ -> Assert.Pass()
  | Sum.Left((), result) -> Assert.Fail $"Expected unification to fail but it succeeded with {result}"


[<Test>]
let ``LangNext-Unify binding equivalence classes over variables and primitives or variables in a chain succeeds`` () =
  let program: State<unit, _, EquivalenceClasses<string, Sum<string, PrimitiveType>>, Errors> =
    state {
      do! EquivalenceClasses.Bind("v1", PrimitiveType.Int |> Right)
      do! EquivalenceClasses.Bind("v2", PrimitiveType.String |> Right)
      do! EquivalenceClasses.Bind("v3", PrimitiveType.Decimal |> Right)
      do! EquivalenceClasses.Bind("v4", PrimitiveType.Decimal |> Right)
      do! EquivalenceClasses.Bind("v5", PrimitiveType.Decimal |> Right)
      do! EquivalenceClasses.Bind("v6", PrimitiveType.Decimal |> Right)
      do! EquivalenceClasses.Bind("v4", "v3" |> Left)
      do! EquivalenceClasses.Bind("v6", "v5" |> Left)
      do! EquivalenceClasses.Bind("v6", "v3" |> Left)
    }

  let actual = program.run (valueOperations, initialClasses)

  let expected: EquivalenceClasses<string, Sum<string, PrimitiveType>> =
    { Classes =
        Map.ofList
          [ "v1", [ "v1" |> Left; PrimitiveType.Int |> Right ] |> Set.ofList
            "v2", [ "v2" |> Left; PrimitiveType.String |> Right ] |> Set.ofList
            "v6",
            [ "v3" |> Left
              "v4" |> Left
              "v5" |> Left
              "v6" |> Left
              PrimitiveType.Decimal |> Right ]
            |> Set.ofList ]
      Variables = Map.ofList [ "v1", "v1"; "v2", "v2"; "v3", "v6"; "v4", "v6"; "v5", "v6"; "v6", "v6" ] }

  match actual with
  | Sum.Left((), Some(actual)) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"
  | _ -> Assert.Fail $"Expected a new state but:with equivalence classes but got none"


[<Test>]
let ``LangNext-Unify unifies types without variables`` () =
  let inputs =
    [ TypeValue.Primitive PrimitiveType.Int
      TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int, TypeValue.Primitive PrimitiveType.String)
      TypeValue.List(TypeValue.Primitive PrimitiveType.Int)
      TypeValue.Set(TypeValue.Primitive PrimitiveType.Int)
      TypeValue.Map(TypeValue.Primitive PrimitiveType.Int, TypeValue.Primitive PrimitiveType.String)
      TypeValue.Tuple(
        [ TypeValue.Primitive PrimitiveType.Int
          TypeValue.Primitive PrimitiveType.String ]
      )
      TypeValue.Sum(
        [ TypeValue.Primitive PrimitiveType.Int
          TypeValue.Primitive PrimitiveType.String ]
      )
      TypeValue.Record(
        [ "a", TypeValue.Primitive PrimitiveType.Int
          "b", TypeValue.Primitive PrimitiveType.String ]
        |> Map.ofList
      )
      TypeValue.Union(
        [ "a", TypeValue.Primitive PrimitiveType.Int
          "b", TypeValue.Primitive PrimitiveType.String ]
        |> Map.ofList
      ) ]

  let actual =
    inputs
    |> List.map (fun input -> TypeValue.Unify(input, input).run (UnificationContext.Empty, EquivalenceClasses.Empty))

  let expected: EquivalenceClasses<_, _> =
    { Classes = Map.empty
      Variables = Map.empty }

  for actual in actual do
    match actual with
    | Sum.Left((), Some(actual)) when actual <> expected -> Assert.That(actual, Is.EqualTo expected)
    | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"
    | _ -> ()

[<Test>]
let ``LangNext-Unify unifies arrows`` () =
  let a = TypeVar.Create "a"
  let b = TypeVar.Create "b"

  let inputs =
    TypeValue.Arrow(TypeValue.Var(a), TypeValue.Primitive PrimitiveType.String),
    TypeValue.Arrow(TypeValue.Primitive PrimitiveType.String, TypeValue.Var(b))

  let actual =
    (TypeValue.Unify(inputs).run (UnificationContext.Empty, EquivalenceClasses.Empty))

  let expected: EquivalenceClasses<_, _> =
    { Classes =
        [ "a", [ TypeValue.Var a; TypeValue.Primitive PrimitiveType.String ] |> Set.ofList
          "b", [ TypeValue.Var b; TypeValue.Primitive PrimitiveType.String ] |> Set.ofList ]
        |> Map.ofList
      Variables = Map.ofList [ a, "a"; b, "b" ] }

  match actual with
  | Sum.Left((), Some(actual)) when actual <> expected -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"
  | _ -> ()

[<Test>]
let ``LangNext-Unify unifies lists of tuples`` () =
  let a = TypeVar.Create "a"
  let b = TypeVar.Create "b"

  let inputs =
    TypeValue.List(TypeValue.Tuple([ TypeValue.Var(a); TypeValue.Primitive PrimitiveType.String ])),
    TypeValue.List(TypeValue.Tuple([ TypeValue.Var(b); TypeValue.Primitive PrimitiveType.String ]))

  let actual =
    (TypeValue.Unify(inputs).run (UnificationContext.Empty, EquivalenceClasses.Empty))

  let expected: EquivalenceClasses<_, _> =
    { Classes = [ "a", [ TypeValue.Var a; TypeValue.Var b ] |> Set.ofList ] |> Map.ofList
      Variables = Map.ofList [ a, "a"; b, "a" ] }

  match actual with
  | Sum.Left((), Some(actual)) when actual <> expected -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"
  | _ -> ()

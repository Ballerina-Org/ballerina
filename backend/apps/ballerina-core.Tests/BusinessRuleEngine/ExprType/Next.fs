module Ballerina.Core.Tests.BusinessRuleEngine.Next

open Ballerina.Collections.Sum
open NUnit.Framework
open Ballerina.DSL.Expr.Types.Model
open Ballerina.DSL.Expr.Model
open Ballerina.DSL.Expr.Types.TypeCheck
open Ballerina.Errors
open Ballerina.DSL.Extensions.BLPLang
open Ballerina.DSL.Expr.Extensions.Primitives
open Ballerina.DSL.Expr.Extensions.Collections
open Ballerina.DSL.Expr.Next
open Ballerina.State.WithError

[<Test>]
let ``Types-Next Sums of non-overlapping unions simplify`` () =
  let t1 =
    TypeExpr.Union(
      Map.ofList
        [ "a", TypeExpr.Primitive PrimitiveType.Int
          "b", TypeExpr.Primitive PrimitiveType.String ]
    )

  let t2 =
    TypeExpr.Union(
      Map.ofList
        [ "c", TypeExpr.Primitive PrimitiveType.Decimal
          "d", TypeExpr.Primitive PrimitiveType.Bool ]
    )

  let actual =
    TypeExpr.Sum [ t1; t2 ] |> TypeExpr.Eval |> ReaderWithError.Run Map.empty

  let expected =
    TypeValue.Union(
      Map.ofList
        [ "a", TypeValue.Primitive PrimitiveType.Int
          "b", TypeValue.Primitive PrimitiveType.String
          "c", TypeValue.Primitive PrimitiveType.Decimal
          "d", TypeValue.Primitive PrimitiveType.Bool ]
    )

  match actual with
  | Sum.Left actual -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``Types-Next Sums of unions and non-unions do not simplify`` () =
  let t1 =
    TypeExpr.Union(
      Map.ofList
        [ "a", TypeExpr.Primitive PrimitiveType.Int
          "b", TypeExpr.Primitive PrimitiveType.String ]
    )

  let t2 = TypeExpr.List(TypeExpr.Primitive PrimitiveType.Decimal)

  let actual =
    TypeExpr.Sum [ t1; t2 ] |> TypeExpr.Eval |> ReaderWithError.Run Map.empty

  let expected =
    TypeValue.Sum(
      [ TypeValue.Union(
          Map.ofList
            [ "a", TypeValue.Primitive PrimitiveType.Int
              "b", TypeValue.Primitive PrimitiveType.String ]
        )
        TypeValue.List(TypeValue.Primitive PrimitiveType.Decimal) ]
    )

  match actual with
  | Sum.Left actual -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``Types-Next Keyof extracts record keys`` () =
  let t1 =
    TypeExpr.Record(
      Map.ofList
        [ "a", TypeExpr.Primitive PrimitiveType.Int
          "b", TypeExpr.Primitive PrimitiveType.String
          "c", TypeExpr.Primitive PrimitiveType.Decimal ]
    )
    |> TypeExpr.KeyOf

  let actual = t1 |> TypeExpr.Eval |> ReaderWithError.Run Map.empty

  let expected =
    TypeValue.Union(
      Map.ofList
        [ "a", TypeValue.Primitive PrimitiveType.Unit
          "b", TypeValue.Primitive PrimitiveType.Unit
          "c", TypeValue.Primitive PrimitiveType.Unit ]
    )

  match actual with
  | Sum.Left actual -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

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
let ``Types-Next binding trivial equivalence classes over primitives succeeds`` () =

  let program: State<unit, unit, EquivalenceClasses<string, Sum<string, PrimitiveType>>, Errors> =
    state {
      do! EquivalenceClasses.Bind valueOperations ("v1", PrimitiveType.Int |> Right)
      do! EquivalenceClasses.Bind valueOperations ("v2", PrimitiveType.String |> Right)
      do! EquivalenceClasses.Bind valueOperations ("v3", PrimitiveType.Decimal |> Right)
      do! EquivalenceClasses.Bind valueOperations ("v4", PrimitiveType.Decimal |> Right)
    }

  let actual = program.run ((), initialClasses)

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
let ``Types-Next binding equivalence classes over variables and primitives or variables succeeds`` () =

  let program: State<unit, unit, EquivalenceClasses<string, Sum<string, PrimitiveType>>, Errors> =
    state {
      do! EquivalenceClasses.Bind valueOperations ("v1", PrimitiveType.Int |> Right)
      do! EquivalenceClasses.Bind valueOperations ("v2", PrimitiveType.String |> Right)
      do! EquivalenceClasses.Bind valueOperations ("v3", PrimitiveType.Decimal |> Right)
      do! EquivalenceClasses.Bind valueOperations ("v4", PrimitiveType.Decimal |> Right)
      do! EquivalenceClasses.Bind valueOperations ("v4", "v3" |> Left)
    }

  let actual = program.run ((), initialClasses)

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
let ``Types-Next binding equivalence classes over variables and primitives or variables fails`` () =
  let program: State<unit, unit, EquivalenceClasses<string, Sum<string, PrimitiveType>>, Errors> =
    state {
      do! EquivalenceClasses.Bind valueOperations ("v1", PrimitiveType.Int |> Right)
      do! EquivalenceClasses.Bind valueOperations ("v2", PrimitiveType.String |> Right)
      do! EquivalenceClasses.Bind valueOperations ("v3", PrimitiveType.Decimal |> Right)
      do! EquivalenceClasses.Bind valueOperations ("v4", PrimitiveType.Decimal |> Right)
      do! EquivalenceClasses.Bind valueOperations ("v4", "v1" |> Left)
    }

  let actual = program.run ((), initialClasses)

  match actual with
  | Sum.Right _ -> Assert.Pass()
  | Sum.Left((), result) -> Assert.Fail $"Expected unification to fail but it succeeded with {result}"


[<Test>]
let ``Types-Next binding equivalence classes over variables and primitives or variables in a chain succeeds`` () =
  let program: State<unit, unit, EquivalenceClasses<string, Sum<string, PrimitiveType>>, Errors> =
    state {
      do! EquivalenceClasses.Bind valueOperations ("v1", PrimitiveType.Int |> Right)
      do! EquivalenceClasses.Bind valueOperations ("v2", PrimitiveType.String |> Right)
      do! EquivalenceClasses.Bind valueOperations ("v3", PrimitiveType.Decimal |> Right)
      do! EquivalenceClasses.Bind valueOperations ("v4", PrimitiveType.Decimal |> Right)
      do! EquivalenceClasses.Bind valueOperations ("v5", PrimitiveType.Decimal |> Right)
      do! EquivalenceClasses.Bind valueOperations ("v6", PrimitiveType.Decimal |> Right)
      do! EquivalenceClasses.Bind valueOperations ("v4", "v3" |> Left)
      do! EquivalenceClasses.Bind valueOperations ("v6", "v5" |> Left)
      do! EquivalenceClasses.Bind valueOperations ("v6", "v3" |> Left)
    }

  let actual = program.run ((), initialClasses)

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

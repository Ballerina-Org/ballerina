module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Type.Unify

open Ballerina.Collections.Sum
open NUnit.Framework
open Ballerina.Errors
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.DSL.Next.KitchenSink
open Ballerina.DSL.Next.EquivalenceClasses
open Ballerina.DSL.Next.Unification
open Ballerina.State.WithError

let private initialClasses = EquivalenceClasses<string, PrimitiveType>.Empty
let private (!) = Identifier.LocalScope

let private valueOperations =
  { tryCompare = fun (v1: PrimitiveType, v2: PrimitiveType) -> if v1 = v2 then Some v1 else None
    equalize =
      (fun (v1, v2) ->
        if v1 <> v2 then
          $"Error: cannot unify {v1} and {v2}" |> Errors.Singleton |> state.Throw
        else
          state { return () }) }

[<Test>]
let ``LangNext-Unify binding trivial equivalence classes over primitives succeeds`` () =

  let program: State<unit, _, EquivalenceClasses<string, PrimitiveType>, Errors> =
    state {
      do! EquivalenceClasses.Bind("v1", PrimitiveType.Int32 |> Right)
      do! EquivalenceClasses.Bind("v2", PrimitiveType.String |> Right)
      do! EquivalenceClasses.Bind("v3", PrimitiveType.Decimal |> Right)
      do! EquivalenceClasses.Bind("v4", PrimitiveType.Decimal |> Right)
    }

  let actual = program.run (valueOperations, initialClasses)

  let expected: EquivalenceClasses<string, PrimitiveType> =
    { Classes =
        Map.ofList
          [ "v1", EquivalenceClass.Create("v1" |> Set.singleton, PrimitiveType.Int32 |> Some)
            "v2", EquivalenceClass.Create("v2" |> Set.singleton, PrimitiveType.String |> Some)
            "v3", EquivalenceClass.Create("v3" |> Set.singleton, PrimitiveType.Decimal |> Some)
            "v4", EquivalenceClass.Create("v4" |> Set.singleton, PrimitiveType.Decimal |> Some) ]
      Variables = Map.ofList [ "v1", "v1"; "v2", "v2"; "v3", "v3"; "v4", "v4" ] }

  match actual with
  | Sum.Left((), Some(actual)) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"
  | _ -> Assert.Fail $"Expected a new state but:with equivalence classes but got none"

[<Test>]
let ``LangNext-Unify binding equivalence classes over variables and primitives or variables succeeds`` () =

  let program: State<unit, _, EquivalenceClasses<string, PrimitiveType>, Errors> =
    state {
      do! EquivalenceClasses.Bind("v1", PrimitiveType.Int32 |> Right)
      do! EquivalenceClasses.Bind("v2", PrimitiveType.String |> Right)
      do! EquivalenceClasses.Bind("v3", PrimitiveType.Decimal |> Right)
      do! EquivalenceClasses.Bind("v4", PrimitiveType.Decimal |> Right)
      do! EquivalenceClasses.Bind("v4", "v3" |> Left)
    }

  let actual = program.run (valueOperations, initialClasses)

  let expected: EquivalenceClasses<string, PrimitiveType> =
    { Classes =
        Map.ofList
          [ "v1", EquivalenceClass.Create("v1" |> Set.singleton, PrimitiveType.Int32 |> Some)
            "v2", EquivalenceClass.Create("v2" |> Set.singleton, PrimitiveType.String |> Some)
            "v4", EquivalenceClass.Create([ "v3"; "v4" ] |> Set.ofList, PrimitiveType.Decimal |> Some) ]
      Variables = Map.ofList [ "v1", "v1"; "v2", "v2"; "v3", "v4"; "v4", "v4" ] }

  match actual with
  | Sum.Left((), Some(actual)) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"
  | _ -> Assert.Fail $"Expected a new state but:with equivalence classes but got none"

[<Test>]
let ``LangNext-Unify binding equivalence classes over variables and primitives or variables fails`` () =
  let program: State<unit, _, EquivalenceClasses<string, PrimitiveType>, Errors> =
    state {
      do! EquivalenceClasses.Bind("v1", PrimitiveType.Int32 |> Right)
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
  let program: State<unit, _, EquivalenceClasses<string, PrimitiveType>, Errors> =
    state {
      do! EquivalenceClasses.Bind("v1", PrimitiveType.Int32 |> Right)
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

  let expected: EquivalenceClasses<string, PrimitiveType> =
    { Classes =
        Map.ofList
          [ "v1", EquivalenceClass.Create(Set.ofList [ "v1" ], PrimitiveType.Int32 |> Some)
            "v2", EquivalenceClass.Create(Set.ofList [ "v2" ], PrimitiveType.String |> Some)
            "v6", EquivalenceClass.Create(Set.ofList [ "v3"; "v4"; "v5"; "v6" ], PrimitiveType.Decimal |> Some) ]
      Variables = Map.ofList [ "v1", "v1"; "v2", "v2"; "v3", "v6"; "v4", "v6"; "v5", "v6"; "v6", "v6" ] }

  match actual with
  | Sum.Left((), Some(actual)) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"
  | _ -> Assert.Fail $"Expected a new state but:with equivalence classes but got none"

  match actual with
  | Sum.Left((), Some(actual)) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"
  | _ -> Assert.Fail $"Expected a new state but:with equivalence classes but got none"


[<Test>]
let ``LangNext-Unify unifies types without variables`` () =
  let inputs =
    [ TypeValue.Primitive PrimitiveType.Int32
      TypeValue.Arrow(TypeValue.Primitive PrimitiveType.Int32, TypeValue.Primitive PrimitiveType.String)
      TypeValue.List(TypeValue.Primitive PrimitiveType.Int32)
      TypeValue.Set(TypeValue.Primitive PrimitiveType.Int32)
      TypeValue.Map(TypeValue.Primitive PrimitiveType.Int32, TypeValue.Primitive PrimitiveType.String)
      TypeValue.Tuple(
        [ TypeValue.Primitive PrimitiveType.Int32
          TypeValue.Primitive PrimitiveType.String ]
      )
      TypeValue.Sum(
        [ TypeValue.Primitive PrimitiveType.Int32
          TypeValue.Primitive PrimitiveType.String ]
      )
      TypeValue.Record(
        [ "a" |> TypeSymbol.Create, TypeValue.Primitive PrimitiveType.Int32
          "b" |> TypeSymbol.Create, TypeValue.Primitive PrimitiveType.String ]
        |> Map.ofList
      )
      TypeValue.Union(
        [ "a" |> TypeSymbol.Create, TypeValue.Primitive PrimitiveType.Int32
          "b" |> TypeSymbol.Create, TypeValue.Primitive PrimitiveType.String ]
        |> Map.ofList
      ) ]

  let actual =
    inputs
    |> List.map (fun input -> TypeValue.Unify(input, input).run (UnificationContext.Empty, EquivalenceClasses.Empty))

  let expected: EquivalenceClasses<TypeVar, TypeValue> =
    { Classes = Map.empty
      Variables = Map.empty }

  for actual in actual do
    match actual with
    | Sum.Left((), Some(actual)) -> Assert.That(actual, Is.EqualTo expected)
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

  let expected: EquivalenceClasses<TypeVar, TypeValue> =
    { Classes =
        [ "a", EquivalenceClass.Create(a |> Set.singleton, TypeValue.Primitive PrimitiveType.String |> Some)
          "b", EquivalenceClass.Create(b |> Set.singleton, TypeValue.Primitive PrimitiveType.String |> Some) ]
        |> Map.ofList
      Variables = Map.ofList [ a, "a"; b, "b" ] }

  match actual with
  | Sum.Left((), Some(actual)) -> Assert.That(actual, Is.EqualTo expected)
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

  let expected: EquivalenceClasses<TypeVar, TypeValue> =
    { Classes = [ "a", EquivalenceClass.Create([ a; b ] |> Set.ofList, None) ] |> Map.ofList
      Variables = Map.ofList [ a, "a"; b, "a" ] }

  match actual with
  | Sum.Left((), Some(actual)) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"
  | _ -> ()

[<Test>]
let ``LangNext-Unify unifies type values inside type lambdas`` () =
  let a1 = TypeParameter.Create("a1", Kind.Star)
  let b1 = TypeParameter.Create("b1", Kind.Star)

  let inputs =
    TypeValue.Lambda(a1, TypeExpr.Arrow(TypeExpr.Lookup !a1.Name, TypeExpr.Primitive PrimitiveType.String)),
    TypeValue.Lambda(b1, TypeExpr.Arrow(TypeExpr.Lookup !b1.Name, TypeExpr.Primitive PrimitiveType.String))

  let actual =
    (TypeValue.Unify(inputs).run (UnificationContext.Empty, EquivalenceClasses.Empty))

  let expected: EquivalenceClasses<TypeVar, TypeValue> =
    { Classes = Map.empty
      Variables = Map.empty }

  match actual with
  | Sum.Left((), Some(actual)) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"
  | _ -> ()

[<Test>]
let ``LangNext-Unify unifies type values inside curried type lambdas`` () =
  let a1 = TypeParameter.Create("a1", Kind.Star)
  let a2 = TypeParameter.Create("a2", Kind.Star)
  let b1 = TypeParameter.Create("b1", Kind.Star)
  let b2 = TypeParameter.Create("b2", Kind.Star)

  let inputs =
    TypeValue.Lambda(
      a1,
      TypeExpr.Lambda(
        a2,
        TypeExpr.Arrow(
          TypeExpr.Tuple([ TypeExpr.Lookup !a1.Name; TypeExpr.Lookup !a2.Name ]),
          TypeExpr.Primitive PrimitiveType.String
        )
      )
    ),
    TypeValue.Lambda(
      b1,
      TypeExpr.Lambda(
        b2,
        TypeExpr.Arrow(
          TypeExpr.Tuple([ TypeExpr.Lookup !b1.Name; TypeExpr.Lookup !b2.Name ]),
          TypeExpr.Primitive PrimitiveType.String
        )
      )
    )

  let actual =
    TypeValue.Unify(inputs).run (UnificationContext.Empty, EquivalenceClasses.Empty)

  let expected: EquivalenceClasses<TypeVar, TypeValue> =
    { Classes = Map.empty
      Variables = Map.empty }

  match actual with
  | Sum.Left((), Some(actual)) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"
  | _ -> ()

[<Test>]
let ``LangNext-Unify fails to unify incompatible type values inside type lambdas`` () =
  let a = TypeParameter.Create("a", Kind.Star)
  let b = TypeParameter.Create("b", Kind.Star)

  let inputs =
    TypeValue.Lambda(a, TypeExpr.Arrow(TypeExpr.Lookup !a.Name, TypeExpr.Primitive PrimitiveType.String)),
    TypeValue.Lambda(b, TypeExpr.Arrow(TypeExpr.Lookup !b.Name, TypeExpr.Primitive PrimitiveType.Int32))

  let actual =
    (TypeValue.Unify(inputs).run (UnificationContext.Empty, EquivalenceClasses.Empty))

  match actual with
  | Sum.Right _ -> Assert.Pass()
  | Sum.Left((), result) -> Assert.Fail $"Expected unification to fail but it succeeded with {result}"


[<Test>]
let ``LangNext-Unify fails to unify incompatible params of type lambdas`` () =
  let a = TypeParameter.Create("a", Kind.Star)
  let b = TypeParameter.Create("b", Kind.Symbol)

  let inputs =
    TypeValue.Lambda(a, TypeExpr.Arrow(TypeExpr.Lookup !a.Name, TypeExpr.Primitive PrimitiveType.String)),
    TypeValue.Lambda(b, TypeExpr.Arrow(TypeExpr.Lookup !b.Name, TypeExpr.Primitive PrimitiveType.String))

  let actual =
    (TypeValue.Unify(inputs).run (UnificationContext.Empty, EquivalenceClasses.Empty))

  match actual with
  | Sum.Right _ -> Assert.Pass()
  | Sum.Left((), result) -> Assert.Fail $"Expected unification to fail but it succeeded with {result}"

[<Test>]
let ``LangNext-Unify fails to unify type expressions inside type lambdas`` () =
  let a = TypeParameter.Create("a", Kind.Star)
  let b = TypeParameter.Create("b", Kind.Star)

  let inputs =
    TypeValue.Lambda(a, TypeExpr.Exclude(TypeExpr.Lookup !a.Name, TypeExpr.Primitive PrimitiveType.String)),
    TypeValue.Lambda(b, TypeExpr.Exclude(TypeExpr.Lookup !b.Name, TypeExpr.Primitive PrimitiveType.Int32))

  let actual =
    (TypeValue.Unify(inputs).run (UnificationContext.Empty, EquivalenceClasses.Empty))

  match actual with
  | Sum.Right _ -> Assert.Pass()
  | Sum.Left((), result) -> Assert.Fail $"Expected unification to fail but it succeeded with {result}"

[<Test>]
let ``LangNext-Unify unifies structurally and symbolically identical records and unions`` () =
  let s1 = TypeSymbol.Create "s1"
  let s2 = TypeSymbol.Create "s2"

  let inputs1 =
    TypeValue.Record(
      [ s1, TypeValue.Primitive PrimitiveType.String
        s2, TypeValue.Primitive PrimitiveType.Int32 ]
      |> Map.ofList
    ),
    TypeValue.Record(
      [ s1, TypeValue.Primitive PrimitiveType.String
        s2, TypeValue.Primitive PrimitiveType.Int32 ]
      |> Map.ofList
    )

  let actual1 =
    (TypeValue.Unify(inputs1).run (UnificationContext.Empty, EquivalenceClasses.Empty))

  let inputs2 =
    TypeValue.Union(
      [ s1, TypeValue.Primitive PrimitiveType.String
        s2, TypeValue.Primitive PrimitiveType.Int32 ]
      |> Map.ofList
    ),
    TypeValue.Union(
      [ s1, TypeValue.Primitive PrimitiveType.String
        s2, TypeValue.Primitive PrimitiveType.Int32 ]
      |> Map.ofList
    )

  let actual2 =
    (TypeValue.Unify(inputs2).run (UnificationContext.Empty, EquivalenceClasses.Empty))

  match actual1, actual2 with
  | Sum.Left((), None), Sum.Left((), None) -> Assert.Pass()
  | Sum.Right err, _ -> Assert.Fail $"Expected success but got error: {err}"
  | _, Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"
  | res -> Assert.Fail $"Expected success but got : {res}"

[<Test>]
let ``LangNext-Unify does not unify structurally different records and unions`` () =
  let s1 = TypeSymbol.Create "s1"
  let s2 = TypeSymbol.Create "s2"

  let inputs1 =
    TypeValue.Record(
      [ s1, TypeValue.Primitive PrimitiveType.Int32
        s2, TypeValue.Primitive PrimitiveType.Int32 ]
      |> Map.ofList
    ),
    TypeValue.Record(
      [ s1, TypeValue.Primitive PrimitiveType.String
        s2, TypeValue.Primitive PrimitiveType.Int32 ]
      |> Map.ofList
    )

  let actual1 =
    (TypeValue.Unify(inputs1).run (UnificationContext.Empty, EquivalenceClasses.Empty))

  let inputs2 =
    TypeValue.Union(
      [ s1, TypeValue.Primitive PrimitiveType.String
        s2, TypeValue.Primitive PrimitiveType.Decimal ]
      |> Map.ofList
    ),
    TypeValue.Union(
      [ s1, TypeValue.Primitive PrimitiveType.String
        s2, TypeValue.Primitive PrimitiveType.Int32 ]
      |> Map.ofList
    )

  let actual2 =
    (TypeValue.Unify(inputs2).run (UnificationContext.Empty, EquivalenceClasses.Empty))

  match actual1, actual2 with
  | Sum.Right _, Sum.Right _ -> Assert.Pass()
  | res -> Assert.Fail $"Expected failure but got : {res}"

[<Test>]
let ``LangNext-Unify does not unify structurally identical but symbolically different records and unions`` () =

  let inputs1 =
    TypeValue.Record(
      [ TypeSymbol.Create "s1", TypeValue.Primitive PrimitiveType.String
        TypeSymbol.Create "s2", TypeValue.Primitive PrimitiveType.Int32 ]
      |> Map.ofList
    ),
    TypeValue.Record(
      [ TypeSymbol.Create "s1", TypeValue.Primitive PrimitiveType.String
        TypeSymbol.Create "s2", TypeValue.Primitive PrimitiveType.Int32 ]
      |> Map.ofList
    )

  let actual1 =
    (TypeValue.Unify(inputs1).run (UnificationContext.Empty, EquivalenceClasses.Empty))

  let inputs2 =
    TypeValue.Union(
      [ TypeSymbol.Create "s1", TypeValue.Primitive PrimitiveType.String
        TypeSymbol.Create "s2", TypeValue.Primitive PrimitiveType.Int32 ]
      |> Map.ofList
    ),
    TypeValue.Union(
      [ TypeSymbol.Create "s1", TypeValue.Primitive PrimitiveType.String
        TypeSymbol.Create "s2", TypeValue.Primitive PrimitiveType.Int32 ]
      |> Map.ofList
    )

  let actual2 =
    (TypeValue.Unify(inputs2).run (UnificationContext.Empty, EquivalenceClasses.Empty))

  match actual1, actual2 with
  | Sum.Right _, Sum.Right _ -> Assert.Pass()
  | res -> Assert.Fail $"Expected failure but got : {res}"

[<Test>]
let ``LangNext-Unify unifies structurally and symbolically identical tuples and sums`` () =
  let inputs1 =
    TypeValue.Tuple(
      [ TypeValue.Primitive PrimitiveType.String
        TypeValue.Primitive PrimitiveType.Int32 ]
    ),
    TypeValue.Tuple(
      [ TypeValue.Primitive PrimitiveType.String
        TypeValue.Primitive PrimitiveType.Int32 ]
    )

  let actual1 =
    (TypeValue.Unify(inputs1).run (UnificationContext.Empty, EquivalenceClasses.Empty))

  let inputs2 =
    TypeValue.Sum(
      [ TypeValue.Primitive PrimitiveType.String
        TypeValue.Primitive PrimitiveType.Int32 ]
    ),
    TypeValue.Sum(
      [ TypeValue.Primitive PrimitiveType.String
        TypeValue.Primitive PrimitiveType.Int32 ]
    )

  let actual2 =
    (TypeValue.Unify(inputs2).run (UnificationContext.Empty, EquivalenceClasses.Empty))

  match actual1, actual2 with
  | Sum.Left((), None), Sum.Left((), None) -> Assert.Pass()
  | Sum.Right err, _ -> Assert.Fail $"Expected success but got error: {err}"
  | _, Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"
  | res -> Assert.Fail $"Expected success but got : {res}"

[<Test>]
let ``LangNext-Unify does not unify structurally different tuples and sums`` () =
  let inputs1 =
    TypeValue.Tuple(
      [ TypeValue.Primitive PrimitiveType.String
        TypeValue.Primitive PrimitiveType.Decimal ]
    ),
    TypeValue.Tuple(
      [ TypeValue.Primitive PrimitiveType.String
        TypeValue.Primitive PrimitiveType.Int32 ]
    )

  let actual1 =
    (TypeValue.Unify(inputs1).run (UnificationContext.Empty, EquivalenceClasses.Empty))

  let inputs2 =
    TypeValue.Sum(
      [ TypeValue.Primitive PrimitiveType.String
        TypeValue.Primitive PrimitiveType.Decimal ]
    ),
    TypeValue.Sum(
      [ TypeValue.Primitive PrimitiveType.String
        TypeValue.Primitive PrimitiveType.Int32 ]
    )

  let actual2 =
    (TypeValue.Unify(inputs2).run (UnificationContext.Empty, EquivalenceClasses.Empty))

  match actual1, actual2 with
  | Sum.Right _, Sum.Right _ -> Assert.Pass()
  | res -> Assert.Fail $"Expected failure but got : {res}"

[<Test>]
let ``LangNext-Unify unifies can look lookups up`` () =
  let inputs =
    TypeValue.Tuple(
      [ TypeValue.Primitive PrimitiveType.String
        TypeValue.Primitive PrimitiveType.Decimal ]
    ),
    TypeValue.Lookup(!"T1")

  let actual =
    (TypeValue
      .Unify(inputs)
      .run (UnificationContext.Create([ !"T1", inputs |> fst ] |> Map.ofList, Map.empty), EquivalenceClasses.Empty))

  match actual with
  | Sum.Left _ -> Assert.Pass()
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-Unify unifies can look lookups up and fail on structure`` () =
  let inputs =
    TypeValue.Tuple(
      [ TypeValue.Primitive PrimitiveType.String
        TypeValue.Primitive PrimitiveType.Decimal ]
    ),
    TypeValue.Lookup(!"T1")

  let actual =
    (TypeValue
      .Unify(inputs)
      .run (
        UnificationContext.Create([ !"T1", TypeValue.Primitive PrimitiveType.Decimal ] |> Map.ofList, Map.empty),
        EquivalenceClasses.Empty
      ))

  match actual with
  | Sum.Left res -> Assert.Fail $"Expected failure but got error: {res}"
  | Sum.Right _ -> Assert.Pass()

[<Test>]
let ``LangNext-Unify unifies can look lookups up and fail on missing identifier`` () =
  let inputs =
    TypeValue.Tuple(
      [ TypeValue.Primitive PrimitiveType.String
        TypeValue.Primitive PrimitiveType.Decimal ]
    ),
    TypeValue.Lookup(!"T1")

  let actual =
    (TypeValue
      .Unify(inputs)
      .run (UnificationContext.Create([ !"T2", inputs |> fst ] |> Map.ofList, Map.empty), EquivalenceClasses.Empty))

  match actual with
  | Sum.Left res -> Assert.Fail $"Expected failure but got error: {res}"
  | Sum.Right _ -> Assert.Pass()


[<Test>]
let ``LangNext-Unify unifies fails on different constructors`` () =
  let a = TypeVar.Create("a")
  let b = TypeVar.Create("b")

  let program: State<unit, UnificationContext, EquivalenceClasses<TypeVar, TypeValue>, Errors> =
    state {
      do! EquivalenceClasses.Bind(b, PrimitiveType.Int32 |> TypeValue.Primitive |> Right)
      do! EquivalenceClasses.Bind(a, b |> TypeValue.Var |> TypeValue.Set |> Right)
      do! EquivalenceClasses.Bind(a, b |> TypeValue.Var |> TypeValue.List |> Right)
    }
    |> TypeValue.EquivalenceClassesOp

  let actual = program.run (UnificationContext.Empty, EquivalenceClasses.Empty)

  match actual with
  | Sum.Left res -> Assert.Fail $"Expected failure but got error: {res}"
  | Sum.Right _ -> Assert.Pass()

[<Test>]
let ``LangNext-Unify unifies fails on different transitively unified generic arguments`` () =
  let a = TypeVar.Create("a")
  let b = TypeVar.Create("b")
  let c = TypeVar.Create("c")

  let program: State<unit, UnificationContext, EquivalenceClasses<TypeVar, TypeValue>, Errors> =
    state {
      do! EquivalenceClasses.Bind(b, PrimitiveType.Int32 |> TypeValue.Primitive |> Right)
      do! EquivalenceClasses.Bind(c, PrimitiveType.String |> TypeValue.Primitive |> Right)
      do! EquivalenceClasses.Bind(a, b |> TypeValue.Var |> TypeValue.List |> Right)
      do! EquivalenceClasses.Bind(a, c |> TypeValue.Var |> TypeValue.List |> Right)
    }
    |> TypeValue.EquivalenceClassesOp

  let actual = program.run (UnificationContext.Empty, EquivalenceClasses.Empty)

  match actual with
  | Sum.Left res -> Assert.Fail $"Expected failure but got error: {res}"
  | Sum.Right _ -> Assert.Pass()


[<Test>]
let ``LangNext-Unify unifies fails on different transitively unified generic arguments pointing to primitives and constructors``
  ()
  =

  let a = TypeVar.Create("a")
  let b = TypeVar.Create("b")
  let c = TypeVar.Create("c")

  let program: State<unit, UnificationContext, EquivalenceClasses<TypeVar, TypeValue>, Errors> =
    state {
      do! EquivalenceClasses.Bind(c, PrimitiveType.Int32 |> TypeValue.Primitive |> Right)
      do! EquivalenceClasses.Bind(b, c |> Left)
      do! EquivalenceClasses.Bind(a, b |> TypeValue.Var |> TypeValue.List |> Right)
      do! EquivalenceClasses.Bind(a, c |> Left)
    }
    |> TypeValue.EquivalenceClassesOp

  let actual = program.run (UnificationContext.Empty, EquivalenceClasses.Empty)

  match actual with
  | Sum.Left res -> Assert.Fail $"Expected failure but got error: {res}"
  | Sum.Right _ -> Assert.Pass()

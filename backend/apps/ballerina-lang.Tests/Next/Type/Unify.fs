module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Type.Unify

open System
open Ballerina.Collections.Sum
open Ballerina.DSL.Next.StdLib.Extensions
open NUnit.Framework
open Ballerina.LocalizedErrors
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.DSL.Next.EquivalenceClasses
open Ballerina.DSL.Next.Unification
open Ballerina.State.WithError
open Ballerina.Cat.Collections.OrderedMap
open Ballerina.DSL.Next.Types.TypeChecker.Model
open Ballerina.DSL.Next.Types.TypeChecker.Patterns

let private initialClasses = EquivalenceClasses<string, PrimitiveType>.Empty
let private (!) = Identifier.LocalScope

let private valueOperations =
  { tryCompare = fun (v1: PrimitiveType, v2: PrimitiveType) -> if v1 = v2 then Some v1 else None
    equalize =
      (fun (v1, v2) ->
        if v1 <> v2 then
          (Location.Unknown, $"Error: cannot unify {v1} and {v2}")
          |> Errors.Singleton
          |> state.Throw
        else
          state { return () }) }

[<Test>]
let ``LangNext-Unify binding trivial equivalence classes over primitives succeeds`` () =

  let program: State<unit, _, EquivalenceClasses<string, PrimitiveType>, Errors> =
    state {
      do! EquivalenceClasses.Bind("v1", PrimitiveType.Int32 |> Right, Location.Unknown)
      do! EquivalenceClasses.Bind("v2", PrimitiveType.String |> Right, Location.Unknown)
      do! EquivalenceClasses.Bind("v3", PrimitiveType.Decimal |> Right, Location.Unknown)
      do! EquivalenceClasses.Bind("v4", PrimitiveType.Decimal |> Right, Location.Unknown)
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
      do! EquivalenceClasses.Bind("v1", PrimitiveType.Int32 |> Right, Location.Unknown)
      do! EquivalenceClasses.Bind("v2", PrimitiveType.String |> Right, Location.Unknown)
      do! EquivalenceClasses.Bind("v3", PrimitiveType.Decimal |> Right, Location.Unknown)
      do! EquivalenceClasses.Bind("v4", PrimitiveType.Decimal |> Right, Location.Unknown)
      do! EquivalenceClasses.Bind("v4", "v3" |> Left, Location.Unknown)
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
      do! EquivalenceClasses.Bind("v1", PrimitiveType.Int32 |> Right, Location.Unknown)
      do! EquivalenceClasses.Bind("v2", PrimitiveType.String |> Right, Location.Unknown)
      do! EquivalenceClasses.Bind("v3", PrimitiveType.Decimal |> Right, Location.Unknown)
      do! EquivalenceClasses.Bind("v4", PrimitiveType.Decimal |> Right, Location.Unknown)
      do! EquivalenceClasses.Bind("v4", "v1" |> Left, Location.Unknown)
    }

  let actual = program.run (valueOperations, initialClasses)

  match actual with
  | Sum.Right _ -> Assert.Pass()
  | Sum.Left((), result) -> Assert.Fail $"Expected unification to fail but it succeeded with {result}"


[<Test>]
let ``LangNext-Unify binding equivalence classes over variables and primitives or variables in a chain succeeds`` () =
  let program: State<unit, _, EquivalenceClasses<string, PrimitiveType>, Errors> =
    state {
      do! EquivalenceClasses.Bind("v1", PrimitiveType.Int32 |> Right, Location.Unknown)
      do! EquivalenceClasses.Bind("v2", PrimitiveType.String |> Right, Location.Unknown)
      do! EquivalenceClasses.Bind("v3", PrimitiveType.Decimal |> Right, Location.Unknown)
      do! EquivalenceClasses.Bind("v4", PrimitiveType.Decimal |> Right, Location.Unknown)
      do! EquivalenceClasses.Bind("v5", PrimitiveType.Decimal |> Right, Location.Unknown)
      do! EquivalenceClasses.Bind("v6", PrimitiveType.Decimal |> Right, Location.Unknown)
      do! EquivalenceClasses.Bind("v4", "v3" |> Left, Location.Unknown)
      do! EquivalenceClasses.Bind("v6", "v5" |> Left, Location.Unknown)
      do! EquivalenceClasses.Bind("v6", "v3" |> Left, Location.Unknown)
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
    [ TypeValue.CreateInt32()
      TypeValue.CreateArrow(TypeValue.CreateInt32(), TypeValue.CreateString())
      TypeValue.CreateSet(TypeValue.CreateInt32())
      TypeValue.CreateMap(TypeValue.CreateInt32(), TypeValue.CreateString())
      TypeValue.CreateTuple([ TypeValue.CreateInt32(); TypeValue.CreateString() ])
      TypeValue.CreateSum([ TypeValue.CreateInt32(); TypeValue.CreateString() ])
      TypeValue.CreateRecord(
        [ "a" |> Identifier.LocalScope |> TypeSymbol.Create, (TypeValue.CreateInt32(), Kind.Star)
          "b" |> Identifier.LocalScope |> TypeSymbol.Create, (TypeValue.CreateString(), Kind.Star) ]
        |> OrderedMap.ofList
      )
      TypeValue.CreateUnion(
        [ "a" |> Identifier.LocalScope |> TypeSymbol.Create, (TypeValue.CreateInt32())
          "b" |> Identifier.LocalScope |> TypeSymbol.Create, (TypeValue.CreateString()) ]
        |> OrderedMap.ofList
      ) ]

  let actual =
    inputs
    |> List.map (fun input ->
      TypeValue.Unify(Location.Unknown, input, input).run (UnificationContext.Empty, UnificationState.Empty))

  let expected: EquivalenceClasses<TypeVar, TypeValue<ValueExt>> =
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
    Location.Unknown,
    TypeValue.CreateArrow(TypeValue.Var(a), TypeValue.CreateString()),
    TypeValue.CreateArrow(TypeValue.CreateString(), TypeValue.Var(b))

  let actual =
    (TypeValue.Unify(inputs).run (UnificationContext.Empty, UnificationState.Empty))

  let expected: EquivalenceClasses<TypeVar, TypeValue<IComparable>> =
    { Classes =
        [ "a", EquivalenceClass.Create(a |> Set.singleton, TypeValue.CreateString() |> Some)
          "b", EquivalenceClass.Create(b |> Set.singleton, TypeValue.CreateString() |> Some) ]
        |> Map.ofList
      Variables = Map.ofList [ a, "a"; b, "b" ] }

  match actual with
  | Sum.Left((), Some(actual)) -> Assert.That(actual.Classes, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"
  | _ -> ()

[<Test>]
let ``LangNext-Unify unifies lists of tuples`` () =
  let a = TypeVar.Create "a"
  let b = TypeVar.Create "b"

  let inputs =
    Location.Unknown,
    TypeValue.CreateSet(TypeValue.CreateTuple([ TypeValue.Var(a); TypeValue.CreateString() ])),
    TypeValue.CreateSet(TypeValue.CreateTuple([ TypeValue.Var(b); TypeValue.CreateString() ]))

  let actual =
    (TypeValue.Unify(inputs).run (UnificationContext.Empty, UnificationState.Empty))

  let expected: EquivalenceClasses<TypeVar, TypeValue<IComparable>> =
    { Classes = [ "a", EquivalenceClass.Create([ a; b ] |> Set.ofList, None) ] |> Map.ofList
      Variables = Map.ofList [ a, "a"; b, "a" ] }

  match actual with
  | Sum.Left((), Some(actual)) -> Assert.That(actual.Classes, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"
  | _ -> ()

[<Test>]
let ``LangNext-Unify unifies type values inside type lambdas`` () =
  let a1 = TypeParameter.Create("a1", Kind.Star)
  let b1 = TypeParameter.Create("b1", Kind.Star)

  let inputs =
    Location.Unknown,
    TypeValue.CreateLambda(a1, TypeExpr.Arrow(TypeExpr.Lookup !a1.Name, TypeExpr.Primitive PrimitiveType.String)),
    TypeValue.CreateLambda(b1, TypeExpr.Arrow(TypeExpr.Lookup !b1.Name, TypeExpr.Primitive PrimitiveType.String))

  let actual =
    (TypeValue.Unify(inputs).run (UnificationContext.Empty, UnificationState.Empty))

  let expected: EquivalenceClasses<TypeVar, TypeValue<IComparable>> =
    { Classes = Map.empty
      Variables = Map.empty }

  match actual with
  | Sum.Left((), Some(actual)) -> Assert.That(actual.Classes, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"
  | _ -> ()

[<Test>]
let ``LangNext-Unify unifies type values inside curried type lambdas`` () =
  let a1 = TypeParameter.Create("a1", Kind.Star)
  let a2 = TypeParameter.Create("a2", Kind.Star)
  let b1 = TypeParameter.Create("b1", Kind.Star)
  let b2 = TypeParameter.Create("b2", Kind.Star)

  let inputs =
    Location.Unknown,
    TypeValue.CreateLambda(
      a1,
      TypeExpr.Lambda(
        a2,
        TypeExpr.Arrow(
          TypeExpr.Tuple([ TypeExpr.Lookup !a1.Name; TypeExpr.Lookup !a2.Name ]),
          TypeExpr.Primitive PrimitiveType.String
        )
      )
    ),
    TypeValue.CreateLambda(
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
    TypeValue.Unify(inputs).run (UnificationContext.Empty, UnificationState.Empty)

  let expected: EquivalenceClasses<TypeVar, TypeValue<IComparable>> =
    { Classes = Map.empty
      Variables = Map.empty }

  match actual with
  | Sum.Left((), Some(actual)) -> Assert.That(actual.Classes, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"
  | _ -> ()

[<Test>]
let ``LangNext-Unify fails to unify incompatible type values inside type lambdas`` () =
  let a = TypeParameter.Create("a", Kind.Star)
  let b = TypeParameter.Create("b", Kind.Star)

  let inputs =
    Location.Unknown,
    TypeValue.CreateLambda(a, TypeExpr.Arrow(TypeExpr.Lookup !a.Name, TypeExpr.Primitive PrimitiveType.String)),
    TypeValue.CreateLambda(b, TypeExpr.Arrow(TypeExpr.Lookup !b.Name, TypeExpr.Primitive PrimitiveType.Int32))

  let actual =
    (TypeValue.Unify(inputs).run (UnificationContext.Empty, UnificationState.Empty))

  match actual with
  | Sum.Right _ -> Assert.Pass()
  | Sum.Left((), result) -> Assert.Fail $"Expected unification to fail but it succeeded with {result}"


[<Test>]
let ``LangNext-Unify fails to unify incompatible params of type lambdas`` () =
  let a = TypeParameter.Create("a", Kind.Star)
  let b = TypeParameter.Create("b", Kind.Symbol)

  let inputs =
    Location.Unknown,
    TypeValue.CreateLambda(a, TypeExpr.Arrow(TypeExpr.Lookup !a.Name, TypeExpr.Primitive PrimitiveType.String)),
    TypeValue.CreateLambda(b, TypeExpr.Arrow(TypeExpr.Lookup !b.Name, TypeExpr.Primitive PrimitiveType.String))

  let actual =
    (TypeValue.Unify(inputs).run (UnificationContext.Empty, UnificationState.Empty))

  match actual with
  | Sum.Right _ -> Assert.Pass()
  | Sum.Left((), result) -> Assert.Fail $"Expected unification to fail but it succeeded with {result}"

[<Test>]
let ``LangNext-Unify fails to unify type expressions inside type lambdas`` () =
  let a = TypeParameter.Create("a", Kind.Star)
  let b = TypeParameter.Create("b", Kind.Star)

  let inputs =
    Location.Unknown,
    TypeValue.CreateLambda(a, TypeExpr.Exclude(TypeExpr.Lookup !a.Name, TypeExpr.Primitive PrimitiveType.String)),
    TypeValue.CreateLambda(b, TypeExpr.Exclude(TypeExpr.Lookup !b.Name, TypeExpr.Primitive PrimitiveType.Int32))

  let actual =
    (TypeValue.Unify(inputs).run (UnificationContext.Empty, UnificationState.Empty))

  match actual with
  | Sum.Right _ -> Assert.Pass()
  | Sum.Left((), result) -> Assert.Fail $"Expected unification to fail but it succeeded with {result}"

[<Test>]
let ``LangNext-Unify unifies structurally and symbolically identical records and unions`` () =
  let s1 = "s1" |> Identifier.LocalScope |> TypeSymbol.Create
  let s2 = "s2" |> Identifier.LocalScope |> TypeSymbol.Create

  let inputs1 =
    Location.Unknown,

    TypeValue.CreateRecord(
      [ s1, (TypeValue.CreateString(), Kind.Star)
        s2, (TypeValue.CreateInt32(), Kind.Star) ]
      |> OrderedMap.ofList
    ),
    TypeValue.CreateRecord(
      [ s1, (TypeValue.CreateString(), Kind.Star)
        s2, (TypeValue.CreateInt32(), Kind.Star) ]
      |> OrderedMap.ofList
    )

  let actual1 =
    (TypeValue.Unify(inputs1).run (UnificationContext.Empty, UnificationState.Empty))

  let inputs2 =
    Location.Unknown,
    TypeValue.CreateUnion(
      [ s1, TypeValue.CreateString(); s2, TypeValue.CreateInt32() ]
      |> OrderedMap.ofList
    ),
    TypeValue.CreateUnion(
      [ s1, TypeValue.CreateString(); s2, TypeValue.CreateInt32() ]
      |> OrderedMap.ofList
    )

  let actual2 =
    (TypeValue.Unify(inputs2).run (UnificationContext.Empty, UnificationState.Empty))

  match actual1, actual2 with
  | Sum.Left((), None), Sum.Left((), None) -> Assert.Pass()
  | Sum.Right err, _ -> Assert.Fail $"Expected success but got error: {err}"
  | _, Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"
  | res -> Assert.Fail $"Expected success but got : {res}"

[<Test>]
let ``LangNext-Unify does not unify structurally different records and unions`` () =
  let s1 = "s1" |> Identifier.LocalScope |> TypeSymbol.Create
  let s2 = "s2" |> Identifier.LocalScope |> TypeSymbol.Create

  let inputs1 =
    Location.Unknown,
    TypeValue.CreateRecord(
      [ s1, (TypeValue.CreateInt32(), Kind.Star)
        s2, (TypeValue.CreateInt32(), Kind.Star) ]
      |> OrderedMap.ofList
    ),
    TypeValue.CreateRecord(
      [ s1, (TypeValue.CreateString(), Kind.Star)
        s2, (TypeValue.CreateInt32(), Kind.Star) ]
      |> OrderedMap.ofList
    )

  let actual1 =
    (TypeValue.Unify(inputs1).run (UnificationContext.Empty, UnificationState.Empty))

  let inputs2 =
    Location.Unknown,
    TypeValue.CreateUnion(
      [ s1, TypeValue.CreateString(); s2, TypeValue.CreateDecimal() ]
      |> OrderedMap.ofList
    ),
    TypeValue.CreateUnion(
      [ s1, TypeValue.CreateString(); s2, TypeValue.CreateInt32() ]
      |> OrderedMap.ofList
    )

  let actual2 =
    (TypeValue.Unify(inputs2).run (UnificationContext.Empty, UnificationState.Empty))

  match actual1, actual2 with
  | Sum.Right _, Sum.Right _ -> Assert.Pass()
  | res -> Assert.Fail $"Expected failure but got : {res}"

[<Test>]
let ``LangNext-Unify does not unify structurally identical but symbolically different records and unions`` () =

  let inputs1 =
    Location.Unknown,
    TypeValue.CreateRecord(
      [ "s1" |> Identifier.LocalScope |> TypeSymbol.Create, (TypeValue.CreateString(), Kind.Star)
        "s2" |> Identifier.LocalScope |> TypeSymbol.Create, (TypeValue.CreateInt32(), Kind.Star) ]
      |> OrderedMap.ofList
    ),
    TypeValue.CreateRecord(
      [ "s1" |> Identifier.LocalScope |> TypeSymbol.Create, (TypeValue.CreateString(), Kind.Star)
        "s2" |> Identifier.LocalScope |> TypeSymbol.Create, (TypeValue.CreateInt32(), Kind.Star) ]
      |> OrderedMap.ofList
    )

  let actual1 =
    (TypeValue.Unify(inputs1).run (UnificationContext.Empty, UnificationState.Empty))

  let inputs2 =
    Location.Unknown,
    TypeValue.CreateUnion(
      [ "s1" |> Identifier.LocalScope |> TypeSymbol.Create, TypeValue.CreateString()
        "s2" |> Identifier.LocalScope |> TypeSymbol.Create, TypeValue.CreateInt32() ]
      |> OrderedMap.ofList
    ),
    TypeValue.CreateUnion(
      [ "s1" |> Identifier.LocalScope |> TypeSymbol.Create, TypeValue.CreateString()
        "s2" |> Identifier.LocalScope |> TypeSymbol.Create, TypeValue.CreateInt32() ]
      |> OrderedMap.ofList
    )

  let actual2 =
    (TypeValue.Unify(inputs2).run (UnificationContext.Empty, UnificationState.Empty))

  match actual1, actual2 with
  | Sum.Right _, Sum.Right _ -> Assert.Pass()
  | res -> Assert.Fail $"Expected failure but got : {res}"

[<Test>]
let ``LangNext-Unify unifies structurally and symbolically identical tuples and sums`` () =
  let inputs1 =
    Location.Unknown,
    TypeValue.CreateTuple([ TypeValue.CreateString(); TypeValue.CreateInt32() ]),
    TypeValue.CreateTuple([ TypeValue.CreateString(); TypeValue.CreateInt32() ])

  let actual1 =
    (TypeValue.Unify(inputs1).run (UnificationContext.Empty, UnificationState.Empty))

  let inputs2 =
    Location.Unknown,
    TypeValue.CreateSum([ TypeValue.CreateString(); TypeValue.CreateInt32() ]),
    TypeValue.CreateSum([ TypeValue.CreateString(); TypeValue.CreateInt32() ])

  let actual2 =
    (TypeValue.Unify(inputs2).run (UnificationContext.Empty, UnificationState.Empty))

  match actual1, actual2 with
  | Sum.Left((), None), Sum.Left((), None) -> Assert.Pass()
  | Sum.Right err, _ -> Assert.Fail $"Expected success but got error: {err}"
  | _, Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"
  | res -> Assert.Fail $"Expected success but got : {res}"

[<Test>]
let ``LangNext-Unify does not unify structurally different tuples and sums`` () =
  let inputs1 =
    Location.Unknown,
    TypeValue.CreateTuple([ TypeValue.CreateString(); TypeValue.CreateDecimal() ]),
    TypeValue.CreateTuple([ TypeValue.CreateString(); TypeValue.CreateInt32() ])

  let actual1 =
    (TypeValue.Unify(inputs1).run (UnificationContext.Empty, UnificationState.Empty))

  let inputs2 =
    Location.Unknown,
    TypeValue.CreateSum([ TypeValue.CreateString(); TypeValue.CreateDecimal() ]),
    TypeValue.CreateSum([ TypeValue.CreateString(); TypeValue.CreateInt32() ])

  let actual2 =
    (TypeValue.Unify(inputs2).run (UnificationContext.Empty, UnificationState.Empty))

  match actual1, actual2 with
  | Sum.Right _, Sum.Right _ -> Assert.Pass()
  | res -> Assert.Fail $"Expected failure but got : {res}"

[<Test>]
let ``LangNext-Unify unifies can look lookups up`` () =
  let input1, input2 =
    TypeValue.CreateTuple([ TypeValue.CreateString(); TypeValue.CreateDecimal() ]), TypeValue.Lookup(!"T1")

  let actual =
    (TypeValue
      .Unify(Location.Unknown, input1, input2)
      .run (
        UnificationContext.Create(
          [ !"T1" |> TypeCheckScope.Empty.Resolve, (input1, Kind.Star) ] |> Map.ofList,
          Ballerina.DSL.Next.Types.TypeChecker.Model.TypeExprEvalSymbols.Empty
        ),
        UnificationState.Empty
      ))

  match actual with
  | Sum.Left _ -> Assert.Pass()
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-Unify unifies can look lookups up and fail on structure`` () =
  let inputs =
    Location.Unknown,
    TypeValue.CreateTuple([ TypeValue.CreateString(); TypeValue.CreateDecimal() ]),
    TypeValue.Lookup(!"T1")

  let actual =
    (TypeValue
      .Unify(inputs)
      .run (
        UnificationContext.Create(
          [ !"T1" |> TypeCheckScope.Empty.Resolve, (TypeValue.CreateDecimal(), Kind.Star) ]
          |> Map.ofList,
          Ballerina.DSL.Next.Types.TypeChecker.Model.TypeExprEvalSymbols.Empty
        ),
        UnificationState.Empty
      ))

  match actual with
  | Sum.Left res -> Assert.Fail $"Expected failure but got error: {res}"
  | Sum.Right _ -> Assert.Pass()

[<Test>]
let ``LangNext-Unify unifies can look lookups up and fail on missing identifier`` () =
  let input1, input2 =
    TypeValue.CreateTuple([ TypeValue.CreateString(); TypeValue.CreateDecimal() ]), TypeValue.Lookup(!"T1")

  let actual =
    (TypeValue
      .Unify(Location.Unknown, input1, input2)
      .run (
        UnificationContext.Create(
          [ !"T2" |> TypeCheckScope.Empty.Resolve, (input1, Kind.Star) ] |> Map.ofList,
          Ballerina.DSL.Next.Types.TypeChecker.Model.TypeExprEvalSymbols.Empty
        ),
        UnificationState.Empty
      ))

  match actual with
  | Sum.Left res -> Assert.Fail $"Expected failure but got error: {res}"
  | Sum.Right _ -> Assert.Pass()


[<Test>]
let ``LangNext-Unify unifies fails on different transitively unified generic arguments`` () =
  let a = TypeVar.Create("a")
  let b = TypeVar.Create("b")
  let c = TypeVar.Create("c")

  let program: State<unit, UnificationContext<ValueExt>, UnificationState<ValueExt>, Errors> =
    state {
      do! EquivalenceClasses.Bind(b, PrimitiveType.Int32 |> TypeValue.CreatePrimitive |> Right, Location.Unknown)
      do! EquivalenceClasses.Bind(c, PrimitiveType.String |> TypeValue.CreatePrimitive |> Right, Location.Unknown)
      do! EquivalenceClasses.Bind(a, b |> TypeValue.Var |> TypeValue.CreateSet |> Right, Location.Unknown)
      do! EquivalenceClasses.Bind(a, c |> TypeValue.Var |> TypeValue.CreateSet |> Right, Location.Unknown)
    }
    |> State.mapState (fun (s, _) -> s.Classes) (fun (s, _) _ -> UnificationState.Create s)
    |> TypeValue.EquivalenceClassesOp Location.Unknown

  let actual = program.run (UnificationContext.Empty, UnificationState.Empty)

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

  let program: State<unit, UnificationContext<ValueExt>, UnificationState<ValueExt>, Errors> =
    state {
      do! EquivalenceClasses.Bind(c, PrimitiveType.Int32 |> TypeValue.CreatePrimitive |> Right, Location.Unknown)
      do! EquivalenceClasses.Bind(b, c |> Left, Location.Unknown)
      do! EquivalenceClasses.Bind(a, b |> TypeValue.Var |> TypeValue.CreateSet |> Right, Location.Unknown)
      do! EquivalenceClasses.Bind(a, c |> Left, Location.Unknown)
    }
    |> State.mapState (fun (s, _) -> s.Classes) (fun (s, _) _ -> UnificationState.Create s)
    |> TypeValue.EquivalenceClassesOp Location.Unknown

  let actual = program.run (UnificationContext.Empty, UnificationState.Empty)

  match actual with
  | Sum.Left res -> Assert.Fail $"Expected failure but got error: {res}"
  | Sum.Right _ -> Assert.Pass()

module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Type.Eval

open Ballerina.Collections.Sum
open NUnit.Framework
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.DSL.Next.Types.Eval
open Ballerina.DSL.Next.KitchenSink
open Ballerina.State.WithError

[<Test>]
let ``LangNext-TypeEval lookup looks up existing types`` () =
  let t1 = TypeValue.Primitive PrimitiveType.String

  let actual =
    TypeExpr.Lookup(Identifier.LocalScope "T1")
    |> TypeExpr.Eval
    |> State.Run(
      TypeExprEvalContext.Empty,
      TypeExprEvalState.Create([ "T1" |> Identifier.LocalScope, t1 ] |> Map.ofSeq, Map.empty)
    )

  match actual with
  | Sum.Left(actual, _) ->
    match actual with
    | TypeValue.Primitive PrimitiveType.String -> Assert.Pass()
    | _ -> Assert.Fail $"Expected 'string' but got {actual}"
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Flatten of anonymous unions`` () =
  let A = TypeSymbol.Create "A"
  let B = TypeSymbol.Create "B"
  let C = TypeSymbol.Create "C"
  let D = TypeSymbol.Create "D"

  let t1 =
    TypeExpr.Union(
      [ "A" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int
        "B" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String ]
    )

  let t2 =
    TypeExpr.Union(
      [ "C" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Decimal
        "D" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Bool ]
    )

  let actual =
    TypeExpr.Flatten(t1, t2)
    |> TypeExpr.Eval
    |> State.Run(
      TypeExprEvalContext.Empty,
      [ A.Name |> Identifier.LocalScope, A
        B.Name |> Identifier.LocalScope, B
        C.Name |> Identifier.LocalScope, C
        D.Name |> Identifier.LocalScope, D ]
      |> Map.ofList
      |> TypeExprEvalState.CreateFromSymbols
    )

  match actual with
  | Sum.Left(actual, _) ->
    match actual with
    | TypeValue.Union(cases) ->
      match cases |> Map.toList |> List.map (fun (k, v) -> k.Name, v) with
      | [ ("A", TypeValue.Primitive PrimitiveType.Int)
          ("B", TypeValue.Primitive PrimitiveType.String)
          ("C", TypeValue.Primitive PrimitiveType.Decimal)
          ("D", TypeValue.Primitive PrimitiveType.Bool) ] -> Assert.Pass()
      | _ -> Assert.Fail $"Expected flattened union but got {cases}"
    | _ -> Assert.Fail $"Expected union but got {actual}"
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Flatten of named unions`` () =
  let A = TypeSymbol.Create "A"
  let B = TypeSymbol.Create "B"
  let C = TypeSymbol.Create "C"
  let D = TypeSymbol.Create "D"

  let t1 =
    TypeValue.Union(
      Map.ofList
        [ A, TypeValue.Primitive PrimitiveType.Int
          B, TypeValue.Primitive PrimitiveType.String ]
    )

  let t2 =
    TypeValue.Union(
      Map.ofList
        [ C, TypeValue.Primitive PrimitiveType.Decimal
          D, TypeValue.Primitive PrimitiveType.Bool ]
    )

  let actual =
    TypeExpr.Flatten(TypeExpr.Lookup(Identifier.LocalScope "T1"), TypeExpr.Lookup(Identifier.LocalScope "T2"))
    |> TypeExpr.Eval
    |> State.Run(
      TypeExprEvalContext.Empty,
      TypeExprEvalState.Create(
        [ "T1" |> Identifier.LocalScope, t1; "T2" |> Identifier.LocalScope, t2 ]
        |> Map.ofSeq,
        [ A.Name |> Identifier.LocalScope, A
          B.Name |> Identifier.LocalScope, B
          C.Name |> Identifier.LocalScope, C
          D.Name |> Identifier.LocalScope, D ]
        |> Map.ofList
      )
    )

  match actual with
  | Sum.Left(actual, _) ->
    match actual with
    | TypeValue.Union(cases) ->
      match cases |> Map.toList |> List.map (fun (k, v) -> k.Name, v) with
      | [ ("A", TypeValue.Primitive PrimitiveType.Int)
          ("B", TypeValue.Primitive PrimitiveType.String)
          ("C", TypeValue.Primitive PrimitiveType.Decimal)
          ("D", TypeValue.Primitive PrimitiveType.Bool) ] -> Assert.Pass()
      | _ -> Assert.Fail $"Expected flattened union but got {cases}"
    | _ -> Assert.Fail $"Expected union but got {actual}"
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Flatten of named records`` () =
  let A = TypeSymbol.Create "A"
  let B = TypeSymbol.Create "B"
  let C = TypeSymbol.Create "C"
  let D = TypeSymbol.Create "D"

  let t1 =
    TypeValue.Record(
      Map.ofList
        [ A, TypeValue.Primitive PrimitiveType.Int
          B, TypeValue.Primitive PrimitiveType.String ]
    )

  let t2 =
    TypeValue.Record(
      Map.ofList
        [ C, TypeValue.Primitive PrimitiveType.Decimal
          D, TypeValue.Primitive PrimitiveType.Bool ]
    )

  let actual =
    TypeExpr.Flatten(TypeExpr.Lookup(Identifier.LocalScope "T1"), TypeExpr.Lookup(Identifier.LocalScope "T2"))
    |> TypeExpr.Eval
    |> State.Run(
      TypeExprEvalContext.Empty,
      TypeExprEvalState.Create(
        [ "T1" |> Identifier.LocalScope, t1; "T2" |> Identifier.LocalScope, t2 ]
        |> Map.ofSeq,
        [ A.Name |> Identifier.LocalScope, A
          B.Name |> Identifier.LocalScope, B
          C.Name |> Identifier.LocalScope, C
          D.Name |> Identifier.LocalScope, D ]
        |> Map.ofList
      )
    )

  match actual with
  | Sum.Left(actual, _) ->
    match actual with
    | TypeValue.Record(fields) ->
      match fields |> Map.toList |> List.map (fun (k, v) -> k.Name, v) with
      | [ ("A", TypeValue.Primitive PrimitiveType.Int)
          ("B", TypeValue.Primitive PrimitiveType.String)
          ("C", TypeValue.Primitive PrimitiveType.Decimal)
          ("D", TypeValue.Primitive PrimitiveType.Bool) ] -> Assert.Pass()
      | _ -> Assert.Fail $"Expected flattened union but got {fields}"
    | _ -> Assert.Fail $"Expected union but got {actual}"
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Flatten of incompatible types fails`` () =
  let A = TypeSymbol.Create "A"
  let B = TypeSymbol.Create "B"

  let t1 =
    TypeExpr.Union(
      [ TypeExpr.Lookup("A" |> Identifier.LocalScope), TypeExpr.Primitive PrimitiveType.Int
        TypeExpr.Lookup("B" |> Identifier.LocalScope), TypeExpr.Primitive PrimitiveType.String ]
    )

  let t2 = TypeExpr.List(TypeExpr.Primitive PrimitiveType.Decimal)

  let actual =
    TypeExpr.Flatten(t1, t2)
    |> TypeExpr.Eval
    |> State.Run(
      TypeExprEvalContext.Empty,
      [ A.Name |> Identifier.LocalScope, A; B.Name |> Identifier.LocalScope, B ]
      |> Map.ofList
      |> TypeExprEvalState.CreateFromSymbols
    )

  match actual with
  | Sum.Right _err -> Assert.Pass()
  | Sum.Left res -> Assert.Fail $"Expected failure but got result: {res}"

[<Test>]
let ``LangNext-TypeEval Keyof extracts record keys`` () =
  let A = TypeSymbol.Create "A"
  let B = TypeSymbol.Create "B"
  let C = TypeSymbol.Create "C"

  let t1 =
    TypeExpr.Record(
      [ TypeExpr.Lookup("A" |> Identifier.LocalScope), TypeExpr.Primitive PrimitiveType.Int
        TypeExpr.Lookup("B" |> Identifier.LocalScope), TypeExpr.Primitive PrimitiveType.String
        TypeExpr.Lookup("C" |> Identifier.LocalScope), TypeExpr.Primitive PrimitiveType.Decimal ]
    )
    |> TypeExpr.KeyOf

  let actual =
    t1
    |> TypeExpr.Eval
    |> State.Run(
      TypeExprEvalContext.Empty,
      [ A.Name |> Identifier.LocalScope, A
        B.Name |> Identifier.LocalScope, B
        C.Name |> Identifier.LocalScope, C ]
      |> Map.ofList
      |> TypeExprEvalState.CreateFromSymbols
    )

  let expected =
    TypeValue.Union(
      Map.ofList
        [ A, TypeValue.Primitive PrimitiveType.Unit
          B, TypeValue.Primitive PrimitiveType.Unit
          C, TypeValue.Primitive PrimitiveType.Unit ]
    )

  match actual with
  | Sum.Left(actual, _) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval flatten of Keyofs`` () =
  let A = TypeSymbol.Create "A"
  let B = TypeSymbol.Create "B"
  let C = TypeSymbol.Create "C"
  let D = TypeSymbol.Create "D"
  let E = TypeSymbol.Create "E"

  let t1 =
    TypeExpr.Record(
      [ "A" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int
        "B" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String
        "C" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Decimal ]
    )
    |> TypeExpr.KeyOf

  let t2 =
    TypeExpr.Record(
      [ "D" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int
        "E" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String ]
    )
    |> TypeExpr.KeyOf

  let t3 = TypeExpr.Flatten(t1, t2)

  let actual =
    t3
    |> TypeExpr.Eval
    |> State.Run(
      TypeExprEvalContext.Empty,
      [ A.Name |> Identifier.LocalScope, A
        B.Name |> Identifier.LocalScope, B
        C.Name |> Identifier.LocalScope, C
        D.Name |> Identifier.LocalScope, D
        E.Name |> Identifier.LocalScope, E ]
      |> Map.ofList
      |> TypeExprEvalState.CreateFromSymbols
    )

  match actual with
  | Sum.Left(actual, _) ->
    match actual with
    | TypeValue.Union(cases) ->
      match cases |> Map.toList |> List.map (fun (k, v) -> k.Name, v) with
      | [ ("A", TypeValue.Primitive PrimitiveType.Unit)
          ("B", TypeValue.Primitive PrimitiveType.Unit)
          ("C", TypeValue.Primitive PrimitiveType.Unit)
          ("D", TypeValue.Primitive PrimitiveType.Unit)
          ("E", TypeValue.Primitive PrimitiveType.Unit) ] -> Assert.Pass()
      | _ -> Assert.Fail $"Expected flattened union but got {cases}"
    | _ -> Assert.Fail $"Expected union but got {actual}"
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Exclude of Keyofs`` () =
  let A = TypeSymbol.Create "A"
  let B = TypeSymbol.Create "B"
  let C = TypeSymbol.Create "C"
  let X = TypeSymbol.Create "X"

  let t1 =
    TypeExpr.Record(
      [ A.Name |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int
        B.Name |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String
        C.Name |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Decimal ]
    )
    |> TypeExpr.KeyOf

  let t2 =
    TypeExpr.Record(
      [ A.Name |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int
        X.Name |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String ]
    )
    |> TypeExpr.KeyOf

  let t3 = TypeExpr.Exclude(t1, t2)

  let actual =
    t3
    |> TypeExpr.Eval
    |> State.Run(
      TypeExprEvalContext.Empty,
      [ A.Name |> Identifier.LocalScope, A
        B.Name |> Identifier.LocalScope, B
        C.Name |> Identifier.LocalScope, C
        X.Name |> Identifier.LocalScope, X ]
      |> Map.ofList
      |> TypeExprEvalState.CreateFromSymbols
    )

  let expected =
    TypeValue.Union(
      Map.ofList
        [ B, TypeValue.Primitive PrimitiveType.Unit
          C, TypeValue.Primitive PrimitiveType.Unit ]
    )

  match actual with
  | Sum.Left(actual, _) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Exclude of Records`` () =
  let A = TypeSymbol.Create "A"
  let B = TypeSymbol.Create "B"
  let C = TypeSymbol.Create "C"
  let X = TypeSymbol.Create "X"

  let t1 =
    TypeExpr.Record(
      [ "A" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int
        "B" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String
        "C" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Decimal ]
    )

  let t2 =
    TypeExpr.Record(
      [ "A" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int
        "X" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String ]
    )

  let t3 = TypeExpr.Exclude(t1, t2)

  let actual =
    t3
    |> TypeExpr.Eval
    |> State.Run(
      TypeExprEvalContext.Empty,
      [ A.Name |> Identifier.LocalScope, A
        B.Name |> Identifier.LocalScope, B
        C.Name |> Identifier.LocalScope, C
        X.Name |> Identifier.LocalScope, X ]
      |> Map.ofList
      |> TypeExprEvalState.CreateFromSymbols
    )

  let expected =
    TypeValue.Record(
      Map.ofList
        [ B, TypeValue.Primitive PrimitiveType.String
          C, TypeValue.Primitive PrimitiveType.Decimal ]
    )

  match actual with
  | Sum.Left(actual, _) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Exclude of Unions`` () =
  let A = TypeSymbol.Create "A"
  let B = TypeSymbol.Create "B"
  let C = TypeSymbol.Create "C"
  let X = TypeSymbol.Create "X"

  let t1 =
    TypeExpr.Union(
      [ "A" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int
        "B" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String
        "C" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Decimal ]
    )

  let t2 =
    TypeExpr.Union(
      [ "A" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int
        "X" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String ]
    )

  let t3 = TypeExpr.Exclude(t1, t2)

  let actual =
    t3
    |> TypeExpr.Eval
    |> State.Run(
      TypeExprEvalContext.Empty,
      [ A.Name |> Identifier.LocalScope, A
        B.Name |> Identifier.LocalScope, B
        C.Name |> Identifier.LocalScope, C
        X.Name |> Identifier.LocalScope, X ]
      |> Map.ofList
      |> TypeExprEvalState.CreateFromSymbols
    )

  let expected =
    TypeValue.Union(
      Map.ofList
        [ B, TypeValue.Primitive PrimitiveType.String
          C, TypeValue.Primitive PrimitiveType.Decimal ]
    )

  match actual with
  | Sum.Left(actual, _) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Exclude fails on incompatible types`` () =
  let A = TypeSymbol.Create "A"
  let B = TypeSymbol.Create "B"
  let C = TypeSymbol.Create "C"
  let X = TypeSymbol.Create "X"

  let t1 =
    TypeExpr.Union(
      [ "A" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int
        "B" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String
        "C" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Decimal ]
    )

  let t2 =
    TypeExpr.Record(
      [ "A" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int
        "X" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String ]
    )

  let t3 = TypeExpr.Exclude(t1, t2)

  let actual =
    t3
    |> TypeExpr.Eval
    |> State.Run(
      TypeExprEvalContext.Empty,
      [ A.Name |> Identifier.LocalScope, A
        B.Name |> Identifier.LocalScope, B
        C.Name |> Identifier.LocalScope, C
        X.Name |> Identifier.LocalScope, X ]
      |> Map.ofList
      |> TypeExprEvalState.CreateFromSymbols
    )

  match actual with
  | Sum.Left(actual, _) -> Assert.Fail $"Expected failure but got result: {actual}"
  | Sum.Right _err -> Assert.Pass()

[<Test>]
let ``LangNext-TypeEval Rotate from union to record`` () =
  let t =
    TypeExpr.Union(
      [ "A" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int
        "B" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String
        "C" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Decimal ]
    )
    |> TypeExpr.Rotate

  let A = TypeSymbol.Create "A"
  let B = TypeSymbol.Create "B"
  let C = TypeSymbol.Create "C"

  let actual =
    t
    |> TypeExpr.Eval
    |> State.Run(
      TypeExprEvalContext.Empty,
      [ A.Name |> Identifier.LocalScope, A
        B.Name |> Identifier.LocalScope, B
        C.Name |> Identifier.LocalScope, C ]
      |> Map.ofList
      |> TypeExprEvalState.CreateFromSymbols
    )

  let expected =
    TypeValue.Record(
      Map.ofList
        [ A, TypeValue.Primitive PrimitiveType.Int
          B, TypeValue.Primitive PrimitiveType.String
          C, TypeValue.Primitive PrimitiveType.Decimal ]
    )

  match actual with
  | Sum.Left(actual, _) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Rotate from record to union`` () =
  let A = TypeSymbol.Create "A"
  let B = TypeSymbol.Create "B"
  let C = TypeSymbol.Create "C"

  let t =
    TypeExpr.Record(
      [ "A" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int
        "B" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String
        "C" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Decimal ]
    )
    |> TypeExpr.Rotate

  let actual =
    t
    |> TypeExpr.Eval
    |> State.Run(
      TypeExprEvalContext.Empty,
      [ A.Name |> Identifier.LocalScope, A
        B.Name |> Identifier.LocalScope, B
        C.Name |> Identifier.LocalScope, C ]
      |> Map.ofList
      |> TypeExprEvalState.CreateFromSymbols
    )

  let expected =
    TypeValue.Union(
      Map.ofList
        [ A, TypeValue.Primitive PrimitiveType.Int
          B, TypeValue.Primitive PrimitiveType.String
          C, TypeValue.Primitive PrimitiveType.Decimal ]
    )

  match actual with
  | Sum.Left(actual, _) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval (generic) Apply`` () =
  let t =
    TypeExpr.Apply(
      TypeExpr.Lambda(
        TypeParameter.Create("a", Kind.Star),
        TypeExpr.Tuple(
          [ TypeExpr.Primitive PrimitiveType.Int
            TypeExpr.Lookup(Identifier.LocalScope "a") ]
        )
      ),
      TypeExpr.Primitive PrimitiveType.String
    )

  let actual =
    t
    |> TypeExpr.Eval
    |> State.Run(TypeExprEvalContext.Empty, TypeExprEvalState.Empty)

  let expected =
    TypeValue.Tuple(
      [ TypeValue.Primitive PrimitiveType.Int
        TypeValue.Primitive PrimitiveType.String ]
    )

  match actual with
  | Sum.Left(actual, _) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval (generic) Apply over symbol`` () =
  let t =
    TypeExpr.Apply(
      TypeExpr.Lambda(
        TypeParameter.Create("a", Kind.Symbol),
        TypeExpr.Record([ TypeExpr.Lookup(Identifier.LocalScope "a"), TypeExpr.Primitive PrimitiveType.Int ])
      ),
      TypeExpr.Lookup(Identifier.LocalScope "Value")
    )

  let Value = TypeSymbol.Create "Value"

  let actual =
    t
    |> TypeExpr.Eval
    |> State.Run(
      TypeExprEvalContext.Empty,
      TypeExprEvalState.Create(Map.empty, [ Value.Name |> Identifier.LocalScope, Value ] |> Map.ofList)
    )

  let expected =
    TypeValue.Record([ Value, TypeValue.Primitive PrimitiveType.Int ] |> Map.ofList)

  match actual with
  | Sum.Left(actual, _) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval (generic) Apply of type instead of symbol fails`` () =
  let t =
    TypeExpr.Apply(
      TypeExpr.Lambda(
        TypeParameter.Create("a", Kind.Symbol),
        TypeExpr.Record([ TypeExpr.Lookup(Identifier.LocalScope "a"), TypeExpr.Primitive PrimitiveType.Int ])
      ),
      TypeExpr.Primitive PrimitiveType.String
    )

  let actual =
    t
    |> TypeExpr.Eval
    |> State.Run(
      TypeExprEvalContext.Empty,
      TypeExprEvalState.Create(
        Map.empty,
        [ "Value" ]
        |> Seq.map (fun s -> s |> Identifier.LocalScope, s |> TypeSymbol.Create)
        |> Map.ofSeq
      )
    )

  match actual with
  | Sum.Right _ -> Assert.Pass()
  | Sum.Left res -> Assert.Fail $"Expected failure but got result: {res}"


[<Test>]
let ``LangNext-TypeEval (generic) Apply of symbol instead of type fails`` () =
  let t =
    TypeExpr.Apply(
      TypeExpr.Lambda(
        TypeParameter.Create("a", Kind.Star),
        TypeExpr.Tuple(
          [ TypeExpr.Lookup(Identifier.LocalScope "a")
            TypeExpr.Primitive PrimitiveType.Int ]
        )
      ),
      TypeExpr.Lookup(Identifier.LocalScope "Value")
    )

  let actual =
    t
    |> TypeExpr.Eval
    |> State.Run(
      TypeExprEvalContext.Empty,
      TypeExprEvalState.Create(
        Map.empty,
        [ "Value" ]
        |> Seq.map (fun s -> s |> Identifier.LocalScope, s |> TypeSymbol.Create)
        |> Map.ofSeq
      )
    )

  match actual with
  | Sum.Right _ -> Assert.Pass()
  | Sum.Left res -> Assert.Fail $"Expected failure but got result: {res}"

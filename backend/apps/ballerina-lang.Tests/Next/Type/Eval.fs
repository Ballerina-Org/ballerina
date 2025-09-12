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
      TypeExprEvalState.Create([ "T1" |> Identifier.LocalScope, (t1, Kind.Star) ] |> Map.ofSeq, Map.empty)
    )

  match actual with
  | Sum.Left((actual, _), _) ->
    match actual with
    | TypeValue.Primitive PrimitiveType.String -> Assert.Pass()
    | _ -> Assert.Fail $"Expected 'string' but got {actual}"
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Flatten of anonymous unions`` () =
  let A = TypeSymbol.Create("A" |> Identifier.LocalScope)
  let B = TypeSymbol.Create("B" |> Identifier.LocalScope)
  let C = TypeSymbol.Create("C" |> Identifier.LocalScope)
  let D = TypeSymbol.Create("D" |> Identifier.LocalScope)

  let t1 =
    TypeExpr.Union(
      [ "A" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int32
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
      [ A.Name, A; B.Name, B; C.Name, C; D.Name, D ]
      |> Map.ofList
      |> TypeExprEvalState.CreateFromSymbols
    )

  match actual with
  | Sum.Left(actual, _) ->
    match actual with
    | TypeValue.Union(cases), Kind.Star ->
      match cases |> Map.toList |> List.map (fun (k, v) -> k.Name, v) with
      | [ (Identifier.LocalScope "A", TypeValue.Primitive PrimitiveType.Int32)
          (Identifier.LocalScope "B", TypeValue.Primitive PrimitiveType.String)
          (Identifier.LocalScope "C", TypeValue.Primitive PrimitiveType.Decimal)
          (Identifier.LocalScope "D", TypeValue.Primitive PrimitiveType.Bool) ] -> Assert.Pass()
      | _ -> Assert.Fail $"Expected flattened union but got {cases}"
    | _ -> Assert.Fail $"Expected union but got {actual}"
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Flatten of named unions`` () =
  let A = TypeSymbol.Create("A" |> Identifier.LocalScope)
  let B = TypeSymbol.Create("B" |> Identifier.LocalScope)
  let C = TypeSymbol.Create("C" |> Identifier.LocalScope)
  let D = TypeSymbol.Create("D" |> Identifier.LocalScope)

  let t1 =
    TypeValue.Union(
      Map.ofList
        [ A, TypeValue.Primitive PrimitiveType.Int32
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
        [ "T1" |> Identifier.LocalScope, (t1, Kind.Star)
          "T2" |> Identifier.LocalScope, (t2, Kind.Star) ]
        |> Map.ofSeq,
        [ A.Name, A; B.Name, B; C.Name, C; D.Name, D ] |> Map.ofList
      )
    )

  match actual with
  | Sum.Left((actual, _), _) ->
    match actual with
    | TypeValue.Union(cases) ->
      match cases |> Map.toList |> List.map (fun (k, v) -> k.Name, v) with
      | [ (Identifier.LocalScope "A", TypeValue.Primitive PrimitiveType.Int32)
          (Identifier.LocalScope "B", TypeValue.Primitive PrimitiveType.String)
          (Identifier.LocalScope "C", TypeValue.Primitive PrimitiveType.Decimal)
          (Identifier.LocalScope "D", TypeValue.Primitive PrimitiveType.Bool) ] -> Assert.Pass()
      | _ -> Assert.Fail $"Expected flattened union but got {cases}"
    | _ -> Assert.Fail $"Expected union but got {actual}"
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Flatten of named records`` () =
  let A = TypeSymbol.Create("A" |> Identifier.LocalScope)
  let B = TypeSymbol.Create("B" |> Identifier.LocalScope)
  let C = TypeSymbol.Create("C" |> Identifier.LocalScope)
  let D = TypeSymbol.Create("D" |> Identifier.LocalScope)

  let t1 =
    TypeValue.Record(
      Map.ofList
        [ A, TypeValue.Primitive PrimitiveType.Int32
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
        [ "T1" |> Identifier.LocalScope, (t1, Kind.Star)
          "T2" |> Identifier.LocalScope, (t2, Kind.Star) ]
        |> Map.ofSeq,
        [ A.Name, A; B.Name, B; C.Name, C; D.Name, D ] |> Map.ofList
      )
    )

  match actual with
  | Sum.Left((actual, _), _) ->
    match actual with
    | TypeValue.Record(fields) ->
      match fields |> Map.toList |> List.map (fun (k, v) -> k.Name, v) with
      | [ (Identifier.LocalScope "A", TypeValue.Primitive PrimitiveType.Int32)
          (Identifier.LocalScope "B", TypeValue.Primitive PrimitiveType.String)
          (Identifier.LocalScope "C", TypeValue.Primitive PrimitiveType.Decimal)
          (Identifier.LocalScope "D", TypeValue.Primitive PrimitiveType.Bool) ] -> Assert.Pass()
      | _ -> Assert.Fail $"Expected flattened union but got {fields}"
    | _ -> Assert.Fail $"Expected union but got {actual}"
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Flatten of incompatible types fails`` () =
  let A = TypeSymbol.Create("A" |> Identifier.LocalScope)
  let B = TypeSymbol.Create("B" |> Identifier.LocalScope)

  let t1 =
    TypeExpr.Union(
      [ TypeExpr.Lookup("A" |> Identifier.LocalScope), TypeExpr.Primitive PrimitiveType.Int32
        TypeExpr.Lookup("B" |> Identifier.LocalScope), TypeExpr.Primitive PrimitiveType.String ]
    )

  let t2 = TypeExpr.Primitive PrimitiveType.Decimal

  let actual =
    TypeExpr.Flatten(t1, t2)
    |> TypeExpr.Eval
    |> State.Run(
      TypeExprEvalContext.Empty,
      [ A.Name, A; B.Name, B ] |> Map.ofList |> TypeExprEvalState.CreateFromSymbols
    )

  match actual with
  | Sum.Right _err -> Assert.Pass()
  | Sum.Left res -> Assert.Fail $"Expected failure but got result: {res}"

[<Test>]
let ``LangNext-TypeEval Keyof extracts record keys`` () =
  let A = TypeSymbol.Create(Identifier.LocalScope "A")
  let B = TypeSymbol.Create(Identifier.LocalScope "B")
  let C = TypeSymbol.Create(Identifier.LocalScope "C")

  let t1 =
    TypeExpr.Record(
      [ TypeExpr.Lookup("A" |> Identifier.LocalScope), TypeExpr.Primitive PrimitiveType.Int32
        TypeExpr.Lookup("B" |> Identifier.LocalScope), TypeExpr.Primitive PrimitiveType.String
        TypeExpr.Lookup("C" |> Identifier.LocalScope), TypeExpr.Primitive PrimitiveType.Decimal ]
    )
    |> TypeExpr.KeyOf

  let actual =
    t1
    |> TypeExpr.Eval
    |> State.Run(
      TypeExprEvalContext.Empty,
      [ A.Name, A; B.Name, B; C.Name, C ]
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
  | Sum.Left((actual, _), _) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval flatten of Keyofs`` () =
  let A = TypeSymbol.Create("A" |> Identifier.LocalScope)
  let B = TypeSymbol.Create("B" |> Identifier.LocalScope)
  let C = TypeSymbol.Create("C" |> Identifier.LocalScope)
  let D = TypeSymbol.Create("D" |> Identifier.LocalScope)
  let E = TypeSymbol.Create("E" |> Identifier.LocalScope)

  let t1 =
    TypeExpr.Record(
      [ "A" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int32
        "B" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String
        "C" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Decimal ]
    )
    |> TypeExpr.KeyOf

  let t2 =
    TypeExpr.Record(
      [ "D" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int32
        "E" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String ]
    )
    |> TypeExpr.KeyOf

  let t3 = TypeExpr.Flatten(t1, t2)

  let actual =
    t3
    |> TypeExpr.Eval
    |> State.Run(
      TypeExprEvalContext.Empty,
      [ A.Name, A; B.Name, B; C.Name, C; D.Name, D; E.Name, E ]
      |> Map.ofList
      |> TypeExprEvalState.CreateFromSymbols
    )

  match actual with
  | Sum.Left((actual, _), _) ->
    match actual with
    | TypeValue.Union(cases) ->
      match cases |> Map.toList |> List.map (fun (k, v) -> k.Name, v) with
      | [ (Identifier.LocalScope "A", TypeValue.Primitive PrimitiveType.Unit)
          (Identifier.LocalScope "B", TypeValue.Primitive PrimitiveType.Unit)
          (Identifier.LocalScope "C", TypeValue.Primitive PrimitiveType.Unit)
          (Identifier.LocalScope "D", TypeValue.Primitive PrimitiveType.Unit)
          (Identifier.LocalScope "E", TypeValue.Primitive PrimitiveType.Unit) ] -> Assert.Pass()
      | _ -> Assert.Fail $"Expected flattened union but got {cases}"
    | _ -> Assert.Fail $"Expected union but got {actual}"
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Exclude of Keyofs`` () =
  let A = TypeSymbol.Create(Identifier.LocalScope "A")
  let B = TypeSymbol.Create(Identifier.LocalScope "B")
  let C = TypeSymbol.Create(Identifier.LocalScope "C")
  let X = TypeSymbol.Create(Identifier.LocalScope "X")

  let t1 =
    TypeExpr.Record(
      [ A.Name |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int32
        B.Name |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String
        C.Name |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Decimal ]
    )
    |> TypeExpr.KeyOf

  let t2 =
    TypeExpr.Record(
      [ A.Name |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int32
        X.Name |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String ]
    )
    |> TypeExpr.KeyOf

  let t3 = TypeExpr.Exclude(t1, t2)

  let actual =
    t3
    |> TypeExpr.Eval
    |> State.Run(
      TypeExprEvalContext.Empty,
      [ A.Name, A; B.Name, B; C.Name, C; X.Name, X ]
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
  | Sum.Left((actual, _), _) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Exclude of Records`` () =
  let A = TypeSymbol.Create(Identifier.LocalScope "A")
  let B = TypeSymbol.Create(Identifier.LocalScope "B")
  let C = TypeSymbol.Create(Identifier.LocalScope "C")
  let X = TypeSymbol.Create(Identifier.LocalScope "X")

  let t1 =
    TypeExpr.Record(
      [ "A" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int32
        "B" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String
        "C" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Decimal ]
    )

  let t2 =
    TypeExpr.Record(
      [ "A" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int32
        "X" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String ]
    )

  let t3 = TypeExpr.Exclude(t1, t2)

  let actual =
    t3
    |> TypeExpr.Eval
    |> State.Run(
      TypeExprEvalContext.Empty,
      [ A.Name, A; B.Name, B; C.Name, C; X.Name, X ]
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
  | Sum.Left((actual, _), _) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Exclude of Unions`` () =
  let A = TypeSymbol.Create(Identifier.LocalScope "A")
  let B = TypeSymbol.Create(Identifier.LocalScope "B")
  let C = TypeSymbol.Create(Identifier.LocalScope "C")
  let X = TypeSymbol.Create(Identifier.LocalScope "X")

  let t1 =
    TypeExpr.Union(
      [ "A" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int32
        "B" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String
        "C" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Decimal ]
    )

  let t2 =
    TypeExpr.Union(
      [ "A" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int32
        "X" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String ]
    )

  let t3 = TypeExpr.Exclude(t1, t2)

  let actual =
    t3
    |> TypeExpr.Eval
    |> State.Run(
      TypeExprEvalContext.Empty,
      [ A.Name, A; B.Name, B; C.Name, C; X.Name, X ]
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
  | Sum.Left((actual, _), _) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Exclude fails on incompatible types`` () =
  let A = TypeSymbol.Create(Identifier.LocalScope "A")
  let B = TypeSymbol.Create(Identifier.LocalScope "B")
  let C = TypeSymbol.Create(Identifier.LocalScope "C")
  let X = TypeSymbol.Create(Identifier.LocalScope "X")

  let t1 =
    TypeExpr.Union(
      [ "A" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int32
        "B" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String
        "C" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Decimal ]
    )

  let t2 =
    TypeExpr.Record(
      [ "A" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int32
        "X" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String ]
    )

  let t3 = TypeExpr.Exclude(t1, t2)

  let actual =
    t3
    |> TypeExpr.Eval
    |> State.Run(
      TypeExprEvalContext.Empty,
      [ A.Name, A; B.Name, B; C.Name, C; X.Name, X ]
      |> Map.ofList
      |> TypeExprEvalState.CreateFromSymbols
    )

  match actual with
  | Sum.Left((actual, _), _) -> Assert.Fail $"Expected failure but got result: {actual}"
  | Sum.Right _err -> Assert.Pass()

[<Test>]
let ``LangNext-TypeEval Rotate from union to record`` () =
  let t =
    TypeExpr.Union(
      [ "A" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int32
        "B" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String
        "C" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Decimal ]
    )
    |> TypeExpr.Rotate

  let A = TypeSymbol.Create(Identifier.LocalScope "A")
  let B = TypeSymbol.Create(Identifier.LocalScope "B")
  let C = TypeSymbol.Create(Identifier.LocalScope "C")

  let actual =
    t
    |> TypeExpr.Eval
    |> State.Run(
      TypeExprEvalContext.Empty,
      [ A.Name, A; B.Name, B; C.Name, C ]
      |> Map.ofList
      |> TypeExprEvalState.CreateFromSymbols
    )

  let expected =
    TypeValue.Record(
      Map.ofList
        [ A, TypeValue.Primitive PrimitiveType.Int32
          B, TypeValue.Primitive PrimitiveType.String
          C, TypeValue.Primitive PrimitiveType.Decimal ]
    )

  match actual with
  | Sum.Left((actual, _), _) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Rotate from record to union`` () =
  let A = TypeSymbol.Create(Identifier.LocalScope "A")
  let B = TypeSymbol.Create(Identifier.LocalScope "B")
  let C = TypeSymbol.Create(Identifier.LocalScope "C")

  let t =
    TypeExpr.Record(
      [ "A" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int32
        "B" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String
        "C" |> Identifier.LocalScope |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Decimal ]
    )
    |> TypeExpr.Rotate

  let actual =
    t
    |> TypeExpr.Eval
    |> State.Run(
      TypeExprEvalContext.Empty,
      [ A.Name, A; B.Name, B; C.Name, C ]
      |> Map.ofList
      |> TypeExprEvalState.CreateFromSymbols
    )

  let expected =
    TypeValue.Union(
      Map.ofList
        [ A, TypeValue.Primitive PrimitiveType.Int32
          B, TypeValue.Primitive PrimitiveType.String
          C, TypeValue.Primitive PrimitiveType.Decimal ]
    )

  match actual with
  | Sum.Left((actual, _), _) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval (generic) Apply`` () =
  let t =
    TypeExpr.Apply(
      TypeExpr.Lambda(
        TypeParameter.Create("a", Kind.Star),
        TypeExpr.Tuple(
          [ TypeExpr.Primitive PrimitiveType.Int32
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
      [ TypeValue.Primitive PrimitiveType.Int32
        TypeValue.Primitive PrimitiveType.String ]
    )

  match actual with
  | Sum.Left((actual, _), _) -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

// [<Test>]
// let ``LangNext-TypeEval (generic) Apply over symbol`` () =
//   let t =
//     TypeExpr.Apply(
//       TypeExpr.Lambda(
//         TypeParameter.Create("a", Kind.Symbol),
//         TypeExpr.Record([ TypeExpr.Lookup(Identifier.LocalScope "a"), TypeExpr.Primitive PrimitiveType.Int32 ])
//       ),
//       TypeExpr.Lookup(Identifier.LocalScope "Value")
//     )

//   let Value = TypeSymbol.Create(Identifier.LocalScope "Value"

//   let actual =
//     t
//     |> TypeExpr.Eval
//     |> State.Run(
//       TypeExprEvalContext.Empty,
//       TypeExprEvalState.Create(Map.empty, [ Value.Name, Value ] |> Map.ofList)
//     )

//   let expected =
//     TypeValue.Record([ Value, TypeValue.Primitive PrimitiveType.Int32 ] |> Map.ofList)

//   match actual with
//   | Sum.Left((actual, _), _) -> Assert.That(actual, Is.EqualTo expected)
//   | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval (generic) Apply of type instead of symbol fails`` () =
  let t =
    TypeExpr.Apply(
      TypeExpr.Lambda(
        TypeParameter.Create("a", Kind.Symbol),
        TypeExpr.Record([ TypeExpr.Lookup(Identifier.LocalScope "a"), TypeExpr.Primitive PrimitiveType.Int32 ])
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
        |> Seq.map (fun s -> s |> Identifier.LocalScope, s |> Identifier.LocalScope |> TypeSymbol.Create)
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
            TypeExpr.Primitive PrimitiveType.Int32 ]
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
        |> Seq.map (fun s -> s |> Identifier.LocalScope, s |> Identifier.LocalScope |> TypeSymbol.Create)
        |> Map.ofSeq
      )
    )

  match actual with
  | Sum.Right _ -> Assert.Pass()
  | Sum.Left res -> Assert.Fail $"Expected failure but got result: {res}"

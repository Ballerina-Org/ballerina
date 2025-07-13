module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Type.Eval

open Ballerina.Collections.Sum
open NUnit.Framework
open Ballerina.DSL.Expr.Types.Model
open Ballerina.DSL.Expr.Model
open Ballerina.DSL.Expr.Types.TypeCheck
open Ballerina.Errors
open Ballerina.DSL.Extensions.BLPLang
open Ballerina.DSL.Expr.Extensions.Primitives
open Ballerina.DSL.Expr.Extensions.Collections
open Ballerina.DSL.Next.Model
open Ballerina.Reader.WithError
open Ballerina.State.WithError
open Ballerina.Collections.Sum
open NUnit.Framework
open Ballerina.Errors
open Ballerina.DSL.Next.Model
open Ballerina.DSL.Next.EquivalenceClasses
open Ballerina.DSL.Next.Unification
open Ballerina.State.WithError

[<Test>]
let ``LangNext-TypeEval lookup looks up existing types`` () =
  let t1 = TypeValue.Primitive PrimitiveType.String

  let actual =
    TypeExpr.Lookup("T1")
    |> TypeExpr.Eval
    |> ReaderWithError.Run(TypeExprEvalContext.Create([ "T1", t1 ] |> Map.ofSeq, Map.empty))

  match actual with
  | Sum.Left actual ->
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
      Map.ofList
        [ "A" |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int
          "B" |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String ]
    )

  let t2 =
    TypeExpr.Union(
      Map.ofList
        [ "C" |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Decimal
          "D" |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Bool ]
    )

  let actual =
    TypeExpr.Flatten(
      { Left =
          { Identifier = TypeIdentifier.Create "Left"
            Type = t1 }
        Right =
          { Identifier = TypeIdentifier.Create "Right"
            Type = t2 } }
    )
    |> TypeExpr.Eval
    |> ReaderWithError.Run(
      [ A.Name, A; B.Name, B; C.Name, C; D.Name, D ]
      |> Map.ofList
      |> TypeExprEvalContext.CreateFromSymbols
    )

  match actual with
  | Sum.Left actual ->
    match actual with
    | TypeValue.Union(cases) ->
      match cases |> Map.toList |> List.map (fun (k, v) -> k.Name, v) with
      | [ ("Left.A", TypeValue.Primitive PrimitiveType.Int)
          ("Left.B", TypeValue.Primitive PrimitiveType.String)
          ("Right.C", TypeValue.Primitive PrimitiveType.Decimal)
          ("Right.D", TypeValue.Primitive PrimitiveType.Bool) ] -> Assert.Pass()
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
    TypeExpr.Flatten(
      { Left =
          { Identifier = TypeIdentifier.Create "T1"
            Type = TypeExpr.Lookup("T1") }
        Right =
          { Identifier = TypeIdentifier.Create "T2"
            Type = TypeExpr.Lookup("T2") } }
    )
    |> TypeExpr.Eval
    |> ReaderWithError.Run(
      TypeExprEvalContext.Create(
        [ "T1", t1; "T2", t2 ] |> Map.ofSeq,
        [ A.Name, A; B.Name, B; C.Name, C; D.Name, D ] |> Map.ofList
      )
    )

  match actual with
  | Sum.Left actual ->
    match actual with
    | TypeValue.Union(cases) ->
      match cases |> Map.toList |> List.map (fun (k, v) -> k.Name, v) with
      | [ ("T1.A", TypeValue.Primitive PrimitiveType.Int)
          ("T1.B", TypeValue.Primitive PrimitiveType.String)
          ("T2.C", TypeValue.Primitive PrimitiveType.Decimal)
          ("T2.D", TypeValue.Primitive PrimitiveType.Bool) ] -> Assert.Pass()
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
    TypeExpr.Flatten(
      { Left =
          { Identifier = TypeIdentifier.Create "T1"
            Type = TypeExpr.Lookup("T1") }
        Right =
          { Identifier = TypeIdentifier.Create "T2"
            Type = TypeExpr.Lookup("T2") } }
    )
    |> TypeExpr.Eval
    |> ReaderWithError.Run(
      TypeExprEvalContext.Create(
        [ "T1", t1; "T2", t2 ] |> Map.ofSeq,
        [ A.Name, A; B.Name, B; C.Name, C; D.Name, D ] |> Map.ofList
      )
    )

  match actual with
  | Sum.Left actual ->
    match actual with
    | TypeValue.Record(fields) ->
      match fields |> Map.toList |> List.map (fun (k, v) -> k.Name, v) with
      | [ ("T1.A", TypeValue.Primitive PrimitiveType.Int)
          ("T1.B", TypeValue.Primitive PrimitiveType.String)
          ("T2.C", TypeValue.Primitive PrimitiveType.Decimal)
          ("T2.D", TypeValue.Primitive PrimitiveType.Bool) ] -> Assert.Pass()
      | _ -> Assert.Fail $"Expected flattened union but got {fields}"
    | _ -> Assert.Fail $"Expected union but got {actual}"
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Flatten of incompatible types fails`` () =
  let A = TypeSymbol.Create "A"
  let B = TypeSymbol.Create "B"

  let t1 =
    TypeExpr.Union(
      Map.ofList
        [ TypeExpr.Lookup "A", TypeExpr.Primitive PrimitiveType.Int
          TypeExpr.Lookup "B", TypeExpr.Primitive PrimitiveType.String ]
    )

  let t2 = TypeExpr.List(TypeExpr.Primitive PrimitiveType.Decimal)

  let actual =
    TypeExpr.Flatten(
      { Left =
          { Identifier = TypeIdentifier.Create "Left"
            Type = t1 }
        Right =
          { Identifier = TypeIdentifier.Create "Right"
            Type = t2 } }
    )
    |> TypeExpr.Eval
    |> ReaderWithError.Run([ A.Name, A; B.Name, B ] |> Map.ofList |> TypeExprEvalContext.CreateFromSymbols)

  match actual with
  | Sum.Right _err -> Assert.Pass()
  | Sum.Left res -> Assert.Fail $"Expected failure but got result: {res}"

[<Test>]
let ``LangNext-TypeEval Flatten with same name fails`` () =
  let t1 =
    TypeExpr.Union(
      Map.ofList
        [ "A" |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int
          "B" |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String ]
    )

  let t2 =
    TypeExpr.Union(
      Map.ofList
        [ TypeExpr.Lookup "C", TypeExpr.Primitive PrimitiveType.Int
          TypeExpr.Lookup "D", TypeExpr.Primitive PrimitiveType.String ]
    )

  let actual =
    TypeExpr.Flatten(
      { Left =
          { Identifier = TypeIdentifier.Create "Name"
            Type = t1 }
        Right =
          { Identifier = TypeIdentifier.Create "Name"
            Type = t2 } }
    )
    |> TypeExpr.Eval
    |> ReaderWithError.Run(
      [ "A"; "B"; "C"; "D" ]
      |> Seq.map (fun s -> s, s |> TypeSymbol.Create)
      |> Map.ofSeq
      |> TypeExprEvalContext.CreateFromSymbols
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
      Map.ofList
        [ TypeExpr.Lookup "A", TypeExpr.Primitive PrimitiveType.Int
          TypeExpr.Lookup "B", TypeExpr.Primitive PrimitiveType.String
          TypeExpr.Lookup "C", TypeExpr.Primitive PrimitiveType.Decimal ]
    )
    |> TypeExpr.KeyOf

  let actual =
    t1
    |> TypeExpr.Eval
    |> ReaderWithError.Run(
      [ A.Name, A; B.Name, B; C.Name, C ]
      |> Map.ofList
      |> TypeExprEvalContext.CreateFromSymbols
    )

  let expected =
    TypeValue.Union(
      Map.ofList
        [ A, TypeValue.Primitive PrimitiveType.Unit
          B, TypeValue.Primitive PrimitiveType.Unit
          C, TypeValue.Primitive PrimitiveType.Unit ]
    )

  match actual with
  | Sum.Left actual -> Assert.That(actual, Is.EqualTo expected)
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
      Map.ofList
        [ "A" |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int
          "B" |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String
          "C" |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Decimal ]
    )
    |> TypeExpr.KeyOf

  let t2 =
    TypeExpr.Record(
      Map.ofList
        [ "D" |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int
          "E" |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String ]
    )
    |> TypeExpr.KeyOf

  let t3 =
    TypeExpr.Flatten(
      { Left =
          { Identifier = TypeIdentifier.Create "Left"
            Type = t1 }
        Right =
          { Identifier = TypeIdentifier.Create "Right"
            Type = t2 } }
    )

  let actual =
    t3
    |> TypeExpr.Eval
    |> ReaderWithError.Run(
      [ A.Name, A; B.Name, B; C.Name, C; D.Name, D; E.Name, E ]
      |> Map.ofList
      |> TypeExprEvalContext.CreateFromSymbols
    )

  match actual with
  | Sum.Left actual ->
    match actual with
    | TypeValue.Union(cases) ->
      match cases |> Map.toList |> List.map (fun (k, v) -> k.Name, v) with
      | [ ("Left.A", TypeValue.Primitive PrimitiveType.Unit)
          ("Left.B", TypeValue.Primitive PrimitiveType.Unit)
          ("Left.C", TypeValue.Primitive PrimitiveType.Unit)
          ("Right.D", TypeValue.Primitive PrimitiveType.Unit)
          ("Right.E", TypeValue.Primitive PrimitiveType.Unit) ] -> Assert.Pass()
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
      Map.ofList
        [ A.Name |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int
          B.Name |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String
          C.Name |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Decimal ]
    )
    |> TypeExpr.KeyOf

  let t2 =
    TypeExpr.Record(
      Map.ofList
        [ A.Name |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int
          X.Name |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String ]
    )
    |> TypeExpr.KeyOf

  let t3 = TypeExpr.Exclude(t1, t2)

  let actual =
    t3
    |> TypeExpr.Eval
    |> ReaderWithError.Run(
      [ A.Name, A; B.Name, B; C.Name, C; X.Name, X ]
      |> Map.ofList
      |> TypeExprEvalContext.CreateFromSymbols
    )

  let expected =
    TypeValue.Union(
      Map.ofList
        [ B, TypeValue.Primitive PrimitiveType.Unit
          C, TypeValue.Primitive PrimitiveType.Unit ]
    )

  match actual with
  | Sum.Left actual -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Exclude of Records`` () =
  let A = TypeSymbol.Create "A"
  let B = TypeSymbol.Create "B"
  let C = TypeSymbol.Create "C"
  let X = TypeSymbol.Create "X"

  let t1 =
    TypeExpr.Record(
      Map.ofList
        [ "A" |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int
          "B" |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String
          "C" |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Decimal ]
    )

  let t2 =
    TypeExpr.Record(
      Map.ofList
        [ "A" |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int
          "X" |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String ]
    )

  let t3 = TypeExpr.Exclude(t1, t2)

  let actual =
    t3
    |> TypeExpr.Eval
    |> ReaderWithError.Run(
      [ A.Name, A; B.Name, B; C.Name, C; X.Name, X ]
      |> Map.ofList
      |> TypeExprEvalContext.CreateFromSymbols
    )

  let expected =
    TypeValue.Record(
      Map.ofList
        [ B, TypeValue.Primitive PrimitiveType.String
          C, TypeValue.Primitive PrimitiveType.Decimal ]
    )

  match actual with
  | Sum.Left actual -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Exclude of Unions`` () =
  let A = TypeSymbol.Create "A"
  let B = TypeSymbol.Create "B"
  let C = TypeSymbol.Create "C"
  let X = TypeSymbol.Create "X"

  let t1 =
    TypeExpr.Union(
      Map.ofList
        [ "A" |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int
          "B" |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String
          "C" |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Decimal ]
    )

  let t2 =
    TypeExpr.Union(
      Map.ofList
        [ "A" |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int
          "X" |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String ]
    )

  let t3 = TypeExpr.Exclude(t1, t2)

  let actual =
    t3
    |> TypeExpr.Eval
    |> ReaderWithError.Run(
      [ A.Name, A; B.Name, B; C.Name, C; X.Name, X ]
      |> Map.ofList
      |> TypeExprEvalContext.CreateFromSymbols
    )

  let expected =
    TypeValue.Union(
      Map.ofList
        [ B, TypeValue.Primitive PrimitiveType.String
          C, TypeValue.Primitive PrimitiveType.Decimal ]
    )

  match actual with
  | Sum.Left actual -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Exclude fails on incompatible types`` () =
  let A = TypeSymbol.Create "A"
  let B = TypeSymbol.Create "B"
  let C = TypeSymbol.Create "C"
  let X = TypeSymbol.Create "X"

  let t1 =
    TypeExpr.Union(
      Map.ofList
        [ "A" |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int
          "B" |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String
          "C" |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Decimal ]
    )

  let t2 =
    TypeExpr.Record(
      Map.ofList
        [ "A" |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int
          "X" |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String ]
    )

  let t3 = TypeExpr.Exclude(t1, t2)

  let actual =
    t3
    |> TypeExpr.Eval
    |> ReaderWithError.Run(
      [ A.Name, A; B.Name, B; C.Name, C; X.Name, X ]
      |> Map.ofList
      |> TypeExprEvalContext.CreateFromSymbols
    )

  match actual with
  | Sum.Left actual -> Assert.Fail $"Expected failure but got result: {actual}"
  | Sum.Right _err -> Assert.Pass()

[<Test>]
let ``LangNext-TypeEval Rotate from union to record`` () =
  let t =
    TypeExpr.Union(
      Map.ofList
        [ "A" |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int
          "B" |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String
          "C" |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Decimal ]
    )
    |> TypeExpr.Rotate

  let A = TypeSymbol.Create "A"
  let B = TypeSymbol.Create "B"
  let C = TypeSymbol.Create "C"

  let actual =
    t
    |> TypeExpr.Eval
    |> ReaderWithError.Run(
      [ A.Name, A; B.Name, B; C.Name, C ]
      |> Map.ofList
      |> TypeExprEvalContext.CreateFromSymbols
    )

  let expected =
    TypeValue.Record(
      Map.ofList
        [ A, TypeValue.Primitive PrimitiveType.Int
          B, TypeValue.Primitive PrimitiveType.String
          C, TypeValue.Primitive PrimitiveType.Decimal ]
    )

  match actual with
  | Sum.Left actual -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Rotate from record to union`` () =
  let A = TypeSymbol.Create "A"
  let B = TypeSymbol.Create "B"
  let C = TypeSymbol.Create "C"

  let t =
    TypeExpr.Record(
      Map.ofList
        [ "A" |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Int
          "B" |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.String
          "C" |> TypeExpr.Lookup, TypeExpr.Primitive PrimitiveType.Decimal ]
    )
    |> TypeExpr.Rotate

  let actual =
    t
    |> TypeExpr.Eval
    |> ReaderWithError.Run(
      [ A.Name, A; B.Name, B; C.Name, C ]
      |> Map.ofList
      |> TypeExprEvalContext.CreateFromSymbols
    )

  let expected =
    TypeValue.Union(
      Map.ofList
        [ A, TypeValue.Primitive PrimitiveType.Int
          B, TypeValue.Primitive PrimitiveType.String
          C, TypeValue.Primitive PrimitiveType.Decimal ]
    )

  match actual with
  | Sum.Left actual -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval (generic) Apply`` () =
  let t =
    TypeExpr.Apply(
      TypeExpr.Lambda(
        TypeParameter.Create("a", Kind.Star),
        TypeExpr.Tuple([ TypeExpr.Primitive PrimitiveType.Int; TypeExpr.Lookup("a") ])
      ),
      TypeExpr.Primitive PrimitiveType.String
    )

  let actual = t |> TypeExpr.Eval |> ReaderWithError.Run TypeExprEvalContext.Empty

  let expected =
    TypeValue.Tuple(
      [ TypeValue.Primitive PrimitiveType.Int
        TypeValue.Primitive PrimitiveType.String ]
    )

  match actual with
  | Sum.Left actual -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval (generic) Apply over symbol`` () =
  let t =
    TypeExpr.Apply(
      TypeExpr.Lambda(
        TypeParameter.Create("a", Kind.Symbol),
        TypeExpr.Record([ TypeExpr.Lookup("a"), TypeExpr.Primitive PrimitiveType.Int ] |> Map.ofList)
      ),
      TypeExpr.Lookup("Value")
    )

  let Value = TypeSymbol.Create "Value"

  let actual =
    t
    |> TypeExpr.Eval
    |> ReaderWithError.Run(TypeExprEvalContext.Create(Map.empty, [ Value.Name, Value ] |> Map.ofList))

  let expected =
    TypeValue.Record([ Value, TypeValue.Primitive PrimitiveType.Int ] |> Map.ofList)

  match actual with
  | Sum.Left actual -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval (generic) Apply of type instead of symbol fails`` () =
  let t =
    TypeExpr.Apply(
      TypeExpr.Lambda(
        TypeParameter.Create("a", Kind.Symbol),
        TypeExpr.Record([ TypeExpr.Lookup("a"), TypeExpr.Primitive PrimitiveType.Int ] |> Map.ofList)
      ),
      TypeExpr.Primitive PrimitiveType.String
    )

  let actual =
    t
    |> TypeExpr.Eval
    |> ReaderWithError.Run(
      TypeExprEvalContext.Create(Map.empty, [ "Value" ] |> Seq.map (fun s -> s, s |> TypeSymbol.Create) |> Map.ofSeq)
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
        TypeExpr.Tuple([ TypeExpr.Lookup("a"); TypeExpr.Primitive PrimitiveType.Int ])
      ),
      TypeExpr.Lookup("Value")
    )

  let actual =
    t
    |> TypeExpr.Eval
    |> ReaderWithError.Run(
      TypeExprEvalContext.Create(Map.empty, [ "Value" ] |> Seq.map (fun s -> s, s |> TypeSymbol.Create) |> Map.ofSeq)
    )

  match actual with
  | Sum.Right _ -> Assert.Pass()
  | Sum.Left res -> Assert.Fail $"Expected failure but got result: {res}"

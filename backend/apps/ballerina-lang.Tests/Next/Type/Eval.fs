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
let ``LangNext-TypeEval Flatten of anonymous unions`` () =
  let t1 =
    TypeExpr.Union(
      Map.ofList
        [ "A", TypeExpr.Primitive PrimitiveType.Int
          "B", TypeExpr.Primitive PrimitiveType.String ]
    )

  let t2 =
    TypeExpr.Union(
      Map.ofList
        [ "C", TypeExpr.Primitive PrimitiveType.Decimal
          "D", TypeExpr.Primitive PrimitiveType.Bool ]
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
    |> ReaderWithError.Run TypeExprEvalContext.Empty

  let expected =
    TypeValue.Union(
      Map.ofList
        [ "Left.A", TypeValue.Primitive PrimitiveType.Int
          "Left.B", TypeValue.Primitive PrimitiveType.String
          "Right.C", TypeValue.Primitive PrimitiveType.Decimal
          "Right.D", TypeValue.Primitive PrimitiveType.Bool ]
    )

  match actual with
  | Sum.Left actual -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Flatten of named unions`` () =
  let t1 =
    TypeValue.Union(
      Map.ofList
        [ "A", TypeValue.Primitive PrimitiveType.Int
          "B", TypeValue.Primitive PrimitiveType.String ]
    )

  let t2 =
    TypeValue.Union(
      Map.ofList
        [ "C", TypeValue.Primitive PrimitiveType.Decimal
          "D", TypeValue.Primitive PrimitiveType.Bool ]
    )

  let actual =
    TypeExpr.Flatten(
      { Left =
          { Identifier = TypeIdentifier.Create "T1"
            Type = TypeExpr.Lookup(TypeIdentifier.Create "T1") }
        Right =
          { Identifier = TypeIdentifier.Create "T2"
            Type = TypeExpr.Lookup(TypeIdentifier.Create "T2") } }
    )
    |> TypeExpr.Eval
    |> ReaderWithError.Run(
      ([ "T1", t1; "T2", t2 ])
      |> Seq.map (fun (k, v) -> (TypeIdentifier.Create(k), v))
      |> Map.ofSeq
      |> TypeExprEvalContext.CreateFromTypes
    )

  let expected =
    TypeValue.Union(
      Map.ofList
        [ "T1.A", TypeValue.Primitive PrimitiveType.Int
          "T1.B", TypeValue.Primitive PrimitiveType.String
          "T2.C", TypeValue.Primitive PrimitiveType.Decimal
          "T2.D", TypeValue.Primitive PrimitiveType.Bool ]
    )

  match actual with
  | Sum.Left actual -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Flatten of named records`` () =
  let t1 =
    TypeValue.Record(
      Map.ofList
        [ "A", TypeValue.Primitive PrimitiveType.Int
          "B", TypeValue.Primitive PrimitiveType.String ]
    )

  let t2 =
    TypeValue.Record(
      Map.ofList
        [ "C", TypeValue.Primitive PrimitiveType.Decimal
          "D", TypeValue.Primitive PrimitiveType.Bool ]
    )

  let actual =
    TypeExpr.Flatten(
      { Left =
          { Identifier = TypeIdentifier.Create "T1"
            Type = TypeExpr.Lookup(TypeIdentifier.Create "T1") }
        Right =
          { Identifier = TypeIdentifier.Create "T2"
            Type = TypeExpr.Lookup(TypeIdentifier.Create "T2") } }
    )
    |> TypeExpr.Eval
    |> ReaderWithError.Run(
      ([ "T1", t1; "T2", t2 ])
      |> Seq.map (fun (k, v) -> (TypeIdentifier.Create(k), v))
      |> Map.ofSeq
      |> TypeExprEvalContext.CreateFromTypes
    )

  let expected =
    TypeValue.Record(
      Map.ofList
        [ "T1.A", TypeValue.Primitive PrimitiveType.Int
          "T1.B", TypeValue.Primitive PrimitiveType.String
          "T2.C", TypeValue.Primitive PrimitiveType.Decimal
          "T2.D", TypeValue.Primitive PrimitiveType.Bool ]
    )

  match actual with
  | Sum.Left actual -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Flatten of incompatible types fails`` () =
  let t1 =
    TypeExpr.Union(
      Map.ofList
        [ "a", TypeExpr.Primitive PrimitiveType.Int
          "b", TypeExpr.Primitive PrimitiveType.String ]
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
    |> ReaderWithError.Run TypeExprEvalContext.Empty

  match actual with
  | Sum.Right _err -> Assert.Pass()
  | Sum.Left res -> Assert.Fail $"Expected failure but got result: {res}"

[<Test>]
let ``LangNext-TypeEval Flatten with same name fails`` () =
  let t1 =
    TypeExpr.Union(
      Map.ofList
        [ "a", TypeExpr.Primitive PrimitiveType.Int
          "b", TypeExpr.Primitive PrimitiveType.String ]
    )

  let t2 =
    TypeExpr.Union(
      Map.ofList
        [ "c", TypeExpr.Primitive PrimitiveType.Int
          "d", TypeExpr.Primitive PrimitiveType.String ]
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
    |> ReaderWithError.Run TypeExprEvalContext.Empty

  match actual with
  | Sum.Right _err -> Assert.Pass()
  | Sum.Left res -> Assert.Fail $"Expected failure but got result: {res}"

[<Test>]
let ``LangNext-TypeEval Keyof extracts record keys`` () =
  let t1 =
    TypeExpr.Record(
      Map.ofList
        [ "a", TypeExpr.Primitive PrimitiveType.Int
          "b", TypeExpr.Primitive PrimitiveType.String
          "c", TypeExpr.Primitive PrimitiveType.Decimal ]
    )
    |> TypeExpr.KeyOf

  let actual = t1 |> TypeExpr.Eval |> ReaderWithError.Run TypeExprEvalContext.Empty

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

[<Test>]
let ``LangNext-TypeEval flatten of Keyofs`` () =
  let t1 =
    TypeExpr.Record(
      Map.ofList
        [ "A", TypeExpr.Primitive PrimitiveType.Int
          "B", TypeExpr.Primitive PrimitiveType.String
          "C", TypeExpr.Primitive PrimitiveType.Decimal ]
    )
    |> TypeExpr.KeyOf

  let t2 =
    TypeExpr.Record(
      Map.ofList
        [ "D", TypeExpr.Primitive PrimitiveType.Int
          "E", TypeExpr.Primitive PrimitiveType.String ]
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

  let actual = t3 |> TypeExpr.Eval |> ReaderWithError.Run TypeExprEvalContext.Empty

  let expected =
    TypeValue.Union(
      Map.ofList
        [ "Left.A", TypeValue.Primitive PrimitiveType.Unit
          "Left.B", TypeValue.Primitive PrimitiveType.Unit
          "Left.C", TypeValue.Primitive PrimitiveType.Unit
          "Right.D", TypeValue.Primitive PrimitiveType.Unit
          "Right.E", TypeValue.Primitive PrimitiveType.Unit ]
    )

  match actual with
  | Sum.Left actual -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Exclude of Keyofs`` () =
  let t1 =
    TypeExpr.Record(
      Map.ofList
        [ "A", TypeExpr.Primitive PrimitiveType.Int
          "B", TypeExpr.Primitive PrimitiveType.String
          "C", TypeExpr.Primitive PrimitiveType.Decimal ]
    )
    |> TypeExpr.KeyOf

  let t2 =
    TypeExpr.Record(
      Map.ofList
        [ "A", TypeExpr.Primitive PrimitiveType.Int
          "X", TypeExpr.Primitive PrimitiveType.String ]
    )
    |> TypeExpr.KeyOf

  let t3 = TypeExpr.Exclude(t1, t2)

  let actual = t3 |> TypeExpr.Eval |> ReaderWithError.Run TypeExprEvalContext.Empty

  let expected =
    TypeValue.Union(
      Map.ofList
        [ "B", TypeValue.Primitive PrimitiveType.Unit
          "C", TypeValue.Primitive PrimitiveType.Unit ]
    )

  match actual with
  | Sum.Left actual -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Exclude of Records`` () =
  let t1 =
    TypeExpr.Record(
      Map.ofList
        [ "A", TypeExpr.Primitive PrimitiveType.Int
          "B", TypeExpr.Primitive PrimitiveType.String
          "C", TypeExpr.Primitive PrimitiveType.Decimal ]
    )

  let t2 =
    TypeExpr.Record(
      Map.ofList
        [ "A", TypeExpr.Primitive PrimitiveType.Int
          "X", TypeExpr.Primitive PrimitiveType.String ]
    )

  let t3 = TypeExpr.Exclude(t1, t2)

  let actual = t3 |> TypeExpr.Eval |> ReaderWithError.Run TypeExprEvalContext.Empty

  let expected =
    TypeValue.Record(
      Map.ofList
        [ "B", TypeValue.Primitive PrimitiveType.String
          "C", TypeValue.Primitive PrimitiveType.Decimal ]
    )

  match actual with
  | Sum.Left actual -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Exclude of Unions`` () =
  let t1 =
    TypeExpr.Union(
      Map.ofList
        [ "A", TypeExpr.Primitive PrimitiveType.Int
          "B", TypeExpr.Primitive PrimitiveType.String
          "C", TypeExpr.Primitive PrimitiveType.Decimal ]
    )

  let t2 =
    TypeExpr.Union(
      Map.ofList
        [ "A", TypeExpr.Primitive PrimitiveType.Int
          "X", TypeExpr.Primitive PrimitiveType.String ]
    )

  let t3 = TypeExpr.Exclude(t1, t2)

  let actual = t3 |> TypeExpr.Eval |> ReaderWithError.Run TypeExprEvalContext.Empty

  let expected =
    TypeValue.Union(
      Map.ofList
        [ "B", TypeValue.Primitive PrimitiveType.String
          "C", TypeValue.Primitive PrimitiveType.Decimal ]
    )

  match actual with
  | Sum.Left actual -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Exclude fails on incompatible types`` () =
  let t1 =
    TypeExpr.Union(
      Map.ofList
        [ "A", TypeExpr.Primitive PrimitiveType.Int
          "B", TypeExpr.Primitive PrimitiveType.String
          "C", TypeExpr.Primitive PrimitiveType.Decimal ]
    )

  let t2 =
    TypeExpr.Record(
      Map.ofList
        [ "A", TypeExpr.Primitive PrimitiveType.Int
          "X", TypeExpr.Primitive PrimitiveType.String ]
    )

  let t3 = TypeExpr.Exclude(t1, t2)

  let actual = t3 |> TypeExpr.Eval |> ReaderWithError.Run TypeExprEvalContext.Empty

  match actual with
  | Sum.Left actual -> Assert.Fail $"Expected failure but got result: {actual}"
  | Sum.Right _err -> Assert.Pass()

[<Test>]
let ``LangNext-TypeEval Rotate from union to record`` () =
  let t =
    TypeExpr.Union(
      Map.ofList
        [ "A", TypeExpr.Primitive PrimitiveType.Int
          "B", TypeExpr.Primitive PrimitiveType.String
          "C", TypeExpr.Primitive PrimitiveType.Decimal ]
    )
    |> TypeExpr.Rotate

  let actual = t |> TypeExpr.Eval |> ReaderWithError.Run TypeExprEvalContext.Empty

  let expected =
    TypeValue.Record(
      Map.ofList
        [ "A", TypeValue.Primitive PrimitiveType.Int
          "B", TypeValue.Primitive PrimitiveType.String
          "C", TypeValue.Primitive PrimitiveType.Decimal ]
    )

  match actual with
  | Sum.Left actual -> Assert.That(actual, Is.EqualTo expected)
  | Sum.Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``LangNext-TypeEval Rotate from record to union`` () =
  let t =
    TypeExpr.Record(
      Map.ofList
        [ "A", TypeExpr.Primitive PrimitiveType.Int
          "B", TypeExpr.Primitive PrimitiveType.String
          "C", TypeExpr.Primitive PrimitiveType.Decimal ]
    )
    |> TypeExpr.Rotate

  let actual = t |> TypeExpr.Eval |> ReaderWithError.Run TypeExprEvalContext.Empty

  let expected =
    TypeValue.Union(
      Map.ofList
        [ "A", TypeValue.Primitive PrimitiveType.Int
          "B", TypeValue.Primitive PrimitiveType.String
          "C", TypeValue.Primitive PrimitiveType.Decimal ]
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
        TypeExpr.Tuple([ TypeExpr.Primitive PrimitiveType.Int; TypeExpr.Var(TypeVar.Create("a")) ])
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

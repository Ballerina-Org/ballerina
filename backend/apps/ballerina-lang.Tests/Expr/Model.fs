module Ballerina.Cat.Tests.BusinessRuleEngine.Expr.Model

open NUnit.Framework
open Ballerina.DSL.Expr.Model
open Ballerina.DSL.Extensions.BLPLang
open Ballerina.DSL.Expr.Extensions.Primitives
open Ballerina.DSL.Expr.Extensions.Collections
open System

type TestCase =
  { expr: Expr<BLPExprExtension, BLPValueExtension>
    expected: string }

let testCases =
  [ { expr = Expr.Value Value.Unit
      expected = "()" }
    { expr =
        Expr.Value(
          (PrimitivesValueExtension.ConstInt 42)
          |> blpLanguageExtension.primitivesExtension.toValue
        )
      expected = "42" }
    { expr =
        Expr.Value(
          (PrimitivesValueExtension.ConstFloat 42m)
          |> blpLanguageExtension.primitivesExtension.toValue
        )
      expected = "42" }
    { expr =
        Expr.Value(
          (PrimitivesValueExtension.ConstString "42")
          |> blpLanguageExtension.primitivesExtension.toValue
        )
      expected = "42" }
    { expr =
        Expr.Value(
          (PrimitivesValueExtension.ConstBool true)
          |> blpLanguageExtension.primitivesExtension.toValue
        )
      expected = "True" }
    { expr =
        Expr.Value(
          (PrimitivesValueExtension.ConstGuid(Guid.Parse "00000000-0000-0000-0000-000000000000"))
          |> blpLanguageExtension.primitivesExtension.toValue
        )
      expected = "00000000-0000-0000-0000-000000000000" }
    { expr =
        Expr.Value(
          Value.CaseCons(
            "Some",
            (PrimitivesValueExtension.ConstInt 42
             |> blpLanguageExtension.primitivesExtension.toValue)
          )
        )
      expected = "Some(42)" }
    { expr =
        Expr.Value(
          Value.Tuple
            [ (PrimitivesValueExtension.ConstInt 42
               |> blpLanguageExtension.primitivesExtension.toValue)
              (PrimitivesValueExtension.ConstString "42"
               |> blpLanguageExtension.primitivesExtension.toValue) ]
        )
      expected = "(42, 42)" }
    { expr =
        Expr.Value(
          Value.Record(
            Map.ofList
              [ "x",
                (PrimitivesValueExtension.ConstInt 42
                 |> blpLanguageExtension.primitivesExtension.toValue)
                "y",
                (PrimitivesValueExtension.ConstString "42"
                 |> blpLanguageExtension.primitivesExtension.toValue) ]
          )
        )
      expected = "{ x = 42; y = 42 }" }
    { expr =
        Expr.Value(
          Value.Lambda(
            { VarName = "x" },
            Some(ExprType.PrimitiveType PrimitiveType.IntType),
            None,
            Expr.Value(
              (PrimitivesValueExtension.ConstInt 42
               |> blpLanguageExtension.primitivesExtension.toValue)
            )
          )
        )
      expected = "fun (x:Int) -> 42" }
    { expr =
        Expr.Value(
          CollectionsValueExtension.List
            [ (PrimitivesValueExtension.ConstInt 42
               |> blpLanguageExtension.primitivesExtension.toValue)
              (PrimitivesValueExtension.ConstInt 43
               |> blpLanguageExtension.primitivesExtension.toValue) ]
          |> blpLanguageExtension.collectionsExtension.toValue
        )
      expected = "[42, 43]" }
    { expr =
        Expr.Apply(
          Expr.Value(
            Value.Lambda(
              { VarName = "x" },
              Some(ExprType.PrimitiveType PrimitiveType.IntType),
              None,
              Expr.Value(
                (PrimitivesValueExtension.ConstInt 42
                 |> blpLanguageExtension.primitivesExtension.toValue)
              )
            )
          ),
          Expr.Value(
            (PrimitivesValueExtension.ConstInt 2
             |> blpLanguageExtension.primitivesExtension.toValue)
          )
        )
      expected = "(fun (x:Int) -> 42)(2)" }
    { expr =
        PrimitivesExprExtension.Binary(
          BinaryOperator.And,
          Expr.Value(
            (PrimitivesValueExtension.ConstBool true
             |> blpLanguageExtension.primitivesExtension.toValue)
          ),
          Expr.Value(
            (PrimitivesValueExtension.ConstBool false
             |> blpLanguageExtension.primitivesExtension.toValue)
          )
        )
        |> blpLanguageExtension.primitivesExtension.toExpr
      expected = "(True And False)" }
    { expr =
        PrimitivesExprExtension.Binary(
          BinaryOperator.Or,
          Expr.Value(
            (PrimitivesValueExtension.ConstBool true
             |> blpLanguageExtension.primitivesExtension.toValue)
          ),
          Expr.Value(
            (PrimitivesValueExtension.ConstBool false
             |> blpLanguageExtension.primitivesExtension.toValue)
          )
        )
        |> blpLanguageExtension.primitivesExtension.toExpr
      expected = "(True Or False)" }
    { expr = Expr.VarLookup { VarName = "x" }
      expected = "x" }
    { expr =
        Expr.MakeRecord(
          Map.ofList
            [ "x",
              Expr.Value(
                (PrimitivesValueExtension.ConstInt 42
                 |> blpLanguageExtension.primitivesExtension.toValue)
              )
              "y",
              Expr.Value(
                (PrimitivesValueExtension.ConstString "42"
                 |> blpLanguageExtension.primitivesExtension.toValue)
              ) ]
        )
      expected = "{ x = 42; y = 42 }" }
    { expr =
        Expr.RecordFieldLookup(
          Expr.Value(
            Value.Record(
              Map.ofList
                [ "x",
                  (PrimitivesValueExtension.ConstInt 42
                   |> blpLanguageExtension.primitivesExtension.toValue)
                  "y",
                  (PrimitivesValueExtension.ConstString "42"
                   |> blpLanguageExtension.primitivesExtension.toValue) ]
            )
          ),
          "x"
        )
      expected = "{ x = 42; y = 42 }.x" }
    { expr =
        Expr.MakeTuple
          [ Expr.Value(
              (PrimitivesValueExtension.ConstInt 42
               |> blpLanguageExtension.primitivesExtension.toValue)
            )
            Expr.Value(
              (PrimitivesValueExtension.ConstString "42"
               |> blpLanguageExtension.primitivesExtension.toValue)
            ) ]
      expected = "(42, 42)" }
    { expr =
        Expr.MakeSet
          [ Expr.Value(
              (PrimitivesValueExtension.ConstInt 42
               |> blpLanguageExtension.primitivesExtension.toValue)
            )
            Expr.Value(
              (PrimitivesValueExtension.ConstString "42"
               |> blpLanguageExtension.primitivesExtension.toValue)
            ) ]
      expected = "{42; 42}" }
    { expr =
        Expr.Project(
          Expr.Value(
            Value.Tuple
              [ (PrimitivesValueExtension.ConstInt 42
                 |> blpLanguageExtension.primitivesExtension.toValue)
                (PrimitivesValueExtension.ConstString "42"
                 |> blpLanguageExtension.primitivesExtension.toValue) ]
          ),
          0
        )
      expected = "(42, 42).π0" }
    { expr =
        Expr.MakeCase(
          "Some",
          Expr.Value(
            (PrimitivesValueExtension.ConstInt 42
             |> blpLanguageExtension.primitivesExtension.toValue)
          )
        )
      expected = "Some(42)" }
    { expr =
        Expr.MatchCase(
          Expr.Value(
            Value.CaseCons(
              "Some",
              (PrimitivesValueExtension.ConstInt 42
               |> blpLanguageExtension.primitivesExtension.toValue)
            )
          ),
          Map.ofList
            [ "Some",
              ({ VarName = "y" },
               Expr.Value(
                 (PrimitivesValueExtension.ConstInt 42
                  |> blpLanguageExtension.primitivesExtension.toValue)
               ))
              "None", ({ VarName = "_" }, Expr.Value Value.Unit) ]
        )
      expected = "match Some(42) with | None(_) -> () | Some(y) -> 42" } ]

[<TestCaseSource(nameof testCases)>]
let ``Should correctly map to string for visualization`` (testCase: TestCase) =
  let { expr = expr; expected = expected } = testCase
  let stringRepresentation = expr.ToString()
  Assert.That(stringRepresentation, Is.EqualTo expected)

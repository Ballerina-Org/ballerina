module Ballerina.Core.Tests.BusinessRuleEngine.Expr.Model

open NUnit.Framework
open Ballerina.DSL.Expr.Model
open System

type ExprExtension = unit
type ValueExtension = unit

type TestCase =
  { expr: Expr<ExprExtension, ValueExtension>
    expected: string }

let testCases =
  [ { expr = Expr.Value Value.Unit
      expected = "()" }
    { expr = Expr.Value(Value.ConstInt 42)
      expected = "42" }
    { expr = Expr.Value(Value.ConstFloat 42m)
      expected = "42" }
    { expr = Expr.Value(Value.ConstString "42")
      expected = "42" }
    { expr = Expr.Value(Value.ConstBool true)
      expected = "True" }
    { expr = Expr.Value(Value.ConstGuid(Guid.Parse "00000000-0000-0000-0000-000000000000"))
      expected = "00000000-0000-0000-0000-000000000000" }
    { expr = Expr.Value(Value.CaseCons("Some", Value.ConstInt 42))
      expected = "Some(42)" }
    { expr = Expr.Value(Value.Tuple [ Value.ConstInt 42; Value.ConstString "42" ])
      expected = "(42, 42)" }
    { expr = Expr.Value(Value.Record(Map.ofList [ "x", Value.ConstInt 42; "y", Value.ConstString "42" ]))
      expected = "{ x = 42; y = 42 }" }
    { expr = Expr.Value(Value.Lambda({ VarName = "x" }, Expr.Value(Value.ConstInt 42)))
      expected = "fun x -> 42" }
    { expr = Expr.Value(Value.List [ Value.ConstInt 42; Value.ConstInt 43 ])
      expected = "[42, 43]" }
    { expr =
        Expr.Apply(
          Expr.Value(Value.Lambda({ VarName = "x" }, Expr.Value(Value.ConstInt 42))),
          Expr.Value(Value.ConstInt 2)
        )
      expected = "(fun x -> 42)(2)" }
    { expr = Expr.Binary(BinaryOperator.And, Expr.Value(Value.ConstBool true), Expr.Value(Value.ConstBool false))
      expected = "(True And False)" }
    { expr = Expr.Binary(BinaryOperator.Or, Expr.Value(Value.ConstBool true), Expr.Value(Value.ConstBool false))
      expected = "(True Or False)" }
    { expr = Expr.VarLookup { VarName = "x" }
      expected = "x" }
    { expr = Expr.MakeRecord(Map.ofList [ "x", Expr.Value(Value.ConstInt 42); "y", Expr.Value(Value.ConstString "42") ])
      expected = "{ x = 42; y = 42 }" }
    { expr =
        Expr.RecordFieldLookup(
          Expr.Value(Value.Record(Map.ofList [ "x", Value.ConstInt 42; "y", Value.ConstString "42" ])),
          "x"
        )
      expected = "{ x = 42; y = 42 }.x" }
    { expr = Expr.MakeTuple [ Expr.Value(Value.ConstInt 42); Expr.Value(Value.ConstString "42") ]
      expected = "(42, 42)" }
    { expr = Expr.MakeSet [ Expr.Value(Value.ConstInt 42); Expr.Value(Value.ConstString "42") ]
      expected = "{42; 42}" }
    { expr = Expr.Project(Expr.Value(Value.Tuple [ Value.ConstInt 42; Value.ConstString "42" ]), 0)
      expected = "(42, 42).π0" }
    { expr = Expr.MakeCase("Some", Expr.Value(Value.ConstInt 42))
      expected = "Some(42)" }
    { expr =
        Expr.MatchCase(
          Expr.Value(Value.CaseCons("Some", Value.ConstInt 42)),
          Map.ofList
            [ "Some", ({ VarName = "y" }, Expr.Value(Value.ConstInt 42))
              "None", ({ VarName = "_" }, Expr.Value Value.Unit) ]
        )
      expected = "match Some(42) with | None(_) -> () | Some(y) -> 42" } ]

[<TestCaseSource(nameof testCases)>]
let ``Should correctly map to string for visualization`` (testCase: TestCase) =
  let { expr = expr; expected = expected } = testCase
  let stringRepresentation = expr.ToString()
  Assert.That(stringRepresentation, Is.EqualTo expected)

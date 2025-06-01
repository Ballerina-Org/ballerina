module Ballerina.Core.Tests.BusinessRuleEngine.ExprType.TypeCheck

open Ballerina.Collections.Sum
open NUnit.Framework
open Ballerina.DSL.Expr.Types.Model
open Ballerina.DSL.Expr.Model
open Ballerina.DSL.Expr.Types.TypeCheck
open Ballerina.Errors

let private typeCheck (expr: Expr) : Sum<ExprType, Errors> = Expr.typeCheck Map.empty Map.empty expr

type ValuePrimitiveTypeCheckTestCase = { expr: Expr; expected: ExprType }

let valuePrimitiveTypeCheckTestCases =
  [ { expr = Expr.Value(Value.ConstInt 42)
      expected = ExprType.PrimitiveType PrimitiveType.IntType }
    { expr = Expr.Value(Value.ConstString "42")
      expected = ExprType.PrimitiveType PrimitiveType.StringType }
    { expr = Expr.Value(Value.ConstBool true)
      expected = ExprType.PrimitiveType PrimitiveType.BoolType } ]

[<TestCaseSource(nameof valuePrimitiveTypeCheckTestCases)>]
let ``Should typecheck values primitives`` (testCase: ValuePrimitiveTypeCheckTestCase) =
  let { expr = expr; expected = expected } = testCase

  match typeCheck expr with
  | Left value -> Assert.That(value, Is.EqualTo expected)
  | Right err -> Assert.Fail $"Expected success but got error: {err}"

type BoolReturningBinaryExpressionTestCase = { expr: Expr; expected: ExprType }

let boolReturningBinaryExpressionTestCases =
  [ { expr = Expr.Binary(Or, Expr.Value(Value.ConstBool true), Expr.Value(Value.ConstBool false))
      expected = ExprType.PrimitiveType PrimitiveType.BoolType } ]

[<TestCaseSource(nameof boolReturningBinaryExpressionTestCases)>]
let ``Should typecheck bool returning binary expressions`` (testCase: BoolReturningBinaryExpressionTestCase) =
  let { expr = expr; expected = expected } = testCase

  match typeCheck expr with
  | Left value -> Assert.That(value, Is.EqualTo expected)
  | Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``Should typecheck tuple`` () =
  let expr = Expr.Value(Value.Tuple [ Value.ConstBool true; Value.ConstInt 42 ])

  let expected =
    ExprType.TupleType
      [ ExprType.PrimitiveType PrimitiveType.BoolType
        ExprType.PrimitiveType PrimitiveType.IntType ]

  match typeCheck expr with
  | Left value -> Assert.That(value, Is.EqualTo expected)
  | Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``Should typecheck tuple projection (with 1-based indexing)`` () =
  let expr =
    Expr.Project(Expr.Value(Value.Tuple [ Value.ConstBool true; Value.ConstInt 42 ]), 1)

  let expected = ExprType.PrimitiveType PrimitiveType.BoolType

  match typeCheck expr with
  | Left value -> Assert.That(value, Is.EqualTo expected)
  | Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``Should typecheck var lookup`` () =
  let expr = Expr.VarLookup { VarName = "x" }

  let expected = ExprType.PrimitiveType PrimitiveType.BoolType

  let inputVarTypes =
    Map.ofList [ { VarName = "x" }, ExprType.PrimitiveType PrimitiveType.BoolType ]


  match Expr.typeCheck Map.empty inputVarTypes expr with
  | Left value -> Assert.That(value, Is.EqualTo expected)
  | Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``Should typecheck var lookup should fail if not in seen variables`` () =
  let expr = Expr.VarLookup { VarName = "x" }

  let vars = Map.empty

  match Expr.typeCheck Map.empty vars expr with
  | Left _ -> Assert.Fail $"Expected error but got success"
  | Right err -> Assert.That(err.ToString(), Contains.Substring "x")

module Ballerina.Core.Tests.BusinessRuleEngine.ExprType.TypeCheck

open Ballerina.Collections.Sum
open NUnit.Framework
open Ballerina.DSL.Expr.Types.Model
open Ballerina.DSL.Expr.Model
open Ballerina.DSL.Expr.Types.TypeCheck
open Ballerina.DSL.Model
open Ballerina.Errors

let private typeCheck (expr: Expr) : Sum<ExprType * VarTypes, Errors> =
  let schema: Schema =
    { tryFindEntity = fun _ -> None
      tryFindField = fun _ -> None }

  Expr.typeCheck Map.empty schema Map.empty expr

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
  | Left(value, varTypes) ->
    Assert.That(value, Is.EqualTo expected)
    Assert.That(varTypes, Is.Empty)
  | Right err -> Assert.Fail $"Expected success but got error: {err}"

type BoolReturningBinaryExpressionTestCase = { expr: Expr; expected: ExprType }

let boolReturningBinaryExpressionTestCases =
  [ { expr = Expr.Binary(Or, Expr.Value(Value.ConstBool true), Expr.Value(Value.ConstBool false))
      expected = ExprType.PrimitiveType PrimitiveType.BoolType } ]

[<TestCaseSource(nameof boolReturningBinaryExpressionTestCases)>]
let ``Should typecheck bool returning binary expressions`` (testCase: BoolReturningBinaryExpressionTestCase) =
  let { expr = expr; expected = expected } = testCase

  match typeCheck expr with
  | Left(value, varTypes) ->
    Assert.That(value, Is.EqualTo expected)
    Assert.That(varTypes, Is.Empty)
  | Right err -> Assert.Fail $"Expected success but got error: {err}"

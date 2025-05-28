module Ballerina.Core.Tests.BusinessRuleEngine.ExprType.TypeCheck

open Ballerina.Collections.Sum
open NUnit.Framework
open Ballerina.DSL.Expr.Types.Model
open Ballerina.DSL.Expr.Model
open Ballerina.DSL.Expr.Types.TypeCheck
open Ballerina.DSL.Model

type TestCase = { expr: Expr; expected: ExprType }

let testCases =
  [ { expr = Expr.Value(Value.ConstInt 42)
      expected = ExprType.PrimitiveType PrimitiveType.IntType }
    { expr = Expr.Value(Value.ConstString "42")
      expected = ExprType.PrimitiveType PrimitiveType.StringType }
    { expr = Expr.Value(Value.ConstBool true)
      expected = ExprType.PrimitiveType PrimitiveType.BoolType } ]

[<TestCaseSource(nameof testCases)>]
let ``Should typecheck values primitives`` (testCase: TestCase) =
  let { expr = expr; expected = expected } = testCase

  let schema: Schema =
    { tryFindEntity = fun _ -> None
      tryFindField = fun _ -> None }

  let inferredType = Expr.typeCheck Map.empty schema Map.empty expr

  match inferredType with
  | Left(value, varTypes) ->
    Assert.That(value, Is.EqualTo expected)
    Assert.That(varTypes, Is.Empty)
  | Right err -> Assert.Fail($"Expected success but got error: {err}")

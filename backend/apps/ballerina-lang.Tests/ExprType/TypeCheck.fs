module Ballerina.Cat.Tests.BusinessRuleEngine.ExprType.TypeCheck

open Ballerina.Collections.Sum
open NUnit.Framework
open Ballerina.DSL.Expr.Types.Model
open Ballerina.DSL.Expr.Model
open Ballerina.DSL.Expr.Types.TypeCheck
open Ballerina.Errors
open Ballerina.DSL.Extensions.BLPLang
open Ballerina.DSL.Expr.Extensions.Primitives
open Ballerina.DSL.Expr.Extensions.Collections

let private typeCheck (expr: Expr<BLPExprExtension, BLPValueExtension>) : Sum<ExprType, Errors> =
  Expr.typeCheck
    (fun typeCheckRootExpr typeCheckRootValue bindings vars (BLPExprExtension e) ->
      blpLanguageExtension.typeCheck.expr typeCheckRootExpr typeCheckRootValue bindings vars e)
    (fun typeCheckRootExpr typeCheckRootValue bindings vars (BLPValueExtension v) ->
      blpLanguageExtension.typeCheck.value typeCheckRootExpr typeCheckRootValue bindings vars v)
    blpLanguageExtension.typeCheck.typeBindings
    Map.empty
    expr

type ValuePrimitiveTypeCheckTestCase =
  { expr: Expr<BLPExprExtension, BLPValueExtension>
    expected: ExprType }

let valuePrimitiveTypeCheckTestCases: ValuePrimitiveTypeCheckTestCase list =
  [ { expr =
        Expr.Value(
          (PrimitivesValueExtension.ConstInt 42
           |> blpLanguageExtension.primitivesExtension.toValue)
        )
      expected = ExprType.PrimitiveType PrimitiveType.IntType }
    { expr =
        Expr.Value(
          (PrimitivesValueExtension.ConstString "42"
           |> blpLanguageExtension.primitivesExtension.toValue)
        )
      expected = ExprType.PrimitiveType PrimitiveType.StringType }
    { expr =
        Expr.Value(
          (PrimitivesValueExtension.ConstBool true
           |> blpLanguageExtension.primitivesExtension.toValue)
        )
      expected = ExprType.PrimitiveType PrimitiveType.BoolType } ]

[<TestCaseSource(nameof valuePrimitiveTypeCheckTestCases)>]
let ``Should typecheck values primitives`` (testCase: ValuePrimitiveTypeCheckTestCase) =
  let { expr = expr; expected = expected } = testCase

  match typeCheck expr with
  | Left value -> Assert.That(value, Is.EqualTo expected)
  | Right err -> Assert.Fail $"Expected success but got error: {err}"

type BoolReturningBinaryExpressionTestCase =
  { expr: Expr<BLPExprExtension, BLPValueExtension>
    expected: ExprType }

let boolReturningBinaryExpressionTestCases =
  [ { expr =
        PrimitivesExprExtension.Binary(
          Or,
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
      expected = ExprType.PrimitiveType PrimitiveType.BoolType } ]

[<TestCaseSource(nameof boolReturningBinaryExpressionTestCases)>]
let ``Should typecheck bool returning binary expressions`` (testCase: BoolReturningBinaryExpressionTestCase) =
  let { expr = expr; expected = expected } = testCase

  match typeCheck expr with
  | Left value -> Assert.That(value, Is.EqualTo expected)
  | Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``Should typecheck tuple`` () =
  let expr =
    Expr.Value(
      Value.Tuple
        [ (PrimitivesValueExtension.ConstBool true
           |> blpLanguageExtension.primitivesExtension.toValue)
          (PrimitivesValueExtension.ConstInt 42
           |> blpLanguageExtension.primitivesExtension.toValue) ]
    )

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
    Expr.Project(
      Expr.Value(
        Value.Tuple
          [ (PrimitivesValueExtension.ConstBool true
             |> blpLanguageExtension.primitivesExtension.toValue)
            (PrimitivesValueExtension.ConstInt 42
             |> blpLanguageExtension.primitivesExtension.toValue) ]
      ),
      1
    )

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


  match
    Expr<BLPExprExtension, BLPValueExtension>.typeCheck
      (fun typeCheckRootExpr typeCheckRootValue bindings vars (BLPExprExtension e) ->
        blpLanguageExtension.typeCheck.expr typeCheckRootExpr typeCheckRootValue bindings vars e)
      (fun typeCheckRootExpr typeCheckRootValue bindings vars (BLPValueExtension v) ->
        blpLanguageExtension.typeCheck.value typeCheckRootExpr typeCheckRootValue bindings vars v)
      blpLanguageExtension.typeCheck.typeBindings
      inputVarTypes
      expr
  with
  | Left value -> Assert.That(value, Is.EqualTo expected)
  | Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``Should typecheck var lookup should fail if not in seen variables`` () =
  let expr = Expr.VarLookup { VarName = "x" }

  let vars = Map.empty

  match
    Expr.typeCheck
      (fun typeCheckRootExpr typeCheckRootValue bindings vars (BLPExprExtension e) ->
        blpLanguageExtension.typeCheck.expr typeCheckRootExpr typeCheckRootValue bindings vars e)
      (fun typeCheckRootExpr typeCheckRootValue bindings vars (BLPValueExtension v) ->
        blpLanguageExtension.typeCheck.value typeCheckRootExpr typeCheckRootValue bindings vars v)
      blpLanguageExtension.typeCheck.typeBindings
      vars
      expr
  with
  | Left _ -> Assert.Fail $"Expected error but got success"
  | Right err -> Assert.That(err.ToString(), Contains.Substring "x")

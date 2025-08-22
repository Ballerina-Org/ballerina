module Ballerina.Cat.Tests.BusinessRuleEngine.ExprType.TypeCheck

open Ballerina.Collections.Sum
open NUnit.Framework
open Ballerina.DSL.Expr.Model
open Ballerina.Errors
open Ballerina.DSL.Extensions.BLPLang
open Ballerina.DSL.Expr.Extensions

let private typeCheck (expr: Expr<BLPExprExtension, BLPValueExtension>) : Sum<ExprType, Errors> =
  blpLanguageExtension.typeCheck Map.empty Map.empty expr

type ValuePrimitiveTypeCheckTestCase =
  { expr: Expr<BLPExprExtension, BLPValueExtension>
    expected: ExprType }

let valuePrimitiveTypeCheckTestCases: ValuePrimitiveTypeCheckTestCase list =
  [ { expr =
        Expr.Value(
          (Primitives.ValueExtension.ConstInt 42
           |> blpLanguageExtension.primitivesExtension.toValue)
        )
      expected = ExprType.PrimitiveType PrimitiveType.IntType }
    { expr =
        Expr.Value(
          (Primitives.ValueExtension.ConstString "42"
           |> blpLanguageExtension.primitivesExtension.toValue)
        )
      expected = ExprType.PrimitiveType PrimitiveType.StringType }
    { expr =
        Expr.Value(
          (Primitives.ValueExtension.ConstBool true
           |> blpLanguageExtension.primitivesExtension.toValue)
        )
      expected = ExprType.PrimitiveType PrimitiveType.BoolType } ]

[<TestCaseSource(nameof valuePrimitiveTypeCheckTestCases)>]
let ``Should typecheck values primitives`` (testCase: ValuePrimitiveTypeCheckTestCase) =
  let { expr = expr; expected = expected } = testCase

  match typeCheck expr with
  | Left value -> Assert.That(value, Is.EqualTo expected)
  | Right err -> Assert.Fail $"Expected success but got error: {err}"

type SimpleBinaryExpressionTestCase =
  { expr: Expr<BLPExprExtension, BLPValueExtension>
    expected: ExprType }

let simpleBinaryExpressionTestCases =
  [ { expr =
        Primitives.ExprExtension.Binary(
          Primitives.BinaryOperator.Or,
          Expr.Value(
            (Primitives.ValueExtension.ConstBool true
             |> blpLanguageExtension.primitivesExtension.toValue)
          ),
          Expr.Value(
            (Primitives.ValueExtension.ConstBool false
             |> blpLanguageExtension.primitivesExtension.toValue)
          )
        )
        |> blpLanguageExtension.primitivesExtension.toExpr
      expected = ExprType.PrimitiveType PrimitiveType.BoolType }
    { expr =
        Primitives.ExprExtension.Binary(
          Primitives.BinaryOperator.And,
          Expr.Value(
            (Primitives.ValueExtension.ConstBool true
             |> blpLanguageExtension.primitivesExtension.toValue)
          ),
          Expr.Value(
            (Primitives.ValueExtension.ConstBool false
             |> blpLanguageExtension.primitivesExtension.toValue)
          )
        )
        |> blpLanguageExtension.primitivesExtension.toExpr
      expected = ExprType.PrimitiveType PrimitiveType.BoolType }
    { expr =
        Primitives.ExprExtension.Binary(
          Primitives.BinaryOperator.Plus,
          Expr.Value(
            (Primitives.ValueExtension.ConstInt 1
             |> blpLanguageExtension.primitivesExtension.toValue)
          ),
          Expr.Value(
            (Primitives.ValueExtension.ConstInt 2
             |> blpLanguageExtension.primitivesExtension.toValue)
          )
        )
        |> blpLanguageExtension.primitivesExtension.toExpr
      expected = ExprType.PrimitiveType PrimitiveType.IntType }
    { expr =
        Primitives.ExprExtension.Binary(
          Primitives.BinaryOperator.Minus,
          Expr.Value(
            (Primitives.ValueExtension.ConstInt 1
             |> blpLanguageExtension.primitivesExtension.toValue)
          ),
          Expr.Value(
            (Primitives.ValueExtension.ConstInt 2
             |> blpLanguageExtension.primitivesExtension.toValue)
          )
        )
        |> blpLanguageExtension.primitivesExtension.toExpr
      expected = ExprType.PrimitiveType PrimitiveType.IntType }
    { expr =
        Primitives.ExprExtension.Binary(
          Primitives.BinaryOperator.Times,
          Expr.Value(
            (Primitives.ValueExtension.ConstInt 1
             |> blpLanguageExtension.primitivesExtension.toValue)
          ),
          Expr.Value(
            (Primitives.ValueExtension.ConstInt 2
             |> blpLanguageExtension.primitivesExtension.toValue)
          )
        )
        |> blpLanguageExtension.primitivesExtension.toExpr
      expected = ExprType.PrimitiveType PrimitiveType.IntType }
    { expr =
        Primitives.ExprExtension.Binary(
          Primitives.BinaryOperator.DividedBy,
          Expr.Value(
            (Primitives.ValueExtension.ConstInt 1
             |> blpLanguageExtension.primitivesExtension.toValue)
          ),
          Expr.Value(
            (Primitives.ValueExtension.ConstInt 2
             |> blpLanguageExtension.primitivesExtension.toValue)
          )
        )
        |> blpLanguageExtension.primitivesExtension.toExpr
      expected = ExprType.PrimitiveType PrimitiveType.IntType }
    { expr =
        Primitives.ExprExtension.Binary(
          Primitives.BinaryOperator.Equals,
          Expr.Value(
            (Primitives.ValueExtension.ConstInt 1
             |> blpLanguageExtension.primitivesExtension.toValue)
          ),
          Expr.Value(
            (Primitives.ValueExtension.ConstInt 1
             |> blpLanguageExtension.primitivesExtension.toValue)
          )
        )
        |> blpLanguageExtension.primitivesExtension.toExpr
      expected = ExprType.PrimitiveType PrimitiveType.BoolType }
    { expr =
        Primitives.ExprExtension.Binary(
          Primitives.BinaryOperator.GreaterThan,
          Expr.Value(
            (Primitives.ValueExtension.ConstInt 1
             |> blpLanguageExtension.primitivesExtension.toValue)
          ),
          Expr.Value(
            (Primitives.ValueExtension.ConstInt 2
             |> blpLanguageExtension.primitivesExtension.toValue)
          )
        )
        |> blpLanguageExtension.primitivesExtension.toExpr
      expected = ExprType.PrimitiveType PrimitiveType.BoolType }
    { expr =
        Primitives.ExprExtension.Binary(
          Primitives.BinaryOperator.GreaterThanEquals,
          Expr.Value(
            (Primitives.ValueExtension.ConstInt 1
             |> blpLanguageExtension.primitivesExtension.toValue)
          ),
          Expr.Value(
            (Primitives.ValueExtension.ConstInt 2
             |> blpLanguageExtension.primitivesExtension.toValue)
          )
        )
        |> blpLanguageExtension.primitivesExtension.toExpr
      expected = ExprType.PrimitiveType PrimitiveType.BoolType }
    { expr =
        Primitives.ExprExtension.Binary(
          Primitives.BinaryOperator.Equals,
          Expr.Value(
            Value.Tuple
              [ (Primitives.ValueExtension.ConstBool true
                 |> blpLanguageExtension.primitivesExtension.toValue)
                (Primitives.ValueExtension.ConstInt 42
                 |> blpLanguageExtension.primitivesExtension.toValue) ]
          ),
          Expr.Value(
            Value.Tuple
              [ (Primitives.ValueExtension.ConstBool false
                 |> blpLanguageExtension.primitivesExtension.toValue)
                (Primitives.ValueExtension.ConstInt 13
                 |> blpLanguageExtension.primitivesExtension.toValue) ]
          )
        )
        |> blpLanguageExtension.primitivesExtension.toExpr
      expected = ExprType.PrimitiveType PrimitiveType.BoolType } ]

[<TestCaseSource(nameof simpleBinaryExpressionTestCases)>]
let ``Should typecheck simple binary expressions`` (testCase: SimpleBinaryExpressionTestCase) =
  let { expr = expr; expected = expected } = testCase

  match typeCheck expr with
  | Left value -> Assert.That(value, Is.EqualTo expected)
  | Right err -> Assert.Fail $"Expected success but got error: {err}"


type BinaryExpressionTestCaseRequiringUnification =
  { expr: Expr<BLPExprExtension, BLPValueExtension>
    expected: ExprType
    varTypes: VarTypes }

let binaryExpressionTestCasesRequiringUnification: BinaryExpressionTestCaseRequiringUnification list =
  [ { expr =
        Primitives.ExprExtension.Binary(
          Primitives.BinaryOperator.Or,
          Expr.VarLookup { VarName = "x" },
          Expr.Value(
            Primitives.ValueExtension.ConstBool true
            |> blpLanguageExtension.primitivesExtension.toValue
          )
        )
        |> blpLanguageExtension.primitivesExtension.toExpr
      expected = ExprType.PrimitiveType PrimitiveType.BoolType
      varTypes = Map.ofList [ { VarName = "x" }, ExprType.PrimitiveType PrimitiveType.BoolType ] }
    { expr =
        Primitives.ExprExtension.Binary(
          Primitives.BinaryOperator.Plus,
          Expr.VarLookup { VarName = "x" },
          Expr.Value(
            Primitives.ValueExtension.ConstInt 42
            |> blpLanguageExtension.primitivesExtension.toValue
          )
        )
        |> blpLanguageExtension.primitivesExtension.toExpr
      expected = ExprType.PrimitiveType PrimitiveType.IntType
      varTypes = Map.ofList [ { VarName = "x" }, ExprType.PrimitiveType PrimitiveType.IntType ] } ]

[<TestCaseSource(nameof binaryExpressionTestCasesRequiringUnification)>]
let ``Should typecheck binary expressions requiring unification``
  (testCase: BinaryExpressionTestCaseRequiringUnification)
  =
  let { expr = expr
        expected = expected
        varTypes = varTypes } =
    testCase

  match blpLanguageExtension.typeCheck Map.empty varTypes expr with
  | Left value -> Assert.That(value, Is.EqualTo expected)
  | Right err -> Assert.Fail $"Expected success but got error: {err}"


type SimpleIncorrectBinaryExpressionTestCase =
  { expr: Expr<BLPExprExtension, BLPValueExtension> }

let simpleIncorrectBinaryExpressionTestCases =
  [ { expr =
        Primitives.ExprExtension.Binary(
          Primitives.BinaryOperator.Or,
          Expr.Value(
            (Primitives.ValueExtension.ConstBool true
             |> blpLanguageExtension.primitivesExtension.toValue)
          ),
          Expr.Value(
            (Primitives.ValueExtension.ConstString "42"
             |> blpLanguageExtension.primitivesExtension.toValue)
          )
        )
        |> blpLanguageExtension.primitivesExtension.toExpr }
    { expr =
        Primitives.ExprExtension.Binary(
          Primitives.BinaryOperator.Equals,
          Expr.Value(
            (Primitives.ValueExtension.ConstBool true
             |> blpLanguageExtension.primitivesExtension.toValue)
          ),
          Expr.Value(
            (Primitives.ValueExtension.ConstString "42"
             |> blpLanguageExtension.primitivesExtension.toValue)
          )
        )
        |> blpLanguageExtension.primitivesExtension.toExpr }
    { expr =
        Primitives.ExprExtension.Binary(
          Primitives.BinaryOperator.GreaterThanEquals,
          Expr.Value(
            (Primitives.ValueExtension.ConstBool true
             |> blpLanguageExtension.primitivesExtension.toValue)
          ),
          Expr.Value(
            (Primitives.ValueExtension.ConstString "42"
             |> blpLanguageExtension.primitivesExtension.toValue)
          )
        )
        |> blpLanguageExtension.primitivesExtension.toExpr }
    { expr =
        Primitives.ExprExtension.Binary(
          Primitives.BinaryOperator.Equals,
          Expr.Value(
            Value.Tuple
              [ (Primitives.ValueExtension.ConstBool true
                 |> blpLanguageExtension.primitivesExtension.toValue)
                (Primitives.ValueExtension.ConstInt 42
                 |> blpLanguageExtension.primitivesExtension.toValue) ]
          ),
          Expr.Value(
            Value.Tuple
              [ (Primitives.ValueExtension.ConstBool false
                 |> blpLanguageExtension.primitivesExtension.toValue) ]
          )
        )
        |> blpLanguageExtension.primitivesExtension.toExpr } ]

[<TestCaseSource(nameof simpleIncorrectBinaryExpressionTestCases)>]
let ``Should not typecheck incorrect binary expressions`` (testCase: SimpleIncorrectBinaryExpressionTestCase) =
  let { expr = expr } = testCase

  match typeCheck expr with
  | Left _value -> Assert.Fail $"Expected error but got success"
  | Right _err -> ()

[<Test>]
let ``Should typecheck tuple`` () =
  let expr =
    Expr.Value(
      Value.Tuple
        [ (Primitives.ValueExtension.ConstBool true
           |> blpLanguageExtension.primitivesExtension.toValue)
          (Primitives.ValueExtension.ConstInt 42
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
          [ (Primitives.ValueExtension.ConstBool true
             |> blpLanguageExtension.primitivesExtension.toValue)
            (Primitives.ValueExtension.ConstInt 42
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

  match blpLanguageExtension.typeCheck Map.empty inputVarTypes expr with
  | Left value -> Assert.That(value, Is.EqualTo expected)
  | Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``Should typecheck var lookup should fail if not in seen variables`` () =
  let expr = Expr.VarLookup { VarName = "x" }

  let vars = Map.empty

  match blpLanguageExtension.typeCheck Map.empty vars expr with
  | Left _ -> Assert.Fail $"Expected error but got success"
  | Right err -> Assert.That(err.ToString(), Contains.Substring "x")

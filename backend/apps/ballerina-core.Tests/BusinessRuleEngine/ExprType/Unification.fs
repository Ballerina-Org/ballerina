module Ballerina.Core.Tests.BusinessRuleEngine.ExprType.Unification

open NUnit.Framework
open Ballerina.DSL.Expr.Types.Model
open Ballerina.DSL.Expr.Types.Unification
open Ballerina.Collections.Sum
open Ballerina.DSL.Expr.Model

[<Test>]
let ``Should unify unit types (sanity check)`` () =
  let unification =
    ExprType.Unify Map.empty Map.empty ExprType.UnitType ExprType.UnitType

  match unification with
  | Left value -> Assert.That(value, Is.EqualTo(UnificationConstraints.Zero()))
  | Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``Should unify two variables if they have the same type`` () =
  let varNameA: VarName = { VarName = "a" }
  let varNameB: VarName = { VarName = "b" }

  let vars =
    Map.empty.Add(varNameA, ExprType.PrimitiveType IntType).Add(varNameB, ExprType.PrimitiveType IntType)

  let unification =
    ExprType.Unify vars Map.empty (ExprType.VarType varNameA) (ExprType.VarType varNameB)

  match unification with
  | Left value -> Assert.That(value, Is.EqualTo(UnificationConstraints.Zero()))
  | Right err -> Assert.Fail $"Expected success but got error: {err}"

[<Test>]
let ``Should not unify two variables if they have different types`` () =
  let varNameA: VarName = { VarName = "a" }
  let varNameB: VarName = { VarName = "b" }

  let vars =
    Map.empty.Add(varNameA, ExprType.PrimitiveType IntType).Add(varNameB, ExprType.PrimitiveType StringType)

  let unification =
    ExprType.Unify vars Map.empty (ExprType.VarType varNameA) (ExprType.VarType varNameB)

  match unification with
  | Left value -> Assert.Fail $"Expected error but got success: {value}"
  | Right err -> Assert.That(err.ToString(), Contains.Substring "cannot be unified")

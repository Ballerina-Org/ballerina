module Ballerina.DSL.Codegen.Python.Tests.Common

open Ballerina.DSL.Expr.Types.Model

let normalize (s: string) = s.Replace("\r\n", "\n")

let createEnumCase (caseName: string) : CaseName * UnionCase =
  { CaseName = caseName },
  { CaseName = caseName
    Fields = ExprType.UnitType }

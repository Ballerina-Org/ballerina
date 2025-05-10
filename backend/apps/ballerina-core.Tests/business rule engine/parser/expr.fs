module Ballerina.Core.Tests.BusinessRuleEngine.Parser.Expr

open Ballerina.DSL.Expr.Model
open Ballerina.DSL.Parser.Expr
open FSharp.Data
open NUnit.Framework
open Common

let private parseExpr json = (Expr.Parse json).run ((), ())

[<Test>]
let ``Should parse boolean`` () =
  let json = JsonValue.Boolean true

  let result = parseExpr json

  assertSuccess result (Expr.Value(Value.ConstBool true), None)

[<Test>]
let ``Should parse string`` () =
  let json = JsonValue.String "string"
  let result = parseExpr json
  assertSuccess result (Expr.Value(Value.ConstString "string"), None)

[<Test>]
let ``Should parse int`` () =
  let json = JsonValue.Number 1m
  let result = parseExpr json
  assertSuccess result (Expr.Value(Value.ConstInt 1), None)

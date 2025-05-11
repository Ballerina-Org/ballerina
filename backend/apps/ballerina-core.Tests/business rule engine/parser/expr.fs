module Ballerina.Core.Tests.BusinessRuleEngine.Parser.Expr

open Ballerina.DSL.Expr.Model
open Ballerina.DSL.Parser.Expr
open FSharp.Data
open NUnit.Framework
open Common
open Ballerina.Collections.Sum

let private parseExpr json =
  (Expr.Parse json).run ((), ()) |> Sum.map2 fst fst

module ExprParserTests =
  [<Test>]
  let ``Should parse boolean`` () =
    let json = JsonValue.Boolean true

    let result = parseExpr json

    assertSuccess result (Expr.Value(Value.ConstBool true))

  [<Test>]
  let ``Should parse string`` () =
    let json = JsonValue.String "string"
    let result = parseExpr json
    assertSuccess result (Expr.Value(Value.ConstString "string"))

  [<Test>]
  let ``Should parse int`` () =
    let json = JsonValue.Number 1m
    let result = parseExpr json
    assertSuccess result (Expr.Value(Value.ConstInt 1))

  [<Test>]
  let ``Should parse binary operator`` () =
    let json =
      JsonValue.Record
        [| "kind", JsonValue.String "and"
           "operands", JsonValue.Array [| JsonValue.Boolean true; JsonValue.Boolean false |] |]

    let result = parseExpr json

    assertSuccess
      result
      (Expr.Binary(BinaryOperator.And, Expr.Value(Value.ConstBool true), Expr.Value(Value.ConstBool false)))

  [<Test>]
  let ``Should parse lambda`` () =
    let json =
      JsonValue.Record
        [| "kind", JsonValue.String "lambda"
           "parameter", JsonValue.String "x"
           "body", JsonValue.Boolean true |]

    let result = parseExpr json

    assertSuccess result (Expr.Value(Value.Lambda({ VarName = "x" }, Expr.Value(Value.ConstBool true))))


  [<Test>]
  let ``Should parse matchCase`` () =
    let json =
      JsonValue.Record
        [| "kind", JsonValue.String "matchCase"
           "operands",
           JsonValue.Array
             [| JsonValue.String "test"
                JsonValue.Record
                  [| "caseName", JsonValue.String "case1"
                     "handler",
                     JsonValue.Record
                       [| "kind", JsonValue.String "lambda"
                          "parameter", JsonValue.String "x"
                          "body", JsonValue.Boolean true |] |] |] |]


    let result = parseExpr json

    assertSuccess
      result
      (Expr.MatchCase(
        Expr.Value(Value.ConstString "test"),
        Map.ofList [ ("case1", ({ VarName = "x" }, Expr.Value(Value.ConstBool true))) ]
      ))

  [<Test>]
  let ``Should parse fieldLookup`` () =
    let json =
      JsonValue.Record
        [| "kind", JsonValue.String "fieldLookup"
           "operands", JsonValue.Array [| JsonValue.String "record"; JsonValue.String "fieldName" |] |]

    let result = parseExpr json

    assertSuccess result (Expr.RecordFieldLookup(Expr.Value(Value.ConstString "record"), "fieldName"))

  [<Test>]
  let ``Should parse isCase`` () =
    let json =
      JsonValue.Record
        [| "kind", JsonValue.String "isCase"
           "operands", JsonValue.Array [| JsonValue.String "value"; JsonValue.String "caseName" |] |]

    let result = parseExpr json

    assertSuccess result (Expr.IsCase("caseName", Expr.Value(Value.ConstString "value")))



  [<Test>]
  let ``Should parse varLookup`` () =
    let json =
      JsonValue.Record [| "kind", JsonValue.String "varLookup"; "varName", JsonValue.String "x" |]

    let result = parseExpr json

    assertSuccess result (Expr.VarLookup { VarName = "x" })

  [<Test>]
  let ``Should parse itemLookup`` () =
    let json =
      JsonValue.Record
        [| "kind", JsonValue.String "itemLookup"
           "operands", JsonValue.Array [| JsonValue.String "array"; JsonValue.Number 2m |] |]

    let result = parseExpr json

    assertSuccess result (Expr.Project(Expr.Value(Value.ConstString "array"), 2))

module Ballerina.Core.Tests.BusinessRuleEngine.Parser.Expr

open Ballerina.DSL.Expr.Model
open Ballerina.DSL.Parser.Expr
open FSharp.Data
open NUnit.Framework
open Common
open Ballerina.Collections.Sum
open Ballerina.Collections.NonEmptyList

let private parseExpr json =
  (Expr.Parse json).run ((), ()) |> Sum.map2 fst fst

module ExprParserTests =
  [<Test>]
  let ``Should parse unit`` () =
    let json = JsonValue.Record [| "kind", JsonValue.String "unit" |]
    let result = parseExpr json
    assertSuccess result (Expr.Value(Value.Unit))

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
    let json =
      JsonValue.Record [| "kind", JsonValue.String "int"; "value", JsonValue.String "1" |]

    let result = parseExpr json
    assertSuccess result (Expr.Value(Value.ConstInt 1))

  [<Test>]
  let ``Should parse int (backward compatibility)`` () =
    let json = JsonValue.Number 2m

    let result = parseExpr json
    assertSuccess result (Expr.Value(Value.ConstInt 2))

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
  let ``Should parse projection`` () =
    let json =
      JsonValue.Record
        [| "kind", JsonValue.String "itemLookup"
           "operands", JsonValue.Array [| JsonValue.String "array"; JsonValue.Number 2m |] |]

    let result = parseExpr json

    assertSuccess result (Expr.Project(Expr.Value(Value.ConstString "array"), 2))

  [<Test>]
  let ``Should parse record`` () =
    let json =
      JsonValue.Record
        [| "kind", JsonValue.String "record"
           "fields",
           JsonValue.Record
             [| "name", JsonValue.String "Alice"
                "age", JsonValue.Record [| "kind", JsonValue.String "int"; "value", JsonValue.String "30" |] |] |]

    let result = parseExpr json

    let expectedExpr =
      Expr.Value(Value.Record(Map.ofList [ ("name", Value.ConstString "Alice"); ("age", Value.ConstInt 30) ]))

    assertSuccess result expectedExpr

  [<Test>]
  let ``Should parse caseCons`` () =
    let json =
      JsonValue.Record
        [| "kind", JsonValue.String "caseCons"
           "case", JsonValue.String "caseName"
           "value", JsonValue.Boolean true |]

    let result = parseExpr json

    let expectedExpr = Expr.Value(Value.CaseCons("caseName", Value.ConstBool true))

    assertSuccess result expectedExpr

  [<Test>]
  let ``Should parse tuple`` () =
    let json =
      JsonValue.Record
        [| "kind", JsonValue.String "tuple"
           "elements", JsonValue.Array [| JsonValue.String "a"; JsonValue.String "b" |] |]

    let result = parseExpr json

    let expectedExpr =
      Expr.Value(Value.Tuple [ Value.ConstString "a"; Value.ConstString "b" ])

    assertSuccess result expectedExpr


module ExprToAndFromJsonTests =
  [<Test>]
  let ``Should convert to and from Json unit`` () =
    let expr = Expr.Value(Value.Unit)
    let result = expr |> Expr.ToJson |> Sum.bind parseExpr

    assertSuccess result expr

  [<Test>]
  let ``Should convert to and from Json boolean`` () =
    let expr = Expr.Value(Value.ConstBool true)
    let result = expr |> Expr.ToJson |> Sum.bind parseExpr

    assertSuccess result expr

  [<Test>]
  let ``Should convert to and from Json string`` () =
    let expr = Expr.Value(Value.ConstString "string")
    let result = expr |> Expr.ToJson |> Sum.bind parseExpr

    assertSuccess result expr

  [<Test>]
  let ``Should convert int to and from Json`` () =
    let expr = Expr.Value(Value.ConstInt 42)
    let result = expr |> Expr.ToJson |> Sum.bind parseExpr

    assertSuccess result expr

  [<Test>]
  let ``Should convert to and from Json binary operation`` () =
    let expr =
      Expr.Binary(BinaryOperator.Or, Expr.Value(Value.ConstBool true), Expr.Value(Value.ConstBool false))

    let result = expr |> Expr.ToJson |> Sum.bind parseExpr

    assertSuccess result expr

  [<Test>]
  let ``Should convert to and from Json varLookup`` () =
    let expr = Expr.VarLookup { VarName = "x" }
    let result = expr |> Expr.ToJson |> Sum.bind parseExpr

    assertSuccess result expr

  [<Test>]
  let ``Should convert to and from Json projection`` () =
    let expr = Expr.Project(Expr.Value(Value.ConstString "array"), 2)
    let result = expr |> Expr.ToJson |> Sum.bind parseExpr

    assertSuccess result expr


  [<Test>]
  let ``Should convert to and from Json lambda`` () =
    let expr =
      Expr.Value(Value.Lambda({ VarName = "x" }, Expr.Value(Value.ConstBool true)))

    let result = expr |> Expr.ToJson |> Sum.bind parseExpr

    assertSuccess result expr

  [<Test>]
  let ``Should convert to and from Json matchCase`` () =
    let expr =
      Expr.MatchCase(
        Expr.Value(Value.ConstString "test"),
        Map.ofList [ ("case1", ({ VarName = "x" }, Expr.Value(Value.ConstBool true))) ]
      )

    let result = expr |> Expr.ToJson |> Sum.bind parseExpr

    assertSuccess result expr

  [<Test>]
  let ``Should convert to and from Json fieldLookup`` () =
    let expr =
      Expr.RecordFieldLookup(Expr.Value(Value.ConstString "record"), "fieldName")

    let result = expr |> Expr.ToJson |> Sum.bind parseExpr

    assertSuccess result expr

  [<Test>]
  let ``Should convert to and from Json isCase`` () =
    let expr = Expr.IsCase("caseName", Expr.Value(Value.ConstString "value"))
    let result = expr |> Expr.ToJson |> Sum.bind parseExpr

    assertSuccess result expr

  [<Test>]
  let ``Should convert to and from Json record`` () =
    let expr =
      Expr.Value(Value.Record(Map.ofList [ ("name", Value.ConstString "Alice"); ("age", Value.ConstInt 30) ]))

    let result = expr |> Expr.ToJson |> Sum.bind parseExpr

    assertSuccess result expr

  [<Test>]
  let ``Should convert to and from Json caseCons`` () =
    let expr = Expr.Value(Value.CaseCons("caseName", Value.ConstInt 30))
    let result = expr |> Expr.ToJson |> Sum.bind parseExpr

    assertSuccess result expr

  [<Test>]
  let ``Should convert tuple to and from Json`` () =
    let expr = Expr.Value(Value.Tuple [ Value.ConstString "a"; Value.ConstString "b" ])

    let result = expr |> Expr.ToJson |> Sum.bind parseExpr

    assertSuccess result expr


module ExprToAndFromJsonRecursiveExpressions =

  [<Test>]
  let ``Should convert to and from Json nested match case`` () =
    let expr =
      Expr.MatchCase(
        Expr.Value(Value.ConstString "test"),
        Map.ofList
          [ ("case1",
             ({ VarName = "x" },
              Expr.Binary(BinaryOperator.And, Expr.Value(Value.ConstBool true), Expr.VarLookup { VarName = "x" }))) ]
      )

    let result = expr |> Expr.ToJson |> Sum.bind parseExpr

    assertSuccess result expr

  [<Test>]
  let ``Should convert to and from Json nested field lookup`` () =
    let expr =
      Expr.RecordFieldLookup(
        Expr.Binary(BinaryOperator.Or, Expr.Value(Value.ConstString "record1"), Expr.Value(Value.ConstString "record2")),
        "fieldName"
      )

    let result = expr |> Expr.ToJson |> Sum.bind parseExpr

    assertSuccess result expr

  [<Test>]
  let ``Should convert to and from Json nested isCase`` () =
    let expr =
      Expr.IsCase("caseName", Expr.Project(Expr.Value(Value.ConstString "array"), 2))

    let result = expr |> Expr.ToJson |> Sum.bind parseExpr

    assertSuccess result expr

  [<Test>]
  let ``Should convert to and from Json deeply nested binary operations`` () =
    let expr =
      Expr.Binary(
        BinaryOperator.And,
        Expr.Binary(
          BinaryOperator.Or,
          Expr.Value(Value.ConstBool true),
          Expr.Binary(BinaryOperator.And, Expr.Value(Value.ConstBool false), Expr.VarLookup { VarName = "x" })
        ),
        Expr.Value(Value.ConstBool true)
      )

    let result = expr |> Expr.ToJson |> Sum.bind parseExpr

    assertSuccess result expr

  [<Test>]
  let ``Should convert to and from Json nested lambda with match case`` () =
    let expr =
      Expr.Value(
        Value.Lambda(
          { VarName = "x" },
          Expr.MatchCase(
            Expr.VarLookup { VarName = "x" },
            Map.ofList [ ("case1", ({ VarName = "y" }, Expr.Value(Value.ConstBool true))) ]
          )
        )
      )

    let result = expr |> Expr.ToJson |> Sum.bind parseExpr

    assertSuccess result expr

module ExprParserErrorTests =
  [<Test>]
  let ``Should fail on invalid operator`` () =
    let json =
      JsonValue.Record
        [| "kind", JsonValue.String "invalidOperator"
           "operands", JsonValue.Array [| JsonValue.Boolean true; JsonValue.Boolean false |] |]

    let result = parseExpr json

    match result with
    | Left _ -> Assert.Fail "Expected error but got success"
    | Right _ -> Assert.Pass()

  [<Test>]
  let ``Should fail explicitly on not implemented`` () =
    let expr =
      Expr.MakeRecord(Map.ofList [ ("fieldName", Expr.Value(Value.ConstString "value")) ])

    let result = expr |> Expr.ToJson |> Sum.bind parseExpr

    match result with
    | Left _ -> Assert.Fail "Expected error but got success"
    | Right errors ->
      Assert.That(errors.Errors.Head.Message, Is.EqualTo "Error: MakeRecord not implemented")
      Assert.That(errors.Errors.Tail, Is.Empty)

  [<Test>]
  let ``Should return an error for each parser if the kind is invalid`` () =
    let json = JsonValue.Record [| "kind", JsonValue.String "invalid" |]

    let result = parseExpr json

    match result with
    | Left _ -> Assert.Fail "Expected error but got success"
    | Right errors -> Assert.That(errors.Errors |> NonEmptyList.ToList |> List.length, Is.GreaterThan 1)

  [<TestCase("int")>]
  [<TestCase("and")>]
  [<TestCase("lambda")>]
  [<TestCase("matchCase")>]
  [<TestCase("fieldLookup")>]
  [<TestCase("isCase")>]
  [<TestCase("record")>]
  [<TestCase("caseCons")>]
  [<TestCase("tuple")>]
  let ``Should only return an error for the specific parser if the kind is invalid`` (kind: string) =
    let json = JsonValue.Record [| "kind", JsonValue.String kind |]

    let result = parseExpr json

    match result with
    | Left _ -> Assert.Fail "Expected error but got success"
    | Right errors -> Assert.That(errors.Errors |> NonEmptyList.ToList |> List.length, Is.EqualTo 1)

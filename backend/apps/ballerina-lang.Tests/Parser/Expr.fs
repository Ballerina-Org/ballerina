module Ballerina.Cat.Tests.BusinessRuleEngine.Parser.Expr

open Ballerina.DSL.Expr.Model
open Ballerina.DSL.Parser.Expr
open FSharp.Data
open NUnit.Framework
open Common
open Ballerina.Collections.Sum
open Ballerina.Collections.NonEmptyList
open Ballerina.Errors
open Ballerina.DSL.Extensions.BLPLang
open Ballerina.DSL.Expr.Extensions.Primitives
open Ballerina.DSL.Expr.Extensions.Collections
open Unbound.AI.Extensions.AI

module ExprParserTests =

  [<Test>]
  let ``Should parse unit`` () =
    let json = JsonValue.Record [| "kind", JsonValue.String "unit" |]
    let result = Expr.Parse blpLanguageExtension.parse json
    assertSuccess result (Expr.Value(Value.Unit))

  [<Test>]
  let ``Should parse boolean`` () =
    let json = JsonValue.Boolean true

    let result = Expr.Parse blpLanguageExtension.parse json

    assertSuccess
      result
      (Expr.Value(
        PrimitivesValueExtension.ConstBool true
        |> blpLanguageExtension.primitivesExtension.toValue
      ))

  [<Test>]
  let ``Should parse string`` () =
    let json = JsonValue.String "string"
    let result = Expr.Parse blpLanguageExtension.parse json

    assertSuccess
      result
      (Expr.Value(
        PrimitivesValueExtension.ConstString "string"
        |> blpLanguageExtension.primitivesExtension.toValue
      ))

  [<Test>]
  let ``Should parse int`` () =
    let json =
      JsonValue.Record [| "kind", JsonValue.String "int"; "value", JsonValue.String "1" |]

    let result = Expr.Parse blpLanguageExtension.parse json

    assertSuccess
      result
      (Expr.Value(
        PrimitivesValueExtension.ConstInt 1
        |> blpLanguageExtension.primitivesExtension.toValue
      ))

  [<Test>]
  let ``Should parse int (backward compatibility)`` () =
    let json = JsonValue.Number 2m

    let result = Expr.Parse blpLanguageExtension.parse json

    assertSuccess
      result
      (Expr.Value(
        PrimitivesValueExtension.ConstInt 2
        |> blpLanguageExtension.primitivesExtension.toValue
      ))

  [<Test>]
  let ``Should parse binary operator`` () =
    let json =
      JsonValue.Record
        [| "kind", JsonValue.String "and"
           "operands", JsonValue.Array [| JsonValue.Boolean true; JsonValue.Boolean false |] |]

    let result = Expr.Parse blpLanguageExtension.parse json

    assertSuccess
      result
      (PrimitivesExprExtension.Binary(
        BinaryOperator.And,
        Expr.Value(
          PrimitivesValueExtension.ConstBool true
          |> blpLanguageExtension.primitivesExtension.toValue
        ),
        Expr.Value(
          PrimitivesValueExtension.ConstBool false
          |> blpLanguageExtension.primitivesExtension.toValue
        )
       )
       |> blpLanguageExtension.primitivesExtension.toExpr)

  [<Test>]
  let ``Should parse lambda`` () =
    let json =
      JsonValue.Record
        [| "kind", JsonValue.String "lambda"
           "parameter", JsonValue.String "x"
           "body", JsonValue.Record [| "kind", JsonValue.String "unit" |] |]

    let result = Expr.Parse blpLanguageExtension.parse json

    assertSuccess result (Expr.Value(Value.Lambda({ VarName = "x" }, None, None, Expr.Value(Value.Unit))))


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


    let result = Expr.Parse blpLanguageExtension.parse json

    assertSuccess
      result
      (Expr.MatchCase(
        Expr.Value(
          PrimitivesValueExtension.ConstString "test"
          |> blpLanguageExtension.primitivesExtension.toValue
        ),
        Map.ofList
          [ ("case1",
             ({ VarName = "x" },
              Expr.Value(
                PrimitivesValueExtension.ConstBool true
                |> blpLanguageExtension.primitivesExtension.toValue
              ))) ]
      ))

  [<Test>]
  let ``Should parse fieldLookup`` () =
    let json =
      JsonValue.Record
        [| "kind", JsonValue.String "fieldLookup"
           "operands", JsonValue.Array [| JsonValue.String "record"; JsonValue.String "fieldName" |] |]

    let result = Expr.Parse blpLanguageExtension.parse json

    assertSuccess
      result
      (Expr.RecordFieldLookup(
        Expr.Value(
          PrimitivesValueExtension.ConstString "record"
          |> blpLanguageExtension.primitivesExtension.toValue
        ),
        "fieldName"
      ))

  [<Test>]
  let ``Should parse varLookup`` () =
    let json =
      JsonValue.Record [| "kind", JsonValue.String "varLookup"; "varName", JsonValue.String "x" |]

    let result = Expr.Parse blpLanguageExtension.parse json

    assertSuccess result (Expr.VarLookup { VarName = "x" })

  [<Test>]
  let ``Should parse projection`` () =
    let json =
      JsonValue.Record
        [| "kind", JsonValue.String "itemLookup"
           "operands", JsonValue.Array [| JsonValue.String "array"; JsonValue.Number 2m |] |]

    let result = Expr.Parse blpLanguageExtension.parse json

    assertSuccess
      result
      (Expr.Project(
        Expr.Value(
          PrimitivesValueExtension.ConstString "array"
          |> blpLanguageExtension.primitivesExtension.toValue
        ),
        2
      ))

  [<Test>]
  let ``Should parse record`` () =
    let json =
      JsonValue.Record
        [| "kind", JsonValue.String "record"
           "fields",
           JsonValue.Record
             [| "name", JsonValue.String "Alice"
                "age", JsonValue.Record [| "kind", JsonValue.String "int"; "value", JsonValue.String "30" |] |] |]

    let result = Expr.Parse blpLanguageExtension.parse json

    let expectedExpr =
      Expr.MakeRecord(
        Map.ofList
          [ ("name",
             PrimitivesValueExtension.ConstString "Alice"
             |> blpLanguageExtension.primitivesExtension.toValue
             |> Expr.Value)
            ("age",
             PrimitivesValueExtension.ConstInt 30
             |> blpLanguageExtension.primitivesExtension.toValue
             |> Expr.Value) ]
      )

    assertSuccess result expectedExpr

  [<Test>]
  let ``Should parse caseCons`` () =
    let json =
      JsonValue.Record
        [| "kind", JsonValue.String "caseCons"
           "caseName", JsonValue.String "caseName"
           "value", JsonValue.String "abc" |]

    let result = Expr.Parse blpLanguageExtension.parse json

    let expectedExpr =
      Expr.MakeCase(
        "caseName",
        PrimitivesValueExtension.ConstString "abc"
        |> blpLanguageExtension.primitivesExtension.toValue
        |> Expr.Value
      )

    assertSuccess result expectedExpr

  [<Test>]
  let ``Should parse tuple`` () =
    let json =
      JsonValue.Record
        [| "kind", JsonValue.String "tuple"
           "elements", JsonValue.Array [| JsonValue.String "a"; JsonValue.String "b" |] |]

    let result = Expr.Parse blpLanguageExtension.parse json

    let expectedExpr =
      Expr.MakeTuple(
        [ PrimitivesValueExtension.ConstString "a"
          |> blpLanguageExtension.primitivesExtension.toValue
          |> Expr.Value
          PrimitivesValueExtension.ConstString "b"
          |> blpLanguageExtension.primitivesExtension.toValue
          |> Expr.Value ]
      )

    assertSuccess result expectedExpr

  [<Test>]
  let ``Should parse list`` () =
    let json =
      JsonValue.Record
        [| "kind", JsonValue.String "list"
           "elements", JsonValue.Array [| JsonValue.String "a"; JsonValue.String "b" |] |]

    let result = Expr.Parse blpLanguageExtension.parse json

    let expectedExpr =
      [ PrimitivesValueExtension.ConstString "a"
        |> blpLanguageExtension.primitivesExtension.toValue
        |> Expr.Value
        PrimitivesValueExtension.ConstString "b"
        |> blpLanguageExtension.primitivesExtension.toValue
        |> Expr.Value ]
      |> CollectionsExprExtension.List
      |> blpLanguageExtension.collectionsExtension.toExpr

    assertSuccess result expectedExpr

[<Test>]
let ``Should parse predict (AI extension)`` () =
  let json = JsonValue.Record [| "kind", JsonValue.String "predict" |]

  let result = Expr.Parse blpLanguageExtension.parse json

  let expectedExpr =
    AIValueExtension.Predict
    |> blpLanguageExtension.aiExtension.toValue
    |> Expr.Value

  assertSuccess result expectedExpr

[<Test>]
let ``Should parse let type`` () =
  let json =
    JsonValue.Record
      [| "kind", JsonValue.String "typeDecl"
         "typeName", JsonValue.String "name"
         "typeDef", JsonValue.String "unit"
         "in", JsonValue.Record [| "kind", JsonValue.String "unit" |] |]

  let result = Expr.Parse blpLanguageExtension.parse json

  let expectedExpr =
    Expr.LetType({ VarName = "name" }, ExprType.UnitType, Expr.Value Value.Unit)

  assertSuccess result expectedExpr

[<Test>]
let ``Should convert type applied predict (AI extension) to JSON as expression`` () =
  let value =
    AIValueExtension.TypeAppliedPredict
      { OutputType = ExprType.UnitType
        Refs = Map.empty }
    |> blpLanguageExtension.aiExtension.toValue
    |> Expr.Value

  let convertedJson =
    value
    |> Expr.ToJson blpLanguageExtension.toJson.expr blpLanguageExtension.toJson.value

  let expectedJson =
    JsonValue.Record
      [| "kind", JsonValue.String "Apply"
         "function", JsonValue.Record [| "kind", JsonValue.String "predict" |]
         "argument", JsonValue.String "unit" |]

  assertSuccess convertedJson expectedJson

let toAndFromJson
  (expr: Expr<BLPExprExtension, BLPValueExtension>)
  : Sum<Expr<BLPExprExtension, BLPValueExtension>, Errors> =
  expr
  |> Expr.ToJson blpLanguageExtension.toJson.expr blpLanguageExtension.toJson.value
  |> Sum.bind (Expr.Parse blpLanguageExtension.parse)

module ExprToAndFromJsonTests =
  [<Test>]
  let ``Should convert unit to and from Json`` () =
    let expr = Expr.Value(Value.Unit)
    let result = toAndFromJson expr
    assertSuccess result expr

  [<Test>]
  let ``Should convert boolean to and from Json`` () =
    let expr =
      Expr.Value(
        PrimitivesValueExtension.ConstBool true
        |> blpLanguageExtension.primitivesExtension.toValue
      )

    let result = toAndFromJson expr
    assertSuccess result expr


  [<Test>]
  let ``Should convert string to and from Json`` () =
    let expr =
      Expr.Value(
        PrimitivesValueExtension.ConstString "string"
        |> blpLanguageExtension.primitivesExtension.toValue
      )

    let result = toAndFromJson expr
    assertSuccess result expr


  [<Test>]
  let ``Should convert int to and from Json`` () =
    let expr =
      Expr.Value(
        PrimitivesValueExtension.ConstInt 42
        |> blpLanguageExtension.primitivesExtension.toValue
      )

    let result = toAndFromJson expr

    assertSuccess result expr

  [<Test>]
  let ``Should convert binary operation to and from Json`` () =
    let expr =
      PrimitivesExprExtension.Binary(
        BinaryOperator.Or,
        Expr.Value(
          PrimitivesValueExtension.ConstBool true
          |> blpLanguageExtension.primitivesExtension.toValue
        ),
        Expr.Value(
          PrimitivesValueExtension.ConstBool false
          |> blpLanguageExtension.primitivesExtension.toValue
        )
      )
      |> blpLanguageExtension.primitivesExtension.toExpr

    let result = toAndFromJson expr

    assertSuccess result expr

  [<Test>]
  let ``Should convert varLookup to and from Json`` () =
    let expr = Expr.VarLookup { VarName = "x" }
    let result = toAndFromJson expr

    assertSuccess result expr

  [<Test>]
  let ``Should convert projection to and from Json`` () =
    let expr =
      Expr.Project(
        Expr.Value(
          PrimitivesValueExtension.ConstString "array"
          |> blpLanguageExtension.primitivesExtension.toValue
        ),
        2
      )

    let result = toAndFromJson expr

    assertSuccess result expr


  [<Test>]
  let ``Should convert lambda to and from Json`` () =
    let expr =
      Expr.Value(
        Value.Lambda(
          { VarName = "x" },
          Some ExprType.UnitType,
          None,
          Expr.Value(
            PrimitivesValueExtension.ConstBool true
            |> blpLanguageExtension.primitivesExtension.toValue
          )
        )
      )

    let result = toAndFromJson expr

    assertSuccess result expr

  [<Test>]
  let ``Should convert lambda with return type to and from Json`` () =
    let expr =
      Expr.Value(
        Value.Lambda(
          { VarName = "x" },
          Some ExprType.UnitType,
          Some(ExprType.PrimitiveType PrimitiveType.BoolType),
          Expr.Value(
            PrimitivesValueExtension.ConstBool true
            |> blpLanguageExtension.primitivesExtension.toValue
          )
        )
      )

    let result = toAndFromJson expr

    assertSuccess result expr

  [<Test>]
  let ``Should convert matchCase to and from Json`` () =
    let expr =
      Expr.MatchCase(
        Expr.Value(
          PrimitivesValueExtension.ConstString "test"
          |> blpLanguageExtension.primitivesExtension.toValue
        ),
        Map.ofList
          [ ("case1",
             ({ VarName = "x" },
              Expr.Value(
                PrimitivesValueExtension.ConstBool true
                |> blpLanguageExtension.primitivesExtension.toValue
              ))) ]
      )

    let result = toAndFromJson expr

    assertSuccess result expr

  [<Test>]
  let ``Should convert fieldLookup to and from Json`` () =
    let expr =
      Expr.RecordFieldLookup(
        Expr.Value(
          PrimitivesValueExtension.ConstString "record"
          |> blpLanguageExtension.primitivesExtension.toValue
        ),
        "fieldName"
      )

    let result = toAndFromJson expr

    assertSuccess result expr

  [<Test>]
  let ``Should convert record to and from Json`` () =
    let expr =
      Expr.MakeRecord(
        Map.ofList
          [ ("name",
             PrimitivesValueExtension.ConstString "Alice"
             |> blpLanguageExtension.primitivesExtension.toValue
             |> Expr.Value)
            ("age",
             PrimitivesValueExtension.ConstInt 30
             |> blpLanguageExtension.primitivesExtension.toValue
             |> Expr.Value) ]
      )

    let result = toAndFromJson expr

    assertSuccess result expr

  [<Test>]
  [<Ignore("Not implemented")>]
  let ``Should convert caseCons to and from Json`` () =
    let expr = Expr.MakeCase("caseName", Expr.Value(Value.Unit))

    let result = toAndFromJson expr

    assertSuccess result expr

  [<Test>]
  [<Ignore("Not implemented")>]
  let ``Should convert tuple to and from Json`` () =
    let expr =
      Expr.MakeTuple(
        [ PrimitivesValueExtension.ConstString "a"
          |> blpLanguageExtension.primitivesExtension.toValue
          |> Expr.Value
          PrimitivesValueExtension.ConstString "b"
          |> blpLanguageExtension.primitivesExtension.toValue
          |> Expr.Value ]
      )

    let result = toAndFromJson expr

    assertSuccess result expr

  [<Test>]
  let ``Should convert list to and from Json`` () =
    let expr =
      [ PrimitivesValueExtension.ConstString "a"
        |> blpLanguageExtension.primitivesExtension.toValue
        |> Expr.Value
        PrimitivesValueExtension.ConstString "b"
        |> blpLanguageExtension.primitivesExtension.toValue
        |> Expr.Value ]
      |> CollectionsExprExtension.List
      |> blpLanguageExtension.collectionsExtension.toExpr

    let result = toAndFromJson expr

    assertSuccess result expr

module ExprToAndFromJsonRecursiveExpressions =

  [<Test>]
  let ``Should convert nested match case to and from Json`` () =
    let expr =
      Expr.MatchCase(
        Expr.Value(
          PrimitivesValueExtension.ConstString "test"
          |> blpLanguageExtension.primitivesExtension.toValue
        ),
        Map.ofList
          [ ("case1",
             ({ VarName = "x" },
              PrimitivesExprExtension.Binary(
                BinaryOperator.And,
                Expr.Value(
                  PrimitivesValueExtension.ConstBool true
                  |> blpLanguageExtension.primitivesExtension.toValue
                ),
                Expr.VarLookup { VarName = "x" }
              )
              |> blpLanguageExtension.primitivesExtension.toExpr)) ]
      )

    let result = toAndFromJson expr

    assertSuccess result expr

  [<Test>]
  let ``Should convert nested field lookup to and from Json`` () =
    let expr =
      Expr.RecordFieldLookup(
        PrimitivesExprExtension.Binary(
          BinaryOperator.Or,
          Expr.Value(
            PrimitivesValueExtension.ConstString "record1"
            |> blpLanguageExtension.primitivesExtension.toValue
          ),
          Expr.Value(
            PrimitivesValueExtension.ConstString "record2"
            |> blpLanguageExtension.primitivesExtension.toValue
          )
        )
        |> blpLanguageExtension.primitivesExtension.toExpr,
        "fieldName"
      )

    let result = toAndFromJson expr

    assertSuccess result expr

  [<Test>]
  let ``Should convert deeply nested binary operations to and from Json`` () =
    let expr =
      PrimitivesExprExtension.Binary(
        BinaryOperator.And,
        PrimitivesExprExtension.Binary(
          BinaryOperator.Or,
          Expr.Value(
            PrimitivesValueExtension.ConstBool true
            |> blpLanguageExtension.primitivesExtension.toValue
          ),
          PrimitivesExprExtension.Binary(
            BinaryOperator.And,
            Expr.Value(
              PrimitivesValueExtension.ConstBool false
              |> blpLanguageExtension.primitivesExtension.toValue
            ),
            Expr.VarLookup { VarName = "x" }
          )
          |> blpLanguageExtension.primitivesExtension.toExpr
        )
        |> blpLanguageExtension.primitivesExtension.toExpr,
        Expr.Value(
          PrimitivesValueExtension.ConstBool true
          |> blpLanguageExtension.primitivesExtension.toValue
        )
      )
      |> blpLanguageExtension.primitivesExtension.toExpr

    let result = toAndFromJson expr

    assertSuccess result expr

  [<Test>]
  let ``Should convert nested lambda with match case to and from Json`` () =
    let expr =
      Expr.Value(
        Value.Lambda(
          { VarName = "x" },
          None,
          None,
          Expr.MatchCase(
            Expr.VarLookup { VarName = "x" },
            Map.ofList
              [ ("case1",
                 ({ VarName = "y" },
                  Expr.Value(
                    PrimitivesValueExtension.ConstBool true
                    |> blpLanguageExtension.primitivesExtension.toValue
                  ))) ]
          )
        )
      )

    let result = toAndFromJson expr

    assertSuccess result expr

[<Test>]
let ``Should convert predict (AI extension) to and from Json`` () =
  let expr =
    AIValueExtension.Predict
    |> blpLanguageExtension.aiExtension.toValue
    |> Expr.Value

  let result = toAndFromJson expr
  assertSuccess result expr

[<Test>]
let ``Should convert let type to and from Json`` () =
  let expr =
    Expr.LetType({ VarName = "name" }, ExprType.UnitType, Expr.Value Value.Unit)

  let result = toAndFromJson expr
  assertSuccess result expr

[<Test>]
let ``Should convert generic apply to and from Json`` () =
  let expr =
    Expr.GenericApply(
      AIValueExtension.Predict
      |> blpLanguageExtension.aiExtension.toValue
      |> Expr.Value,
      ExprType.UnitType
    )

  let result = toAndFromJson expr
  assertSuccess result expr

module ExprParserErrorTests =
  [<Test>]
  let ``Should fail on invalid operator`` () =
    let json =
      JsonValue.Record
        [| "kind", JsonValue.String "invalidOperator"
           "operands", JsonValue.Array [| JsonValue.Boolean true; JsonValue.Boolean false |] |]

    let result = Expr.Parse blpLanguageExtension.parse json

    match result with
    | Left _ -> Assert.Fail "Expected error but got success"
    | Right _ -> Assert.Pass()

  [<Test>]
  let ``Should fail explicitly on not implemented`` () =
    let expr =
      Expr.Let(
        { VarName = "name" },
        Expr.Value Value.Unit,
        Expr.Value(
          PrimitivesValueExtension.ConstString "value"
          |> blpLanguageExtension.primitivesExtension.toValue
        )
      )

    let result = toAndFromJson expr

    match result with
    | Left _ -> Assert.Fail "Expected error but got success"
    | Right errors ->
      Assert.That(errors.Errors.Head.Message, Is.EqualTo "Error: Let not implemented")
      Assert.That(errors.Errors.Tail, Is.Empty)

  [<Test>]
  let ``Should return an error for each parser if the kind is invalid`` () =
    let json = JsonValue.Record [| "kind", JsonValue.String "invalid" |]

    let result = Expr.Parse blpLanguageExtension.parse json

    match result with
    | Left _ -> Assert.Fail "Expected error but got success"
    | Right errors -> Assert.That(errors.Errors |> NonEmptyList.ToList |> List.length, Is.GreaterThan 1)

  [<TestCase("int")>]
  [<TestCase("and")>]
  [<TestCase("lambda")>]
  [<TestCase("matchCase")>]
  [<TestCase("fieldLookup")>]
  [<TestCase("record")>]
  [<TestCase("caseCons")>]
  [<TestCase("tuple")>]
  let ``Should only return an error for the specific parser if the kind is invalid`` (kind: string) =
    let json = JsonValue.Record [| "kind", JsonValue.String kind |]

    let result = Expr.Parse blpLanguageExtension.parse json

    match result with
    | Left _ -> Assert.Fail "Expected error but got success"
    | Right errors -> Assert.That(errors.Errors |> NonEmptyList.ToList |> List.length, Is.EqualTo 1)

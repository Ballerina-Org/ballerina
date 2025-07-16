module Ballerina.Cat.Tests.BusinessRuleEngine.Parser.ExprType

open Ballerina.DSL.Expr.Model
open Ballerina.DSL.Parser.ExprType
open FSharp.Data
open NUnit.Framework
open Common
open Ballerina.DSL.Expr.Types.Model
open Ballerina.Collections.Sum
open Ballerina.Errors

let private parseExprType json = ExprType.Parse json

[<Test>]
let ``Should parse boolean`` () =
  let json = JsonValue.String "boolean"

  let result = parseExprType json

  assertSuccess result (ExprType.PrimitiveType PrimitiveType.BoolType)

[<Test>]
let ``Should parse union`` () =
  let json =
    JsonValue.Record
      [| "fun", JsonValue.String "Union"
         "args",
         JsonValue.Array
           [| JsonValue.Record [| "caseName", JsonValue.String "First"; "fields", JsonValue.Record [||] |]
              JsonValue.Record [| "caseName", JsonValue.String "Second"; "fields", JsonValue.Record [||] |] |] |]

  let result = parseExprType json

  assertSuccess
    result
    (ExprType.UnionType(
      Map.ofList
        [ { CaseName = "First" },
          { CaseName = "First"
            Fields = ExprType.UnitType }
          { CaseName = "Second" },
          { CaseName = "Second"
            Fields = ExprType.UnitType } ]
    ))

[<Test>]
let ``Should parse string`` () =
  let json = JsonValue.String "string"
  let result = parseExprType json
  assertSuccess result (ExprType.PrimitiveType PrimitiveType.StringType)

[<Test>]
let ``Should parse int (backward compatibility)`` () =
  let json = JsonValue.String "number"
  let result = parseExprType json
  assertSuccess result (ExprType.PrimitiveType PrimitiveType.IntType)

[<Test>]
let ``Should parse int`` () =
  let json = JsonValue.String "int"
  let result = parseExprType json
  assertSuccess result (ExprType.PrimitiveType PrimitiveType.IntType)

[<Test>]
let ``Should parse float`` () =
  let json = JsonValue.String "float"
  let result = parseExprType json
  assertSuccess result (ExprType.PrimitiveType PrimitiveType.FloatType)

[<Test>]
let ``Should parse date`` () =
  let json = JsonValue.String "Date"
  let result = parseExprType json
  assertSuccess result (ExprType.PrimitiveType PrimitiveType.DateOnlyType)

[<Test>]
let ``Should parse guid`` () =
  let json = JsonValue.String "guid"
  let result = parseExprType json
  assertSuccess result (ExprType.PrimitiveType PrimitiveType.GuidType)

[<Test>]
let ``Should parse unit`` () =
  let json = JsonValue.String "unit"
  let result = parseExprType json
  assertSuccess result (ExprType.UnitType)

[<Test>]
let ``Should parse option`` () =
  let json =
    JsonValue.Record
      [| "fun", JsonValue.String "Option"
         "args", JsonValue.Array [| JsonValue.String "string" |] |]

  let result = parseExprType json
  assertSuccess result (ExprType.OptionType(ExprType.PrimitiveType PrimitiveType.StringType))

[<Test>]
let ``Should parse list`` () =
  let json =
    JsonValue.Record
      [| "fun", JsonValue.String "List"
         "args", JsonValue.Array [| JsonValue.String "string" |] |]

  let result = parseExprType json
  assertSuccess result (ExprType.ListType(ExprType.PrimitiveType PrimitiveType.StringType))

[<Test>]
let ``Should parse set`` () =
  let json =
    JsonValue.Record
      [| "fun", JsonValue.String "MultiSelection"
         "args", JsonValue.Array [| JsonValue.String "string" |] |]

  let result = parseExprType json
  assertSuccess result (ExprType.SetType(ExprType.PrimitiveType PrimitiveType.StringType))

[<Test>]
let ``Should parse map`` () =
  let json =
    JsonValue.Record
      [| "fun", JsonValue.String "Map"
         "args", JsonValue.Array [| JsonValue.String "string"; JsonValue.String "number" |] |]

  let result = parseExprType json

  assertSuccess
    result
    (ExprType.MapType(ExprType.PrimitiveType PrimitiveType.StringType, ExprType.PrimitiveType PrimitiveType.IntType))

[<Test>]
let ``Should parse sum`` () =
  let json =
    JsonValue.Record
      [| "fun", JsonValue.String "Sum"
         "args", JsonValue.Array [| JsonValue.String "string"; JsonValue.String "number" |] |]

  let result = parseExprType json

  assertSuccess
    result
    (ExprType.SumType(ExprType.PrimitiveType PrimitiveType.StringType, ExprType.PrimitiveType PrimitiveType.IntType))

[<Test>]
let ``Should parse tuple`` () =
  let json =
    JsonValue.Record
      [| "fun", JsonValue.String "Tuple"
         "args",
         JsonValue.Array
           [| JsonValue.String "string"
              JsonValue.String "number"
              JsonValue.String "boolean" |] |]

  let result = parseExprType json

  assertSuccess
    result
    (ExprType.TupleType(
      [ ExprType.PrimitiveType PrimitiveType.StringType
        ExprType.PrimitiveType PrimitiveType.IntType
        ExprType.PrimitiveType PrimitiveType.BoolType ]
    ))

[<Test>]
let ``Should parse one`` () =
  let json =
    JsonValue.Record
      [| "fun", JsonValue.String "One"
         "args", JsonValue.Array [| JsonValue.String "string" |] |]

  let result = parseExprType json
  assertSuccess result (ExprType.OneType(ExprType.PrimitiveType PrimitiveType.StringType))

[<Test>]
let ``Should parse many`` () =
  let json =
    JsonValue.Record
      [| "fun", JsonValue.String "Many"
         "args", JsonValue.Array [| JsonValue.String "string" |] |]

  let result = parseExprType json
  assertSuccess result (ExprType.ManyType(ExprType.PrimitiveType PrimitiveType.StringType))

[<Test>]
let ``Should parse table`` () =
  let json =
    JsonValue.Record
      [| "fun", JsonValue.String "Table"
         "args", JsonValue.Array [| JsonValue.String "string" |] |]

  let result = parseExprType json
  assertSuccess result (ExprType.TableType(ExprType.PrimitiveType PrimitiveType.StringType))

[<Test>]
let ``Should parse readonly`` () =
  let json =
    JsonValue.Record
      [| "fun", JsonValue.String "ReadOnly"
         "args", JsonValue.Array [| JsonValue.String "string" |] |]

  let result = parseExprType json
  assertSuccess result (ExprType.ReadOnlyType(ExprType.PrimitiveType PrimitiveType.StringType))

[<Test>]
let ``Should parse record`` () =
  let json =
    JsonValue.Record [| "fields", JsonValue.Record [| "a", JsonValue.String "string"; "b", JsonValue.String "unit" |] |]

  let result = parseExprType json

  assertSuccess
    result
    (ExprType.RecordType(Map.ofList [ "a", ExprType.PrimitiveType PrimitiveType.StringType; "b", ExprType.UnitType ]))

module ExprTypeToAndFromJsonTests =
  let private toAndFromJson (expr: ExprType) : Sum<ExprType, Errors> =
    expr |> ExprType.ToJson |> ExprType.Parse

  [<Test>]
  let ``Should convert boolean to and from Json`` () =
    let expr = ExprType.PrimitiveType PrimitiveType.BoolType
    let result = toAndFromJson expr
    assertSuccess result expr

  [<Test>]
  let ``Should convert union to and from Json`` () =
    let expr =
      ExprType.UnionType(
        Map.ofList
          [ { CaseName = "First" },
            { CaseName = "First"
              Fields = ExprType.UnitType }
            { CaseName = "Second" },
            { CaseName = "Second"
              Fields = ExprType.UnitType } ]
      )

    let result = toAndFromJson expr
    assertSuccess result expr

  [<Test>]
  let ``Should convert string to and from Json`` () =
    let expr = ExprType.PrimitiveType PrimitiveType.StringType
    let result = toAndFromJson expr
    assertSuccess result expr

  [<Test>]
  let ``Should convert int to and from Json`` () =
    let expr = ExprType.PrimitiveType PrimitiveType.IntType
    let result = toAndFromJson expr
    assertSuccess result expr

  [<Test>]
  let ``Should convert float to and from Json`` () =
    let expr = ExprType.PrimitiveType PrimitiveType.FloatType
    let result = toAndFromJson expr
    assertSuccess result expr

  [<Test>]
  let ``Should convert date to and from Json`` () =
    let expr = ExprType.PrimitiveType PrimitiveType.DateOnlyType
    let result = toAndFromJson expr
    assertSuccess result expr

  [<Test>]
  let ``Should convert guid to and from Json`` () =
    let expr = ExprType.PrimitiveType PrimitiveType.GuidType
    let result = toAndFromJson expr
    assertSuccess result expr

  [<Test>]
  let ``Should convert unit to and from Json`` () =
    let expr = ExprType.UnitType
    let result = toAndFromJson expr
    assertSuccess result expr

  [<Test>]
  let ``Should convert option to and from Json`` () =
    let expr = ExprType.OptionType(ExprType.PrimitiveType PrimitiveType.StringType)
    let result = toAndFromJson expr
    assertSuccess result expr

  [<Test>]
  let ``Should convert list to and from Json`` () =
    let expr = ExprType.ListType(ExprType.PrimitiveType PrimitiveType.StringType)
    let result = toAndFromJson expr
    assertSuccess result expr

  [<Test>]
  let ``Should convert set to and from Json`` () =
    let expr = ExprType.SetType(ExprType.PrimitiveType PrimitiveType.StringType)
    let result = toAndFromJson expr
    assertSuccess result expr

  [<Test>]
  let ``Should convert map to and from Json`` () =
    let expr =
      ExprType.MapType(ExprType.PrimitiveType PrimitiveType.StringType, ExprType.PrimitiveType PrimitiveType.IntType)

    let result = toAndFromJson expr
    assertSuccess result expr

  [<Test>]
  let ``Should convert sum to and from Json`` () =
    let expr =
      ExprType.SumType(ExprType.PrimitiveType PrimitiveType.StringType, ExprType.PrimitiveType PrimitiveType.IntType)

    let result = toAndFromJson expr
    assertSuccess result expr

  [<Test>]
  let ``Should convert tuple to and from Json`` () =
    let expr =
      ExprType.TupleType(
        [ ExprType.PrimitiveType PrimitiveType.StringType
          ExprType.PrimitiveType PrimitiveType.IntType
          ExprType.PrimitiveType PrimitiveType.BoolType ]
      )

    let result = toAndFromJson expr
    assertSuccess result expr

  [<Test>]
  let ``Should convert one to and from Json`` () =
    let expr = ExprType.OneType(ExprType.PrimitiveType PrimitiveType.StringType)
    let result = toAndFromJson expr
    assertSuccess result expr

  [<Test>]
  let ``Should convert many to and from Json`` () =
    let expr = ExprType.ManyType(ExprType.PrimitiveType PrimitiveType.StringType)
    let result = toAndFromJson expr
    assertSuccess result expr

  [<Test>]
  let ``Should convert table to and from Json`` () =
    let expr = ExprType.TableType(ExprType.PrimitiveType PrimitiveType.StringType)
    let result = toAndFromJson expr
    assertSuccess result expr

  [<Test>]
  let ``Should convert readonly to and from Json`` () =
    let expr = ExprType.ReadOnlyType(ExprType.PrimitiveType PrimitiveType.StringType)
    let result = toAndFromJson expr
    assertSuccess result expr

  [<Test>]
  let ``Should convert record to and from Json`` () =
    let expr =
      ExprType.RecordType(Map.ofList [ "a", ExprType.PrimitiveType PrimitiveType.StringType; "b", ExprType.UnitType ])

    let result = toAndFromJson expr
    assertSuccess result expr

module ReadOnlyTypeStringRepresentationTests =
  [<Test>]
  let ``Should generate correct string representation for ReadOnlyType`` () =
    let expr = ExprType.ReadOnlyType(ExprType.PrimitiveType PrimitiveType.StringType)
    let result = expr.ToString()
    Assert.That(result, Is.EqualTo "ReadOnly<String>")

  [<Test>]
  let ``Should generate correct string representation for nested ReadOnlyType`` () =
    let expr =
      ExprType.ReadOnlyType(ExprType.OptionType(ExprType.PrimitiveType PrimitiveType.IntType))

    let result = expr.ToString()
    Assert.That(result, Is.EqualTo "ReadOnly<Option<Int>>")

  [<Test>]
  let ``Should generate correct string representation for ReadOnlyType with complex inner type`` () =
    let expr =
      ExprType.ReadOnlyType(ExprType.ListType(ExprType.PrimitiveType PrimitiveType.StringType))

    let result = expr.ToString()
    Assert.That(result, Is.EqualTo "ReadOnly<List<String>>")

module ReadOnlyTypeModelTests =
  [<Test>]
  let ``Should extract free type variables from ReadOnlyType`` () =
    let typeVar = { VarName = "TestType" }
    let readOnlyType = ExprType.ReadOnlyType(ExprType.LookupType typeVar)

    let freeVars = ExprType.GetTypesFreeVars readOnlyType

    Assert.That(freeVars, Does.Contain typeVar)

  [<Test>]
  let ``Should substitute types in ReadOnlyType`` () =
    let typeVar = { VarName = "T" }
    let originalType = ExprType.ReadOnlyType(ExprType.VarType typeVar)
    let substitutionType = ExprType.PrimitiveType PrimitiveType.StringType
    let substitutions = Map.ofList [ typeVar, substitutionType ]

    let result = ExprType.Substitute substitutions originalType

    let expectedType = ExprType.ReadOnlyType(substitutionType)
    Assert.That(result, Is.EqualTo expectedType)

  [<Test>]
  let ``Should handle ReadOnlyType with no free variables`` () =
    let readOnlyType =
      ExprType.ReadOnlyType(ExprType.PrimitiveType PrimitiveType.StringType)

    let freeVars = ExprType.GetTypesFreeVars readOnlyType

    Assert.That(freeVars, Is.Empty)

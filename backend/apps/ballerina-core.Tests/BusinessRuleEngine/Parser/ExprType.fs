module Ballerina.Core.Tests.BusinessRuleEngine.Parser.ExprType

open Ballerina.DSL.Expr.Types.Model
open Ballerina.DSL.Parser.ExprType
open FSharp.Data
open NUnit.Framework
open Common

let private parseExprType json =
  (ExprType.Parse contextActions json).run ((), ())

[<Test>]
let ``Should parse boolean`` () =
  let json = JsonValue.String "boolean"

  let result = parseExprType json

  assertSuccess result (ExprType.PrimitiveType PrimitiveType.BoolType, None)

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
     ),
     None)

[<Test>]
let ``Should parse string`` () =
  let json = JsonValue.String "string"
  let result = parseExprType json
  assertSuccess result (ExprType.PrimitiveType PrimitiveType.StringType, None)

[<Test>]
let ``Should parse int (backward compatibility)`` () =
  let json = JsonValue.String "number"
  let result = parseExprType json
  assertSuccess result (ExprType.PrimitiveType PrimitiveType.IntType, None)

[<Test>]
let ``Should parse int`` () =
  let json = JsonValue.String "int"
  let result = parseExprType json
  assertSuccess result (ExprType.PrimitiveType PrimitiveType.IntType, None)

[<Test>]
let ``Should parse float`` () =
  let json = JsonValue.String "float"
  let result = parseExprType json
  assertSuccess result (ExprType.PrimitiveType PrimitiveType.FloatType, None)

[<Test>]
let ``Should parse date`` () =
  let json = JsonValue.String "Date"
  let result = parseExprType json
  assertSuccess result (ExprType.PrimitiveType PrimitiveType.DateOnlyType, None)

[<Test>]
let ``Should parse guid`` () =
  let json = JsonValue.String "guid"
  let result = parseExprType json
  assertSuccess result (ExprType.PrimitiveType PrimitiveType.GuidType, None)

[<Test>]
let ``Should parse unit`` () =
  let json = JsonValue.String "unit"
  let result = parseExprType json
  assertSuccess result (ExprType.UnitType, None)

[<Test>]
let ``Should parse option`` () =
  let json =
    JsonValue.Record
      [| "fun", JsonValue.String "Option"
         "args", JsonValue.Array [| JsonValue.String "string" |] |]

  let result = parseExprType json
  assertSuccess result (ExprType.OptionType(ExprType.PrimitiveType PrimitiveType.StringType), None)

[<Test>]
let ``Should parse list`` () =
  let json =
    JsonValue.Record
      [| "fun", JsonValue.String "List"
         "args", JsonValue.Array [| JsonValue.String "string" |] |]

  let result = parseExprType json
  assertSuccess result (ExprType.ListType(ExprType.PrimitiveType PrimitiveType.StringType), None)

[<Test>]
let ``Should parse set`` () =
  let json =
    JsonValue.Record
      [| "fun", JsonValue.String "MultiSelection"
         "args", JsonValue.Array [| JsonValue.String "string" |] |]

  let result = parseExprType json
  assertSuccess result (ExprType.SetType(ExprType.PrimitiveType PrimitiveType.StringType), None)

[<Test>]
let ``Should parse map`` () =
  let json =
    JsonValue.Record
      [| "fun", JsonValue.String "Map"
         "args", JsonValue.Array [| JsonValue.String "string"; JsonValue.String "number" |] |]

  let result = parseExprType json

  assertSuccess
    result
    (ExprType.MapType(ExprType.PrimitiveType PrimitiveType.StringType, ExprType.PrimitiveType PrimitiveType.IntType),
     None)

[<Test>]
let ``Should parse sum`` () =
  let json =
    JsonValue.Record
      [| "fun", JsonValue.String "Sum"
         "args", JsonValue.Array [| JsonValue.String "string"; JsonValue.String "number" |] |]

  let result = parseExprType json

  assertSuccess
    result
    (ExprType.SumType(ExprType.PrimitiveType PrimitiveType.StringType, ExprType.PrimitiveType PrimitiveType.IntType),
     None)

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
     ),
     None)

[<Test>]
let ``Should parse one`` () =
  let json =
    JsonValue.Record
      [| "fun", JsonValue.String "One"
         "args", JsonValue.Array [| JsonValue.String "string" |] |]

  let result = parseExprType json
  assertSuccess result (ExprType.OneType(ExprType.PrimitiveType PrimitiveType.StringType), None)

[<Test>]
let ``Should parse many`` () =
  let json =
    JsonValue.Record
      [| "fun", JsonValue.String "Many"
         "args", JsonValue.Array [| JsonValue.String "string" |] |]

  let result = parseExprType json
  assertSuccess result (ExprType.ManyType(ExprType.PrimitiveType PrimitiveType.StringType), None)

[<Test>]
let ``Should parse table`` () =
  let json =
    JsonValue.Record
      [| "fun", JsonValue.String "Table"
         "args", JsonValue.Array [| JsonValue.String "string" |] |]

  let result = parseExprType json
  assertSuccess result (ExprType.TableType(ExprType.PrimitiveType PrimitiveType.StringType), None)

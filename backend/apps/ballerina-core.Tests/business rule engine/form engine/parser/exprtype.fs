module Ballerina.Core.Tests.BusinessRuleEngine.FormEngine.Parser.Expr

open Ballerina.Collections.Sum
open Ballerina.DSL.Expr.Types.Model
open Ballerina.DSL.FormEngine.Model
open Ballerina.DSL.FormEngine.Parser.ExprType
open FSharp.Data
open NUnit.Framework


let private assertSuccess<'T, 'E> (result: Sum<'T, 'E>) (expected: 'T) =
  match result with
  | Left value -> Assert.That(value, Is.EqualTo expected)
  | Right err -> Assert.Fail($"Expected success but got error: {err}")

let codegenConfig: CodeGenConfig =
  { Int =
      { GeneratedTypeName = "Int"
        DeltaTypeName = "Int"
        DefaultValue = "0"
        RequiredImport = None
        SupportedRenderers = Set.empty }
    Bool =
      { GeneratedTypeName = "Bool"
        DeltaTypeName = "Bool"
        DefaultValue = "false"
        RequiredImport = None
        SupportedRenderers = Set.empty }
    String =
      { GeneratedTypeName = "String"
        DeltaTypeName = "String"
        DefaultValue = "\"\""
        RequiredImport = None
        SupportedRenderers = Set.empty }
    Date =
      { GeneratedTypeName = "Date"
        DeltaTypeName = "Date"
        DefaultValue = "Date.MinValue"
        RequiredImport = Some "System"
        SupportedRenderers = Set.empty }
    Guid =
      { GeneratedTypeName = "Guid"
        DeltaTypeName = "Guid"
        DefaultValue = "Guid.Empty"
        RequiredImport = Some "System"
        SupportedRenderers = Set.empty }
    Unit =
      { GeneratedTypeName = "Unit"
        DeltaTypeName = "Unit"
        RequiredImport = None
        DefaultConstructor = "()"
        SupportedRenderers = Set.empty }
    Option =
      { GeneratedTypeName = "Option"
        RequiredImport = None
        DefaultConstructor = "None"
        DeltaTypeName = "Option"
        SupportedRenderers =
          {| Enum = Set.empty
             Stream = Set.empty
             Plain = Set.empty |} }
    Set =
      { GeneratedTypeName = "Set"
        RequiredImport = None
        DefaultConstructor = "Set.empty"
        DeltaTypeName = "Set"
        SupportedRenderers =
          {| Enum = Set.empty
             Stream = Set.empty |} }
    List =
      { GeneratedTypeName = "List"
        RequiredImport = None
        DeltaTypeName = "List"
        SupportedRenderers = Set.empty
        DefaultConstructor = "[]"
        MappingFunction = "List.map" }
    Table =
      { GeneratedTypeName = "Table"
        RequiredImport = None
        DeltaTypeName = "Table"
        SupportedRenderers = Set.empty
        DefaultConstructor = "Table.empty"
        MappingFunction = "Table.map" }
    One =
      { GeneratedTypeName = "One"
        RequiredImport = None
        DeltaTypeName = "DeltaOne"
        SupportedRenderers = Set.empty
        DefaultConstructor = "One.empty"
        MappingFunction = "One.map" }
    Many =
      { GeneratedTypeName = "Many"
        RequiredImport = None
        DeltaTypeName = "DeltaMany"
        SupportedRenderers = Set.empty
        DefaultConstructor = "Many.empty"
        MappingFunction = "Many.map" }
    Map =
      { GeneratedTypeName = "Map"
        RequiredImport = None
        DeltaTypeName = "Map"
        DefaultConstructor = "Map.empty"
        SupportedRenderers = Set.empty }
    Sum =
      { GeneratedTypeName = "Sum"
        RequiredImport = None
        DeltaTypeName = "Sum"
        LeftConstructor = "Left"
        RightConstructor = "Right"
        SupportedRenderers = Set.empty }
    Tuple = []
    Union = { SupportedRenderers = Set.empty }
    Record = { SupportedRenderers = Map.empty }
    Custom = Map.empty
    Generic = []
    IdentifierAllowedRegex = "^[a-zA-Z_][a-zA-Z0-9_]*$"
    DeltaBase =
      { GeneratedTypeName = "DeltaBase"
        RequiredImport = None }
    EntityNotFoundError =
      { GeneratedTypeName = "EntityNotFoundError"
        Constructor = "EntityNotFound"
        RequiredImport = None }
    OneNotFoundError =
      { GeneratedTypeName = "OneNotFoundError"
        Constructor = "OneNotFound"
        RequiredImport = None }
    LookupStreamNotFoundError =
      { GeneratedTypeName = "LookupStreamNotFoundError"
        Constructor = "LookupStreamNotFound"
        RequiredImport = None }
    ManyNotFoundError =
      { GeneratedTypeName = "ManyNotFoundError"
        Constructor = "ManyNotFound"
        RequiredImport = None }
    TableNotFoundError =
      { GeneratedTypeName = "TableNotFoundError"
        Constructor = "TableNotFound"
        RequiredImport = None }
    EntityNameAndDeltaTypeMismatchError =
      { GeneratedTypeName = "EntityNameAndDeltaTypeMismatchError"
        Constructor = "EntityNameAndDeltaTypeMismatch"
        RequiredImport = None }
    EnumNotFoundError =
      { GeneratedTypeName = "EnumNotFoundError"
        Constructor = "EnumNotFound"
        RequiredImport = None }
    InvalidEnumValueCombinationError =
      { GeneratedTypeName = "InvalidEnumValueCombinationError"
        Constructor = "InvalidEnumValueCombination"
        RequiredImport = None }
    StreamNotFoundError =
      { GeneratedTypeName = "StreamNotFoundError"
        Constructor = "StreamNotFound"
        RequiredImport = None }
    ContainerRenderers = Set.empty }


[<Test>]
let ``Should parse boolean`` () =
  let json = JsonValue.String "boolean"

  let result = (ExprType.Parse json).run (codegenConfig, ParsedFormsContext.Empty)

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

  let result = (ExprType.Parse json).run (codegenConfig, ParsedFormsContext.Empty)

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
  let result = (ExprType.Parse json).run (codegenConfig, ParsedFormsContext.Empty)
  assertSuccess result (ExprType.PrimitiveType PrimitiveType.StringType, None)

[<Test>]
let ``Should parse int`` () =
  let json = JsonValue.String "number"
  let result = (ExprType.Parse json).run (codegenConfig, ParsedFormsContext.Empty)
  assertSuccess result (ExprType.PrimitiveType PrimitiveType.IntType, None)

[<Test>]
let ``Should parse date`` () =
  let json = JsonValue.String "Date"
  let result = (ExprType.Parse json).run (codegenConfig, ParsedFormsContext.Empty)
  assertSuccess result (ExprType.PrimitiveType PrimitiveType.DateOnlyType, None)

[<Test>]
let ``Should parse guid`` () =
  let json = JsonValue.String "guid"
  let result = (ExprType.Parse json).run (codegenConfig, ParsedFormsContext.Empty)
  assertSuccess result (ExprType.PrimitiveType PrimitiveType.GuidType, None)

[<Test>]
let ``Should parse unit`` () =
  let json = JsonValue.String "unit"
  let result = (ExprType.Parse json).run (codegenConfig, ParsedFormsContext.Empty)
  assertSuccess result (ExprType.UnitType, None)

[<Test>]
let ``Should parse option`` () =
  let json =
    JsonValue.Record
      [| "fun", JsonValue.String "Option"
         "args", JsonValue.Array [| JsonValue.String "string" |] |]

  let result = (ExprType.Parse json).run (codegenConfig, ParsedFormsContext.Empty)
  assertSuccess result (ExprType.OptionType(ExprType.PrimitiveType PrimitiveType.StringType), None)

[<Test>]
let ``Should parse list`` () =
  let json =
    JsonValue.Record
      [| "fun", JsonValue.String "List"
         "args", JsonValue.Array [| JsonValue.String "string" |] |]

  let result = (ExprType.Parse json).run (codegenConfig, ParsedFormsContext.Empty)
  assertSuccess result (ExprType.ListType(ExprType.PrimitiveType PrimitiveType.StringType), None)

[<Test>]
let ``Should parse set`` () =
  let json =
    JsonValue.Record
      [| "fun", JsonValue.String "MultiSelection"
         "args", JsonValue.Array [| JsonValue.String "string" |] |]

  let result = (ExprType.Parse json).run (codegenConfig, ParsedFormsContext.Empty)
  assertSuccess result (ExprType.SetType(ExprType.PrimitiveType PrimitiveType.StringType), None)

[<Test>]
let ``Should parse map`` () =
  let json =
    JsonValue.Record
      [| "fun", JsonValue.String "Map"
         "args", JsonValue.Array [| JsonValue.String "string"; JsonValue.String "number" |] |]

  let result = (ExprType.Parse json).run (codegenConfig, ParsedFormsContext.Empty)

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

  let result = (ExprType.Parse json).run (codegenConfig, ParsedFormsContext.Empty)

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

  let result = (ExprType.Parse json).run (codegenConfig, ParsedFormsContext.Empty)

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

  let result = (ExprType.Parse json).run (codegenConfig, ParsedFormsContext.Empty)
  assertSuccess result (ExprType.OneType(ExprType.PrimitiveType PrimitiveType.StringType), None)

[<Test>]
let ``Should parse many`` () =
  let json =
    JsonValue.Record
      [| "fun", JsonValue.String "Many"
         "args", JsonValue.Array [| JsonValue.String "string" |] |]

  let result = (ExprType.Parse json).run (codegenConfig, ParsedFormsContext.Empty)
  assertSuccess result (ExprType.ManyType(ExprType.PrimitiveType PrimitiveType.StringType), None)

[<Test>]
let ``Should parse table`` () =
  let json =
    JsonValue.Record
      [| "fun", JsonValue.String "Table"
         "args", JsonValue.Array [| JsonValue.String "string" |] |]

  let result = (ExprType.Parse json).run (codegenConfig, ParsedFormsContext.Empty)
  assertSuccess result (ExprType.TableType(ExprType.PrimitiveType PrimitiveType.StringType), None)

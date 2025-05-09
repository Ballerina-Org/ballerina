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

  let parsedFormContext: ParsedFormsContext = ParsedFormsContext.Empty

  let result = (ExprType.Parse json).run (codegenConfig, parsedFormContext)

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

  let parsedFormContext: ParsedFormsContext = ParsedFormsContext.Empty

  let result = (ExprType.Parse json).run (codegenConfig, parsedFormContext)

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

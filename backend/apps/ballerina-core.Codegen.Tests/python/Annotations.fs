module Ballerina.DSL.Codegen.Python.Tests.Annotations

open NUnit.Framework
open Ballerina.DSL.Codegen.Python.LanguageConstructs.TypeAnnotations
open Ballerina.State.WithError
open Ballerina.DSL.Expr.Types.Model
open Ballerina.DSL.Codegen.Python.LanguageConstructs.Model
open Ballerina.Collections.Sum

let testConfig: PythonCodeGenConfig =
  { Int =
      { GeneratedTypeName = "int"
        RequiredImport = None }
    Float =
      { GeneratedTypeName = "Decimal"
        RequiredImport =
          Some
            { Source = "decimal"
              Target = "Decimal" } }
    Bool =
      { GeneratedTypeName = "bool"
        RequiredImport = None }
    String =
      { GeneratedTypeName = "str"
        RequiredImport = None }
    Date =
      { GeneratedTypeName = "date"
        RequiredImport = Some { Source = "datetime"; Target = "date" } }
    DateTime =
      { GeneratedTypeName = "datetime"
        RequiredImport =
          Some
            { Source = "datetime"
              Target = "datetime" } }
    Guid =
      { GeneratedTypeName = "UUID"
        RequiredImport = Some { Source = "uuid"; Target = "UUID" } }
    Unit =
      { GeneratedTypeName = "Literal[\"Unit\"]"
        RequiredImport =
          Some
            { Source = "typing"
              Target = "Literal" } }
    Option =
      { GeneratedTypeName = "Option"
        RequiredImport =
          Some
            { Source = "ballerina_core.primitives"
              Target = "Option" } }
    Set =
      { GeneratedTypeName = "frozenset"
        RequiredImport = None }
    List =
      { GeneratedTypeName = "Sequence"
        RequiredImport =
          Some
            { Source = "collections.abc"
              Target = "Sequence" } }
    Tuple =
      { GeneratedTypeName = "tuple"
        RequiredImport = None }
    Map =
      { GeneratedTypeName = "Mapping"
        RequiredImport =
          Some
            { Source = "collections.abc"
              Target = "Mapping" } }
    Sum =
      { GeneratedTypeName = "Sum"
        RequiredImport =
          Some
            { Source = "ballerina_core.primitives"
              Target = "Sum" } } }


[<Test>]
let ``Test should create annotation for unit`` () =
  let annotationResult = ExprType.GenerateTypeAnnotation UnitType

  let expectedAnnotation = "Literal[\"Unit\"]"

  let expectedImports: Set<Import> =
    Set.singleton
      { Source = "typing"
        Target = "Literal" }

  match annotationResult.run (testConfig, { UsedImports = Set.empty }) with
  | Left(annotation, Some finalState) ->
    Assert.That(annotation, Is.EqualTo expectedAnnotation)
    Assert.That(finalState.UsedImports, Is.EqualTo expectedImports)

  | Left(_, None) -> Assert.Fail "Expected the state to be Some, but it was None"
  | Right(errs, _) -> Assert.Fail $"Expected a Left result, but got Right with errors: %A{errs}"

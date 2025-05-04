module Ballerina.DSL.Codegen.Python.Tests.Annotations

open NUnit.Framework
open Ballerina.DSL.Codegen.Python.LanguageConstructs.TypeAnnotations
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


type AnnotationTestCase =
  { InputType: ExprType
    ExpectedAnnotation: string
    ExpectedImports: Set<Import> }

let annotationCases: AnnotationTestCase[] =
  [| { InputType = UnitType
       ExpectedAnnotation = "Literal[\"Unit\"]"
       ExpectedImports =
         Set.singleton
           { Source = "typing"
             Target = "Literal" } }
     { InputType = PrimitiveType IntType
       ExpectedAnnotation = "int"
       ExpectedImports = Set.empty }
     { InputType = PrimitiveType FloatType
       ExpectedAnnotation = "Decimal"
       ExpectedImports =
         Set.singleton
           { Source = "decimal"
             Target = "Decimal" } }
     { InputType = SetType(PrimitiveType IntType)
       ExpectedAnnotation = "frozenset[int]"
       ExpectedImports = Set.empty }
     { InputType = TupleType [ PrimitiveType IntType; PrimitiveType FloatType ]
       ExpectedAnnotation = "tuple[int, Decimal]"
       ExpectedImports =
         Set.singleton
           { Source = "decimal"
             Target = "Decimal" } }
     { InputType = SetType(PrimitiveType StringType)
       ExpectedAnnotation = "frozenset[str]"
       ExpectedImports = Set.empty }
     { InputType = SumType(UnitType, PrimitiveType FloatType)
       ExpectedAnnotation = "Sum[Literal[\"Unit\"], Decimal]"
       ExpectedImports =
         Set.ofList
           [ { Source = "typing"
               Target = "Literal" }
             { Source = "decimal"
               Target = "Decimal" }
             { Source = "ballerina_core.primitives"
               Target = "Sum" } ] }
     { InputType = TupleType [ PrimitiveType IntType; SetType(PrimitiveType StringType) ]
       ExpectedAnnotation = "tuple[int, frozenset[str]]"
       ExpectedImports = Set.empty } |]

[<Test; TestCaseSource(nameof annotationCases)>]
let ``Test should create annotation`` (case: AnnotationTestCase) =
  let annotationResult = ExprType.GenerateTypeAnnotation case.InputType

  match annotationResult.run (testConfig, { UsedImports = Set.empty }) with
  | Left(annotation, Some finalState) ->
    Assert.That(annotation, Is.EqualTo case.ExpectedAnnotation)

    Assert.That(
      finalState.UsedImports,
      Is.EqualTo case.ExpectedImports,
      $"Expected imports to be %A{case.ExpectedImports}, but got %A{finalState.UsedImports}"
    )

  | Left(_, None) -> Assert.Fail "Expected the state to be Some, but it was None"
  | Right(errs, _) -> Assert.Fail $"Expected a Left result, but got Right with errors: %A{errs}"

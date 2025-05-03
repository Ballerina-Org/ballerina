module Ballerina.DSL.Codegen.Python.Tests.Record

open NUnit.Framework
open Ballerina.DSL.Codegen.Python.LanguageConstructs.Record
open Ballerina.Core.StringBuilder
open Ballerina.DSL.Codegen.Python.LanguageConstructs.Model
open Ballerina.DSL.Codegen.Python.Tests.Common

[<Test>]
let ``Test should create non-empty record`` () =
  let recordType =
    { Name = "TestRecord"
      Fields =
        [ {| FieldName = "field1"
             FieldType = "str" |}
          {| FieldName = "field2"
             FieldType = "int" |} ] }

  let recordCode, imports = recordType |> PythonRecord.Generate

  let expected =
    """
@dataclass(frozen=True)
class TestRecord:
    field1: str
    field2: int

"""

  let expectedImports =
    Set.singleton
      { Source = "dataclasses"
        Target = "dataclass" }

  let actualNormalized = recordCode |> StringBuilder.ToString |> normalize
  let expectedNormalized = expected |> normalize

  Assert.That(
    actualNormalized,
    Is.EqualTo expectedNormalized,
    $"Expected:\n{expectedNormalized}\n\nBut was:\n{actualNormalized}"
  )

  Assert.That(imports, Is.EqualTo expectedImports)

[<Test>]
let ``Test should create empty record`` () =
  let recordType = { Name = "TestRecord"; Fields = [] }

  let recordCode, imports = recordType |> PythonRecord.Generate

  let expected =
    """
@dataclass(frozen=True)
class TestRecord:
    pass
"""

  let expectedImports =
    Set.singleton
      { Source = "dataclasses"
        Target = "dataclass" }

  let actualNormalized = recordCode |> StringBuilder.ToString |> normalize
  let expectedNormalized = expected |> normalize

  Assert.That(
    actualNormalized,
    Is.EqualTo expectedNormalized,
    $"Expected:\n{expectedNormalized}\n\nBut was:\n{actualNormalized}"
  )

  Assert.That(imports, Is.EqualTo expectedImports)

module ballerina_core.Codegen.Tests

open NUnit.Framework
open Ballerina.DSL.Codegen.Python.LanguageConstructs.Record
open Ballerina.DSL.Codegen.Python.LanguageConstructs.Union
open Ballerina.DSL.Codegen.Python.LanguageConstructs.Enum
open Ballerina.Core.StringBuilder
open Ballerina.Collections.NonEmptyList

let private normalize (s: string) = s.Replace("\r\n", "\n").Trim()

[<Test>]
let ``Test should create record`` () =
  let recordType =
    { Name = "TestRecord"
      Fields =
        [ {| FieldName = "field1"
             FieldType = "str" |}
          {| FieldName = "field2"
             FieldType = "int" |} ] }

  let recordCode, imports = recordType |> PythonRecord.Generate

  let expected =
    """@dataclass(frozen=True)
class TestRecord:
    field1: str
    field2: int

"""

  let expectedImports = Set.singleton "from dataclasses import dataclass"

  let actualNormalized = recordCode |> StringBuilder.ToString |> normalize
  let expectedNormalized = expected |> normalize

  Assert.That(
    actualNormalized,
    Is.EqualTo expectedNormalized,
    $"Expected:\n{expectedNormalized}\n\nBut was:\n{actualNormalized}"
  )

  Assert.That(imports, Is.EqualTo expectedImports)

[<Test>]
let ``Test should create union`` () =
  let unionType: PythonUnion =
    { Name = "TestUnion"
      Cases =
        NonEmptyList.Many(
          {| CaseName = "Case1"
             Fields =
              [ {| FieldName = "field1"
                   FieldType = "str" |} ] |},
          NonEmptyList.One
            {| CaseName = "Case2"
               Fields =
                [ {| FieldName = "field2"
                     FieldType = "int" |} ] |}
        ) }

  let unionCode, imports = unionType |> PythonUnion.Generate

  let expectedCode =
    """@dataclass(frozen=True)
class TestUnion:
    @dataclass(frozen=True)
    class Case1:
        field1: str

    @dataclass(frozen=True)
    class Case2:
        field2: int

    value: Case1 | Case2

"""

  let expectedImports = Set.singleton "from dataclasses import dataclass"
  let actualNormalized = unionCode |> StringBuilder.ToString |> normalize
  let expectedNormalized = expectedCode |> normalize

  Assert.That(
    actualNormalized,
    Is.EqualTo expectedNormalized,
    $"Expected:\n{expectedNormalized}\n\nBut was:\n{actualNormalized}"
  )

  Assert.That(imports, Is.EqualTo expectedImports)


[<Test>]
let ``Test should create enum`` () =
  let enumType =
    { Name = "TestEnum"
      Cases = [ {| Name = "Case1" |}; {| Name = "Case2" |} ] }

  let enumCode, imports = enumType |> PythonEnum.Generate


  let expectedCode =
    """@unique
class TestEnum(Enum):
    CASE1 = auto()
    CASE2 = auto()

"""

  let expectedImports = Set.singleton "from enum import Enum, auto"

  let actualNormalized = enumCode |> StringBuilder.ToString |> normalize
  let expectedNormalized = expectedCode |> normalize

  Assert.That(
    actualNormalized,
    Is.EqualTo expectedNormalized,
    $"Expected:\n{expectedNormalized}\n\nBut was:\n{actualNormalized}"
  )

  Assert.That(imports, Is.EqualTo expectedImports)

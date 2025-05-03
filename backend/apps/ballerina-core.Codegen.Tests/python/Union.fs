module Ballerina.DSL.Codegen.Python.Tests.Union

open NUnit.Framework
open Ballerina.DSL.Codegen.Python.LanguageConstructs.Union
open Ballerina.Core.StringBuilder
open Ballerina.Collections.NonEmptyList
open Ballerina.DSL.Codegen.Python.LanguageConstructs.Model
open Ballerina.DSL.Codegen.Python.Tests.Common

[<Test>]
let ``Test should create union`` () =
  let unionType: PythonUnion =
    { Name = "TestUnion"
      Cases = NonEmptyList.Many({| Name = "Case1"; Type = "str" |}, NonEmptyList.One {| Name = "Case2"; Type = "int" |}) }

  let unionCode, imports = unionType |> PythonUnion.Generate

  let expectedCode =
    """@dataclass(frozen=True)
class TestUnion:
    @dataclass(frozen=True)
    class Case1:
        _value: str

    @dataclass(frozen=True)
    class Case2:
        _value: int

    value: Case1 | Case2

"""

  let expectedImports =
    Set.singleton { Source = "dataclasses"
                    Target = "dataclass" }

  let actualNormalized = unionCode |> StringBuilder.ToString |> normalize
  let expectedNormalized = expectedCode |> normalize

  Assert.That(
    actualNormalized,
    Is.EqualTo expectedNormalized,
    $"Expected:\n{expectedNormalized}\n\nBut was:\n{actualNormalized}"
  )

  Assert.That(imports, Is.EqualTo expectedImports)

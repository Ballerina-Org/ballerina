module Ballerina.DSL.Codegen.Python.Tests.Unit

open NUnit.Framework
open Ballerina.DSL.Codegen.Python.LanguageConstructs.Unit
open Ballerina.DSL.Codegen.Python.LanguageConstructs.Model
open Ballerina.Core.StringBuilder
open Ballerina.DSL.Codegen.Python.Tests.Common

[<Test>]
let ``Test should create unit`` () =
  let unit = { Name = "Unit" }
  let code, imports = unit |> PythonUnit.Generate

  let expected =
    """Unit = Literal[None]
"""

  let expectedImports =
    Set.singleton { Source = "typing"
                    Target = "Literal" }

  let actualNormalized = code |> StringBuilder.ToString |> normalize
  let expectedNormalized = expected |> normalize

  Assert.That(
    actualNormalized,
    Is.EqualTo expectedNormalized,
    $"Expected:\n{expectedNormalized}\n\nBut was:\n{actualNormalized}"
  )

  Assert.That(imports, Is.EqualTo expectedImports)

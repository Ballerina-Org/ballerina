namespace Ballerina.DSL.Codegen.Python.LanguageConstructs


module Unit =

  open Ballerina.Core.StringBuilder
  open Ballerina.DSL.Codegen.Python.LanguageConstructs.Model


  type PythonUnit =
    { Name: string }

    static member Generate(unit: PythonUnit) =
      let unitCode = StringBuilder.One $"{unit.Name} = Literal[None]\n"

      let imports =
        { Source = "typing"
          Target = "Literal" }
        |> Set.singleton

      unitCode, imports

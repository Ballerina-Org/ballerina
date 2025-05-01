namespace Ballerina.DSL.Codegen.Python.LanguageConstructs


module Enum =

  open Ballerina.Core.StringBuilder

  let private indent = (+) "    "

  type PythonEnum =
    { Name: string
      Cases: List<{| Name: string |}> }

    static member Generate(enum: PythonEnum) =
      let enumCode =
        seq {
          yield StringBuilder.One $"@unique\n"
          yield StringBuilder.One $"class {enum.Name}(Enum):\n"

          yield
            seq {
              for case in enum.Cases do
                yield StringBuilder.One $"{case.Name.ToUpper()} = auto()\n"
            }
            |> StringBuilder.Many
            |> StringBuilder.Map indent

          yield StringBuilder.One "\n"
        }
        |> StringBuilder.Many

      let imports = "from enum import Enum, auto" |> Set.singleton

      enumCode, imports

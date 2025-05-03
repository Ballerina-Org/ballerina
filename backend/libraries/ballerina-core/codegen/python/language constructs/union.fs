namespace Ballerina.DSL.Codegen.Python.LanguageConstructs


module Union =
  open Ballerina.Core.StringBuilder
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Codegen.Python.LanguageConstructs.Model

  let private indent = (+) "    "

  let private appendCaseName (allCases: string) (nextCase: string) = $"{allCases} | {nextCase}"

  type PythonUnion =
    { Name: string
      Cases: NonEmptyList<{| Name: string; Type: string |}> }

    static member Generate(union: PythonUnion) =
      let unionCode =
        StringBuilder.Many(
          seq {
            yield StringBuilder.One $"@dataclass(frozen=True)\n"
            yield StringBuilder.One $"class {union.Name}:\n"


            yield
              seq {
                for case in union.Cases do
                  yield
                    StringBuilder.Many(
                      seq {
                        yield StringBuilder.One $"@dataclass(frozen=True)\n"
                        yield StringBuilder.One $"class {case.Name}:\n"

                        yield StringBuilder.One $"_value: {case.Type}\n" |> StringBuilder.Map indent
                      }
                    )
                    |> StringBuilder.Map indent

                  yield StringBuilder.One "\n"

                yield
                  StringBuilder.One
                    $"value: {union.Cases
                              |> NonEmptyList.map (fun c -> c.Name)
                              |> NonEmptyList.reduce appendCaseName}\n"
                  |> StringBuilder.Map indent
              }
              |> StringBuilder.Many
          }
        )

      let imports = "from dataclasses import dataclass" |> Import |> Set.singleton

      unionCode, imports

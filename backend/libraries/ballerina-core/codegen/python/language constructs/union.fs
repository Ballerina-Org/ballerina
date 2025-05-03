namespace Ballerina.DSL.Codegen.Python.LanguageConstructs


module Union =
  open Ballerina.Core.StringBuilder
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Codegen.Python.LanguageConstructs.Model
  open Ballerina.Core.String
  open Ballerina.DSL.Codegen.Python.LanguageConstructs.Common

  let private appendCaseName (allCases: string) (nextCase: string) = $"{allCases} | {nextCase}"

  type PythonUnion =
    { Name: NonEmptyString
      Cases: NonEmptyList<{| Name: string; Type: string |}> }

    static member Generate(union: PythonUnion) =
      let unionCode =
        StringBuilder.Many(
          seq {
            yield StringBuilder.One $"@dataclass(frozen=True)\n"
            yield StringBuilder.One $"class {NonEmptyString.AsString union.Name}:\n"


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
                              |> NonEmptyList.reduce appendCaseName}\n\n"
                  |> StringBuilder.Map indent
              }
              |> StringBuilder.Many
          }
        )

      let imports =
        { Source = "dataclasses"
          Target = "dataclass" }
        |> Set.singleton

      unionCode, imports

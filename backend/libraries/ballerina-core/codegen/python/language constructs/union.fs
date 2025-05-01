namespace Ballerina.DSL.Codegen.Python.LanguageConstructs


module Union =
  open Ballerina.Core.StringBuilder
  open Ballerina.Collections.NonEmptyList

  let private indent = (+) "    "

  let private appendCaseName (allCases: string) (nextCase: string) = $"{allCases} | {nextCase}"

  type PythonUnion =
    { Name: string
      Cases:
        NonEmptyList<
          {| CaseName: string
             Fields:
               List<
                 {| FieldName: string
                    FieldType: string |}
                > |}
         > }

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
                        yield StringBuilder.One $"class {case.CaseName}:\n"

                        for field in case.Fields do
                          yield
                            StringBuilder.One $"{field.FieldName}: {field.FieldType}\n"
                            |> StringBuilder.Map indent
                      }
                    )
                    |> StringBuilder.Map indent

                  yield StringBuilder.One "\n"

                yield
                  StringBuilder.One
                    $"value: {union.Cases
                              |> NonEmptyList.map (fun c -> c.CaseName)
                              |> NonEmptyList.reduce appendCaseName}\n"
                  |> StringBuilder.Map indent
              }
              |> StringBuilder.Many
          }
        )

      let imports = "from dataclasses import dataclass" |> Set.singleton

      unionCode, imports

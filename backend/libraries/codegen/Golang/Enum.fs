namespace Codegen.Golang

module Enum =
  open Ballerina.StdLib.StringBuilder
  open Codegen.Golang.Syntax

  type GolangEnumCase = { Name: string; Value: string }

  type GolangEnum =
    { Name: string
      AllCasesVariableName: string
      Cases: List<GolangEnumCase> }

    static member FromNameAndCases (name: string) (cases: List<GolangEnumCase>) (isPrivate: bool) : GolangEnum =
      match isPrivate with
      | true ->
        { Name = $"_{name}"
          AllCasesVariableName = $"_All{name}Cases"
          Cases = cases |> List.map (fun c -> { c with Name = $"_{c.Name}" }) }
      | false ->
        { Name = name
          AllCasesVariableName = $"All{name}Cases"
          Cases = cases }

    static member Generate(enum: GolangEnum) : StringBuilder =
      seq {
        yield StringBuilder.One $"type {enum.Name} string"
        yield StringBuilder.One "const ("

        for case in enum.Cases do
          yield StringBuilder.One $$"""  {{case.Name}} {{enum.Name}} = "{{case.Value}}{{"\""}}"""


        yield StringBuilder.One ")"
        yield StringBuilder.One $$"""var {{enum.AllCasesVariableName}} = [...]{{enum.Name}}{"""

        for case in enum.Cases do
          yield StringBuilder.One $$"""  {{case.Name}},"""

        yield StringBuilder.One "}"
      }
      |> StringBuilder.Many
      |> StringBuilder.Map appendNewline

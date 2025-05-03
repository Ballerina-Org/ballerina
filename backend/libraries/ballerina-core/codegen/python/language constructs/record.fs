namespace Ballerina.DSL.Codegen.Python.LanguageConstructs


module Record =

  open Ballerina.Core.StringBuilder
  open Ballerina.DSL.Codegen.Python.LanguageConstructs.Model
  open Ballerina.Core.String

  let private indent = (+) "    "

  type PythonRecord =
    { Name: NonEmptyString
      Fields:
        List<
          {| FieldName: string
             FieldType: string |}
         > }

    static member Generate(record: PythonRecord) =
      let recordCode =
        StringBuilder.Many(
          seq {
            let typeStart =
              $$"""@dataclass(frozen=True)"""
              + "\n"
              + $$"""class {{NonEmptyString.AsString record.Name}}:"""
              + "\n"

            let fieldDeclarations =
              seq {
                for field in record.Fields do
                  yield StringBuilder.One $"{field.FieldName}: {field.FieldType}\n"
              }
              |> StringBuilder.Many

            yield StringBuilder.One "\n"
            yield StringBuilder.One typeStart

            yield
              (if record.Fields.IsEmpty then
                 StringBuilder.One "pass"
               else
                 fieldDeclarations)
              |> StringBuilder.Map indent

            yield StringBuilder.One "\n"
          }
        )

      let imports =
        { Source = "dataclasses"
          Target = "dataclass" }
        |> Set.singleton

      recordCode, imports

namespace Codegen.Python

module ParsingTypes =
  type PythonInlineParser =
    | PythonInlineParser of string

    static member FromTypeName(typeName: string) : PythonInlineParser =
      PythonInlineParser(sprintf "%s_parser" typeName)

  type PythonInlineSerializer =
    | PythonInlineSerializer of string

    static member FromTypeName(typeName: string) : PythonInlineSerializer =
      PythonInlineSerializer(sprintf "%s_to_json" typeName)

namespace Codegen.Golang

module SerializationTypes =
  type GolangInlineDeserializer =
    | GolangInlineDeserializer of string

    static member FromTypeName(typeName: string) : GolangInlineDeserializer =
      GolangInlineDeserializer(sprintf "_%s_Deserializer" typeName)

  type GolangInlineSerializer =
    | GolangInlineSerializer of string

    static member FromTypeName(typeName: string) : GolangInlineSerializer =
      GolangInlineSerializer(sprintf "_%s_Serializer" typeName)

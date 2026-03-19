namespace Codegen.Golang

module Record =
  open Ballerina.StdLib.String
  open Ballerina.StdLib.StringBuilder
  open Codegen.Golang.Serialization
  open Codegen.Golang.Syntax

  type GolangRecordField =
    { FieldName: string
      FieldType: TypeAnnotation
      FieldSerializationName: string option }

  type GolangRecord =
    { Name: string
      Fields: List<GolangRecordField> }

  let private generateTypeDefinition (serializationSyntax: SerializationSyntax) (record: GolangRecord) : StringBuilder =
    let fieldDeclarations =
      StringBuilder.Many(
        seq {
          for field in record.Fields do
            let serializationTag =
              match serializationSyntax with
              | SerializationSyntax.FormEngine ->
                match field.FieldSerializationName with
                | Some serializationName -> sprintf "  `json:\"%s\"`" serializationName
                | None -> ""
              | SerializationSyntax.Next -> ""

            let (TypeAnnotation fieldType) = field.FieldType

            yield StringBuilder.One(sprintf "%s %s%s" field.FieldName.ToFirstUpper fieldType serializationTag)
        }
      )


    StringBuilder.Many
      [ StringBuilder.One(sprintf "type %s struct {" record.Name)
        fieldDeclarations |> StringBuilder.Map indent
        StringBuilder.One "}" ]

  let private generateConstructor (record: GolangRecord) : StringBuilder =
    let consParams =
      StringBuilder.Many(
        seq {
          for field in record.Fields do
            let (TypeAnnotation fieldType) = field.FieldType
            yield StringBuilder.One(sprintf "%s %s," field.FieldName fieldType)
        }
      )


    let consFieldInits =
      StringBuilder.Many(
        seq {
          for field in record.Fields do
            yield StringBuilder.One(sprintf "res.%s = %s" field.FieldName.ToFirstUpper field.FieldName)
        }
      )


    StringBuilder.Many
      [ StringBuilder.One(sprintf "func New%s(" record.Name)
        consParams |> StringBuilder.Map indent
        StringBuilder.One(sprintf ") %s {" record.Name)
        StringBuilder.One(sprintf "  var res %s" record.Name)
        consFieldInits |> StringBuilder.Map indent
        StringBuilder.One "  return res"
        StringBuilder.One "}" ]

  let private generateGetters (record: GolangRecord) : StringBuilder =
    StringBuilder.Many(
      seq {
        for field in record.Fields do
          let (TypeAnnotation fieldType) = field.FieldType
          yield StringBuilder.One(sprintf "func (r %s) Get%s() %s {" record.Name field.FieldName.ToFirstUpper fieldType)

          yield
            StringBuilder.One(sprintf "return r.%s" field.FieldName.ToFirstUpper)
            |> StringBuilder.Map indent

          yield StringBuilder.One "}"
      }
    )

  type GolangRecord with
    static member Generate (serializationSyntax: SerializationSyntax) (record: GolangRecord) : StringBuilder =
      StringBuilder.Many(
        seq {
          yield generateTypeDefinition serializationSyntax record
          yield generateConstructor record
          yield generateGetters record
        }
      )
      |> StringBuilder.Map appendNewline

namespace Codegen.Golang

module RecordSerialization =
  open Ballerina.StdLib.String
  open Ballerina.StdLib.StringBuilder
  open Codegen.Golang.Syntax
  open Codegen.Golang.SerializationTypes

  type GolangRecordFieldSerializerAndDeserializer =
    { FieldName: string
      FieldType: TypeAnnotation
      Serializer: string
      Deserializer: string }

  type GolangRecordSerializerAndDeserializer =
    { Name: string
      Fields: List<GolangRecordFieldSerializerAndDeserializer> }

  let private getRecordFieldByNameFunctionName = "getRecordFieldByName"

  let generateNextSerializer
    ({ Name = recordName; Fields = fields }: GolangRecordSerializerAndDeserializer)
    : StringBuilder =
    let functionDefinition (body: StringBuilder) : StringBuilder =
      let (GolangInlineSerializer serializerName) =
        GolangInlineSerializer.FromTypeName recordName

      StringBuilder.Many
        [ StringBuilder.One(
            sprintf """func %s(value %s) ballerina.Sum[error, json.RawMessage] {""" serializerName recordName
          )
          body |> StringBuilder.Map indent
          StringBuilder.One """}""" ]

    let serializedFieldVariableName (fieldName: string) : string = sprintf "%sSerialized" fieldName

    let constructRecord =
      seq {
        yield StringBuilder.One """[][2]json.RawMessage{"""

        for { FieldName = fieldName } in fields do
          yield
            StringBuilder.One(
              sprintf """{json.RawMessage(`"%s"`), %s},""" fieldName (serializedFieldVariableName fieldName)
            )
            |> StringBuilder.Map indent

        yield StringBuilder.One """},"""
      }
      |> StringBuilder.Many

    let rec generateFieldBindings (fields: List<GolangRecordFieldSerializerAndDeserializer>) : StringBuilder =
      match fields with
      | [] ->
        seq {
          yield StringBuilder.One """return ballerinaserialization.WrappedMarshal("""

          yield
            seq {
              yield StringBuilder.One """ballerinaserialization.NewRecordForSerialization("""

              yield constructRecord |> StringBuilder.Map indent

              yield StringBuilder.One """),"""
            }
            |> StringBuilder.Many
            |> StringBuilder.Map indent

          yield StringBuilder.One """)"""
        }
        |> StringBuilder.Many
      | { FieldName = fieldName
          Serializer = serializer } :: rest ->

        seq {
          yield StringBuilder.One """return ballerina.Bind("""

          yield
            seq {
              yield
                StringBuilder.One(
                  sprintf
                    """ballerinaserialization.WithContext("on %s", %s)(value.%s),"""
                    fieldName
                    serializer
                    fieldName.ToFirstUpper
                )

              yield
                StringBuilder.One(
                  sprintf
                    """func(%s json.RawMessage) ballerina.Sum[error, json.RawMessage] {"""
                    (serializedFieldVariableName fieldName)
                )

              yield generateFieldBindings rest |> StringBuilder.Map indent
              yield StringBuilder.One """},"""
            }
            |> StringBuilder.Many
            |> StringBuilder.Map indent

          yield StringBuilder.One """)"""
        }
        |> StringBuilder.Many

    functionDefinition (generateFieldBindings fields)

  let generateNextDeserializer
    ({ Name = recordName; Fields = fields }: GolangRecordSerializerAndDeserializer)
    : StringBuilder =
    let functionDefinition (body: StringBuilder) : StringBuilder =
      let (GolangInlineDeserializer deserializerName) =
        GolangInlineDeserializer.FromTypeName recordName

      StringBuilder.Many
        [ StringBuilder.One(
            sprintf """func %s(value json.RawMessage) ballerina.Sum[error, %s] {""" deserializerName recordName
          )
          body |> StringBuilder.Map indent
          StringBuilder.One """}""" ]

    let constructRecord =
      seq {
        yield StringBuilder.One(sprintf """%s{""" recordName)

        for { FieldName = fieldName } in fields do
          yield
            StringBuilder.One(sprintf """%s: %s,""" fieldName.ToFirstUpper fieldName)
            |> StringBuilder.Map indent

        yield StringBuilder.One """},"""

      }
      |> StringBuilder.Many


    let rec generateGetFieldByNameBindings (fields: List<GolangRecordFieldSerializerAndDeserializer>) : StringBuilder =
      match fields with
      | [] ->
        seq {
          yield StringBuilder.One(sprintf """return ballerina.Right[error, %s](""" recordName)

          yield constructRecord |> StringBuilder.Map indent

          yield StringBuilder.One """)"""
        }
        |> StringBuilder.Many
      | { FieldName = fieldName
          Deserializer = deserializer
          FieldType = TypeAnnotation fieldType } :: rest ->
        seq {
          yield StringBuilder.One """return ballerina.Bind("""

          yield
            seq {
              yield
                StringBuilder.One(
                  sprintf
                    """ballerina.Bind(%s("%s"), ballerinaserialization.WithContext("on field %s", %s)),"""
                    getRecordFieldByNameFunctionName
                    fieldName
                    fieldName
                    deserializer
                )

              yield
                StringBuilder.One(sprintf """func(%s %s) ballerina.Sum[error, %s] {""" fieldName fieldType recordName)

              yield generateGetFieldByNameBindings rest |> StringBuilder.Map indent

              yield StringBuilder.One """},"""

            }
            |> StringBuilder.Many
            |> StringBuilder.Map indent

          yield StringBuilder.One """)"""

        }
        |> StringBuilder.Many

    let fieldCountCheck =
      seq {

        yield StringBuilder.One(sprintf """if len(recordForSerialization) != %d {""" (List.length fields))

        yield
          StringBuilder.One(
            sprintf
              """return ballerina.Left[error, %s](fmt.Errorf("expected %d fields in record, got %%d", len(recordForSerialization)))"""
              recordName
              (List.length fields)
          )
          |> StringBuilder.Map indent

        yield StringBuilder.One """}"""
      }
      |> StringBuilder.Many
      |> StringBuilder.Map indent

    let deserializerBody = generateGetFieldByNameBindings fields

    let fullBody =
      seq {
        yield StringBuilder.One """return ballerina.Bind("""

        yield
          seq {
            yield StringBuilder.One """ballerinaserialization.DeserializeRecord(value),"""

            yield
              StringBuilder.One(
                sprintf
                  """func(recordForSerialization map[string]json.RawMessage) ballerina.Sum[error, %s] {"""
                  recordName
              )

            yield fieldCountCheck

            if List.isEmpty fields |> not then
              yield
                StringBuilder.One(
                  sprintf
                    """%s := ballerinaserialization.GetRecordFieldByName(recordForSerialization)"""
                    getRecordFieldByNameFunctionName
                )
                |> StringBuilder.Map indent

            yield deserializerBody |> StringBuilder.Map indent
            yield StringBuilder.One """},"""
          }
          |> StringBuilder.Many
          |> StringBuilder.Map indent

        yield StringBuilder.One """)"""

      }
      |> StringBuilder.Many

    functionDefinition fullBody


  type GolangRecordSerializerAndDeserializer with
    static member Generate(record: GolangRecordSerializerAndDeserializer) : StringBuilder * Set<GoImport> =
      let code =
        StringBuilder.Many [ generateNextSerializer record; generateNextDeserializer record ]
        |> StringBuilder.Map appendNewline

      let imports =
        Set.ofList
          [ GoImport "encoding/json"
            GoImport "fmt"
            GoImport "ballerina.com/core"
            GoImport "ballerina.com/core/ballerinaserialization" ]

      code, imports

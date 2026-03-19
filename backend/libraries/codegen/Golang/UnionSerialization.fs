namespace Codegen.Golang

module UnionSerialization =
  open Ballerina.StdLib.StringBuilder
  open Ballerina.Collections.NonEmptyList
  open Codegen.Golang.Syntax
  open Codegen.Golang.SerializationTypes

  type GolangUnionCaseSerializerAndDeserializer =
    { CaseName: string
      Type: TypeAnnotation
      Serializer: string
      Deserializer: string }

  type GolangUnionSerializerAndDeserializer =
    { Name: string
      Cases: NonEmptyList<GolangUnionCaseSerializerAndDeserializer> }

  let private generateNextSerializer
    ({ Name = unionName; Cases = cases }: GolangUnionSerializerAndDeserializer)
    : StringBuilder =

    let functionDefinition (body: StringBuilder) : StringBuilder =
      let (GolangInlineSerializer serializerName) =
        GolangInlineSerializer.FromTypeName unionName

      StringBuilder.Many
        [ StringBuilder.One(
            sprintf """func %s(value %s) ballerina.Sum[error, json.RawMessage] {""" serializerName unionName
          )
          body |> StringBuilder.Map indent
          StringBuilder.One """}""" ]

    let caseHandler
      ({ CaseName = caseName
         Type = TypeAnnotation fieldType
         Serializer = serializer }: GolangUnionCaseSerializerAndDeserializer)
      : StringBuilder =
      seq {
        yield StringBuilder.One(sprintf """func(item %s) (ballerina.Sum[error, json.RawMessage], error) {""" fieldType)

        yield
          seq {
            yield StringBuilder.One """return ballerina.Bind("""

            yield
              seq {
                yield
                  StringBuilder.One(
                    sprintf """ballerinaserialization.WithContext("on %s", %s)(item),""" caseName serializer
                  )

                yield
                  StringBuilder.One(sprintf """func(item json.RawMessage) ballerina.Sum[error, json.RawMessage] {""")

                yield
                  seq {
                    yield StringBuilder.One """return ballerina.Bind("""

                    yield
                      seq {
                        yield
                          StringBuilder.One(
                            sprintf """ballerinaserialization.NewUnionForSerialization("%s", item),""" caseName
                          )

                        yield StringBuilder.One """ballerinaserialization.WrappedMarshal,"""
                      }
                      |> StringBuilder.Many
                      |> StringBuilder.Map indent

                    yield StringBuilder.One """)"""
                  }
                  |> StringBuilder.Many
                  |> StringBuilder.Map indent

                yield StringBuilder.One """},"""
              }
              |> StringBuilder.Many
              |> StringBuilder.Map indent

            yield StringBuilder.One """), nil"""
          }
          |> StringBuilder.Many
          |> StringBuilder.Map indent

        yield StringBuilder.One """},"""
      }
      |> StringBuilder.Many

    let serializerBody =
      seq {
        yield StringBuilder.One(sprintf """serialized, err := Match%s(""" unionName)
        yield StringBuilder.One """value,""" |> StringBuilder.Map indent
        yield cases |> Seq.map caseHandler |> StringBuilder.Many |> StringBuilder.Map indent
        yield StringBuilder.One """)"""
        yield StringBuilder.One """if err != nil {"""
        yield StringBuilder.One """  return ballerina.Left[error, json.RawMessage](err)"""
        yield StringBuilder.One """}"""
        yield StringBuilder.One """return serialized"""
      }
      |> StringBuilder.Many

    functionDefinition serializerBody

  let private generateNextDeserializer
    ({ Name = unionName; Cases = cases }: GolangUnionSerializerAndDeserializer)
    : StringBuilder =
    let functionDefinition (body: StringBuilder) : StringBuilder =
      let (GolangInlineDeserializer deserializerName) =
        GolangInlineDeserializer.FromTypeName unionName

      StringBuilder.Many
        [ StringBuilder.One(
            sprintf """func %s(value json.RawMessage) ballerina.Sum[error, %s] {""" deserializerName unionName
          )
          body |> StringBuilder.Map indent
          StringBuilder.One """}""" ]

    let caseHandler
      ({ CaseName = caseName
         Type = TypeAnnotation _
         Deserializer = deserializer }: GolangUnionCaseSerializerAndDeserializer)
      : StringBuilder =
      seq {
        yield StringBuilder.One(sprintf """case string(_%s%s):""" unionName caseName)

        yield
          seq {
            yield StringBuilder.One """return ballerina.MapRight("""

            yield
              seq {
                yield
                  StringBuilder.One(
                    sprintf
                      """ballerinaserialization.WithContext("on case %s", %s)(unionForSerialization.Value[1]),"""
                      caseName
                      deserializer
                  )

                yield StringBuilder.One(sprintf """New%s%s,""" unionName caseName)
              }
              |> StringBuilder.Many
              |> StringBuilder.Map indent


            yield StringBuilder.One """)"""
          }
          |> StringBuilder.Many
          |> StringBuilder.Map indent
      }
      |> StringBuilder.Many

    let deserializerBody =
      seq {

        yield StringBuilder.One """return ballerina.Bind("""

        yield
          seq {
            yield StringBuilder.One """ballerinaserialization.DeserializeUnion(value),"""

            yield
              StringBuilder.One(
                sprintf
                  """func(unionForSerialization ballerinaserialization.UnionForSerialization) ballerina.Sum[error, %s] {"""
                  unionName
              )

            yield
              seq {
                yield StringBuilder.One """return ballerina.Bind("""

                yield
                  seq {

                    yield StringBuilder.One """unionForSerialization.GetCaseName(),"""

                    yield StringBuilder.One(sprintf """func(caseName string) ballerina.Sum[error, %s] {""" unionName)

                    yield
                      seq {

                        yield StringBuilder.One """switch caseName {"""

                        yield
                          seq {
                            yield! cases |> Seq.map caseHandler
                            yield StringBuilder.One """default:"""

                            yield
                              StringBuilder.One(
                                sprintf
                                  """return ballerina.Left[error, %s](fmt.Errorf("unknown union case: %%s", caseName))"""
                                  unionName
                              )
                              |> StringBuilder.Map indent

                          }
                          |> StringBuilder.Many
                          |> StringBuilder.Map indent

                        yield StringBuilder.One """}"""
                      }
                      |> StringBuilder.Many
                      |> StringBuilder.Map indent

                    yield StringBuilder.One """},"""
                  }
                  |> StringBuilder.Many
                  |> StringBuilder.Map indent

                yield StringBuilder.One """)"""
              }
              |> StringBuilder.Many
              |> StringBuilder.Map indent

            yield StringBuilder.One """},"""
          }
          |> StringBuilder.Many
          |> StringBuilder.Map indent

        yield StringBuilder.One """)"""
      }
      |> StringBuilder.Many

    functionDefinition deserializerBody

  type GolangUnionSerializerAndDeserializer with
    static member Generate(union: GolangUnionSerializerAndDeserializer) : StringBuilder * Set<GoImport> =
      let imports =
        Set.ofList
          [ GoImport "ballerina.com/core"
            GoImport "fmt"
            GoImport "encoding/json"
            GoImport "ballerina.com/core/ballerinaserialization" ]

      let code =
        StringBuilder.Many [ generateNextSerializer union; generateNextDeserializer union ]
        |> StringBuilder.Map appendNewline

      code, imports

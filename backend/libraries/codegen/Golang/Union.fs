namespace Codegen.Golang

module Union =
  open Ballerina.StdLib.StringBuilder
  open Ballerina.Collections.NonEmptyList
  open Codegen.Golang.Serialization
  open Codegen.Golang.Syntax

  type GolangUnionCase =
    { CaseName: string
      Type: TypeAnnotation }

  type GolangUnion =
    { Name: string
      Cases: NonEmptyList<GolangUnionCase> }

  let private generateDiscriminatorType (unionName: string) : string = sprintf "_%sCases" unionName

  let private generateDiscriminatorCaseName (unionName: string) (caseName: string) : string =
    sprintf "_%s%s" unionName caseName

  let private generateUnionTypeDefinition (union: GolangUnion) : StringBuilder =
    let cases = union.Cases

    let generateFieldDeclaration (caseName: string) (TypeAnnotation typeAnnotation) : StringBuilder =
      StringBuilder.One(sprintf "  _%s *%s" caseName typeAnnotation)

    StringBuilder.Many(
      seq {
        yield StringBuilder.One(sprintf "type %s string" (generateDiscriminatorType union.Name))
        yield StringBuilder.One "const ("

        for case in cases do
          yield
            StringBuilder.One(
              sprintf
                "  %s %s = \"%s\""
                (generateDiscriminatorCaseName union.Name case.CaseName)
                (generateDiscriminatorType union.Name)
                case.CaseName
            )

        yield StringBuilder.One ")"
        yield StringBuilder.One(sprintf "type %s struct {" union.Name)
        yield StringBuilder.One(sprintf "  discriminator %s" (generateDiscriminatorType union.Name))

        for case in cases do
          yield generateFieldDeclaration case.CaseName case.Type

        yield StringBuilder.One "}"
      }
    )

  let private generateMarshalJSON (union: GolangUnion) : StringBuilder =
    let cases = union.Cases

    let generateFieldDeclaration (caseName: string) (TypeAnnotation typeAnnotation) : StringBuilder =
      StringBuilder.One(sprintf "%s *%s" caseName typeAnnotation)

    StringBuilder.Many(
      seq {
        yield StringBuilder.One(sprintf "var _ json.Marshaler = %s{}" union.Name)
        yield StringBuilder.One(sprintf "func (d %s) MarshalJSON() ([]byte, error) {" union.Name)

        yield
          seq {
            yield StringBuilder.One "return json.Marshal(struct {"

            yield
              seq {
                yield StringBuilder.One(sprintf "Discriminator %s" (generateDiscriminatorType union.Name))

                for case in cases do
                  yield generateFieldDeclaration case.CaseName case.Type
              }
              |> StringBuilder.Many
              |> StringBuilder.Map indent

            yield StringBuilder.One "}{"

            yield StringBuilder.One "  Discriminator: d.discriminator,"

            for case in cases do
              yield StringBuilder.One(sprintf "  %s: d._%s," case.CaseName case.CaseName)

            yield StringBuilder.One "})"
          }
          |> StringBuilder.Many
          |> StringBuilder.Map indent

        yield StringBuilder.One "}"
      }
    )

  let private generateUnmarshalJSON (union: GolangUnion) : StringBuilder =
    let cases = union.Cases

    let generateFieldDeclaration (caseName: string) (TypeAnnotation typeAnnotation) : StringBuilder =
      StringBuilder.One(sprintf "%s *%s" caseName typeAnnotation)

    StringBuilder.Many(
      seq {
        yield StringBuilder.One(sprintf "var _ json.Unmarshaler = &%s{}" union.Name)
        yield StringBuilder.One(sprintf "func (d *%s) UnmarshalJSON(data []byte) error {" union.Name)

        yield
          seq {
            yield StringBuilder.One "var tmp struct {"

            yield
              seq {
                yield StringBuilder.One(sprintf "Discriminator %s" (generateDiscriminatorType union.Name))

                for case in cases do
                  yield generateFieldDeclaration case.CaseName case.Type
              }
              |> StringBuilder.Many
              |> StringBuilder.Map indent


            yield StringBuilder.One "}"
            yield StringBuilder.One "dec := json.NewDecoder(bytes.NewReader(data))"
            yield StringBuilder.One "dec.DisallowUnknownFields()"
            yield StringBuilder.One "if err := dec.Decode(&tmp); err != nil {"
            yield StringBuilder.One "return err" |> StringBuilder.Map indent
            yield StringBuilder.One "}"
            yield StringBuilder.One "d.discriminator = tmp.Discriminator"

            for case in cases do
              yield StringBuilder.One(sprintf "d._%s = tmp.%s" case.CaseName case.CaseName)

            yield StringBuilder.One "return nil"
          }
          |> StringBuilder.Many
          |> StringBuilder.Map indent

        yield StringBuilder.One "}"
      }
    )

  let private generateCaseConstructors (union: GolangUnion) : StringBuilder =
    let generateCaseConstructor (unionName: string) (caseName: string) (TypeAnnotation typeAnnotation) : StringBuilder =
      StringBuilder.Many(
        seq {
          yield StringBuilder.One(sprintf "func New%s%s(value %s) %s {" unionName caseName typeAnnotation unionName)

          yield
            seq {

              yield StringBuilder.One(sprintf "var res %s" union.Name)

              yield
                StringBuilder.One(sprintf "res.discriminator = %s" (generateDiscriminatorCaseName unionName caseName))

              yield StringBuilder.One(sprintf "res._%s = &value" caseName)
              yield StringBuilder.One "return res"
            }
            |> StringBuilder.Many
            |> StringBuilder.Map indent

          yield StringBuilder.One "}"
        }
      )

    StringBuilder.Many(
      union.Cases
      |> Seq.map (fun c -> generateCaseConstructor union.Name c.CaseName c.Type)
    )

  let private generateMatchFunction (union: GolangUnion) : StringBuilder =
    let cases = union.Cases

    let generateHandler (unionName: string) (caseName: string) (TypeAnnotation typeAnnotation) : StringBuilder =
      StringBuilder.One(sprintf "on%s%s func(%s) (Result,error)," unionName caseName typeAnnotation)

    StringBuilder.Many(
      seq {
        yield StringBuilder.One(sprintf "func Match%s[Result any](" union.Name)

        yield
          seq {
            yield StringBuilder.One(sprintf "value %s," union.Name)

            for case in cases do
              yield generateHandler union.Name case.CaseName case.Type
          }
          |> StringBuilder.Many
          |> StringBuilder.Map indent

        yield StringBuilder.One ") (Result,error) {"

        yield
          seq {
            yield StringBuilder.One "switch value.discriminator {"

            yield
              seq {


                for case in cases do
                  yield StringBuilder.One(sprintf "case %s:" (generateDiscriminatorCaseName union.Name case.CaseName))

                  yield
                    seq {
                      yield
                        StringBuilder.One(
                          sprintf "result, err := on%s%s(*value._%s)" union.Name case.CaseName case.CaseName
                        )

                      yield StringBuilder.One "if err != nil {"

                      yield
                        StringBuilder.One(
                          sprintf "return *new(Result), fmt.Errorf(\"on%s%s:%%w\", err)" union.Name case.CaseName
                        )
                        |> StringBuilder.Map indent

                      yield StringBuilder.One "}"
                      yield StringBuilder.One "return result, nil"
                    }
                    |> StringBuilder.Many
                    |> StringBuilder.Map indent
              }
              |> StringBuilder.Many
              |> StringBuilder.Map indent

            yield StringBuilder.One "}"

            yield
              StringBuilder.One
                "return *new(Result), fmt.Errorf(\"%s is not a valid discriminator value\", value.discriminator)"

          }
          |> StringBuilder.Many
          |> StringBuilder.Map indent

        yield StringBuilder.One "}"
      }
    )

  let private generateMatchNamedArgsFunction (union: GolangUnion) : StringBuilder =
    let cases = union.Cases

    let functionName = sprintf "Match%s_NamedArgs" union.Name
    let argsStructName = sprintf "%sHandlers" functionName

    let generateHandlerField (unionName: string) (caseName: string) (TypeAnnotation typeAnnotation) : StringBuilder =
      StringBuilder.One(sprintf "On%s%s func(%s) (Result,error)" unionName caseName typeAnnotation)

    let generateHandlersStructDefinition (union: GolangUnion) : StringBuilder =
      StringBuilder.Many(
        seq {
          yield StringBuilder.One(sprintf "type %s[Result any] struct {" argsStructName)

          for case in cases do
            yield
              generateHandlerField union.Name case.CaseName case.Type
              |> StringBuilder.Map indent

          yield StringBuilder.One "}"
        }
      )

    StringBuilder.Many(
      seq {
        yield generateHandlersStructDefinition union
        yield StringBuilder.One(sprintf "func %s[Result any](" functionName)

        yield
          StringBuilder.One(sprintf "handlers %s[Result]," argsStructName)
          |> StringBuilder.Map indent

        yield StringBuilder.One(sprintf ") func(%s) (Result,error) {" union.Name)

        yield
          seq {
            yield StringBuilder.One(sprintf "return func(value %s) (Result,error) {" union.Name)

            yield
              seq {



                yield StringBuilder.One "switch value.discriminator {"

                yield
                  seq {


                    for case in cases do
                      yield
                        StringBuilder.One(sprintf "case %s:" (generateDiscriminatorCaseName union.Name case.CaseName))

                      yield
                        seq {



                          yield
                            StringBuilder.One(
                              sprintf
                                "result, err := handlers.On%s%s(*value._%s)"
                                union.Name
                                case.CaseName
                                case.CaseName
                            )

                          yield StringBuilder.One "if err != nil {"

                          yield
                            StringBuilder.One(
                              sprintf "return *new(Result), fmt.Errorf(\"on%s%s:%%w\", err)" union.Name case.CaseName
                            )
                            |> StringBuilder.Map indent

                          yield StringBuilder.One "}"
                          yield StringBuilder.One "return result, nil"
                        }
                        |> StringBuilder.Many
                        |> StringBuilder.Map indent
                  }
                  |> StringBuilder.Many
                  |> StringBuilder.Map indent

                yield StringBuilder.One "}"

                yield
                  StringBuilder.One
                    "return *new(Result), fmt.Errorf(\"%s is not a valid discriminator value\", value.discriminator)"
              }
              |> StringBuilder.Many
              |> StringBuilder.Map indent

            yield StringBuilder.One "}"
          }
          |> StringBuilder.Many
          |> StringBuilder.Map indent

        yield StringBuilder.One "}"
      }
    )

  let private generateAllCode (serializationSyntax: SerializationSyntax) (union: GolangUnion) : StringBuilder =
    StringBuilder.Many(
      seq {
        yield generateUnionTypeDefinition union
        yield generateCaseConstructors union
        yield generateMatchFunction union
        yield generateMatchNamedArgsFunction union

        match serializationSyntax with
        | SerializationSyntax.Next -> yield! []
        | SerializationSyntax.FormEngine ->
          yield generateMarshalJSON union
          yield generateUnmarshalJSON union

      }
    )
    |> StringBuilder.Map appendNewline

  type GolangUnion with

    static member Generate<'context, 'errors>
      (serializationSyntax: SerializationSyntax)
      (union: GolangUnion)
      : StringBuilder * Set<GoImport> =
      let serializationImports =
        match serializationSyntax with
        | SerializationSyntax.Next -> Set.empty
        | SerializationSyntax.FormEngine -> Set.ofList [ GoImport "encoding/json"; GoImport "bytes" ]

      let imports = Set.ofList [ GoImport "fmt" ] + serializationImports

      let code = generateAllCode serializationSyntax union
      code, imports

namespace Codegen.Python

module Record =
  open Ballerina.StdLib.StringBuilder
  open Codegen.Python.Model
  open Ballerina.StdLib.String
  open Codegen.Python.Syntax
  open Codegen.Python.Parsing
  open Codegen.Python.ParsingTypes

  type PythonRecordField =
    { FieldName: string
      FieldType: TypeAnnotation }

  type PythonRecord =
    { Name: NonEmptyString
      Fields: List<PythonRecordField> }


  type PythonRecordFieldParser =
    { FieldName: string
      FieldType: TypeAnnotation
      Parser: PythonInlineParser }

  type PythonRecordParser =
    { Name: NonEmptyString
      Fields: List<PythonRecordFieldParser> }

  type PythonRecordFieldSerializer =
    { FieldName: string
      FieldType: TypeAnnotation
      Serializer: PythonInlineSerializer }

  type PythonRecordSerializer =
    { Name: NonEmptyString
      Fields: List<PythonRecordFieldSerializer> }

  let private matchFieldsCode (isEmpty: bool) : StringBuilder =
    let getRecordFieldCode =
      StringBuilder.Many
        [ StringBuilder.One
            """def get_record_field(fields_list: Sequence[Json], field_name: str) -> Sum[ParsingError, Json]:"""
          StringBuilder.Many
            [ StringBuilder.One """for field_tuple in fields_list:"""
              StringBuilder.Many
                [ StringBuilder.One """match field_tuple:"""
                  StringBuilder.Many
                    [ StringBuilder.One """case [name, value]:"""
                      StringBuilder.One """    if name == field_name:"""
                      StringBuilder.One """        return Sum.right(value)"""
                      StringBuilder.One """    continue"""
                      StringBuilder.One """case _:"""
                      StringBuilder.One
                        """    return Sum.left(ParsingError.single(f"Expected tuple [name, value], got {field_tuple}"))""" ]
                  |> StringBuilder.Map indent ]
              |> StringBuilder.Map indent
              StringBuilder.One
                """return Sum.left(ParsingError.single(f"Did not find field {field_name} in {fields_list}"))""" ]
          |> StringBuilder.Map indent ]

    let asListCode =
      StringBuilder.Many
        [ StringBuilder.One """def as_list(fields: Json) -> Sum[ParsingError, Sequence[Json]]:"""
          StringBuilder.Many
            [ StringBuilder.One """match fields:"""
              StringBuilder.Many
                [ StringBuilder.One """case list():"""
                  StringBuilder.One """    return Sum.right(fields)"""
                  StringBuilder.One """case _:"""
                  StringBuilder.One """    return Sum.left(ParsingError.single(f"Expected list, got {fields}"))""" ]
              |> StringBuilder.Map indent ]
          |> StringBuilder.Map indent ]

    StringBuilder.Many
      [ if isEmpty then
          StringBuilder.Many []
        else
          getRecordFieldCode
        asListCode ]


  let private parserFunctionCode
    (discriminatorKey: string)
    (valueKey: string)
    ({ Name = recordName
       Fields = recordFields }: PythonRecordParser)
    : StringBuilder =
    let signatureCode =
      let (PythonInlineParser parserName) =
        PythonInlineParser.FromTypeName(NonEmptyString.AsString recordName)

      StringBuilder.One(
        sprintf "def %s(data: Json, /) -> Sum[ParsingError, %s]:" parserName (NonEmptyString.AsString recordName)
      )

    let fieldChainCode =
      StringBuilder.Many
        [ StringBuilder.One """lambda fields: """
          StringBuilder.Many(
            recordFields
            |> Seq.mapi
              (fun
                   _i
                   { FieldName = fieldName
                     Parser = PythonInlineParser fieldParser } ->
                StringBuilder.Many
                  [ StringBuilder.One(sprintf "get_record_field(fields, \"%s\")" fieldName)
                    StringBuilder.One(
                      sprintf
                        ".flat_map(%s).map_left(ParsingError.with_context(\"Parsing %s:\"))"
                        fieldParser
                        fieldName
                    )
                    StringBuilder.One """.flat_map("""
                    StringBuilder.One(sprintf "lambda %s: " fieldName) ])
          ) ]

    let closingParens =
      recordFields |> Seq.map (fun _ -> StringBuilder.One ")") |> StringBuilder.Many

    let matchBody =
      StringBuilder.Many
        [ StringBuilder.One """match data:"""
          StringBuilder.Many
            [ StringBuilder.One """case dict():"""
              StringBuilder.Many
                [ StringBuilder.One """fields_list = ("""
                  StringBuilder.One(sprintf "    get_field(data, \"%s\")" discriminatorKey)
                  StringBuilder.One """    .flat_map("""
                  StringBuilder.One """        lambda k: ("""
                  StringBuilder.One """            Sum.right(k)"""
                  StringBuilder.One """            if k == "record" """
                  StringBuilder.One """            else Sum.left("""
                  StringBuilder.One """                ParsingError.single(f"Expected record, got {k}")"""
                  StringBuilder.One """            )"""
                  StringBuilder.One """        )"""
                  StringBuilder.One """    )"""
                  StringBuilder.One(sprintf "    .flat_map(lambda _: get_field(data, \"%s\"))" valueKey)
                  StringBuilder.One """    .flat_map(as_list)"""
                  StringBuilder.One """)"""
                  StringBuilder.One "return fields_list.flat_map("
                  fieldChainCode
                  StringBuilder.One(sprintf "Sum.right(%s(" (NonEmptyString.AsString recordName))
                  StringBuilder.Many(
                    recordFields
                    |> Seq.map (fun f ->
                      StringBuilder.One(sprintf "%s=%s," f.FieldName f.FieldName)
                      |> StringBuilder.Map indent)
                  )
                  StringBuilder.One """)"""
                  StringBuilder.One """)"""
                  StringBuilder.One """)"""
                  closingParens ]
              |> StringBuilder.Map indent
              StringBuilder.One """case _:"""
              StringBuilder.One """return Sum.left(ParsingError.single(f"Expected dict, got {data}"))"""
              |> StringBuilder.Map indent ]
          |> StringBuilder.Map indent ]

    let bodyCode =
      StringBuilder.Many [ getFieldCode; matchFieldsCode (recordFields |> List.isEmpty); matchBody ]

    StringBuilder.Many [ signatureCode; bodyCode |> StringBuilder.Map indent ]

  type PythonRecord with
    static member Generate(record: PythonRecord) =
      let fieldDeclarations: StringBuilder =
        record.Fields
        |> List.map
          (fun
               { FieldName = fieldName
                 FieldType = TypeAnnotation fieldType } -> StringBuilder.One(sprintf "%s: %s" fieldName fieldType))
        |> List.toSeq
        |> StringBuilder.Many

      let recordCode: StringBuilder =
        StringBuilder.Many
          [ StringBuilder.One """@dataclass(frozen=True, kw_only=True)"""
            StringBuilder.One(sprintf "class %s:" (NonEmptyString.AsString record.Name))
            StringBuilder.Many
              [ if record.Fields.IsEmpty then
                  StringBuilder.One "pass"
                else
                  fieldDeclarations ]
            |> StringBuilder.Map indent ]

      let imports =
        [ { Source = "dataclasses"
            Target = "dataclass" } ]
        |> Set.ofList

      recordCode |> StringBuilder.Map appendNewline, imports


  let private serializerFunctionCode
    (discriminatorKey: string)
    (valueKey: string)
    ({ Name = recordName
       Fields = recordFields }: PythonRecordSerializer)
    : StringBuilder =
    let (PythonInlineSerializer serializerName) =
      PythonInlineSerializer.FromTypeName(NonEmptyString.AsString recordName)


    let innerBody =
      StringBuilder.Many
        [ StringBuilder.One """return {"""
          StringBuilder.One(sprintf "\"%s\": \"record\"," discriminatorKey)
          StringBuilder.One(sprintf "\"%s\": [" valueKey)
          StringBuilder.Many
            [ recordFields
              |> Seq.map (fun f ->
                let (PythonInlineSerializer serializer) = f.Serializer

                StringBuilder.One(sprintf "[\"%s\", %s(value.%s)]," f.FieldName serializer f.FieldName))
              |> StringBuilder.Many ]
          StringBuilder.One "],"
          StringBuilder.One """}""" ]

    StringBuilder.Many
      [ StringBuilder.One(sprintf "def %s(value: %s) -> Json:" serializerName (NonEmptyString.AsString recordName))
        innerBody |> StringBuilder.Map indent ]


  type PythonRecordParser with
    static member Generate(discriminatorKey: string, valueKey: string, record: PythonRecordParser) =
      let imports =
        [ { Source = "ballerina_core.parsing.parsing_types"
            Target = "Json" }
          { Source = "ballerina_core.sum"
            Target = "Sum" }
          { Source = "collections.abc"
            Target = "Sequence" }
          { Source = "ballerina_core.parsing.parsing_types"
            Target = "ParsingError" } ]
        |> Set.ofList

      parserFunctionCode discriminatorKey valueKey record
      |> StringBuilder.Map appendNewline,
      imports

  type PythonRecordSerializer with
    static member Generate(discriminatorKey: string, valueKey: string, record: PythonRecordSerializer) =
      let imports =
        [ { Source = "ballerina_core.parsing.parsing_types"
            Target = "Json" } ]
        |> Set.ofList

      serializerFunctionCode discriminatorKey valueKey record
      |> StringBuilder.Map appendNewline,
      imports

namespace Codegen.Python

module Union =
  open Ballerina.StdLib.StringBuilder
  open Ballerina.Collections.NonEmptyList
  open Codegen.Python.Model
  open Codegen.Python.Syntax
  open Codegen.Python.Parsing
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Codegen.Python.ParsingTypes

  let private appendCaseName (allCases: string) (nextCase: string) = sprintf "%s | %s" allCases nextCase

  type PythonUnionCase = { Name: string; Type: TypeAnnotation }

  type PythonUnion =
    private
      { Name: string
        Cases: NonEmptyList<PythonUnionCase> }

  type PythonUnionCaseParser =
    { Name: string
      Type: TypeAnnotation
      Parser: PythonInlineParser }

  type PythonUnionParser =
    private
      { Name: string
        Cases: NonEmptyList<PythonUnionCaseParser> }

  type PythonUnionCaseSerializer =
    { Name: string
      Type: TypeAnnotation
      Serializer: PythonInlineSerializer }

  type PythonUnionSerializer =
    private
      { Name: string
        Cases: NonEmptyList<PythonUnionCaseSerializer> }

  type PythonUnion with
    static member Create(name: string, cases: NonEmptyList<PythonUnionCase>) : Sum<PythonUnion, Errors<unit>> =
      let casesWithErrors: Sum<NonEmptyList<PythonUnionCase>, Errors<unit>> =
        cases
        |> NonEmptyList.map
          (fun
               { Name = name
                 Type = TypeAnnotation type_ } ->
            if name = type_ then
              Sum.Right(
                Errors.Singleton () (fun () -> (sprintf "Union case name must be different from type name: %s" name))
              )
            else
              Sum.Left
                { Name = name
                  Type = TypeAnnotation type_ })
        |> sum.AllNonEmpty

      casesWithErrors |> sum.Map(fun cases -> { Name = name; Cases = cases })

  let private matchUnionCaseCode (valueKey: string) (union: PythonUnionParser) : StringBuilder =
    let unionCasesCode =
      StringBuilder.Many
        [ union.Cases
          |> NonEmptyList.ToList
          |> List.collect
            (fun
                 { Name = caseName
                   Parser = PythonInlineParser caseParser } ->
              [ StringBuilder.One(sprintf "case \"%s\":" caseName)
                StringBuilder.Many
                  [ StringBuilder.One "return ("
                    StringBuilder.One(sprintf "    get_field(data, \"%s\")" valueKey)
                    StringBuilder.One(
                      sprintf
                        "    .flat_map(%s).map_left(ParsingError.with_context(\"Parsing %s:\"))"
                        caseParser
                        caseName
                    )
                    StringBuilder.One(sprintf "    .map_right(%s.%s)" union.Name caseName)
                    StringBuilder.One(sprintf "    .map_right(%s)" union.Name)
                    StringBuilder.One ")" |> StringBuilder.Map indent ]
                |> StringBuilder.Map indent ])
          |> List.toSeq
          |> StringBuilder.Many
          StringBuilder.One """case _:"""
          StringBuilder.One """return Sum.left(ParsingError.single(f"Unknown union case: {union_case}"))"""
          |> StringBuilder.Map indent ]

    StringBuilder.Many
      [ StringBuilder.One(
          sprintf "def match_union_case(union_case: Json, data: Json) -> Sum[ParsingError, %s]:" union.Name
        )
        StringBuilder.Many
          [ StringBuilder.One """match union_case:"""
            unionCasesCode |> StringBuilder.Map indent ]
        |> StringBuilder.Map indent ]

  let private parseValueArrayCode (valueKey: string) (union: PythonUnionParser) : StringBuilder =
    StringBuilder.Many
      [ StringBuilder.One(sprintf "def parse_value_array(value_array: Json) -> Sum[ParsingError, %s]:" union.Name)
        StringBuilder.Many
          [ StringBuilder.One """match value_array:"""
            StringBuilder.One """    case [case_identifier, actual_value]:"""
            StringBuilder.One(
              sprintf """        return match_union_case(case_identifier, {"%s": actual_value})""" valueKey
            )
            StringBuilder.One """    case _:"""
            StringBuilder.One
              """        return Sum.left(ParsingError.single(f"Expected array with 2 elements, got {value_array}"))""" ]
        |> StringBuilder.Map indent ]

  let private parseBodyCode (discriminatorKey: string) (valueKey: string) =
    StringBuilder.Many
      [ StringBuilder.One """return ("""
        StringBuilder.Many
          [ StringBuilder.One(sprintf "get_field(data, \"%s\")" discriminatorKey)
            StringBuilder.One """.flat_map("""
            StringBuilder.One """    lambda k: ("""
            StringBuilder.One """        Sum.right(k)"""
            StringBuilder.One """        if k == "union-case" """
            StringBuilder.One """        else Sum.left("""
            StringBuilder.One """            ParsingError.single(f"Expected union-case, got {k}")"""
            StringBuilder.One """        )"""
            StringBuilder.One """    )"""
            StringBuilder.One """)"""
            StringBuilder.One(sprintf ".flat_map(lambda _: get_field(data, \"%s\"))" valueKey)
            StringBuilder.One ".flat_map(parse_value_array)" ]
        |> StringBuilder.Map indent
        StringBuilder.One ")" ]

  let private parserFunctionCode
    (discriminatorKey: string)
    (valueKey: string)
    (union: PythonUnionParser)
    : StringBuilder =
    let (PythonInlineParser parserName) = PythonInlineParser.FromTypeName union.Name

    StringBuilder.Many
      [ StringBuilder.One(sprintf "def %s(data: Json) -> Sum[ParsingError, %s]:" parserName union.Name)
        StringBuilder.Many
          [ getFieldCode
            matchUnionCaseCode valueKey union
            parseValueArrayCode valueKey union
            parseBodyCode discriminatorKey valueKey ]
        |> StringBuilder.Map indent ]

  let private foldMethodCode (union: PythonUnion) : StringBuilder =
    let typeVarCode = StringBuilder.One "FoldOutput = TypeVar(\"FoldOutput\")"

    let onCase (caseName: string) (caseType: string) =
      sprintf "on_%s: Callable[[%s], FoldOutput]," (caseName.ToLowerInvariant()) caseType

    let functionSignatureCode =
      StringBuilder.Many
        [ StringBuilder.One "def fold(self, *,"
          StringBuilder.Many
            [ union.Cases
              |> NonEmptyList.ToList
              |> List.map
                (fun
                     { Name = name
                       Type = TypeAnnotation type_ } -> StringBuilder.One(onCase name type_))
              |> List.toSeq
              |> StringBuilder.Many ]
          StringBuilder.One ") -> FoldOutput:" ]

    let matchCasesCode =
      union.Cases
      |> NonEmptyList.ToList
      |> List.map (fun c ->
        StringBuilder.Many
          [ StringBuilder.One(sprintf "case self.%s(value):" c.Name)
            StringBuilder.One(sprintf "return on_%s(value)" (c.Name.ToLowerInvariant()))
            |> StringBuilder.Map indent ])
      |> List.toSeq
      |> StringBuilder.Many

    let matchBlockCode =
      StringBuilder.Many
        [ StringBuilder.One """match self.value:"""
          matchCasesCode |> StringBuilder.Map indent ]
      |> StringBuilder.Map indent

    let assertNeverCode =
      StringBuilder.One """assert_never(self.value)""" |> StringBuilder.Map indent

    StringBuilder.Many [ typeVarCode; functionSignatureCode; matchBlockCode; assertNeverCode ]


  type PythonUnion with
    static member Generate(union: PythonUnion) =
      let caseClassesCode =
        union.Cases
        |> NonEmptyList.ToList
        |> Seq.map
          (fun
               { Name = name
                 Type = TypeAnnotation type_ } ->
            StringBuilder.Many
              [ StringBuilder.One """@dataclass(frozen=True)"""
                StringBuilder.One(sprintf "class %s:" name)
                StringBuilder.One(sprintf "_value: %s" type_) |> StringBuilder.Map indent ])
        |> StringBuilder.Many

      let valueFieldCode =
        StringBuilder.One(
          sprintf
            "value: %s"
            (union.Cases
             |> NonEmptyList.map (fun c -> c.Name)
             |> NonEmptyList.reduce appendCaseName)
        )

      let unionCode: StringBuilder =
        StringBuilder.Many
          [ StringBuilder.One """@dataclass(frozen=True)"""
            StringBuilder.One(sprintf "class %s:" union.Name)
            StringBuilder.Many [ caseClassesCode; valueFieldCode; foldMethodCode union ]
            |> StringBuilder.Map indent ]

      let imports =
        [ { Source = "dataclasses"
            Target = "dataclass" }
          { Source = "collections.abc"
            Target = "Callable" }
          { Source = "typing"
            Target = "TypeVar" }
          { Source = "typing"
            Target = "assert_never" } ]
        |> Set.ofList


      unionCode |> StringBuilder.Map appendNewline, imports

  let private serializerFunctionCode
    (discriminatorKey: string)
    (valueKey: string)
    (union: PythonUnionSerializer)
    : StringBuilder =
    let (PythonInlineSerializer serializerName) =
      PythonInlineSerializer.FromTypeName union.Name


    let innerBody =
      StringBuilder.Many
        [ StringBuilder.One "return value.fold("
          StringBuilder.Many
            [ union.Cases
              |> NonEmptyList.ToList
              |> List.map (fun c ->
                let (PythonInlineSerializer serializer) = c.Serializer

                StringBuilder.One(
                  sprintf
                    "on_%s=lambda v: {\"%s\": \"union-case\", \"%s\": [\"%s\", %s(v)]},"
                    (c.Name.ToLowerInvariant())
                    discriminatorKey
                    valueKey
                    c.Name
                    serializer
                ))
              |> List.toSeq
              |> StringBuilder.Many ]
          |> StringBuilder.Map indent
          StringBuilder.One ")" ]

    StringBuilder.Many
      [ StringBuilder.One(sprintf "def %s(value: %s) -> Json:" serializerName union.Name)
        innerBody |> StringBuilder.Map indent ]


  type PythonUnionParser with

    static member Create
      (name: string, cases: NonEmptyList<PythonUnionCaseParser>)
      : Sum<PythonUnionParser, Errors<unit>> =
      let casesWithErrors: Sum<NonEmptyList<PythonUnionCaseParser>, Errors<unit>> =
        cases
        |> NonEmptyList.map
          (fun
               { Name = name
                 Type = TypeAnnotation type_
                 Parser = parser } ->
            if name = type_ then
              Sum.Right(
                Errors.Singleton () (fun () -> (sprintf "Union case name must be different from type name: %s" name))
              )
            else
              Sum.Left
                { Name = name
                  Type = TypeAnnotation type_
                  Parser = parser })
        |> sum.AllNonEmpty

      casesWithErrors |> sum.Map(fun cases -> { Name = name; Cases = cases })

    static member Generate(discriminatorKey: string, valueKey: string, union: PythonUnionParser) =
      let imports =
        [ { Source = "ballerina_core.parsing.parsing_types"
            Target = "Json" }
          { Source = "ballerina_core.sum"
            Target = "Sum" }
          { Source = "ballerina_core.parsing.parsing_types"
            Target = "ParsingError" } ]
        |> Set.ofList

      parserFunctionCode discriminatorKey valueKey union
      |> StringBuilder.Map appendNewline,
      imports

  type PythonUnionSerializer with

    static member Create
      (name: string, cases: NonEmptyList<PythonUnionCaseSerializer>)
      : Sum<PythonUnionSerializer, Errors<unit>> =
      let casesWithErrors: Sum<NonEmptyList<PythonUnionCaseSerializer>, Errors<unit>> =
        cases
        |> NonEmptyList.map
          (fun
               { Name = caseName
                 Type = TypeAnnotation type_
                 Serializer = serializer } ->
            if caseName = type_ then
              Sum.Right(
                Errors.Singleton () (fun () ->
                  (sprintf "Union case name must be different from type name: %s" caseName))
              )
            else
              Sum.Left
                { Name = caseName
                  Type = TypeAnnotation type_
                  Serializer = serializer })
        |> sum.AllNonEmpty

      casesWithErrors |> sum.Map(fun cases -> { Name = name; Cases = cases })

    static member Generate(discriminatorKey: string, valueKey: string, union: PythonUnionSerializer) =
      let imports =
        [ { Source = "ballerina_core.parsing.parsing_types"
            Target = "Json" } ]
        |> Set.ofList

      serializerFunctionCode discriminatorKey valueKey union
      |> StringBuilder.Map appendNewline,
      imports

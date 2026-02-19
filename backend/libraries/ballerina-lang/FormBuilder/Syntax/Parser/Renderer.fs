namespace Ballerina.DSL.FormBuilder.Syntax


module Parser =
  open System
  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.FormBuilder.Model.FormAST
  open Ballerina
  open Ballerina.Collections.Sum.Model
  open Ballerina.DSL.FormBuilder.Syntax.Lexer
  open Ballerina.Parser
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.Collections.NonEmptyList
  open Ballerina.Cat.Collections.OrderedMap

  type FormMember =
    | FormField of Field<Unchecked>
    | FormTab of TabIdentifier * Tab

  let parser =
    ParserBuilder<LocalizedToken, Location, Errors<Location>>(
      {| Step = fun lt _ -> lt.Location |},
      {| UnexpectedEndOfFile = fun loc -> (loc, fun () -> $"Unexpected end of file at {loc}") ||> Errors.Singleton
         AnyFailed = fun loc -> (loc, fun () -> "No matching token") ||> Errors.Singleton
         NotFailed = fun loc -> (loc, fun () -> $"Expected token not found at {loc}") ||> Errors.Singleton
         UnexpectedSymbol = fun loc c -> (loc, fun () -> $"Unexpected symbol: {c}") ||> Errors.Singleton
         FilterHighestPriorityOnly = Errors<_>.FilterHighestPriorityOnly
         Concat = Errors.Concat<Location> |}
    )

  let stringLiteral () =
    parser.Exactly(fun t ->
      match t.Token with
      | Token.StringLiteral s -> s |> Some
      | _ -> None)

  let identifier () =
    parser.Exactly(fun t ->
      //Console.WriteLine("Trying to parse identifier from token: " + t.Token.ToString())
      match t.Token with
      | Token.Identifier id -> Some id
      | Token.Keyword k -> k.ToString() |> Some
      | _ -> None)


  let returnKeyword (keyword: Keyword) =
    parser.Exactly(fun t ->
      match t.Token with
      | Keyword k when k = keyword -> true
      | _ -> false)

  let keyword = returnKeyword >> parser.Ignore

  let returnOperator (operator: Operator) =
    parser.Exactly(fun t ->
      match t.Token with
      | Operator o when o = operator -> true
      | _ -> false)

  let operator = returnOperator >> parser.Ignore

  let parseDisplayOption (option: Keyword) =
    parser {
      do! keyword option
      let! label = stringLiteral ()
      return label
    }

  let parseRendererDef () =
    parser {
      do! operator (RoundBracket Open)
      let! rendererName = identifier ()
      //Console.WriteLine("Parsing " + rendererName)
      do! operator (RoundBracket Close)
      return rendererName
    }

  let parseOptionalLabel () =
    parser.Try(parseDisplayOption As)
    |> parser.Map(Sum.toOption >> Option.map Label)

  let parseApi () =
    parser {
      let! first = identifier ()

      let! path =
        parser.Many(
          parser {
            do! operator Dot
            return! identifier ()
          }
        )

      return NonEmptyList.OfList(first, path)
    }

  let rec parsePrimitive () =
    parser {
      let parsePrimitiveKind () =
        parser {
          let! loc = parser.Location

          let! primitiveKind =
            parser.Any(
              [ returnKeyword String
                returnKeyword Int32
                returnKeyword Int64
                returnKeyword Float32
                returnKeyword Float64
                returnKeyword Date
                returnKeyword DateOnly
                returnKeyword StringId
                returnKeyword Guid
                returnKeyword Bool
                returnKeyword Base64
                returnKeyword Secret
                returnKeyword Unit ]
            )

          match primitiveKind.Token with
          | Keyword String -> return PrimitiveRendererKind.String
          | Keyword Int32 -> return PrimitiveRendererKind.Int32
          | Keyword Int64 -> return PrimitiveRendererKind.Int64
          | Keyword Float32 -> return PrimitiveRendererKind.Float32
          | Keyword Float64 -> return PrimitiveRendererKind.Float
          | Keyword Date -> return PrimitiveRendererKind.Date
          | Keyword DateOnly -> return PrimitiveRendererKind.DateOnly
          | Keyword StringId -> return PrimitiveRendererKind.StringId
          | Keyword Guid -> return PrimitiveRendererKind.Guid
          | Keyword Bool -> return PrimitiveRendererKind.Bool
          | Keyword Base64 -> return PrimitiveRendererKind.Base64
          | Keyword Secret -> return PrimitiveRendererKind.Secret
          | Keyword Unit -> return PrimitiveRendererKind.Unit
          | _ ->
            return!
              parser.Throw(
                Errors.Singleton loc (fun () -> $"Unsupported parsed primitive: {primitiveKind.Token}")
                |> Errors.MapPriority(replaceWith ErrorPriority.High)
              )

        }

      let! primitiveRenderer = parsePrimitiveKind ()
      let! primitiveName = parseRendererDef ()

      return
        { Primitive = RendererIdentifier primitiveName
          Renderer = primitiveRenderer
          Type = Unchecked }
    }

  and parseMap () =
    parser {
      do! keyword Map
      let! rendererName = parseRendererDef ()
      let! keyRenderer = parseRenderer ()
      do! operator Comma
      let! valueRenderer = parseRenderer ()

      return
        { Map = RendererIdentifier rendererName
          Key = keyRenderer
          Value = valueRenderer
          Type = Unchecked }
    }

  and parsePair () =
    parser {
      let! firstRenderer = parseRenderer ()
      do! operator Comma
      let! secondRenderer = parseRenderer ()
      return firstRenderer, secondRenderer
    }

  and parseTupleSeq () =
    parser.Many(
      parser {
        do! operator Comma
        return! parseRenderer ()
      }
    )

  and parseTupleItems () =
    parser {
      let! firstRenderer, secondRenderer = parsePair ()

      let! remainingItems =
        parser.Try(parseTupleSeq ())
        |> parser.Map(Sum.toOption >> Option.toList >> List.concat)

      return firstRenderer :: secondRenderer :: remainingItems
    }

  and parseTuple () =
    parser {
      do! keyword Tuple
      let! rendererName = parseRendererDef ()
      do! operator (RoundBracket Open)
      let! tupleRenderers = parseTupleItems ()
      do! operator (RoundBracket Close)

      return
        { Tuple = RendererIdentifier rendererName
          Items = tupleRenderers
          Type = Unchecked }
    }

  and parseTwoCasesUnion (k: Keyword) =
    parser {
      let parseAnyChoice () =
        parser.Any([ returnOperator Choice1; returnOperator Choice2 ])

      do! keyword k
      let! rendererName = parseRendererDef ()
      do! keyword With
      let! caseLoc = parser.Location

      do! operator Pipe
      let! firstCase = parseAnyChoice ()
      do! operator Arrow
      let! firstCaseNestedRenderer = parseRenderer ()
      do! operator Pipe
      let! secondCase = parseAnyChoice ()
      do! operator Arrow
      let! secondCaseNestedRenderer = parseRenderer ()

      return
        {| RendererName = rendererName
           CaseLocation = caseLoc
           FirstCase = firstCase
           FirstCaseRenderer = firstCaseNestedRenderer
           SecondCase = secondCase
           SecondCaseRenderer = secondCaseNestedRenderer |}
    }

  and twoCasesUnionError loc (firstCase: Token) (secondCase: Token) =
    parser.Throw(
      Errors.Singleton loc (fun () ->
        $"Invalid option pattern matching cases: {string firstCase}, {string secondCase}. Expected {string Choice1} or {string Choice2}.")
    )

  and parseList () =
    parser {
      do! keyword List
      let! rendererName = parseRendererDef ()
      let! elementRenderer = parseRenderer ()

      return
        { List = RendererIdentifier rendererName
          Element = elementRenderer
          Type = Unchecked }
    }
  and parseReferenceOne () =
    parser {
      do! keyword ReferenceOne
      //Console.WriteLine("Parsing reference one renderer")
      let! rendererName = parseRendererDef ()
      //Console.WriteLine("Parsing reference one renderer" + rendererName)
      let! elementRenderer = parseRenderer ()
      //Console.WriteLine("Parsing reference one renderer" + elementRenderer.ToString())
      let! previewRenderer = parseRenderer ()
      //Console.WriteLine("Parsing reference one renderer" + previewRenderer.ToString())
      let! schemaEntityName = identifier()  
      //Console.WriteLine("Parsing reference one renderer" + schemaEntityName)

      return
        { ReferenceOne = RendererIdentifier rendererName
          CurrentElement = elementRenderer
          Preview = previewRenderer
          SchemaEntityName = { EntityName = schemaEntityName}
          Type = Unchecked
          TypeID = Unchecked }
    }

  and parseReadonly () =
    parser {
      do! keyword ReadOnly
      let! rendererName = parseRendererDef ()
      let! nestedRenderer = parseRenderer ()

      return
        { Readonly = RendererIdentifier rendererName
          Value = nestedRenderer
          Type = Unchecked }
    }

  and parseSum () =
    parser {
      let! parsedUnion = parseTwoCasesUnion Keyword.Sum

      match parsedUnion.FirstCase.Token, parsedUnion.SecondCase.Token with
      | Operator Choice1, Operator Choice2 ->
        return
          { Sum = RendererIdentifier parsedUnion.RendererName
            Left = parsedUnion.FirstCaseRenderer
            Right = parsedUnion.SecondCaseRenderer
            Type = Unchecked }
      | Operator Choice2, Operator Choice1 ->
        return
          { Sum = RendererIdentifier parsedUnion.RendererName
            Left = parsedUnion.SecondCaseRenderer
            Right = parsedUnion.FirstCaseRenderer
            Type = Unchecked }
      | _ ->
        return! twoCasesUnionError parsedUnion.CaseLocation parsedUnion.FirstCase.Token parsedUnion.SecondCase.Token
    }

  and parseCardinality () =
    parser.Try(
      parser.Exactly(fun t ->
        match t.Token with
        | Keyword Multi
        | Keyword Single -> true
        | _ -> false)
    )

  and parseEnum () =
    parser {
      do! keyword Enum
      let! rendererName = parseRendererDef ()
      let! cardinality = parseCardinality ()
      do! keyword From
      let! api = identifier ()

      let cardinality =
        match cardinality with
        | Left token when token.Token = Keyword Multi -> Cardinality.Multi
        | _ -> Cardinality.Single

      return
        { Enum = RendererIdentifier rendererName
          Cardinality = cardinality
          Api = ApiIdentifier api
          Type = Unchecked }
    }

  and parseStream () =
    parser {
      do! keyword Stream
      let! rendererName = parseRendererDef ()
      let! cardinality = parseCardinality ()

      let cardinality =
        match cardinality with
        | Left token when token.Token = Keyword Multi -> Cardinality.Multi
        | _ -> Cardinality.Single

      do! keyword From
      let! api = identifier ()

      return
        { Stream = RendererIdentifier rendererName
          Cardinality = cardinality
          Api = ApiIdentifier api
          Type = Unchecked }
    }

  and parseTable () =
    parser {
      do! keyword Table
      let! formConfig = parseRendererDef ()
      do! keyword From
      let! api = identifier ()

      return
        { Table = RendererIdentifier formConfig
          Api = ApiIdentifier api
          Type = Unchecked }
    }

  and parseForm () =
    parser {
      do! keyword View
      let! formConfig = parseRendererDef ()

      (*
        This is necessary because parseForm and parseInlineForm might share a common prefix if a renderer name is provided, otherwise
        it will try to parse (successfully) an inline form as a regular form renderer. Then the following form body will be left in 
        the buffer and it will detect an unexpected token. In order to disambiguate this situation, we look ahead if there is an open 
        curly bracket because in such case it means that it should be parsing an inline form.
    *)
      do!
        parser.Lookahead(parser.Exactly((fun token -> token.Token = Operator(CurlyBracket Open)) >> not))
        |> parser.Ignore

      return FormIdentifier formConfig
    }

  and parseUnionCase () =
    parser {
      do! operator Pipe
      let! caseId = identifier ()
      do! operator Arrow
      let! nestedRenderer = parseRenderer ()
      return caseId, nestedRenderer
    }

  and parseUnionCases () =
    parser {
      let rec parseOtherCases (caseMap: Map<CaseIdentifier, RendererExpression<Unchecked>>) =
        parser {
          let! loc = parser.Location

          match! parser.Try(parseUnionCase ()) |> parser.Map Sum.toOption with
          | Some(caseId, renderer) ->
            if caseMap.ContainsKey(CaseIdentifier caseId) |> not then
              let caseMap = caseMap.Add(CaseIdentifier caseId, renderer)
              let! updatedCases = parseOtherCases caseMap
              return updatedCases
            else
              return!
                parser.Throw(
                  Errors.Singleton loc (fun () -> $"Duplicate union case: {caseId}")
                  |> Errors.MapPriority(replaceWith ErrorPriority.High)
                )
          | _ -> return caseMap
        }

      let cases = Map.empty
      let! caseId, renderer = parseUnionCase ()
      let cases = cases.Add(CaseIdentifier caseId, renderer)
      let! cases = parseOtherCases cases
      return cases
    }

  and parseUnion () =
    parser {
      do! keyword Union
      let! rendererName = parseRendererDef ()
      do! keyword With
      let! cases = parseUnionCases ()

      return
        { Union = RendererIdentifier rendererName
          Cases = cases
          Type = Unchecked }
    }

  // and parseField() = parser {
  //   let! fieldName = identifier()
  //   let! label = parseDisplayOption As |> parser.Try |> parser.Map (Sum.toOption >> Option.map Label)
  //   let! tooltip = parseDisplayOption Tooltip |> parser.Try |> parser.Map (Sum.toOption >> Option.map Tooltip.Tooltip)
  //   let! details = parseDisplayOption Details |> parser.Try |> parser.Map (Sum.toOption >> Option.map Details.Details)
  //   let! renderer = parseRenderer()
  //   do! operator Semicolon
  //   return {
  //     Name = FieldIdentifier fieldName
  //     Label = label
  //     Tooltip = tooltip
  //     Details = details
  //     Renderer = renderer
  //   }
  // }

  and parseFieldSeq () =
    parser {
      let! field = identifier ()

      let! fields =
        parser.Many(
          parser {
            do! operator Comma
            return! identifier ()
          }
        )

      return field :: fields |> Set.ofList
    }

  and parseGroup () =
    parser {
      do! keyword Group
      let! groupName = identifier ()
      do! keyword With
      let! fields = parseFieldSeq ()
      return GroupIdentifier groupName, fields |> Set.map FieldIdentifier
    }

  and parseColumn () =
    parser {
      do! keyword Column
      let! columnName = identifier ()
      do! keyword With
      let! groups = parser.Many(parseGroup ()) |> parser.Map Map.ofList
      return ColumnIdentifier columnName, { Groups = groups }
    }

  and parseTab () =
    parser {
      do! keyword Keyword.Tab
      let! tabName = identifier ()
      do! keyword With
      let! columns = parser.Many(parseColumn ()) |> parser.Map Map.ofList
      return TabIdentifier tabName, { Columns = columns }
    }

  and parseDisplayOptions () =
    parser {
      let! label =
        parser.Try(parseDisplayOption As)
        |> parser.Map(Sum.toOption >> Option.map Label)

      let! tooltip =
        parser.Try(parseDisplayOption Tooltip)
        |> parser.Map(Sum.toOption >> Option.map Tooltip.Tooltip)

      let! details =
        parser.Try(parseDisplayOption Details)
        |> parser.Map(Sum.toOption >> Option.map Details.Details)

      return label, tooltip, details
    }


  and parseFormField () =
    parser {
      let! fieldName = identifier ()
      let! label, tooltip, details = parseDisplayOptions ()
      let! renderer = parseRenderer ()
      do! operator Semicolon

      return
        { Name = FieldIdentifier fieldName
          Label = label
          Tooltip = tooltip
          Details = details
          Renderer = renderer
          Type = Unchecked }
    }

  and parseFormMember () =
    parser.Any([ parseFormField () |> parser.Map FormField; parseTab () |> parser.Map FormTab ])

  and parseMembers (fields: Map<FieldIdentifier, Field<Unchecked>>) (tabs: Map<TabIdentifier, Tab>) =
    parser {
      let! loc = parser.Location

      match! parser.Try(parseFormMember ()) |> parser.Map Sum.toOption with
      | None -> return fields, tabs
      | Some(FormField field) ->
        if fields |> Map.containsKey field.Name then
          return!
            parser.Throw(
              Errors.Singleton loc (fun () -> $"Field {field.Name} already defined.")
              |> Errors.MapPriority(replaceWith ErrorPriority.High)
            )
        else
          return! parseMembers (fields.Add(field.Name, field)) tabs
      | Some(FormTab(tabId, tab)) ->
        if tabs |> Map.containsKey tabId then
          return!
            parser.Throw(
              Errors.Singleton loc (fun () -> $"Tab {tabId} already defined.")
              |> Errors.MapPriority(replaceWith ErrorPriority.High)
            )
        else
          return! parseMembers fields (tabs.Add(tabId, tab))

    }

  and parseDisableBlock () =
    parser {
      do! keyword Disable
      return! parseFieldSeq () |> parser.Map(Set.map FieldIdentifier)
    }

  and parseHighlights () =
    parser {
      do! keyword Highlights
      return! parseFieldSeq ()
    }

  and parseDetailRenderer () =
    parser {
      do! keyword Details
      return! parseRenderer ()
    }

  and parsePreviewRenderer () =
    parser {
      do! keyword Preview
      return! parseRenderer ()
    }

  and parseFormBody () =
    parser {
      do! operator (CurlyBracket Open)
      let! fields, tabs = parseMembers Map.empty Map.empty
      let! disabledFields = parser.Try(parseDisableBlock ()) |> parser.Map Sum.toOption
      let! detailsRenderer = parser.Try(parseDetailRenderer ()) |> parser.Map Sum.toOption

      let! highlights =
        parser.Try(parseHighlights ())
        |> parser.Map(
          Sum.toOption
          >> Option.toList
          >> Seq.concat
          >> Set.ofSeq
          >> Set.map FieldIdentifier
        )

      do! operator (CurlyBracket Close)
      return fields, tabs, disabledFields, detailsRenderer, highlights
    }

  and parseRecord () =
    parser {
      do! keyword Record

      let! rendererId =
        parser.Try(parseRendererDef ())
        |> parser.Map(Sum.toOption >> Option.map RendererIdentifier)

      do! operator (CurlyBracket Open)
      let! fields, tabs = parseMembers Map.empty Map.empty
      let! disabledFields = parser.Try(parseDisableBlock ()) |> parser.Map Sum.toOption
      do! operator (CurlyBracket Close)

      return
        { Renderer = rendererId
          Members = { Fields = fields; Tabs = tabs }
          DisabledFields =
            match disabledFields with
            | None -> Set.empty
            | Some set -> set
          Type = Unchecked }
    }

  and parseInlineForm () =
    parser {
      do! keyword View

      let! rendererId =
        parser.Try(parseRendererDef ())
        |> parser.Map(Sum.toOption >> Option.map RendererIdentifier)

      let! fields, tabs, disabledFields, detailsRenderer, highlights = parseFormBody ()

      return
        { InlineForm = rendererId
          Body = createFormBody fields tabs disabledFields detailsRenderer highlights
          Type = Unchecked }
    }

  and parseBodyWithDetailsAndPreview () =
    parser {
      let! detailsRenderer = parseDetailRenderer ()
      let! previewRenderer = parser.Try(parsePreviewRenderer ()) |> parser.Map Sum.toOption
      return detailsRenderer, previewRenderer
    }

  and parseOne () =
    parser {
      do! keyword One
      let! rendererName = parseRendererDef ()
      do! keyword From
      let! api = identifier ()
      let! detailsRenderer, previewRenderer = parseBodyWithDetailsAndPreview ()

      return
        { One = RendererIdentifier rendererName
          Api = ApiIdentifier api
          Details = detailsRenderer
          Preview = previewRenderer
          Type = Unchecked }
    }

  and parseLinkedUnlinked () =
    parser {
      do! keyword Linked
      let! linkedRenderer = parseRenderer ()

      let! unlinkedRenderer =
        parser.Try(
          parser {
            do! keyword Unlinked
            return! parseRenderer ()
          }
        )
        |> parser.Map Sum.toOption

      return linkedRenderer, unlinkedRenderer
    }

  and parseManyBody () =
    parser.Any(
      [ parseLinkedUnlinked ()
        |> parser.Map(fun (linked, unlinked) -> LinkedUnlinked { Linked = linked; Unlinked = unlinked })
        parser {
          do! keyword Element
          return! parseRenderer ()
        }
        |> parser.Map ManyRendererDefinition.Element ]
    )

  and parseMany () =
    parser {
      do! keyword Many
      let! rendererName = parseRendererDef ()
      do! keyword From
      let! apiId = identifier () |> parser.Map ApiIdentifier
      let! body = parseManyBody ()

      return
        { Many = RendererIdentifier rendererName
          Api = apiId
          Body = body
          Type = Unchecked }
    }

  and parsePinco() = 
    parser {
      do! keyword Pinco
      let! rendererName = parseRendererDef ()
      
      return 
        { Pinco = RendererIdentifier rendererName
          Type = Unchecked }
    }

  and parseRenderer () =
    parser.Any(
      [ parsePrimitive () |> parser.Map RendererExpression.Primitive
        parseMap () |> parser.Map RendererExpression.Map
        parseTuple () |> parser.Map RendererExpression.Tuple
        parseList () |> parser.Map RendererExpression.List
        parseReadonly () |> parser.Map RendererExpression.Readonly
        parseSum () |> parser.Map RendererExpression.Sum
        parseEnum () |> parser.Map RendererExpression.Enum
        parseStream () |> parser.Map RendererExpression.Stream
        parseTable () |> parser.Map RendererExpression.Table
        parseForm () |> parser.Map(fun expr -> RendererExpression.Form(expr, Unchecked))
        parseInlineForm () |> parser.Map RendererExpression.InlineForm
        parseUnion () |> parser.Map RendererExpression.Union
        parseRecord () |> parser.Map RendererExpression.Record
        parseOne () |> parser.Map RendererExpression.One
        parseMany () |> parser.Map RendererExpression.Many 
        parsePinco () |> parser.Map RendererExpression.Pinco
        parseReferenceOne () |> parser.Map RendererExpression.ReferenceOne ]
    )

  and createFormBody fields tabs disabledFields detailsRenderer highlights =
    { Members = { Fields = fields; Tabs = tabs }
      DisabledFields =
        match disabledFields with
        | None -> Set.empty
        | Some set -> set
      Details = detailsRenderer
      Highlights = highlights }


  and parseFormTable () =
    parser {
      let! isEntryPoint = parser.Try(keyword EntryPoint) |> parser.Map(Sum.toOption >> Option.isSome)
      do! keyword View

      return!
        parser {
          let! rendererId =
            parser.Try(parseRendererDef ())
            |> parser.Map(Sum.toOption >> Option.map RendererIdentifier)

          let! formName = identifier ()
          do! operator Colon
          let! typeName = identifier ()
          let! fields, tabs, disabledFields, detailsRenderer, highlights = parseFormBody ()

          return
            { IsEntryPoint = isEntryPoint
              RendererId = rendererId
              Form = FormIdentifier formName
              TypeIdentifier = TypeIdentifier typeName
              Body = createFormBody fields tabs disabledFields detailsRenderer highlights
              Type = Unchecked }
        }
        |> parser.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
    }

  and parseFormSpec () =
    parser {
      //improve by checking if an existing form identifier has already been parsed.
      let! forms =
        parser.AtLeastOne(parseFormTable ())
        |> parser.Map(
          NonEmptyList.map (fun form -> form.Form, form)
          >> NonEmptyList.ToList
          >> OrderedMap.ofList
        )

      do! parser.EndOfStream()
      return { Forms = forms }
    }

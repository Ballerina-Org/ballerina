namespace Ballerina.DSL.FormEngine.Parser

module Renderers =
  open Ballerina.DSL.Parser.Patterns
  open Ballerina.DSL.Parser.Expr

  open Ballerina.DSL.FormEngine.Model
  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Types.Model
  open FormsPatterns
  open System
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Map
  open Ballerina.State.WithError
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.StdLib.String
  open FSharp.Data
  open Ballerina.Collections.NonEmptyList
  open RendererDefinitions.Many

  type Renderer<'ExprExtension, 'ValueExtension> with
    static member ParseBoolRenderer
      (label: string option)
      (_: (string * JsonValue)[])
      (json: JsonValue)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! config = state.GetContext()
        let! s = json |> JsonValue.AsString |> state.OfSum

        if config.Bool.SupportedRenderers |> Set.contains s then
          return
            PrimitiveRenderer
              { PrimitiveRendererName = s
                PrimitiveRendererId = Guid.CreateVersion7()
                Label = label
                Type = ExprType.PrimitiveType PrimitiveType.BoolType }
        else
          return!
            state.Throw(Errors.Singleton $"Error: cannot parse bool renderer from {json.ToString().ReasonablyClamped}")
      }

  type Renderer<'ExprExtension, 'ValueExtension> with
    static member ParseDateRenderer
      (label: string option)
      (_: (string * JsonValue)[])
      (json: JsonValue)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! config = state.GetContext()
        let! s = json |> JsonValue.AsString |> state.OfSum

        if config.Date.SupportedRenderers |> Set.contains s then
          return
            PrimitiveRenderer
              { PrimitiveRendererName = s
                PrimitiveRendererId = Guid.CreateVersion7()
                Label = label
                Type = ExprType.PrimitiveType PrimitiveType.DateOnlyType }
        else
          return!
            state.Throw(Errors.Singleton $"Error: cannot parse date renderer from {json.ToString().ReasonablyClamped}")
      }


  type Renderer<'ExprExtension, 'ValueExtension> with
    static member ParseUnitRenderer
      (label: string option)
      (_: (string * JsonValue)[])
      (json: JsonValue)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! config = state.GetContext()
        let! s = json |> JsonValue.AsString |> state.OfSum

        if config.Unit.SupportedRenderers |> Set.contains s then
          return
            PrimitiveRenderer
              { PrimitiveRendererName = s
                PrimitiveRendererId = Guid.CreateVersion7()
                Label = label
                Type = ExprType.UnitType }
        else
          return!
            state.Throw(Errors.Singleton $"Error: cannot parse unit renderer from {json.ToString().ReasonablyClamped}")
      }

  type Renderer<'ExprExtension, 'ValueExtension> with
    static member ParseGuidRenderer
      (label: string option)
      (_: (string * JsonValue)[])
      (json: JsonValue)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! config = state.GetContext()
        let! s = json |> JsonValue.AsString |> state.OfSum

        if config.Guid.SupportedRenderers |> Set.contains s then
          return
            PrimitiveRenderer
              { PrimitiveRendererName = s
                PrimitiveRendererId = Guid.CreateVersion7()
                Label = label
                Type = ExprType.PrimitiveType PrimitiveType.GuidType }
        else
          return!
            state.Throw(Errors.Singleton $"Error: cannot parse guid renderer from {json.ToString().ReasonablyClamped}")
      }

  type Renderer<'ExprExtension, 'ValueExtension> with
    static member ParseIntRenderer
      (label: string option)
      (_: (string * JsonValue)[])
      (json: JsonValue)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! config = state.GetContext()
        let! s = json |> JsonValue.AsString |> state.OfSum

        if config.Int.SupportedRenderers |> Set.contains s then
          return
            PrimitiveRenderer
              { PrimitiveRendererName = s
                PrimitiveRendererId = Guid.CreateVersion7()
                Label = label
                Type = ExprType.PrimitiveType PrimitiveType.IntType }
        else
          return!
            state.Throw(Errors.Singleton $"Error: cannot parse int renderer from {json.ToString().ReasonablyClamped}")
      }

  type Renderer<'ExprExtension, 'ValueExtension> with
    static member ParseStringRenderer
      (label: string option)
      (_: (string * JsonValue)[])
      (json: JsonValue)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! config = state.GetContext()
        let! s = json |> JsonValue.AsString |> state.OfSum

        if config.String.SupportedRenderers |> Set.contains s then
          return
            PrimitiveRenderer
              { PrimitiveRendererName = s
                PrimitiveRendererId = Guid.CreateVersion7()
                Label = label
                Type = ExprType.PrimitiveType PrimitiveType.StringType }
        else
          return!
            state.Throw(
              Errors.Singleton $"Error: cannot parse string renderer from {json.ToString().ReasonablyClamped}"
            )
      }

  type Renderer<'ExprExtension, 'ValueExtension> with
    static member ParseEnumRenderer
      (label: string option)
      (parentJsonFields: (string * JsonValue)[])
      (json: JsonValue)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! config = state.GetContext()
        let! s = json |> JsonValue.AsString |> state.OfSum

        if
          config.Option.SupportedRenderers.Enum |> Set.contains s
          || config.Set.SupportedRenderers.Enum |> Set.contains s
        then
          let containerTypeConstructor =
            if config.Option.SupportedRenderers.Enum |> Set.contains s then
              ExprType.OptionType
            else
              ExprType.SetType

          return!
            state {

              let! (formsState: ParsedFormsContext<'ExprExtension, 'ValueExtension>) = state.GetState()
              let! optionJson = parentJsonFields |> sum.TryFindField "options" |> state.OfSum
              let! enumName = optionJson |> JsonValue.AsString |> state.OfSum
              let! enum = formsState.TryFindEnum enumName |> state.OfSum
              let! enumType = formsState.TryFindType enum.TypeId.VarName |> state.OfSum

              return
                EnumRenderer(
                  enum |> EnumApi.Id,
                  PrimitiveRenderer
                    { PrimitiveRendererName = s
                      PrimitiveRendererId = Guid.CreateVersion7()
                      Label = label
                      Type = containerTypeConstructor (enumType.Type) }
                )
            }
            |> state.MapError(Errors.WithPriority ErrorPriority.High)
        else
          return!
            state.Throw(Errors.Singleton $"Error: cannot parse enum renderer from {json.ToString().ReasonablyClamped}")
      }

  type Renderer<'ExprExtension, 'ValueExtension> with
    static member ParseStreamRenderer
      (label: string option)
      (parentJsonFields: (string * JsonValue)[])
      (json: JsonValue)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! config = state.GetContext()
        let! s = json |> JsonValue.AsString |> state.OfSum

        if
          config.Option.SupportedRenderers.Stream |> Set.contains s
          || config.Set.SupportedRenderers.Stream |> Set.contains s
        then
          let containerTypeConstructor =
            if config.Option.SupportedRenderers.Stream |> Set.contains s then
              ExprType.OptionType
            else
              ExprType.SetType

          return!
            state {
              let! streamNameJson = parentJsonFields |> sum.TryFindField "stream" |> state.OfSum
              let! (formsState: ParsedFormsContext<'ExprExtension, 'ValueExtension>) = state.GetState()

              let! stream, streamType =
                (state.Either
                  (state {
                    let! streamName = streamNameJson |> JsonValue.AsString |> state.OfSum

                    return!
                      state {
                        let! stream = formsState.TryFindStream streamName |> state.OfSum
                        let! streamType = formsState.TryFindType stream.TypeId.VarName |> state.OfSum
                        return StreamRendererApi.Stream(StreamApi.Id stream), streamType
                      }
                      |> state.MapError(Errors.WithPriority ErrorPriority.High)
                  })
                  (state {
                    let! streamTypeName, streamName = streamNameJson |> JsonValue.AsPair |> state.OfSum

                    return!
                      state {
                        let! streamTypeName, streamName =
                          state.All2
                            (streamTypeName |> JsonValue.AsString |> state.OfSum)
                            (streamName |> JsonValue.AsString |> state.OfSum)

                        let! stream = formsState.TryFindLookupStream streamTypeName streamName |> state.OfSum
                        let! streamType = formsState.TryFindType stream.TypeId.VarName |> state.OfSum
                        let! lookupType = formsState.TryFindType streamTypeName |> state.OfSum

                        return
                          StreamRendererApi.LookupStream(
                            {| Type = lookupType.TypeId
                               Stream = StreamApi.Id stream |}
                          ),
                          streamType
                      }
                      |> state.MapError(Errors.WithPriority ErrorPriority.High)
                  }))
                |> state.MapError Errors.HighestPriority


              return
                StreamRenderer(
                  stream,
                  PrimitiveRenderer
                    { PrimitiveRendererName = s
                      PrimitiveRendererId = Guid.CreateVersion7()
                      Label = label
                      Type = containerTypeConstructor (ExprType.LookupType streamType.TypeId) }
                )
            }
            |> state.MapError(Errors.WithPriority ErrorPriority.High)
        else
          return!
            state.Throw(
              Errors.Singleton $"Error: cannot parse stream renderer from {json.ToString().ReasonablyClamped}"
            )
      }

  type Renderer<'ExprExtension, 'ValueExtension> with
    static member ParseMapRenderer
      (label: string option)
      (parseNestedRenderer)
      (parentJsonFields: (string * JsonValue)[])
      (json: JsonValue)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! config = state.GetContext()
        let! s = json |> JsonValue.AsString |> state.OfSum

        if config.Map.SupportedRenderers |> Set.contains s then
          return!
            state {
              let! (keyRendererJson, valueRendererJson) =
                state.All2
                  (parentJsonFields |> state.TryFindField "keyRenderer")
                  (parentJsonFields |> state.TryFindField "valueRenderer")

              let! keyRenderer = parseNestedRenderer keyRendererJson
              let! valueRenderer = parseNestedRenderer valueRendererJson

              return
                MapRenderer
                  {| Label = label
                     Map =
                      PrimitiveRenderer
                        { PrimitiveRendererName = s
                          PrimitiveRendererId = Guid.CreateVersion7()
                          Label = label
                          Type = ExprType.MapType(keyRenderer.Renderer.Type, valueRenderer.Renderer.Type) }

                     Key = keyRenderer
                     Value = valueRenderer |}
            }
            |> state.MapError(Errors.WithPriority ErrorPriority.High)
        else
          return!
            state.Throw(Errors.Singleton $"Error: cannot parse map renderer from {json.ToString().ReasonablyClamped}")
      }

  type Renderer<'ExprExtension, 'ValueExtension> with
    static member ParseSumRenderer
      (label: string option)
      (parseNestedRenderer)
      (parentJsonFields: (string * JsonValue)[])
      (json: JsonValue)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! config = state.GetContext()
        let! s = json |> JsonValue.AsString |> state.OfSum

        if config.Sum.SupportedRenderers |> Set.contains s then
          return!
            state {
              let! (leftRendererJson, rightRendererJson) =
                state.All2
                  (parentJsonFields |> state.TryFindField "leftRenderer")
                  (parentJsonFields |> state.TryFindField "rightRenderer")

              let! leftRenderer = parseNestedRenderer leftRendererJson
              let! rightRenderer = parseNestedRenderer rightRendererJson

              return
                SumRenderer
                  {| Sum =
                      PrimitiveRenderer
                        { PrimitiveRendererName = s
                          PrimitiveRendererId = Guid.CreateVersion7()
                          Label = None
                          Type = ExprType.SumType(leftRenderer.Renderer.Type, rightRenderer.Renderer.Type) }
                     Label = label
                     Left = leftRenderer
                     Right = rightRenderer |}
            }
            |> state.MapError(Errors.WithPriority ErrorPriority.High)
        else
          return!
            state.Throw(Errors.Singleton $"Error: cannot parse sum renderer from {json.ToString().ReasonablyClamped}")
      }

  type Renderer<'ExprExtension, 'ValueExtension> with
    static member ParseOptionRenderer
      (label: string option)
      (parseNestedRenderer)
      (parentJsonFields: (string * JsonValue)[])
      (json: JsonValue)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! config = state.GetContext()
        let! s = json |> JsonValue.AsString |> state.OfSum

        if config.Option.SupportedRenderers.Plain |> Set.contains s then
          return!
            state {
              let! someRendererJson = parentJsonFields |> sum.TryFindField "someRenderer" |> state.OfSum
              let! someRenderer = parseNestedRenderer someRendererJson

              let! noneRendererJson = parentJsonFields |> sum.TryFindField "noneRenderer" |> state.OfSum
              let! noneRenderer = parseNestedRenderer noneRendererJson

              let res =
                OptionRenderer
                  {| Label = label
                     Option =
                      PrimitiveRenderer
                        { PrimitiveRendererName = s
                          PrimitiveRendererId = Guid.CreateVersion7()
                          Label = label
                          Type = ExprType.OptionType someRenderer.Renderer.Type }

                     Some = someRenderer
                     None = noneRenderer |}

              return res
            }
            |> state.MapError(Errors.WithPriority ErrorPriority.High)
        else
          return!
            state.Throw(
              Errors.Singleton $"Error: cannot parse option renderer from {json.ToString().ReasonablyClamped}"
            )
      }

  type Renderer<'ExprExtension, 'ValueExtension> with
    static member ParseOneRenderer
      (label: string option)
      (parseNestedRenderer)
      (parentJsonFields: (string * JsonValue)[])
      (json: JsonValue)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! config = state.GetContext()
        let! s = json |> JsonValue.AsString |> state.OfSum

        if config.One.SupportedRenderers |> Set.contains s then

          return!
            state {
              let! (formsState: ParsedFormsContext<'ExprExtension, 'ValueExtension>) = state.GetState()
              let! detailsJson = parentJsonFields |> state.TryFindField "detailsRenderer"

              let! previewJson =
                parentJsonFields
                |> state.TryFindField "previewRenderer"
                |> state.Catch
                |> state.Map(Sum.toOption)

              let! (details: NestedRenderer<'ExprExtension, 'ValueExtension>) = parseNestedRenderer detailsJson

              let! preview =
                previewJson
                |> Option.map (fun previewJson -> state { return! parseNestedRenderer previewJson })
                |> state.RunOption

              let! apiSourceTypeNameJson, oneApiNameJson =
                parentJsonFields
                |> sum.TryFindField "api"
                |> Sum.bind JsonValue.AsPair
                |> state.OfSum

              let! apiSourceTypeName, oneApiName =
                state.All2
                  (apiSourceTypeNameJson |> JsonValue.AsString |> state.OfSum)
                  (oneApiNameJson |> JsonValue.AsString |> state.OfSum)

              let! apiType = formsState.TryFindType apiSourceTypeName |> state.OfSum
              let! oneApi, _ = formsState.TryFindOne apiType.TypeId.VarName oneApiName |> state.OfSum

              let oneApiId: ExprTypeId * string = apiType.TypeId, oneApi.EntityName

              return
                OneRenderer
                  {| Label = label
                     One =
                      PrimitiveRenderer
                        { PrimitiveRendererName = s
                          PrimitiveRendererId = Guid.CreateVersion7()
                          Label = label
                          Type = ExprType.OneType details.Type }

                     OneApiId = oneApiId
                     Details = details
                     Preview = preview |}
            }
            |> state.MapError(Errors.WithPriority ErrorPriority.High)
        else
          return!
            state.Throw(Errors.Singleton $"Error: cannot parse one renderer from {json.ToString().ReasonablyClamped}")
      }

  type Renderer<'ExprExtension, 'ValueExtension> with
    static member ParseReadOnlyRenderer
      (label: string option)
      (parseNestedRenderer)
      (parentJsonFields: (string * JsonValue)[])
      (json: JsonValue)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! config = state.GetContext()
        let! s = json |> JsonValue.AsString |> state.OfSum

        if config.ReadOnly.SupportedRenderers |> Set.contains s then
          return!
            state {
              let! valueRendererJson = parentJsonFields |> state.TryFindField "childRenderer"
              let! valueRenderer = parseNestedRenderer valueRendererJson

              return
                ReadOnlyRenderer
                  {| ReadOnly =
                      PrimitiveRenderer
                        { PrimitiveRendererName = s
                          PrimitiveRendererId = Guid.CreateVersion7()
                          Label = None
                          Type = ExprType.ReadOnlyType valueRenderer.Renderer.Type }
                     Label = label
                     Value = valueRenderer |}
            }
            |> state.MapError(Errors.WithPriority ErrorPriority.High)
        else
          return!
            state.Throw(
              Errors.Singleton $"Error: cannot parse read only renderer from {json.ToString().ReasonablyClamped}"
            )
      }

  type Renderer<'ExprExtension, 'ValueExtension> with
    static member ParseListRenderer
      (label: string option)
      (parseNestedRenderer)
      (parentJsonFields: (string * JsonValue)[])
      (json: JsonValue)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! config = state.GetContext()
        let! s = json |> JsonValue.AsString |> state.OfSum

        if config.List.SupportedRenderers |> Set.contains s then

          return!
            state {
              let! (_: ParsedFormsContext<'ExprExtension, 'ValueExtension>) = state.GetState()
              let! (_: ParsedFormsContext<'ExprExtension, 'ValueExtension>) = state.GetState()
              let! elementRendererJson = parentJsonFields |> sum.TryFindField "elementRenderer" |> state.OfSum
              let! elementRenderer = parseNestedRenderer elementRendererJson

              let! actionLabelsJson =
                parentJsonFields
                |> state.TryFindField "actions"
                |> state.Catch
                |> state.Map Sum.toOption

              let! actionLabels = Renderer.ParseActionLabels actionLabelsJson

              return
                ListRenderer
                  {| Label = label
                     List =
                      PrimitiveRenderer
                        { PrimitiveRendererName = s
                          PrimitiveRendererId = Guid.CreateVersion7()
                          Label = label
                          Type = ExprType.ListType elementRenderer.Renderer.Type }
                     Element = elementRenderer
                     MethodLabels = actionLabels |}
            }
            |> state.MapError(Errors.WithPriority ErrorPriority.High)
        else
          return!
            state.Throw(Errors.Singleton $"Error: cannot parse list renderer from {json.ToString().ReasonablyClamped}")
      }

  and Renderer<'ExprExtension, 'ValueExtension> with
    static member ParseCustomRenderer
      (label: string option)
      (_)
      (_: (string * JsonValue)[])
      (json: JsonValue)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! config = state.GetContext()
        let! s = json |> JsonValue.AsString |> state.OfSum

        let! c =
          config.Custom
          |> Seq.tryFind (fun c -> c.Value.SupportedRenderers |> Set.contains s)
          |> Sum.fromOption (fun () -> $"Error: cannot parse custom renderer {s}" |> Errors.Singleton)
          |> state.OfSum

        return!
          state {
            let! (formsState: ParsedFormsContext<'ExprExtension, 'ValueExtension>) = state.GetState()
            let! t = formsState.TryFindType c.Key |> state.OfSum

            return
              PrimitiveRenderer
                { PrimitiveRendererName = s
                  PrimitiveRendererId = Guid.CreateVersion7()
                  Label = label
                  Type = t.Type }
          }
          |> state.MapError(Errors.WithPriority ErrorPriority.High)
      }

  and FormBody<'ExprExtension, 'ValueExtension> with
    static member Parse
      (primitivesExt: FormParserPrimitivesExtension<'ExprExtension, 'ValueExtension>)
      (exprParser: ExprParser<'ExprExtension, 'ValueExtension>)
      (fields: (string * JsonValue)[])
      (formTypeId: ExprTypeId)
      =
      state.Either3
        (state {
          let! casesJson = fields |> state.TryFindField "cases"

          return!
            state {
              let! casesJson = casesJson |> JsonValue.AsRecord |> state.OfSum
              let! rendererJson = fields |> state.TryFindField "renderer"
              let! renderer = JsonValue.AsString rendererJson |> state.OfSum
              let! ctx = state.GetContext()

              let! _ =
                fields
                |> state.TryFindField "containerRenderer"
                |> state.Catch
                |> state.Map(Sum.toOption)

              if ctx.Union.SupportedRenderers |> Set.contains renderer |> not then
                return! state.Throw(Errors.Singleton $"Error: cannot find union renderer {renderer}")
              else
                let! t = state.TryFindType formTypeId.VarName

                let! cases =
                  casesJson
                  |> Seq.map (fun (caseName, caseJson) ->
                    state.Either
                      (state {
                        let! caseBody = Renderer.Parse primitivesExt exprParser [||] caseJson

                        return
                          caseName,
                          { Label = None
                            Tooltip = None
                            Details = None
                            Renderer = caseBody }
                      })
                      (state {
                        let! caseBody = NestedRenderer.Parse primitivesExt exprParser caseJson

                        return caseName, caseBody
                      })
                    |> state.MapError(Errors.Map(String.appendNewline $"\n...when parsing form case {caseName}")))
                  |> state.All
                  |> state.Map(Map.ofSeq)

                return
                  {| Cases = cases
                     Renderer =
                      PrimitiveRenderer
                        { PrimitiveRendererName = renderer
                          PrimitiveRendererId = Guid.CreateVersion7()
                          Label = None
                          Type = t.Type }

                     UnionType = t.Type |}
                  |> FormBody.Union
            }
            |> state.MapError(Errors.WithPriority ErrorPriority.High)
        })
        (state {
          let! columnsJson = fields |> state.TryFindField "columns"

          return!
            state {
              let! columnsJson = columnsJson |> JsonValue.AsRecord |> state.OfSum
              let! rendererJson = fields |> state.TryFindField "renderer"

              let! detailsJson =
                fields
                |> state.TryFindField "detailsRenderer"
                |> state.Catch
                |> state.Map Sum.toOption

              let! actionLabelsJson =
                fields
                |> state.TryFindField "actionLabels"
                |> state.Catch
                |> state.Map Sum.toOption

              // parse actionLabelsJson as a map of TableMethod to string
              let! actionLabels = Renderer.ParseActionLabels actionLabelsJson

              // let! previewJson =
              //   fields
              //   |> state.TryFindField "previewRenderer"
              //   |> state.Catch
              //   |> state.Map(Sum.toOption)

              let! renderer = rendererJson |> JsonValue.AsString |> state.OfSum
              let! (config: CodeGenConfig) = state.GetContext()
              let! t = state.TryFindType formTypeId.VarName

              let! details =
                detailsJson
                |> Option.map (fun detailsJson ->
                  state { return! NestedRenderer.Parse primitivesExt exprParser detailsJson })
                |> state.RunOption

              // let! preview =
              //   previewJson
              //   |> Option.map (fun previewJson ->
              //     state {
              //       let! previewFields = previewJson |> JsonValue.AsRecord |> state.OfSum

              //       return! FormBody.Parse previewFields formTypeId
              //     })
              //   |> state.RunOption

              if config.Table.SupportedRenderers |> Set.contains renderer |> not then
                return! state.Throw(Errors.Singleton $"Error: cannot find table renderer {renderer}")
              else
                let! columns =
                  columnsJson
                  |> Seq.map (fun (columnName, columnJson) ->
                    state {
                      let! columnBody = FieldConfig.Parse primitivesExt exprParser columnName columnJson

                      return columnName, { FieldConfig = columnBody }
                    }
                    |> state.MapError(Errors.Map(String.appendNewline $"\n...when parsing table column {columnName}")))
                  |> state.All
                  |> state.Map(Map.ofSeq)

                let! visibleColumnsJson = fields |> state.TryFindField "visibleColumns"

                let! visibleColumns =
                  FormBody.ParseGroup
                    primitivesExt
                    exprParser
                    "visibleColumns"
                    (columns |> Map.map (fun _ c -> c.FieldConfig))
                    visibleColumnsJson

                let! highlightedFilters = fields |> state.TryFindField "highlightedFilters" |> state.Catch
                let highlightedFilters = highlightedFilters |> Sum.toOption

                let! highlightedFilters =
                  highlightedFilters
                  |> Option.map (fun highlightedFilters ->
                    state {
                      let! highlightedFilters = highlightedFilters |> JsonValue.AsArray |> state.OfSum
                      return! highlightedFilters |> Seq.map (JsonValue.AsString >> state.OfSum) |> state.All
                    })
                  |> state.RunOption

                let highlightedFilters = highlightedFilters |> Option.defaultWith (fun () -> [])

                return
                  {| Columns = columns
                     RowType = t.Type
                     Details = details
                     //  Preview = preview
                     HighlightedFilters = highlightedFilters
                     Renderer = renderer
                     MethodLabels = actionLabels
                     VisibleColumns = visibleColumns |}
                  |> FormBody.Table
            }
            |> state.MapError(Errors.WithPriority ErrorPriority.High)
        })
        (state {
          let! formFields = FormFields<'ExprExtension, 'ValueExtension>.Parse primitivesExt exprParser fields
          let! t = state.TryFindType formTypeId.VarName

          let! rendererJson =
            fields
            |> state.TryFindField "renderer"
            |> state.Catch
            |> state.Map(Sum.toOption)

          let! renderer =
            rendererJson
            |> Option.map (JsonValue.AsString >> state.OfSum)
            |> state.RunOption

          return
            FormBody.Record
              {| Renderer = renderer
                 Fields = formFields
                 RecordType = t.Type |}
        })
      |> state.MapError(Errors.HighestPriority)

  and Renderer<'ExprExtension, 'ValueExtension> with
    static member ParseActionLabels(actionLabelsJson: option<JsonValue>) =
      state {
        let! actionLabels =
          actionLabelsJson
          |> Option.map (fun actionLabelsJson ->
            state {
              let! actionLabelsMap = actionLabelsJson |> JsonValue.AsRecord |> state.OfSum

              return!
                actionLabelsMap
                |> Seq.map (fun (k, v) ->
                  state {
                    let! method =
                      state {
                        return!
                          Map.ofSeq
                            [ "add", TableMethod.Add
                              "remove", TableMethod.Remove
                              "duplicate", TableMethod.Duplicate
                              "move", TableMethod.Move ]
                          |> Map.tryFindWithError k "TableMethod" k
                          |> sum.MapError(Errors.Map(String.appendNewline $"...when parsing actionLabels"))
                          |> state.OfSum
                      }

                    let! label = v |> JsonValue.AsString |> state.OfSum
                    return method, label
                  })
                |> state.All
                |> state.Map Map.ofSeq
            })
          |> state.RunOption

        return actionLabels |> Option.defaultValue Map.empty
      }

    static member Parse
      (primitivesExt: FormParserPrimitivesExtension<'ExprExtension, 'ValueExtension>)
      (exprParser: ExprParser<'ExprExtension, 'ValueExtension>)
      (parentJsonFields: (string * JsonValue)[])
      (json: JsonValue)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state.Either
        (state {
          let! config = state.GetContext()
          let! s = json |> JsonValue.AsString |> state.OfSum
          let! (formsState: ParsedFormsContext<'ExprExtension, 'ValueExtension>) = state.GetState()

          let label =
            parentJsonFields
            |> sum.TryFindField "label"
            |> Sum.toOption
            |> Option.bind (JsonValue.AsString >> Sum.toOption)

          return!
            state.Any(
              NonEmptyList.OfList(
                Renderer.ParseBoolRenderer label parentJsonFields json,
                [ Renderer.ParseDateRenderer label parentJsonFields json
                  Renderer.ParseUnitRenderer label parentJsonFields json
                  Renderer.ParseGuidRenderer label parentJsonFields json
                  Renderer.ParseIntRenderer label parentJsonFields json
                  Renderer.ParseStringRenderer label parentJsonFields json
                  Renderer.ParseEnumRenderer label parentJsonFields json
                  Renderer.ParseStreamRenderer label parentJsonFields json
                  Renderer.ParseMapRenderer label (NestedRenderer.Parse primitivesExt exprParser) parentJsonFields json
                  Renderer.ParseSumRenderer label (NestedRenderer.Parse primitivesExt exprParser) parentJsonFields json
                  Renderer.ParseOptionRenderer
                    label
                    (NestedRenderer.Parse primitivesExt exprParser)
                    parentJsonFields
                    json
                  Renderer.ParseOneRenderer label (NestedRenderer.Parse primitivesExt exprParser) parentJsonFields json
                  Renderer.ParseManyAllRenderer
                    label
                    (NestedRenderer.Parse primitivesExt exprParser)
                    parentJsonFields
                    json
                  Renderer.ParseManyItemRenderer
                    label
                    (NestedRenderer.Parse primitivesExt exprParser)
                    parentJsonFields
                    json
                  Renderer.ParseReadOnlyRenderer
                    label
                    (NestedRenderer.Parse primitivesExt exprParser)
                    parentJsonFields
                    json
                  Renderer.ParseListRenderer label (NestedRenderer.Parse primitivesExt exprParser) parentJsonFields json
                  Renderer.ParseCustomRenderer
                    label
                    (NestedRenderer.Parse primitivesExt exprParser)
                    parentJsonFields
                    json

                  state {
                    return!
                      state.Any(
                        NonEmptyList.OfList(
                          state {
                            let! { GenericRenderers = genericRenderers } = state.GetState()

                            match genericRenderers with
                            | [] -> return! state.Throw(Errors.Singleton $"Error: cannot match empty generic renderers")
                            | g :: gs ->
                              let genericRenderers = NonEmptyList.OfList(g, gs)

                              return!
                                genericRenderers
                                |> NonEmptyList.map (fun g ->
                                  state {
                                    if g.SupportedRenderers |> Set.contains s then
                                      return
                                        PrimitiveRenderer
                                          { PrimitiveRendererName = s
                                            PrimitiveRendererId = Guid.CreateVersion7()
                                            Label = label
                                            Type = g.Type }

                                    else
                                      return! state.Throw(Errors.Singleton $"Error: generic renderer does not match")
                                  })
                                |> state.Any
                          },
                          [ state {
                              let! tupleConfig =
                                config.Tuple
                                |> List.tryFind (fun t -> t.SupportedRenderers.Contains s)
                                |> Sum.fromOption (fun () ->
                                  Errors.Singleton $"Error: cannot find tuple config for renderer {s}")
                                |> state.OfSum

                              return!
                                state {
                                  let! itemRenderersJson =
                                    parentJsonFields |> sum.TryFindField "itemRenderers" |> state.OfSum

                                  let! itemRenderersJson = itemRenderersJson |> JsonValue.AsArray |> state.OfSum

                                  let! itemRenderers =
                                    itemRenderersJson
                                    |> Seq.map (NestedRenderer.Parse primitivesExt exprParser)
                                    |> state.All

                                  if itemRenderers.Length <> tupleConfig.Ariety then
                                    return!
                                      state.Throw(
                                        Errors.Singleton
                                          $"Error: mismatched tuple size. Expected {tupleConfig.Ariety}, found {itemRenderers.Length}."
                                      )
                                  else
                                    return
                                      TupleRenderer
                                        {| Label = label
                                           Tuple =
                                            PrimitiveRenderer
                                              { PrimitiveRendererName = s
                                                PrimitiveRendererId = Guid.CreateVersion7()
                                                Label = label
                                                Type =
                                                  ExprType.TupleType(
                                                    itemRenderers |> Seq.map (fun nr -> nr.Type) |> List.ofSeq
                                                  ) }
                                           Elements = itemRenderers |}
                                }
                                |> state.MapError(Errors.WithPriority ErrorPriority.High)
                            }
                            state {
                              let! form = formsState.TryFindForm s |> state.OfSum

                              return!
                                state {
                                  match form.Body with
                                  | FormBody.Union cases ->
                                    let formType = cases.UnionType

                                    return
                                      FormRenderer(form |> FormConfig<'ExprExtension, 'ValueExtension>.Id, formType)

                                    return
                                      FormRenderer(form |> FormConfig<'ExprExtension, 'ValueExtension>.Id, formType)
                                  | FormBody.Record fields ->
                                    return
                                      FormRenderer(
                                        form |> FormConfig<'ExprExtension, 'ValueExtension>.Id,
                                        fields.RecordType
                                      )

                                    return
                                      FormRenderer(
                                        form |> FormConfig<'ExprExtension, 'ValueExtension>.Id,
                                        fields.RecordType
                                      )
                                  | FormBody.Table _ ->
                                    let! tableApiNameJson = parentJsonFields |> sum.TryFindField "api" |> state.OfSum
                                    let! tableApiName = tableApiNameJson |> JsonValue.AsString |> state.OfSum
                                    let! tableApi = formsState.TryFindTableApi tableApiName |> state.OfSum

                                    let! tableType = formsState.TryFindType (fst tableApi).TypeId.VarName |> state.OfSum

                                    return
                                      TableFormRenderer(
                                        form |> FormConfig<'ExprExtension, 'ValueExtension>.Id,
                                        tableType.Type |> ExprType.TableType,
                                        tableApi |> fst |> TableApi.Id
                                      )
                                }
                                |> state.MapError(Errors.WithPriority ErrorPriority.High)
                            }
                            state.Throw(
                              Errors.Singleton $"Error: cannot resolve field renderer {s}."
                              |> Errors.WithPriority ErrorPriority.High
                            ) ]
                        )
                      )
                  } ]
              )
            )
            |> state.MapError(Errors.HighestPriority)
        })
        (state {
          let! _ = state.GetContext()
          let! (_: ParsedFormsContext<'ExprExtension, 'ValueExtension>) = state.GetState()
          let! (_: ParsedFormsContext<'ExprExtension, 'ValueExtension>) = state.GetState()

          let! fields = json |> JsonValue.AsRecord |> state.OfSum

          return!
            state.Either
              (state {

                let! typeJson = (fields |> state.TryFindField "type")

                return!
                  state {
                    let! typeName = typeJson |> JsonValue.AsString |> state.OfSum
                    let! (s: ParsedFormsContext<'ExprExtension, 'ValueExtension>) = state.GetState()
                    let! typeBinding = s.TryFindType typeName |> state.OfSum

                    let! formBody =
                      FormBody<'ExprExtension, 'ValueExtension>.Parse primitivesExt exprParser fields typeBinding.TypeId
                    // do Console.WriteLine $"found record for type {typeName}/{typeBinding.Type}"
                    // do Console.ReadLine() |> ignore

                    let! containerRendererJson =
                      fields
                      |> state.TryFindField "containerRenderer"
                      |> state.Catch
                      |> state.Map(Sum.toOption)

                    let! (containerRenderer: Option<string>) =
                      containerRendererJson
                      |> Option.map (JsonValue.AsString >> state.OfSum)
                      |> state.RunOption

                    return
                      Renderer.InlineFormRenderer
                        {| Body = formBody
                           ContainerRenderer = containerRenderer |}
                  }
                  |> state.MapError(Errors.WithPriority ErrorPriority.High)
              })
              (state {

                let! renderers =
                  fields
                  |> Seq.map (fun (fieldName, fieldJson) ->
                    state {
                      let! parsedField = NestedRenderer.Parse primitivesExt exprParser fieldJson
                      return fieldName, parsedField
                    })
                  |> state.All

                match renderers with
                | [] -> return! state.Throw(Errors.Singleton $"Error: cannot match empty generic renderers")
                | (firstName, firstRenderer) :: gs ->

                  return
                    Renderer.Multiple
                      {| First =
                          {| Name = firstName
                             NestedRenderer = firstRenderer |}
                         Rest = gs |> Map.ofList
                         Label = None |}
              })


        })
      |> state.MapError(Errors.HighestPriority)
  // |> state.WithErrorContext $"...when parsing renderer {json.ToString().ReasonablyClamped}"

  and NestedRenderer<'ExprExtension, 'ValueExtension> with
    static member Parse
      (primitivesExt: FormParserPrimitivesExtension<'ExprExtension, 'ValueExtension>)
      (exprParser: ExprParser<'ExprExtension, 'ValueExtension>)
      (json: JsonValue)
      : State<
          NestedRenderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! jsonFields = json |> JsonValue.AsRecord |> state.OfSum

        let! label =
          jsonFields
          |> sum.TryFindField "label"
          |> Sum.toOption
          |> Option.map (JsonValue.AsString >> state.OfSum)
          |> state.RunOption

        let! tooltip =
          jsonFields
          |> sum.TryFindField "tooltip"
          |> Sum.toOption
          |> Option.map (JsonValue.AsString >> state.OfSum)
          |> state.RunOption

        let! details =
          jsonFields
          |> sum.TryFindField "details"
          |> Sum.toOption
          |> Option.map (JsonValue.AsString >> state.OfSum)
          |> state.RunOption

        let! rendererJson = jsonFields |> state.TryFindField "renderer"
        let! renderer = Renderer.Parse primitivesExt exprParser jsonFields rendererJson

        return
          { Label = label
            Tooltip = tooltip
            Details = details
            Renderer = renderer }
      }
      |> state.WithErrorContext $"...when parsing (nested) renderer {json.ToString().ReasonablyClamped}"

  and FieldConfig<'ExprExtension, 'ValueExtension> with
    static member Parse
      (primitivesExt: FormParserPrimitivesExtension<'ExprExtension, 'ValueExtension>)
      (exprParser: ExprParser<'ExprExtension, 'ValueExtension>)
      (fieldName: string)
      (json: JsonValue)
      : State<
          FieldConfig<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! fields = json |> JsonValue.AsRecord |> state.OfSum

        let! label =
          fields
          |> sum.TryFindField "label"
          |> Sum.toOption
          |> Option.map (JsonValue.AsString >> state.OfSum)
          |> state.RunOption

        let! tooltip =
          fields
          |> sum.TryFindField "tooltip"
          |> Sum.toOption
          |> Option.map (JsonValue.AsString >> state.OfSum)
          |> state.RunOption

        let! details =
          fields
          |> sum.TryFindField "details"
          |> Sum.toOption
          |> Option.map (JsonValue.AsString >> state.OfSum)
          |> state.RunOption

        let! rendererJson, visibleJson =
          state.All2 (fields |> state.TryFindField "renderer") (fields |> state.TryFindField "visible" |> state.Catch)

        let! disabledJson = sum.TryFindField "disabled" fields |> state.OfSum |> state.Catch
        let disabledJson = disabledJson |> Sum.toOption
        let! renderer = Renderer.Parse primitivesExt exprParser fields rendererJson

        let! visible =
          visibleJson
          |> Sum.toOption
          |> Option.map (exprParser >> state.OfSum)
          |> state.RunOption

        let visible = visible |> Option.defaultWith (fun () -> primitivesExt.ConstBool true)

        let! disabled = disabledJson |> Option.map (exprParser >> state.OfSum) |> state.RunOption

        let fc =
          { FieldName = fieldName
            FieldId = Guid.CreateVersion7()
            Label = label
            Tooltip = tooltip
            Details = details
            Renderer = renderer
            Visible = visible
            Disabled = disabled }

        return fc
      }
      |> state.WithErrorContext $"...when parsing field {fieldName}"

  and FormFields<'ExprExtension, 'ValueExtension> with
    static member Parse
      (primitivesExt: FormParserPrimitivesExtension<'ExprExtension, 'ValueExtension>)
      (exprParser: ExprParser<'ExprExtension, 'ValueExtension>)
      (fields: (string * JsonValue)[])
      =
      state {
        let! fieldsJson, tabsJson =
          state.All2 (fields |> state.TryFindField "fields") (fields |> state.TryFindField "tabs")

        return!
          state {

            let! extendsJson = fields |> state.TryFindField "extends" |> state.Catch |> state.Map Sum.toOption

            let! extendedForms =
              extendsJson
              |> Option.map (fun extendsJson ->
                state {
                  let! extendsJson = extendsJson |> JsonValue.AsArray |> state.OfSum

                  return!
                    extendsJson
                    |> Seq.map (fun extendJson ->
                      state {
                        let! extendsFormName = extendJson |> JsonValue.AsString |> state.OfSum
                        return! state.TryFindForm extendsFormName
                      })
                    |> state.All
                })
              |> state.RunOption

            let! extendedFields =
              match extendedForms with
              | None -> state.Return []
              | Some fs ->
                fs
                |> Seq.map (fun f -> FormBody.TryGetFields f.Body)
                |> state.All
                |> state.Map(List.map (fun f -> f.Fields.Fields))

            let! formFields = fieldsJson |> JsonValue.AsRecord |> state.OfSum

            let! fieldConfigs =
              formFields
              |> Seq.map (fun (fieldName, fieldJson) ->
                state {
                  let! parsedField =
                    FieldConfig<'ExprExtension, 'ValueExtension>.Parse primitivesExt exprParser fieldName fieldJson

                  return fieldName, parsedField
                })
              |> state.All<_, CodeGenConfig, ParsedFormsContext<'ExprExtension, 'ValueExtension>, Errors>

            let fieldConfigs = fieldConfigs |> Map.ofSeq
            let fieldConfigs = Map.mergeMany (fun x _ -> x) (fieldConfigs :: extendedFields)

            let! disabledFieldsJson =
              fields
              |> state.TryFindField "disabledFields"
              |> state.Catch
              |> state.Map Sum.toOption
              |> state.Map(Option.defaultWith (fun () -> JsonValue.Array [||]))

            let! disabledFields =
              FormBody.ParseGroup primitivesExt exprParser "disabledFields" fieldConfigs disabledFieldsJson

            let! tabs =
              FormBody<'ExprExtension, 'ValueExtension>.ParseTabs primitivesExt exprParser fieldConfigs tabsJson

            return
              { FormFields.Fields = fieldConfigs
                FormFields.Disabled = disabledFields
                FormFields.Tabs = tabs }
          }
          |> state.MapError(Errors.WithPriority ErrorPriority.High)
      }

  and FormBody<'ExprExtension, 'ValueExtension> with
    static member ParseTabs
      (primitivesExt: FormParserPrimitivesExtension<'ExprExtension, 'ValueExtension>)
      (exprParser: ExprParser<'ExprExtension, 'ValueExtension>)
      fieldConfigs
      (json: JsonValue)
      : State<
          FormTabs<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! tabs = json |> JsonValue.AsRecord |> state.OfSum

        let! tabs =
          seq {
            for tabName, tabJson in tabs do
              yield
                state {
                  let! column = FormBody.ParseTab primitivesExt exprParser tabName fieldConfigs tabJson
                  return tabName, column
                }
          }
          |> state.All
          |> state.Map Map.ofList

        return { FormTabs = tabs }
      }
      |> state.WithErrorContext $"...when parsing tabs"

    static member ParseGroup
      (_primitivesExt: FormParserPrimitivesExtension<'ExprExtension, 'ValueExtension>)
      (exprParser: ExprParser<'ExprExtension, 'ValueExtension>)
      (groupName: string)
      (fieldConfigs: Map<string, FieldConfig<'ExprExtension, 'ValueExtension>>)
      (json: JsonValue)
      : State<
          FormGroup<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state.Either
        (state {
          let! fields = json |> JsonValue.AsArray |> state.OfSum

          return!
            seq {
              for fieldJson in fields do
                yield
                  state {
                    let! fieldName = fieldJson |> JsonValue.AsString |> state.OfSum

                    return!
                      fieldConfigs
                      |> Map.tryFindWithError fieldName "field name" fieldName
                      |> Sum.map (FieldConfig.Id)
                      |> state.OfSum
                  }
            }
            |> state.All
            |> state.Map(FormGroup.Inlined)
            |> state.MapError(Errors.WithPriority ErrorPriority.High)
        })
        (state {
          let! expr = json |> exprParser |> state.OfSum
          return FormGroup.Computed expr
        })
      |> state.WithErrorContext $"...when parsing group {groupName}"

    static member ParseColumn
      (primitivesExt: FormParserPrimitivesExtension<'ExprExtension, 'ValueExtension>)
      (exprParser: ExprParser<'ExprExtension, 'ValueExtension>)
      (columnName: string)
      fieldConfigs
      (json: JsonValue)
      : State<
          FormGroups<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! jsonFields = json |> JsonValue.AsRecord |> state.OfSum

        match jsonFields with
        | [| "groups", JsonValue.Record groups |] ->
          let! groups =
            seq {
              for groupName, groupJson in groups do
                yield
                  state {
                    let! column = FormBody.ParseGroup primitivesExt exprParser groupName fieldConfigs groupJson
                    return groupName, column
                  }
            }
            |> state.All
            |> state.Map Map.ofList

          return { FormGroups = groups }
        | _ ->
          return!
            $"Error: cannot parse groups. Expected a single field 'groups', instead found {json}"
            |> Errors.Singleton
            |> state.Throw
      }
      |> state.WithErrorContext $"...when parsing column {columnName}"

    static member ParseTab
      (primitivesExt: FormParserPrimitivesExtension<'ExprExtension, 'ValueExtension>)
      (exprParser: ExprParser<'ExprExtension, 'ValueExtension>)
      (tabName: string)
      fieldConfigs
      (json: JsonValue)
      : State<
          FormColumns<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! jsonFields = json |> JsonValue.AsRecord |> state.OfSum

        match jsonFields with
        | [| "columns", JsonValue.Record columns |] ->
          let! columns =
            seq {
              for columnName, columnJson in columns do
                yield
                  state {
                    let! column = FormBody.ParseColumn primitivesExt exprParser columnName fieldConfigs columnJson
                    return columnName, column
                  }
            }
            |> state.All
            |> state.Map Map.ofList

          return { FormColumns = columns }
        | _ ->
          return!
            $"Error: cannot parse columns. Expected a single field 'columns', instead found {json}"
            |> Errors.Singleton
            |> state.Throw
      }
      |> state.WithErrorContext $"...when parsing tab {tabName}"

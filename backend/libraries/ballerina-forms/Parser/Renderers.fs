namespace Ballerina.DSL.FormEngine.Parser

module Renderers =
  open Ballerina.DSL.Parser.Patterns
  open Ballerina.DSL.Parser.Expr

  open Ballerina.DSL.FormEngine.Model
  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Types.Patterns
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
    static member private ParseBoolRenderer
      (label: Label option)
      (_: (string * JsonValue)[])
      (name: RendererName)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! config = state.GetContext()

        if config.Bool.SupportedRenderers |> Set.contains name then
          return
            PrimitiveRenderer
              { PrimitiveRendererName = name
                PrimitiveRendererId = Guid.CreateVersion7()
                Label = label
                Type = ExprType.PrimitiveType PrimitiveType.BoolType }
        else
          return! state.Throw(Errors.Singleton $"Error: cannot parse bool renderer from {name}")
      }

  type Renderer<'ExprExtension, 'ValueExtension> with
    static member private ParseDateRenderer
      (label: Label option)
      (_: (string * JsonValue)[])
      (name: RendererName)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! config = state.GetContext()

        if config.Date.SupportedRenderers |> Set.contains name then
          return
            PrimitiveRenderer
              { PrimitiveRendererName = name
                PrimitiveRendererId = Guid.CreateVersion7()
                Label = label
                Type = ExprType.PrimitiveType PrimitiveType.DateOnlyType }
        else
          return! state.Throw(Errors.Singleton $"Error: cannot parse date renderer from {name}")
      }


  type Renderer<'ExprExtension, 'ValueExtension> with
    static member private ParseUnitRenderer
      (label: Label option)
      (_: (string * JsonValue)[])
      (name: RendererName)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! config = state.GetContext()

        if config.Unit.SupportedRenderers |> Set.contains name then
          return
            PrimitiveRenderer
              { PrimitiveRendererName = name
                PrimitiveRendererId = Guid.CreateVersion7()
                Label = label
                Type = ExprType.UnitType }
        else
          return! state.Throw(Errors.Singleton $"Error: cannot parse unit renderer from {name}")
      }

  type Renderer<'ExprExtension, 'ValueExtension> with
    static member private ParseGuidRenderer
      (label: Label option)
      (_: (string * JsonValue)[])
      (name: RendererName)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! config = state.GetContext()

        if config.Guid.SupportedRenderers |> Set.contains name then
          return
            PrimitiveRenderer
              { PrimitiveRendererName = name
                PrimitiveRendererId = Guid.CreateVersion7()
                Label = label
                Type = ExprType.PrimitiveType PrimitiveType.GuidType }
        else
          return! state.Throw(Errors.Singleton $"Error: cannot parse guid renderer from {name}")
      }

  type Renderer<'ExprExtension, 'ValueExtension> with
    static member private ParseIntRenderer
      (label: Label option)
      (_: (string * JsonValue)[])
      (name: RendererName)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! config = state.GetContext()

        if config.Int.SupportedRenderers |> Set.contains name then
          return
            PrimitiveRenderer
              { PrimitiveRendererName = name
                PrimitiveRendererId = Guid.CreateVersion7()
                Label = label
                Type = ExprType.PrimitiveType PrimitiveType.IntType }
        else
          return! state.Throw(Errors.Singleton $"Error: cannot parse int renderer from {name}")
      }

  type Renderer<'ExprExtension, 'ValueExtension> with
    static member private ParseStringRenderer
      (label: Label option)
      (_: (string * JsonValue)[])
      (name: RendererName)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! config = state.GetContext()

        if config.String.SupportedRenderers |> Set.contains name then
          return
            PrimitiveRenderer
              { PrimitiveRendererName = name
                PrimitiveRendererId = Guid.CreateVersion7()
                Label = label
                Type = ExprType.PrimitiveType PrimitiveType.StringType }
        else
          return! state.Throw(Errors.Singleton $"Error: cannot parse string renderer from {name}")
      }

  type Renderer<'ExprExtension, 'ValueExtension> with
    static member private ParseEnumRenderer
      (label: Label option)
      (parentJsonFields: (string * JsonValue)[])
      (name: RendererName)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! config = state.GetContext()

        if
          config.Option.SupportedRenderers.Enum |> Set.contains name
          || config.Set.SupportedRenderers.Enum |> Set.contains name
        then
          let rendererType =
            if config.Option.SupportedRenderers.Enum |> Set.contains name then
              EnumRendererType.Option
            else
              EnumRendererType.Set

          return!
            state {

              let! (formsState: ParsedFormsContext<'ExprExtension, 'ValueExtension>) = state.GetState()
              let! optionJson = parentJsonFields |> sum.TryFindField "options" |> state.OfSum
              let! enumName = optionJson |> JsonValue.AsString |> state.OfSum
              let! enum = formsState.TryFindEnum enumName |> state.OfSum

              return EnumRenderer(enum |> EnumApi.Id, label, rendererType, enum.TypeId, name)
            }
            |> state.MapError(Errors.WithPriority ErrorPriority.High)
        else
          return! state.Throw(Errors.Singleton $"Error: cannot parse enum renderer from {name}")
      }

  type Renderer<'ExprExtension, 'ValueExtension> with
    static member private ParseStreamRenderer
      (label: Label option)
      (parentJsonFields: (string * JsonValue)[])
      (name: RendererName)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! config = state.GetContext()

        if
          config.Option.SupportedRenderers.Stream |> Set.contains name
          || config.Set.SupportedRenderers.Stream |> Set.contains name
        then
          let rendererType =
            if config.Option.SupportedRenderers.Stream |> Set.contains name then
              StreamRendererType.Option
            else
              StreamRendererType.Set

          return!
            state {
              let! streamNameJson = parentJsonFields |> sum.TryFindField "stream" |> state.OfSum
              let! (formsState: ParsedFormsContext<'ExprExtension, 'ValueExtension>) = state.GetState()

              let! stream, streamTypeId =
                (state.Either
                  (state {
                    let! streamName = streamNameJson |> JsonValue.AsString |> state.OfSum

                    return!
                      state {
                        let! stream = formsState.TryFindStream streamName |> state.OfSum
                        return StreamRendererApi.Stream(StreamApi.Id stream), stream.TypeId
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
                        let! lookupType = formsState.TryFindType streamTypeName |> state.OfSum

                        return
                          StreamRendererApi.LookupStream(
                            {| Type = lookupType.TypeId
                               Stream = StreamApi.Id stream |}
                          ),
                          stream.TypeId
                      }
                      |> state.MapError(Errors.WithPriority ErrorPriority.High)
                  }))
                |> state.MapError Errors.HighestPriority


              return StreamRenderer(stream, label, rendererType, streamTypeId, name)
            }
            |> state.MapError(Errors.WithPriority ErrorPriority.High)
        else
          return! state.Throw(Errors.Singleton $"Error: cannot parse stream renderer from {name}")
      }

  type Renderer<'ExprExtension, 'ValueExtension> with
    static member private ParseMapRenderer
      (label: Label option)
      (parseNestedRenderer)
      (parentJsonFields: (string * JsonValue)[])
      (name: RendererName)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! config = state.GetContext()

        if config.Map.SupportedRenderers |> Set.contains name then
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
                  { Label = label
                    Map = name
                    Key = keyRenderer
                    Value = valueRenderer }
            }
            |> state.MapError(Errors.WithPriority ErrorPriority.High)
        else
          return! state.Throw(Errors.Singleton $"Error: cannot parse map renderer from {name}")
      }

  type Renderer<'ExprExtension, 'ValueExtension> with
    static member private ParseSumRenderer
      (label: Label option)
      (parseNestedRenderer)
      (parentJsonFields: (string * JsonValue)[])
      (name: RendererName)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! config = state.GetContext()

        if config.Sum.SupportedRenderers |> Set.contains name then
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
                  { Sum = name
                    Label = label
                    Left = leftRenderer
                    Right = rightRenderer }
            }
            |> state.MapError(Errors.WithPriority ErrorPriority.High)
        else
          return! state.Throw(Errors.Singleton $"Error: cannot parse sum renderer from {name}")
      }

  type Renderer<'ExprExtension, 'ValueExtension> with
    static member ParseOptionRenderer
      (label: Label option)
      (parseNestedRenderer)
      (parentJsonFields: (string * JsonValue)[])
      (name: RendererName)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! config = state.GetContext()

        if config.Option.SupportedRenderers.Plain |> Set.contains name then
          return!
            state {
              let! someRendererJson = parentJsonFields |> sum.TryFindField "someRenderer" |> state.OfSum
              let! someRenderer = parseNestedRenderer someRendererJson

              let! noneRendererJson = parentJsonFields |> sum.TryFindField "noneRenderer" |> state.OfSum
              let! noneRenderer = parseNestedRenderer noneRendererJson

              let res =
                OptionRenderer
                  { Label = label
                    Option = name
                    Some = someRenderer
                    None = noneRenderer }

              return res
            }
            |> state.MapError(Errors.WithPriority ErrorPriority.High)
        else
          return! state.Throw(Errors.Singleton $"Error: cannot parse option renderer from {name}")
      }

  type Renderer<'ExprExtension, 'ValueExtension> with
    static member private ParseOneRenderer
      (label: Label option)
      (parseNestedRenderer)
      (parentJsonFields: (string * JsonValue)[])
      (name: RendererName)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! config = state.GetContext()

        if config.One.SupportedRenderers |> Set.contains name then

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
                  { Label = label
                    One = name
                    OneApiId = oneApiId
                    Details = details
                    Preview = preview }
            }
            |> state.MapError(Errors.WithPriority ErrorPriority.High)
        else
          return! state.Throw(Errors.Singleton $"Error: cannot parse one renderer from {name}")
      }

  type Renderer<'ExprExtension, 'ValueExtension> with
    static member private ParseReadOnlyRenderer
      (label: Label option)
      (parseNestedRenderer)
      (parentJsonFields: (string * JsonValue)[])
      (name: RendererName)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! config = state.GetContext()

        if config.ReadOnly.SupportedRenderers |> Set.contains name then
          return!
            state {
              let! valueRendererJson = parentJsonFields |> state.TryFindField "childRenderer"
              let! valueRenderer = parseNestedRenderer valueRendererJson

              return
                ReadOnlyRenderer
                  { ReadOnly = name
                    Label = label
                    Value = valueRenderer }
            }
            |> state.MapError(Errors.WithPriority ErrorPriority.High)
        else
          return! state.Throw(Errors.Singleton $"Error: cannot parse read only renderer from {name}")
      }

  type Renderer<'ExprExtension, 'ValueExtension> with
    static member private ParseAllTranslationOverridesRenderer
      (parentJsonFields: (string * JsonValue)[])
      (name: RendererName)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! config = state.GetContext()

        let! (mapRendererJson, keyRendererJson, optionsJson, valueRendererJson) =
          state.All4
            (parentJsonFields |> state.TryFindField "mapRenderer")
            (parentJsonFields |> state.TryFindField "keyRenderer")
            (parentJsonFields |> state.TryFindField "options")
            (parentJsonFields |> state.TryFindField "valueRenderer")

        let! mapRenderer = mapRendererJson |> JsonValue.AsString |> state.OfSum
        let! keyRenderer = keyRendererJson |> JsonValue.AsString |> state.OfSum
        let! options = optionsJson |> JsonValue.AsString |> state.OfSum
        let! valueRenderer = valueRendererJson |> JsonValue.AsString |> state.OfSum

        let! formsState = state.GetState()

        if formsState.TryFindEnum options |> Sum.toOption |> Option.isNone then
          return! state.Throw(Errors.Singleton $"Error: cannot find enum {options}")

        if config.Record.SupportedRenderers.Keys |> Seq.contains name |> not then
          return! state.Throw(Errors.Singleton $"Error: cannot parse record renderer from {name}")

        if config.Map.SupportedRenderers |> Set.contains (RendererName mapRenderer) |> not then
          return! state.Throw(Errors.Singleton $"Error: cannot parse map renderer from {mapRenderer}")

        let enumRenderers =
          Set.union config.Option.SupportedRenderers.Enum config.Set.SupportedRenderers.Enum

        if enumRenderers |> Set.contains (RendererName keyRenderer) |> not then
          return!
            state.Throw(Errors.Singleton $"Error: cannot parse key renderer from {keyRenderer}, must be enum renderer")

        if
          config.String.SupportedRenderers
          |> Set.contains (RendererName valueRenderer)
          |> not
        then
          return!
            state.Throw(
              Errors.Singleton $"Error: cannot parse value renderer from {valueRenderer}, must be string renderer"
            )

        let! typeId =
          state {
            let! typeFieldJson = parentJsonFields |> state.TryFindField "type"
            let! typeName = typeFieldJson |> JsonValue.AsString |> state.OfSum
            let! formsState = state.GetState()
            let! typeBinding = formsState.TryFindType typeName |> state.OfSum

            match typeBinding.Type with
            | ExprType.AllTranslationOverrides _ -> return typeBinding.TypeId
            | _ ->
              return!
                state.Throw(
                  Errors.Singleton
                    $"Error: cannot parse AllTranslationOverrides renderer from {name} because type {typeName} is not AllTranslationOverrides"
                )
          }

        return
          Renderer.AllTranslationOverridesRenderer
            { Renderer = name
              TypeId = typeId
              MapRenderer = RendererName mapRenderer
              KeyRenderer = RendererName keyRenderer
              Options = options
              ValueRenderer = RendererName valueRenderer }
      }

  type Renderer<'ExprExtension, 'ValueExtension> with
    static member private ParseListRenderer
      (label: Label option)
      (parseNestedRenderer)
      (parentJsonFields: (string * JsonValue)[])
      (name: RendererName)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! config = state.GetContext()

        if config.List.SupportedRenderers |> Set.contains name then

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
                  { Label = label
                    List = name
                    Element = elementRenderer
                    MethodLabels = actionLabels }
            }
            |> state.MapError(Errors.WithPriority ErrorPriority.High)
        else
          return! state.Throw(Errors.Singleton $"Error: cannot parse list renderer from {name}")
      }

  and Renderer<'ExprExtension, 'ValueExtension> with
    static member private ParseCustomRenderer
      (label: Label option)
      (_)
      (_: (string * JsonValue)[])
      (name: RendererName)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! config = state.GetContext()

        let! c =
          config.Custom
          |> Seq.tryFind (fun c -> c.Value.SupportedRenderers |> Set.contains name)
          |> Sum.fromOption (fun () -> $"Error: cannot parse custom renderer {name}" |> Errors.Singleton)
          |> state.OfSum

        return!
          state {
            let! (formsState: ParsedFormsContext<'ExprExtension, 'ValueExtension>) = state.GetState()
            let! t = formsState.TryFindType c.Key |> state.OfSum

            return
              PrimitiveRenderer
                { PrimitiveRendererName = name
                  PrimitiveRendererId = Guid.CreateVersion7()
                  Label = label
                  Type = t.Type }
          }
          |> state.MapError(Errors.WithPriority ErrorPriority.High)
      }

    static member private ParseGenericRenderer
      (label: Label option)
      (name: RendererName)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
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
                if g.SupportedRenderers |> Set.contains name then
                  return
                    PrimitiveRenderer
                      { PrimitiveRendererName = name
                        PrimitiveRendererId = Guid.CreateVersion7()
                        Label = label
                        Type = g.Type }

                else
                  return! state.Throw(Errors.Singleton $"Error: generic renderer {name} does not match")
              })
            |> state.Any
      }

    static member private ParseTupleRenderer
      (primitivesExt: FormParserPrimitivesExtension<'ExprExtension, 'ValueExtension>)
      (exprParser: ExprParser<'ExprExtension, 'ValueExtension>)
      (label: Label option)
      (config: CodeGenConfig)
      (parentJsonFields: (string * JsonValue)[])
      (name: RendererName)
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! tupleConfig =
          config.Tuple
          |> List.tryFind (fun t -> t.SupportedRenderers.Contains name)
          |> Sum.fromOption (fun () -> Errors.Singleton $"Error: cannot find tuple config for renderer {name}")
          |> state.OfSum

        return!
          state {
            let! itemRenderersJson = parentJsonFields |> sum.TryFindField "itemRenderers" |> state.OfSum

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
                  { Label = label
                    Tuple = name
                    Elements = itemRenderers }
          }
          |> state.MapError(Errors.WithPriority ErrorPriority.High)
      }

    static member ParseRecordRenderer
      (primitivesExt: FormParserPrimitivesExtension<'ExprExtension, 'ValueExtension>)
      (exprParser: ExprParser<'ExprExtension, 'ValueExtension>)
      (fields: (string * JsonValue)[])
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! formFields = FormFields<'ExprExtension, 'ValueExtension>.Parse primitivesExt exprParser fields

        let! rendererJson =
          fields
          |> state.TryFindField "renderer"
          |> state.Catch
          |> state.Map(Sum.toOption)

        let! renderer =
          rendererJson
          |> Option.map (JsonValue.AsString >> state.OfSum >> state.Map RendererName)
          |> state.RunOption

        Renderer.RecordRenderer
          { Renderer = renderer
            Fields = formFields }
      }

    static member ParseUnionRenderer
      (primitivesExt: FormParserPrimitivesExtension<'ExprExtension, 'ValueExtension>)
      (exprParser: ExprParser<'ExprExtension, 'ValueExtension>)
      (fields: (string * JsonValue)[])
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! casesJson = fields |> state.TryFindField "cases"

        return!
          state {
            let! casesJson = casesJson |> JsonValue.AsRecord |> state.OfSum
            let! rendererJson = fields |> state.TryFindField "renderer"
            let! renderer = JsonValue.AsString rendererJson |> state.OfSum |> state.Map RendererName
            let! ctx = state.GetContext()

            let! _ =
              fields
              |> state.TryFindField "containerRenderer"
              |> state.Catch
              |> state.Map(Sum.toOption)

            if ctx.Union.SupportedRenderers |> Set.contains renderer |> not then
              return! state.Throw(Errors.Singleton $"Error: cannot find union renderer {renderer}")
            else
              let! cases =
                casesJson
                |> Seq.map (fun (caseName, caseJson) ->
                  state.Either
                    (state {
                      let! caseBody = Renderer.Parse primitivesExt exprParser [| "renderer", caseJson |]

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


              Renderer.UnionRenderer { Renderer = renderer; Cases = cases }
          }
          |> state.MapError(Errors.WithPriority ErrorPriority.High)
      }

  and FormBody<'ExprExtension, 'ValueExtension> with
    static member ParseTableRenderer
      (primitivesExt: FormParserPrimitivesExtension<'ExprExtension, 'ValueExtension>)
      (exprParser: ExprParser<'ExprExtension, 'ValueExtension>)
      (fields: (string * JsonValue)[])
      (formTypeId: ExprTypeId)
      : State<
          FormBody<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
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

            let! renderer = rendererJson |> JsonValue.AsString |> state.OfSum |> state.Map RendererName
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

              let! disabledColumnsJson =
                fields
                |> state.TryFindField "disabledColumns"
                |> state.Catch
                |> state.Map Sum.toOption
                |> state.Map(Option.defaultWith (fun () -> JsonValue.Array [||]))

              let! disabledColumns =
                FormBody.ParseGroup
                  primitivesExt
                  exprParser
                  "disabledColumns"
                  (columns |> Map.map (fun _ c -> c.FieldConfig))
                  disabledColumnsJson

              let! dataContextColumnsJson =
                fields
                |> state.TryFindField "dataContextColumns"
                |> state.Catch
                |> state.Map Sum.toOption
                |> state.Map(Option.defaultWith (fun () -> JsonValue.Array [||]))

              let! dataContextColumns = FormBody.ParseFieldList ExprType.UnitType dataContextColumnsJson

              return
                {| Columns = columns
                   RowTypeId = t.TypeId
                   Details = details
                   //  Preview = preview
                   HighlightedFilters = highlightedFilters
                   Renderer = renderer
                   MethodLabels = actionLabels
                   VisibleColumns = visibleColumns
                   DisabledColumns = disabledColumns
                   Tabs = { FormTabs = Map.empty }
                   DataContextColumns = dataContextColumns |}
                |> FormBody.Table
          }
          |> state.MapError(Errors.WithPriority ErrorPriority.High)
      }

  and FormBody<'ExprExtension, 'ValueExtension> with
    static member Parse
      (primitivesExt: FormParserPrimitivesExtension<'ExprExtension, 'ValueExtension>)
      (exprParser: ExprParser<'ExprExtension, 'ValueExtension>)
      (fields: (string * JsonValue)[])
      (formTypeId: ExprTypeId)
      : State<
          FormBody<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      let parseAnnotatedRenderer
        : State<
            FormBody<'ExprExtension, 'ValueExtension>,
            CodeGenConfig,
            ParsedFormsContext<'ExprExtension, 'ValueExtension>,
            Errors
           > =
        state {
          let! renderer = Renderer.Parse primitivesExt exprParser fields

          FormBody.Annotated
            {| Renderer = renderer
               TypeId = formTypeId |}
        }

      state.Either parseAnnotatedRenderer (FormBody.ParseTableRenderer primitivesExt exprParser fields formTypeId)
      |> state.MapError(Errors.HighestPriority)

  and Renderer<'ExprExtension, 'ValueExtension> with
    static member private ParseActionLabels(actionLabelsJson: option<JsonValue>) =
      state {
        let! (actionLabels: Option<Map<TableMethod, Label>>) =
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
                              "removeAll", TableMethod.RemoveAll
                              "duplicate", TableMethod.Duplicate
                              "actionOnAll", TableMethod.ActionOnAll
                              "move", TableMethod.Move ]
                          |> Map.tryFindWithError k "TableMethod" k
                          |> sum.MapError(Errors.Map(String.appendNewline $"...when parsing actionLabels"))
                          |> state.OfSum
                      }

                    let! label = v |> JsonValue.AsString |> state.OfSum |> state.Map Label
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
      : State<
          Renderer<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state.Either3
        (Renderer.ParseRecordRenderer primitivesExt exprParser parentJsonFields) // NOTE: the renderer field is optional
        (state {
          let! config = state.GetContext()

          let! name =
            parentJsonFields
            |> state.TryFindField "renderer"
            |> State.bind (JsonValue.AsString >> state.OfSum >> state.Map RendererName)

          let! (formsState: ParsedFormsContext<'ExprExtension, 'ValueExtension>) = state.GetState()

          let label =
            parentJsonFields
            |> sum.TryFindField "label"
            |> Sum.toOption
            |> Option.bind (JsonValue.AsString >> Sum.toOption)
            |> Option.map Label

          return!
            state.Any(
              NonEmptyList.OfList(
                Renderer.ParseBoolRenderer label parentJsonFields name,
                [ Renderer.ParseDateRenderer label parentJsonFields name
                  Renderer.ParseUnitRenderer label parentJsonFields name
                  Renderer.ParseGuidRenderer label parentJsonFields name
                  Renderer.ParseIntRenderer label parentJsonFields name
                  Renderer.ParseStringRenderer label parentJsonFields name
                  Renderer.ParseEnumRenderer label parentJsonFields name
                  Renderer.ParseStreamRenderer label parentJsonFields name
                  Renderer.ParseMapRenderer label (NestedRenderer.Parse primitivesExt exprParser) parentJsonFields name
                  Renderer.ParseSumRenderer label (NestedRenderer.Parse primitivesExt exprParser) parentJsonFields name
                  Renderer.ParseOptionRenderer
                    label
                    (NestedRenderer.Parse primitivesExt exprParser)
                    parentJsonFields
                    name
                  Renderer.ParseOneRenderer label (NestedRenderer.Parse primitivesExt exprParser) parentJsonFields name
                  Renderer.ParseManyAllRenderer
                    label
                    (NestedRenderer.Parse primitivesExt exprParser)
                    parentJsonFields
                    name
                  Renderer.ParseManyItemRenderer
                    label
                    (NestedRenderer.Parse primitivesExt exprParser)
                    parentJsonFields
                    name
                  Renderer.ParseReadOnlyRenderer
                    label
                    (NestedRenderer.Parse primitivesExt exprParser)
                    parentJsonFields
                    name
                  Renderer.ParseListRenderer label (NestedRenderer.Parse primitivesExt exprParser) parentJsonFields name
                  Renderer.ParseCustomRenderer
                    label
                    (NestedRenderer.Parse primitivesExt exprParser)
                    parentJsonFields
                    name
                  Renderer.ParseTupleRenderer primitivesExt exprParser label config parentJsonFields name
                  Renderer.ParseUnionRenderer primitivesExt exprParser parentJsonFields
                  Renderer.ParseAllTranslationOverridesRenderer parentJsonFields name
                  state.Any(
                    NonEmptyList.OfList(
                      Renderer.ParseGenericRenderer label name,
                      [ state {
                          let! formName =
                            parentJsonFields
                            |> state.TryFindField "renderer"
                            |> State.bind (JsonValue.AsString >> state.OfSum >> state.Map FormName)

                          let! form = formsState.TryFindForm formName |> state.OfSum

                          return!
                            state {
                              match form.Body with
                              | FormBody.Annotated fields ->
                                FormRenderer(form |> FormConfig<'ExprExtension, 'ValueExtension>.Id, fields.TypeId)
                              | FormBody.Table _ ->
                                let! tableApiNameJson = parentJsonFields |> sum.TryFindField "api" |> state.OfSum
                                let! tableApiName = tableApiNameJson |> JsonValue.AsString |> state.OfSum
                                let! tableApi = formsState.TryFindTableApi tableApiName |> state.OfSum

                                let! tableType = formsState.TryFindType (fst tableApi).TypeId.VarName |> state.OfSum

                                TableFormRenderer(
                                  form |> FormConfig<'ExprExtension, 'ValueExtension>.Id,
                                  tableType.Type |> ExprType.TableType,
                                  tableApi |> fst |> TableApi.Id
                                )
                            }
                            |> state.MapError(Errors.WithPriority ErrorPriority.High)
                        }
                        state.Throw(
                          Errors.Singleton $"Error: cannot resolve field renderer {name}."
                          |> Errors.WithPriority ErrorPriority.High
                        ) ]
                    )
                  ) ]
              )
            )
            |> state.MapError(Errors.HighestPriority)
        })
        (state {
          let! fields =
            parentJsonFields
            |> state.TryFindField "renderer"
            |> State.bind (JsonValue.AsRecord >> state.OfSum)


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

              return Renderer.InlineFormRenderer formBody
            }
            |> state.MapError(Errors.WithPriority ErrorPriority.High)
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
          |> Option.map (JsonValue.AsString >> state.OfSum >> state.Map Label)
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

        let! renderer = Renderer.Parse primitivesExt exprParser jsonFields

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
          |> Option.map (JsonValue.AsString >> state.OfSum >> state.Map Label)
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

        let! renderer = Renderer.Parse primitivesExt exprParser fields

        match renderer with
        // FIXME: This replicas a shortcoming of the frontend parser, can be removed once fixed
        | Renderer.RecordRenderer _ ->
          return!
            state.Throw(Errors.Singleton """For record fields, record renderers must be wrapped in {"renderer": ...}""")
        | _ ->

          let fc =
            { FieldName = fieldName
              FieldId = Guid.CreateVersion7()
              Label = label
              Tooltip = tooltip
              Details = details
              Renderer = renderer }

          fc
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
                        let! extendsFormName = extendJson |> JsonValue.AsString |> state.OfSum |> state.Map FormName
                        return! state.TryFindForm extendsFormName
                      })
                    |> state.All
                })
              |> state.RunOption

            let! extendedFields =
              match extendedForms with
              | None -> state.Return []
              | Some fs -> fs |> Seq.map (fun f -> FormBody.TryGetFields f.Body) |> state.All

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

            let! dataContextFieldsJson =
              fields
              |> state.TryFindField "dataContextFields"
              |> state.Catch
              |> state.Map Sum.toOption
              |> state.Map(Option.defaultWith (fun () -> JsonValue.Array [||]))

            // TODO: use correct type
            let! dataContextFields = FormBody.ParseFieldList ExprType.UnitType dataContextFieldsJson

            let! tabs =
              FormBody<'ExprExtension, 'ValueExtension>.ParseTabs primitivesExt exprParser fieldConfigs tabsJson

            return
              { FormFields.Fields = fieldConfigs
                FormFields.Disabled = disabledFields
                FormFields.DataContextFields = dataContextFields
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

    static member ParseFieldList
      (_: ExprType)
      (json: JsonValue)
      : State<
          FormGroup<'ExprExtension, 'ValueExtension>,
          CodeGenConfig,
          ParsedFormsContext<'ExprExtension, 'ValueExtension>,
          Errors
         >
      =
      state {
        let! fields = json |> JsonValue.AsArray |> state.OfSum


        return!
          fields
          |> Seq.map (
            JsonValue.AsString
            >> state.OfSum
            >> state.Map(fun fieldName ->
              { FieldName = fieldName
                FieldId = Guid.CreateVersion7() })
          )
          |> state.All
          |> state.Map FormGroup.Inlined

      // TODO: implement type-checking
      // match t with
      // | ExprType.RecordType typeFields ->
      //   return!
      //     seq {
      //       for fieldNameJson in fields do
      //         yield
      //           state {
      //             let! fieldName = fieldNameJson |> JsonValue.AsString |> state.OfSum

      //             return!
      //               typeFields
      //               |> Map.tryFindWithError fieldName "field name" fieldName
      //               |> Sum.map (fun _ -> fieldName)
      //               |> state.OfSum
      //           }
      //     }
      //     |> state.All
      // | _ -> return! state.Throw(Errors.Singleton $"Expected record type, got {t}")
      }

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

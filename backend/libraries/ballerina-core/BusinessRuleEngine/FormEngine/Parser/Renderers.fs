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
  open Ballerina.Core.Json
  open Ballerina.Core.String
  open FSharp.Data
  open Ballerina.Collections.NonEmptyList

  type Renderer with
    static member ParseBoolRenderer
      (_: (string * JsonValue)[])
      (json: JsonValue)
      : State<Renderer, CodeGenConfig, ParsedFormsContext, Errors> =
      state {
        let! config = state.GetContext()
        let! s = json |> JsonValue.AsString |> state.OfSum

        if config.Bool.SupportedRenderers |> Set.contains s then
          return
            PrimitiveRenderer
              { PrimitiveRendererName = s
                PrimitiveRendererId = Guid.CreateVersion7()
                Type = ExprType.PrimitiveType PrimitiveType.BoolType }
        else
          return!
            state.Throw(Errors.Singleton $"Error: cannot parse bool renderer from {json.ToString().ReasonablyClamped}")
      }

  type Renderer with
    static member ParseDateRenderer
      (_: (string * JsonValue)[])
      (json: JsonValue)
      : State<Renderer, CodeGenConfig, ParsedFormsContext, Errors> =
      state {
        let! config = state.GetContext()
        let! s = json |> JsonValue.AsString |> state.OfSum

        if config.Date.SupportedRenderers |> Set.contains s then
          return
            PrimitiveRenderer
              { PrimitiveRendererName = s
                PrimitiveRendererId = Guid.CreateVersion7()
                Type = ExprType.PrimitiveType PrimitiveType.DateOnlyType }
        else
          return!
            state.Throw(Errors.Singleton $"Error: cannot parse date renderer from {json.ToString().ReasonablyClamped}")
      }


  type Renderer with
    static member ParseUnitRenderer
      (_: (string * JsonValue)[])
      (json: JsonValue)
      : State<Renderer, CodeGenConfig, ParsedFormsContext, Errors> =
      state {
        let! config = state.GetContext()
        let! s = json |> JsonValue.AsString |> state.OfSum

        if config.Unit.SupportedRenderers |> Set.contains s then
          return
            PrimitiveRenderer
              { PrimitiveRendererName = s
                PrimitiveRendererId = Guid.CreateVersion7()
                Type = ExprType.UnitType }
        else
          return!
            state.Throw(Errors.Singleton $"Error: cannot parse unit renderer from {json.ToString().ReasonablyClamped}")
      }

  type Renderer with
    static member ParseGuidRenderer
      (_: (string * JsonValue)[])
      (json: JsonValue)
      : State<Renderer, CodeGenConfig, ParsedFormsContext, Errors> =
      state {
        let! config = state.GetContext()
        let! s = json |> JsonValue.AsString |> state.OfSum

        if config.Guid.SupportedRenderers |> Set.contains s then
          return
            PrimitiveRenderer
              { PrimitiveRendererName = s
                PrimitiveRendererId = Guid.CreateVersion7()
                Type = ExprType.PrimitiveType PrimitiveType.GuidType }
        else
          return!
            state.Throw(Errors.Singleton $"Error: cannot parse guid renderer from {json.ToString().ReasonablyClamped}")
      }

  type Renderer with
    static member ParseIntRenderer
      (_: (string * JsonValue)[])
      (json: JsonValue)
      : State<Renderer, CodeGenConfig, ParsedFormsContext, Errors> =
      state {
        let! config = state.GetContext()
        let! s = json |> JsonValue.AsString |> state.OfSum

        if config.Int.SupportedRenderers |> Set.contains s then
          return
            PrimitiveRenderer
              { PrimitiveRendererName = s
                PrimitiveRendererId = Guid.CreateVersion7()
                Type = ExprType.PrimitiveType PrimitiveType.IntType }
        else
          return!
            state.Throw(Errors.Singleton $"Error: cannot parse int renderer from {json.ToString().ReasonablyClamped}")
      }

  type Renderer with
    static member ParseStringRenderer
      (_: (string * JsonValue)[])
      (json: JsonValue)
      : State<Renderer, CodeGenConfig, ParsedFormsContext, Errors> =
      state {
        let! config = state.GetContext()
        let! s = json |> JsonValue.AsString |> state.OfSum

        if config.String.SupportedRenderers |> Set.contains s then
          return
            PrimitiveRenderer
              { PrimitiveRendererName = s
                PrimitiveRendererId = Guid.CreateVersion7()
                Type = ExprType.PrimitiveType PrimitiveType.StringType }
        else
          return!
            state.Throw(
              Errors.Singleton $"Error: cannot parse string renderer from {json.ToString().ReasonablyClamped}"
            )
      }

  type Renderer with
    static member ParseEnumRenderer
      (parentJsonFields: (string * JsonValue)[])
      (json: JsonValue)
      : State<Renderer, CodeGenConfig, ParsedFormsContext, Errors> =
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

              let! (formsState: ParsedFormsContext) = state.GetState()
              let! optionJson = parentJsonFields |> sum.TryFindField "options" |> state.OfSum
              let! enumName = optionJson |> JsonValue.AsString |> state.OfSum
              let! enum = formsState.TryFindEnum enumName |> state.OfSum
              let! enumType = formsState.TryFindType enum.TypeId.TypeName |> state.OfSum

              return
                EnumRenderer(
                  enum |> EnumApi.Id,
                  PrimitiveRenderer
                    { PrimitiveRendererName = s
                      PrimitiveRendererId = Guid.CreateVersion7()
                      Type = containerTypeConstructor (enumType.Type) }
                )
            }
            |> state.MapError(Errors.WithPriority ErrorPriority.High)
        else
          return!
            state.Throw(Errors.Singleton $"Error: cannot parse enum renderer from {json.ToString().ReasonablyClamped}")
      }

  type Renderer with
    static member ParseStreamRenderer
      (parentJsonFields: (string * JsonValue)[])
      (json: JsonValue)
      : State<Renderer, CodeGenConfig, ParsedFormsContext, Errors> =
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
              let! (formsState: ParsedFormsContext) = state.GetState()

              let! stream, streamType =
                (state.Either
                  (state {
                    let! streamName = streamNameJson |> JsonValue.AsString |> state.OfSum

                    return!
                      state {
                        let! stream = formsState.TryFindStream streamName |> state.OfSum
                        let! streamType = formsState.TryFindType stream.TypeId.TypeName |> state.OfSum
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
                        let! streamType = formsState.TryFindType stream.TypeId.TypeName |> state.OfSum
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

  type Renderer with
    static member ParseMapRenderer
      (parseNestedRenderer)
      (parentJsonFields: (string * JsonValue)[])
      (json: JsonValue)
      : State<Renderer, CodeGenConfig, ParsedFormsContext, Errors> =
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
                  {| Map =
                      PrimitiveRenderer
                        { PrimitiveRendererName = s
                          PrimitiveRendererId = Guid.CreateVersion7()
                          Type = ExprType.MapType(keyRenderer.Renderer.Type, valueRenderer.Renderer.Type) }

                     Key = keyRenderer
                     Value = valueRenderer |}
            }
            |> state.MapError(Errors.WithPriority ErrorPriority.High)
        else
          return!
            state.Throw(Errors.Singleton $"Error: cannot parse map renderer from {json.ToString().ReasonablyClamped}")
      }

  type Renderer with
    static member ParseSumRenderer
      (parseNestedRenderer)
      (parentJsonFields: (string * JsonValue)[])
      (json: JsonValue)
      : State<Renderer, CodeGenConfig, ParsedFormsContext, Errors> =
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
                          Type = ExprType.SumType(leftRenderer.Renderer.Type, rightRenderer.Renderer.Type) }

                     Left = leftRenderer
                     Right = rightRenderer |}
            }
            |> state.MapError(Errors.WithPriority ErrorPriority.High)
        else
          return!
            state.Throw(Errors.Singleton $"Error: cannot parse sum renderer from {json.ToString().ReasonablyClamped}")
      }

  type Renderer with
    static member ParseOptionRenderer
      (parseNestedRenderer)
      (parentJsonFields: (string * JsonValue)[])
      (json: JsonValue)
      : State<Renderer, CodeGenConfig, ParsedFormsContext, Errors> =
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
                  {| Option =
                      PrimitiveRenderer
                        { PrimitiveRendererName = s
                          PrimitiveRendererId = Guid.CreateVersion7()
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

  type Renderer with
    static member ParseOneRenderer
      (parseNestedRenderer)
      (parentJsonFields: (string * JsonValue)[])
      (json: JsonValue)
      : State<Renderer, CodeGenConfig, ParsedFormsContext, Errors> =
      state {
        let! config = state.GetContext()
        let! s = json |> JsonValue.AsString |> state.OfSum

        if config.One.SupportedRenderers |> Set.contains s then

          return!
            state {
              let! (formsState: ParsedFormsContext) = state.GetState()
              let! detailsJson = parentJsonFields |> state.TryFindField "detailsRenderer"

              let! previewJson =
                parentJsonFields
                |> state.TryFindField "previewRenderer"
                |> state.Catch
                |> state.Map(Sum.toOption)

              let! (details: NestedRenderer) = parseNestedRenderer detailsJson

              let! preview =
                previewJson
                |> Option.map (fun previewJson -> state { return! parseNestedRenderer previewJson })
                |> state.RunOption

              let! apiRendererJson = parentJsonFields |> sum.TryFindField "api" |> state.OfSum

              let! oneApiId =
                state.Either
                  (state {
                    let! entityApiNameJson = apiRendererJson |> JsonValue.AsString |> state.OfSum

                    let! entityApi = formsState.TryFindTableApi entityApiNameJson |> state.OfSum
                    return Choice1Of2(entityApi |> TableApi.Id)
                  })
                  (state {
                    let! (apiSourceTypeNameJson, oneApiNameJson) = apiRendererJson |> JsonValue.AsPair |> state.OfSum

                    let! (apiSourceTypeName, oneApiName) =
                      state.All2
                        (apiSourceTypeNameJson |> JsonValue.AsString |> state.OfSum)
                        (oneApiNameJson |> JsonValue.AsString |> state.OfSum)

                    let! apiType = formsState.TryFindType apiSourceTypeName |> state.OfSum
                    let! (oneApi, _) = formsState.TryFindOne apiType.TypeId.TypeName oneApiName |> state.OfSum
                    return Choice2Of2(apiType.TypeId, oneApi.EntityName)
                  })

              return
                OneRenderer
                  {| One =
                      PrimitiveRenderer
                        { PrimitiveRendererName = s
                          PrimitiveRendererId = Guid.CreateVersion7()
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

  type Renderer with
    static member ParseManyRenderer
      (parseNestedRenderer)
      (parentJsonFields: (string * JsonValue)[])
      (json: JsonValue)
      : State<Renderer, CodeGenConfig, ParsedFormsContext, Errors> =
      state {
        let! config = state.GetContext()
        let! s = json |> JsonValue.AsString |> state.OfSum

        if config.Many.SupportedRenderers |> Set.contains s then

          return!
            state {
              let! (formsState: ParsedFormsContext) = state.GetState()
              let! detailsJson = parentJsonFields |> state.TryFindField "detailsRenderer"

              let! previewJson =
                parentJsonFields
                |> state.TryFindField "previewRenderer"
                |> state.Catch
                |> state.Map(Sum.toOption)

              let! (details: NestedRenderer) = parseNestedRenderer detailsJson

              let! preview =
                previewJson
                |> Option.map (fun previewJson -> state { return! parseNestedRenderer previewJson })
                |> state.RunOption

              let! apiRendererJson = parentJsonFields |> sum.TryFindField "api" |> state.OfSum
              let! (apiSourceTypeNameJson, manyApiNameJson) = apiRendererJson |> JsonValue.AsPair |> state.OfSum

              let! (apiSourceTypeName, manyApiName) =
                state.All2
                  (apiSourceTypeNameJson |> JsonValue.AsString |> state.OfSum)
                  (manyApiNameJson |> JsonValue.AsString |> state.OfSum)

              let! apiType = formsState.TryFindType apiSourceTypeName |> state.OfSum
              let! (manyApi, _) = formsState.TryFindMany apiType.TypeId.TypeName manyApiName |> state.OfSum

              return
                ManyRenderer
                  {| Many =
                      PrimitiveRenderer
                        { PrimitiveRendererName = s
                          PrimitiveRendererId = Guid.CreateVersion7()
                          Type = ExprType.ManyType details.Type }

                     ManyApiId = apiType.TypeId, manyApi.TableName
                     Details = details
                     Preview = preview |}
            }
            |> state.MapError(Errors.WithPriority ErrorPriority.High)
        else
          return!
            state.Throw(Errors.Singleton $"Error: cannot parse many renderer from {json.ToString().ReasonablyClamped}")
      }

  type Renderer with
    static member ParseListRenderer
      (parseNestedRenderer)
      (parentJsonFields: (string * JsonValue)[])
      (json: JsonValue)
      : State<Renderer, CodeGenConfig, ParsedFormsContext, Errors> =
      state {
        let! config = state.GetContext()
        let! s = json |> JsonValue.AsString |> state.OfSum

        if config.List.SupportedRenderers |> Set.contains s then

          return!
            state {
              let! (_: ParsedFormsContext) = state.GetState()
              let! elementRendererJson = parentJsonFields |> sum.TryFindField "elementRenderer" |> state.OfSum
              let! elementRenderer = parseNestedRenderer elementRendererJson

              return
                ListRenderer
                  {| List =
                      PrimitiveRenderer
                        { PrimitiveRendererName = s
                          PrimitiveRendererId = Guid.CreateVersion7()
                          Type = ExprType.ListType elementRenderer.Renderer.Type }
                     Element = elementRenderer |}
            }
            |> state.MapError(Errors.WithPriority ErrorPriority.High)
        else
          return!
            state.Throw(Errors.Singleton $"Error: cannot parse list renderer from {json.ToString().ReasonablyClamped}")
      }

  type Renderer with
    static member ParseCustomRenderer
      (_)
      (_: (string * JsonValue)[])
      (json: JsonValue)
      : State<Renderer, CodeGenConfig, ParsedFormsContext, Errors> =
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
            let! (formsState: ParsedFormsContext) = state.GetState()
            let! t = formsState.TryFindType c.Key |> state.OfSum

            return
              PrimitiveRenderer
                { PrimitiveRendererName = s
                  PrimitiveRendererId = Guid.CreateVersion7()
                  Type = t.Type }
          }
          |> state.MapError(Errors.WithPriority ErrorPriority.High)
      }

  type Renderer with
    static member Parse
      (parentJsonFields: (string * JsonValue)[])
      (json: JsonValue)
      : State<Renderer, CodeGenConfig, ParsedFormsContext, Errors> =
      state.Either
        (state {
          let! config = state.GetContext()
          let! s = json |> JsonValue.AsString |> state.OfSum
          let! (formsState: ParsedFormsContext) = state.GetState()

          return!
            state.Any(
              NonEmptyList.OfList(
                Renderer.ParseBoolRenderer parentJsonFields json,
                [ Renderer.ParseDateRenderer parentJsonFields json
                  Renderer.ParseUnitRenderer parentJsonFields json
                  Renderer.ParseGuidRenderer parentJsonFields json
                  Renderer.ParseIntRenderer parentJsonFields json
                  Renderer.ParseStringRenderer parentJsonFields json
                  Renderer.ParseEnumRenderer parentJsonFields json
                  Renderer.ParseStreamRenderer parentJsonFields json
                  Renderer.ParseMapRenderer NestedRenderer.Parse parentJsonFields json
                  Renderer.ParseSumRenderer NestedRenderer.Parse parentJsonFields json
                  Renderer.ParseOptionRenderer NestedRenderer.Parse parentJsonFields json
                  Renderer.ParseOneRenderer NestedRenderer.Parse parentJsonFields json
                  Renderer.ParseManyRenderer NestedRenderer.Parse parentJsonFields json
                  Renderer.ParseListRenderer NestedRenderer.Parse parentJsonFields json
                  Renderer.ParseCustomRenderer NestedRenderer.Parse parentJsonFields json

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

                                  let! itemRenderers = itemRenderersJson |> Seq.map (NestedRenderer.Parse) |> state.All

                                  if itemRenderers.Length <> tupleConfig.Ariety then
                                    return!
                                      state.Throw(
                                        Errors.Singleton
                                          $"Error: mismatched tuple size. Expected {tupleConfig.Ariety}, found {itemRenderers.Length}."
                                      )
                                  else
                                    return
                                      TupleRenderer
                                        {| Tuple =
                                            PrimitiveRenderer
                                              { PrimitiveRendererName = s
                                                PrimitiveRendererId = Guid.CreateVersion7()
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
                                    return FormRenderer(form |> FormConfig.Id, formType)
                                  | FormBody.Record fields ->
                                    return FormRenderer(form |> FormConfig.Id, fields.RecordType)
                                  | FormBody.Table _ ->
                                    let! tableApiNameJson = parentJsonFields |> sum.TryFindField "api" |> state.OfSum
                                    let! tableApiName = tableApiNameJson |> JsonValue.AsString |> state.OfSum
                                    let! tableApi = formsState.TryFindTableApi tableApiName |> state.OfSum

                                    let! tableType = formsState.TryFindType tableApi.TypeId.TypeName |> state.OfSum

                                    return
                                      TableFormRenderer(
                                        form |> FormConfig.Id,
                                        tableType.Type |> ExprType.TableType,
                                        tableApi |> TableApi.Id
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
          let! (_: ParsedFormsContext) = state.GetState()

          let! fields = json |> JsonValue.AsRecord |> state.OfSum

          return!
            state.Either
              (state {

                let! typeJson = (fields |> state.TryFindField "type")

                return!
                  state {
                    let! typeName = typeJson |> JsonValue.AsString |> state.OfSum
                    let! (s: ParsedFormsContext) = state.GetState()
                    let! typeBinding = s.TryFindType typeName |> state.OfSum
                    let! formBody = FormBody.Parse fields typeBinding.TypeId
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
                      let! parsedField = NestedRenderer.Parse fieldJson
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
                         Rest = gs |> Map.ofList |}
              })


        })
      |> state.MapError(Errors.HighestPriority)
  // |> state.WithErrorContext $"...when parsing renderer {json.ToString().ReasonablyClamped}"

  and NestedRenderer with
    static member Parse(json: JsonValue) : State<NestedRenderer, CodeGenConfig, ParsedFormsContext, Errors> =
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
        let! renderer = Renderer.Parse jsonFields rendererJson

        return
          { Label = label
            Tooltip = tooltip
            Details = details
            Renderer = renderer }
      }
      |> state.WithErrorContext $"...when parsing (nested) renderer {json.ToString().ReasonablyClamped}"

  and FieldConfig with
    static member Parse
      (fieldName: string)
      (json: JsonValue)
      : State<FieldConfig, CodeGenConfig, ParsedFormsContext, Errors> =
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
        let! renderer = Renderer.Parse fields rendererJson

        let! visible =
          visibleJson
          |> Sum.toOption
          |> Option.map (Expr.Parse >> state.OfSum)
          |> state.RunOption

        let visible =
          visible |> Option.defaultWith (fun () -> Expr.Value(Value.ConstBool true))

        let! disabled = disabledJson |> Option.map (Expr.Parse >> state.OfSum) |> state.RunOption

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

  and FormFields with
    static member Parse(fields: (string * JsonValue)[]) =
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
                  let! parsedField = FieldConfig.Parse fieldName fieldJson
                  return fieldName, parsedField
                })
              |> state.All

            let fieldConfigs = fieldConfigs |> Map.ofSeq
            let fieldConfigs = Map.mergeMany (fun x _ -> x) (fieldConfigs :: extendedFields)
            let! tabs = FormBody.ParseTabs fieldConfigs tabsJson

            return
              { FormFields.Fields = fieldConfigs
                FormFields.Tabs = tabs }
          }
          |> state.MapError(Errors.WithPriority ErrorPriority.High)
      }

  and FormBody with
    static member Parse (fields: (string * JsonValue)[]) (formTypeId: TypeId) =
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
                let! t = state.TryFindType formTypeId.TypeName

                let! cases =
                  casesJson
                  |> Seq.map (fun (caseName, caseJson) ->
                    state.Either
                      (state {
                        let! caseBody = Renderer.Parse [||] caseJson

                        return
                          caseName,
                          { Label = None
                            Tooltip = None
                            Details = None
                            Renderer = caseBody }
                      })
                      (state {
                        let! caseBody = NestedRenderer.Parse caseJson

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
                |> state.Map(Sum.toOption)

              // let! previewJson =
              //   fields
              //   |> state.TryFindField "previewRenderer"
              //   |> state.Catch
              //   |> state.Map(Sum.toOption)

              let! renderer = rendererJson |> JsonValue.AsString |> state.OfSum
              let! config = state.GetContext()
              let! t = state.TryFindType formTypeId.TypeName

              let! details =
                detailsJson
                |> Option.map (fun detailsJson -> state { return! NestedRenderer.Parse detailsJson })
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
                      let! columnFields = columnJson |> JsonValue.AsRecord |> state.OfSum
                      let! columnBody = FieldConfig.Parse columnName columnJson

                      let! isFilterable =
                        state.Either
                          (columnFields |> sum.TryFindField "isFilterable" |> state.OfSum)
                          (JsonValue.Boolean true |> state.Return)

                      let! isSortable =
                        state.Either
                          (columnFields |> sum.TryFindField "isSortable" |> state.OfSum)
                          (JsonValue.Boolean true |> state.Return)

                      let! isFilterable, isSortable =
                        state.All2
                          (isFilterable |> JsonValue.AsBoolean |> state.OfSum)
                          (isSortable |> JsonValue.AsBoolean |> state.OfSum)

                      return
                        columnName,
                        { FieldConfig = columnBody
                          IsFilterable = isFilterable
                          IsSortable = isSortable }
                    }
                    |> state.MapError(Errors.Map(String.appendNewline $"\n...when parsing table column {columnName}")))
                  |> state.All
                  |> state.Map(Map.ofSeq)

                let! visibleColumnsJson = fields |> state.TryFindField "visibleColumns"

                let! visibleColumns =
                  FormBody.ParseGroup
                    "visibleColumns"
                    (columns |> Map.map (fun _ c -> c.FieldConfig))
                    visibleColumnsJson

                return
                  {| Columns = columns
                     RowType = t.Type
                     Details = details
                     //  Preview = preview
                     Renderer = renderer
                     VisibleColumns = visibleColumns |}
                  |> FormBody.Table
            }
            |> state.MapError(Errors.WithPriority ErrorPriority.High)
        })
        (state {
          let! formFields = FormFields.Parse fields
          let! t = state.TryFindType formTypeId.TypeName

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

    static member ParseGroup
      (groupName: string)
      (fieldConfigs: Map<string, FieldConfig>)
      (json: JsonValue)
      : State<FormGroup, CodeGenConfig, ParsedFormsContext, Errors> =
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
          let! expr = json |> Expr.Parse |> state.OfSum
          return FormGroup.Computed expr
        })
      |> state.WithErrorContext $"...when parsing group {groupName}"

    static member ParseColumn
      (columnName: string)
      fieldConfigs
      (json: JsonValue)
      : State<FormGroups, CodeGenConfig, ParsedFormsContext, Errors> =
      state {
        let! jsonFields = json |> JsonValue.AsRecord |> state.OfSum

        match jsonFields with
        | [| "groups", JsonValue.Record groups |] ->
          let! groups =
            seq {
              for groupName, groupJson in groups do
                yield
                  state {
                    let! column = FormBody.ParseGroup groupName fieldConfigs groupJson
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
      (tabName: string)
      fieldConfigs
      (json: JsonValue)
      : State<FormColumns, CodeGenConfig, ParsedFormsContext, Errors> =
      state {
        let! jsonFields = json |> JsonValue.AsRecord |> state.OfSum

        match jsonFields with
        | [| "columns", JsonValue.Record columns |] ->
          let! columns =
            seq {
              for columnName, columnJson in columns do
                yield
                  state {
                    let! column = FormBody.ParseColumn columnName fieldConfigs columnJson
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

    static member ParseTabs
      fieldConfigs
      (json: JsonValue)
      : State<FormTabs, CodeGenConfig, ParsedFormsContext, Errors> =
      state {
        let! tabs = json |> JsonValue.AsRecord |> state.OfSum

        let! tabs =
          seq {
            for tabName, tabJson in tabs do
              yield
                state {
                  let! column = FormBody.ParseTab tabName fieldConfigs tabJson
                  return tabName, column
                }
          }
          |> state.All
          |> state.Map Map.ofList

        return { FormTabs = tabs }
      }
      |> state.WithErrorContext $"...when parsing tabs"

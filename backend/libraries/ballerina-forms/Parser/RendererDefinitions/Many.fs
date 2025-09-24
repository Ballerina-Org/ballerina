namespace Ballerina.DSL.FormEngine.Parser.RendererDefinitions

module Many =
  open Ballerina.DSL.Parser.Patterns

  open Ballerina.DSL.FormEngine.Model
  open Ballerina.DSL.FormEngine.Parser.FormsPatterns
  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Types.Model
  open System
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Errors
  open Ballerina.StdLib.Json.Patterns
  open Ballerina.StdLib.String
  open Ballerina.StdLib.Object
  open FSharp.Data

  [<Literal>]
  let ItemRendererKeyword = "itemRenderer"

  [<Literal>]
  let LinkedRendererKeyword = "linkedRenderer"

  [<Literal>]
  let UnlinkedRendererKeyword = "unlinkedRenderer"

  [<Literal>]
  let apiKeyword = "api"

  type Renderer<'ExprExtension, 'ValueExtension> with

    static member private ParseManyApi
      (parentJsonFields: (string * JsonValue)[])
      : State<_, CodeGenConfig, ParsedFormsContext<'ExprExtension, 'ValueExtension>, Errors> =
      state {
        let! apiRendererJson = parentJsonFields |> sum.TryFindField apiKeyword |> state.OfSum |> state.Catch
        let apiRendererJson = apiRendererJson |> Sum.toOption

        return!
          apiRendererJson
          |> Option.map (fun apiRendererJson ->
            state {
              let! apiSourceTypeNameJson, manyApiNameJson = apiRendererJson |> JsonValue.AsPair |> state.OfSum

              let! apiSourceTypeName, manyApiName =
                state.All2
                  (apiSourceTypeNameJson |> JsonValue.AsString |> state.OfSum)
                  (manyApiNameJson |> JsonValue.AsString |> state.OfSum)

              let! (formsState: ParsedFormsContext<'ExprExtension, 'ValueExtension>) = state.GetState()
              let! apiType = formsState.TryFindType apiSourceTypeName |> state.OfSum
              let! manyApi, _ = formsState.TryFindMany apiType.TypeId.VarName manyApiName |> state.OfSum
              return apiType.TypeId, manyApi.TableName
            })
          |> state.RunOption
      }

    static member ParseManyAllRenderer
      (label: Label option)
      parseNestedRenderer
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

        if config.Many.SupportedRenderers.AllRenderers |> Set.contains name then

          return!
            state {
              let! itemRendererJson = parentJsonFields |> state.TryFindField ItemRendererKeyword

              let! (itemRenderer: NestedRenderer<'ExprExtension, 'ValueExtension>) =
                parseNestedRenderer itemRendererJson

              let! manyApi = Renderer.ParseManyApi parentJsonFields

              return
                ManyAllRenderer
                  {| Label = label
                     Many = name
                     Element = itemRenderer
                     ManyApiId = manyApi |}
                |> ManyRenderer
            }

            |> state.MapError(Errors.WithPriority ErrorPriority.High)
        else
          return! state.Throw(Errors.Singleton $"Error: cannot parse many renderer from {name}")
      }

    static member ParseManyItemRenderer
      (label: Label option)
      parseNestedRenderer
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

        if config.Many.SupportedRenderers.LinkedUnlinkedRenderers |> Set.contains name then

          return!
            state {
              let! linkedRendererJson = parentJsonFields |> state.TryFindField LinkedRendererKeyword

              let! (linkedRenderer: NestedRenderer<'ExprExtension, 'ValueExtension>) =
                parseNestedRenderer linkedRendererJson

              let! unlinkedRendererJson = parentJsonFields |> state.TryFindField UnlinkedRendererKeyword |> state.Catch
              let unlinkedRendererJson = unlinkedRendererJson |> Sum.toOption

              let! unlinkedRenderer = unlinkedRendererJson |> Option.map parseNestedRenderer |> state.RunOption

              let! manyApi = Renderer.ParseManyApi parentJsonFields

              return
                ManyLinkedUnlinkedRenderer
                  {| Label = label
                     Many = name
                     Linked = linkedRenderer
                     Unlinked = unlinkedRenderer
                     ManyApiId = manyApi |}
                |> ManyRenderer
            }

            |> state.MapError(Errors.WithPriority ErrorPriority.High)
        else
          return! state.Throw(Errors.Singleton $"Error: cannot parse many renderer from {name}")
      }

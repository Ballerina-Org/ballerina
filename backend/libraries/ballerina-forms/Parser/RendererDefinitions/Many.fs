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

    static member ParseManyAllRenderer
      parseNestedRenderer
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

        if config.Many.SupportedRenderers.AllRenderers |> Set.contains s then

          return!
            state {
              let! itemRendererJson = parentJsonFields |> state.TryFindField ItemRendererKeyword

              let! (itemRenderer: NestedRenderer<'ExprExtension, 'ValueExtension>) =
                parseNestedRenderer itemRendererJson

              // TODO: Do we need api here?
              // let! (formsState: ParsedFormsContext<'ExprExtension, 'ValueExtension>) = state.GetState()
              // let! apiRendererJson = parentJsonFields |> sum.TryFindField apiKeyword |> state.OfSum
              // let! apiSourceTypeNameJson, manyApiNameJson = apiRendererJson |> JsonValue.AsPair |> state.OfSum
              // let! apiSourceTypeName, manyApiName =
              //   state.All2
              //     (apiSourceTypeNameJson |> JsonValue.AsString |> state.OfSum)
              //     (manyApiNameJson |> JsonValue.AsString |> state.OfSum)
              // let! apiType = formsState.TryFindType apiSourceTypeName |> state.OfSum
              // let! manyApi, _ = formsState.TryFindMany apiType.TypeId.VarName manyApiName |> state.OfSum

              return
                ManyAllRenderer
                  {| Many =
                      PrimitiveRenderer
                        { PrimitiveRendererName = s
                          PrimitiveRendererId = Guid.CreateVersion7()
                          Type = ManyType itemRenderer.Type }
                     Element = itemRenderer |}
                |> ManyRenderer
            }

            |> state.MapError(Errors.WithPriority ErrorPriority.High)
        else
          return!
            state.Throw(Errors.Singleton $"Error: cannot parse many renderer from {json.ToString().ReasonablyClamped}")
      }

    static member ParseManyItemRenderer
      parseNestedRenderer
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

        if config.Many.SupportedRenderers.LinkedUnlinkedRenderers |> Set.contains s then

          return!
            state {
              let! linkedRendererJson = parentJsonFields |> state.TryFindField LinkedRendererKeyword

              let! (linkedRenderer: NestedRenderer<'ExprExtension, 'ValueExtension>) =
                parseNestedRenderer linkedRendererJson

              let! unlinkedRendererJson = parentJsonFields |> state.TryFindField UnlinkedRendererKeyword

              let! (unlinkedRenderer: NestedRenderer<'ExprExtension, 'ValueExtension>) =
                parseNestedRenderer unlinkedRendererJson

              // TODO: Do we need api here?
              // let! (formsState: ParsedFormsContext<'ExprExtension, 'ValueExtension>) = state.GetState()
              // let! apiRendererJson = parentJsonFields |> sum.TryFindField apiKeyword |> state.OfSum
              // let! apiSourceTypeNameJson, manyApiNameJson = apiRendererJson |> JsonValue.AsPair |> state.OfSum
              // let! apiSourceTypeName, manyApiName =
              //   state.All2
              //     (apiSourceTypeNameJson |> JsonValue.AsString |> state.OfSum)
              //     (manyApiNameJson |> JsonValue.AsString |> state.OfSum)
              // let! apiType = formsState.TryFindType apiSourceTypeName |> state.OfSum
              // let! manyApi, _ = formsState.TryFindMany apiType.TypeId.VarName manyApiName |> state.OfSum

              return
                ManyLinkedUnlinkedRenderer
                  {| Many =
                      PrimitiveRenderer
                        { PrimitiveRendererName = s
                          PrimitiveRendererId = Guid.CreateVersion7()
                          Type = ManyType linkedRenderer.Type }
                     Linked = linkedRenderer
                     Unlinked = unlinkedRenderer |}
                |> ManyRenderer
            }

            |> state.MapError(Errors.WithPriority ErrorPriority.High)
        else
          return!
            state.Throw(Errors.Singleton $"Error: cannot parse many renderer from {json.ToString().ReasonablyClamped}")
      }

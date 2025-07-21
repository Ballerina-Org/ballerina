namespace Ballerina.DSL.FormEngine.Parser

module ManyRenderer =
  open Ballerina.DSL.Parser.Patterns

  open Ballerina.DSL.FormEngine.Model
  open Ballerina.DSL.FormEngine.Parser.FormsPatterns
  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Types.Model
  open System
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Errors
  open Ballerina.StdLib.Json
  open Ballerina.StdLib.String
  open FSharp.Data

  type Renderer<'ExprExtension, 'ValueExtension> with
    static member ParseManyRenderer
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

        if config.Many.SupportedRenderers |> Set.contains s then

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

              let! apiRendererJson = parentJsonFields |> sum.TryFindField "api" |> state.OfSum
              let! (apiSourceTypeNameJson, manyApiNameJson) = apiRendererJson |> JsonValue.AsPair |> state.OfSum

              let! (apiSourceTypeName, manyApiName) =
                state.All2
                  (apiSourceTypeNameJson |> JsonValue.AsString |> state.OfSum)
                  (manyApiNameJson |> JsonValue.AsString |> state.OfSum)

              let! apiType = formsState.TryFindType apiSourceTypeName |> state.OfSum
              let! (manyApi, _) = formsState.TryFindMany apiType.TypeId.VarName manyApiName |> state.OfSum

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

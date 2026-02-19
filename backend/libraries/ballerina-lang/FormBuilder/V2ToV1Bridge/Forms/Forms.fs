namespace Ballerina.DSL.FormBuilder.V2ToV1Bridge.Forms

module Forms =

  open Ballerina.DSL.FormBuilder.Model.FormAST
  open Ballerina.Cat.Collections.OrderedMap
  open FSharp.Data

  let rec private generateRendererJson<'typeValue>
    (renderer: RendererExpression<'typeValue>)
    : list<string * JsonValue> =
    match renderer with
    | RendererExpression.Pinco(_) -> [ "renderer", JsonValue.String "Int32" ]
    | RendererExpression.Primitive { Primitive = RendererIdentifier id } -> [ "renderer", JsonValue.String id ]
    | RendererExpression.List { List = RendererIdentifier id
                                Element = element } ->
      [ "renderer", JsonValue.String id
        "elementRenderer", JsonValue.Record(Array.ofList (generateRendererJson element)) ]
    | RendererExpression.Sum { Sum = RendererIdentifier id
                               Left = left
                               Right = right } ->
      [ "renderer", JsonValue.String id
        "leftRenderer", JsonValue.Record(Array.ofList (generateRendererJson left))
        "rightRenderer", JsonValue.Record(Array.ofList (generateRendererJson right)) ]
    | RendererExpression.Tuple { Tuple = RendererIdentifier id
                                 Items = items } ->
      [ "renderer", JsonValue.String id
        "itemRenderers",
        JsonValue.Array(
          Array.ofList (
            items
            |> List.map (fun item -> JsonValue.Record(Array.ofList (generateRendererJson item)))
          )
        ) ]
    | RendererExpression.Union { Union = RendererIdentifier id
                                 Cases = cases } ->
      [ "renderer", JsonValue.String id
        "cases",
        JsonValue.Record(
          Array.ofSeq (
            cases
            |> Map.toSeq
            |> Seq.map (fun ((CaseIdentifier caseName), renderer) ->
              caseName, JsonValue.Record(Array.ofList (generateRendererJson renderer)))
          )
        ) ]
    | RendererExpression.Form(FormIdentifier formName, _) -> [ "renderer", JsonValue.String formName ]
    // Preferably we make no distinction between inline forms and regular forms inthe AST
    | RendererExpression.InlineForm(inlineForm) ->
      let form: Form<'typeValue> =
        { IsEntryPoint = false
          RendererId = None
          Form = FormIdentifier "anonymous form"
          // This feels brittle
          TypeIdentifier = TypeIdentifier(inlineForm.Type.ToString())
          Type = inlineForm.Type
          Body = inlineForm.Body }

      [ "renderer", generateFormJson form ]
    | RendererExpression.Record { Renderer = rendererId
                                  Members = members
                                  DisabledFields = disabledFields
                                  Type = typeValue } ->
      let form: Form<'typeValue> =
        { IsEntryPoint = false
          RendererId = rendererId
          Form = FormIdentifier "anonymous form"
          // This feels brittle
          TypeIdentifier = TypeIdentifier(typeValue.ToString())
          Type = typeValue
          Body =
            { Members = members
              DisabledFields = disabledFields
              Details = None
              Highlights = Set.empty } }

      [ "renderer", generateFormJson form ]
    | RendererExpression.Map { Map = RendererIdentifier id
                               Key = key
                               Value = value } ->
      [ "renderer", JsonValue.String id
        "keyRenderer", JsonValue.Record(Array.ofList (generateRendererJson key))
        "valueRenderer", JsonValue.Record(Array.ofList (generateRendererJson value)) ]
    | RendererExpression.Readonly { Readonly = RendererIdentifier readonly
                                    Value = value } ->
      [ "renderer", JsonValue.String readonly
        "childRenderer", JsonValue.Record(Array.ofList (generateRendererJson value)) ]
    | RendererExpression.Table _ -> failwith $"Table Renderers are not supported"
    | RendererExpression.One _ -> failwith $"One Renderers are not supported"
    | RendererExpression.Many _ -> failwith $"Many Renderers are not supported"
    | _ -> failwith $"Renderer {renderer} not supported"

  and private generateFieldJson<'typeValue> (field: Field<'typeValue>) : JsonValue =
    let (FieldIdentifier fieldName) = field.Name
    let typeProp = [ "type", JsonValue.String fieldName ]

    let labelProp =
      field.Label
      |> Option.map (fun (Label label) -> "label", JsonValue.String label)
      |> Option.toList

    let tooltipProp =
      field.Tooltip
      |> Option.map (fun (Tooltip tooltip) -> "tooltip", JsonValue.String tooltip)
      |> Option.toList

    let detailsProp =
      field.Details
      |> Option.map (fun (Details details) -> "details", JsonValue.String details)
      |> Option.toList

    let rendererProp = generateRendererJson field.Renderer
    JsonValue.Record(Array.ofList (typeProp @ labelProp @ tooltipProp @ detailsProp @ rendererProp))

  and private generateFormJson<'typeValue> (form: Form<'typeValue>) : JsonValue =
    let (TypeIdentifier typeName) = form.TypeIdentifier
    let baseProps = [ "type", JsonValue.String typeName ]

    let rendererProp =
      form.RendererId
      |> Option.map (fun (RendererIdentifier id) -> "renderer", JsonValue.String id)
      |> Option.toList

    let fieldsProp =
      form.Body.Members.Fields
      |> Map.toSeq
      |> Seq.map (fun ((FieldIdentifier fieldName), field) -> fieldName, generateFieldJson field)
      |> Array.ofSeq

    let disabledFieldsProp =
      form.Body.DisabledFields
      |> Set.toSeq
      |> Seq.map (fun (FieldIdentifier fieldName) -> JsonValue.String fieldName)
      |> Array.ofSeq
      |> JsonValue.Array

    let tabsProps =
      form.Body.Members.Tabs
      |> Map.toSeq
      |> Seq.map (fun ((TabIdentifier tabName), tab) ->
        let columnsProps =
          tab.Columns
          |> Map.toSeq
          |> Seq.map (fun ((ColumnIdentifier columnName), column) ->
            let groupsProps =
              column.Groups
              |> Map.toSeq
              |> Seq.map (fun ((GroupIdentifier groupName), group) ->
                let fieldsArray =
                  group
                  |> Set.toSeq
                  |> Seq.map (fun (FieldIdentifier fieldName) -> JsonValue.String fieldName)
                  |> Array.ofSeq

                groupName, JsonValue.Array fieldsArray)
              |> Array.ofSeq

            columnName, JsonValue.Record [| "groups", JsonValue.Record groupsProps |])
          |> Array.ofSeq

        tabName, JsonValue.Record [| "columns", JsonValue.Record columnsProps |])
      |> Array.ofSeq

    let tabsProp = [ "tabs", JsonValue.Record tabsProps ]

    JsonValue.Record(
      Array.ofList (
        baseProps
        @ rendererProp
        @ [ "fields", JsonValue.Record fieldsProp ]
        @ [ "disabledFields", disabledFieldsProp ]
        @ tabsProp
      )
    )

  and internal generateForms<'typeValue> (forms: OrderedMap<FormIdentifier, Form<'typeValue>>) : JsonValue =
    forms
    |> OrderedMap.toSeq
    |> Seq.distinctBy fst
    |> Seq.map (fun (formId, form) ->
      let (FormIdentifier formName) = formId
      let formJson = generateFormJson form
      (formName, formJson))
    |> Array.ofSeq
    |> JsonValue.Record

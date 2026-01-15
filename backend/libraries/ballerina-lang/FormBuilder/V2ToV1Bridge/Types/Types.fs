namespace Ballerina.DSL.FormBuilder.V2ToV1Bridge.Types

module Types =

  open Ballerina.DSL.FormBuilder.Model.FormAST
  open Ballerina.Cat.Collections.OrderedMap
  open FSharp.Data

  let rec private generateFieldTypeJson<'typeValue>
    (forms: OrderedMap<FormIdentifier, Form<'typeValue>>)
    (renderer: RendererExpression<'typeValue>)
    : JsonValue =
    match renderer with
    | RendererExpression.Primitive(primitive) ->
      match primitive.Renderer with
      | PrimitiveRendererKind.Unit -> JsonValue.String "unit"
      | PrimitiveRendererKind.Guid -> JsonValue.String "guid"
      | PrimitiveRendererKind.Int32 -> JsonValue.String "Int32"
      | PrimitiveRendererKind.Int64 -> JsonValue.String "Int64"
      | PrimitiveRendererKind.Float32 -> JsonValue.String "Float32"
      | PrimitiveRendererKind.Float -> JsonValue.String "Float64"
      | PrimitiveRendererKind.Bool -> JsonValue.String "boolean"
      | PrimitiveRendererKind.String -> JsonValue.String "string"
      | PrimitiveRendererKind.Date -> JsonValue.String "datetime"
      | PrimitiveRendererKind.DateOnly -> JsonValue.String "dateonly"
      | PrimitiveRendererKind.StringId -> JsonValue.String "string"
      | PrimitiveRendererKind.Base64 -> JsonValue.String "base64"
      | PrimitiveRendererKind.Secret -> JsonValue.String "secret"
    | RendererExpression.List { Element = element } ->
      JsonValue.Record
        [| "fun", JsonValue.String "List"
           "args", JsonValue.Array [| generateFieldTypeJson forms element |] |]
    | RendererExpression.Sum { Left = left; Right = right } ->
      JsonValue.Record
        [| "fun", JsonValue.String "Sum"
           "args", JsonValue.Array [| generateFieldTypeJson forms left; generateFieldTypeJson forms right |] |]

    | RendererExpression.Tuple { Items = items } ->
      JsonValue.Record
        [| "fun", JsonValue.String "Tuple"
           "args",
           items
           |> List.map (generateFieldTypeJson forms)
           |> List.toArray
           |> JsonValue.Array |]
    | RendererExpression.Union { Cases = cases } ->
      let casesJson =
        cases
        |> Map.toSeq
        |> Seq.map (fun ((CaseIdentifier caseName), renderer) ->
          caseName,
          match renderer with
          | RendererExpression.Primitive(primitive) ->
            match primitive.Renderer with
            | PrimitiveRendererKind.Unit -> JsonValue.Record [||]
            | _ -> failwith $"Only named records and unit are supported as a union case"
          | RendererExpression.Record { Members = members } ->
            members.Fields
            |> Map.toSeq
            |> Seq.map (fun ((FieldIdentifier fieldName), field) ->
              fieldName, generateFieldTypeJson forms field.Renderer)
            |> Array.ofSeq
            |> JsonValue.Record
          | RendererExpression.Form(formIdentifier, _) ->
            match OrderedMap.tryFind formIdentifier forms with
            | Some form ->
              let (TypeIdentifier typeId) = form.TypeIdentifier
              JsonValue.String(typeId)
            | None -> failwith $"Form {formIdentifier} not found"
          | _ -> failwith $"Only named records and unit are supported as a union case")

        |> Seq.map (fun (caseName, caseJson) ->
          JsonValue.Record [| "caseName", JsonValue.String caseName; "fields", caseJson |])
        |> Seq.toArray
        |> JsonValue.Array

      JsonValue.Record [| "fun", JsonValue.String "Union"; "args", casesJson |]
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

      generateFormTypeJson forms form
    | RendererExpression.Map { Key = key; Value = value } ->
      JsonValue.Record
        [| "fun", JsonValue.String "Map"
           "args", JsonValue.Array [| generateFieldTypeJson forms key; generateFieldTypeJson forms value |] |]
    | RendererExpression.Form(formIdentifier, _) ->
      match OrderedMap.tryFind formIdentifier forms with
      | Some form ->
        let (TypeIdentifier typeId) = form.TypeIdentifier
        JsonValue.String(typeId)
      | None -> failwith $"Form {formIdentifier} not found"

    | RendererExpression.InlineForm(inlineForm) ->
      let form: Form<'typeValue> =
        { IsEntryPoint = false
          RendererId = None
          Form = FormIdentifier "anonymous form"
          // This feels brittle
          TypeIdentifier = TypeIdentifier(inlineForm.Type.ToString())
          Type = inlineForm.Type
          Body = inlineForm.Body }

      generateFormTypeJson forms form
    | RendererExpression.Readonly(readonly) ->
      let childTypeJson = generateFieldTypeJson forms readonly.Value

      JsonValue.Record
        [| "fun", JsonValue.String "ReadOnly"
           "args", JsonValue.Array [| childTypeJson |] |]
    | RendererExpression.Table _ -> failwith $"Table Renderers are not supported for field type generation"
    | RendererExpression.One _ -> failwith $"One Renderers are not supported for field type generation"
    | RendererExpression.Many _ -> failwith $"Many Renderers are not supported for field type generation"
    | _ -> failwith $"Renderer {renderer} not supported for field type generation"

  and private generateFormTypeJson<'typeValue>
    (forms: OrderedMap<FormIdentifier, Form<'typeValue>>)
    (form: Form<'typeValue>)
    : JsonValue =
    let fieldsProp =
      form.Body.Members.Fields
      |> Map.toSeq
      |> Seq.map (fun ((FieldIdentifier fieldName), field) -> fieldName, generateFieldTypeJson forms field.Renderer)
      |> Array.ofSeq

    // Simplify
    JsonValue.Record [| "fields", JsonValue.Record fieldsProp |]

  and internal generateFormsTypeJson<'typeValue> (forms: OrderedMap<FormIdentifier, Form<'typeValue>>) : JsonValue =
    // Config types are mandatory in the front end engine, so we use this placeholder for every launcher
    let emptyConfigType =
      ("EmptyConfig", JsonValue.Record [| "fields", JsonValue.Record [||] |])

    let otherTypes =
      forms
      |> OrderedMap.toSeq
      |> Seq.distinctBy fst
      |> Seq.map (fun (_, form) ->
        let (TypeIdentifier typeId) = form.TypeIdentifier
        let formJson = generateFormTypeJson forms form
        (typeId, formJson))
      |> Array.ofSeq

    Array.append [| emptyConfigType |] otherTypes |> JsonValue.Record

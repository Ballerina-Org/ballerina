namespace Ballerina.DSL.FormBuilder.V2ToV1Bridge.Types

module Types =

  open Ballerina.DSL.FormBuilder.Model.FormAST
  open Ballerina.Cat.Collections.OrderedMap
  open FSharp.Data


  // Due to some technical debt, we have to treat union types specially.
  // We have to generate types from renderers because renderers encode Readonly types
  // Readonly types only exist in v1 ballerina, but should not be created as types in v2
  // At the moment, the form AST only supports top level record renderers, but in v1, unions have to be defined as top level types
  // So, we have a problem because we cannot specify a top level union form in v2 which would be needed to generate the top level union form in v1.
  // The work around is to extract the union types from the renderers and then generate them before the form which references them.
  // This is similar to hoisting.
  let rec private generateUnionTypesJson<'typeValue>
    (forms: OrderedMap<FormIdentifier, Form<'typeValue>>)
    (seen: Set<string>)
    (unionTypes: UnionRenderer<'typeValue> list)
    : (string * JsonValue) list * UnionRenderer<'typeValue> list * Set<string> =

    let (json, additionalTypes, newSeen) =
      unionTypes
      |> Seq.fold
        (fun (accResults, accAdditional, seen) (unionType) ->
          seen |> Set.iter (fun s -> System.Console.WriteLine($"s: {s}"))

          if Set.contains (unionType.Type.ToString()) seen then
            (accResults, accAdditional, seen)
          else
            let casesJsonAndFurtherUnionTypes =
              unionType.Cases
              |> Map.toSeq
              |> Seq.map (fun ((CaseIdentifier caseName), renderer) ->
                caseName,
                match renderer with
                | RendererExpression.Primitive(primitive) ->
                  match primitive.Renderer with
                  | PrimitiveRendererKind.Unit -> JsonValue.Record [||], Seq.empty
                  | _ -> failwith $"Only named records and unit are supported as a union case"
                | RendererExpression.Record { Members = members } ->
                  let results =
                    members.Fields
                    |> Map.toSeq
                    |> Seq.map (fun ((FieldIdentifier fieldName), field) ->
                      let fieldTypeJson, fieldUnionTypes =
                        generateFieldTypeJson forms Seq.empty field.Renderer

                      (fieldName, fieldTypeJson), fieldUnionTypes)

                  let jsonFields = results |> Seq.map fst |> Seq.toArray |> JsonValue.Record
                  let unionTypes = results |> Seq.map snd |> Seq.concat
                  jsonFields, unionTypes
                | RendererExpression.Form(formIdentifier, _) ->
                  match OrderedMap.tryFind formIdentifier forms with
                  | Some form ->
                    let (TypeIdentifier typeId) = form.TypeIdentifier
                    JsonValue.String(typeId), Seq.empty
                  | None -> failwith $"Form {formIdentifier} not found"
                | _ -> failwith $"Only named records and unit are supported as a union case")

              |> Seq.map (fun (caseName, (caseJson, caseUnionTypes)) ->
                JsonValue.Record [| "caseName", JsonValue.String caseName; "fields", caseJson |], caseUnionTypes)

            let casesJson =
              casesJsonAndFurtherUnionTypes |> Seq.map fst |> Seq.toArray |> JsonValue.Array

            let additional =
              casesJsonAndFurtherUnionTypes |> Seq.map snd |> Seq.concat |> List.ofSeq

            let newResult =
              (unionType.Type.ToString(), JsonValue.Record [| "fun", JsonValue.String "Union"; "args", casesJson |])

            newResult :: accResults, additional @ accAdditional, Set.add (unionType.Type.ToString()) seen)
        (List.Empty, List.Empty, seen)

    // we want additional types to be in reverse order because a nested union type has a reverse dependency graph
    // so we don't reverse them
    match additionalTypes with
    | [] -> json, [], newSeen
    | _ ->
      let jsonValues, _, additionalSeen =
        generateUnionTypesJson forms newSeen additionalTypes

      jsonValues @ json, [], additionalSeen

  and private generateFieldTypeJson<'typeValue>
    (forms: OrderedMap<FormIdentifier, Form<'typeValue>>)
    (unionTypes: UnionRenderer<'typeValue> seq)
    (renderer: RendererExpression<'typeValue>)
    : JsonValue * unionTypes: UnionRenderer<'typeValue> seq =
    match renderer with
    | RendererExpression.Primitive(primitive) ->
      match primitive.Renderer with
      | PrimitiveRendererKind.Unit -> JsonValue.String "unit", unionTypes
      | PrimitiveRendererKind.Guid -> JsonValue.String "guid", unionTypes
      | PrimitiveRendererKind.Int32 -> JsonValue.String "Int32", unionTypes
      | PrimitiveRendererKind.Int64 -> JsonValue.String "Int64", unionTypes
      | PrimitiveRendererKind.Float32 -> JsonValue.String "Float32", unionTypes
      | PrimitiveRendererKind.Float -> JsonValue.String "Float64", unionTypes
      | PrimitiveRendererKind.Bool -> JsonValue.String "boolean", unionTypes
      | PrimitiveRendererKind.String -> JsonValue.String "string", unionTypes
      | PrimitiveRendererKind.Date -> JsonValue.String "Date", unionTypes
      | PrimitiveRendererKind.DateOnly -> failwith $"DateOnly is not supported in the bridge, use Datetime"
      | PrimitiveRendererKind.StringId -> JsonValue.String "string", unionTypes
      | PrimitiveRendererKind.Base64 -> failwith $"Base64 is not supported in the bridge"
      | PrimitiveRendererKind.Secret -> failwith $"Secret is not supported in the bridge"
    | RendererExpression.List { Element = element } ->
      let elementTypeJson, elementUnionTypes =
        generateFieldTypeJson forms unionTypes element

      JsonValue.Record
        [| "fun", JsonValue.String "List"
           "args", JsonValue.Array [| elementTypeJson |] |],
      elementUnionTypes
    | RendererExpression.Sum { Left = left; Right = right } ->
      let leftTypeJson, leftUnionTypes = generateFieldTypeJson forms unionTypes left
      let rightTypeJson, rightUnionTypes = generateFieldTypeJson forms unionTypes right

      JsonValue.Record
        [| "fun", JsonValue.String "Sum"
           "args", JsonValue.Array [| leftTypeJson; rightTypeJson |] |],
      Seq.append leftUnionTypes rightUnionTypes

    | RendererExpression.Tuple { Items = items } ->
      let itemsJsons = items |> Seq.map (generateFieldTypeJson forms unionTypes)
      let itemTypesJsons = itemsJsons |> Seq.map fst
      let itemsUnionTypes = itemsJsons |> Seq.map snd |> Seq.concat

      JsonValue.Record
        [| "fun", JsonValue.String "Tuple"
           "args", itemTypesJsons |> Seq.toArray |> JsonValue.Array |],
      itemsUnionTypes
    // needs to be a lookup type
    | RendererExpression.Union union -> JsonValue.String(union.Type.ToString()), Seq.singleton union
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

      generateFormTypeJson forms unionTypes form
    | RendererExpression.Map { Key = key; Value = value } ->
      let keyTypeJson, keyUnionTypes = generateFieldTypeJson forms unionTypes key
      let valueTypeJson, valueUnionTypes = generateFieldTypeJson forms unionTypes value

      JsonValue.Record
        [| "fun", JsonValue.String "Map"
           "args", JsonValue.Array [| keyTypeJson; valueTypeJson |] |],
      Seq.append keyUnionTypes valueUnionTypes
    | RendererExpression.Form(formIdentifier, _) ->
      match OrderedMap.tryFind formIdentifier forms with
      | Some form ->
        let (TypeIdentifier typeId) = form.TypeIdentifier
        JsonValue.String(typeId), Seq.empty
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

      generateFormTypeJson forms unionTypes form
    | RendererExpression.Readonly(readonly) ->
      let childTypeJson, childUnionTypes =
        generateFieldTypeJson forms unionTypes readonly.Value

      JsonValue.Record
        [| "fun", JsonValue.String "ReadOnly"
           "args", JsonValue.Array [| childTypeJson |] |],
      childUnionTypes
    | RendererExpression.Table _ -> failwith $"Table Renderers are not supported for field type generation"
    | RendererExpression.One _ -> failwith $"One Renderers are not supported for field type generation"
    | RendererExpression.Many _ -> failwith $"Many Renderers are not supported for field type generation"
    | _ -> failwith $"Renderer {renderer} not supported for field type generation"

  and private generateFormTypeJson<'typeValue>
    (forms: OrderedMap<FormIdentifier, Form<'typeValue>>)
    (unionTypes: UnionRenderer<'typeValue> seq)
    (form: Form<'typeValue>)
    : JsonValue * UnionRenderer<'typeValue> seq =

    let fieldsRes =
      form.Body.Members.Fields
      |> Map.toSeq
      |> Seq.map (fun ((FieldIdentifier fieldName), field) ->
        fieldName, generateFieldTypeJson forms unionTypes field.Renderer)

    let fieldsProps =
      fieldsRes
      |> Seq.map (fun (fieldName, (fieldTypeJson, _)) -> fieldName, fieldTypeJson)
      |> Array.ofSeq

    let fieldsUnionTypes =
      fieldsRes |> Seq.map (fun (_, (_, unionTypes)) -> unionTypes) |> Seq.concat

    JsonValue.Record [| "fields", JsonValue.Record fieldsProps |], fieldsUnionTypes

  and internal generateFormsTypeJson<'typeValue> (forms: OrderedMap<FormIdentifier, Form<'typeValue>>) : JsonValue =
    // Config types are mandatory in the front end engine, so we use this placeholder for every launcher
    let emptyConfigType =
      ("EmptyConfig", JsonValue.Record [| "fields", JsonValue.Record [||] |])

    let otherTypes, _ =
      forms
      |> OrderedMap.toSeq
      |> Seq.distinctBy fst
      |> Seq.fold
        (fun (accResults, seen) (_, form) ->
          let (TypeIdentifier typeId) = form.TypeIdentifier

          if Set.contains typeId seen then
            (accResults, seen)
          else
            let formJson, unionTypes = generateFormTypeJson forms Seq.empty form

            let newUnionTypes =
              unionTypes |> Seq.filter (fun u -> not (Set.contains (u.Type.ToString()) seen))

            let unionTypesJson, _, newSeen =
              newUnionTypes |> Seq.toList |> generateUnionTypesJson forms seen

            let results = (typeId, formJson) :: unionTypesJson

            results @ accResults, Set.add typeId newSeen)
        (List.empty, Set.empty)
      |> fun (results, seen) -> (results |> List.rev |> Array.ofSeq, seen)

    Array.append [| emptyConfigType |] otherTypes |> JsonValue.Record

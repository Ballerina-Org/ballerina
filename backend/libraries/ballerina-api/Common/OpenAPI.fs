namespace Ballerina.DSL.Next

open System
open Ballerina.Collections.Sum
open Ballerina.Reader.WithError
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.TypeChecker
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.State.WithError
open Ballerina.Errors
open Ballerina.DSL.Next.StdLib.Extensions
open Ballerina.Cat.Collections.OrderedMap

module OpenAPI =
  type OpenAPISpec =
    { Title: string
      Version: string
      Endpoints: List<OpenAPIEndpoint>
      DataModels: Map<OpenAPIDataModelName, OpenAPIDataModel> }

  and OpenAPIEndpoint =
    { Path: string
      Method: OpenAPIEndpointModel
      RequestModel: Option<OpenAPIDataModel>
      ResponseModel: Option<OpenAPIDataModel> }

  and OpenAPIDataModelName = { OpenAPIDataModelName: string }

  and OpenAPIEndpointModel =
    | Get
    | Post
    | Put
    | Delete
    | Patch

  and OpenAPIDataModel =
    | Ref of OpenAPIDataModelName
    | Record of List<ResolvedIdentifier * OpenAPIDataModel>
    | Primitive of PrimitiveType
    | AnyObject
    | Union of List<ResolvedIdentifier * OpenAPIDataModel>
    | Sum of List<OpenAPIDataModel>
    | Tuple of List<OpenAPIDataModel>
    | List of OpenAPIDataModel

  let private yaml_escape (value: string) = value.Replace("'", "''")

  let private yaml_string (value: string) = $"'{yaml_escape value}'"

  let private indent level = String.replicate (level * 2) " "

  let private compare_ordinal (a: string) (b: string) =
    String.CompareOrdinal(a, b)

  let private resolved_identifier_to_string (id: ResolvedIdentifier) =
    match id.Type with
    | Some t -> $"{t}::{id.Name}"
    | None -> id.Name

  let private openapi_method_to_string =
    function
    | Get -> "get"
    | Post -> "post"
    | Put -> "put"
    | Delete -> "delete"
    | Patch -> "patch"

  let private wrap_case_in_property level case_name case_schema_lines =
    [ $"{indent level}type: object"
      $"{indent level}properties:"
      $"{indent (level + 1)}{yaml_string case_name}:" ]
    @ case_schema_lines
    @ [ $"{indent level}required:"
        $"{indent (level + 1)}- {yaml_string case_name}" ]

  let private render_primitive_schema_lines level primitive_type =
    let primitive_case_name =
      match primitive_type with
      | PrimitiveType.Unit -> "Unit"
      | PrimitiveType.Guid -> "Guid"
      | PrimitiveType.Int32 -> "Int32"
      | PrimitiveType.Int64 -> "Int64"
      | PrimitiveType.Float32 -> "Float32"
      | PrimitiveType.Float64 -> "Float64"
      | PrimitiveType.Decimal -> "Decimal"
      | PrimitiveType.Bool -> "Bool"
      | PrimitiveType.String -> "String"
      | PrimitiveType.DateTime -> "DateTime"
      | PrimitiveType.DateOnly -> "DateOnly"
      | PrimitiveType.TimeSpan -> "TimeSpan"
      | PrimitiveType.Vector -> "Vector"

    let primitive_schema_lines =
      match primitive_type with
      | PrimitiveType.Unit -> [ $"{indent (level + 2)}type: 'null'" ]
      | PrimitiveType.Guid ->
        [ $"{indent (level + 2)}type: string"
          $"{indent (level + 2)}format: uuid" ]
      | PrimitiveType.Int32 ->
        [ $"{indent (level + 2)}type: integer"
          $"{indent (level + 2)}format: int32" ]
      | PrimitiveType.Int64 ->
        [ $"{indent (level + 2)}type: integer"
          $"{indent (level + 2)}format: int64" ]
      | PrimitiveType.Float32 ->
        [ $"{indent (level + 2)}type: number"
          $"{indent (level + 2)}format: float" ]
      | PrimitiveType.Float64 ->
        [ $"{indent (level + 2)}type: number"
          $"{indent (level + 2)}format: double" ]
      | PrimitiveType.Decimal -> [ $"{indent (level + 2)}type: number" ]
      | PrimitiveType.Bool -> [ $"{indent (level + 2)}type: boolean" ]
      | PrimitiveType.String -> [ $"{indent (level + 2)}type: string" ]
      | PrimitiveType.DateTime ->
        [ $"{indent (level + 2)}type: string"
          $"{indent (level + 2)}format: date-time" ]
      | PrimitiveType.DateOnly ->
        [ $"{indent (level + 2)}type: string"
          $"{indent (level + 2)}format: date" ]
      | PrimitiveType.TimeSpan ->
        [ $"{indent (level + 2)}type: string"
          $"{indent (level + 2)}format: duration" ]
      | PrimitiveType.Vector ->
        [ $"{indent (level + 2)}type: array"
          $"{indent (level + 2)}items:"
          $"{indent (level + 3)}type: number"
          $"{indent (level + 3)}format: float" ]

    wrap_case_in_property level primitive_case_name primitive_schema_lines

  let private render_parameter_primitive_schema_lines level primitive_type =
    match primitive_type with
    | PrimitiveType.Unit -> [ $"{indent level}type: 'null'" ]
    | PrimitiveType.Guid ->
      [ $"{indent level}type: string"; $"{indent level}format: uuid" ]
    | PrimitiveType.Int32 ->
      [ $"{indent level}type: integer"; $"{indent level}format: int32" ]
    | PrimitiveType.Int64 ->
      [ $"{indent level}type: integer"; $"{indent level}format: int64" ]
    | PrimitiveType.Float32 ->
      [ $"{indent level}type: number"; $"{indent level}format: float" ]
    | PrimitiveType.Float64 ->
      [ $"{indent level}type: number"; $"{indent level}format: double" ]
    | PrimitiveType.Decimal -> [ $"{indent level}type: number" ]
    | PrimitiveType.Bool -> [ $"{indent level}type: boolean" ]
    | PrimitiveType.String -> [ $"{indent level}type: string" ]
    | PrimitiveType.DateTime ->
      [ $"{indent level}type: string"; $"{indent level}format: date-time" ]
    | PrimitiveType.DateOnly ->
      [ $"{indent level}type: string"; $"{indent level}format: date" ]
    | PrimitiveType.TimeSpan ->
      [ $"{indent level}type: string"; $"{indent level}format: duration" ]
    | PrimitiveType.Vector ->
      [ $"{indent level}type: array"
        $"{indent level}items:"
        $"{indent (level + 1)}type: number"
        $"{indent (level + 1)}format: float" ]

  let private extract_path_parameter_names (path: string) =
    let rec loop index acc =
      if index >= path.Length then
        List.rev acc
      else
        let start_index = path.IndexOf('{', index)

        if start_index < 0 then
          List.rev acc
        else
          let end_index = path.IndexOf('}', start_index + 1)

          if end_index < 0 then
            List.rev acc
          else
            let name =
              path.Substring(start_index + 1, end_index - start_index - 1)

            loop (end_index + 1) (name :: acc)

    loop 0 []

  let rec private render_schema_lines level =
    function
    | Ref model_name ->
      let schema_ref = $"#/components/schemas/{model_name.OpenAPIDataModelName}"
      [ $"{indent level}$ref: {yaml_string schema_ref}" ]
    | Primitive primitive_type ->
      render_primitive_schema_lines level primitive_type
    | AnyObject ->
      [ $"{indent level}type: object"
        $"{indent level}additionalProperties: true" ]
    | List item_type ->
      let list_schema_lines =
        [ $"{indent (level + 2)}type: array"; $"{indent (level + 2)}items:" ]
        @ (render_schema_lines (level + 3) item_type)

      (wrap_case_in_property level "List" list_schema_lines)
      @ [ $"{indent level}additionalProperties: false" ]
    | Tuple elements ->
      let prefix_items =
        elements
        |> List.collect (fun element ->
          [ $"{indent (level + 3)}-" ]
          @ (render_schema_lines (level + 4) element))

      let tuple_schema_lines =
        [ $"{indent (level + 2)}type: array"
          $"{indent (level + 2)}prefixItems:" ]
        @ prefix_items
        @ [ $"{indent (level + 2)}minItems: {elements.Length}"
            $"{indent (level + 2)}maxItems: {elements.Length}" ]

      (wrap_case_in_property level "Tuple" tuple_schema_lines)
      @ [ $"{indent level}additionalProperties: false" ]
    | Sum options ->
      let sum_schema_lines =
        [ $"{indent (level + 2)}oneOf:" ]
        @ (options
           |> List.mapi (fun i o -> (i, o))
           |> List.collect (fun (i, option_model) ->
             [ $"{indent (level + 3)}-" ]
             @ wrap_case_in_property
                 (level + 4)
                 $"{i + 1}Of{options.Length}"
                 (render_schema_lines (level + 6) option_model)))

      (wrap_case_in_property level "Sum" sum_schema_lines)
      @ [ $"{indent level}additionalProperties: false" ]
    | Record fields ->
      let sorted_fields =
        fields
        |> List.sortWith (fun (left_name, _) (right_name, _) ->
          compare_ordinal
            (left_name |> resolved_identifier_to_string)
            (right_name |> resolved_identifier_to_string))

      let rendered_properties =
        sorted_fields
        |> List.collect (fun (field_name, field_model) ->
          let field_name = field_name |> resolved_identifier_to_string

          [ $"{indent (level + 3)}{yaml_string field_name}:" ]
          @ (render_schema_lines (level + 4) field_model))

      let required_fields =
        sorted_fields
        |> List.map (fun (field_name, _) ->
          let field_name = field_name |> resolved_identifier_to_string
          $"{indent (level + 3)}- {yaml_string field_name}")

      let record_schema_lines =
        [ $"{indent (level + 2)}type: object"
          $"{indent (level + 2)}properties:" ]
        @ (if List.isEmpty fields then
             [ $"{indent (level + 3)}{{}}" ]
           else
             rendered_properties)
        @ (if List.isEmpty fields then
             []
           else
             [ $"{indent (level + 2)}required:" ] @ required_fields)
        @ [ $"{indent (level + 2)}additionalProperties: false" ]


      wrap_case_in_property level "Record" record_schema_lines
    | Union cases ->
      let sorted_cases =
        cases
        |> List.sortWith (fun (left_name, _) (right_name, _) ->
          compare_ordinal
            (left_name |> resolved_identifier_to_string)
            (right_name |> resolved_identifier_to_string))

      let union_schema_lines =
        [ $"{indent (level + 2)}oneOf:" ]
        @ (sorted_cases
           |> List.collect (fun (case_name, case_model) ->
             let case_name = case_name |> resolved_identifier_to_string

             [ $"{indent (level + 3)}-"
               $"{indent (level + 4)}type: object"
               $"{indent (level + 4)}properties:"
               $"{indent (level + 5)}{yaml_string case_name}:" ]
             @ (render_schema_lines (level + 6) case_model)
             @ [ $"{indent (level + 4)}required:"
                 $"{indent (level + 5)}- {yaml_string case_name}" ]))
        @ [ $"{indent (level + 2)}additionalProperties: false" ]

      (wrap_case_in_property level "Union" union_schema_lines)
      @ [ $"{indent level}additionalProperties: false" ]

  let private render_endpoint_lines level (endpoint: OpenAPIEndpoint) =
    let path_parameter_names = extract_path_parameter_names endpoint.Path

    let operation_name =
      sprintf
        "%s %s"
        ((openapi_method_to_string endpoint.Method).ToUpperInvariant())
        endpoint.Path

    let operation_id =
      let sanitized_path =
        endpoint.Path
          .Replace("/", "_")
          .Replace("{", "")
          .Replace("}", "")
          .Trim('_')

      sprintf "%s_%s" (openapi_method_to_string endpoint.Method) sanitized_path

    let path_parameters_lines =
      match endpoint.Method, path_parameter_names, endpoint.RequestModel with
      | Get, [ parameter_name ], Some(Primitive primitive_type) ->
        [ $"{indent (level + 2)}parameters:"
          $"{indent (level + 3)}- name: {parameter_name}"
          $"{indent (level + 4)}in: path"
          $"{indent (level + 4)}required: true"
          $"{indent (level + 4)}schema:" ]
        @ (render_parameter_primitive_schema_lines (level + 5) primitive_type)
      | Get, [ parameter_name ], Some(Ref id_model_name) ->
        let path_parameter_schema_ref =
          sprintf "#/components/schemas/%s" id_model_name.OpenAPIDataModelName

        [ $"{indent (level + 2)}parameters:"
          $"{indent (level + 3)}- name: {parameter_name}"
          $"{indent (level + 4)}in: path"
          $"{indent (level + 4)}required: true"
          $"{indent (level + 4)}schema:"
          $"{indent (level + 5)}$ref: {yaml_string path_parameter_schema_ref}" ]
      | Delete, [ parameter_name ], Some(Primitive primitive_type) ->
        [ $"{indent (level + 2)}parameters:"
          $"{indent (level + 3)}- name: {parameter_name}"
          $"{indent (level + 4)}in: path"
          $"{indent (level + 4)}required: true"
          $"{indent (level + 4)}schema:" ]
        @ (render_parameter_primitive_schema_lines (level + 5) primitive_type)
      | Delete, [ parameter_name ], Some(Ref id_model_name) ->
        let path_parameter_schema_ref =
          sprintf "#/components/schemas/%s" id_model_name.OpenAPIDataModelName

        [ $"{indent (level + 2)}parameters:"
          $"{indent (level + 3)}- name: {parameter_name}"
          $"{indent (level + 4)}in: path"
          $"{indent (level + 4)}required: true"
          $"{indent (level + 4)}schema:"
          $"{indent (level + 5)}$ref: {yaml_string path_parameter_schema_ref}" ]
      | _ -> []

    let query_parameters_lines =
      match endpoint.Method, path_parameter_names, endpoint.RequestModel with
      | Get, [], Some(Record fields) ->
        let sorted_fields =
          fields
          |> List.sortWith (fun (left_name, _) (right_name, _) ->
            compare_ordinal
              (left_name |> resolved_identifier_to_string)
              (right_name |> resolved_identifier_to_string))

        let rendered_query_parameters =
          sorted_fields
          |> List.collect (fun (field_name, field_model) ->
            match field_model with
            | Primitive primitive_type ->
              [ $"{indent (level + 3)}- name: {field_name.Name}"
                $"{indent (level + 4)}in: query"
                $"{indent (level + 4)}required: true"
                $"{indent (level + 4)}schema:" ]
              @ (render_parameter_primitive_schema_lines
                (level + 5)
                primitive_type)
            | Ref model_name ->
              let schema_ref =
                sprintf
                  "#/components/schemas/%s"
                  model_name.OpenAPIDataModelName

              [ $"{indent (level + 3)}- name: {field_name.Name}"
                $"{indent (level + 4)}in: query"
                $"{indent (level + 4)}required: true"
                $"{indent (level + 4)}schema:"
                $"{indent (level + 5)}$ref: {yaml_string schema_ref}" ]
            | _ -> [])

        if List.isEmpty rendered_query_parameters then
          []
        else
          [ $"{indent (level + 2)}parameters:" ] @ rendered_query_parameters
      | _ -> []

    let parameters_lines =
      if List.isEmpty path_parameters_lines then
        query_parameters_lines
      else
        path_parameters_lines

    let request_lines =
      endpoint.RequestModel
      |> Option.map (fun request_model ->
        [ $"{indent (level + 2)}requestBody:"
          $"{indent (level + 3)}required: true"
          $"{indent (level + 3)}content:"
          $"{indent (level + 4)}application/json:"
          $"{indent (level + 5)}schema:" ]
        @ (render_schema_lines (level + 6) request_model))
      |> Option.defaultValue []

    let request_lines =
      if List.isEmpty parameters_lines then request_lines else []

    let response_lines =
      endpoint.ResponseModel
      |> Option.map (fun response_model ->
        [ $"{indent (level + 4)}description: OK"
          $"{indent (level + 4)}content:"
          $"{indent (level + 5)}application/json:"
          $"{indent (level + 6)}schema:" ]
        @ (render_schema_lines (level + 7) response_model))
      |> Option.defaultValue [ $"{indent (level + 4)}description: OK" ]

    [ $"{indent level}{yaml_string endpoint.Path}:"
      $"{indent (level + 1)}{openapi_method_to_string endpoint.Method}:"
      $"{indent (level + 2)}summary: {yaml_string operation_name}"
      $"{indent (level + 2)}operationId: {yaml_string operation_id}"
      $"{indent (level + 2)}security: []" ]
    @ parameters_lines
    @ request_lines
    @ [ $"{indent (level + 2)}responses:"; $"{indent (level + 3)}'200':" ]
    @ response_lines
    @ [ $"{indent (level + 3)}'400':"
        $"{indent (level + 4)}description: Bad Request"
        $"{indent (level + 3)}'404':"
        $"{indent (level + 4)}description: Not Found" ]

  let to_yaml (spec: OpenAPISpec) =
    let sorted_endpoints =
      spec.Endpoints
      |> List.sortWith (fun left right ->
        let by_path = compare_ordinal left.Path right.Path

        if by_path <> 0 then
          by_path
        else
          compare_ordinal
            (left.Method |> openapi_method_to_string)
            (right.Method |> openapi_method_to_string))

    let paths_lines = sorted_endpoints |> List.collect (render_endpoint_lines 1)

    let components_lines =
      spec.DataModels
      |> Map.toList
      |> List.sortWith (fun (left_name, _) (right_name, _) ->
        compare_ordinal
          left_name.OpenAPIDataModelName
          right_name.OpenAPIDataModelName)
      |> List.collect (fun (model_name, model_schema) ->
        [ $"{indent 2}{yaml_string model_name.OpenAPIDataModelName}:" ]
        @ (render_schema_lines 3 model_schema))

    [ "openapi: 3.1.0"
      "info:"
      $"  title: {yaml_string spec.Title}"
      $"  version: {yaml_string spec.Version}"
      "  license:"
      "    name: 'UNLICENSED'"
      "    identifier: 'UNLICENSED'"
      "servers:"
      "  - url: '/'"
      "paths:" ]
    @ (if List.isEmpty paths_lines then [ "  {}" ] else paths_lines)
    @ [ "components:"; "  schemas:" ]
    @ (if List.isEmpty components_lines then
         [ "    {}" ]
       else
         components_lines)
    |> String.concat "\n"

  let generate_data_models
    (schema: Schema<ValueExt<'runtimeContext, 'db, 'customExtension>>)
    : State<
        unit,
        (TypeCheckContext<ValueExt<'runtimeContext, 'db, 'customExtension>> *
        TypeCheckState<ValueExt<'runtimeContext, 'db, 'customExtension>>),
        Map<OpenAPIDataModelName, OpenAPIDataModel>,
        Errors<Unit>
       >
    =
    let rec generate_data_model
      (type_value: TypeValue<'ext>)
      (properties:
        List<
          SchemaEntityProperty<ValueExt<'runtimeContext, 'db, 'customExtension>>
         >)
      : State<
          OpenAPIDataModel,
          TypeCheckContext<ValueExt<'runtimeContext, 'db, 'customExtension>> *
          TypeCheckState<ValueExt<'runtimeContext, 'db, 'customExtension>>,
          Map<OpenAPIDataModelName, OpenAPIDataModel>,
          Errors<Unit>
         >
      =
      state {
        let properties_at_this_level =
          properties
          |> Seq.filter (fun prop -> prop.Path |> List.isEmpty)
          |> Seq.map (fun prop -> prop.PropertyName, prop)
          |> Map.ofSeq

        let properties_at_next_level =
          properties
          |> List.collect (fun prop ->
            match prop.Path with
            | [] -> []
            | _ :: rest -> [ { prop with Path = rest } ])

        match type_value |> TypeValue.GetSourceMapping with
        | TypeExprSourceMapping.OriginExprTypeLet(ExprTypeLetBindingName let_id,
                                                  _) ->
          let type_value =
            TypeValue.SetSourceMapping(
              type_value,
              TypeExprSourceMapping.NoSourceMapping ""
            )

          let! type_value =
            generate_data_model type_value properties_at_next_level

          let type_name = { OpenAPIDataModelName = let_id }
          do! state.SetState(Map.add type_name type_value)

          return OpenAPIDataModel.Ref type_name
        | TypeExprSourceMapping.OriginTypeExpr(_)
        | TypeExprSourceMapping.NoSourceMapping(_) ->

          match type_value with
          | TypeValue.Primitive { value = p } ->
            return OpenAPIDataModel.Primitive p
          | TypeValue.Record { value = fields } ->
            let! fields =
              fields
              |> OrderedMap.toSeq
              |> Seq.map (fun (name, (field_type, _)) ->
                state {
                  let! name =
                    state.Either
                      (properties_at_this_level
                       |> Map.tryFind (
                         name.Name.LocalName |> LocalIdentifier.Create
                       )
                       |> Option.map (fun prop ->
                         prop.PropertyName
                         |> ResolvedIdentifier.FromLocalIdentifier)
                       |> sum.OfOption(
                         (fun () ->
                           $"Field '{name.Name.LocalName}' not found in properties at this level.")
                         |> Errors.Singleton()
                       )
                       |> state.OfSum)
                      (TypeCheckState.tryFindResolvedIdentifier (name, ())
                       |> reader.MapContext snd
                       |> state.OfReader)


                  let! field_type =
                    generate_data_model field_type properties_at_next_level

                  return name, field_type
                })
              |> state.All

            return OpenAPIDataModel.Record fields
          | TypeValue.Union { value = cases } ->
            let! cases =
              cases
              |> OrderedMap.toSeq
              |> Seq.map (fun (name, case_type) ->
                state {
                  let! name =
                    state.Either
                      (properties_at_this_level
                       |> Map.tryFind (
                         name.Name.LocalName |> LocalIdentifier.Create
                       )
                       |> Option.map (fun prop ->
                         prop.PropertyName
                         |> ResolvedIdentifier.FromLocalIdentifier)
                       |> sum.OfOption(
                         (fun () ->
                           $"Field '{name.Name.LocalName}' not found in properties at this level.")
                         |> Errors.Singleton()
                       )
                       |> state.OfSum)
                      (TypeCheckState.tryFindResolvedIdentifier (name, ())
                       |> reader.MapContext snd
                       |> state.OfReader)

                  let! case_type =
                    generate_data_model case_type properties_at_next_level

                  return name, case_type
                })
              |> state.All

            return OpenAPIDataModel.Union cases
          | TypeValue.Sum { value = options } ->
            let! options =
              options
              |> Seq.map (fun option_type ->
                generate_data_model option_type properties_at_next_level)
              |> state.All

            return OpenAPIDataModel.Sum options
          | TypeValue.Tuple { value = elements } ->
            let! elements =
              elements
              |> Seq.map (fun element_type ->
                generate_data_model element_type properties_at_next_level)
              |> state.All

            return OpenAPIDataModel.Tuple elements
          | TypeValue.Imported { Id = type_id; Arguments = [ arg_t ] } when
            type_id = ("List"
                       |> Identifier.LocalScope
                       |> TypeCheckScope.Empty.Resolve)
            ->
            let! arg_t = generate_data_model arg_t properties_at_next_level
            return OpenAPIDataModel.List arg_t
          | _ ->
            return!
              (fun () -> $"Not supported type value: {type_value}")
              |> Errors.Singleton()
              |> state.Throw
      }

    state {
      do!
        schema.Entities
        |> OrderedMap.toSeq
        |> Seq.map (fun (entity_name, entity_desc) ->
          state {
            let! type_with_props =
              generate_data_model
                entity_desc.TypeWithProps
                entity_desc.Properties

            let! type_original =
              generate_data_model entity_desc.TypeOriginal []

            let! id = generate_data_model entity_desc.Id []

            let type_with_props_name =
              { OpenAPIDataModelName.OpenAPIDataModelName =
                  $"{entity_name.Name}-WithProps" }

            do! state.SetState(Map.add type_with_props_name type_with_props)

            let type_original_name =
              { OpenAPIDataModelName.OpenAPIDataModelName =
                  $"{entity_name.Name}-Original" }

            do! state.SetState(Map.add type_original_name type_original)

            let id_name =
              { OpenAPIDataModelName.OpenAPIDataModelName =
                  $"{entity_name.Name}-Id" }

            do! state.SetState(Map.add id_name id)
            return ()
          })
        |> state.All
        |> state.Ignore
    }

  let generate_endpoints
    (schema: Schema<ValueExt<'runtimeContext, 'db, 'customExtension>>)
    : State<
        unit,
        (TypeCheckContext<ValueExt<'runtimeContext, 'db, 'customExtension>> *
        TypeCheckState<ValueExt<'runtimeContext, 'db, 'customExtension>>) *
        Map<OpenAPIDataModelName, OpenAPIDataModel>,
        List<OpenAPIEndpoint>,
        Errors<Unit>
       >
    =
    state {
      do!
        schema.Entities
        |> OrderedMap.toSeq
        |> Seq.map (fun (entity_name, entity_desc) ->
          state {
            let type_with_props_name =
              { OpenAPIDataModelName.OpenAPIDataModelName =
                  $"{entity_name.Name}-WithProps" }

            let type_original_name =
              { OpenAPIDataModelName.OpenAPIDataModelName =
                  $"{entity_name.Name}-Original" }

            let id_name =
              { OpenAPIDataModelName.OpenAPIDataModelName =
                  $"{entity_name.Name}-Id" }
            // let! _, id_primitive = entity_desc.Id |> TypeValue.AsPrimaryKey |> state.OfSum

            let endpoint =
              { Path = $"/entities/{entity_name.Name}/getById/{{id}}"
                Method = OpenAPIEndpointModel.Get
                RequestModel = Some(OpenAPIDataModel.Ref id_name)
                ResponseModel = Some(OpenAPIDataModel.Ref type_with_props_name) }

            do! state.SetState(fun l -> endpoint :: l)

            let endpoint =
              { Path = $"/entities/{entity_name.Name}/getMany"
                Method = OpenAPIEndpointModel.Get
                RequestModel =
                  Some(
                    OpenAPIDataModel.Record
                      [ ("offset" |> ResolvedIdentifier.Create,
                         OpenAPIDataModel.Primitive PrimitiveType.Int32)
                        ("limit" |> ResolvedIdentifier.Create,
                         OpenAPIDataModel.Primitive PrimitiveType.Int32) ]
                  )
                ResponseModel =
                  type_with_props_name
                  |> OpenAPIDataModel.Ref
                  |> OpenAPIDataModel.List
                  |> Some }

            do! state.SetState(fun l -> endpoint :: l)

            let endpoint =
              { Path = $"/entities/{entity_name.Name}/create"
                Method = OpenAPIEndpointModel.Post
                RequestModel =
                  Some(
                    OpenAPIDataModel.Record
                      [ ("id" |> ResolvedIdentifier.Create,
                         OpenAPIDataModel.Ref id_name)
                        ("value" |> ResolvedIdentifier.Create,
                         OpenAPIDataModel.Ref type_original_name) ]
                  )
                ResponseModel =
                  type_with_props_name
                  |> OpenAPIDataModel.Ref
                  |> OpenAPIDataModel.List
                  |> Some }

            do! state.SetState(fun l -> endpoint :: l)

            let endpoint =
              { Path = $"/entities/{entity_name.Name}/upsert"
                Method = OpenAPIEndpointModel.Post
                RequestModel =
                  Some(
                    OpenAPIDataModel.Tuple
                      [ OpenAPIDataModel.Ref id_name
                        OpenAPIDataModel.Ref type_original_name
                        OpenAPIDataModel.AnyObject ]
                  )
                ResponseModel =
                  type_with_props_name
                  |> OpenAPIDataModel.Ref
                  |> OpenAPIDataModel.List
                  |> Some }

            do! state.SetState(fun l -> endpoint :: l)

            let endpoint =
              { Path = $"/entities/{entity_name.Name}/update"
                Method = OpenAPIEndpointModel.Post
                RequestModel =
                  Some(
                    OpenAPIDataModel.Tuple
                      [ OpenAPIDataModel.Ref id_name
                        OpenAPIDataModel.AnyObject ]
                  )
                ResponseModel =
                  type_with_props_name
                  |> OpenAPIDataModel.Ref
                  |> OpenAPIDataModel.List
                  |> Some }

            do! state.SetState(fun l -> endpoint :: l)

            let endpoint =
              { Path = $"/entities/{entity_name.Name}/delete/{{id}}"
                Method = OpenAPIEndpointModel.Delete
                RequestModel = Some(OpenAPIDataModel.Ref id_name)
                ResponseModel = None }

            do! state.SetState(fun l -> endpoint :: l)

            let root_level_primitive_properties =
              entity_desc.Properties
              |> List.choose (fun p ->
                if p.Path |> List.isEmpty then
                  match p.ReturnType with
                  | TypeValue.Primitive { value = primitive_type } ->
                    Some(
                      p.PropertyName |> ResolvedIdentifier.FromLocalIdentifier,
                      OpenAPIDataModel.Primitive primitive_type
                    )
                  | TypeValue.Sum { value = sum_values } ->
                    match sum_values with
                    | [ TypeValue.Primitive { value = PrimitiveType.Unit }
                        TypeValue.Primitive { value = primitive_type } ] ->
                      Some(
                        p.PropertyName
                        |> ResolvedIdentifier.FromLocalIdentifier,
                        OpenAPIDataModel.Sum
                          [ OpenAPIDataModel.Primitive PrimitiveType.Unit
                            OpenAPIDataModel.Primitive primitive_type ]
                      )
                    | _ -> None
                  | _ -> None
                else
                  None)

            let vector_search_response_model =
              OpenAPIDataModel.Record(
                [ ("id" |> ResolvedIdentifier.Create,
                   OpenAPIDataModel.Ref id_name) ]
                @ root_level_primitive_properties
              )

            do!
              entity_desc.Vectors
              |> Seq.map (fun vector_desc ->
                state {
                  let endpoint =
                    { Path =
                        $"/entities/{entity_name.Name}/vectors/{vector_desc.VectorName.Name}/search"
                      Method = OpenAPIEndpointModel.Post
                      RequestModel =
                        Some(
                          OpenAPIDataModel.Record
                            [ ("query" |> ResolvedIdentifier.Create,
                               OpenAPIDataModel.Primitive PrimitiveType.String)
                              ("offset" |> ResolvedIdentifier.Create,
                               OpenAPIDataModel.Primitive PrimitiveType.Int32)
                              ("limit" |> ResolvedIdentifier.Create,
                               OpenAPIDataModel.Primitive PrimitiveType.Int32) ]
                        )
                      ResponseModel =
                        Some(
                          OpenAPIDataModel.List(vector_search_response_model)
                        ) }

                  do! state.SetState(fun l -> endpoint :: l)
                  return ()
                })
              |> state.All
              |> state.Ignore

            return ()
          })
        |> state.All
        |> state.Ignore

      do!
        schema.Relations
        |> OrderedMap.toSeq
        |> Seq.map (fun (relation_name, relation_desc) ->
          state {
            let from_id_name =
              { OpenAPIDataModelName.OpenAPIDataModelName =
                  sprintf "%s-Id" (relation_desc.From.ToString()) }

            let to_id_name =
              { OpenAPIDataModelName.OpenAPIDataModelName =
                  sprintf "%s-Id" (relation_desc.To.ToString()) }

            let from_with_props_name =
              { OpenAPIDataModelName.OpenAPIDataModelName =
                  sprintf "%s-WithProps" (relation_desc.From.ToString()) }

            let to_with_props_name =
              { OpenAPIDataModelName.OpenAPIDataModelName =
                  sprintf "%s-WithProps" (relation_desc.To.ToString()) }

            let link_request_model =
              OpenAPIDataModel.Record
                [ ("FromId" |> ResolvedIdentifier.Create,
                   OpenAPIDataModel.Ref from_id_name)
                  ("ToId" |> ResolvedIdentifier.Create,
                   OpenAPIDataModel.Ref to_id_name) ]

            let endpoint =
              { Path = sprintf "/relations/%s/link" relation_name.Name
                Method = OpenAPIEndpointModel.Post
                RequestModel = Some(link_request_model)
                ResponseModel = None }

            do! state.SetState(fun l -> endpoint :: l)

            let endpoint =
              { Path = sprintf "/relations/%s/unlink" relation_name.Name
                Method = OpenAPIEndpointModel.Post
                RequestModel = Some(link_request_model)
                ResponseModel = None }

            do! state.SetState(fun l -> endpoint :: l)

            let endpoint =
              { Path = sprintf "/relations/%s/isLinked" relation_name.Name
                Method = OpenAPIEndpointModel.Post
                RequestModel = Some(link_request_model)
                ResponseModel =
                  Some(OpenAPIDataModel.Primitive PrimitiveType.Bool) }

            do! state.SetState(fun l -> endpoint :: l)

            let cardinality =
              relation_desc.Cardinality
              |> Option.defaultValue
                { SchemaRelationCardinality.From = Cardinality.Many
                  To = Cardinality.Many }

            let add_lookup_endpoint_from target_cardinality =
              state {
                let lookup_path, request_model, response_model =
                  match target_cardinality with
                  | Cardinality.One ->
                    (sprintf "/relations/%s/lookupOne/From" relation_name.Name,
                     OpenAPIDataModel.Record
                       [ ("FromId" |> ResolvedIdentifier.Create,
                          OpenAPIDataModel.Ref from_id_name) ],
                     OpenAPIDataModel.Tuple
                       [ OpenAPIDataModel.Ref to_id_name
                         OpenAPIDataModel.Ref to_with_props_name ])
                  | Cardinality.Zero ->
                    (sprintf
                      "/relations/%s/lookupOption/From"
                      relation_name.Name,
                     OpenAPIDataModel.Record
                       [ ("FromId" |> ResolvedIdentifier.Create,
                          OpenAPIDataModel.Ref from_id_name) ],
                     OpenAPIDataModel.Sum
                       [ OpenAPIDataModel.Primitive PrimitiveType.Unit
                         OpenAPIDataModel.Tuple
                           [ OpenAPIDataModel.Ref to_id_name
                             OpenAPIDataModel.Ref to_with_props_name ] ])
                  | Cardinality.Many ->
                    (sprintf "/relations/%s/lookupMany/From" relation_name.Name,
                     OpenAPIDataModel.Record
                       [ ("FromId" |> ResolvedIdentifier.Create,
                          OpenAPIDataModel.Ref from_id_name)
                         ("offset" |> ResolvedIdentifier.Create,
                          OpenAPIDataModel.Primitive PrimitiveType.Int32)
                         ("limit" |> ResolvedIdentifier.Create,
                          OpenAPIDataModel.Primitive PrimitiveType.Int32) ],
                     OpenAPIDataModel.List(
                       OpenAPIDataModel.Tuple
                         [ OpenAPIDataModel.Ref to_id_name
                           OpenAPIDataModel.Ref to_with_props_name ]
                     ))

                let endpoint =
                  { Path = lookup_path
                    Method = OpenAPIEndpointModel.Get
                    RequestModel = Some request_model
                    ResponseModel = Some response_model }

                do! state.SetState(fun l -> endpoint :: l)
                return ()
              }

            let add_lookup_endpoint_to target_cardinality =
              state {
                let lookup_path, request_model, response_model =
                  match target_cardinality with
                  | Cardinality.One ->
                    (sprintf "/relations/%s/lookupOne/To" relation_name.Name,
                     OpenAPIDataModel.Record
                       [ ("ToId" |> ResolvedIdentifier.Create,
                          OpenAPIDataModel.Ref to_id_name) ],
                     OpenAPIDataModel.Tuple
                       [ OpenAPIDataModel.Ref from_id_name
                         OpenAPIDataModel.Ref from_with_props_name ])
                  | Cardinality.Zero ->
                    (sprintf "/relations/%s/lookupOption/To" relation_name.Name,
                     OpenAPIDataModel.Record
                       [ ("ToId" |> ResolvedIdentifier.Create,
                          OpenAPIDataModel.Ref to_id_name) ],
                     OpenAPIDataModel.Sum
                       [ OpenAPIDataModel.Primitive PrimitiveType.Unit
                         OpenAPIDataModel.Tuple
                           [ OpenAPIDataModel.Ref from_id_name
                             OpenAPIDataModel.Ref from_with_props_name ] ])
                  | Cardinality.Many ->
                    (sprintf "/relations/%s/lookupMany/To" relation_name.Name,
                     OpenAPIDataModel.Record
                       [ ("ToId" |> ResolvedIdentifier.Create,
                          OpenAPIDataModel.Ref to_id_name)
                         ("offset" |> ResolvedIdentifier.Create,
                          OpenAPIDataModel.Primitive PrimitiveType.Int32)
                         ("limit" |> ResolvedIdentifier.Create,
                          OpenAPIDataModel.Primitive PrimitiveType.Int32) ],
                     OpenAPIDataModel.List(
                       OpenAPIDataModel.Tuple
                         [ OpenAPIDataModel.Ref from_id_name
                           OpenAPIDataModel.Ref from_with_props_name ]
                     ))

                let endpoint =
                  { Path = lookup_path
                    Method = OpenAPIEndpointModel.Get
                    RequestModel = Some request_model
                    ResponseModel = Some response_model }

                do! state.SetState(fun l -> endpoint :: l)
                return ()
              }

            do! add_lookup_endpoint_from cardinality.To
            do! add_lookup_endpoint_to cardinality.From
            return ()
          })
        |> state.All
        |> state.Ignore
    }

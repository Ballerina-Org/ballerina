namespace Ballerina.DSL.Next

module YamlGeneration =
  open OpenAPIModel
  open System
  open Ballerina.DSL.Next.Types
  let private yaml_escape (value: string) = value.Replace("'", "''")

  let private yaml_string (value: string) = $"'{yaml_escape value}'"

  let private indent level = String.replicate (level * 2) " "

  let private compare_ordinal (a: string) (b: string) = String.CompareOrdinal(a, b)


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
    @ [ $"{indent level}required:"; $"{indent (level + 1)}- {yaml_string case_name}" ]

  let private render_primitive_schema_lines level primitive_type =
    match primitive_type with
    | PrimitiveType.Unit ->
      [
        $"{indent level}type: object"
        $"{indent level}maxProperties: 0"
      ]
    | _ ->
      let primitive_case_name =
        match primitive_type with
        | PrimitiveType.Unit -> ""
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
        | PrimitiveType.Unit -> []
        | PrimitiveType.Guid -> [ $"{indent (level + 2)}type: string"; $"{indent (level + 2)}format: uuid" ]
        | PrimitiveType.Int32 -> [ $"{indent (level + 2)}type: integer"; $"{indent (level + 2)}format: int32" ]
        | PrimitiveType.Int64 -> [ $"{indent (level + 2)}type: integer"; $"{indent (level + 2)}format: int64" ]
        | PrimitiveType.Float32 -> [ $"{indent (level + 2)}type: number"; $"{indent (level + 2)}format: float" ]
        | PrimitiveType.Float64 -> [ $"{indent (level + 2)}type: number"; $"{indent (level + 2)}format: double" ]
        | PrimitiveType.Decimal -> [ $"{indent (level + 2)}type: number" ]
        | PrimitiveType.Bool -> [ $"{indent (level + 2)}type: boolean" ]
        | PrimitiveType.String -> [ $"{indent (level + 2)}type: string" ]
        | PrimitiveType.DateTime ->
          [ $"{indent (level + 2)}type: string"
            $"{indent (level + 2)}format: date-time" ]
        | PrimitiveType.DateOnly -> [ $"{indent (level + 2)}type: string"; $"{indent (level + 2)}format: date" ]
        | PrimitiveType.TimeSpan -> [ $"{indent (level + 2)}type: string"; $"{indent (level + 2)}format: duration" ]
        | PrimitiveType.Vector ->
          [ $"{indent (level + 2)}type: array"
            $"{indent (level + 2)}items:"
            $"{indent (level + 3)}type: number"
            $"{indent (level + 3)}format: float" ]

      wrap_case_in_property level primitive_case_name primitive_schema_lines

  let private render_parameter_primitive_schema_lines level primitive_type =
    match primitive_type with
    | PrimitiveType.Unit -> [ $"{indent level}type: 'null'" ]
    | PrimitiveType.Guid -> [ $"{indent level}type: string"; $"{indent level}format: uuid" ]
    | PrimitiveType.Int32 -> [ $"{indent level}type: integer"; $"{indent level}format: int32" ]
    | PrimitiveType.Int64 -> [ $"{indent level}type: integer"; $"{indent level}format: int64" ]
    | PrimitiveType.Float32 -> [ $"{indent level}type: number"; $"{indent level}format: float" ]
    | PrimitiveType.Float64 -> [ $"{indent level}type: number"; $"{indent level}format: double" ]
    | PrimitiveType.Decimal -> [ $"{indent level}type: number" ]
    | PrimitiveType.Bool -> [ $"{indent level}type: boolean" ]
    | PrimitiveType.String -> [ $"{indent level}type: string" ]
    | PrimitiveType.DateTime -> [ $"{indent level}type: string"; $"{indent level}format: date-time" ]
    | PrimitiveType.DateOnly -> [ $"{indent level}type: string"; $"{indent level}format: date" ]
    | PrimitiveType.TimeSpan -> [ $"{indent level}type: string"; $"{indent level}format: duration" ]
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
            let name = path.Substring(start_index + 1, end_index - start_index - 1)
            loop (end_index + 1) (name :: acc)

    loop 0 []

  let rec private get_record_schema_lines fields level =
    let sorted_fields =
        fields
        |> List.sortWith (fun (left_name, _) (right_name, _) ->
          compare_ordinal (left_name |> resolved_identifier_to_string) (right_name |> resolved_identifier_to_string))

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
      [ $"{indent (level + 2)}type: object"; $"{indent (level + 2)}properties:" ]
      @ (if List.isEmpty fields then
            [ $"{indent (level + 3)}{{}}" ]
          else
            rendered_properties)
      @ (if List.isEmpty fields then
            []
          else
            [ $"{indent (level + 2)}required:" ] @ required_fields)
      @ [ $"{indent (level + 2)}additionalProperties: false" ]

    record_schema_lines

  and private render_schema_lines level =
    function
    | Ref model_name ->
      let schema_ref = $"#/components/schemas/{model_name.OpenAPIDataModelName}"
      [ $"{indent level}$ref: {yaml_string schema_ref}" ]
    | OpenAPIDataModel.Primitive primitive_type ->
        // [
        //   $"{indent level}type: object"
        //   $"{indent level}properties:"
        //   $"{indent (level + 1)}'Primitive':"
        // ] @
        render_primitive_schema_lines (level + 2) primitive_type
        // [
        //   $"{indent level}additionalProperties: false"
        //   $"{indent level}required:"
        //   $"{indent (level + 1)}- 'Primitive'"
        // ]
    | AnyObject -> [ $"{indent level}type: object"; $"{indent level}additionalProperties: true" ]
    | List item_type ->
      let list_schema_lines =
        [ $"{indent (level + 2)}type: array"; $"{indent (level + 2)}items:" ]
        @ (render_schema_lines (level + 3) item_type)

      (wrap_case_in_property level "List" list_schema_lines)
      @ [ $"{indent level}additionalProperties: false" ]
    | OpenAPIDataModel.Tuple elements ->
      let prefix_items =
        elements
        |> List.collect (fun element -> [ $"{indent (level + 3)}-" ] @ (render_schema_lines (level + 4) element))

      let tuple_schema_lines =
        [ $"{indent (level + 2)}type: array"; $"{indent (level + 2)}prefixItems:" ]
        @ prefix_items
        @ [ $"{indent (level + 2)}minItems: {elements.Length}"
            $"{indent (level + 2)}maxItems: {elements.Length}" ]

      (wrap_case_in_property level "Tuple" tuple_schema_lines)
      @ [ $"{indent level}additionalProperties: false" ]
    | OpenAPIDataModel.Sum options ->
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
    | OpenAPIDataModel.Record fields ->
      let record_schema_lines = get_record_schema_lines fields level
      wrap_case_in_property level "Record" record_schema_lines
    | Object fields -> get_record_schema_lines fields level
    | OpenAPIDataModel.Union cases ->
      let sorted_cases =
        cases
        |> List.sortWith (fun (left_name, _) (right_name, _) ->
          compare_ordinal (left_name |> resolved_identifier_to_string) (right_name |> resolved_identifier_to_string))

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
      sprintf "%s %s" ((openapi_method_to_string endpoint.Method).ToUpperInvariant()) endpoint.Path

    let operation_id =
      let sanitized_path =
        endpoint.Path.Replace("/", "_").Replace("{", "").Replace("}", "").Trim('_')

      sprintf "%s_%s" (openapi_method_to_string endpoint.Method) sanitized_path

    let path_parameters_lines =
      match endpoint.Method, path_parameter_names, endpoint.RequestModel with
      | Get, [ parameter_name ], Some(OpenAPIDataModel.Primitive primitive_type) ->
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
      | Delete, [ parameter_name ], Some(OpenAPIDataModel.Primitive primitive_type) ->
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
      let sorted_query_params =
        endpoint.QueryParameters
        |> List.sortBy (fun p -> p.Name)

      let rendered_query_parameters =
        sorted_query_params
        |> List.collect (
            fun p ->
              $"{indent (level + 3)}- name: {p.Name}\n" +
              $"{indent (level + 4)}in: query\n" +
              $"{indent (level + 4)}required: true\n" +
              $"{indent (level + 4)}schema:" ::
              (render_parameter_primitive_schema_lines (level + 5) p.Type)
        )

      if List.isEmpty rendered_query_parameters then
        []
      else
        $"{indent (level + 2)}parameters:" :: rendered_query_parameters
      // match endpoint.Method, path_parameter_names, endpoint.RequestModel with
      // | Get, [], Some(Record fields) ->
      //   let sorted_fields =
      //     fields
      //     |> List.sortWith (fun (left_name, _) (right_name, _) ->
      //       compare_ordinal (left_name |> resolved_identifier_to_string) (right_name |> resolved_identifier_to_string))

      //   let rendered_query_parameters =
      //     sorted_fields
      //     |> List.collect (fun (field_name, field_model) ->
      //       match field_model with
      //       | Primitive primitive_type ->
      //         [ $"{indent (level + 3)}- name: {field_name.Name}"
      //           $"{indent (level + 4)}in: query"
      //           $"{indent (level + 4)}required: true"
      //           $"{indent (level + 4)}schema:" ]
      //         @ (render_parameter_primitive_schema_lines (level + 5) primitive_type)
      //       | Ref model_name ->
      //         let schema_ref = sprintf "#/components/schemas/%s" model_name.OpenAPIDataModelName

      //         [ $"{indent (level + 3)}- name: {field_name.Name}"
      //           $"{indent (level + 4)}in: query"
      //           $"{indent (level + 4)}required: true"
      //           $"{indent (level + 4)}schema:"
      //           $"{indent (level + 5)}$ref: {yaml_string schema_ref}" ]
      //       | _ -> [])

      //   if List.isEmpty rendered_query_parameters then
      //     []
      //   else
      //     [ $"{indent (level + 2)}parameters:" ] @ rendered_query_parameters
      // | _ -> []

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
          compare_ordinal (left.Method |> openapi_method_to_string) (right.Method |> openapi_method_to_string))

    let paths_lines = sorted_endpoints |> List.collect (render_endpoint_lines 1)

    let components_lines =
      spec.DataModels
      |> Map.toList
      |> List.sortWith (fun (left_name, _) (right_name, _) ->
        compare_ordinal left_name.OpenAPIDataModelName right_name.OpenAPIDataModelName)
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
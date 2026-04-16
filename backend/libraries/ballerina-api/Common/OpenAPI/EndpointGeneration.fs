namespace Ballerina.DSL.Next

module EndpointGeneration =
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.StdLib.Extensions
  open Ballerina.State.WithError
  open Ballerina.DSL.Next.Types.TypeChecker
  open OpenAPIModel
  open Ballerina.Errors
  open Ballerina.Cat.Collections.OrderedMap

  let primitiveTypeFilterName (primitiveType: PrimitiveType) =
    match primitiveType with
    | PrimitiveType.Int32 -> "Int32Filter"
    | PrimitiveType.Int64 -> "Int64Filter"
    | PrimitiveType.Float32 -> "Float32Filter"
    | PrimitiveType.Float64 -> "Float64Filter"
    | PrimitiveType.Decimal -> "DecimalFilter"
    | PrimitiveType.Bool -> "BoolFilter"
    | PrimitiveType.String -> "StringFilter"
    | PrimitiveType.Guid -> "GuidFilter"
    | PrimitiveType.DateOnly -> "DateOnlyFilter"
    | PrimitiveType.DateTime -> "DateTimeFilter"
    | PrimitiveType.TimeSpan -> "TimeSpanFilter"
    | PrimitiveType.Unit -> "UnitFilter"
    | PrimitiveType.Vector -> "VectorFilter"

  let private isComparablePrimitive primitiveType =
    match primitiveType with
    | PrimitiveType.Int32
    | PrimitiveType.Int64
    | PrimitiveType.Float32
    | PrimitiveType.Float64
    | PrimitiveType.Decimal
    | PrimitiveType.DateOnly
    | PrimitiveType.DateTime
    | PrimitiveType.TimeSpan
    | PrimitiveType.String -> true
    | _ -> false

  let private isStringType primitiveType =
    primitiveType = PrimitiveType.String

  let buildPrimitiveFilterModel (primitiveType: PrimitiveType) : OpenAPIDataModel =
    let valueModel = OpenAPIDataModel.Scalar primitiveType

    let eqCases =
      [ ("Eq" |> ResolvedIdentifier.Create, valueModel)
        ("NotEq" |> ResolvedIdentifier.Create, valueModel) ]

    let comparableCases =
      if isComparablePrimitive primitiveType then
        [ ("Gt" |> ResolvedIdentifier.Create, valueModel)
          ("Gte" |> ResolvedIdentifier.Create, valueModel)
          ("Lt" |> ResolvedIdentifier.Create, valueModel)
          ("Lte" |> ResolvedIdentifier.Create, valueModel) ]
      else
        []

    let stringCases =
      if isStringType primitiveType then
        [ ("Contains" |> ResolvedIdentifier.Create, valueModel)
          ("StartsWith" |> ResolvedIdentifier.Create, valueModel)
          ("EndsWith" |> ResolvedIdentifier.Create, valueModel) ]
      else
        []

    let nullCases =
      [ ("IsNull" |> ResolvedIdentifier.Create, OpenAPIDataModel.Scalar PrimitiveType.Bool)
        ("IsNotNull" |> ResolvedIdentifier.Create, OpenAPIDataModel.Scalar PrimitiveType.Bool) ]

    OpenAPIDataModel.OneOf(eqCases @ comparableCases @ stringCases @ nullCases)

  let collectFilterableProperties
    (entity_desc: SchemaEntity<ValueExt<'runtimeContext, 'db, 'customExtension>>)
    =
    entity_desc.Properties
    |> List.choose (fun p ->
      if p.Path |> List.isEmpty then
        match p.ReturnType with
        | TypeValue.Primitive { value = primitive_type } ->
          Some(p.PropertyName, primitive_type, false)
        | TypeValue.Sum { value = sum_values } ->
          match sum_values with
          | [ TypeValue.Primitive { value = PrimitiveType.Unit }
              TypeValue.Primitive { value = primitive_type } ] ->
            Some(p.PropertyName, primitive_type, true)
          | _ -> None
        | _ -> None
      else
        None)

  type RelationDirection = FromTo | ToFrom

  let collectFilterableRelations
    (entityName: SchemaEntityName)
    (schema: Schema<ValueExt<'runtimeContext, 'db, 'customExtension>>)
    =
    let entityNameStr = entityName.Name

    schema.Relations
    |> OrderedMap.toSeq
    |> Seq.choose (fun (relationName, relation) ->
      let fromName =
        match relation.From with
        | Identifier.LocalScope name -> name
        | Identifier.FullyQualified(_, name) -> name

      let toName =
        match relation.To with
        | Identifier.LocalScope name -> name
        | Identifier.FullyQualified(_, name) -> name

      if fromName = entityNameStr then
        Some(relationName.Name, toName, FromTo)
      elif toName = entityNameStr then
        Some(relationName.Name, fromName, ToFrom)
      else
        None)
    |> Seq.toList

  let generate_endpoints
    (tenantId: string)
    (schemaName: string)
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
      let draftQueryParam =
        { Name = "draft" |> ResolvedIdentifier.Create
          Type = PrimitiveType.Bool }

      let routePrefix = $"/{tenantId}/{schemaName}"

      do!
        schema.Entities
        |> OrderedMap.toSeq
        |> Seq.map (fun (entity_name, entity_desc) ->
          state {
            let type_with_props_name =
              { OpenAPIDataModelName.OpenAPIDataModelName = $"{entity_name.Name}-WithProps" }

            let type_original_name =
              { OpenAPIDataModelName.OpenAPIDataModelName = $"{entity_name.Name}-Original" }

            let id_name =
              { OpenAPIDataModelName.OpenAPIDataModelName = $"{entity_name.Name}-Id" }
            // let! _, id_primitive = entity_desc.Id |> TypeValue.AsPrimaryKey |> state.OfSum

            let _delta_with_props_name =
              { OpenAPIDataModelName.OpenAPIDataModelName = $"{entity_name.Name}-Delta-WithProps" }

            let delta_original_name =
              { OpenAPIDataModelName.OpenAPIDataModelName = $"{entity_name.Name}-Delta-Original" }

            let endpoint =
              { Path = $"{routePrefix}/{entity_name.Name}/get-by-id"
                Method = OpenAPIEndpointModel.Post
                QueryParameters = [ draftQueryParam ]
                RequestModel = Some(OpenAPIDataModel.Ref id_name)
                ResponseModel = Some(OpenAPIDataModel.Ref type_with_props_name) }

            do! state.SetState(fun l -> endpoint :: l)

            let endpoint =
              { Path = $"{routePrefix}/{entity_name.Name}/many"
                Method = OpenAPIEndpointModel.Get
                QueryParameters =
                  [ draftQueryParam
                    { Name = "offset" |> ResolvedIdentifier.Create
                      Type = PrimitiveType.Int32 }
                    { Name = "limit" |> ResolvedIdentifier.Create
                      Type = PrimitiveType.Int32 } ]
                RequestModel = None
                ResponseModel =
                  OpenAPIDataModel.Tuple
                    [ id_name |> OpenAPIDataModel.Ref
                      type_with_props_name |> OpenAPIDataModel.Ref ]
                  |> listToOpenApi
                  |> Some }

            do! state.SetState(fun l -> endpoint :: l)

            let endpoint =
              { Path = $"{routePrefix}/{entity_name.Name}/create"
                Method = OpenAPIEndpointModel.Post
                QueryParameters = [ draftQueryParam ]
                RequestModel =
                  Some(
                    OpenAPIDataModel.Object
                      [ ("id" |> ResolvedIdentifier.Create, OpenAPIDataModel.Ref id_name)
                        ("value" |> ResolvedIdentifier.Create, OpenAPIDataModel.Ref type_original_name) ]
                  )
                ResponseModel =
                  [ OpenAPIDataModel.Primitive PrimitiveType.Unit
                    type_with_props_name |> OpenAPIDataModel.Ref ]
                  |> OpenAPIDataModel.Sum
                  |> Some }

            do! state.SetState(fun l -> endpoint :: l)

            let endpoint =
              { Path = $"{routePrefix}/{entity_name.Name}/upsert"
                Method = OpenAPIEndpointModel.Post
                QueryParameters = [ draftQueryParam ]
                RequestModel =
                  Some(
                    OpenAPIDataModel.Object
                      [ "id" |> ResolvedIdentifier.Create, OpenAPIDataModel.Ref id_name
                        "entity" |> ResolvedIdentifier.Create, OpenAPIDataModel.Ref type_original_name
                        "delta" |> ResolvedIdentifier.Create, OpenAPIDataModel.Ref delta_original_name ]
                  )
                ResponseModel =
                  [ OpenAPIDataModel.Primitive PrimitiveType.Unit
                    type_with_props_name |> OpenAPIDataModel.Ref ]
                  |> OpenAPIDataModel.Sum
                  |> Some }

            do! state.SetState(fun l -> endpoint :: l)

            let endpoint =
              { Path = $"{routePrefix}/{entity_name.Name}/update"
                Method = OpenAPIEndpointModel.Post
                QueryParameters = [ draftQueryParam ]
                RequestModel =
                  Some(
                    OpenAPIDataModel.Object
                      [ "id" |> ResolvedIdentifier.Create, OpenAPIDataModel.Ref id_name
                        "delta" |> ResolvedIdentifier.Create, OpenAPIDataModel.Ref delta_original_name ]
                  )
                ResponseModel =
                  [ OpenAPIDataModel.Primitive PrimitiveType.Unit
                    type_with_props_name |> OpenAPIDataModel.Ref ]
                  |> OpenAPIDataModel.Sum
                  |> Some }

            do! state.SetState(fun l -> endpoint :: l)

            let endpoint =
              { Path = $"{routePrefix}/{entity_name.Name}/delete"
                QueryParameters = [ draftQueryParam ]
                Method = OpenAPIEndpointModel.Post
                RequestModel = Some(OpenAPIDataModel.Ref id_name)
                ResponseModel = Some(primitiveToOpenAPI PrimitiveType.Bool) }

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
                        p.PropertyName |> ResolvedIdentifier.FromLocalIdentifier,
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
                [ ("id" |> ResolvedIdentifier.Create, OpenAPIDataModel.Ref id_name) ]
                @ root_level_primitive_properties
              )

            do!
              entity_desc.Vectors
              |> Seq.map (fun vector_desc ->
                state {
                  let endpoint =
                    { Path = $"{routePrefix}/{entity_name.Name}/vectors/{vector_desc.VectorName.Name}/search"
                      Method = OpenAPIEndpointModel.Post
                      QueryParameters = []
                      RequestModel =
                        Some(
                          OpenAPIDataModel.Record
                            [ ("query" |> ResolvedIdentifier.Create, OpenAPIDataModel.Primitive PrimitiveType.String)
                              ("offset" |> ResolvedIdentifier.Create, OpenAPIDataModel.Primitive PrimitiveType.Int32)
                              ("limit" |> ResolvedIdentifier.Create, OpenAPIDataModel.Primitive PrimitiveType.Int32) ]
                        )
                      ResponseModel = Some(OpenAPIDataModel.List(vector_search_response_model)) }

                  do! state.SetState(fun l -> endpoint :: l)
                  return ()
                })
              |> state.All
              |> state.Ignore

            let filterableProperties = collectFilterableProperties entity_desc

            if not (List.isEmpty filterableProperties) then
              let filterEndpoint =
                { Path = $"{routePrefix}/{entity_name.Name}/filter"
                  Method = OpenAPIEndpointModel.Post
                  QueryParameters =
                    [ draftQueryParam
                      { Name = "offset" |> ResolvedIdentifier.Create
                        Type = PrimitiveType.Int32 }
                      { Name = "limit" |> ResolvedIdentifier.Create
                        Type = PrimitiveType.Int32 } ]
                  RequestModel =
                    Some(
                      OpenAPIDataModel.Ref
                        { OpenAPIDataModelName.OpenAPIDataModelName = $"{entity_name.Name}-FilterTree" }
                    )
                  ResponseModel =
                    OpenAPIDataModel.Tuple
                      [ id_name |> OpenAPIDataModel.Ref
                        type_with_props_name |> OpenAPIDataModel.Ref ]
                    |> listToOpenApi
                    |> Some }

              do! state.SetState(fun l -> filterEndpoint :: l)

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
              { OpenAPIDataModelName.OpenAPIDataModelName = sprintf "%s-Id" (relation_desc.From.ToString()) }

            let to_id_name =
              { OpenAPIDataModelName.OpenAPIDataModelName = sprintf "%s-Id" (relation_desc.To.ToString()) }

            let from_with_props_name =
              { OpenAPIDataModelName.OpenAPIDataModelName = sprintf "%s-WithProps" (relation_desc.From.ToString()) }

            let to_with_props_name =
              { OpenAPIDataModelName.OpenAPIDataModelName = sprintf "%s-WithProps" (relation_desc.To.ToString()) }

            let link_request_model =
              OpenAPIDataModel.Object
                [ ("FromId" |> ResolvedIdentifier.Create, OpenAPIDataModel.Ref from_id_name)
                  ("ToId" |> ResolvedIdentifier.Create, OpenAPIDataModel.Ref to_id_name) ]

            let endpoint =
              { Path = sprintf "%s/%s/link" routePrefix relation_name.Name
                QueryParameters = [ draftQueryParam ]
                Method = OpenAPIEndpointModel.Post
                RequestModel = Some(link_request_model)
                ResponseModel =
                  Some(
                    OpenAPIDataModel.Sum[OpenAPIDataModel.Primitive PrimitiveType.Unit
                                         OpenAPIDataModel.Primitive PrimitiveType.Unit]
                  ) }

            do! state.SetState(fun l -> endpoint :: l)

            let endpoint =
              { Path = sprintf "%s/%s/unlink" routePrefix relation_name.Name
                QueryParameters = [ draftQueryParam ]
                Method = OpenAPIEndpointModel.Post
                RequestModel = Some(link_request_model)
                ResponseModel =
                  Some(
                    OpenAPIDataModel.Sum[OpenAPIDataModel.Primitive PrimitiveType.Unit
                                         OpenAPIDataModel.Primitive PrimitiveType.Unit]
                  ) }

            do! state.SetState(fun l -> endpoint :: l)

            let endpoint =
              { Path = sprintf "%s/%s/is-linked" routePrefix relation_name.Name
                QueryParameters = [ draftQueryParam ]
                Method = OpenAPIEndpointModel.Post
                RequestModel = Some(link_request_model)
                ResponseModel = Some(primitiveToOpenAPI PrimitiveType.Bool) }

            do! state.SetState(fun l -> endpoint :: l)

            let cardinality =
              relation_desc.Cardinality
              |> Option.defaultValue
                { SchemaRelationCardinality.From = Cardinality.Many
                  To = Cardinality.Many }

            let add_lookup_endpoint_from target_cardinality =
              state {
                let lookup_path, request_model, response_model, query_params =
                  match target_cardinality with
                  | Cardinality.One ->
                    sprintf "%s/%s/lookup-one/From" routePrefix relation_name.Name,
                    OpenAPIDataModel.Ref from_id_name,
                    OpenAPIDataModel.Tuple [ OpenAPIDataModel.Ref to_id_name; OpenAPIDataModel.Ref to_with_props_name ],
                    []
                  | Cardinality.Zero ->
                    sprintf "%s/%s/lookup-option/From" routePrefix relation_name.Name,
                    OpenAPIDataModel.Ref from_id_name,
                    OpenAPIDataModel.Sum
                      [ OpenAPIDataModel.Primitive PrimitiveType.Unit
                        OpenAPIDataModel.Sum[OpenAPIDataModel.Primitive PrimitiveType.Unit

                                             OpenAPIDataModel.Tuple
                                               [ OpenAPIDataModel.Ref to_id_name
                                                 OpenAPIDataModel.Ref to_with_props_name ]] ],
                    []
                  | Cardinality.Many ->
                    sprintf "%s/%s/lookup-many/From" routePrefix relation_name.Name,
                    OpenAPIDataModel.Ref to_id_name,
                    (OpenAPIDataModel.Tuple
                      [ OpenAPIDataModel.Ref to_id_name; OpenAPIDataModel.Ref to_with_props_name ])
                    |> listToOpenApi,
                    [ { Name = "offset" |> ResolvedIdentifier.Create
                        Type = PrimitiveType.Int32 }
                      { Name = "limit" |> ResolvedIdentifier.Create
                        Type = PrimitiveType.Int32 } ]

                let endpoint =
                  { Path = lookup_path
                    Method = OpenAPIEndpointModel.Post
                    QueryParameters = draftQueryParam :: query_params
                    RequestModel = Some request_model
                    ResponseModel = Some response_model }

                do! state.SetState(fun l -> endpoint :: l)
                return ()
              }

            let add_lookup_endpoint_to target_cardinality =
              state {
                let lookup_path, request_model, response_model, query_params =
                  match target_cardinality with
                  | Cardinality.One ->
                    (sprintf "%s/%s/lookup-one/To" routePrefix relation_name.Name,
                     OpenAPIDataModel.Ref to_id_name,
                     OpenAPIDataModel.Tuple
                       [ OpenAPIDataModel.Ref from_id_name; OpenAPIDataModel.Ref from_with_props_name ],
                     [])
                  | Cardinality.Zero ->
                    (sprintf "%s/%s/lookup-option/To" routePrefix relation_name.Name,
                     OpenAPIDataModel.Ref to_id_name,
                     OpenAPIDataModel.Sum
                       [ OpenAPIDataModel.Primitive PrimitiveType.Unit
                         OpenAPIDataModel.Sum[OpenAPIDataModel.Primitive PrimitiveType.Unit

                                              OpenAPIDataModel.Tuple
                                                [ OpenAPIDataModel.Ref to_id_name
                                                  OpenAPIDataModel.Ref to_with_props_name ]] ],
                     [])
                  | Cardinality.Many ->
                    (sprintf "%s/%s/lookup-many/To" routePrefix relation_name.Name,
                     OpenAPIDataModel.Ref to_id_name,
                     OpenAPIDataModel.Tuple
                       [ OpenAPIDataModel.Ref from_id_name; OpenAPIDataModel.Ref from_with_props_name ]
                     |> listToOpenApi,
                     [ { Name = "offset" |> ResolvedIdentifier.Create
                         Type = PrimitiveType.Int32 }
                       { Name = "limit" |> ResolvedIdentifier.Create
                         Type = PrimitiveType.Int32 } ])

                let endpoint =
                  { Path = lookup_path
                    Method = OpenAPIEndpointModel.Post
                    QueryParameters = draftQueryParam :: query_params
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

namespace Ballerina.DSL.Next

module EndpointGeneration =
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.StdLib.Extensions
  open Ballerina.State.WithError
  open Ballerina.DSL.Next.Types.TypeChecker
  open OpenAPIModel
  open Ballerina.Errors
  open Ballerina.Cat.Collections.OrderedMap
  let generate_endpoints
    (tenantId : string)
    (schemaName : string)
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
      let draftQueryParam = { Name = "draft" |> ResolvedIdentifier.Create; Type = PrimitiveType.Bool  }
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
                QueryParameters = [
                  draftQueryParam
                  { Name = "offset" |> ResolvedIdentifier.Create; Type = PrimitiveType.Int32  }
                  { Name = "limit" |> ResolvedIdentifier.Create; Type = PrimitiveType.Int32 }
                ]
                RequestModel = None
                ResponseModel = 
                   OpenAPIDataModel.Tuple [
                      id_name |> OpenAPIDataModel.Ref
                      type_with_props_name |> OpenAPIDataModel.Ref
                   ] |> listToOpenApi |> Some }

            do! state.SetState(fun l -> endpoint :: l)

            let endpoint =
              { Path = $"{routePrefix}/{entity_name.Name}/create"
                Method = OpenAPIEndpointModel.Post
                QueryParameters = [draftQueryParam]
                RequestModel =
                  Some(
                    OpenAPIDataModel.Object
                      [ ("id" |> ResolvedIdentifier.Create, OpenAPIDataModel.Ref id_name)
                        ("value" |> ResolvedIdentifier.Create, OpenAPIDataModel.Ref type_original_name) ]
                  )
                ResponseModel = [
                  OpenAPIDataModel.Primitive PrimitiveType.Unit
                  type_with_props_name |> OpenAPIDataModel.Ref
                ] |> OpenAPIDataModel.Sum |> Some }

            do! state.SetState(fun l -> endpoint :: l)

            let endpoint =
              { Path = $"{routePrefix}/{entity_name.Name}/upsert"
                Method = OpenAPIEndpointModel.Post
                QueryParameters = [draftQueryParam ]
                RequestModel =
                  Some(
                    OpenAPIDataModel.Tuple
                      [ OpenAPIDataModel.Ref id_name
                        OpenAPIDataModel.Ref type_original_name
                        OpenAPIDataModel.AnyObject ]
                  )
                ResponseModel = type_with_props_name |> OpenAPIDataModel.Ref |> OpenAPIDataModel.List |> Some }

            do! state.SetState(fun l -> endpoint :: l)

            let endpoint =
              { Path = $"{routePrefix}/{entity_name.Name}/update"
                Method = OpenAPIEndpointModel.Post
                QueryParameters = []
                RequestModel =
                  Some(OpenAPIDataModel.Tuple [ OpenAPIDataModel.Ref id_name; OpenAPIDataModel.AnyObject ])
                ResponseModel = type_with_props_name |> OpenAPIDataModel.Ref |> OpenAPIDataModel.List |> Some }

            do! state.SetState(fun l -> endpoint :: l)

            let endpoint =
              { Path = $"{routePrefix}/{entity_name.Name}/delete/{{id}}"
                QueryParameters = []
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
              OpenAPIDataModel.Record
                [ ("FromId" |> ResolvedIdentifier.Create, OpenAPIDataModel.Ref from_id_name)
                  ("ToId" |> ResolvedIdentifier.Create, OpenAPIDataModel.Ref to_id_name) ]

            let endpoint =
              { Path = sprintf "%s/relations/%s/link" routePrefix relation_name.Name
                QueryParameters = []
                Method = OpenAPIEndpointModel.Post
                RequestModel = Some(link_request_model)
                ResponseModel = None }

            do! state.SetState(fun l -> endpoint :: l)

            let endpoint =
              { Path = sprintf "%s/relations/%s/unlink" routePrefix relation_name.Name
                QueryParameters = []
                Method = OpenAPIEndpointModel.Post
                RequestModel = Some(link_request_model)
                ResponseModel = None }

            do! state.SetState(fun l -> endpoint :: l)

            let endpoint =
              { Path = sprintf "%s/relations/%s/isLinked" routePrefix relation_name.Name
                QueryParameters = []
                Method = OpenAPIEndpointModel.Post
                RequestModel = Some(link_request_model)
                ResponseModel = Some(OpenAPIDataModel.Primitive PrimitiveType.Bool) }

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
                    sprintf "%s/%s/lookupOne/From" routePrefix relation_name.Name,
                    OpenAPIDataModel.Record
                      [ ("FromId" |> ResolvedIdentifier.Create, OpenAPIDataModel.Ref from_id_name) ],
                    OpenAPIDataModel.Tuple
                      [ OpenAPIDataModel.Ref to_id_name; OpenAPIDataModel.Ref to_with_props_name ]
                  | Cardinality.Zero ->
                    sprintf "%s/%s/lookupOption/From" routePrefix relation_name.Name,
                    OpenAPIDataModel.Record
                      [ ("FromId" |> ResolvedIdentifier.Create, OpenAPIDataModel.Ref from_id_name) ],
                    OpenAPIDataModel.Sum
                      [ OpenAPIDataModel.Primitive PrimitiveType.Unit
                        OpenAPIDataModel.Tuple
                          [ OpenAPIDataModel.Ref to_id_name; OpenAPIDataModel.Ref to_with_props_name ] ]
                  | Cardinality.Many ->
                    sprintf "%s/%s/lookupMany/From" routePrefix relation_name.Name,
                    OpenAPIDataModel.Record
                      [ "FromId" |> ResolvedIdentifier.Create, OpenAPIDataModel.Ref from_id_name
                        "offset" |> ResolvedIdentifier.Create, OpenAPIDataModel.Primitive PrimitiveType.Int32
                        "limit" |> ResolvedIdentifier.Create, OpenAPIDataModel.Primitive PrimitiveType.Int32 ],
                    OpenAPIDataModel.List(
                      OpenAPIDataModel.Tuple
                        [ OpenAPIDataModel.Ref to_id_name; OpenAPIDataModel.Ref to_with_props_name ]
                    )

                let endpoint =
                  { Path = lookup_path
                    Method = OpenAPIEndpointModel.Get
                    QueryParameters = []
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
                    (sprintf "%s/%s/lookupOne/To" routePrefix relation_name.Name,
                     OpenAPIDataModel.Record [ ("ToId" |> ResolvedIdentifier.Create, OpenAPIDataModel.Ref to_id_name) ],
                     OpenAPIDataModel.Tuple
                       [ OpenAPIDataModel.Ref from_id_name; OpenAPIDataModel.Ref from_with_props_name ])
                  | Cardinality.Zero ->
                    (sprintf "%s/%s/lookupOption/To" routePrefix relation_name.Name,
                     OpenAPIDataModel.Record [ ("ToId" |> ResolvedIdentifier.Create, OpenAPIDataModel.Ref to_id_name) ],
                     OpenAPIDataModel.Sum
                       [ OpenAPIDataModel.Primitive PrimitiveType.Unit
                         OpenAPIDataModel.Tuple
                           [ OpenAPIDataModel.Ref from_id_name; OpenAPIDataModel.Ref from_with_props_name ] ])
                  | Cardinality.Many ->
                    (sprintf "%s/%s/lookupMany/To" routePrefix relation_name.Name,
                     OpenAPIDataModel.Record
                       [ ("ToId" |> ResolvedIdentifier.Create, OpenAPIDataModel.Ref to_id_name)
                         ("offset" |> ResolvedIdentifier.Create, OpenAPIDataModel.Primitive PrimitiveType.Int32)
                         ("limit" |> ResolvedIdentifier.Create, OpenAPIDataModel.Primitive PrimitiveType.Int32) ],
                     OpenAPIDataModel.List(
                       OpenAPIDataModel.Tuple
                         [ OpenAPIDataModel.Ref from_id_name; OpenAPIDataModel.Ref from_with_props_name ]
                     ))

                let endpoint =
                  { Path = lookup_path
                    Method = OpenAPIEndpointModel.Get
                    QueryParameters = []
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
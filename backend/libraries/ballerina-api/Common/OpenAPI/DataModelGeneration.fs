namespace Ballerina.DSL.Next

module DataModelGeneration =
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.StdLib.Extensions
  open Ballerina.State.WithError
  open Ballerina.DSL.Next.Types.TypeChecker
  open OpenAPIModel
  open Ballerina.Errors
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
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
      (properties: List<SchemaEntityProperty<ValueExt<'runtimeContext, 'db, 'customExtension>>>)
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
        | TypeExprSourceMapping.OriginExprTypeLet(ExprTypeLetBindingName let_id, _) ->
          let type_value =
            TypeValue.SetSourceMapping(type_value, TypeExprSourceMapping.NoSourceMapping "")

          let! type_value = generate_data_model type_value properties_at_next_level
          let type_name = { OpenAPIDataModelName = let_id }
          do! state.SetState(Map.add type_name type_value)

          return OpenAPIDataModel.Ref type_name
        | TypeExprSourceMapping.OriginTypeExpr(_)
        | TypeExprSourceMapping.NoSourceMapping(_) ->

          match type_value with
          | TypeValue.Primitive { value = p } ->
              return OpenAPIDataModel.Object
                [
                  "Primitive" |> ResolvedIdentifier.Create, OpenAPIDataModel.Primitive p
                ]
          | TypeValue.Record { value = fields } ->
            let! fields =
              fields
              |> OrderedMap.toSeq
              |> Seq.map (fun (name, (field_type, _)) ->
                state {
                  let! name =
                    state.Either
                      (properties_at_this_level
                       |> Map.tryFind (name.Name.LocalName |> LocalIdentifier.Create)
                       |> Option.map (fun prop -> prop.PropertyName |> ResolvedIdentifier.FromLocalIdentifier)
                       |> sum.OfOption(
                         (fun () -> $"Field '{name.Name.LocalName}' not found in properties at this level.")
                         |> Errors.Singleton()
                       )
                       |> state.OfSum)
                      (TypeCheckState.tryFindResolvedIdentifier (name, ())
                       |> reader.MapContext snd
                       |> state.OfReader)


                  let! field_type = generate_data_model field_type properties_at_next_level

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
                       |> Map.tryFind (name.Name.LocalName |> LocalIdentifier.Create)
                       |> Option.map (fun prop -> prop.PropertyName |> ResolvedIdentifier.FromLocalIdentifier)
                       |> sum.OfOption(
                         (fun () -> $"Field '{name.Name.LocalName}' not found in properties at this level.")
                         |> Errors.Singleton()
                       )
                       |> state.OfSum)
                      (TypeCheckState.tryFindResolvedIdentifier (name, ())
                       |> reader.MapContext snd
                       |> state.OfReader)

                  let! case_type = generate_data_model case_type properties_at_next_level

                  return name, case_type
                })
              |> state.All

            return OpenAPIDataModel.Union cases
          | TypeValue.Sum { value = options } ->
            let! options =
              options
              |> Seq.map (fun option_type -> generate_data_model option_type properties_at_next_level)
              |> state.All

            return OpenAPIDataModel.Sum options
          | TypeValue.Tuple { value = elements } ->
            let! elements =
              elements
              |> Seq.map (fun element_type -> generate_data_model element_type properties_at_next_level)
              |> state.All

            return OpenAPIDataModel.Tuple elements
          | TypeValue.Imported { Id = type_id; Arguments = [ arg_t ] } when
            type_id = ("List" |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve)
            ->
            let! arg_t = generate_data_model arg_t properties_at_next_level
            return listToOpenApi arg_t
              
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
            let! type_with_props = generate_data_model entity_desc.TypeWithProps entity_desc.Properties
            let! type_original = generate_data_model entity_desc.TypeOriginal []
            let! id = generate_data_model entity_desc.Id []

            let type_with_props_name =
              { OpenAPIDataModelName.OpenAPIDataModelName = $"{entity_name.Name}-WithProps" }

            do! state.SetState(Map.add type_with_props_name type_with_props)

            let type_original_name =
              { OpenAPIDataModelName.OpenAPIDataModelName = $"{entity_name.Name}-Original" }

            do! state.SetState(Map.add type_original_name type_original)

            let id_name =
              { OpenAPIDataModelName.OpenAPIDataModelName = $"{entity_name.Name}-Id" }

            do! state.SetState(Map.add id_name id)
            return ()
          })
        |> state.All
        |> state.Ignore
    }

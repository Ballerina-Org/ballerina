namespace Ballerina.DSL.Next

module DeltaGeneration =
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.StdLib.Extensions
  open Ballerina.State.WithError
  open Ballerina.DSL.Next.Types.TypeChecker
  open OpenAPIModel
  open Ballerina.Errors
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError

  let generate_delta_models
    (schema: Schema<ValueExt<'runtimeContext, 'db, 'customExtension>>)
    : State<
        unit,
        (TypeCheckContext<ValueExt<'runtimeContext, 'db, 'customExtension>> *
        TypeCheckState<ValueExt<'runtimeContext, 'db, 'customExtension>>),
        Map<OpenAPIDataModelName, OpenAPIDataModel>,
        Errors<Unit>
       >
    =
    let resolve_name
      (name: TypeSymbol)
      (properties_at_this_level:
        Map<LocalIdentifier, SchemaEntityProperty<ValueExt<'runtimeContext, 'db, 'customExtension>>>)
      =
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

    let decompose_properties
      (properties: List<SchemaEntityProperty<ValueExt<'runtimeContext, 'db, 'customExtension>>>)
      =
      let at_this_level =
        properties
        |> Seq.filter (fun prop -> prop.Path |> List.isEmpty)
        |> Seq.map (fun prop -> prop.PropertyName, prop)
        |> Map.ofSeq

      let at_next_level =
        properties
        |> List.collect (fun prop ->
          match prop.Path with
          | [] -> []
          | _ :: rest -> [ { prop with Path = rest } ])

      at_this_level, at_next_level

    let rec generate_delta_model
      (type_value: TypeValue<'ext>)
      (properties: List<SchemaEntityProperty<ValueExt<'runtimeContext, 'db, 'customExtension>>>)
      : State<
          OpenAPIDataModel * OpenAPIDataModel,
          TypeCheckContext<ValueExt<'runtimeContext, 'db, 'customExtension>> *
          TypeCheckState<ValueExt<'runtimeContext, 'db, 'customExtension>>,
          Map<OpenAPIDataModelName, OpenAPIDataModel>,
          Errors<Unit>
         >
      =
      state {
        let properties_at_this_level, properties_at_next_level =
          decompose_properties properties

        match type_value |> TypeValue.GetSourceMapping with
        | TypeExprSourceMapping.OriginExprTypeLet(ExprTypeLetBindingName let_id, _) ->
          let type_value =
            TypeValue.SetSourceMapping(type_value, TypeExprSourceMapping.NoSourceMapping "")

          let! (_, delta_model) = generate_delta_model type_value properties_at_next_level

          let delta_type_name = { OpenAPIDataModelName = $"{let_id}-Delta" }
          do! state.SetState(Map.add delta_type_name delta_model)

          return
            OpenAPIDataModel.Ref { OpenAPIDataModelName = let_id },
            OpenAPIDataModel.Ref delta_type_name
        | TypeExprSourceMapping.OriginTypeExpr(_)
        | TypeExprSourceMapping.NoSourceMapping(_) ->

          match type_value with
          | TypeValue.Primitive { value = p } ->
            let value_model =
              OpenAPIDataModel.Object [ "Primitive" |> ResolvedIdentifier.Create, OpenAPIDataModel.Primitive p ]

            let delta_model =
              OpenAPIDataModel.Object
                [ "Multiple" |> ResolvedIdentifier.Create, OpenAPIDataModel.List OpenAPIDataModel.AnyObject
                  "Replace" |> ResolvedIdentifier.Create, value_model ]

            return value_model, delta_model

          | TypeValue.Record { value = fields } ->
            let! field_results =
              fields
              |> OrderedMap.toSeq
              |> Seq.map (fun (name, (field_type, _)) ->
                state {
                  let! resolved_name = resolve_name name properties_at_this_level
                  let! (field_value, field_delta) = generate_delta_model field_type properties_at_next_level
                  return resolved_name, field_value, field_delta
                })
              |> state.All

            let value_model =
              OpenAPIDataModel.Record(field_results |> List.map (fun (n, v, _) -> n, v))

            let delta_model =
              OpenAPIDataModel.Object
                [ "Multiple" |> ResolvedIdentifier.Create, OpenAPIDataModel.List OpenAPIDataModel.AnyObject
                  "Replace" |> ResolvedIdentifier.Create, value_model
                  "Record" |> ResolvedIdentifier.Create,
                  OpenAPIDataModel.Record(field_results |> List.map (fun (n, _, d) -> n, d)) ]

            return value_model, delta_model

          | TypeValue.Union { value = cases } ->
            let! case_results =
              cases
              |> OrderedMap.toSeq
              |> Seq.map (fun (name, case_type) ->
                state {
                  let! resolved_name = resolve_name name properties_at_this_level
                  let! (case_value, case_delta) = generate_delta_model case_type properties_at_next_level
                  return resolved_name, case_value, case_delta
                })
              |> state.All

            let value_model =
              OpenAPIDataModel.Union(case_results |> List.map (fun (n, v, _) -> n, v))

            let delta_model =
              OpenAPIDataModel.Object
                [ "Multiple" |> ResolvedIdentifier.Create, OpenAPIDataModel.List OpenAPIDataModel.AnyObject
                  "Replace" |> ResolvedIdentifier.Create, value_model
                  "Union" |> ResolvedIdentifier.Create,
                  OpenAPIDataModel.Union(case_results |> List.map (fun (n, _, d) -> n, d)) ]

            return value_model, delta_model

          | TypeValue.Sum { value = options } ->
            let! option_results =
              options
              |> Seq.map (fun option_type -> generate_delta_model option_type properties_at_next_level)
              |> state.All

            let value_model =
              OpenAPIDataModel.Sum(option_results |> List.map fst)

            let delta_model =
              OpenAPIDataModel.Object
                [ "Multiple" |> ResolvedIdentifier.Create, OpenAPIDataModel.List OpenAPIDataModel.AnyObject
                  "Replace" |> ResolvedIdentifier.Create, value_model
                  "Sum" |> ResolvedIdentifier.Create,
                  OpenAPIDataModel.Sum(option_results |> List.map snd) ]

            return value_model, delta_model

          | TypeValue.Tuple { value = elements } ->
            let! element_results =
              elements
              |> Seq.map (fun element_type -> generate_delta_model element_type properties_at_next_level)
              |> state.All

            let value_model =
              OpenAPIDataModel.Tuple(element_results |> List.map fst)

            let delta_model =
              OpenAPIDataModel.Object
                [ "Multiple" |> ResolvedIdentifier.Create, OpenAPIDataModel.List OpenAPIDataModel.AnyObject
                  "Replace" |> ResolvedIdentifier.Create, value_model
                  "Tuple" |> ResolvedIdentifier.Create,
                  OpenAPIDataModel.Tuple(element_results |> List.map snd) ]

            return value_model, delta_model

          | TypeValue.Imported { Id = type_id; Arguments = [ arg_t ] } when
            type_id = ("List" |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve)
            ->
            let! (element_value, element_delta) = generate_delta_model arg_t properties_at_next_level

            let value_model = listToOpenApi element_value

            let list_delta_ext =
              OpenAPIDataModel.Object
                [ "UpdateElement" |> ResolvedIdentifier.Create,
                  OpenAPIDataModel.Record
                    [ "Index" |> ResolvedIdentifier.Create, OpenAPIDataModel.Primitive PrimitiveType.Int32
                      "Value" |> ResolvedIdentifier.Create, element_delta ]
                  "AppendElement" |> ResolvedIdentifier.Create, element_value
                  "RemoveElement" |> ResolvedIdentifier.Create, OpenAPIDataModel.Primitive PrimitiveType.Int32
                  "InsertElement" |> ResolvedIdentifier.Create,
                  OpenAPIDataModel.Record
                    [ "Index" |> ResolvedIdentifier.Create, OpenAPIDataModel.Primitive PrimitiveType.Int32
                      "Value" |> ResolvedIdentifier.Create, element_value ]
                  "DuplicateElement" |> ResolvedIdentifier.Create, OpenAPIDataModel.Primitive PrimitiveType.Int32
                  "SetAllElements" |> ResolvedIdentifier.Create, element_value
                  "RemoveAllElements" |> ResolvedIdentifier.Create, OpenAPIDataModel.Primitive PrimitiveType.Bool
                  "MoveElement" |> ResolvedIdentifier.Create,
                  OpenAPIDataModel.Record
                    [ "From" |> ResolvedIdentifier.Create, OpenAPIDataModel.Primitive PrimitiveType.Int32
                      "To" |> ResolvedIdentifier.Create, OpenAPIDataModel.Primitive PrimitiveType.Int32 ] ]

            let delta_model =
              OpenAPIDataModel.Object
                [ "Multiple" |> ResolvedIdentifier.Create, OpenAPIDataModel.List OpenAPIDataModel.AnyObject
                  "Replace" |> ResolvedIdentifier.Create, value_model
                  "Ext" |> ResolvedIdentifier.Create, list_delta_ext ]

            return value_model, delta_model

          | _ ->
            return!
              (fun () -> $"Not supported type value for delta generation: {type_value}")
              |> Errors.Singleton()
              |> state.Throw
      }

    state {
      do!
        schema.Entities
        |> OrderedMap.toSeq
        |> Seq.map (fun (entity_name, entity_desc) ->
          state {
            let! (_, delta_with_props) = generate_delta_model entity_desc.TypeWithProps entity_desc.Properties
            let! (_, delta_original) = generate_delta_model entity_desc.TypeOriginal []

            let delta_with_props_name =
              { OpenAPIDataModelName.OpenAPIDataModelName = $"{entity_name.Name}-Delta-WithProps" }

            do! state.SetState(Map.add delta_with_props_name delta_with_props)

            let delta_original_name =
              { OpenAPIDataModelName.OpenAPIDataModelName = $"{entity_name.Name}-Delta-Original" }

            do! state.SetState(Map.add delta_original_name delta_original)
            return ()
          })
        |> state.All
        |> state.Ignore
    }

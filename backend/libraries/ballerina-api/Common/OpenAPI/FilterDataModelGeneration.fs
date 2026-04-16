namespace Ballerina.DSL.Next

module FilterDataModelGeneration =
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.StdLib.Extensions
  open Ballerina.State.WithError
  open Ballerina.DSL.Next.Types.TypeChecker
  open OpenAPIModel
  open Ballerina.Errors
  open Ballerina.Cat.Collections.OrderedMap
  open EndpointGeneration

  let generate_filter_data_models
    (schema: Schema<ValueExt<'runtimeContext, 'db, 'customExtension>>)
    : State<
        unit,
        (TypeCheckContext<ValueExt<'runtimeContext, 'db, 'customExtension>> *
        TypeCheckState<ValueExt<'runtimeContext, 'db, 'customExtension>>),
        Map<OpenAPIDataModelName, OpenAPIDataModel>,
        Errors<Unit>
       >
    =
    state {
      do!
        schema.Entities
        |> OrderedMap.toSeq
        |> Seq.map (fun (entity_name, entity_desc) ->
          state {
            let filterableProperties = collectFilterableProperties entity_desc

            if not (List.isEmpty filterableProperties) then
              let distinctPrimitiveTypes =
                filterableProperties
                |> List.map (fun (_, pt, _) -> pt)
                |> List.distinct

              do!
                distinctPrimitiveTypes
                |> List.map (fun pt ->
                  state {
                    let filterModelName =
                      { OpenAPIDataModelName.OpenAPIDataModelName = primitiveTypeFilterName pt }

                    let filterModel = buildPrimitiveFilterModel pt
                    do! state.SetState(Map.add filterModelName filterModel)
                    return ()
                  })
                |> state.All
                |> state.Ignore

              let propertyPredicateCases =
                filterableProperties
                |> List.map (fun (propName, primitiveType, _) ->
                  let filterRef =
                    { OpenAPIDataModelName.OpenAPIDataModelName = primitiveTypeFilterName primitiveType }

                  (propName |> ResolvedIdentifier.FromLocalIdentifier,
                   OpenAPIDataModel.Ref filterRef))

              let predicateModelName =
                { OpenAPIDataModelName.OpenAPIDataModelName = $"{entity_name.Name}-PropertyPredicate" }

              let predicateModel = OpenAPIDataModel.OneOf propertyPredicateCases

              do! state.SetState(Map.add predicateModelName predicateModel)

              let filterableRelations = collectFilterableRelations entity_name schema

              let relationExistsCases =
                filterableRelations
                |> List.choose (fun (relationName, targetEntityName, _direction) ->
                  let targetEntityOpt =
                    schema.Entities
                    |> OrderedMap.toSeq
                    |> Seq.tryFind (fun (en, _) -> en.Name = targetEntityName)
                    |> Option.map snd

                  match targetEntityOpt with
                  | None -> None
                  | Some targetEntityDesc ->
                    if collectFilterableProperties targetEntityDesc |> List.isEmpty then
                      None
                    else
                      let targetFilterTreeRef =
                        { OpenAPIDataModelName.OpenAPIDataModelName = $"{targetEntityName}-FilterTree" }

                      let existsModel =
                        OpenAPIDataModel.Record
                          [ ("RelationName" |> ResolvedIdentifier.Create,
                             OpenAPIDataModel.Primitive PrimitiveType.String)
                            ("TargetEntity" |> ResolvedIdentifier.Create,
                             OpenAPIDataModel.Primitive PrimitiveType.String)
                            ("SubFilter" |> ResolvedIdentifier.Create,
                             OpenAPIDataModel.Ref targetFilterTreeRef) ]

                      Some ($"{relationName}->{targetEntityName}" |> ResolvedIdentifier.Create, existsModel))

              let relationPredicateModelName =
                { OpenAPIDataModelName.OpenAPIDataModelName = $"{entity_name.Name}-RelationPredicate" }

              if not (List.isEmpty relationExistsCases) then
                let relationPredicateModel = OpenAPIDataModel.OneOf relationExistsCases
                do! state.SetState(Map.add relationPredicateModelName relationPredicateModel)

              let filterTreeModelName =
                { OpenAPIDataModelName.OpenAPIDataModelName = $"{entity_name.Name}-FilterTree" }

              let baseCases =
                [ ("And" |> ResolvedIdentifier.Create,
                   OpenAPIDataModel.Array(OpenAPIDataModel.Ref filterTreeModelName))
                  ("Or" |> ResolvedIdentifier.Create,
                   OpenAPIDataModel.Array(OpenAPIDataModel.Ref filterTreeModelName))
                  ("Not" |> ResolvedIdentifier.Create,
                   OpenAPIDataModel.Ref filterTreeModelName)
                  ("Predicate" |> ResolvedIdentifier.Create,
                   OpenAPIDataModel.Ref predicateModelName) ]

              let existsCase =
                if not (List.isEmpty relationExistsCases) then
                  [ ("Exists" |> ResolvedIdentifier.Create,
                     OpenAPIDataModel.Ref relationPredicateModelName) ]
                else
                  []

              let filterTreeModel =
                OpenAPIDataModel.OneOf (baseCases @ existsCase)

              do! state.SetState(Map.add filterTreeModelName filterTreeModel)

            return ()
          })
        |> state.All
        |> state.Ignore
    }

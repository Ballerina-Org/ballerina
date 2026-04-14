namespace Ballerina.DSL.Next.Types.TypeChecker

module SchemaTypeEval =
  open System
  open Ballerina
  open Ballerina.Fun
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Object
  open Ballerina.StdLib.String
  open Ballerina.State.WithError
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.Collections.Map
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.LiftOtherSteps
  open Ballerina.DSL.Next.Unification
  open Ballerina.DSL.Next.Terms.Patterns

  let evalSchemaExpr<'ve when 've: comparison>
    (config: TypeCheckingConfig<'ve>)
    (typeCheckExpr: ExprTypeChecker<'ve>)
    (evalTypeExpr: TypeExpr<'ve> -> TypeExprEvalResult<'ve>)
    (loc0: Location)
    (source: TypeExprSourceMapping<'ve>)
    parsed_schema
    : TypeExprEvalResult<'ve> =
    state {
      let { MkQueryType = mk_query_type } = config

      let (!) = evalTypeExpr

      let error e = Errors.Singleton loc0 e

      let ofSum (p: Sum<'a, Errors<Unit>>) =
        p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

      let! included_schema_entityhooks_relation_hooks =
        match parsed_schema.Includes with
        | Some(includeName, entity_hooks, relation_hooks) ->
          state {
            let! included_schema, included_schema_k =
              TypeCheckState.tryFindType (
                includeName.Name
                |> Identifier.LocalScope
                |> TypeCheckScope.Empty.Resolve,
                loc0
              )
              |> state.OfStateReader

            do! included_schema_k |> Kind.AsSchema |> ofSum |> state.Ignore

            let! included_schema =
              included_schema |> TypeValue.AsSchema |> ofSum

            return Some(included_schema, entity_hooks, relation_hooks)
          }
        | None -> state { return None }

      let repeatedEntityNames =
        parsed_schema.Entities
        |> List.map (fun e -> e.Name)
        |> List.append (
          included_schema_entityhooks_relation_hooks
          |> Option.map (fun (s, _, _) ->
            s.Entities |> OrderedMap.values |> List.map (fun e -> e.Name))
          |> Option.defaultValue []
        )
        |> List.groupBy id
        |> List.filter (fun (_, l) -> List.length l > 1)
        |> List.map (fst >> fun n -> n.Name)

      if not (List.isEmpty repeatedEntityNames) then
        return!
          let sep = ", " in

          (fun () ->
            $"Error: schema has repeated entity names: {String.join sep repeatedEntityNames}")
          |> error
          |> state.Throw
      else
        let! entities =
          parsed_schema.Entities
          |> List.map (fun e ->
            e.Name,
            state {
              let! id, id_k = !e.Id
              do! id_k |> Kind.AsStar |> ofSum |> state.Ignore
              let! t, t_k = !e.Type
              do! t_k |> Kind.AsStar |> ofSum |> state.Ignore

              let! properties =
                e.Properties
                |> Seq.map (fun p ->
                  state {
                    let! p_decl_t, p_decl_k = !p.Type
                    do! p_decl_k |> Kind.AsStar |> ofSum |> state.Ignore

                    let path_segments = p.Path |> Option.defaultValue []

                    if
                      path_segments |> List.isEmpty
                      && (p.Name.Name = "Id" || p.Name.Name = "Value")
                    then
                      return!
                        (fun () ->
                          $"Error: property {p.Name} cannot have an empty path as it would conflict with the entity's Id or Value property")
                        |> error
                        |> state.Throw
                    else

                      let! _, path_scope, resolved_scope =
                        path_segments
                        |> List.fold
                          (fun acc segment ->
                            state {
                              let! t, segments_acc, resolved_scope = acc

                              match segment with
                              | (maybe_var_name,
                                 SchemaPathTypeDecompositionExpr.Field f) ->
                                let! t_record =
                                  t
                                  |> TypeValue.AsRecordWithSourceMapping
                                  |> ofSum

                                let t_record_scope =
                                  t_record.typeCheckScopeSource

                                let t_record = t_record.value

                                let! (f_i, (f_t, f_k)) =
                                  t_record
                                  |> OrderedMap.toSeq
                                  |> Seq.tryFind (fun (k, _) ->
                                    k.Name.LocalName = f.LocalName)
                                  |> Sum.fromOption (fun () ->
                                    (fun () ->
                                      $"Error: cannot find field {f} in record type {t}")
                                    |> Errors.Singleton loc0)
                                  |> state.OfSum

                                do!
                                  f_k |> Kind.AsStar |> ofSum |> state.Ignore

                                let next_t = f_t

                                let next_segments =
                                  match maybe_var_name with
                                  | Some var_name ->
                                    (Identifier.LocalScope var_name.Name
                                     |> TypeCheckScope.Empty.Resolve,
                                     (next_t, f_k))
                                    :: segments_acc
                                  | None -> segments_acc

                                return
                                  next_t,
                                  next_segments,
                                  (maybe_var_name,
                                   f_i.Name
                                   |> t_record_scope.Resolve
                                   |> SchemaPathTypeDecomposition.Field)
                                  :: resolved_scope
                              | (maybe_var_name,
                                 SchemaPathTypeDecompositionExpr.UnionCase f) ->
                                let! _, t_union_scope, t_union =
                                  t
                                  |> TypeValue.AsUnionWithSourceMapping
                                  |> ofSum

                                let! (case_i, case_t) =
                                  t_union
                                  |> OrderedMap.toSeq
                                  |> Seq.tryFind (fun (k, _) ->
                                    k.Name.LocalName = f.LocalName)
                                  |> Sum.fromOption (fun () ->
                                    (fun () ->
                                      $"Error: cannot find case {f} in union type {t}")
                                    |> Errors.Singleton loc0)
                                  |> state.OfSum

                                let next_t = case_t

                                let next_segments =
                                  match maybe_var_name with
                                  | Some var_name ->
                                    (Identifier.LocalScope var_name.Name
                                     |> TypeCheckScope.Empty.Resolve,
                                     (next_t, Kind.Star))
                                    :: segments_acc
                                  | None -> segments_acc

                                return
                                  next_t,
                                  next_segments,
                                  (maybe_var_name,
                                   case_i.Name
                                   |> t_union_scope.Resolve
                                   |> SchemaPathTypeDecomposition.UnionCase)
                                  :: resolved_scope
                              | (maybe_var_name,
                                 SchemaPathTypeDecompositionExpr.SumCase f) ->
                                let! t_sum = t |> TypeValue.AsSum |> ofSum

                                if
                                  f.Case < 1
                                  || f.Case > t_sum.Length
                                  || f.Count <> t_sum.Length
                                then
                                  return!
                                    (fun () ->
                                      $"Error: sum case {f} is out of bounds for sum type {t}")
                                    |> Errors.Singleton loc0
                                    |> state.Throw
                                else
                                  let! case_t =
                                    t_sum
                                    |> Seq.tryItem (f.Case - 1)
                                    |> Sum.fromOption (fun () ->
                                      (fun () ->
                                        $"Error: cannot find sum case {f} in sum type {t}")
                                      |> Errors.Singleton loc0)
                                    |> state.OfSum

                                  let next_t = case_t

                                  let next_segments =
                                    match maybe_var_name with
                                    | Some var_name ->
                                      (Identifier.LocalScope var_name.Name
                                       |> TypeCheckScope.Empty.Resolve,
                                       (next_t, Kind.Star))
                                      :: segments_acc
                                    | None -> segments_acc

                                  return
                                    next_t,
                                    next_segments,
                                    (maybe_var_name,
                                     SchemaPathTypeDecomposition.SumCase f)
                                    :: resolved_scope
                              | (maybe_var_name,
                                 SchemaPathTypeDecompositionExpr.Item f) ->
                                let! t_tuple =
                                  t |> TypeValue.AsTuple |> ofSum

                                if
                                  f.Index < 1 || f.Index > t_tuple.Length
                                then
                                  return!
                                    (fun () ->
                                      $"Error: tuple index {f} is out of bounds for tuple type {t}")
                                    |> Errors.Singleton loc0
                                    |> state.Throw
                                else
                                  let! item_t =
                                    t_tuple
                                    |> Seq.tryItem (f.Index - 1)
                                    |> Sum.fromOption (fun () ->
                                      (fun () ->
                                        $"Error: cannot find tuple index {f} in tuple type {t}")
                                      |> Errors.Singleton loc0)
                                    |> state.OfSum

                                  let next_t = item_t

                                  let next_segments =
                                    match maybe_var_name with
                                    | Some var_name ->
                                      (Identifier.LocalScope var_name.Name
                                       |> TypeCheckScope.Empty.Resolve,
                                       (next_t, Kind.Star))
                                      :: segments_acc
                                    | None -> segments_acc

                                  return
                                    next_t,
                                    next_segments,
                                    (maybe_var_name,
                                     SchemaPathTypeDecomposition.Item f)
                                    :: resolved_scope
                              | (maybe_var_name,
                                 SchemaPathTypeDecompositionExpr.Iterator it) ->
                                let! container, _ =
                                  it.Container |> TypeExpr.Lookup |> (!)

                                let! t_arg, _ =
                                  it.TypeDef |> TypeExpr.Lookup |> (!)

                                let! mapper, _ =
                                  typeCheckExpr
                                    None
                                    (it.Mapper |> Expr.Lookup)

                                let mapper = Conversion.convertExpression mapper

                                let next_t = t_arg

                                let next_segments =
                                  match maybe_var_name with
                                  | Some var_name ->
                                    (Identifier.LocalScope var_name.Name
                                     |> TypeCheckScope.Empty.Resolve,
                                     (next_t, Kind.Star))
                                    :: segments_acc
                                  | None -> segments_acc

                                return
                                  next_t,
                                  next_segments,
                                  (maybe_var_name,
                                   SchemaPathTypeDecomposition.Iterator
                                     {| Container = container
                                        TypeDef = t_arg
                                        Mapper = mapper |})
                                  :: resolved_scope
                            })
                          (state { return t, [], [] })

                      let resolved_scope = resolved_scope |> List.rev

                      let path_scope =
                        path_scope
                        |> List.map (fun (id, (t, k)) -> Map.add id (t, k))
                        |> List.fold (>>) (fun x -> x)

                      let! body_e, _ =
                        typeCheckExpr (Some p_decl_t) p.Body
                        |> state.MapContext(
                          TypeCheckContext.Updaters.Values(
                            Map.add
                              (Identifier.LocalScope "self"
                               |> TypeCheckScope.Empty.Resolve)
                              (t, t_k)
                            >> path_scope
                          )
                          >> TypeCheckContext.Updaters.Scope(
                            TypeCheckScope.Empty |> replaceWith
                          )
                        )

                      let body_t = body_e.Type

                      let body_k = body_e.Kind

                      do! body_k |> Kind.AsStar |> ofSum |> state.Ignore

                      do!
                        TypeValue.Unify(loc0, body_t, p_decl_t)
                        |> Expr.liftUnification

                      let body_runnable = Conversion.convertExpression body_e

                      return
                        { SchemaEntityProperty.Path = resolved_scope
                          PropertyName = p.Name
                          ReturnType = p_decl_t
                          ReturnKind = p_decl_k
                          Body = body_runnable }
                  })
                |> state.All
                |> state.Map(List.ofSeq)

              let! vectors =
                e.Vectors
                |> Seq.map (fun p ->
                  state {
                    if p.Name.Name = "Id" || p.Name.Name = "Value" then
                      return!
                        (fun () ->
                          $"Error: please rename vector, it conflicts with the entity's Id or Value property")
                        |> error
                        |> state.Throw
                    else

                      let! body_e, _ =
                        typeCheckExpr
                          (Some(
                            TypeValue.CreatePrimitive(PrimitiveType.String)
                          ))
                          p.Body
                        |> state.MapContext(
                          TypeCheckContext.Updaters.Values(
                            Map.add
                              (Identifier.LocalScope "self"
                               |> TypeCheckScope.Empty.Resolve)
                              (t, t_k)
                          )
                          >> TypeCheckContext.Updaters.Scope(
                            TypeCheckScope.Empty |> replaceWith
                          )
                        )

                      let body_t = body_e.Type

                      let body_k = body_e.Kind

                      do! body_k |> Kind.AsStar |> ofSum |> state.Ignore

                      do!
                        TypeValue.Unify(
                          loc0,
                          body_t,
                          TypeValue.CreatePrimitive(PrimitiveType.String)
                        )
                        |> Expr.liftUnification

                      let body_runnable = Conversion.convertExpression body_e

                      return
                        { SchemaEntityVector.VectorName = p.Name
                          Body = body_runnable }
                  })
                |> state.All
                |> state.Map(List.ofSeq)

              let rec (+)
                (t: TypeValue<'ve>)
                (
                  path: List<SchemaPathSegment<'ve>>,
                  name: LocalIdentifier,
                  result_t: TypeValue<'ve>
                ) =
                state {
                  match path with
                  | [] ->
                    let! fields = t |> TypeValue.AsRecord |> ofSum

                    if
                      fields
                      |> OrderedMap.toSeq
                      |> Seq.filter (fun (k, _) ->
                        k.Name.LocalName = name.Name)
                      |> Seq.isEmpty
                      |> not
                    then
                      return!
                        (fun () ->
                          $"Error: a field with the same name as property {name.Name} already exists in record type {t}")
                        |> Errors.Singleton loc0
                        |> state.Throw
                    else
                      let fields =
                        fields
                        |> OrderedMap.add
                          (name.Name
                           |> Identifier.LocalScope
                           |> TypeSymbol.Create)
                          (result_t, Kind.Star)

                      return TypeValue.CreateRecord fields
                  | (_, SchemaPathTypeDecomposition.Field f) :: path ->
                    let! fields = t |> TypeValue.AsRecord |> ofSum

                    let! f_s, (f_t, f_k) =
                      fields
                      |> OrderedMap.toSeq
                      |> Seq.tryFind (fun (k, _) -> k.Name.LocalName = f.Name)
                      |> Sum.fromOption (fun () ->
                        (fun () ->
                          $"Error: cannot find field {f} in record type {t}")
                        |> Errors.Singleton loc0)
                      |> state.OfSum

                    let! f_t = f_t + (path, name, result_t)

                    let fields = fields |> OrderedMap.add f_s (f_t, f_k)

                    return TypeValue.CreateRecord fields
                  | (_, SchemaPathTypeDecomposition.UnionCase f) :: path ->
                    let! _, fields = t |> TypeValue.AsUnion |> ofSum

                    let! f_s, f_t =
                      fields
                      |> OrderedMap.toSeq
                      |> Seq.tryFind (fun (k, _) -> k.Name.LocalName = f.Name)
                      |> Sum.fromOption (fun () ->
                        (fun () ->
                          $"Error: cannot find field {f} in record type {t}")
                        |> Errors.Singleton loc0)
                      |> state.OfSum

                    let! f_t = f_t + (path, name, result_t)

                    let fields = fields |> OrderedMap.add f_s f_t

                    return TypeValue.CreateUnion fields
                  | (_, SchemaPathTypeDecomposition.SumCase f) :: path ->
                    let! fields = t |> TypeValue.AsSum |> ofSum

                    let! f_t =
                      fields
                      |> Seq.tryItem (f.Case - 1)
                      |> Sum.fromOption (fun () ->
                        (fun () ->
                          $"Error: cannot find field {f} in record type {t}")
                        |> Errors.Singleton loc0)
                      |> state.OfSum

                    let! f_t = f_t + (path, name, result_t)

                    let fields =
                      fields
                      |> List.mapi (fun i ft ->
                        if i = f.Case - 1 then f_t else ft)

                    return TypeValue.CreateSum fields
                  | (_, SchemaPathTypeDecomposition.Item f) :: path ->
                    let! fields = t |> TypeValue.AsTuple |> ofSum

                    let! f_t =
                      fields
                      |> Seq.tryItem (f.Index - 1)
                      |> Sum.fromOption (fun () ->
                        (fun () ->
                          $"Error: cannot find field {f} in record type {t}")
                        |> Errors.Singleton loc0)
                      |> state.OfSum

                    let! f_t = f_t + (path, name, result_t)

                    let fields =
                      fields
                      |> List.mapi (fun i ft ->
                        if i = f.Index - 1 then f_t else ft)

                    return TypeValue.CreateTuple fields
                  | (_, SchemaPathTypeDecomposition.Iterator f) :: path ->
                    let f_t = f.TypeDef
                    let! f_t = f_t + (path, name, result_t)

                    let! t_res, _ =
                      !TypeExpr.Apply(TypeExpr.FromTypeValue f.Container,
                                      TypeExpr.FromTypeValue f_t)

                    return t_res
                }

              let! t_with_props =
                properties
                |> Seq.fold
                  (fun acc (p: SchemaEntityProperty<'ve>) ->
                    state {
                      let! t = acc
                      return! t + (p.Path, p.PropertyName, p.ReturnType)
                    })
                  (state { return t })

              // let rec (+) (t: TypeValue<'ve>) (name: LocalIdentifier) =
              //   state {
              //     let! fields = t |> TypeValue.AsRecord |> ofSum

              //     let vector_name_already_exists =
              //       fields
              //       |> OrderedMap.toSeq
              //       |> Seq.tryFind (fun (k, _) -> k.Name.LocalName = name.Name)

              //     if vector_name_already_exists.IsSome then
              //       return!
              //         (fun () ->
              //           $"Error: a field with the same name as vector {name.Name} already exists in record type {t}")
              //         |> Errors.Singleton loc0
              //         |> state.Throw

              //     else
              //       let fields =
              //         fields
              //         |> OrderedMap.add
              //           (name.Name |> Identifier.LocalScope |> TypeSymbol.Create)
              //           (TypeValue.CreatePrimitive PrimitiveType.Vector, Kind.Star)

              //       return TypeValue.CreateRecord fields
              //   }

              // let! t_with_props =
              //   vectors
              //   |> Seq.fold
              //     (fun acc (p: SchemaEntityVector<'ve>) ->
              //       state {
              //         let! t = acc
              //         return! t + p.VectorName
              //       })
              //     (state { return t_with_props })

              return
                { SchemaEntity.Name = e.Name
                  Id = id
                  TypeOriginal = t
                  TypeWithProps = t_with_props
                  Properties = properties
                  Vectors = vectors
                  Hooks =
                    { SchemaEntityHooks.OnCreating = None
                      OnCreated = None
                      OnUpdating = None
                      OnUpdated = None
                      OnDeleting = None
                      OnDeleted = None
                      OnBackground = None
                      CanCreate = None
                      CanRead = None
                      CanUpdate = None
                      CanDelete = None } }
            })
          |> OrderedMap.ofList
          |> state.AllMapOrdered

        let! all_id_name_and_types =
          entities
          |> OrderedMap.values
          |> Seq.map (fun e ->
            state {
              let! n, p = e.Id |> TypeValue.AsPrimaryKey |> ofSum
              return n, TypeValue.CreatePrimitive p
            })
          |> state.All

        let all_id_names = all_id_name_and_types |> List.map fst |> Set.ofList

        if Set.count all_id_names <> List.length all_id_name_and_types then
          return!
            (fun () -> $"Error: entity id fields must have unique names.")
            |> error
            |> state.Throw
        else
          ()

        let repeatedRelationNames =
          parsed_schema.Relations
          |> List.map (fun r -> r.Name)
          |> List.groupBy id
          |> List.filter (fun (_, l) -> List.length l > 1)
          |> List.map (fst >> fun n -> n.Name)

        if not (List.isEmpty repeatedRelationNames) then
          return!
            let sep = ", " in

            (fun () ->
              $"Error: schema has repeated relation names: {String.join sep repeatedRelationNames}")
            |> error
            |> state.Throw
        else
          let! context = state.GetContext()

          let all_entities =
            match included_schema_entityhooks_relation_hooks with
            | Some(s, _, _) ->
              entities |> OrderedMap.mergeSecondAfterFirst s.Entities
            | None -> entities

          let! relations =
            parsed_schema.Relations
            |> List.map (fun r ->
              r.Name,
              state {
                let fromPath = r.From |> snd
                let toPath = r.To |> snd

                let r_loc = r.Location

                let! fromEntity =
                  all_entities
                  |> OrderedMap.tryFind (
                    (r.From |> fst).LocalName |> SchemaEntityName.Create
                  )
                  |> Sum.fromOption (fun () ->
                    (fun () ->
                      $"Error: cannot find entity {r.From |> fst} for relation {r.Name.Name}")
                    |> Errors.Singleton r_loc)
                  |> state.OfSum

                let! toEntity =
                  all_entities
                  |> OrderedMap.tryFind (
                    (r.To |> fst).LocalName |> SchemaEntityName.Create
                  )
                  |> Sum.fromOption (fun () ->
                    (fun () ->
                      $"Error: cannot find entity {r.To |> fst} for relation {r.Name.Name}")
                    |> Errors.Singleton r_loc)
                  |> state.OfSum

                match fromPath with
                | Some fromPath ->
                  do!
                    SchemaPathValidation.validatePath
                      (!)
                      context
                      r_loc
                      fromEntity.TypeOriginal
                      toEntity.Id
                      fromPath
                | None -> ()

                match toPath with
                | Some toPath ->
                  do!
                    SchemaPathValidation.validatePath
                      (!)
                      context
                      r_loc
                      toEntity.TypeOriginal
                      fromEntity.Id
                      toPath
                | None -> ()

                return
                  { Name = r.Name
                    From = r.From |> fst
                    To = r.To |> fst
                    Cardinality = r.Cardinality
                    Hooks =
                      { SchemaRelationHooks.OnLinking = None
                        SchemaRelationHooks.OnLinked = None
                        SchemaRelationHooks.OnUnlinking = None
                        SchemaRelationHooks.OnUnlinked = None } }
              })
            |> OrderedMap.ofList
            |> state.AllMapOrdered

          let included_schema =
            included_schema_entityhooks_relation_hooks
            |> Option.map (fun (s, _, _) -> s)

          let included_entities =
            included_schema_entityhooks_relation_hooks
            |> Option.map (fun (s, _, _) -> s.Entities)
            |> Option.defaultValue OrderedMap.empty

          let included_relations =
            included_schema_entityhooks_relation_hooks
            |> Option.map (fun (s, _, _) -> s.Relations)
            |> Option.defaultValue OrderedMap.empty

          let included_entities_hooks =
            included_schema_entityhooks_relation_hooks
            |> Option.map (fun (_, e, _) -> e)
            |> Option.defaultValue []
            |> Map.ofList

          let resulting_schema_without_hooks =
            { DeclaredAtForNominalEquality = loc0
              Source = source
              Entities =
                entities |> OrderedMap.mergeSecondAfterFirst included_entities
              Relations =
                relations |> OrderedMap.mergeSecondAfterFirst included_relations
              Included = included_schema }

          let typecheck_entity_hooks
            (e_typechecked: SchemaEntity<'ve>)
            (parsed_hooks: SchemaEntityHooksExpr<'ve>)
            =
            state {
              let! onCreating =
                SchemaEntityHookOnCreating.typecheck
                  typeCheckExpr
                  loc0
                  resulting_schema_without_hooks
                  e_typechecked
                  parsed_hooks.OnCreating

              let! onCreated =
                SchemaEntityHookOnCreated.typecheck
                  typeCheckExpr
                  loc0
                  resulting_schema_without_hooks
                  e_typechecked
                  parsed_hooks.OnCreated

              let! onUpdating =
                SchemaEntityHookOnUpdating.typecheck
                  typeCheckExpr
                  loc0
                  resulting_schema_without_hooks
                  e_typechecked
                  parsed_hooks.OnUpdating

              let! onUpdated =
                SchemaEntityHookOnUpdated.typecheck
                  typeCheckExpr
                  loc0
                  resulting_schema_without_hooks
                  e_typechecked
                  parsed_hooks.OnUpdated

              let! onDeleting =
                SchemaEntityHookOnDeleting.typecheck
                  typeCheckExpr
                  loc0
                  resulting_schema_without_hooks
                  e_typechecked
                  parsed_hooks.OnDeleting

              let! onDeleted =
                SchemaEntityHookOnDeleted.typecheck
                  typeCheckExpr
                  loc0
                  resulting_schema_without_hooks
                  e_typechecked
                  parsed_hooks.OnDeleted

              let! onBackground =
                SchemaEntityHookOnBackground.typecheck
                  typeCheckExpr
                  loc0
                  resulting_schema_without_hooks
                  e_typechecked
                  parsed_hooks.OnBackground

              let! canCreate =
                SchemaEntityHookCanCreate.typecheck
                  typeCheckExpr
                  loc0
                  resulting_schema_without_hooks
                  parsed_hooks.CanCreate

              let! canRead =
                SchemaEntityHookCanRead.typecheck
                  typeCheckExpr
                  mk_query_type
                  loc0
                  resulting_schema_without_hooks
                  e_typechecked
                  parsed_hooks.CanRead

              let! canUpdate =
                SchemaEntityHookCanUpdate.typecheck
                  typeCheckExpr
                  loc0
                  resulting_schema_without_hooks
                  e_typechecked
                  parsed_hooks.CanUpdate

              let! canDelete =
                SchemaEntityHookCanDelete.typecheck
                  typeCheckExpr
                  loc0
                  resulting_schema_without_hooks
                  e_typechecked
                  parsed_hooks.CanDelete

              let onCreating = Conversion.convertExpressionOption onCreating
              let onCreated = Conversion.convertExpressionOption onCreated
              let onUpdating = Conversion.convertExpressionOption onUpdating
              let onUpdated = Conversion.convertExpressionOption onUpdated
              let onDeleting = Conversion.convertExpressionOption onDeleting
              let onDeleted = Conversion.convertExpressionOption onDeleted
              let onBackground = Conversion.convertExpressionOption onBackground
              let canCreate = Conversion.convertExpressionOption canCreate
              let canRead = Conversion.convertExpressionOption canRead
              let canUpdate = Conversion.convertExpressionOption canUpdate
              let canDelete = Conversion.convertExpressionOption canDelete

              return
                { OnCreating = onCreating
                  OnCreated = onCreated
                  OnUpdating = onUpdating
                  OnUpdated = onUpdated
                  OnDeleting = onDeleting
                  OnDeleted = onDeleted
                  OnBackground = onBackground
                  CanCreate = canCreate
                  CanRead = canRead
                  CanUpdate = canUpdate
                  CanDelete = canDelete }
            }


          let typecheck_relation_hooks
            (assert_no_cardinality:
              State<
                unit,
                TypeCheckContext<'ve>,
                TypeCheckState<'ve>,
                Errors<Location>
               >)
            (relation_hook_type: TypeValue<'ve>)
            (parsed_hooks: SchemaRelationHooksExpr<'ve>)
            =
            state {
              let r_typechecked =
                { OnLinking = None
                  OnLinked = None
                  OnUnlinking = None
                  OnUnlinked = None }

              let! r_typechecked =
                state {
                  match parsed_hooks.OnLinking with
                  | None -> return { r_typechecked with OnLinking = None }
                  | Some on_linking ->
                    do! assert_no_cardinality

                    let! on_linking_expr, _ =
                      typeCheckExpr None on_linking
                      |> state.MapContext(
                        TypeCheckContext.Updaters.Scope(
                          TypeCheckScope.Empty |> replaceWith
                        )
                      )

                    let on_linking_t = on_linking_expr.Type

                    let on_linking_k = on_linking_expr.Kind

                    do! on_linking_k |> Kind.AsStar |> ofSum |> state.Ignore

                    do!
                      TypeValue.Unify(
                        on_linking.Location,
                        on_linking_t,
                        relation_hook_type
                      )
                      |> Expr.liftUnification
                      |> state.MapContext(
                        TypeCheckContext.Updaters.Scope(
                          TypeCheckScope.Empty |> replaceWith
                        )
                      )

                    let on_linking_runnable = Conversion.convertExpression on_linking_expr

                    return
                      { r_typechecked with
                          OnLinking = Some on_linking_runnable }
                }

              let! r_typechecked =
                state {
                  match parsed_hooks.OnLinked with
                  | None -> return { r_typechecked with OnLinked = None }
                  | Some on_linked ->
                    do! assert_no_cardinality

                    let! on_linked_expr, _ =
                      typeCheckExpr None on_linked
                      |> state.MapContext(
                        TypeCheckContext.Updaters.Scope(
                          TypeCheckScope.Empty |> replaceWith
                        )
                      )

                    let on_linked_t = on_linked_expr.Type

                    let on_linked_k = on_linked_expr.Kind

                    do! on_linked_k |> Kind.AsStar |> ofSum |> state.Ignore

                    do!
                      TypeValue.Unify(
                        on_linked.Location,
                        on_linked_t,
                        relation_hook_type
                      )
                      |> Expr.liftUnification
                      |> state.MapContext(
                        TypeCheckContext.Updaters.Scope(
                          TypeCheckScope.Empty |> replaceWith
                        )
                      )

                    let on_linked_runnable = Conversion.convertExpression on_linked_expr

                    return
                      { r_typechecked with
                          OnLinked = Some on_linked_runnable }
                }

              let! r_typechecked =
                state {
                  match parsed_hooks.OnUnlinking with
                  | None ->
                    return
                      { r_typechecked with
                          OnUnlinking = None }
                  | Some on_unlinking ->
                    do! assert_no_cardinality

                    let! on_unlinking_expr, _ =
                      typeCheckExpr None on_unlinking
                      |> state.MapContext(
                        TypeCheckContext.Updaters.Scope(
                          TypeCheckScope.Empty |> replaceWith
                        )
                      )

                    let on_unlinking_t = on_unlinking_expr.Type

                    let on_unlinking_k = on_unlinking_expr.Kind

                    do! on_unlinking_k |> Kind.AsStar |> ofSum |> state.Ignore

                    do!
                      TypeValue.Unify(
                        on_unlinking.Location,
                        on_unlinking_t,
                        relation_hook_type
                      )
                      |> Expr.liftUnification
                      |> state.MapContext(
                        TypeCheckContext.Updaters.Scope(
                          TypeCheckScope.Empty |> replaceWith
                        )
                      )

                    let on_unlinking_runnable = Conversion.convertExpression on_unlinking_expr

                    return
                      { r_typechecked with
                          OnUnlinking = Some on_unlinking_runnable }
                }

              let! r_typechecked =
                state {
                  match parsed_hooks.OnUnlinked with
                  | None -> return { r_typechecked with OnUnlinked = None }
                  | Some on_unlinked ->
                    do! assert_no_cardinality

                    let! on_unlinked_expr, _ =
                      typeCheckExpr None on_unlinked
                      |> state.MapContext(
                        TypeCheckContext.Updaters.Scope(
                          TypeCheckScope.Empty |> replaceWith
                        )
                      )

                    let on_unlinked_t = on_unlinked_expr.Type

                    let on_unlinked_k = on_unlinked_expr.Kind

                    do! on_unlinked_k |> Kind.AsStar |> ofSum |> state.Ignore

                    do!
                      TypeValue.Unify(
                        on_unlinked.Location,
                        on_unlinked_t,
                        relation_hook_type
                      )
                      |> Expr.liftUnification
                      |> state.MapContext(
                        TypeCheckContext.Updaters.Scope(
                          TypeCheckScope.Empty |> replaceWith
                        )
                      )

                    let on_unlinked_runnable = Conversion.convertExpression on_unlinked_expr

                    return
                      { r_typechecked with
                          OnUnlinked = Some on_unlinked_runnable }
                }

              return r_typechecked
            }

          let! included_entities_hooks =
            included_entities_hooks
            |> Map.toSeq
            |> Seq.map (fun (entityName, hooks) ->
              state {
                let! included_entity =
                  included_entities
                  |> OrderedMap.tryFind entityName
                  |> Sum.fromOption (fun () ->
                    (fun () ->
                      $"Error: cannot find included entity {entityName} for hooks")
                    |> Errors.Singleton loc0)
                  |> state.OfSum

                let! typechecked_hooks =
                  typecheck_entity_hooks included_entity hooks

                return entityName, typechecked_hooks
              })
            |> state.All
            |> state.Map(Map.ofSeq)

          let included_entities =
            included_entities_hooks
            |> Map.fold
              (fun acc entityName hooks ->
                match acc |> OrderedMap.tryFind entityName with
                | Some v ->
                  let v =
                    { v with
                        SchemaEntity.Hooks =
                          { SchemaEntityHooks.OnCreating =
                              v.Hooks.OnCreating
                              |> Option.orElse hooks.OnCreating
                            SchemaEntityHooks.OnCreated =
                              v.Hooks.OnCreated
                              |> Option.orElse hooks.OnCreated
                            SchemaEntityHooks.OnUpdating =
                              v.Hooks.OnUpdating
                              |> Option.orElse hooks.OnUpdating
                            SchemaEntityHooks.OnUpdated =
                              v.Hooks.OnUpdated
                              |> Option.orElse hooks.OnUpdated
                            SchemaEntityHooks.OnDeleting =
                              v.Hooks.OnDeleting
                              |> Option.orElse hooks.OnDeleting
                            SchemaEntityHooks.OnDeleted =
                              v.Hooks.OnDeleted
                              |> Option.orElse hooks.OnDeleted
                            SchemaEntityHooks.OnBackground =
                              v.Hooks.OnBackground
                              |> Option.orElse hooks.OnBackground
                            SchemaEntityHooks.CanCreate =
                              v.Hooks.CanCreate
                              |> Option.orElse hooks.CanCreate
                            SchemaEntityHooks.CanRead =
                              v.Hooks.CanRead |> Option.orElse hooks.CanRead
                            SchemaEntityHooks.CanUpdate =
                              v.Hooks.CanUpdate
                              |> Option.orElse hooks.CanUpdate
                            SchemaEntityHooks.CanDelete =
                              v.Hooks.CanDelete
                              |> Option.orElse hooks.CanDelete } }

                  acc |> OrderedMap.add entityName v
                | None -> acc)
              included_entities

          let included_entities_hooks_names =
            included_entities_hooks
            |> Map.toSeq
            |> Seq.map (fst >> (fun n -> n.Name))
            |> Set.ofSeq

          let included_relations_hooks =
            included_schema_entityhooks_relation_hooks
            |> Option.map (fun (_, _, r) -> r)
            |> Option.defaultValue []
            |> Map.ofList

          let hooks_scope =
            { TypeCheckScope.Empty with
                Type = Some "Schema" }

          let! (state_to_restore: TypeCheckState<'ve>) = state.GetState()

          do!
            resulting_schema_without_hooks.Entities
            |> OrderedMap.values
            |> Seq.map (fun e_typechecked ->
              state {
                let! (_ctx: TypeCheckContext<'ve>) = state.GetContext()
                let! (_s: TypeCheckState<'ve>) = state.GetState()

                do!
                  TypeCheckState.bindType
                    (e_typechecked.Name.Name
                     |> Identifier.LocalScope
                     |> hooks_scope.Resolve)
                    (e_typechecked.TypeWithProps, Kind.Star)
              })
            |> state.All
            |> state.Ignore


          let! entities_with_hooks =
            parsed_schema.Entities
            |> Seq.map (fun e_parsed ->
              state {
                let! e_typechecked =
                  resulting_schema_without_hooks.Entities
                  |> OrderedMap.tryFind (
                    e_parsed.Name.Name |> SchemaEntityName.Create
                  )
                  |> Sum.fromOption (fun () ->
                    (fun () ->
                      $"Error: cannot find typechecked entity for parsed entity {e_parsed.Name}")
                    |> Errors.Singleton loc0)
                  |> state.OfSum

                let! typechecked_hooks =
                  typecheck_entity_hooks e_typechecked e_parsed.Hooks

                let e_typechecked =
                  { e_typechecked with
                      Hooks = typechecked_hooks }

                return e_parsed.Name, e_typechecked
              })
            |> state.All
            |> state.MapContext(
              TypeCheckContext.Updaters.Scope(
                TypeCheckScope.Empty |> replaceWith
              )
            )
            |> state.MapContext(
              TypeCheckContext.Updaters.TypeVariables(
                Map.add
                  "Schema"
                  (TypeValue.Schema resulting_schema_without_hooks, Kind.Schema)
              )
            )

          let entities_with_hooks = entities_with_hooks |> OrderedMap.ofList

          let entities_with_hooks =
            entities_with_hooks
            |> OrderedMap.mergeSecondAfterFirst included_entities

          let! included_relations_hooks =
            included_relations_hooks
            |> Map.toSeq
            |> Seq.map (fun (relationName, hooks) ->
              state {
                let! included_relation =
                  included_relations
                  |> OrderedMap.tryFind relationName
                  |> Sum.fromOption (fun () ->
                    (fun () ->
                      $"Error: cannot find included relation {relationName} for hooks")
                    |> Errors.Singleton loc0)
                  |> state.OfSum

                let! from_e =
                  entities_with_hooks
                  |> OrderedMap.tryFind (
                    included_relation.From.LocalName |> SchemaEntityName.Create
                  )
                  |> Sum.fromOption (fun () ->
                    (fun () ->
                      $"Error: cannot find entity {included_relation.From} for relation {included_relation.Name}")
                    |> Errors.Singleton loc0)
                  |> state.OfSum

                let! to_e =
                  entities_with_hooks
                  |> OrderedMap.tryFind (
                    included_relation.To.LocalName |> SchemaEntityName.Create
                  )
                  |> Sum.fromOption (fun () ->
                    (fun () ->
                      $"Error: cannot find entity {included_relation.To} for relation {included_relation.Name}")
                    |> Errors.Singleton loc0)
                  |> state.OfSum

                let relation_hook_type =
                  TypeValue.CreateArrow(
                    TypeValue.Schema resulting_schema_without_hooks,
                    TypeValue.CreateArrow(
                      from_e.Id,
                      TypeValue.CreateArrow(
                        to_e.Id,
                        TypeValue.CreateSum
                          [ TypeValue.CreateUnit()
                            TypeValue.CreatePrimitive PrimitiveType.String ]
                      )
                    )
                  )

                let assert_no_cardinality =
                  match included_relation.Cardinality with
                  | None ->
                    (fun () ->
                      $"Error: cannot define hooks on relation {included_relation.Name} with cardinality constraints")
                    |> Errors.Singleton loc0
                    |> state.Throw
                  | Some _ -> state { return () }


                let! typechecked_hooks =
                  typecheck_relation_hooks
                    assert_no_cardinality
                    relation_hook_type
                    hooks

                return relationName, typechecked_hooks
              })
            |> state.All
            |> state.Map(Map.ofSeq)

          let included_relations =
            included_relations_hooks
            |> Map.fold
              (fun acc relationName hooks ->
                match acc |> OrderedMap.tryFind relationName with
                | Some v ->
                  let v =
                    { v with
                        SchemaRelation.Hooks =
                          { SchemaRelationHooks.OnLinking =
                              v.Hooks.OnLinking
                              |> Option.orElse hooks.OnLinking
                            SchemaRelationHooks.OnLinked =
                              v.Hooks.OnLinked |> Option.orElse hooks.OnLinked
                            SchemaRelationHooks.OnUnlinking =
                              v.Hooks.OnUnlinking
                              |> Option.orElse hooks.OnUnlinking
                            SchemaRelationHooks.OnUnlinked =
                              v.Hooks.OnUnlinked
                              |> Option.orElse hooks.OnUnlinked } }

                  acc |> OrderedMap.add relationName v
                | None -> acc)
              included_relations

          let included_relations_hooks_names =
            included_relations_hooks
            |> Map.toSeq
            |> Seq.map (fst >> (fun n -> n.Name))
            |> Set.ofSeq

          let existing_entity_names =
            resulting_schema_without_hooks.Entities
            |> OrderedMap.keys
            |> Seq.map (fun n -> n.Name)
            |> Set.ofSeq

          let existing_relation_names =
            resulting_schema_without_hooks.Relations
            |> OrderedMap.keys
            |> Seq.map (fun n -> n.Name)
            |> Set.ofSeq

          if
            not (
              Set.isSubset included_entities_hooks_names existing_entity_names
            )
          then
            let conflicting_entity_names =
              included_entities_hooks_names
              |> Seq.filter (fun n ->
                not (Set.contains n existing_entity_names))

            let comma = ", "

            return!
              (fun () ->
                $"Error: cannot specify hooks for included entities that have the same name as entities in the including schema, see {conflicting_entity_names |> Seq.map (fun n -> n) |> String.join comma} for details")
              |> Errors.Singleton loc0
              |> state.Throw
          else if
            not (
              Set.isSubset
                included_relations_hooks_names
                existing_relation_names
            )
          then
            let conflicting_relation_names =
              included_relations_hooks_names
              |> Seq.filter (fun n ->
                not (Set.contains n existing_relation_names))

            let comma = ", "

            return!
              (fun () ->
                $"Error: cannot specify hooks for included relations that have the same name as relations in the including schema, see {conflicting_relation_names |> Seq.map (fun n -> n) |> String.join comma} for details")
              |> Errors.Singleton loc0
              |> state.Throw
          else
            ()

          let! relations_with_hooks =
            parsed_schema.Relations
            |> Seq.map (fun r_parsed ->
              state {
                let! r_typechecked =
                  resulting_schema_without_hooks.Relations
                  |> OrderedMap.tryFind (
                    r_parsed.Name.Name |> SchemaRelationName.Create
                  )
                  |> Sum.fromOption (fun () ->
                    (fun () ->
                      $"Error: cannot find typechecked relation for parsed relation {r_parsed.Name}")
                    |> Errors.Singleton loc0)
                  |> state.OfSum

                let! from_e =
                  entities_with_hooks
                  |> OrderedMap.tryFind (
                    (r_parsed.From |> fst).LocalName |> SchemaEntityName.Create
                  )
                  |> Sum.fromOption (fun () ->
                    (fun () ->
                      $"Error: cannot find entity {r_parsed.From} for relation {r_parsed.Name}")
                    |> Errors.Singleton loc0)
                  |> state.OfSum

                let! to_e =
                  entities_with_hooks
                  |> OrderedMap.tryFind (
                    (r_parsed.To |> fst).LocalName |> SchemaEntityName.Create
                  )
                  |> Sum.fromOption (fun () ->
                    (fun () ->
                      $"Error: cannot find entity {r_parsed.To} for relation {r_parsed.Name}")
                    |> Errors.Singleton loc0)
                  |> state.OfSum

                let relation_hook_type =
                  TypeValue.CreateArrow(
                    TypeValue.Schema resulting_schema_without_hooks,
                    TypeValue.CreateArrow(
                      from_e.Id,
                      TypeValue.CreateArrow(
                        to_e.Id,
                        TypeValue.CreateSum
                          [ TypeValue.CreateUnit()
                            TypeValue.CreatePrimitive PrimitiveType.String ]
                      )
                    )
                  )

                let assert_no_cardinality =
                  match r_parsed.Cardinality with
                  | None ->
                    (fun () ->
                      $"Error: cannot define hooks on relation {r_parsed.Name} with cardinality constraints")
                    |> Errors.Singleton loc0
                    |> state.Throw
                  | Some _ -> state { return () }

                let! r_typechecked_hooks =
                  typecheck_relation_hooks
                    assert_no_cardinality
                    relation_hook_type
                    r_parsed.Hooks

                return
                  r_parsed.Name,
                  { r_typechecked with
                      Hooks = r_typechecked_hooks }
              })
            |> state.All
            |> state.MapContext(
              TypeCheckContext.Updaters.Scope(
                TypeCheckScope.Empty |> replaceWith
              )
            )
            |> state.MapContext(
              TypeCheckContext.Updaters.TypeVariables(
                Map.add
                  "Schema"
                  (TypeValue.Schema resulting_schema_without_hooks, Kind.Schema)
              )
            )

          let relations_with_hooks = relations_with_hooks |> OrderedMap.ofList

          let relations_with_hooks =
            relations_with_hooks
            |> OrderedMap.mergeSecondAfterFirst included_relations

          do! state.SetState(state_to_restore |> replaceWith)

          let resulting_schema =
            { resulting_schema_without_hooks with
                Entities = entities_with_hooks
                Relations = relations_with_hooks }

          return TypeValue.Schema resulting_schema, Kind.Schema
    }

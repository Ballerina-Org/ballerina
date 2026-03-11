namespace Ballerina.DSL.Next.Types.TypeChecker

module Eval =
  open System
  open Ballerina.Fun
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.StdLib.Object
  open Ballerina.StdLib.String
  open Ballerina.State.WithError
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.Errors
  open Ballerina.Collections.Map
  open System
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.KindEval
  open Ballerina.DSL.Next.Types.TypeChecker.LiftOtherSteps
  open Ballerina.DSL.Next.Unification
  open Ballerina.DSL.Next.Terms.Patterns

  type TypeQueryRowExpr<'valueExt> with
    static member Eval<'ve when 've: comparison>
      (query_type_symbol: TypeSymbol)
      (mk_query_type: Schema<'ve> -> TypeQueryRow<'ve> -> TypeValue<'ve>)
      : TypeQueryRowExprEval<'ve> =
      fun typeCheckExpr _n loc0 q_row ->
        state {
          let (!) = TypeExpr.Eval<'ve> query_type_symbol mk_query_type typeCheckExpr None loc0
          // let (!!) = TypeQueryRowExpr.Eval<'ve> () typeCheckExpr None loc0
          // let! ctx = state.GetContext()

          // let error e = Errors.Singleton loc0 e

          let ofSum (p: Sum<'a, Errors<Unit>>) =
            p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

          match q_row with
          | TypeQueryRowExpr.PrimaryKey q ->
            let! q, q_k = !q
            do! q_k |> Kind.AsStar |> ofSum |> state.Ignore

            return TypeQueryRow.PrimaryKey q
          | TypeQueryRowExpr.Json q ->
            let! q, q_k = !q
            do! q_k |> Kind.AsStar |> ofSum |> state.Ignore

            return TypeQueryRow.Json q
          | TypeQueryRowExpr.PrimitiveType(pt, is_nullable) -> return TypeQueryRow.PrimitiveType(pt, is_nullable)
          | TypeQueryRowExpr.Tuple qs ->
            let! qs =
              qs
              |> List.map (fun q ->
                state {
                  let! q, q_k = !q
                  let! q = q |> TypeValue.AsQueryRow |> ofSum
                  do! q_k |> Kind.AsStar |> ofSum |> state.Ignore
                  return q
                })
              |> state.All

            return TypeQueryRow.Tuple qs
          | TypeQueryRowExpr.Record qs ->
            let! qs =
              qs
              |> Map.map (fun _k q ->
                state {
                  let! q, q_k = !q
                  let! q = q |> TypeValue.AsQueryRow |> ofSum
                  do! q_k |> Kind.AsStar |> ofSum |> state.Ignore
                  return q
                })
              |> state.AllMap

            return TypeQueryRow.Record qs
        }

  and TypeExpr<'valueExt> with
    static member EvalAsSymbol<'ve when 've: comparison>
      (query_type_symbol: TypeSymbol)
      (mk_query_type: Schema<'ve> -> TypeQueryRow<'ve> -> TypeValue<'ve>)
      : TypeExprSymbolEval<'ve> =
      fun exprTypeCheck loc0 t ->
        state {
          let (!) = TypeExpr.EvalAsSymbol query_type_symbol mk_query_type exprTypeCheck loc0
          let (!!) = TypeExpr.Eval query_type_symbol mk_query_type exprTypeCheck None loc0
          let! ctx = state.GetContext()

          let error e = Errors.Singleton loc0 e

          let ofSum (p: Sum<'a, Errors<Unit>>) =
            p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

          match t with
          | TypeExpr.NewSymbol name -> return TypeSymbol.Create(Identifier.LocalScope name)
          | TypeExpr.Lookup v ->

            return!
              reader.Any(
                NonEmptyList.OfList(
                  TypeCheckState.tryFindTypeSymbol (v |> ctx.Scope.Resolve, loc0),
                  [ TypeCheckState.tryFindRecordFieldSymbol (v |> ctx.Scope.Resolve, loc0)
                    TypeCheckState.tryFindUnionCaseSymbol (v |> ctx.Scope.Resolve, loc0)
                    TypeCheckState.tryFindTypeSymbol (v |> TypeCheckScope.Empty.Resolve, loc0)
                    TypeCheckState.tryFindRecordFieldSymbol (v |> TypeCheckScope.Empty.Resolve, loc0)
                    TypeCheckState.tryFindUnionCaseSymbol (v |> TypeCheckScope.Empty.Resolve, loc0) ]
                )
              )
              |> state.OfStateReader
          | TypeExpr.Apply(f, a) ->
            let! f, f_k = !!f
            do! Kind.AsArrow f_k |> ofSum |> state.Ignore
            let! a, a_k = !!a

            let! param, body = f |> TypeValue.AsLambda |> ofSum |> state.Map WithSourceMapping.Getters.Value

            let! ctx = state.GetContext()
            let closure = ctx.TypeVariables |> Map.add (param.Name) (a, a_k)

            return!
              !body
              |> state.MapContext(TypeCheckContext.Updaters.TypeVariables(replaceWith closure))
          | _ ->
            return!
              (fun () -> $"Error: invalid type expression when evaluating for symbol, got {t}")
              |> error
              |> state.Throw
        }

    static member Eval<'ve when 've: comparison>
      (query_type_symbol: TypeSymbol)
      (mk_query_type: Schema<'ve> -> TypeQueryRow<'ve> -> TypeValue<'ve>)
      : TypeExprEval<'ve> =
      fun typeCheckExpr n loc0 t ->
        state {
          let (!) = TypeExpr.Eval<'ve> query_type_symbol mk_query_type typeCheckExpr None loc0

          let (!!) =
            TypeExpr.EvalAsSymbol<'ve> query_type_symbol mk_query_type typeCheckExpr loc0

          let (!!!) =
            TypeQueryRowExpr.Eval<'ve> query_type_symbol mk_query_type typeCheckExpr None loc0

          let! ctx = state.GetContext()

          let error e = Errors.Singleton loc0 e

          let ofSum (p: Sum<'a, Errors<Unit>>) =
            p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

          let source =
            match n with
            | Some name -> TypeExprSourceMapping.OriginExprTypeLet(name, t)
            | None -> TypeExprSourceMapping.OriginTypeExpr t

          match t with
          | TypeExpr.Entities schema ->
            let! schema, schema_k = !schema
            do! schema_k |> Kind.AsSchema |> ofSum |> state.Ignore

            let! a_schema = schema |> TypeValue.AsSchema |> ofSum
            return TypeValue.CreateEntities a_schema, Kind.Star
          | TypeExpr.Relations schema ->
            let! schema, schema_k = !schema
            do! schema_k |> Kind.AsSchema |> ofSum |> state.Ignore

            let! a_schema = schema |> TypeValue.AsSchema |> ofSum
            return TypeValue.CreateRelations a_schema, Kind.Star
          | TypeExpr.Entity(s, e, e_with_props, id) ->
            let! s, s_k = !s
            do! s_k |> Kind.AsSchema |> ofSum |> state.Ignore
            let! e, e_k = !e
            do! e_k |> Kind.AsStar |> ofSum |> state.Ignore
            let! e_with_props, e_with_props_k = !e_with_props
            do! e_with_props_k |> Kind.AsStar |> ofSum |> state.Ignore
            let! id, id_k = !id
            do! id_k |> Kind.AsStar |> ofSum |> state.Ignore

            let! a_schema = s |> TypeValue.AsSchema |> ofSum
            return TypeValue.CreateEntity(a_schema, e, e_with_props, id), Kind.Star
          | TypeExpr.Relation(s, f, f_with_props, f_id, t, t_with_props, t_id) ->
            let! s, s_k = !s
            do! s_k |> Kind.AsSchema |> ofSum |> state.Ignore
            let! f, f_k = !f
            do! f_k |> Kind.AsStar |> ofSum |> state.Ignore
            let! f_with_props, f_with_props_k = !f_with_props
            do! f_with_props_k |> Kind.AsStar |> ofSum |> state.Ignore
            let! f_id, f_id_k = !f_id
            do! f_id_k |> Kind.AsStar |> ofSum |> state.Ignore
            let! t, t_k = !t
            do! t_k |> Kind.AsStar |> ofSum |> state.Ignore
            let! t_with_props, t_with_props_k = !t_with_props
            do! t_with_props_k |> Kind.AsStar |> ofSum |> state.Ignore
            let! t_id, t_id_k = !t_id
            do! t_id_k |> Kind.AsStar |> ofSum |> state.Ignore

            let! a_schema = s |> TypeValue.AsSchema |> ofSum

            return
              TypeValue.CreateRelation(
                a_schema,
                "@implicit" |> SchemaRelationName.Create,
                None,
                f,
                f_with_props,
                f_id,
                t,
                t_with_props,
                t_id
              ),
              Kind.Star
          | TypeExpr.RelationLookupOne(s, t', t_id, f_id) ->
            let! s, s_k = !s
            do! s_k |> Kind.AsSchema |> ofSum |> state.Ignore
            let! t', t'_k = !t'
            do! t'_k |> Kind.AsStar |> ofSum |> state.Ignore
            let! f_id, f_id_k = !f_id
            do! f_id_k |> Kind.AsStar |> ofSum |> state.Ignore
            let! t_id, t_id_k = !t_id
            do! t_id_k |> Kind.AsStar |> ofSum |> state.Ignore

            let! a_schema = s |> TypeValue.AsSchema |> ofSum

            return TypeValue.CreateRelationLookupOne(a_schema, t', f_id, t_id), Kind.Star
          | TypeExpr.RelationLookupOption(s, t', t_id, f_id) ->
            let! s, s_k = !s
            do! s_k |> Kind.AsSchema |> ofSum |> state.Ignore
            let! t', t'_k = !t'
            do! t'_k |> Kind.AsStar |> ofSum |> state.Ignore
            let! f_id, f_id_k = !f_id
            do! f_id_k |> Kind.AsStar |> ofSum |> state.Ignore
            let! t_id, t_id_k = !t_id
            do! t_id_k |> Kind.AsStar |> ofSum |> state.Ignore

            let! a_schema = s |> TypeValue.AsSchema |> ofSum

            return TypeValue.CreateRelationLookupOption(a_schema, t', f_id, t_id), Kind.Star
          | TypeExpr.RelationLookupMany(s, t', t_id, f_id) ->
            let! s, s_k = !s
            do! s_k |> Kind.AsSchema |> ofSum |> state.Ignore
            let! t', t'_k = !t'
            do! t'_k |> Kind.AsStar |> ofSum |> state.Ignore
            let! f_id, f_id_k = !f_id
            do! f_id_k |> Kind.AsStar |> ofSum |> state.Ignore
            let! t_id, t_id_k = !t_id
            do! t_id_k |> Kind.AsStar |> ofSum |> state.Ignore

            let! a_schema = s |> TypeValue.AsSchema |> ofSum

            return TypeValue.CreateRelationLookupMany(a_schema, t', f_id, t_id), Kind.Star
          | TypeExpr.Schema parsed_schema ->
            let! included_schema_entityhooks_relation_hooks =
              match parsed_schema.Includes with
              | Some(includeName, entity_hooks, relation_hooks) ->
                state {
                  let! included_schema, included_schema_k =
                    TypeCheckState.tryFindType (
                      includeName.Name |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve,
                      loc0
                    )
                    |> state.OfStateReader

                  do! included_schema_k |> Kind.AsSchema |> ofSum |> state.Ignore

                  let! included_schema = included_schema |> TypeValue.AsSchema |> ofSum

                  return Some(included_schema, entity_hooks, relation_hooks)
                }
              | None -> state { return None }

            let repeatedEntityNames =
              parsed_schema.Entities
              |> List.map (fun e -> e.Name)
              |> List.append (
                included_schema_entityhooks_relation_hooks
                |> Option.map (fun (s, _, _) -> s.Entities |> OrderedMap.values |> List.map (fun e -> e.Name))
                |> Option.defaultValue []
              )
              |> List.groupBy id
              |> List.filter (fun (_, l) -> List.length l > 1)
              |> List.map (fst >> fun n -> n.Name)

            if not (List.isEmpty repeatedEntityNames) then
              return!
                let sep = ", " in

                (fun () -> $"Error: schema has repeated entity names: {String.join sep repeatedEntityNames}")
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

                          if path_segments |> List.isEmpty && (p.Name.Name = "Id" || p.Name.Name = "Value") then
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
                                    | (maybe_var_name, SchemaPathTypeDecompositionExpr.Field f) ->
                                      let! t_record = t |> TypeValue.AsRecordWithSourceMapping |> ofSum
                                      let t_record_scope = t_record.typeCheckScopeSource
                                      let t_record = t_record.value

                                      let! (f_i, (f_t, f_k)) =
                                        t_record
                                        |> OrderedMap.toSeq
                                        |> Seq.tryFind (fun (k, _) -> k.Name.LocalName = f.LocalName)
                                        |> Sum.fromOption (fun () ->
                                          (fun () -> $"Error: cannot find field {f} in record type {t}")
                                          |> Errors.Singleton loc0)
                                        |> state.OfSum

                                      do! f_k |> Kind.AsStar |> ofSum |> state.Ignore

                                      let next_t = f_t

                                      let next_segments =
                                        match maybe_var_name with
                                        | Some var_name ->
                                          (Identifier.LocalScope var_name.Name |> TypeCheckScope.Empty.Resolve,
                                           (next_t, f_k))
                                          :: segments_acc
                                        | None -> segments_acc

                                      return
                                        next_t,
                                        next_segments,
                                        (maybe_var_name,
                                         f_i.Name |> t_record_scope.Resolve |> SchemaPathTypeDecomposition.Field)
                                        :: resolved_scope
                                    | (maybe_var_name, SchemaPathTypeDecompositionExpr.UnionCase f) ->
                                      let! _, t_union_scope, t_union =
                                        t |> TypeValue.AsUnionWithSourceMapping |> ofSum

                                      let! (case_i, case_t) =
                                        t_union
                                        |> OrderedMap.toSeq
                                        |> Seq.tryFind (fun (k, _) -> k.Name.LocalName = f.LocalName)
                                        |> Sum.fromOption (fun () ->
                                          (fun () -> $"Error: cannot find case {f} in union type {t}")
                                          |> Errors.Singleton loc0)
                                        |> state.OfSum

                                      let next_t = case_t

                                      let next_segments =
                                        match maybe_var_name with
                                        | Some var_name ->
                                          (Identifier.LocalScope var_name.Name |> TypeCheckScope.Empty.Resolve,
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
                                    | (maybe_var_name, SchemaPathTypeDecompositionExpr.SumCase f) ->
                                      let! t_sum = t |> TypeValue.AsSum |> ofSum

                                      if f.Case < 1 || f.Case > t_sum.Length || f.Count <> t_sum.Length then
                                        return!
                                          (fun () -> $"Error: sum case {f} is out of bounds for sum type {t}")
                                          |> Errors.Singleton loc0
                                          |> state.Throw
                                      else
                                        let! case_t =
                                          t_sum
                                          |> Seq.tryItem (f.Case - 1)
                                          |> Sum.fromOption (fun () ->
                                            (fun () -> $"Error: cannot find sum case {f} in sum type {t}")
                                            |> Errors.Singleton loc0)
                                          |> state.OfSum

                                        let next_t = case_t

                                        let next_segments =
                                          match maybe_var_name with
                                          | Some var_name ->
                                            (Identifier.LocalScope var_name.Name |> TypeCheckScope.Empty.Resolve,
                                             (next_t, Kind.Star))
                                            :: segments_acc
                                          | None -> segments_acc

                                        return
                                          next_t,
                                          next_segments,
                                          (maybe_var_name, SchemaPathTypeDecomposition.SumCase f) :: resolved_scope
                                    | (maybe_var_name, SchemaPathTypeDecompositionExpr.Item f) ->
                                      let! t_tuple = t |> TypeValue.AsTuple |> ofSum

                                      if f.Index < 1 || f.Index > t_tuple.Length then
                                        return!
                                          (fun () -> $"Error: tuple index {f} is out of bounds for tuple type {t}")
                                          |> Errors.Singleton loc0
                                          |> state.Throw
                                      else
                                        let! item_t =
                                          t_tuple
                                          |> Seq.tryItem (f.Index - 1)
                                          |> Sum.fromOption (fun () ->
                                            (fun () -> $"Error: cannot find tuple index {f} in tuple type {t}")
                                            |> Errors.Singleton loc0)
                                          |> state.OfSum

                                        let next_t = item_t

                                        let next_segments =
                                          match maybe_var_name with
                                          | Some var_name ->
                                            (Identifier.LocalScope var_name.Name |> TypeCheckScope.Empty.Resolve,
                                             (next_t, Kind.Star))
                                            :: segments_acc
                                          | None -> segments_acc

                                        return
                                          next_t,
                                          next_segments,
                                          (maybe_var_name, SchemaPathTypeDecomposition.Item f) :: resolved_scope
                                    | (maybe_var_name, SchemaPathTypeDecompositionExpr.Iterator it) ->
                                      let! container, _ = it.Container |> TypeExpr.Lookup |> (!)
                                      let! t_arg, _ = it.TypeDef |> TypeExpr.Lookup |> (!)
                                      let! mapper, _, _, _ = typeCheckExpr None (it.Mapper |> Expr.Lookup)

                                      let next_t = t_arg

                                      let next_segments =
                                        match maybe_var_name with
                                        | Some var_name ->
                                          (Identifier.LocalScope var_name.Name |> TypeCheckScope.Empty.Resolve,
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

                            let! body_e, body_t, body_k, _ =
                              typeCheckExpr (Some p_decl_t) p.Body
                              |> state.MapContext(
                                TypeCheckContext.Updaters.Values(
                                  Map.add (Identifier.LocalScope "self" |> TypeCheckScope.Empty.Resolve) (t, t_k)
                                  >> path_scope
                                )
                                >> TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith)
                              )

                            do! body_k |> Kind.AsStar |> ofSum |> state.Ignore

                            do! TypeValue.Unify(loc0, body_t, p_decl_t) |> Expr.liftUnification

                            return
                              { SchemaEntityProperty.Path = resolved_scope
                                PropertyName = p.Name
                                ReturnType = p_decl_t
                                ReturnKind = p_decl_k
                                Body = body_e }
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

                            let! body_e, body_t, body_k, _ =
                              typeCheckExpr (Some(TypeValue.CreatePrimitive(PrimitiveType.String))) p.Body
                              |> state.MapContext(
                                TypeCheckContext.Updaters.Values(
                                  Map.add (Identifier.LocalScope "self" |> TypeCheckScope.Empty.Resolve) (t, t_k)
                                )
                                >> TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith)
                              )

                            do! body_k |> Kind.AsStar |> ofSum |> state.Ignore

                            do!
                              TypeValue.Unify(loc0, body_t, TypeValue.CreatePrimitive(PrimitiveType.String))
                              |> Expr.liftUnification

                            return
                              { SchemaEntityVector.VectorName = p.Name
                                Body = body_e }
                        })
                      |> state.All
                      |> state.Map(List.ofSeq)

                    let rec (+)
                      (t: TypeValue<'ve>)
                      (path: List<SchemaPathSegment<'ve>>, name: LocalIdentifier, result_t: TypeValue<'ve>)
                      =
                      state {
                        match path with
                        | [] ->
                          let! fields = t |> TypeValue.AsRecord |> ofSum

                          if
                            fields
                            |> OrderedMap.toSeq
                            |> Seq.filter (fun (k, _) -> k.Name.LocalName = name.Name)
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
                                (name.Name |> Identifier.LocalScope |> TypeSymbol.Create)
                                (result_t, Kind.Star)

                            return TypeValue.CreateRecord fields
                        | (_, SchemaPathTypeDecomposition.Field f) :: path ->
                          let! fields = t |> TypeValue.AsRecord |> ofSum

                          let! f_s, (f_t, f_k) =
                            fields
                            |> OrderedMap.toSeq
                            |> Seq.tryFind (fun (k, _) -> k.Name.LocalName = f.Name)
                            |> Sum.fromOption (fun () ->
                              (fun () -> $"Error: cannot find field {f} in record type {t}")
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
                              (fun () -> $"Error: cannot find field {f} in record type {t}")
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
                              (fun () -> $"Error: cannot find field {f} in record type {t}")
                              |> Errors.Singleton loc0)
                            |> state.OfSum

                          let! f_t = f_t + (path, name, result_t)

                          let fields = fields |> List.mapi (fun i ft -> if i = f.Case - 1 then f_t else ft)

                          return TypeValue.CreateSum fields
                        | (_, SchemaPathTypeDecomposition.Item f) :: path ->
                          let! fields = t |> TypeValue.AsTuple |> ofSum

                          let! f_t =
                            fields
                            |> Seq.tryItem (f.Index - 1)
                            |> Sum.fromOption (fun () ->
                              (fun () -> $"Error: cannot find field {f} in record type {t}")
                              |> Errors.Singleton loc0)
                            |> state.OfSum

                          let! f_t = f_t + (path, name, result_t)

                          let fields = fields |> List.mapi (fun i ft -> if i = f.Index - 1 then f_t else ft)

                          return TypeValue.CreateTuple fields
                        | (_, SchemaPathTypeDecomposition.Iterator f) :: path ->
                          let f_t = f.TypeDef
                          let! f_t = f_t + (path, name, result_t)

                          let! t_res, _ =
                            !TypeExpr.Apply(TypeExpr.FromTypeValue f.Container, TypeExpr.FromTypeValue f_t)

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

                    let rec (+) (t: TypeValue<'ve>) (name: LocalIdentifier) =
                      state {
                        let! fields = t |> TypeValue.AsRecord |> ofSum

                        let vector_name_already_exists =
                          fields
                          |> OrderedMap.toSeq
                          |> Seq.tryFind (fun (k, _) -> k.Name.LocalName = name.Name)

                        if vector_name_already_exists.IsSome then
                          return!
                            (fun () ->
                              $"Error: a field with the same name as vector {name.Name} already exists in record type {t}")
                            |> Errors.Singleton loc0
                            |> state.Throw

                        else
                          let fields =
                            fields
                            |> OrderedMap.add
                              (name.Name |> Identifier.LocalScope |> TypeSymbol.Create)
                              (TypeValue.CreatePrimitive PrimitiveType.Vector, Kind.Star)

                          return TypeValue.CreateRecord fields
                      }

                    let! t_with_props =
                      vectors
                      |> Seq.fold
                        (fun acc (p: SchemaEntityVector<'ve>) ->
                          state {
                            let! t = acc
                            return! t + p.VectorName
                          })
                        (state { return t_with_props })

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
                    let! id_fields = e.Id |> TypeValue.AsRecord |> ofSum

                    let id_fields =
                      id_fields
                      |> OrderedMap.toSeq
                      |> Seq.map (fun (k, (t, _)) -> (k.Name.LocalName, t))
                      |> Seq.toList

                    match id_fields with
                    | [ field_name, field_type ] ->
                      match field_type with
                      | TypeValue.Primitive({ value = PrimitiveType.Guid })
                      | TypeValue.Primitive({ value = PrimitiveType.String })
                      | TypeValue.Primitive({ value = PrimitiveType.Int32 })
                      | TypeValue.Primitive({ value = PrimitiveType.Int64 }) -> return field_name, field_type
                      | _ ->
                        return!
                          (fun () ->
                            $"Error: entity id field type can only be Guid, String, Int32, or Int64, got {field_type}")
                          |> error
                          |> state.Throw
                    | _ ->
                      return!
                        (fun () -> $"Error: entity id must be a single field record, got {e.Id}")
                        |> error
                        |> state.Throw
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

                  (fun () -> $"Error: schema has repeated relation names: {String.join sep repeatedRelationNames}")
                  |> error
                  |> state.Throw
              else
                let! context = state.GetContext()

                let rec validatePath
                  (source: TypeValue<'ve>)
                  (target: TypeValue<'ve>)
                  (path: SchemaPathSegmentExpr list)
                  =
                  state {
                    match path with
                    | [] -> do! TypeValue.Unify(loc0, source, target) |> Expr.liftUnification
                    | (_, segment) :: rest ->
                      match segment with
                      | SchemaPathTypeDecompositionExpr.Field fieldName ->
                        let! sourceRecord = source |> TypeValue.AsRecord |> ofSum

                        let! (_, (fieldType, _)) =
                          sourceRecord
                          |> OrderedMap.toSeq
                          |> Seq.tryFind (fun (k, _) -> k.Name.LocalName = fieldName.LocalName)
                          |> Sum.fromOption (fun () ->
                            (fun () -> $"Error: cannot find field {fieldName} in record type {source}")
                            |> Errors.Singleton loc0)
                          |> state.OfSum

                        return! validatePath fieldType target rest
                      | SchemaPathTypeDecompositionExpr.UnionCase caseName ->
                        let! _, sourceCase = source |> TypeValue.AsUnion |> ofSum

                        let! (_, caseType) =
                          sourceCase
                          |> OrderedMap.toSeq
                          |> Seq.tryFind (fun (k, _) -> k.Name.LocalName = caseName.LocalName)
                          |> Sum.fromOption (fun () ->
                            (fun () -> $"Error: cannot find case {caseName} in union type {source}")
                            |> Errors.Singleton loc0)
                          |> state.OfSum

                        return! validatePath caseType target rest
                      | SchemaPathTypeDecompositionExpr.SumCase caseName ->
                        let! sourceCase = source |> TypeValue.AsSum |> ofSum

                        if
                          caseName.Case < 1
                          || caseName.Case > sourceCase.Length
                          || caseName.Count <> sourceCase.Length
                        then
                          return!
                            (fun () -> $"Error: sum case {caseName} is out of bounds for sum type {source}")
                            |> Errors.Singleton loc0
                            |> state.Throw
                        else
                          let! caseType =
                            sourceCase
                            |> Seq.tryItem (caseName.Case - 1)
                            |> Sum.fromOption (fun () ->
                              (fun () -> $"Error: cannot find sum case {caseName} in sum type {source}")
                              |> Errors.Singleton loc0)
                            |> state.OfSum

                          return! validatePath caseType target rest
                      | SchemaPathTypeDecompositionExpr.Item item ->
                        let! sourceCase = source |> TypeValue.AsTuple |> ofSum

                        if item.Index < 1 || item.Index > sourceCase.Length then
                          return!
                            (fun () -> $"Error: tuple index {item} is out of bounds for tuple type {source}")
                            |> Errors.Singleton loc0
                            |> state.Throw
                        else
                          let! caseType =
                            sourceCase
                            |> Seq.tryItem (item.Index - 1)
                            |> Sum.fromOption (fun () ->
                              (fun () -> $"Error: cannot find tuple index {item} in tuple type {source}")
                              |> Errors.Singleton loc0)
                            |> state.OfSum

                          return! validatePath caseType target rest
                      | SchemaPathTypeDecompositionExpr.Iterator it ->
                        let! container, _ = it.Container |> TypeExpr.Lookup |> (!)
                        let! t_arg, _ = it.TypeDef |> TypeExpr.Lookup |> (!)

                        let! t_map, _ =
                          context.Values
                          |> Map.tryFindWithError
                            (it.Mapper |> TypeCheckScope.Empty.Resolve)
                            "mapper function"
                            (fun () -> it.Mapper.LocalName)
                            loc0
                          |> state.OfSum

                        let! t_map, _ =
                          !TypeExpr.Apply(TypeExpr.Apply(TypeExpr.FromTypeValue t_map, TypeExpr.FromTypeValue t_arg),
                                          TypeExpr.FromTypeValue t_arg)

                        let! expected, _ =
                          !(TypeExpr.Apply(TypeExpr.FromTypeValue container, TypeExpr.FromTypeValue t_arg))

                        let! expected, _ =
                          !(TypeExpr.Arrow(
                            TypeExpr.Arrow(TypeExpr.FromTypeValue t_arg, TypeExpr.FromTypeValue t_arg),
                            TypeExpr.Arrow(TypeExpr.FromTypeValue source, TypeExpr.FromTypeValue expected)
                          ))

                        do! TypeValue.Unify(loc0, t_map, expected) |> Expr.liftUnification

                        return! validatePath t_arg target rest
                  }

                let! relations =
                  parsed_schema.Relations
                  |> List.map (fun r ->
                    r.Name,
                    state {
                      let fromPath = r.From |> snd
                      let toPath = r.To |> snd

                      let! fromEntity =
                        entities
                        |> OrderedMap.tryFind ((r.From |> fst).LocalName |> SchemaEntityName.Create)
                        |> Sum.fromOption (fun () ->
                          (fun () -> $"Error: cannot find entity {r.From} for relation {r.Name}")
                          |> Errors.Singleton loc0)
                        |> state.OfSum

                      let! toEntity =
                        entities
                        |> OrderedMap.tryFind ((r.To |> fst).LocalName |> SchemaEntityName.Create)
                        |> Sum.fromOption (fun () ->
                          (fun () -> $"Error: cannot find entity {r.To} for relation {r.Name}")
                          |> Errors.Singleton loc0)
                        |> state.OfSum

                      match fromPath with
                      | Some fromPath -> do! validatePath fromEntity.TypeOriginal toEntity.Id fromPath
                      | None -> ()

                      match toPath with
                      | Some toPath -> do! validatePath toEntity.TypeOriginal fromEntity.Id toPath
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
                    Entities = entities |> OrderedMap.mergeSecondAfterFirst included_entities
                    Relations = relations |> OrderedMap.mergeSecondAfterFirst included_relations }

                let typecheck_entity_hooks
                  (e_typechecked: SchemaEntity<'ve>)
                  (parsed_hooks: SchemaEntityHooksExpr<'ve>)
                  =
                  state {
                    let error_type = TypeValue.Lookup(Identifier.FullyQualified([], "Error"))

                    let typechecked_hooks =
                      { OnCreating = None
                        OnCreated = None
                        OnUpdating = None
                        OnUpdated = None
                        OnDeleting = None
                        OnDeleted = None
                        OnBackground = None
                        CanCreate = None
                        CanRead = None
                        CanUpdate = None
                        CanDelete = None }

                    let! typechecked_hooks =
                      state {
                        match parsed_hooks.OnCreating with
                        | None ->
                          return
                            { typechecked_hooks with
                                OnCreating = None }
                        | Some on_creating ->
                          let! on_creating_expr, on_creating_t, on_creating_k, _ = typeCheckExpr None on_creating
                          do! on_creating_k |> Kind.AsStar |> ofSum |> state.Ignore

                          do!
                            TypeValue.Unify(
                              on_creating.Location,
                              on_creating_t,
                              // fun (schema:Schema) (e_id:AID) (e:A) (e_with_props:Schema::As)
                              TypeValue.CreateArrow(
                                TypeValue.Schema resulting_schema_without_hooks,
                                TypeValue.CreateArrow(
                                  e_typechecked.Id,
                                  TypeValue.CreateArrow(
                                    e_typechecked.TypeOriginal,
                                    TypeValue.CreateArrow(
                                      e_typechecked.TypeWithProps,
                                      TypeValue.CreateSum
                                        [ TypeValue.CreateUnit(); error_type; e_typechecked.TypeOriginal ]
                                    )
                                  )
                                )
                              )
                            )
                            |> Expr.liftUnification
                            |> state.MapContext(TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith))

                          return
                            { typechecked_hooks with
                                OnCreating = Some on_creating_expr }
                      }

                    let! typechecked_hooks =
                      state {
                        match parsed_hooks.OnCreated with
                        | None ->
                          return
                            { typechecked_hooks with
                                OnCreated = None }
                        | Some on_created ->
                          let! on_created_expr, on_created_t, on_created_k, _ = typeCheckExpr None on_created
                          do! on_created_k |> Kind.AsStar |> ofSum |> state.Ignore

                          do!
                            TypeValue.Unify(
                              on_created.Location,
                              on_created_t,
                              // fun (schema:Schema) (e_id:AID) (e:A) (e_with_props:Schema::As)
                              TypeValue.CreateArrow(
                                TypeValue.Schema resulting_schema_without_hooks,
                                TypeValue.CreateArrow(
                                  e_typechecked.Id,
                                  TypeValue.CreateArrow(
                                    e_typechecked.TypeWithProps,
                                    TypeValue.CreateSum [ TypeValue.CreateUnit(); error_type ]
                                  )
                                )
                              )
                            )
                            |> Expr.liftUnification
                            |> state.MapContext(TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith))

                          return
                            { typechecked_hooks with
                                OnCreated = Some on_created_expr }
                      }

                    let! typechecked_hooks =
                      state {
                        match parsed_hooks.OnUpdating with
                        | None ->
                          return
                            { typechecked_hooks with
                                OnUpdating = None }
                        | Some on_updating ->
                          let! on_updating_expr, on_updating_t, on_updating_k, _ = typeCheckExpr None on_updating
                          do! on_updating_k |> Kind.AsStar |> ofSum |> state.Ignore

                          do!
                            TypeValue.Unify(
                              on_updating.Location,
                              on_updating_t,
                              // fun (schema:Schema) (e_id:AID) (e:A) (e_with_props:Schema::As)
                              TypeValue.CreateArrow(
                                TypeValue.Schema resulting_schema_without_hooks,
                                TypeValue.CreateArrow(
                                  e_typechecked.Id,
                                  TypeValue.CreateArrow(
                                    e_typechecked.TypeWithProps,
                                    TypeValue.CreateArrow(
                                      e_typechecked.TypeOriginal,
                                      TypeValue.CreateArrow(
                                        e_typechecked.TypeWithProps,
                                        TypeValue.CreateSum
                                          [ TypeValue.CreateUnit(); error_type; e_typechecked.TypeOriginal ]
                                      )
                                    )
                                  )
                                )
                              )
                            )
                            |> Expr.liftUnification
                            |> state.MapContext(TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith))

                          return
                            { typechecked_hooks with
                                OnUpdating = Some on_updating_expr }
                      }

                    let! typechecked_hooks =
                      state {
                        match parsed_hooks.OnUpdated with
                        | None ->
                          return
                            { typechecked_hooks with
                                OnUpdated = None }
                        | Some on_updated ->
                          let! on_updated_expr, on_updated_t, on_updated_k, _ = typeCheckExpr None on_updated
                          do! on_updated_k |> Kind.AsStar |> ofSum |> state.Ignore

                          do!
                            TypeValue.Unify(
                              on_updated.Location,
                              on_updated_t,
                              // fun (schema:Schema) (e_id:AID) (e:A) (e_with_props:Schema::As)
                              TypeValue.CreateArrow(
                                TypeValue.Schema resulting_schema_without_hooks,
                                TypeValue.CreateArrow(
                                  e_typechecked.Id,
                                  TypeValue.CreateArrow(
                                    e_typechecked.TypeWithProps,
                                    TypeValue.CreateArrow(
                                      e_typechecked.TypeWithProps,
                                      TypeValue.CreateSum [ TypeValue.CreateUnit(); error_type ]
                                    )
                                  )
                                )
                              )
                            )
                            |> Expr.liftUnification
                            |> state.MapContext(TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith))

                          return
                            { typechecked_hooks with
                                OnUpdated = Some on_updated_expr }
                      }

                    let! typechecked_hooks =
                      state {
                        match parsed_hooks.OnDeleting with
                        | None ->
                          return
                            { typechecked_hooks with
                                OnDeleting = None }
                        | Some on_deleting ->
                          let! on_deleting_expr, on_deleting_t, on_deleting_k, _ = typeCheckExpr None on_deleting
                          do! on_deleting_k |> Kind.AsStar |> ofSum |> state.Ignore

                          do!
                            TypeValue.Unify(
                              on_deleting.Location,
                              on_deleting_t,
                              // fun (schema:Schema) (e_id:AID) (e:A) (e_with_props:Schema::As)
                              TypeValue.CreateArrow(
                                TypeValue.Schema resulting_schema_without_hooks,
                                TypeValue.CreateArrow(
                                  e_typechecked.Id,
                                  TypeValue.CreateArrow(
                                    e_typechecked.TypeWithProps,
                                    TypeValue.CreateSum [ TypeValue.CreateUnit(); error_type ]
                                  )
                                )
                              )
                            )
                            |> Expr.liftUnification
                            |> state.MapContext(TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith))

                          return
                            { typechecked_hooks with
                                OnDeleting = Some on_deleting_expr }
                      }

                    let! typechecked_hooks =
                      state {
                        match parsed_hooks.OnDeleted with
                        | None ->
                          return
                            { typechecked_hooks with
                                OnDeleted = None }
                        | Some on_deleted ->
                          let! on_deleted_expr, on_deleted_t, on_deleted_k, _ = typeCheckExpr None on_deleted
                          do! on_deleted_k |> Kind.AsStar |> ofSum |> state.Ignore

                          do!
                            TypeValue.Unify(
                              on_deleted.Location,
                              on_deleted_t,
                              // fun (schema:Schema) (e_id:AID) (e:A) (e_with_props:Schema::As)
                              TypeValue.CreateArrow(
                                TypeValue.Schema resulting_schema_without_hooks,
                                TypeValue.CreateArrow(
                                  e_typechecked.Id,
                                  TypeValue.CreateArrow(
                                    e_typechecked.TypeWithProps,
                                    TypeValue.CreateSum [ TypeValue.CreateUnit(); error_type ]
                                  )
                                )
                              )
                            )
                            |> Expr.liftUnification
                            |> state.MapContext(TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith))

                          return
                            { typechecked_hooks with
                                OnDeleted = Some on_deleted_expr }
                      }

                    let! typechecked_hooks =
                      state {
                        match parsed_hooks.OnBackground with
                        | None ->
                          return
                            { typechecked_hooks with
                                OnBackground = None }
                        | Some on_background ->
                          let! ctx = state.GetContext()
                          let extra_scope = ctx.BackgroundHooksExtraScope |> Map.map (fun _ v -> v |> fst)

                          let! on_background_expr, on_background_t, on_background_k, _ =
                            typeCheckExpr None on_background
                            |> state.MapContext(TypeCheckContext.Updaters.Values(Map.merge (fun _ -> id) extra_scope))

                          do! on_background_k |> Kind.AsStar |> ofSum |> state.Ignore

                          do!
                            TypeValue.Unify(
                              on_background.Location,
                              on_background_t,
                              // fun (schema:Schema) (e_id:AID) (e:A) (e_with_props:Schema::As)
                              TypeValue.CreateArrow(
                                TypeValue.Schema resulting_schema_without_hooks,
                                TypeValue.CreateArrow(
                                  e_typechecked.Id,
                                  TypeValue.CreateArrow(
                                    e_typechecked.TypeWithProps,
                                    TypeValue.CreateSum [ TypeValue.CreateUnit(); TypeValue.CreateTimeSpan() ]
                                  )
                                )
                              )
                            )
                            |> Expr.liftUnification
                            |> state.MapContext(TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith))

                          return
                            { typechecked_hooks with
                                OnBackground = Some on_background_expr }
                      }

                    let! typechecked_hooks =
                      state {
                        match parsed_hooks.CanCreate with
                        | None ->
                          return
                            { typechecked_hooks with
                                CanCreate = None }
                        | Some can_create ->

                          let! ctx = state.GetContext()
                          let extra_scope = ctx.PermissionHooksExtraScope |> Map.map (fun _ v -> v |> fst)

                          let! can_create_expr, can_create_t, can_create_k, _ =
                            typeCheckExpr None can_create
                            |> state.MapContext(TypeCheckContext.Updaters.Values(Map.merge (fun _ -> id) extra_scope))

                          do! can_create_k |> Kind.AsStar |> ofSum |> state.Ignore

                          do!
                            TypeValue.Unify(
                              can_create.Location,
                              can_create_t,
                              TypeValue.CreateArrow(
                                TypeValue.Schema resulting_schema_without_hooks,
                                TypeValue.CreateBool()
                              )
                            )
                            |> Expr.liftUnification
                            |> state.MapContext(TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith))

                          return
                            { typechecked_hooks with
                                CanCreate = Some can_create_expr }
                      }

                    let! typechecked_hooks =
                      state {
                        match parsed_hooks.CanRead with
                        | None ->
                          return
                            { typechecked_hooks with
                                CanRead = None }
                        | Some can_read ->

                          let! ctx = state.GetContext()
                          let extra_scope = ctx.PermissionHooksExtraScope |> Map.map (fun _ v -> v |> fst)

                          let! can_read_expr, can_read_t, can_read_k, _ =
                            typeCheckExpr None can_read
                            |> state.MapContext(TypeCheckContext.Updaters.Values(Map.merge (fun _ -> id) extra_scope))

                          do! can_read_k |> Kind.AsStar |> ofSum |> state.Ignore

                          do!
                            TypeValue.Unify(
                              can_read.Location,
                              can_read_t,
                              TypeValue.CreateArrow(
                                TypeValue.Schema resulting_schema_without_hooks,
                                mk_query_type resulting_schema_without_hooks (TypeQueryRow.PrimaryKey e_typechecked.Id)
                              )
                            )
                            |> Expr.liftUnification
                            |> state.MapContext(TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith))

                          return
                            { typechecked_hooks with
                                CanRead = Some can_read_expr }
                      }

                    let! typechecked_hooks =
                      state {
                        match parsed_hooks.CanUpdate with
                        | None ->
                          return
                            { typechecked_hooks with
                                CanUpdate = None }
                        | Some can_update ->

                          let! ctx = state.GetContext()
                          let extra_scope = ctx.PermissionHooksExtraScope |> Map.map (fun _ v -> v |> fst)

                          let! can_update_expr, can_update_t, can_update_k, _ =
                            typeCheckExpr None can_update
                            |> state.MapContext(TypeCheckContext.Updaters.Values(Map.merge (fun _ -> id) extra_scope))

                          do! can_update_k |> Kind.AsStar |> ofSum |> state.Ignore

                          do!
                            TypeValue.Unify(
                              can_update.Location,
                              can_update_t,
                              TypeValue.CreateArrow(
                                TypeValue.Schema resulting_schema_without_hooks,
                                mk_query_type resulting_schema_without_hooks (TypeQueryRow.PrimaryKey e_typechecked.Id)
                              )
                            )
                            |> Expr.liftUnification
                            |> state.MapContext(TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith))

                          return
                            { typechecked_hooks with
                                CanUpdate = Some can_update_expr }
                      }

                    let! typechecked_hooks =
                      state {
                        match parsed_hooks.CanDelete with
                        | None ->
                          return
                            { typechecked_hooks with
                                CanDelete = None }
                        | Some can_delete ->

                          let! ctx = state.GetContext()
                          let extra_scope = ctx.PermissionHooksExtraScope |> Map.map (fun _ v -> v |> fst)

                          let! can_delete_expr, can_delete_t, can_delete_k, _ =
                            typeCheckExpr None can_delete
                            |> state.MapContext(TypeCheckContext.Updaters.Values(Map.merge (fun _ -> id) extra_scope))

                          do! can_delete_k |> Kind.AsStar |> ofSum |> state.Ignore

                          do!
                            TypeValue.Unify(
                              can_delete.Location,
                              can_delete_t,
                              TypeValue.CreateArrow(
                                TypeValue.Schema resulting_schema_without_hooks,
                                mk_query_type resulting_schema_without_hooks (TypeQueryRow.PrimaryKey e_typechecked.Id)
                              )
                            )
                            |> Expr.liftUnification
                            |> state.MapContext(TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith))

                          return
                            { typechecked_hooks with
                                CanDelete = Some can_delete_expr }
                      }

                    return typechecked_hooks
                  }


                let typecheck_relation_hooks
                  (assert_no_cardinality: State<unit, TypeCheckContext<'ve>, TypeCheckState<'ve>, Errors<Location>>)
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
                          let! on_linking_expr, on_linking_t, on_linking_k, _ = typeCheckExpr None on_linking
                          do! on_linking_k |> Kind.AsStar |> ofSum |> state.Ignore

                          do!
                            TypeValue.Unify(on_linking.Location, on_linking_t, relation_hook_type)
                            |> Expr.liftUnification
                            |> state.MapContext(TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith))

                          return
                            { r_typechecked with
                                OnLinking = Some on_linking_expr }
                      }

                    let! r_typechecked =
                      state {
                        match parsed_hooks.OnLinked with
                        | None -> return { r_typechecked with OnLinked = None }
                        | Some on_linked ->
                          do! assert_no_cardinality
                          let! on_linked_expr, on_linked_t, on_linked_k, _ = typeCheckExpr None on_linked
                          do! on_linked_k |> Kind.AsStar |> ofSum |> state.Ignore

                          do!
                            TypeValue.Unify(on_linked.Location, on_linked_t, relation_hook_type)
                            |> Expr.liftUnification
                            |> state.MapContext(TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith))

                          return
                            { r_typechecked with
                                OnLinked = Some on_linked_expr }
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
                          let! on_unlinking_expr, on_unlinking_t, on_unlinking_k, _ = typeCheckExpr None on_unlinking
                          do! on_unlinking_k |> Kind.AsStar |> ofSum |> state.Ignore

                          do!
                            TypeValue.Unify(on_unlinking.Location, on_unlinking_t, relation_hook_type)
                            |> Expr.liftUnification
                            |> state.MapContext(TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith))

                          return
                            { r_typechecked with
                                OnUnlinking = Some on_unlinking_expr }
                      }

                    let! r_typechecked =
                      state {
                        match parsed_hooks.OnUnlinked with
                        | None -> return { r_typechecked with OnUnlinked = None }
                        | Some on_unlinked ->
                          do! assert_no_cardinality
                          let! on_unlinked_expr, on_unlinked_t, on_unlinked_k, _ = typeCheckExpr None on_unlinked
                          do! on_unlinked_k |> Kind.AsStar |> ofSum |> state.Ignore

                          do!
                            TypeValue.Unify(on_unlinked.Location, on_unlinked_t, relation_hook_type)
                            |> Expr.liftUnification
                            |> state.MapContext(TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith))

                          return
                            { r_typechecked with
                                OnUnlinked = Some on_unlinked_expr }
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
                          (fun () -> $"Error: cannot find included entity {entityName} for hooks")
                          |> Errors.Singleton loc0)
                        |> state.OfSum

                      let! typechecked_hooks = typecheck_entity_hooks included_entity hooks

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
                                { SchemaEntityHooks.OnCreating = v.Hooks.OnCreating |> Option.orElse hooks.OnCreating
                                  SchemaEntityHooks.OnCreated = v.Hooks.OnCreated |> Option.orElse hooks.OnCreated
                                  SchemaEntityHooks.OnUpdating = v.Hooks.OnUpdating |> Option.orElse hooks.OnUpdating
                                  SchemaEntityHooks.OnUpdated = v.Hooks.OnUpdated |> Option.orElse hooks.OnUpdated
                                  SchemaEntityHooks.OnDeleting = v.Hooks.OnDeleting |> Option.orElse hooks.OnDeleting
                                  SchemaEntityHooks.OnDeleted = v.Hooks.OnDeleted |> Option.orElse hooks.OnDeleted
                                  SchemaEntityHooks.OnBackground =
                                    v.Hooks.OnBackground |> Option.orElse hooks.OnBackground
                                  SchemaEntityHooks.CanCreate = v.Hooks.CanCreate |> Option.orElse hooks.CanCreate
                                  SchemaEntityHooks.CanRead = v.Hooks.CanRead |> Option.orElse hooks.CanRead
                                  SchemaEntityHooks.CanUpdate = v.Hooks.CanUpdate |> Option.orElse hooks.CanUpdate
                                  SchemaEntityHooks.CanDelete = v.Hooks.CanDelete |> Option.orElse hooks.CanDelete } }

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
                          (e_typechecked.Name.Name |> Identifier.LocalScope |> hooks_scope.Resolve)
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
                        |> OrderedMap.tryFind (e_parsed.Name.Name |> SchemaEntityName.Create)
                        |> Sum.fromOption (fun () ->
                          (fun () -> $"Error: cannot find typechecked entity for parsed entity {e_parsed.Name}")
                          |> Errors.Singleton loc0)
                        |> state.OfSum

                      let! typechecked_hooks = typecheck_entity_hooks e_typechecked e_parsed.Hooks

                      let e_typechecked =
                        { e_typechecked with
                            Hooks = typechecked_hooks }

                      return e_parsed.Name, e_typechecked
                    })
                  |> state.All
                  |> state.MapContext(TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith))
                  |> state.MapContext(
                    TypeCheckContext.Updaters.TypeVariables(
                      Map.add "Schema" (TypeValue.Schema resulting_schema_without_hooks, Kind.Schema)
                    )
                  )

                let entities_with_hooks = entities_with_hooks |> OrderedMap.ofList

                let entities_with_hooks =
                  entities_with_hooks |> OrderedMap.mergeSecondAfterFirst included_entities

                let! included_relations_hooks =
                  included_relations_hooks
                  |> Map.toSeq
                  |> Seq.map (fun (relationName, hooks) ->
                    state {
                      let! included_relation =
                        included_relations
                        |> OrderedMap.tryFind relationName
                        |> Sum.fromOption (fun () ->
                          (fun () -> $"Error: cannot find included relation {relationName} for hooks")
                          |> Errors.Singleton loc0)
                        |> state.OfSum

                      let! from_e =
                        entities_with_hooks
                        |> OrderedMap.tryFind (included_relation.From.LocalName |> SchemaEntityName.Create)
                        |> Sum.fromOption (fun () ->
                          (fun () ->
                            $"Error: cannot find entity {included_relation.From} for relation {included_relation.Name}")
                          |> Errors.Singleton loc0)
                        |> state.OfSum

                      let! to_e =
                        entities_with_hooks
                        |> OrderedMap.tryFind (included_relation.To.LocalName |> SchemaEntityName.Create)
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
                                [ TypeValue.CreateUnit(); TypeValue.CreatePrimitive PrimitiveType.String ]
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


                      let! typechecked_hooks = typecheck_relation_hooks assert_no_cardinality relation_hook_type hooks

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
                                { SchemaRelationHooks.OnLinking = v.Hooks.OnLinking |> Option.orElse hooks.OnLinking
                                  SchemaRelationHooks.OnLinked = v.Hooks.OnLinked |> Option.orElse hooks.OnLinked
                                  SchemaRelationHooks.OnUnlinking =
                                    v.Hooks.OnUnlinking |> Option.orElse hooks.OnUnlinking
                                  SchemaRelationHooks.OnUnlinked = v.Hooks.OnUnlinked |> Option.orElse hooks.OnUnlinked } }

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

                if not (Set.isSubset included_entities_hooks_names existing_entity_names) then
                  let conflicting_entity_names =
                    included_entities_hooks_names
                    |> Seq.filter (fun n -> not (Set.contains n existing_entity_names))

                  let comma = ", "

                  return!
                    (fun () ->
                      $"Error: cannot specify hooks for included entities that have the same name as entities in the including schema, see {conflicting_entity_names |> Seq.map (fun n -> n) |> String.join comma} for details")
                    |> Errors.Singleton loc0
                    |> state.Throw
                else if not (Set.isSubset included_relations_hooks_names existing_relation_names) then
                  let conflicting_relation_names =
                    included_relations_hooks_names
                    |> Seq.filter (fun n -> not (Set.contains n existing_relation_names))

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
                        |> OrderedMap.tryFind (r_parsed.Name.Name |> SchemaRelationName.Create)
                        |> Sum.fromOption (fun () ->
                          (fun () -> $"Error: cannot find typechecked relation for parsed relation {r_parsed.Name}")
                          |> Errors.Singleton loc0)
                        |> state.OfSum

                      let! from_e =
                        entities_with_hooks
                        |> OrderedMap.tryFind ((r_parsed.From |> fst).LocalName |> SchemaEntityName.Create)
                        |> Sum.fromOption (fun () ->
                          (fun () -> $"Error: cannot find entity {r_parsed.From} for relation {r_parsed.Name}")
                          |> Errors.Singleton loc0)
                        |> state.OfSum

                      let! to_e =
                        entities_with_hooks
                        |> OrderedMap.tryFind ((r_parsed.To |> fst).LocalName |> SchemaEntityName.Create)
                        |> Sum.fromOption (fun () ->
                          (fun () -> $"Error: cannot find entity {r_parsed.To} for relation {r_parsed.Name}")
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
                                [ TypeValue.CreateUnit(); TypeValue.CreatePrimitive PrimitiveType.String ]
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
                        typecheck_relation_hooks assert_no_cardinality relation_hook_type r_parsed.Hooks

                      return
                        r_parsed.Name,
                        { r_typechecked with
                            Hooks = r_typechecked_hooks }
                    })
                  |> state.All
                  |> state.MapContext(TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith))
                  |> state.MapContext(
                    TypeCheckContext.Updaters.TypeVariables(
                      Map.add "Schema" (TypeValue.Schema resulting_schema_without_hooks, Kind.Schema)
                    )
                  )

                let relations_with_hooks = relations_with_hooks |> OrderedMap.ofList

                let relations_with_hooks =
                  relations_with_hooks |> OrderedMap.mergeSecondAfterFirst included_relations

                do! state.SetState(state_to_restore |> replaceWith)

                let resulting_schema =
                  { resulting_schema_without_hooks with
                      Entities = entities_with_hooks
                      Relations = relations_with_hooks }

                return TypeValue.Schema resulting_schema, Kind.Schema
          | TypeExpr.FromTypeValue tv ->
            // do Console.WriteLine($"Instantiating type value {tv}")
            let! (ctx: TypeCheckContext<'ve>) = state.GetContext()
            let! (s: TypeCheckState<'ve>) = state.GetState()
            let scope = ctx.TypeVariables |> Map.map (fun _ (_, k) -> k)
            let scope = Map.merge (fun _ -> id) scope ctx.TypeParameters
            let! k = TypeValue.KindEval () n loc0 tv |> state.MapContext(fun _ -> scope)

            let! tv =
              tv
              |> TypeValue.Instantiate () (TypeExpr.Eval query_type_symbol mk_query_type typeCheckExpr) loc0
              |> State.Run(TypeInstantiateContext.FromEvalContext(ctx), s)
              |> sum.Map fst
              |> sum.MapError fst
              |> state.OfSum

            // do Console.WriteLine($"Instantiated type value to {tv}")

            return TypeValue.SetSourceMapping(tv, source), k
          | TypeExpr.Imported i ->
            let! parameters =
              i.Parameters
              |> List.map (fun p -> !(TypeExpr.Lookup(Identifier.LocalScope p.Name)) |> state.Map fst)
              |> state.All

            let! args = i.Arguments |> List.map (fun p -> !p.AsExpr |> state.Map fst) |> state.All

            return
              TypeValue.Imported
                { i with
                    Parameters = []
                    Arguments = parameters @ args },
              Kind.Star
          | TypeExpr.NewSymbol _ -> return! (fun () -> $"Errors cannot evaluate {t} as a type") |> error |> state.Throw
          | TypeExpr.Primitive p ->
            return
              TypeValue.Primitive
                { value = p
                  typeExprSource = source
                  typeCheckScopeSource = ctx.Scope },
              Kind.Star
          | TypeExpr.Lookup v ->
            // do Console.WriteLine($"Looking up type for {v} in scope {ctx.Scope}")

            let! res =
              state.Any(
                NonEmptyList.OfList(
                  TypeCheckContext.tryFindTypeVariable (v.LocalName, loc0) |> state.OfReader,
                  [ TypeCheckContext.tryFindTypeParameter (v.LocalName, loc0)
                    |> reader.Map(fun k -> TypeValue.Lookup v, k)
                    |> state.OfReader
                    TypeCheckState.tryFindType (v |> TypeCheckScope.Empty.Resolve, loc0)
                    |> state.OfStateReader

                    state {
                      match v with
                      | Identifier.LocalScope "SchemaEntities" ->
                        return
                          TypeValue.CreateLambda(
                            TypeParameter.Create("s", Kind.Schema),
                            TypeExpr.Entities(TypeExpr.Lookup(Identifier.LocalScope "s"))
                          ),
                          Kind.Arrow(Kind.Schema, Kind.Star)
                      | Identifier.LocalScope "SchemaEntity" ->
                        return
                          TypeValue.CreateLambda(
                            TypeParameter.Create("s", Kind.Schema),
                            TypeExpr.Lambda(
                              TypeParameter.Create("e", Kind.Star),
                              TypeExpr.Lambda(
                                TypeParameter.Create("e_with_props", Kind.Star),
                                TypeExpr.Lambda(
                                  TypeParameter.Create("id", Kind.Star),
                                  TypeExpr.Entity(
                                    TypeExpr.Lookup(Identifier.LocalScope "s"),
                                    TypeExpr.Lookup(Identifier.LocalScope "e"),
                                    TypeExpr.Lookup(Identifier.LocalScope "e_with_props"),
                                    TypeExpr.Lookup(Identifier.LocalScope "id")
                                  )
                                )
                              )
                            )
                          ),
                          Kind.Arrow(
                            Kind.Schema,
                            Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star)))
                          )
                      | Identifier.LocalScope "SchemaRelation" ->
                        return
                          TypeValue.CreateLambda(
                            TypeParameter.Create("s", Kind.Schema),
                            TypeExpr.Lambda(
                              TypeParameter.Create("f", Kind.Star),
                              TypeExpr.Lambda(
                                TypeParameter.Create("f_with_props", Kind.Star),
                                TypeExpr.Lambda(
                                  TypeParameter.Create("f_id", Kind.Star),
                                  TypeExpr.Lambda(
                                    TypeParameter.Create("t", Kind.Star),
                                    TypeExpr.Lambda(
                                      TypeParameter.Create("t_with_props", Kind.Star),
                                      TypeExpr.Lambda(
                                        TypeParameter.Create("t_id", Kind.Star),
                                        TypeExpr.Relation(
                                          TypeExpr.Lookup(Identifier.LocalScope "s"),
                                          TypeExpr.Lookup(Identifier.LocalScope "f"),
                                          TypeExpr.Lookup(Identifier.LocalScope "f_with_props"),
                                          TypeExpr.Lookup(Identifier.LocalScope "f_id"),
                                          TypeExpr.Lookup(Identifier.LocalScope "t"),
                                          TypeExpr.Lookup(Identifier.LocalScope "t_with_props"),
                                          TypeExpr.Lookup(Identifier.LocalScope "t_id")
                                        )
                                      )
                                    )
                                  )
                                )
                              )
                            )
                          ),
                          Kind.Arrow(
                            Kind.Schema,
                            Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star)))
                          )
                      | Identifier.LocalScope "SchemaLookupOne" ->
                        return
                          TypeValue.CreateLambda(
                            TypeParameter.Create("s", Kind.Schema),
                            TypeExpr.Lambda(
                              TypeParameter.Create("f_id", Kind.Star),
                              TypeExpr.Lambda(
                                TypeParameter.Create("t_id", Kind.Star),
                                TypeExpr.Lambda(
                                  TypeParameter.Create("t_with_props", Kind.Star),
                                  TypeExpr.RelationLookupOne(
                                    TypeExpr.Lookup(Identifier.LocalScope "s"),
                                    TypeExpr.Lookup(Identifier.LocalScope "f_id"),
                                    TypeExpr.Lookup(Identifier.LocalScope "t_id"),
                                    TypeExpr.Lookup(Identifier.LocalScope "t_with_props")
                                  )
                                )
                              )
                            )
                          ),
                          Kind.Arrow(
                            Kind.Schema,
                            Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star)))
                          )
                      | Identifier.LocalScope "SchemaLookupOption" ->
                        return
                          TypeValue.CreateLambda(
                            TypeParameter.Create("s", Kind.Schema),
                            TypeExpr.Lambda(
                              TypeParameter.Create("f_id", Kind.Star),
                              TypeExpr.Lambda(
                                TypeParameter.Create("t_id", Kind.Star),
                                TypeExpr.Lambda(
                                  TypeParameter.Create("t_with_props", Kind.Star),
                                  TypeExpr.RelationLookupOption(
                                    TypeExpr.Lookup(Identifier.LocalScope "s"),
                                    TypeExpr.Lookup(Identifier.LocalScope "f_id"),
                                    TypeExpr.Lookup(Identifier.LocalScope "t_id"),
                                    TypeExpr.Lookup(Identifier.LocalScope "t_with_props")
                                  )
                                )
                              )
                            )
                          ),
                          Kind.Arrow(
                            Kind.Schema,
                            Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star)))
                          )
                      | Identifier.LocalScope "SchemaLookupMany" ->
                        return
                          TypeValue.CreateLambda(
                            TypeParameter.Create("s", Kind.Schema),
                            TypeExpr.Lambda(
                              TypeParameter.Create("f_id", Kind.Star),
                              TypeExpr.Lambda(
                                TypeParameter.Create("t_id", Kind.Star),
                                TypeExpr.Lambda(
                                  TypeParameter.Create("t_with_props", Kind.Star),
                                  TypeExpr.RelationLookupMany(
                                    TypeExpr.Lookup(Identifier.LocalScope "s"),
                                    TypeExpr.Lookup(Identifier.LocalScope "f_id"),
                                    TypeExpr.Lookup(Identifier.LocalScope "t_id"),
                                    TypeExpr.Lookup(Identifier.LocalScope "t_with_props")
                                  )
                                )
                              )
                            )
                          ),
                          Kind.Arrow(
                            Kind.Schema,
                            Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star)))
                          )
                      | _ ->
                        let! c = state.GetContext()

                        return!
                          (fun () -> $"Error: cannot find type for {v} with context ({c.TypeVariables.AsFSharpString})")
                          |> error
                          |> state.Throw
                          |> state.MapError(Errors<_>.MapPriority(replaceWith ErrorPriority.High))
                    } ]
                )
              )
              |> state.MapError(Errors<_>.FilterHighestPriorityOnly)

            // do Console.WriteLine($"Lookup {v} resolved to {res}")

            return res

          | TypeExpr.LetSymbols(xts, symbolsKind, rest) ->
            do!
              xts
              |> Seq.map (fun x ->
                state {
                  let x0 = Identifier.LocalScope x
                  // match ctx.Scope.Type with
                  // | Some t -> Identifier.FullyQualified([ t ], x)
                  // | None -> Identifier.LocalScope x

                  let s_x = TypeSymbol.Create(x0)
                  let x = x0 |> ctx.Scope.Resolve

                  do! TypeCheckState.bindIdentifierToResolvedIdentifier x x0

                  match ctx.Scope.Type with
                  | Some t ->
                    do!
                      TypeCheckState.bindIdentifierToResolvedIdentifier
                        x
                        (Identifier.FullyQualified([ t ], x0.LocalName))
                  | None -> ()

                  // do Console.WriteLine($"Binding symbol {s_x.Name.ToString()} to {x.ToString()}")

                  match symbolsKind with
                  | RecordFields -> do! TypeCheckState.bindRecordFieldSymbol x s_x
                  | UnionConstructors -> do! TypeCheckState.bindUnionCaseSymbol x s_x

                })
              |> state.All
              |> state.Ignore

            let! resultValue, resultKind = !rest
            return TypeValue.SetSourceMapping(resultValue, source), resultKind
          | TypeExpr.Let(x, t_x, rest) ->

            return!
              state.Either3
                (state {
                  let! t_x = !t_x

                  let! resultValue, resultKind =
                    !rest
                    |> state.MapContext(TypeCheckContext.Updaters.TypeVariables(Map.add x t_x))

                  return TypeValue.SetSourceMapping(resultValue, source), resultKind
                })
                (state {
                  let! s_x = !!t_x
                  let x = Identifier.LocalScope x |> ctx.Scope.Resolve
                  do! TypeCheckState.bindTypeSymbol x s_x
                  let! resultValue, resultKind = !rest
                  return TypeValue.SetSourceMapping(resultValue, source), resultKind
                })
                (state { return! (fun () -> $"Error: cannot evaluate let binding {x}") |> error |> state.Throw }
                 |> state.MapError(Errors.MapPriority(replaceWith ErrorPriority.High)))

          | TypeExpr.Apply(f, a) ->
            // do Console.WriteLine($"Evaluating type application of {f} to {a}")
            let! f, f_k = !f
            // do Console.WriteLine($"Evaluated function part to {f}\n{f_k}")
            // do Console.ReadLine() |> ignore
            let! f_k_i, f_k_o = f_k |> Kind.AsArrow |> ofSum

            return!
              state.Either6
                (state {
                  let! param, body = f |> TypeValue.AsLambda |> ofSum |> state.Map WithSourceMapping.Getters.Value

                  match param.Kind with
                  | Kind.Symbol ->
                    let! a = !!a

                    do!
                      TypeCheckState.bindTypeSymbol
                        (param.Name |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve)
                        a

                    let! resultValue, resultKind = !body
                    return TypeValue.SetSourceMapping(resultValue, source), resultKind
                  | _ ->
                    let! a = !a

                    // do Console.WriteLine($"Applying type lambda param {param} to argument {a |> fst} in {body}")
                    // do Console.ReadLine() |> ignore

                    let! resultValue, resultKind =
                      !body
                      |> state.MapContext(TypeCheckContext.Updaters.TypeVariables(Map.add param.Name a))

                    // do Console.WriteLine($"Result of type application is {resultValue.AsFSharpString}")
                    // do Console.ReadLine() |> ignore

                    return TypeValue.SetSourceMapping(resultValue, source), resultKind
                })
                (state {
                  let! f_i = f |> TypeValue.AsImported |> ofSum

                  // let! ctx = state.GetContext()
                  // do Console.WriteLine($"Evaluating argument part {a}")
                  // do Console.WriteLine($"Context is {ctx.TypeParameters.ToFSharpString}")
                  // do Console.ReadLine() |> ignore
                  let! a, a_k = !a
                  // do Console.WriteLine($"aka {a}")
                  // do Console.ReadLine() |> ignore

                  if a_k <> f_k_i then
                    return!
                      (fun () -> $"Error: cannot apply type {f} of input kind {f_k_i} to argument of kind {a_k}")
                      |> error
                      |> state.Throw
                  else
                    match f_i.Parameters with
                    | [] ->
                      return!
                        (fun () -> $"Error: cannot apply imported type {f_i.Id.Name} with no parameters")
                        |> error
                        |> state.Throw
                    | _ :: ps ->
                      return
                        TypeValue.Imported
                          { f_i with
                              Parameters = ps
                              Arguments = f_i.Arguments @ [ a ] },
                        f_k_o
                })
                (state {
                  let! f_l = f |> TypeValue.AsLookup |> ofSum

                  let! a, a_k = !a

                  if a_k <> f_k_i then
                    return!
                      (fun () -> $"Error: cannot apply type {f} of input kind {f_k_i} to argument of kind {a_k}")
                      |> error
                      |> state.Throw
                  else
                    return TypeValue.CreateApplication(SymbolicTypeApplication.Lookup(f_l, a)), f_k_o
                })
                (state {
                  let! { value = f_app } = f |> TypeValue.AsApplication |> ofSum

                  let! a, a_k = !a

                  if a_k <> f_k_i then
                    return!
                      (fun () -> $"Error: cannot apply type {f} of input kind {f_k_i} to argument of kind {a_k}")
                      |> error
                      |> state.Throw
                  else
                    return TypeValue.CreateApplication(SymbolicTypeApplication.Application(f_app, a)), f_k_o
                })
                (state {
                  do! f |> TypeValue.AsQueryTypeFunction |> ofSum

                  return!
                    state {
                      let! a_t, a_k = !a
                      // do Console.WriteLine $"Applying query type function to argument of type {a_t} and kind {a_k}"
                      do! a_k |> Kind.AsQueryRow |> ofSum |> state.Ignore

                      return!
                        state.Either3
                          (state {
                            let! a_t = a_t |> TypeValue.AsQueryRow |> ofSum

                            let rec try_convert a_t =
                              state {
                                match a_t with
                                | TypeQueryRow.Json j -> return j, Kind.Star
                                | TypeQueryRow.PrimaryKey k -> return k, Kind.Star
                                | TypeQueryRow.PrimitiveType(pt, false) ->
                                  return TypeValue.CreatePrimitive pt, Kind.Star
                                | TypeQueryRow.PrimitiveType(pt, true) ->
                                  return
                                    TypeValue.CreateSum [ TypeValue.CreateUnit(); TypeValue.CreatePrimitive pt ],
                                    Kind.Star
                                | TypeQueryRow.Tuple ts ->
                                  let! ts = ts |> List.map (fun t -> try_convert t) |> state.All
                                  let ts = ts |> List.map fst
                                  return TypeValue.CreateTuple ts, Kind.Star
                                | TypeQueryRow.Record _ ->
                                  return!
                                    (fun () ->
                                      $"Error: cannot return record query rows, please convert to a tuple first")
                                    |> error
                                    |> state.Throw
                              }

                            let! result, _ = a_t |> try_convert

                            return TypeValue.SetSourceMapping(result, source), Kind.Star
                          })
                          (state {
                            let! a_t = a_t |> TypeValue.AsLookup |> ofSum
                            return TypeValue.CreateApplication(SymbolicTypeApplication.FromQueryRow a_t), Kind.Star
                          })
                          (state {
                            let! a_t = a_t |> TypeValue.AsVar |> ofSum

                            return
                              TypeValue.CreateApplication(
                                SymbolicTypeApplication.FromQueryRow(a_t.Name |> Identifier.LocalScope)
                              ),
                              Kind.Star
                          })
                    }
                    |> state.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
                })
                (state {
                  return!
                    (fun () -> $"TypeEval(TypeExpr.Apply): cannot evaluate type application {t}")
                    |> error
                    |> state.Throw
                 }
                 |> state.MapError(Errors.MapPriority(replaceWith ErrorPriority.High)))
              |> state.MapError(Errors<_>.FilterHighestPriorityOnly)
          | TypeExpr.Lambda(param, bodyExpr) ->
            // let fresh_var_t =
            //   TypeValue.Var(
            //     { TypeVar.Name = param.Name
            //       Guid = Guid.CreateVersion7() }
            //   )

            // let! ctx = state.GetContext()
            // let closure = ctx.TypeVariables
            // let closure = ctx.TypeVariables |> Map.add param.Name (fresh_var_t, param.Kind)
            // do Console.WriteLine($"Type lambda closure: {closure.ToFSharpString}")
            // do Console.ReadLine() |> ignore

            // do Console.WriteLine($"Evaluating type lambda with param {param} and body {bodyExpr}")

            let! body_t, body_k =
              !bodyExpr
              // |> TypeExpr.KindEval n loc0
              |> state.MapContext(
                TypeCheckContext.Updaters.TypeParameters(Map.add param.Name param.Kind)
                >> TypeCheckContext.Updaters.TypeVariables(Map.remove param.Name)
              )

            // do Console.WriteLine($"Evaluated type lambda body to {body_t}")

            // |> state.MapContext(TypeExprEvalContext.Updaters.TypeVariables(replaceWith closure))

            return
              TypeValue.Lambda
                { value = param, body_t.AsExpr
                  typeExprSource = source
                  typeCheckScopeSource = ctx.Scope },
              Kind.Arrow(param.Kind, body_k)
          | TypeExpr.Arrow(input, output) ->
            // do Console.WriteLine($"Evaluating arrow from {input} to {output}")
            let! input, input_k = !input
            // do Console.WriteLine $"Evaluated input type to {input} with kind {input_k}"
            let! output, output_k = !output

            do!
              sum.Any2 (input_k |> Kind.AsStar) (input_k |> Kind.AsSchema)
              |> ofSum
              |> state.Ignore

            do! output_k |> Kind.AsStar |> ofSum |> state.Ignore

            return
              TypeValue.Arrow
                { value = (input, output)
                  typeExprSource = source
                  typeCheckScopeSource = ctx.Scope },
              Kind.Star
          | TypeExpr.Record(fields) ->
            let! fields =
              fields
              |> Seq.map (fun (k, v) ->
                state {
                  let! k = !!k
                  let! v, v_k = !v
                  // do! v_k |> Kind.AsStar |> ofSum |> state.Ignore
                  return (k, (v, v_k))
                })
              |> state.All
              |> state.Map(OrderedMap.ofSeq)

            return
              TypeValue.Record
                { value = fields
                  typeExprSource = source
                  typeCheckScopeSource = ctx.Scope },
              Kind.Star
          | TypeExpr.Tuple(items) ->
            let! items =
              items
              |> List.map (fun i ->
                state {
                  let! i, i_k = !i
                  do! i_k |> Kind.AsStar |> ofSum |> state.Ignore
                  return i
                })
              |> state.All

            return
              TypeValue.Tuple
                { value = items
                  typeExprSource = source
                  typeCheckScopeSource = ctx.Scope },
              Kind.Star
          | TypeExpr.Union(cases) ->
            let! cases =
              cases
              |> Seq.map (fun (k, v) ->
                state {
                  let! k = !!k
                  let! v, v_k = !v
                  do! v_k |> Kind.AsStar |> ofSum |> state.Ignore
                  return (k, v)
                })
              |> state.All
              |> state.Map(OrderedMap.ofSeq)

            return
              TypeValue.Union
                { value = cases
                  typeExprSource = source
                  typeCheckScopeSource = ctx.Scope },
              Kind.Star
          | TypeExpr.Set(element) ->
            let! element, element_k = !element
            do! element_k |> Kind.AsStar |> ofSum |> state.Ignore

            return
              TypeValue.Set
                { value = element
                  typeExprSource = source
                  typeCheckScopeSource = ctx.Scope },
              Kind.Star
          | TypeExpr.KeyOf(arg) ->
            let! arg, arg_k = !arg
            do! arg_k |> Kind.AsStar |> ofSum |> state.Ignore

            let! cases = arg |> TypeValue.AsRecord |> ofSum

            let mappedCasesFixThis =
              cases
              |> OrderedMap.map (fun _ _ -> TypeValue.CreatePrimitive PrimitiveType.Unit)

            return
              TypeValue.Union
                { value = mappedCasesFixThis
                  typeExprSource = source
                  typeCheckScopeSource = ctx.Scope },
              Kind.Star
          | TypeExpr.Sum(variants) ->
            let! variants =
              variants
              |> List.map (fun i ->
                state {
                  let! i, i_k = !i
                  do! i_k |> Kind.AsStar |> ofSum |> state.Ignore
                  return i
                })
              |> state.All

            return
              TypeValue.Sum
                { value = variants
                  typeExprSource = source
                  typeCheckScopeSource = ctx.Scope },
              Kind.Star
          | TypeExpr.RecordDes(t, comp) ->
            let! tv, tk = !t
            do! tk |> Kind.AsStar |> ofSum |> state.Ignore

            let failure =
              (fun () -> $"Error: cannot evaluate record destructuring {tv}.{comp}")
              |> error
              |> state.Throw
              |> state.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))

            match comp with
            | Left field ->
              return!
                state.Any(
                  state {
                    let! fields = tv |> TypeValue.AsRecord |> ofSum

                    let! (_, (field_t, field_k)) =
                      fields
                      |> OrderedMap.toSeq
                      |> Seq.tryFind (fun (k, _) -> k.Name.LocalName = field.Name)
                      |> Sum.fromOption (fun () ->
                        (fun () -> $"Error: cannot find field {field} in record type {tv}")
                        |> Errors.Singleton loc0)
                      |> state.OfSum

                    return TypeValue.SetSourceMapping(field_t, source), field_k
                  },
                  [ state {
                      let! _, cases = tv |> TypeValue.AsUnion |> ofSum

                      let! (_, case_t) =
                        cases
                        |> OrderedMap.toSeq
                        |> Seq.tryFind (fun (k, _) -> k.Name.LocalName = field.Name)
                        |> Sum.fromOption (fun () ->
                          (fun () -> $"Error: cannot find case {field} in union type {tv}")
                          |> Errors.Singleton loc0)
                        |> state.OfSum

                      return TypeValue.SetSourceMapping(case_t, source), Kind.Star
                    }
                    failure ]
                )
            | Right item ->
              return!
                state.Any(
                  state {
                    let! items = tv |> TypeValue.AsTuple |> ofSum

                    if item < 1 || item > items.Length then
                      return!
                        (fun () -> $"Error: tuple index {item} is out of bounds for tuple type {tv}")
                        |> Errors.Singleton loc0
                        |> state.Throw
                    else
                      let! item_t =
                        items
                        |> Seq.tryItem (item - 1)
                        |> Sum.fromOption (fun () ->
                          (fun () -> $"Error: cannot find tuple index {item} in tuple type {tv}")
                          |> Errors.Singleton loc0)
                        |> state.OfSum

                      return TypeValue.SetSourceMapping(item_t, source), Kind.Star
                  },
                  [ state {
                      let! items = tv |> TypeValue.AsSum |> ofSum

                      if item < 1 || item > items.Length then
                        return!
                          (fun () -> $"Error: sum case {item} is out of bounds for sum type {tv}")
                          |> Errors.Singleton loc0
                          |> state.Throw
                      else
                        let! item_t =
                          items
                          |> Seq.tryItem (item - 1)
                          |> Sum.fromOption (fun () ->
                            (fun () -> $"Error: cannot find sum case {item} in sum type {tv}")
                            |> Errors.Singleton loc0)
                          |> state.OfSum

                        return TypeValue.SetSourceMapping(item_t, source), Kind.Star
                    }
                    state {
                      let! imported = tv |> TypeValue.AsImported |> ofSum

                      let! item_t =
                        imported.Arguments
                        |> List.tryItem (item - 1)
                        |> Sum.fromOption (fun () ->
                          (fun () -> $"Error: cannot find argument {item} in imported type {tv}")
                          |> Errors.Singleton loc0)
                        |> state.OfSum

                      return TypeValue.SetSourceMapping(item_t, source), Kind.Star
                    }
                    failure ]
                )
          | TypeExpr.Flatten(type1, type2) ->
            let! type1, type1_k = !type1
            let! type2, type2_k = !type2
            do! type1_k |> Kind.AsStar |> ofSum |> state.Ignore
            do! type2_k |> Kind.AsStar |> ofSum |> state.Ignore

            return!
              state.Either3
                (state {
                  let! cases1 = type1 |> TypeValue.AsUnion |> sum.Map snd |> ofSum

                  let! cases2 = type2 |> TypeValue.AsUnion |> sum.Map snd |> ofSum

                  let cases1 = cases1 |> OrderedMap.toSeq
                  let keys1 = cases1 |> Seq.map fst |> Set.ofSeq

                  let cases2 = cases2 |> OrderedMap.toSeq
                  let keys2 = cases2 |> Seq.map fst |> Set.ofSeq

                  if keys1 |> Set.intersect keys2 |> Set.isEmpty then
                    let cases = cases1 |> Seq.append cases2 |> OrderedMap.ofSeq

                    return
                      TypeValue.Union
                        { value = cases
                          typeExprSource = source
                          typeCheckScopeSource = ctx.Scope },
                      Kind.Star
                  else
                    return!
                      (fun () -> $"Error: cannot flatten types with overlapping keys: {keys1} and {keys2}")
                      |> error
                      |> state.Throw
                })
                (state {
                  let! fields1 = type1 |> TypeValue.AsRecord |> ofSum

                  let! fields2 = type2 |> TypeValue.AsRecord |> ofSum

                  let fields1 = fields1 |> OrderedMap.toSeq
                  let keys1 = fields1 |> Seq.map fst |> Set.ofSeq

                  let fields2 = fields2 |> OrderedMap.toSeq
                  let keys2 = fields2 |> Seq.map fst |> Set.ofSeq

                  if keys1 |> Set.intersect keys2 |> Set.isEmpty then
                    let fields = fields1 |> Seq.append fields2 |> OrderedMap.ofSeq

                    return
                      TypeValue.Record
                        { value = fields
                          typeExprSource = source
                          typeCheckScopeSource = ctx.Scope },
                      Kind.Star
                  else
                    return!
                      (fun () -> $"Error: cannot flatten types with overlapping keys: {keys1} and {keys2}")
                      |> error
                      |> state.Throw
                })
                (state { return! (fun () -> $"Error: cannot evaluate flattening ") |> error |> state.Throw }
                 |> state.MapError(Errors.MapPriority(replaceWith ErrorPriority.High)))
          | TypeExpr.Exclude(type1, type2) ->
            let! type1, type1_k = !type1
            let! type2, type2_k = !type2
            do! type1_k |> Kind.AsStar |> ofSum |> state.Ignore
            do! type2_k |> Kind.AsStar |> ofSum |> state.Ignore

            return!
              state.Either3
                (state {
                  let! cases1 = type1 |> TypeValue.AsUnion |> sum.Map snd |> ofSum

                  let! cases2 = type2 |> TypeValue.AsUnion |> sum.Map snd |> ofSum

                  let keys2 = cases2 |> OrderedMap.keys |> Set.ofSeq
                  let cases = cases1 |> OrderedMap.filter (fun k _ -> keys2 |> Set.contains k |> not)

                  return
                    TypeValue.Union
                      { value = cases
                        typeExprSource = source
                        typeCheckScopeSource = ctx.Scope },
                    Kind.Star
                })
                (state {
                  let! fields1 = type1 |> TypeValue.AsRecord |> ofSum
                  let! fields2 = type2 |> TypeValue.AsRecord |> ofSum

                  let keys2 = fields2 |> OrderedMap.keys |> Set.ofSeq

                  let fields =
                    fields1 |> OrderedMap.filter (fun k _ -> keys2 |> Set.contains k |> not)

                  return
                    TypeValue.Record
                      { value = fields
                        typeExprSource = source
                        typeCheckScopeSource = ctx.Scope },
                    Kind.Star
                })
                (state { return! (fun () -> $"Error: cannot evaluate exclude ") |> error |> state.Throw }
                 |> state.MapError(Errors.MapPriority(replaceWith ErrorPriority.High)))
          | TypeExpr.Rotate(t) ->
            let! t, t_k = !t
            do! t_k |> Kind.AsStar |> ofSum |> state.Ignore

            return!
              state.Either3
                (state {
                  let! cases = t |> TypeValue.AsUnion |> sum.Map snd |> ofSum
                  let cases = cases |> OrderedMap.map (fun _ v -> v, Kind.Star)

                  return
                    TypeValue.Record
                      { value = cases
                        typeExprSource = source
                        typeCheckScopeSource = ctx.Scope },
                    Kind.Star
                })
                (state {
                  let! fields = t |> TypeValue.AsRecord |> ofSum

                  let! _ =
                    fields
                    |> OrderedMap.map (fun _ (_, k) -> k |> Kind.AsStar |> ofSum |> state.Ignore)
                    |> OrderedMap.toList
                    |> List.map snd
                    |> state.All

                  return
                    TypeValue.Union
                      { value = fields |> OrderedMap.map (fun _ (v, _) -> v)
                        typeExprSource = source
                        typeCheckScopeSource = ctx.Scope },
                    Kind.Star
                })
                (state { return! (fun () -> $"Error: cannot evaluate rotation") |> error |> state.Throw }
                 |> state.MapError(Errors.MapPriority(replaceWith ErrorPriority.High)))
          | TypeExpr.QueryRow q_row ->

            let! q_row = !!!q_row

            return TypeValue.QueryRow q_row, Kind.QueryRow

          | TypeExpr.FromQueryRow -> return TypeValue.QueryTypeFunction, Kind.Arrow(Kind.QueryRow, Kind.Star)

        }

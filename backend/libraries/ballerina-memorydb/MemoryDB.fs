namespace Ballerina.DSL.Next.StdLib

open Ballerina
open Ballerina.Collections.Sum
open Ballerina.Reader.WithError
open System
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.TypeChecker
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.DSL.Next.Types.TypeChecker.Eval
open Ballerina.DSL.Next.Extensions
open Ballerina.DSL.Next.Terms.Eval
open Ballerina.DSL.Next.Terms.Patterns
open Ballerina.State.WithError
open Ballerina.LocalizedErrors
open Ballerina.Errors
open Ballerina.Parser
open Ballerina.StdLib.Object
open Ballerina.DSL.Next.Syntax
open Ballerina.DSL.Next.StdLib.Extensions
open Ballerina.DSL.Next.Runners
open Ballerina
open Ballerina.Collections.Option
open Ballerina.StdLib.String
open Ballerina.Collections.NonEmptyList
open Ballerina.DSL.Next.Serialization.ValueSerializer
open Ballerina.DSL.Next.Serialization.ValueDeserializer
open Ballerina.DSL.Next.StdLib.DB

module MutableMemoryDB =

  type MemoryDBValue<'runtimeContext, 'customExt when 'customExt: comparison> =
    Value<
      TypeValue<ValueExt<'runtimeContext, MutableMemoryDB<'runtimeContext, 'customExt>, 'customExt>>,
      ValueExt<'runtimeContext, MutableMemoryDB<'runtimeContext, 'customExt>, 'customExt>
     >

  and MemoryDBRelation<'runtimeContext, 'customExt when 'customExt: comparison> =
    { All: Set<MemoryDBValue<'runtimeContext, 'customExt> * MemoryDBValue<'runtimeContext, 'customExt>>
      FromTo: Map<MemoryDBValue<'runtimeContext, 'customExt>, Set<MemoryDBValue<'runtimeContext, 'customExt>>>
      ToFrom: Map<MemoryDBValue<'runtimeContext, 'customExt>, Set<MemoryDBValue<'runtimeContext, 'customExt>>> }

    static member Empty: MemoryDBRelation<'runtimeContext, 'customExt> =
      { All = Set.empty
        FromTo = Map.empty
        ToFrom = Map.empty }

  and MutableMemoryDB<'runtimeContext, 'customExt when 'customExt: comparison> =
    { mutable entities:
        Map<
          SchemaEntityName,
          Map<MemoryDBValue<'runtimeContext, 'customExt>, MemoryDBValue<'runtimeContext, 'customExt>>
         >
      mutable relations: Map<SchemaRelationName, MemoryDBRelation<'runtimeContext, 'customExt>>
      mutable operations: List<DBOperation<'runtimeContext, 'customExt>> }

  and DBOperation<'runtimeContext, 'customExt when 'customExt: comparison> =
    | Create of
      EntityName: SchemaEntityName *
      Id: MemoryDBValue<'runtimeContext, 'customExt> *
      Value: MemoryDBValue<'runtimeContext, 'customExt>
    | Delete of EntityName: SchemaEntityName * Id: MemoryDBValue<'runtimeContext, 'customExt>
    | Update of
      EntityName: SchemaEntityName *
      Id: MemoryDBValue<'runtimeContext, 'customExt> *
      Value: MemoryDBValue<'runtimeContext, 'customExt>
    | Link of
      RelationName: SchemaRelationName *
      FromId: MemoryDBValue<'runtimeContext, 'customExt> *
      ToId: MemoryDBValue<'runtimeContext, 'customExt>
    | Unlink of
      RelationName: SchemaRelationName *
      FromId: MemoryDBValue<'runtimeContext, 'customExt> *
      ToId: MemoryDBValue<'runtimeContext, 'customExt>

  type EvalQueryContext<'ext when 'ext: comparison> =
    { Bindings: Map<ResolvedIdentifier, Value<TypeValue<'ext>, 'ext>> }

  let rec private evalQueryExpr
    (query: ExprQueryExpr<TypeValue<'ext>, ResolvedIdentifier, 'ext>)
    : Reader<Value<TypeValue<'ext>, 'ext>, EvalQueryContext<'ext>, Errors<Location>> =
    reader {
      match query.Expr with
      | ExprQueryExprRec.QueryApply(func, arg) ->
        let! argVal = evalQueryExpr arg

        match func.Expr, argVal with
        | ExprQueryExprRec.QueryIntrinsic QueryIntrinsic.GreaterThan,
          Value.Tuple [ Value.Primitive(PrimitiveValue.Int32 v1); Value.Primitive(PrimitiveValue.Int32 v2) ] ->
          return Value.Primitive(PrimitiveValue.Bool(v1 > v2))
        | ExprQueryExprRec.QueryIntrinsic QueryIntrinsic.Multiply,
          Value.Tuple [ Value.Primitive(PrimitiveValue.Int32 v1); Value.Primitive(PrimitiveValue.Int32 v2) ] ->
          return Value.Primitive(PrimitiveValue.Int32(v1 * v2))
        | _ ->
          return!
            Errors.Singleton func.Location (fun () -> $"Not implemented intrinsic {func} in query")
            |> reader.Throw
      | ExprQueryExprRec.QueryTupleCons items ->
        let! items = items |> Seq.map evalQueryExpr |> reader.All
        return Value.Tuple(Seq.toList items)
      | ExprQueryExprRec.QueryRecordDes(expr, field) ->
        let! recordVal = evalQueryExpr expr

        match recordVal with
        | Value.Record fields ->
          return!
            fields
            |> Map.tryFind field
            |> sum.OfOption(
              Errors.Singleton query.Location (fun () -> $"Field {field} not found in record {recordVal}")
            )
            |> reader.OfSum
        | _ ->
          return!
            Errors.Singleton query.Location (fun () ->
              $"Expected a record value for record destructuring, got {recordVal}")
            |> reader.Throw
      | ExprQueryExprRec.QueryTupleDes(expr, index) ->
        let! tupleVal = evalQueryExpr expr

        match tupleVal with
        | Value.Tuple items ->
          let! item =
            items
            |> List.tryItem (index.Index - 1)
            |> sum.OfOption(
              Errors.Singleton query.Location (fun () -> $"Index {index} out of bounds for tuple {tupleVal}")
            )
            |> reader.OfSum

          return item
        | _ ->
          return!
            Errors.Singleton query.Location (fun () ->
              $"Expected a tuple value for tuple destructuring, got {tupleVal}")
            |> reader.Throw
      | ExprQueryExprRec.QueryConditional(cond, ``then``, ``else``) ->
        let! condVal = evalQueryExpr cond

        match condVal with
        | Value.Primitive(PrimitiveValue.Bool true) -> return! evalQueryExpr ``then``
        | Value.Primitive(PrimitiveValue.Bool false) -> return! evalQueryExpr ``else``
        | _ ->
          return!
            Errors.Singleton cond.Location (fun () ->
              $"Expected a boolean value for conditional expression, got {condVal}")
            |> reader.Throw
      | ExprQueryExprRec.QueryUnionDes(_expr, _handlers) ->
        return!
          Errors.Singleton query.Location (fun () -> $"Union destructuring not implemented yet")
          |> reader.Throw
      | ExprQueryExprRec.QuerySumDes(_expr, _handlers) ->
        return!
          Errors.Singleton query.Location (fun () -> $"Sum destructuring not implemented yet")
          |> reader.Throw
      | ExprQueryExprRec.QueryLookup id ->
        let! ctx = reader.GetContext()

        return!
          ctx.Bindings
          |> Map.tryFind id
          |> sum.OfOption(Errors.Singleton query.Location (fun () -> $"Identifier {id} not found in query context"))
          |> reader.OfSum
      | ExprQueryExprRec.QueryIntrinsic(_) ->
        return!
          Errors.Singleton query.Location (fun () -> $"Standalone intrinsics are not supported in the query engine")
          |> reader.Throw
      | ExprQueryExprRec.QueryConstant v -> return Value.Primitive v
    }

  let rec private runQuery (query: ValueQuery<_, _>) : Sum<seq<Value<_, _>> * MutableMemoryDB<_, _>, Errors<Location>> =
    sum {
      let! iterators =
        query.Iterators
        |> NonEmptyList.map (fun it ->
          sum {
            match it.Source with
            | Value.Ext(ValueExt.ValueExt(Choice5Of7(DBExt.DBValues(DBValues.EntityRef entity_ref))), _) ->
              let _, (db: MutableMemoryDB<_, _>), entity_ref, _ = entity_ref

              let db_values =
                db.entities
                |> Map.tryFind entity_ref.Name
                |> Option.defaultValue Map.empty
                |> Map.toSeq
                |> Seq.map (fun (id, value) ->
                  [ "Id" |> ResolvedIdentifier.Create, id
                    "Value" |> ResolvedIdentifier.Create, value ]
                  |> Map.ofSeq
                  |> Value.Record)

              return it.Var, db_values, db
            | Value.Record fields ->
              let! relation =
                fields
                |> Map.tryFind ("Relation" |> ResolvedIdentifier.Create)
                |> sum.OfOption(Errors.Singleton it.Location (fun () -> "Relation field not found in iterator source"))

              match relation with
              | Value.Ext(ValueExt.ValueExt(Choice5Of7(DBExt.DBValues(DBValues.RelationRef relation_ref))), _) ->
                let _, (db: MutableMemoryDB<_, _>), relation_ref, _, _, _ = relation_ref

                let relation_values =
                  db.relations
                  |> Map.tryFind relation_ref.Name
                  |> Option.defaultValue MemoryDBRelation.Empty
                  |> fun rel -> rel.All
                  |> Seq.map (fun (fromId, toId) ->
                    [ "FromId" |> ResolvedIdentifier.Create, fromId
                      "ToId" |> ResolvedIdentifier.Create, toId ]
                    |> Map.ofSeq
                    |> Value.Record)

                return it.Var, relation_values, db
              | _ ->
                return!
                  Errors.Singleton it.Location (fun () ->
                    "Expected Relation field to be a RelationRef in iterator source")
                  |> sum.Throw
            | Value.Query q ->
              let! values, db = runQuery q
              return it.Var, values, db
            | _ ->
              return!
                Errors.Singleton it.Location (fun () -> $"Unsupported iterator source: {it.Source}")
                |> sum.Throw
          })
        |> sum.AllNonEmpty

      let vars_with_values =
        iterators |> NonEmptyList.map (fun (v, values, _) -> v, values)

      let db = iterators.Head |> fun (_, _, db) -> db

      let rec cross_join_of_sources (vars_with_values: NonEmptyList<Var * seq<Value<_, _>>>) =
        match vars_with_values.Tail with
        | [] ->
          let (var, values) = vars_with_values.Head

          values
          |> Seq.map (fun value -> Map.ofList [ var.Name |> ResolvedIdentifier.Create, value ])
          |> Seq.toList
        | v :: vs ->
          let rest_joined = cross_join_of_sources (NonEmptyList.OfList(v, vs))
          let (var, values) = vars_with_values.Head

          [ for value in values do
              for rest in rest_joined do
                yield Map.add (var.Name |> ResolvedIdentifier.Create) value rest ]

      let all_scopes = vars_with_values |> cross_join_of_sources

      let! all_scopes =
        sum {
          match query.Joins with
          | None -> return all_scopes
          | Some joins ->
            let! all_scopes_with_join_predicate =
              all_scopes
              |> Seq.map (fun scope ->
                sum {
                  let! join_predicates_hold =
                    joins
                    |> Seq.map (fun join ->
                      sum {
                        let! left = join.Left |> evalQueryExpr |> Reader.Run { Bindings = scope }
                        let! right = join.Right |> evalQueryExpr |> Reader.Run { Bindings = scope }

                        let! left =
                          left
                          |> Value.AsRecord
                          |> sum.MapError(Errors.MapContext(replaceWith join.Location))

                        let! right =
                          right
                          |> Value.AsRecord
                          |> sum.MapError(Errors.MapContext(replaceWith join.Location))

                        let! _, left =
                          left
                          |> Map.toList
                          |> List.tryHead
                          |> sum.OfOption(
                            Errors.Singleton join.Location (fun () ->
                              "Expected left expression of join to be a record with at least one field")
                          )

                        let! _, right =
                          right
                          |> Map.toList
                          |> List.tryHead
                          |> sum.OfOption(
                            Errors.Singleton join.Location (fun () ->
                              "Expected right expression of join to be a record with at least one field")
                          )

                        let! left =
                          left
                          |> Value.AsPrimitive
                          |> sum.MapError(Errors.MapContext(replaceWith join.Location))

                        let! right =
                          right
                          |> Value.AsPrimitive
                          |> sum.MapError(Errors.MapContext(replaceWith join.Location))

                        return left = right
                      })
                    |> sum.All

                  return scope, join_predicates_hold |> Seq.forall id
                })
              |> sum.All

            let all_scopes_with_join_predicate =
              all_scopes_with_join_predicate |> Seq.filter snd |> Seq.map fst

            return all_scopes_with_join_predicate |> Seq.toList
        }

      let! all_scopes =
        sum {
          match query.Where with
          | None -> return all_scopes
          | Some predicate ->
            let! all_scopes_with_filtering_predicate =
              all_scopes
              |> Seq.map (fun scope ->
                sum {
                  let! predicate_value = predicate |> evalQueryExpr |> Reader.Run { Bindings = scope }

                  let! predicate_value =
                    predicate_value
                    |> Value.AsPrimitive
                    |> sum.MapError(Errors.MapContext(replaceWith predicate.Location))

                  let! predicate_value =
                    predicate_value
                    |> PrimitiveValue.AsBool
                    |> sum.MapError(Errors.MapContext(replaceWith predicate.Location))

                  return scope, predicate_value
                })
              |> sum.All

            let all_scopes_with_join_predicate =
              all_scopes_with_filtering_predicate |> Seq.filter snd |> Seq.map fst

            return all_scopes_with_join_predicate |> Seq.toList
        }

      let! all_scopes =
        all_scopes
        |> Seq.map (fun scope ->
          sum {
            let! return_value = query.Select |> evalQueryExpr |> Reader.Run { Bindings = scope }
            return scope, return_value
          })
        |> sum.All

      let! all_scopes =
        sum {
          match query.OrderBy with
          | None -> return all_scopes
          | Some(ordering, direction) ->
            let! all_scopes_with_ordering_values =
              all_scopes
              |> Seq.map (fun (scope, return_value) ->
                sum {
                  let! ordering_value = ordering |> evalQueryExpr |> Reader.Run { Bindings = scope }
                  return scope, return_value, ordering_value
                })
              |> sum.All

            let all_scopes_with_ordering_values = all_scopes_with_ordering_values |> Seq.toList

            let all_scopes_with_ordering_values =
              match direction with
              | Asc ->
                all_scopes_with_ordering_values
                |> List.sortBy (fun (_, _, ordering_value) -> ordering_value)
              | Desc ->
                all_scopes_with_ordering_values
                |> List.sortByDescending (fun (_, _, ordering_value) -> ordering_value)

            return
              all_scopes_with_ordering_values
              |> List.map (fun (scope, return_value, _) -> scope, return_value)
        }

      return all_scopes |> Seq.map snd, db
    }


  let db_ops<'runtimeContext, 'customExt when 'customExt: comparison>
    ()
    : DBTypeClass<
        'runtimeContext,
        MutableMemoryDB<'runtimeContext, 'customExt>,
        ValueExt<'runtimeContext, MutableMemoryDB<'runtimeContext, 'customExt>, 'customExt>
       >
    =
    let actual_lookup
      (
        db: MutableMemoryDB<'runtimeContext, 'customExt>,
        dir,
        relation: SchemaRelation<'ext>,
        from: SchemaEntity<'ext>,
        to_: SchemaEntity<'ext>
      )
      (v:
        Value<
          TypeValue<ValueExt<'runtimeContext, MutableMemoryDB<'runtimeContext, 'customExt>, 'customExt>>,
          ValueExt<'runtimeContext, MutableMemoryDB<'runtimeContext, 'customExt>, 'customExt>
         >)
      : Reader<
          List<
            Value<
              TypeValue<ValueExt<'runtimeContext, MutableMemoryDB<'runtimeContext, 'customExt>, 'customExt>>,
              ValueExt<'runtimeContext, MutableMemoryDB<'runtimeContext, 'customExt>, 'customExt>
             >
           >,
          'runtimeContext,
          Errors<unit>
         >
      =
      sum {
        let! relation_ref =
          db.relations
          |> Map.tryFind relation.Name
          |> sum.OfOption(Errors.Singleton () (fun () -> "Relation not found"))

        let _source_entity_ref, target_entity_ref, source_to_targets =
          match dir with
          | FromTo -> from, to_, relation_ref.FromTo
          | ToFrom -> to_, from, relation_ref.ToFrom

        let source_id = v

        let! targets =
          db.entities
          |> Map.tryFind target_entity_ref.Name
          |> sum.OfOption(Errors.Singleton () (fun () -> "Target entity not found"))

        let target_ids =
          source_to_targets |> Map.tryFind source_id |> Option.defaultValue Set.empty

        return!
          target_ids
          |> Set.toSeq
          |> Seq.map (fun target_id ->
            sum {
              let! target_v =
                targets
                |> Map.tryFind target_id
                |> sum.OfOption(Errors.Singleton () (fun () -> "Target ID not found"))

              return Value.Tuple [ target_id; target_v ]
            })
          |> sum.All
      }
      |> reader.OfSum

    { DB =
        { entities = Map.empty
          relations = Map.empty
          operations = [] }
      BeginTransaction = fun _ -> sum { return Guid.CreateVersion7() }
      CommitTransaction = fun _ _ -> sum { return () }
      RunQuery =
        fun query range ->
          reader {

            let! (values, _db) =
              runQuery query
              |> reader.OfSum
              |> reader.MapError(Errors.MapContext(replaceWith ()))

            match range with
            | None -> return values |> Seq.toList
            | Some(skip, take) -> return values |> Seq.skip skip |> Seq.truncate take |> Seq.toList
          }
      Create =
        fun
            (entity_ref:
              EntityRef<
                MutableMemoryDB<'runtimeContext, 'customExt>,
                ValueExt<'runtimeContext, MutableMemoryDB<'runtimeContext, 'customExt>, 'customExt>
               >)
            create_arg ->
          reader {
            let _, db, entity, _ = entity_ref
            let entityId = create_arg.Id
            let value = create_arg.Value

            do
              db.entities <-
                db.entities
                |> Map.change entity.Name (function
                  | Some entities -> Some(entities |> Map.add entityId value)
                  | None -> Some(Map.empty |> Map.add entityId value))

            do db.operations <- db.operations @ [ Create(entity.Name, create_arg.Id, create_arg.Value) ]

            return Value.Tuple [ entityId; value ]
          }
      Upsert =
        fun
            (entity_ref:
              EntityRef<
                MutableMemoryDB<'runtimeContext, 'customExt>,
                ValueExt<'runtimeContext, MutableMemoryDB<'runtimeContext, 'customExt>, 'customExt>
               >)
            update_arg ->
          reader {
            let _, db, entity, _ = entity_ref
            let entityId = update_arg.Id
            let value = update_arg.Value

            do
              db.entities <-
                db.entities
                |> Map.change entity.Name (function
                  | Some entities -> Some(entities |> Map.add entityId value)
                  | None -> Some(Map.empty |> Map.add entityId value))

            do db.operations <- db.operations @ [ Update(entity.Name, entityId, value) ]

            return Value.Tuple [ entityId; value ]
          }

      Delete =
        fun
            (entity_ref:
              EntityRef<
                MutableMemoryDB<'runtimeContext, 'customExt>,
                ValueExt<'runtimeContext, MutableMemoryDB<'runtimeContext, 'customExt>, 'customExt>
               >)
            id_to_delete ->
          reader {
            let _, db, entity, _ = entity_ref

            do
              db.entities <-
                db.entities
                |> Map.change entity.Name (function
                  | Some entities -> Some(entities |> Map.remove id_to_delete)
                  | None -> None)

            do db.operations <- db.operations @ [ Delete(entity.Name, id_to_delete) ]

            return ()
          }

      DeleteMany =
        fun
            (entity_ref:
              EntityRef<
                MutableMemoryDB<'runtimeContext, 'customExt>,
                ValueExt<'runtimeContext, MutableMemoryDB<'runtimeContext, 'customExt>, 'customExt>
               >)
            ids_to_delete ->
          reader {
            let _, db, entity, _ = entity_ref

            let! _ =
              ids_to_delete
              |> Seq.map (fun id_to_delete ->
                reader {
                  do
                    db.entities <-
                      db.entities
                      |> Map.change entity.Name (function
                        | Some entities -> Some(entities |> Map.remove id_to_delete)
                        | None -> None)

                  do db.operations <- db.operations @ [ Delete(entity.Name, id_to_delete) ]

                  return ()
                })
              |> reader.All

            return ()
          }
      Link =
        fun relation_ref unlink_arg ->
          reader {
            let fromId, toId = unlink_arg.FromId, unlink_arg.ToId
            let _, db, relation, _, _, _ = relation_ref

            let add_link
              (rel: MemoryDBRelation<'runtimeContext, 'customExt>)
              : MemoryDBRelation<'runtimeContext, 'customExt> =
              { rel with
                  All = rel.All |> Set.add (fromId, toId)
                  FromTo =
                    rel.FromTo
                    |> Map.change fromId (function
                      | Some toSet -> Some(toSet |> Set.add toId)
                      | None -> Some(Set.empty |> Set.add toId))
                  ToFrom =
                    rel.ToFrom
                    |> Map.change toId (function
                      | Some fromSet -> Some(fromSet |> Set.add fromId)
                      | None -> Some(Set.empty |> Set.add fromId)) }

            db.relations <-
              db.relations
              |> Map.change relation.Name (function
                | Some rel -> Some(add_link rel)
                | None -> Some(MemoryDBRelation.Empty |> add_link))

            do db.operations <- db.operations @ [ Link(relation.Name, fromId, toId) ]

            return ()

          } //: RelationRef<'db, 'ext> -> UnlinkArgs<'runtimeContext, 'db, 'ext> -> Sum<unit, Errors<Unit>>
      //: RelationRef<'db, 'ext> -> LinkArgs<'runtimeContext, 'db, 'ext> -> Sum<unit, Errors<Unit>>
      Unlink =
        fun relation_ref unlink_arg ->
          reader {
            let fromId, toId = unlink_arg.FromId, unlink_arg.ToId
            let _, db, relation, _, _, _ = relation_ref

            let remove_link
              (rel: MemoryDBRelation<'runtimeContext, 'customExt>)
              : MemoryDBRelation<'runtimeContext, 'customExt> =
              { rel with
                  All = rel.All |> Set.remove (fromId, toId)
                  FromTo =
                    rel.FromTo
                    |> Map.change fromId (function
                      | Some toSet -> Some(toSet |> Set.remove toId)
                      | None -> Some(Set.empty))
                  ToFrom =
                    rel.ToFrom
                    |> Map.change toId (function
                      | Some fromSet -> Some(fromSet |> Set.remove fromId)
                      | None -> Some(Set.empty)) }

            db.relations <-
              db.relations
              |> Map.change relation.Name (function
                | Some rel -> Some(remove_link rel)
                | None -> Some(MemoryDBRelation.Empty |> remove_link))

            do db.operations <- db.operations @ [ Unlink(relation.Name, fromId, toId) ]

            return ()
          } //: RelationRef<'db, 'ext> -> UnlinkArgs<'runtimeContext, 'db, 'ext> -> Sum<unit, Errors<Unit>>
      IsLinked =
        fun relation_ref is_linked_arg ->
          reader {
            let fromId, toId = is_linked_arg.FromId, is_linked_arg.ToId
            let _, db, relation, _, _, _ = relation_ref

            let rel =
              db.relations
              |> Map.tryFind relation.Name
              |> Option.defaultValue MemoryDBRelation.Empty

            return rel.All |> Set.contains (fromId, toId)
          } //: RelationRef<'db, 'ext> -> IsLinkedArgs<'runtimeContext, 'db, 'ext> -> Sum<bool, Errors<Unit>>

      GetById =
        fun entity_ref entityId ->
          option {
            let _, db, entity, _ = entity_ref

            let! entityMap = db.entities |> Map.tryFind entity.Name
            let! value = entityMap |> Map.tryFind entityId
            return value
          }
          |> sum.OfOption((fun () -> $"Entity not found with id: {entityId}") |> Errors.Singleton(()))
          |> reader.OfSum

      //: EntityRef<'db, 'ext> -> Value<TypeValue<'ext>, 'ext> -> Sum<Value<TypeValue<'ext>, 'ext>, Errors<Unit>>
      GetMany =
        fun entity_ref (skip, take) ->
          reader {
            let _, db, entity, _ = entity_ref

            let entityMap =
              db.entities |> Map.tryFind entity.Name |> Option.defaultValue Map.empty

            let values =
              entityMap |> Map.toSeq |> Seq.skip skip |> Seq.truncate take |> Seq.toList

            return values |> Seq.map (fun (id, value) -> Value.Tuple [ id; value ]) |> Seq.toList
          }
      //: EntityRef<'db, 'ext> -> int * int -> Sum<List<Value<TypeValue<'ext>, 'ext>>, Errors<Unit>>
      LookupMaybe =
        fun relation_ref source dir ->
          reader {
            let _, db, relation, from, to_, _ = relation_ref
            let! results = actual_lookup (db, dir, relation, from, to_) source

            // (db: MutableMemoryDB, dir, relation: SchemaRelation<'ext>, from: SchemaEntity<'ext>, to_: SchemaEntity<'ext>)
            // (v: Value<TypeValue<ValueExt<MutableMemoryDB, 'customExt>>, ValueExt<MutableMemoryDB, 'customExt>>)

            return
              match results with
              | [] -> None
              | r :: _ -> Some r
          }
      LookupOne =
        fun relation_ref source dir ->
          reader {
            let _, db, relation, from, to_, _ = relation_ref
            let! results = actual_lookup (db, dir, relation, from, to_) source

            // (db: MutableMemoryDB, dir, relation: SchemaRelation<'ext>, from: SchemaEntity<'ext>, to_: SchemaEntity<'ext>)
            // (v: Value<TypeValue<ValueExt<MutableMemoryDB, 'customExt>>, ValueExt<MutableMemoryDB, 'customExt>>)

            match results with
            | [] -> return! Errors.Singleton () (fun () -> "No related value found") |> reader.Throw
            | r :: _ -> return r
          }
      LookupMany =
        fun relation_ref source dir (skip, truncate) ->
          reader {
            let _, db, relation, from, to_, _ = relation_ref
            let! results = actual_lookup (db, dir, relation, from, to_) source

            // (db: MutableMemoryDB, dir, relation: SchemaRelation<'ext>, from: SchemaEntity<'ext>, to_: SchemaEntity<'ext>)
            // (v: Value<TypeValue<ValueExt<MutableMemoryDB, 'customExt>>, ValueExt<MutableMemoryDB, 'customExt>>)

            return results |> Seq.skip skip |> Seq.truncate truncate |> Seq.toList
          } }

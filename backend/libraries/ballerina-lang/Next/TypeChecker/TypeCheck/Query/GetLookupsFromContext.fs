namespace Ballerina.DSL.Next.Types.TypeChecker

module QueryLookups =
  open Ballerina
  open Ballerina.State.WithError
  open Ballerina.Collections.Map
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Unification
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.LiftOtherSteps

  let rec get_lookups_from_context<'T, 'Id, 'valueExt
    when 'Id: comparison and 'valueExt: comparison>
    (typeCheckNestedQuery:
      ExprQuery<TypeExpr<'valueExt>, Identifier, 'valueExt>
        -> TypeCheckerResult<
          (TypeCheckedExprQuery<'valueExt> *
          TypeValue<'valueExt> *
          Kind *
          TypeCheckContext<'valueExt>),
          'valueExt
         >)
    loc0
    schema
    (ctx: TypeCheckContext<'valueExt>)
    (iterator_bindings: Map<LocalIdentifier, TypeQueryRow<'valueExt>>)
    (q: ExprQueryExpr<TypeExpr<'valueExt>, Identifier, 'valueExt>)
    : TypeCheckerResult<
        Map<ResolvedIdentifier, TypeQueryRow<'valueExt>>,
        'valueExt
       >
    =
    state {
      let get_lookups_from_context =
        get_lookups_from_context<'T, 'Id, 'valueExt>
          typeCheckNestedQuery
          loc0
          schema
          ctx
          iterator_bindings

      match q.Expr with
      | ExprQueryExprRec.QueryLookup(l: Identifier) ->
        if
          l.IsLocalScope
          && iterator_bindings
             |> Map.containsKey (l.LocalName |> LocalIdentifier.Create)
        then
          return Map.empty
        else
          let l = l |> ResolvedIdentifier.FromIdentifier

          match ctx.Values |> Map.tryFind l with
          | Some(TypeValue.Primitive { value = p }, Kind.Star) ->
            return Map.empty |> Map.add l (TypeQueryRow.PrimitiveType(p, false))
          | Some(TypeValue.Sum { value = [ TypeValue.Primitive { value = PrimitiveType.Unit }
                                           TypeValue.Primitive { value = p } ] },
                 Kind.Star) ->
            return Map.empty |> Map.add l (TypeQueryRow.PrimitiveType(p, true))
          | Some((TypeValue.Record _) as t, Kind.Star) ->
            let! entities =
              schema.Entities
              |> OrderedMap.values
              |> NonEmptyList.TryOfList
              |> sum.OfOption(
                Errors.Singleton loc0 (fun () ->
                  $"Type checking error: No entities found in schema for lookup {l}")
              )
              |> state.OfSum

            let! matching_entity_id =
              entities
              |> NonEmptyList.map (fun entity ->
                state {
                  do!
                    TypeValue.Unify(q.Location, t, entity.Id)
                    |> Expr<'T, 'Id, 'valueExt>.liftUnification

                  return entity.Id
                })
              |> state.Any

            return
              Map.empty
              |> Map.add l (TypeQueryRow.PrimaryKey matching_entity_id)
          | _ ->
            return!
              state.Throw(
                Errors.Singleton q.Location (fun () ->
                  $"Type checking error: Undefined identifier {l} or unsupported type for query closure. Only primitives and primary keys are supported.")
              )

      | ExprQueryExprRec.QueryTupleCons items ->
        let! maps = items |> Seq.map get_lookups_from_context |> state.All

        return
          maps
          |> Seq.fold (fun acc m -> Map.merge (fun _ -> id) acc m) Map.empty
      | ExprQueryExprRec.QueryRecordDes(expr, _field, _) ->
        let! map = get_lookups_from_context expr
        return map
      | ExprQueryExprRec.QueryTupleDes(expr, _, _) ->
        let! map = get_lookups_from_context expr
        return map
      | ExprQueryExprRec.QueryConditional(cond, ``then``, ``else``) ->
        let! cond_map = get_lookups_from_context cond
        let! then_map = get_lookups_from_context ``then``
        let! else_map = get_lookups_from_context ``else``

        return
          Map.merge
            (fun _ -> id)
            cond_map
            (Map.merge (fun _ -> id) then_map else_map)
      | ExprQueryExprRec.QueryUnionDes(_expr, _handlers) ->
        return!
          (fun () ->
            $"Error: Query union destruction expressions are not supported in the current implementation")
          |> Errors.Singleton q.Location
          |> state.Throw
      | ExprQueryExprRec.QuerySumDes(_, _) ->
        return!
          (fun () ->
            $"Error: Query sum destruction expressions are not supported in the current implementation")
          |> Errors.Singleton q.Location
          |> state.Throw
      | ExprQueryExprRec.QueryApply(func, arg) ->
        let! func_map = get_lookups_from_context func
        let! arg_map = get_lookups_from_context arg
        return Map.merge (fun _ -> id) func_map arg_map
      | ExprQueryExprRec.QueryIntrinsic(_, _) -> return Map.empty
      | ExprQueryExprRec.QueryConstant(_) -> return Map.empty
      | ExprQueryExprRec.QueryClosureValue(_, _) -> return Map.empty
      | ExprQueryExprRec.QueryCastTo(v, _) -> return! get_lookups_from_context v
      | ExprQueryExprRec.QueryCount q
      | ExprQueryExprRec.QueryExists q
      | ExprQueryExprRec.QueryArray q ->
        let! q', _, _, _ = q |> typeCheckNestedQuery
        return q'.Closure
      | ExprQueryExprRec.QueryCountEvaluated _
      | ExprQueryExprRec.QueryExistsEvaluated _
      | ExprQueryExprRec.QueryArrayEvaluated _ -> return Map.empty
    }

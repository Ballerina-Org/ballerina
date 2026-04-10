namespace Ballerina.DSL.Next.Types.TypeChecker

module Query =
  open Ballerina.StdLib.String
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Collections.Option
  open Ballerina.Collections.Map
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open System
  open Ballerina.StdLib.Object
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Unification
  open Ballerina.DSL.Next.Types.TypeChecker.AdHocPolymorphicOperators
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Eval
  open Ballerina.DSL.Next.Types.TypeChecker.LiftOtherSteps
  open Ballerina.DSL.Next.Types.TypeChecker.Primitive
  open Ballerina.DSL.Next.Types.TypeChecker.Lookup
  open Ballerina.DSL.Next.Types.TypeChecker.Lambda
  open Ballerina.DSL.Next.Types.TypeChecker.Apply
  open Ballerina.DSL.Next.Types.TypeChecker.If
  open Ballerina.DSL.Next.Types.TypeChecker.Let
  open Ballerina.DSL.Next.Types.TypeChecker.RecordCons
  open Ballerina.DSL.Next.Types.TypeChecker.RecordWith
  open Ballerina.DSL.Next.Types.TypeChecker.RecordDes
  open Ballerina.DSL.Next.Types.TypeChecker.UnionDes
  open Ballerina.DSL.Next.Types.TypeChecker.SumDes
  open Ballerina.DSL.Next.Types.TypeChecker.SumCons
  open Ballerina.DSL.Next.Types.TypeChecker.TupleDes
  open Ballerina.DSL.Next.Types.TypeChecker.TupleCons
  open Ballerina.DSL.Next.Types.TypeChecker.TypeLambda
  open Ballerina.Fun
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Next.Types.TypeChecker.QueryUtilities
  open Ballerina.DSL.Next.Types.TypeChecker.QueryOperators
  open Ballerina.DSL.Next.Types.TypeChecker.QueryLookups
  open Ballerina.DSL.Next.Types.TypeChecker.QueryCaseTupleCons
  open Ballerina.DSL.Next.Types.TypeChecker.QueryCaseLookupFullyQualified
  open Ballerina.DSL.Next.Types.TypeChecker.QueryCaseLookupLocalScope
  open Ballerina.DSL.Next.Types.TypeChecker.QueryCaseConstant
  open Ballerina.DSL.Next.Types.TypeChecker.QueryCaseClosureValue
  open Ballerina.DSL.Next.Types.TypeChecker.QueryCaseCount
  open Ballerina.DSL.Next.Types.TypeChecker.QueryCaseExists
  open Ballerina.DSL.Next.Types.TypeChecker.QueryCaseArray
  open Ballerina.DSL.Next.Types.TypeChecker.QueryCaseUnsupported
  open Ballerina.DSL.Next.Types.TypeChecker.QueryCaseTupleDes
  open Ballerina.DSL.Next.Types.TypeChecker.QueryCaseRecordDes
  open Ballerina.DSL.Next.Types.TypeChecker.QueryCaseApplyIntrinsic
  open Ballerina.DSL.Next.Types.TypeChecker.QueryCaseConditional

  type QueryTypeCheckContext<'valueExt> =
    { Iterators: Map<LocalIdentifier, TypeQueryRow<'valueExt>>
      Closure: Map<ResolvedIdentifier, TypeQueryRow<'valueExt>> }

    static member Create iterators closure =
      { Iterators = iterators
        Closure = closure }

  type QueryTypeCheckerResult<'r, 'valueExt when 'valueExt: comparison> =
    State<
      'r,
      TypeCheckContext<'valueExt>,
      TypeCheckState<'valueExt>,
      Errors<Location>
     >

  type QueryTypeChecker<'input, 'valueExt when 'valueExt: comparison> =
    QueryTypeCheckContext<'valueExt>
      -> 'input
      -> QueryTypeCheckerResult<
        TypeCheckedExprQueryExpr<'valueExt> * TypeQueryRow<'valueExt>,
        'valueExt
       >

  type ExprQueryExpr<'T, 'Id, 've when 'Id: comparison> with
    static member internal TypeCheckQueryExpr<'valueExt
      when 'valueExt: comparison>
      (typeCheckQuery:
        ExprQuery<TypeExpr<'valueExt>, Identifier, 'valueExt>
          -> TypeCheckerResult<
            (TypeCheckedExprQuery<'valueExt> *
            TypeValue<'valueExt> *
            Kind *
            TypeCheckContext<'valueExt>),
            'valueExt
           >)
      (depth: int)
      (_loc0: Location)
      : QueryTypeChecker<
          ExprQueryExpr<TypeExpr<'valueExt>, Identifier, 'valueExt>,
          'valueExt
         >
      =
      fun identifiers_context expr ->
        let recurExpr
          (nextExpr: ExprQueryExpr<TypeExpr<'valueExt>, Identifier, 'valueExt>)
          : QueryTypeCheckerResult<
              (TypeCheckedExprQueryExpr<'valueExt> * TypeQueryRow<'valueExt>),
              'valueExt
             >
          =
          ExprQueryExpr.TypeCheckQueryExpr
            typeCheckQuery
            (depth + 1)
            nextExpr.Location
            identifiers_context
            nextExpr
        // let (=>) c e = typeCheckExpr c e

        state {
          match expr.Expr with
          | ExprQueryExprRec.QueryTupleCons args ->
            return! typeCheckQueryTupleCons recurExpr expr args
          | ExprQueryExprRec.QueryLookup(Identifier.FullyQualified _ as l) ->
            return!
              typeCheckQueryLookupFullyQualified
                expr.Location
                identifiers_context.Closure
                expr
                l
          | ExprQueryExprRec.QueryLookup(Identifier.LocalScope v as l) ->
            return!
              typeCheckQueryLookupLocalScope
                expr.Location
                identifiers_context.Iterators
                identifiers_context.Closure
                expr
                v
                l

          | ExprQueryExprRec.QueryTupleDes(tuple, item, _) ->
            return!
              typeCheckQueryTupleDes expr.Location recurExpr expr tuple item
          | ExprQueryExprRec.QueryRecordDes(record, field, _) ->
            return!
              typeCheckQueryRecordDes expr.Location recurExpr expr record field
          | ExprQueryExprRec.QueryConstant c ->
            return! typeCheckQueryConstant expr c

          | ExprQueryExprRec.QueryApply({ Expr = ExprQueryExprRec.QueryIntrinsic(intrinsic,
                                                                                 _) },
                                        arg) ->
            return!
              typeCheckQueryApplyIntrinsic
                expr.Location
                recurExpr
                expr
                intrinsic
                arg

          | ExprQueryExprRec.QueryClosureValue(v, t) ->
            return! typeCheckQueryClosureValue expr v t
          | ExprQueryExprRec.QueryCount q ->
            return! typeCheckQueryCount typeCheckQuery expr q
          | ExprQueryExprRec.QueryExists q ->
            return! typeCheckQueryExists typeCheckQuery expr q
          | ExprQueryExprRec.QueryArray q ->
            return! typeCheckQueryArray typeCheckQuery expr q
          | ExprQueryExprRec.QueryConditional(cond, thenExpr, elseExpr) ->
            return!
              typeCheckQueryConditional
                expr.Location
                recurExpr
                expr
                cond
                thenExpr
                elseExpr
          | _ -> return! typeCheckQueryUnsupported expr
        }


  and Expr<'T, 'Id, 've when 'Id: comparison> with
    static member internal TypeCheckQuery<'valueExt when 'valueExt: comparison>
      (config: TypeCheckingConfig<'valueExt>)
      (typeCheckExpr: ExprTypeChecker<'valueExt>)
      : TypeCheckerQuery<
          ExprQuery<TypeExpr<'valueExt>, Identifier, 'valueExt>,
          'valueExt
         >
      =
      fun
          _context_t
          (closure_bindings: Map<LocalIdentifier, TypeQueryRow<'valueExt>>)
          initial_lookups
          (query: ExprQuery<TypeExpr<'valueExt>, Identifier, 'valueExt>) ->
        let { QueryTypeSymbol = query_type_symbol
              MkQueryType = mk_query_type } =
          config
        // let (!) = typeCheckExpr context_t
        let (=>) c e = typeCheckExpr c e
        let loc0 = query.Location

        let ofSum (p: Sum<'a, Errors<Unit>>) =
          p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

        // let error e = Errors.Singleton loc0 e

        state {
          // do Console.WriteLine($"TypeApply: fExpr = {fExpr}")

          match query with
          | UnionQueries(q1, q2) ->
            let! q1_e, q1_t, _q1_k, _ctx =
              q1
              |> Expr.TypeCheckQuery
                config
                typeCheckExpr
                _context_t
                closure_bindings
                initial_lookups

            let! q2_e, q2_t, _q2_k, _ctx =
              q2
              |> Expr.TypeCheckQuery
                config
                typeCheckExpr
                _context_t
                closure_bindings
                initial_lookups

            match q1_t, q2_t with
            | TypeValue.Imported { Sym = sym1
                                   Arguments = [ TypeValue.Schema schema1
                                                 TypeValue.QueryRow query_row1 ] },
              TypeValue.Imported { Sym = sym2
                                   Arguments = [ TypeValue.Schema schema2
                                                 TypeValue.QueryRow query_row2 ] } when
              sym1 = query_type_symbol && sym2 = query_type_symbol
              ->
              do!
                TypeValue.Unify(
                  loc0,
                  TypeValue.Schema schema1,
                  TypeValue.Schema schema2
                )
                |> Expr<'T, 'Id, 'valueExt>.liftUnification

              do!
                TypeValue.Unify(
                  loc0,
                  TypeValue.QueryRow query_row1,
                  TypeValue.QueryRow query_row2
                )
                |> Expr<'T, 'Id, 'valueExt>.liftUnification

              let return_type = mk_query_type schema1 query_row1
              let! ctx = state.GetContext()

              return
                TypeCheckedExprQuery.UnionQueries(q1_e, q2_e),
                return_type,
                Kind.Star,
                ctx
            | _ ->
              return!
                (fun () ->
                  $"Type checking error: Both sides of a union query must be query expressions, but got {q1_t} and {q2_t}")
                |> Errors.Singleton loc0
                |> state.Throw

          | SimpleQuery { Iterators = iterators
                          Joins = joins_expr
                          Where = where_expr
                          Select = select_expr
                          OrderBy = orderby_expr
                          Distinct = distinct } ->
            let! iterators =
              iterators
              |> Seq.map (fun iterator ->
                state {
                  let! type_checked_source_expr, _ctx = None => iterator.Source
                  let t = type_checked_source_expr.Type
                  let _k = type_checked_source_expr.Kind

                  return!
                    state {
                      match t with
                      | TypeValue.Imported { Sym = sym
                                             Arguments = [ TypeValue.Schema schema
                                                           TypeValue.QueryRow query_row ] } when
                        sym = query_type_symbol
                        ->
                        return
                          schema,
                          iterator.Var,
                          iterator.VarType,
                          iterator.Location,
                          type_checked_source_expr,
                          query_row
                      | TypeValue.Entity(schema, _, e', e_id) ->
                        let vectors =
                          match e' with
                          | TypeValue.Record({ value = fields' }) ->
                            let fields' =
                              fields'
                              |> OrderedMap.toMap
                              |> Map.filter (fun _k (t, _) ->
                                match t with
                                | TypeValue.Primitive { value = PrimitiveType.Vector } ->
                                  true
                                | _ -> false)

                            fields'
                            |> Map.toSeq
                            |> Seq.map (fun (k, _) ->
                              k.Name.LocalName |> LocalIdentifier.Create,
                              TypeQueryRow.PrimitiveType(
                                PrimitiveType.Vector,
                                false
                              ))
                            |> List.ofSeq
                          | _ -> []

                        let fields =
                          ("Id" |> LocalIdentifier.Create,
                           TypeQueryRow.PrimaryKey e_id)
                          :: ("Value" |> LocalIdentifier.Create,
                              TypeQueryRow.Json e')
                          :: vectors
                          |> Map.ofList

                        return
                          schema,
                          iterator.Var,
                          iterator.VarType,
                          iterator.Location,
                          type_checked_source_expr,
                          fields |> TypeQueryRow.Record
                      | TypeValue.Relation(schema,
                                           _,
                                           _,
                                           _f,
                                           _f',
                                           f_id,
                                           _t,
                                           _t',
                                           t_id) ->
                        let fields =
                          ("FromId" |> LocalIdentifier.Create,
                           TypeQueryRow.PrimaryKey f_id)
                          :: ("ToId" |> LocalIdentifier.Create,
                              TypeQueryRow.PrimaryKey t_id)
                          :: []
                          |> Map.ofList

                        return
                          schema,
                          iterator.Var,
                          iterator.VarType,
                          iterator.Location,
                          type_checked_source_expr,
                          fields |> TypeQueryRow.Record
                      | _ ->
                        return!
                          (fun () ->
                            $"Type checking error: Expected an entity or relation type, but got {t}")
                          |> Errors.Singleton iterator.Location
                          |> state.Throw
                    }
                })
              // |> state.MapError (Errors.MapContext(replaceWith loc0))
              |> state.All

            // List<Var * Expr<TypeValue<'valueExt>,ResolvedIdentifier,'valueExt> * TypeValue<'valueExt>>
            let iterator_schemas =
              iterators
              |> Seq.map (fun (schema, _, _, _, _, _) -> schema)
              |> Seq.toList

            let! schema =
              state {
                match iterator_schemas with
                | [] ->
                  return!
                    (fun () ->
                      $"Type checking error: At least one iterator is required in a query expression")
                    |> Errors.Singleton loc0
                    |> state.Throw
                | schema :: other_schemas ->
                  do!
                    other_schemas
                    |> Seq.map (fun other_schema ->
                      state {
                        do!
                          TypeValue.Unify(
                            loc0,
                            TypeValue.Schema schema,
                            TypeValue.Schema other_schema
                          )
                          |> Expr<'T, 'Id, 'valueExt>.liftUnification
                      })
                    |> state.All
                    |> state.Ignore

                  return schema
              }

            let iterator_bindings =
              iterators
              |> Seq.map (fun (_, v, _, _, _, q) ->
                v.Name |> LocalIdentifier.Create, q)
              |> Map.ofSeq
              |> Map.merge (fun _ -> id) closure_bindings

            do!
              iterators
              |> Seq.map (fun (_, v, maybeVarType, varLoc, _, qRowType) ->
                match maybeVarType with
                | Some _ -> state { return () }
                | None ->
                  TypeCheckState.bindInlayHint (
                    varLoc,
                    v.Name,
                    TypeValue.QueryRow qRowType
                  ))
              |> state.All
              |> state.Ignore

            let! ctx = state.GetContext()

            let typeCheckNestedQuery q =
              q
              |> Expr.TypeCheckQuery
                config
                typeCheckExpr
                _context_t
                iterator_bindings
                initial_lookups

            let get_lookups_from_context =
              QueryLookups.get_lookups_from_context<'T, 'Id, 'valueExt>
                typeCheckNestedQuery
                loc0
                schema
                ctx
                iterator_bindings

            let! where_lookups =
              state {
                match where_expr with
                | Some where_expr -> return! get_lookups_from_context where_expr
                | None -> return initial_lookups
              }

            let! select_lookups = get_lookups_from_context select_expr

            let select_lookups =
              where_lookups |> Map.merge (fun _ -> id) select_lookups

            let! orderby_lookups =
              state {
                match orderby_expr with
                | Some(orderby_expr, _) ->
                  return! get_lookups_from_context orderby_expr
                | None -> return Map.empty
              }

            let select_orderby_lookups =
              select_lookups |> Map.merge (fun _ -> id) orderby_lookups

            let! iterators =
              iterators
              |> NonEmptyList.TryOfList
              |> sum.OfOption(
                Errors.Singleton loc0 (fun () ->
                  "Type checking error: At least one iterator is required in a query expression")
              )
              |> state.OfSum

            let queryTypeCheckingContext =
              QueryTypeCheckContext<_>.Create
                iterator_bindings
                select_orderby_lookups

            let typeCheckQuery =
              Expr.TypeCheckQuery
                config
                typeCheckExpr
                _context_t
                iterator_bindings
                initial_lookups

            let! joins_expr' =
              state {
                match joins_expr with
                | None -> return None
                | Some joins_expr ->
                  let joins_expr = joins_expr |> Seq.toList

                  let! joins_expr' =
                    joins_expr
                    |> Seq.map (fun join_expr ->
                      state {
                        let! left_e, left_t =
                          ExprQueryExpr.TypeCheckQueryExpr
                            typeCheckQuery
                            0
                            join_expr.Left.Location
                            queryTypeCheckingContext
                            join_expr.Left

                        let! right_e, right_t =
                          ExprQueryExpr.TypeCheckQueryExpr
                            typeCheckQuery
                            0
                            join_expr.Right.Location
                            queryTypeCheckingContext
                            join_expr.Right

                        let! left_t =
                          left_t |> TypeQueryRow.AsPrimaryKey |> ofSum

                        let! right_t =
                          right_t |> TypeQueryRow.AsPrimaryKey |> ofSum

                        do!
                          TypeValue.Unify(join_expr.Location, left_t, right_t)
                          |> Expr<'T, 'Id, 'valueExt>.liftUnification

                        let res: TypeCheckedExprQueryJoin<'valueExt> =
                          { Location = join_expr.Location
                            Left = left_e
                            Right = right_e }

                        res
                      })
                    |> state.All

                  return Some joins_expr'
              }

            let! joins_expr' =
              joins_expr'
              |> Option.map NonEmptyList.TryOfList
              |> Option.map (
                sum.OfOption(
                  Errors.Singleton loc0 (fun () ->
                    "Type checking error: At least one join is required in a query expression")
                )
                >> state.OfSum
              )
              |> state.RunOption

            let! where_expr' =
              state {
                match where_expr with
                | None -> return None
                | Some where_expr ->

                  let! where_expr', where_expr'_t =
                    ExprQueryExpr.TypeCheckQueryExpr
                      typeCheckQuery
                      0
                      where_expr.Location
                      queryTypeCheckingContext
                      where_expr

                  match where_expr'_t with
                  | TypeQueryRow.PrimitiveType(PrimitiveType.Bool, _)
                  | TypeQueryRow.Json(TypeValue.Primitive { value = PrimitiveType.Bool }) ->
                    return Some where_expr'
                  | _ ->
                    return!
                      (fun () ->
                        $"Type checking error: Expected boolean type for where clause, but got {where_expr'_t}")
                      |> Errors.Singleton where_expr.Location
                      |> state.Throw
              }

            let! select_expr', select_expr'_t =
              ExprQueryExpr.TypeCheckQueryExpr
                typeCheckQuery
                0
                select_expr.Location
                queryTypeCheckingContext
                select_expr

            let! orderby_expr' =
              state {
                match orderby_expr with
                | None -> return None
                | Some(orderby_expr, direction) ->
                  let! orderby_expr', _ =
                    ExprQueryExpr.TypeCheckQueryExpr
                      typeCheckQuery
                      0
                      orderby_expr.Location
                      queryTypeCheckingContext
                      orderby_expr

                  return Some(orderby_expr', direction)
              }

            let! distinct_expr' =
              state {
                match distinct with
                | None -> return None
                | Some distinct_expr ->
                  let! distinct_expr', _distinct_expr_t =
                    ExprQueryExpr.TypeCheckQueryExpr
                      typeCheckQuery
                      0
                      distinct_expr.Location
                      queryTypeCheckingContext
                      distinct_expr

                  return Some distinct_expr'
              }

            let return_expr: TypeCheckedExprQuery<'valueExt> =
              { TypeCheckedSimpleQuery.Iterators =
                  iterators
                  |> NonEmptyList.map
                    (fun (_, v, _, _, source_expr, q_row_t) ->
                      { TypeCheckedExprQueryIterator.Location =
                          source_expr.Location
                        TypeCheckedExprQueryIterator.Var = v
                        TypeCheckedExprQueryIterator.VarType = q_row_t
                        TypeCheckedExprQueryIterator.Source = source_expr })
                TypeCheckedSimpleQuery.Joins = joins_expr'
                TypeCheckedSimpleQuery.Where = where_expr'
                TypeCheckedSimpleQuery.Select = select_expr'
                TypeCheckedSimpleQuery.OrderBy = orderby_expr'
                TypeCheckedSimpleQuery.Distinct = distinct_expr'
                TypeCheckedSimpleQuery.Closure = select_orderby_lookups
                TypeCheckedSimpleQuery.DeserializeFrom = select_expr'_t }
              |> TypeCheckedExprQuery.SimpleQuery

            let return_type = mk_query_type schema select_expr'_t

            let! ctx = state.GetContext()

            return return_expr, return_type, Kind.Star, ctx
        }

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


  type QueryTypeCheckContext<'valueExt> =
    { Iterators: Map<LocalIdentifier, TypeQueryRow<'valueExt>>
      Closure: Map<ResolvedIdentifier, TypeQueryRow<'valueExt>> }

    static member Create iterators closure =
      { Iterators = iterators
        Closure = closure }

  type QueryTypeCheckerResult<'r, 'valueExt when 'valueExt: comparison> =
    State<'r, TypeCheckContext<'valueExt>, TypeCheckState<'valueExt>, Errors<Location>>

  type QueryTypeChecker<'input, 'valueExt when 'valueExt: comparison> =
    QueryTypeCheckContext<'valueExt>
      -> 'input
      -> QueryTypeCheckerResult<
        ExprQueryExpr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt> * TypeQueryRow<'valueExt>,
        'valueExt
       >

  type ExprQueryExpr<'T, 'Id, 've when 'Id: comparison> with
    static member internal TypeCheckQueryExpr<'valueExt when 'valueExt: comparison>
      (loc0: Location)
      : QueryTypeChecker<ExprQueryExpr<TypeExpr<'valueExt>, Identifier, 'valueExt>, 'valueExt> =
      fun identifiers_context expr ->
        let (!) = ExprQueryExpr.TypeCheckQueryExpr loc0 identifiers_context
        // let (=>) c e = typeCheckExpr c e

        let ofSum (p: Sum<'a, Errors<Unit>>) =
          p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

        // let error e = Errors.Singleton loc0 e

        let two_int32s =
          function
          | [ TypeQueryRow.PrimitiveType(PrimitiveType.Int32, _); TypeQueryRow.PrimitiveType(PrimitiveType.Int32, _) ]
          | [ TypeQueryRow.Json(TypeValue.Primitive { value = PrimitiveType.Int32 })
              TypeQueryRow.PrimitiveType(PrimitiveType.Int32, _) ]
          | [ TypeQueryRow.PrimitiveType(PrimitiveType.Int32, _)
              TypeQueryRow.Json(TypeValue.Primitive { value = PrimitiveType.Int32 }) ]
          | [ TypeQueryRow.Json(TypeValue.Primitive { value = PrimitiveType.Int32 })
              TypeQueryRow.PrimitiveType(PrimitiveType.Int32, _) ] -> true
          | [ TypeQueryRow.Json(TypeValue.Primitive { value = PrimitiveType.Int32 })
              TypeQueryRow.Json(TypeValue.Primitive { value = PrimitiveType.Int32 }) ] -> true
          | _ -> false

        state {
          match expr.Expr with
          | ExprQueryExprRec.QueryTupleCons args ->
            let! args_e_t = args |> Seq.map (fun arg -> !arg) |> state.All

            let args_t = args_e_t |> Seq.map snd |> Seq.toList

            return
              ExprQueryExprRec.QueryTupleCons(args_e_t |> Seq.map fst |> Seq.toList)
              |> ExprQueryExpr.Create expr.Location,
              TypeQueryRow.Tuple args_t
          | ExprQueryExprRec.QueryLookup(Identifier.FullyQualified _ as l) ->
            let l = l |> ResolvedIdentifier.FromIdentifier

            let! t =
              identifiers_context.Closure
              |> Map.tryFindWithError
                l
                "query closure variable"
                (fun () -> $"Type checking error: Undefined closure variable {l}")
                ()
              |> ofSum

            return ExprQueryExprRec.QueryLookup l |> ExprQueryExpr.Create expr.Location, t
          | ExprQueryExprRec.QueryLookup(Identifier.LocalScope v as l) ->
            return!
              state.Either
                (state {
                  let! t =
                    identifiers_context.Iterators
                    |> Map.tryFindWithError
                      (v |> LocalIdentifier.Create)
                      "query iteration variable"
                      (fun () -> $"Type checking error: Undefined iterator variable {v}")
                      ()
                    |> ofSum

                  return
                    ExprQueryExprRec.QueryLookup(v |> ResolvedIdentifier.Create)
                    |> ExprQueryExpr.Create expr.Location,
                    t
                })
                (state {
                  let l = l |> ResolvedIdentifier.FromIdentifier

                  let! t =
                    identifiers_context.Closure
                    |> Map.tryFindWithError
                      l
                      "query closure variable"
                      (fun () -> $"Type checking error: Undefined closure variable {l}")
                      ()
                    |> ofSum

                  return ExprQueryExprRec.QueryLookup l |> ExprQueryExpr.Create expr.Location, t
                })

          | ExprQueryExprRec.QueryTupleDes(tuple, item) ->
            let! tuple_e, tuple_t = !tuple

            let! tuple_t_elements = tuple_t |> TypeQueryRow.AsTuple |> ofSum

            if item.Index - 1 < tuple_t_elements.Length then
              let item_t = tuple_t_elements.[item.Index - 1]

              return
                ExprQueryExprRec.QueryTupleDes(tuple_e, item)
                |> ExprQueryExpr.Create expr.Location,
                item_t
            else
              return!
                (fun () ->
                  $"Type checking error: Tuple type {tuple_t} has only {tuple_t_elements.Length} elements, but tried to access item {item.Index}")
                |> Errors.Singleton loc0
                |> state.Throw
          | ExprQueryExprRec.QueryRecordDes(record, field) ->
            let! record_e, record_t = !record

            return!
              state.Either
                (state {
                  let! record_t = record_t |> TypeQueryRow.AsRecord |> ofSum

                  return!
                    state {

                      let! field_t =
                        record_t
                        |> Map.tryFindWithError
                          (field.LocalName |> LocalIdentifier.Create)
                          "field in query record desugarization"
                          (fun () -> $"Type checking error: Field {field} not found")
                          ()
                        |> ofSum

                      return
                        ExprQueryExprRec.QueryRecordDes(record_e, field.LocalName |> ResolvedIdentifier.Create)
                        |> ExprQueryExpr.Create expr.Location,
                        field_t
                    }
                    |> state.MapError(Errors<_>.MapPriority(replaceWith ErrorPriority.High))
                })
                (state {
                  let! json_t = record_t |> TypeQueryRow.AsJson |> ofSum

                  return!
                    state.Either
                      (state {
                        let! record_t = json_t |> TypeValue.AsRecord |> ofSum

                        let! id = TypeCheckState.TryResolveIdentifier(field, loc0)

                        let! field_sym =
                          TypeCheckState.tryFindRecordFieldSymbol (id, loc0)
                          |> state.OfStateReader
                          |> Expr.liftTypeEval

                        let! field_t, _ =
                          record_t
                          |> OrderedMap.tryFindWithError
                            field_sym
                            "field in query record desugarization"
                            ($"Type checking error: Field {field} not found in record type {record_t}")
                          |> ofSum

                        return
                          ExprQueryExprRec.QueryRecordDes(record_e, field.LocalName |> ResolvedIdentifier.Create)
                          |> ExprQueryExpr.Create expr.Location,
                          field_t |> TypeQueryRow.Json
                      })
                      (state {
                        let! record_t = json_t |> TypeValue.AsRecord |> ofSum

                        let! _, (field_t, _) =
                          record_t
                          |> OrderedMap.toSeq
                          |> Seq.tryFind (fun (field_sym, _) -> field_sym.Name = field)
                          |> sum.OfOption(
                            Errors.Singleton loc0 (fun () ->
                              $"Type checking error: Field {field} not found in record type {record_t}")
                          )
                          |> state.OfSum

                        return
                          ExprQueryExprRec.QueryRecordDes(record_e, field.LocalName |> ResolvedIdentifier.Create)
                          |> ExprQueryExpr.Create expr.Location,
                          field_t |> TypeQueryRow.Json
                      })
                    |> state.MapError(Errors<_>.MapPriority(replaceWith ErrorPriority.High))
                })
              |> state.MapError(Errors<_>.FilterHighestPriorityOnly)
          | ExprQueryExprRec.QueryConstant c ->
            match c with
            | PrimitiveValue.Unit ->
              return
                ExprQueryExprRec.QueryConstant c |> ExprQueryExpr.Create expr.Location,
                TypeQueryRow.PrimitiveType(PrimitiveType.Unit, false)
            | PrimitiveValue.Int32(_) ->
              return
                ExprQueryExprRec.QueryConstant c |> ExprQueryExpr.Create expr.Location,
                TypeQueryRow.PrimitiveType(PrimitiveType.Int32, false)
            | PrimitiveValue.Int64(_) ->
              return
                ExprQueryExprRec.QueryConstant c |> ExprQueryExpr.Create expr.Location,
                TypeQueryRow.PrimitiveType(PrimitiveType.Int64, false)
            | PrimitiveValue.Float32(_) ->
              return
                ExprQueryExprRec.QueryConstant c |> ExprQueryExpr.Create expr.Location,
                TypeQueryRow.PrimitiveType(PrimitiveType.Float32, false)
            | PrimitiveValue.Float64(_) ->
              return
                ExprQueryExprRec.QueryConstant c |> ExprQueryExpr.Create expr.Location,
                TypeQueryRow.PrimitiveType(PrimitiveType.Float64, false)
            | PrimitiveValue.Decimal(_) ->
              return
                ExprQueryExprRec.QueryConstant c |> ExprQueryExpr.Create expr.Location,
                TypeQueryRow.PrimitiveType(PrimitiveType.Decimal, false)
            | PrimitiveValue.Bool(_) ->
              return
                ExprQueryExprRec.QueryConstant c |> ExprQueryExpr.Create expr.Location,
                TypeQueryRow.PrimitiveType(PrimitiveType.Bool, false)
            | PrimitiveValue.Guid(_) ->
              return
                ExprQueryExprRec.QueryConstant c |> ExprQueryExpr.Create expr.Location,
                TypeQueryRow.PrimitiveType(PrimitiveType.Guid, false)
            | PrimitiveValue.String(_) ->
              return
                ExprQueryExprRec.QueryConstant c |> ExprQueryExpr.Create expr.Location,
                TypeQueryRow.PrimitiveType(PrimitiveType.String, false)
            | PrimitiveValue.Date(_) ->
              return
                ExprQueryExprRec.QueryConstant c |> ExprQueryExpr.Create expr.Location,
                TypeQueryRow.PrimitiveType(PrimitiveType.DateOnly, false)
            | PrimitiveValue.DateTime(_) ->
              return
                ExprQueryExprRec.QueryConstant c |> ExprQueryExpr.Create expr.Location,
                TypeQueryRow.PrimitiveType(PrimitiveType.DateTime, false)
            | PrimitiveValue.TimeSpan(_) ->
              return
                ExprQueryExprRec.QueryConstant c |> ExprQueryExpr.Create expr.Location,
                TypeQueryRow.PrimitiveType(PrimitiveType.TimeSpan, false)
          | ExprQueryExprRec.QueryApply({ Expr = ExprQueryExprRec.QueryIntrinsic(QueryIntrinsic.GreaterThan) }, arg) ->
            let! arg_e, arg_t = !arg
            let! arg_t_elements = arg_t |> TypeQueryRow.AsTuple |> ofSum

            if arg_t_elements |> two_int32s then
              return
                ExprQueryExprRec.QueryApply(
                  ExprQueryExprRec.QueryIntrinsic(QueryIntrinsic.GreaterThan)
                  |> ExprQueryExpr.Create expr.Location,
                  arg_e
                )
                |> ExprQueryExpr.Create expr.Location,
                TypeQueryRow.PrimitiveType(PrimitiveType.Bool, false)
            else
              return!
                (fun () ->
                  $"Type checking error: invalid type arguments {arg_t_elements} for > operator in query expression")
                |> Errors.Singleton expr.Location
                |> state.Throw

          | ExprQueryExprRec.QueryApply({ Expr = ExprQueryExprRec.QueryIntrinsic(QueryIntrinsic.Multiply) }, arg) ->
            let! arg_e, arg_t = !arg
            let! arg_t_elements = arg_t |> TypeQueryRow.AsTuple |> ofSum

            if arg_t_elements |> two_int32s then
              return
                ExprQueryExprRec.QueryApply(
                  ExprQueryExprRec.QueryIntrinsic(QueryIntrinsic.Multiply)
                  |> ExprQueryExpr.Create expr.Location,
                  arg_e
                )
                |> ExprQueryExpr.Create expr.Location,
                TypeQueryRow.PrimitiveType(PrimitiveType.Int32, false)
            else
              return!
                (fun () ->
                  $"Type checking error: invalid type arguments {arg_t_elements} for * operator in query expression")
                |> Errors.Singleton expr.Location
                |> state.Throw

          | _ ->
            return!
              (fun () ->
                $"Type checking error: Unsupported query expression {expr.Expr.AsFSharpString.ReasonablyClamped}")
              |> Errors.Singleton expr.Location
              |> state.Throw
        }


  type Expr<'T, 'Id, 've when 'Id: comparison> with
    static member internal TypeCheckQuery<'valueExt when 'valueExt: comparison>
      (query_type_symbol: TypeSymbol)
      (mk_query_type: Schema<'valueExt> -> TypeQueryRow<'valueExt> -> TypeValue<'valueExt>)
      (typeCheckExpr: ExprTypeChecker<'valueExt>, loc0: Location)
      : TypeChecker<ExprQuery<TypeExpr<'valueExt>, Identifier, 'valueExt>, 'valueExt> =
      fun
          _context_t
          { Iterators = iterators
            Joins = joins_expr
            Where = where_expr
            Select = select_expr
            OrderBy = orderby_expr } ->
        // let (!) = typeCheckExpr context_t
        let (=>) c e = typeCheckExpr c e

        let ofSum (p: Sum<'a, Errors<Unit>>) =
          p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

        // let error e = Errors.Singleton loc0 e

        state {
          // do Console.WriteLine($"TypeApply: fExpr = {fExpr}")

          let! iterators =
            iterators
            |> Seq.map (fun iterator ->
              state {
                let! type_checked_source_expr, t, _k, _ctx = None => iterator.Source

                return!
                  state {
                    match t with
                    | TypeValue.Imported { Sym = sym
                                           Arguments = [ TypeValue.Schema schema; TypeValue.QueryRow query_row ] } when
                      sym = query_type_symbol
                      ->
                      return schema, iterator.Var, type_checked_source_expr, query_row
                    | TypeValue.Entity(schema, _, e', e_id) ->
                      let vectors =
                        match e' with
                        | TypeValue.Record({ value = fields' }) ->
                          let fields' =
                            fields'
                            |> OrderedMap.toMap
                            |> Map.filter (fun _k (t, _) ->
                              match t with
                              | TypeValue.Primitive { value = PrimitiveType.Vector } -> true
                              | _ -> false)

                          fields'
                          |> Map.toSeq
                          |> Seq.map (fun (k, _) ->
                            k.Name.LocalName |> LocalIdentifier.Create,
                            TypeQueryRow.PrimitiveType(PrimitiveType.Vector, false))
                          |> List.ofSeq
                        | _ -> []

                      let fields =
                        ("Id" |> LocalIdentifier.Create, TypeQueryRow.PrimaryKey e_id)
                        :: ("Value" |> LocalIdentifier.Create, TypeQueryRow.Json e')
                        :: vectors
                        |> Map.ofList

                      return schema, iterator.Var, type_checked_source_expr, fields |> TypeQueryRow.Record
                    | TypeValue.Relation(schema, _, _, _f, _f', f_id, _t, _t', t_id) ->
                      let fields =
                        ("FromId" |> LocalIdentifier.Create, TypeQueryRow.PrimaryKey f_id)
                        :: ("ToId" |> LocalIdentifier.Create, TypeQueryRow.PrimaryKey t_id)
                        :: []
                        |> Map.ofList

                      return schema, iterator.Var, type_checked_source_expr, fields |> TypeQueryRow.Record
                    | _ ->
                      return!
                        (fun () -> $"Type checking error: Expected an entity or relation type, but got {t}")
                        |> Errors.Singleton iterator.Location
                        |> state.Throw
                  }
              })
            // |> state.MapError (Errors.MapContext(replaceWith loc0))
            |> state.All

          // List<Var * Expr<TypeValue<'valueExt>,ResolvedIdentifier,'valueExt> * TypeValue<'valueExt>>
          let iterator_schemas =
            iterators |> Seq.map (fun (schema, _, _, _) -> schema) |> Seq.toList

          let! schema =
            state {
              match iterator_schemas with
              | [] ->
                return!
                  (fun () -> $"Type checking error: At least one iterator is required in a query expression")
                  |> Errors.Singleton loc0
                  |> state.Throw
              | schema :: other_schemas ->
                do!
                  other_schemas
                  |> Seq.map (fun other_schema ->
                    state {
                      do!
                        TypeValue.Unify(loc0, TypeValue.Schema schema, TypeValue.Schema other_schema)
                        |> Expr<'T, 'Id, 'valueExt>.liftUnification
                    })
                  |> state.All
                  |> state.Ignore

                return schema
            }

          let iterator_bindings =
            iterators
            |> Seq.map (fun (_, v, _, q) -> v.Name |> LocalIdentifier.Create, q)
            |> Map.ofSeq

          let! ctx = state.GetContext()

          let rec get_lookups_from_context (q: ExprQueryExpr<_, _, _>) =
            state {
              match q.Expr with
              | ExprQueryExprRec.QueryLookup(l: Identifier) ->
                if
                  l.IsLocalScope
                  && iterator_bindings |> Map.containsKey (l.LocalName |> LocalIdentifier.Create)
                then
                  return Map.empty
                else
                  let l = l |> ResolvedIdentifier.FromIdentifier

                  match ctx.Values |> Map.tryFind l with
                  | Some(TypeValue.Primitive { value = p }, Kind.Star) ->
                    return Map.empty |> Map.add l (TypeQueryRow.PrimitiveType(p, false))
                  | Some(TypeValue.Sum { value = [ TypeValue.Primitive { value = PrimitiveType.Unit }
                                                   TypeValue.Primitive { value = p } ] },
                         Kind.Star) -> return Map.empty |> Map.add l (TypeQueryRow.PrimitiveType(p, true))
                  | _ ->
                    return!
                      state.Throw(
                        Errors.Singleton q.Location (fun () -> $"Type checking error: Undefined identifier {l}")
                      )

              | ExprQueryExprRec.QueryTupleCons items ->
                let! maps = items |> Seq.map get_lookups_from_context |> state.All
                return maps |> Seq.fold (fun acc m -> Map.merge (fun _ -> id) acc m) Map.empty
              | ExprQueryExprRec.QueryRecordDes(expr, _field) ->
                let! map = get_lookups_from_context expr
                return map
              | ExprQueryExprRec.QueryTupleDes(expr, _) ->
                let! map = get_lookups_from_context expr
                return map
              | ExprQueryExprRec.QueryConditional(cond, ``then``, ``else``) ->
                let! cond_map = get_lookups_from_context cond
                let! then_map = get_lookups_from_context ``then``
                let! else_map = get_lookups_from_context ``else``
                return Map.merge (fun _ -> id) cond_map (Map.merge (fun _ -> id) then_map else_map)
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
              | ExprQueryExprRec.QueryIntrinsic(_) -> return Map.empty
              | ExprQueryExprRec.QueryConstant(_) -> return Map.empty
            }

          let! where_lookups =
            state {
              match where_expr with
              | Some where_expr -> return! get_lookups_from_context where_expr
              | None -> return Map.empty
            }

          let! select_lookups = get_lookups_from_context select_expr
          let select_lookups = where_lookups |> Map.merge (fun _ -> id) select_lookups

          let! orderby_lookups =
            state {
              match orderby_expr with
              | Some(orderby_expr, _) -> return! get_lookups_from_context orderby_expr
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
            QueryTypeCheckContext<_>.Create iterator_bindings select_orderby_lookups

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
                        ExprQueryExpr.TypeCheckQueryExpr loc0 queryTypeCheckingContext join_expr.Left

                      let! right_e, right_t =
                        ExprQueryExpr.TypeCheckQueryExpr loc0 queryTypeCheckingContext join_expr.Right

                      let! left_t = left_t |> TypeQueryRow.AsPrimaryKey |> ofSum
                      let! right_t = right_t |> TypeQueryRow.AsPrimaryKey |> ofSum

                      do!
                        TypeValue.Unify(join_expr.Location, left_t, right_t)
                        |> Expr<'T, 'Id, 'valueExt>.liftUnification

                      return
                        { Location = join_expr.Location
                          Left = left_e
                          Right = right_e }
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
                  ExprQueryExpr.TypeCheckQueryExpr loc0 queryTypeCheckingContext where_expr

                match where_expr'_t with
                | TypeQueryRow.PrimitiveType(PrimitiveType.Bool, _)
                | TypeQueryRow.Json(TypeValue.Primitive { value = PrimitiveType.Bool }) -> return Some where_expr'
                | _ ->
                  return!
                    (fun () -> $"Type checking error: Expected boolean type for where clause, but got {where_expr'_t}")
                    |> Errors.Singleton where_expr.Location
                    |> state.Throw
            }

          let! select_expr', select_expr'_t = ExprQueryExpr.TypeCheckQueryExpr loc0 queryTypeCheckingContext select_expr

          let! orderby_expr' =
            state {
              match orderby_expr with
              | None -> return None
              | Some(orderby_expr, direction) ->
                let! orderby_expr', _ = ExprQueryExpr.TypeCheckQueryExpr loc0 queryTypeCheckingContext orderby_expr

                return Some(orderby_expr', direction)
            }

          let return_expr =
            Expr.Query
              { Iterators =
                  iterators
                  |> NonEmptyList.map (fun (_, v, source_expr, q_row_t) ->
                    { Location = source_expr.Location
                      Var = v
                      VarType = q_row_t |> Some
                      Source = source_expr })
                Joins = joins_expr'
                Where = where_expr'
                Select = select_expr'
                OrderBy = orderby_expr' }

          let return_type = mk_query_type schema select_expr'_t

          let! ctx = state.GetContext()

          return return_expr, return_type, Kind.Star, ctx
        }

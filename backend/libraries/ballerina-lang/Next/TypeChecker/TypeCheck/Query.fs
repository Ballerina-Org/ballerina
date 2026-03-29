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

  let two_equal_primitives loc0 expected_primitive_type (t, (e: ExprQueryExpr<_, _, _>)) =
    match t, e.Expr with
    | [ TypeQueryRow.PrimitiveType(p1, is_p1_nullable); TypeQueryRow.PrimitiveType(p2, is_p2_nullable) ], _ when
      p1 = expected_primitive_type && p2 = expected_primitive_type
      ->
      Some(e, is_p1_nullable || is_p2_nullable)
    | [ TypeQueryRow.Json(TypeValue.Primitive { value = p1 }); TypeQueryRow.PrimitiveType(p2, is_p2_nullable) ],
      ExprQueryExprRec.QueryTupleCons [ v1; v2 ] when p1 = expected_primitive_type && p2 = expected_primitive_type ->
      Some(
        [ ExprQueryExprRec.QueryCastTo(v1, TypeQueryRow.PrimitiveType(expected_primitive_type, false))
          |> ExprQueryExpr.Create loc0
          v2 ]
        |> ExprQueryExprRec.QueryTupleCons
        |> ExprQueryExpr.Create loc0,
        is_p2_nullable
      )
    | [ TypeQueryRow.PrimitiveType(p1, is_p1_nullable); TypeQueryRow.Json(TypeValue.Primitive { value = p2 }) ],
      ExprQueryExprRec.QueryTupleCons [ v1; v2 ] when p1 = expected_primitive_type && p2 = expected_primitive_type ->
      Some(
        [ v1
          ExprQueryExprRec.QueryCastTo(v2, TypeQueryRow.PrimitiveType(expected_primitive_type, false))
          |> ExprQueryExpr.Create loc0 ]
        |> ExprQueryExprRec.QueryTupleCons
        |> ExprQueryExpr.Create loc0,
        is_p1_nullable
      )
    | [ TypeQueryRow.Json(TypeValue.Primitive { value = p1 }); TypeQueryRow.Json(TypeValue.Primitive { value = p2 }) ],
      ExprQueryExprRec.QueryTupleCons [ v1; v2 ] when p1 = expected_primitive_type && p2 = expected_primitive_type ->
      Some(
        [ ExprQueryExprRec.QueryCastTo(v1, TypeQueryRow.PrimitiveType(expected_primitive_type, false))
          |> ExprQueryExpr.Create loc0
          ExprQueryExprRec.QueryCastTo(v2, TypeQueryRow.PrimitiveType(expected_primitive_type, false))
          |> ExprQueryExpr.Create loc0 ]
        |> ExprQueryExprRec.QueryTupleCons
        |> ExprQueryExpr.Create loc0,
        false
      )
    | _ -> None

  let binary_operators: Map<QueryIntrinsic, NonEmptyList<PrimitiveType>> =
    [ QueryIntrinsic.Multiply,
      NonEmptyList.ofList<PrimitiveType> (
        PrimitiveType.Int32,
        [ PrimitiveType.Int64
          PrimitiveType.Float32
          PrimitiveType.Float64
          PrimitiveType.Decimal ]
      )
      QueryIntrinsic.Divide,
      NonEmptyList.ofList<PrimitiveType> (
        PrimitiveType.Int32,
        [ PrimitiveType.Int64
          PrimitiveType.Float32
          PrimitiveType.Float64
          PrimitiveType.Decimal ]
      )
      QueryIntrinsic.Minus,
      NonEmptyList.ofList<PrimitiveType> (
        PrimitiveType.Int32,
        [ PrimitiveType.Int64
          PrimitiveType.Float32
          PrimitiveType.Float64
          PrimitiveType.Decimal ]
      )
      QueryIntrinsic.Modulo,
      NonEmptyList.ofList<PrimitiveType> (
        PrimitiveType.Int32,
        [ PrimitiveType.Int64
          PrimitiveType.Float32
          PrimitiveType.Float64
          PrimitiveType.Decimal ]
      )
      QueryIntrinsic.Plus,
      NonEmptyList.ofList<PrimitiveType> (
        PrimitiveType.Int32,
        [ PrimitiveType.Int64
          PrimitiveType.Float32
          PrimitiveType.Float64
          PrimitiveType.Decimal
          PrimitiveType.String ]
      )
      QueryIntrinsic.And, NonEmptyList.ofList<PrimitiveType> (PrimitiveType.Bool, [])
      QueryIntrinsic.Or, NonEmptyList.ofList<PrimitiveType> (PrimitiveType.Bool, []) ]
    |> List.map (fun (op, types) -> op, types)
    |> Map.ofList

  let comparison_operators =
    [ QueryIntrinsic.GreaterThan,
      NonEmptyList.ofList<PrimitiveType> (
        PrimitiveType.Int32,
        [ PrimitiveType.Int64
          PrimitiveType.Float32
          PrimitiveType.Float64
          PrimitiveType.Decimal
          PrimitiveType.TimeSpan
          PrimitiveType.DateOnly
          PrimitiveType.DateTime ]
      )
      QueryIntrinsic.LessThan,
      NonEmptyList.ofList<PrimitiveType> (
        PrimitiveType.Int32,
        [ PrimitiveType.Int64
          PrimitiveType.Float32
          PrimitiveType.Float64
          PrimitiveType.Decimal
          PrimitiveType.TimeSpan
          PrimitiveType.DateOnly
          PrimitiveType.DateTime ]
      )
      QueryIntrinsic.GreaterThanOrEqual,
      NonEmptyList.ofList<PrimitiveType> (
        PrimitiveType.Int32,
        [ PrimitiveType.Int64
          PrimitiveType.Float32
          PrimitiveType.Float64
          PrimitiveType.Decimal
          PrimitiveType.TimeSpan
          PrimitiveType.DateOnly
          PrimitiveType.DateTime ]
      )
      QueryIntrinsic.LessThanOrEqual,
      NonEmptyList.ofList<PrimitiveType> (
        PrimitiveType.Int32,
        [ PrimitiveType.Int64
          PrimitiveType.Float32
          PrimitiveType.Float64
          PrimitiveType.Decimal
          PrimitiveType.TimeSpan
          PrimitiveType.DateOnly
          PrimitiveType.DateTime ]
      )
      QueryIntrinsic.Equals,
      NonEmptyList.ofList<PrimitiveType> (
        PrimitiveType.Int32,
        [ PrimitiveType.Int64
          PrimitiveType.Float32
          PrimitiveType.Float64
          PrimitiveType.Decimal
          PrimitiveType.String
          PrimitiveType.Bool
          PrimitiveType.TimeSpan
          PrimitiveType.DateOnly
          PrimitiveType.DateTime
          PrimitiveType.Guid ]
      )
      QueryIntrinsic.NotEquals,
      NonEmptyList.ofList<PrimitiveType> (
        PrimitiveType.Int32,
        [ PrimitiveType.Int64
          PrimitiveType.Float32
          PrimitiveType.Float64
          PrimitiveType.Decimal
          PrimitiveType.String
          PrimitiveType.Bool
          PrimitiveType.TimeSpan
          PrimitiveType.DateOnly
          PrimitiveType.DateTime
          PrimitiveType.Guid ]
      ) ]
    |> List.map (fun (op, types) -> op, types)
    |> Map.ofList

  let query_constant_to_type loc c =
    state {
      match c with
      | PrimitiveValue.Unit ->
        return
          ExprQueryExprRec.QueryConstant c |> ExprQueryExpr.Create loc,
          TypeQueryRow.PrimitiveType(PrimitiveType.Unit, false)
      | PrimitiveValue.Int32(_) ->
        return
          ExprQueryExprRec.QueryConstant c |> ExprQueryExpr.Create loc,
          TypeQueryRow.PrimitiveType(PrimitiveType.Int32, false)
      | PrimitiveValue.Int64(_) ->
        return
          ExprQueryExprRec.QueryConstant c |> ExprQueryExpr.Create loc,
          TypeQueryRow.PrimitiveType(PrimitiveType.Int64, false)
      | PrimitiveValue.Float32(_) ->
        return
          ExprQueryExprRec.QueryConstant c |> ExprQueryExpr.Create loc,
          TypeQueryRow.PrimitiveType(PrimitiveType.Float32, false)
      | PrimitiveValue.Float64(_) ->
        return
          ExprQueryExprRec.QueryConstant c |> ExprQueryExpr.Create loc,
          TypeQueryRow.PrimitiveType(PrimitiveType.Float64, false)
      | PrimitiveValue.Decimal(_) ->
        return
          ExprQueryExprRec.QueryConstant c |> ExprQueryExpr.Create loc,
          TypeQueryRow.PrimitiveType(PrimitiveType.Decimal, false)
      | PrimitiveValue.Bool(_) ->
        return
          ExprQueryExprRec.QueryConstant c |> ExprQueryExpr.Create loc,
          TypeQueryRow.PrimitiveType(PrimitiveType.Bool, false)
      | PrimitiveValue.Guid(_) ->
        return
          ExprQueryExprRec.QueryConstant c |> ExprQueryExpr.Create loc,
          TypeQueryRow.PrimitiveType(PrimitiveType.Guid, false)
      | PrimitiveValue.String(_) ->
        return
          ExprQueryExprRec.QueryConstant c |> ExprQueryExpr.Create loc,
          TypeQueryRow.PrimitiveType(PrimitiveType.String, false)
      | PrimitiveValue.Date(_) ->
        return
          ExprQueryExprRec.QueryConstant c |> ExprQueryExpr.Create loc,
          TypeQueryRow.PrimitiveType(PrimitiveType.DateOnly, false)
      | PrimitiveValue.DateTime(_) ->
        return
          ExprQueryExprRec.QueryConstant c |> ExprQueryExpr.Create loc,
          TypeQueryRow.PrimitiveType(PrimitiveType.DateTime, false)
      | PrimitiveValue.TimeSpan(_) ->
        return
          ExprQueryExprRec.QueryConstant c |> ExprQueryExpr.Create loc,
          TypeQueryRow.PrimitiveType(PrimitiveType.TimeSpan, false)

    }


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
      (typeCheckQuery:
        ExprQuery<TypeExpr<'valueExt>, Identifier, 'valueExt>
          -> TypeCheckerResult<
            (ExprQuery<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt> *
            TypeValue<'valueExt> *
            Kind *
            TypeCheckContext<'valueExt>),
            'valueExt
           >)
      (depth: int)
      (loc0: Location)
      : QueryTypeChecker<ExprQueryExpr<TypeExpr<'valueExt>, Identifier, 'valueExt>, 'valueExt> =
      fun identifiers_context expr ->
        let (!) =
          ExprQueryExpr.TypeCheckQueryExpr typeCheckQuery (depth + 1) loc0 identifiers_context
        // let (=>) c e = typeCheckExpr c e

        let ofSum (p: Sum<'a, Errors<Unit>>) =
          p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

        // let error e = Errors.Singleton loc0 e

        let two_primary_keys ts es =
          match ts, es with
          | [ TypeQueryRow.PrimaryKey(TypeValue.Record { value = k1 })
              TypeQueryRow.PrimaryKey(TypeValue.Record { value = k2 }) ],
            _ when k1 |> OrderedMap.count = 1 && k2 |> OrderedMap.count = 1 ->
            let k1, k2 =
              k1 |> OrderedMap.toSeq |> Seq.head |> fst, k2 |> OrderedMap.toSeq |> Seq.head |> fst

            if k1 = k2 then Some(es) else None
          | [ TypeQueryRow.Json(TypeValue.Record { value = k1 })
              TypeQueryRow.PrimaryKey(TypeValue.Record { value = k2 }) ],
            [ e1; e2 ] when k1 |> OrderedMap.count = 1 && k2 |> OrderedMap.count = 1 ->
            let (k1, (_t1, _)), (k2, (t2, _)) =
              k1 |> OrderedMap.toSeq |> Seq.head, k2 |> OrderedMap.toSeq |> Seq.head

            match k1 = k2, t2 with
            | true, TypeValue.Primitive { value = pt } ->
              let e1 =
                ExprQueryExprRec.QueryRecordDes(e1, k1.Name.LocalName |> ResolvedIdentifier.Create, true)
                |> ExprQueryExpr.Create loc0

              let e1 =
                ExprQueryExprRec.QueryCastTo(e1, TypeQueryRow.PrimitiveType(pt, false))
                |> ExprQueryExpr.Create loc0

              Some([ e1; e2 ])
            | _ -> None
          | [ TypeQueryRow.PrimaryKey(TypeValue.Record { value = k1 })
              TypeQueryRow.Json(TypeValue.Record { value = k2 }) ],
            [ e1; e2 ] when k1 |> OrderedMap.count = 1 && k2 |> OrderedMap.count = 1 ->
            let (k1, (t1, _)), (k2, (_t2, _)) =
              k1 |> OrderedMap.toSeq |> Seq.head, k2 |> OrderedMap.toSeq |> Seq.head

            match k1 = k2, t1 with
            | true, TypeValue.Primitive { value = pt } ->
              let e2 =
                ExprQueryExprRec.QueryRecordDes(e2, k2.Name.LocalName |> ResolvedIdentifier.Create, true)
                |> ExprQueryExpr.Create loc0

              let e2 =
                ExprQueryExprRec.QueryCastTo(e2, TypeQueryRow.PrimitiveType(pt, false))
                |> ExprQueryExpr.Create loc0

              Some([ e1; e2 ])
            | _ -> None
          | _ -> None

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

          | ExprQueryExprRec.QueryTupleDes(tuple, item, _) ->
            let! tuple_e, tuple_t = !tuple

            return!
              state.Either
                (state {
                  let! tuple_t_elements = tuple_t |> TypeQueryRow.AsTuple |> ofSum

                  if item.Index - 1 < tuple_t_elements.Length then
                    let item_t = tuple_t_elements.[item.Index - 1]

                    return
                      ExprQueryExprRec.QueryTupleDes(tuple_e, item, false)
                      |> ExprQueryExpr.Create expr.Location,
                      item_t
                  else
                    return!
                      (fun () ->
                        $"Type checking error: Tuple type {tuple_t} has only {tuple_t_elements.Length} elements, but tried to access item {item.Index}")
                      |> Errors.Singleton loc0
                      |> state.Throw
                })
                (state {
                  let! json_t = tuple_t |> TypeQueryRow.AsJson |> ofSum
                  let! tuple_t_elements = json_t |> TypeValue.AsTuple |> ofSum

                  if item.Index - 1 < tuple_t_elements.Length then
                    let item_t = tuple_t_elements.[item.Index - 1]

                    return
                      ExprQueryExprRec.QueryTupleDes(tuple_e, item, true)
                      |> ExprQueryExpr.Create expr.Location,
                      item_t |> TypeQueryRow.Json
                  else
                    return!
                      (fun () ->
                        $"Type checking error: Tuple type {tuple_t} has only {tuple_t_elements.Length} elements, but tried to access item {item.Index}")
                      |> Errors.Singleton loc0
                      |> state.Throw
                })
              |> state.MapError(Errors<_>.FilterHighestPriorityOnly)
          | ExprQueryExprRec.QueryRecordDes(record, field, _) ->
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
                        ExprQueryExprRec.QueryRecordDes(record_e, field.LocalName |> ResolvedIdentifier.Create, false)
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

                        let! field_id = TypeCheckState.TryResolveIdentifier(field, loc0)

                        let! field_sym =
                          TypeCheckState.tryFindRecordFieldSymbol (field_id, loc0)
                          |> state.OfStateReader
                          |> Expr.liftTypeEval

                        let! field_t, _ =
                          record_t
                          |> OrderedMap.tryFindWithError
                            field_sym
                            "field in query record desugarization"
                            ($"Type checking error: Field {field} not found in record type {record_t}")
                          |> ofSum

                        // let field_index =
                        //   record_t
                        //   |> OrderedMap.toSeq
                        //   |> Seq.sort
                        //   |> Seq.tryFindIndex (fun (s, _) -> s = field_sym)
                        //   |> Option.defaultValue 0

                        return
                          ExprQueryExprRec.QueryRecordDes(record_e, field_id, true)
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

                        // let field_index =
                        //   record_t
                        //   |> OrderedMap.toSeq
                        //   |> Seq.sort
                        //   |> Seq.tryFindIndex (fun (s, _) -> s = field_sym)
                        //   |> Option.defaultValue 0

                        return
                          ExprQueryExprRec.QueryRecordDes(record_e, field.LocalName |> ResolvedIdentifier.Create, true)
                          |> ExprQueryExpr.Create expr.Location,
                          field_t |> TypeQueryRow.Json
                      })
                    |> state.MapError(Errors<_>.MapPriority(replaceWith ErrorPriority.High))
                })
              |> state.MapError(Errors<_>.FilterHighestPriorityOnly)
          | ExprQueryExprRec.QueryConstant c -> return! query_constant_to_type expr.Location c

          | ExprQueryExprRec.QueryApply({ Expr = ExprQueryExprRec.QueryIntrinsic(intrinsic) }, arg) ->
            return!
              state.Either3
                (state {
                  match intrinsic with
                  | QueryIntrinsic.Equals
                  | QueryIntrinsic.NotEquals ->
                    let! arg_e, arg_t = !arg
                    let! arg_t_elements = arg_t |> TypeQueryRow.AsTuple |> ofSum
                    let! arg_e_elements = arg_e |> ExprQueryExpr.AsTupleCons |> ofSum

                    match two_primary_keys arg_t_elements arg_e_elements with
                    | Some arg_e_elements ->
                      let res =
                        ExprQueryExprRec.QueryApply(
                          ExprQueryExprRec.QueryIntrinsic(QueryIntrinsic.Equals)
                          |> ExprQueryExpr.Create expr.Location,
                          arg_e_elements
                          |> ExprQueryExprRec.QueryTupleCons
                          |> ExprQueryExpr.Create expr.Location
                        )
                        |> ExprQueryExpr.Create expr.Location

                      return res, TypeQueryRow.PrimitiveType(PrimitiveType.Bool, false)
                    | None ->
                      return!
                        (fun () ->
                          $"Type checking error: Equals/not equals operator in query expressions only supports tuples of two primary keys, but got {arg_t}")
                        |> Errors.Singleton loc0
                        |> state.Throw
                  | _ -> return! (fun () -> $"Skipping branch.") |> Errors.Singleton loc0 |> state.Throw

                })
                (state {
                  let! binary_operator =
                    binary_operators
                    |> Map.tryFind intrinsic
                    |> sum.OfOption(
                      (fun () -> $"Type checking error: unknown binary operator {intrinsic}")
                      |> Errors.Singleton loc0
                    )
                    |> state.OfSum

                  let! arg_e, arg_t = !arg
                  let! arg_t_elements = arg_t |> TypeQueryRow.AsTuple |> ofSum

                  return!
                    state.Any(
                      binary_operator
                      |> NonEmptyList.map (fun expected_primitive_type ->
                        state {
                          let! arg_e, is_nullable =
                            (arg_t_elements, arg_e)
                            |> two_equal_primitives expr.Location expected_primitive_type
                            |> sum.OfOption(
                              (fun () ->
                                $"Type checking error: invalid type arguments {arg_t_elements} for {intrinsic} operator in query expression")
                              |> Errors.Singleton expr.Location
                            )
                            |> state.OfSum

                          return
                            ExprQueryExprRec.QueryApply(
                              ExprQueryExprRec.QueryIntrinsic(intrinsic) |> ExprQueryExpr.Create expr.Location,
                              arg_e
                            )
                            |> ExprQueryExpr.Create expr.Location,
                            TypeQueryRow.PrimitiveType(expected_primitive_type, is_nullable)
                        })
                    )
                })
                (state {
                  let! comparison_operator =
                    comparison_operators
                    |> Map.tryFind intrinsic
                    |> sum.OfOption(
                      (fun () -> $"Type checking error: unknown binary operator {intrinsic}")
                      |> Errors.Singleton loc0
                    )
                    |> state.OfSum

                  let! arg_e, arg_t = !arg
                  let! arg_t_elements = arg_t |> TypeQueryRow.AsTuple |> ofSum

                  return!
                    state.Any(
                      comparison_operator
                      |> NonEmptyList.map (fun expected_primitive_type ->
                        state {
                          let! arg_e, is_nullable =
                            (arg_t_elements, arg_e)
                            |> two_equal_primitives expr.Location expected_primitive_type
                            |> sum.OfOption(
                              (fun () ->
                                $"Type checking error: invalid type arguments {arg_t_elements} for {intrinsic} operator in query expression")
                              |> Errors.Singleton expr.Location
                            )
                            |> state.OfSum

                          return
                            ExprQueryExprRec.QueryApply(
                              ExprQueryExprRec.QueryIntrinsic(intrinsic) |> ExprQueryExpr.Create expr.Location,
                              arg_e
                            )
                            |> ExprQueryExpr.Create expr.Location,
                            TypeQueryRow.PrimitiveType(PrimitiveType.Bool, is_nullable)
                        })
                    )
                })

          | ExprQueryExprRec.QueryClosureValue(v, t) ->
            return ExprQueryExprRec.QueryClosureValue(v, t) |> ExprQueryExpr.Create expr.Location, t
          | ExprQueryExprRec.QueryCount q ->
            let! q_e, _, _, _ = q |> typeCheckQuery

            return
              ExprQueryExprRec.QueryCount q_e |> ExprQueryExpr.Create expr.Location,
              TypeQueryRow.PrimitiveType(PrimitiveType.Int32, false)
          | ExprQueryExprRec.QueryExists q ->
            let! q_e, _, _, _ = q |> typeCheckQuery

            return
              ExprQueryExprRec.QueryExists q_e |> ExprQueryExpr.Create expr.Location,
              TypeQueryRow.PrimitiveType(PrimitiveType.Bool, false)
          | _ ->
            return!
              (fun () ->
                $"Type checking error: Unsupported query expression {expr.Expr.AsFSharpString.ReasonablyClamped}")
              |> Errors.Singleton expr.Location
              |> state.Throw
        }


  and Expr<'T, 'Id, 've when 'Id: comparison> with
    static member internal TypeCheckQuery<'valueExt when 'valueExt: comparison>
      (query_type_symbol: TypeSymbol)
      (mk_query_type: Schema<'valueExt> -> TypeQueryRow<'valueExt> -> TypeValue<'valueExt>)
      (typeCheckExpr: ExprTypeChecker<'valueExt>)
      : TypeCheckerQuery<ExprQuery<TypeExpr<'valueExt>, Identifier, 'valueExt>, 'valueExt> =
      fun
          _context_t
          (closure_bindings: Map<LocalIdentifier, TypeQueryRow<'valueExt>>)
          initial_lookups
          (query: ExprQuery<TypeExpr<'valueExt>, Identifier, 'valueExt>) ->
        // let (!) = typeCheckExpr context_t
        let (=>) c e = typeCheckExpr c e
        let loc0 = query.Location

        let ofSum (p: Sum<'a, Errors<Unit>>) =
          p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

        // let error e = Errors.Singleton loc0 e

        let rec get_lookups_from_context
          loc0
          schema
          (ctx: TypeCheckContext<_>)
          iterator_bindings
          (q: ExprQueryExpr<_, _, _>)
          =
          state {
            let get_lookups_from_context =
              get_lookups_from_context loc0 schema ctx iterator_bindings

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

                  return Map.empty |> Map.add l (TypeQueryRow.PrimaryKey matching_entity_id)
                | _ ->
                  return!
                    state.Throw(
                      Errors.Singleton q.Location (fun () ->
                        $"Type checking error: Undefined identifier {l} or unsupported type for query closure. Only primitives and primary keys are supported.")
                    )

            | ExprQueryExprRec.QueryTupleCons items ->
              let! maps = items |> Seq.map get_lookups_from_context |> state.All
              return maps |> Seq.fold (fun acc m -> Map.merge (fun _ -> id) acc m) Map.empty
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
              return Map.merge (fun _ -> id) cond_map (Map.merge (fun _ -> id) then_map else_map)
            | ExprQueryExprRec.QueryUnionDes(_expr, _handlers) ->
              return!
                (fun () ->
                  $"Error: Query union destruction expressions are not supported in the current implementation")
                |> Errors.Singleton q.Location
                |> state.Throw
            | ExprQueryExprRec.QuerySumDes(_, _) ->
              return!
                (fun () -> $"Error: Query sum destruction expressions are not supported in the current implementation")
                |> Errors.Singleton q.Location
                |> state.Throw
            | ExprQueryExprRec.QueryApply(func, arg) ->
              let! func_map = get_lookups_from_context func
              let! arg_map = get_lookups_from_context arg
              return Map.merge (fun _ -> id) func_map arg_map
            | ExprQueryExprRec.QueryIntrinsic(_) -> return Map.empty
            | ExprQueryExprRec.QueryConstant(_) -> return Map.empty
            | ExprQueryExprRec.QueryClosureValue(_, _) -> return Map.empty
            | ExprQueryExprRec.QueryCastTo(v, _) -> return! get_lookups_from_context v
            | ExprQueryExprRec.QueryCount q
            | ExprQueryExprRec.QueryExists q
            | ExprQueryExprRec.QueryArray q ->
              let! q', _, _, _ =
                q
                |> Expr.TypeCheckQuery
                  query_type_symbol
                  mk_query_type
                  typeCheckExpr
                  _context_t
                  iterator_bindings
                  initial_lookups

              return q'.Closure
            | ExprQueryExprRec.QueryCountEvaluated _
            | ExprQueryExprRec.QueryExistsEvaluated _
            | ExprQueryExprRec.QueryArrayEvaluated _ -> return Map.empty // this cannot happen, evaluation strictly follows type checking
          }

        state {
          // do Console.WriteLine($"TypeApply: fExpr = {fExpr}")

          match query with
          | UnionQueries(q1, q2) ->
            let! q1_e, q1_t, _q1_k, _ctx =
              q1
              |> Expr.TypeCheckQuery
                query_type_symbol
                mk_query_type
                typeCheckExpr
                _context_t
                closure_bindings
                initial_lookups

            let! q2_e, q2_t, _q2_k, _ctx =
              q2
              |> Expr.TypeCheckQuery
                query_type_symbol
                mk_query_type
                typeCheckExpr
                _context_t
                closure_bindings
                initial_lookups

            match q1_t, q2_t with
            | TypeValue.Imported { Sym = sym1
                                   Arguments = [ TypeValue.Schema schema1; TypeValue.QueryRow query_row1 ] },
              TypeValue.Imported { Sym = sym2
                                   Arguments = [ TypeValue.Schema schema2; TypeValue.QueryRow query_row2 ] } when
              sym1 = query_type_symbol && sym2 = query_type_symbol
              ->
              do!
                TypeValue.Unify(loc0, TypeValue.Schema schema1, TypeValue.Schema schema2)
                |> Expr<'T, 'Id, 'valueExt>.liftUnification

              do!
                TypeValue.Unify(loc0, TypeValue.QueryRow query_row1, TypeValue.QueryRow query_row2)
                |> Expr<'T, 'Id, 'valueExt>.liftUnification

              let return_type = mk_query_type schema1 query_row1
              let! ctx = state.GetContext()

              return ExprQuery.UnionQueries(q1_e, q2_e), return_type, Kind.Star, ctx
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
              |> Map.merge (fun _ -> id) closure_bindings

            let! ctx = state.GetContext()

            let get_lookups_from_context =
              get_lookups_from_context loc0 schema ctx iterator_bindings

            let! where_lookups =
              state {
                match where_expr with
                | Some where_expr -> return! get_lookups_from_context where_expr
                | None -> return initial_lookups
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

            let typeCheckQuery =
              Expr.TypeCheckQuery
                query_type_symbol
                mk_query_type
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
                            loc0
                            queryTypeCheckingContext
                            join_expr.Left

                        let! right_e, right_t =
                          ExprQueryExpr.TypeCheckQueryExpr
                            typeCheckQuery
                            0
                            loc0
                            queryTypeCheckingContext
                            join_expr.Right

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
                    ExprQueryExpr.TypeCheckQueryExpr typeCheckQuery 0 loc0 queryTypeCheckingContext where_expr

                  match where_expr'_t with
                  | TypeQueryRow.PrimitiveType(PrimitiveType.Bool, _)
                  | TypeQueryRow.Json(TypeValue.Primitive { value = PrimitiveType.Bool }) -> return Some where_expr'
                  | _ ->
                    return!
                      (fun () ->
                        $"Type checking error: Expected boolean type for where clause, but got {where_expr'_t}")
                      |> Errors.Singleton where_expr.Location
                      |> state.Throw
              }

            let! select_expr', select_expr'_t =
              ExprQueryExpr.TypeCheckQueryExpr typeCheckQuery 0 loc0 queryTypeCheckingContext select_expr

            let! orderby_expr' =
              state {
                match orderby_expr with
                | None -> return None
                | Some(orderby_expr, direction) ->
                  let! orderby_expr', _ =
                    ExprQueryExpr.TypeCheckQueryExpr typeCheckQuery 0 loc0 queryTypeCheckingContext orderby_expr

                  return Some(orderby_expr', direction)
              }

            let! distinct_expr' =
              state {
                match distinct with
                | None -> return None
                | Some distinct_expr ->
                  let! distinct_expr', _distinct_expr_t =
                    ExprQueryExpr.TypeCheckQueryExpr typeCheckQuery 0 loc0 queryTypeCheckingContext distinct_expr

                  return Some distinct_expr'
              }

            let return_expr =
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
                OrderBy = orderby_expr'
                Distinct = distinct_expr'
                Closure = select_orderby_lookups
                DeserializeFrom = select_expr'_t }
              |> SimpleQuery

            let return_type = mk_query_type schema select_expr'_t

            let! ctx = state.GetContext()

            return return_expr, return_type, Kind.Star, ctx
        }

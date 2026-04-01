namespace Ballerina.DSL.Next.Types.TypeChecker

module QueryCaseApplyIntrinsic =
  open Ballerina.State.WithError
  open Ballerina.Collections.Map
  open Ballerina.Collections.Sum
  open Ballerina.Collections.NonEmptyList
  open Ballerina.Errors
  open Ballerina.Fun
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.QueryUtilities
  open Ballerina.DSL.Next.Types.TypeChecker.QueryOperators

  let private two_primary_keys loc0 ts es =
    match ts, es with
    | [ TypeQueryRow.PrimaryKey(TypeValue.Record { value = k1 })
        TypeQueryRow.PrimaryKey(TypeValue.Record { value = k2 }) ],
      _ when k1 |> OrderedMap.count = 1 && k2 |> OrderedMap.count = 1 ->
      let k1, k2 =
        k1 |> OrderedMap.toSeq |> Seq.head |> fst, k2 |> OrderedMap.toSeq |> Seq.head |> fst

      if k1 = k2 then Some(es) else None
    | [ TypeQueryRow.Json(TypeValue.Record { value = k1 }); TypeQueryRow.PrimaryKey(TypeValue.Record { value = k2 }) ],
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
    | [ TypeQueryRow.PrimaryKey(TypeValue.Record { value = k1 }); TypeQueryRow.Json(TypeValue.Record { value = k2 }) ],
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

  let typeCheckQueryApplyIntrinsic<'valueExt when 'valueExt: comparison>
    loc0
    (recur:
      ExprQueryExpr<TypeExpr<'valueExt>, Identifier, 'valueExt>
        -> TypeCheckerResult<
          (ExprQueryExpr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt> * TypeQueryRow<'valueExt>),
          'valueExt
         >)
    (expr: ExprQueryExpr<TypeExpr<'valueExt>, Identifier, 'valueExt>)
    intrinsic
    arg
    : TypeCheckerResult<
        (ExprQueryExpr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt> * TypeQueryRow<'valueExt>),
        'valueExt
       >
    =
    let ofSum (p: Sum<'a, Errors<Unit>>) =
      p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

    state {
      return!
        state.Either3
          (state {
            match intrinsic with
            | QueryIntrinsic.Equals
            | QueryIntrinsic.NotEquals ->
              let! arg_e, arg_t = recur arg
              let! arg_t_elements = arg_t |> TypeQueryRow.AsTuple |> ofSum
              let! arg_e_elements = arg_e |> ExprQueryExpr.AsTupleCons |> ofSum

              match two_primary_keys loc0 arg_t_elements arg_e_elements with
              | Some arg_e_elements ->
                let res =
                  ExprQueryExprRec.QueryApply(
                    ExprQueryExprRec.QueryIntrinsic(
                      QueryIntrinsic.Equals,
                      TypeQueryRow.PrimitiveType(PrimitiveType.Bool, false)
                    )
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

            let! arg_e, arg_t = recur arg
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
                        ExprQueryExprRec.QueryIntrinsic(
                          intrinsic,
                          TypeQueryRow.PrimitiveType(expected_primitive_type, is_nullable)
                        )
                        |> ExprQueryExpr.Create expr.Location,
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

            let! arg_e, arg_t = recur arg
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
                        ExprQueryExprRec.QueryIntrinsic(
                          intrinsic,
                          TypeQueryRow.PrimitiveType(PrimitiveType.Bool, is_nullable)
                        )
                        |> ExprQueryExpr.Create expr.Location,
                        arg_e
                      )
                      |> ExprQueryExpr.Create expr.Location,
                      TypeQueryRow.PrimitiveType(PrimitiveType.Bool, is_nullable)
                  })
              )
          })
    }

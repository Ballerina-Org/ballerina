namespace Ballerina.DSL.Next.Types.TypeChecker

module QueryCaseConditional =
  open Ballerina.State.WithError
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Unification
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.LiftOtherSteps

  let typeCheckQueryConditional<'valueExt when 'valueExt: comparison>
    loc0
    (recur:
      ExprQueryExpr<TypeExpr<'valueExt>, Identifier, 'valueExt>
        -> TypeCheckerResult<(TypeCheckedExprQueryExpr<'valueExt> * TypeQueryRow<'valueExt>), 'valueExt>)
    (expr: ExprQueryExpr<TypeExpr<'valueExt>, Identifier, 'valueExt>)
    cond
    thenExpr
    elseExpr
    : TypeCheckerResult<(TypeCheckedExprQueryExpr<'valueExt> * TypeQueryRow<'valueExt>), 'valueExt> =
    state {
      let! cond_e, cond_t = recur cond

      let! cond_e =
        match cond_t with
        | TypeQueryRow.PrimitiveType(PrimitiveType.Bool, _) -> state { return cond_e }
        | TypeQueryRow.Json(TypeValue.Primitive { value = PrimitiveType.Bool }) ->
          state {
            return
              TypeCheckedExprQueryExprRec.QueryCastTo(cond_e, TypeQueryRow.PrimitiveType(PrimitiveType.Bool, false))
              |> TypeCheckedExprQueryExpr.Create expr.Location
          }
        | _ ->
          (fun () -> $"Type checking error: condition of if expression in query must be bool, but got {cond_t}")
          |> Errors.Singleton loc0
          |> state.Throw

      let! then_e, then_t = recur thenExpr
      let! else_e, else_t = recur elseExpr

      let! then_e_final, else_e_final, result_t =
        match then_t, else_t with
        | TypeQueryRow.Json(TypeValue.Primitive { value = pt }), TypeQueryRow.PrimitiveType(pt2, n) when pt = pt2 ->
          state {
            let cast_t = TypeQueryRow.PrimitiveType(pt, false)

            return
              TypeCheckedExprQueryExprRec.QueryCastTo(then_e, cast_t)
              |> TypeCheckedExprQueryExpr.Create expr.Location,
              else_e,
              TypeQueryRow.PrimitiveType(pt, n)
          }
        | TypeQueryRow.PrimitiveType(pt, n), TypeQueryRow.Json(TypeValue.Primitive { value = pt2 }) when pt = pt2 ->
          state {
            let cast_t = TypeQueryRow.PrimitiveType(pt, false)

            return
              then_e,
              TypeCheckedExprQueryExprRec.QueryCastTo(else_e, cast_t)
              |> TypeCheckedExprQueryExpr.Create expr.Location,
              TypeQueryRow.PrimitiveType(pt, n)
          }
        | _ ->
          state {
            do!
              TypeQueryRow.Unify(loc0, then_t, else_t)
              |> Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>.liftUnification

            return then_e, else_e, then_t
          }

      return
        TypeCheckedExprQueryExprRec.QueryConditional(cond_e, then_e_final, else_e_final)
        |> TypeCheckedExprQueryExpr.Create expr.Location,
        result_t
    }

namespace Ballerina.DSL.Next.Types.TypeChecker

module SchemaEntityHookCanDelete =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Errors
  open Ballerina.Collections.Map
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.LiftOtherSteps
  open Ballerina.DSL.Next.Unification
  open Ballerina.DSL.Next.Terms.Patterns

  let typecheck<'ve when 've: comparison>
    (typeCheckExpr: ExprTypeChecker<'ve>)
    (loc0: Location)
    (schema: Schema<'ve>)
    (e: SchemaEntity<'ve>)
    (canDelete: Option<Expr<TypeExpr<'ve>, Identifier, 've>>)
    : State<
        Option<TypeCheckedExpr<'ve>>,
        TypeCheckContext<'ve>,
        TypeCheckState<'ve>,
        Errors<Location>
       >
    =
    state {
      match canDelete with
      | None -> return None
      | Some can_delete ->
        let! ctx = state.GetContext()
        let extra_scope = ctx.PermissionHooksExtraScope

        let ofSum (p: Sum<'a, Errors<Unit>>) =
          p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

        let! can_delete_expr, _ =
          typeCheckExpr None can_delete
          |> state.MapContext(
            TypeCheckContext.Updaters.Values(
              Map.merge (fun _ -> id) extra_scope
            )
            >> TypeCheckContext.Updaters.Scope(
              TypeCheckScope.Empty |> replaceWith
            )
          )

        let can_delete_t = can_delete_expr.Type

        let can_delete_k = can_delete_expr.Kind


        do! can_delete_k |> Kind.AsStar |> ofSum |> state.Ignore

        do!
          TypeValue.Unify(
            can_delete.Location,
            can_delete_t,
            TypeValue.CreateArrow(
              TypeValue.Schema schema,
              TypeValue.CreateArrow(
                e.Id,
                TypeValue.CreateArrow(e.TypeWithProps, TypeValue.CreateBool())
              )
            )
          )
          |> Expr.liftUnification
          |> state.MapContext(
            TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith)
          )

        return Some can_delete_expr
    }

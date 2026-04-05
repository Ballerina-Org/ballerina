namespace Ballerina.DSL.Next.Types.TypeChecker

module SchemaEntityHookCanUpdate =
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
    (canUpdate: Option<Expr<TypeExpr<'ve>, Identifier, 've>>)
    : State<Option<TypeCheckedExpr<'ve>>, TypeCheckContext<'ve>, TypeCheckState<'ve>, Errors<Location>> =
    state {
      match canUpdate with
      | None -> return None
      | Some can_update ->
        let! ctx = state.GetContext()
        let extra_scope = ctx.PermissionHooksExtraScope

        let ofSum (p: Sum<'a, Errors<Unit>>) =
          p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

        let! can_update_expr, can_update_t, can_update_k, _ =
          typeCheckExpr None can_update
          |> state.MapContext(
            TypeCheckContext.Updaters.Values(Map.merge (fun _ -> id) extra_scope)
            >> TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith)
          )

        do! can_update_k |> Kind.AsStar |> ofSum |> state.Ignore

        do!
          TypeValue.Unify(
            can_update.Location,
            can_update_t,
            TypeValue.CreateArrow(
              TypeValue.Schema schema,
              TypeValue.CreateArrow(e.Id, TypeValue.CreateArrow(e.TypeWithProps, TypeValue.CreateBool()))
            )
          )
          |> Expr.liftUnification
          |> state.MapContext(TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith))

        return Some can_update_expr
    }

namespace Ballerina.DSL.Next.Types.TypeChecker

module SchemaEntityHookCanRead =
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
    (mkQueryType: Schema<'ve> -> TypeQueryRow<'ve> -> TypeValue<'ve>)
    (loc0: Location)
    (schema: Schema<'ve>)
    (e: SchemaEntity<'ve>)
    (canRead: Option<Expr<TypeExpr<'ve>, Identifier, 've>>)
    : State<Option<TypeCheckedExpr<'ve>>, TypeCheckContext<'ve>, TypeCheckState<'ve>, Errors<Location>> =
    state {
      match canRead with
      | None -> return None
      | Some can_read ->
        let! ctx = state.GetContext()
        let extra_scope = ctx.PermissionHooksExtraScope

        let ofSum (p: Sum<'a, Errors<Unit>>) =
          p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

        let! can_read_expr, can_read_t, can_read_k, _ =
          typeCheckExpr None can_read
          |> state.MapContext(
            TypeCheckContext.Updaters.Values(Map.merge (fun _ -> id) extra_scope)
            >> TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith)
          )

        do! can_read_k |> Kind.AsStar |> ofSum |> state.Ignore

        do!
          TypeValue.Unify(
            can_read.Location,
            can_read_t,
            TypeValue.CreateArrow(TypeValue.Schema schema, mkQueryType schema (TypeQueryRow.PrimaryKey e.Id))
          )
          |> Expr.liftUnification
          |> state.MapContext(TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith))

        return Some can_read_expr
    }

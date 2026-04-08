namespace Ballerina.DSL.Next.Types.TypeChecker

module SchemaEntityHookOnBackground =
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
    (onBackground: Option<Expr<TypeExpr<'ve>, Identifier, 've>>)
    : State<Option<TypeCheckedExpr<'ve>>, TypeCheckContext<'ve>, TypeCheckState<'ve>, Errors<Location>> =
    state {
      match onBackground with
      | None -> return None
      | Some on_background ->
        let! ctx = state.GetContext()
        let extra_scope = ctx.BackgroundHooksExtraScope

        let ofSum (p: Sum<'a, Errors<Unit>>) =
          p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

        let! on_background_expr, _ =
          typeCheckExpr None on_background
          |> state.MapContext(
            TypeCheckContext.Updaters.Values(Map.merge (fun _ -> id) extra_scope)
            >> TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith)
          )

        let on_background_t = on_background_expr.Type

        let on_background_k = on_background_expr.Kind


        do! on_background_k |> Kind.AsStar |> ofSum |> state.Ignore

        do!
          TypeValue.Unify(
            on_background.Location,
            on_background_t,
            TypeValue.CreateArrow(
              TypeValue.Schema schema,
              TypeValue.CreateArrow(
                e.Id,
                TypeValue.CreateArrow(
                  e.TypeWithProps,
                  TypeValue.CreateSum [ TypeValue.CreateUnit(); TypeValue.CreateTimeSpan() ]
                )
              )
            )
          )
          |> Expr.liftUnification
          |> state.MapContext(TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith))

        return Some on_background_expr
    }

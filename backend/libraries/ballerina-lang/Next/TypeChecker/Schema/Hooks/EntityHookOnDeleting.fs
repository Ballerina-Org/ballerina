namespace Ballerina.DSL.Next.Types.TypeChecker

module SchemaEntityHookOnDeleting =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Errors
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
    (onDeleting: Option<Expr<TypeExpr<'ve>, Identifier, 've>>)
    =
    state {
      match onDeleting with
      | None -> return None
      | Some on_deleting ->
        let error_type = TypeValue.Lookup(Identifier.FullyQualified([], "Error"))

        let ofSum (p: Sum<'a, Errors<Unit>>) =
          p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

        let! on_deleting_expr, on_deleting_t, on_deleting_k, _ =
          typeCheckExpr None on_deleting
          |> state.MapContext(TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith))

        do! on_deleting_k |> Kind.AsStar |> ofSum |> state.Ignore

        do!
          TypeValue.Unify(
            on_deleting.Location,
            on_deleting_t,
            TypeValue.CreateArrow(
              TypeValue.Schema schema,
              TypeValue.CreateArrow(
                e.Id,
                TypeValue.CreateArrow(e.TypeWithProps, TypeValue.CreateSum [ TypeValue.CreateUnit(); error_type ])
              )
            )
          )
          |> Expr.liftUnification
          |> state.MapContext(TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith))

        return Some on_deleting_expr
    }

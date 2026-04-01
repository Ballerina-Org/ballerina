namespace Ballerina.DSL.Next.Types.TypeChecker

module SchemaEntityHookOnUpdating =
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
    (onUpdating: Option<Expr<TypeExpr<'ve>, Identifier, 've>>)
    =
    state {
      match onUpdating with
      | None -> return None
      | Some on_updating ->
        let error_type = TypeValue.Lookup(Identifier.FullyQualified([], "Error"))

        let ofSum (p: Sum<'a, Errors<Unit>>) =
          p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

        let! on_updating_expr, on_updating_t, on_updating_k, _ =
          typeCheckExpr None on_updating
          |> state.MapContext(TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith))

        do! on_updating_k |> Kind.AsStar |> ofSum |> state.Ignore

        do!
          TypeValue.Unify(
            on_updating.Location,
            on_updating_t,
            TypeValue.CreateArrow(
              TypeValue.Schema schema,
              TypeValue.CreateArrow(
                e.Id,
                TypeValue.CreateArrow(
                  e.TypeWithProps,
                  TypeValue.CreateArrow(
                    e.TypeOriginal,
                    TypeValue.CreateArrow(
                      e.TypeWithProps,
                      TypeValue.CreateSum [ TypeValue.CreateUnit(); error_type; e.TypeOriginal ]
                    )
                  )
                )
              )
            )
          )
          |> Expr.liftUnification
          |> state.MapContext(TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith))

        return Some on_updating_expr
    }

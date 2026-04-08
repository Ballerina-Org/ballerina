namespace Ballerina.DSL.Next.Types.TypeChecker

module SchemaEntityHookOnCreating =
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
    (onCreating: Option<Expr<TypeExpr<'ve>, Identifier, 've>>)
    =
    state {
      match onCreating with
      | None -> return None
      | Some on_creating ->
        let error_type =
          TypeValue.Lookup(Identifier.FullyQualified([], "Error"))

        let ofSum (p: Sum<'a, Errors<Unit>>) =
          p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

        let! on_creating_expr, _ =
          typeCheckExpr None on_creating
          |> state.MapContext(
            TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith)
          )

        let on_creating_t = on_creating_expr.Type

        let on_creating_k = on_creating_expr.Kind


        do! on_creating_k |> Kind.AsStar |> ofSum |> state.Ignore

        do!
          TypeValue.Unify(
            on_creating.Location,
            on_creating_t,
            TypeValue.CreateArrow(
              TypeValue.Schema schema,
              TypeValue.CreateArrow(
                e.Id,
                TypeValue.CreateArrow(
                  e.TypeOriginal,
                  TypeValue.CreateArrow(
                    e.TypeWithProps,
                    TypeValue.CreateSum
                      [ TypeValue.CreateUnit(); error_type; e.TypeOriginal ]
                  )
                )
              )
            )
          )
          |> Expr.liftUnification
          |> state.MapContext(
            TypeCheckContext.Updaters.Scope(TypeCheckScope.Empty |> replaceWith)
          )

        return Some on_creating_expr
    }

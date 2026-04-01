namespace Ballerina.DSL.Next.Types.TypeChecker

module QueryCaseTupleDes =
  open Ballerina
  open Ballerina.State.WithError
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Model

  let typeCheckQueryTupleDes<'valueExt when 'valueExt: comparison>
    loc0
    (recur:
      ExprQueryExpr<TypeExpr<'valueExt>, Identifier, 'valueExt>
        -> TypeCheckerResult<
          (ExprQueryExpr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt> * TypeQueryRow<'valueExt>),
          'valueExt
         >)
    (expr: ExprQueryExpr<TypeExpr<'valueExt>, Identifier, 'valueExt>)
    tuple
    item
    : TypeCheckerResult<
        (ExprQueryExpr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt> * TypeQueryRow<'valueExt>),
        'valueExt
       >
    =
    let ofSum (p: Sum<'a, Errors<Unit>>) =
      p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

    state {
      let! tuple_e, tuple_t = recur tuple

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
    }

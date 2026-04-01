namespace Ballerina.DSL.Next.Types.TypeChecker

module QueryCaseArray =
  open Ballerina.State.WithError
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Model

  let typeCheckQueryArray<'valueExt when 'valueExt: comparison>
    (typeCheckQuery:
      ExprQuery<TypeExpr<'valueExt>, Identifier, 'valueExt>
        -> TypeCheckerResult<
          (ExprQuery<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt> *
          TypeValue<'valueExt> *
          Kind *
          TypeCheckContext<'valueExt>),
          'valueExt
         >)
    (expr: ExprQueryExpr<TypeExpr<'valueExt>, Identifier, 'valueExt>)
    q
    : TypeCheckerResult<
        (ExprQueryExpr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt> * TypeQueryRow<'valueExt>),
        'valueExt
       >
    =
    state {
      let! q_e, query_type, _, _ = q |> typeCheckQuery

      match query_type with
      | TypeValue.Imported { Arguments = [ _; TypeValue.QueryRow row ] } ->
        return ExprQueryExprRec.QueryArray q_e |> ExprQueryExpr.Create expr.Location, TypeQueryRow.Array row
      | _ ->
        return!
          (fun () -> $"Type checking error: QueryArray expects Query type, got {query_type}")
          |> Errors.Singleton expr.Location
          |> state.Throw
    }

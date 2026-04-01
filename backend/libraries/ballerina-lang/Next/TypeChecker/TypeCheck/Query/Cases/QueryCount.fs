namespace Ballerina.DSL.Next.Types.TypeChecker

module QueryCaseCount =
  open Ballerina.State.WithError
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Model

  let typeCheckQueryCount<'valueExt when 'valueExt: comparison>
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
      let! q_e, _, _, _ = q |> typeCheckQuery

      return
        ExprQueryExprRec.QueryCount q_e |> ExprQueryExpr.Create expr.Location,
        TypeQueryRow.PrimitiveType(PrimitiveType.Int32, false)
    }

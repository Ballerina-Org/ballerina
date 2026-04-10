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
          (TypeCheckedExprQuery<'valueExt> *
          TypeValue<'valueExt> *
          Kind *
          TypeCheckContext<'valueExt>),
          'valueExt
         >)
    (expr: ExprQueryExpr<TypeExpr<'valueExt>, Identifier, 'valueExt>)
    q
    : TypeCheckerResult<
        (TypeCheckedExprQueryExpr<'valueExt> * TypeQueryRow<'valueExt>),
        'valueExt
       >
    =
    state {
      let! q_e, _, _, _ = q |> typeCheckQuery

      return
        TypeCheckedExprQueryExprRec.QueryCount q_e
        |> TypeCheckedExprQueryExpr.Create expr.Location,
        TypeQueryRow.PrimitiveType(PrimitiveType.Int32, false)
    }

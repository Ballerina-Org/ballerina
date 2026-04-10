namespace Ballerina.DSL.Next.Types.TypeChecker

module QueryCaseConstant =
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Types.TypeChecker.QueryUtilities
  open Ballerina.DSL.Next.Types.TypeChecker.Model

  let typeCheckQueryConstant<'valueExt when 'valueExt: comparison>
    (expr: ExprQueryExpr<TypeExpr<'valueExt>, Identifier, 'valueExt>)
    c
    : TypeCheckerResult<
        (TypeCheckedExprQueryExpr<'valueExt> * TypeQueryRow<'valueExt>),
        'valueExt
       >
    =
    query_constant_to_type expr.Location c

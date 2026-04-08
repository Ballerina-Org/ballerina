namespace Ballerina.DSL.Next.Types.TypeChecker

module QueryCaseClosureValue =
  open Ballerina.State.WithError
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Model

  let typeCheckQueryClosureValue<'valueExt when 'valueExt: comparison>
    (expr: ExprQueryExpr<TypeExpr<'valueExt>, Identifier, 'valueExt>)
    v
    t
    : TypeCheckerResult<
        (TypeCheckedExprQueryExpr<'valueExt> * TypeQueryRow<'valueExt>),
        'valueExt
       >
    =
    state {
      return
        TypeCheckedExprQueryExprRec.QueryClosureValue(v, t)
        |> TypeCheckedExprQueryExpr.Create expr.Location,
        t
    }

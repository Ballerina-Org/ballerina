namespace Ballerina.DSL.Next.Types.TypeChecker

module QueryCaseTupleCons =
  open Ballerina.State.WithError
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Model

  let typeCheckQueryTupleCons<'valueExt when 'valueExt: comparison>
    (recur:
      ExprQueryExpr<TypeExpr<'valueExt>, Identifier, 'valueExt>
        -> TypeCheckerResult<
          (TypeCheckedExprQueryExpr<'valueExt> * TypeQueryRow<'valueExt>),
          'valueExt
         >)
    (expr: ExprQueryExpr<TypeExpr<'valueExt>, Identifier, 'valueExt>)
    (args: List<ExprQueryExpr<TypeExpr<'valueExt>, Identifier, 'valueExt>>)
    =
    state {
      let! args_e_t = args |> Seq.map recur |> state.All
      let args_t = args_e_t |> Seq.map snd |> Seq.toList

      return
        TypeCheckedExprQueryExprRec.QueryTupleCons(
          args_e_t |> Seq.map fst |> Seq.toList
        )
        |> TypeCheckedExprQueryExpr.Create expr.Location,
        TypeQueryRow.Tuple args_t
    }

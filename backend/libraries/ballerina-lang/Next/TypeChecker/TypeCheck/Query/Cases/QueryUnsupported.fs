namespace Ballerina.DSL.Next.Types.TypeChecker

module QueryCaseUnsupported =
  open Ballerina.State.WithError
  open Ballerina.Errors
  open Ballerina.StdLib.Object
  open Ballerina.StdLib.String
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Model

  let typeCheckQueryUnsupported<'valueExt when 'valueExt: comparison>
    (expr: ExprQueryExpr<TypeExpr<'valueExt>, Identifier, 'valueExt>)
    : TypeCheckerResult<
        (ExprQueryExpr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt> * TypeQueryRow<'valueExt>),
        'valueExt
       >
    =
    state {
      return!
        (fun () -> $"Type checking error: Unsupported query expression {expr.Expr.AsFSharpString.ReasonablyClamped}")
        |> Errors.Singleton expr.Location
        |> state.Throw
    }

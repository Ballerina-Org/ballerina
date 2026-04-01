namespace Ballerina.DSL.Next.Types.TypeChecker

module QueryCaseLookupFullyQualified =
  open Ballerina
  open Ballerina.Collections.Map
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Model

  let typeCheckQueryLookupFullyQualified<'valueExt when 'valueExt: comparison>
    loc0
    (closure: Map<ResolvedIdentifier, TypeQueryRow<'valueExt>>)
    (expr: ExprQueryExpr<TypeExpr<'valueExt>, Identifier, 'valueExt>)
    (l: Identifier)
    : TypeCheckerResult<
        (ExprQueryExpr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt> * TypeQueryRow<'valueExt>),
        'valueExt
       >
    =
    state {
      let l = l |> ResolvedIdentifier.FromIdentifier

      let! t =
        closure
        |> Map.tryFindWithError
          l
          "query closure variable"
          (fun () -> $"Type checking error: Undefined closure variable {l}")
          ()
        |> Sum.mapRight (Errors.MapContext(replaceWith loc0))
        |> state.OfSum

      return ExprQueryExprRec.QueryLookup l |> ExprQueryExpr.Create expr.Location, t
    }

namespace Ballerina.DSL.Next.Types.TypeChecker

module QueryCaseLookupLocalScope =
  open Ballerina
  open Ballerina.Collections.Map
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Model

  let typeCheckQueryLookupLocalScope<'valueExt when 'valueExt: comparison>
    loc0
    (iterators: Map<LocalIdentifier, TypeQueryRow<'valueExt>>)
    (closure: Map<ResolvedIdentifier, TypeQueryRow<'valueExt>>)
    (expr: ExprQueryExpr<TypeExpr<'valueExt>, Identifier, 'valueExt>)
    v
    l
    : TypeCheckerResult<
        (ExprQueryExpr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt> * TypeQueryRow<'valueExt>),
        'valueExt
       >
    =
    let ofSum (p: Sum<'a, Errors<Unit>>) =
      p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

    state.Either
      (state {
        let! t =
          iterators
          |> Map.tryFindWithError
            (v |> LocalIdentifier.Create)
            "query iteration variable"
            (fun () -> $"Type checking error: Undefined iterator variable {v}")
            ()
          |> ofSum

        return
          ExprQueryExprRec.QueryLookup(v |> ResolvedIdentifier.Create)
          |> ExprQueryExpr.Create expr.Location,
          t
      })
      (state {
        let l = l |> ResolvedIdentifier.FromIdentifier

        let! t =
          closure
          |> Map.tryFindWithError
            l
            "query closure variable"
            (fun () -> $"Type checking error: Undefined closure variable {l}")
            ()
          |> ofSum

        return ExprQueryExprRec.QueryLookup l |> ExprQueryExpr.Create expr.Location, t
      })

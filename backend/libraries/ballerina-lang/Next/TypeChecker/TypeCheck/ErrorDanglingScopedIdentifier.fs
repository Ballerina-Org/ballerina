namespace Ballerina.DSL.Next.Types.TypeChecker

module ErrorDanglingScopedIdentifier =
  open Ballerina
  open Ballerina.State.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open System
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Patterns

  type Expr<'T, 'Id, 've when 'Id: comparison> with
    static member internal TypeCheckErrorDanglingScopedIdentifier<'valueExt
      when 'valueExt: comparison>
      (prefixParts: List<string>)
      (loc0: Location)
      : TypeCheckerResult<
          TypeCheckedExpr<'valueExt> * TypeCheckContext<'valueExt>,
          'valueExt
         >
      =
        state {
          let! ctx = state.GetContext()
          let! st = state.GetState()

          let prefix =
            match prefixParts with
            | [ p ] -> p
            | ps -> String.Join("::", ps)

          let availableSymbols =
            st.ScopePrefixHints
            |> Map.tryFind prefix
            |> Option.defaultValue Map.empty

          if not (Map.isEmpty availableSymbols) then
            do!
              TypeCheckState.bindScopeAccessHint(
                loc0,
                prefix,
                availableSymbols
              )

          return
            { TypeCheckedExpr.Expr =
                TypeCheckedExprRec.ErrorDanglingScopedIdentifier(
                  { TypeCheckedExprErrorDanglingScopedIdentifier.PrefixParts = prefixParts }
                )
              Location = loc0
              Type = TypeValue.CreateUnit()
              Kind = Kind.Star
              Scope = ctx.Scope },
            ctx
        }

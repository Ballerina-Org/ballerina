namespace Ballerina.DSL.Next.Types.TypeChecker

module Lookup =
  open Ballerina.StdLib.String
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Collections.Option
  open Ballerina.LocalizedErrors
  open System
  open Ballerina.StdLib.Object
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Unification
  open Ballerina.DSL.Next.Types.TypeChecker.AdHocPolymorphicOperators
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Eval
  open Ballerina.DSL.Next.Types.TypeChecker.LiftOtherSteps
  open Ballerina.Fun
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Collections.NonEmptyList

  type Expr<'T, 'Id when 'Id: comparison> with
    static member internal TypeCheckLookup
      (_typeCheckExpr: TypeChecker, loc0: Location)
      : TypeChecker<ExprLookup<TypeExpr, Identifier>> =
      fun _context_t ({ Id = id }) ->
        state {
          let! ctx = state.GetContext()

          let id = ctx.Types.Scope.Resolve id

          let error e = Errors.Singleton(loc0, e)

          let! t_id, id_k =
            state.Either3
              (TypeCheckContext.TryFindVar(id, loc0))
              (TypeCheckState.TryFindType(id, loc0))
              ($"Error: cannot resolve identifier '{id}'."
               |> error
               |> Errors.SetPriority(ErrorPriority.High)
               |> state.Throw)
            |> state.MapError(Errors.FilterHighestPriorityOnly)

          return Expr.Lookup(id, loc0, ctx.Types.Scope), t_id, id_k
        }

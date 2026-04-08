namespace Ballerina.DSL.Next.Types.TypeChecker

module Lookup =
  open Ballerina.StdLib.String
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Collections.Option
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
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
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList

  type Expr<'T, 'Id, 've when 'Id: comparison> with
    static member internal TypeCheckLookup<'valueExt when 'valueExt: comparison>
      (_typeCheckExpr: ExprTypeChecker<'valueExt>, loc0: Location)
      : TypeChecker<ExprLookup<TypeExpr<'valueExt>, Identifier, 'valueExt>, 'valueExt> =
      fun _context_t ({ Id = id }) ->
        state {
          let! ctx = state.GetContext()

          let id_original = TypeCheckScope.Empty.Resolve id
          let id_resolved = ctx.Scope.Resolve id

          let error e = Errors.Singleton loc0 e

          // do Console.WriteLine($"TypeCheckLookup: resolving identifier '{id}'")
          // do Console.WriteLine($"Current Scope: {ctx.Scope}")
          // do Console.ReadLine() |> ignore

          return!
            state.Either3
              (state {
                let! t_id, id_k =
                  state.Either
                    (TypeCheckContext.TryFindVar(id_resolved, loc0))
                    (TypeCheckState.TryFindType(id_resolved, loc0))

                return TypeCheckedExpr.Lookup(id_resolved, t_id, id_k, loc0, ctx.Scope), ctx
              })
              (state {
                let! t_id, id_k =
                  state.Either
                    (TypeCheckContext.TryFindVar(id_original, loc0))
                    (TypeCheckState.TryFindType(id_original, loc0))

                return TypeCheckedExpr.Lookup(id_original, t_id, id_k, loc0, ctx.Scope), ctx

              })
              (fun () -> $"Error: cannot resolve identifier '{id_resolved}'/'{id_original}'."
               |> error
               |> Errors<_>.MapPriority(replaceWith ErrorPriority.High)
               |> state.Throw)
            |> state.MapError(Errors<_>.FilterHighestPriorityOnly)

        }

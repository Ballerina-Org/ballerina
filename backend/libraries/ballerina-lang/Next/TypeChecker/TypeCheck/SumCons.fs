namespace Ballerina.DSL.Next.Types.TypeChecker

module SumCons =
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
  open Ballerina.DSL.Next.Types.TypeChecker.Primitive
  open Ballerina.DSL.Next.Types.TypeChecker.Lookup
  open Ballerina.DSL.Next.Types.TypeChecker.Lambda
  open Ballerina.DSL.Next.Types.TypeChecker.Apply
  open Ballerina.DSL.Next.Types.TypeChecker.If
  open Ballerina.DSL.Next.Types.TypeChecker.Let
  open Ballerina.DSL.Next.Types.TypeChecker.TypeLambda
  open Ballerina.Fun
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList

  type Expr<'T, 'Id, 've when 'Id: comparison> with
    static member internal TypeCheckSumCons<'valueExt when 'valueExt: comparison>
      (typeCheckExpr: ExprTypeChecker<'valueExt>, loc0: Location)
      : TypeChecker<ExprSumCons<TypeExpr<'valueExt>, Identifier, 'valueExt>, 'valueExt> =
      fun _context_t ({ Selector = cons }) ->
        state {
          let! ctx = state.GetContext()

          let cases =
            [| 0 .. cons.Count - 1 |]
            |> Array.map (fun i ->
              let guid = Guid.CreateVersion7()

              ({ TypeVar.Name = $"a_{i}_of_{cons.Count} " + guid.ToString()
                 Synthetic = true
                 Guid = guid }))

          for c in cases do
            do! state.SetState(TypeCheckState.Updaters.Vars(UnificationState.EnsureVariableExists c))

          let cases = cases |> Array.map TypeValue.Var

          let! return_t =
            TypeValue.CreateArrow(cases[cons.Case - 1], TypeValue.CreateSum(cases |> List.ofSeq))
            |> TypeValue.Instantiate () (TypeExpr.Eval () typeCheckExpr) loc0
            |> Expr.liftInstantiation

          return Expr.SumCons(cons, loc0, ctx.Scope), return_t, Kind.Star, ctx
        }

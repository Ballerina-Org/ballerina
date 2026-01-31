namespace Ballerina.DSL.Next.Types.TypeChecker

module If =
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
  open Ballerina.DSL.Next.Types.TypeChecker.Primitive
  open Ballerina.DSL.Next.Types.TypeChecker.Lookup
  open Ballerina.DSL.Next.Types.TypeChecker.Lambda
  open Ballerina.DSL.Next.Types.TypeChecker.Apply
  open Ballerina.Fun
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList

  type Expr<'T, 'Id, 'v when 'Id: comparison> with
    static member internal TypeCheckIf<'valueExt when 'valueExt: comparison>
      (typeCheckExpr: ExprTypeChecker<'valueExt>, loc0: Location)
      : TypeChecker<ExprIf<TypeExpr<'valueExt>, Identifier, 'valueExt>, 'valueExt> =
      fun
          context_t
          ({ Cond = cond
             Then = thenBranch
             Else = elseBranch }) ->
        let (!) = typeCheckExpr context_t

        let ofSum (p: Sum<'a, Errors<Unit>>) =
          p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

        state {
          let! ctx = state.GetContext()
          let! cond, t_cond, cond_k, _ = !cond
          do! cond_k |> Kind.AsStar |> ofSum |> state.Ignore

          do!
            TypeValue.Unify(loc0, t_cond, TypeValue.CreatePrimitive PrimitiveType.Bool)
            |> Expr<'T, 'Id, 'valueExt>.liftUnification

          let! thenBranch, t_then, then_k, _ = !thenBranch
          let! elseBranch, t_else, else_k, _ = !elseBranch
          do! then_k |> Kind.AsStar |> ofSum |> state.Ignore
          do! else_k |> Kind.AsStar |> ofSum |> state.Ignore

          do!
            TypeValue.Unify(loc0, t_then, t_else)
            |> Expr<'T, 'Id, 'valueExt>.liftUnification

          let! t_then =
            t_then
            |> TypeValue.Instantiate () (TypeExpr.Eval () typeCheckExpr) loc0
            |> Expr<'T, 'Id, 'valueExt>.liftInstantiation

          return Expr.If(cond, thenBranch, elseBranch, loc0, ctx.Scope), t_then, Kind.Star, ctx
        }

namespace Ballerina.DSL.Next.Types.TypeChecker

module Let =
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
  open Ballerina.Fun
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Collections.NonEmptyList

  type Expr<'T, 'Id when 'Id: comparison> with
    static member internal TypeCheckLet
      (typeCheckExpr: TypeChecker, loc0: Location)
      : TypeChecker<ExprLet<TypeExpr, Identifier>> =
      fun
          context_t
          ({ Var = x
             Type = var_type
             Val = e1
             Rest = e2 }) ->
        let (!) = typeCheckExpr context_t
        let (=>) c e = typeCheckExpr c e

        let ofSum (p: Sum<'a, Ballerina.Errors.Errors>) =
          p |> Sum.mapRight (Errors.FromErrors loc0) |> state.OfSum

        state {
          let! ctx = state.GetContext()

          let! x_type =
            var_type
            |> Option.map (TypeExpr.Eval None loc0 >> Expr<'T, 'Id>.liftTypeEval)
            |> state.RunOption

          let! e1, t1, k1 = (x_type |> Option.map fst) => e1

          match x_type with
          | Some(x_type, x_type_kind) ->
            do! x_type_kind |> Kind.AsStar |> ofSum |> state.Ignore
            do! TypeValue.Unify(loc0, t1, x_type) |> Expr<'T, 'Id>.liftUnification
          | _ -> ()

          let! e2, t2, k2 =
            !e2
            |> state.MapContext(
              TypeCheckContext.Updaters.Values(
                Map.add (x.Name |> Identifier.LocalScope |> ctx.Types.Scope.Resolve) (t1, k1)
              )
            )

          return Expr.Let(x, None, e1, e2, loc0, ctx.Types.Scope), t2, k2
        }

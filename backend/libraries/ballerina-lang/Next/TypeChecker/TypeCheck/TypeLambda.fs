namespace Ballerina.DSL.Next.Types.TypeChecker

module TypeLambda =
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

  type Expr<'T, 'Id, 've when 'Id: comparison> with
    static member internal TypeCheckTypeLambda<'valueExt when 'valueExt: comparison>
      (typeCheckExpr: ExprTypeChecker<'valueExt>)
      : TypeChecker<ExprTypeLambda<TypeExpr<'valueExt>, Identifier, 'valueExt>, 'valueExt> =
      fun context_t ({ Param = t_par; Body = body }) ->
        let (!) = typeCheckExpr context_t
        let loc0 = body.Location

        state {
          let! ctx = state.GetContext()

          // let fresh_t_par_var =
          //   let id = Guid.CreateVersion7()

          //   { TypeVar.Name = t_par.Name; Guid = id }

          // do! state.SetState(TypeCheckState.Updaters.Vars(UnificationState.EnsureVariableExists fresh_t_par_var))

          // let! scope = state.GetContext() |> state.Map(fun ctx -> ctx.Scope)

          // let! t_par_type =
          //   TypeExprEvalState.tryFindType (t_par.Name |> Identifier.LocalScope |> scope.Resolve, loc0)
          //   |> state.OfStateReader
          //   |> Expr.liftTypeEval
          //   |> state.Catch

          // // push binding
          // do!
          //   TypeExprEvalState.bindType
          //     (t_par.Name |> Identifier.LocalScope |> scope.Resolve)
          //     (TypeValue.Var fresh_t_par_var, t_par.Kind)
          //   |> Expr.liftTypeEval

          let! body, _ =
            !body
            |> state.MapContext(TypeCheckContext.Updaters.TypeParameters(Map.add t_par.Name t_par.Kind))

          let t_body = body.Type
          let body_k = body.Kind

          // // pop binding
          // match t_par_type with
          // | Left t_par_type ->
          //   do!
          //     TypeExprEvalState.bindType (t_par.Name |> Identifier.LocalScope |> scope.Resolve) t_par_type
          //     |> Expr.liftTypeEval
          // | Right _ ->
          //   do!
          //     TypeExprEvalState.unbindType (t_par.Name |> Identifier.LocalScope |> scope.Resolve)
          //     |> Expr.liftTypeEval

          // cleanup unification state, slightly more radical than pop
          // do!
          //   UnificationState.TryDeleteFreeVariable(fresh_t_par_var, loc0)
          //   |> TypeValue.EquivalenceClassesOp loc0
          //   |> Expr.liftUnification

          // do Console.WriteLine($"TypeLambda body type: {t_body}")

          let t_res = TypeValue.CreateLambda(t_par, t_body.AsExpr)
          let k_res = Kind.Arrow(t_par.Kind, body_k)

          return TypeCheckedExpr.TypeLambda(t_par, body, t_res, k_res, loc0, ctx.Scope), ctx
        }

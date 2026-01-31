namespace Ballerina.DSL.Next.Types.TypeChecker

module Lambda =
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
  open Ballerina.Fun
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList

  type Expr<'T, 'Id, 've when 'Id: comparison> with
    static member internal TypeCheckLambda<'valueExt when 'valueExt: comparison>
      (typeCheckExpr: ExprTypeChecker<'valueExt>, loc0: Location)
      : TypeChecker<ExprLambda<TypeExpr<'valueExt>, Identifier, 'valueExt>, 'valueExt> =
      fun
          context_t
          ({ Param = x
             ParamType = t
             Body = body }) ->
        let (!) = typeCheckExpr context_t

        let ofSum (p: Sum<'a, Errors<Unit>>) =
          p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

        state {
          let! ctx = state.GetContext()

          let! t =
            t
            |> Option.map (fun t ->
              t
              |> TypeExpr.Eval () typeCheckExpr None loc0
              |> Expr<'T, 'Id, 'valueExt>.liftTypeEval)
            |> state.RunOption

          // (p: State<'a, UnificationContext, UnificationState, Errors>)
          // : State<'a, TypeCheckContext, TypeCheckState, Errors> =

          let guid = Guid.CreateVersion7()

          let freshVar =
            { TypeVar.Name = x.Name + "_lambda_" + guid.ToString()
              Synthetic = true
              Guid = guid }

          let freshVarType =
            Option.defaultWith (fun () -> freshVar |> TypeValue.Var, Kind.Star) t

          do! state.SetState(TypeCheckState.Updaters.Vars(UnificationState.EnsureVariableExists freshVar))

          let! body, t_body, body_k, _ =
            !body
            |> state.MapContext(
              TypeCheckContext.Updaters.Values(
                Map.add (x.Name |> Identifier.LocalScope |> ctx.Scope.Resolve) freshVarType
              )
            )

          do! body_k |> Kind.AsStar |> ofSum |> state.Ignore

          let! t_x =
            freshVarType
            |> fst
            |> TypeValue.Instantiate () (TypeExpr.Eval () typeCheckExpr) loc0
            |> Expr<'T, 'Id, 'valueExt>.liftInstantiation
          // let! t_body = t_body |> TypeValue.Instantiate () (TypeExpr.Eval ()) loc0 |> Expr<'T, 'Id, 'valueExt>.liftInstantiation

          // do!
          //     UnificationState.DeleteVariable freshVar
          //       |> TypeValue.EquivalenceClassesOp
          //       |> Expr<'T, 'Id, 'valueExt>.liftUnification

          let! t_res =
            TypeValue.CreateArrow(t_x, t_body)
            |> TypeValue.Instantiate () (TypeExpr.Eval () typeCheckExpr) loc0
            |> Expr.liftInstantiation

          return Expr.Lambda(x, Some t_x, body, loc0, ctx.Scope), t_res, Kind.Star, ctx
        }
// |> state.MapError(Errors.Map(String.appendNewline $"...when typechecking `fun {x.Name} -> ...`"))

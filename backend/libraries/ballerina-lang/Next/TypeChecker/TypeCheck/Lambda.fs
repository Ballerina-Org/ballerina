namespace Ballerina.DSL.Next.Types.TypeChecker

module Lambda =
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
  open Ballerina.Fun
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList

  type Expr<'T, 'Id when 'Id: comparison> with
    static member internal TypeCheckLambda
      (typeCheckExpr: TypeChecker, loc0: Location)
      : TypeChecker<ExprLambda<TypeExpr, Identifier>> =
      fun
          context_t
          ({ Param = x
             ParamType = t
             Body = body }) ->
        let (!) = typeCheckExpr context_t

        let ofSum (p: Sum<'a, Ballerina.Errors.Errors>) =
          p |> Sum.mapRight (Errors.FromErrors loc0) |> state.OfSum

        state {
          let! ctx = state.GetContext()

          let! t =
            t
            |> Option.map (fun t -> t |> TypeExpr.Eval None loc0 |> Expr<'T, 'Id>.liftTypeEval)
            |> state.RunOption

          // (p: State<'a, UnificationContext, UnificationState, Errors>)
          // : State<'a, TypeCheckContext, TypeCheckState, Errors> =

          let guid = Guid.CreateVersion7()

          let freshVar =
            { TypeVar.Name = x.Name + "_lambda_" + guid.ToString()
              Guid = guid }

          let freshVarType =
            Option.defaultWith (fun () -> freshVar |> TypeValue.Var, Kind.Star) t

          do! state.SetState(TypeCheckState.Updaters.Vars(UnificationState.EnsureVariableExists freshVar))

          let! body, t_body, body_k =
            !body
            |> state.MapContext(
              TypeCheckContext.Updaters.Values(
                Map.add (x.Name |> Identifier.LocalScope |> ctx.Types.Scope.Resolve) freshVarType
              )
            )

          do! body_k |> Kind.AsStar |> ofSum |> state.Ignore

          let! t_x =
            freshVarType
            |> fst
            |> TypeValue.Instantiate loc0
            |> Expr<'T, 'Id>.liftInstantiation
          // let! t_body = t_body |> TypeValue.Instantiate loc0 |> Expr<'T, 'Id>.liftInstantiation

          // do!
          //     UnificationState.DeleteVariable freshVar
          //       |> TypeValue.EquivalenceClassesOp
          //       |> Expr<'T, 'Id>.liftUnification

          let! t_res =
            TypeValue.CreateArrow(t_x, t_body)
            |> TypeValue.Instantiate loc0
            |> Expr.liftInstantiation

          return Expr.Lambda(x, Some t_x, body, loc0, ctx.Types.Scope), t_res, Kind.Star
        }
// |> state.MapError(Errors.Map(String.appendNewline $"...when typechecking `fun {x.Name} -> ...`"))

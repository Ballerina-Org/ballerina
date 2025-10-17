namespace Ballerina.DSL.Next.Types.TypeChecker

module LiftOtherSteps =
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
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Eval
  open Ballerina.Fun
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Collections.NonEmptyList

  type Expr<'T, 'Id when 'Id: comparison> with
    static member internal liftUnification
      (p: State<'a, UnificationContext, UnificationState, Errors>)
      : State<'a, TypeCheckContext, TypeCheckState, Errors> =
      state {
        let! s = state.GetState()
        let! ctx = state.GetContext()

        let newUnificationState =
          p
          |> State.Run(
            { EvalState = s.Types
              Scope = ctx.Types.Scope },
            s.Vars
          )

        match newUnificationState with
        | Left(res, newUnificationState) ->
          do!
            newUnificationState
            |> Option.map (fun (newUnificationState: UnificationState) ->
              state.SetState(TypeCheckState.Updaters.Vars(replaceWith newUnificationState)))
            |> state.RunOption
            |> state.Map ignore

          return res
        | Right(err, _) -> return! state.Throw err
      }

    static member internal liftTypeEval
      (p: State<'a, TypeExprEvalContext, TypeExprEvalState, Errors>)
      : State<'a, TypeCheckContext, TypeCheckState, Errors> =
      state {
        let! s = state.GetState()
        let! ctx = state.GetContext()

        let newTypesState = p |> State.Run(ctx.Types, s.Types)

        match newTypesState with
        | Left(res, newTypesState) ->
          do!
            newTypesState
            |> Option.map (fun (newTypesState: TypeExprEvalState) ->
              state.SetState(TypeCheckState.Updaters.Types(replaceWith newTypesState)))
            |> state.RunOption
            |> state.Map ignore

          return res
        | Right(err, _) -> return! state.Throw err
      }

    static member internal liftInstantiation
      (p: State<'a, TypeInstantiateContext, UnificationState, Errors>)
      : State<'a, TypeCheckContext, TypeCheckState, Errors> =
      state {
        let! s = state.GetState()
        let! ctx = state.GetContext()

        let newUnificationState =
          p
          |> State.Run(TypeCheckState.ToInstantiationContext(s, ctx.Types.Scope), s.Vars)

        match newUnificationState with
        | Left(res, newUnificationState) ->
          do!
            newUnificationState
            |> Option.map (fun (newUnificationState: UnificationState) ->
              state.SetState(TypeCheckState.Updaters.Vars(replaceWith newUnificationState)))
            |> state.RunOption
            |> state.Map ignore

          return res
        | Right(err, _) -> return! state.Throw err
      }

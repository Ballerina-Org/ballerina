namespace Ballerina.DSL.Next.Types.TypeChecker

module LiftOtherSteps =
  open Ballerina.StdLib.String
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Collections.Option
  open Ballerina.Errors
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open System
  open Ballerina.StdLib.Object
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Unification
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Patterns
  // open Ballerina.DSL.Next.Types.TypeChecker.Eval
  open Ballerina.Fun
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList

  type Expr<'T, 'Id, 've when 'Id: comparison> with
    static member liftUnification<'a, 'valueExt when 'valueExt: comparison>
      (p: State<'a, UnificationContext<'valueExt>, UnificationState<'valueExt>, Errors<Location>>)
      : State<'a, TypeCheckContext<'valueExt>, TypeCheckState<'valueExt>, Errors<Location>> =
      state {
        let! s = state.GetState()
        let! ctx = state.GetContext()

        let newUnificationState =
          p
          |> State.Run(
            { EvalState = s
              Scope = ctx.Scope
              TypeParameters = ctx.TypeParameters },
            s.Vars
          )

        match newUnificationState with
        | Left(res, newUnificationState) ->
          do!
            newUnificationState
            |> Option.map (fun (newUnificationState: UnificationState<'valueExt>) ->
              state.SetState(TypeCheckState.Updaters.Vars(replaceWith newUnificationState)))
            |> state.RunOption
            |> state.Map ignore

          return res
        | Right(err, _) -> return! state.Throw err
      }

    static member liftTypeEval<'a, 'valueExt when 'valueExt: comparison>
      (p: State<'a, TypeCheckContext<'valueExt>, TypeCheckState<'valueExt>, Errors<Location>>)
      : State<'a, TypeCheckContext<'valueExt>, TypeCheckState<'valueExt>, Errors<Location>> =
      state {
        let! s = state.GetState()
        let! ctx = state.GetContext()

        let newTypesState = p |> State.Run(ctx, s)

        match newTypesState with
        | Left(res, newTypesState) ->
          do!
            newTypesState
            |> Option.map (fun (newTypesState: TypeCheckState<'valueExt>) -> state.SetState(replaceWith newTypesState))
            |> state.RunOption
            |> state.Map ignore

          return res
        | Right(err, _) -> return! state.Throw err
      }

    static member liftInstantiation<'a, 'valueExt when 'valueExt: comparison>
      (p: State<'a, TypeInstantiateContext<'valueExt>, TypeCheckState<'valueExt>, Errors<Location>>)
      : State<'a, TypeCheckContext<'valueExt>, TypeCheckState<'valueExt>, Errors<Location>> =
      state {
        let! s = state.GetState()
        let! ctx = state.GetContext()

        let newUnificationState =
          p |> State.Run(TypeInstantiateContext.FromEvalContext(ctx), s)

        match newUnificationState with
        | Left(res, newUnificationState) ->
          do!
            newUnificationState
            |> Option.map (fun newState -> state.SetState(replaceWith newState))
            |> state.RunOption
            |> state.Map ignore

          return res
        | Right(err, _) -> return! state.Throw err
      }

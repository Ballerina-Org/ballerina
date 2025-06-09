namespace Ballerina.DSL.Expr

module Eval =
  open System
  open System.Linq
  open Ballerina.Fun
  open Ballerina.Core.Object
  open Ballerina.Coroutines.Model
  open Ballerina.Collections.Sum
  open Ballerina.DSL.Expr
  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Patterns
  open Ballerina.DSL.Expr.Types.Model
  open Ballerina.DSL.Expr.Types.TypeCheck
  open Ballerina.Errors

  type ExprEvalContext<'ExprExtension, 'ValueExtension> =
    { Vars: Vars<'ExprExtension, 'ValueExtension> }

    static member Update =
      {| Vars =
          fun
              (f: Updater<Vars<'ExprExtension, 'ValueExtension>>)
              (ctx: ExprEvalContext<'ExprExtension, 'ValueExtension>) -> { ctx with Vars = f ctx.Vars } |}

  type ExprEvalState = unit

  type Expr<'ExprExtension, 'ValueExtension> with
    static member eval
      (e: Expr<'ExprExtension, 'ValueExtension>)
      : Coroutine<
          Value<'ExprExtension, 'ValueExtension>,
          ExprEvalState,
          ExprEvalContext<'ExprExtension, 'ValueExtension>,
          Unit,
          Errors
         >
      =
      let (!) = Expr.eval

      co {
        match e with
        | Apply(f, arg) ->
          let! fValue = !f
          let! arg = !arg

          match fValue with
          | Value.Lambda(v, b) -> return! !b |> co.mapContext (ExprEvalContext.Update.Vars(Map.add v arg))
          | _ ->
            return!
              $"runtime error: {fValue} should be a function because it is applied"
              |> Errors.Singleton
              |> co.Throw

        | VarLookup varName ->
          let! ctx = co.GetContext()
          let! varValue = ctx.Vars |> Map.tryFindWithError varName "var" varName.VarName |> co.ofSum

          return varValue
        | Value v -> return v
        | Binary(Plus, e1, e2) ->
          let! v1 = !e1
          let! v2 = !e2
          let! v1 = v1 |> Value.AsInt |> co.ofSum
          let! v2 = v2 |> Value.AsInt |> co.ofSum
          return Value.ConstInt(v1 + v2)

        | e -> return! $"not implemented Expr evaluator for {e}" |> Errors.Singleton |> co.Throw
      }

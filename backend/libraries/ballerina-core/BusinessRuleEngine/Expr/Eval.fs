namespace Ballerina.DSL.Expr

module Eval =
  open System
  open System.Linq
  open Ballerina.Fun
  open Ballerina.Core.Object
  open Ballerina.Core.Object
  open Ballerina.Coroutines.Model
  open Ballerina.Collections.Sum
  open Ballerina.DSL.Expr
  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Patterns
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

  type ExprEval<'ExprExtension, 'ValueExtension> =
    (Expr<'ExprExtension, 'ValueExtension>)
      -> Coroutine<
        Value<'ExprExtension, 'ValueExtension>,
        ExprEvalState,
        ExprEvalContext<'ExprExtension, 'ValueExtension>,
        Unit,
        Errors
       >

  type EvalFrom<'ExprExtension, 'ValueExtension, 'ExprExtensionTail> =
    'ExprExtensionTail
      -> Coroutine<
        Value<'ExprExtension, 'ValueExtension>,
        ExprEvalState,
        ExprEvalContext<'ExprExtension, 'ValueExtension>,
        Unit,
        Errors
       >

  type Expr<'ExprExtension, 'ValueExtension> with
    static member eval
      : (ExprEval<'ExprExtension, 'ValueExtension> -> EvalFrom<'ExprExtension, 'ValueExtension, 'ExprExtension>)
          -> ExprEval<'ExprExtension, 'ValueExtension> =
      fun evalExtension e ->
        let (!) = Expr.eval evalExtension

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

          | Expr.Extension e -> return! evalExtension (Expr.eval evalExtension) e
          | e -> return! $"runtime error: eval({e}) not implemented" |> Errors.Singleton |> co.Throw
        }

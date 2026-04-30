module ballerinalang.Runners.BackgroundJob

open Ballerina
open Ballerina.Collections.Sum
open Ballerina.DSL.Next.StdLib.DB
open Ballerina.Reader.WithError
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.DSL.Next.Terms.FastEval
open Ballerina.DSL.Next.Terms.Eval
open Ballerina.DSL.Next.Terms.Patterns
open Ballerina.Errors
open Ballerina.Collections.NonEmptyList
open Ballerina.Collections.Map

let private mergeEvalScope
  (baseScope: ExprEvalContextScope<'valueExtension>)
  (evaluatedScope: ExprEvalContextScope<'valueExtension>)
  : ExprEvalContextScope<'valueExtension> =
  { Values =
      evaluatedScope.Values
      |> Map.merge (fun evaluatedValue _baseValue -> evaluatedValue) baseScope.Values
    Symbols =
      ExprEvalContextSymbols.Append baseScope.Symbols evaluatedScope.Symbols }

let executeBackgroundJob
  dbio
  (baseEvalContext: ExprEvalContext<'runtimeContext, 'valueExtension>)
  (injectBackgroundContext:
    Updater<ExprEvalContext<'runtimeContext, 'valueExtension>>)
  backgroundJob
  value
  entityId
  =
  sum {
    let backgroundJobExpr =
      RunnableExpr.Apply(
        RunnableExpr.Apply(
          RunnableExpr.Apply(
            backgroundJob,
            RunnableExpr.FromValue(
              dbio.SchemaAsValue,
              TypeValue.CreatePrimitive PrimitiveType.Unit,
              Kind.Star
            ),
            TypeValue.CreatePrimitive PrimitiveType.Unit,
            Kind.Star
          ),
          RunnableExpr.FromValue(
            entityId,
            TypeValue.CreatePrimitive PrimitiveType.Unit,
            Kind.Star
          ),
          TypeValue.CreatePrimitive PrimitiveType.Unit,
          Kind.Star
        ),
        RunnableExpr.FromValue(
          value,
          TypeValue.CreatePrimitive PrimitiveType.Unit,
          Kind.Star
        ),
        TypeValue.CreatePrimitive PrimitiveType.Unit,
        Kind.Star
      )

    let backgroundJobResult =
      Expr.Eval(NonEmptyList.OfList(backgroundJobExpr, []))
      |> Reader.mapContext injectBackgroundContext
      |> Reader.Run
        { baseEvalContext with
            Scope = mergeEvalScope baseEvalContext.Scope dbio.EvalContext
            ValueOverlays = []
            RootLevelEval = true }

    match backgroundJobResult with
    | Left(Value.Sum(_,
                     Value.Primitive(PrimitiveValue.TimeSpan delayBeforeNextExecution))) ->
      return Some delayBeforeNextExecution
    | Left _ -> return None
    | Right err -> return! Right err
  }
  |> sum.WithErrorContext(fun () -> "...while executing background job")

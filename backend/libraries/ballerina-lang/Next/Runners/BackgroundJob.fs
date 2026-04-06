module ballerinalang.Runners.BackgroundJob

open Ballerina
open Ballerina.Collections.Sum
open Ballerina.DSL.Next.StdLib.DB
open Ballerina.Reader.WithError
open Ballerina.DSL.Next.Types.Model
open Ballerina.DSL.Next.Types.Patterns
open Ballerina.DSL.Next.Terms.Eval
open Ballerina.DSL.Next.Terms.Patterns
open Ballerina.Errors
open Ballerina.Collections.NonEmptyList

let executeBackgroundJob
  dbio
  (evalContext: ValueExtensionOps<'runtimeContext, 'valueExtension>)
  (injectBackgroundContext: Updater<ExprEvalContext<'runtimeContext, 'valueExtension>>)
  runtimeContext
  backgroundJob
  value
  entityId
  =
  sum {
    let backgroundJobExpr =
      TypeCheckedExpr.Apply(
        TypeCheckedExpr.Apply(
          TypeCheckedExpr.Apply(
            backgroundJob,
            TypeCheckedExpr.FromValue(dbio.SchemaAsValue, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star),
            TypeValue.CreatePrimitive PrimitiveType.Unit,
            Kind.Star
          ),
          TypeCheckedExpr.FromValue(entityId, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star),
          TypeValue.CreatePrimitive PrimitiveType.Unit,
          Kind.Star
        ),
        TypeCheckedExpr.FromValue(value, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star),
        TypeValue.CreatePrimitive PrimitiveType.Unit,
        Kind.Star
      )

    let backgroundJobResult =
      Expr.Eval(NonEmptyList.OfList(backgroundJobExpr, []))
      |> Reader.mapContext injectBackgroundContext
      |> Reader.Run
        { Scope = dbio.EvalContext
          ExtensionOps = evalContext
          RuntimeContext = runtimeContext
          RootLevelEval = true }

    match backgroundJobResult with
    | Left(Value.Sum(_, Value.Primitive(PrimitiveValue.TimeSpan delayBeforeNextExecution))) ->
      return Some delayBeforeNextExecution
    | Left _ -> return None
    | Right err -> return! Right err
  }
  |> sum.WithErrorContext(fun () -> "...while executing background job")

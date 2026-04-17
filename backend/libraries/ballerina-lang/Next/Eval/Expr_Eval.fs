namespace Ballerina.DSL.Next.Terms

[<AutoOpen>]
module Eval =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Map
  open Ballerina.Coroutines.Model
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.StdLib.String
  open Ballerina.StdLib.Object
  open System
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList

  type ExprEvalContext<'runtimeContext, 'valueExtension> with
    static member Empty
      : 'runtimeContext -> ExprEvalContext<'runtimeContext, 'valueExtension> =
      fun runtimeContext ->
        { RootLevelEval = true
          RuntimeContext = runtimeContext
          Scope =
            { Values = Map.empty
              Symbols = ExprEvalContextSymbols.Empty }
          ExtensionOps =
            { Eval =
                fun loc0 _ _ ->
                  (fun () -> $"Error: cannot evaluate empty extension")
                  |> Errors.Singleton loc0
                  |> reader.Throw
              Applicables = Map.empty
              FastApplicables = Map.empty } }

    static member WithTypeCheckingSymbols<'valueExtension>
      (ctx: ExprEvalContext<'runtimeContext, 'valueExtension>)
      (symbols: TypeExprEvalSymbols)
      : ExprEvalContext<'runtimeContext, 'valueExtension> =
      { ctx with
          Scope =
            { ctx.Scope with
                Symbols =
                  ExprEvalContextSymbols.Append
                    ctx.Scope.Symbols
                    (ExprEvalContextSymbols.FromTypeChecker symbols) } }

    static member Getters =
      {| Values =
          fun (c: ExprEvalContext<'runtimeContext, 'valueExtension>) ->
            c.Scope.Values
         ExtensionOps =
          fun (c: ExprEvalContext<'runtimeContext, 'valueExtension>) ->
            c.ExtensionOps
         Symbols =
          fun (c: ExprEvalContext<'runtimeContext, 'valueExtension>) ->
            c.Scope.Symbols |}

    static member Updaters =
      {| Values =
          fun u (c: ExprEvalContext<'runtimeContext, 'valueExtension>) ->
            { c with
                Scope =
                  { c.Scope with
                      Values = u (c.Scope.Values) } }
         ExtensionOps =
          fun u (c: ExprEvalContext<'runtimeContext, 'valueExtension>) ->
            { c with
                ExtensionOps = u (c.ExtensionOps) }
         Symbols =
          fun u (c: ExprEvalContext<'runtimeContext, 'valueExtension>) ->
            { c with
                Scope =
                  { c.Scope with
                      Symbols = u (c.Scope.Symbols) } }
         RootLevelEval =
          fun u (c: ExprEvalContext<'runtimeContext, 'valueExtension>) ->
            { c with
                RootLevelEval = u (c.RootLevelEval) } |}

  type Expr<'T, 'Id, 'valueExt when 'Id: comparison> with
    static member EvalApply (loc0: Location) (rest: List<_>) (fV, argV) =
      Reader(fun (ctx: ExprEvalContext<'runtimeContext, 'valueExtension>) ->
        try
          Left(
            fastApply<'runtimeContext, 'valueExtension>
              loc0
              TypeCheckScope.Empty
              rest
              ctx
              fV
              argV
          )
        with :? EvalException as ex ->
          Right ex.Errors)

    // NOTE: expressions are concatenated in the order of the input (the returned value is of the type of the last expression)
    static member Eval<'runtimeContext, 'valueExtension>
      (NonEmptyList(e, rest): NonEmptyList<RunnableExpr<'valueExtension>>)
      : ExprEvaluator<
          'runtimeContext,
          'valueExtension,
          Value<TypeValue<'valueExtension>, 'valueExtension>
         >
      =
      Reader(fun (ctx: ExprEvalContext<'runtimeContext, 'valueExtension>) ->
        try
          let compiled =
            compileSequence<'runtimeContext, 'valueExtension>(
              NonEmptyList.OfList(e, rest)
            )

          Left(compiled ctx)
        with :? EvalException as ex ->
          Right ex.Errors)

namespace Ballerina.DSL.Next.Extensions

[<AutoOpen>]
module TypeLambdas =
  open Ballerina
  open Ballerina.Reader.WithError
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Types.TypeCheck
  open Ballerina.DSL.Next.Extensions
  open Ballerina.DSL.Next.Terms

  type TypeLambdaExtension<'ext, 'extTypeLambda> with
    static member RegisterTypeCheckContext(ext: TypeLambdaExtension<'ext, 'extTypeLambda>) : Updater<TypeCheckContext> =
      fun typeCheckContext ->
        let values = typeCheckContext.Values

        let values = Map.add ext.Id (ext.Type, ext.Kind) values

        { typeCheckContext with
            Values = values }

    static member RegisterTypeCheckState(_ext: TypeLambdaExtension<'ext, 'extTypeLambda>) : Updater<TypeCheckState> = id

    static member RegisterExprEvalContext
      (ext: TypeLambdaExtension<'ext, 'extTypeLambda>)
      : Updater<ExprEvalContext<'ext>> =
      fun evalContext ->
        let ops = evalContext.ExtensionOps.Eval

        let newOps = [ ext.EvalToTypeApplicable; ext.EvalToApplicable ]

        let ops = fun v -> reader.Any(ops v, newOps |> List.map (fun newOp -> newOp v))

        { evalContext with
            Values = Map.add ext.Id (ext.Value |> ext.ValueLens.Set |> Ext) evalContext.Values
            ExtensionOps = { Eval = ops } }

    static member RegisterLanguageContext
      (ext: TypeLambdaExtension<'ext, 'extTypeLambda>)
      : Updater<LanguageContext<'ext>> =
      fun langCtx ->
        { TypeCheckContext =
            langCtx.TypeCheckContext
            |> (ext |> TypeLambdaExtension.RegisterTypeCheckContext)
          TypeCheckState = langCtx.TypeCheckState |> (ext |> TypeLambdaExtension.RegisterTypeCheckState)
          ExprEvalContext = langCtx.ExprEvalContext |> (ext |> TypeLambdaExtension.RegisterExprEvalContext) }

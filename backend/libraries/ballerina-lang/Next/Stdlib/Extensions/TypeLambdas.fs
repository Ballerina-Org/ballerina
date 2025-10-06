namespace Ballerina.DSL.Next.Extensions

[<AutoOpen>]
module TypeLambdas =
  open Ballerina
  open Ballerina.Reader.WithError
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Types.TypeCheck
  open Ballerina.DSL.Next.Extensions
  open Ballerina.DSL.Next.Terms
  open Ballerina.Collections.NonEmptyList

  type TypeLambdaExtension<'ext, 'extTypeLambda> with
    static member RegisterTypeCheckContext(ext: TypeLambdaExtension<'ext, 'extTypeLambda>) : Updater<TypeCheckContext> =
      fun typeCheckContext ->
        let values = typeCheckContext.Values

        let id, typeValue, kind = ext.ExtensionType
        let values = Map.add id (typeValue, kind) values

        { typeCheckContext with
            Values = values }

    static member RegisterTypeCheckState(ext: TypeLambdaExtension<'ext, 'extTypeLambda>) : Updater<TypeCheckState> =
      fun typeCheckState ->

        let bindings = typeCheckState.Types.Bindings

        let bindings =
          ext.ReferencedTypes
          |> NonEmptyList.ToSeq
          |> Seq.fold (fun acc (id, typeValue, kind) -> acc |> Map.add id (typeValue, kind)) bindings

        { typeCheckState with
            Types =
              { typeCheckState.Types with
                  Bindings = bindings
                  Symbols = typeCheckState.Types.Symbols } }

    static member RegisterExprEvalContext
      (ext: TypeLambdaExtension<'ext, 'extTypeLambda>)
      : Updater<ExprEvalContext<'ext>> =
      fun evalContext ->
        let ops = evalContext.ExtensionOps.Eval

        let newOps = [ ext.EvalToTypeApplicable; ext.EvalToApplicable ]

        let ops = fun v -> reader.Any(ops v, newOps |> List.map (fun newOp -> newOp v))

        let id, _, _ = ext.ExtensionType

        { evalContext with
            Values = Map.add id (ext.Value |> ext.ValueLens.Set |> Ext) evalContext.Values
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

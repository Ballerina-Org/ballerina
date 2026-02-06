namespace Ballerina.DSL.Next.Extensions

[<AutoOpen>]
module TypeLambdas =
  open Ballerina
  open Ballerina.Reader.WithError
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Expr
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Extensions
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Next.Serialization
  open Ballerina.Collections.Map

  type TypeLambdaExtension<'e, 'extDTO, 'extTypeLambda when 'extDTO: not null and 'extDTO: not struct> with
    static member RegisterTypeCheckContext<'ext when 'ext: comparison>
      (ext: TypeLambdaExtension<'ext, 'extDTO, 'extTypeLambda>)
      : Updater<TypeCheckContext<'ext>> =
      fun typeCheckContext ->
        let values = typeCheckContext.Values

        let id, typeValue, kind = ext.ExtensionType
        let values = Map.add id (typeValue, kind) values

        { typeCheckContext with
            Values = values }


    static member RegisterTypeCheckState<'ext when 'ext: comparison>
      (ext: TypeLambdaExtension<'ext, 'extDTO, 'extTypeLambda>)
      : Updater<TypeCheckState<'ext>> =
      fun typeCheckState ->
        { typeCheckState with
            Bindings = typeCheckState.Bindings |> Map.merge (fun _ -> id) ext.ExtraBindings }

    static member RegisterExprEvalContext<'ext when 'ext: comparison>
      (ext: TypeLambdaExtension<'ext, 'extDTO, 'extTypeLambda>)
      : Updater<ExprEvalContext<'ext>> =
      fun evalContext ->
        let ops = evalContext.ExtensionOps.Eval

        let newOps = [ ext.EvalToTypeApplicable; ext.EvalToApplicable ]

        let ops =
          fun loc0 rest v -> reader.Any(ops loc0 rest v, newOps |> List.map (fun newOp -> newOp loc0 rest v))

        let id, _, _ = ext.ExtensionType

        { evalContext with
            Scope =
              { evalContext.Scope with
                  Values = Map.add id ((ext.Value |> ext.ValueLens.Set, None) |> Ext) evalContext.Scope.Values }
            ExtensionOps =
              { Eval = ops
                Applicables = evalContext.ExtensionOps.Applicables } }

    static member RegisterLanguageContext<'ext, 'extDTO when 'ext: comparison>
      (ext: TypeLambdaExtension<'ext, 'extDTO, 'extTypeLambda>)
      : Updater<LanguageContext<'ext, 'extDTO>> =
      fun langCtx ->
        { TypeCheckContext =
            langCtx.TypeCheckContext
            |> (ext |> TypeLambdaExtension.RegisterTypeCheckContext)
          ExprEvalContext = langCtx.ExprEvalContext |> (ext |> TypeLambdaExtension.RegisterExprEvalContext)
          TypeCheckedPreludes = langCtx.TypeCheckedPreludes
          TypeCheckState = langCtx.TypeCheckState |> (ext |> TypeLambdaExtension.RegisterTypeCheckState)
          SerializationContext = langCtx.SerializationContext }

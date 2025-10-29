namespace Ballerina.DSL.Next.Extensions

[<AutoOpen>]
module Operations =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Expr
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Extensions
  open Ballerina.DSL.Next.Terms

  type OperationsExtension<'ext, 'extOperations> with
    static member RegisterTypeCheckContext
      (opsExt: OperationsExtension<'ext, 'extOperations>)
      : Updater<TypeCheckContext> =
      fun typeCheckContext ->
        let values = typeCheckContext.Values

        let values =
          opsExt.Operations
          |> Map.toSeq
          |> Seq.fold (fun acc (caseId, caseExt) -> acc |> Map.add caseId (caseExt.Type, caseExt.Kind)) values

        { typeCheckContext with
            Values = values }

    static member RegisterTypeCheckState(_opsExt: OperationsExtension<'ext, 'extOperations>) : Updater<TypeCheckState> =
      id // leave it here, it will be needed later

    static member RegisterExprEvalContext
      (opsExt: OperationsExtension<'ext, 'extOperations>)
      : Updater<ExprEvalContext<'ext>> =
      fun evalContext ->
        let ops = evalContext.ExtensionOps.Eval

        let ops =
          opsExt.Operations
          |> Map.values
          |> Seq.fold
            (fun (acc: Location -> 'ext -> ExprEvaluator<'ext, ExtEvalResult<'ext>>) caseExt ->
              fun loc0 v ->
                reader.Any(
                  reader {
                    let! v =
                      caseExt.OperationsLens.Get v
                      |> sum.OfOption((loc0, $"Error: cannot extra constructor from extension") |> Errors.Singleton)
                      |> reader.OfSum

                    return Applicable(fun arg -> caseExt.Apply loc0 (v, arg))
                  },
                  [ acc loc0 v ]
                ))
            ops

        let values = evalContext.Values

        let values =
          opsExt.Operations
          |> Map.toSeq
          |> Seq.fold
            (fun acc (caseId, caseExt) ->
              acc |> Map.add caseId (caseExt.Operation |> caseExt.OperationsLens.Set |> Ext))
            values

        { evalContext with
            Values = values
            ExtensionOps = { Eval = ops } }

    static member RegisterLanguageContext
      (opsExt: OperationsExtension<'ext, 'extOperations>)
      : Updater<LanguageContext<'ext>> =
      fun langCtx ->
        { TypeCheckContext =
            langCtx.TypeCheckContext
            |> (opsExt |> OperationsExtension.RegisterTypeCheckContext)
          TypeCheckState = langCtx.TypeCheckState |> (opsExt |> OperationsExtension.RegisterTypeCheckState)
          ExprEvalContext =
            langCtx.ExprEvalContext
            |> (opsExt |> OperationsExtension.RegisterExprEvalContext) }

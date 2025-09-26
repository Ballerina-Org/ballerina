namespace Ballerina.DSL.Next.Extensions

[<AutoOpen>]
module Operations =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Types.TypeCheck
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
            (fun (acc: 'ext -> ExprEvaluator<'ext, ExtEvalResult<'ext>>) caseExt ->
              fun v ->
                reader.Any(
                  reader {
                    let! v =
                      caseExt.OperationsLens.Get v
                      |> sum.OfOption($"Error: cannot extra constructor from extension" |> Errors.Singleton)
                      |> reader.OfSum

                    return Applicable(fun arg -> caseExt.Apply(v, arg))
                  },
                  [ acc v ]
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

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
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.Terms

  type OperationsExtension<'e, 'extOperations> with
    static member RegisterTypeCheckContext<'ext when 'ext: comparison>
      (opsExt: OperationsExtension<'ext, 'extOperations>)
      : Updater<TypeCheckContext<'ext>> =
      fun typeCheckContext ->
        let values = typeCheckContext.Values

        let values =
          opsExt.Operations
          |> Map.toSeq
          |> Seq.fold
            (fun acc (caseId, caseExt) ->
              match caseExt.PublicIdentifiers with
              | None -> acc
              | Some(t, k, _) -> acc |> Map.add caseId (t, k))
            values

        { typeCheckContext with
            Values = values }

    static member RegisterTypeCheckState<'ext when 'ext: comparison>
      (_opsExt: OperationsExtension<'ext, 'extOperations>)
      : Updater<TypeCheckState<'ext>> =
      id // leave it here, it will be needed later

    static member RegisterExprEvalContext<'ext when 'ext: comparison>
      (opsExt: OperationsExtension<'ext, 'extOperations>)
      : Updater<ExprEvalContext<'ext>> =
      fun evalContext ->
        let ops = evalContext.ExtensionOps.Eval

        let ops =
          opsExt.Operations
          |> Map.values
          |> Seq.fold
            (fun (acc: Location -> List<_> -> 'ext -> ExprEvaluator<'ext, ExtEvalResult<'ext>>) caseExt ->
              fun loc0 rest v ->
                reader.Any(
                  reader {
                    let! v =
                      caseExt.OperationsLens.Get v
                      |> sum.OfOption((loc0, $"Error: cannot extra constructor from extension") |> Errors.Singleton)
                      |> reader.OfSum

                    return Applicable(fun arg -> caseExt.Apply loc0 rest (v, arg))
                  },
                  [ acc loc0 rest v ]
                ))
            ops

        let values = evalContext.Values

        let values =
          opsExt.Operations
          |> Map.toSeq
          |> Seq.fold
            (fun acc (caseId, caseExt) ->
              match caseExt.PublicIdentifiers with
              | None -> acc
              | Some(_, _, publicIdentifiers) ->
                acc
                |> Map.add caseId (publicIdentifiers |> caseExt.OperationsLens.Set |> Value.Ext))
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

namespace Ballerina.DSL.Next.Extensions

[<AutoOpen>]
module Operations =
  open Ballerina
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Map
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
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
                      |> sum.OfOption(
                        (fun () -> $"Error: cannot extra constructor from extension")
                        |> Errors.Singleton loc0
                      )
                      |> reader.OfSum

                    return Applicable(fun arg -> caseExt.Apply loc0 rest (v, arg))
                  },
                  [ acc loc0 rest v ]
                ))
            ops

        let values = evalContext.Scope.Values

        let values =
          opsExt.Operations
          |> Map.toSeq
          |> Seq.fold
            (fun acc (caseId, caseExt) ->
              match caseExt.PublicIdentifiers with
              | None -> acc
              | Some(_, _, publicIdentifiers) ->
                acc
                |> Map.add caseId ((publicIdentifiers |> caseExt.OperationsLens.Set, Some caseId) |> Value.Ext))
            values

        let applicables: Map<ResolvedIdentifier, ApplicableExtEvalResult<'ext>> =
          opsExt.Operations
          |> Map.map (fun (_k: ResolvedIdentifier) (op: OperationExtension<'ext, 'extOperations>) ->
            fun loc0 rest f v ->
              reader {
                let! f =
                  op.OperationsLens.Get f
                  |> sum.OfOption(
                    (fun () -> $"Error: cannot extract constructor from extension")
                    |> Errors.Singleton loc0
                  )
                  |> reader.OfSum

                return! op.Apply loc0 rest (f, v)
              })

        let applicables =
          Map.merge (fun _ -> id) evalContext.ExtensionOps.Applicables applicables

        { evalContext with
            Scope =
              { evalContext.Scope with
                  Values = values }
            ExtensionOps =
              { Eval = ops
                Applicables = applicables } }

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
            |> (opsExt |> OperationsExtension.RegisterExprEvalContext)
          TypeCheckedPreludes = langCtx.TypeCheckedPreludes }

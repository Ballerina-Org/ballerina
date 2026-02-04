namespace Ballerina.DSL.Next.Extensions

[<AutoOpen>]
module Types =
  open Ballerina
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Map
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Extensions
  open Ballerina.DSL.Next.Terms
  open Ballerina.Collections.NonEmptyList

  type TypeExtension<'e, 'extConstructors, 'extValues, 'extOperations> with
    static member RegisterTypeCheckContext<'ext when 'ext: comparison>
      (typeExt: TypeExtension<'ext, 'extConstructors, 'extValues, 'extOperations>)
      : Updater<TypeCheckContext<'ext>> =
      fun typeCheckContext ->
        let kind =
          typeExt.TypeVars
          |> List.map snd
          |> List.fold (fun acc k -> Kind.Arrow(k, acc)) Kind.Star

        let values =
          typeExt.Cases
          |> Map.toSeq
          |> Seq.fold
            (fun acc ((caseId, _), caseExt) -> acc |> Map.add caseId (caseExt.ConstructorType, kind))
            typeCheckContext.Values

        let values =
          typeExt.Operations
          |> Map.toSeq
          |> Seq.fold (fun acc (caseId, caseExt) -> acc |> Map.add caseId (caseExt.Type, caseExt.Kind)) values

        { typeCheckContext with
            Values = values }

    static member RegisterTypeCheckState<'ext when 'ext: comparison>
      (typeExt: TypeExtension<'ext, 'extConstructors, 'extValues, 'extOperations>)
      : Updater<TypeCheckState<'ext>> =
      fun typeCheckState ->
        let kind =
          typeExt.TypeVars
          |> List.map snd
          |> List.fold (fun acc k -> Kind.Arrow(k, acc)) Kind.Star

        let typeExtUnion =
          typeExt |> TypeExtension.ToImportedTypeValue |> TypeValue.Imported

        let bindings =
          typeCheckState.Bindings
          |> Map.add (typeExt.TypeName |> fst) (typeExtUnion, kind)

        { typeCheckState with
            Bindings = bindings
            Symbols =
              { typeCheckState.Symbols with
                  UnionCases =
                    typeExt.Cases
                    |> Map.keys
                    |> Seq.fold (fun acc (id, sym) -> acc |> Map.add id sym) typeCheckState.Symbols.UnionCases } }

    static member RegisterExprEvalContext<'ext when 'ext: comparison>
      (typeExt: TypeExtension<'ext, 'extConstructors, 'extValues, 'extOperations>)
      : Updater<ExprEvalContext<'ext>> =
      fun evalContext ->
        // let ops =
        //   typeExt.Cases
        //   |> Map.toSeq
        //   |> Seq.fold
        //     (fun
        //          (acc:
        //            Location
        //              -> List<Expr<TypeValue<'ext>, ResolvedIdentifier, 'ext>>
        //              -> 'ext
        //              -> ExprEvaluator<'ext, ExtEvalResult<'ext>>)
        //          ((caseId, _), caseExt) ->
        //       fun loc0 rest v ->
        //         reader.Any(
        //           reader {
        //             // let! v =
        //             //   caseExt.ValueLens.Get v
        //             //   |> sum.OfOption((fun () -> $"Error: cannot get value from extension") |> Errors.Singleton loc0)
        //             //   |> reader.OfSum

        //             // let v = typeExt.Deconstruct v

        //             return
        //               Matchable(fun handlers ->
        //                 reader {
        //                   let! handlerVar, handlerBody =
        //                     handlers
        //                     |> Map.tryFindWithError caseId "handlers" (fun () -> "Option.Some") loc0
        //                     |> reader.OfSum


        //                   return!
        //                     handlerBody
        //                     |> NonEmptyList.One
        //                     |> Expr.Eval
        //                     |> reader.MapContext(
        //                       ExprEvalContext.Updaters.Values(
        //                         Map.add
        //                           (handlerVar.Name |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve)
        //                           (Ext(v, None))
        //                       )
        //                     )
        //                 })
        //           },
        //           [ (reader {
        //               let! v =
        //                 caseExt.ConsLens.Get v
        //                 |> sum.OfOption(
        //                   (fun () -> $"Error: cannot extra constructor from extension")
        //                   |> Errors.Singleton loc0
        //                 )
        //                 |> reader.OfSum

        //               return
        //                 Applicable(fun arg ->
        //                   reader {
        //                     let! constructed = caseExt.Apply loc0 rest (v, arg)
        //                     return constructed
        //                   })
        //             })
        //             acc loc0 rest v ]
        //         ))
        //     evalContext.ExtensionOps.Eval

        let ops =
          typeExt.Operations
          |> Map.values
          |> Seq.fold
            (fun
                 (acc:
                   Location
                     -> List<Expr<TypeValue<'ext>, ResolvedIdentifier, 'ext>>
                     -> 'ext
                     -> ExprEvaluator<'ext, ExtEvalResult<'ext>>)
                 caseExt ->
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

                    return
                      Applicable(fun arg ->
                        reader {
                          let! constructed = caseExt.Apply loc0 rest (v, arg)
                          return constructed
                        })
                  },
                  [ acc loc0 rest v ]
                ))
            evalContext.ExtensionOps.Eval

        let values =
          typeExt.Cases
          |> Map.toSeq
          |> Seq.fold
            (fun acc ((caseId, _), caseExt) ->
              acc
              |> Map.add caseId ((caseExt.Constructor |> caseExt.ConsLens.Set, None) |> Ext))
            evalContext.Scope.Values

        let values =
          typeExt.Operations
          |> Map.toSeq
          |> Seq.fold
            (fun acc (caseId, caseExt) ->
              acc
              |> Map.add caseId ((caseExt.Operation |> caseExt.OperationsLens.Set, None) |> Ext))
            values

        let applicables: Map<ResolvedIdentifier, ApplicableExtEvalResult<'ext>> =
          typeExt.Operations
          |> Map.map
            (fun
                 (_k: ResolvedIdentifier)
                 (op: TypeOperationExtension<'ext, 'extConstructors, 'extValues, 'extOperations>) ->
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
      (typeExt: TypeExtension<'ext, 'extConstructors, 'extValues, 'extOperations>)
      : Updater<LanguageContext<'ext>> =
      fun langCtx ->
        { TypeCheckContext = langCtx.TypeCheckContext |> (typeExt |> TypeExtension.RegisterTypeCheckContext)
          TypeCheckState = langCtx.TypeCheckState |> (typeExt |> TypeExtension.RegisterTypeCheckState)
          ExprEvalContext = langCtx.ExprEvalContext |> (typeExt |> TypeExtension.RegisterExprEvalContext)
          TypeCheckedPreludes = langCtx.TypeCheckedPreludes }

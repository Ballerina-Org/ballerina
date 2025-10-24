namespace Ballerina.DSL.Next.Extensions

[<AutoOpen>]
module Types =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Expr
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Eval
  open Ballerina.DSL.Next.Extensions
  open Ballerina.DSL.Next.Terms
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap

  type TypeExtension<'ext, 'extConstructors, 'extValues, 'extOperations> with
    static member RegisterTypeCheckContext
      (typeExt: TypeExtension<'ext, 'extConstructors, 'extValues, 'extOperations>)
      : Updater<TypeCheckContext> =
      fun typeCheckContext ->
        let kind =
          typeExt.TypeVars
          |> List.map snd
          |> List.fold (fun acc k -> Kind.Arrow(k, acc)) Kind.Star

        let values =
          typeExt.Cases
          |> Map.toSeq
          |> Seq.fold
            (fun acc ((caseId, _), caseExt) ->
              acc |> Map.add caseId (typeExt.WrapTypeVars caseExt.ConstructorType, kind))
            typeCheckContext.Values

        let values =
          typeExt.Operations
          |> Map.toSeq
          |> Seq.fold (fun acc (caseId, caseExt) -> acc |> Map.add caseId (caseExt.Type, caseExt.Kind)) values

        { typeCheckContext with
            Values = values }

    static member RegisterTypeCheckState
      (typeExt: TypeExtension<'ext, 'extConstructors, 'extValues, 'extOperations>)
      : Updater<TypeCheckState> =
      fun typeCheckState ->
        let kind =
          typeExt.TypeVars
          |> List.map snd
          |> List.fold (fun acc k -> Kind.Arrow(k, acc)) Kind.Star

        let typeExtUnion =
          TypeValue
            .Imported(
              { Id = typeExt.TypeName |> fst
                Sym = typeExt.TypeName |> snd
                Parameters = typeExt.TypeVars |> List.map (fun (tv, k) -> TypeParameter.Create(tv.Name, k))
                Arguments = []
                UnionLike =
                  if typeExt.Cases |> Map.isEmpty then
                    None
                  else
                    typeExt.Cases
                    |> Map.toSeq
                    |> Seq.map (fun ((_, sym), caseExt) -> (sym, caseExt.CaseType))
                    |> OrderedMap.ofSeq
                    |> Some
                RecordLike = None }
            )
            .AsExpr
          |> typeExt.WrapTypeVars

        let bindings =
          typeCheckState.Types.Bindings
          |> Map.add (typeExt.TypeName |> fst) (typeExtUnion, kind)

        { typeCheckState with
            Types =
              { typeCheckState.Types with
                  Bindings = bindings
                  Symbols =
                    { typeCheckState.Types.Symbols with
                        UnionCases =
                          typeExt.Cases
                          |> Map.keys
                          |> Seq.fold
                            (fun acc (id, sym) -> acc |> Map.add id sym)
                            typeCheckState.Types.Symbols.UnionCases } } }

    static member RegisterExprEvalContext
      (typeExt: TypeExtension<'ext, 'extConstructors, 'extValues, 'extOperations>)
      : Updater<ExprEvalContext<'ext>> =
      fun evalContext ->
        let ops =
          typeExt.Cases
          |> Map.toSeq
          |> Seq.fold
            (fun (acc: Location -> 'ext -> ExprEvaluator<'ext, ExtEvalResult<'ext>>) ((caseId, _), caseExt) ->
              fun loc0 v ->
                reader.Any(
                  reader {
                    let! v =
                      caseExt.ValueLens.Get v
                      |> sum.OfOption((loc0, $"Error: cannot get value from extension") |> Errors.Singleton)
                      |> reader.OfSum

                    let v = typeExt.Deconstruct v

                    return
                      Matchable(fun handlers ->
                        reader {
                          let! handlerVar, handlerBody =
                            handlers
                            |> Map.tryFindWithError caseId "handlers" "Option.Some" loc0
                            |> reader.OfSum


                          return!
                            handlerBody
                            |> Expr.Eval
                            |> reader.MapContext(
                              ExprEvalContext.Updaters.Values(
                                Map.add (handlerVar.Name |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve) v
                              )
                            )
                        })
                  },
                  [ (reader {
                      let! v =
                        caseExt.ConsLens.Get v
                        |> sum.OfOption((loc0, $"Error: cannot extra constructor from extension") |> Errors.Singleton)
                        |> reader.OfSum

                      return
                        Applicable(fun arg ->
                          reader {
                            let! constructed = caseExt.Apply loc0 (v, arg)
                            return constructed
                          })
                    })
                    acc loc0 v ]
                ))
            evalContext.ExtensionOps.Eval

        let ops =
          typeExt.Operations
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

                    return
                      Applicable(fun arg ->
                        reader {
                          let! constructed = caseExt.Apply loc0 (v, arg)
                          return constructed
                        })
                  },
                  [ acc loc0 v ]
                ))
            ops

        let values =
          typeExt.Cases
          |> Map.toSeq
          |> Seq.fold
            (fun acc ((caseId, _), caseExt) ->
              acc |> Map.add caseId (caseExt.Constructor |> caseExt.ConsLens.Set |> Ext))
            evalContext.Values

        let values =
          typeExt.Operations
          |> Map.toSeq
          |> Seq.fold
            (fun acc (caseId, caseExt) ->
              acc |> Map.add caseId (caseExt.Operation |> caseExt.OperationsLens.Set |> Ext))
            values

        { evalContext with
            Values = values
            ExtensionOps = { Eval = ops } }

    static member RegisterLanguageContext
      (typeExt: TypeExtension<'ext, 'extConstructors, 'extValues, 'extOperations>)
      : Updater<LanguageContext<'ext>> =
      fun langCtx ->
        { TypeCheckContext = langCtx.TypeCheckContext |> (typeExt |> TypeExtension.RegisterTypeCheckContext)
          TypeCheckState = langCtx.TypeCheckState |> (typeExt |> TypeExtension.RegisterTypeCheckState)
          ExprEvalContext = langCtx.ExprEvalContext |> (typeExt |> TypeExtension.RegisterExprEvalContext) }

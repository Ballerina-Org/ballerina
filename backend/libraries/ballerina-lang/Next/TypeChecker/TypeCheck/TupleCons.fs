namespace Ballerina.DSL.Next.Types.TypeChecker

module TupleCons =
  open Ballerina.StdLib.String
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Collections.Option
  open Ballerina.LocalizedErrors
  open System
  open Ballerina.StdLib.Object
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Unification
  open Ballerina.DSL.Next.Types.TypeChecker.AdHocPolymorphicOperators
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Eval
  open Ballerina.DSL.Next.Types.TypeChecker.LiftOtherSteps
  open Ballerina.DSL.Next.Types.TypeChecker.Primitive
  open Ballerina.DSL.Next.Types.TypeChecker.Lookup
  open Ballerina.DSL.Next.Types.TypeChecker.Lambda
  open Ballerina.DSL.Next.Types.TypeChecker.Apply
  open Ballerina.DSL.Next.Types.TypeChecker.If
  open Ballerina.DSL.Next.Types.TypeChecker.Let
  open Ballerina.DSL.Next.Types.TypeChecker.RecordCons
  open Ballerina.DSL.Next.Types.TypeChecker.RecordWith
  open Ballerina.DSL.Next.Types.TypeChecker.RecordDes
  open Ballerina.DSL.Next.Types.TypeChecker.UnionDes
  open Ballerina.DSL.Next.Types.TypeChecker.SumDes
  open Ballerina.DSL.Next.Types.TypeChecker.SumCons
  open Ballerina.DSL.Next.Types.TypeChecker.TupleDes
  open Ballerina.DSL.Next.Types.TypeChecker.TypeLambda
  open Ballerina.Fun
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Collections.NonEmptyList

  type Expr<'T, 'Id when 'Id: comparison> with
    static member internal TypeCheckTupleCons
      (typeCheckExpr: TypeChecker, loc0: Location)
      : TypeChecker<ExprTupleCons<TypeExpr, Identifier>> =
      fun context_t ({ Items = fields }) ->
        let (!) = typeCheckExpr context_t

        let ofSum (p: Sum<'a, Ballerina.Errors.Errors>) =
          p |> Sum.mapRight (Errors.FromErrors loc0) |> state.OfSum

        state {
          let! ctx = state.GetContext()

          let! fields =
            fields
            |> List.map (fun (v) ->
              state {
                let! v, t_v, v_k = !v
                do! v_k |> Kind.AsStar |> ofSum |> state.Ignore
                return v, t_v
              })
            |> state.All

          let fieldsExpr = fields |> List.map fst
          let fieldsTypes = fields |> List.map snd

          let! return_t =
            TypeValue.CreateTuple fieldsTypes
            |> TypeValue.Instantiate loc0
            |> Expr.liftInstantiation

          return Expr.TupleCons(fieldsExpr, loc0, ctx.Types.Scope), return_t, Kind.Star
        }

  type Expr<'T, 'Id when 'Id: comparison> with
    static member internal TypeCheckTypeLet
      (typeCheckExpr: TypeChecker, loc0: Location)
      : TypeChecker<ExprTypeLet<TypeExpr, Identifier>> =
      fun
          context_t
          ({ Name = typeIdentifier
             TypeDef = typeDefinition
             Body = rest }) ->
        let (!) = typeCheckExpr context_t

        let ofSum (p: Sum<'a, Ballerina.Errors.Errors>) =
          p |> Sum.mapRight (Errors.FromErrors loc0) |> state.OfSum

        state {
          let! ctx = state.GetContext()

          let! typeDefinition =
            TypeExpr.Eval (Some(ExprTypeLetBindingName typeIdentifier)) loc0 typeDefinition
            |> Expr.liftTypeEval
            |> state.MapContext(
              TypeCheckContext.Updaters.Types(
                TypeExprEvalContext.Updaters.Scope(TypeCheckScope.Updaters.Type(replaceWith (Some typeIdentifier)))
              )
            )

          let! scope = state.GetContext() |> state.Map(fun ctx -> ctx.Types.Scope)

          do!
            TypeExprEvalState.bindType (typeIdentifier |> Identifier.LocalScope |> scope.Resolve) typeDefinition
            |> Expr.liftTypeEval

          let scope = scope |> TypeCheckScope.Updaters.Type(replaceWith (Some typeIdentifier))

          let bind_component (v, t, k) : Updater<Map<ResolvedIdentifier, (TypeValue * Kind)>> = Map.add v (t, k)

          let! definition_cases =
            typeDefinition
            |> fst
            |> TypeValue.AsUnion
            |> ofSum
            |> state.Catch
            |> state.Map(Sum.toOption)
            |> state.Map(Option.map WithTypeExprSourceMapping.Getters.Value)

          let! bind_definition_cases =
            definition_cases
            |> Option.map (fun definition_cases ->
              definition_cases
              |> OrderedMap.toSeq
              |> Seq.map (fun (k, argT) ->
                state {
                  do!
                    TypeExprEvalState.bindUnionCaseConstructor (k.Name |> scope.Resolve) (argT, definition_cases)
                    |> Expr.liftTypeEval

                  return
                    bind_component (
                      k.Name |> scope.Resolve,
                      TypeValue.CreateRecord(
                        OrderedMap.ofList
                          [ TypeSymbol.Create(Identifier.LocalScope "cons"),
                            TypeValue.CreateArrow(argT, TypeValue.CreateUnion(definition_cases))
                            TypeSymbol.Create(Identifier.LocalScope "map"),
                            TypeValue.CreateArrow(
                              TypeValue.CreateArrow(argT, argT),
                              TypeValue.CreateArrow(
                                TypeValue.CreateUnion(definition_cases),
                                TypeValue.CreateUnion(definition_cases)
                              )
                            ) ]
                      ),
                      //
                      Kind.Star
                    )
                })
              |> state.All)
            |> state.RunOption
            |> state.Map(Option.map (List.fold (>>) id) >> Option.defaultValue id)

          let! definition_fields =
            typeDefinition
            |> fst
            |> TypeValue.AsRecord
            |> ofSum
            |> state.Catch
            |> state.Map(Sum.toOption)
            |> state.Map(Option.map WithTypeExprSourceMapping.Getters.Value)

          let! bind_definition_fields =
            definition_fields
            |> Option.map (fun definition_fields ->
              definition_fields
              |> OrderedMap.toSeq
              |> Seq.map (fun (k, argT) ->
                state {
                  do!
                    TypeExprEvalState.bindRecordField (k.Name |> scope.Resolve) (definition_fields, argT)
                    |> Expr.liftTypeEval

                  return
                    bind_component (
                      k.Name |> scope.Resolve,
                      TypeValue.CreateRecord(
                        OrderedMap.ofList
                          [ TypeSymbol.Create(Identifier.LocalScope "get"),
                            TypeValue.CreateArrow(TypeValue.CreateRecord(definition_fields), argT)
                            TypeSymbol.Create(Identifier.LocalScope "map"),
                            TypeValue.CreateArrow(
                              TypeValue.CreateArrow(argT, argT),
                              TypeValue.CreateArrow(
                                TypeValue.CreateRecord(definition_fields),
                                TypeValue.CreateRecord(definition_fields)
                              )
                            ) ]
                      ),
                      Kind.Star
                    )
                })
              |> state.All)
            |> state.RunOption
            |> state.Map(Option.map (List.fold (>>) id) >> Option.defaultValue id)

          let! rest, rest_t, rest_k =
            !rest
            |> state.MapContext(TypeCheckContext.Updaters.Values(bind_definition_cases >> bind_definition_fields))

          return
            Expr.TypeLet(
              typeIdentifier,
              typeDefinition |> fst,
              rest,
              loc0,
              ctx.Types.Scope
              |> TypeCheckScope.Updaters.Type(replaceWith (Some typeIdentifier))
            ),
            rest_t,
            rest_k
        }

  type Expr<'T, 'Id when 'Id: comparison> with
    static member internal TypeCheckTypeApply
      (typeCheckExpr: TypeChecker, loc0: Location)
      : TypeChecker<ExprTypeApply<TypeExpr, Identifier>> =
      fun context_t ({ Func = fExpr; TypeArg = tExpr }) ->
        let (!) = typeCheckExpr context_t

        let ofSum (p: Sum<'a, Ballerina.Errors.Errors>) =
          p |> Sum.mapRight (Errors.FromErrors loc0) |> state.OfSum

        let error e = Errors.Singleton(loc0, e)

        state {
          let! ctx = state.GetContext()
          let! f, f_t, f_k = !fExpr

          let! f_k_i, f_k_o = f_k |> Kind.AsArrow |> ofSum
          let! t_val, t_k = tExpr |> TypeExpr.Eval None loc0 |> Expr.liftTypeEval

          if f_k_i <> t_k then
            return!
              $"Error: mismatched kind, expected {f_k_i} but got {t_k}"
              |> error
              |> state.Throw
          else
            let! f_res, _ =
              TypeExpr.Apply(f_t.AsExpr, tExpr)
              |> TypeExpr.Eval None loc0
              |> Expr.liftTypeEval

            return Expr.TypeApply(f, t_val, loc0, ctx.Types.Scope), f_res, f_k_o
        }


  type Expr<'T, 'Id when 'Id: comparison> with
    static member TypeCheck: TypeChecker =
      fun context_t t ->
        let loc0 = t.Location

        state {
          match t.Expr with
          | ExprRec.Primitive(p) -> return! Expr.TypeCheckPrimitive (Expr<'T, 'Id>.TypeCheck, loc0) context_t p

          | ExprRec.Lookup({ Id = id }) ->
            return! Expr.TypeCheckLookup (Expr<'T, 'Id>.TypeCheck, loc0) context_t { Id = id }

          | ExprRec.Apply apply -> return! Expr.TypeCheckApply (Expr<'T, 'Id>.TypeCheck, loc0) context_t apply

          | ExprRec.If if_expr -> return! Expr.TypeCheckIf (Expr<'T, 'Id>.TypeCheck, loc0) context_t if_expr

          | ExprRec.Let let_expr -> return! Expr.TypeCheckLet (Expr<'T, 'Id>.TypeCheck, loc0) context_t let_expr

          | ExprRec.Lambda(lambda) ->
            return! Expr<'T, 'Id>.TypeCheckLambda (Expr<'T, 'Id>.TypeCheck, loc0) context_t lambda

          | ExprRec.RecordCons record_cons_expr ->
            return! Expr.TypeCheckRecordCons (Expr<'T, 'Id>.TypeCheck, loc0) context_t record_cons_expr

          | ExprRec.RecordWith record_with_expr ->
            return! Expr.TypeCheckRecordWith (Expr<'T, 'Id>.TypeCheck, loc0) context_t record_with_expr

          | ExprRec.TupleCons tuple_cons_expr ->
            return! Expr.TypeCheckTupleCons (Expr<'T, 'Id>.TypeCheck, loc0) context_t tuple_cons_expr

          | ExprRec.SumCons sum_cons_expr ->
            return! Expr.TypeCheckSumCons (Expr<'T, 'Id>.TypeCheck, loc0) context_t sum_cons_expr

          | ExprRec.RecordDes record_des_expr ->
            return! Expr.TypeCheckRecordDes (Expr<'T, 'Id>.TypeCheck, loc0) context_t record_des_expr

          | ExprRec.TupleDes tuple_des_expr ->
            return! Expr.TypeCheckTupleDes (Expr<'T, 'Id>.TypeCheck, loc0) context_t tuple_des_expr

          | ExprRec.UnionDes union_des_handlers ->
            return! Expr.TypeCheckUnionDes (Expr<'T, 'Id>.TypeCheck, loc0) context_t union_des_handlers

          | ExprRec.SumDes sum_des_expr ->
            return! Expr.TypeCheckSumDes (Expr<'T, 'Id>.TypeCheck, loc0) context_t sum_des_expr

          | ExprRec.TypeLet type_let_expr ->
            return! Expr.TypeCheckTypeLet (Expr<'T, 'Id>.TypeCheck, loc0) context_t type_let_expr

          | ExprRec.TypeLambda type_lambda_expr ->
            return! Expr.TypeCheckTypeLambda (Expr<'T, 'Id>.TypeCheck, loc0) context_t type_lambda_expr

          | ExprRec.TypeApply type_apply_expr ->
            return! Expr.TypeCheckTypeApply (Expr<'T, 'Id>.TypeCheck, loc0) context_t type_apply_expr
        }

namespace Ballerina.DSL.Next.Types.TypeChecker

[<AutoOpen>]
module Expr =
  open Ballerina.StdLib.String
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Collections.Option
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
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
  open Ballerina.DSL.Next.Types.TypeChecker.Do
  open Ballerina.DSL.Next.Types.TypeChecker.RecordCons
  open Ballerina.DSL.Next.Types.TypeChecker.RecordWith
  open Ballerina.DSL.Next.Types.TypeChecker.RecordDes
  open Ballerina.DSL.Next.Types.TypeChecker.UnionDes
  open Ballerina.DSL.Next.Types.TypeChecker.SumDes
  open Ballerina.DSL.Next.Types.TypeChecker.SumCons
  open Ballerina.DSL.Next.Types.TypeChecker.TupleDes
  open Ballerina.DSL.Next.Types.TypeChecker.TupleCons
  open Ballerina.DSL.Next.Types.TypeChecker.TypeLambda
  open Ballerina.DSL.Next.Types.TypeChecker.TypeLet
  open Ballerina.DSL.Next.Types.TypeChecker.TypeApply
  open Ballerina.DSL.Next.Types.TypeChecker.Query
  open Ballerina.DSL.Next.Types.TypeChecker.ErrorDanglingScopedIdentifier
  open Ballerina.DSL.Next.Types.TypeChecker.ErrorDanglingRecordDes
  open Ballerina.Fun
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList

  type Expr<'T, 'Id, 've when 'Id: comparison> with
    static member TypeCheck<'valueExt when 'valueExt: comparison>
      (config: TypeCheckingConfig<'valueExt>)
      : TypeChecker<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>, 'valueExt> =
      fun context_t t ->
        let loc0 = t.Location

        let context_t =
          match context_t with
          | Some(TypeValue.Var v) when v.Synthetic -> None
          | Some ctx -> Some ctx
          | None -> None

        // let (!) = typeCheckExpr context_t
        // let (=>) c e = typeCheckExpr c e

        let typeCheckExpr =
          fun c e ->
            let contextUpdater =
              match e.Expr with
              | ExprRec.Lambda _
              | ExprRec.TypeLambda _ -> id
              | _ ->
                TypeCheckContext.Updaters.IsTypeCheckingLetValue(
                  replaceWith false
                )

            Expr<'T, 'Id, 'valueExt>.TypeCheck config c e
            |> state.MapContext(contextUpdater)

        // let ofSum (p: Sum<'a, Errors<Unit>>) =
        //   p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

        // let error e = Errors.Singleton loc0 e

        state {
          let! expr, ctx =
            state {
              match t.Expr with
              | ExprRec.Primitive(p) ->
                return!
                  Expr.TypeCheckPrimitive
                    (typeCheckExpr, t.Location)
                    context_t
                    p

              | ExprRec.FromValue({ Value = v
                                    ValueType = t_v
                                    ValueKind = k }) ->
                let! ctx = state.GetContext()

                return
                  TypeCheckedExpr.FromValue(v, t_v, k, t.Location, t.Scope), ctx

              | ExprRec.Lookup({ Id = id }) ->
                return!
                  Expr.TypeCheckLookup
                    (typeCheckExpr, t.Location)
                    context_t
                    { Id = id }

              | ExprRec.Apply apply ->
                return! Expr.TypeCheckApply config typeCheckExpr context_t apply
              | ExprRec.If if_expr ->
                return! Expr.TypeCheckIf config typeCheckExpr context_t if_expr

              | ExprRec.Let let_expr ->
                return!
                  Expr.TypeCheckLet
                    config
                    typeCheckExpr
                    context_t
                    (t.Location, let_expr)

              | ExprRec.Do do_expr ->
                return! Expr.TypeCheckDo config typeCheckExpr context_t do_expr

              | ExprRec.Lambda(lambda) ->
                return!
                  Expr<'T, 'Id, 'valueExt>.TypeCheckLambda
                    config
                    typeCheckExpr
                    context_t
                    (t.Location, lambda)
              | ExprRec.RecordCons record_cons_expr ->
                return!
                  Expr.TypeCheckRecordCons
                    config
                    typeCheckExpr
                    context_t
                    record_cons_expr

              | ExprRec.RecordWith record_with_expr ->
                return!
                  Expr.TypeCheckRecordWith
                    config
                    typeCheckExpr
                    context_t
                    record_with_expr

              | ExprRec.TupleCons tuple_cons_expr ->
                return!
                  Expr.TypeCheckTupleCons
                    config
                    typeCheckExpr
                    context_t
                    tuple_cons_expr

              | ExprRec.SumCons sum_cons_expr ->
                return!
                  Expr.TypeCheckSumCons
                    config
                    (typeCheckExpr, t.Location)
                    context_t
                    sum_cons_expr

              | ExprRec.RecordDes record_des_expr ->
                return!
                  Expr.TypeCheckRecordDes
                    typeCheckExpr
                    context_t
                    record_des_expr

              | ExprRec.TupleDes tuple_des_expr ->
                return!
                  Expr.TypeCheckTupleDes typeCheckExpr context_t tuple_des_expr

              | ExprRec.UnionDes union_des_handlers ->
                return!
                  Expr.TypeCheckUnionDes
                    config
                    typeCheckExpr
                    context_t
                    union_des_handlers

              | ExprRec.SumDes sum_des_expr ->
                return!
                  Expr.TypeCheckSumDes
                    config
                    typeCheckExpr
                    context_t
                    sum_des_expr

              | ExprRec.TypeLet type_let_expr ->
                return!
                  Expr.TypeCheckTypeLet
                    config
                    typeCheckExpr
                    context_t
                    type_let_expr

              | ExprRec.TypeLambda type_lambda_expr ->
                return!
                  Expr.TypeCheckTypeLambda
                    typeCheckExpr
                    context_t
                    type_lambda_expr

              | ExprRec.TypeApply type_apply_expr ->
                return!
                  Expr.TypeCheckTypeApply
                    config
                    typeCheckExpr
                    context_t
                    type_apply_expr

              | ExprRec.EntityDes _
              | ExprRec.RelationDes _
              | ExprRec.EntitiesDes _
              | ExprRec.RelationsDes _
              | ExprRec.RelationLookupDes _ ->
                return!
                  Errors.Singleton loc0 (fun () ->
                    $"Error: unexpected expression pattern schema entity and entities (should not occur, are only constructed as record destructuring) ")
                  |> state.Throw
              | ExprRec.Query q ->
                let! q, t, k, ctx =
                  Expr.TypeCheckQuery
                    config
                    typeCheckExpr
                    context_t
                    Map.empty
                    Map.empty
                    q

                return TypeCheckedExpr.Query(q, t, k), ctx

              | ExprRec.RecoveredSyntaxError err ->
                return!
                  Errors.Singleton err.ErrorLocation (fun () -> err.ErrorMessage)
                  |> state.Throw

              | ExprRec.ErrorDanglingRecordDes({ Expr = record_expr; Field = _field }) ->
                return!
                  Expr.TypeCheckErrorDanglingRecordDes
                    typeCheckExpr
                    context_t
                    record_expr
                    loc0

              | ExprRec.ErrorDanglingScopedIdentifier({ PrefixParts = prefixParts }) ->
                return!
                  Expr.TypeCheckErrorDanglingScopedIdentifier
                    prefixParts
                    loc0
            }

          let! expr =
            expr
            |> TypeCheckedExpr.InstantiateSyntheticVars config typeCheckExpr

          return expr, ctx
        }

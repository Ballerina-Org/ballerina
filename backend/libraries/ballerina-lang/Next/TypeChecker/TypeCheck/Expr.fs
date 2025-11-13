namespace Ballerina.DSL.Next.Types.TypeChecker

[<AutoOpen>]
module Expr =
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
  open Ballerina.DSL.Next.Types.TypeChecker.ApplyValue
  open Ballerina.DSL.Next.Types.TypeChecker.If
  open Ballerina.DSL.Next.Types.TypeChecker.Let
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
  open Ballerina.Fun
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList

  type Expr<'T, 'Id, 'valueExt when 'Id: comparison> with
    static member TypeCheck: ExprTypeChecker<'valueExt> =
      fun context_t t ->
        let loc0 = t.Location

        // let (!) = typeCheckExpr context_t
        // let (=>) c e = typeCheckExpr c e

        // let ofSum (p: Sum<'a, Ballerina.Errors.Errors>) =
        //   p |> Sum.mapRight (Errors.FromErrors loc0) |> state.OfSum

        // let error e = Errors.Singleton(loc0, e)

        state {
          match t.Expr with
          | ExprRec.Primitive(p) ->
            return! Expr.TypeCheckPrimitive (Expr<'T, 'Id, 'valueExt>.TypeCheck, loc0) context_t p

          | ExprRec.Lookup({ Id = id }) ->
            return! Expr.TypeCheckLookup (Expr<'T, 'Id, 'valueExt>.TypeCheck, loc0) context_t { Id = id }

          | ExprRec.Apply apply ->
            return! Expr.TypeCheckApply (Expr<'T, 'Id, 'valueExt>.TypeCheck, loc0) context_t apply
          | ExprRec.ApplyValue apply ->
            return! Expr.TypeCheckApplyValue (Expr<'T, 'Id, 'valueExt>.TypeCheck, loc0) context_t apply

          | ExprRec.If if_expr -> return! Expr.TypeCheckIf (Expr<'T, 'Id, 'valueExt>.TypeCheck, loc0) context_t if_expr

          | ExprRec.Let let_expr ->
            return! Expr.TypeCheckLet (Expr<'T, 'Id, 'valueExt>.TypeCheck, loc0) context_t let_expr

          | ExprRec.Lambda(lambda) ->
            return! Expr<'T, 'Id, 'valueExt>.TypeCheckLambda (Expr<'T, 'Id, 'valueExt>.TypeCheck, loc0) context_t lambda

          | ExprRec.RecordCons record_cons_expr ->
            return! Expr.TypeCheckRecordCons (Expr<'T, 'Id, 'valueExt>.TypeCheck, loc0) context_t record_cons_expr

          | ExprRec.RecordWith record_with_expr ->
            return! Expr.TypeCheckRecordWith (Expr<'T, 'Id, 'valueExt>.TypeCheck, loc0) context_t record_with_expr

          | ExprRec.TupleCons tuple_cons_expr ->
            return! Expr.TypeCheckTupleCons (Expr<'T, 'Id, 'valueExt>.TypeCheck, loc0) context_t tuple_cons_expr

          | ExprRec.SumCons sum_cons_expr ->
            return! Expr.TypeCheckSumCons (Expr<'T, 'Id, 'valueExt>.TypeCheck, loc0) context_t sum_cons_expr

          | ExprRec.RecordDes record_des_expr ->
            return! Expr.TypeCheckRecordDes (Expr<'T, 'Id, 'valueExt>.TypeCheck, loc0) context_t record_des_expr

          | ExprRec.TupleDes tuple_des_expr ->
            return! Expr.TypeCheckTupleDes (Expr<'T, 'Id, 'valueExt>.TypeCheck, loc0) context_t tuple_des_expr

          | ExprRec.UnionDes union_des_handlers ->
            return! Expr.TypeCheckUnionDes (Expr<'T, 'Id, 'valueExt>.TypeCheck, loc0) context_t union_des_handlers

          | ExprRec.SumDes sum_des_expr ->
            return! Expr.TypeCheckSumDes (Expr<'T, 'Id, 'valueExt>.TypeCheck, loc0) context_t sum_des_expr

          | ExprRec.TypeLet type_let_expr ->
            return! Expr.TypeCheckTypeLet (Expr<'T, 'Id, 'valueExt>.TypeCheck, loc0) context_t type_let_expr

          | ExprRec.TypeLambda type_lambda_expr ->
            return! Expr.TypeCheckTypeLambda (Expr<'T, 'Id, 'valueExt>.TypeCheck, loc0) context_t type_lambda_expr

          | ExprRec.TypeApply type_apply_expr ->
            return! Expr.TypeCheckTypeApply (Expr<'T, 'Id, 'valueExt>.TypeCheck, loc0) context_t type_apply_expr
        }

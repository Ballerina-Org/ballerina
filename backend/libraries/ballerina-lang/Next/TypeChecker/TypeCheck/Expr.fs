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
  open Ballerina.Fun
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList

  type Expr<'T, 'Id, 've when 'Id: comparison> with
    static member TypeCheck<'valueExt when 'valueExt: comparison>
      (
        query_type_symbol: TypeSymbol,
        mk_query_type: Schema<'valueExt> -> TypeQueryRow<'valueExt> -> TypeValue<'valueExt>
      ) : TypeChecker<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>, 'valueExt> =
      fun context_t t ->
        let loc0 = t.Location

        let context_t =
          match context_t with
          | Some(TypeValue.Var v) when v.Synthetic -> None
          | Some ctx -> Some ctx
          | None -> None

        // let (!) = typeCheckExpr context_t
        // let (=>) c e = typeCheckExpr c e

        // let ofSum (p: Sum<'a, Errors<Unit>>) =
        //   p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

        // let error e = Errors.Singleton loc0 e

        state {
          let! expr, typeValue, kind, ctx =
            state {
              match t.Expr with
              | ExprRec.Primitive(p) ->
                return!
                  Expr.TypeCheckPrimitive
                    (Expr<'T, 'Id, 'valueExt>.TypeCheck(query_type_symbol, mk_query_type), loc0)
                    context_t
                    p

              | ExprRec.FromValue({ Value = v
                                    ValueType = t_v
                                    ValueKind = k }) ->
                let! ctx = state.GetContext()
                return Expr.FromValue(v, t_v, k, loc0, t.Scope), t_v, k, ctx

              | ExprRec.Lookup({ Id = id }) ->
                return!
                  Expr.TypeCheckLookup
                    (Expr<'T, 'Id, 'valueExt>.TypeCheck(query_type_symbol, mk_query_type), loc0)
                    context_t
                    { Id = id }

              | ExprRec.Apply apply ->
                return!
                  Expr.TypeCheckApply
                    (query_type_symbol, mk_query_type)
                    (Expr<'T, 'Id, 'valueExt>.TypeCheck(query_type_symbol, mk_query_type), loc0)
                    context_t
                    apply
              | ExprRec.If if_expr ->
                return!
                  Expr.TypeCheckIf
                    (query_type_symbol, mk_query_type)
                    (Expr<'T, 'Id, 'valueExt>.TypeCheck(query_type_symbol, mk_query_type), loc0)
                    context_t
                    if_expr

              | ExprRec.Let let_expr ->
                return!
                  Expr.TypeCheckLet
                    (query_type_symbol, mk_query_type)
                    (Expr<'T, 'Id, 'valueExt>.TypeCheck(query_type_symbol, mk_query_type), loc0)
                    context_t
                    let_expr

              | ExprRec.Do do_expr ->
                return!
                  Expr.TypeCheckDo
                    (query_type_symbol, mk_query_type)
                    (Expr<'T, 'Id, 'valueExt>.TypeCheck(query_type_symbol, mk_query_type), loc0)
                    context_t
                    do_expr

              | ExprRec.Lambda(lambda) ->
                return!
                  Expr<'T, 'Id, 'valueExt>.TypeCheckLambda
                    (query_type_symbol, mk_query_type)
                    (Expr<'T, 'Id, 'valueExt>.TypeCheck(query_type_symbol, mk_query_type), loc0)
                    context_t
                    lambda
              | ExprRec.RecordCons record_cons_expr ->
                return!
                  Expr.TypeCheckRecordCons
                    (query_type_symbol, mk_query_type)
                    (Expr<'T, 'Id, 'valueExt>.TypeCheck(query_type_symbol, mk_query_type), loc0)
                    context_t
                    record_cons_expr

              | ExprRec.RecordWith record_with_expr ->
                return!
                  Expr.TypeCheckRecordWith
                    (query_type_symbol, mk_query_type)
                    (Expr<'T, 'Id, 'valueExt>.TypeCheck(query_type_symbol, mk_query_type), loc0)
                    context_t
                    record_with_expr

              | ExprRec.TupleCons tuple_cons_expr ->
                return!
                  Expr.TypeCheckTupleCons
                    (query_type_symbol, mk_query_type)
                    (Expr<'T, 'Id, 'valueExt>.TypeCheck(query_type_symbol, mk_query_type), loc0)
                    context_t
                    tuple_cons_expr

              | ExprRec.SumCons sum_cons_expr ->
                return!
                  Expr.TypeCheckSumCons
                    (query_type_symbol, mk_query_type)
                    (Expr<'T, 'Id, 'valueExt>.TypeCheck(query_type_symbol, mk_query_type), loc0)
                    context_t
                    sum_cons_expr

              | ExprRec.RecordDes record_des_expr ->
                return!
                  Expr.TypeCheckRecordDes
                    (Expr<'T, 'Id, 'valueExt>.TypeCheck(query_type_symbol, mk_query_type), loc0)
                    context_t
                    record_des_expr

              | ExprRec.TupleDes tuple_des_expr ->
                return!
                  Expr.TypeCheckTupleDes
                    (Expr<'T, 'Id, 'valueExt>.TypeCheck(query_type_symbol, mk_query_type), loc0)
                    context_t
                    tuple_des_expr

              | ExprRec.UnionDes union_des_handlers ->
                return!
                  Expr.TypeCheckUnionDes
                    (query_type_symbol, mk_query_type)
                    (Expr<'T, 'Id, 'valueExt>.TypeCheck(query_type_symbol, mk_query_type), loc0)
                    context_t
                    union_des_handlers

              | ExprRec.SumDes sum_des_expr ->
                return!
                  Expr.TypeCheckSumDes
                    (query_type_symbol, mk_query_type)
                    (Expr<'T, 'Id, 'valueExt>.TypeCheck(query_type_symbol, mk_query_type), loc0)
                    context_t
                    sum_des_expr

              | ExprRec.TypeLet type_let_expr ->
                return!
                  Expr.TypeCheckTypeLet
                    (query_type_symbol, mk_query_type)
                    (Expr<'T, 'Id, 'valueExt>.TypeCheck(query_type_symbol, mk_query_type), loc0)
                    context_t
                    type_let_expr

              | ExprRec.TypeLambda type_lambda_expr ->
                return!
                  Expr.TypeCheckTypeLambda
                    (Expr<'T, 'Id, 'valueExt>.TypeCheck(query_type_symbol, mk_query_type), loc0)
                    context_t
                    type_lambda_expr

              | ExprRec.TypeApply type_apply_expr ->
                return!
                  Expr.TypeCheckTypeApply
                    (query_type_symbol, mk_query_type)
                    (Expr<'T, 'Id, 'valueExt>.TypeCheck(query_type_symbol, mk_query_type), loc0)
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
                    query_type_symbol
                    mk_query_type
                    (Expr<'T, 'Id, 'valueExt>.TypeCheck(query_type_symbol, mk_query_type), loc0)
                    context_t
                    Map.empty
                    Map.empty
                    q

                return Expr.Query q, t, k, ctx
            }

          let! expr =
            expr
            |> Expr.InstantiateSyntheticVars
              (query_type_symbol, mk_query_type)
              (Expr<'T, 'Id, 'valueExt>.TypeCheck(query_type_symbol, mk_query_type))

          return expr, typeValue, kind, ctx
        }

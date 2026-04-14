namespace Ballerina.DSL.Next.Types.TypeChecker

open Ballerina
open Ballerina.Collections
open Ballerina.Collections.NonEmptyList
open Ballerina.DSL.Next.Types.Model

[<RequireQualifiedAccess>]
module Conversion =

  let rec private convertCaseHandler<'valueExt when 'valueExt: comparison>
    ((v, body): TypeCheckedCaseHandler<'valueExt>)
    : RunnableCaseHandler<'valueExt>
    =
    v, convertExpression body

  and private convertExprRec<'valueExt when 'valueExt: comparison>
    (expr: TypeCheckedExprRec<'valueExt>)
    : RunnableExprRec<'valueExt>
    =
    match expr with
    | TypeCheckedExprRec.Primitive v -> RunnableExprRec.Primitive v
    | TypeCheckedExprRec.Lookup l -> RunnableExprRec.Lookup { Id = l.Id }
    | TypeCheckedExprRec.TypeLambda tl ->
      RunnableExprRec.TypeLambda { Param = tl.Param; Body = convertExpression tl.Body }
    | TypeCheckedExprRec.TypeApply ta ->
      RunnableExprRec.TypeApply { Func = convertExpression ta.Func; TypeArg = ta.TypeArg }
    | TypeCheckedExprRec.TypeLet tl ->
      RunnableExprRec.TypeLet { Name = tl.Name; TypeDef = tl.TypeDef; Body = convertExpression tl.Body }
    | TypeCheckedExprRec.Lambda l ->
      RunnableExprRec.Lambda { Param = l.Param; ParamType = l.ParamType; Body = convertExpression l.Body; BodyType = l.BodyType }
    | TypeCheckedExprRec.FromValue fv ->
      RunnableExprRec.FromValue { Value = fv.Value; ValueType = fv.ValueType; ValueKind = fv.ValueKind }
    | TypeCheckedExprRec.Apply app ->
      RunnableExprRec.Apply { F = convertExpression app.F; Arg = convertExpression app.Arg }
    | TypeCheckedExprRec.Let l ->
      RunnableExprRec.Let { Var = l.Var; Type = l.Type; Val = convertExpression l.Val; Rest = convertExpression l.Rest }
    | TypeCheckedExprRec.Do d ->
      RunnableExprRec.Do { Val = convertExpression d.Val; Rest = convertExpression d.Rest }
    | TypeCheckedExprRec.If i ->
      RunnableExprRec.If { Cond = convertExpression i.Cond; Then = convertExpression i.Then; Else = convertExpression i.Else }
    | TypeCheckedExprRec.RecordCons rc ->
      RunnableExprRec.RecordCons { Fields = rc.Fields |> List.map (fun (k, v) -> k, convertExpression v) }
    | TypeCheckedExprRec.RecordWith rw ->
      RunnableExprRec.RecordWith { Record = convertExpression rw.Record; Fields = rw.Fields |> List.map (fun (k, v) -> k, convertExpression v) }
    | TypeCheckedExprRec.TupleCons tc ->
      RunnableExprRec.TupleCons { Items = tc.Items |> List.map convertExpression }
    | TypeCheckedExprRec.SumCons sc ->
      RunnableExprRec.SumCons { Selector = sc.Selector }
    | TypeCheckedExprRec.RecordDes rd ->
      RunnableExprRec.RecordDes { Expr = convertExpression rd.Expr; Field = rd.Field }
    | TypeCheckedExprRec.EntitiesDes ed ->
      RunnableExprRec.EntitiesDes { Expr = convertExpression ed.Expr }
    | TypeCheckedExprRec.RelationsDes rd ->
      RunnableExprRec.RelationsDes { Expr = convertExpression rd.Expr }
    | TypeCheckedExprRec.EntityDes ed ->
      RunnableExprRec.EntityDes { Expr = convertExpression ed.Expr; EntityName = ed.EntityName }
    | TypeCheckedExprRec.RelationDes rd ->
      RunnableExprRec.RelationDes { Expr = convertExpression rd.Expr; RelationName = rd.RelationName }
    | TypeCheckedExprRec.RelationLookupDes rd ->
      RunnableExprRec.RelationLookupDes { Expr = convertExpression rd.Expr; RelationName = rd.RelationName; Direction = rd.Direction }
    | TypeCheckedExprRec.UnionDes ud ->
      RunnableExprRec.UnionDes
        { Handlers = ud.Handlers |> Map.map (fun _ h -> convertCaseHandler h)
          Fallback = ud.Fallback |> Option.map convertExpression }
    | TypeCheckedExprRec.TupleDes td ->
      RunnableExprRec.TupleDes { Tuple = convertExpression td.Tuple; Item = td.Item }
    | TypeCheckedExprRec.SumDes sd ->
      RunnableExprRec.SumDes { Handlers = sd.Handlers |> Map.map (fun _ h -> convertCaseHandler h) }
    | TypeCheckedExprRec.Query q -> RunnableExprRec.Query(convertQuery q)
    | TypeCheckedExprRec.RecoveredSyntaxError err ->
      failwith $"Cannot convert RecoveredSyntaxError to RunnableExpr: {err}"

  and private convertQueryCaseHandler<'valueExt when 'valueExt: comparison>
    (h: TypeCheckedQueryCaseHandler<'valueExt>)
    : RunnableQueryCaseHandler<'valueExt>
    =
    { Param = h.Param; Body = convertQueryExpr h.Body }

  and private convertQueryExpr<'valueExt when 'valueExt: comparison>
    (e: TypeCheckedExprQueryExpr<'valueExt>)
    : RunnableExprQueryExpr<'valueExt>
    =
    { Location = e.Location; Expr = convertQueryExprRec e.Expr }

  and private convertQueryExprRec<'valueExt when 'valueExt: comparison>
    (expr: TypeCheckedExprQueryExprRec<'valueExt>)
    : RunnableExprQueryExprRec<'valueExt>
    =
    match expr with
    | TypeCheckedExprQueryExprRec.QueryTupleCons items ->
      RunnableExprQueryExprRec.QueryTupleCons(items |> List.map convertQueryExpr)
    | TypeCheckedExprQueryExprRec.QueryRecordDes(e, field, isJson) ->
      RunnableExprQueryExprRec.QueryRecordDes(convertQueryExpr e, field, isJson)
    | TypeCheckedExprQueryExprRec.QueryTupleDes(e, item, isJson) ->
      RunnableExprQueryExprRec.QueryTupleDes(convertQueryExpr e, item, isJson)
    | TypeCheckedExprQueryExprRec.QueryConditional(cond, ``then``, ``else``) ->
      RunnableExprQueryExprRec.QueryConditional(convertQueryExpr cond, convertQueryExpr ``then``, convertQueryExpr ``else``)
    | TypeCheckedExprQueryExprRec.QueryUnionDes(e, handlers) ->
      RunnableExprQueryExprRec.QueryUnionDes(convertQueryExpr e, handlers |> Map.map (fun _ h -> convertQueryCaseHandler h))
    | TypeCheckedExprQueryExprRec.QuerySumDes(e, handlers) ->
      RunnableExprQueryExprRec.QuerySumDes(convertQueryExpr e, handlers |> Map.map (fun _ h -> convertQueryCaseHandler h))
    | TypeCheckedExprQueryExprRec.QueryApply(func, arg) ->
      RunnableExprQueryExprRec.QueryApply(convertQueryExpr func, convertQueryExpr arg)
    | TypeCheckedExprQueryExprRec.QueryLookup id -> RunnableExprQueryExprRec.QueryLookup id
    | TypeCheckedExprQueryExprRec.QueryIntrinsic(i, t) -> RunnableExprQueryExprRec.QueryIntrinsic(i, t)
    | TypeCheckedExprQueryExprRec.QueryConstant v -> RunnableExprQueryExprRec.QueryConstant v
    | TypeCheckedExprQueryExprRec.QueryClosureValue(v, t) -> RunnableExprQueryExprRec.QueryClosureValue(v, t)
    | TypeCheckedExprQueryExprRec.QueryCastTo(e, t) -> RunnableExprQueryExprRec.QueryCastTo(convertQueryExpr e, t)
    | TypeCheckedExprQueryExprRec.QueryCount q -> RunnableExprQueryExprRec.QueryCount(convertQuery q)
    | TypeCheckedExprQueryExprRec.QueryExists q -> RunnableExprQueryExprRec.QueryExists(convertQuery q)
    | TypeCheckedExprQueryExprRec.QueryArray q -> RunnableExprQueryExprRec.QueryArray(convertQuery q)
    | TypeCheckedExprQueryExprRec.QueryCountEvaluated v -> RunnableExprQueryExprRec.QueryCountEvaluated v
    | TypeCheckedExprQueryExprRec.QueryExistsEvaluated v -> RunnableExprQueryExprRec.QueryExistsEvaluated v
    | TypeCheckedExprQueryExprRec.QueryArrayEvaluated v -> RunnableExprQueryExprRec.QueryArrayEvaluated v
    | TypeCheckedExprQueryExprRec.QueryRecoveredSyntaxError err ->
      failwith $"Cannot convert QueryRecoveredSyntaxError to RunnableExprQueryExprRec: {err}"

  and convertQuery<'valueExt when 'valueExt: comparison>
    (q: TypeCheckedExprQuery<'valueExt>)
    : RunnableExprQuery<'valueExt>
    =
    match q with
    | TypeCheckedExprQuery.SimpleQuery sq ->
      RunnableExprQuery.SimpleQuery
        { RunnableSimpleQuery.Iterators =
            sq.Iterators
            |> NonEmptyList.map (fun it ->
              { RunnableExprQueryIterator.Location = it.Location
                Var = it.Var
                VarType = it.VarType
                Source = convertExpression it.Source })
          Joins =
            sq.Joins
            |> Option.map (NonEmptyList.map (fun j ->
              { RunnableExprQueryJoin.Location = j.Location
                Left = convertQueryExpr j.Left
                Right = convertQueryExpr j.Right }))
          Where = sq.Where |> Option.map convertQueryExpr
          Select = convertQueryExpr sq.Select
          OrderBy = sq.OrderBy |> Option.map (fun (e, dir) -> convertQueryExpr e, dir)
          Closure = sq.Closure
          DeserializeFrom = sq.DeserializeFrom
          Distinct = sq.Distinct |> Option.map convertQueryExpr }
    | TypeCheckedExprQuery.UnionQueries(q1, q2) ->
      RunnableExprQuery.UnionQueries(convertQuery q1, convertQuery q2)

  and convertExpression<'valueExt when 'valueExt: comparison>
    (expr: TypeCheckedExpr<'valueExt>)
    : RunnableExpr<'valueExt>
    =
    { Expr = convertExprRec expr.Expr
      Location = expr.Location
      Type = expr.Type
      Kind = expr.Kind
      Scope = expr.Scope }

  let convertExpressionOption<'valueExt when 'valueExt: comparison>
    (expr: Option<TypeCheckedExpr<'valueExt>>)
    : Option<RunnableExpr<'valueExt>>
    =
    expr |> Option.map convertExpression

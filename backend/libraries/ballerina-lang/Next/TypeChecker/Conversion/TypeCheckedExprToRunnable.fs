namespace Ballerina.DSL.Next.Types.TypeChecker

open Ballerina
open Ballerina.Collections
open Ballerina.Collections.Sum
open Ballerina.Collections.NonEmptyList
open Ballerina.DSL.Next.Types.Model
open Ballerina.Errors
open Ballerina.LocalizedErrors

[<RequireQualifiedAccess>]
module Conversion =

  let private conversionError (loc: Location) (msg: string) : Errors<Location> =
    Errors.Singleton loc (fun () -> msg)

  let rec private convertCaseHandler<'valueExt when 'valueExt: comparison>
    ((v, body): TypeCheckedCaseHandler<'valueExt>)
    : Sum<RunnableCaseHandler<'valueExt>, Errors<Location>>
    =
    sum {
      let! body' = convertExpression body
      return v, body'
    }

  and private convertExprRec<'valueExt when 'valueExt: comparison>
    (_loc: Location)
    (expr: TypeCheckedExprRec<'valueExt>)
    : Sum<RunnableExprRec<'valueExt>, Errors<Location>>
    =
    match expr with
    | TypeCheckedExprRec.Primitive v -> Left(RunnableExprRec.Primitive v)
    | TypeCheckedExprRec.Lookup l -> Left(RunnableExprRec.Lookup { Id = l.Id })
    | TypeCheckedExprRec.TypeLambda tl ->
      sum {
        let! body = convertExpression tl.Body
        return RunnableExprRec.TypeLambda { Param = tl.Param; Body = body }
      }
    | TypeCheckedExprRec.TypeApply ta ->
      sum {
        let! func = convertExpression ta.Func
        return RunnableExprRec.TypeApply { Func = func; TypeArg = ta.TypeArg }
      }
    | TypeCheckedExprRec.TypeLet tl ->
      sum {
        let! body = convertExpression tl.Body
        return RunnableExprRec.TypeLet { Name = tl.Name; TypeDef = tl.TypeDef; Body = body }
      }
    | TypeCheckedExprRec.Lambda l ->
      sum {
        let! body = convertExpression l.Body
        return RunnableExprRec.Lambda { Param = l.Param; Body = body }
      }
    | TypeCheckedExprRec.FromValue fv ->
      Left(RunnableExprRec.FromValue { Value = fv.Value; ValueType = fv.ValueType; ValueKind = fv.ValueKind })
    | TypeCheckedExprRec.Apply app ->
      sum {
        let! f, arg = sum.All2 (convertExpression app.F) (convertExpression app.Arg)
        return RunnableExprRec.Apply { F = f; Arg = arg }
      }
    | TypeCheckedExprRec.Let l ->
      sum {
        let! v, rest = sum.All2 (convertExpression l.Val) (convertExpression l.Rest)
        return RunnableExprRec.Let { Var = l.Var; Type = l.Type; Val = v; Rest = rest }
      }
    | TypeCheckedExprRec.Do d ->
      sum {
        let! v, rest = sum.All2 (convertExpression d.Val) (convertExpression d.Rest)
        return RunnableExprRec.Do { Val = v; Rest = rest }
      }
    | TypeCheckedExprRec.If i ->
      sum {
        let! cond, thenE, elseE = sum.All3 (convertExpression i.Cond) (convertExpression i.Then) (convertExpression i.Else)
        return RunnableExprRec.If { Cond = cond; Then = thenE; Else = elseE }
      }
    | TypeCheckedExprRec.RecordCons rc ->
      sum {
        let! fields = rc.Fields |> List.map (fun (k, v) -> convertExpression v |> Sum.map (fun v' -> k, v')) |> sum.All
        return RunnableExprRec.RecordCons { Fields = fields }
      }
    | TypeCheckedExprRec.RecordWith rw ->
      sum {
        let! record = convertExpression rw.Record
        let! fields = rw.Fields |> List.map (fun (k, v) -> convertExpression v |> Sum.map (fun v' -> k, v')) |> sum.All
        return RunnableExprRec.RecordWith { Record = record; Fields = fields }
      }
    | TypeCheckedExprRec.TupleCons tc ->
      sum {
        let! items = tc.Items |> List.map convertExpression |> sum.All
        return RunnableExprRec.TupleCons { Items = items }
      }
    | TypeCheckedExprRec.SumCons sc ->
      Left(RunnableExprRec.SumCons { Selector = sc.Selector })
    | TypeCheckedExprRec.RecordDes rd ->
      sum {
        let! expr = convertExpression rd.Expr
        return RunnableExprRec.RecordDes { Expr = expr; Field = rd.Field }
      }
    | TypeCheckedExprRec.EntitiesDes ed ->
      sum {
        let! expr = convertExpression ed.Expr
        return RunnableExprRec.EntitiesDes { Expr = expr }
      }
    | TypeCheckedExprRec.RelationsDes rd ->
      sum {
        let! expr = convertExpression rd.Expr
        return RunnableExprRec.RelationsDes { Expr = expr }
      }
    | TypeCheckedExprRec.EntityDes ed ->
      sum {
        let! expr = convertExpression ed.Expr
        return RunnableExprRec.EntityDes { Expr = expr; EntityName = ed.EntityName }
      }
    | TypeCheckedExprRec.RelationDes rd ->
      sum {
        let! expr = convertExpression rd.Expr
        return RunnableExprRec.RelationDes { Expr = expr; RelationName = rd.RelationName }
      }
    | TypeCheckedExprRec.RelationLookupDes rd ->
      sum {
        let! expr = convertExpression rd.Expr
        return RunnableExprRec.RelationLookupDes { Expr = expr; RelationName = rd.RelationName; Direction = rd.Direction }
      }
    | TypeCheckedExprRec.UnionDes ud ->
      sum {
        let! handlers = ud.Handlers |> Map.map (fun _ h -> convertCaseHandler h) |> sum.AllMap
        let! fallback = ud.Fallback |> Option.map convertExpression |> sum.RunOption
        return RunnableExprRec.UnionDes { Handlers = handlers; Fallback = fallback }
      }
    | TypeCheckedExprRec.TupleDes td ->
      sum {
        let! tuple = convertExpression td.Tuple
        return RunnableExprRec.TupleDes { Tuple = tuple; Item = td.Item }
      }
    | TypeCheckedExprRec.SumDes sd ->
      sum {
        let! handlers = sd.Handlers |> Map.map (fun _ h -> convertCaseHandler h) |> sum.AllMap
        return RunnableExprRec.SumDes { Handlers = handlers }
      }
    | TypeCheckedExprRec.Query q ->
      sum {
        let! q' = convertQuery q
        return RunnableExprRec.Query q'
      }
    | TypeCheckedExprRec.View v ->
      sum {
        let! body = convertViewNode v.Body
        return RunnableExprRec.View { Param = v.Param; ParamType = v.ParamType; Body = body; Location = v.Location }
      }
    | TypeCheckedExprRec.Co co ->
      sum {
        let! body = convertCoStep co.Body
        return RunnableExprRec.Co { Body = body; Location = co.Location }
      }
    | TypeCheckedExprRec.CoOp op -> Left(RunnableExprRec.CoOp op)
    | TypeCheckedExprRec.ViewOp op -> Left(RunnableExprRec.ViewOp op)
    | TypeCheckedExprRec.RecoveredSyntaxError err ->
      Right(conversionError err.ErrorLocation $"Cannot convert RecoveredSyntaxError to RunnableExpr: {err.ErrorMessage} (context: {err.RecoveryContext})")
    | TypeCheckedExprRec.ErrorDanglingRecordDes _ ->
      Right(conversionError _loc $"Cannot convert ErrorDanglingRecordDes to RunnableExpr")
    | TypeCheckedExprRec.ErrorDanglingScopedIdentifier _ ->
      Right(conversionError _loc $"Cannot convert ErrorDanglingScopedIdentifier to RunnableExpr")
    | TypeCheckedExprRec.ErrorRecordDesButInvalidField _ ->
      Right(conversionError _loc $"Cannot convert ErrorRecordDesButInvalidField to RunnableExpr")

  and private convertViewNode<'valueExt when 'valueExt: comparison>
    (node: TypeCheckedViewNode<'valueExt>)
    : Sum<RunnableViewNode<'valueExt>, Errors<Location>>
    =
    sum {
      let! nodeRec = convertViewNodeRec node.Node
      return { Location = node.Location; Node = nodeRec }
    }

  and private convertViewNodeRec<'valueExt when 'valueExt: comparison>
    (node: TypeCheckedViewNodeRec<'valueExt>)
    : Sum<RunnableViewNodeRec<'valueExt>, Errors<Location>>
    =
    match node with
    | TypeCheckedViewNodeRec.ViewLet(var, value, rest) ->
      sum {
        let! value', rest' = sum.All2 (convertExpression value) (convertViewNode rest)
        return RunnableViewNodeRec.ViewLet(var, value', rest')
      }
    | TypeCheckedViewNodeRec.ViewElement el ->
      sum {
        let! el' = convertViewElement el
        return RunnableViewNodeRec.ViewElement el'
      }
    | TypeCheckedViewNodeRec.ViewFragment children ->
      sum {
        let! children' = children |> List.map convertViewNode |> sum.All
        return RunnableViewNodeRec.ViewFragment children'
      }
    | TypeCheckedViewNodeRec.ViewExprContainer e ->
      sum {
        let! e' = convertExpression e
        return RunnableViewNodeRec.ViewExprContainer e'
      }
    | TypeCheckedViewNodeRec.ViewText t ->
      Left(RunnableViewNodeRec.ViewText t)
    | TypeCheckedViewNodeRec.ViewMapContext(mapper, inner) ->
      sum {
        let! mapper', inner' = sum.All2 (convertExpression mapper) (convertExpression inner)
        return RunnableViewNodeRec.ViewMapContext(mapper', inner')
      }
    | TypeCheckedViewNodeRec.ViewMapState(mapDown, mapUp, inner) ->
      sum {
        let! mapDown', mapUp', inner' = sum.All3 (convertExpression mapDown) (convertExpression mapUp) (convertExpression inner)
        return RunnableViewNodeRec.ViewMapState(mapDown', mapUp', inner')
      }

  and private convertViewElement<'valueExt when 'valueExt: comparison>
    (el: TypeCheckedViewElement<'valueExt>)
    : Sum<RunnableViewElement<'valueExt>, Errors<Location>>
    =
    sum {
      let! attrs = el.Attributes |> List.map convertViewAttribute |> sum.All
      let! children = el.Children |> List.map convertViewNode |> sum.All
      return { Tag = el.Tag; Attributes = attrs; Children = children; SelfClosing = el.SelfClosing }
    }

  and private convertViewAttribute<'valueExt when 'valueExt: comparison>
    (attr: TypeCheckedViewAttribute<'valueExt>)
    : Sum<RunnableViewAttribute<'valueExt>, Errors<Location>>
    =
    match attr with
    | TypeCheckedViewAttribute.ViewAttrStringValue(name, value) ->
      Left(RunnableViewAttribute.ViewAttrStringValue(name, value))
    | TypeCheckedViewAttribute.ViewAttrExprValue(name, value) ->
      sum {
        let! value' = convertExpression value
        return RunnableViewAttribute.ViewAttrExprValue(name, value')
      }

  and private convertCoStep<'valueExt when 'valueExt: comparison>
    (step: TypeCheckedCoStep<'valueExt>)
    : Sum<RunnableCoStep<'valueExt>, Errors<Location>>
    =
    sum {
      let! stepRec = convertCoStepRec step.Step
      return { Location = step.Location; Step = stepRec }
    }

  and private convertCoStepRec<'valueExt when 'valueExt: comparison>
    (step: TypeCheckedCoStepRec<'valueExt>)
    : Sum<RunnableCoStepRec<'valueExt>, Errors<Location>>
    =
    match step with
    | TypeCheckedCoStepRec.CoLetBang(var, value, rest) ->
      sum {
        let! value', rest' = sum.All2 (convertExpression value) (convertCoStep rest)
        return RunnableCoStepRec.CoLetBang(var, value', rest')
      }
    | TypeCheckedCoStepRec.CoLet(var, value, rest) ->
      sum {
        let! value', rest' = sum.All2 (convertExpression value) (convertCoStep rest)
        return RunnableCoStepRec.CoLet(var, value', rest')
      }
    | TypeCheckedCoStepRec.CoDoBang(value, rest) ->
      sum {
        let! value', rest' = sum.All2 (convertExpression value) (convertCoStep rest)
        return RunnableCoStepRec.CoDoBang(value', rest')
      }
    | TypeCheckedCoStepRec.CoReturn e ->
      sum {
        let! e' = convertExpression e
        return RunnableCoStepRec.CoReturn e'
      }
    | TypeCheckedCoStepRec.CoReturnBang e ->
      sum {
        let! e' = convertExpression e
        return RunnableCoStepRec.CoReturnBang e'
      }
    | TypeCheckedCoStepRec.CoShow(predicate, view) ->
      sum {
        let! predicate', view' = sum.All2 (convertExpression predicate) (convertExpression view)
        return RunnableCoStepRec.CoShow(predicate', view')
      }
    | TypeCheckedCoStepRec.CoUntil(predicate, inner) ->
      sum {
        let! predicate', inner' = sum.All2 (convertExpression predicate) (convertExpression inner)
        return RunnableCoStepRec.CoUntil(predicate', inner')
      }
    | TypeCheckedCoStepRec.CoIgnore inner ->
      sum {
        let! inner' = convertExpression inner
        return RunnableCoStepRec.CoIgnore inner'
      }
    | TypeCheckedCoStepRec.CoMapContext(mapper, inner) ->
      sum {
        let! mapper', inner' = sum.All2 (convertExpression mapper) (convertExpression inner)
        return RunnableCoStepRec.CoMapContext(mapper', inner')
      }
    | TypeCheckedCoStepRec.CoMapState(mapDown, mapUp, inner) ->
      sum {
        let! mapDown', mapUp', inner' = sum.All3 (convertExpression mapDown) (convertExpression mapUp) (convertExpression inner)
        return RunnableCoStepRec.CoMapState(mapDown', mapUp', inner')
      }
    | TypeCheckedCoStepRec.CoGetContext ->
      Left(RunnableCoStepRec.CoGetContext)
    | TypeCheckedCoStepRec.CoGetState ->
      Left(RunnableCoStepRec.CoGetState)
    | TypeCheckedCoStepRec.CoSetState updater ->
      sum {
        let! updater' = convertExpression updater
        return RunnableCoStepRec.CoSetState updater'
      }

  and private convertQueryCaseHandler<'valueExt when 'valueExt: comparison>
    (h: TypeCheckedQueryCaseHandler<'valueExt>)
    : Sum<RunnableQueryCaseHandler<'valueExt>, Errors<Location>>
    =
    sum {
      let! body = convertQueryExpr h.Body
      return { Param = h.Param; Body = body }
    }

  and private convertQueryExpr<'valueExt when 'valueExt: comparison>
    (e: TypeCheckedExprQueryExpr<'valueExt>)
    : Sum<RunnableExprQueryExpr<'valueExt>, Errors<Location>>
    =
    sum {
      let! expr = convertQueryExprRec e.Expr
      return { Location = e.Location; Expr = expr }
    }

  and private convertQueryExprRec<'valueExt when 'valueExt: comparison>
    (expr: TypeCheckedExprQueryExprRec<'valueExt>)
    : Sum<RunnableExprQueryExprRec<'valueExt>, Errors<Location>>
    =
    match expr with
    | TypeCheckedExprQueryExprRec.QueryTupleCons items ->
      sum {
        let! items' = items |> List.map convertQueryExpr |> sum.All
        return RunnableExprQueryExprRec.QueryTupleCons items'
      }
    | TypeCheckedExprQueryExprRec.QueryRecordDes(e, field, isJson) ->
      sum {
        let! e' = convertQueryExpr e
        return RunnableExprQueryExprRec.QueryRecordDes(e', field, isJson)
      }
    | TypeCheckedExprQueryExprRec.QueryTupleDes(e, item, isJson) ->
      sum {
        let! e' = convertQueryExpr e
        return RunnableExprQueryExprRec.QueryTupleDes(e', item, isJson)
      }
    | TypeCheckedExprQueryExprRec.QueryConditional(cond, ``then``, ``else``) ->
      sum {
        let! cond', then', else' = sum.All3 (convertQueryExpr cond) (convertQueryExpr ``then``) (convertQueryExpr ``else``)
        return RunnableExprQueryExprRec.QueryConditional(cond', then', else')
      }
    | TypeCheckedExprQueryExprRec.QueryUnionDes(e, handlers) ->
      sum {
        let! e' = convertQueryExpr e
        let! handlers' = handlers |> Map.map (fun _ h -> convertQueryCaseHandler h) |> sum.AllMap
        return RunnableExprQueryExprRec.QueryUnionDes(e', handlers')
      }
    | TypeCheckedExprQueryExprRec.QuerySumDes(e, handlers) ->
      sum {
        let! e' = convertQueryExpr e
        let! handlers' = handlers |> Map.map (fun _ h -> convertQueryCaseHandler h) |> sum.AllMap
        return RunnableExprQueryExprRec.QuerySumDes(e', handlers')
      }
    | TypeCheckedExprQueryExprRec.QueryApply(func, arg) ->
      sum {
        let! func', arg' = sum.All2 (convertQueryExpr func) (convertQueryExpr arg)
        return RunnableExprQueryExprRec.QueryApply(func', arg')
      }
    | TypeCheckedExprQueryExprRec.QueryLookup id -> Left(RunnableExprQueryExprRec.QueryLookup id)
    | TypeCheckedExprQueryExprRec.QueryIntrinsic(i, t) -> Left(RunnableExprQueryExprRec.QueryIntrinsic(i, t))
    | TypeCheckedExprQueryExprRec.QueryConstant v -> Left(RunnableExprQueryExprRec.QueryConstant v)
    | TypeCheckedExprQueryExprRec.QueryClosureValue(v, t) -> Left(RunnableExprQueryExprRec.QueryClosureValue(v, t))
    | TypeCheckedExprQueryExprRec.QueryCastTo(e, t) ->
      sum {
        let! e' = convertQueryExpr e
        return RunnableExprQueryExprRec.QueryCastTo(e', t)
      }
    | TypeCheckedExprQueryExprRec.QueryCount q ->
      sum {
        let! q' = convertQuery q
        return RunnableExprQueryExprRec.QueryCount q'
      }
    | TypeCheckedExprQueryExprRec.QueryExists q ->
      sum {
        let! q' = convertQuery q
        return RunnableExprQueryExprRec.QueryExists q'
      }
    | TypeCheckedExprQueryExprRec.QueryArray q ->
      sum {
        let! q' = convertQuery q
        return RunnableExprQueryExprRec.QueryArray q'
      }
    | TypeCheckedExprQueryExprRec.QueryCountEvaluated v -> Left(RunnableExprQueryExprRec.QueryCountEvaluated v)
    | TypeCheckedExprQueryExprRec.QueryExistsEvaluated v -> Left(RunnableExprQueryExprRec.QueryExistsEvaluated v)
    | TypeCheckedExprQueryExprRec.QueryArrayEvaluated v -> Left(RunnableExprQueryExprRec.QueryArrayEvaluated v)
    | TypeCheckedExprQueryExprRec.QueryRecoveredSyntaxError err ->
      Right(conversionError err.ErrorLocation $"Cannot convert QueryRecoveredSyntaxError: {err.ErrorMessage} (context: {err.RecoveryContext})")

  and convertQuery<'valueExt when 'valueExt: comparison>
    (q: TypeCheckedExprQuery<'valueExt>)
    : Sum<RunnableExprQuery<'valueExt>, Errors<Location>>
    =
    match q with
    | TypeCheckedExprQuery.SimpleQuery sq ->
      sum {
        let! iterators =
          sq.Iterators
          |> NonEmptyList.map (fun (it: TypeCheckedExprQueryIterator<_>) ->
            sum {
              let! source = convertExpression it.Source
              return
                ({ Location = it.Location
                   Var = it.Var
                   VarType = it.VarType
                   Source = source } : RunnableExprQueryIterator<_>)
            })
          |> sum.AllNonEmpty

        let! joins =
          sq.Joins
          |> Option.map (fun js ->
            js
            |> NonEmptyList.map (fun (j: TypeCheckedExprQueryJoin<_>) ->
              sum {
                let! left, right = sum.All2 (convertQueryExpr j.Left) (convertQueryExpr j.Right)
                return
                  ({ Location = j.Location
                     Left = left
                     Right = right } : RunnableExprQueryJoin<_>)
              })
            |> sum.AllNonEmpty
          )
          |> sum.RunOption

        let! where' = sq.Where |> Option.map convertQueryExpr |> sum.RunOption
        let! select = convertQueryExpr sq.Select
        let! orderBy =
          sq.OrderBy
          |> Option.map (fun (e, dir) ->
            sum {
              let! e' = convertQueryExpr e
              return e', dir
            })
          |> sum.RunOption
        let! distinct = sq.Distinct |> Option.map convertQueryExpr |> sum.RunOption

        return
          RunnableExprQuery.SimpleQuery
            { RunnableSimpleQuery.Iterators = iterators
              Joins = joins
              Where = where'
              Select = select
              OrderBy = orderBy
              Closure = sq.Closure
              DeserializeFrom = sq.DeserializeFrom
              Distinct = distinct }
      }
    | TypeCheckedExprQuery.UnionQueries(q1, q2) ->
      sum {
        let! q1', q2' = sum.All2 (convertQuery q1) (convertQuery q2)
        return RunnableExprQuery.UnionQueries(q1', q2')
      }

  and convertExpression<'valueExt when 'valueExt: comparison>
    (expr: TypeCheckedExpr<'valueExt>)
    : Sum<RunnableExpr<'valueExt>, Errors<Location>>
    =
    sum {
      let! exprRec = convertExprRec expr.Location expr.Expr
      return
        { Expr = exprRec
          Location = expr.Location
          Type = expr.Type
          Kind = expr.Kind
          Scope = expr.Scope }
    }

  let convertExpressionOption<'valueExt when 'valueExt: comparison>
    (expr: Option<TypeCheckedExpr<'valueExt>>)
    : Sum<Option<RunnableExpr<'valueExt>>, Errors<Location>>
    =
    expr |> Option.map convertExpression |> sum.RunOption

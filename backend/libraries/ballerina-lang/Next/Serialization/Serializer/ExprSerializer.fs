namespace Ballerina.DSL.Next.Serialization

open Ballerina.Collections.Sum
open Ballerina.DSL.Next.Serialization.PocoObjects
open Ballerina.DSL.Next.Types

module ExprSerializer =
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.Errors
  open System.Text.Json
  open Ballerina.Reader.WithError
  open Ballerina.DSL.Next.Serialization.ValueSerializer

  let rec exprApplyDTO
    (apply: ExprApply<'T, 'Id, 'valueExt>)
    : Reader<ExprApplyDTO<'T, 'Id, 'valueExtDTO>, obj, Errors> =
    reader {
      let! f = exprToDTO apply.F
      let! arg = exprToDTO apply.Arg
      return { F = f; Arg = arg }
    }

  and exprTypeApplyDTO
    (typeApply: ExprTypeApply<'T, 'Id, 'valueExt>)
    : Reader<ExprTypeApplyDTO<'T, 'Id, 'valueExtDTO>, obj, Errors> =
    reader {
      let! f = exprToDTO typeApply.Func

      return
        { Func = f
          TypeArg = typeApply.TypeArg }
    }

  and exprEntitiesDesDTO
    (entitiesDes: ExprEntitiesDes<'T, 'Id, 'valueExt>)
    : Reader<ExprEntitiesDesDTO<'T, 'Id, 'valueExtDTO>, obj, Errors> =
    reader {
      let! ed = exprToDTO entitiesDes.Expr
      return { Expr = ed }
    }

  and exprEntityDesDTO
    (entityDes: ExprEntityDes<'T, 'Id, 'valueExt>)
    : Reader<ExprEntityDesDTO<'T, 'Id, 'valueExtDTO>, obj, Errors> =
    reader {
      let! ed = exprToDTO entityDes.Expr

      return
        { Expr = ed
          EntityName = entityDes.EntityName }
    }

  and exprFromValueDTO
    (fromValue: ExprFromValue<'T, 'Id, 'valueExt>)
    : Reader<ExprFromValueDTO<'T, 'Id, 'valueExtDTO>, obj, Errors> =
    reader {
      let! v = valueToDTO fromValue.Value

      return
        { Value = v
          ValueType = fromValue.ValueType
          ValueKind = fromValue.ValueKind }
    }

  and exprFromValueDTO
    (fromValue: ExprFromValue<'T, 'Id, 'valueExt>)
    : Reader<ExprFromValueDTO<'T, 'Id, 'valueExtDTO>, obj, Errors> =
    reader {
      let! v = valueToDTO fromValue.Value

      return
        { Value = v
          ValueType = fromValue.ValueType
          ValueKind = fromValue.ValueKind }
    }

  and exprIfDTO (``if``: ExprIf<'T, 'Id, 'valueExt>) : Reader<ExprIfDTO<'T, 'Id, 'valueExtDTO>, obj, Errors> =
    reader {
      let! cond = exprToDTO ``if``.Cond
      let! ``then`` = exprToDTO ``if``.Then
      let! ``else`` = exprToDTO ``if``.Else

      return
        { Cond = cond
          Then = ``then``
          Else = ``else`` }
    }

  and exprLambdaDTO
    (lambda: ExprLambda<'T, 'Id, 'valueExt>)
    : Reader<ExprLambdaDTO<'T, 'Id, 'valueExtDTO>, obj, Errors> =
    reader {
      let! body = exprToDTO lambda.Body

      return
        { Param = lambda.Param
          ParamType = Option.toObj lambda.ParamType
          Body = body }
    }

  and exprLetDTO (``let``: ExprLet<'T, 'Id, 'valueExt>) : Reader<ExprLetDTO<'T, 'Id, 'valueExtDTO>, obj, Errors> =
    reader {
      let! ``val`` = exprToDTO ``let``.Val
      let! rest = exprToDTO ``let``.Rest

      return
        { Var = ``let``.Var
          Type = Option.toObj ``let``.Type
          Val = ``val``
          Rest = rest }
    }

  and exprRecordConsDTO
    (rc: ExprRecordCons<'T, 'Id, 'valueExt>)
    : Reader<ExprRecordConsDTO<'T, 'Id, 'valueExtDTO>, obj, Errors> =
    reader {
      let! fields =
        rc.Fields
        |> List.map (fun (id, e) -> exprToDTO e |> reader.Map(fun e -> id, e))
        |> reader.All

      return { Fields = fields }
    }

  and exprRecordDesDTO
    (rd: ExprRecordDes<'T, 'Id, 'valueExt>)
    : Reader<ExprRecordDesDTO<'T, 'Id, 'valueExtDTO>, obj, Errors> =
    reader {
      let! expr = exprToDTO rd.Expr
      return { Expr = expr; Field = rd.Field }
    }

  and exprRecordWithDTO
    (rw: ExprRecordWith<'T, 'Id, 'valueExt>)
    : Reader<ExprRecordWithDTO<'T, 'Id, 'valueExtDTO>, obj, Errors> =
    reader {
      let! fields =
        rw.Fields
        |> List.map (fun (id, e) -> exprToDTO e |> reader.Map(fun e -> id, e))
        |> reader.All

      let! record = exprToDTO rw.Record
      return { Fields = fields; Record = record }
    }

  and exprRelationDesDTO
    (rd: ExprRelationDes<'T, 'Id, 'valueExt>)
    : Reader<ExprRelationDesDTO<'T, 'Id, 'valueExtDTO>, obj, Errors> =
    reader {
      let! e = exprToDTO rd.Expr

      return
        { Expr = e
          RelationName = rd.RelationName }
    }

  and exprRelationsDesDTO
    (rd: ExprRelationsDes<'T, 'Id, 'valueExt>)
    : Reader<ExprRelationsDesDTO<'T, 'Id, 'valueExtDTO>, obj, Errors> =
    reader {
      let! e = exprToDTO rd.Expr
      return { Expr = e }
    }

  and exprRelationLookupDesDTO
    (rld: ExprRelationLookupDes<'T, 'Id, 'valueExt>)
    : Reader<ExprRelationLookupDesDTO<'T, 'Id, 'valueExtDTO>, obj, Errors> =
    reader {
      let! e = exprToDTO rld.Expr

      return
        { Expr = e
          RelationName = rld.RelationName
          Direction = rld.Direction }
    }

  and exprTupleConsDTO
    (tc: ExprTupleCons<'T, 'Id, 'valueExt>)
    : Reader<ExprTupleConsDTO<'T, 'Id, 'valueExtDTO>, obj, Errors> =
    reader {
      let! items = tc.Items |> List.map exprToDTO |> reader.All
      return { Items = items }
    }

  and exprTupleDesDTO
    (td: ExprTupleDes<'T, 'Id, 'valueExt>)
    : Reader<ExprTupleDesDTO<'T, 'Id, 'valueExtDTO>, obj, Errors> =
    reader {
      let! e = td.Tuple |> exprToDTO
      return { Tuple = e; Item = td.Item }
    }

  and exprTypeLambdaDTO
    (tl: ExprTypeLambda<'T, 'Id, 'valueExt>)
    : Reader<ExprTypeLambdaDTO<'T, 'Id, 'valueExtDTO>, obj, Errors> =
    reader {
      let! body = exprToDTO tl.Body

      return
        { Body = body
          Param =
            { Name = tl.Param.Name
              Kind = tl.Param.Kind } }
    }

  and exprTypeLetDTO
    (tl: ExprTypeLet<'T, 'Id, 'valueExt>)
    : Reader<ExprTypeLetDTO<'T, 'Id, 'valueExtDTO>, obj, Errors> =
    reader {
      let! body = exprToDTO tl.Body

      return
        { Body = body
          Name = tl.Name
          TypeDef = tl.TypeDef }
    }

  and exprUnionDesDTO
    (ud: ExprUnionDes<'T, 'Id, 'valueExt>)
    : Reader<ExprUnionDesDTO<'T, 'Id, 'valueExtDTO>, obj, Errors> =
    reader {
      let! fallback = ud.Fallback |> Option.map exprToDTO |> reader.RunOption //|> sum.OfOption |> reader.OfSum)

      return
        { Fallback = Option.toObj fallback
          Handlers = ud.Handlers }
    }

  and exprRecToDTO (expr: ExprRec<'T, 'Id, 'valueExt>) =
    reader {
      match expr with
      | ExprRec.Primitive primitiveValue ->
        return ExprRecDTO<'T, 'Id, 'valueExtDTO>.CreatePrimitive(primitiveToDTO primitiveValue)
      | Lookup exprLookup -> return ExprRecDTO<'T, 'Id, 'valueExtDTO>.CreateLookup exprLookup
      | Apply apply ->
        let! a = exprApplyDTO apply
        return ExprRecDTO<'T, 'Id, 'valueExtDTO>.CreateApply a
      | TypeApply typeApply ->
        let! a = exprTypeApplyDTO typeApply
        return ExprRecDTO<'T, 'Id, 'valueExtDTO>.CreateTypeApply a
      | ExprRec.EntitiesDes ed ->
        let! ed = exprEntitiesDesDTO ed
        return ExprRecDTO<'T, 'Id, 'valueExtDTO>.CreateEntitiesDes ed
      | ExprRec.EntityDes ed ->
        let! ed = exprEntityDesDTO ed
        return ExprRecDTO<'T, 'Id, 'valueExtDTO>.CreateEntityDes ed
      | ExprRec.FromValue v ->
        let! v = exprFromValueDTO v
        return ExprRecDTO<'T, 'Id, 'valueExtDTO>.CreateFromValue v
      | ExprRec.If v ->
        let! v = exprIfDTO v
        return ExprRecDTO<'T, 'Id, 'valueExtDTO>.CreateIf v
      | ExprRec.Lambda l ->
        let! l = exprLambdaDTO l
        return ExprRecDTO<'T, 'Id, 'valueExtDTO>.CreateLambda l
      | ExprRec.Let l ->
        let! l = exprLetDTO l
        return ExprRecDTO<'T, 'Id, 'valueExtDTO>.CreateLet l
      | ExprRec.RecordCons rc ->
        let! rc = exprRecordConsDTO rc
        return ExprRecDTO<'T, 'Id, 'valueExtDTO>.CreateRecordCons rc
      | ExprRec.RecordDes rd ->
        let! rd = exprRecordDesDTO rd
        return ExprRecDTO<'T, 'Id, 'valueExtDTO>.CreateRecordDes rd
      | ExprRec.RecordWith rw ->
        let! rw = exprRecordWithDTO rw
        return ExprRecDTO<'T, 'Id, 'valueExtDTO>.CreateRecordWith rw
      | ExprRec.RelationDes rd ->
        let! rd = exprRelationDesDTO rd
        return ExprRecDTO<'T, 'Id, 'valueExtDTO>.CreateRelationDes rd
      | ExprRec.RelationsDes rd ->
        let! rd = exprRelationsDesDTO rd
        return ExprRecDTO<'T, 'Id, 'valueExtDTO>.CreateRelationsDes rd
      | ExprRec.RelationLookupDes rld ->
        let! rld = exprRelationLookupDesDTO rld
        return ExprRecDTO<'T, 'Id, 'valueExtDTO>.CreateRelationLookupDes rld
      | ExprRec.SumCons sc -> return ExprRecDTO<'T, 'Id, 'valueExtDTO>.CreateSumCons { Selector = sc.Selector }
      | ExprRec.SumDes sd -> return ExprRecDTO<'T, 'Id, 'valueExtDTO>.CreateSumDes { Handlers = sd.Handlers }
      | ExprRec.TupleCons tc ->
        let! items = exprTupleConsDTO tc
        return ExprRecDTO<'T, 'Id, 'valueExtDTO>.CreateTupleCons items
      | ExprRec.TupleDes td ->
        let! td = exprTupleDesDTO td
        return ExprRecDTO<'T, 'Id, 'valueExtDTO>.CreateTupleDes td
      | ExprRec.TypeLambda tl ->
        let! tl = exprTypeLambdaDTO tl
        return ExprRecDTO<'T, 'Id, 'valueExtDTO>.CreateTypeLambda tl
      | ExprRec.TypeLet tl ->
        let! tl = exprTypeLetDTO tl
        return ExprRecDTO<'T, 'Id, 'valueExtDTO>.CreateTypeLet tl
      | ExprRec.UnionDes ud ->
        let! ud = exprUnionDesDTO ud
        return ExprRecDTO<'T, 'Id, 'valueExtDTO>.CreateUnionDes ud
    }

  and exprToDTO (expr: Expr<'T, 'Id, 'valueExt>) : Reader<ExprDTO<'T, 'Id, 'valueExtDTO>, obj, Errors> =
    reader {
      let! exprDTO = exprRecToDTO expr.Expr

      let scope: TypeCheckScopeDTO =
        { Assembly = expr.Scope.Assembly
          Module = expr.Scope.Module
          Type = Option.toObj expr.Scope.Type }

      return
        { Expr = exprDTO
          Location = expr.Location
          Scope = scope }
    }

  type Expr<'T, 'Id, 'valueExt when 'Id: comparison> with
    static member JsonSerialize(expr: Expr<'T, 'Id, 'valueExt>) =
      reader {
        let exprDTO = exprToDTO expr
        return JsonSerializer.Serialize exprDTO
      }

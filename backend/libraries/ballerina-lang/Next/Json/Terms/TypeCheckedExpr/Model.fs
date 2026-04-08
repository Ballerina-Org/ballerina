namespace Ballerina.DSL.Next.Terms.Json.TypeCheckedExpr

open Ballerina
open Ballerina.DSL.Next.Json

[<AutoOpen>]
module ExprJson =
  open FSharp.Data
  open Ballerina.Reader.WithError
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Json.TypeCheckedExpr
  open Ballerina.DSL.Next.Terms.Json
  open Ballerina.Errors
  open Ballerina.StdLib.String
  open Ballerina.StdLib.Object

  type TypeCheckedExpr<'valueExt> with
    static member FromJson: TypeCheckedExprParser<'valueExt> =
      fun json ->
        reader.Any(
          TypeCheckedExpr.FromJsonLambda TypeCheckedExpr.FromJson json,
          [ TypeCheckedExpr.FromJsonTypeLambda TypeCheckedExpr.FromJson json
            TypeCheckedExpr.FromJsonTypeApply TypeCheckedExpr.FromJson json
            TypeCheckedExpr.FromJsonApply TypeCheckedExpr.FromJson json
            TypeCheckedExpr.FromJsonLet TypeCheckedExpr.FromJson json
            TypeCheckedExpr.FromJsonDo TypeCheckedExpr.FromJson json
            TypeCheckedExpr.FromJsonTypeLet TypeCheckedExpr.FromJson json
            TypeCheckedExpr.FromJsonRecordCons TypeCheckedExpr.FromJson json
            TypeCheckedExpr.FromJsonTupleCons TypeCheckedExpr.FromJson json
            TypeCheckedExpr.FromJsonSumCons TypeCheckedExpr.FromJson json
            TypeCheckedExpr.FromJsonRecordDes TypeCheckedExpr.FromJson json
            TypeCheckedExpr.FromJsonUnionDes TypeCheckedExpr.FromJson json
            TypeCheckedExpr.FromJsonTupleDes TypeCheckedExpr.FromJson json
            TypeCheckedExpr.FromJsonSumDes TypeCheckedExpr.FromJson json
            TypeCheckedExpr.FromJsonIf TypeCheckedExpr.FromJson json
            TypeCheckedExpr.FromJsonPrimitive(json)
            TypeCheckedExpr.FromJsonLookup(json)
            fun () -> $"Unknown Expr JSON: {json.AsFSharpString.ReasonablyClamped}"
            |> Errors.Singleton()
            |> Errors.MapPriority(replaceWith ErrorPriority.Medium)
            |> reader.Throw ]
        )
        |> reader.MapError(Errors.HighestPriority)
        |> reader.MapError(Errors.Map(fun e -> $"{e}\n..when parsing {json.ToString().ReasonablyClamped}"))

    static member ToJson: TypeCheckedExprEncoder<'valueExt> =
      fun expr ->
        match expr.Expr with
        | TypeCheckedExprRec.Lambda({ TypeCheckedExprLambda.Param = name
                                      ParamType = _
                                      Body = body }) -> TypeCheckedExpr.ToJsonLambda TypeCheckedExpr.ToJson name body
        | TypeCheckedExprRec.TypeLambda({ TypeCheckedExprTypeLambda.Param = name
                                          Body = body }) ->
          TypeCheckedExpr.ToJsonTypeLambda TypeCheckedExpr.ToJson name body
        | TypeCheckedExprRec.TypeApply({ TypeCheckedExprTypeApply.TypeArg = t
                                         Func = e }) -> TypeCheckedExpr.ToJsonTypeApply TypeCheckedExpr.ToJson e t
        | TypeCheckedExprRec.Apply({ TypeCheckedExprApply.F = e1
                                     Arg = e2 }) -> TypeCheckedExpr.ToJsonApply TypeCheckedExpr.ToJson e1 e2
        | TypeCheckedExprRec.FromValue({ TypeCheckedExprFromValue.Value = _
                                         ValueType = _
                                         ValueKind = _ }) -> failwith "Not implemented"
        | TypeCheckedExprRec.Let({ TypeCheckedExprLet.Var = v
                                   Type = _
                                   Val = e1
                                   Rest = e2 }) -> TypeCheckedExpr.ToJsonLet TypeCheckedExpr.ToJson v e1 e2
        | TypeCheckedExprRec.Do({ TypeCheckedExprDo.Val = e1
                                  Rest = e2 }) -> TypeCheckedExpr.ToJsonDo TypeCheckedExpr.ToJson e1 e2
        | TypeCheckedExprRec.TypeLet({ TypeCheckedExprTypeLet.Name = v
                                       TypeDef = t
                                       Body = e }) -> TypeCheckedExpr.ToJsonTypeLet TypeCheckedExpr.ToJson v t e
        | TypeCheckedExprRec.RecordCons { Fields = fields } ->
          TypeCheckedExpr.ToJsonRecordCons TypeCheckedExpr.ToJson fields
        | TypeCheckedExprRec.RecordWith _ -> failwith "not implemented"
        | TypeCheckedExprRec.TupleCons { Items = items } -> TypeCheckedExpr.ToJsonTupleCons TypeCheckedExpr.ToJson items
        | TypeCheckedExprRec.SumCons({ TypeCheckedExprSumCons.Selector = selector }) ->
          TypeCheckedExpr.ToJsonSumCons TypeCheckedExpr.ToJson selector
        | TypeCheckedExprRec.RecordDes({ TypeCheckedExprRecordDes.Expr = record
                                         Field = field }) ->
          TypeCheckedExpr.ToJsonRecordDes TypeCheckedExpr.ToJson record field
        | TypeCheckedExprRec.UnionDes({ TypeCheckedExprUnionDes.Handlers = cases
                                        Fallback = fallback }) ->
          TypeCheckedExpr.ToJsonUnionDes TypeCheckedExpr.ToJson cases fallback
        | TypeCheckedExprRec.TupleDes({ TypeCheckedExprTupleDes.Tuple = tuple
                                        Item = selector }) ->
          TypeCheckedExpr.ToJsonTupleDes TypeCheckedExpr.ToJson tuple selector
        | TypeCheckedExprRec.SumDes { Handlers = cases } -> TypeCheckedExpr.ToJsonSumDes TypeCheckedExpr.ToJson cases
        | TypeCheckedExprRec.If({ TypeCheckedExprIf.Cond = cond
                                  Then = thenExpr
                                  Else = elseExpr }) ->
          TypeCheckedExpr.ToJsonIf TypeCheckedExpr.ToJson cond thenExpr elseExpr
        | TypeCheckedExprRec.Primitive p -> TypeCheckedExpr.ToJsonPrimitive p
        | TypeCheckedExprRec.Lookup s -> TypeCheckedExpr.ToJsonLookup s
        | TypeCheckedExprRec.EntitiesDes _ -> failwith "Not implemented"
        | TypeCheckedExprRec.RelationsDes _ -> failwith "Not implemented"
        | TypeCheckedExprRec.EntityDes _ -> failwith "Not implemented"
        | TypeCheckedExprRec.RelationDes _ -> failwith "Not implemented"
        | TypeCheckedExprRec.RelationLookupDes _ -> failwith "Not implemented"
        | TypeCheckedExprRec.Query _ -> failwith "Not implemented"

namespace Ballerina.DSL.Next.Terms.Json

open Ballerina.DSL.Next.Json

[<AutoOpen>]
module ExprJson =
  open FSharp.Data
  open Ballerina.Reader.WithError
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Json.Expr
  open Ballerina.DSL.Next.Terms.Json
  open Ballerina.Errors
  open Ballerina.StdLib.String
  open Ballerina.StdLib.Object

  type Expr<'T, 'Id, 'valueExt when 'Id: comparison> with
    static member FromJson: ExprParser<'T, 'Id, 'valueExt> =
      fun json ->
        reader.Any(
          Expr.FromJsonLambda Expr.FromJson json,
          [ Expr.FromJsonTypeLambda Expr.FromJson json
            Expr.FromJsonTypeApply Expr.FromJson json
            Expr.FromJsonApply Expr.FromJson json
            Expr.FromJsonLet Expr.FromJson json
            Expr.FromJsonTypeLet Expr.FromJson json
            Expr.FromJsonRecordCons Expr.FromJson json
            Expr.FromJsonTupleCons Expr.FromJson json
            Expr.FromJsonSumCons Expr.FromJson json
            Expr.FromJsonRecordDes Expr.FromJson json
            Expr.FromJsonUnionDes Expr.FromJson json
            Expr.FromJsonTupleDes Expr.FromJson json
            Expr.FromJsonSumDes Expr.FromJson json
            Expr.FromJsonIf Expr.FromJson json
            Expr.FromJsonPrimitive(json)
            Expr.FromJsonLookup(json)
            $"Unknown Expr JSON: {json.ToFSharpString.ReasonablyClamped}"
            |> Errors.Singleton
            |> Errors.WithPriority ErrorPriority.Medium
            |> reader.Throw ]
        )
        |> reader.MapError(Errors.HighestPriority)
        |> reader.MapError(Errors.Map(fun e -> $"{e}\n..when parsing {json.ToString().ReasonablyClamped}"))

    static member ToJson: ExprEncoder<'T, 'Id, 'valueExt> =
      fun expr ->
        match expr.Expr with
        | ExprRec.Lambda({ Param = name
                           ParamType = _
                           Body = body }) -> Expr.ToJsonLambda Expr.ToJson name body
        | ExprRec.TypeLambda({ Param = name; Body = body }) -> Expr.ToJsonTypeLambda Expr.ToJson name body
        | ExprRec.TypeApply({ TypeArg = t; Func = e }) -> Expr.ToJsonTypeApply Expr.ToJson e t
        | ExprRec.Apply({ F = e1; Arg = e2 }) -> Expr.ToJsonApply Expr.ToJson e1 e2
        | ExprRec.ApplyValue({ F = _e1; Arg = _e2 }) ->
          // Expr.ToJsonApply Expr.ToJson e1 e2
          failwith "Not implemented"
        | ExprRec.Let({ Var = v
                        Type = _
                        Val = e1
                        Rest = e2 }) -> Expr.ToJsonLet Expr.ToJson v e1 e2
        | ExprRec.TypeLet({ ExprTypeLet.Name = v
                            TypeDef = t
                            Body = e }) -> Expr.ToJsonTypeLet Expr.ToJson v t e
        | ExprRec.RecordCons { Fields = fields } -> Expr.ToJsonRecordCons Expr.ToJson fields
        | ExprRec.RecordWith _ -> failwith "not implemented"
        | ExprRec.TupleCons { Items = items } -> Expr.ToJsonTupleCons Expr.ToJson items
        | ExprRec.SumCons({ Selector = selector }) -> Expr.ToJsonSumCons Expr.ToJson selector
        | ExprRec.RecordDes({ Expr = record; Field = field }) -> Expr.ToJsonRecordDes Expr.ToJson record field
        | ExprRec.UnionDes({ Handlers = cases
                             Fallback = fallback }) -> Expr.ToJsonUnionDes Expr.ToJson cases fallback
        | ExprRec.TupleDes({ Tuple = tuple; Item = selector }) -> Expr.ToJsonTupleDes Expr.ToJson tuple selector
        | ExprRec.SumDes { Handlers = cases } -> Expr.ToJsonSumDes Expr.ToJson cases
        | ExprRec.If({ Cond = cond
                       Then = thenExpr
                       Else = elseExpr }) -> Expr.ToJsonIf Expr.ToJson cond thenExpr elseExpr
        | ExprRec.Primitive p -> Expr.ToJsonPrimitive p
        | ExprRec.Lookup s -> Expr.ToJsonLookup s

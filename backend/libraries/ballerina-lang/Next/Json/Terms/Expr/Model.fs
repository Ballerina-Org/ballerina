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

  type Expr<'T> with
    static member FromJson: ExprParser<'T> =
      fun json ->
        reader.Any(
          Expr.FromJsonLambda Expr.FromJson json,
          [ Expr.FromJsonTypeLambda Expr.FromJson json
            Expr.FromJsonTypeApply Expr.FromJson json
            Expr.FromJsonApply Expr.FromJson json
            Expr.FromJsonLet Expr.FromJson json
            Expr.FromJsonTypeLet Expr.FromJson json
            Expr.FromJsonRecordCons Expr.FromJson json
            Expr.FromJsonUnionCons Expr.FromJson json
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

    static member ToJson: ExprEncoder<'T> =
      fun expr ->
        match expr with
        | Expr.Lambda(name, _, body) -> Expr.ToJsonLambda Expr.ToJson name body
        | Expr.TypeLambda(name, body) -> Expr.ToJsonTypeLambda Expr.ToJson name body
        | Expr.TypeApply(t, e) -> Expr.ToJsonTypeApply Expr.ToJson t e
        | Expr.Apply(e1, e2) -> Expr.ToJsonApply Expr.ToJson e1 e2
        | Expr.Let(v, e1, e2) -> Expr.ToJsonLet Expr.ToJson v e1 e2
        | Expr.TypeLet(v, e1, e2) -> Expr.ToJsonTypeLet Expr.ToJson v e1 e2
        | Expr.RecordCons t -> Expr.ToJsonRecordCons Expr.ToJson t
        | Expr.UnionCons(a, b) -> Expr.ToJsonUnionCons Expr.ToJson a b
        | Expr.TupleCons t -> Expr.ToJsonTupleCons Expr.ToJson t
        | Expr.SumCons(i, t) -> Expr.ToJsonSumCons Expr.ToJson i t
        | Expr.RecordDes(v, e) -> Expr.ToJsonRecordDes Expr.ToJson v e
        | Expr.UnionDes(t, fallback) -> Expr.ToJsonUnionDes Expr.ToJson t fallback
        | Expr.TupleDes(v, e) -> Expr.ToJsonTupleDes Expr.ToJson v e
        | Expr.SumDes m -> Expr.ToJsonSumDes Expr.ToJson m
        | Expr.If(cond, thenExpr, elseExpr) -> Expr.ToJsonIf Expr.ToJson cond thenExpr elseExpr
        | Expr.Primitive p -> Expr.ToJsonPrimitive p
        | Expr.Lookup s -> Expr.ToJsonLookup s

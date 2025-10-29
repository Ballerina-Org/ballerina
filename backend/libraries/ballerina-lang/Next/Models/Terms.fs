namespace Ballerina.DSL.Next.Terms

[<AutoOpen>]
module Model =
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open System
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.LocalizedErrors

  type Var =
    { Name: string }

    static member Create name : Var = { Var.Name = name }

  type ExprLookup<'T, 'Id when 'Id: comparison> =
    { Id: 'Id }

    override self.ToString() = self.Id.ToString()

  and ExprLambda<'T, 'Id when 'Id: comparison> =
    { Param: Var
      ParamType: Option<'T>
      Body: Expr<'T, 'Id> }

  and ExprApply<'T, 'Id when 'Id: comparison> =
    { F: Expr<'T, 'Id>; Arg: Expr<'T, 'Id> }

  and ExprLet<'T, 'Id when 'Id: comparison> =
    { Var: Var
      Type: Option<'T>
      Val: Expr<'T, 'Id>
      Rest: Expr<'T, 'Id> }

  and ExprIf<'T, 'Id when 'Id: comparison> =
    { Cond: Expr<'T, 'Id>
      Then: Expr<'T, 'Id>
      Else: Expr<'T, 'Id> }

  and ExprTypeLambda<'T, 'Id when 'Id: comparison> =
    { Param: TypeParameter
      Body: Expr<'T, 'Id> }

  and ExprTypeApply<'T, 'Id when 'Id: comparison> = { Func: Expr<'T, 'Id>; TypeArg: 'T }

  and ExprTypeLet<'T, 'Id when 'Id: comparison> =
    { Name: string
      TypeDef: 'T
      Body: Expr<'T, 'Id> }

  and ExprRecordCons<'T, 'Id when 'Id: comparison> = { Fields: List<'Id * Expr<'T, 'Id>> }

  and ExprRecordWith<'T, 'Id when 'Id: comparison> =
    { Record: Expr<'T, 'Id>
      Fields: List<'Id * Expr<'T, 'Id>> }

  and ExprTupleCons<'T, 'Id when 'Id: comparison> = { Items: List<Expr<'T, 'Id>> }

  and ExprTupleDes<'T, 'Id when 'Id: comparison> =
    { Tuple: Expr<'T, 'Id>
      Item: TupleDesSelector }

  and ExprSumCons<'T, 'Id when 'Id: comparison> = { Selector: SumConsSelector }

  and ExprRecordDes<'T, 'Id when 'Id: comparison> = { Expr: Expr<'T, 'Id>; Field: 'Id }

  and ExprUnionDes<'T, 'Id when 'Id: comparison> =
    { Handlers: Map<'Id, CaseHandler<'T, 'Id>>
      Fallback: Option<Expr<'T, 'Id>> }

  and ExprSumDes<'T, 'Id when 'Id: comparison> =
    { Handlers: Map<SumConsSelector, CaseHandler<'T, 'Id>> }

  and ExprRec<'T, 'Id when 'Id: comparison> =
    | Primitive of PrimitiveValue
    | Lookup of ExprLookup<'T, 'Id>
    | TypeLambda of ExprTypeLambda<'T, 'Id>
    | TypeApply of ExprTypeApply<'T, 'Id>
    | TypeLet of ExprTypeLet<'T, 'Id>
    | Lambda of ExprLambda<'T, 'Id>
    | Apply of ExprApply<'T, 'Id>
    | Let of ExprLet<'T, 'Id>
    | If of ExprIf<'T, 'Id>
    | RecordCons of ExprRecordCons<'T, 'Id>
    | RecordWith of ExprRecordWith<'T, 'Id>
    | TupleCons of ExprTupleCons<'T, 'Id>
    | SumCons of ExprSumCons<'T, 'Id>
    | RecordDes of ExprRecordDes<'T, 'Id>
    | UnionDes of ExprUnionDes<'T, 'Id>
    | TupleDes of ExprTupleDes<'T, 'Id>
    | SumDes of ExprSumDes<'T, 'Id>

    override self.ToString() : string =
      match self with
      | TypeLambda({ ExprTypeLambda.Param = tp
                     Body = body }) -> $"(Λ{tp.ToString()} => {body.ToString()})"
      | TypeApply({ Func = e; TypeArg = t }) -> $"{e.ToString()} [{t.ToString()}]"
      | Lambda({ Param = v
                 ParamType = topt
                 Body = body }) ->
        match topt with
        | Some t -> $"(fun ({v.Name}: {t.ToString()}) -> {body.ToString()})"
        | None -> $"(fun {v.Name} -> {body.ToString()})"
      | Apply({ F = e1; Arg = e2 }) -> $"({e1.ToString()} {e2.ToString()})"
      | Let({ Var = v
              Type = topt
              Val = e1
              Rest = e2 }) ->
        match topt with
        | Some t -> $"(let {v.Name}: {t.ToString()} = {e1.ToString()} in {e2.ToString()})"
        | None -> $"(let {v.Name} = {e1.ToString()} in {e2.ToString()})"
      | TypeLet({ Name = name
                  TypeDef = t
                  Body = body }) -> $"(type {name} = {t.ToString()}; {body.ToString()})"
      | RecordCons { Fields = fields } ->
        let fieldStr =
          fields
          |> List.map (fun (k, v) -> $"{k.ToString()} = {v.ToString()}")
          |> String.concat "; "

        $"{{ {fieldStr} }}"
      | RecordWith({ Record = record; Fields = fields }) ->
        let fieldStr =
          fields
          |> List.map (fun (k, v) -> $"{k.ToString()} = {v.ToString()}")
          |> String.concat "; "

        $"{{ {record.ToString()} with {fieldStr} }}"
      | TupleCons { Items = items } ->
        let itemStr = items |> List.map (fun v -> v.ToString()) |> String.concat ", "
        $"({itemStr})"
      | SumCons({ Selector = selector }) -> $"{selector.Case}Of{selector.Count}"
      | RecordDes({ Expr = record; Field = field }) -> $"{record.ToString()}.{field.ToString()}"
      | UnionDes({ Handlers = handlers
                   Fallback = defaultOpt }) ->
        let handlerStr =
          handlers
          |> Map.toList
          |> List.map (fun (k, (v, body)) -> $"{k.ToString()}({v.Name}) => {body.ToString()}")
          |> String.concat " | "

        match defaultOpt with
        | Some defaultExpr -> $"(match {handlerStr} | _ => {defaultExpr.ToString()})"
        | None -> $"(match {handlerStr})"
      | TupleDes({ ExprTupleDes.Tuple = tuple
                   Item = selector }) -> $"{tuple.ToString()}.{selector.Index}"
      | SumDes { Handlers = handlers } ->
        let handlerStr =
          handlers
          |> Map.toList
          |> List.map (fun (k, (v, body)) -> $"{k.Case}Of{k.Count} ({v.Name} => {body.ToString()})")
          |> String.concat " | "

        $"(match {handlerStr})"
      | Primitive p -> p.ToString()
      | Lookup id -> id.ToString()
      | If({ Cond = cond
             Then = thenExpr
             Else = elseExpr }) -> $"(if {cond.ToString()} then {thenExpr.ToString()} else {elseExpr.ToString()})"

  and Expr<'T, 'Id when 'Id: comparison> =
    { Expr: ExprRec<'T, 'Id>
      Location: Location
      Scope: TypeCheckScope }

    override self.ToString() : string = self.Expr.ToString()

  and SumConsSelector = { Case: int; Count: int }
  and TupleDesSelector = { Index: int }

  and CaseHandler<'T, 'Id when 'Id: comparison> = Var * Expr<'T, 'Id>

  and PrimitiveValue =
    | Int32 of Int32
    | Int64 of Int64
    | Float32 of float32
    | Float64 of float
    | Decimal of decimal
    | Bool of bool
    | Guid of Guid
    | String of string
    | Date of DateOnly
    | DateTime of DateTime
    | TimeSpan of TimeSpan
    | Unit

    override self.ToString() : string =
      match self with
      | Int32 v -> v.ToString()
      | Int64 v -> v.ToString()
      | Float32 v -> v.ToString()
      | Float64 v -> v.ToString()
      | Decimal v -> v.ToString()
      | Bool v -> v.ToString()
      | Guid v -> v.ToString()
      | String v -> $"\"{v}\""
      | Date v -> v.ToString("yyyy-MM-dd")
      | DateTime v -> v.ToString("o")
      | TimeSpan v -> v.ToString()
      | Unit -> "()"

  and Value<'T, 'valueExt> =
    | TypeLambda of TypeParameter * Expr<'T, ResolvedIdentifier>
    | Lambda of Var * Expr<'T, ResolvedIdentifier> * Map<ResolvedIdentifier, Value<'T, 'valueExt>> * TypeCheckScope
    | Record of Map<ResolvedIdentifier, Value<'T, 'valueExt>>
    | UnionCase of ResolvedIdentifier * Value<'T, 'valueExt>
    | RecordDes of ResolvedIdentifier
    | UnionCons of ResolvedIdentifier
    | Tuple of List<Value<'T, 'valueExt>>
    | Sum of SumConsSelector * Value<'T, 'valueExt>
    | Primitive of PrimitiveValue
    | Var of Var
    | Ext of 'valueExt

    override self.ToString() : string =
      match self with
      | TypeLambda(tp, body) -> $"(Fun {tp.ToString()} => {body})"
      | Lambda(v, body, _closure, _scope) -> $"(fun {v.Name} -> {body})"
      | Record fields ->
        let fieldStr =
          fields
          |> Map.toList
          |> List.map (fun (k, v) -> $"{k.ToString()} = {v.ToString()}")
          |> String.concat "; "

        $"{{ {fieldStr} }}"
      | UnionCase(case, value) -> $"{case}({value.ToString()})"
      | RecordDes ts -> ts.ToString()
      | UnionCons ts -> ts.ToString()
      | Tuple values ->
        let valueStr = values |> List.map (fun v -> v.ToString()) |> String.concat ", "
        $"({valueStr})"
      | Sum(selector, value) -> $"{selector.Case}Of{selector.Count}({value.ToString()})"
      | Primitive p -> p.ToString()
      | Var v -> v.Name
      | Ext e -> e.ToString()

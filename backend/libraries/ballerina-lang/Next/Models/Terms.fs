namespace Ballerina.DSL.Next.Terms

[<AutoOpen>]
module Model =
  open System
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.LocalizedErrors
  open Ballerina.StdLib.Formats

  type Var =
    { Name: string }

    static member Create name : Var = { Var.Name = name }

  type ExprLookup<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Id: 'Id }

    override self.ToString() = self.Id.ToString()

  and ExprLambda<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Param: Var
      ParamType: Option<'T>
      Body: Expr<'T, 'Id, 'valueExt> }

  and ExprApply<'T, 'Id, 'valueExt when 'Id: comparison> =
    { F: Expr<'T, 'Id, 'valueExt>
      Arg: Expr<'T, 'Id, 'valueExt> }

  and ExprApplyValue<'T, 'Id, 'valueExt when 'Id: comparison> =
    { F: Expr<'T, 'Id, 'valueExt>
      Arg: Value<TypeValue, 'valueExt>
      ArgT: TypeValue }

  and ExprLet<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Var: Var
      Type: Option<'T>
      Val: Expr<'T, 'Id, 'valueExt>
      Rest: Expr<'T, 'Id, 'valueExt> }

  and ExprIf<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Cond: Expr<'T, 'Id, 'valueExt>
      Then: Expr<'T, 'Id, 'valueExt>
      Else: Expr<'T, 'Id, 'valueExt> }

  and ExprTypeLambda<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Param: TypeParameter
      Body: Expr<'T, 'Id, 'valueExt> }

  and ExprTypeApply<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Func: Expr<'T, 'Id, 'valueExt>
      TypeArg: 'T }

  and ExprTypeLet<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Name: string
      TypeDef: 'T
      Body: Expr<'T, 'Id, 'valueExt> }

  and ExprRecordCons<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Fields: List<'Id * Expr<'T, 'Id, 'valueExt>> }

  and ExprRecordWith<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Record: Expr<'T, 'Id, 'valueExt>
      Fields: List<'Id * Expr<'T, 'Id, 'valueExt>> }

  and ExprTupleCons<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Items: List<Expr<'T, 'Id, 'valueExt>> }

  and ExprTupleDes<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Tuple: Expr<'T, 'Id, 'valueExt>
      Item: TupleDesSelector }

  and ExprSumCons<'T, 'Id, 'valueExt when 'Id: comparison> = { Selector: SumConsSelector }

  and ExprRecordDes<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Expr: Expr<'T, 'Id, 'valueExt>
      Field: 'Id }

  and ExprUnionDes<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Handlers: Map<'Id, CaseHandler<'T, 'Id, 'valueExt>>
      Fallback: Option<Expr<'T, 'Id, 'valueExt>> }

  and ExprSumDes<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Handlers: Map<SumConsSelector, CaseHandler<'T, 'Id, 'valueExt>> }

  and ExprFromValue<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Value: Value<TypeValue, 'valueExt>
      ValueType: TypeValue
      ValueKind: Kind }

  and ExprRec<'T, 'Id, 'valueExt when 'Id: comparison> =
    | Primitive of PrimitiveValue
    | Lookup of ExprLookup<'T, 'Id, 'valueExt>
    | TypeLambda of ExprTypeLambda<'T, 'Id, 'valueExt>
    | TypeApply of ExprTypeApply<'T, 'Id, 'valueExt>
    | TypeLet of ExprTypeLet<'T, 'Id, 'valueExt>
    | Lambda of ExprLambda<'T, 'Id, 'valueExt>
    | FromValue of ExprFromValue<'T, 'Id, 'valueExt>
    | Apply of ExprApply<'T, 'Id, 'valueExt>
    | Let of ExprLet<'T, 'Id, 'valueExt>
    | If of ExprIf<'T, 'Id, 'valueExt>
    | RecordCons of ExprRecordCons<'T, 'Id, 'valueExt>
    | RecordWith of ExprRecordWith<'T, 'Id, 'valueExt>
    | TupleCons of ExprTupleCons<'T, 'Id, 'valueExt>
    | SumCons of ExprSumCons<'T, 'Id, 'valueExt>
    | RecordDes of ExprRecordDes<'T, 'Id, 'valueExt>
    | UnionDes of ExprUnionDes<'T, 'Id, 'valueExt>
    | TupleDes of ExprTupleDes<'T, 'Id, 'valueExt>
    | SumDes of ExprSumDes<'T, 'Id, 'valueExt>

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
      | FromValue({ Value = v
                    ValueType = t
                    ValueKind = k }) -> $"({v.ToString()} : {t.ToString()} :: {k.ToString()}])"
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

  and Expr<'T, 'Id, 'valueExt when 'Id: comparison> =
    { Expr: ExprRec<'T, 'Id, 'valueExt>
      Location: Location
      Scope: TypeCheckScope }

    override self.ToString() : string = self.Expr.ToString()

  and SumConsSelector = { Case: int; Count: int }
  and TupleDesSelector = { Index: int }

  and CaseHandler<'T, 'Id, 'valueExt when 'Id: comparison> = Var * Expr<'T, 'Id, 'valueExt>

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
      | Date v -> Iso8601.DateOnly.print v
      | DateTime v -> Iso8601.DateTime.printUtc v
      | TimeSpan v -> v.ToString()
      | Unit -> "()"

  and Value<'T, 'valueExt> =
    | TypeLambda of TypeParameter * Expr<'T, ResolvedIdentifier, 'valueExt>
    | Lambda of
      Var *
      Expr<'T, ResolvedIdentifier, 'valueExt> *
      Map<ResolvedIdentifier, Value<'T, 'valueExt>> *
      TypeCheckScope
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

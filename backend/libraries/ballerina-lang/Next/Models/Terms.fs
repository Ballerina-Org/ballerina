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

  type ExprRec<'T> =
    | TypeLambda of TypeParameter * Expr<'T>
    | TypeApply of Expr<'T> * 'T
    | Lambda of Var * Option<'T> * Expr<'T>
    | Apply of Expr<'T> * Expr<'T>
    | Let of Var * Option<'T> * Expr<'T> * Expr<'T>
    | TypeLet of string * 'T * Expr<'T>
    | RecordCons of List<Identifier * Expr<'T>>
    | RecordWith of Expr<'T> * List<Identifier * Expr<'T>>
    | UnionCons of Identifier * Expr<'T>
    | TupleCons of List<Expr<'T>>
    | SumCons of SumConsSelector
    | RecordDes of Expr<'T> * Identifier
    | UnionDes of Map<Identifier, CaseHandler<'T>> * Option<Expr<'T>>
    | TupleDes of Expr<'T> * TupleDesSelector
    | SumDes of Map<SumConsSelector, CaseHandler<'T>>
    | Primitive of PrimitiveValue
    | Lookup of Identifier
    | If of Expr<'T> * Expr<'T> * Expr<'T>

    override self.ToString() : string =
      match self with
      | TypeLambda(tp, body) -> $"(Λ{tp.ToString()}. {body.ToString()})"
      | TypeApply(e, t) -> $"({e.ToString()} [{t.ToString()}])"
      | Lambda(v, topt, body) ->
        match topt with
        | Some t -> $"(fun ({v.Name}: {t.ToString()}) -> {body.ToString()})"
        | None -> $"(fun {v.Name} -> {body.ToString()})"
      | Apply(e1, e2) -> $"({e1.ToString()} {e2.ToString()})"
      | Let(v, topt, e1, e2) ->
        match topt with
        | Some t -> $"(let {v.Name}: {t.ToString()} = {e1.ToString()} in {e2.ToString()})"
        | None -> $"(let {v.Name} = {e1.ToString()} in {e2.ToString()})"
      | TypeLet(name, t, body) -> $"(type {name} = {t.ToString()}; {body.ToString()})"
      | RecordCons fields ->
        let fieldStr =
          fields
          |> List.map (fun (k, v) -> $"{k.LocalName} = {v.ToString()}")
          |> String.concat "; "

        $"{{ {fieldStr} }}"
      | RecordWith(record, fields) ->
        let fieldStr =
          fields
          |> List.map (fun (k, v) -> $"{k.LocalName} = {v.ToString()}")
          |> String.concat "; "

        $"{{ {record.ToString()} with {fieldStr} }}"
      | UnionCons(case, value) -> $"{case.LocalName}({value.ToString()})"
      | TupleCons values ->
        let valueStr = values |> List.map (fun v -> v.ToString()) |> String.concat ", "
        $"({valueStr})"
      | SumCons(selector) -> $"{selector.Case}Of{selector.Count}"
      | RecordDes(record, field) -> $"{record.ToString()}.{field.LocalName}"
      | UnionDes(handlers, defaultOpt) ->
        let handlerStr =
          handlers
          |> Map.toList
          |> List.map (fun (k, (v, body)) -> $"{k.LocalName}({v.Name}) => {body.ToString()}")
          |> String.concat " | "

        match defaultOpt with
        | Some defaultExpr -> $"(match {handlerStr} | _ => {defaultExpr.ToString()})"
        | None -> $"(match {handlerStr})"
      | TupleDes(tuple, selector) -> $"{tuple.ToString()}.{selector.Index}"
      | SumDes handlers ->
        let handlerStr =
          handlers
          |> Map.toList
          |> List.map (fun (k, (v, body)) -> $"{k.Case}Of{k.Count} ({v.Name} => {body.ToString()})")
          |> String.concat " | "

        $"(match {handlerStr})"
      | Primitive p -> p.ToString()
      | Lookup id -> id.ToString()
      | If(cond, thenExpr, elseExpr) -> $"(if {cond.ToString()} then {thenExpr.ToString()} else {elseExpr.ToString()})"

  and Expr<'T> =
    { Expr: ExprRec<'T>
      Location: Location }

    override self.ToString() : string = self.Expr.ToString()

  and SumConsSelector = { Case: int; Count: int }
  and TupleDesSelector = { Index: int }

  and CaseHandler<'T> = Var * Expr<'T>

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
    | TypeLambda of TypeParameter * Expr<'T>
    | Lambda of Var * Expr<'T> * Map<Identifier, Value<'T, 'valueExt>>
    | Record of Map<TypeSymbol, Value<'T, 'valueExt>>
    | UnionCase of TypeSymbol * Value<'T, 'valueExt>
    | Tuple of List<Value<'T, 'valueExt>>
    | Sum of SumConsSelector * Value<'T, 'valueExt>
    | Primitive of PrimitiveValue
    | Var of Var
    | Ext of 'valueExt

    override self.ToString() : string =
      match self with
      | TypeLambda(tp, body) -> $"(Fun {tp.ToString()} => {body})"
      | Lambda(v, body, _closure) -> $"(fun {v.Name} -> {body})"
      | Record fields ->
        let fieldStr =
          fields
          |> Map.toList
          |> List.map (fun (k, v) -> $"{k.Name} = {v.ToString()}")
          |> String.concat "; "

        $"{{ {fieldStr} }}"
      | UnionCase(case, value) -> $"{case.Name}({value.ToString()})"
      | Tuple values ->
        let valueStr = values |> List.map (fun v -> v.ToString()) |> String.concat ", "
        $"({valueStr})"
      | Sum(selector, value) -> $"{selector.Case}Of{selector.Count}({value.ToString()})"
      | Primitive p -> p.ToString()
      | Var v -> v.Name
      | Ext e -> e.ToString()

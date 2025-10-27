namespace Ballerina.DSL.Next.Terms

open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Types.Model

module Patterns =
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open System
  open Ballerina.DSL.Next.Terms.Model

  type Location = Ballerina.LocalizedErrors.Location

  type Var with
    static member Create(name: string) : Var = { Name = name }

  type PrimitiveValue with
    static member AsInt32(v: PrimitiveValue) : Sum<int32, Errors> =
      match v with
      | PrimitiveValue.Int32 i -> sum.Return i
      | other -> sum.Throw(Errors.Singleton $"Expected an int but got {other}")

    static member AsInt64(v: PrimitiveValue) : Sum<int64, Errors> =
      match v with
      | PrimitiveValue.Int64 i -> sum.Return i
      | other -> sum.Throw(Errors.Singleton $"Expected an int64 but got {other}")

    static member AsFloat32(v: PrimitiveValue) : Sum<float32, Errors> =
      match v with
      | PrimitiveValue.Float32 i -> sum.Return i
      | other -> sum.Throw(Errors.Singleton $"Expected a float32 but got {other}")

    static member AsFloat64(v: PrimitiveValue) : Sum<float, Errors> =
      match v with
      | PrimitiveValue.Float64 i -> sum.Return i
      | other -> sum.Throw(Errors.Singleton $"Expected a float64 but got {other}")

    static member AsDecimal(v: PrimitiveValue) : Sum<decimal, Errors> =
      match v with
      | PrimitiveValue.Decimal i -> sum.Return i
      | other -> sum.Throw(Errors.Singleton $"Expected a decimal but got {other}")

    static member AsBool(v: PrimitiveValue) : Sum<bool, Errors> =
      match v with
      | PrimitiveValue.Bool i -> sum.Return i
      | other -> sum.Throw(Errors.Singleton $"Expected a bool but got {other}")

    static member AsGuid(v: PrimitiveValue) : Sum<Guid, Errors> =
      match v with
      | PrimitiveValue.Guid i -> sum.Return i
      | other -> sum.Throw(Errors.Singleton $"Expected a guid but got {other}")

    static member AsString(v: PrimitiveValue) : Sum<string, Errors> =
      match v with
      | PrimitiveValue.String i -> sum.Return i
      | other -> sum.Throw(Errors.Singleton $"Expected a string but got {other}")

    static member AsDate(v: PrimitiveValue) : Sum<DateOnly, Errors> =
      match v with
      | PrimitiveValue.Date i -> sum.Return i
      | other -> sum.Throw(Errors.Singleton $"Expected a date but got {other}")

    static member AsDateTime(v: PrimitiveValue) : Sum<DateTime, Errors> =
      match v with
      | PrimitiveValue.DateTime i -> sum.Return i
      | other -> sum.Throw(Errors.Singleton $"Expected a datetime but got {other}")

    static member AsTimeSpan(v: PrimitiveValue) : Sum<TimeSpan, Errors> =
      match v with
      | PrimitiveValue.TimeSpan i -> sum.Return i
      | other -> sum.Throw(Errors.Singleton $"Expected a timespan but got {other}")

    static member AsUnit(v: PrimitiveValue) : Sum<unit, Errors> =
      match v with
      | PrimitiveValue.Unit -> sum.Return()
      | other -> sum.Throw(Errors.Singleton $"Expected a unit but got {other}")

  type Value<'T, 'valueExt> with
    static member AsRecord(v: Value<'T, 'valueExt>) : Sum<Map<ResolvedIdentifier, Value<'T, 'valueExt>>, Errors> =
      match v with
      | Value.Record m -> sum.Return m
      | other -> sum.Throw(Errors.Singleton $"Expected a record type but got {other}")

    static member AsTuple(v: Value<'T, 'valueExt>) : Sum<Value<'T, 'valueExt> list, Errors> =
      match v with
      | Value.Tuple vs -> sum.Return vs
      | other -> sum.Throw(Errors.Singleton $"Expected a tuple type but got {other}")

    static member AsUnion(v: Value<'T, 'valueExt>) : Sum<ResolvedIdentifier * Value<'T, 'valueExt>, Errors> =
      match v with
      | Value.UnionCase(s, v) -> sum.Return(s, v)
      | other -> sum.Throw(Errors.Singleton $"Expected a union type but got {other}")

    static member AsUnionCons(v: Value<'T, 'valueExt>) : Sum<ResolvedIdentifier, Errors> =
      match v with
      | Value.UnionCons(s) -> sum.Return(s)
      | other -> sum.Throw(Errors.Singleton $"Expected a union cons but got {other}")

    static member AsRecordDes(v: Value<'T, 'valueExt>) : Sum<ResolvedIdentifier, Errors> =
      match v with
      | Value.RecordDes(s) -> sum.Return(s)
      | other -> sum.Throw(Errors.Singleton $"Expected a record des but got {other}")

    static member AsSum(v: Value<'T, 'valueExt>) : Sum<SumConsSelector * Value<'T, 'valueExt>, Errors> =
      match v with
      | Value.Sum(i, v) -> sum.Return(i, v)
      | other -> sum.Throw(Errors.Singleton $"Expected a sum type but got {other}")

    static member AsPrimitive(v: Value<'T, 'valueExt>) : Sum<PrimitiveValue, Errors> =
      match v with
      | Value.Primitive p -> sum.Return p
      | other -> sum.Throw(Errors.Singleton $"Expected a primitive type but got {other}")

    static member AsVar(v: Value<'T, 'valueExt>) : Sum<Var, Errors> =
      match v with
      | Value.Var var -> sum.Return var
      | other -> sum.Throw(Errors.Singleton $"Expected a variable type but got {other}")

    static member AsLambda
      (v: Value<'T, 'valueExt>)
      : Sum<Var * Expr<'T, ResolvedIdentifier> * Map<ResolvedIdentifier, Value<'T, 'valueExt>> * TypeCheckScope, Errors> =
      match v with
      | Value.Lambda(v, e, closure, scope) -> sum.Return(v, e, closure, scope)
      | other -> sum.Throw(Errors.Singleton $"Expected a lambda but got {other}")

    static member AsTypeLamba(v: Value<'T, 'valueExt>) : Sum<TypeParameter * Expr<'T, ResolvedIdentifier>, Errors> =
      match v with
      | Value.TypeLambda(v, t) -> sum.Return(v, t)
      | other -> sum.Throw(Errors.Singleton $"Expected a type lambda but got {other}")

    static member AsExt(v: Value<'T, 'valueExt>) : Sum<'valueExt, Errors> =
      match v with
      | Value.Ext(v) -> sum.Return(v)
      | other -> sum.Throw(Errors.Singleton $"Expected an Ext but got {other}")

  type Expr<'T, 'Id when 'Id: comparison> with
    static member TypeLambda(p: TypeParameter, e: Expr<'T, 'Id>, loc: Location, scope: TypeCheckScope) =
      { Expr = ExprRec.TypeLambda({ Param = p; Body = e })
        Location = loc
        Scope = scope }

    static member TypeLambda(p: TypeParameter, e: Expr<'T, 'Id>) =
      Expr<'T, 'Id>.TypeLambda(p, e, Location.Unknown, TypeCheckScope.Empty)

    static member TypeApply(e1: Expr<'T, 'Id>, t: 'T, loc: Location, scope: TypeCheckScope) =
      { Expr = TypeApply({ ExprTypeApply.Func = e1; TypeArg = t })
        Location = loc
        Scope = scope }

    static member TypeApply(e1: Expr<'T, 'Id>, t: 'T) =
      Expr<'T, 'Id>.TypeApply(e1, t, Location.Unknown, TypeCheckScope.Empty)

    static member Lambda(v: Var, t: Option<'T>, e: Expr<'T, 'Id>, loc: Location, scope: TypeCheckScope) =
      { Expr = ExprRec.Lambda({ Param = v; ParamType = t; Body = e })
        Location = loc
        Scope = scope }

    static member Lambda(v: Var, t: Option<'T>, e: Expr<'T, 'Id>) =
      Expr<'T, 'Id>.Lambda(v, t, e, Location.Unknown, TypeCheckScope.Empty)

    static member Apply(f: Expr<'T, 'Id>, a: Expr<'T, 'Id>, loc: Location, scope: TypeCheckScope) =
      { Expr = Apply({ F = f; Arg = a })
        Location = loc
        Scope = scope }

    static member Apply(f: Expr<'T, 'Id>, a: Expr<'T, 'Id>) =
      Expr<'T, 'Id>.Apply(f, a, Location.Unknown, TypeCheckScope.Empty)

    static member Let(v: Var, t: Option<'T>, a: Expr<'T, 'Id>, e: Expr<'T, 'Id>, loc: Location, scope: TypeCheckScope) =
      { Expr = Let({ Var = v; Type = t; Val = a; Rest = e })
        Location = loc
        Scope = scope }

    static member Let(v: Var, t: Option<'T>, a: Expr<'T, 'Id>, e: Expr<'T, 'Id>) =
      Expr<'T, 'Id>.Let(v, t, a, e, Location.Unknown, TypeCheckScope.Empty)

    static member TypeLet(name: string, t: 'T, e: Expr<'T, 'Id>, loc: Location, scope: TypeCheckScope) =
      { Expr = TypeLet({ Name = name; TypeDef = t; Body = e })
        Location = loc
        Scope = scope }

    static member TypeLet(name: string, t: 'T, e: Expr<'T, 'Id>) =
      Expr<'T, 'Id>.TypeLet(name, t, e, Location.Unknown, TypeCheckScope.Empty)

    static member RecordCons(fields: List<'Id * Expr<'T, 'Id>>, loc: Location, scope: TypeCheckScope) =
      { Expr = RecordCons({ Fields = fields })
        Location = loc
        Scope = scope }

    static member RecordCons(fields: List<'Id * Expr<'T, 'Id>>) =
      Expr<'T, 'Id>.RecordCons(fields, Location.Unknown, TypeCheckScope.Empty)

    static member RecordWith
      (record: Expr<'T, 'Id>, fields: List<'Id * Expr<'T, 'Id>>, loc: Location, scope: TypeCheckScope)
      =
      { Expr = RecordWith({ Record = record; Fields = fields })
        Location = loc
        Scope = scope }

    static member RecordWith(record: Expr<'T, 'Id>, fields: List<'Id * Expr<'T, 'Id>>) =
      Expr<'T, 'Id>.RecordWith(record, fields, Location.Unknown, TypeCheckScope.Empty)

    static member TupleCons(elements: List<Expr<'T, 'Id>>, loc: Location, scope: TypeCheckScope) =
      { Expr = TupleCons({ Items = elements })
        Location = loc
        Scope = scope }

    static member TupleCons(elements: List<Expr<'T, 'Id>>) =
      Expr<'T, 'Id>.TupleCons(elements, Location.Unknown, TypeCheckScope.Empty)

    static member SumCons(selector: SumConsSelector, loc: Location, scope: TypeCheckScope) =
      { Expr = SumCons({ Selector = selector })
        Location = loc
        Scope = scope }

    static member SumCons(selector: SumConsSelector) =
      Expr<'T, 'Id>.SumCons(selector, Location.Unknown, TypeCheckScope.Empty)

    static member RecordDes(e: Expr<'T, 'Id>, id: 'Id, loc: Location, scope: TypeCheckScope) =
      { Expr = ExprRec.RecordDes({ Expr = e; Field = id })
        Location = loc
        Scope = scope }

    static member RecordDes(e: Expr<'T, 'Id>, id: 'Id) =
      Expr<'T, 'Id>.RecordDes(e, id, Location.Unknown, TypeCheckScope.Empty)

    static member UnionDes
      (cases: Map<'Id, CaseHandler<'T, 'Id>>, fallback: Option<Expr<'T, 'Id>>, loc: Location, scope: TypeCheckScope)
      =
      { Expr =
          ExprRec.UnionDes(
            { Handlers = cases
              Fallback = fallback }
          )
        Location = loc
        Scope = scope }

    static member UnionDes(cases: Map<'Id, CaseHandler<'T, 'Id>>, fallback: Option<Expr<'T, 'Id>>) =
      Expr<'T, 'Id>.UnionDes(cases, fallback, Location.Unknown, TypeCheckScope.Empty)

    static member TupleDes(e: Expr<'T, 'Id>, selector: TupleDesSelector, loc: Location, scope: TypeCheckScope) =
      { Expr = TupleDes({ Tuple = e; Item = selector })
        Location = loc
        Scope = scope }

    static member TupleDes(e: Expr<'T, 'Id>, selector: TupleDesSelector) =
      Expr<'T, 'Id>.TupleDes(e, selector, Location.Unknown, TypeCheckScope.Empty)

    static member SumDes(cases: Map<SumConsSelector, CaseHandler<'T, 'Id>>, loc: Location, scope: TypeCheckScope) =
      { Expr = SumDes({ Handlers = cases })
        Location = loc
        Scope = scope }

    static member SumDes(cases: Map<SumConsSelector, CaseHandler<'T, 'Id>>) =
      Expr<'T, 'Id>.SumDes(cases, Location.Unknown, TypeCheckScope.Empty)

    static member Primitive(p: PrimitiveValue, loc: Location, scope: TypeCheckScope) =
      { Expr = ExprRec.Primitive(p)
        Location = loc
        Scope = scope }

    static member Primitive(p: PrimitiveValue) =
      Expr<'T, 'Id>.Primitive(p, Location.Unknown, TypeCheckScope.Empty)

    static member Lookup(id: 'Id, loc: Location, scope: TypeCheckScope) =
      { Expr = Lookup({ ExprLookup.Id = id })
        Location = loc
        Scope = scope }

    static member Lookup(id: 'Id) =
      Expr<'T, 'Id>.Lookup(id, Location.Unknown, TypeCheckScope.Empty)

    static member If(c: Expr<'T, 'Id>, t: Expr<'T, 'Id>, f: Expr<'T, 'Id>, loc: Location, scope: TypeCheckScope) =
      { Expr = If({ Cond = c; Then = t; Else = f })
        Location = loc
        Scope = scope }

    static member If(c: Expr<'T, 'Id>, t: Expr<'T, 'Id>, f: Expr<'T, 'Id>) =
      Expr<'T, 'Id>.If(c, t, f, Location.Unknown, TypeCheckScope.Empty)

    static member AsUnionDes(e: Expr<'T, 'Id>) : Sum<ExprUnionDes<'T, 'Id>, Errors> =
      match e.Expr with
      | UnionDes union_des_expr -> sum.Return union_des_expr
      | other -> sum.Throw(Errors.Singleton $"Expected a union destruct but got {other}")

    static member AsTypeLet(e: Expr<'T, 'Id>) : Sum<ExprTypeLet<'T, 'Id>, Errors> =
      match e.Expr with
      | TypeLet typelet_expr -> sum.Return typelet_expr
      | other -> sum.Throw(Errors.Singleton $"Expected a type let but got {other}")

    static member AsTypeLambda(e: Expr<'T, 'Id>) : Sum<ExprTypeLambda<'T, 'Id>, Errors> =
      match e.Expr with
      | ExprRec.TypeLambda type_lambda_expr -> sum.Return type_lambda_expr
      | other -> sum.Throw(Errors.Singleton $"Expected a type lambda but got {other}")

    static member AsTypeApply(e: Expr<'T, 'Id>) : Sum<ExprTypeApply<'T, 'Id>, Errors> =
      match e.Expr with
      | TypeApply typeapply_expr -> sum.Return typeapply_expr
      | other -> sum.Throw(Errors.Singleton $"Expected a type apply but got {other}")

    static member AsTupleDes(e: Expr<'T, 'Id>) : Sum<ExprTupleDes<'T, 'Id>, Errors> =
      match e.Expr with
      | TupleDes tuple_des_expr -> sum.Return tuple_des_expr
      | other -> sum.Throw(Errors.Singleton $"Expected a tuple destruct but got {other}")

    static member AsTupleCons(e: Expr<'T, 'Id>) : Sum<ExprTupleCons<'T, 'Id>, Errors> =
      match e.Expr with
      | TupleCons es -> sum.Return es
      | other -> sum.Throw(Errors.Singleton $"Expected a tuple construct but got {other}")

    static member AsSumDes(e: Expr<'T, 'Id>) : Sum<ExprSumDes<'T, 'Id>, Errors> =
      match e.Expr with
      | SumDes sum_des_expr -> sum.Return sum_des_expr
      | other -> sum.Throw(Errors.Singleton $"Expected a sum destruct but got {other}")

    static member AsSumCons(e: Expr<'T, 'Id>) : Sum<ExprSumCons<'T, 'Id>, Errors> =
      match e.Expr with
      | SumCons sum_expr -> sum.Return sum_expr
      | other -> sum.Throw(Errors.Singleton $"Expected a sum construct but got {other}")

    static member AsRecordDes(e: Expr<'T, 'Id>) : Sum<ExprRecordDes<'T, 'Id>, Errors> =
      match e.Expr with
      | ExprRec.RecordDes record_des_expr -> sum.Return record_des_expr
      | other -> sum.Throw(Errors.Singleton $"Expected a record destruct but got {other}")

    static member AsRecordCons(e: Expr<'T, 'Id>) : Sum<ExprRecordCons<'T, 'Id>, Errors> =
      match e.Expr with
      | ExprRec.RecordCons m -> sum.Return m
      | other -> sum.Throw(Errors.Singleton $"Expected a record construct but got {other}")

    static member AsPrimitive(e: Expr<'T, 'Id>) : Sum<PrimitiveValue, Errors> =
      match e.Expr with
      | ExprRec.Primitive p -> sum.Return p
      | other -> sum.Throw(Errors.Singleton $"Expected a primitive type but got {other}")

    static member AsLookup(e: Expr<'T, 'Id>) : Sum<ExprLookup<'T, 'Id>, Errors> =
      match e.Expr with
      | Lookup l -> sum.Return l
      | other -> sum.Throw(Errors.Singleton $"Expected a lookup but got {other}")

    static member AsLet(e: Expr<'T, 'Id>) : Sum<ExprLet<'T, 'Id>, Errors> =
      match e.Expr with
      | Let let_expr -> sum.Return let_expr
      | other -> sum.Throw(Errors.Singleton $"Expected a let but got {other}")

    static member AsLambda(e: Expr<'T, 'Id>) : Sum<ExprLambda<'T, 'Id>, Errors> =
      match e.Expr with
      | ExprRec.Lambda(lambda) -> sum.Return(lambda)
      | other -> sum.Throw(Errors.Singleton $"Expected a lambda but got {other}")

    static member AsIf(e: Expr<'T, 'Id>) : Sum<ExprIf<'T, 'Id>, Errors> =
      match e.Expr with
      | If if_expr -> sum.Return if_expr
      | other -> sum.Throw(Errors.Singleton $"Expected an if expression but got {other}")

    static member AsApply(e: Expr<'T, 'Id>) : Sum<ExprApply<'T, 'Id>, Errors> =
      match e.Expr with
      | Apply apply -> sum.Return apply
      | other -> sum.Throw(Errors.Singleton $"Expected an apply expression but got {other}")

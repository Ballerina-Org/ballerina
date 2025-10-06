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
    static member AsRecord(v: Value<'T, 'valueExt>) : Sum<Map<TypeSymbol, Value<'T, 'valueExt>>, Errors> =
      match v with
      | Value.Record m -> sum.Return m
      | other -> sum.Throw(Errors.Singleton $"Expected a record type but got {other}")

    static member AsTuple(v: Value<'T, 'valueExt>) : Sum<Value<'T, 'valueExt> list, Errors> =
      match v with
      | Value.Tuple vs -> sum.Return vs
      | other -> sum.Throw(Errors.Singleton $"Expected a tuple type but got {other}")

    static member AsUnion(v: Value<'T, 'valueExt>) : Sum<TypeSymbol * Value<'T, 'valueExt>, Errors> =
      match v with
      | Value.UnionCase(s, v) -> sum.Return(s, v)
      | other -> sum.Throw(Errors.Singleton $"Expected a union type but got {other}")

    static member AsSum(v: Value<'T, 'valueExt>) : Sum<int * Value<'T, 'valueExt>, Errors> =
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

    static member AsLambda(v: Value<'T, 'valueExt>) : Sum<Var * Expr<'T>, Errors> =
      match v with
      | Value.Lambda(v, e) -> sum.Return(v, e)
      | other -> sum.Throw(Errors.Singleton $"Expected a lambda but got {other}")

    static member AsTypeLamba(v: Value<'T, 'valueExt>) : Sum<TypeParameter * Expr<'T>, Errors> =
      match v with
      | Value.TypeLambda(v, t) -> sum.Return(v, t)
      | other -> sum.Throw(Errors.Singleton $"Expected a type lambda but got {other}")

    static member AsExt(v: Value<'T, 'valueExt>) : Sum<'valueExt, Errors> =
      match v with
      | Value.Ext(v) -> sum.Return(v)
      | other -> sum.Throw(Errors.Singleton $"Expected an Ext but got {other}")

  type Expr<'T> with
    static member TypeLambda(p: TypeParameter, e: Expr<'T>, loc: Location) =
      { Expr = ExprRec.TypeLambda(p, e)
        Location = loc }

    static member TypeLambda(p: TypeParameter, e: Expr<'T>) =
      Expr<'T>.TypeLambda(p, e, Location.Unknown)

    static member TypeApply(e1: Expr<'T>, t: 'T, loc: Location) =
      { Expr = TypeApply(e1, t)
        Location = loc }

    static member TypeApply(e1: Expr<'T>, t: 'T) =
      Expr<'T>.TypeApply(e1, t, Location.Unknown)

    static member Lambda(v: Var, t: Option<'T>, e: Expr<'T>, loc: Location) =
      { Expr = ExprRec.Lambda(v, t, e)
        Location = loc }

    static member Lambda(v: Var, t: Option<'T>, e: Expr<'T>) =
      Expr<'T>.Lambda(v, t, e, Location.Unknown)

    static member Apply(f: Expr<'T>, a: Expr<'T>, loc: Location) = { Expr = Apply(f, a); Location = loc }

    static member Apply(f: Expr<'T>, a: Expr<'T>) = Expr<'T>.Apply(f, a, Location.Unknown)

    static member Let(v: Var, t: Option<'T>, a: Expr<'T>, e: Expr<'T>, loc: Location) =
      { Expr = Let(v, t, a, e)
        Location = loc }

    static member Let(v: Var, t: Option<'T>, a: Expr<'T>, e: Expr<'T>) =
      Expr<'T>.Let(v, t, a, e, Location.Unknown)

    static member TypeLet(name: string, t: 'T, e: Expr<'T>, loc: Location) =
      { Expr = TypeLet(name, t, e)
        Location = loc }

    static member TypeLet(name: string, t: 'T, e: Expr<'T>) =
      Expr<'T>.TypeLet(name, t, e, Location.Unknown)

    static member RecordCons(fields: List<Identifier * Expr<'T>>, loc: Location) =
      { Expr = RecordCons(fields)
        Location = loc }

    static member RecordCons(fields: List<Identifier * Expr<'T>>) =
      Expr<'T>.RecordCons(fields, Location.Unknown)

    static member UnionCons(id: Identifier, e: Expr<'T>, loc: Location) =
      { Expr = UnionCons(id, e)
        Location = loc }

    static member UnionCons(id: Identifier, e: Expr<'T>) =
      Expr<'T>.UnionCons(id, e, Location.Unknown)

    static member TupleCons(elements: List<Expr<'T>>, loc: Location) =
      { Expr = TupleCons(elements)
        Location = loc }

    static member TupleCons(elements: List<Expr<'T>>) =
      Expr<'T>.TupleCons(elements, Location.Unknown)

    static member SumCons(selector: SumConsSelector, e: Expr<'T>, loc: Location) =
      { Expr = SumCons(selector, e)
        Location = loc }

    static member SumCons(selector: SumConsSelector, e: Expr<'T>) =
      Expr<'T>.SumCons(selector, e, Location.Unknown)

    static member RecordDes(e: Expr<'T>, id: Identifier, loc: Location) =
      { Expr = RecordDes(e, id)
        Location = loc }

    static member RecordDes(e: Expr<'T>, id: Identifier) =
      Expr<'T>.RecordDes(e, id, Location.Unknown)

    static member UnionDes(cases: Map<Identifier, CaseHandler<'T>>, fallback: Option<Expr<'T>>, loc: Location) =
      { Expr = UnionDes(cases, fallback)
        Location = loc }

    static member UnionDes(cases: Map<Identifier, CaseHandler<'T>>, fallback: Option<Expr<'T>>) =
      Expr<'T>.UnionDes(cases, fallback, Location.Unknown)

    static member TupleDes(e: Expr<'T>, selector: TupleDesSelector, loc: Location) =
      { Expr = TupleDes(e, selector)
        Location = loc }

    static member TupleDes(e: Expr<'T>, selector: TupleDesSelector) =
      Expr<'T>.TupleDes(e, selector, Location.Unknown)

    static member SumDes(cases: List<CaseHandler<'T>>, loc: Location) =
      { Expr = SumDes(cases); Location = loc }

    static member SumDes(cases: List<CaseHandler<'T>>) =
      Expr<'T>.SumDes(cases, Location.Unknown)

    static member Primitive(p: PrimitiveValue, loc: Location) =
      { Expr = ExprRec.Primitive(p)
        Location = loc }

    static member Primitive(p: PrimitiveValue) = Expr<'T>.Primitive(p, Location.Unknown)

    static member Lookup(id: Identifier, loc: Location) = { Expr = Lookup(id); Location = loc }

    static member Lookup(id: Identifier) = Expr<'T>.Lookup(id, Location.Unknown)

    static member If(c: Expr<'T>, t: Expr<'T>, f: Expr<'T>, loc: Location) = { Expr = If(c, t, f); Location = loc }

    static member If(c: Expr<'T>, t: Expr<'T>, f: Expr<'T>) = Expr<'T>.If(c, t, f, Location.Unknown)

    static member AsUnionDes(e: Expr<'T>) : Sum<Map<Identifier, CaseHandler<'T>> * Option<Expr<'T>>, Errors> =
      match e.Expr with
      | UnionDes(m, f) -> sum.Return(m, f)
      | other -> sum.Throw(Errors.Singleton $"Expected a union destruct but got {other}")

    static member AsUnionCons(e: Expr<'T>) : Sum<Identifier * Expr<'T>, Errors> =
      match e.Expr with
      | UnionCons(s, m) -> sum.Return(s, m)
      | other -> sum.Throw(Errors.Singleton $"Expected a union construct but got {other}")

    static member AsTypeLet(e: Expr<'T>) : Sum<string * 'T * Expr<'T>, Errors> =
      match e.Expr with
      | TypeLet(i, a, e) -> sum.Return(i, a, e)
      | other -> sum.Throw(Errors.Singleton $"Expected a type let but got {other}")

    static member AsTypeLambda(e: Expr<'T>) : Sum<TypeParameter * Expr<'T>, Errors> =
      match e.Expr with
      | ExprRec.TypeLambda(v, t) -> sum.Return(v, t)
      | other -> sum.Throw(Errors.Singleton $"Expected a type lambda but got {other}")

    static member AsTypeApply(e: Expr<'T>) : Sum<Expr<'T> * 'T, Errors> =
      match e.Expr with
      | TypeApply(i, e) -> sum.Return(i, e)
      | other -> sum.Throw(Errors.Singleton $"Expected a type apply but got {other}")

    static member AsTupleDes(e: Expr<'T>) : Sum<Expr<'T> * TupleDesSelector, Errors> =
      match e.Expr with
      | TupleDes(e, d) -> sum.Return(e, d)
      | other -> sum.Throw(Errors.Singleton $"Expected a tuple destruct but got {other}")

    static member AsTupleCons(e: Expr<'T>) : Sum<Expr<'T> list, Errors> =
      match e.Expr with
      | TupleCons es -> sum.Return es
      | other -> sum.Throw(Errors.Singleton $"Expected a tuple construct but got {other}")

    static member AsSumDes(e: Expr<'T>) : Sum<List<CaseHandler<'T>>, Errors> =
      match e.Expr with
      | SumDes m -> sum.Return m
      | other -> sum.Throw(Errors.Singleton $"Expected a sum destruct but got {other}")

    static member AsSumCons(e: Expr<'T>) : Sum<SumConsSelector * Expr<'T>, Errors> =
      match e.Expr with
      | SumCons(i, m) -> sum.Return(i, m)
      | other -> sum.Throw(Errors.Singleton $"Expected a sum construct but got {other}")

    static member AsRecordDes(e: Expr<'T>) : Sum<Expr<'T> * Identifier, Errors> =
      match e.Expr with
      | RecordDes(e, s) -> sum.Return(e, s)
      | other -> sum.Throw(Errors.Singleton $"Expected a record destruct but got {other}")

    static member AsRecordCons(e: Expr<'T>) : Sum<List<Identifier * Expr<'T>>, Errors> =
      match e.Expr with
      | RecordCons m -> sum.Return m
      | other -> sum.Throw(Errors.Singleton $"Expected a record construct but got {other}")

    static member AsPrimitive(e: Expr<'T>) : Sum<PrimitiveValue, Errors> =
      match e.Expr with
      | ExprRec.Primitive p -> sum.Return p
      | other -> sum.Throw(Errors.Singleton $"Expected a primitive type but got {other}")

    static member AsLookup(e: Expr<'T>) : Sum<Identifier, Errors> =
      match e.Expr with
      | Lookup l -> sum.Return l
      | other -> sum.Throw(Errors.Singleton $"Expected a lookup but got {other}")

    static member AsLet(e: Expr<'T>) : Sum<Var * Option<'T> * Expr<'T> * Expr<'T>, Errors> =
      match e.Expr with
      | Let(i, i_t, a, e) -> sum.Return(i, i_t, a, e)
      | other -> sum.Throw(Errors.Singleton $"Expected a let but got {other}")

    static member AsLambda(e: Expr<'T>) : Sum<Var * Option<'T> * Expr<'T>, Errors> =
      match e.Expr with
      | ExprRec.Lambda(v, t, e) -> sum.Return(v, t, e)
      | other -> sum.Throw(Errors.Singleton $"Expected a lambda but got {other}")

    static member AsIf(e: Expr<'T>) : Sum<Expr<'T> * Expr<'T> * Expr<'T>, Errors> =
      match e.Expr with
      | If(c, t, f) -> sum.Return(c, t, f)
      | other -> sum.Throw(Errors.Singleton $"Expected an if expression but got {other}")

    static member AsApply(e: Expr<'T>) : Sum<Expr<'T> * Expr<'T>, Errors> =
      match e.Expr with
      | Apply(f, a) -> sum.Return(f, a)
      | other -> sum.Throw(Errors.Singleton $"Expected an apply expression but got {other}")

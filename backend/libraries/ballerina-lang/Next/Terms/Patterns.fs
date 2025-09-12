namespace Ballerina.DSL.Next.Terms

open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Types.Model

module Patterns =
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open System
  open Ballerina.DSL.Next.Terms.Model

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
    static member AsUnionDes(e: Expr<'T>) : Sum<Map<Identifier, CaseHandler<'T>>, Errors> =
      match e with
      | Expr.UnionDes m -> sum.Return m
      | other -> sum.Throw(Errors.Singleton $"Expected a union destruct but got {other}")

    static member AsUnionCons(e: Expr<'T>) : Sum<Identifier * Expr<'T>, Errors> =
      match e with
      | Expr.UnionCons(s, m) -> sum.Return(s, m)
      | other -> sum.Throw(Errors.Singleton $"Expected a union construct but got {other}")

    static member AsTypeLet(e: Expr<'T>) : Sum<string * 'T * Expr<'T>, Errors> =
      match e with
      | Expr.TypeLet(i, a, e) -> sum.Return(i, a, e)
      | other -> sum.Throw(Errors.Singleton $"Expected a type let but got {other}")

    static member AsTypeLambda(e: Expr<'T>) : Sum<TypeParameter * Expr<'T>, Errors> =
      match e with
      | Expr.TypeLambda(v, t) -> sum.Return(v, t)
      | other -> sum.Throw(Errors.Singleton $"Expected a type lambda but got {other}")

    static member AsTypeApply(e: Expr<'T>) : Sum<Expr<'T> * 'T, Errors> =
      match e with
      | Expr.TypeApply(i, e) -> sum.Return(i, e)
      | other -> sum.Throw(Errors.Singleton $"Expected a type apply but got {other}")

    static member AsTupleDes(e: Expr<'T>) : Sum<Expr<'T> * TupleDesSelector, Errors> =
      match e with
      | Expr.TupleDes(e, d) -> sum.Return(e, d)
      | other -> sum.Throw(Errors.Singleton $"Expected a tuple destruct but got {other}")

    static member AsTupleCons(e: Expr<'T>) : Sum<Expr<'T> list, Errors> =
      match e with
      | Expr.TupleCons es -> sum.Return es
      | other -> sum.Throw(Errors.Singleton $"Expected a tuple construct but got {other}")

    static member AsSumDes(e: Expr<'T>) : Sum<List<CaseHandler<'T>>, Errors> =
      match e with
      | Expr.SumDes m -> sum.Return m
      | other -> sum.Throw(Errors.Singleton $"Expected a sum destruct but got {other}")

    static member AsSumCons(e: Expr<'T>) : Sum<SumConsSelector * Expr<'T>, Errors> =
      match e with
      | Expr.SumCons(i, m) -> sum.Return(i, m)
      | other -> sum.Throw(Errors.Singleton $"Expected a sum construct but got {other}")

    static member AsRecordDes(e: Expr<'T>) : Sum<Expr<'T> * Identifier, Errors> =
      match e with
      | Expr.RecordDes(e, s) -> sum.Return(e, s)
      | other -> sum.Throw(Errors.Singleton $"Expected a record destruct but got {other}")

    static member AsRecordCons(e: Expr<'T>) : Sum<List<Identifier * Expr<'T>>, Errors> =
      match e with
      | Expr.RecordCons m -> sum.Return m
      | other -> sum.Throw(Errors.Singleton $"Expected a record construct but got {other}")

    static member AsPrimitive(e: Expr<'T>) : Sum<PrimitiveValue, Errors> =
      match e with
      | Expr.Primitive p -> sum.Return p
      | other -> sum.Throw(Errors.Singleton $"Expected a primitive type but got {other}")

    static member AsLookup(e: Expr<'T>) : Sum<Identifier, Errors> =
      match e with
      | Expr.Lookup l -> sum.Return l
      | other -> sum.Throw(Errors.Singleton $"Expected a lookup but got {other}")

    static member AsLet(e: Expr<'T>) : Sum<Var * Expr<'T> * Expr<'T>, Errors> =
      match e with
      | Expr.Let(i, a, e) -> sum.Return(i, a, e)
      | other -> sum.Throw(Errors.Singleton $"Expected a let but got {other}")

    static member AsLambda(e: Expr<'T>) : Sum<Var * Option<'T> * Expr<'T>, Errors> =
      match e with
      | Expr.Lambda(v, t, e) -> sum.Return(v, t, e)
      | other -> sum.Throw(Errors.Singleton $"Expected a lambda but got {other}")

    static member AsIf(e: Expr<'T>) : Sum<Expr<'T> * Expr<'T> * Expr<'T>, Errors> =
      match e with
      | Expr.If(c, t, f) -> sum.Return(c, t, f)
      | other -> sum.Throw(Errors.Singleton $"Expected an if expression but got {other}")

    static member AsApply(e: Expr<'T>) : Sum<Expr<'T> * Expr<'T>, Errors> =
      match e with
      | Expr.Apply(f, a) -> sum.Return(f, a)
      | other -> sum.Throw(Errors.Singleton $"Expected an apply expression but got {other}")

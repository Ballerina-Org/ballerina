namespace Ballerina.DSL.Next.Terms

open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Types
open Ballerina.DSL.Next.Types.Model

module Patterns =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open System
  open Ballerina.DSL.Next.Terms.Model

  type Location = Ballerina.LocalizedErrors.Location

  type Var with
    static member Create(name: string) : Var = { Name = name }

  type PrimitiveValue with
    static member AsInt32(v: PrimitiveValue) : Sum<int32, Errors<Unit>> =
      match v with
      | PrimitiveValue.Int32 i -> sum.Return i
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () -> $"Expected an int but got {other}")
        )

    static member AsInt64(v: PrimitiveValue) : Sum<int64, Errors<Unit>> =
      match v with
      | PrimitiveValue.Int64 i -> sum.Return i
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () -> $"Expected an int64 but got {other}")
        )

    static member AsFloat32(v: PrimitiveValue) : Sum<float32, Errors<Unit>> =
      match v with
      | PrimitiveValue.Float32 i -> sum.Return i
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () -> $"Expected a float32 but got {other}")
        )

    static member AsFloat64(v: PrimitiveValue) : Sum<float, Errors<Unit>> =
      match v with
      | PrimitiveValue.Float64 i -> sum.Return i
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () -> $"Expected a float64 but got {other}")
        )

    static member AsDecimal(v: PrimitiveValue) : Sum<decimal, Errors<Unit>> =
      match v with
      | PrimitiveValue.Decimal i -> sum.Return i
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () -> $"Expected a decimal but got {other}")
        )

    static member AsBool(v: PrimitiveValue) : Sum<bool, Errors<Unit>> =
      match v with
      | PrimitiveValue.Bool i -> sum.Return i
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () -> $"Expected a bool but got {other}")
        )

    static member AsGuid(v: PrimitiveValue) : Sum<Guid, Errors<Unit>> =
      match v with
      | PrimitiveValue.Guid i -> sum.Return i
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () -> $"Expected a guid but got {other}")
        )

    static member AsString(v: PrimitiveValue) : Sum<string, Errors<Unit>> =
      match v with
      | PrimitiveValue.String i -> sum.Return i
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () -> $"Expected a string but got {other}")
        )

    static member AsDate(v: PrimitiveValue) : Sum<DateOnly, Errors<Unit>> =
      match v with
      | PrimitiveValue.Date i -> sum.Return i
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () -> $"Expected a date but got {other}")
        )

    static member AsDateTime(v: PrimitiveValue) : Sum<DateTime, Errors<Unit>> =
      match v with
      | PrimitiveValue.DateTime i -> sum.Return i
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () -> $"Expected a datetime but got {other}")
        )

    static member AsTimeSpan(v: PrimitiveValue) : Sum<TimeSpan, Errors<Unit>> =
      match v with
      | PrimitiveValue.TimeSpan i -> sum.Return i
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () -> $"Expected a timespan but got {other}")
        )

    static member AsUnit(v: PrimitiveValue) : Sum<unit, Errors<Unit>> =
      match v with
      | PrimitiveValue.Unit -> sum.Return()
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () -> $"Expected a unit but got {other}")
        )

  type Value<'T, 'valueExt> with
    static member AsRecord
      (v: Value<'T, 'valueExt>)
      : Sum<Map<ResolvedIdentifier, Value<'T, 'valueExt>>, Errors<Unit>> =
      match v with
      | Value.Record m -> sum.Return m
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () ->
            $"Expected a record type but got {other}")
        )

    static member AsTuple
      (v: Value<'T, 'valueExt>)
      : Sum<Value<'T, 'valueExt> list, Errors<Unit>> =
      match v with
      | Value.Tuple vs -> sum.Return vs
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () ->
            $"Expected a tuple type but got {other}")
        )

    static member AsTuple2
      (v: Value<'T, 'valueExt>)
      : Sum<Value<'T, 'valueExt> * Value<'T, 'valueExt>, Errors<Unit>> =
      sum {
        let! elements = Value.AsTuple v

        match elements with
        | [ e1; e2 ] -> return (e1, e2)
        | other ->
          return!
            Right
            <| Errors.Singleton () (fun () ->
              $"Expected tuple pair but got {other}")
      }

    static member AsTuple3
      (v: Value<'T, 'valueExt>)
      : Sum<
          Value<'T, 'valueExt> * Value<'T, 'valueExt> * Value<'T, 'valueExt>,
          Errors<Unit>
         >
      =
      sum {
        let! elements = Value.AsTuple v

        match elements with
        | [ e1; e2; e3 ] -> return (e1, e2, e3)
        | other ->
          return!
            Right
            <| Errors.Singleton () (fun () ->
              $"Expected tuple triplet but got {other}")
      }

    static member AsUnion
      (v: Value<'T, 'valueExt>)
      : Sum<ResolvedIdentifier * Value<'T, 'valueExt>, Errors<Unit>> =
      match v with
      | Value.UnionCase(s, v) -> sum.Return(s, v)
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () ->
            $"Expected a union type but got {other}")
        )

    static member AsUnionCons
      (v: Value<'T, 'valueExt>)
      : Sum<ResolvedIdentifier, Errors<Unit>> =
      match v with
      | Value.UnionCons(s) -> sum.Return(s)
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () ->
            $"Expected a union cons but got {other}")
        )

    static member AsRecordDes
      (v: Value<'T, 'valueExt>)
      : Sum<ResolvedIdentifier, Errors<Unit>> =
      match v with
      | Value.RecordDes(s) -> sum.Return(s)
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () ->
            $"Expected a record des but got {other}")
        )

    static member AsSum
      (v: Value<'T, 'valueExt>)
      : Sum<SumConsSelector * Value<'T, 'valueExt>, Errors<Unit>> =
      match v with
      | Value.Sum(i, v) -> sum.Return(i, v)
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () -> $"Expected a sum type but got {other}")
        )

    static member AsPrimitive
      (v: Value<'T, 'valueExt>)
      : Sum<PrimitiveValue, Errors<Unit>> =
      match v with
      | Value.Primitive p -> sum.Return p
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () ->
            $"Expected a primitive type but got {other}")
        )

    static member AsUnit(v: Value<'T, 'valueExt>) : Sum<Unit, Errors<Unit>> =
      match v with
      | Value.Primitive PrimitiveValue.Unit -> sum.Return()
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () -> $"Expected a unit but got {other}")
        )

    static member AsVar(v: Value<'T, 'valueExt>) : Sum<Var, Errors<Unit>> =
      match v with
      | Value.Var var -> sum.Return var
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () ->
            $"Expected a variable type but got {other}")
        )

    static member AsQuery
      (v: Value<'T, 'valueExt>)
      : Sum<ValueQuery<'T, 'valueExt>, Errors<Unit>> =
      match v with
      | Value.Query q -> sum.Return q
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () ->
            $"Expected a query type but got {other}")
        )

    static member AsLambda
      (v: Value<'T, 'valueExt>)
      : Sum<
          Var *
          RunnableExpr<'valueExt> *
          Map<ResolvedIdentifier, Value<'T, 'valueExt>> *
          TypeCheckScope,
          Errors<Unit>
         >
      =
      match v with
      | Value.Lambda(v, e, closure, scope) -> sum.Return(v, e, closure, scope)
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () -> $"Expected a lambda but got {other}")
        )

    static member AsTypeLamba
      (v: Value<'T, 'valueExt>)
      : Sum<TypeParameter * RunnableExpr<'valueExt>, Errors<Unit>> =
      match v with
      | Value.TypeLambda(v, t) -> sum.Return(v, t)
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () ->
            $"Expected a type lambda but got {other}")
        )

    static member AsExt
      (v: Value<'T, 'valueExt>)
      : Sum<'valueExt * Option<ResolvedIdentifier>, Errors<Unit>> =
      match v with
      | Value.Ext(v, app_id) -> sum.Return(v, app_id)
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () -> $"Expected an Ext but got {other}")
        )

  type Expr<'T, 'Id, 'valueExt when 'Id: comparison> with
    static member TypeLambda
      (
        p: TypeParameter,
        e: Expr<'T, 'Id, 'valueExt>,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { Expr = ExprRec.TypeLambda({ Param = p; Body = e })
        Location = loc
        Scope = scope }

    static member TypeLambda(p: TypeParameter, e: Expr<'T, 'Id, 'valueExt>) =
      Expr<'T, 'Id, 'valueExt>
        .TypeLambda(p, e, Location.Unknown, TypeCheckScope.Empty)

    static member TypeApply
      (e1: Expr<'T, 'Id, 'valueExt>, t: 'T, loc: Location, scope: TypeCheckScope) =
      { Expr = TypeApply({ ExprTypeApply.Func = e1; TypeArg = t })
        Location = loc
        Scope = scope }

    static member TypeApply(e1: Expr<'T, 'Id, 'valueExt>, t: 'T) =
      Expr<'T, 'Id, 'valueExt>
        .TypeApply(e1, t, Location.Unknown, TypeCheckScope.Empty)

    static member Lambda
      (
        v: Var,
        t: Option<'T>,
        e: Expr<'T, 'Id, 'valueExt>,
        r: Option<'T>,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { Expr =
          ExprRec.Lambda(
            { Param = v
              ParamType = t
              Body = e
              BodyType = r }
          )
        Location = loc
        Scope = scope }

    static member Lambda
      (v: Var, t: Option<'T>, e: Expr<'T, 'Id, 'valueExt>, r: Option<'T>)
      =
      Expr<'T, 'Id, 'valueExt>
        .Lambda(v, t, e, r, Location.Unknown, TypeCheckScope.Empty)

    static member Apply
      (
        f: Expr<'T, 'Id, 'valueExt>,
        a: Expr<'T, 'Id, 'valueExt>,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { Expr = Apply({ F = f; Arg = a })
        Location = loc
        Scope = scope }

    static member Apply
      (f: Expr<'T, 'Id, 'valueExt>, a: Expr<'T, 'Id, 'valueExt>)
      =
      Expr<'T, 'Id, 'valueExt>
        .Apply(f, a, Location.Unknown, TypeCheckScope.Empty)

    static member Query
      (q: ExprQuery<'T, 'Id, 'valueExt>, loc: Location, scope: TypeCheckScope)
      =
      { Expr = ExprRec.Query(q)
        Location = loc
        Scope = scope }

    static member Query(q: ExprQuery<'T, 'Id, 'valueExt>) =
      Expr<'T, 'Id, 'valueExt>.Query(q, Location.Unknown, TypeCheckScope.Empty)

    static member FromValue
      (
        v: Value<TypeValue<'valueExt>, 'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { Expr =
          FromValue(
            { Value = v
              ValueType = t
              ValueKind = k }
          )
        Location = loc
        Scope = scope }

    static member FromValue
      (
        v: Value<TypeValue<'valueExt>, 'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      Expr<'T, 'Id, 'valueExt>
        .FromValue(v, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member Let
      (
        v: Var,
        t: Option<'T>,
        a: Expr<'T, 'Id, 'valueExt>,
        e: Expr<'T, 'Id, 'valueExt>,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { Expr = Let({ Var = v; Type = t; Val = a; Rest = e })
        Location = loc
        Scope = scope }

    static member Let
      (
        v: Var,
        t: Option<'T>,
        a: Expr<'T, 'Id, 'valueExt>,
        e: Expr<'T, 'Id, 'valueExt>
      ) =
      Expr<'T, 'Id, 'valueExt>
        .Let(v, t, a, e, Location.Unknown, TypeCheckScope.Empty)

    static member Do
      (
        a: Expr<'T, 'Id, 'valueExt>,
        e: Expr<'T, 'Id, 'valueExt>,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { Expr = Do({ Val = a; Rest = e })
        Location = loc
        Scope = scope }

    static member Do(a: Expr<'T, 'Id, 'valueExt>, e: Expr<'T, 'Id, 'valueExt>) =
      Expr<'T, 'Id, 'valueExt>.Do(a, e, Location.Unknown, TypeCheckScope.Empty)

    static member TypeLet
      (
        name: string,
        t: 'T,
        e: Expr<'T, 'Id, 'valueExt>,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { Expr = TypeLet({ Name = name; TypeDef = t; Body = e })
        Location = loc
        Scope = scope }

    static member TypeLet(name: string, t: 'T, e: Expr<'T, 'Id, 'valueExt>) =
      Expr<'T, 'Id, 'valueExt>
        .TypeLet(name, t, e, Location.Unknown, TypeCheckScope.Empty)

    static member RecordCons
      (
        fields: List<'Id * Expr<'T, 'Id, 'valueExt>>,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { Expr = RecordCons({ Fields = fields })
        Location = loc
        Scope = scope }

    static member RecordCons(fields: List<'Id * Expr<'T, 'Id, 'valueExt>>) =
      Expr<'T, 'Id, 'valueExt>
        .RecordCons(fields, Location.Unknown, TypeCheckScope.Empty)

    static member RecordWith
      (
        record: Expr<'T, 'Id, 'valueExt>,
        fields: List<'Id * Expr<'T, 'Id, 'valueExt>>,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { Expr = RecordWith({ Record = record; Fields = fields })
        Location = loc
        Scope = scope }

    static member RecordWith
      (
        record: Expr<'T, 'Id, 'valueExt>,
        fields: List<'Id * Expr<'T, 'Id, 'valueExt>>
      ) =
      Expr<'T, 'Id, 'valueExt>
        .RecordWith(record, fields, Location.Unknown, TypeCheckScope.Empty)

    static member TupleCons
      (
        elements: List<Expr<'T, 'Id, 'valueExt>>,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { Expr = TupleCons({ Items = elements })
        Location = loc
        Scope = scope }

    static member TupleCons(elements: List<Expr<'T, 'Id, 'valueExt>>) =
      Expr<'T, 'Id, 'valueExt>
        .TupleCons(elements, Location.Unknown, TypeCheckScope.Empty)

    static member SumCons
      (selector: SumConsSelector, loc: Location, scope: TypeCheckScope)
      =
      { Expr = SumCons({ Selector = selector })
        Location = loc
        Scope = scope }

    static member SumCons(selector: SumConsSelector) =
      Expr<'T, 'Id, 'valueExt>
        .SumCons(selector, Location.Unknown, TypeCheckScope.Empty)

    static member RecordDes
      (
        e: Expr<'T, 'Id, 'valueExt>,
        id: 'Id,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { Expr = ExprRec.RecordDes({ Expr = e; Field = id })
        Location = loc
        Scope = scope }

    static member RecordDes(e: Expr<'T, 'Id, 'valueExt>, id: 'Id) =
      Expr<'T, 'Id, 'valueExt>
        .RecordDes(e, id, Location.Unknown, TypeCheckScope.Empty)

    static member EntitiesDes
      (e: Expr<'T, 'Id, 'valueExt>, loc: Location, scope: TypeCheckScope)
      =
      { Expr = ExprRec.EntitiesDes({ Expr = e })
        Location = loc
        Scope = scope }

    static member EntitiesDes(e: Expr<'T, 'Id, 'valueExt>) =
      Expr<'T, 'Id, 'valueExt>
        .EntitiesDes(e, Location.Unknown, TypeCheckScope.Empty)

    static member RelationsDes
      (e: Expr<'T, 'Id, 'valueExt>, loc: Location, scope: TypeCheckScope)
      =
      { Expr = ExprRec.RelationsDes({ Expr = e })
        Location = loc
        Scope = scope }

    static member RelationsDes(e: Expr<'T, 'Id, 'valueExt>) =
      Expr<'T, 'Id, 'valueExt>
        .RelationsDes(e, Location.Unknown, TypeCheckScope.Empty)

    static member EntityDes
      (
        e: Expr<'T, 'Id, 'valueExt>,
        n: SchemaEntityName,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { Expr = ExprRec.EntityDes({ Expr = e; EntityName = n })
        Location = loc
        Scope = scope }

    static member EntityDes(e: Expr<'T, 'Id, 'valueExt>, n: SchemaEntityName) =
      Expr<'T, 'Id, 'valueExt>
        .EntityDes(e, n, Location.Unknown, TypeCheckScope.Empty)

    static member RelationDes
      (
        e: Expr<'T, 'Id, 'valueExt>,
        n: SchemaRelationName,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { Expr = ExprRec.RelationDes({ Expr = e; RelationName = n })
        Location = loc
        Scope = scope }

    static member RelationDes
      (e: Expr<'T, 'Id, 'valueExt>, n: SchemaRelationName)
      =
      Expr<'T, 'Id, 'valueExt>
        .RelationDes(e, n, Location.Unknown, TypeCheckScope.Empty)

    static member RelationLookupDes
      (
        e: Expr<'T, 'Id, 'valueExt>,
        n: SchemaRelationName,
        d: RelationLookupDirection,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { Expr =
          ExprRec.RelationLookupDes(
            { Expr = e
              RelationName = n
              Direction = d }
          )
        Location = loc
        Scope = scope }

    static member RelationLookupDes
      (
        e: Expr<'T, 'Id, 'valueExt>,
        n: SchemaRelationName,
        d: RelationLookupDirection
      ) =
      Expr<'T, 'Id, 'valueExt>
        .RelationLookupDes(e, n, d, Location.Unknown, TypeCheckScope.Empty)

    static member UnionDes
      (
        cases: Map<'Id, CaseHandler<'T, 'Id, 'valueExt>>,
        fallback: Option<Expr<'T, 'Id, 'valueExt>>,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { Expr =
          ExprRec.UnionDes(
            { Handlers = cases
              Fallback = fallback }
          )
        Location = loc
        Scope = scope }

    static member UnionDes
      (
        cases: Map<'Id, CaseHandler<'T, 'Id, 'valueExt>>,
        fallback: Option<Expr<'T, 'Id, 'valueExt>>
      ) =
      Expr<'T, 'Id, 'valueExt>
        .UnionDes(cases, fallback, Location.Unknown, TypeCheckScope.Empty)

    static member TupleDes
      (
        e: Expr<'T, 'Id, 'valueExt>,
        selector: TupleDesSelector,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { Expr = TupleDes({ Tuple = e; Item = selector })
        Location = loc
        Scope = scope }

    static member TupleDes
      (e: Expr<'T, 'Id, 'valueExt>, selector: TupleDesSelector)
      =
      Expr<'T, 'Id, 'valueExt>
        .TupleDes(e, selector, Location.Unknown, TypeCheckScope.Empty)

    static member SumDes
      (
        cases: Map<SumConsSelector, CaseHandler<'T, 'Id, 'valueExt>>,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { Expr = SumDes({ Handlers = cases })
        Location = loc
        Scope = scope }

    static member SumDes
      (cases: Map<SumConsSelector, CaseHandler<'T, 'Id, 'valueExt>>)
      =
      Expr<'T, 'Id, 'valueExt>
        .SumDes(cases, Location.Unknown, TypeCheckScope.Empty)

    static member Primitive
      (p: PrimitiveValue, loc: Location, scope: TypeCheckScope)
      =
      { Expr = ExprRec.Primitive(p)
        Location = loc
        Scope = scope }

    static member Primitive(p: PrimitiveValue) =
      Expr<'T, 'Id, 'valueExt>
        .Primitive(p, Location.Unknown, TypeCheckScope.Empty)

    static member Lookup(id: 'Id, loc: Location, scope: TypeCheckScope) =
      { Expr = Lookup({ ExprLookup.Id = id })
        Location = loc
        Scope = scope }

    static member Lookup(id: 'Id) =
      Expr<'T, 'Id, 'valueExt>
        .Lookup(id, Location.Unknown, TypeCheckScope.Empty)

    static member If
      (
        c: Expr<'T, 'Id, 'valueExt>,
        t: Expr<'T, 'Id, 'valueExt>,
        f: Expr<'T, 'Id, 'valueExt>,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { Expr = If({ Cond = c; Then = t; Else = f })
        Location = loc
        Scope = scope }

    static member If
      (
        c: Expr<'T, 'Id, 'valueExt>,
        t: Expr<'T, 'Id, 'valueExt>,
        f: Expr<'T, 'Id, 'valueExt>
      ) =
      Expr<'T, 'Id, 'valueExt>
        .If(c, t, f, Location.Unknown, TypeCheckScope.Empty)

    static member AsUnionDes
      (e: Expr<'T, 'Id, 'valueExt>)
      : Sum<ExprUnionDes<'T, 'Id, 'valueExt>, Errors<Unit>> =
      match e.Expr with
      | UnionDes union_des_expr -> sum.Return union_des_expr
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () ->
            $"Expected a union destruct but got {other}")
        )

    static member AsTypeLet
      (e: Expr<'T, 'Id, 'valueExt>)
      : Sum<ExprTypeLet<'T, 'Id, 'valueExt>, Errors<Unit>> =
      match e.Expr with
      | TypeLet typelet_expr -> sum.Return typelet_expr
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () -> $"Expected a type let but got {other}")
        )

    static member AsTypeLambda
      (e: Expr<'T, 'Id, 'valueExt>)
      : Sum<ExprTypeLambda<'T, 'Id, 'valueExt>, Errors<Unit>> =
      match e.Expr with
      | ExprRec.TypeLambda type_lambda_expr -> sum.Return type_lambda_expr
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () ->
            $"Expected a type lambda but got {other}")
        )

    static member AsTypeApply
      (e: Expr<'T, 'Id, 'valueExt>)
      : Sum<ExprTypeApply<'T, 'Id, 'valueExt>, Errors<Unit>> =
      match e.Expr with
      | TypeApply typeapply_expr -> sum.Return typeapply_expr
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () ->
            $"Expected a type apply but got {other}")
        )

    static member AsTupleDes
      (e: Expr<'T, 'Id, 'valueExt>)
      : Sum<ExprTupleDes<'T, 'Id, 'valueExt>, Errors<Unit>> =
      match e.Expr with
      | TupleDes tuple_des_expr -> sum.Return tuple_des_expr
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () ->
            $"Expected a tuple destruct but got {other}")
        )

    static member AsTupleCons
      (e: Expr<'T, 'Id, 'valueExt>)
      : Sum<ExprTupleCons<'T, 'Id, 'valueExt>, Errors<Unit>> =
      match e.Expr with
      | TupleCons es -> sum.Return es
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () ->
            $"Expected a tuple construct but got {other}")
        )

    static member AsSumDes
      (e: Expr<'T, 'Id, 'valueExt>)
      : Sum<ExprSumDes<'T, 'Id, 'valueExt>, Errors<Unit>> =
      match e.Expr with
      | SumDes sum_des_expr -> sum.Return sum_des_expr
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () ->
            $"Expected a sum destruct but got {other}")
        )

    static member AsSumCons
      (e: Expr<'T, 'Id, 'valueExt>)
      : Sum<ExprSumCons<'T, 'Id, 'valueExt>, Errors<Unit>> =
      match e.Expr with
      | SumCons sum_expr -> sum.Return sum_expr
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () ->
            $"Expected a sum construct but got {other}")
        )

    static member AsRecordDes
      (e: Expr<'T, 'Id, 'valueExt>)
      : Sum<ExprRecordDes<'T, 'Id, 'valueExt>, Errors<Unit>> =
      match e.Expr with
      | ExprRec.RecordDes record_des_expr -> sum.Return record_des_expr
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () ->
            $"Expected a record destruct but got {other}")
        )

    static member AsRecordCons
      (e: Expr<'T, 'Id, 'valueExt>)
      : Sum<ExprRecordCons<'T, 'Id, 'valueExt>, Errors<Unit>> =
      match e.Expr with
      | ExprRec.RecordCons m -> sum.Return m
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () ->
            $"Expected a record construct but got {other}")
        )

    static member AsPrimitive
      (e: Expr<'T, 'Id, 'valueExt>)
      : Sum<PrimitiveValue, Errors<Unit>> =
      match e.Expr with
      | ExprRec.Primitive p -> sum.Return p
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () ->
            $"Expected a primitive type but got {other}")
        )

    static member AsLookup
      (e: Expr<'T, 'Id, 'valueExt>)
      : Sum<ExprLookup<'T, 'Id, 'valueExt>, Errors<Unit>> =
      match e.Expr with
      | Lookup l -> sum.Return l
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () -> $"Expected a lookup but got {other}")
        )

    static member AsLet
      (e: Expr<'T, 'Id, 'valueExt>)
      : Sum<ExprLet<'T, 'Id, 'valueExt>, Errors<Unit>> =
      match e.Expr with
      | Let let_expr -> sum.Return let_expr
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () -> $"Expected a let but got {other}")
        )

    static member AsLambda
      (e: Expr<'T, 'Id, 'valueExt>)
      : Sum<ExprLambda<'T, 'Id, 'valueExt>, Errors<Unit>> =
      match e.Expr with
      | ExprRec.Lambda(lambda) -> sum.Return(lambda)
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () -> $"Expected a lambda but got {other}")
        )

    static member AsIf
      (e: Expr<'T, 'Id, 'valueExt>)
      : Sum<ExprIf<'T, 'Id, 'valueExt>, Errors<Unit>> =
      match e.Expr with
      | If if_expr -> sum.Return if_expr
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () ->
            $"Expected an if expression but got {other}")
        )

    static member AsApply
      (e: Expr<'T, 'Id, 'valueExt>)
      : Sum<ExprApply<'T, 'Id, 'valueExt>, Errors<Unit>> =
      match e.Expr with
      | Apply apply -> sum.Return apply
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () ->
            $"Expected an apply expression but got {other}")
        )

    static member AsQuery
      (e: Expr<'T, 'Id, 'valueExt>)
      : Sum<ExprQuery<'T, 'Id, 'valueExt>, Errors<Unit>> =
      match e.Expr with
      | ExprRec.Query q -> sum.Return q
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () ->
            $"Expected a query expression but got {other}")
        )

    static member AsTerminatedByConstantUnit
      (e: Expr<'T, 'Id, 'valueExt>)
      : Sum<Unit, Errors<Unit>> =
      match e.Expr with
      | ExprRec.Primitive PrimitiveValue.Unit -> sum.Return()
      | ExprRec.TypeLet({ Body = rest }) ->
        Expr<'T, 'Id, 'valueExt>.AsTerminatedByConstantUnit rest
      | ExprRec.Let({ Rest = rest }) ->
        Expr<'T, 'Id, 'valueExt>.AsTerminatedByConstantUnit rest
      | ExprRec.Do({ Rest = rest }) ->
        Expr<'T, 'Id, 'valueExt>.AsTerminatedByConstantUnit rest
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () ->
            $"Expected a termination by constant unit but got {other}")
        )

  type ExprQueryExpr<'T, 'Id, 'valueExt when 'Id: comparison> with
    static member AsLookup(e: ExprQueryExpr<'T, 'Id, 'valueExt>) =
      match e.Expr with
      | QueryLookup r -> sum.Return r
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () ->
            $"Expected a query lookup but got {other}")
        )

    static member AsTupleCons(e: ExprQueryExpr<'T, 'Id, 'valueExt>) =
      match e.Expr with
      | QueryTupleCons items -> sum.Return items
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () ->
            $"Expected a query tuple construct but got {other}")
        )

    static member AsRecordDes(e: ExprQueryExpr<'T, 'Id, 'valueExt>) =
      match e.Expr with
      | QueryRecordDes(r, id, isJson) -> sum.Return(r, id, isJson)
      | other ->
        sum.Throw(
          Errors.Singleton () (fun () ->
            $"Expected a query record destruct but got {other}")
        )

  type TypeCheckedExpr<'valueExt> with
    static member TypeLambda
      (
        p: TypeParameter,
        e: TypeCheckedExpr<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { TypeCheckedExpr.Expr =
          TypeCheckedExprRec.TypeLambda(
            { TypeCheckedExprTypeLambda.Param = p
              Body = e }
          )
        TypeCheckedExpr.Type = t
        TypeCheckedExpr.Kind = k
        TypeCheckedExpr.Location = loc
        TypeCheckedExpr.Scope = scope }

    static member TypeLambda
      (
        p: TypeParameter,
        e: TypeCheckedExpr<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      TypeCheckedExpr<'valueExt>
        .TypeLambda(p, e, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member TypeApply
      (
        e1: TypeCheckedExpr<'valueExt>,
        typeArg: TypeValue<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { TypeCheckedExpr.Expr =
          TypeCheckedExprRec.TypeApply(
            { TypeCheckedExprTypeApply.Func = e1
              TypeArg = typeArg }
          )
        TypeCheckedExpr.Type = t
        TypeCheckedExpr.Kind = k
        TypeCheckedExpr.Location = loc
        TypeCheckedExpr.Scope = scope }

    static member TypeApply
      (
        e1: TypeCheckedExpr<'valueExt>,
        typeArg: TypeValue<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      TypeCheckedExpr<'valueExt>
        .TypeApply(e1, typeArg, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member Lambda
      (
        v: Var,
        paramType: TypeValue<'valueExt>,
        e: TypeCheckedExpr<'valueExt>,
        returnType: TypeValue<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { TypeCheckedExpr.Expr =
          TypeCheckedExprRec.Lambda(
            { TypeCheckedExprLambda.Param = v
              ParamType = paramType
              Body = e
              BodyType = returnType }
          )
        TypeCheckedExpr.Type = t
        TypeCheckedExpr.Kind = k
        TypeCheckedExpr.Location = loc
        TypeCheckedExpr.Scope = scope }

    static member Lambda
      (
        v: Var,
        paramType: TypeValue<'valueExt>,
        e: TypeCheckedExpr<'valueExt>,
        returnType: TypeValue<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      TypeCheckedExpr<'valueExt>
        .Lambda(
          v,
          paramType,
          e,
          returnType,
          t,
          k,
          Location.Unknown,
          TypeCheckScope.Empty
        )

    static member Apply
      (
        f: TypeCheckedExpr<'valueExt>,
        a: TypeCheckedExpr<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { TypeCheckedExpr.Expr =
          TypeCheckedExprRec.Apply({ TypeCheckedExprApply.F = f; Arg = a })
        TypeCheckedExpr.Type = t
        TypeCheckedExpr.Kind = k
        TypeCheckedExpr.Location = loc
        TypeCheckedExpr.Scope = scope }

    static member Apply
      (
        f: TypeCheckedExpr<'valueExt>,
        a: TypeCheckedExpr<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      TypeCheckedExpr<'valueExt>
        .Apply(f, a, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member Query
      (
        q: TypeCheckedExprQuery<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { TypeCheckedExpr.Expr = TypeCheckedExprRec.Query(q)
        TypeCheckedExpr.Type = t
        TypeCheckedExpr.Kind = k
        TypeCheckedExpr.Location = loc
        TypeCheckedExpr.Scope = scope }

    static member Query
      (q: TypeCheckedExprQuery<'valueExt>, t: TypeValue<'valueExt>, k: Kind)
      =
      TypeCheckedExpr<'valueExt>
        .Query(q, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member View
      (
        v: TypeCheckedExprView<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { TypeCheckedExpr.Expr = TypeCheckedExprRec.View(v)
        TypeCheckedExpr.Type = t
        TypeCheckedExpr.Kind = k
        TypeCheckedExpr.Location = loc
        TypeCheckedExpr.Scope = scope }

    static member View
      (v: TypeCheckedExprView<'valueExt>, t: TypeValue<'valueExt>, k: Kind)
      =
      TypeCheckedExpr<'valueExt>
        .View(v, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member Co
      (
        c: TypeCheckedExprCo<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { TypeCheckedExpr.Expr = TypeCheckedExprRec.Co(c)
        TypeCheckedExpr.Type = t
        TypeCheckedExpr.Kind = k
        TypeCheckedExpr.Location = loc
        TypeCheckedExpr.Scope = scope }

    static member Co
      (c: TypeCheckedExprCo<'valueExt>, t: TypeValue<'valueExt>, k: Kind)
      =
      TypeCheckedExpr<'valueExt>
        .Co(c, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member FromValue
      (
        v: Value<TypeValue<'valueExt>, 'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { TypeCheckedExpr.Expr =
          TypeCheckedExprRec.FromValue(
            { TypeCheckedExprFromValue.Value = v
              ValueType = t
              ValueKind = k }
          )
        TypeCheckedExpr.Type = t
        TypeCheckedExpr.Kind = k
        TypeCheckedExpr.Location = loc
        TypeCheckedExpr.Scope = scope }

    static member FromValue
      (
        v: Value<TypeValue<'valueExt>, 'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      TypeCheckedExpr<'valueExt>
        .FromValue(v, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member Let
      (
        v: Var,
        varType: TypeValue<'valueExt>,
        a: TypeCheckedExpr<'valueExt>,
        e: TypeCheckedExpr<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { TypeCheckedExpr.Expr =
          TypeCheckedExprRec.Let(
            { TypeCheckedExprLet.Var = v
              Type = varType
              Val = a
              Rest = e }
          )
        TypeCheckedExpr.Type = t
        TypeCheckedExpr.Kind = k
        TypeCheckedExpr.Location = loc
        TypeCheckedExpr.Scope = scope }

    static member Let
      (
        v: Var,
        varType: TypeValue<'valueExt>,
        a: TypeCheckedExpr<'valueExt>,
        e: TypeCheckedExpr<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      TypeCheckedExpr<'valueExt>
        .Let(v, varType, a, e, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member Do
      (
        a: TypeCheckedExpr<'valueExt>,
        e: TypeCheckedExpr<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { TypeCheckedExpr.Expr =
          TypeCheckedExprRec.Do({ TypeCheckedExprDo.Val = a; Rest = e })
        TypeCheckedExpr.Type = t
        TypeCheckedExpr.Kind = k
        TypeCheckedExpr.Location = loc
        TypeCheckedExpr.Scope = scope }

    static member Do
      (
        a: TypeCheckedExpr<'valueExt>,
        e: TypeCheckedExpr<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      TypeCheckedExpr<'valueExt>
        .Do(a, e, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member TypeLet
      (
        name: string,
        typeDef: TypeValue<'valueExt>,
        e: TypeCheckedExpr<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { TypeCheckedExpr.Expr =
          TypeCheckedExprRec.TypeLet(
            { TypeCheckedExprTypeLet.Name = name
              TypeDef = typeDef
              Body = e }
          )
        TypeCheckedExpr.Type = t
        TypeCheckedExpr.Kind = k
        TypeCheckedExpr.Location = loc
        TypeCheckedExpr.Scope = scope }

    static member TypeLet
      (
        name: string,
        typeDef: TypeValue<'valueExt>,
        e: TypeCheckedExpr<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      TypeCheckedExpr<'valueExt>
        .TypeLet(name, typeDef, e, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member RecordCons
      (
        fields: List<ResolvedIdentifier * TypeCheckedExpr<'valueExt>>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { TypeCheckedExpr.Expr =
          TypeCheckedExprRec.RecordCons(
            { TypeCheckedExprRecordCons.Fields = fields }
          )
        TypeCheckedExpr.Type = t
        TypeCheckedExpr.Kind = k
        TypeCheckedExpr.Location = loc
        TypeCheckedExpr.Scope = scope }

    static member RecordCons
      (
        fields: List<ResolvedIdentifier * TypeCheckedExpr<'valueExt>>,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      TypeCheckedExpr<'valueExt>
        .RecordCons(fields, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member RecordWith
      (
        record: TypeCheckedExpr<'valueExt>,
        fields: List<ResolvedIdentifier * TypeCheckedExpr<'valueExt>>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { TypeCheckedExpr.Expr =
          TypeCheckedExprRec.RecordWith(
            { TypeCheckedExprRecordWith.Record = record
              Fields = fields }
          )
        TypeCheckedExpr.Type = t
        TypeCheckedExpr.Kind = k
        TypeCheckedExpr.Location = loc
        TypeCheckedExpr.Scope = scope }

    static member RecordWith
      (
        record: TypeCheckedExpr<'valueExt>,
        fields: List<ResolvedIdentifier * TypeCheckedExpr<'valueExt>>,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      TypeCheckedExpr<'valueExt>
        .RecordWith(
          record,
          fields,
          t,
          k,
          Location.Unknown,
          TypeCheckScope.Empty
        )

    static member TupleCons
      (
        elements: List<TypeCheckedExpr<'valueExt>>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { TypeCheckedExpr.Expr =
          TypeCheckedExprRec.TupleCons(
            { TypeCheckedExprTupleCons.Items = elements }
          )
        TypeCheckedExpr.Type = t
        TypeCheckedExpr.Kind = k
        TypeCheckedExpr.Location = loc
        TypeCheckedExpr.Scope = scope }

    static member TupleCons
      (
        elements: List<TypeCheckedExpr<'valueExt>>,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      TypeCheckedExpr<'valueExt>
        .TupleCons(elements, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member SumCons
      (
        selector: SumConsSelector,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { TypeCheckedExpr.Expr =
          TypeCheckedExprRec.SumCons(
            { TypeCheckedExprSumCons.Selector = selector }
          )
        TypeCheckedExpr.Type = t
        TypeCheckedExpr.Kind = k
        TypeCheckedExpr.Location = loc
        TypeCheckedExpr.Scope = scope }

    static member SumCons
      (selector: SumConsSelector, t: TypeValue<'valueExt>, k: Kind)
      =
      TypeCheckedExpr<'valueExt>
        .SumCons(selector, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member RecordDes
      (
        e: TypeCheckedExpr<'valueExt>,
        id: ResolvedIdentifier,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { TypeCheckedExpr.Expr =
          TypeCheckedExprRec.RecordDes(
            { TypeCheckedExprRecordDes.Expr = e
              Field = id }
          )
        TypeCheckedExpr.Type = t
        TypeCheckedExpr.Kind = k
        TypeCheckedExpr.Location = loc
        TypeCheckedExpr.Scope = scope }

    static member RecordDes
      (
        e: TypeCheckedExpr<'valueExt>,
        id: ResolvedIdentifier,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      TypeCheckedExpr<'valueExt>
        .RecordDes(e, id, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member EntitiesDes
      (
        e: TypeCheckedExpr<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { TypeCheckedExpr.Expr =
          TypeCheckedExprRec.EntitiesDes(
            { TypeCheckedExprEntitiesDes.Expr = e }
          )
        TypeCheckedExpr.Type = t
        TypeCheckedExpr.Kind = k
        TypeCheckedExpr.Location = loc
        TypeCheckedExpr.Scope = scope }

    static member EntitiesDes
      (e: TypeCheckedExpr<'valueExt>, t: TypeValue<'valueExt>, k: Kind)
      =
      TypeCheckedExpr<'valueExt>
        .EntitiesDes(e, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member RelationsDes
      (
        e: TypeCheckedExpr<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { TypeCheckedExpr.Expr =
          TypeCheckedExprRec.RelationsDes(
            { TypeCheckedExprRelationsDes.Expr = e }
          )
        TypeCheckedExpr.Type = t
        TypeCheckedExpr.Kind = k
        TypeCheckedExpr.Location = loc
        TypeCheckedExpr.Scope = scope }

    static member RelationsDes
      (e: TypeCheckedExpr<'valueExt>, t: TypeValue<'valueExt>, k: Kind)
      =
      TypeCheckedExpr<'valueExt>
        .RelationsDes(e, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member EntityDes
      (
        e: TypeCheckedExpr<'valueExt>,
        n: SchemaEntityName,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { TypeCheckedExpr.Expr =
          TypeCheckedExprRec.EntityDes(
            { TypeCheckedExprEntityDes.Expr = e
              EntityName = n }
          )
        TypeCheckedExpr.Type = t
        TypeCheckedExpr.Kind = k
        TypeCheckedExpr.Location = loc
        TypeCheckedExpr.Scope = scope }

    static member EntityDes
      (
        e: TypeCheckedExpr<'valueExt>,
        n: SchemaEntityName,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      TypeCheckedExpr<'valueExt>
        .EntityDes(e, n, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member RelationDes
      (
        e: TypeCheckedExpr<'valueExt>,
        n: SchemaRelationName,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { TypeCheckedExpr.Expr =
          TypeCheckedExprRec.RelationDes(
            { TypeCheckedExprRelationDes.Expr = e
              RelationName = n }
          )
        TypeCheckedExpr.Type = t
        TypeCheckedExpr.Kind = k
        TypeCheckedExpr.Location = loc
        TypeCheckedExpr.Scope = scope }

    static member RelationDes
      (
        e: TypeCheckedExpr<'valueExt>,
        n: SchemaRelationName,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      TypeCheckedExpr<'valueExt>
        .RelationDes(e, n, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member RelationLookupDes
      (
        e: TypeCheckedExpr<'valueExt>,
        n: SchemaRelationName,
        d: RelationLookupDirection,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { TypeCheckedExpr.Expr =
          TypeCheckedExprRec.RelationLookupDes(
            { TypeCheckedExprRelationLookupDes.Expr = e
              RelationName = n
              Direction = d }
          )
        TypeCheckedExpr.Type = t
        TypeCheckedExpr.Kind = k
        TypeCheckedExpr.Location = loc
        TypeCheckedExpr.Scope = scope }

    static member RelationLookupDes
      (
        e: TypeCheckedExpr<'valueExt>,
        n: SchemaRelationName,
        d: RelationLookupDirection,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      TypeCheckedExpr<'valueExt>
        .RelationLookupDes(
          e,
          n,
          d,
          t,
          k,
          Location.Unknown,
          TypeCheckScope.Empty
        )

    static member UnionDes
      (
        cases: Map<ResolvedIdentifier, TypeCheckedCaseHandler<'valueExt>>,
        fallback: Option<TypeCheckedExpr<'valueExt>>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { TypeCheckedExpr.Expr =
          TypeCheckedExprRec.UnionDes(
            { TypeCheckedExprUnionDes.Handlers = cases
              Fallback = fallback }
          )
        TypeCheckedExpr.Type = t
        TypeCheckedExpr.Kind = k
        TypeCheckedExpr.Location = loc
        TypeCheckedExpr.Scope = scope }

    static member UnionDes
      (
        cases: Map<ResolvedIdentifier, TypeCheckedCaseHandler<'valueExt>>,
        fallback: Option<TypeCheckedExpr<'valueExt>>,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      TypeCheckedExpr<'valueExt>
        .UnionDes(cases, fallback, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member TupleDes
      (
        e: TypeCheckedExpr<'valueExt>,
        selector: TupleDesSelector,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { TypeCheckedExpr.Expr =
          TypeCheckedExprRec.TupleDes(
            { TypeCheckedExprTupleDes.Tuple = e
              Item = selector }
          )
        TypeCheckedExpr.Type = t
        TypeCheckedExpr.Kind = k
        TypeCheckedExpr.Location = loc
        TypeCheckedExpr.Scope = scope }

    static member TupleDes
      (
        e: TypeCheckedExpr<'valueExt>,
        selector: TupleDesSelector,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      TypeCheckedExpr<'valueExt>
        .TupleDes(e, selector, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member SumDes
      (
        cases: Map<SumConsSelector, TypeCheckedCaseHandler<'valueExt>>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { TypeCheckedExpr.Expr =
          TypeCheckedExprRec.SumDes({ TypeCheckedExprSumDes.Handlers = cases })
        TypeCheckedExpr.Type = t
        TypeCheckedExpr.Kind = k
        TypeCheckedExpr.Location = loc
        TypeCheckedExpr.Scope = scope }

    static member SumDes
      (
        cases: Map<SumConsSelector, TypeCheckedCaseHandler<'valueExt>>,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      TypeCheckedExpr<'valueExt>
        .SumDes(cases, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member Primitive
      (
        p: PrimitiveValue,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { TypeCheckedExpr.Expr = TypeCheckedExprRec.Primitive(p)
        TypeCheckedExpr.Type = t
        TypeCheckedExpr.Kind = k
        TypeCheckedExpr.Location = loc
        TypeCheckedExpr.Scope = scope }

    static member Primitive
      (p: PrimitiveValue, t: TypeValue<'valueExt>, k: Kind)
      =
      TypeCheckedExpr<'valueExt>
        .Primitive(p, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member Lookup
      (
        id: ResolvedIdentifier,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { TypeCheckedExpr.Expr =
          TypeCheckedExprRec.Lookup({ TypeCheckedExprLookup.Id = id })
        TypeCheckedExpr.Type = t
        TypeCheckedExpr.Kind = k
        TypeCheckedExpr.Location = loc
        TypeCheckedExpr.Scope = scope }

    static member Lookup
      (id: ResolvedIdentifier, t: TypeValue<'valueExt>, k: Kind)
      =
      TypeCheckedExpr<'valueExt>
        .Lookup(id, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member If
      (
        c: TypeCheckedExpr<'valueExt>,
        t: TypeCheckedExpr<'valueExt>,
        f: TypeCheckedExpr<'valueExt>,
        rt: TypeValue<'valueExt>,
        rk: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) =
      { TypeCheckedExpr.Expr =
          TypeCheckedExprRec.If(
            { TypeCheckedExprIf.Cond = c
              Then = t
              Else = f }
          )
        TypeCheckedExpr.Type = rt
        TypeCheckedExpr.Kind = rk
        TypeCheckedExpr.Location = loc
        TypeCheckedExpr.Scope = scope }

    static member If
      (
        c: TypeCheckedExpr<'valueExt>,
        t: TypeCheckedExpr<'valueExt>,
        f: TypeCheckedExpr<'valueExt>,
        rt: TypeValue<'valueExt>,
        rk: Kind
      ) =
      TypeCheckedExpr<'valueExt>
        .If(c, t, f, rt, rk, Location.Unknown, TypeCheckScope.Empty)

  // ── RunnableExpr constructors (mirror of TypeCheckedExpr above) ─────

  type RunnableExpr<'valueExt> with

    static member TypeLambda
      (
        p: TypeParameter,
        e: RunnableExpr<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) : RunnableExpr<'valueExt> =
      { Expr =
          RunnableExprRec.TypeLambda(
            { RunnableExprTypeLambda.Param = p
              Body = e }
          )
        Type = t
        Kind = k
        Location = loc
        Scope = scope }

    static member TypeLambda
      (
        p: TypeParameter,
        e: RunnableExpr<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      RunnableExpr<'valueExt>
        .TypeLambda(p, e, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member TypeApply
      (
        e1: RunnableExpr<'valueExt>,
        typeArg: TypeValue<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) : RunnableExpr<'valueExt> =
      { Expr =
          RunnableExprRec.TypeApply(
            { RunnableExprTypeApply.Func = e1
              TypeArg = typeArg }
          )
        Type = t
        Kind = k
        Location = loc
        Scope = scope }

    static member TypeApply
      (
        e1: RunnableExpr<'valueExt>,
        typeArg: TypeValue<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      RunnableExpr<'valueExt>
        .TypeApply(e1, typeArg, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member Lambda
      (
        v: Var,
        paramType: TypeValue<'valueExt>,
        e: RunnableExpr<'valueExt>,
        returnType: TypeValue<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) : RunnableExpr<'valueExt> =
      { Expr =
          RunnableExprRec.Lambda(
            { RunnableExprLambda.Param = v
              ParamType = paramType
              Body = e
              BodyType = returnType }
          )
        Type = t
        Kind = k
        Location = loc
        Scope = scope }

    static member Lambda
      (
        v: Var,
        paramType: TypeValue<'valueExt>,
        e: RunnableExpr<'valueExt>,
        returnType: TypeValue<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      RunnableExpr<'valueExt>
        .Lambda(
          v,
          paramType,
          e,
          returnType,
          t,
          k,
          Location.Unknown,
          TypeCheckScope.Empty
        )

    static member Apply
      (
        f: RunnableExpr<'valueExt>,
        a: RunnableExpr<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) : RunnableExpr<'valueExt> =
      { Expr =
          RunnableExprRec.Apply({ RunnableExprApply.F = f; Arg = a })
        Type = t
        Kind = k
        Location = loc
        Scope = scope }

    static member Apply
      (
        f: RunnableExpr<'valueExt>,
        a: RunnableExpr<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      RunnableExpr<'valueExt>
        .Apply(f, a, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member Query
      (
        q: RunnableExprQuery<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) : RunnableExpr<'valueExt> =
      { Expr = RunnableExprRec.Query(q)
        Type = t
        Kind = k
        Location = loc
        Scope = scope }

    static member Query
      (q: RunnableExprQuery<'valueExt>, t: TypeValue<'valueExt>, k: Kind)
      =
      RunnableExpr<'valueExt>
        .Query(q, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member FromValue
      (
        v: Value<TypeValue<'valueExt>, 'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) : RunnableExpr<'valueExt> =
      { Expr =
          RunnableExprRec.FromValue(
            { RunnableExprFromValue.Value = v
              ValueType = t
              ValueKind = k }
          )
        Type = t
        Kind = k
        Location = loc
        Scope = scope }

    static member FromValue
      (
        v: Value<TypeValue<'valueExt>, 'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      RunnableExpr<'valueExt>
        .FromValue(v, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member Let
      (
        v: Var,
        varType: TypeValue<'valueExt>,
        a: RunnableExpr<'valueExt>,
        e: RunnableExpr<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) : RunnableExpr<'valueExt> =
      { Expr =
          RunnableExprRec.Let(
            { RunnableExprLet.Var = v
              Type = varType
              Val = a
              Rest = e }
          )
        Type = t
        Kind = k
        Location = loc
        Scope = scope }

    static member Let
      (
        v: Var,
        varType: TypeValue<'valueExt>,
        a: RunnableExpr<'valueExt>,
        e: RunnableExpr<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      RunnableExpr<'valueExt>
        .Let(v, varType, a, e, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member Do
      (
        a: RunnableExpr<'valueExt>,
        e: RunnableExpr<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) : RunnableExpr<'valueExt> =
      { Expr =
          RunnableExprRec.Do({ RunnableExprDo.Val = a; Rest = e })
        Type = t
        Kind = k
        Location = loc
        Scope = scope }

    static member Do
      (
        a: RunnableExpr<'valueExt>,
        e: RunnableExpr<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      RunnableExpr<'valueExt>
        .Do(a, e, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member TypeLet
      (
        name: string,
        typeDef: TypeValue<'valueExt>,
        e: RunnableExpr<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) : RunnableExpr<'valueExt> =
      { Expr =
          RunnableExprRec.TypeLet(
            { RunnableExprTypeLet.Name = name
              TypeDef = typeDef
              Body = e }
          )
        Type = t
        Kind = k
        Location = loc
        Scope = scope }

    static member TypeLet
      (
        name: string,
        typeDef: TypeValue<'valueExt>,
        e: RunnableExpr<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      RunnableExpr<'valueExt>
        .TypeLet(name, typeDef, e, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member RecordCons
      (
        fields: List<ResolvedIdentifier * RunnableExpr<'valueExt>>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) : RunnableExpr<'valueExt> =
      { Expr =
          RunnableExprRec.RecordCons(
            { RunnableExprRecordCons.Fields = fields }
          )
        Type = t
        Kind = k
        Location = loc
        Scope = scope }

    static member RecordCons
      (
        fields: List<ResolvedIdentifier * RunnableExpr<'valueExt>>,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      RunnableExpr<'valueExt>
        .RecordCons(fields, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member RecordWith
      (
        record: RunnableExpr<'valueExt>,
        fields: List<ResolvedIdentifier * RunnableExpr<'valueExt>>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) : RunnableExpr<'valueExt> =
      { Expr =
          RunnableExprRec.RecordWith(
            { RunnableExprRecordWith.Record = record
              Fields = fields }
          )
        Type = t
        Kind = k
        Location = loc
        Scope = scope }

    static member RecordWith
      (
        record: RunnableExpr<'valueExt>,
        fields: List<ResolvedIdentifier * RunnableExpr<'valueExt>>,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      RunnableExpr<'valueExt>
        .RecordWith(
          record,
          fields,
          t,
          k,
          Location.Unknown,
          TypeCheckScope.Empty
        )

    static member TupleCons
      (
        elements: List<RunnableExpr<'valueExt>>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) : RunnableExpr<'valueExt> =
      { Expr =
          RunnableExprRec.TupleCons(
            { RunnableExprTupleCons.Items = elements }
          )
        Type = t
        Kind = k
        Location = loc
        Scope = scope }

    static member TupleCons
      (
        elements: List<RunnableExpr<'valueExt>>,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      RunnableExpr<'valueExt>
        .TupleCons(elements, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member SumCons
      (
        selector: SumConsSelector,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) : RunnableExpr<'valueExt> =
      { Expr =
          RunnableExprRec.SumCons(
            { RunnableExprSumCons.Selector = selector }
          )
        Type = t
        Kind = k
        Location = loc
        Scope = scope }

    static member SumCons
      (selector: SumConsSelector, t: TypeValue<'valueExt>, k: Kind)
      =
      RunnableExpr<'valueExt>
        .SumCons(selector, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member RecordDes
      (
        e: RunnableExpr<'valueExt>,
        id: ResolvedIdentifier,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) : RunnableExpr<'valueExt> =
      { Expr =
          RunnableExprRec.RecordDes(
            { RunnableExprRecordDes.Expr = e
              Field = id }
          )
        Type = t
        Kind = k
        Location = loc
        Scope = scope }

    static member RecordDes
      (
        e: RunnableExpr<'valueExt>,
        id: ResolvedIdentifier,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      RunnableExpr<'valueExt>
        .RecordDes(e, id, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member EntitiesDes
      (
        e: RunnableExpr<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) : RunnableExpr<'valueExt> =
      { Expr =
          RunnableExprRec.EntitiesDes(
            { RunnableExprEntitiesDes.Expr = e }
          )
        Type = t
        Kind = k
        Location = loc
        Scope = scope }

    static member EntitiesDes
      (e: RunnableExpr<'valueExt>, t: TypeValue<'valueExt>, k: Kind)
      =
      RunnableExpr<'valueExt>
        .EntitiesDes(e, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member RelationsDes
      (
        e: RunnableExpr<'valueExt>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) : RunnableExpr<'valueExt> =
      { Expr =
          RunnableExprRec.RelationsDes(
            { RunnableExprRelationsDes.Expr = e }
          )
        Type = t
        Kind = k
        Location = loc
        Scope = scope }

    static member RelationsDes
      (e: RunnableExpr<'valueExt>, t: TypeValue<'valueExt>, k: Kind)
      =
      RunnableExpr<'valueExt>
        .RelationsDes(e, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member EntityDes
      (
        e: RunnableExpr<'valueExt>,
        n: SchemaEntityName,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) : RunnableExpr<'valueExt> =
      { Expr =
          RunnableExprRec.EntityDes(
            { RunnableExprEntityDes.Expr = e
              EntityName = n }
          )
        Type = t
        Kind = k
        Location = loc
        Scope = scope }

    static member EntityDes
      (
        e: RunnableExpr<'valueExt>,
        n: SchemaEntityName,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      RunnableExpr<'valueExt>
        .EntityDes(e, n, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member RelationDes
      (
        e: RunnableExpr<'valueExt>,
        n: SchemaRelationName,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) : RunnableExpr<'valueExt> =
      { Expr =
          RunnableExprRec.RelationDes(
            { RunnableExprRelationDes.Expr = e
              RelationName = n }
          )
        Type = t
        Kind = k
        Location = loc
        Scope = scope }

    static member RelationDes
      (
        e: RunnableExpr<'valueExt>,
        n: SchemaRelationName,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      RunnableExpr<'valueExt>
        .RelationDes(e, n, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member RelationLookupDes
      (
        e: RunnableExpr<'valueExt>,
        n: SchemaRelationName,
        d: RelationLookupDirection,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) : RunnableExpr<'valueExt> =
      { Expr =
          RunnableExprRec.RelationLookupDes(
            { RunnableExprRelationLookupDes.Expr = e
              RelationName = n
              Direction = d }
          )
        Type = t
        Kind = k
        Location = loc
        Scope = scope }

    static member RelationLookupDes
      (
        e: RunnableExpr<'valueExt>,
        n: SchemaRelationName,
        d: RelationLookupDirection,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      RunnableExpr<'valueExt>
        .RelationLookupDes(
          e,
          n,
          d,
          t,
          k,
          Location.Unknown,
          TypeCheckScope.Empty
        )

    static member UnionDes
      (
        cases: Map<ResolvedIdentifier, RunnableCaseHandler<'valueExt>>,
        fallback: Option<RunnableExpr<'valueExt>>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) : RunnableExpr<'valueExt> =
      { Expr =
          RunnableExprRec.UnionDes(
            { RunnableExprUnionDes.Handlers = cases
              Fallback = fallback }
          )
        Type = t
        Kind = k
        Location = loc
        Scope = scope }

    static member UnionDes
      (
        cases: Map<ResolvedIdentifier, RunnableCaseHandler<'valueExt>>,
        fallback: Option<RunnableExpr<'valueExt>>,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      RunnableExpr<'valueExt>
        .UnionDes(cases, fallback, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member TupleDes
      (
        e: RunnableExpr<'valueExt>,
        selector: TupleDesSelector,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) : RunnableExpr<'valueExt> =
      { Expr =
          RunnableExprRec.TupleDes(
            { RunnableExprTupleDes.Tuple = e
              Item = selector }
          )
        Type = t
        Kind = k
        Location = loc
        Scope = scope }

    static member TupleDes
      (
        e: RunnableExpr<'valueExt>,
        selector: TupleDesSelector,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      RunnableExpr<'valueExt>
        .TupleDes(e, selector, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member SumDes
      (
        cases: Map<SumConsSelector, RunnableCaseHandler<'valueExt>>,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) : RunnableExpr<'valueExt> =
      { Expr =
          RunnableExprRec.SumDes({ RunnableExprSumDes.Handlers = cases })
        Type = t
        Kind = k
        Location = loc
        Scope = scope }

    static member SumDes
      (
        cases: Map<SumConsSelector, RunnableCaseHandler<'valueExt>>,
        t: TypeValue<'valueExt>,
        k: Kind
      ) =
      RunnableExpr<'valueExt>
        .SumDes(cases, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member Primitive
      (
        p: PrimitiveValue,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) : RunnableExpr<'valueExt> =
      { Expr = RunnableExprRec.Primitive(p)
        Type = t
        Kind = k
        Location = loc
        Scope = scope }

    static member Primitive
      (p: PrimitiveValue, t: TypeValue<'valueExt>, k: Kind)
      =
      RunnableExpr<'valueExt>
        .Primitive(p, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member Lookup
      (
        id: ResolvedIdentifier,
        t: TypeValue<'valueExt>,
        k: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) : RunnableExpr<'valueExt> =
      { Expr =
          RunnableExprRec.Lookup({ RunnableExprLookup.Id = id })
        Type = t
        Kind = k
        Location = loc
        Scope = scope }

    static member Lookup
      (id: ResolvedIdentifier, t: TypeValue<'valueExt>, k: Kind)
      =
      RunnableExpr<'valueExt>
        .Lookup(id, t, k, Location.Unknown, TypeCheckScope.Empty)

    static member If
      (
        c: RunnableExpr<'valueExt>,
        t: RunnableExpr<'valueExt>,
        f: RunnableExpr<'valueExt>,
        rt: TypeValue<'valueExt>,
        rk: Kind,
        loc: Location,
        scope: TypeCheckScope
      ) : RunnableExpr<'valueExt> =
      { Expr =
          RunnableExprRec.If(
            { RunnableExprIf.Cond = c
              Then = t
              Else = f }
          )
        Type = rt
        Kind = rk
        Location = loc
        Scope = scope }

    static member If
      (
        c: RunnableExpr<'valueExt>,
        t: RunnableExpr<'valueExt>,
        f: RunnableExpr<'valueExt>,
        rt: TypeValue<'valueExt>,
        rk: Kind
      ) =
      RunnableExpr<'valueExt>
        .If(c, t, f, rt, rk, Location.Unknown, TypeCheckScope.Empty)

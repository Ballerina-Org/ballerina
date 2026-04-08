namespace Ballerina.DSL.Next.Types

[<AutoOpen>]
module TypeCheckedExprLegacyConstructors =
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Patterns

  type TypeCheckedExpr<'valueExt> with
    // Unsafe constructors are only for synthesizing untyped eval expressions.
    // They intentionally inject placeholder Type/Kind values.
    static member UnsafeLookupForUntypedEval(id: ResolvedIdentifier) =
      TypeCheckedExpr<'valueExt>.Lookup(id, TypeValue.CreateUnit(), Kind.Star)

    static member UnsafeApplyForUntypedEval
      (f: TypeCheckedExpr<'valueExt>, a: TypeCheckedExpr<'valueExt>)
      =
      TypeCheckedExpr<'valueExt>.Apply(f, a, TypeValue.CreateUnit(), Kind.Star)

    static member UnsafeLambdaForUntypedEval
      (
        v: Var,
        t: TypeValue<'valueExt>,
        e: TypeCheckedExpr<'valueExt>,
        r: TypeValue<'valueExt>
      ) =
      TypeCheckedExpr<'valueExt>
        .Lambda(v, t, e, r, TypeValue.CreateArrow(t, r), Kind.Star)

    static member UnsafeQueryForUntypedEval
      (q: TypeCheckedExprQuery<'valueExt>)
      =
      TypeCheckedExpr<'valueExt>.Query(q, TypeValue.CreateUnit(), Kind.Star)

    static member UnsafeSumConsForUntypedEval(selector: SumConsSelector) =
      TypeCheckedExpr<'valueExt>
        .SumCons(selector, TypeValue.CreateUnit(), Kind.Star)

    static member UnsafeTupleConsForUntypedEval
      (items: List<TypeCheckedExpr<'valueExt>>)
      =
      TypeCheckedExpr<'valueExt>
        .TupleCons(items, TypeValue.CreateUnit(), Kind.Star)

    static member UnsafeRecordConsForUntypedEval
      (fields: List<ResolvedIdentifier * TypeCheckedExpr<'valueExt>>)
      =
      TypeCheckedExpr<'valueExt>
        .RecordCons(fields, TypeValue.CreateUnit(), Kind.Star)

    static member UnsafePrimitiveForUntypedEval(p: PrimitiveValue) =
      TypeCheckedExpr<'valueExt>.Primitive(p, TypeValue.CreateUnit(), Kind.Star)

namespace Ballerina.DSL.Next.Types

[<AutoOpen>]
module RunnableExprLegacyConstructors =
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Patterns

  let private defaultLocation: Location =
    { File = ""; Line = 1; Column = 1 }

  let private defaultScope: TypeCheckScope = TypeCheckScope.Empty

  type RunnableExpr<'valueExt> with
    // Unsafe constructors are only for synthesizing untyped eval expressions.
    // They intentionally inject placeholder Type/Kind values.
    static member UnsafeLookupForUntypedEval(id: ResolvedIdentifier) : RunnableExpr<'valueExt> =
      { Expr = RunnableExprRec.Lookup { RunnableExprLookup.Id = id }
        Location = defaultLocation
        Type = TypeValue.CreateUnit()
        Kind = Kind.Star
        Scope = defaultScope }

    static member UnsafeApplyForUntypedEval
      (f: RunnableExpr<'valueExt>, a: RunnableExpr<'valueExt>)
      : RunnableExpr<'valueExt> =
      { Expr = RunnableExprRec.Apply { F = f; Arg = a }
        Location = defaultLocation
        Type = TypeValue.CreateUnit()
        Kind = Kind.Star
        Scope = defaultScope }

    static member UnsafeLambdaForUntypedEval
      (
        v: Var,
        t: TypeValue<'valueExt>,
        e: RunnableExpr<'valueExt>,
        r: TypeValue<'valueExt>
      ) : RunnableExpr<'valueExt> =
      { Expr = RunnableExprRec.Lambda { Param = v; ParamType = t; Body = e; BodyType = r }
        Location = defaultLocation
        Type = TypeValue.CreateArrow(t, r)
        Kind = Kind.Star
        Scope = defaultScope }

    static member UnsafeQueryForUntypedEval
      (q: RunnableExprQuery<'valueExt>)
      : RunnableExpr<'valueExt> =
      { Expr = RunnableExprRec.Query q
        Location = defaultLocation
        Type = TypeValue.CreateUnit()
        Kind = Kind.Star
        Scope = defaultScope }

    static member UnsafeSumConsForUntypedEval(selector: SumConsSelector) : RunnableExpr<'valueExt> =
      { Expr = RunnableExprRec.SumCons { Selector = selector }
        Location = defaultLocation
        Type = TypeValue.CreateUnit()
        Kind = Kind.Star
        Scope = defaultScope }

    static member UnsafeTupleConsForUntypedEval
      (items: List<RunnableExpr<'valueExt>>)
      : RunnableExpr<'valueExt> =
      { Expr = RunnableExprRec.TupleCons { Items = items }
        Location = defaultLocation
        Type = TypeValue.CreateUnit()
        Kind = Kind.Star
        Scope = defaultScope }

    static member UnsafeRecordConsForUntypedEval
      (fields: List<ResolvedIdentifier * RunnableExpr<'valueExt>>)
      : RunnableExpr<'valueExt> =
      { Expr = RunnableExprRec.RecordCons { Fields = fields }
        Location = defaultLocation
        Type = TypeValue.CreateUnit()
        Kind = Kind.Star
        Scope = defaultScope }

    static member UnsafePrimitiveForUntypedEval(p: PrimitiveValue) : RunnableExpr<'valueExt> =
      { Expr = RunnableExprRec.Primitive p
        Location = defaultLocation
        Type = TypeValue.CreateUnit()
        Kind = Kind.Star
        Scope = defaultScope }

    static member FromValue
      (value: Value<TypeValue<'valueExt>, 'valueExt>,
       valueType: TypeValue<'valueExt>,
       valueKind: Kind)
      : RunnableExpr<'valueExt> =
      { Expr = RunnableExprRec.FromValue { Value = value; ValueType = valueType; ValueKind = valueKind }
        Location = defaultLocation
        Type = valueType
        Kind = valueKind
        Scope = defaultScope }

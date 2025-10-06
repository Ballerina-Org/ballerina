module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Type.Patterns

open Ballerina.DSL.Next.Types.Model

type TypeValue with

  static member PrimitiveWithTrivialSource(v: PrimitiveType) : TypeValue =
    match v with
    | PrimitiveType.Unit ->
      TypeValue.Primitive
        { value = v
          source = TypeExprSourceMapping.OriginTypeExpr(TypeExpr.Primitive PrimitiveType.Unit) }
    | PrimitiveType.Guid ->
      TypeValue.Primitive
        { value = v
          source = TypeExprSourceMapping.OriginTypeExpr(TypeExpr.Primitive PrimitiveType.Guid) }
    | PrimitiveType.Int32 ->
      TypeValue.Primitive
        { value = v
          source = TypeExprSourceMapping.OriginTypeExpr(TypeExpr.Primitive PrimitiveType.Int32) }
    | PrimitiveType.Int64 ->
      TypeValue.Primitive
        { value = v
          source = TypeExprSourceMapping.OriginTypeExpr(TypeExpr.Primitive PrimitiveType.Int64) }
    | PrimitiveType.Float32 ->
      TypeValue.Primitive
        { value = v
          source = TypeExprSourceMapping.OriginTypeExpr(TypeExpr.Primitive PrimitiveType.Float32) }
    | PrimitiveType.Float64 ->
      TypeValue.Primitive
        { value = v
          source = TypeExprSourceMapping.OriginTypeExpr(TypeExpr.Primitive PrimitiveType.Float64) }
    | PrimitiveType.Decimal ->
      TypeValue.Primitive
        { value = v
          source = TypeExprSourceMapping.OriginTypeExpr(TypeExpr.Primitive PrimitiveType.Decimal) }
    | PrimitiveType.Bool ->
      TypeValue.Primitive
        { value = v
          source = TypeExprSourceMapping.OriginTypeExpr(TypeExpr.Primitive PrimitiveType.Bool) }
    | PrimitiveType.String ->
      TypeValue.Primitive
        { value = v
          source = TypeExprSourceMapping.OriginTypeExpr(TypeExpr.Primitive PrimitiveType.String) }
    | PrimitiveType.DateTime ->
      TypeValue.Primitive
        { value = v
          source = TypeExprSourceMapping.OriginTypeExpr(TypeExpr.Primitive PrimitiveType.DateTime) }
    | PrimitiveType.DateOnly ->
      TypeValue.Primitive
        { value = v
          source = TypeExprSourceMapping.OriginTypeExpr(TypeExpr.Primitive PrimitiveType.DateOnly) }
    | PrimitiveType.TimeSpan ->
      TypeValue.Primitive
        { value = v
          source = TypeExprSourceMapping.OriginTypeExpr(TypeExpr.Primitive PrimitiveType.TimeSpan) }

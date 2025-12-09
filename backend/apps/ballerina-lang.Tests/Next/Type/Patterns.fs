module Ballerina.Cat.Tests.BusinessRuleEngine.Next.Type.Patterns

open Ballerina.DSL.Next.Types.Model

type TypeValue with

  static member PrimitiveWithTrivialSource(v: PrimitiveType) : TypeValue =
    match v with
    | PrimitiveType.Unit ->
      TypeValue.Primitive
        { value = v
          typeExprSource = TypeExprSourceMapping.OriginTypeExpr(TypeExpr.Primitive PrimitiveType.Unit)
          typeCheckScopeSource = TypeCheckScope.Empty }
    | PrimitiveType.Guid ->
      TypeValue.Primitive
        { value = v
          typeExprSource = TypeExprSourceMapping.OriginTypeExpr(TypeExpr.Primitive PrimitiveType.Guid)
          typeCheckScopeSource = TypeCheckScope.Empty }
    | PrimitiveType.Int32 ->
      TypeValue.Primitive
        { value = v
          typeExprSource = TypeExprSourceMapping.OriginTypeExpr(TypeExpr.Primitive PrimitiveType.Int32)
          typeCheckScopeSource = TypeCheckScope.Empty }
    | PrimitiveType.Int64 ->
      TypeValue.Primitive
        { value = v
          typeExprSource = TypeExprSourceMapping.OriginTypeExpr(TypeExpr.Primitive PrimitiveType.Int64)
          typeCheckScopeSource = TypeCheckScope.Empty }
    | PrimitiveType.Float32 ->
      TypeValue.Primitive
        { value = v
          typeExprSource = TypeExprSourceMapping.OriginTypeExpr(TypeExpr.Primitive PrimitiveType.Float32)
          typeCheckScopeSource = TypeCheckScope.Empty }
    | PrimitiveType.Float64 ->
      TypeValue.Primitive
        { value = v
          typeExprSource = TypeExprSourceMapping.OriginTypeExpr(TypeExpr.Primitive PrimitiveType.Float64)
          typeCheckScopeSource = TypeCheckScope.Empty }
    | PrimitiveType.Decimal ->
      TypeValue.Primitive
        { value = v
          typeExprSource = TypeExprSourceMapping.OriginTypeExpr(TypeExpr.Primitive PrimitiveType.Decimal)
          typeCheckScopeSource = TypeCheckScope.Empty }
    | PrimitiveType.Bool ->
      TypeValue.Primitive
        { value = v
          typeExprSource = TypeExprSourceMapping.OriginTypeExpr(TypeExpr.Primitive PrimitiveType.Bool)
          typeCheckScopeSource = TypeCheckScope.Empty }
    | PrimitiveType.String ->
      TypeValue.Primitive
        { value = v
          typeExprSource = TypeExprSourceMapping.OriginTypeExpr(TypeExpr.Primitive PrimitiveType.String)
          typeCheckScopeSource = TypeCheckScope.Empty }
    | PrimitiveType.DateTime ->
      TypeValue.Primitive
        { value = v
          typeExprSource = TypeExprSourceMapping.OriginTypeExpr(TypeExpr.Primitive PrimitiveType.DateTime)
          typeCheckScopeSource = TypeCheckScope.Empty }
    | PrimitiveType.DateOnly ->
      TypeValue.Primitive
        { value = v
          typeExprSource = TypeExprSourceMapping.OriginTypeExpr(TypeExpr.Primitive PrimitiveType.DateOnly)
          typeCheckScopeSource = TypeCheckScope.Empty }
    | PrimitiveType.TimeSpan ->
      TypeValue.Primitive
        { value = v
          typeExprSource = TypeExprSourceMapping.OriginTypeExpr(TypeExpr.Primitive PrimitiveType.TimeSpan)
          typeCheckScopeSource = TypeCheckScope.Empty }

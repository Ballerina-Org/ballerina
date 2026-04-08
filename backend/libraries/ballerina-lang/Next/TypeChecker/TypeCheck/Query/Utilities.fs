namespace Ballerina.DSL.Next.Types.TypeChecker

module QueryUtilities =
  open Ballerina.State.WithError
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns

  let two_equal_primitives
    loc0
    expected_primitive_type
    (t, (e: TypeCheckedExprQueryExpr<_>))
    =
    match t, e.Expr with
    | [ TypeQueryRow.PrimitiveType(p1, is_p1_nullable)
        TypeQueryRow.PrimitiveType(p2, is_p2_nullable) ],
      _ when p1 = expected_primitive_type && p2 = expected_primitive_type ->
      Some(e, is_p1_nullable || is_p2_nullable)
    | [ TypeQueryRow.Json(TypeValue.Primitive { value = p1 })
        TypeQueryRow.PrimitiveType(p2, is_p2_nullable) ],
      TypeCheckedExprQueryExprRec.QueryTupleCons [ v1; v2 ] when
      p1 = expected_primitive_type && p2 = expected_primitive_type
      ->
      Some(
        [ TypeCheckedExprQueryExprRec.QueryCastTo(
            v1,
            TypeQueryRow.PrimitiveType(expected_primitive_type, false)
          )
          |> TypeCheckedExprQueryExpr.Create loc0
          v2 ]
        |> TypeCheckedExprQueryExprRec.QueryTupleCons
        |> TypeCheckedExprQueryExpr.Create loc0,
        is_p2_nullable
      )
    | [ TypeQueryRow.PrimitiveType(p1, is_p1_nullable)
        TypeQueryRow.Json(TypeValue.Primitive { value = p2 }) ],
      TypeCheckedExprQueryExprRec.QueryTupleCons [ v1; v2 ] when
      p1 = expected_primitive_type && p2 = expected_primitive_type
      ->
      Some(
        [ v1
          TypeCheckedExprQueryExprRec.QueryCastTo(
            v2,
            TypeQueryRow.PrimitiveType(expected_primitive_type, false)
          )
          |> TypeCheckedExprQueryExpr.Create loc0 ]
        |> TypeCheckedExprQueryExprRec.QueryTupleCons
        |> TypeCheckedExprQueryExpr.Create loc0,
        is_p1_nullable
      )
    | [ TypeQueryRow.Json(TypeValue.Primitive { value = p1 })
        TypeQueryRow.Json(TypeValue.Primitive { value = p2 }) ],
      TypeCheckedExprQueryExprRec.QueryTupleCons [ v1; v2 ] when
      p1 = expected_primitive_type && p2 = expected_primitive_type
      ->
      Some(
        [ TypeCheckedExprQueryExprRec.QueryCastTo(
            v1,
            TypeQueryRow.PrimitiveType(expected_primitive_type, false)
          )
          |> TypeCheckedExprQueryExpr.Create loc0
          TypeCheckedExprQueryExprRec.QueryCastTo(
            v2,
            TypeQueryRow.PrimitiveType(expected_primitive_type, false)
          )
          |> TypeCheckedExprQueryExpr.Create loc0 ]
        |> TypeCheckedExprQueryExprRec.QueryTupleCons
        |> TypeCheckedExprQueryExpr.Create loc0,
        false
      )
    | _ -> None

  let query_constant_to_type loc c =
    state {
      match c with
      | PrimitiveValue.Unit ->
        return
          TypeCheckedExprQueryExprRec.QueryConstant c
          |> TypeCheckedExprQueryExpr.Create loc,
          TypeQueryRow.PrimitiveType(PrimitiveType.Unit, false)
      | PrimitiveValue.Int32(_) ->
        return
          TypeCheckedExprQueryExprRec.QueryConstant c
          |> TypeCheckedExprQueryExpr.Create loc,
          TypeQueryRow.PrimitiveType(PrimitiveType.Int32, false)
      | PrimitiveValue.Int64(_) ->
        return
          TypeCheckedExprQueryExprRec.QueryConstant c
          |> TypeCheckedExprQueryExpr.Create loc,
          TypeQueryRow.PrimitiveType(PrimitiveType.Int64, false)
      | PrimitiveValue.Float32(_) ->
        return
          TypeCheckedExprQueryExprRec.QueryConstant c
          |> TypeCheckedExprQueryExpr.Create loc,
          TypeQueryRow.PrimitiveType(PrimitiveType.Float32, false)
      | PrimitiveValue.Float64(_) ->
        return
          TypeCheckedExprQueryExprRec.QueryConstant c
          |> TypeCheckedExprQueryExpr.Create loc,
          TypeQueryRow.PrimitiveType(PrimitiveType.Float64, false)
      | PrimitiveValue.Decimal(_) ->
        return
          TypeCheckedExprQueryExprRec.QueryConstant c
          |> TypeCheckedExprQueryExpr.Create loc,
          TypeQueryRow.PrimitiveType(PrimitiveType.Decimal, false)
      | PrimitiveValue.Bool(_) ->
        return
          TypeCheckedExprQueryExprRec.QueryConstant c
          |> TypeCheckedExprQueryExpr.Create loc,
          TypeQueryRow.PrimitiveType(PrimitiveType.Bool, false)
      | PrimitiveValue.Guid(_) ->
        return
          TypeCheckedExprQueryExprRec.QueryConstant c
          |> TypeCheckedExprQueryExpr.Create loc,
          TypeQueryRow.PrimitiveType(PrimitiveType.Guid, false)
      | PrimitiveValue.String(_) ->
        return
          TypeCheckedExprQueryExprRec.QueryConstant c
          |> TypeCheckedExprQueryExpr.Create loc,
          TypeQueryRow.PrimitiveType(PrimitiveType.String, false)
      | PrimitiveValue.Date(_) ->
        return
          TypeCheckedExprQueryExprRec.QueryConstant c
          |> TypeCheckedExprQueryExpr.Create loc,
          TypeQueryRow.PrimitiveType(PrimitiveType.DateOnly, false)
      | PrimitiveValue.DateTime(_) ->
        return
          TypeCheckedExprQueryExprRec.QueryConstant c
          |> TypeCheckedExprQueryExpr.Create loc,
          TypeQueryRow.PrimitiveType(PrimitiveType.DateTime, false)
      | PrimitiveValue.TimeSpan(_) ->
        return
          TypeCheckedExprQueryExprRec.QueryConstant c
          |> TypeCheckedExprQueryExpr.Create loc,
          TypeQueryRow.PrimitiveType(PrimitiveType.TimeSpan, false)

    }

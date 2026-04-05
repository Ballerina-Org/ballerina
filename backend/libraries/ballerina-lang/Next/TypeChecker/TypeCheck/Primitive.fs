namespace Ballerina.DSL.Next.Types.TypeChecker

module Primitive =
  open Ballerina.StdLib.String
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Collections.Option
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open System
  open Ballerina.StdLib.Object
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Unification
  open Ballerina.DSL.Next.Types.TypeChecker.AdHocPolymorphicOperators
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Eval
  open Ballerina.DSL.Next.Types.TypeChecker.LiftOtherSteps
  open Ballerina.Fun
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList

  type Expr<'T, 'Id, 've when 'Id: comparison> with
    static member internal TypeCheckPrimitive<'valueExt when 'valueExt: comparison>
      (_typeCheckExpr: ExprTypeChecker<'valueExt>, loc0: Location)
      : TypeChecker<PrimitiveValue, 'valueExt> =
      fun _context_t (p: PrimitiveValue) ->
        state {
          let! ctx = state.GetContext()

          match p with
          | PrimitiveValue.Int32 v ->
            return
              TypeCheckedExpr.Primitive(PrimitiveValue.Int32 v, loc0, ctx.Scope),
              TypeValue.CreatePrimitive PrimitiveType.Int32,
              Kind.Star,
              ctx

          | (PrimitiveValue.Int64 v) ->
            return
              TypeCheckedExpr.Primitive(PrimitiveValue.Int64 v, loc0, ctx.Scope),
              TypeValue.CreatePrimitive PrimitiveType.Int64,
              Kind.Star,
              ctx

          | (PrimitiveValue.Float32 v) ->
            return
              TypeCheckedExpr.Primitive(PrimitiveValue.Float32 v, loc0, ctx.Scope),
              TypeValue.CreatePrimitive PrimitiveType.Float32,
              Kind.Star,
              ctx

          | (PrimitiveValue.Float64 v) ->
            return
              TypeCheckedExpr.Primitive(PrimitiveValue.Float64 v, loc0, ctx.Scope),
              TypeValue.CreatePrimitive PrimitiveType.Float64,
              Kind.Star,
              ctx

          | (PrimitiveValue.Bool v) ->
            return
              TypeCheckedExpr.Primitive(PrimitiveValue.Bool v, loc0, ctx.Scope),
              TypeValue.CreatePrimitive PrimitiveType.Bool,
              Kind.Star,
              ctx

          | (PrimitiveValue.Date v) ->
            return
              TypeCheckedExpr.Primitive(PrimitiveValue.Date v, loc0, ctx.Scope),
              TypeValue.CreatePrimitive PrimitiveType.DateOnly,
              Kind.Star,
              ctx

          | (PrimitiveValue.DateTime v) ->
            return
              TypeCheckedExpr.Primitive(PrimitiveValue.DateTime v, loc0, ctx.Scope),
              TypeValue.CreatePrimitive PrimitiveType.DateTime,
              Kind.Star,
              ctx

          | (PrimitiveValue.TimeSpan v) ->
            return
              TypeCheckedExpr.Primitive(PrimitiveValue.TimeSpan v, loc0, ctx.Scope),
              TypeValue.CreatePrimitive PrimitiveType.TimeSpan,
              Kind.Star,
              ctx

          | (PrimitiveValue.Decimal v) ->
            return
              TypeCheckedExpr.Primitive(PrimitiveValue.Decimal v, loc0, ctx.Scope),
              TypeValue.CreatePrimitive PrimitiveType.Decimal,
              Kind.Star,
              ctx

          | (PrimitiveValue.Guid v) ->
            return
              TypeCheckedExpr.Primitive(PrimitiveValue.Guid v, loc0, ctx.Scope),
              TypeValue.CreatePrimitive PrimitiveType.Guid,
              Kind.Star,
              ctx

          | (PrimitiveValue.String v) ->
            return
              TypeCheckedExpr.Primitive(PrimitiveValue.String v, loc0, ctx.Scope),
              TypeValue.CreatePrimitive PrimitiveType.String,
              Kind.Star,
              ctx

          | (PrimitiveValue.Unit) ->
            return
              TypeCheckedExpr.Primitive(PrimitiveValue.Unit, loc0, ctx.Scope),
              TypeValue.CreatePrimitive PrimitiveType.Unit,
              Kind.Star,
              ctx
        }

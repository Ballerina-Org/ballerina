namespace Ballerina.DSL.Next.Types.TypeChecker

module Primitive =
  open Ballerina.StdLib.String
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Collections.Option
  open Ballerina.LocalizedErrors
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
  open Ballerina.Collections.NonEmptyList

  type Expr<'T, 'Id when 'Id: comparison> with
    static member internal TypeCheckPrimitive
      (_typeCheckExpr: TypeChecker, loc0: Location)
      : TypeChecker<PrimitiveValue> =
      fun _context_t (p: PrimitiveValue) ->
        state {
          let! ctx = state.GetContext()

          match p with
          | PrimitiveValue.Int32 v ->
            return
              Expr.Primitive(PrimitiveValue.Int32 v, loc0, ctx.Types.Scope),
              TypeValue.CreatePrimitive PrimitiveType.Int32,
              Kind.Star

          | (PrimitiveValue.Int64 v) ->
            return
              Expr.Primitive(PrimitiveValue.Int64 v, loc0, ctx.Types.Scope),
              TypeValue.CreatePrimitive PrimitiveType.Int64,
              Kind.Star

          | (PrimitiveValue.Float32 v) ->
            return
              Expr.Primitive(PrimitiveValue.Float32 v, loc0, ctx.Types.Scope),
              TypeValue.CreatePrimitive PrimitiveType.Float32,
              Kind.Star

          | (PrimitiveValue.Float64 v) ->
            return
              Expr.Primitive(PrimitiveValue.Float64 v, loc0, ctx.Types.Scope),
              TypeValue.CreatePrimitive PrimitiveType.Float64,
              Kind.Star

          | (PrimitiveValue.Bool v) ->
            return
              Expr.Primitive(PrimitiveValue.Bool v, loc0, ctx.Types.Scope),
              TypeValue.CreatePrimitive PrimitiveType.Bool,
              Kind.Star

          | (PrimitiveValue.Date v) ->
            return
              Expr.Primitive(PrimitiveValue.Date v, loc0, ctx.Types.Scope),
              TypeValue.CreatePrimitive PrimitiveType.DateOnly,
              Kind.Star

          | (PrimitiveValue.DateTime v) ->
            return
              Expr.Primitive(PrimitiveValue.DateTime v, loc0, ctx.Types.Scope),
              TypeValue.CreatePrimitive PrimitiveType.DateTime,
              Kind.Star

          | (PrimitiveValue.TimeSpan v) ->
            return
              Expr.Primitive(PrimitiveValue.TimeSpan v, loc0, ctx.Types.Scope),
              TypeValue.CreatePrimitive PrimitiveType.TimeSpan,
              Kind.Star

          | (PrimitiveValue.Decimal v) ->
            return
              Expr.Primitive(PrimitiveValue.Decimal v, loc0, ctx.Types.Scope),
              TypeValue.CreatePrimitive PrimitiveType.Decimal,
              Kind.Star

          | (PrimitiveValue.Guid v) ->
            return
              Expr.Primitive(PrimitiveValue.Guid v, loc0, ctx.Types.Scope),
              TypeValue.CreatePrimitive PrimitiveType.Guid,
              Kind.Star

          | (PrimitiveValue.String v) ->
            return
              Expr.Primitive(PrimitiveValue.String v, loc0, ctx.Types.Scope),
              TypeValue.CreatePrimitive PrimitiveType.String,
              Kind.Star

          | (PrimitiveValue.Unit) ->
            return
              Expr.Primitive(PrimitiveValue.Unit, loc0, ctx.Types.Scope),
              TypeValue.CreatePrimitive PrimitiveType.Unit,
              Kind.Star
        }

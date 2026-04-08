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
    static member internal TypeCheckPrimitive<'valueExt
      when 'valueExt: comparison>
      (_typeCheckExpr: ExprTypeChecker<'valueExt>, loc0: Location)
      : TypeChecker<PrimitiveValue, 'valueExt> =
      fun _context_t (p: PrimitiveValue) ->
        state {
          let! ctx = state.GetContext()

          match p with
          | PrimitiveValue.Int32 v ->
            let t = TypeValue.CreatePrimitive PrimitiveType.Int32

            return
              TypeCheckedExpr.Primitive(
                PrimitiveValue.Int32 v,
                t,
                Kind.Star,
                loc0,
                ctx.Scope
              ),
              ctx

          | (PrimitiveValue.Int64 v) ->
            let t = TypeValue.CreatePrimitive PrimitiveType.Int64

            return
              TypeCheckedExpr.Primitive(
                PrimitiveValue.Int64 v,
                t,
                Kind.Star,
                loc0,
                ctx.Scope
              ),
              ctx

          | (PrimitiveValue.Float32 v) ->
            let t = TypeValue.CreatePrimitive PrimitiveType.Float32

            return
              TypeCheckedExpr.Primitive(
                PrimitiveValue.Float32 v,
                t,
                Kind.Star,
                loc0,
                ctx.Scope
              ),
              ctx

          | (PrimitiveValue.Float64 v) ->
            let t = TypeValue.CreatePrimitive PrimitiveType.Float64

            return
              TypeCheckedExpr.Primitive(
                PrimitiveValue.Float64 v,
                t,
                Kind.Star,
                loc0,
                ctx.Scope
              ),
              ctx

          | (PrimitiveValue.Bool v) ->
            let t = TypeValue.CreatePrimitive PrimitiveType.Bool

            return
              TypeCheckedExpr.Primitive(
                PrimitiveValue.Bool v,
                t,
                Kind.Star,
                loc0,
                ctx.Scope
              ),
              ctx

          | (PrimitiveValue.Date v) ->
            let t = TypeValue.CreatePrimitive PrimitiveType.DateOnly

            return
              TypeCheckedExpr.Primitive(
                PrimitiveValue.Date v,
                t,
                Kind.Star,
                loc0,
                ctx.Scope
              ),
              ctx

          | (PrimitiveValue.DateTime v) ->
            let t = TypeValue.CreatePrimitive PrimitiveType.DateTime

            return
              TypeCheckedExpr.Primitive(
                PrimitiveValue.DateTime v,
                t,
                Kind.Star,
                loc0,
                ctx.Scope
              ),
              ctx

          | (PrimitiveValue.TimeSpan v) ->
            let t = TypeValue.CreatePrimitive PrimitiveType.TimeSpan

            return
              TypeCheckedExpr.Primitive(
                PrimitiveValue.TimeSpan v,
                t,
                Kind.Star,
                loc0,
                ctx.Scope
              ),
              ctx

          | (PrimitiveValue.Decimal v) ->
            let t = TypeValue.CreatePrimitive PrimitiveType.Decimal

            return
              TypeCheckedExpr.Primitive(
                PrimitiveValue.Decimal v,
                t,
                Kind.Star,
                loc0,
                ctx.Scope
              ),
              ctx

          | (PrimitiveValue.Guid v) ->
            let t = TypeValue.CreatePrimitive PrimitiveType.Guid

            return
              TypeCheckedExpr.Primitive(
                PrimitiveValue.Guid v,
                t,
                Kind.Star,
                loc0,
                ctx.Scope
              ),
              ctx

          | (PrimitiveValue.String v) ->
            let t = TypeValue.CreatePrimitive PrimitiveType.String

            return
              TypeCheckedExpr.Primitive(
                PrimitiveValue.String v,
                t,
                Kind.Star,
                loc0,
                ctx.Scope
              ),
              ctx

          | (PrimitiveValue.Unit) ->
            let t = TypeValue.CreatePrimitive PrimitiveType.Unit

            return
              TypeCheckedExpr.Primitive(
                PrimitiveValue.Unit,
                t,
                Kind.Star,
                loc0,
                ctx.Scope
              ),
              ctx
        }

namespace Ballerina.DSL.Next.Types.TypeChecker

module Do =
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
  open Ballerina.DSL.Next.Types.TypeChecker.Primitive
  open Ballerina.DSL.Next.Types.TypeChecker.Lookup
  open Ballerina.DSL.Next.Types.TypeChecker.Lambda
  open Ballerina.DSL.Next.Types.TypeChecker.Apply
  open Ballerina.Fun
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList

  type Expr<'T, 'Id, 've when 'Id: comparison> with
    static member internal TypeCheckDo<'valueExt when 'valueExt: comparison>
      (_config: TypeCheckingConfig<'valueExt>)
      (typeCheckExpr: ExprTypeChecker<'valueExt>)
      : TypeChecker<
          ExprDo<TypeExpr<'valueExt>, Identifier, 'valueExt>,
          'valueExt
         >
      =
      fun context_t ({ Val = e1; Rest = e2 }) ->
        let (!) = typeCheckExpr context_t
        let (=>) c e = typeCheckExpr c e
        let loc0 = e1.Location

        let ofSum (p: Sum<'a, Errors<Unit>>) =
          p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

        state {
          let! ctx = state.GetContext()

          let! e1, _ = TypeValue.CreateUnit() |> Some => e1
          let t1 = e1.Type
          let k1 = e1.Kind
          let! t1 = t1 |> TypeValue.AsPrimitive |> ofSum
          do! t1.value |> PrimitiveType.AsUnit |> ofSum
          do! k1 |> Kind.AsStar |> ofSum

          let! e2, ctx_e2 = !e2
          let t2 = e2.Type
          let k2 = e2.Kind

          return TypeCheckedExpr.Do(e1, e2, t2, k2, loc0, ctx.Scope), ctx_e2
        }

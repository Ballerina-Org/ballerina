namespace Ballerina.DSL.Next.Types.TypeChecker

module Let =
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
    static member internal TypeCheckLet<'valueExt when 'valueExt: comparison>
      (config: TypeCheckingConfig<'valueExt>)
      (typeCheckExpr: ExprTypeChecker<'valueExt>)
      : TypeChecker<Location * ExprLet<TypeExpr<'valueExt>, Identifier, 'valueExt>, 'valueExt> =
      fun
          context_t
          (letLoc,
            { Var = x
              Type = var_type
              Val = e1
              Rest = e2 }) ->
        let (!) = typeCheckExpr context_t
        let (=>) c e = typeCheckExpr c e
        let loc0 = e1.Location

        let ofSum (p: Sum<'a, Errors<Unit>>) =
          p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

        state {
          let! ctx = state.GetContext()

          let! x_type =
            var_type
            |> Option.map (
              TypeExpr.Eval config typeCheckExpr None loc0
              >> Expr<'T, 'Id, 'valueExt>.liftTypeEval
            )
            |> state.RunOption

          let! e1, _ =
            (x_type |> Option.map fst) => e1
            |> state.MapContext(TypeCheckContext.Updaters.IsTypeCheckingLetValue(replaceWith true))
          let t1 = e1.Type
          let k1 = e1.Kind

          match x_type with
          | Some(x_type, x_type_kind) ->
            do! x_type_kind |> Kind.AsStar |> ofSum |> state.Ignore

            do! TypeValue.Unify(loc0, t1, x_type) |> Expr<'T, 'Id, 'valueExt>.liftUnification
          | _ -> ()

          let! e2, ctx_e2 =
            !e2
            |> state.MapContext(
              TypeCheckContext.Updaters.Values(Map.add (x.Name |> Identifier.LocalScope |> ctx.Scope.Resolve) (t1, k1))
              >> TypeCheckContext.Updaters.IsTypeCheckingLetValue(replaceWith false)
            )

          let t2 = e2.Type
          let k2 = e2.Kind

          do!
            match var_type with
            | Some _ -> state { return () }
            | None ->
              let equalsLoc = { letLoc with Column = letLoc.Column + x.Name.Length + 2 }

              TypeCheckState.bindInlayHint (equalsLoc, x.Name, t1)

          return TypeCheckedExpr.Let(x, t1, e1, e2, t2, k2, loc0, ctx.Scope), ctx_e2
        }

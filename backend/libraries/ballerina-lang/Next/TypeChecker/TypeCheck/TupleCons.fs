namespace Ballerina.DSL.Next.Types.TypeChecker

module TupleCons =
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
  open Ballerina.DSL.Next.Types.TypeChecker.Primitive
  open Ballerina.DSL.Next.Types.TypeChecker.Lookup
  open Ballerina.DSL.Next.Types.TypeChecker.Lambda
  open Ballerina.DSL.Next.Types.TypeChecker.Apply
  open Ballerina.DSL.Next.Types.TypeChecker.If
  open Ballerina.DSL.Next.Types.TypeChecker.Let
  open Ballerina.DSL.Next.Types.TypeChecker.RecordCons
  open Ballerina.DSL.Next.Types.TypeChecker.RecordWith
  open Ballerina.DSL.Next.Types.TypeChecker.RecordDes
  open Ballerina.DSL.Next.Types.TypeChecker.UnionDes
  open Ballerina.DSL.Next.Types.TypeChecker.SumDes
  open Ballerina.DSL.Next.Types.TypeChecker.SumCons
  open Ballerina.DSL.Next.Types.TypeChecker.TupleDes
  open Ballerina.DSL.Next.Types.TypeChecker.TypeLambda
  open Ballerina.Fun
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList

  type Expr<'T, 'Id, 'valueExt when 'Id: comparison> with
    static member internal TypeCheckTupleCons
      (typeCheckExpr: ExprTypeChecker<'valueExt>, loc0: Location)
      : TypeChecker<ExprTupleCons<TypeExpr, Identifier, 'valueExt>, 'valueExt> =
      fun context_t ({ Items = fields }) ->
        let (!) = typeCheckExpr context_t

        let ofSum (p: Sum<'a, Ballerina.Errors.Errors>) =
          p |> Sum.mapRight (Errors.FromErrors loc0) |> state.OfSum

        state {
          let! ctx = state.GetContext()

          let! fields =
            fields
            |> List.map (fun (v) ->
              state {
                let! v, t_v, v_k = !v
                do! v_k |> Kind.AsStar |> ofSum |> state.Ignore
                return v, t_v
              })
            |> state.All

          let fieldsExpr = fields |> List.map fst
          let fieldsTypes = fields |> List.map snd

          let! return_t =
            TypeValue.CreateTuple fieldsTypes
            |> TypeValue.Instantiate loc0
            |> Expr.liftInstantiation

          return Expr.TupleCons(fieldsExpr, loc0, ctx.Types.Scope), return_t, Kind.Star
        }

namespace Ballerina.DSL.Next.Types.TypeChecker

module RecordWith =
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
  open Ballerina.DSL.Next.Types.TypeChecker.TypeLambda
  open Ballerina.Fun
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList

  type Expr<'T, 'Id, 'valueExt when 'Id: comparison> with
    static member internal TypeCheckRecordWith
      (typeCheckExpr: ExprTypeChecker<'valueExt>, loc0: Location)
      : TypeChecker<ExprRecordWith<TypeExpr, Identifier, 'valueExt>, 'valueExt> =
      fun context_t ({ Record = record; Fields = fields }) ->
        let (!) = typeCheckExpr context_t

        let ofSum (p: Sum<'a, Ballerina.Errors.Errors>) =
          p |> Sum.mapRight (Errors.FromErrors loc0) |> state.OfSum

        state {
          let! ctx = state.GetContext()
          let! record, t_record, k_record, _ = !record
          do! k_record |> Kind.AsStar |> ofSum |> state.Ignore

          let! t_record = t_record |> TypeValue.AsRecord |> ofSum

          let! fields =
            fields
            |> List.map (fun (k, v) ->
              state {
                let! id = TypeCheckState.TryResolveIdentifier(k, loc0)
                let! v, t_v, _v_k, _ = !v
                // do! v_k |> Kind.AsStar |> ofSum |> state.Ignore
                let! k_s = TypeCheckState.TryFindRecordFieldSymbol(id, loc0)

                let! t_v_record, _ = t_record |> OrderedMap.tryFindWithError k_s "fields" k.AsFSharpString |> ofSum

                do! TypeValue.Unify(loc0, t_v, t_v_record) |> Expr.liftUnification

                return (id, v), (k_s, t_v)
              })
            |> state.All

          let fieldsExpr = fields |> List.map fst

          let! t_record =
            t_record
            |> TypeValue.CreateRecord
            |> TypeValue.Instantiate TypeExpr.Eval loc0
            |> Expr.liftInstantiation

          return Expr.RecordWith(record, fieldsExpr, loc0, ctx.Scope), t_record, Kind.Star, ctx
        }

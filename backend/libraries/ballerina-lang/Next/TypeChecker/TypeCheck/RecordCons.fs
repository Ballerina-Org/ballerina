namespace Ballerina.DSL.Next.Types.TypeChecker

module RecordCons =
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

  type Expr<'T, 'Id, 've when 'Id: comparison> with
    static member internal TypeCheckRecordCons<'valueExt when 'valueExt: comparison>
      (typeCheckExpr: ExprTypeChecker<'valueExt>, loc0: Location)
      : TypeChecker<ExprRecordCons<TypeExpr<'valueExt>, Identifier, 'valueExt>, 'valueExt> =
      fun context_t ({ Fields = fields }) ->
        let (!) = typeCheckExpr context_t
        let (=>) c e = typeCheckExpr c e

        let ofSum (p: Sum<'a, Ballerina.Errors.Errors>) =
          p |> Sum.mapRight (Errors.FromErrors loc0) |> state.OfSum

        state {
          let! ctx = state.GetContext()

          let! fields =
            state {
              match context_t with
              | None ->
                return!
                  fields
                  |> List.map (fun (k, v) ->
                    state {
                      let! v, t_v, v_k, _ = !v
                      // do! v_k |> Kind.AsStar |> ofSum |> state.Ignore
                      let! id = TypeCheckState.TryResolveIdentifier(k, loc0)
                      let! k_s = TypeCheckState.TryFindRecordFieldSymbol(id, loc0)

                      return (id, v), (k_s, (t_v, v_k))
                    })
                  |> state.All
              | Some context_t ->
                let! context_fields = context_t |> TypeValue.AsRecord |> ofSum

                let context_fields =
                  context_fields
                  |> OrderedMap.toSeq
                  |> Seq.map (fun (k, v) -> (k.Name, (k, v)))
                  |> OrderedMap.ofSeq

                return!
                  fields
                  |> List.map (fun (k, v) ->
                    state {
                      let! k_s, (k_t_v, _) =
                        context_fields
                        |> OrderedMap.tryFindWithError k "fields" k.AsFSharpString
                        |> ofSum

                      let! v, t_v, v_k, _ = (Some k_t_v) => v
                      // do! v_k |> Kind.AsStar |> ofSum |> state.Ignore

                      do! TypeValue.Unify(loc0, t_v, k_t_v) |> Expr.liftUnification

                      let! id = TypeCheckState.TryResolveIdentifier(k_s, loc0)

                      return (id, v), (k_s, (t_v, v_k))
                    })
                  |> state.All
            }

          let fieldsExpr = fields |> List.map fst
          let fieldsTypes = fields |> List.map snd |> OrderedMap.ofList

          let! return_t =
            TypeValue.CreateRecord fieldsTypes
            |> TypeValue.Instantiate () (TypeExpr.Eval () typeCheckExpr) loc0
            |> Expr.liftInstantiation

          return Expr.RecordCons(fieldsExpr, loc0, ctx.Scope), return_t, Kind.Star, ctx
        }

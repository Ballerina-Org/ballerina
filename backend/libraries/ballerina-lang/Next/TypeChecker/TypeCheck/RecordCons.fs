namespace Ballerina.DSL.Next.Types.TypeChecker

module RecordCons =
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
  open Ballerina.DSL.Next.Types.TypeChecker.If
  open Ballerina.DSL.Next.Types.TypeChecker.Let
  open Ballerina.DSL.Next.Types.TypeChecker.TypeLambda
  open Ballerina.Fun
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList

  type Expr<'T, 'Id, 've when 'Id: comparison> with
    static member internal TypeCheckRecordCons<'valueExt
      when 'valueExt: comparison>
      (config: TypeCheckingConfig<'valueExt>)
      (typeCheckExpr: ExprTypeChecker<'valueExt>)
      : TypeChecker<
          ExprRecordCons<TypeExpr<'valueExt>, Identifier, 'valueExt>,
          'valueExt
         >
      =
      fun context_t ({ Fields = fields }) ->
        let (!) = typeCheckExpr context_t
        let (=>) c e = typeCheckExpr c e

        let loc0 =
          fields
          |> List.map (fun (_, v) -> v.Location)
          |> List.tryHead
          |> Option.defaultValue Location.Unknown

        let ofSum (p: Sum<'a, Errors<Unit>>) =
          p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

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
                      let! v, _ = !v
                      let t_v = v.Type
                      let v_k = v.Kind
                      // do! v_k |> Kind.AsStar |> ofSum |> state.Ignore
                      let! id = TypeCheckState.TryResolveIdentifier(k, loc0)

                      let! k_s =
                        TypeCheckState.TryFindRecordFieldSymbol(id, loc0)

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
                        |> OrderedMap.tryFindWithError
                          k
                          "fields"
                          k.AsFSharpString
                        |> ofSum

                      let! v, _ = (Some k_t_v) => v
                      let t_v = v.Type
                      let v_k = v.Kind
                      // do! v_k |> Kind.AsStar |> ofSum |> state.Ignore

                      do!
                        TypeValue.Unify(loc0, t_v, k_t_v)
                        |> Expr.liftUnification

                      let! id = TypeCheckState.TryResolveIdentifier(k_s, loc0)

                      return (id, v), (k_s, (t_v, v_k))
                    })
                  |> state.All
            }

          let fieldsExpr = fields |> List.map fst
          let fieldsTypes = fields |> List.map snd |> OrderedMap.ofList

          let inferredRecordTypeName =
            fieldsExpr
            |> List.choose (fun (id, _v) -> id.Type)
            |> List.distinct
            |> function
              | [ typeName ] -> Some typeName
              | _ -> None

          let! return_t =
            TypeValue.CreateRecord fieldsTypes
            |> TypeValue.Instantiate
              ()
              (TypeExpr.Eval config typeCheckExpr)
              loc0
            |> Expr.liftInstantiation

          let return_t =
            match inferredRecordTypeName with
            | Some typeName ->
              TypeValue.SetSourceMapping(
                return_t,
                TypeExprSourceMapping.OriginTypeExpr(
                  TypeExpr.Lookup(Identifier.LocalScope typeName)
                )
              )
            | None -> return_t

          return
            TypeCheckedExpr.RecordCons(
              fieldsExpr,
              return_t,
              Kind.Star,
              loc0,
              ctx.Scope
            ),
            ctx
        }

namespace Ballerina.DSL.Next.Types.TypeChecker

module RecordDes =
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
  open Ballerina.Fun
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Next.Types.TypeChecker.LiftOtherSteps

  type Expr<'T, 'Id, 've when 'Id: comparison> with
    static member internal TypeCheckRecordDes<'valueExt when 'valueExt: comparison>
      (typeCheckExpr: ExprTypeChecker<'valueExt>, loc0: Location)
      : TypeChecker<ExprRecordDes<TypeExpr<'valueExt>, Identifier, 'valueExt>, 'valueExt> =
      fun
          context_t
          ({ Expr = record_expr
             Field = fieldName }) ->
        let (!) = typeCheckExpr context_t

        let ofSum (p: Sum<'a, Errors<Unit>>) =
          p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

        state {
          let! ctx = state.GetContext()
          let! record_v, record_t, record_k, _ = !record_expr

          return!
            state.Either5
              (state {
                do! record_k |> Kind.AsStar |> ofSum |> state.Ignore

                let resolve_lookup (fields_t: OrderedMap<TypeSymbol, (TypeValue<'valueExt> * Kind)>) =
                  state {
                    let! field_n, (field_t, field_k) =
                      fields_t
                      |> OrderedMap.toSeq
                      |> Seq.map (fun (k, v) -> (k, v))
                      |> Seq.tryFind (fun (k, _v) -> k.Name.LocalName = fieldName.LocalName)
                      |> sum.OfOption(
                        (fun () -> $"Error: cannot find field {fieldName} in record {record_v}")
                        |> Errors<Unit>.Singleton()
                      )
                      |> ofSum

                    let! fieldName =
                      state.Either
                        (TypeCheckState.TryResolveIdentifier(field_n, loc0))
                        (state { return fieldName |> ctx.Scope.Resolve })

                    return Expr.RecordDes(record_v, fieldName, loc0, ctx.Scope), field_t, field_k, ctx
                  }
                  |> state.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))


                return!
                  state.Either
                    (state {
                      let! fields_t = record_t |> TypeValue.AsRecord |> ofSum
                      return! resolve_lookup fields_t
                    })
                    (state {
                      let! id = TypeCheckState.TryResolveIdentifier(fieldName, loc0)
                      let! fields_t = TypeCheckState.TryFindRecordField(id, loc0) |> state.Map fst
                      let expected_record_t = TypeValue.CreateRecord fields_t

                      do!
                        TypeValue.Unify(loc0, record_t, expected_record_t)
                        |> Expr.liftUnification
                        |> state.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))

                      return! resolve_lookup fields_t
                    })
                  |> state.MapError Errors<_>.FilterHighestPriorityOnly

              })
              (state {
                do! record_k |> Kind.AsSchema |> ofSum |> state.Ignore
                let! schema_t = record_t |> TypeValue.AsSchema |> ofSum

                return!
                  state {
                    let schema_v = record_v

                    if fieldName.LocalName = "Entities" then
                      return
                        Expr.EntitiesDes(schema_v, loc0, ctx.Scope), TypeValue.CreateEntities(schema_t), Kind.Star, ctx
                    elif fieldName.LocalName = "Relations" then
                      return
                        Expr.RelationsDes(schema_v, loc0, ctx.Scope),
                        TypeValue.CreateRelations(schema_t),
                        Kind.Star,
                        ctx
                    else

                      return!
                        Errors.Singleton loc0 (fun () -> $"Error: cannot find field {fieldName} in schema {schema_v}")
                        |> state.Throw
                  }
                  |> state.MapError(Errors<_>.MapPriority(replaceWith ErrorPriority.High))
              })
              (state {
                do! record_k |> Kind.AsStar |> ofSum |> state.Ignore
                let! schema_t = record_t |> TypeValue.AsEntities |> ofSum

                return!
                  state {
                    let schema_v = record_v

                    let fieldName = fieldName.LocalName |> SchemaEntityName.Create

                    let! entity =
                      schema_t.Entities
                      |> OrderedMap.tryFindWithError fieldName "entity" fieldName.Name
                      |> ofSum

                    return
                      Expr.EntityDes(schema_v, fieldName, loc0, ctx.Scope),
                      TypeValue.CreateEntity(schema_t, entity.TypeOriginal, entity.TypeWithProps, entity.Id),
                      Kind.Star,
                      ctx
                  }
                  |> state.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
              })
              (state {
                do! record_k |> Kind.AsStar |> ofSum |> state.Ignore
                let! schema_t = record_t |> TypeValue.AsRelations |> ofSum

                return!
                  state {
                    let schema_v = record_v

                    let fieldName = fieldName.LocalName |> SchemaRelationName.Create

                    let! relation =
                      schema_t.Relations
                      |> OrderedMap.tryFindWithError fieldName "relations" fieldName.Name
                      |> ofSum

                    let! from =
                      schema_t.Entities
                      |> OrderedMap.tryFindWithError
                        (relation.From.LocalName |> SchemaEntityName.Create)
                        "entity"
                        relation.From.LocalName
                      |> ofSum

                    let! to_ =
                      schema_t.Entities
                      |> OrderedMap.tryFindWithError
                        (relation.To.LocalName |> SchemaEntityName.Create)
                        "entity"
                        relation.To.LocalName
                      |> ofSum

                    return
                      Expr.RelationDes(schema_v, fieldName, loc0, ctx.Scope),
                      (match relation.Cardinality with
                       | None ->
                         TypeValue.CreateForeignKeyRelation(
                           schema_t,
                           fieldName,
                           from.TypeOriginal,
                           from.TypeWithProps,
                           from.Id,
                           to_.TypeOriginal,
                           to_.TypeWithProps,
                           to_.Id
                         )
                       | Some _ ->
                         TypeValue.CreateRelation(
                           schema_t,
                           fieldName,
                           relation.Cardinality,
                           from.TypeOriginal,
                           from.TypeWithProps,
                           from.Id,
                           to_.TypeOriginal,
                           to_.TypeWithProps,
                           to_.Id
                         )),
                      Kind.Star,
                      ctx
                  }
                  |> state.MapError(Errors.MapPriority(replaceWith ErrorPriority.High))
              })
              (state {
                do! record_k |> Kind.AsStar |> ofSum |> state.Ignore

                let! schema_t, relation_name, cardinality, _from, from', from_id, _to_, to', to_id =
                  record_t |> TypeValue.AsRelation |> ofSum

                let! cardinality =
                  cardinality
                  |> sum.OfOption((fun () -> "Error: relation cardinality is missing") |> Errors<Unit>.Singleton())
                  |> ofSum

                return!
                  state {
                    let! flipped =
                      state {
                        if fieldName.LocalName = "To" then
                          return false
                        elif fieldName.LocalName = "From" then
                          return true
                        else
                          return!
                            Errors.Singleton loc0 (fun () ->
                              $"Error: cannot find field {fieldName} in relation {record_v}")
                            |> state.Throw
                      }

                    let source_id, target', target_cardinality =
                      if flipped then
                        to_id, from', cardinality.From
                      else
                        from_id, to', cardinality.To

                    let result =
                      Expr.RelationLookupDes(
                        record_v,
                        relation_name,
                        (if flipped then
                           RelationLookupDirection.ToFrom
                         else
                           RelationLookupDirection.FromTo),
                        loc0,
                        ctx.Scope
                      )

                    match target_cardinality with
                    | Cardinality.Zero ->
                      return result, TypeValue.RelationLookupOption(schema_t, source_id, target'), Kind.Star, ctx
                    | Cardinality.One ->
                      return result, TypeValue.RelationLookupOne(schema_t, source_id, target'), Kind.Star, ctx
                    | Cardinality.Many ->
                      return result, TypeValue.RelationLookupMany(schema_t, source_id, target'), Kind.Star, ctx
                  }
                  |> state.MapError(Errors<_>.MapPriority(replaceWith ErrorPriority.High))
              })
            |> state.MapError Errors<_>.FilterHighestPriorityOnly

        }

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

  let private formatAvailableFieldNames (fieldNames: seq<string>) : string =
    let names = fieldNames |> Seq.distinct |> Seq.sort |> Seq.toList

    match names with
    | [] -> "none"
    | _ -> String.Join(", ", names)

  type Expr<'T, 'Id, 've when 'Id: comparison> with
    static member internal TypeCheckRecordDes<'valueExt
      when 'valueExt: comparison>
      (typeCheckExpr: ExprTypeChecker<'valueExt>)
      : TypeChecker<
          ExprRecordDes<TypeExpr<'valueExt>, Identifier, 'valueExt>,
          'valueExt
         >
      =
      fun
          context_t
          ({ Expr = record_expr
             Field = fieldName }) ->
        let (!) = typeCheckExpr context_t
        let loc0 = record_expr.Location

        let ofSum (p: Sum<'a, Errors<Unit>>) =
          p |> Sum.mapRight (Errors.MapContext(replaceWith loc0)) |> state.OfSum

        state {
          let! ctx = state.GetContext()
          let! record_v, _ = !record_expr
          let record_t = record_v.Type
          let record_k = record_v.Kind

          let resolve_lookup
            (fields_t: OrderedMap<TypeSymbol, (TypeValue<'valueExt> * Kind)>)
            =
            state {
              let availableFieldsMap =
                fields_t
                |> OrderedMap.toSeq
                |> Seq.map (fun (fieldSym, (fieldType, _)) ->
                  fieldSym.Name.LocalName, fieldType)
                |> Map.ofSeq

              do!
                TypeCheckState.bindDotAccessHint(
                  loc0,
                  record_t,
                  availableFieldsMap
                )

              let fieldFound =
                fields_t
                |> OrderedMap.toSeq
                |> Seq.map (fun (k, v) -> (k, v))
                |> Seq.tryFind (fun (k, _v) ->
                  k.Name.LocalName = fieldName.LocalName)

              match fieldFound with
              | Some(field_n, (field_t, field_k)) ->
                let! fieldName =
                  state.Either3
                    (TypeCheckState.TryResolveIdentifier(field_n, loc0))
                    (state { return fieldName |> ctx.Scope.Resolve })
                    (state.Throw(
                      Errors.Singleton loc0 (fun () ->
                        $"Error: cannot resolve field name {fieldName}")
                      |> Errors<_>.MapPriority(replaceWith ErrorPriority.High)
                    ))
                  |> state.MapError(Errors<_>.FilterHighestPriorityOnly)

                return
                  TypeCheckedExpr.RecordDes(
                    record_v,
                    fieldName,
                    field_t,
                    field_k,
                    loc0,
                    ctx.Scope
                  ),
                  ctx
              | None ->
                let resolvedFieldName = fieldName |> ctx.Scope.Resolve

                return
                  { TypeCheckedExpr.Expr =
                      TypeCheckedExprRec.ErrorRecordDesButInvalidField(
                        { TypeCheckedExprErrorRecordDesButInvalidField.Expr = record_v
                          Field = resolvedFieldName }
                      )
                    Location = loc0
                    Type = TypeValue.CreateUnit()
                    Kind = Kind.Star
                    Scope = ctx.Scope },
                  ctx
            }
            |> state.MapError(
              Errors.MapPriority(replaceWith ErrorPriority.High)
            )

          return!
            match record_k, record_t with
            | Kind.Schema, TypeValue.Schema schema_t ->
              state {
                let schema_v = record_v

                if fieldName.LocalName = "Entities" then
                  let t_res = TypeValue.CreateEntities(schema_t)
                  let k_res = Kind.Star

                  return
                    TypeCheckedExpr.EntitiesDes(
                      schema_v,
                      t_res,
                      k_res,
                      loc0,
                      ctx.Scope
                    ),
                    ctx
                elif fieldName.LocalName = "Relations" then
                  let t_res = TypeValue.CreateRelations(schema_t)
                  let k_res = Kind.Star

                  return
                    TypeCheckedExpr.RelationsDes(
                      schema_v,
                      t_res,
                      k_res,
                      loc0,
                      ctx.Scope
                    ),
                    ctx
                else

                  return!
                    Errors.Singleton loc0 (fun () ->
                      $"Error: cannot find field {fieldName} in schema {schema_v}")
                    |> Errors<_>
                      .MapPriority(
                        replaceWith (
                          if record_k = Kind.Schema then
                            ErrorPriority.High
                          else
                            ErrorPriority.Medium
                        )
                      )
                    |> state.Throw
              }

            | Kind.Star, TypeValue.Entities schema_t ->
              state {
                let schema_v = record_v

                let fieldName = fieldName.LocalName |> SchemaEntityName.Create

                let! entity =
                  schema_t.Entities
                  |> OrderedMap.tryFindWithError
                    fieldName
                    "entity"
                    fieldName.Name
                  |> ofSum

                let t_res =
                  TypeValue.CreateEntity(
                    schema_t,
                    entity.TypeOriginal,
                    entity.TypeWithProps,
                    entity.Id
                  )

                let k_res = Kind.Star

                return
                  TypeCheckedExpr.EntityDes(
                    schema_v,
                    fieldName,
                    t_res,
                    k_res,
                    loc0,
                    ctx.Scope
                  ),
                  ctx
              }
              |> state.MapError(
                Errors.MapPriority(replaceWith ErrorPriority.High)
              )

            | Kind.Star, TypeValue.Relations schema_t ->
              state {
                let schema_v = record_v

                let fieldName = fieldName.LocalName |> SchemaRelationName.Create

                let! relation =
                  schema_t.Relations
                  |> OrderedMap.tryFindWithError
                    fieldName
                    "relations"
                    fieldName.Name
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

                let t_res =
                  match relation.Cardinality with
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
                    )

                let k_res = Kind.Star

                return
                  TypeCheckedExpr.RelationDes(
                    schema_v,
                    fieldName,
                    t_res,
                    k_res,
                    loc0,
                    ctx.Scope
                  ),
                  ctx
              }
              |> state.MapError(
                Errors.MapPriority(replaceWith ErrorPriority.High)
              )

            | Kind.Star,
              TypeValue.Relation(schema_t,
                                 relation_name,
                                 cardinality,
                                 _from,
                                 from',
                                 from_id,
                                 _to_,
                                 to',
                                 to_id) ->
              state {
                let! cardinality =
                  cardinality
                  |> sum.OfOption(
                    (fun () -> "Error: relation cardinality is missing")
                    |> Errors<Unit>.Singleton()
                  )
                  |> ofSum

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

                let source_id, target', target_id, target_cardinality =
                  if flipped then
                    to_id, from', from_id, cardinality.From
                  else
                    from_id, to', to_id, cardinality.To

                let result =
                  TypeCheckedExpr.RelationLookupDes(
                    record_v,
                    relation_name,
                    (if flipped then
                       RelationLookupDirection.ToFrom
                     else
                       RelationLookupDirection.FromTo),
                    TypeValue.CreateUnit(),
                    Kind.Star,
                    loc0,
                    ctx.Scope
                  )

                match target_cardinality with
                | Cardinality.Zero ->
                  let t_res =
                    TypeValue.RelationLookupOption(
                      schema_t,
                      source_id,
                      target',
                      target_id
                    )

                  let k_res = Kind.Star

                  return
                    { result with
                        Type = t_res
                        Kind = k_res },
                    ctx
                | Cardinality.One ->
                  let t_res =
                    TypeValue.RelationLookupOne(
                      schema_t,
                      source_id,
                      target',
                      target_id
                    )

                  let k_res = Kind.Star

                  return
                    { result with
                        Type = t_res
                        Kind = k_res },
                    ctx
                | Cardinality.Many ->
                  let t_res =
                    TypeValue.RelationLookupMany(
                      schema_t,
                      source_id,
                      target',
                      target_id
                    )

                  let k_res = Kind.Star

                  return
                    { result with
                        Type = t_res
                        Kind = k_res },
                    ctx
              }
              |> state.MapError(
                Errors<_>.MapPriority(replaceWith ErrorPriority.High)
              )

            | Kind.Star, _ ->
              state {
                match record_t with
                | TypeValue.Record { value = fields_t } ->
                  return! resolve_lookup fields_t
                | _ ->
                  let! id = TypeCheckState.TryResolveIdentifier(fieldName, loc0)

                  let! fields_t =
                    TypeCheckState.TryFindRecordField(id, loc0) |> state.Map fst

                  let expected_record_t = TypeValue.CreateRecord fields_t

                  do!
                    TypeValue.Unify(loc0, record_t, expected_record_t)
                    |> Expr.liftUnification
                    |> state.MapError(
                      Errors.MapPriority(replaceWith ErrorPriority.High)
                    )

                  return! resolve_lookup fields_t
              }
              |> state.MapError Errors<_>.FilterHighestPriorityOnly

            | _ ->
              Errors.Singleton loc0 (fun () ->
                $"Type checking error: record lookup is not supported for kind {record_k} and type {record_t}")
              |> state.Throw

        }

    static member internal TypeCheckErrorDanglingRecordDes<'valueExt
      when 'valueExt: comparison>
      (typeCheckExpr: ExprTypeChecker<'valueExt>)
      (context_t: Option<TypeValue<'valueExt>>)
      (record_expr: Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>)
      (loc0: Location)
      : TypeCheckerResult<
          TypeCheckedExpr<'valueExt> * TypeCheckContext<'valueExt>,
          'valueExt
         >
      =
        let (!) = typeCheckExpr context_t

        state {
          let! ctx = state.GetContext()
          let! record_v, _ = !record_expr
          let record_t = record_v.Type
          let record_k = record_v.Kind

          match record_k, record_t with
          | Kind.Star, TypeValue.Record { value = fields_t } ->
            let availableFieldsMap =
              fields_t
              |> OrderedMap.toSeq
              |> Seq.map (fun (fieldSym, (fieldType, _)) ->
                fieldSym.Name.LocalName, fieldType)
              |> Map.ofSeq

            do!
              TypeCheckState.bindDotAccessHint(
                loc0,
                record_t,
                availableFieldsMap
              )
          | Kind.Schema, TypeValue.Schema schema_t ->
            let availableFieldsMap =
              [ "Entities", TypeValue.CreateEntities(schema_t)
                "Relations", TypeValue.CreateRelations(schema_t) ]
              |> Map.ofList

            do!
              TypeCheckState.bindDotAccessHint(
                loc0,
                record_t,
                availableFieldsMap
              )
          | Kind.Star, TypeValue.Entities schema_t ->
            let availableFieldsMap =
              schema_t.Entities
              |> OrderedMap.toSeq
              |> Seq.map (fun (name, _) ->
                name.Name, TypeValue.CreateUnit())
              |> Map.ofSeq

            do!
              TypeCheckState.bindDotAccessHint(
                loc0,
                record_t,
                availableFieldsMap
              )
          | Kind.Star, TypeValue.Relations schema_t ->
            let availableFieldsMap =
              schema_t.Relations
              |> OrderedMap.toSeq
              |> Seq.map (fun (name, _) ->
                name.Name, TypeValue.CreateUnit())
              |> Map.ofSeq

            do!
              TypeCheckState.bindDotAccessHint(
                loc0,
                record_t,
                availableFieldsMap
              )
          | Kind.Star, TypeValue.Relation(_, _, _, _, _, _, _, _, _) ->
            let availableFieldsMap =
              [ "From", TypeValue.CreateUnit()
                "To", TypeValue.CreateUnit() ]
              |> Map.ofList

            do!
              TypeCheckState.bindDotAccessHint(
                loc0,
                record_t,
                availableFieldsMap
              )
          | _ -> ()

          return
            { TypeCheckedExpr.Expr =
                TypeCheckedExprRec.ErrorDanglingRecordDes(
                  { TypeCheckedExprErrorDanglingRecordDes.Expr = record_v
                    Field = None }
                )
              Location = loc0
              Type = TypeValue.CreateUnit()
              Kind = Kind.Star
              Scope = ctx.Scope },
            ctx
        }

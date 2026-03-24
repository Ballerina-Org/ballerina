namespace Ballerina.API

module Upsert =
  open Microsoft.AspNetCore.Builder
  open Microsoft.AspNetCore.Routing
  open System
  open Ballerina.DSL.Next.Serialization.PocoObjects
  open Ballerina.DSL.Next.StdLib.Extensions
  open Ballerina.Collections.Sum
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.DSL.Next.Types
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina
  open APIUtils
  open Ballerina.DSL.Next.Serialization.ValueDeserializer
  open Ballerina.Reader.WithError
  open Ballerina.DSL.Next.Terms.Eval
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Next.Extensions
  open Microsoft.AspNetCore.Http
  open Ballerina.DSL.Next.Serialization.ValueSerializer
  open Ballerina.DSL.Next.StdLib.DB

  type EntityWithId =
    { Id: ValueDTO<ValueExtDTO>
      Entity: ValueDTO<ValueExtDTO> }

  type UpsertPayload =
    { EntityName: string
      EntityWithId: EntityWithId }

  type UpsertManyPayload =
    { EntityName: string
      Entities: EntityWithId[] }

  let upsert<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName
    when 'customExtension: comparison and 'db: comparison>
    (app: IEndpointRouteBuilder)
    (context: APIContext<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName>)
    =

    app.MapPost(
      "/{tenantId}/{schemaName}/upsert",
      Func<'tenantId, 'schemaName, bool, UpsertPayload, IResult>(fun tenantId schemaName draft payload ->
        let result =
          sum {
            let entityName, id, entity =
              payload.EntityName, payload.EntityWithId.Id, payload.EntityWithId.Entity

            let! dbio, languageContext, evalContext, typeCheckContext, typeCheckState =
              getDbDescriptor tenantId schemaName draft context

            let! _tableDescriptor =
              dbio.Schema.Entities
              |> OrderedMap.tryFind (entityName |> SchemaEntityName.Create)
              |> Sum.fromOption (fun () ->
                Errors<Location>.Singleton Location.Unknown (fun () ->
                  $"Entity {entityName} not found in schema {dbio.Schema}."))
              |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

            let! idValue =
              valueFromDTO >> runDTOConverter languageContext <| id
              |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

            let! entityValue =
              valueFromDTO >> runDTOConverter languageContext <| entity
              |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

            let idType, entityType = _tableDescriptor.Id, _tableDescriptor.TypeOriginal

            do! typeCheckValue idValue idType languageContext typeCheckContext typeCheckState
            do! typeCheckValue entityValue entityType languageContext typeCheckContext typeCheckState

            let updater
              : Expr<
                  TypeValue<ValueExt<'runtimeContext, 'db, 'customExtension>>,
                  ResolvedIdentifier,
                  ValueExt<'runtimeContext, 'db, 'customExtension>
                 > =
              Expr.Lambda(
                Var.Create "a'",
                None,
                Expr.Lambda(
                  Var.Create "a",
                  None,
                  Expr.FromValue(entityValue, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star),
                  None
                ),
                None
              )

            let! schema =
              dbio.SchemaAsValue
              |> Value.AsRecord
              |> sum.MapError(
                Errors.MapContext(replaceWith Location.Unknown)
                >> APIError<'runtimeContext, 'db, 'customExtension, Location>.Create
              )

            let! entities =
              schema
              |> Map.tryFindWithError
                ("Entities" |> Identifier.LocalScope |> ResolvedIdentifier.FromIdentifier)
                "schema"
                (fun () -> "Entities")
                Location.Unknown
              |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

            let! entities =
              entities
              |> Value.AsRecord
              |> sum.MapError(
                Errors.MapContext(replaceWith Location.Unknown)
                >> APIError<'runtimeContext, 'db, 'customExtension, Location>.Create
              )

            let! entityDescriptor =
              entities
              |> Map.tryFindWithError
                (entityName |> Identifier.LocalScope |> ResolvedIdentifier.FromIdentifier)
                "schema"
                (fun () -> "Entities")
                Location.Unknown
              |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

            let doUpsertExpr
              : Expr<
                  TypeValue<ValueExt<'runtimeContext, 'db, 'customExtension>>,
                  ResolvedIdentifier,
                  ValueExt<'runtimeContext, 'db, 'customExtension>
                 > =
              Expr.Apply(
                Expr.Apply(
                  Expr.Lookup(
                    Identifier.FullyQualified([ "DB" ], "upsert")
                    |> ResolvedIdentifier.FromIdentifier
                  ),
                  Expr.FromValue(entityDescriptor, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                ),
                Expr.TupleCons
                  [ Expr.FromValue(idValue, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                    Expr.FromValue(entityValue, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                    updater ]
              )

            let! evalResult =
              Expr.Eval(
                NonEmptyList.prependList languageContext.TypeCheckedPreludes (NonEmptyList.OfList(doUpsertExpr, []))
              )
              |> Reader.Run evalContext
              |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

            return!
              valueToDTO >> runDTOConverter languageContext <| evalResult
              |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create
          }

        apiResponseFromSum result id)
    )
    |> ignore

    app.MapPost(
      "/{tenantId}/{schemaName}/upsert-many",
      Func<'tenantId, 'schemaName, bool, UpsertManyPayload, IResult>(fun tenantId schemaName draft payload ->
        let entityName, entitiesWithId = payload.EntityName, payload.Entities

        let result =
          sum {
            let! dbio, languageContext, evalContext, typeCheckContext, typeCheckState =
              getDbDescriptor tenantId schemaName draft context

            let! _tableDescriptor =
              dbio.Schema.Entities
              |> OrderedMap.tryFind (entityName |> SchemaEntityName.Create)
              |> Sum.fromOption (fun () ->
                Errors<Location>.Singleton Location.Unknown (fun () ->
                  $"Entity {entityName} not found in schema {dbio.Schema}."))
              |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

            let! schema =
              dbio.SchemaAsValue
              |> Value.AsRecord
              |> sum.MapError(
                Errors.MapContext(replaceWith Location.Unknown)
                >> APIError<'runtimeContext, 'db, 'customExtension, Location>.Create
              )

            let! entities =
              schema
              |> Map.tryFindWithError
                ("Entities" |> Identifier.LocalScope |> ResolvedIdentifier.FromIdentifier)
                "schema"
                (fun () -> "Entities")
                Location.Unknown
              |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

            let! entities =
              entities
              |> Value.AsRecord
              |> sum.MapError(
                Errors.MapContext(replaceWith Location.Unknown)
                >> APIError<'runtimeContext, 'db, 'customExtension, Location>.Create
              )

            let! entityDescriptor =
              entities
              |> Map.tryFindWithError
                (entityName |> Identifier.LocalScope |> ResolvedIdentifier.FromIdentifier)
                "schema"
                (fun () -> "Entities")
                Location.Unknown
              |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

            let! upserters =
              entitiesWithId
              |> Array.map (fun entityWithId ->
                reader {
                  let! idValue = valueFromDTO entityWithId.Id
                  let! entityValue = valueFromDTO entityWithId.Entity

                  return
                    idValue,
                    Value.Tuple
                      [ entityValue
                        Value.Lambda(
                          Var.Create "b'",
                          Expr.Lambda(
                            Var.Create "b",
                            None,
                            Expr.FromValue(entityValue, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star),
                            None
                          ),
                          Map.empty,
                          TypeCheckScope.Empty
                        ) ]
                })
              |> reader.All
              |> runDTOConverter languageContext
              |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

            let idType, entityType = _tableDescriptor.Id, _tableDescriptor.TypeOriginal

            do!
              upserters
              |> List.map (fun (idValue, ballerinaTuple) ->
                sum {
                  match ballerinaTuple with
                  | Value.Tuple(entityValue :: _) ->
                    do! typeCheckValue idValue idType languageContext typeCheckContext typeCheckState
                    do! typeCheckValue entityValue entityType languageContext typeCheckContext typeCheckState
                  | _ ->
                    return!
                      sum.Throw(Errors.Singleton Location.Unknown (fun _ -> "Malformed upsert lambda parameter."))
                      |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create
                })
              |> Sum.All
              |> Sum.map (fun _ -> ())

            let upserters =
              upserters
              |> Map.ofList
              |> Ballerina.DSL.Next.StdLib.Map.Model.MapValues.Map
              |> MapExt.MapValues
              |> Choice6Of7
              |> ValueExt

            let upserters = Value.Ext(upserters, None)

            let doUpdateExpr
              : Expr<
                  TypeValue<ValueExt<'runtimeContext, 'db, 'customExtension>>,
                  ResolvedIdentifier,
                  ValueExt<'runtimeContext, 'db, 'customExtension>
                 > =
              Expr.Apply(
                Expr.Apply(
                  Expr.Lookup(
                    Identifier.FullyQualified([ "DB" ], "upsertMany")
                    |> ResolvedIdentifier.FromIdentifier
                  ),
                  Expr.FromValue(entityDescriptor, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                ),
                Expr.FromValue(upserters, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
              )

            let! evalResult =
              Expr.Eval(
                NonEmptyList.prependList languageContext.TypeCheckedPreludes (NonEmptyList.OfList(doUpdateExpr, []))
              )
              |> Reader.Run evalContext
              |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

            return!
              valueToDTO >> runDTOConverter languageContext <| evalResult
              |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create
          }

        apiResponseFromSum result id)
    )
    |> ignore

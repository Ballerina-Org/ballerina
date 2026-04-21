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
  open Ballerina.DSL.Next.Terms.FastEval
  open Ballerina.DSL.Next.Terms.Eval
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Next.Extensions
  open Microsoft.AspNetCore.Http
  open Ballerina.DSL.Next.Serialization.ValueSerializer
  open Ballerina.DSL.Next.StdLib.DB
  open Ballerina.Data.Delta.Serialization.DeltaDTO
  open Ballerina.Data.Delta.Serialization.DeltaDeserializer
  open Npgsql

  [<NoComparison; NoEquality>]
  type EntityWithId =
    { Id: ValueDTO<ValueExtDTO>
      Entity: ValueDTO<ValueExtDTO>
      Delta: DeltaDTO<ValueExtDTO, DeltaExtDTO> }

  let upsert<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName
    when 'customExtension: comparison and 'db: comparison>
    (app: IEndpointRouteBuilder)
    (context:
      APIRegistrationFactory<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName>)
    =

    app.MapPost(
      "/{tenantId}/{schemaName}/{entityName}/upsert",
      Func<
        HttpContext,
        'tenantId,
        'schemaName,
        string,
        EntityWithId,
        IResult
       >
        (fun httpContext tenantId schemaName entityName payload ->
          let txn : NpgsqlTransaction option ref = ref None
          let conn : NpgsqlConnection option ref = ref None
          let cleanup () =
            txn.Value |> Option.iter (fun t -> try t.Rollback() with _ -> ())
            conn.Value |> Option.iter (fun c -> try c.Dispose() with _ -> ())
            txn.Value <- None
            conn.Value <- None
          try
            let result =
              sum {
                let id, entity, delta = payload.Id, payload.Entity, payload.Delta

                let! dbio,
                     languageContext,
                     evalContext,
                     typeCheckContext,
                     typeCheckState,
                     dataSource =
                  getDbDescriptor tenantId schemaName context

                let! _tableDescriptor =
                  dbio.Schema.Entities
                  |> OrderedMap.tryFind (entityName |> SchemaEntityName.Create)
                  |> Sum.fromOption (fun () ->
                    Errors<Location>.Singleton Location.Unknown (fun () ->
                      $"Entity {entityName} not found in schema {dbio.Schema}."))
                  |> sum.MapError
                    APIError<'runtimeContext, 'db, 'customExtension, Location>
                      .Create

                let! idValue =
                  valueFromDTO >> runDTOConverter languageContext <| id
                  |> sum.MapError
                    APIError<'runtimeContext, 'db, 'customExtension, Location>
                      .Create

                let! entityValue =
                  valueFromDTO >> runDTOConverter languageContext <| entity
                  |> sum.MapError
                    APIError<'runtimeContext, 'db, 'customExtension, Location>
                      .Create

                let idType, entityType =
                  _tableDescriptor.Id, _tableDescriptor.TypeOriginal

                do!
                  typeCheckValue
                    idValue
                    idType
                    languageContext
                    typeCheckContext
                    typeCheckState

                do!
                  typeCheckValue
                    entityValue
                    entityType
                    languageContext
                    typeCheckContext
                    typeCheckState

                let! delta =
                  deltaFromDTO delta
                  |> Reader.Run languageContext.SerializationContext
                  |> sum.MapError(
                    Errors.MapContext(replaceWith Location.Unknown)
                  )
                  |> sum.MapError
                    APIError<'runtimeContext, 'db, 'customExtension, Location>
                      .Create

                let! updaterLambda =
                  createUpdaterFromDelta delta
                  |> sum.MapError(
                    Errors.MapContext(replaceWith Location.Unknown)
                  )
                  |> sum.MapError
                    APIError<'runtimeContext, 'db, 'customExtension, Location>
                      .Create

                let! schema =
                  dbio.SchemaAsValue
                  |> Value.AsRecord
                  |> sum.MapError(
                    Errors.MapContext(replaceWith Location.Unknown)
                    >> APIError<'runtimeContext, 'db, 'customExtension, Location>
                      .Create
                  )

                let! entities =
                  schema
                  |> Map.tryFindWithError
                    ("Entities"
                     |> Identifier.LocalScope
                     |> ResolvedIdentifier.FromIdentifier)
                    "schema"
                    (fun () -> "Entities")
                    Location.Unknown
                  |> sum.MapError
                    APIError<'runtimeContext, 'db, 'customExtension, Location>
                      .Create

                let! entities =
                  entities
                  |> Value.AsRecord
                  |> sum.MapError(
                    Errors.MapContext(replaceWith Location.Unknown)
                    >> APIError<'runtimeContext, 'db, 'customExtension, Location>
                      .Create
                  )

                let! entityDescriptor =
                  entities
                  |> Map.tryFindWithError
                    (entityName
                     |> Identifier.LocalScope
                     |> ResolvedIdentifier.FromIdentifier)
                    "schema"
                    (fun () -> "Entities")
                    Location.Unknown
                  |> sum.MapError
                    APIError<'runtimeContext, 'db, 'customExtension, Location>
                      .Create

                let doUpsertExpr
                  : RunnableExpr<
                      ValueExt<'runtimeContext, 'db, 'customExtension>
                     > =
                  RunnableExpr.UnsafeApplyForUntypedEval(
                    RunnableExpr.UnsafeApplyForUntypedEval(
                      RunnableExpr.UnsafeLookupForUntypedEval(
                        Identifier.FullyQualified([ "DB" ], "upsert")
                        |> ResolvedIdentifier.FromIdentifier
                      ),
                      RunnableExpr.FromValue(
                        entityDescriptor,
                        TypeValue.CreatePrimitive PrimitiveType.Unit,
                        Kind.Star
                      )
                    ),
                    RunnableExpr.UnsafeTupleConsForUntypedEval
                      [ RunnableExpr.FromValue(
                          idValue,
                          TypeValue.CreatePrimitive PrimitiveType.Unit,
                          Kind.Star
                        )
                        RunnableExpr.FromValue(
                          entityValue,
                          TypeValue.CreatePrimitive PrimitiveType.Unit,
                          Kind.Star
                        )
                        RunnableExpr.UnsafeLambdaForUntypedEval(
                          Var.Create "_",
                          TypeValue.CreatePrimitive PrimitiveType.Unit,
                          updaterLambda,
                          TypeValue.CreatePrimitive PrimitiveType.Unit
                        ) ]
                  )

                match dataSource with
                | Some ds ->
                  let c = ds.OpenConnection()
                  let t = c.BeginTransaction()
                  conn.Value <- Some c
                  txn.Value <- Some t
                | None -> ()

                let! evalResult =
                  Expr.Eval(
                    NonEmptyList.prependList
                      languageContext.TypeCheckedPreludes
                      (NonEmptyList.OfList(doUpsertExpr, []))
                  )
                  |> Reader.Run(
                    evalContext |> context.PermissionHookInjector httpContext
                  )
                  |> sum.MapError
                    APIError<'runtimeContext, 'db, 'customExtension, Location>
                      .Create

                txn.Value |> Option.iter (fun t -> t.Commit())
                conn.Value |> Option.iter (fun c -> c.Dispose())
                txn.Value <- None
                conn.Value <- None

                return!
                  valueToDTO >> runDTOConverter languageContext <| evalResult
                  |> sum.MapError
                    APIError<'runtimeContext, 'db, 'customExtension, Location>
                      .Create
              }

            apiResponseFromSum result (fun _ -> cleanup()) id
          with ex ->
            cleanup()
            reraise())
    )
    |> ignore

    app.MapPost(
      "/{tenantId}/{schemaName}/{entityName}/upsert-many",
      Func<
        HttpContext,
        'tenantId,
        'schemaName,
        string,
        EntityWithId[],
        IResult
       >
        (fun httpContext tenantId schemaName entityName payload ->
          let txn : NpgsqlTransaction option ref = ref None
          let conn : NpgsqlConnection option ref = ref None
          let cleanup () =
            txn.Value |> Option.iter (fun t -> try t.Rollback() with _ -> ())
            conn.Value |> Option.iter (fun c -> try c.Dispose() with _ -> ())
            txn.Value <- None
            conn.Value <- None
          try
            let result =
              sum {

                let! dbio,
                     languageContext,
                     evalContext,
                     typeCheckContext,
                     typeCheckState,
                     dataSource =
                  getDbDescriptor tenantId schemaName context

                let! _tableDescriptor =
                  dbio.Schema.Entities
                  |> OrderedMap.tryFind (entityName |> SchemaEntityName.Create)
                  |> Sum.fromOption (fun () ->
                    Errors<Location>.Singleton Location.Unknown (fun () ->
                      $"Entity {entityName} not found in schema {dbio.Schema}."))
                  |> sum.MapError
                    APIError<'runtimeContext, 'db, 'customExtension, Location>
                      .Create

                let! schema =
                  dbio.SchemaAsValue
                  |> Value.AsRecord
                  |> sum.MapError(
                    Errors.MapContext(replaceWith Location.Unknown)
                    >> APIError<'runtimeContext, 'db, 'customExtension, Location>
                      .Create
                  )

                let! entities =
                  schema
                  |> Map.tryFindWithError
                    ("Entities"
                     |> Identifier.LocalScope
                     |> ResolvedIdentifier.FromIdentifier)
                    "schema"
                    (fun () -> "Entities")
                    Location.Unknown
                  |> sum.MapError
                    APIError<'runtimeContext, 'db, 'customExtension, Location>
                      .Create

                let! entities =
                  entities
                  |> Value.AsRecord
                  |> sum.MapError(
                    Errors.MapContext(replaceWith Location.Unknown)
                    >> APIError<'runtimeContext, 'db, 'customExtension, Location>
                      .Create
                  )

                let! entityDescriptor =
                  entities
                  |> Map.tryFindWithError
                    (entityName
                     |> Identifier.LocalScope
                     |> ResolvedIdentifier.FromIdentifier)
                    "schema"
                    (fun () -> "Entities")
                    Location.Unknown
                  |> sum.MapError
                    APIError<'runtimeContext, 'db, 'customExtension, Location>
                      .Create

                let! upserters =
                  payload
                  |> Array.map (fun entityWithId ->
                    reader {
                      let! delta = deltaFromDTO entityWithId.Delta

                      let! idValue =
                        valueFromDTO entityWithId.Id
                        |> reader.MapContext(fun deltaSerializationContext ->
                          deltaSerializationContext.SerializationContext)

                      let! entityValue =
                        valueFromDTO entityWithId.Entity
                        |> reader.MapContext(fun deltaSerializationContext ->
                          deltaSerializationContext.SerializationContext)

                      let! updaterLambda =
                        createUpdaterFromDelta delta |> reader.OfSum


                      return
                        idValue,
                        Value.Tuple
                          [ entityValue
                            Value.Lambda(
                              Var.Create "_",
                              updaterLambda,
                              Map.empty,
                              TypeCheckScope.Empty
                            ) ]
                    })
                  |> reader.All
                  |> Reader.Run languageContext.SerializationContext
                  |> sum.MapError(
                    Errors.MapContext(replaceWith Location.Unknown)
                  )
                  |> sum.MapError
                    APIError<'runtimeContext, 'db, 'customExtension, Location>
                      .Create

                let idType, entityType =
                  _tableDescriptor.Id, _tableDescriptor.TypeOriginal

                do!
                  upserters
                  |> List.map (fun (idValue, ballerinaTuple) ->
                    sum {
                      match ballerinaTuple with
                      | Value.Tuple(entityValue :: _) ->
                        do!
                          typeCheckValue
                            idValue
                            idType
                            languageContext
                            typeCheckContext
                            typeCheckState

                        do!
                          typeCheckValue
                            entityValue
                            entityType
                            languageContext
                            typeCheckContext
                            typeCheckState
                      | _ ->
                        return!
                          sum.Throw(
                            Errors.Singleton Location.Unknown (fun _ ->
                              "Malformed upsert lambda parameter.")
                          )
                          |> sum.MapError
                            APIError<
                              'runtimeContext,
                              'db,
                              'customExtension,
                              Location
                             >.Create
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
                  : RunnableExpr<
                      ValueExt<'runtimeContext, 'db, 'customExtension>
                     > =
                  RunnableExpr.UnsafeApplyForUntypedEval(
                    RunnableExpr.UnsafeApplyForUntypedEval(
                      RunnableExpr.UnsafeLookupForUntypedEval(
                        Identifier.FullyQualified([ "DB" ], "upsertMany")
                        |> ResolvedIdentifier.FromIdentifier
                      ),
                      RunnableExpr.FromValue(
                        entityDescriptor,
                        TypeValue.CreatePrimitive PrimitiveType.Unit,
                        Kind.Star
                      )
                    ),
                    RunnableExpr.FromValue(
                      upserters,
                      TypeValue.CreatePrimitive PrimitiveType.Unit,
                      Kind.Star
                    )
                  )

                match dataSource with
                | Some ds ->
                  let c = ds.OpenConnection()
                  let t = c.BeginTransaction()
                  conn.Value <- Some c
                  txn.Value <- Some t
                | None -> ()

                let! evalResult =
                  Expr.Eval(
                    NonEmptyList.prependList
                      languageContext.TypeCheckedPreludes
                      (NonEmptyList.OfList(doUpdateExpr, []))
                  )
                  |> Reader.Run(
                    evalContext |> context.PermissionHookInjector httpContext
                  )
                  |> sum.MapError
                    APIError<'runtimeContext, 'db, 'customExtension, Location>
                      .Create

                txn.Value |> Option.iter (fun t -> t.Commit())
                conn.Value |> Option.iter (fun c -> c.Dispose())
                txn.Value <- None
                conn.Value <- None

                return!
                  valueToDTO >> runDTOConverter languageContext <| evalResult
                  |> sum.MapError
                    APIError<'runtimeContext, 'db, 'customExtension, Location>
                      .Create
              }

            apiResponseFromSum result (fun _ -> cleanup()) id
          with ex ->
            cleanup()
            reraise())
    )
    |> ignore

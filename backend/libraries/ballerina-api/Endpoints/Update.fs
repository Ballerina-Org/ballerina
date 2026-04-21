namespace Ballerina.API

module Update =
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
  open Ballerina.DSL.Next.Types.TypeChecker.Value
  open Ballerina.DSL.Next.StdLib.DB
  open Ballerina.Data.Delta.Serialization.DeltaDTO
  open Ballerina.Data.Delta.Serialization.DeltaDeserializer
  open Ballerina.Data.Delta.ToUpdater
  open Ballerina.Data.Delta
  open Ballerina.DSL.Next.StdLib.Updater.Model
  open Npgsql

  [<NoComparison; NoEquality>]
  type UpdateDeltaWithId =
    { Id: ValueDTO<ValueExtDTO>
      Delta: DeltaDTO<ValueExtDTO, DeltaExtDTO> }

  let update<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName
    when 'customExtension: comparison and 'db: comparison>
    (app: IEndpointRouteBuilder)
    (context:
      APIRegistrationFactory<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName>)
    =
    app.MapPost(
      "/{tenantId}/{schemaName}/{entityName}/update",
      Func<
        HttpContext,
        'tenantId,
        'schemaName,
        string,
        UpdateDeltaWithId,
        IResult
       >
        (fun httpContext tenantId schemaName entityName payload ->
          let entityId = payload.Id
          let delta = payload.Delta

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
                    Errors.Singleton Location.Unknown (fun () ->
                      $"Entity {entityName} not found in schema {dbio.Schema}."))
                  |> sum.MapError
                    APIError<'runtimeContext, 'db, 'customExtension, Location>
                      .Create

                let! schema =
                  dbio.SchemaAsValue
                  |> Value.AsRecord
                  |> toUknonwLocation
                  |> sum.MapError
                    APIError<'runtimeContext, 'db, 'customExtension, Location>
                      .Create

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
                  )
                  |> sum.MapError
                    APIError<'runtimeContext, 'db, 'customExtension, Location>
                      .Create

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

                let! idValue =
                  runDTOConverter languageContext (valueFromDTO entityId)
                  |> sum.MapError
                    APIError<'runtimeContext, 'db, 'customExtension, Location>
                      .Create

                let idType = _tableDescriptor.Id

                do!
                  typeCheckValue
                    idValue
                    idType
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

                let doUpdateExpr
                  : RunnableExpr<
                      ValueExt<'runtimeContext, 'db, 'customExtension>
                     > =

                  RunnableExpr.UnsafeApplyForUntypedEval(
                    RunnableExpr.UnsafeApplyForUntypedEval(
                      RunnableExpr.UnsafeLookupForUntypedEval(
                        Identifier.FullyQualified([ "DB" ], "update")
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

                let! result =
                  runDTOConverter languageContext (valueToDTO evalResult)
                  |> sum.MapError
                    APIError<'runtimeContext, 'db, 'customExtension, Location>
                      .Create

                return result
              }
            apiResponseFromSum result (fun _ -> cleanup()) id
          with ex ->
            cleanup()
            reraise())
    )
    |> ignore

    app.MapPost(
      "/{tenantId}/{schemaName}/{entityName}/update-many",
      Func<
        HttpContext,
        'tenantId,
        'schemaName,
        string,
        UpdateDeltaWithId[],
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
                    Errors.Singleton Location.Unknown (fun () ->
                      $"Entity {entityName} not found in schema {dbio.Schema}."))
                  |> sum.MapError
                    APIError<'runtimeContext, 'db, 'customExtension, Location>
                      .Create

                let! schema =
                  dbio.SchemaAsValue
                  |> Value.AsRecord
                  |> toUknonwLocation
                  |> sum.MapError
                    APIError<'runtimeContext, 'db, 'customExtension, Location>
                      .Create

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
                  )
                  |> sum.MapError
                    APIError<'runtimeContext, 'db, 'customExtension, Location>
                      .Create

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

                let! updaters =
                  payload
                  |> Array.map (fun updater ->
                    reader {
                      let! delta = deltaFromDTO updater.Delta

                      let! idValue =
                        valueFromDTO updater.Id
                        |> reader.MapContext(fun deltaSerializationContext ->
                          deltaSerializationContext.SerializationContext)

                      let! updaterLambda =
                        createUpdaterFromDelta delta |> reader.OfSum

                      return
                        idValue,
                        Value.Lambda(
                          Var.Create "_",
                          updaterLambda,
                          Map.empty,
                          TypeCheckScope.Empty
                        )
                    })
                  |> reader.All
                  |> Reader.Run languageContext.SerializationContext
                  |> sum.MapError(
                    Errors.MapContext(replaceWith Location.Unknown)
                  )
                  |> sum.MapError
                    APIError<'runtimeContext, 'db, 'customExtension, Location>
                      .Create

                let idType = _tableDescriptor.Id

                do!
                  updaters
                  |> List.map (fun (idValue, _) ->
                    typeCheckValue
                      idValue
                      idType
                      languageContext
                      typeCheckContext
                      typeCheckState)
                  |> Sum.All
                  |> Sum.map (fun _ -> ())

                let updaters =
                  updaters
                  |> Map.ofList
                  |> Ballerina.DSL.Next.StdLib.Map.Model.MapValues.Map
                  |> MapExt.MapValues
                  |> VMap

                let updaters = Value.Ext(updaters, None)

                let doUpdateExpr
                  : RunnableExpr<
                      ValueExt<'runtimeContext, 'db, 'customExtension>
                     > =
                  RunnableExpr.UnsafeApplyForUntypedEval(
                    RunnableExpr.UnsafeApplyForUntypedEval(
                      RunnableExpr.UnsafeLookupForUntypedEval(
                        Identifier.FullyQualified([ "DB" ], "updateMany")
                        |> ResolvedIdentifier.FromIdentifier
                      ),
                      RunnableExpr.FromValue(
                        entityDescriptor,
                        TypeValue.CreatePrimitive PrimitiveType.Unit,
                        Kind.Star
                      )
                    ),
                    RunnableExpr.FromValue(
                      updaters,
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

                let! result =
                  runDTOConverter languageContext (valueToDTO evalResult)
                  |> sum.MapError
                    APIError<'runtimeContext, 'db, 'customExtension, Location>
                      .Create

                return result
              }
            apiResponseFromSum result (fun _ -> cleanup()) id
          with ex ->
            cleanup()
            reraise())
    )
    |> ignore

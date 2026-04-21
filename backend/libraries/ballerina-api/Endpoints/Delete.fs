namespace Ballerina.API

module Delete =
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
  open Npgsql

  let delete<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName
    when 'customExtension: comparison and 'db: comparison>
    (app: IEndpointRouteBuilder)
    (context:
      APIRegistrationFactory<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName>)
    =

    app.MapPost(
      "/{tenantId}/{schemaName}/{entityName}/delete",
      Func<
        HttpContext,
        'tenantId,
        'schemaName,
        string,
        ValueDTO<ValueExtDTO>,
        IResult
       >
        (fun httpContext tenantId schemaName entityName payload ->
          let entityId = payload
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

                let doDeleteExpr
                  : RunnableExpr<
                      ValueExt<'runtimeContext, 'db, 'customExtension>
                     > =
                  RunnableExpr.UnsafeApplyForUntypedEval(
                    RunnableExpr.UnsafeApplyForUntypedEval(
                      RunnableExpr.UnsafeLookupForUntypedEval(
                        Identifier.FullyQualified([ "DB" ], "delete")
                        |> ResolvedIdentifier.FromIdentifier
                      ),
                      RunnableExpr.FromValue(
                        entityDescriptor,
                        TypeValue.CreatePrimitive PrimitiveType.Unit,
                        Kind.Star
                      )
                    ),
                    RunnableExpr.FromValue(
                      idValue,
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
                      (NonEmptyList.OfList(doDeleteExpr, []))
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
                  runDTOConverter languageContext (valueToDTO evalResult)
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
      "/{tenantId}/{schemaName}/{entityName}/delete-many",
      Func<
        HttpContext,
        'tenantId,
        'schemaName,
        string,
        ValueDTO<ValueExtDTO>[],
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
                let ids = payload

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

                let! deleters =
                  ids
                  |> Array.map (
                    valueFromDTO
                    >> reader.MapError(
                      Errors.MapContext(replaceWith Location.Unknown)
                    )
                  )
                  |> reader.All
                  |> Reader.Run
                    languageContext.SerializationContext.SerializationContext
                  |> sum.MapError
                    APIError<'runtimeContext, 'db, 'customExtension, Location>
                      .Create

                let idType = _tableDescriptor.Id

                do!
                  deleters
                  |> List.map (fun deleter ->
                    typeCheckValue
                      deleter
                      idType
                      languageContext
                      typeCheckContext
                      typeCheckState)
                  |> Sum.All
                  |> Sum.map (fun _ -> ())

                let deleters =
                  deleters
                  |> List.map (fun deleter ->
                    deleter, Value.Primitive PrimitiveValue.Unit)
                  |> Map.ofList
                  |> Ballerina.DSL.Next.StdLib.Map.Model.MapValues.Map
                  |> MapExt.MapValues
                  |> Choice6Of7
                  |> ValueExt


                let deleters
                  : Value<
                      TypeValue<ValueExt<'runtimeContext, 'db, 'customExtension>>,
                      ValueExt<'runtimeContext, 'db, 'customExtension>
                     > =
                  Ext(deleters, None)

                let doUpdateExpr
                  : RunnableExpr<
                      ValueExt<'runtimeContext, 'db, 'customExtension>
                     > =
                  RunnableExpr.UnsafeApplyForUntypedEval(
                    RunnableExpr.UnsafeApplyForUntypedEval(
                      RunnableExpr.UnsafeLookupForUntypedEval(
                        Identifier.FullyQualified([ "DB" ], "deleteMany")
                        |> ResolvedIdentifier.FromIdentifier
                      ),
                      RunnableExpr.FromValue(
                        entityDescriptor,
                        TypeValue.CreatePrimitive PrimitiveType.Unit,
                        Kind.Star
                      )
                    ),
                    RunnableExpr.FromValue(
                      deleters,
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
                  runDTOConverter languageContext (valueToDTO evalResult)
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

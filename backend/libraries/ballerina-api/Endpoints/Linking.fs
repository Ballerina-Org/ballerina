namespace Ballerina.API

module Linking =
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

  [<NoComparison; NoEquality>]
  type LinkPayload =
    { FromId: ValueDTO<ValueExtDTO>
      ToId: ValueDTO<ValueExtDTO> }

  [<NoComparison; NoEquality>]
  type MovePayload =
    { FromId: ValueDTO<ValueExtDTO>
      SourceId: ValueDTO<ValueExtDTO>
      TargetId: ValueDTO<ValueExtDTO> }

  let link<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName
    when 'customExtension: comparison and 'db: comparison>
    (app: IEndpointRouteBuilder)
    (context:
      APIRegistrationFactory<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName>)
    =

    app.MapPost(
      "/{tenantId}/{schemaName}/{relationName}/link",
      Func<
        HttpContext,
        'tenantId,
        'schemaName,
        string,
        LinkPayload,
        IResult
       >
        (fun httpContext tenantId schemaName relationName payload ->
          let fromId, toId = payload.FromId, payload.ToId

          let result =
            sum {
              let! dbio,
                   languageContext,
                   evalContext,
                   typeCheckContext,
                   typeCheckState, _ =
                getDbDescriptor tenantId schemaName context

              let! fromIdValue =
                runDTOConverter languageContext (valueFromDTO fromId)
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let! toIdValue =
                runDTOConverter languageContext (valueFromDTO toId)
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create


              let! _tableDescriptor =
                dbio.Schema.Relations
                |> OrderedMap.tryFind (
                  relationName |> SchemaRelationName.Create
                )
                |> Sum.fromOption (fun () ->
                  Errors<Location>.Singleton Location.Unknown (fun () ->
                    $"Relation {relationName} not found in schema {dbio.Schema}."))
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let fromName, toName =
                _tableDescriptor.From.ToString(),
                _tableDescriptor.To.ToString()

              let! _fromDescriptor =
                dbio.Schema.Entities
                |> OrderedMap.tryFind (fromName |> SchemaEntityName.Create)
                |> Sum.fromOption (fun () ->
                  Errors.Singleton Location.Unknown (fun () ->
                    $"Entity {fromName} not found in schema {dbio.Schema}."))
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let! _toDescriptor =
                dbio.Schema.Entities
                |> OrderedMap.tryFind (toName |> SchemaEntityName.Create)
                |> Sum.fromOption (fun () ->
                  Errors.Singleton Location.Unknown (fun () ->
                    $"Entity {toName} not found in schema {dbio.Schema}."))
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let fromIdType, toIdType = _fromDescriptor.Id, _toDescriptor.Id

              do!
                typeCheckValue
                  fromIdValue
                  fromIdType
                  languageContext
                  typeCheckContext
                  typeCheckState

              do!
                typeCheckValue
                  toIdValue
                  toIdType
                  languageContext
                  typeCheckContext
                  typeCheckState

              let! schema =
                dbio.SchemaAsValue
                |> Value.AsRecord
                |> sum.MapError(
                  Errors.MapContext(replaceWith Location.Unknown)
                )
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let! relations =
                schema
                |> Map.tryFindWithError
                  ("Relations"
                   |> Identifier.LocalScope
                   |> ResolvedIdentifier.FromIdentifier)
                  "schema"
                  (fun () -> "Relations")
                  Location.Unknown
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let! relations =
                relations
                |> Value.AsRecord
                |> sum.MapError(
                  Errors.MapContext(replaceWith Location.Unknown)
                  >> APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create
                )

              let! relationDescriptor =
                relations
                |> Map.tryFindWithError
                  (relationName
                   |> Identifier.LocalScope
                   |> ResolvedIdentifier.FromIdentifier)
                  "schema"
                  (fun () -> relationName)
                  Location.Unknown
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let doLinkExpr
                : RunnableExpr<
                    ValueExt<'runtimeContext, 'db, 'customExtension>
                   > =
                RunnableExpr.UnsafeApplyForUntypedEval(
                  RunnableExpr.UnsafeApplyForUntypedEval(
                    RunnableExpr.UnsafeLookupForUntypedEval(
                      Identifier.FullyQualified([ "DB" ], "link")
                      |> ResolvedIdentifier.FromIdentifier
                    ),
                    RunnableExpr.FromValue(
                      relationDescriptor,
                      TypeValue.CreatePrimitive PrimitiveType.Unit,
                      Kind.Star
                    )
                  ),
                  RunnableExpr.UnsafeTupleConsForUntypedEval
                    [ RunnableExpr.FromValue(
                        fromIdValue,
                        TypeValue.CreatePrimitive PrimitiveType.Unit,
                        Kind.Star
                      )
                      RunnableExpr.FromValue(
                        toIdValue,
                        TypeValue.CreatePrimitive PrimitiveType.Unit,
                        Kind.Star
                      ) ]
                )

              let! evalResult =
                Expr.Eval(
                  NonEmptyList.prependList
                    languageContext.TypeCheckedPreludes
                    (NonEmptyList.OfList(doLinkExpr, []))
                )
                |> Reader.Run(
                  evalContext |> context.PermissionHookInjector httpContext
                )
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              return!
                runDTOConverter languageContext (valueToDTO evalResult)
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create
            }

          apiResponseFromSum result (fun _ -> ()) id)
    )
    |> ignore

    app.MapPost(
      "/{tenantId}/{schemaName}/{relationName}/unlink",
      Func<
        HttpContext,
        'tenantId,
        'schemaName,
        string,
        LinkPayload,
        IResult
       >
        (fun httpContext tenantId schemaName relationName payload ->
          let fromId, toId = payload.FromId, payload.ToId

          let result =
            sum {
              let! dbio,
                   languageContext,
                   evalContext,
                   typeCheckContext,
                   typeCheckState, _ =
                getDbDescriptor tenantId schemaName context

              let! fromIdValue =
                runDTOConverter languageContext (valueFromDTO fromId)
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let! toIdValue =
                runDTOConverter languageContext (valueFromDTO toId)
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let! _tableDescriptor =
                dbio.Schema.Relations
                |> OrderedMap.tryFind (
                  relationName |> SchemaRelationName.Create
                )
                |> Sum.fromOption (fun () ->
                  Errors<Location>.Singleton Location.Unknown (fun () ->
                    $"Relation {relationName} not found in schema {dbio.Schema}."))
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let fromName, toName =
                _tableDescriptor.From.ToString(),
                _tableDescriptor.To.ToString()

              let! _fromDescriptor =
                dbio.Schema.Entities
                |> OrderedMap.tryFind (fromName |> SchemaEntityName.Create)
                |> Sum.fromOption (fun () ->
                  Errors.Singleton Location.Unknown (fun () ->
                    $"Entity {fromName} not found in schema {dbio.Schema}."))
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let! _toDescriptor =
                dbio.Schema.Entities
                |> OrderedMap.tryFind (toName |> SchemaEntityName.Create)
                |> Sum.fromOption (fun () ->
                  Errors.Singleton Location.Unknown (fun () ->
                    $"Entity {toName} not found in schema {dbio.Schema}."))
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let fromIdType, toIdType = _fromDescriptor.Id, _toDescriptor.Id

              do!
                typeCheckValue
                  fromIdValue
                  fromIdType
                  languageContext
                  typeCheckContext
                  typeCheckState

              do!
                typeCheckValue
                  toIdValue
                  toIdType
                  languageContext
                  typeCheckContext
                  typeCheckState

              let! schema =
                dbio.SchemaAsValue
                |> Value.AsRecord
                |> sum.MapError(
                  Errors.MapContext(replaceWith Location.Unknown)
                )
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let! relations =
                schema
                |> Map.tryFindWithError
                  ("Relations"
                   |> Identifier.LocalScope
                   |> ResolvedIdentifier.FromIdentifier)
                  "schema"
                  (fun () -> "Relations")
                  Location.Unknown
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let! relations =
                relations
                |> Value.AsRecord
                |> sum.MapError(
                  Errors.MapContext(replaceWith Location.Unknown)
                  >> APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create
                )

              let! relationDescriptor =
                relations
                |> Map.tryFindWithError
                  (relationName
                   |> Identifier.LocalScope
                   |> ResolvedIdentifier.FromIdentifier)
                  "schema"
                  (fun () -> relationName)
                  Location.Unknown
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let doUnlinkExpr
                : RunnableExpr<
                    ValueExt<'runtimeContext, 'db, 'customExtension>
                   > =
                RunnableExpr.UnsafeApplyForUntypedEval(
                  RunnableExpr.UnsafeApplyForUntypedEval(
                    RunnableExpr.UnsafeLookupForUntypedEval(
                      Identifier.FullyQualified([ "DB" ], "unlink")
                      |> ResolvedIdentifier.FromIdentifier
                    ),
                    RunnableExpr.FromValue(
                      relationDescriptor,
                      TypeValue.CreatePrimitive PrimitiveType.Unit,
                      Kind.Star
                    )
                  ),
                  RunnableExpr.UnsafeTupleConsForUntypedEval
                    [ RunnableExpr.FromValue(
                        fromIdValue,
                        TypeValue.CreatePrimitive PrimitiveType.Unit,
                        Kind.Star
                      )
                      RunnableExpr.FromValue(
                        toIdValue,
                        TypeValue.CreatePrimitive PrimitiveType.Unit,
                        Kind.Star
                      ) ]
                )

              let! evalResult =
                Expr.Eval(
                  NonEmptyList.prependList
                    languageContext.TypeCheckedPreludes
                    (NonEmptyList.OfList(doUnlinkExpr, []))
                )
                |> Reader.Run(
                  evalContext |> context.PermissionHookInjector httpContext
                )
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              return!
                runDTOConverter languageContext (valueToDTO evalResult)
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create
            }

          apiResponseFromSum result (fun _ -> ()) id)
    )
    |> ignore

  let private moveEndpoint<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName
    when 'customExtension: comparison and 'db: comparison>
    (app: IEndpointRouteBuilder)
    (context:
      APIRegistrationFactory<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName>)
    (routeSuffix: string)
    (dbIdentifier: string)
    =

    app.MapPost(
      $"/{{tenantId}}/{{schemaName}}/{{relationName}}/{routeSuffix}",
      Func<
        HttpContext,
        'tenantId,
        'schemaName,
        string,
        MovePayload,
        IResult
       >
        (fun httpContext tenantId schemaName relationName payload ->
          let fromId, sourceId, targetId =
            payload.FromId, payload.SourceId, payload.TargetId

          let result =
            sum {
              let! dbio,
                   languageContext,
                   evalContext,
                   typeCheckContext,
                   typeCheckState, _ =
                getDbDescriptor tenantId schemaName context

              let! fromIdValue =
                runDTOConverter languageContext (valueFromDTO fromId)
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let! sourceIdValue =
                runDTOConverter languageContext (valueFromDTO sourceId)
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let! targetIdValue =
                runDTOConverter languageContext (valueFromDTO targetId)
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let! _tableDescriptor =
                dbio.Schema.Relations
                |> OrderedMap.tryFind (
                  relationName |> SchemaRelationName.Create
                )
                |> Sum.fromOption (fun () ->
                  Errors<Location>.Singleton Location.Unknown (fun () ->
                    $"Relation {relationName} not found in schema {dbio.Schema}."))
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let fromName, toName =
                _tableDescriptor.From.ToString(),
                _tableDescriptor.To.ToString()

              let! _fromDescriptor =
                dbio.Schema.Entities
                |> OrderedMap.tryFind (fromName |> SchemaEntityName.Create)
                |> Sum.fromOption (fun () ->
                  Errors.Singleton Location.Unknown (fun () ->
                    $"Entity {fromName} not found in schema {dbio.Schema}."))
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let! _toDescriptor =
                dbio.Schema.Entities
                |> OrderedMap.tryFind (toName |> SchemaEntityName.Create)
                |> Sum.fromOption (fun () ->
                  Errors.Singleton Location.Unknown (fun () ->
                    $"Entity {toName} not found in schema {dbio.Schema}."))
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let fromIdType, toIdType = _fromDescriptor.Id, _toDescriptor.Id

              do!
                typeCheckValue
                  fromIdValue
                  fromIdType
                  languageContext
                  typeCheckContext
                  typeCheckState

              do!
                typeCheckValue
                  sourceIdValue
                  toIdType
                  languageContext
                  typeCheckContext
                  typeCheckState

              do!
                typeCheckValue
                  targetIdValue
                  toIdType
                  languageContext
                  typeCheckContext
                  typeCheckState

              let! schema =
                dbio.SchemaAsValue
                |> Value.AsRecord
                |> sum.MapError(
                  Errors.MapContext(replaceWith Location.Unknown)
                )
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let! relations =
                schema
                |> Map.tryFindWithError
                  ("Relations"
                   |> Identifier.LocalScope
                   |> ResolvedIdentifier.FromIdentifier)
                  "schema"
                  (fun () -> "Relations")
                  Location.Unknown
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let! relations =
                relations
                |> Value.AsRecord
                |> sum.MapError(
                  Errors.MapContext(replaceWith Location.Unknown)
                  >> APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create
                )

              let! relationDescriptor =
                relations
                |> Map.tryFindWithError
                  (relationName
                   |> Identifier.LocalScope
                   |> ResolvedIdentifier.FromIdentifier)
                  "schema"
                  (fun () -> relationName)
                  Location.Unknown
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let doMoveExpr
                : RunnableExpr<
                    ValueExt<'runtimeContext, 'db, 'customExtension>
                   > =
                RunnableExpr.UnsafeApplyForUntypedEval(
                  RunnableExpr.UnsafeApplyForUntypedEval(
                    RunnableExpr.UnsafeLookupForUntypedEval(
                      Identifier.FullyQualified([ "DB" ], dbIdentifier)
                      |> ResolvedIdentifier.FromIdentifier
                    ),
                    RunnableExpr.FromValue(
                      relationDescriptor,
                      TypeValue.CreatePrimitive PrimitiveType.Unit,
                      Kind.Star
                    )
                  ),
                  RunnableExpr.UnsafeTupleConsForUntypedEval
                    [ RunnableExpr.FromValue(
                        fromIdValue,
                        TypeValue.CreatePrimitive PrimitiveType.Unit,
                        Kind.Star
                      )
                      RunnableExpr.FromValue(
                        sourceIdValue,
                        TypeValue.CreatePrimitive PrimitiveType.Unit,
                        Kind.Star
                      )
                      RunnableExpr.FromValue(
                        targetIdValue,
                        TypeValue.CreatePrimitive PrimitiveType.Unit,
                        Kind.Star
                      ) ]
                )

              let! evalResult =
                Expr.Eval(
                  NonEmptyList.prependList
                    languageContext.TypeCheckedPreludes
                    (NonEmptyList.OfList(doMoveExpr, []))
                )
                |> Reader.Run(
                  evalContext |> context.PermissionHookInjector httpContext
                )
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              return!
                runDTOConverter languageContext (valueToDTO evalResult)
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create
            }

          apiResponseFromSum result (fun _ -> ()) id)
    )
    |> ignore

  let private moveEndpointReverse<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName
    when 'customExtension: comparison and 'db: comparison>
    (app: IEndpointRouteBuilder)
    (context:
      APIRegistrationFactory<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName>)
    (routeSuffix: string)
    (dbIdentifier: string)
    =

    app.MapPost(
      $"/{{tenantId}}/{{schemaName}}/{{relationName}}/{routeSuffix}",
      Func<
        HttpContext,
        'tenantId,
        'schemaName,
        string,
        MovePayload,
        IResult
       >
        (fun httpContext tenantId schemaName relationName payload ->
          let fromId, sourceId, targetId =
            payload.FromId, payload.SourceId, payload.TargetId

          let result =
            sum {
              let! dbio,
                   languageContext,
                   evalContext,
                   typeCheckContext,
                   typeCheckState, _ =
                getDbDescriptor tenantId schemaName context

              let! fromIdValue =
                runDTOConverter languageContext (valueFromDTO fromId)
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let! sourceIdValue =
                runDTOConverter languageContext (valueFromDTO sourceId)
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let! targetIdValue =
                runDTOConverter languageContext (valueFromDTO targetId)
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let! _tableDescriptor =
                dbio.Schema.Relations
                |> OrderedMap.tryFind (
                  relationName |> SchemaRelationName.Create
                )
                |> Sum.fromOption (fun () ->
                  Errors<Location>.Singleton Location.Unknown (fun () ->
                    $"Relation {relationName} not found in schema {dbio.Schema}."))
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let fromName, toName =
                _tableDescriptor.From.ToString(),
                _tableDescriptor.To.ToString()

              let! _fromDescriptor =
                dbio.Schema.Entities
                |> OrderedMap.tryFind (fromName |> SchemaEntityName.Create)
                |> Sum.fromOption (fun () ->
                  Errors.Singleton Location.Unknown (fun () ->
                    $"Entity {fromName} not found in schema {dbio.Schema}."))
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let! _toDescriptor =
                dbio.Schema.Entities
                |> OrderedMap.tryFind (toName |> SchemaEntityName.Create)
                |> Sum.fromOption (fun () ->
                  Errors.Singleton Location.Unknown (fun () ->
                    $"Entity {toName} not found in schema {dbio.Schema}."))
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let fromIdType, toIdType = _fromDescriptor.Id, _toDescriptor.Id

              // Reverse: FromId is a ToId, Source/Target are FromIds
              do!
                typeCheckValue
                  fromIdValue
                  toIdType
                  languageContext
                  typeCheckContext
                  typeCheckState

              do!
                typeCheckValue
                  sourceIdValue
                  fromIdType
                  languageContext
                  typeCheckContext
                  typeCheckState

              do!
                typeCheckValue
                  targetIdValue
                  fromIdType
                  languageContext
                  typeCheckContext
                  typeCheckState

              let! schema =
                dbio.SchemaAsValue
                |> Value.AsRecord
                |> sum.MapError(
                  Errors.MapContext(replaceWith Location.Unknown)
                )
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let! relations =
                schema
                |> Map.tryFindWithError
                  ("Relations"
                   |> Identifier.LocalScope
                   |> ResolvedIdentifier.FromIdentifier)
                  "schema"
                  (fun () -> "Relations")
                  Location.Unknown
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let! relations =
                relations
                |> Value.AsRecord
                |> sum.MapError(
                  Errors.MapContext(replaceWith Location.Unknown)
                  >> APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create
                )

              let! relationDescriptor =
                relations
                |> Map.tryFindWithError
                  (relationName
                   |> Identifier.LocalScope
                   |> ResolvedIdentifier.FromIdentifier)
                  "schema"
                  (fun () -> relationName)
                  Location.Unknown
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let doMoveExpr
                : RunnableExpr<
                    ValueExt<'runtimeContext, 'db, 'customExtension>
                   > =
                RunnableExpr.UnsafeApplyForUntypedEval(
                  RunnableExpr.UnsafeApplyForUntypedEval(
                    RunnableExpr.UnsafeLookupForUntypedEval(
                      Identifier.FullyQualified([ "DB" ], dbIdentifier)
                      |> ResolvedIdentifier.FromIdentifier
                    ),
                    RunnableExpr.FromValue(
                      relationDescriptor,
                      TypeValue.CreatePrimitive PrimitiveType.Unit,
                      Kind.Star
                    )
                  ),
                  RunnableExpr.UnsafeTupleConsForUntypedEval
                    [ RunnableExpr.FromValue(
                        fromIdValue,
                        TypeValue.CreatePrimitive PrimitiveType.Unit,
                        Kind.Star
                      )
                      RunnableExpr.FromValue(
                        sourceIdValue,
                        TypeValue.CreatePrimitive PrimitiveType.Unit,
                        Kind.Star
                      )
                      RunnableExpr.FromValue(
                        targetIdValue,
                        TypeValue.CreatePrimitive PrimitiveType.Unit,
                        Kind.Star
                      ) ]
                )

              let! evalResult =
                Expr.Eval(
                  NonEmptyList.prependList
                    languageContext.TypeCheckedPreludes
                    (NonEmptyList.OfList(doMoveExpr, []))
                )
                |> Reader.Run(
                  evalContext |> context.PermissionHookInjector httpContext
                )
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              return!
                runDTOConverter languageContext (valueToDTO evalResult)
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create
            }

          apiResponseFromSum result (fun _ -> ()) id)
    )
    |> ignore

  let moveRelation<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName
    when 'customExtension: comparison and 'db: comparison>
    (app: IEndpointRouteBuilder)
    (context:
      APIRegistrationFactory<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName>)
    =
    moveEndpoint app context "move-before" "moveBefore"
    moveEndpoint app context "move-after" "moveAfter"
    moveEndpointReverse app context "move-before-reverse" "moveBeforeReverse"
    moveEndpointReverse app context "move-after-reverse" "moveAfterReverse"

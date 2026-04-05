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
  open Ballerina.DSL.Next.Terms.Eval
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Next.Extensions
  open Microsoft.AspNetCore.Http
  open Ballerina.DSL.Next.Serialization.ValueSerializer

  type LinkPayload =
    { FromId: ValueDTO<ValueExtDTO>
      ToId: ValueDTO<ValueExtDTO> }

  let link<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName
    when 'customExtension: comparison and 'db: comparison>
    (app: IEndpointRouteBuilder)
    (context: APIContext<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName>)
    =

    app.MapPost(
      "/{tenantId}/{schemaName}/{relationName}/link",
      Func<HttpContext, 'tenantId, 'schemaName, string, bool, LinkPayload, IResult>
        (fun httpContext tenantId schemaName relationName draft payload ->
          let fromId, toId = payload.FromId, payload.ToId

          let result =
            sum {
              let! dbio, languageContext, evalContext, typeCheckContext, typeCheckState =
                getDbDescriptor tenantId schemaName draft context

              let! fromIdValue =
                runDTOConverter languageContext (valueFromDTO fromId)
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              let! toIdValue =
                runDTOConverter languageContext (valueFromDTO toId)
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create


              let! _tableDescriptor =
                dbio.Schema.Relations
                |> OrderedMap.tryFind (relationName |> SchemaRelationName.Create)
                |> Sum.fromOption (fun () ->
                  Errors<Location>.Singleton Location.Unknown (fun () ->
                    $"Relation {relationName} not found in schema {dbio.Schema}."))
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              let fromName, toName =
                _tableDescriptor.From.ToString(), _tableDescriptor.To.ToString()

              let! _fromDescriptor =
                dbio.Schema.Entities
                |> OrderedMap.tryFind (fromName |> SchemaEntityName.Create)
                |> Sum.fromOption (fun () ->
                  Errors.Singleton Location.Unknown (fun () ->
                    $"Entity {fromName} not found in schema {dbio.Schema}."))
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              let! _toDescriptor =
                dbio.Schema.Entities
                |> OrderedMap.tryFind (toName |> SchemaEntityName.Create)
                |> Sum.fromOption (fun () ->
                  Errors.Singleton Location.Unknown (fun () -> $"Entity {toName} not found in schema {dbio.Schema}."))
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              let fromIdType, toIdType = _fromDescriptor.Id, _toDescriptor.Id

              do! typeCheckValue fromIdValue fromIdType languageContext typeCheckContext typeCheckState
              do! typeCheckValue toIdValue toIdType languageContext typeCheckContext typeCheckState

              let! schema =
                dbio.SchemaAsValue
                |> Value.AsRecord
                |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              let! relations =
                schema
                |> Map.tryFindWithError
                  ("Relations" |> Identifier.LocalScope |> ResolvedIdentifier.FromIdentifier)
                  "schema"
                  (fun () -> "Relations")
                  Location.Unknown
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              let! relations =
                relations
                |> Value.AsRecord
                |> sum.MapError(
                  Errors.MapContext(replaceWith Location.Unknown)
                  >> APIError<'runtimeContext, 'db, 'customExtension, Location>.Create
                )

              let! relationDescriptor =
                relations
                |> Map.tryFindWithError
                  (relationName |> Identifier.LocalScope |> ResolvedIdentifier.FromIdentifier)
                  "schema"
                  (fun () -> relationName)
                  Location.Unknown
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              let doLinkExpr: TypeCheckedExpr<ValueExt<'runtimeContext, 'db, 'customExtension>> =
                TypeCheckedExpr.Apply(
                  TypeCheckedExpr.Apply(
                    TypeCheckedExpr.Lookup(
                      Identifier.FullyQualified([ "DB" ], "link") |> ResolvedIdentifier.FromIdentifier
                    ),
                    TypeCheckedExpr.FromValue(
                      relationDescriptor,
                      TypeValue.CreatePrimitive PrimitiveType.Unit,
                      Kind.Star
                    )
                  ),
                  TypeCheckedExpr.TupleCons
                    [ TypeCheckedExpr.FromValue(fromIdValue, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                      TypeCheckedExpr.FromValue(toIdValue, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star) ]
                )

              let! evalResult =
                Expr.Eval(
                  NonEmptyList.prependList languageContext.TypeCheckedPreludes (NonEmptyList.OfList(doLinkExpr, []))
                )
                |> Reader.Run(evalContext |> context.PermissionHookInjector httpContext)
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              return!
                runDTOConverter languageContext (valueToDTO evalResult)
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create
            }

          apiResponseFromSum result id)
    )
    |> ignore

    app.MapPost(
      "/{tenantId}/{schemaName}/{relationName}/unlink",
      Func<HttpContext, 'tenantId, 'schemaName, string, bool, LinkPayload, IResult>
        (fun httpContext tenantId schemaName relationName draft payload ->
          let fromId, toId = payload.FromId, payload.ToId

          let result =
            sum {
              let! dbio, languageContext, evalContext, typeCheckContext, typeCheckState =
                getDbDescriptor tenantId schemaName draft context

              let! fromIdValue =
                runDTOConverter languageContext (valueFromDTO fromId)
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              let! toIdValue =
                runDTOConverter languageContext (valueFromDTO toId)
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              let! _tableDescriptor =
                dbio.Schema.Relations
                |> OrderedMap.tryFind (relationName |> SchemaRelationName.Create)
                |> Sum.fromOption (fun () ->
                  Errors<Location>.Singleton Location.Unknown (fun () ->
                    $"Relation {relationName} not found in schema {dbio.Schema}."))
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              let fromName, toName =
                _tableDescriptor.From.ToString(), _tableDescriptor.To.ToString()

              let! _fromDescriptor =
                dbio.Schema.Entities
                |> OrderedMap.tryFind (fromName |> SchemaEntityName.Create)
                |> Sum.fromOption (fun () ->
                  Errors.Singleton Location.Unknown (fun () ->
                    $"Entity {fromName} not found in schema {dbio.Schema}."))
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              let! _toDescriptor =
                dbio.Schema.Entities
                |> OrderedMap.tryFind (toName |> SchemaEntityName.Create)
                |> Sum.fromOption (fun () ->
                  Errors.Singleton Location.Unknown (fun () -> $"Entity {toName} not found in schema {dbio.Schema}."))
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              let fromIdType, toIdType = _fromDescriptor.Id, _toDescriptor.Id

              do! typeCheckValue fromIdValue fromIdType languageContext typeCheckContext typeCheckState
              do! typeCheckValue toIdValue toIdType languageContext typeCheckContext typeCheckState

              let! schema =
                dbio.SchemaAsValue
                |> Value.AsRecord
                |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              let! relations =
                schema
                |> Map.tryFindWithError
                  ("Relations" |> Identifier.LocalScope |> ResolvedIdentifier.FromIdentifier)
                  "schema"
                  (fun () -> "Relations")
                  Location.Unknown
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              let! relations =
                relations
                |> Value.AsRecord
                |> sum.MapError(
                  Errors.MapContext(replaceWith Location.Unknown)
                  >> APIError<'runtimeContext, 'db, 'customExtension, Location>.Create
                )

              let! relationDescriptor =
                relations
                |> Map.tryFindWithError
                  (relationName |> Identifier.LocalScope |> ResolvedIdentifier.FromIdentifier)
                  "schema"
                  (fun () -> relationName)
                  Location.Unknown
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              let doUnlinkExpr: TypeCheckedExpr<ValueExt<'runtimeContext, 'db, 'customExtension>> =
                TypeCheckedExpr.Apply(
                  TypeCheckedExpr.Apply(
                    TypeCheckedExpr.Lookup(
                      Identifier.FullyQualified([ "DB" ], "unlink")
                      |> ResolvedIdentifier.FromIdentifier
                    ),
                    TypeCheckedExpr.FromValue(
                      relationDescriptor,
                      TypeValue.CreatePrimitive PrimitiveType.Unit,
                      Kind.Star
                    )
                  ),
                  TypeCheckedExpr.TupleCons
                    [ TypeCheckedExpr.FromValue(fromIdValue, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                      TypeCheckedExpr.FromValue(toIdValue, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star) ]
                )

              let! evalResult =
                Expr.Eval(
                  NonEmptyList.prependList languageContext.TypeCheckedPreludes (NonEmptyList.OfList(doUnlinkExpr, []))
                )
                |> Reader.Run(evalContext |> context.PermissionHookInjector httpContext)
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              return!
                runDTOConverter languageContext (valueToDTO evalResult)
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create
            }

          apiResponseFromSum result id)
    )
    |> ignore

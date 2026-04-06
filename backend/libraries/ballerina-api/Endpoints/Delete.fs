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
  open Ballerina.DSL.Next.Terms.Eval
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Next.Extensions
  open Microsoft.AspNetCore.Http
  open Ballerina.DSL.Next.Serialization.ValueSerializer

  let delete<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName
    when 'customExtension: comparison and 'db: comparison>
    (app: IEndpointRouteBuilder)
    (context: APIContext<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName>)
    =

    app.MapPost(
      "/{tenantId}/{schemaName}/{entityName}/delete",
      Func<HttpContext, 'tenantId, 'schemaName, string, bool, ValueDTO<ValueExtDTO>, IResult>
        (fun httpContext tenantId schemaName entityName draft payload ->
          let entityId = payload

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

              let! idValue =
                runDTOConverter languageContext (valueFromDTO entityId)
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              let idType = _tableDescriptor.Id

              do! typeCheckValue idValue idType languageContext typeCheckContext typeCheckState

              let doDeleteExpr: TypeCheckedExpr<ValueExt<'runtimeContext, 'db, 'customExtension>> =
                TypeCheckedExpr.UnsafeApplyForUntypedEval(
                  TypeCheckedExpr.UnsafeApplyForUntypedEval(
                    TypeCheckedExpr.UnsafeLookupForUntypedEval(
                      Identifier.FullyQualified([ "DB" ], "delete")
                      |> ResolvedIdentifier.FromIdentifier
                    ),
                    TypeCheckedExpr.FromValue(
                      entityDescriptor,
                      TypeValue.CreatePrimitive PrimitiveType.Unit,
                      Kind.Star
                    )
                  ),
                  TypeCheckedExpr.FromValue(idValue, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                )

              let! evalResult =
                Expr.Eval(
                  NonEmptyList.prependList languageContext.TypeCheckedPreludes (NonEmptyList.OfList(doDeleteExpr, []))
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
      "/{tenantId}/{schemaName}/{entityName}/delete-many",
      Func<HttpContext, 'tenantId, 'schemaName, string, bool, ValueDTO<ValueExtDTO>[], IResult>
        (fun httpContext tenantId schemaName entityName draft payload ->
          let result =
            sum {
              let ids = payload

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

              let! deleters =
                ids
                |> Array.map (valueFromDTO >> reader.MapError(Errors.MapContext(replaceWith Location.Unknown)))
                |> reader.All
                |> Reader.Run languageContext.SerializationContext.SerializationContext
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              let idType = _tableDescriptor.Id

              do!
                deleters
                |> List.map (fun deleter ->
                  typeCheckValue deleter idType languageContext typeCheckContext typeCheckState)
                |> Sum.All
                |> Sum.map (fun _ -> ())

              let deleters =
                deleters
                |> List.map (fun deleter -> deleter, Value.Primitive PrimitiveValue.Unit)
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

              let doUpdateExpr: TypeCheckedExpr<ValueExt<'runtimeContext, 'db, 'customExtension>> =
                TypeCheckedExpr.UnsafeApplyForUntypedEval(
                  TypeCheckedExpr.UnsafeApplyForUntypedEval(
                    TypeCheckedExpr.UnsafeLookupForUntypedEval(
                      Identifier.FullyQualified([ "DB" ], "deleteMany")
                      |> ResolvedIdentifier.FromIdentifier
                    ),
                    TypeCheckedExpr.FromValue(
                      entityDescriptor,
                      TypeValue.CreatePrimitive PrimitiveType.Unit,
                      Kind.Star
                    )
                  ),
                  TypeCheckedExpr.FromValue(deleters, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                )

              let! evalResult =
                Expr.Eval(
                  NonEmptyList.prependList languageContext.TypeCheckedPreludes (NonEmptyList.OfList(doUpdateExpr, []))
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

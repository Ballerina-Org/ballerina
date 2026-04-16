namespace Ballerina.API

module Read =
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

  type GetManyResponseItem =
    { Key: ValueDTO<ValueExtDTO>
      Value: ValueDTO<ValueExtDTO> }

  let checkRelatedEntityId
    (relationName: string)
    (direction: string)
    (idValue:
      Value<
        TypeValue<ValueExt<'runtimeContext, 'db, 'customExtension>>,
        ValueExt<'runtimeContext, 'db, 'customExtension>
       >)
    (dbio: DBIO<'runtimeContext, 'db, ValueExt<'runtimeContext, 'db, 'customExtension>>)
    languageContext
    typeCheckContext
    typeCheckState
    =
    sum {

      let! tableDescriptor =
        dbio.Schema.Relations
        |> OrderedMap.tryFind (relationName |> SchemaRelationName.Create)
        |> Sum.fromOption (fun () ->
          Errors<Location>.Singleton Location.Unknown (fun () ->
            $"Relation {relationName} not found in schema {dbio.Schema}."))
        |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      match direction.ToLower() with
      | "from" ->
        let fromName = tableDescriptor.From.ToString()

        let! fromDescriptor =
          dbio.Schema.Entities
          |> OrderedMap.tryFind (fromName |> SchemaEntityName.Create)
          |> Sum.fromOption (fun () ->
            Errors.Singleton Location.Unknown (fun () -> $"Entity {fromName} not found in schema {dbio.Schema}."))
          |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

        let fromIdType = fromDescriptor.Id

        do! typeCheckValue idValue fromIdType languageContext typeCheckContext typeCheckState
      | "to" ->
        let toName = tableDescriptor.To.ToString()

        let! toDescriptor =
          dbio.Schema.Entities
          |> OrderedMap.tryFind (toName |> SchemaEntityName.Create)
          |> Sum.fromOption (fun () ->
            Errors.Singleton Location.Unknown (fun () -> $"Entity {toName} not found in schema {dbio.Schema}."))
          |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

        let toIdType = toDescriptor.Id

        do! typeCheckValue idValue toIdType languageContext typeCheckContext typeCheckState
      | _ ->
        return!
          sum.Throw(
            Errors.Singleton Location.Unknown (fun () ->
              $"Invalid direction option: {direction}. Allowed values are 'From' and 'To'")
            |> APIError<'runtimeContext, 'db, 'customExtension, Location>.Create
          )
    }

  let get<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName
    when 'customExtension: comparison and 'db: comparison>
    (app: IEndpointRouteBuilder)
    (context: APIRegistrationFactory<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName>)
    =

    app.MapPost(
      "/{tenantId}/{schemaName}/{entityName}/get-by-id",
      Func<HttpContext, 'tenantId, 'schemaName, string, bool, ValueDTO<ValueExtDTO>, IResult>
        (fun httpContext tenantId schemaName entityName draft idDTO ->

          let result =
            sum {
              let! dbio, languageContext, evalContext, typeCheckContext, typeCheckState =
                getDbDescriptor tenantId schemaName draft context

              let! idValue =
                valueFromDTO >> runDTOConverter languageContext <| idDTO
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              let! tableDescriptor =
                dbio.Schema.Entities
                |> OrderedMap.tryFind (entityName |> SchemaEntityName.Create)
                |> Sum.fromOption (fun () ->
                  Errors<Location>.Singleton Location.Unknown (fun () ->
                    $"Entity {entityName} not found in schema {dbio.Schema}."))
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              let idType = tableDescriptor.Id

              do! typeCheckValue idValue idType languageContext typeCheckContext typeCheckState

              let! entityDescriptor =
                entityDescriptorFromDb dbio entityName
                |> sum.MapError(
                  Errors.MapContext(replaceWith Location.Unknown)
                  >> APIError<'runtimeContext, 'db, 'customExtension, Location>.Create
                )



              let doLookupExpr: RunnableExpr<ValueExt<'runtimeContext, 'db, 'customExtension>> =
                RunnableExpr.UnsafeApplyForUntypedEval(
                  RunnableExpr.UnsafeApplyForUntypedEval(
                    RunnableExpr.UnsafeLookupForUntypedEval(
                      Identifier.FullyQualified([ "DB" ], $"getById")
                      |> ResolvedIdentifier.FromIdentifier
                    ),
                    RunnableExpr.FromValue(
                      entityDescriptor,
                      TypeValue.CreatePrimitive PrimitiveType.Unit,
                      Kind.Star
                    )
                  ),
                  RunnableExpr.FromValue(idValue, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                )

              let! evalResult =
                Expr.Eval(
                  NonEmptyList.prependList languageContext.TypeCheckedPreludes (NonEmptyList.OfList(doLookupExpr, []))
                )
                |> Reader.Run(evalContext |> context.PermissionHookInjector httpContext)
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              let! resultDTO =
                valueToDTO >> runDTOConverter languageContext <| evalResult
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              return idDTO, resultDTO
            }

          apiResponseFromSum result id)
    )
    |> ignore

    app.MapGet(
      "/{tenantId}/{schemaName}/{entityName}/many",
      Func<HttpContext, 'tenantId, 'schemaName, string, bool, int, int, IResult>
        (fun httpContext tenantId schemaName entityName draft (offset: int) (limit: int) ->
          let result =
            sum {
              let! dbio, languageContext, evalContext, _, _ = getDbDescriptor tenantId schemaName draft context

              let! entityDescriptor =
                entityDescriptorFromDb dbio entityName
                |> sum.MapError(
                  Errors.MapContext(replaceWith Location.Unknown)
                  >> APIError<'runtimeContext, 'db, 'customExtension, Location>.Create
                )

              let doLookupExpr: RunnableExpr<ValueExt<'runtimeContext, 'db, 'customExtension>> =
                RunnableExpr.UnsafeApplyForUntypedEval(
                  RunnableExpr.UnsafeApplyForUntypedEval(
                    RunnableExpr.UnsafeLookupForUntypedEval(
                      Identifier.FullyQualified([ "DB" ], $"getMany")
                      |> ResolvedIdentifier.FromIdentifier
                    ),
                    RunnableExpr.FromValue(
                      entityDescriptor,
                      TypeValue.CreatePrimitive PrimitiveType.Unit,
                      Kind.Star
                    )
                  ),
                  RunnableExpr.UnsafeTupleConsForUntypedEval
                    [ RunnableExpr.UnsafePrimitiveForUntypedEval(PrimitiveValue.Int32 offset)
                      RunnableExpr.UnsafePrimitiveForUntypedEval(PrimitiveValue.Int32 limit) ]
                )

              let! evalResult =
                Expr.Eval(
                  NonEmptyList.prependList languageContext.TypeCheckedPreludes (NonEmptyList.OfList(doLookupExpr, []))
                )
                |> Reader.Run(evalContext |> context.PermissionHookInjector httpContext)
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              let! resultDTO =
                valueToDTO >> runDTOConverter languageContext <| evalResult
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              return resultDTO
            }

          apiResponseFromSum result id)
    )
    |> ignore

  let lookup<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName
    when 'customExtension: comparison and 'db: comparison>
    (app: IEndpointRouteBuilder)
    (context: APIRegistrationFactory<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName>)
    =

    app.MapPost(
      "/{tenantId}/{schemaName}/{relationName}/lookup-one/{direction}",
      Func<HttpContext, 'tenantId, 'schemaName, string, string, bool, ValueDTO<ValueExtDTO>, IResult>
        (fun httpContext tenantId schemaName relationName direction draft payloadId ->

          let result =
            sum {
              let! dbio, languageContext, evalContext, typeCheckContext, typeCheckState =
                getDbDescriptor tenantId schemaName draft context

              let! idValue =
                valueFromDTO >> runDTOConverter languageContext <| payloadId
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              do!
                checkRelatedEntityId
                  relationName
                  direction
                  idValue
                  dbio
                  languageContext
                  typeCheckContext
                  typeCheckState

              let! lookupDescriptor =
                lookupDescriptorFromDb dbio relationName direction
                |> sum.MapError(
                  Errors.MapContext(replaceWith Location.Unknown)
                  >> APIError<'runtimeContext, 'db, 'customExtension, Location>.Create
                )

              let doLookupExpr: RunnableExpr<ValueExt<'runtimeContext, 'db, 'customExtension>> =
                RunnableExpr.UnsafeApplyForUntypedEval(
                  RunnableExpr.UnsafeApplyForUntypedEval(
                    RunnableExpr.UnsafeLookupForUntypedEval(
                      Identifier.FullyQualified([ "DB" ], $"lookupOne")
                      |> ResolvedIdentifier.FromIdentifier
                    ),
                    RunnableExpr.FromValue(
                      lookupDescriptor,
                      TypeValue.CreatePrimitive PrimitiveType.Unit,
                      Kind.Star
                    )
                  ),
                  RunnableExpr.FromValue(idValue, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                )

              let! evalResult =
                Expr.Eval(
                  NonEmptyList.prependList languageContext.TypeCheckedPreludes (NonEmptyList.OfList(doLookupExpr, []))
                )
                |> Reader.Run(evalContext |> context.PermissionHookInjector httpContext)
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              let! resultDTO =
                valueToDTO >> runDTOConverter languageContext <| evalResult
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              return resultDTO
            }

          apiResponseFromSum result id)
    )
    |> ignore

    app.MapPost(
      "/{tenantId}/{schemaName}/{relationName}/lookup-many/{direction}",
      Func<HttpContext, 'tenantId, 'schemaName, string, string, bool, int, int, ValueDTO<ValueExtDTO>, IResult>
        (fun httpContext tenantId schemaName relationName direction draft offset limit fromId ->

          let result =
            sum {

              let! dbio, languageContext, evalContext, typeCheckContext, typeCheckState =
                getDbDescriptor tenantId schemaName draft context

              let! idValue =
                valueFromDTO >> runDTOConverter languageContext <| fromId
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              do!
                checkRelatedEntityId
                  relationName
                  direction
                  idValue
                  dbio
                  languageContext
                  typeCheckContext
                  typeCheckState

              let! lookupDescriptor =
                lookupDescriptorFromDb dbio relationName direction
                |> sum.MapError(
                  Errors.MapContext(replaceWith Location.Unknown)
                  >> APIError<'runtimeContext, 'db, 'customExtension, Location>.Create
                )

              let doLookupExpr: RunnableExpr<ValueExt<'runtimeContext, 'db, 'customExtension>> =
                RunnableExpr.UnsafeApplyForUntypedEval(
                  RunnableExpr.UnsafeApplyForUntypedEval(
                    RunnableExpr.UnsafeApplyForUntypedEval(
                      RunnableExpr.UnsafeLookupForUntypedEval(
                        Identifier.FullyQualified([ "DB" ], $"lookupMany")
                        |> ResolvedIdentifier.FromIdentifier
                      ),
                      RunnableExpr.FromValue(
                        lookupDescriptor,
                        TypeValue.CreatePrimitive PrimitiveType.Unit,
                        Kind.Star
                      )
                    ),
                    RunnableExpr.FromValue(idValue, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                  ),
                  RunnableExpr.UnsafeTupleConsForUntypedEval
                    [ RunnableExpr.UnsafePrimitiveForUntypedEval(PrimitiveValue.Int32(offset))
                      RunnableExpr.UnsafePrimitiveForUntypedEval(PrimitiveValue.Int32(limit)) ]
                )

              let! evalResult =
                Expr.Eval(
                  NonEmptyList.prependList languageContext.TypeCheckedPreludes (NonEmptyList.OfList(doLookupExpr, []))
                )
                |> Reader.Run(evalContext |> context.PermissionHookInjector httpContext)
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              let! resultDTO =
                valueToDTO >> runDTOConverter languageContext <| evalResult
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              return resultDTO
            }

          apiResponseFromSum result id)
    )
    |> ignore

    app.MapPost(
      "/{tenantId}/{schemaName}/{relationName}/lookup-option/{direction}",
      Func<HttpContext, 'tenantId, 'schemaName, string, string, bool, ValueDTO<ValueExtDTO>, IResult>
        (fun httpContext tenantId schemaName relationName direction draft fromId ->

          let result =
            sum {
              let! dbio, languageContext, evalContext, typeCheckContext, typeCheckState =
                getDbDescriptor tenantId schemaName draft context

              let! idValue =
                valueFromDTO >> runDTOConverter languageContext <| fromId
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              do!
                checkRelatedEntityId
                  relationName
                  direction
                  idValue
                  dbio
                  languageContext
                  typeCheckContext
                  typeCheckState

              let! lookupDescriptor =
                lookupDescriptorFromDb dbio relationName direction
                |> sum.MapError(
                  Errors.MapContext(replaceWith Location.Unknown)
                  >> APIError<'runtimeContext, 'db, 'customExtension, Location>.Create
                )

              let doLookupExpr: RunnableExpr<ValueExt<'runtimeContext, 'db, 'customExtension>> =
                RunnableExpr.UnsafeApplyForUntypedEval(
                  RunnableExpr.UnsafeApplyForUntypedEval(
                    RunnableExpr.UnsafeLookupForUntypedEval(
                      Identifier.FullyQualified([ "DB" ], $"lookupOption")
                      |> ResolvedIdentifier.FromIdentifier
                    ),
                    RunnableExpr.FromValue(
                      lookupDescriptor,
                      TypeValue.CreatePrimitive PrimitiveType.Unit,
                      Kind.Star
                    )
                  ),
                  RunnableExpr.FromValue(idValue, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                )

              let! evalResult =
                Expr.Eval(
                  NonEmptyList.prependList languageContext.TypeCheckedPreludes (NonEmptyList.OfList(doLookupExpr, []))
                )
                |> Reader.Run(evalContext |> context.PermissionHookInjector httpContext)
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              let! resultDTO =
                valueToDTO >> runDTOConverter languageContext <| evalResult
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              return resultDTO
            }

          apiResponseFromSum result id)
    )
    |> ignore

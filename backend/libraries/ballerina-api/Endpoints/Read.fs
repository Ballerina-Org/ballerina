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

  type GetByIdPayload =
    { EntityName: string
      Id: ValueDTO<ValueExtDTO> }

  type GetManyResponseItem =
    { Key: ValueDTO<ValueExtDTO>
      Value: ValueDTO<ValueExtDTO> }

  type LookupPayload =
    { RelationName: string
      Direction: string
      FromId: ValueDTO<ValueExtDTO> }

  type LookupManyPayload =
    { Lookup: LookupPayload
      Offset: int
      Limit: int }

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

      match direction with
      | "From" ->
        let fromName = tableDescriptor.From.ToString()

        let! fromDescriptor =
          dbio.Schema.Entities
          |> OrderedMap.tryFind (fromName |> SchemaEntityName.Create)
          |> Sum.fromOption (fun () ->
            Errors.Singleton Location.Unknown (fun () -> $"Entity {fromName} not found in schema {dbio.Schema}."))
          |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

        let fromIdType = fromDescriptor.Id
        do! typeCheckValue idValue fromIdType languageContext typeCheckContext typeCheckState
      | "To" ->
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
    (context: APIContext<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName>)
    =

    app.MapPost(
      "/{tenantId}/{schemaName}/get-by-id",
      Func<HttpContext, 'tenantId, 'schemaName, bool, GetByIdPayload, IResult>
        (fun httpContext tenantId schemaName draft payload ->
          let entityName, idDTO = payload.EntityName, payload.Id

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



              let doLookupExpr
                : Expr<
                    TypeValue<ValueExt<'runtimeContext, 'db, 'customExtension>>,
                    ResolvedIdentifier,
                    ValueExt<'runtimeContext, 'db, 'customExtension>
                   > =
                Expr.Apply(
                  Expr.Apply(
                    Expr.Lookup(
                      Identifier.FullyQualified([ "DB" ], $"getById")
                      |> ResolvedIdentifier.FromIdentifier
                    ),
                    Expr.FromValue(entityDescriptor, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                  ),
                  Expr.FromValue(idValue, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
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
      "/{tenantId}/{schemaName}/many",
      Func<HttpContext, 'tenantId, 'schemaName, bool, string, int, int, IResult>
        (fun httpContext tenantId schemaName draft (entityName: string) (offset: int) (limit: int) ->
          let result =
            sum {
              let! dbio, languageContext, evalContext, typeCheckContext, _ =
                getDbDescriptor tenantId schemaName draft context

              let! entityDescriptor =
                entityDescriptorFromDb dbio entityName
                |> sum.MapError(
                  Errors.MapContext(replaceWith Location.Unknown)
                  >> APIError<'runtimeContext, 'db, 'customExtension, Location>.Create
                )

              let doLookupExpr
                : Expr<
                    TypeValue<ValueExt<'runtimeContext, 'db, 'customExtension>>,
                    ResolvedIdentifier,
                    ValueExt<'runtimeContext, 'db, 'customExtension>
                   > =
                Expr.Apply(
                  Expr.Apply(
                    Expr.Lookup(
                      Identifier.FullyQualified([ "DB" ], $"getMany")
                      |> ResolvedIdentifier.FromIdentifier
                    ),
                    Expr.FromValue(entityDescriptor, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                  ),
                  Expr.TupleCons
                    [ Expr.Primitive(PrimitiveValue.Int32 offset, Location.Unknown, typeCheckContext.Scope)
                      Expr.Primitive(PrimitiveValue.Int32 limit, Location.Unknown, typeCheckContext.Scope) ]
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
    (context: APIContext<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName>)
    =

    app.MapPost(
      "/{tenantId}/{schemaName}/lookup-one",
      Func<HttpContext, 'tenantId, 'schemaName, bool, LookupPayload, IResult>
        (fun httpContext tenantId schemaName draft payload ->

          let result =
            sum {
              let! dbio, languageContext, evalContext, typeCheckContext, typeCheckState =
                getDbDescriptor tenantId schemaName draft context

              let relationName, direction, payloadId =
                payload.RelationName, payload.Direction, payload.FromId

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

              let doLookupExpr
                : Expr<
                    TypeValue<ValueExt<'runtimeContext, 'db, 'customExtension>>,
                    ResolvedIdentifier,
                    ValueExt<'runtimeContext, 'db, 'customExtension>
                   > =
                Expr.Apply(
                  Expr.Apply(
                    Expr.Lookup(
                      Identifier.FullyQualified([ "DB" ], $"lookupOne")
                      |> ResolvedIdentifier.FromIdentifier
                    ),
                    Expr.FromValue(lookupDescriptor, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                  ),
                  Expr.FromValue(idValue, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
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

              return payloadId, resultDTO
            }

          apiResponseFromSum result (fun (id, result) -> {| Id = id; RelatedEntities = result |}))
    )
    |> ignore

    app.MapPost(
      "/{tenantId}/{schemaName}/lookup-many",
      Func<HttpContext, 'tenantId, 'schemaName, bool, LookupManyPayload, IResult>
        (fun httpContext tenantId schemaName draft payload ->

          let result =
            sum {
              let relationName, direction, fromId, offset, limit =
                payload.Lookup.RelationName,
                payload.Lookup.Direction,
                payload.Lookup.FromId,
                payload.Offset,
                payload.Limit

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

              let doLookupExpr
                : Expr<
                    TypeValue<ValueExt<'runtimeContext, 'db, 'customExtension>>,
                    ResolvedIdentifier,
                    ValueExt<'runtimeContext, 'db, 'customExtension>
                   > =
                Expr.Apply(
                  Expr.Apply(
                    Expr.Apply(
                      Expr.Lookup(
                        Identifier.FullyQualified([ "DB" ], $"lookupMany")
                        |> ResolvedIdentifier.FromIdentifier
                      ),
                      Expr.FromValue(lookupDescriptor, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                    ),
                    Expr.FromValue(idValue, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                  ),
                  Expr.TupleCons
                    [ Expr.Primitive(PrimitiveValue.Int32(offset))
                      Expr.Primitive(PrimitiveValue.Int32(limit)) ]
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

              return fromId, resultDTO
            }

          apiResponseFromSum result (fun (id, result) -> {| Id = id; RelatedEntities = result |}))
    )
    |> ignore

    app.MapPost(
      "/{tenantId}/{schemaName}/lookup-option",
      Func<HttpContext, 'tenantId, 'schemaName, bool, LookupPayload, IResult>
        (fun httpContext tenantId schemaName draft payload ->

          let result =
            sum {
              let relationName, direction, fromId =
                payload.RelationName, payload.Direction, payload.FromId

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

              let doLookupExpr
                : Expr<
                    TypeValue<ValueExt<'runtimeContext, 'db, 'customExtension>>,
                    ResolvedIdentifier,
                    ValueExt<'runtimeContext, 'db, 'customExtension>
                   > =
                Expr.Apply(
                  Expr.Apply(
                    Expr.Lookup(
                      Identifier.FullyQualified([ "DB" ], $"lookupOption")
                      |> ResolvedIdentifier.FromIdentifier
                    ),
                    Expr.FromValue(lookupDescriptor, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                  ),
                  Expr.FromValue(idValue, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
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

              return fromId, resultDTO
            }

          apiResponseFromSum result (fun (id, result) -> {| Id = id; RelatedEntities = result |}))
    )
    |> ignore

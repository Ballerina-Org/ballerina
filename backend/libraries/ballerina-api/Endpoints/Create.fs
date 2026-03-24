namespace Ballerina.API

module Create =
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
  open Ballerina.DSL.Next.Types.TypeChecker.Value
  open Ballerina.DSL.Next.StdLib.DB

  type CreatePayload =
    { EntityName: string
      Id: ValueDTO<ValueExtDTO>
      Entity: ValueDTO<ValueExtDTO> }



  let create<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName
    when 'customExtension: comparison and 'db: comparison>
    (app: IEndpointRouteBuilder)
    (context: APIContext<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName>)
    =
    app.MapPost(
      "/{tenantId}/{schemaName}/create",
      Func<'tenantId, 'schemaName, bool, CreatePayload, IResult>(fun tenantId schemaName draft payload ->
        let entityName = payload.EntityName
        let entityId = payload.Id
        let entity = payload.Entity


        let result =
          sum {
            let! dbio, languageContext, evalContext, typeCheckContext, typeCheckState =
              getDbDescriptor tenantId schemaName draft context

            let! _tableDescriptor =
              dbio.Schema.Entities
              |> OrderedMap.tryFind (entityName |> SchemaEntityName.Create)
              |> Sum.fromOption (fun () ->
                Errors.Singleton Location.Unknown (fun () ->
                  $"Entity {entityName} not found in schema {dbio.Schema}."))
              |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

            let! schema =
              dbio.SchemaAsValue
              |> Value.AsRecord
              |> toUknonwLocation
              |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

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
              |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))
              |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

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

            let! entityValue =
              runDTOConverter languageContext (valueFromDTO entity)
              |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

            let idType, entityType = _tableDescriptor.Id, _tableDescriptor.TypeOriginal

            do! typeCheckValue idValue idType languageContext typeCheckContext typeCheckState

            do! typeCheckValue entityValue entityType languageContext typeCheckContext typeCheckState

            let doUpdateExpr
              : Expr<
                  TypeValue<ValueExt<'runtimeContext, 'db, 'customExtension>>,
                  ResolvedIdentifier,
                  ValueExt<'runtimeContext, 'db, 'customExtension>
                 > =
              Expr.Apply(
                Expr.Apply(
                  Expr.Lookup(
                    Identifier.FullyQualified([ "DB" ], "create")
                    |> ResolvedIdentifier.FromIdentifier
                  ),
                  Expr.FromValue(entityDescriptor, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                ),
                Expr.TupleCons
                  [ Expr.FromValue(idValue, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                    Expr.FromValue(entityValue, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star) ]
              )

            let! evalResult =
              Expr.Eval(
                NonEmptyList.prependList languageContext.TypeCheckedPreludes (NonEmptyList.OfList(doUpdateExpr, []))
              )
              |> Reader.Run evalContext
              |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

            let! result =
              runDTOConverter languageContext (valueToDTO evalResult)
              |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

            return result
          }

        apiResponseFromSum result id)
    )
    |> ignore

    app.MapPost(
      "/{tenantId}/{schemaName}/create-many",
      Func<'tenantId, 'schemaName, bool, CreatePayload[], IResult>(fun tenantId schemaName draft payloads ->
        let createMany
          (dbio: DBIO<'runtimeContext, 'db, ValueExt<'runtimeContext, 'db, 'customExtension>>)
          languageContext
          evalContext
          typeCheckContext
          typeCheckState
          =
          payloads
          |> Array.map (fun payload ->
            let entityName = payload.EntityName
            let entityId = payload.Id
            let entity = payload.Entity

            sum {
              let! _tableDescriptor =
                dbio.Schema.Entities
                |> OrderedMap.tryFind (entityName |> SchemaEntityName.Create)
                |> Sum.fromOption (fun () ->
                  Errors.Singleton Location.Unknown (fun () ->
                    $"Entity {entityName} not found in schema {dbio.Schema}."))
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              let! schema =
                dbio.SchemaAsValue
                |> Value.AsRecord
                |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

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

              let! entityValue =
                runDTOConverter languageContext (valueFromDTO entity)
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              let idType, entityType = _tableDescriptor.Id, _tableDescriptor.TypeOriginal

              do! typeCheckValue idValue idType languageContext typeCheckContext typeCheckState
              do! typeCheckValue entityValue entityType languageContext typeCheckContext typeCheckState

              let doUpdateExpr
                : Expr<
                    TypeValue<ValueExt<'runtimeContext, 'db, 'customExtension>>,
                    ResolvedIdentifier,
                    ValueExt<'runtimeContext, 'db, 'customExtension>
                   > =
                Expr.Apply(
                  Expr.Apply(
                    Expr.Lookup(
                      Identifier.FullyQualified([ "DB" ], "create")
                      |> ResolvedIdentifier.FromIdentifier
                    ),
                    Expr.FromValue(entityDescriptor, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                  ),
                  Expr.TupleCons
                    [ Expr.FromValue(idValue, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                      Expr.FromValue(entityValue, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star) ]
                )

              let! evalResult =
                Expr.Eval(
                  NonEmptyList.prependList languageContext.TypeCheckedPreludes (NonEmptyList.OfList(doUpdateExpr, []))
                )
                |> Reader.Run evalContext
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

              return!
                runDTOConverter languageContext (valueToDTO evalResult)
                |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

            })
          |> Sum.All
          |> Sum.map List.toArray

        let results =
          sum {
            let! dbio, languageContext, evalContext, typeCheckContext, typeCheckState =
              getDbDescriptor tenantId schemaName draft context

            return! createMany dbio languageContext evalContext typeCheckContext typeCheckState
          }

        apiResponseFromSum results id)
    )
    |> ignore

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

  type UpdateDeltaWithId =
    { Id: ValueDTO<ValueExtDTO>
      Delta: DeltaDTO<ValueExtDTO, DeltaExtDTO> }

  type UpdatePayload =
    { EntityName: string
      Updater: UpdateDeltaWithId }

  type UpdateManyPayload =
    { EntityName: string
      Updaters: UpdateDeltaWithId[] }

  let update<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName
    when 'customExtension: comparison and 'db: comparison>
    (app: IEndpointRouteBuilder)
    (context: APIContext<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName>)
    =
    app.MapPost(
      "/{tenantId}/{schemaName}/update",
      Func<'tenantId, 'schemaName, bool, UpdatePayload, IResult>(fun tenantId schemaName draft payload ->
        let entityName = payload.EntityName
        let entityId = payload.Updater.Id
        let delta = payload.Updater.Delta

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

            let idType = _tableDescriptor.Id
            do! typeCheckValue idValue idType languageContext typeCheckContext typeCheckState

            let! delta =
              deltaFromDTO delta
              |> Reader.Run context.LanguageContext.SerializationContext
              |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))
              |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

            let! updaterLambda =
              createUpdaterFromDelta delta
              |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))
              |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

            let doUpdateExpr
              : Expr<
                  TypeValue<ValueExt<'runtimeContext, 'db, 'customExtension>>,
                  ResolvedIdentifier,
                  ValueExt<'runtimeContext, 'db, 'customExtension>
                 > =

              Expr.Apply(
                Expr.Apply(
                  Expr.Lookup(
                    Identifier.FullyQualified([ "DB" ], "update")
                    |> ResolvedIdentifier.FromIdentifier
                  ),
                  Expr.FromValue(entityDescriptor, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                ),
                Expr.TupleCons
                  [ Expr.FromValue(idValue, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                    Expr.Lambda(Var.Create "_", None, updaterLambda, None) ]
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
      "/{tenantId}/{schemaName}/update-many",
      Func<'tenantId, 'schemaName, bool, UpdateManyPayload, IResult>(fun tenantId schemaName draft payload ->
        let entityName = payload.EntityName

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

            let! updaters =
              payload.Updaters
              |> Array.map (fun updater ->
                reader {
                  let! delta = deltaFromDTO updater.Delta

                  let! idValue =
                    valueFromDTO updater.Id
                    |> reader.MapContext(fun deltaSerializationContext ->
                      deltaSerializationContext.SerializationContext)

                  let! updaterLambda = createUpdaterFromDelta delta |> reader.OfSum

                  return idValue, Value.Lambda(Var.Create "_", updaterLambda, Map.empty, TypeCheckScope.Empty)
                })
              |> reader.All
              |> Reader.Run languageContext.SerializationContext
              |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))
              |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

            let idType = _tableDescriptor.Id

            do!
              updaters
              |> List.map (fun (idValue, _) ->
                typeCheckValue idValue idType languageContext typeCheckContext typeCheckState)
              |> Sum.All
              |> Sum.map (fun _ -> ())

            let updaters =
              updaters
              |> Map.ofList
              |> Ballerina.DSL.Next.StdLib.Map.Model.MapValues.Map
              |> MapExt.MapValues
              |> Choice6Of7
              |> ValueExt

            let updaters = Value.Ext(updaters, None)

            let doUpdateExpr
              : Expr<
                  TypeValue<ValueExt<'runtimeContext, 'db, 'customExtension>>,
                  ResolvedIdentifier,
                  ValueExt<'runtimeContext, 'db, 'customExtension>
                 > =
              Expr.Apply(
                Expr.Apply(
                  Expr.Lookup(
                    Identifier.FullyQualified([ "DB" ], "updateMany")
                    |> ResolvedIdentifier.FromIdentifier
                  ),
                  Expr.FromValue(entityDescriptor, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                ),
                Expr.FromValue(updaters, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
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

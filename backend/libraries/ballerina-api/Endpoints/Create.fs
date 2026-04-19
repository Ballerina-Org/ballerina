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
  open Ballerina.DSL.Next.Terms.FastEval
  open Ballerina.DSL.Next.Terms.Eval
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Next.Extensions
  open Microsoft.AspNetCore.Http
  open Ballerina.DSL.Next.Serialization.ValueSerializer
  open Ballerina.DSL.Next.Types.TypeChecker.Value
  open Ballerina.DSL.Next.StdLib.DB

  type CreatePayload =
    { Id: ValueDTO<ValueExtDTO>
      Entity: ValueDTO<ValueExtDTO> }



  let create<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName
    when 'customExtension: comparison and 'db: comparison>
    (app: IEndpointRouteBuilder)
    (context:
      APIRegistrationFactory<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName>)
    =
    app.MapPost(
      "/{tenantId}/{schemaName}/{entityName}/create",
      Func<
        HttpContext,
        'tenantId,
        'schemaName,
        string,
        CreatePayload,
        IResult
       >
        (fun httpContext tenantId schemaName entityName payload ->
          let entityId = payload.Id
          let entity = payload.Entity


          let result =
            sum {
              let! dbio,
                   languageContext,
                   evalContext,
                   typeCheckContext,
                   typeCheckState =
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

              let! entityValue =
                runDTOConverter languageContext (valueFromDTO entity)
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

              let doUpdateExpr
                : RunnableExpr<
                    ValueExt<'runtimeContext, 'db, 'customExtension>
                   > =
                RunnableExpr.UnsafeApplyForUntypedEval(
                  RunnableExpr.UnsafeApplyForUntypedEval(
                    RunnableExpr.UnsafeLookupForUntypedEval(
                      Identifier.FullyQualified([ "DB" ], "create")
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
                      ) ]
                )

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

              let! result =
                runDTOConverter languageContext (valueToDTO evalResult)
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              return result
            }

          apiResponseFromSum result id)
    )
    |> ignore

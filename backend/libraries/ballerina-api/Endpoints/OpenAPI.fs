namespace Ballerina.API

module OpenAPI =
  open Microsoft.AspNetCore.Builder
  open Microsoft.AspNetCore.Routing
  open System
  open Ballerina.Collections.Sum
  open APIUtils
  open Microsoft.AspNetCore.Http
  open Ballerina.DSL.Next.OpenAPI
  open Ballerina.State.WithError
  open Ballerina.Errors
  open Ballerina.LocalizedErrors
  open Ballerina
  open System.IO
  open System.Text

  let openApi<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName
    when 'customExtension: comparison and 'db: comparison>
    (app: IEndpointRouteBuilder)
    (context:
      APIContext<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName>)
    =

    app.MapGet(
      "/{tenantId}/{schemaName}/openapi",
      Func<'tenantId, 'schemaName, bool, IResult>
        (fun tenantId schemaName draft ->
          let result =
            sum {
              let! dbio, _, _, typeCheckContext, typeCheckState =
                getDbDescriptor tenantId schemaName draft context

              let! _, dataModel =
                generate_data_models dbio.Schema
                |> State.Run((typeCheckContext, typeCheckState), Map.empty)
                |> sum.MapError(fun (errors, _) ->
                  errors |> Errors.MapContext(replaceWith Location.Unknown))
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let! dataModels =
                dataModel
                |> sum.OfOption(
                  Errors.Singleton Location.Unknown (fun _ ->
                    "Failed to generate OpenAPI data models: no state produced")
                )
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let! _, endpoints =
                generate_endpoints dbio.Schema
                |> State.Run(
                  ((typeCheckContext, typeCheckState), dataModels),
                  []
                )
                |> sum.MapError(fun (errors, _) ->
                  errors |> Errors.MapContext(replaceWith Location.Unknown))
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let! endpoints =
                endpoints
                |> sum.OfOption(
                  Errors.Singleton Location.Unknown (fun _ ->
                    "Failed to generate OpenAPI data models: no state produced")
                )
                |> sum.MapError
                  APIError<'runtimeContext, 'db, 'customExtension, Location>
                    .Create

              let openApiSpec =
                { Title = $"DB API {tenantId}-{schemaName}"
                  Version = "1.0.0"
                  Endpoints = endpoints
                  DataModels = dataModels }

              return to_yaml openApiSpec

            }

          match result with
          | Left openapi ->
            let bytes = Encoding.UTF8.GetBytes openapi

            Results.File(
              fileContents = bytes,
              contentType = "application/yaml",
              fileDownloadName = "openapi.yaml"
            )
          | Right { Errors = errors; TypeError = _ } ->
            let serializedErrors = errorsToSerializable errors

            Results.BadRequest
              { Errors = serializedErrors
                Examples = [||] }

        )
    )
    |> ignore

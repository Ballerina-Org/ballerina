namespace Ballerina.API

module OpenAPI =
  open Microsoft.AspNetCore.Builder
  open Microsoft.AspNetCore.Routing
  open System
  open Ballerina.Collections.Sum
  open APIUtils
  open Microsoft.AspNetCore.Http
  open Ballerina.State.WithError
  open Ballerina.Errors
  open Ballerina.LocalizedErrors
  open Ballerina
  open System.Text
  open Ballerina.DSL.Next.OpenAPIGeneration

  let openApi<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName
    when 'customExtension: comparison and 'db: comparison>
    (app: IEndpointRouteBuilder)
    (context: APIRegistrationFactory<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName>)
    =

    app.MapGet(
      "/{tenantId}/{schemaName}/openapi",
      Func<'tenantId, 'schemaName, bool, IResult>(fun tenantId schemaName draft ->
        let result =
          sum {
            let! dbio, _, _, typeCheckContext, typeCheckState = getDbDescriptor tenantId schemaName draft context

            let generationState: OpenAPIGenerationState =
              { DataModel = Map.empty
                Endpoints = [] }

            let generationContext: OpenApiGenerationContext<'runtimeContext, 'db, 'customExtension> =
              { TypeCheckContext = typeCheckContext
                TypeCheckState = typeCheckState }

            return!
              generateOpenAPI
                dbio.Schema
                (tenantId.ToString())
                (schemaName.ToString())
                $"DB API {tenantId}-{schemaName}"
                "1.0.0"
              |> State.Run(generationContext, generationState)
              |> sum.MapError(fun (errors, _) -> errors |> Errors.MapContext(replaceWith Location.Unknown))
              |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create
              |> sum.Map fst

          }

        match result with
        | Left openapi ->
          let bytes = Encoding.UTF8.GetBytes openapi

          Results.File(fileContents = bytes, contentType = "application/yaml", fileDownloadName = "openapi.yaml")
        | Right { Errors = errors; TypeError = _ } ->
          let serializedErrors = errorsToSerializable errors

          Results.BadRequest
            { Errors = serializedErrors
              Examples = [||] }

      )
    )
    |> ignore

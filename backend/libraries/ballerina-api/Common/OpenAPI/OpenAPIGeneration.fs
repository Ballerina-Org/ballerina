namespace Ballerina.DSL.Next

module OpenAPIGeneration =
  open Ballerina.State.WithError
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.StdLib.Extensions
  open Ballerina.DSL.Next.Types.TypeChecker
  open OpenAPIModel
  open Ballerina.Errors
  open DataModelGeneration
  open EndpointGeneration
  open YamlGeneration

  type OpenAPIGenerationState = {
    DataModel : Map<OpenAPIDataModelName, OpenAPIDataModel>
    Endpoints : List<OpenAPIEndpoint>
  }

  type OpenApiGenerationContext<'runtimeContext, 'db, 'customExtension when 'db : comparison and 'customExtension : comparison> = {
    TypeCheckContext : TypeCheckContext<ValueExt<'runtimeContext, 'db, 'customExtension>>
    TypeCheckState : TypeCheckState<ValueExt<'runtimeContext, 'db, 'customExtension>>

  }
  let generateOpenAPI 
    (schema: Schema<ValueExt<'runtimeContext, 'db, 'customExtension>>) 
    (tenantId : string) 
    (schemaName : string) 
    (openAPITitle : string)
    (opeanAPIVersion : string): State<
        string,
        OpenApiGenerationContext<'runtimeContext, 'db, 'customExtension>,
        OpenAPIGenerationState,
        Errors<Unit>
       > =
    state {
      do! 
        generate_data_models schema
        |> State.mapContext (fun openApiGenerationContext -> openApiGenerationContext.TypeCheckContext, openApiGenerationContext.TypeCheckState)
        |> State.mapState
              (fun (generationState : OpenAPIGenerationState, _) -> generationState.DataModel)
              (fun (dataModel, _) (generationState : OpenAPIGenerationState) -> { generationState with DataModel = dataModel })

      let! dataModels =
        state.GetState()
        |> state.Map(fun generationState -> generationState.DataModel)

      do!
        generate_endpoints tenantId schemaName schema
        |> State.mapContext (fun (openApiGenerationContext : OpenApiGenerationContext<'runtimeContext,'db,'customExtension>) -> 
              (openApiGenerationContext.TypeCheckContext, openApiGenerationContext.TypeCheckState), dataModels)
        |> State.mapState
              (fun (generationState : OpenAPIGenerationState, _) -> generationState.Endpoints)
              (fun (endpoints, _) (generationState : OpenAPIGenerationState) -> {generationState with Endpoints = endpoints})

      let! generationState = state.GetState()

      let openApiSpec =
        { Title = openAPITitle
          Version = opeanAPIVersion
          Endpoints = generationState.Endpoints
          DataModels = dataModels }

      return to_yaml openApiSpec       
    }
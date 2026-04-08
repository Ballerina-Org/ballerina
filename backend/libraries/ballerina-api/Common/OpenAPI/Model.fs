namespace Ballerina.DSL.Next

module OpenAPIModel =
  open Ballerina.DSL.Next.Types
  type OpenAPISpec =
    { Title: string
      Version: string
      Endpoints: List<OpenAPIEndpoint>
      DataModels: Map<OpenAPIDataModelName, OpenAPIDataModel> }

  and QueryParameter = {
    Name : ResolvedIdentifier
    Type : PrimitiveType
  }

  and OpenAPIEndpoint =
    { Path: string
      Method: OpenAPIEndpointModel
      QueryParameters: List<QueryParameter>
      RequestModel: Option<OpenAPIDataModel>
      ResponseModel: Option<OpenAPIDataModel> }

  and OpenAPIDataModelName = { OpenAPIDataModelName: string }

  and OpenAPIEndpointModel =
    | Get
    | Post
    | Put
    | Delete
    | Patch

  and OpenAPIDataModel =
    | Ref of OpenAPIDataModelName
    | Record of List<ResolvedIdentifier * OpenAPIDataModel>
    | Primitive of PrimitiveType
    | Scalar of PrimitiveType
    | AnyObject
    | Union of List<ResolvedIdentifier * OpenAPIDataModel>
    | OneOf of List<ResolvedIdentifier * OpenAPIDataModel>
    | Sum of List<OpenAPIDataModel>
    | Tuple of List<OpenAPIDataModel>
    | List of OpenAPIDataModel
    | Array of OpenAPIDataModel
    | PositionalElement of OpenAPIDataModel
    | Object of List<ResolvedIdentifier * OpenAPIDataModel>

  let internal listToOpenApi (arg_t : OpenAPIDataModel) =
    OpenAPIDataModel.Object [
      "ext" |> ResolvedIdentifier.Create, OpenAPIDataModel.List arg_t
    ] 

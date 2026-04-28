namespace Ballerina.StdLib.Tests.OpenApi

open NUnit.Framework
open Ballerina.DSL.Next
open Ballerina.DSL.Next.OpenAPIModel
open Ballerina.DSL.Next.Types

module YamlGenerationTests =

  [<Test>]
  let ``Generated OpenAPI includes JSON schema for bad requests`` () =
    let spec =
      { Title = "Test API"
        Version = "1.0.0"
        Endpoints =
          [ { Path = "/tenant/schema/Example/create"
              Method = OpenAPIEndpointModel.Post
              QueryParameters = []
              RequestModel = None
              ResponseModel =
                Some(
                  OpenAPIDataModel.Object
                    [ "Primitive" |> ResolvedIdentifier.Create,
                      OpenAPIDataModel.Primitive PrimitiveType.Bool ]
                ) } ]
        DataModels = Map.empty }

    let yaml = YamlGeneration.to_yaml spec

    Assert.That(yaml, Does.Contain("'400':"))
    Assert.That(yaml, Does.Contain("application/json:"))
    Assert.That(yaml, Does.Contain("$ref: '#/components/schemas/ApiErrorResponse'"))
    Assert.That(yaml, Does.Contain("'ApiErrorResponse':"))
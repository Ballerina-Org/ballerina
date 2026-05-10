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

  [<Test>]
  let ``Generated OpenAPI supports filter array responses with key value entries`` () =
    let localesWithProps =
      { OpenAPIDataModelName.OpenAPIDataModelName = "Locales-WithProps" }

    let spec =
      { Title = "Test API"
        Version = "1.0.0"
        Endpoints =
          [ { Path = "/tenant/schema/Locales/filter"
              Method = OpenAPIEndpointModel.Post
              QueryParameters =
                [ { Name = "offset" |> ResolvedIdentifier.Create
                    Type = PrimitiveType.Int32 }
                  { Name = "limit" |> ResolvedIdentifier.Create
                    Type = PrimitiveType.Int32 } ]
              RequestModel = None
              ResponseModel =
                Some(
                  OpenAPIDataModel.Array(
                    OpenAPIDataModel.Object
                      [ ("Key" |> ResolvedIdentifier.Create,
                         OpenAPIDataModel.Scalar PrimitiveType.Guid)
                        ("Value" |> ResolvedIdentifier.Create,
                         OpenAPIDataModel.Ref localesWithProps) ]
                  )
                ) } ]
        DataModels =
          Map.ofList
            [ localesWithProps,
              OpenAPIDataModel.Object
                [ ("Record" |> ResolvedIdentifier.Create,
                   OpenAPIDataModel.Object
                     [ ("DisplayLanguageCode" |> ResolvedIdentifier.Create,
                        OpenAPIDataModel.Object
                          [ ("Primitive" |> ResolvedIdentifier.Create,
                             OpenAPIDataModel.Primitive PrimitiveType.String) ]) ]) ] ] }

    let yaml = YamlGeneration.to_yaml spec

    Assert.That(yaml, Does.Contain("'/tenant/schema/Locales/filter':"))
    Assert.That(yaml, Does.Contain("type: array"))
    Assert.That(yaml, Does.Contain("'Key':"))
    Assert.That(yaml, Does.Contain("format: uuid"))
    Assert.That(yaml, Does.Contain("'Value':"))
    Assert.That(yaml, Does.Contain("$ref: '#/components/schemas/Locales-WithProps'"))

  [<Test>]
  let ``Generated OpenAPI supports get by id item envelopes`` () =
    let localeId =
      { OpenAPIDataModelName.OpenAPIDataModelName = "Locales-Id" }

    let localesWithProps =
      { OpenAPIDataModelName.OpenAPIDataModelName = "Locales-WithProps" }

    let spec =
      { Title = "Test API"
        Version = "1.0.0"
        Endpoints =
          [ { Path = "/tenant/schema/Locales/get-by-id"
              Method = OpenAPIEndpointModel.Post
              QueryParameters = []
              RequestModel = Some(OpenAPIDataModel.Ref localeId)
              ResponseModel =
                Some(
                  OpenAPIDataModel.Object
                    [ ("Item1" |> ResolvedIdentifier.Create,
                       OpenAPIDataModel.Ref localeId)
                      ("Item2" |> ResolvedIdentifier.Create,
                       OpenAPIDataModel.Sum
                         [ OpenAPIDataModel.Primitive PrimitiveType.Unit
                           OpenAPIDataModel.Ref localesWithProps ]) ]
                ) } ]
        DataModels =
          Map.ofList
            [ localeId,
              OpenAPIDataModel.Object
                [ ("Record" |> ResolvedIdentifier.Create,
                   OpenAPIDataModel.Object
                     [ ("LocaleId::LocaleId" |> ResolvedIdentifier.Create,
                        OpenAPIDataModel.Object
                          [ ("Primitive" |> ResolvedIdentifier.Create,
                             OpenAPIDataModel.Primitive PrimitiveType.Guid) ]) ]) ]
              localesWithProps,
              OpenAPIDataModel.Object
                [ ("Record" |> ResolvedIdentifier.Create,
                   OpenAPIDataModel.Object
                     [ ("DisplayLanguageCode" |> ResolvedIdentifier.Create,
                        OpenAPIDataModel.Object
                          [ ("Primitive" |> ResolvedIdentifier.Create,
                             OpenAPIDataModel.Primitive PrimitiveType.String) ]) ]) ] ] }

    let yaml = YamlGeneration.to_yaml spec

    Assert.That(yaml, Does.Contain("'/tenant/schema/Locales/get-by-id':"))
    Assert.That(yaml, Does.Contain("'Item1':"))
    Assert.That(yaml, Does.Contain("'Item2':"))
    Assert.That(yaml, Does.Contain("$ref: '#/components/schemas/Locales-Id'"))
    Assert.That(yaml, Does.Contain("$ref: '#/components/schemas/Locales-WithProps'"))
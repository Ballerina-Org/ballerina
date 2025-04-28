namespace Ballerina.AI.TGI

[<RequireQualifiedAccess>]
module TGIIntegration =

  open System.Net.Http
  open Ballerina.Collections.Sum
  open System.Threading.Tasks
  open SwaggerProvider
  open NJsonSchema
  open Ballerina.Errors

  module LLM = Ballerina.AI.LLM.LLM
  module JSONSchemaIntegration = Ballerina.AI.LLM.JSONSchemaIntegration

  type TgiApiSchema = OpenApiClientProvider<"tgi/tgi-spec.json">

  let newTGIHttpClient (httpClient: HttpClient) = TgiApiSchema.Client httpClient


  let llmIntegration (client: TgiApiSchema.Client) : LLM.LLMIntegration<JsonSchema> =
    LLM.LLMIntegration(fun (prompt: LLM.Prompt) (schema: JsonSchema) ->
      let (LLM.TaskExplanation textExplanation) = prompt.TaskExplanation
      let (LLM.TextContext context) = prompt.Context

      let (LLM.OutputStructureDescriptionForPrompt outputStructureDescription) =
        prompt.OutputStructureDescriptionForPrompt

      let imagePrefix =
        match prompt.Image with
        | Some(LLM.Base64PNGImage image) -> $"data:image/png;base64,{image}"
        | None -> ""

      let request =
        TgiApiSchema.GenerateRequest(
          Inputs =
            imagePrefix
            + $"{textExplanation}\n\nContext:\n{context}\n\n{outputStructureDescription}\n",
          Parameters =
            TgiApiSchema.GenerateParameters(
              Grammar = TgiApiSchema.GenerateParameters_Grammar(Type = "json", Value = schema.ToJson())
            )
        )

      let responseTask = request |> client.Generate

      try
        let response = responseTask |> Async.AwaitTask |> Async.RunSynchronously
        LLM.LLMOutput response.GeneratedText |> Left
      with
      | :? HttpRequestException as ex -> sum.Throw(Errors.Singleton $"HTTP error: {ex.Message}")
      | :? TaskCanceledException ->
        sum.Throw(Errors.Singleton "Request timed out: The operation took too long to complete.")
      | ex -> sum.Throw(Errors.Singleton $"Unexpected error: {ex.GetType().Name}: {ex.Message}"))

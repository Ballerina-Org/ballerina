namespace Ballerina.DSL.Next.StdLib.AIConfigurator

[<AutoOpen>]
module Extension =
  open System
  open System.Net.Http
  open System.Text
  open System.Text.Json
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.Lenses
  open Ballerina.DSL.Next.Extensions

  [<NoComparison; NoEquality>]
  type AIConfiguratorTypeClass<'runtimeContext> =
    { briefToPlan: 'runtimeContext -> string -> string }

    static member FromEnvironment() : AIConfiguratorTypeClass<'runtimeContext> =
      { briefToPlan =
          fun _ prompt ->
            try
              let baseUrl =
                Environment.GetEnvironmentVariable("BISE_LLM_URL")
                |> Option.ofObj
                |> Option.filter (fun x -> not (String.IsNullOrWhiteSpace(x)))
                |> Option.defaultValue "http://localhost:8080"
                |> fun x -> x.TrimEnd('/')

              let model =
                Environment.GetEnvironmentVariable("LLM_MODEL")
                |> Option.ofObj
                |> Option.filter (fun x -> not (String.IsNullOrWhiteSpace(x)))
                |> Option.defaultValue "qwen3:8b"

              use http = new HttpClient(Timeout = TimeSpan.FromMinutes(2.0))

              let payload =
                JsonSerializer.Serialize(
                  {| model = model
                     messages = [| {| role = "user"; content = prompt |} |]
                     temperature = 0.1 |}
                )

              use request =
                new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v1/chat/completions")

              request.Content <- new StringContent(payload, Encoding.UTF8, "application/json")

              use response = http.Send(request)
              let body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult()

              if not response.IsSuccessStatusCode then
                $"[AIConfigurator::briefToPlan error] status={(int response.StatusCode)} body={body}"
              else
                use doc = JsonDocument.Parse(body)
                let root = doc.RootElement

                let tryGetContent () =
                  match root.TryGetProperty("choices") with
                  | true, choices when choices.ValueKind = JsonValueKind.Array && choices.GetArrayLength() > 0 ->
                    let first = choices[0]

                    match first.TryGetProperty("message") with
                    | true, message ->
                      match message.TryGetProperty("content") with
                      | true, content -> content.GetString() |> Option.ofObj
                      | _ -> None
                    | _ -> None
                  | _ -> None

                tryGetContent ()
                |> Option.defaultValue "[AIConfigurator::briefToPlan error] missing choices[0].message.content"
            with ex ->
              $"[AIConfigurator::briefToPlan exception] {ex.Message}" }

  let AIConfiguratorExtension<'runtimeContext, 'ext>
    (ai_ops: AIConfiguratorTypeClass<'runtimeContext>)
    (operationLens: PartialLens<'ext, AIConfiguratorOperations<'ext>>)
    : OperationsExtension<'runtimeContext, 'ext, AIConfiguratorOperations<'ext>> =

    let stringTypeValue = TypeValue.CreateString()

    let briefToPlanId =
      Identifier.FullyQualified([ "AIConfigurator" ], "briefToPlan")
      |> TypeCheckScope.Empty.Resolve

    let briefToPlanOperation
      : ResolvedIdentifier *
        OperationExtension<'runtimeContext, 'ext, AIConfiguratorOperations<'ext>> =
      briefToPlanId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(stringTypeValue, stringTypeValue),
              Kind.Star,
              AIConfiguratorOperations.BriefToPlan)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | AIConfiguratorOperations.BriefToPlan -> Some AIConfiguratorOperations.BriefToPlan)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              do!
                op
                |> AIConfiguratorOperations.AsBriefToPlan
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! v =
                v
                |> Value.AsPrimitive
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! prompt =
                v
                |> PrimitiveValue.AsString
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! ctx = reader.GetContext()
              let response = ai_ops.briefToPlan ctx.RuntimeContext prompt

              return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.String(response))
            } }

    { TypeVars = []
      Operations = [ briefToPlanOperation ] |> Map.ofList }

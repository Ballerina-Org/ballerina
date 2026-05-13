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
  open Ballerina.Parser
  open Ballerina.DSL.Next.Syntax

  [<CLIMutable>]
  type CompletionResponse =
    { content: string
      stop: bool
      tokens_predicted: int
      tokens_evaluated: int }

  [<NoComparison; NoEquality>]
  type AIConfiguratorTypeClass<'runtimeContext> =
    { briefToPlan: 'runtimeContext -> string -> string
      cmsStage: 'runtimeContext -> string -> string
      productsStage: 'runtimeContext -> string -> string }

    static member FromEnvironment() : AIConfiguratorTypeClass<'runtimeContext> =
      let invalidSentinel opName = $"[AIConfigurator::{opName} invalid]"

      let briefPlanGbnf =
        """root ::= ws brief_plan ws

      brief_plan ::= "{" ws cms_field ws ";" ws products_field ws ";" ws media_field ws ";" ws theme_field ws "}"

      cms_field ::= "BriefPlan::Cms" ws "=" ws "2Of2(" ws cms_record ws ")"
      cms_record ::= "{" ws "PlanCmsInput::Pages" ws "=" ws "2Of2(" ws string_list ws ")" ws ";" ws "PlanCmsInput::HomepageSections" ws "=" ws "2Of2(" ws string_list ws ")" ws "}"

      products_field ::= "BriefPlan::Products" ws "=" ws "2Of2(" ws products_record ws ")"
      products_record ::= "{" ws "PlanProductsInput::Categories" ws "=" ws "2Of2(" ws string_list ws ")" ws ";" ws "PlanProductsInput::ProductIdeas" ws "=" ws "2Of2(" ws string_list ws ")" ws ";" ws "PlanProductsInput::PriceBandHint" ws "=" ws "2Of2(" ws label ws ")" ws "}"

      media_field ::= "BriefPlan::Media" ws "=" ws "2Of2(" ws media_record ws ")"
      media_record ::= "{" ws "PlanMediaInput::FeaturedAssets" ws "=" ws "1Of2()" ws ";" ws "PlanMediaInput::GalleryHints" ws "=" ws "2Of2(" ws string_list ws ")" ws "}"

      theme_field ::= "BriefPlan::Theme" ws "=" ws "2Of2(" ws theme_record ws ")"
      theme_record ::= "{" ws "PlanThemeInput::StyleKeywords" ws "=" ws "2Of2(" ws string_list ws ")" ws ";" ws "PlanThemeInput::LayoutHints" ws "=" ws "2Of2(" ws string_list ws ")" ws "}"

      string_list ::= "{" ws label (ws ";" ws label)? (ws ";" ws label)? (ws ";" ws label)? (ws ";" ws label)? (ws ";" ws label)? (ws ";" ws label)? (ws ";" ws label)? ws "}"

label ::= "\"" jc jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? "\""
jc ::= [^"\\\x00-\x1F]

ws ::= [ \t\n\r]*
"""

      let cmsStageGbnf =
        """root ::= ws cms_stage ws

      cms_stage ::= "{" ws "CmsStageOutput::Pages" ws "=" ws "2Of2(" ws string_list ws ")" ws ";" ws "CmsStageOutput::HomepageSections" ws "=" ws "2Of2(" ws string_list ws ")" ws "}"

      string_list ::= "{" ws label (ws ";" ws label)? (ws ";" ws label)? (ws ";" ws label)? (ws ";" ws label)? (ws ";" ws label)? (ws ";" ws label)? (ws ";" ws label)? ws "}"

label ::= "\"" jc jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? "\""
jc ::= [^"\\\x00-\x1F]

ws ::= [ \t\n\r]*
"""

      let productsStageGbnf =
        """root ::= ws products_stage ws

      products_stage ::= "{" ws "ProductsStageOutput::Categories" ws "=" ws "2Of2(" ws string_list ws ")" ws ";" ws "ProductsStageOutput::ProductIdeas" ws "=" ws "2Of2(" ws string_list ws ")" ws ";" ws "ProductsStageOutput::PriceBandHint" ws "=" ws "2Of2(" ws label ws ")" ws "}"

      string_list ::= "{" ws label (ws ";" ws label)? (ws ";" ws label)? (ws ";" ws label)? (ws ";" ws label)? (ws ";" ws label)? (ws ";" ws label)? (ws ";" ws label)? ws "}"

label ::= "\"" jc jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? jc? "\""
jc ::= [^"\\\x00-\x1F]

ws ::= [ \t\n\r]*
"""

      let validateWithBallerinaParser (candidate: string) : Result<string, string> =
        let trimmed = candidate.Trim()

        if String.IsNullOrWhiteSpace(trimmed) then
          Error "empty completion"
        else
          let programText =
            if trimmed.EndsWith(";", StringComparison.Ordinal) then
              trimmed
            else
              trimmed + ";"

          let initialLocation = Location.Initial "AIConfigurator::briefToPlan"

          match
            tokens
            |> Parser.Run(programText |> Seq.toList, initialLocation)
            |> sum.MapError fst
          with
          | Sum.Right err -> Error $"lexer failed: {err}"
          | Sum.Left(ParserResult(actual, _)) ->
            match
              (Parser.Expr.program ()).Parser
              |> Parser.Run(actual, initialLocation)
              |> sum.MapError fst
            with
            | Sum.Right err -> Error $"parser failed: {err}"
            | Sum.Left(ParserResult(_, _)) -> Ok trimmed

      let runConstrainedCompletion
        (opName: string)
        (grammar: string)
        (fullPrompt: string)
        =
        try
          let baseUrl =
            Environment.GetEnvironmentVariable("BISE_LLM_URL")
            |> Option.ofObj
            |> Option.filter (fun x -> not (String.IsNullOrWhiteSpace(x)))
            |> Option.defaultValue "http://localhost:8080"
            |> fun x -> x.TrimEnd('/')

          let modelName =
            Environment.GetEnvironmentVariable("BISE_LLM_MODEL")
            |> Option.ofObj
            |> Option.filter (fun x -> not (String.IsNullOrWhiteSpace(x)))
            |> Option.defaultValue "qwen3-32b"

          use http = new HttpClient(Timeout = TimeSpan.FromMinutes(2.0))

          let requestOnce () : Result<string, string> =
            let payload =
              JsonSerializer.Serialize(
                {| prompt = fullPrompt
                   n_predict = 2048
                   temperature = 0.2
                   top_p = 0.9
                   stop = [||]
                   grammar = grammar
                   model = modelName |}
              )

            use request =
              new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/completion")

            request.Content <- new StringContent(payload, Encoding.UTF8, "application/json")

            use response = http.Send(request)
            let body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult()

            if not response.IsSuccessStatusCode then
              Error
                $"status={(int response.StatusCode)} body={body}"
            else
              try
                let completion = JsonSerializer.Deserialize<CompletionResponse>(body)

                if isNull (box completion) || String.IsNullOrWhiteSpace(completion.content) then
                  Error "missing completion content"
                else
                  validateWithBallerinaParser completion.content
              with ex ->
                Error $"completion decode failed: {ex.Message}"

          let maxAttempts = 3

          let rec run attemptsLeft lastError =
            if attemptsLeft <= 0 then
              match lastError with
              | Some err ->
                Console.Error.WriteLine(
                  $"[AIConfigurator::{opName} conformance error] {err}"
                )
              | None ->
                Console.Error.WriteLine(
                  $"[AIConfigurator::{opName} conformance error] unknown failure"
                )

              invalidSentinel opName
            else
              match requestOnce () with
              | Ok parsed -> parsed
              | Error err -> run (attemptsLeft - 1) (Some err)

          run maxAttempts None
        with ex ->
          Console.Error.WriteLine($"[AIConfigurator::{opName} exception] {ex.Message}")
          invalidSentinel opName

      { briefToPlan =
          fun _ prompt ->
            try
              let fullPrompt =
                "You are a webshop plan generator. "
                + "Generate a BriefPlan Ballerina record expression. "
                + "Context: "
                + prompt
              runConstrainedCompletion "briefToPlan" briefPlanGbnf fullPrompt
            with ex ->
              Console.Error.WriteLine($"[AIConfigurator::briefToPlan exception] {ex.Message}")
              invalidSentinel "briefToPlan"
        cmsStage =
          fun _ prompt ->
            let fullPrompt =
              "You are a webshop CMS stage generator. "
              + "Generate a CmsStageOutput Ballerina record expression. "
              + "Context: "
              + prompt

            runConstrainedCompletion "cmsStage" cmsStageGbnf fullPrompt
        productsStage =
          fun _ prompt ->
            let fullPrompt =
              "You are a webshop products stage generator. "
              + "Generate a ProductsStageOutput Ballerina record expression. "
              + "Context: "
              + prompt

            runConstrainedCompletion "productsStage" productsStageGbnf fullPrompt }

  let AIConfiguratorExtension<'runtimeContext, 'ext>
    (ai_ops: AIConfiguratorTypeClass<'runtimeContext>)
    (operationLens: PartialLens<'ext, AIConfiguratorOperations<'ext>>)
    : OperationsExtension<'runtimeContext, 'ext, AIConfiguratorOperations<'ext>> =

    let stringTypeValue = TypeValue.CreateString()

    let briefToPlanId =
      Identifier.FullyQualified([ "AIConfigurator" ], "briefToPlan")
      |> TypeCheckScope.Empty.Resolve

    let cmsStageId =
      Identifier.FullyQualified([ "AIConfigurator" ], "cmsStage")
      |> TypeCheckScope.Empty.Resolve

    let productsStageId =
      Identifier.FullyQualified([ "AIConfigurator" ], "productsStage")
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
            | AIConfiguratorOperations.BriefToPlan -> Some AIConfiguratorOperations.BriefToPlan
            | _ -> None)
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

    let cmsStageOperation
      : ResolvedIdentifier *
        OperationExtension<'runtimeContext, 'ext, AIConfiguratorOperations<'ext>> =
      cmsStageId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(stringTypeValue, stringTypeValue),
              Kind.Star,
              AIConfiguratorOperations.CmsStage)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | AIConfiguratorOperations.CmsStage -> Some AIConfiguratorOperations.CmsStage
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              do!
                op
                |> AIConfiguratorOperations.AsCmsStage
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
              let response = ai_ops.cmsStage ctx.RuntimeContext prompt

              return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.String(response))
            } }

    let productsStageOperation
      : ResolvedIdentifier *
        OperationExtension<'runtimeContext, 'ext, AIConfiguratorOperations<'ext>> =
      productsStageId,
      { PublicIdentifiers =
          Some
          <| (TypeValue.CreateArrow(stringTypeValue, stringTypeValue),
              Kind.Star,
              AIConfiguratorOperations.ProductsStage)
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | AIConfiguratorOperations.ProductsStage -> Some AIConfiguratorOperations.ProductsStage
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              do!
                op
                |> AIConfiguratorOperations.AsProductsStage
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
              let response = ai_ops.productsStage ctx.RuntimeContext prompt

              return Value<TypeValue<'ext>, 'ext>.Primitive(PrimitiveValue.String(response))
            } }

    { TypeVars = []
      Operations =
        [ briefToPlanOperation
          cmsStageOperation
          productsStageOperation ]
        |> Map.ofList }

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
  open Ballerina.DSL.Next.Types.TypeChecker
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Terms
  open Ballerina.State.WithError
  open Ballerina.Lenses
  open Ballerina.DSL.Next.Extensions
  open Ballerina.Parser
  open Ballerina.DSL.Next.Syntax
  open Ballerina.Collections.NonEmptyList

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
        """root ::= briefplan

      briefplan ::= "{" cmsfield ";" productsfield ";" mediafield ";" themefield "}"

      cmsfield ::= "BriefPlan::Cms=2Of2(" cmsrecord ")"
      cmsrecord ::= "{" "PlanCmsInput::Pages=2Of2(" stringlist ")" ";" "PlanCmsInput::HomepageSections=2Of2(" stringlist ")" "}"

      productsfield ::= "BriefPlan::Products=2Of2(" productsrecord ")"
      productsrecord ::= "{" "PlanProductsInput::Categories=2Of2(" stringlist ")" ";" "PlanProductsInput::ProductIdeas=2Of2(" stringlist ")" ";" "PlanProductsInput::PriceBandHint=2Of2(" label ")" "}"

      mediafield ::= "BriefPlan::Media=2Of2(" mediarecord ")"
      mediarecord ::= "{" "PlanMediaInput::FeaturedAssets=1Of2()" ";" "PlanMediaInput::GalleryHints=2Of2(" stringlist ")" "}"

      themefield ::= "BriefPlan::Theme=2Of2(" themerecord ")"
      themerecord ::= "{" "PlanThemeInput::StyleKeywords=2Of2(" stringlist ")" ";" "PlanThemeInput::LayoutHints=2Of2(" stringlist ")" "}"

      stringlist ::= "{" label (";" label)? (";" label)? (";" label)? (";" label)? (";" label)? (";" label)? (";" label)? "}"

      label ::= "\"" labelchar labelchar? labelchar? labelchar? labelchar? labelchar? labelchar? labelchar? labelchar? labelchar? labelchar? labelchar? "\""
      labelchar ::= [a-z]
"""

      let cmsStageGbnf =
        """root ::= cmsstage

      cmsstage ::= "{" "CmsStageOutput::Pages=2Of2(" stringlist ")" ";" "CmsStageOutput::HomepageSections=2Of2(" stringlist ")" "}"

      stringlist ::= "{" label (";" label)? (";" label)? (";" label)? (";" label)? (";" label)? (";" label)? (";" label)? "}"

      label ::= "\"" labelchar labelchar? labelchar? labelchar? labelchar? labelchar? labelchar? labelchar? labelchar? labelchar? labelchar? labelchar? "\""
      labelchar ::= [a-z]
"""

      let productsStageGbnf =
        """root ::= productsstage

      productsstage ::= "{" "ProductsStageOutput::Categories=2Of2(" stringlist ")" ";" "ProductsStageOutput::ProductIdeas=2Of2(" stringlist ")" ";" "ProductsStageOutput::PriceBandHint=2Of2(" label ")" "}"

      stringlist ::= "{" label (";" label)? (";" label)? (";" label)? (";" label)? (";" label)? (";" label)? (";" label)? "}"

      label ::= "\"" labelchar labelchar? labelchar? labelchar? labelchar? labelchar? labelchar? labelchar? labelchar? labelchar? labelchar? labelchar? "\""
      labelchar ::= [a-z]
"""

      let aiConfiguratorTypeCheckingConfig : TypeCheckingConfig<unit> =
        let queryTypeSymbol = TypeSymbol.Create(Identifier.LocalScope "Query")
        let listTypeSymbol = TypeSymbol.Create(Identifier.LocalScope "List")
        let viewTypeSymbol = TypeSymbol.Create(Identifier.LocalScope "View")
        let viewPropsTypeSymbol = TypeSymbol.Create(Identifier.LocalScope "ViewProps")
        let reactNodeTypeSymbol = TypeSymbol.Create(Identifier.LocalScope "ReactNode")
        let reactComponentTypeSymbol =
          TypeSymbol.Create(Identifier.LocalScope "ReactComponent")

        let coTypeSymbol = TypeSymbol.Create(Identifier.LocalScope "Co")

        let mkImportedType
          (name: Identifier)
          (sym: TypeSymbol)
          (args: List<TypeValue<unit>>)
          : TypeValue<unit> =
          TypeValue.CreateImported
            { Id = name |> TypeCheckScope.Empty.Resolve
              Sym = sym
              Parameters = []
              Arguments = args }

        { QueryTypeSymbol = queryTypeSymbol
          ListTypeSymbol = listTypeSymbol
          ViewTypeSymbol = viewTypeSymbol
          ViewPropsTypeSymbol = viewPropsTypeSymbol
          ReactNodeTypeSymbol = reactNodeTypeSymbol
          ReactComponentTypeSymbol = reactComponentTypeSymbol
          CoTypeSymbol = coTypeSymbol
          MkQueryType =
            fun schema row ->
              mkImportedType
                (Identifier.FullyQualified([ "DB" ], "Query"))
                queryTypeSymbol
                [ TypeValue.CreateSchema schema; TypeValue.QueryRow row ]
          MkListType =
            fun itemType ->
              mkImportedType
                (Identifier.FullyQualified([ "List" ], "List"))
                listTypeSymbol
                [ itemType ]
          MkViewType =
            fun schema ctx st ->
              mkImportedType
                (Identifier.FullyQualified([ "Frontend" ], "View"))
                viewTypeSymbol
                [ schema; ctx; st ]
          MkViewPropsType =
            fun schema ctx st ->
              mkImportedType
                (Identifier.FullyQualified([ "Frontend" ], "ViewProps"))
                viewPropsTypeSymbol
                [ schema; ctx; st ]
          MkCoType =
            fun model msg cmd schema ->
              mkImportedType
                (Identifier.FullyQualified([ "Frontend" ], "Co"))
                coTypeSymbol
                [ model; msg; cmd; schema ]
          ImportedTypesWithFields = Map.empty }

      let validateWithBallerinaParserAndTypeCheck
        (opName: string)
        (candidate: string)
        : Result<string, string> =
        let trimmed = candidate.Trim()

        let canonicalSeedFor (name: string) : Option<string> =
          match name with
          | "briefToPlan" ->
            Some
              "{BriefPlan::Cms=2Of2({PlanCmsInput::Pages=2Of2({\"home\";\"about\"});PlanCmsInput::HomepageSections=2Of2({\"hero\";\"featured\"})});BriefPlan::Products=2Of2({PlanProductsInput::Categories=2Of2({\"apparel\";\"accessories\"});PlanProductsInput::ProductIdeas=2Of2({\"itemone\";\"itemtwo\"});PlanProductsInput::PriceBandHint=2Of2(\"mid\")});BriefPlan::Media=2Of2({PlanMediaInput::FeaturedAssets=1Of2();PlanMediaInput::GalleryHints=2Of2({\"studio\";\"lifestyle\"})});BriefPlan::Theme=2Of2({PlanThemeInput::StyleKeywords=2Of2({\"clean\";\"modern\"});PlanThemeInput::LayoutHints=2Of2({\"grid\";\"story\"})})}"
          | "cmsStage" ->
            Some
              "{CmsStageOutput::Pages=2Of2({\"home\";\"catalog\"});CmsStageOutput::HomepageSections=2Of2({\"hero\";\"featured\"})}"
          | "productsStage" ->
            Some
              "{ProductsStageOutput::Categories=2Of2({\"apparel\";\"accessories\"});ProductsStageOutput::ProductIdeas=2Of2({\"itemone\";\"itemtwo\"});ProductsStageOutput::PriceBandHint=2Of2(\"mid\")}"
          | _ -> None

        let ensureProgramText (text: string) =
          if text.EndsWith(";", StringComparison.Ordinal) then
            text
          else
            text + ";"

        let typeCheckAndEval
          (programText: string)
          : Result<
              Value<TypeValue<unit>, unit> *
              TypeValue<unit> *
              TypeCheckState<unit>,
              string
             > =
          let initialLocation = Location.Initial "AIConfigurator::semantic-check"

          match
            tokens
            |> Parser.Run(programText |> Seq.toList, initialLocation)
            |> sum.MapError fst
          with
          | Sum.Right err -> Error $"semantic lexer failed: {err}"
          | Sum.Left(ParserResult(actual, _)) ->
            match
              (Parser.Expr.program ()).Parser
              |> Parser.Run(actual, initialLocation)
              |> sum.MapError fst
            with
            | Sum.Right err -> Error $"semantic parser failed: {err}"
            | Sum.Left(ParserResult(program, _)) ->
              let initialTypeCheckContext = TypeCheckContext.Empty("", "")
              let initialTypeCheckState = TypeCheckState.Empty

              match
                Expr.TypeCheck aiConfiguratorTypeCheckingConfig None program
                |> State.Run(initialTypeCheckContext, initialTypeCheckState)
                |> sum.MapError fst
              with
              | Sum.Right err -> Error $"typecheck failed: {err}"
              | Sum.Left((typeCheckedExpr, _), stateOpt) ->
                let typeCheckState = stateOpt |> Option.defaultValue initialTypeCheckState
                let inferredType = typeCheckedExpr.Type

                match Conversion.convertExpression typeCheckedExpr with
                | Sum.Right err -> Error $"runnable conversion failed: {err}"
                | Sum.Left runnableExpr ->
                  match
                    Expr.Eval(NonEmptyList.One runnableExpr)
                    |> Reader.Run(ExprEvalContext<unit, unit>.Empty())
                  with
                  | Sum.Right err -> Error $"evaluation failed: {err}"
                  | Sum.Left value -> Ok(value, inferredType, typeCheckState)

        if String.IsNullOrWhiteSpace(trimmed) then
          Error "empty completion"
        else
          let programText = ensureProgramText trimmed

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
            | Sum.Left(ParserResult(_, _)) ->
              match canonicalSeedFor opName with
              | None -> Error $"unknown operation: {opName}"
              | Some seed ->
                let seedProgramText = ensureProgramText seed

                match typeCheckAndEval seedProgramText with
                | Error err -> Error $"seed typecheck failed: {err}"
                | Ok(_, seedType, seedTypeCheckState) ->
                  match typeCheckAndEval programText with
                  | Error err -> Error err
                  | Ok(candidateValue, _, _) ->
                    let noExtChecker: IsExtInstanceOf<unit> =
                      fun _ _ _ ->
                        Errors.Singleton () (fun () ->
                          "Unexpected extension value in AIConfigurator output")
                        |> reader.Throw

                    match
                      Value.TypeCheck noExtChecker candidateValue seedType
                      |> Reader.Run(
                        TypeCheckContext.Empty("", ""),
                        seedTypeCheckState
                      )
                    with
                    | Sum.Right err -> Error $"value typecheck failed: {err}"
                    | Sum.Left() -> Ok trimmed

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
                  validateWithBallerinaParserAndTypeCheck opName completion.content
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

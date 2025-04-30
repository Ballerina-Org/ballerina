namespace Ballerina.AI.LLM

module LLM =

  open Ballerina.Collections.Sum
  open Ballerina.DSL.Expr.Types.Model
  open Ballerina.Errors
  open Ballerina.DSL.Expr.Model

  type LLMOutput = LLMOutput of string

  type OutputStructureDescriptionForPrompt = OutputStructureDescriptionForPrompt of string
  type TextContext = TextContext of string
  type TaskExplanation = TaskExplanation of string
  type Base64PNGImage = Base64PNGImage of string

  type Prompt =
    { TaskExplanation: TaskExplanation
      Context: TextContext
      Image: Base64PNGImage option
      OutputStructureDescriptionForPrompt: OutputStructureDescriptionForPrompt }

  [<NoComparison; NoEquality>]
  type LLMIntegration<'schema> = LLMIntegration of (Prompt -> 'schema -> Sum<LLMOutput, Errors>)

  type TypeDeclaration =
    { OutputType: ExprType
      Refs: (TypeId * ExprType) list } // TODO: replace with Let bindings

  [<NoComparison; NoEquality>]
  type StructuredOutputIntegration<'schema> =
    | StructuredOutputIntegration of
      (TypeDeclaration
        -> (Sum<OutputStructureDescriptionForPrompt * 'schema, Errors> * (LLMOutput -> Sum<Value, Errors>)))

  [<NoComparison; NoEquality>]
  type LLM<'schema> =
    { LLMIntegration: LLMIntegration<'schema>
      StructuredOutputIntegration: StructuredOutputIntegration<'schema> }


    static member Call<'schema> (llm: LLM<'schema>) outputType taskExplanation context image =
      let (StructuredOutputIntegration structuredOutputIntegration) =
        llm.StructuredOutputIntegration

      let (LLMIntegration llmIntegration) = llm.LLMIntegration

      let outputStructureDescriptionForPrompt, parseOutput =
        structuredOutputIntegration outputType

      sum {
        let! outputStructureDescriptionForPrompt, schema = outputStructureDescriptionForPrompt

        let prompt =
          { TaskExplanation = taskExplanation
            Context = context
            Image = image
            OutputStructureDescriptionForPrompt = outputStructureDescriptionForPrompt }

        return! schema |> llmIntegration prompt |> Sum.bind parseOutput
      }

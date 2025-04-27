namespace Ballerina.AI.LLM

[<RequireQualifiedAccess>]
module LLM =

  open Ballerina.Collections.Sum
  open Ballerina.DSL.Expr.Types.Model
  open Ballerina.Errors
  open Ballerina.DSL.Expr.Model
  open System.Drawing

  type LLMOutput = LLMOutput of string

  type OutputStructureDescriptionForPrompt = OutputStructureDescriptionForPrompt of string
  type TextContext = TextContext of string

  type Prompt =
    { TaskExplanation: string
      Context: TextContext
      Image: Image option
      OutputStructureDescriptionForPrompt: OutputStructureDescriptionForPrompt }

  type LLMIntegration<'schema> = LLMIntegration of (Prompt -> 'schema -> Sum<LLMOutput, Errors>)

  type StructuredOutputIntegration<'schema> =
    | StructuredOutputIntegration of
      (ExprType -> (Sum<OutputStructureDescriptionForPrompt * 'schema, Errors> * (LLMOutput -> Sum<Value, Errors>)))

  let callLLM
    (LLMIntegration llmIntegration)
    (StructuredOutputIntegration exprTypeToPromptAndOutputParser)
    exprType
    taskExplanation
    context
    image
    =

    let outputStructureDescriptionForPrompt, parseOutput =
      exprTypeToPromptAndOutputParser exprType

    sum {
      let! outputStructureDescriptionForPrompt, schema = outputStructureDescriptionForPrompt

      let prompt =
        { TaskExplanation = taskExplanation
          Context = context
          Image = image
          OutputStructureDescriptionForPrompt = outputStructureDescriptionForPrompt }

      return! prompt |> llmIntegration schema |> Sum.bind parseOutput
    }

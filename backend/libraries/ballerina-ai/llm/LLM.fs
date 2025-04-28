namespace Ballerina.AI.LLM

[<RequireQualifiedAccess>]
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

  [<NoComparison; NoEquality>]
  type StructuredOutputIntegration<'schema> =
    | StructuredOutputIntegration of
      (ExprType -> (Sum<OutputStructureDescriptionForPrompt * 'schema, Errors> * (LLMOutput -> Sum<Value, Errors>)))

  let callLLM<'schema>
    (LLMIntegration llmIntegration: LLMIntegration<'schema>)
    (StructuredOutputIntegration structuredOutputIntegration: StructuredOutputIntegration<'schema>)
    exprType
    taskExplanation
    context
    image
    =

    let outputStructureDescriptionForPrompt, parseOutput =
      structuredOutputIntegration exprType

    sum {
      let! outputStructureDescriptionForPrompt, schema = outputStructureDescriptionForPrompt

      let prompt =
        { TaskExplanation = taskExplanation
          Context = context
          Image = image
          OutputStructureDescriptionForPrompt = outputStructureDescriptionForPrompt }

      return! schema |> llmIntegration prompt |> Sum.bind parseOutput
    }

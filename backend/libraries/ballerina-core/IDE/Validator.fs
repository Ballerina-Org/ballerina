namespace Ballerina.IDE

open FSharp.Data

open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization

open Ballerina.Collections.Sum
open Ballerina.State.WithError
open Ballerina.DSL.Expr.Types.Model
open Ballerina.DSL.FormEngine.Model
open Ballerina.DSL.FormEngine.Parser.Runner
open Ballerina.DSL.FormEngine.Validator

module Validator =
  let private generatedLanguageSpecificConfig =
    { EnumValueFieldName = "value"
      StreamIdFieldName = "id"
      StreamDisplayValueFieldName = "displayValue" }
  
  let parseAndValidate(spec: string) =
    let json = spec |> JsonValue.Parse
    
    let injectedTypes: Map<string, TypeBinding> = Map.empty
    let initialContext =
      { ParsedFormsContext.Empty with
          Types = injectedTypes }
      
    let codegenConfigFilePath = Environment.GetEnvironmentVariable("codegenConfigPath")
    let codegenConfig = File.ReadAllText codegenConfigFilePath

    let codegenConfig =
      JsonSerializer.Deserialize<CodeGenConfig>(
        codegenConfig,
        JsonFSharpOptions.Default().ToJsonSerializerOptions()
      )
      
    let parser =
      state {
        return! ParsedFormsContext.Parse generatedLanguageSpecificConfig [json]
      }
      
    parser.run (codegenConfig, initialContext)
    |> function
       | Left (_mergedJson, Some parsedForms) ->
          let validator =
            state {
              return! (ParsedFormsContext.Validate generatedLanguageSpecificConfig parsedForms)
            }
          validator.run (codegenConfig, { PredicateValidationHistory = Set.empty })
          |> function
            | Left(_, _validationState) ->
                Left "tmp-message"
            | Right _e ->
                Right "validation-error"
       | Left (_, None) -> Right "parsing-finished-without-forms"           
       | Right (_err, _) -> Right "parsing-error"
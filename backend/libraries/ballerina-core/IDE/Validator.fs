namespace Ballerina.IDE

open FSharp.Data

open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization

open Ballerina.Collections.NonEmptyList
open Ballerina.Collections.Sum
open Ballerina.DSL.Expr.Types.Model
open Ballerina.DSL.FormEngine.Model
open Ballerina.DSL.FormEngine.Parser.Runner
open Ballerina.DSL.FormEngine.Validator
open Ballerina.Errors
open Ballerina.DSL.Expr.Model

module Validator =
  let private generatedLanguageSpecificConfig =
    { EnumValueFieldName = "Value"
      StreamIdFieldName = "Id"
      StreamDisplayValueFieldName = "DisplayValue" }
    
  let private foldErrors<'T> (errors: Errors * 'T) =
    errors
    |> fun (e, _) ->
      e.Errors
      |> NonEmptyList.ToList
      |> Array.ofList
      |> Array.fold (fun acc e -> $"{acc}\n({e.Priority}): {e.Message}\n") ""
  
  let parseAndValidate(spec: string) =
    let json = spec |> JsonValue.Parse
    
    let codegenConfigFilePath = Environment.GetEnvironmentVariable("codegenConfigPath")
    let codegenConfig = File.ReadAllText codegenConfigFilePath

    let codegenConfig =
      JsonSerializer.Deserialize<CodeGenConfig>(
        codegenConfig,
        JsonFSharpOptions.Default().ToJsonSerializerOptions()
      )
      
    let injectedTypes: Map<string, TypeBinding> = 
      codegenConfig.Custom
      |> Seq.map (fun c ->
        c.Key,
        (c.Key |> ExprTypeId.Create, ExprType.CustomType c.Key)
        |> TypeBinding.Create)
      |> Map.ofSeq
      
    let initialContext =
      { ParsedFormsContext.Empty with
          Types = injectedTypes }
      
    sum {
      let! _mergedJson, parsedForms  =
        (codegenConfig, initialContext)
        |> (ParsedFormsContext.Parse generatedLanguageSpecificConfig [json]).run
        |> sum.MapError foldErrors
          
      let! _ =
        (codegenConfig, { PredicateValidationHistory = Set.empty })
        |> (ParsedFormsContext.Validate generatedLanguageSpecificConfig parsedForms.Value).run //TODO: confirm Options is always Some
        |> sum.MapError foldErrors
      return ()
    }
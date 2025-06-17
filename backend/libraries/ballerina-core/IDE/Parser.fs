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

//TODO: most of the code is from a FromEngine, decouple that and unify
module Parser =
  let codegenConfigFilePath = Environment.GetEnvironmentVariable("codegenConfigPath")
  let private codegenConfig = File.ReadAllText codegenConfigFilePath
  
  let private CodegenConfig =
    JsonSerializer.Deserialize<CodeGenConfig>(
      codegenConfig,
      JsonFSharpOptions.Default().ToJsonSerializerOptions()
    )
    
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
  
  let parse (spec: string) =
    let json = spec |> JsonValue.Parse

    let injectedTypes: Map<string, TypeBinding> = 
      CodegenConfig.Custom
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
        (CodegenConfig, initialContext)
        |> (ParsedFormsContext.Parse generatedLanguageSpecificConfig [json]).run
        |> sum.MapError foldErrors
        
      return _mergedJson, parsedForms 
    }
  
  let validate (spec: string) =
    sum {
      let! _mergedJson, parsedForms = parse spec
          
      let! _ =
        (CodegenConfig, { PredicateValidationHistory = Set.empty })
        |> (ParsedFormsContext.Validate generatedLanguageSpecificConfig parsedForms.Value).run //TODO: confirm Options is always Some
        |> sum.MapError foldErrors
      return ()
    }
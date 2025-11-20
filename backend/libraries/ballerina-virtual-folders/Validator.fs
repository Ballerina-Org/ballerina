namespace Ballerina.VirtualFolders

open System.Text.Json
open System.Text.Json.Serialization
open Ballerina.Collections.Sum
open Ballerina.DSL.Expr.Model
open Ballerina.DSL.Expr.Types.Model
open Ballerina.DSL.FormEngine.Model
open Ballerina.Errors
open Ballerina.VirtualFolders.Model
open Ballerina.VirtualFolders.Operations
open Ballerina.VirtualFolders.Patterns
open FSharp.Data

type Validator<'ExprExt, 'ValExt> =
  { Name: string
    Validate:
      CodeGenConfig
        -> ParsedFormsContext<'ExprExt, 'ValExt>
        -> GeneratedLanguageSpecificConfig
        -> seq<FileContent>
        -> Sum<JsonValue * ParsedFormsContext<'ExprExt, 'ValExt>, Errors> }

module Validator =
  let init (_vfs: VfsNode) =
    sum {
      // let! codegen =
      //   getWellKnownFile vfs WellKnowFile.Codegen
      //   |> sum.OfOption(Errors.Singleton "Codegen config is missing")
      //
      // let! _codegen = FileContent.AsJsonString codegen.Content

      let codegenConfig = Mock.codegenConfig
      //JsonSerializer.Deserialize<CodeGenConfig>(codegen, JsonFSharpOptions.Default().ToJsonSerializerOptions())


      let injectedTypes: Map<string, TypeBinding> =
        codegenConfig.Custom
        |> Seq.map (fun c -> c.Key, (c.Key |> ExprTypeId.Create, ExprType.CustomType c.Key) |> TypeBinding.Create)
        |> Map.ofSeq


      let initialContext: ParsedFormsContext<_, _> =
        { Types = injectedTypes
          Apis = FormApis<_, _>.Empty
          Forms = Map.empty
          GenericRenderers = []
          SupportedRecordRenderers = codegenConfig.Record.SupportedRenderers
          LanguageStreamType = codegenConfig.LanguageStreamType
          Launchers = Map.empty }

      let generatedLanguageSpecificConfig =
        { EnumValueFieldName = "Value"
          StreamIdFieldName = "Id"
          StreamDisplayValueFieldName = "DisplayValue" }

      return
        {| CodegenConfig = codegenConfig
           InitialContext = initialContext
           InjectedTypes = injectedTypes |> Map.keys |> Seq.toList
           LangSpecificConfig = generatedLanguageSpecificConfig |}
    }

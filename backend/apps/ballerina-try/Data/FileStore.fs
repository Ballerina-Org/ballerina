namespace TryBallerina.Interactive

open System.IO
open System.Runtime.CompilerServices

module FileStore =

    open System.IO
    open System.Reflection
    open System.Text.Json
    open System.Text.Json.Serialization
    
    open Ballerina.DSL.FormEngine.Model
    
    let private readEmbeddedResource (resourceName: string) =
        let assembly = Assembly.GetExecutingAssembly()
        use stream = assembly.GetManifestResourceStream($"TryBallerina.data.{resourceName}")
        if isNull stream then
            failwithf $"Resource not found: %s{resourceName}"
        use reader = new StreamReader(stream)
        reader.ReadToEnd()
        
    // module CodeGenConfigs =
    //
    //   let goPath () =
    //       readEmbeddedResource "go-config.json"
    //   
    //   let go () =
    //         JsonSerializer.Deserialize<Ballerina.DSL.FormEngine.Model.CodeGenConfig>(
    //           goPath (),
    //           JsonFSharpOptions.Default().ToJsonSerializerOptions()
    //         )
    //         
    //         
    //   let injectedTypes: Map<string, Ballerina.DSL.Expr.Types.Model.TypeBinding> =
    //         let codegenConfig = go ()
    //         codegenConfig.Custom
    //         |> Seq.map (fun c ->
    //           c.Key,
    //           (c.Key |> Ballerina.DSL.Expr.Types.Model.TypeId.Create, Ballerina.DSL.Expr.Types.Model.ExprType.CustomType c.Key)
    //           //  [ ($"__CUSTOM_TYPE__{c.Key}__", ExprType.UnitType) ]
    //           //  |> Map.ofSeq
    //           //  |> ExprType.RecordType)
    //           |> Ballerina.DSL.Expr.Types.Model.TypeBinding.Create)
    //         |> Map.ofSeq
    //
    //   let initialContext =
    //     {  Ballerina.DSL.FormEngine.Model.ParsedFormsContext.Empty with
    //         Types = injectedTypes }
    //
    //   let generatedLanguageSpecificConfig: GeneratedLanguageSpecificConfig =
    //       { EnumValueFieldName = "Value"
    //         StreamIdFieldName = "Id"
    //         StreamDisplayValueFieldName = "DisplayValue" }
      
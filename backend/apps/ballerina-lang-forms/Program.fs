namespace Form.App

module Program =
  open System
  open System.CommandLine
  open Ballerina
  open Ballerina.Collections.Sum
  open System.IO
  open System.Text.Json
  open System.Text.Json.Serialization
  open FSharp.Data
  open Ballerina.DSL.FormBuilder.Compiler.FormCompiler
  open Ballerina.DSL.FormBuilder.Model.FormAST
  open Ballerina.DSL.FormBuilder.V2ToV1Bridge.ToV1JSON
  open Ballerina.DSL.FormEngine.Runner
  open Ballerina.DSL.FormEngine.Model
  open Ballerina.DSL.FormEngine.Validator
  open Ballerina.DSL.Extensions.BLPLangV1
  open Ballerina.DSL.Next.StdLib.Extensions
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Next.Runners.Project
  open Ballerina.DSL.Next.StdLib.MutableMemoryDB
  open Ballerina.DSL.Next.StdLib.String

  [<EntryPoint>]
  let main args =
    let typesFileArg =
      new Option<string>(name = "--types", Description = "Path of the file with the type definitions for the forms.")

    let formFileArg =
      new Option<string>(name = "--forms", Description = "Path of the file with the form definitions.")

    let logArg =
      new Option<bool>(name = "--log", Description = "Log the result of the form type checking or the returned error.")

    let outputArg =
      new Option<string>(
        name = "--output",
        Description = "Path of the output JSON file for the v1 generated spec result."
      )

    let rootCommand = new RootCommand "Command for the form compiler."
    rootCommand.Add typesFileArg
    rootCommand.Add formFileArg
    rootCommand.Add logArg
    rootCommand.Add outputArg

    rootCommand.SetAction(fun parseResult ->
      let logDirectory = "./out"
      let logFileName = $"{logDirectory}/form-parsing-log.txt"

      try
        let typesFilePath = parseResult.GetValue typesFileArg
        let formsFilePath = parseResult.GetValue formFileArg
        let log = parseResult.GetValue logArg
        let outputPath = parseResult.GetValue outputArg
        let types = File.ReadAllText typesFilePath
        let forms = File.ReadAllText formsFilePath

        let compilerInput =
          { Types =
              { Preludes = NonEmptyList.One(FileBuildConfiguration.FromFile(typesFilePath, types))
                Source = typesFilePath }
            ApiTypes = Map.empty
            Forms =
              { Program = forms
                Source = formsFilePath } }

        if log && Directory.Exists logDirectory |> not then
          Directory.CreateDirectory logDirectory |> ignore

        let extensions, languageContext, _db_query_sym, _make_db_query_type =
          db_ops () |> stdExtensions (StringTypeClass<_>.Console())

        let cache =
          memcache (languageContext.TypeCheckContext, languageContext.TypeCheckState)


        match compileForms compilerInput cache languageContext extensions _db_query_sym _make_db_query_type with
        | Left result ->
          let stringifiedResult = sprintf "%A" result
          Console.WriteLine stringifiedResult

          if log then
            File.WriteAllText(logFileName, stringifiedResult)

          match outputPath with
          | null
          | "" -> ()
          | output ->
            let jsonElement = FormDefinitions.toV1Json result
            let jsonOptions = JsonSerializerOptions(WriteIndented = true)
            let jsonString = JsonSerializer.Serialize(jsonElement, jsonOptions)

            let outputDir = Path.GetDirectoryName output

            if outputDir <> null && outputDir <> "" && Directory.Exists outputDir |> not then
              Directory.CreateDirectory outputDir |> ignore

            File.WriteAllText(output, jsonString)
            Console.WriteLine $"V1 generated spec result written to: {output}"

            let codegenConfigPath = "./Examples/codegenconfig.json"

            try
              let codegenConfigJson = File.ReadAllText codegenConfigPath

              let codegenConfig =
                JsonSerializer.Deserialize<CodeGenConfig>(
                  codegenConfigJson,
                  JsonFSharpOptions.Default().ToJsonSerializerOptions()
                )

              let generatedLanguageSpecificConfig =
                { EnumValueFieldName = "Value"
                  StreamIdFieldName = "Id"
                  StreamDisplayValueFieldName = "DisplayValue" }

              let schema = JsonValue.Parse jsonString
              let parseResult = parseFromJson FormsGenTarget.golang schema codegenConfig

              match parseResult with
              | Left(_, Some parsedForms) ->
                match
                  (ParsedFormsContext.Validate
                    generatedLanguageSpecificConfig
                    parsedForms
                    blpLanguageExtension.typeCheck)
                    .run (codegenConfig, { PredicateValidationHistory = Set.empty })
                with
                | Left _ -> Console.WriteLine "Successfully parsed and validated forms using parseFromJson"
                | Right validationErr -> Console.Error.WriteLine $"Validation error: {validationErr}"
              | Left(_, None) -> Console.WriteLine "Warning: parseFromJson returned no parsed forms"
              | Right err -> Console.Error.WriteLine $"Error from parseFromJson: {err}"
            with exn ->
              Console.Error.WriteLine $"Error calling parseFromJson: {exn.Message}"

          0
        | Right error ->
          Console.Error.Write error

          if log then
            File.WriteAllText(logFileName, error)

          1
      with exn ->
        Console.Error.WriteLine $"{exn.Message}"
        1)

    let parseResult = rootCommand.Parse args
    parseResult.Invoke()

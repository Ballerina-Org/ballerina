module BallerinaRuntime

open System
open Ballerina.DSL.FormEngine.Runner
open System.CommandLine
open Ballerina.DSL.Codegen
open Ballerina.DSL.AI.Codegen.Runner

let formsOptions =
  {| mode = new Option<bool>(name = "-validate", description = "Type check the given forms config.")
     language =
      (new Option<FormsGenTarget>("-codegen", "Language to generate form bindings in.", IsRequired = true))
        .FromAmong("ts", "golang")
     input =
      (new Option<string>(
        "-input",
        "Path of json file to process. Use a folder path to process all files in the folder.",
        IsRequired = true
      ))
     linkedFiles =
      (new Option<string[]>(
        "-linked",
        "Paths of json files to link.",
        IsRequired = false,
        AllowMultipleArgumentsPerToken = true
      ))
     output =
      (new Option<string>(
        "-output",
        "Relative path of the generated source file(s) directory. Will be created if it does not exist.",
        IsRequired = true
      ))
     package_name =
      (new Option<string>("-package_name", "Name of the generated package. Inferred from the filename if absent."))
     form_name =
      (new Option<string>(
        "-form_name",
        "Name of the form, prefixed to disambiguate generated symbols. Inferred from the filename if absent."
      ))
     codegen_config_path =
      (new Option<string>("-codegen_config", "Path of the codegen configuration path.", IsRequired = true)) |}

let aiOptions =
  {| output =
      new Option<string>(
        "-output",
        "Relative path of the generated source file(s) directory. Will be created if it does not exist.",
        IsRequired = true
      ) |}

[<EntryPoint>]
let main args =
  let rootCommand = new RootCommand("Ballerina runtime.")
  let formsCommand = new Command("forms")
  rootCommand.AddCommand(formsCommand)
  formsCommand.AddOption(formsOptions.mode)
  formsCommand.AddOption(formsOptions.language)
  formsCommand.AddOption(formsOptions.input)
  formsCommand.AddOption(formsOptions.linkedFiles)
  formsCommand.AddOption(formsOptions.output)
  formsCommand.AddOption(formsOptions.package_name)
  formsCommand.AddOption(formsOptions.form_name)
  formsCommand.AddOption(formsOptions.codegen_config_path)

  // dotnet run -- forms -input person-config.json -validate -codegen ts
  formsCommand.SetHandler(
    Action<_, _, _, _, _, _, _, _>(Ballerina.DSL.FormEngine.Runner.run),
    formsOptions.mode,
    formsOptions.language,
    formsOptions.input,
    formsOptions.linkedFiles,
    formsOptions.output,
    formsOptions.package_name,
    formsOptions.form_name,
    formsOptions.codegen_config_path
  )

  let aiCommand = new Command "ai"
  rootCommand.AddCommand aiCommand
  aiCommand.AddOption aiOptions.output

  aiCommand.SetHandler(Action<_>(Ballerina.DSL.AI.Codegen.Runner.run), aiOptions.output)

  rootCommand.Invoke(args)

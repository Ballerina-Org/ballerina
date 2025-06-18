namespace TryBallerina.Interactive

open Ballerina.Collections.Sum
open Ballerina.Errors
open Ballerina.DSL
open Ballerina.DSL.FormEngine
open Ballerina.DSL.FormEngine.Parser.Runner

open System.IO
open System.Threading.Tasks

open FSharp.Data

open Microsoft.DotNet.Interactive
open Microsoft.DotNet.Interactive.Commands
open Microsoft.DotNet.Interactive.Directives
open Ballerina.DSL.FormEngine.Model
open Ballerina.DSL.FormEngine.Validator

type FormsCommand() = 

  inherit KernelDirectiveCommand()
    member val FormName: string = null with get, set
    member val Input: string = null with get, set
    member val Language: string = null with get, set
    member val LinkedFiles: string = null with get, set
    member val Output: string = null with get, set
    member val PackageName: string = null with get, set
    member val CodegenConfigPath: string = null with get, set


    static member attachTo (_kernel: Kernel) =
    //     let linkedFiles = [||]
    //     let handler (command: FormsCommand) (context: KernelInvocationContext): Task =
          
          //task {
            // FormEngine.Runner.run
            //    FormsGenTarget.golang
            //    command.Input
            //    linkedFiles
            //    command.Output
            //    (command.PackageName |> Option.ofObj |> Option.defaultValue command.Input)
            //    (command.FormName |> Option.ofObj |> Option.defaultValue command.Input)
            //    command.CodegenConfigPath
              
            // let _displayed = context.DisplayAs($"""<strong>Hello from command</strong>""", "text/html")
            //
            // let inputFiles = (linkedFiles |> Array.toList ) @ [ command.Input ]
            //
            // match inputFiles |> Seq.tryFind (File.Exists >> not) with
            // | Some file -> return Right(Errors.Singleton $"Input file {file} does not exist.")
            // | _ ->
            //     let jsonValues = inputFiles |> List.map (File.ReadAllText >> JsonValue.Parse)
            //     let parse = ParsedFormsContext.Parse
            //     let validate = ParsedFormsContext.Validate
            //     match (parse FileStore.CodeGenConfigs.generatedLanguageSpecificConfig jsonValues).run (FileStore.CodeGenConfigs.go(), FileStore.CodeGenConfigs.initialContext)
            //     with
            //     | Left(_mergedJson, Some parsedForms) ->
            //         match
            //           (validate FileStore.CodeGenConfigs.generatedLanguageSpecificConfig parsedForms).run (FileStore.CodeGenConfigs.go(), { PredicateValidationHistory = Set.empty })
            //         with
            //         | _ -> return Left "" //Left(mergedJson, Some parsedForms) ->
            //     | _ -> return Right(Errors.Singleton "Validation passed.")
            //Task.CompletedTask

          //}
              
        
          
        let cmd = KernelActionDirective("#!forms")

        cmd.Parameters.Add(KernelDirectiveParameter("--input", "Path to input file", Required = true))
        cmd.Parameters.Add(KernelDirectiveParameter("--output", "Path to output", Required = true))
        cmd.Parameters.Add(KernelDirectiveParameter("--packageName", "Package name"))
        cmd.Parameters.Add(KernelDirectiveParameter("--formName", "Form name"))
        cmd.Parameters.Add(KernelDirectiveParameter("--codegenConfigPath", "Path to config"))
        
        //kernel.AddDirective<FormsCommand>(cmd, handler)
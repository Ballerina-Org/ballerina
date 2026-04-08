module SamplesIntegrationTest

open System
open System.IO
open Ballerina.Collections.NonEmptyList
open Ballerina.DSL.Next.StdLib.DB
open Ballerina.DSL.Next.StdLib.Extensions
open Ballerina.DSL.Next.StdLib.MutableMemoryDB
open Ballerina.DSL.Next.Runners
open Ballerina.DSL.Next.Types.TypeChecker
open Ballerina.Collections.Sum
open Ballerina.Errors
open Ballerina.LocalizedErrors
open Ballerina.Reader.WithError
open Ballerina.StdLib.String
open ProjectModel

type ValueExt = ValueExt<unit, MutableMemoryDB<unit, unit>, unit>

let private buildContext, languageContext, typeCheckingConfig, buildCache =
  hddcacheWithStdExtensions<unit, MutableMemoryDB<unit, unit>>
    (Ballerina.DSL.Next.StdLib.String.Extension.StringTypeClass<_>.Console())
    (db_ops ())
    id
    id

/// Build a single project and return success/failure
let buildProject
  (project: ProjectBuildConfiguration)
  : Sum<string, Errors<Location>> =
  sum {
    let! buildResult =
      ProjectBuildConfiguration.BuildCached
        typeCheckingConfig
        buildCache
        project
      |> sum.MapError(fun errors ->
        let inputFiles =
          project.Files
          |> Seq.map (fun def -> def.FileName.Path, def.Content())
          |> Map.ofSeq

        for e in (Errors<_>.FilterHighestPriorityOnly errors).Errors() do
          let source =
            match inputFiles |> Map.tryFind e.Context.File with
            | Some file -> file
            | None -> ""

          let lines =
            source.Split '\n'
            |> Seq.skip (max 0 (e.Context.Line - 1))
            |> Seq.mapi (fun i line ->
              let fmt = "000"
              $"{(e.Context.Line + i).ToString(fmt)} |   {line}")
            |> Seq.truncate 3
            |> String.join "\n"

          Console.ForegroundColor <- ConsoleColor.Red

          Console.WriteLine
            $"  Error: {e.Message} at line {e.Context.Line}:\n{lines}"

          Console.ResetColor()

        errors)

    let _exprs, _typeValue, _typeCheckCtx, _typeCheckState = buildResult
    return "success"
  }

/// Find samples directory from the running executable or environment
let getSamplesDirectory () : string =
  // Try environment variable first
  match Environment.GetEnvironmentVariable("BALLERINA_SAMPLES_DIR") with
  | path when not (String.IsNullOrEmpty path) && Directory.Exists path -> path
  | _ ->
    // Try to find from current directory or parent directories
    let rec findDir marker dir =
      if String.IsNullOrEmpty(dir) || dir = Path.GetPathRoot(dir) then
        None
      else if Directory.Exists(Path.Combine(dir, marker)) then
        Some(Path.Combine(dir, marker))
      else
        findDir marker (Path.GetDirectoryName(dir))

    match findDir "samples" (Environment.CurrentDirectory) with
    | Some dir -> Path.GetFullPath(dir)
    | None ->
      // Try from repo root
      match findDir "ballerina" (Environment.CurrentDirectory) with
      | Some ballerinaDir ->
        let samplesDir = Path.Combine(ballerinaDir, "samples")

        if Directory.Exists samplesDir then
          samplesDir
        else
          failwith
            $"Samples directory not found from current dir {Environment.CurrentDirectory}"
      | None ->
        failwith
          $"Samples directory not found from current dir {Environment.CurrentDirectory}"

/// Test a single sample project file
let testSample (samplePath: string) : bool * string =
  try
    let projectName = Path.GetFileNameWithoutExtension(samplePath)

    match
      ProjectBuildConfiguration.FromProjectFile(
        samplePath,
        Path.GetDirectoryName(samplePath)
      )
    with
    | Sum.Left project ->
      match buildProject project with
      | Sum.Left _msg -> (true, $"✓ {projectName}")
      | Sum.Right errors ->
        let msg = Errors.ToString(errors, "; ")
        (false, $"✗ {projectName}: {msg}")
    | Sum.Right err ->
      let msg = Errors.ToString(err, "; ")
      (false, $"✗ {projectName}: Parse error: {msg}")
  with ex ->
    (false, $"✗ {Path.GetFileNameWithoutExtension(samplePath)}: {ex.Message}")

/// Run all integration tests
let runAllTests (verbose: bool) : int =
  try
    let samplesDir = getSamplesDirectory ()
    printfn "📂 Samples directory: %s\n" samplesDir

    let projectFiles =
      Directory.GetFiles(samplesDir, "*.blproj") |> Array.toList |> List.sort

    if List.isEmpty projectFiles then
      printfn "❌ No sample projects found"
      1
    else
      printfn "🔍 Found %d sample projects\n" projectFiles.Length

      let results =
        projectFiles
        |> List.map (fun projectFile ->
          let (success, message) = testSample projectFile

          if verbose || not success then
            printfn "  %s" message

          (success, message))

      let passCount = results |> List.filter fst |> List.length
      let failCount = List.length results - passCount

      printfn "\n📊 Results:"
      printfn "  ✓ Passed: %d" passCount
      printfn "  ✗ Failed: %d" failCount

      if failCount > 0 then
        printfn "\n❌ Failed samples:"

        results
        |> List.filter (fun (success, _) -> not success)
        |> List.iter (fun (_success, message) -> printfn "    %s" message)

        1
      else
        0
  with ex ->
    printfn "❌ Test execution failed: %s" ex.Message

    if verbose then
      printfn "%s" ex.StackTrace

    1

[<EntryPoint>]
let main args =
  let verbose =
    args |> Array.contains "--verbose" || args |> Array.contains "-v"

  runAllTests verbose

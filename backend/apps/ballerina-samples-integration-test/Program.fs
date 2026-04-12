module SamplesIntegrationTest

open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open System.Text.RegularExpressions
open Ballerina.Collections.NonEmptyList
open Ballerina.Collections.Sum
open Ballerina.DSL.Next.StdLib.DB
open Ballerina.DSL.Next.StdLib.Extensions
open Ballerina.DSL.Next.StdLib.MutableMemoryDB
open Ballerina.Errors
open Ballerina.DSL.Next.Runners
open Ballerina.DSL.Next.Terms
open Ballerina.DSL.Next.Types.Model
open Ballerina.Reader.WithError
open Ballerina.StdLib.Object

type RuntimeValueExt = ValueExt<unit, MutableMemoryDB<unit, unit>, unit>

type ProjectExecutionSuccess =
  { Value: Value<TypeValue<RuntimeValueExt>, RuntimeValueExt>
    TypeValue: TypeValue<RuntimeValueExt>
    ExpressionCount: int
    EmailEvents: string list }

[<CLIMutable>]
type SampleExpectationDto =
  { [<JsonPropertyName "mode">]
    Mode: string
    [<JsonPropertyName "valueRegexes">]
    ValueRegexes: string array option
    [<JsonPropertyName "typeRegexes">]
    TypeRegexes: string array option
    [<JsonPropertyName "errorRegexes">]
    ErrorRegexes: string array option }

[<CLIMutable>]
type SampleExpectationFile =
  { [<JsonPropertyName "expectations">]
    Expectations: Map<string, SampleExpectationDto> }

type SampleExpectation =
  { Mode: string
    ValueRegexes: string list
    TypeRegexes: string list
    ErrorRegexes: string list }

let private expectationConfigPath () =
  Path.Combine(AppContext.BaseDirectory, "sample-expectations.json")

let private expectations =
  lazy
    (let path = expectationConfigPath ()

     if not (File.Exists path) then
       failwith $"Sample expectation file not found: {path}"

     let options = JsonSerializerOptions(PropertyNameCaseInsensitive = true)
     let dto = JsonSerializer.Deserialize<SampleExpectationFile>(File.ReadAllText path, options)

     dto.Expectations
     |> Map.map (fun _ value ->
       { Mode = value.Mode.Trim().ToLowerInvariant()
         ValueRegexes = value.ValueRegexes |> Option.defaultValue [||] |> Array.toList
         TypeRegexes = value.TypeRegexes |> Option.defaultValue [||] |> Array.toList
         ErrorRegexes = value.ErrorRegexes |> Option.defaultValue [||] |> Array.toList }))

let private buildAndEvalProject
  (project: ProjectBuildConfiguration)
  : Sum<ProjectExecutionSuccess, string> =
  let emailEvents = ResizeArray<string>()

  let _, context, typeCheckingConfig, buildCache =
    hddcacheWithStdExtensions<unit, MutableMemoryDB<unit, unit>>
      (Ballerina.DSL.Next.StdLib.String.Extension.StringTypeClass<_>.Console())
      (Ballerina.DSL.Next.StdLib.Email.Extension.EmailTypeClass<_>.FromRuntimeContext(fun _ toEmail subject body ->
        emailEvents.Add($"email_send|to={toEmail}|subject={subject}|body={body}")))
      (db_ops ())
      id
      id

  let buildResult =
    ProjectBuildConfiguration.BuildCached typeCheckingConfig buildCache project

  match buildResult with
  | Left(exprs, typeValue, _, finalState) ->
    let evalContext = ExprEvalContext.Empty() |> context.ExprEvalContext

    let evalContext =
      ExprEvalContext.WithTypeCheckingSymbols evalContext finalState.Symbols

    let evalResult =
      Expr.Eval(NonEmptyList.prependList context.TypeCheckedPreludes exprs)
      |> Reader.Run evalContext

    match evalResult with
    | Left value ->
      Left
        { Value = value
          TypeValue = typeValue
          ExpressionCount = NonEmptyList.ToList exprs |> List.length
          EmailEvents = emailEvents |> Seq.toList }
    | Right e -> Right(sprintf "Evaluation failed: %s" (Errors.ToString(e, "\n")))
  | Right e -> Right(sprintf "Build failed: %s" (Errors.ToString(e, "\n")))

let private matchesAll
  (patterns: string list)
  (text: string)
  : bool * string list =
  let missing =
    patterns
    |> List.filter (fun pattern ->
      not (Regex.IsMatch(text, pattern, RegexOptions.Singleline ||| RegexOptions.IgnoreCase)))

  (List.isEmpty missing, missing)

let private validateExpectation
  (projectName: string)
  (expectation: SampleExpectation)
  (executionResult: Sum<ProjectExecutionSuccess, string>)
  : bool * string =
  match expectation, executionResult with
  | expectation, Left result when expectation.Mode = "run" || expectation.Mode = "success" ->
    let valueText =
      let events = result.EmailEvents |> String.concat "\n"

      if String.IsNullOrWhiteSpace(events) then
        result.Value.AsFSharpString
      else
        $"{result.Value.AsFSharpString}\n{events}"

    let typeText = result.TypeValue.AsFSharpString
    let valueOk, missingValue = matchesAll expectation.ValueRegexes valueText
    let typeOk, missingType = matchesAll expectation.TypeRegexes typeText

    match valueOk, typeOk with
    | true, true ->
      (true, $"matched expected output patterns for {projectName}")
    | _ ->
      let missingValueText = String.concat "; " missingValue
      let missingTypeText = String.concat "; " missingType

      let parts =
        [ if not valueOk then
            yield $"missing value patterns [{missingValueText}] in {valueText}"
          if not typeOk then
            yield $"missing type patterns [{missingTypeText}] in {typeText}" ]

      (false, String.concat " | " parts)
  | expectation, Right errorMessage when expectation.Mode = "build-fails" || expectation.Mode = "failure" ->
    let errorOk, missing = matchesAll expectation.ErrorRegexes errorMessage

    if errorOk then
      (true, "failed as expected")
    else
      let missingErrorText = String.concat "; " missing
      (false,
       $"missing error patterns [{missingErrorText}] in {errorMessage}")
  | expectation, Left result when expectation.Mode = "build-fails" || expectation.Mode = "failure" ->
    (false,
     $"expected build failure, but project executed successfully with final value {result.Value.AsFSharpString}")
  | expectation, Left _ ->
    (false, $"Unsupported expectation mode '{expectation.Mode}'")
  | _, Right errorMessage -> (false, errorMessage)

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
    let projectName = Path.GetFileNameWithoutExtension samplePath
    let expectation =
      match expectations.Value |> Map.tryFind projectName with
      | Some expectation -> expectation
      | None -> failwith $"No sample expectation configured for {projectName}"

    match
      ProjectBuildConfiguration.FromProjectFile(
        samplePath,
        Path.GetDirectoryName(samplePath)
      )
    with
    | Sum.Left project ->
      let success, detail =
        validateExpectation projectName expectation (buildAndEvalProject project)

      if success then
        (true, $"✓ {projectName}: {detail}")
      else
        (false, $"✗ {projectName}: {detail}")
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

module Ballerina.Main

open System
open System.CommandLine
open Ballerina.LanguageServer
open Ballerina.Collections.NonEmptyList
open Ballerina.DSL.Next.Runners
open Ballerina.DSL.Next.Types.TypeChecker
open Ballerina.Collections.Sum
open Ballerina.Errors
open Ballerina.LocalizedErrors
open Ballerina.Build

let private printInlays (typeCheckState: TypeCheckState<ValueExt>) =
  let inlays =
    typeCheckState.InlayHints
    |> Map.toList
    |> List.sortBy (fun (loc, _) -> loc.File, loc.Line, loc.Column)

  if inlays.Length > 0 then
    Console.WriteLine "Inlays:"

    inlays
    |> List.iter (fun (loc, inlay) ->
      Console.WriteLine
        $"  {loc.File}:{loc.Line}:{loc.Column}  {inlay.Type.ToInlayString()}")

let buildOnly showDebugInlays files =
  sum {
    let! _langCtx, (_exprs, _typeValue, _typeCheckCtx, typeCheckState) =
      build files

    if showDebugInlays then
      printInlays typeCheckState

    return ()
  }

let buildOnlyFromProject
  (project: ProjectBuildConfiguration)
  : Sum<InlayHintDTO array, Errors<Location>> =
  sum {
    let! _, (_, _, _, typeCheckState) = buildProject project
    return BuildServer.inlayHintDtosFromTypeCheckState typeCheckState
  }

let buildResultForPath (path: string) : BuildResultDTO =
  BuildServer.buildResultFromPath buildOnlyFromProject path

let runServerLoop () =
  BuildServer.runServerLoop buildResultForPath

let buildProjectStreamingForPath
  (path: string)
  (emitFileEvent: FileBuiltEventDTO -> unit)
  : Sum<InlayHintDTO[] * ScopeSymbolDTO[] * int * int, Errors<Location>> =
  match BuildServer.projectFromPath path with
  | Left project ->
    let totalFiles = 1 + project.Files.Tail.Length
    let mutable errorCount = 0

    let onFileBuilt
      (file: FileBuildConfiguration)
      (ctx: TypeCheckContext<Build.ValueExt>)
      (st: TypeCheckState<Build.ValueExt>)
      =
      let inlayHints =
        BuildServer.inlayHintDtosForFile st file.FileName.Path

      let identifierHints =
        BuildServer.identifierHintDtosFromContext ctx

      let dotAccessHints =
        BuildServer.dotAccessHintDtosForFile st file.FileName.Path

      let scopeAccessHints =
        BuildServer.scopeAccessHintDtosForFile st file.FileName.Path

      let event: FileBuiltEventDTO =
        { EventType = "file-built"
          File = file.FileName.Path
          Success = true
          Errors = [||]
          InlayHints = inlayHints
          IdentifierHints = identifierHints
          DotAccessHints = dotAccessHints
          ScopeAccessHints = scopeAccessHints }

      emitFileEvent event

    match buildProjectStreaming project onFileBuilt with
    | Left(_, (_fileOutputs, _finalContext, finalState)) ->
      let inlayHints =
        BuildServer.inlayHintDtosFromTypeCheckState finalState

      let scopeSymbols =
        BuildServer.scopeSymbolDtosFromState finalState

      Left(inlayHints, scopeSymbols, totalFiles, errorCount)
    | Right errors -> Right errors
  | Right errors -> Right errors

let typecheckSingleFileForPath
  (projectPath: string)
  (filePath: string)
  (fileContent: string)
  : Sum<FileBuiltEventDTO, Errors<Location>> =
  match BuildServer.projectFromPath projectPath with
  | Left project ->
    match
      ProjectBuildConfiguration.TypeCheckSingleFile
        typeCheckingConfig
        buildCache
        project
        filePath
        fileContent
    with
    | Left(ctx, st) ->
      let inlayHints =
        BuildServer.inlayHintDtosForFile st filePath

      let identifierHints =
        BuildServer.identifierHintDtosFromContext ctx

      let dotAccessHints =
        BuildServer.dotAccessHintDtosForFile st filePath

      let scopeAccessHints =
        BuildServer.scopeAccessHintDtosForFile st filePath

      let event: FileBuiltEventDTO =
        { EventType = "file-built"
          File = filePath
          Success = true
          Errors = [||]
          InlayHints = inlayHints
          IdentifierHints = identifierHints
          DotAccessHints = dotAccessHints
          ScopeAccessHints = scopeAccessHints }

      Left event
    | Right errors ->
      let errorDtos =
        (Errors<_>.FilterHighestPriorityOnly errors).Errors()
        |> NonEmptyList.ToList
        |> List.map (fun e ->
          { Message = e.Message
            File = e.Context.File
            Line = e.Context.Line
            Column = e.Context.Column })
        |> List.toArray

      let event: FileBuiltEventDTO =
        { EventType = "file-built"
          File = filePath
          Success = false
          Errors = errorDtos
          InlayHints = [||]
          IdentifierHints = [||]
          DotAccessHints = [||]
          ScopeAccessHints = [||] }

      Left event
  | Right errors -> Right errors

let runServerLoopStreaming () =
  BuildServer.runServerLoopStreaming
    buildProjectStreamingForPath
    typecheckSingleFileForPath

[<EntryPoint>]
let main (args: string array) =
  let fileOption = Cli.createFileOption ()

  let runOption = Cli.createRunOption ()

  let debugInlaysOption = Option<bool> "--debug-inlays"
  debugInlaysOption.Description <- "Print inlay overlay debug output to console"
  debugInlaysOption.DefaultValueFactory <- fun _ -> false

  let rootCommand = RootCommand "Ballerina language runtime"

  rootCommand.Add fileOption
  rootCommand.Add runOption
  rootCommand.Add debugInlaysOption

  Cli.addServerCommand rootCommand runServerLoop
  Cli.addServerStreamingCommand rootCommand runServerLoopStreaming

  rootCommand.SetAction(fun parseResult ->
    let filename = parseResult.GetValue fileOption
    let runProgram = parseResult.GetValue runOption
    let showDebugInlays = parseResult.GetValue debugInlaysOption
    let files = NonEmptyList.OfList(filename, [])

    if runProgram then
      match buildAndRun files with
      | Left value ->
        Console.WriteLine $"Result: {value}"
        Console.WriteLine "Program executed successfully."
        0
      | Right _errors -> 1
    else
      match buildOnly showDebugInlays files with
      | Left() ->
        Console.WriteLine "Program built successfully."
        0
      | Right _errors -> 1)

  if args.Length = 0 then
    rootCommand.Parse("--help").Invoke()
  else
    rootCommand.Parse(args).Invoke()

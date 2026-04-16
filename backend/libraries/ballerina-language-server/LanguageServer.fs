namespace Ballerina.LanguageServer

open System
open System.CommandLine
open System.IO
open System.Text.Json
open Ballerina
open Ballerina.Collections.NonEmptyList
open Ballerina.Collections.Option
open Ballerina.Collections.Sum
open Ballerina.DSL.Next.Runners
open Ballerina.Errors
open Ballerina.LocalizedErrors
open Ballerina.DSL.Next.Types.TypeChecker.Model

type BuildErrorDTO =
  { Message: string
    File: string
    Line: int
    Column: int }

type InlayHintDTO =
  { Type: string
    File: string
    Line: int
    Column: int }

type IdentifierHintDTO =
  { Name: string
    Type: string }

type DotAccessHintDTO =
  { File: string
    Line: int
    Column: int
    ObjectType: string
    AvailableFields: Map<string, string> }

type ScopeAccessHintDTO =
  { File: string
    Line: int
    Column: int
    Prefix: string
    AvailableSymbols: Map<string, string> }

type ScopeSymbolDTO =
  { Prefix: string
    Name: string
    FullName: string }

type FileBuiltEventDTO =
  { EventType: string
    File: string
    Success: bool
    Errors: BuildErrorDTO[]
    InlayHints: InlayHintDTO[]
    IdentifierHints: IdentifierHintDTO[]
    DotAccessHints: DotAccessHintDTO[]
    ScopeAccessHints: ScopeAccessHintDTO[] }

type ProjectCompleteEventDTO =
  { EventType: string
    Project: string
    TotalFiles: int
    TotalErrors: int
    InlayHints: InlayHintDTO[]
    ScopeSymbols: ScopeSymbolDTO[] }

type BuildResultDTO =
  { Success: bool
    Errors: BuildErrorDTO[]
    InlayHints: InlayHintDTO[] }

module Cli =

  let createFileOption () =
    let option = Option<string>("--file", [| "-f" |])
    option.Description <- "The .bl file to execute"
    option.Required <- true
    option

  let createConnectionOption (defaultConnectionString: string) =
    let option = Option<string>("--connection", [| "-c" |])
    option.Description <- "PostgreSQL connection string"
    option.DefaultValueFactory <- (fun _ -> defaultConnectionString)
    option

  let createShowSqlOption () =
    let option = Option<bool>("--show-sql")
    option.Description <- "Print generated SQL query logs"
    option.DefaultValueFactory <- (fun _ -> false)
    option

  let createRunOption () =
    let option = Option<bool>("--run", [| "-r" |])
    option.Description <- "Run Expr.Eval after build"
    option.DefaultValueFactory <- (fun _ -> false)
    option

  let addServerCommand (rootCommand: RootCommand) (runServerLoop: unit -> int) =
    let serverCommand =
      Command(
        "server",
        "Run as a persistent build server, reading project paths from stdin and writing JSON build results to stdout."
      )

    serverCommand.SetAction(fun _ -> runServerLoop ())
    rootCommand.Add(serverCommand)

  let addServerStreamingCommand
    (rootCommand: RootCommand)
    (runServerLoopStreaming: unit -> int)
    =
    let serverCommand =
      Command(
        "server-streaming",
        "Run as a persistent streaming build server, emitting per-file JSON events to stdout."
      )

    serverCommand.SetAction(fun _ -> runServerLoopStreaming ())
    rootCommand.Add(serverCommand)

module BuildServer =

  let projectFromPath
    (path: string)
    : Sum<ProjectBuildConfiguration, Errors<Location>> =
    if path.EndsWith ".blproj" then
      let projectDir =
        System.IO.Path.GetDirectoryName path
        |> Option.ofObj
        |> Option.defaultValue "."

      ProjectBuildConfiguration.FromProjectFile(path, projectDir)
    elif path.EndsWith ".bl" then
      ProjectBuildConfiguration.FromSingleFile path
    else
      Errors.Singleton Location.Unknown (fun () ->
        $"Unsupported file type: {path}")
      |> Sum.Right

  let buildResultFromPath
    (buildProjectOnly:
      ProjectBuildConfiguration -> Sum<InlayHintDTO[], Errors<Location>>)
    (path: string)
    : BuildResultDTO =
    match projectFromPath path with
    | Left project ->
      match buildProjectOnly project with
      | Left inlayHints ->
        { Success = true
          Errors = [||]
          InlayHints = inlayHints }
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

        { Success = false
          Errors = errorDtos
          InlayHints = [||] }
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

      { Success = false
        Errors = errorDtos
        InlayHints = [||] }

  let inlayHintDtosFromTypeCheckState
    (state: TypeCheckState<'valueExt>)
    : InlayHintDTO[] =
    state.InlayHints
    |> Map.toList
    |> List.sortBy (fun (loc, _) -> loc.File, loc.Line, loc.Column)
    |> List.map (fun (loc, hint) ->
      { Type = hint.Type.ToInlayString()
        File = loc.File
        Line = loc.Line
        Column = loc.Column })
    |> List.toArray

  let inlayHintDtosForFile
    (state: TypeCheckState<'valueExt>)
    (fileName: string)
    : InlayHintDTO[] =
    state.InlayHints
    |> Map.toList
    |> List.filter (fun (loc, _) -> loc.File = fileName)
    |> List.sortBy (fun (loc, _) -> loc.Line, loc.Column)
    |> List.map (fun (loc, hint) ->
      { Type = hint.Type.ToInlayString()
        File = loc.File
        Line = loc.Line
        Column = loc.Column })
    |> List.toArray

  let identifierHintDtosFromContext
    (ctx: TypeCheckContext<'valueExt>)
    : IdentifierHintDTO[] =
    ctx.Values
    |> Map.toList
    |> List.map (fun (rid, (tv, _kind)) ->
      { Name = rid.ToString()
        Type = tv.ToInlayString() })
    |> List.toArray

  let scopeSymbolDtosFromState
    (state: TypeCheckState<'valueExt>)
    : ScopeSymbolDTO[] =
    state.Symbols.Types
    |> Map.toList
    |> List.choose (fun (rid, _sym) ->
      match rid.Type with
      | Some prefix ->
        Some
          { Prefix = prefix
            Name = rid.Name
            FullName = rid.ToString() }
      | None -> None)
    |> List.toArray

  let dotAccessHintDtosForFile
    (state: TypeCheckState<'valueExt>)
    (fileName: string)
    : DotAccessHintDTO[] =
    state.DotAccessHints
    |> Map.toList
    |> List.filter (fun (loc, _) -> loc.File = fileName)
    |> List.sortBy (fun (loc, _) -> loc.Line, loc.Column)
    |> List.map (fun (loc, hint) ->
      { File = loc.File
        Line = loc.Line
        Column = loc.Column
        ObjectType = hint.ObjectType.ToInlayString()
        AvailableFields =
          hint.AvailableFields
          |> Map.map (fun _ tv -> tv.ToInlayString()) })
    |> List.toArray

  let dotAccessHintDtosFromState
    (state: TypeCheckState<'valueExt>)
    : DotAccessHintDTO[] =
    state.DotAccessHints
    |> Map.toList
    |> List.sortBy (fun (loc, _) -> loc.File, loc.Line, loc.Column)
    |> List.map (fun (loc, hint) ->
      { File = loc.File
        Line = loc.Line
        Column = loc.Column
        ObjectType = hint.ObjectType.ToInlayString()
        AvailableFields =
          hint.AvailableFields
          |> Map.map (fun _ tv -> tv.ToInlayString()) })
    |> List.toArray

  let scopeAccessHintDtosForFile
    (state: TypeCheckState<'valueExt>)
    (fileName: string)
    : ScopeAccessHintDTO[] =
    state.ScopeAccessHints
    |> Map.toList
    |> List.filter (fun (loc, _) -> loc.File = fileName)
    |> List.sortBy (fun (loc, _) -> loc.Line, loc.Column)
    |> List.map (fun (loc, hint) ->
      { File = loc.File
        Line = loc.Line
        Column = loc.Column
        Prefix = hint.Prefix
        AvailableSymbols = hint.AvailableSymbols })
    |> List.toArray

  let scopeAccessHintDtosFromState
    (state: TypeCheckState<'valueExt>)
    : ScopeAccessHintDTO[] =
    state.ScopeAccessHints
    |> Map.toList
    |> List.sortBy (fun (loc, _) -> loc.File, loc.Line, loc.Column)
    |> List.map (fun (loc, hint) ->
      { File = loc.File
        Line = loc.Line
        Column = loc.Column
        Prefix = hint.Prefix
        AvailableSymbols = hint.AvailableSymbols })
    |> List.toArray

  let runServerLoop (buildResultForPath: string -> BuildResultDTO) : int =
    let jsonOptions =
      JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.CamelCase)

    let savedOut = Console.Out
    let mutable line = Console.ReadLine()

    while not (isNull line) do
      let projectPath = line.Trim()

      if projectPath.Length > 0 then
        let result =
          Console.SetOut(Console.Error)

          try
            buildResultForPath projectPath
          finally
            Console.SetOut(savedOut)

        let json = JsonSerializer.Serialize(result, jsonOptions)
        Console.WriteLine(json)
        Console.Out.Flush()

      line <- Console.ReadLine()

    0

  let runServerLoopStreaming
    (buildProjectStreaming:
      string
        -> (FileBuiltEventDTO -> unit)
        -> Sum<InlayHintDTO[] * ScopeSymbolDTO[] * int * int, Errors<Location>>)
    (typecheckSingleFile:
      string
        -> string
        -> string
        -> Sum<FileBuiltEventDTO, Errors<Location>>)
    : int =
    let jsonOptions =
      JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.CamelCase)

    let savedOut = Console.Out
    let mutable line = Console.ReadLine()

    while not (isNull line) do
      let trimmed = line.Trim()

      if trimmed.Length > 0 then
        if trimmed.StartsWith("TYPECHECK\t") then
          let parts = trimmed.Split('\t')

          if parts.Length >= 4 then
            let projectPath = parts.[1]
            let filePath = parts.[2]
            let fileContent = parts.[3] |> Convert.FromBase64String |> Text.Encoding.UTF8.GetString

            Console.SetOut(Console.Error)

            try
              let result = typecheckSingleFile projectPath filePath fileContent

              Console.SetOut(savedOut)

              match result with
              | Left event ->
                let json = JsonSerializer.Serialize(event, jsonOptions)
                Console.WriteLine(json)
                Console.Out.Flush()
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

                let errorResult: BuildResultDTO =
                  { Success = false
                    Errors = errorDtos
                    InlayHints = [||] }

                let json = JsonSerializer.Serialize(errorResult, jsonOptions)
                Console.WriteLine(json)
                Console.Out.Flush()
            finally
              Console.SetOut(savedOut)
          else
            Console.SetOut(savedOut)

            let errorResult: BuildResultDTO =
              { Success = false
                Errors =
                  [| { Message = "TYPECHECK command requires: TYPECHECK\\tprojectPath\\tfilePath\\tbase64content"
                       File = "unknown"
                       Line = 1
                       Column = 1 } |]
                InlayHints = [||] }

            let json = JsonSerializer.Serialize(errorResult, jsonOptions)
            Console.WriteLine(json)
            Console.Out.Flush()
        else
          let projectPath = trimmed
          Console.SetOut(Console.Error)

          try
            let emitFileEvent (event: FileBuiltEventDTO) =
              Console.SetOut(savedOut)
              let json = JsonSerializer.Serialize(event, jsonOptions)
              Console.WriteLine(json)
              Console.Out.Flush()
              Console.SetOut(Console.Error)

            let buildResult =
              buildProjectStreaming projectPath emitFileEvent

            Console.SetOut(savedOut)

            match buildResult with
            | Left(inlayHints, scopeSymbols, totalFiles, totalErrors) ->
              let completeEvent: ProjectCompleteEventDTO =
                { EventType = "project-complete"
                  Project = projectPath
                  TotalFiles = totalFiles
                  TotalErrors = totalErrors
                  InlayHints = inlayHints
                  ScopeSymbols = scopeSymbols }

              let json =
                JsonSerializer.Serialize(completeEvent, jsonOptions)

              Console.WriteLine(json)
              Console.Out.Flush()
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

              let errorResult: BuildResultDTO =
                { Success = false
                  Errors = errorDtos
                  InlayHints = [||] }

              let json =
                JsonSerializer.Serialize(errorResult, jsonOptions)

              Console.WriteLine(json)
              Console.Out.Flush()
          finally
            Console.SetOut(savedOut)

      line <- Console.ReadLine()

    0

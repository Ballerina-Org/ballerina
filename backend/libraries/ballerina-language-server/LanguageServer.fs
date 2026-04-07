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

type BuildErrorDTO =
  { Message: string
    File: string
    Line: int
    Column: int }

type BuildResultDTO =
  { Success: bool
    Errors: BuildErrorDTO[] }

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

module BuildServer =

  let projectFromPath (path: string) : Sum<ProjectBuildConfiguration, Errors<Location>> =
    if path.EndsWith ".blproj" then
      let projectDir =
        System.IO.Path.GetDirectoryName path |> Option.ofObj |> Option.defaultValue "."

      ProjectBuildConfiguration.FromProjectFile(path, projectDir)
    elif path.EndsWith ".bl" then
      ProjectBuildConfiguration.FromSingleFile path
    else
      Errors.Singleton Location.Unknown (fun () -> $"Unsupported file type: {path}")
      |> Sum.Right

  let buildResultFromPath
    (buildProjectOnly: ProjectBuildConfiguration -> Sum<unit, Errors<Location>>)
    (path: string)
    : BuildResultDTO =
    match projectFromPath path with
    | Left project ->
      match buildProjectOnly project with
      | Left() -> { Success = true; Errors = [||] }
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

        { Success = false; Errors = errorDtos }
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

      { Success = false; Errors = errorDtos }

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

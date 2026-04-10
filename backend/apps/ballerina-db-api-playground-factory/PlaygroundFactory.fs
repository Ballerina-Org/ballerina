namespace Ballerina.FileDBAPIPlayground

open System.Reactive.Subjects
open Ballerina.API
open Ballerina.DSL.Next.StdLib.Extensions
open Ballerina.DSL.Next.StdLib.FileDB
open Ballerina.DSL.Next.StdLib.MutableMemoryDB
open ballerina_db_api_playground

module Factory =
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.API.MemoryDB.Model
  open Ballerina.StdLib.String
  open System.CommandLine
  open System
  open Ballerina.Collections.Sum
  open System.IO
  open Ballerina.Errors
  open Ballerina
  open Ballerina.LocalizedErrors
  open Ballerina.API.MemoryDB.API
  open Microsoft.AspNetCore.Builder
  open Microsoft.Extensions.DependencyInjection
  open System.Text.Json.Serialization
  open Ballerina.API.MemoryDB.MemoryDBAPIFactory
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Runners
  open Microsoft.AspNetCore.Http
  open Ballerina.API.MemoryDB.Utils

  let private tryReadFile path =
    sum {
      if File.Exists path then
        return File.ReadAllText path
      else
        return! sum.Throw(Errors.Singleton () (fun _ -> $"File not found at {path}."))
    }

  let private loadProjectFilesFromPath
    (path: string)
    : Sum<SchemaFileDefinition array, Errors<Location>> =
    sum {
      let! project: ProjectBuildConfiguration =
        if path.EndsWith(".blproj", StringComparison.OrdinalIgnoreCase) then
          ProjectBuildConfiguration.FromProjectFile(
            path,
            Path.GetDirectoryName(path)
          )
        elif path.EndsWith(".bl", StringComparison.OrdinalIgnoreCase) then
          ProjectBuildConfiguration.FromSingleFile(path)
        else
          Errors.Singleton Location.Unknown (fun _ ->
            $"Unsupported input file type: {path}. Expected .bl or .blproj.")
          |> Sum.Right

      return
        project.Files
        |> Seq.map (fun file ->
          { Path = file.FileName.Path
            Content = file.Content() })
        |> Seq.toArray
    }

  let private createFactory
    schemaFileConfig
    dbFileConfig
    addPermissionHookScope
    addBackgroundHookScope
    (hookInjector:
      HttpContext
        -> Updater<
          ExprEvalContext<
            FileDBRuntimeContext,
            ValueExt<FileDBRuntimeContext, MutableMemoryDB<FileDBRuntimeContext, unit>, unit>
           >
         >)
    =
    let descriptorFetcher =
      descriptorFetcherFactory
        dbFileConfig
        schemaFileConfig
        addPermissionHookScope
        addBackgroundHookScope

    { DbDescriptorFetcher = descriptorFetcher
      LanguageContextFactory =
        fun () ->
          contextFactory dbFileConfig
          |> (fun (languageContext, _) -> languageContext)
          |> sum.Return
      PermissionHookInjector = hookInjector }

  let createAndRunAPI
    (builderConfiguration: WebApplicationBuilder -> unit)
    (appConfiguration: WebApplication -> unit)
    (addPermissionHookScope: Updater<Map<ResolvedIdentifier, (TypeValue<FileDbValueExtension> * Kind)>>)
    (addBackgroundHookScope: Updater<Map<ResolvedIdentifier, (TypeValue<FileDbValueExtension> * Kind)>>)
    (permissionsHookInjector:
      HttpContext
        -> Updater<
          ExprEvalContext<
            DSL.Next.StdLib.FileDB.FileDBRuntimeContext,
            DSL.Next.StdLib.Extensions.ValueExt<
              DSL.Next.StdLib.FileDB.FileDBRuntimeContext,
              DSL.Next.StdLib.MutableMemoryDB.MutableMemoryDB<DSL.Next.StdLib.FileDB.FileDBRuntimeContext, unit>,
              unit
             >
           >
         >)
    (backgroundContextInjector:
      Updater<
        ExprEvalContext<
          FileDBRuntimeContext,
          ValueExt<FileDBRuntimeContext, MutableMemoryDB<FileDBRuntimeContext, unit>, unit>
         >
       >)
    (args: string[])
    =

    let sourceFilePathArg =
      new Option<string[]>(name = "--paths", Description = "file paths of ballerina source files.")

    let fileArg =
      new Option<string>(name = "--file", aliases = [| "-f" |], Description = "The .bl or .blproj file to publish.")

    sourceFilePathArg.AllowMultipleArgumentsPerToken <- true
    sourceFilePathArg.Arity <- ArgumentArity.OneOrMore

    let publishArg =
      new Option<bool>(name = "--publish", Description = "specify if the publication from file is published.")

    let tenantIdArg =
      new Option<Guid>(
        name = "--tenant",
        Description = "specify the tenant id used to store the given schema from file."
      )

    let schemaNameArg =
      new Option<string>(name = "--schema-name", Description = "specify the name of the schema loaded from file.")

    let schemaDirectoryArg =
      new Option<string>(name = "--schema-dir", Description = "directory for schema files.")

    let schemaExtensionArg =
      new Option<string>(name = "--schema-ext", Description = "extension for schema files.")

    let dbDirectoryArg =
      new Option<string>(name = "--db-dir", Description = "directory for database files.")

    let dbExtensionArg =
      new Option<string>(name = "--db-ext", Description = "extension for database files.")

    let showEvalArg =
      new Option<bool>(name = "--show-eval", Description = "prints the evaluation of DB::run in the terminal.")

    let rootCommand = new RootCommand "Command line root command."
    rootCommand.Add fileArg
    rootCommand.Add sourceFilePathArg
    rootCommand.Add publishArg
    rootCommand.Add schemaDirectoryArg
    rootCommand.Add schemaExtensionArg
    rootCommand.Add dbDirectoryArg
    rootCommand.Add dbExtensionArg
    rootCommand.Add tenantIdArg
    rootCommand.Add schemaNameArg
    rootCommand.Add showEvalArg


    rootCommand.SetAction(fun parseResult ->
      let run =
        sum {
          let schemaDirectory = parseResult.GetValue schemaDirectoryArg
          let schemaExtension = parseResult.GetValue schemaExtensionArg
          let dbDirectory = parseResult.GetValue dbDirectoryArg
          let dbExtension = parseResult.GetValue dbExtensionArg

          let schemaDirectory =
            match schemaDirectory with
            | null -> "schemas"
            | schemaDirectory -> schemaDirectory

          let schemaExtension =
            match schemaExtension with
            | null -> "bin"
            | schemaExtension -> schemaExtension

          let dbDirectory =
            match dbDirectory with
            | null -> "databases"
            | dbDirectory -> dbDirectory

          let dbExtension =
            match dbExtension with
            | null -> "db"
            | dbExtension -> dbExtension

          let schemaFileConfig: SchemaFileConfig =
            { SchemaDirectory = schemaDirectory
              SchemaExtension = schemaExtension }

          let dbFileConfig: DbFileConfig =
            { DbDirectory = dbDirectory
              DbExtension = dbExtension }

          let sourcePaths = parseResult.GetValue sourceFilePathArg
          let sourceFilePath = parseResult.GetValue fileArg
          let schemaStream = new ReplaySubject<SchemaId> 1

          let isPublished = parseResult.GetValue publishArg

          let inputPaths =
            [|
              if String.IsNullOrWhiteSpace sourceFilePath |> not then
                sourceFilePath

              if isNull sourcePaths |> not then
                yield! sourcePaths
            |]
            |> Array.distinct

          if inputPaths.Length > 0 then
            let tenantId = parseResult.GetRequiredValue tenantIdArg
            let schemaName = parseResult.GetRequiredValue schemaNameArg
            let showEval = parseResult.GetValue showEvalArg

            let! input_files_nested =
              inputPaths
              |> Array.map loadProjectFilesFromPath
              |> sum.All

            let input_files =
              input_files_nested
              |> List.collect Array.toList
              |> List.toArray

            let payload =
              { SchemaDefinition = input_files |> Seq.toArray
                IsDraft = not isPublished }

            let filePaths =
              input_files
              |> Array.map (fun file -> file.Path)
              |> Array.distinct
              |> Array.toList
              |> String.concat ", "

            let pluralization = if input_files.Length > 1 then "s" else ""
            Console.WriteLine $"Publishing schema from file{pluralization} {filePaths}..."

            do!
              publish
                tenantId
                schemaName
                payload
                schemaFileConfig
                dbFileConfig
                showEval
                addPermissionHookScope
                addBackgroundHookScope
                schemaStream

            Console.WriteLine "Done!"
          else
            let tenantId = parseResult.GetValue tenantIdArg
            let schemaName = parseResult.GetValue schemaNameArg

            if tenantId <> Guid.Empty && (not <| String.IsNullOrWhiteSpace schemaName) then
              schemaStream.OnNext
                { TenantId = tenantId
                  SchemaName = schemaName
                  IsDraft = not isPublished }

          let builder = WebApplication.CreateBuilder args

          builder.Services.ConfigureHttpJsonOptions(fun options ->
            options.SerializerOptions.DefaultIgnoreCondition <- JsonIgnoreCondition.WhenWritingNull)
          |> ignore

          let factory =
            createFactory
              schemaFileConfig
              dbFileConfig
              addPermissionHookScope
              addBackgroundHookScope
              permissionsHookInjector

          builder.Services.AddHostedService(fun _ ->
            new MemoryDBBackgroundJobExecutor(
              factory.DbDescriptorFetcher,
              backgroundContextInjector,
              dbFileConfig,
              (fun () -> DateTimeOffset.UtcNow),
              schemaStream
            ))

          do builderConfiguration builder

          let app = builder.Build()

          app.UseHttpsRedirection() |> ignore

          do!
            app.AddFileDbCRUDApi(
              schemaFileConfig,
              dbFileConfig,
              app.MapGroup "/",
              addPermissionHookScope,
              addBackgroundHookScope,
              factory,
              schemaStream
            )

          do appConfiguration app

          return app
        }

      match run with
      | Left app ->
        app.Run()
        0
      | Right errors ->
        do Console.WriteLine $"Errors {errors.Errors()}"

        1)

    let parseResult =
      if args.Length = 0 then
        rootCommand.Parse("--help")
      else
        rootCommand.Parse args

    parseResult.Invoke()

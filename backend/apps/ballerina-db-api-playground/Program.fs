open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Ballerina.API.MemoryDB.API
open Microsoft.Extensions.DependencyInjection
open System.Text.Json.Serialization
open Ballerina.API.MemoryDB.MemoryDBAPIFactory
open Ballerina.API.MemoryDB.Model
open Ballerina.Collections.Sum
open System.CommandLine
open Ballerina.API.MemoryDB.API
open System.IO
open Ballerina.Errors
open Ballerina
open Ballerina.LocalizedErrors

let private tryReadFile path =
  sum {
    if File.Exists path then
      return File.ReadAllText path
    else
      return! sum.Throw(Errors.Singleton () (fun _ -> $"File not found at {path}."))
  }

[<EntryPoint>]
let main args =
  let sourceFilePathArg =
    new Option<string>(name = "--path", Description = "file path of ballerina source file.")

  let publishArg =
    new Option<bool>(name = "--publish", Description = "specify if the publication from file is published.")

  let tenantIdArg =
    new Option<Guid>(name = "--tenant", Description = "specify the tenant id used to store the given schema from file.")

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

        let dbFileConfig: DatabaseFileConfig =
          { DbDirectory = dbDirectory
            DbExtension = dbExtension }

        let sourcePath = parseResult.GetValue sourceFilePathArg

        if isNull sourcePath |> not then
          let isPublished = parseResult.GetValue publishArg
          let tenantId = parseResult.GetRequiredValue tenantIdArg
          let schemaName = parseResult.GetRequiredValue schemaNameArg
          let showEval = parseResult.GetValue showEvalArg

          let! definition =
            tryReadFile sourcePath
            |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))

          let payload =
            { SchemaDefinition = definition
              IsDraft = not isPublished }

          Console.WriteLine $"Publishing schema from file {sourcePath}..."
          do! publish tenantId schemaName payload schemaFileConfig dbFileConfig showEval
          Console.WriteLine "Done!"




        let builder = WebApplication.CreateBuilder(args)

        builder.Services.ConfigureHttpJsonOptions(fun options ->
          options.SerializerOptions.DefaultIgnoreCondition <- JsonIgnoreCondition.WhenWritingNull)
        |> ignore

        let app = builder.Build()

        app.UseHttpsRedirection() |> ignore
        do! app.AddFileDbCRUDApi(schemaFileConfig, dbFileConfig, app.MapGroup "/")
        return app
      }

    match run with
    | Left app ->
      app.Run()
      0
    | Right errors ->
      Console.Error.WriteLine(errors.ToString())
      1)

  let parseResult = rootCommand.Parse args
  parseResult.Invoke()

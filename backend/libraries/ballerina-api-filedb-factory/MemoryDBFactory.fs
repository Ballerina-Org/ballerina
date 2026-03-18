namespace Ballerina.API.MemoryDB

module MemoryDBAPIFactory =
  open Ballerina.DSL.Next.StdLib.Extensions
  open Ballerina.DSL.Next.StdLib.MutableMemoryDB
  open Ballerina.Collections.Sum
  open Ballerina.API
  open System
  open Ballerina.Errors
  open Ballerina.LocalizedErrors
  open Ballerina.DSL.Next.StdLib.FileDbManager
  open Model
  open API
  open Ballerina
  open Ballerina.DSL.Next.Runners
  open Ballerina.DSL.Next.StdLib.FileDB
  open Ballerina.DSL.Next.Extensions
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Next.Terms.Eval
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.Reader.WithError
  open Ballerina.DSL.Next.StdLib.DB
  open Microsoft.AspNetCore.Builder
  open Ballerina.API.APIRegistration
  open Microsoft.AspNetCore.Routing
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.Patterns
  open Utils

  let contextFactory dbFileConfig =
    stdExtensions (fileDbOps dbFileConfig)
    |> fun (_, languageContext, querySymbols, queryTypeFactory) -> languageContext, querySymbols, queryTypeFactory

  let getSchemaVersion tenantId schemaName draft schemaFileConfig =
    sum {
      let schemaDirectory, schemaExtension =
        schemaFileConfig.SchemaDirectory, schemaFileConfig.SchemaExtension

      let fileManager: FileContentManager<Schema> =
        FileContentManager<Schema>.Create(schemaDirectory, schemaExtension, tenantId, schemaName)

      let! schema =
        fileManager.GetContent()
        |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))

      if draft then
        return!
          schema.Draft
          |> sum.OfOption(
            Errors.Singleton Location.Unknown (fun _ ->
              $"Draft not found for schema {schemaName} in tenant {tenantId}.")
          )
      else
        return!
          schema.Publications
          |> List.sortByDescending (fun publication -> publication.PublishedAt)
          |> List.tryHead
          |> sum.OfOption(
            Errors.Singleton Location.Unknown (fun _ ->
              $"Publication not found for schema {schemaName} in tenant {tenantId}.")
          )
    }

  let descriptorFetcherFactory
    (languageContext:
      LanguageContext<FileDBRuntimeContext, FileDbValueExtension, ValueExtDTO, FileDbDeltaExtension, DeltaExtDTO>)
    (schemaFileConfig: SchemaFileConfig)
    dbQuerySymbols
    queryTypeFactory
    (tenantId: Guid)
    (schemaName: string)
    (draft: bool)
    : Sum<DbDescriptor<FileDBRuntimeContext, MutableMemoryDB<FileDBRuntimeContext, unit>, unit>, Errors<Location>> =
    sum {
      let! schemaVersion = getSchemaVersion tenantId schemaName draft schemaFileConfig

      let! evalResult, typeCheckContext, typeCheckState, evalContext =
        buildSchemaDefinition
          languageContext
          dbQuerySymbols
          queryTypeFactory
          tenantId
          schemaName
          schemaVersion.Definition

      match evalResult with
      | Ext(ValueExt.ValueExt(Choice5Of7(DBExt.DBValues(DBValues.DBIO dbio))), _) ->
        return
          { DbExtension = dbio
            EvalContext = evalContext
            TypeCheckContext = typeCheckContext
            TypeCheckState = typeCheckState }
      | _ ->
        return!
          sum.Throw(
            Errors.Singleton Location.Unknown (fun _ ->
              "The evaluation did not return a database extension in descriptorFetcher")
          )
    }

  type WebApplication with
    member this.AddFileDbCRUDApi
      (schemaFileConfig: SchemaFileConfig, databaseFileConfig: DatabaseFileConfig, routeGroupBuilder: RouteGroupBuilder)
      : Sum<unit, Errors<Location>> =
      sum {
        let dbFileConfig: DbFileConfig =
          { DbDirectory = databaseFileConfig.DbDirectory
            DbExtension = databaseFileConfig.DbExtension }

        let languageContext, querySymbols, queryTypeFactory = contextFactory dbFileConfig

        let descriptorFetcher =
          descriptorFetcherFactory languageContext schemaFileConfig querySymbols queryTypeFactory

        let factory =
          { DbDescriptorFetcher = descriptorFetcher
            LanguageContextFactory =
              fun () ->
                contextFactory dbFileConfig
                |> (fun (languageContext, _, _) -> languageContext)
                |> sum.Return }

        this.MapPublish(schemaFileConfig, databaseFileConfig).MapGetSchemaVersions(schemaFileConfig)
        |> ignore

        do! routeGroupBuilder.RegisterAPIEndpoints factory
      }

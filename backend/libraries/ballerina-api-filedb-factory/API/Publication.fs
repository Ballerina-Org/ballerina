namespace Ballerina.API.MemoryDB

module API =
  open Microsoft.AspNetCore.Routing
  open Microsoft.AspNetCore.Builder
  open System
  open Microsoft.AspNetCore.Http
  open Ballerina.Collections.Sum
  open Model
  open Ballerina.Errors
  open Ballerina.DSL.Next.StdLib.FileDbManager
  open Ballerina.DSL.Next.StdLib.MutableMemoryDB
  open Ballerina.DSL.Next.StdLib.FileDB
  open Utils
  open Ballerina
  open Ballerina.LocalizedErrors
  open Ballerina.DSL.Next.StdLib.Extensions
  open Ballerina.DSL.Next.StdLib.DB
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Eval
  open Ballerina.Collections.NonEmptyList
  open Ballerina.Reader.WithError

  type SchemaAPIPayload =
    { SchemaDefinition: string
      IsDraft: bool }

  let private runMain
    (languageContext:
      DSL.Next.Extensions.Model.LanguageContext<
        FileDBRuntimeContext,
        ValueExt<FileDBRuntimeContext, MutableMemoryDB<FileDBRuntimeContext, unit>, unit>,
        ValueExtDTO,
        DeltaExt<FileDBRuntimeContext, MutableMemoryDB<FileDBRuntimeContext, unit>, unit>,
        DeltaExtDTO
       >)
    evalContext
    dbio
    showMainResult
    =
    sum {
      let mainExpr =
        Expr.Apply(
          Expr.FromValue(dbio.Main, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star),
          Expr.FromValue(dbio.SchemaAsValue, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
        )

      let! mainResult =
        Expr.Eval(NonEmptyList.prependList languageContext.TypeCheckedPreludes (NonEmptyList.OfList(mainExpr, [])))
        |> Reader.Run evalContext

      if showMainResult then
        Console.WriteLine $"EVALUATION RESULT:\n{mainResult}"
    }

  let publish tenantId schemaName payload schemaFileConfig (dbFileConfig: DatabaseFileConfig) showMainResult =
    sum {
      let schemaDirectory, schemaExtension =
        schemaFileConfig.SchemaDirectory, schemaFileConfig.SchemaExtension

      let dbFileManager: FileContentManager<MutableMemoryDB<FileDBRuntimeContext, unit>> =
        FileContentManager<MutableMemoryDB<FileDBRuntimeContext, unit>>
          .Create(dbFileConfig.DbDirectory, dbFileConfig.DbExtension, tenantId, schemaName)

      let emptyDb =
        { entities = Map.empty
          relations = Map.empty
          operations = [] }

      match!
        dbFileManager.TryReadContent()
        |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))
      with
      | None ->
        Console.WriteLine "Serialization is breaking...\n\n\n==================="

        do!
          dbFileManager.WriteContent emptyDb
          |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))
      | Some _ -> do! sum.Zero()

      let fileManager: FileContentManager<Schema> =
        FileContentManager<Schema>.Create(schemaDirectory, schemaExtension, tenantId, schemaName)

      let languageContext, dbQuerySymbols, queryTypeFactory =
        contextFactory
          { DbDirectory = dbFileConfig.DbDirectory
            DbExtension = dbFileConfig.DbExtension }

      let! evalResult, _, _, evalContext =
        buildSchemaDefinition
          languageContext
          dbQuerySymbols
          queryTypeFactory
          tenantId
          schemaName
          payload.SchemaDefinition

      match evalResult with
      | Ext(ValueExt.ValueExt(Choice5Of7(DBExt.DBValues(DBValues.DBIO dbio))), _) ->
        let! schema =
          fileManager.TryReadContent()
          |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))


        match schema with
        | None ->
          let newVersion =
            { Id = Guid.CreateVersion7()
              Definition = payload.SchemaDefinition
              Version = 1L
              PublishedAt = DateTime.UtcNow }

          let newSchema: Schema =
            if payload.IsDraft then
              { Id = Guid.CreateVersion7()
                Name = schemaName
                Tenant = tenantId
                Draft = Some newVersion
                Publications = [] }
            else
              { Id = Guid.CreateVersion7()
                Name = schemaName
                Tenant = tenantId
                Draft = None
                Publications = [ newVersion ] }

          do!
            fileManager.WriteContent newSchema
            |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))

        | Some schema ->
          if payload.IsDraft then
            match schema.Draft with
            | None ->
              let newVersion =
                { Id = Guid.CreateVersion7()
                  Definition = payload.SchemaDefinition
                  Version = 1L
                  PublishedAt = DateTime.UtcNow }

              do!
                fileManager.WriteContent { schema with Draft = Some newVersion }
                |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))

            | Some draft ->

              let newDraft =
                { draft with
                    Definition = payload.SchemaDefinition
                    Version = draft.Version + 1L
                    PublishedAt = DateTime.UtcNow }

              do!
                dbFileManager.WriteContent emptyDb
                |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))

              do!
                fileManager.WriteContent { schema with Draft = Some newDraft }
                |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))
          else
            let newVersion =
              { Id = Guid.CreateVersion7()
                Definition = payload.SchemaDefinition
                Version = 1L
                PublishedAt = DateTime.UtcNow }

            do!
              dbFileManager.WriteContent emptyDb
              |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))

            do!
              fileManager.WriteContent
                { schema with
                    Publications = newVersion :: schema.Publications }
              |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))

        do! runMain languageContext evalContext dbio showMainResult

      | _ ->
        return!
          sum.Throw(
            Errors.Singleton Location.Unknown (fun _ ->
              "The evaluation did not return a database extension in descriptorFetcher")
          )
    }

  type WebApplication with
    member app.MapPublish(schemaFileConfig: SchemaFileConfig, dbFileConfig: DatabaseFileConfig) =
      app.MapPost(
        "/publish/{tenantId}/{schemaName}",
        Func<Guid, string, SchemaAPIPayload, IResult>(fun tenantId schemaName payload ->
          match publish tenantId schemaName payload schemaFileConfig dbFileConfig false with
          | Left _ -> Results.Ok()
          | Right errors -> Results.BadRequest(errors.ToString()))
      )
      |> ignore

      app

    member app.MapGetSchemaVersions(schemaFileConfig: SchemaFileConfig) =
      app.MapGet(
        "/get-versions/{tenantId}/{schemaName}",
        Func<Guid, string, IResult>(fun tenantId schemaName ->
          let result =
            sum {
              let schemaDirectory, schemaExtension =
                schemaFileConfig.SchemaDirectory, schemaFileConfig.SchemaExtension

              let fileManager: FileContentManager<Schema> =
                FileContentManager<Schema>.Create(schemaDirectory, schemaExtension, tenantId, schemaName)

              match! fileManager.TryReadContent() with
              | None ->
                return!
                  sum.Throw(Errors.Singleton () (fun _ -> $"Schema {schemaName} not found in tenant {tenantId}."))
              | Some schema -> return SchemaDTO.FromSchema schema
            }

          match result with
          | Left schema -> Results.Ok schema
          | Right errors -> Results.BadRequest(errors.ToString()))
      )
      |> ignore

      app

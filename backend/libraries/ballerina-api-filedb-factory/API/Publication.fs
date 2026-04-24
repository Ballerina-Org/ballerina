namespace Ballerina.API.MemoryDB

module API =
  open Ballerina.DSL.Next.Types.TypeChecker
  open Microsoft.AspNetCore.Routing
  open Microsoft.AspNetCore.Builder
  open System
  open Microsoft.AspNetCore.Http
  open Ballerina.StdLib.String
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
  open Ballerina.DSL.Next.Terms.FastEval
  open Ballerina.DSL.Next.Terms.Eval
  open Ballerina.Collections.NonEmptyList
  open Ballerina.Reader.WithError
  open System.Text

  type SchemaAPIPayload =
    { SchemaDefinition: SchemaFileDefinition[]
      IsDraft: bool }

  let private runMain
    (languageContext:
      DSL.Next.Extensions.Model.LanguageContext<
        FileDBRuntimeContext,
        ValueExt<
          FileDBRuntimeContext,
          MutableMemoryDB<FileDBRuntimeContext, unit>,
          unit
         >,
        ValueExtDTO,
        DeltaExt<
          FileDBRuntimeContext,
          MutableMemoryDB<FileDBRuntimeContext, unit>,
          unit
         >,
        DeltaExtDTO
       >)
    evalContext
    dbio
    showMainResult
    =
    sum {
      let mainExpr =
        RunnableExpr.Apply(
          RunnableExpr.FromValue(
            dbio.Main,
            TypeValue.CreatePrimitive PrimitiveType.Unit,
            Kind.Star
          ),
          RunnableExpr.FromValue(
            dbio.SchemaAsValue,
            TypeValue.CreatePrimitive PrimitiveType.Unit,
            Kind.Star
          ),
          TypeValue.CreatePrimitive PrimitiveType.Unit,
          Kind.Star
        )

      let! mainResult =
        Expr.Eval(
          NonEmptyList.prependList
            languageContext.TypeCheckedPreludes
            (NonEmptyList.OfList(mainExpr, []))
        )
        |> Reader.Run evalContext

      if showMainResult then
        Console.WriteLine $"EVALUATION RESULT:\n{mainResult}"
    }

  let publish
    tenantId
    schemaName
    payload
    schemaFileConfig
    (dbFileConfig: DbFileConfig)
    showMainResult
    addPermissionHookScope
    addBackgroundHookScope
    (schemaStream: SchemaId IObserver)
    =
    sum {
      let schemaDirectory, schemaExtension, schemaDefinition =
        schemaFileConfig.SchemaDirectory,
        schemaFileConfig.SchemaExtension,
        payload.SchemaDefinition |> List.ofArray

      let dbFileManager
        : FileContentManager<MutableMemoryDB<FileDBRuntimeContext, unit>> =
        FileContentManager<MutableMemoryDB<FileDBRuntimeContext, unit>>
          .Create(
            dbFileConfig.DbDirectory,
            dbFileConfig.DbExtension,
            tenantId,
            schemaName
          )

      let emptyDb =
        { entities = Map.empty
          relations = Map.empty
          operations = []
          backgroundJobs = Map.empty }

      do compilationCache.Remove(tenantId, schemaName, payload.IsDraft)

      match!
        dbFileManager.TryReadContent()
        |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))
      with
      | None ->
        do!
          dbFileManager.WriteContent emptyDb
          |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))
      | Some _ -> do! sum.Zero()

      let fileManager: FileContentManager<Schema> =
        FileContentManager<Schema>
          .Create(schemaDirectory, schemaExtension, tenantId, schemaName)

      let languageContext, _ =
        contextFactory
          { DbDirectory = dbFileConfig.DbDirectory
            DbExtension = dbFileConfig.DbExtension }

      let! evalResult, _, _, evalContext =
        buildSchemaDefinition
          { DbDirectory = dbFileConfig.DbDirectory
            DbExtension = dbFileConfig.DbExtension }
          addPermissionHookScope
          addBackgroundHookScope
          tenantId
          schemaName
          payload.IsDraft
          schemaDefinition

      let! dbio =
        match evalResult with
        | Ext(ValueExt.VDB(DBExt.DBValues(DBValues.WebAppIO webAppData)), _) ->
          webAppData.DBIO |> sum.Return
        | Ext(ValueExt.VDB(DBExt.DBValues(DBValues.DBIO dbio)), _) ->
          dbio |> sum.Return
        | _ ->
          sum.Throw(
            Errors.Singleton Location.Unknown (fun _ ->
              "The evaluation did not return a database extension in descriptorFetcher")
          )

      let! schema =
          fileManager.TryReadContent()
          |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))

      let evalContext =
        { evalContext with
            Scope = dbio.EvalContext }


      match schema with
      | None ->
        let newVersion =
          { Id = Guid.CreateVersion7()
            Definition = schemaDefinition
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
                Definition = schemaDefinition
                Version = 1L
                PublishedAt = DateTime.UtcNow }

            do!
              fileManager.WriteContent { schema with Draft = Some newVersion }
              |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))

          | Some draft ->

            let newDraft =
              { draft with
                  Definition = schemaDefinition
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
              Definition = schemaDefinition
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

      schemaStream.OnNext
        { TenantId = tenantId
          SchemaName = schemaName
          IsDraft = payload.IsDraft }
    }

  type WebApplication with
    member app.MapPublish
      (
        schemaFileConfig: SchemaFileConfig,
        dbFileConfig: DbFileConfig,
        addPermissionHookScope,
        addBackgroundHookScope,
        schemaStream: SchemaId IObserver
      ) =
      app.MapPost(
        "/publish/{tenantId}/{schemaName}",
        Func<Guid, string, SchemaAPIPayload, IResult>
          (fun tenantId schemaName payload ->
            match
              publish
                tenantId
                schemaName
                payload
                schemaFileConfig
                dbFileConfig
                false
                addPermissionHookScope
                addBackgroundHookScope
                schemaStream
            with
            | Left _ -> Results.Ok()
            | Right errors ->
              let input_files =
                payload.SchemaDefinition
                |> Seq.map (fun def -> def.Path, def.Content)
                |> Map.ofSeq

              let acc = StringBuilder($"Build errors.\n")

              for e in (Errors<_>.FilterHighestPriorityOnly errors).Errors() do
                let source =
                  match input_files |> Map.tryFind e.Context.File with
                  | Some file -> file
                  | None -> ""

                let lines =
                  source.Split('\n')
                  |> Seq.skip (e.Context.Line - 1)
                  |> Seq.mapi (fun i line ->
                    let fmt = "000" in
                    $"{(e.Context.Line + i).ToString(fmt)} |   {line}")
                  |> Seq.truncate 3
                  |> Seq.toArray

                let lines = lines |> String.join "\n"

                do
                  acc.Append
                    $"  Error: {e.Message} at line {e.Context.Line}:\n"
                  |> ignore

                do acc.Append lines |> ignore

              Results.BadRequest(acc.ToString()))
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
                schemaFileConfig.SchemaDirectory,
                schemaFileConfig.SchemaExtension

              let fileManager: FileContentManager<Schema> =
                FileContentManager<Schema>
                  .Create(
                    schemaDirectory,
                    schemaExtension,
                    tenantId,
                    schemaName
                  )

              match! fileManager.TryReadContent() with
              | None ->
                return!
                  sum.Throw(
                    Errors.Singleton () (fun _ ->
                      $"Schema {schemaName} not found in tenant {tenantId}.")
                  )
              | Some schema -> return SchemaDTO.FromSchema schema
            }

          match result with
          | Left schema -> Results.Ok schema
          | Right errors -> Results.BadRequest(errors.ToString()))
      )
      |> ignore

      app

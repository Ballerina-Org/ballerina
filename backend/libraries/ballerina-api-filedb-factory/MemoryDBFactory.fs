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
  open Ballerina.DSL.Next.Terms.FastEval
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
  open Ballerina.Collections.Map
  open Microsoft.AspNetCore.Http
  open CacheCompilation

  let private mergeEvalScope
    (baseScope: ExprEvalContextScope<'valueExtension>)
    (evaluatedScope: ExprEvalContextScope<'valueExtension>)
    : ExprEvalContextScope<'valueExtension> =
    { Values =
        evaluatedScope.Values
        |> Map.merge (fun evaluatedValue _baseValue -> evaluatedValue) baseScope.Values
      Symbols =
        ExprEvalContextSymbols.Append baseScope.Symbols evaluatedScope.Symbols }

  let contextFactory dbFileConfig =
    hddcacheWithStdExtensions
      (Ballerina.DSL.Next.StdLib.String.Extension.StringTypeClass<_>.Console())
      (Ballerina.DSL.Next.StdLib.Email.Extension.EmailTypeClass<_>.Console())
      (fileDbOps dbFileConfig)
      id
      id
    |> fun (_, languageContext, typeCheckingConfig, _) ->
      languageContext, typeCheckingConfig

  let getSchemaVersion tenantId schemaName schemaFileConfig =
    sum {
      let schemaDirectory, schemaExtension =
        schemaFileConfig.SchemaDirectory, schemaFileConfig.SchemaExtension

      let fileManager: FileContentManager<Schema> =
        FileContentManager<Schema>
          .Create(schemaDirectory, schemaExtension, tenantId, schemaName)

      let! schema =
        fileManager.GetContent()
        |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))

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
    (dbFileConfig: DbFileConfig)
    (schemaFileConfig: SchemaFileConfig)
    (addPermissionHookScope:
      Updater<Map<ResolvedIdentifier, (TypeValue<FileDbValueExtension> * Kind)>>)
    (addBackgroundHookScope:
      Updater<Map<ResolvedIdentifier, (TypeValue<FileDbValueExtension> * Kind)>>)
    (tenantId: Guid)
    (schemaName: string)
    : Sum<
        DbDescriptor<
          FileDBRuntimeContext,
          MutableMemoryDB<FileDBRuntimeContext, unit>,
          unit
         >,
        Errors<Location>
       >
    =
    sum {
      let! schemaVersion =
        getSchemaVersion tenantId schemaName schemaFileConfig

      let! evalResult, typeCheckContext, typeCheckState, evalContext =
        match compilationCache.TryFind(tenantId, schemaName, false) with
        | None ->
          buildSchemaDefinition
            dbFileConfig
            addPermissionHookScope
            addBackgroundHookScope
            tenantId
            schemaName
            false
            schemaVersion.Definition
        | Some cachedCompilationContext ->
          sum.Return(
            cachedCompilationContext.EvalResult,
            cachedCompilationContext.TypeCheckContext,
            cachedCompilationContext.TypeCheckState,
            cachedCompilationContext.EvalContext
          )

      match evalResult with
      | Ext(ValueExt.VDB(DBExt.DBValues(DBValues.WebAppIO webAppData)), _) ->
        let dbio = webAppData.DBIO
        let languageContext, _ = contextFactory dbFileConfig

        return
          { DbExtension = dbio
            EvalContext =
              { evalContext with
                  Scope = mergeEvalScope evalContext.Scope dbio.EvalContext }
            TypeCheckContext = typeCheckContext
            TypeCheckState = typeCheckState
            LanguageContext = languageContext
            DataSource = None }
      | Ext(ValueExt.VDB(DBExt.DBValues(DBValues.DBIO dbio)), _) ->
        let languageContext, _ = contextFactory dbFileConfig

        return
          { DbExtension = dbio
            EvalContext =
              { evalContext with
                  Scope = mergeEvalScope evalContext.Scope dbio.EvalContext }
            TypeCheckContext = typeCheckContext
            TypeCheckState = typeCheckState
            LanguageContext = languageContext
            DataSource = None }
      | _ ->
        return!
          sum.Throw(
            Errors.Singleton Location.Unknown (fun _ ->
              "The evaluation did not return a database extension in descriptorFetcher")
          )
    }

  type WebApplication with
    member this.AddFileDbCRUDApi
      (
        schemaFileConfig: SchemaFileConfig,
        dbFileConfig: DbFileConfig,
        routeGroupBuilder: RouteGroupBuilder,
        addPermissionHookScope:
          Updater<
            Map<ResolvedIdentifier, (TypeValue<FileDbValueExtension> * Kind)>
           >,
        addBackgroundHookScope:
          Updater<
            Map<ResolvedIdentifier, (TypeValue<FileDbValueExtension> * Kind)>
           >,
        factory,
        schemaStream
      ) =
      this
        .MapPublish(
          schemaFileConfig,
          dbFileConfig,
          addPermissionHookScope,
          addBackgroundHookScope,
          schemaStream
        )
        .MapGetSchemaVersions(schemaFileConfig)
      |> ignore

      routeGroupBuilder.RegisterAPIEndpoints factory (fun _ _ ->
        sum.Throw(
          Errors.Singleton Location.Unknown (fun _ ->
            "Filtering is not supported for in-memory database backends.")
        )
      )

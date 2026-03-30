namespace Ballerina.API.MemoryDB

module Utils =
  open Ballerina.DSL.Next.StdLib.Extensions
  open Ballerina.DSL.Next.StdLib.FileDB
  open Ballerina.Collections.Sum
  open Ballerina.DSL.Next.Runners
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Next.Terms.Eval
  open Ballerina.DSL.Next.Types.Model
  open System
  open Ballerina.Reader.WithError
  open Ballerina.DSL.Next.Extensions
  open Model
  open Ballerina.Errors
  open Ballerina.LocalizedErrors
  open Ballerina.DSL.Next.Types.TypeChecker

  let contextFactory (dbFileConfig: DbFileConfig) =
    stdExtensions (Ballerina.DSL.Next.StdLib.String.Extension.StringTypeClass<_>.Console()) (fileDbOps dbFileConfig)
    |> fun (_, languageContext, typeEvalConfig) -> languageContext, typeEvalConfig

  let buildSchemaDefinition
    (languageContext:
      LanguageContext<FileDBRuntimeContext, FileDbValueExtension, ValueExtDTO, FileDbDeltaExtension, DeltaExtDTO>)
    typeEvalConfig
    (addPermissionHookScope:
      Map<ResolvedIdentifier, (TypeValue<FileDbValueExtension> * Kind)>
        -> Map<ResolvedIdentifier, (TypeValue<FileDbValueExtension> * Kind)>)
    (addBackgroundHookScope:
      Map<ResolvedIdentifier, (TypeValue<FileDbValueExtension> * Kind)>
        -> Map<ResolvedIdentifier, (TypeValue<FileDbValueExtension> * Kind)>)
    (tenantId: Guid)
    (schemaName: string)
    (schemaDefinitions: List<SchemaFileDefinition>)
    =
    sum {
      let build_cache =
        memcache (
          languageContext.TypeCheckContext
          |> TypeCheckContext.Updaters.BackgroundHooksExtraScope addBackgroundHookScope
          |> TypeCheckContext.Updaters.PermissionHooksExtraScope addPermissionHookScope,
          languageContext.TypeCheckState
        )

      let files =
        schemaDefinitions
        |> List.map (fun def -> FileBuildConfiguration.FromFile(def.Path, def.Content))

      let! firstFile =
        files
        |> List.tryHead
        |> sum.OfOption(Errors.Singleton Location.Unknown (fun _ -> "Expected at least one schema definitions."))

      let otherFiles = files |> List.skip 1
      let files = NonEmptyList.OfList(firstFile, otherFiles)
      let project: ProjectBuildConfiguration = { Files = files }

      let! NonEmptyList(expr, exprs), _, typeCheckContext, typeCheckState =
        ProjectBuildConfiguration.BuildCached typeEvalConfig build_cache project

      let runtimeContext: FileDBRuntimeContext =
        { TenantId = tenantId
          SchemaName = schemaName }

      let evalContext =
        ExprEvalContext.Empty runtimeContext |> languageContext.ExprEvalContext

      let! evalResult =
        Expr.Eval(NonEmptyList.prependList languageContext.TypeCheckedPreludes (NonEmptyList.OfList(expr, exprs)))
        |> Reader.Run evalContext

      return evalResult, typeCheckContext, typeCheckState, evalContext
    }

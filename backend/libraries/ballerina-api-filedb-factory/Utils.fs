namespace Ballerina.API.MemoryDB

module Utils =
  open Ballerina.DSL.Next.StdLib.Extensions
  open Ballerina.DSL.Next.StdLib.FileDB
  open Ballerina.DSL.Next.Terms
  open Ballerina.Collections.Sum
  open Ballerina.Collections.Map
  open Ballerina.DSL.Next.Runners
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Next.Terms.Eval
  open Ballerina.DSL.Next.Types.Model
  open System
  open Ballerina.Reader.WithError
  open Ballerina.DSL.Next.Extensions
  open Ballerina.DSL.Next.Types.TypeChecker
  open Model
  open Ballerina.Errors
  open Ballerina.LocalizedErrors
  open Ballerina.DSL.Next.Types.TypeChecker
  open CacheCompilation

  let internal compilationCache
    : CompilationCache<FileDBRuntimeContext, FileDbValueExtension> =
    CompilationCache<FileDBRuntimeContext, FileDbValueExtension>.Empty


  let contextFactory (dbFileConfig: DbFileConfig) =
    hddcacheWithStdExtensions
      (Ballerina.DSL.Next.StdLib.String.Extension.StringTypeClass<_>.Console())
      (Ballerina.DSL.Next.StdLib.Email.Extension.EmailTypeClass<_>.Console())
      (fileDbOps dbFileConfig)
      id
      id
    |> fun (_, languageContext, typeCheckingConfig, _) ->
      languageContext, typeCheckingConfig

  let buildSchemaDefinition
    (dbFileConfig: DbFileConfig)
    (addPermissionHookScope:
      Map<ResolvedIdentifier, (TypeValue<FileDbValueExtension> * Kind)>
        -> Map<ResolvedIdentifier, (TypeValue<FileDbValueExtension> * Kind)>)
    (addBackgroundHookScope:
      Map<ResolvedIdentifier, (TypeValue<FileDbValueExtension> * Kind)>
        -> Map<ResolvedIdentifier, (TypeValue<FileDbValueExtension> * Kind)>)
    (tenantId: Guid)
    (schemaName: string)
    (isDraft: bool)
    (schemaDefinitions: List<SchemaFileDefinition>)
    =
    sum {
      let _, languageContext, typeCheckingConfig, build_cache =
        hddcacheWithStdExtensions
          (Ballerina.DSL.Next.StdLib.String.Extension.StringTypeClass<_>
            .Console())
          (Ballerina.DSL.Next.StdLib.Email.Extension.EmailTypeClass<_>.Console())
          (fileDbOps dbFileConfig)
          (TypeCheckContext.Updaters.BackgroundHooksExtraScope
            addBackgroundHookScope
           >> TypeCheckContext.Updaters.PermissionHooksExtraScope
             addPermissionHookScope)
          id

      let domainName = "Bise"

      let injectedRuntimeValues =
        [ ResolvedIdentifier.Create(domainName, "CurrentUser"),
          Value.Sum(
            { Case = 1; Count = 2 },
            Value.Primitive(PrimitiveValue.Unit)
          )
          ResolvedIdentifier.Create(domainName, "CurrentOwner"),
          Value.Sum(
            { Case = 1; Count = 2 },
            Value.Primitive(PrimitiveValue.Unit)
          )
          ResolvedIdentifier.Create(domainName, "CurrentManager"),
          Value.Sum(
            { Case = 1; Count = 2 },
            Value.Primitive(PrimitiveValue.Unit)
          )
          ResolvedIdentifier.Create(domainName, "CurrentApiToken"),
          Value.Sum(
            { Case = 1; Count = 2 },
            Value.Primitive(PrimitiveValue.Unit)
          ) ]
        |> Map.ofList

      let files =
        schemaDefinitions
        |> List.map (fun def ->
          FileBuildConfiguration.FromFile(def.Path, def.Content))

      let! firstFile =
        files
        |> List.tryHead
        |> sum.OfOption(
          Errors.Singleton Location.Unknown (fun _ ->
            "Expected at least one schema definitions.")
        )

      let otherFiles = files |> List.skip 1
      let files = NonEmptyList.OfList(firstFile, otherFiles)
      let project: ProjectBuildConfiguration = { Files = files }

      let! NonEmptyList(expr, exprs), _, typeCheckContext, typeCheckState =
        ProjectBuildConfiguration.BuildCached
          typeCheckingConfig
          build_cache
          project

      let! expr = Conversion.convertExpression expr
      let! exprs = exprs |> List.map Conversion.convertExpression |> sum.All

      let runtimeContext: FileDBRuntimeContext =
        { TenantId = tenantId
          SchemaName = schemaName }

      let evalContext =
        ExprEvalContext.Empty runtimeContext
        |> languageContext.ExprEvalContext
        |> ExprEvalContext.Updaters.Values(
          Map.merge (fun _ -> id) injectedRuntimeValues
        )

      let! evalResult =
        Expr.Eval(
          NonEmptyList.prependList
            languageContext.TypeCheckedPreludes
            (NonEmptyList.OfList(expr, exprs))
        )
        |> Reader.Run evalContext

      compilationCache.Add
        (tenantId, schemaName, isDraft)
        { EvalResult = evalResult
          TypeCheckContext = typeCheckContext
          TypeCheckState = typeCheckState
          EvalContext = evalContext }

      return evalResult, typeCheckContext, typeCheckState, evalContext
    }

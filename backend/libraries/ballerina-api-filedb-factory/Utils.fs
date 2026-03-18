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

  let contextFactory (dbFileConfig: DbFileConfig) =
    stdExtensions (fileDbOps dbFileConfig)
    |> fun (_, languageContext, querySymbols, queryTypeFactory) -> languageContext, querySymbols, queryTypeFactory

  let buildSchemaDefinition
    (languageContext:
      LanguageContext<FileDBRuntimeContext, FileDbValueExtension, ValueExtDTO, FileDbDeltaExtension, DeltaExtDTO>)
    dbQuerySymbols
    queryTypeFactory
    (tenantId: Guid)
    (schemaName: string)
    (schemaDefinition: string)
    =
    sum {
      let build_cache =
        memcache (languageContext.TypeCheckContext, languageContext.TypeCheckState)


      let project: ProjectBuildConfiguration =
        { Files = NonEmptyList.OfList(FileBuildConfiguration.FromFile($"{schemaName}.bl", schemaDefinition), []) }

      let! NonEmptyList(expr, exprs), _, typeCheckContext, typeCheckState =
        ProjectBuildConfiguration.BuildCached dbQuerySymbols queryTypeFactory build_cache project

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

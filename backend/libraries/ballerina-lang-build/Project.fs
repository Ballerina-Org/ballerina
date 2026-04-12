namespace Ballerina.DSL.Next.Runners

[<AutoOpen>]
module Project =
  open ProjectModel

  type ProjectBuildConfiguration = ProjectModel.ProjectBuildConfiguration

  type FileTypeCheckedOutput<'valueExt when 'valueExt: comparison> =
    ProjectModel.FileTypeCheckedOutput<'valueExt>

  type FileName = ProjectModel.FileName
  type Checksum = ProjectModel.Checksum
  type FileBuildConfiguration = ProjectModel.FileBuildConfiguration

  type ProjectCache<'valueExt when 'valueExt: comparison> =
    ProjectModel.ProjectCache<'valueExt>

  type ProjectFileDto = ProjectModel.ProjectFileDto
  type ProjectFile = ProjectModel.ProjectFile
  type Fun<'a, 'b> = Caching.Fun<'a, 'b>

  type BuildCache<'valueExt when 'valueExt: comparison> =
    Caching.BuildCache<'valueExt>

  type BuildCacheDto<'valueExt when 'valueExt: comparison> =
    HddCache.BuildCacheDto<'valueExt>

  let abstract_build_cache<'valueExt when 'valueExt: comparison>
    (cache: BuildCache<'valueExt>)
    (
      ctx0:
        Ballerina.DSL.Next.Types.TypeChecker.Model.TypeCheckContext<'valueExt>,
      st0: Ballerina.DSL.Next.Types.TypeChecker.Model.TypeCheckState<'valueExt>
    ) : ProjectCache<'valueExt> =
    Caching.abstract_build_cache cache (ctx0, st0)

  let hddcacheWithStdExtensions<'runtimeContext, 'db when 'db: comparison>
    (stringOps:
      Ballerina.DSL.Next.StdLib.String.Extension.StringTypeClass<
        Ballerina.DSL.Next.StdLib.Extensions.ValueExt<'runtimeContext, 'db, unit>
       >)
    (emailOps:
      Ballerina.DSL.Next.StdLib.Email.Extension.EmailTypeClass<
        'runtimeContext
       >)
    (dbOps:
      Ballerina.DSL.Next.StdLib.DB.Model.DBTypeClass<
        'runtimeContext,
        'db,
        Ballerina.DSL.Next.StdLib.Extensions.ValueExt<'runtimeContext, 'db, unit>
       >)
    (updateTypeCheckContext:
      Ballerina.DSL.Next.Types.TypeChecker.Model.TypeCheckContext<
        Ballerina.DSL.Next.StdLib.Extensions.ValueExt<'runtimeContext, 'db, unit>
       >
        -> Ballerina.DSL.Next.Types.TypeChecker.Model.TypeCheckContext<
          Ballerina.DSL.Next.StdLib.Extensions.ValueExt<
            'runtimeContext,
            'db,
            unit
           >
         >)
    (updateTypeCheckState:
      Ballerina.DSL.Next.Types.TypeChecker.Model.TypeCheckState<
        Ballerina.DSL.Next.StdLib.Extensions.ValueExt<'runtimeContext, 'db, unit>
       >
        -> Ballerina.DSL.Next.Types.TypeChecker.Model.TypeCheckState<
          Ballerina.DSL.Next.StdLib.Extensions.ValueExt<
            'runtimeContext,
            'db,
            unit
           >
         >)
    =
    HddCache.hddcacheWithStdExtensions
      stringOps
      emailOps
      dbOps
      updateTypeCheckContext
      updateTypeCheckState

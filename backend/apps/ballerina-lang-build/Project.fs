namespace Ballerina.DSL.Next.Runners

[<AutoOpen>]
module Project =
  open ProjectModel

  type ProjectBuildConfiguration = ProjectModel.ProjectBuildConfiguration
  type FileTypeCheckedOutput<'valueExt when 'valueExt: comparison> = ProjectModel.FileTypeCheckedOutput<'valueExt>
  type FileName = ProjectModel.FileName
  type Checksum = ProjectModel.Checksum
  type FileBuildConfiguration = ProjectModel.FileBuildConfiguration
  type ProjectCache<'valueExt when 'valueExt: comparison> = ProjectModel.ProjectCache<'valueExt>
  type ProjectFileDto = ProjectModel.ProjectFileDto
  type ProjectFile = ProjectModel.ProjectFile
  type Fun<'a, 'b> = Caching.Fun<'a, 'b>
  type BuildCache<'valueExt when 'valueExt: comparison> = Caching.BuildCache<'valueExt>
  type BuildCacheDto<'valueExt when 'valueExt: comparison> = HddCache.BuildCacheDto<'valueExt>

  let abstract_build_cache<'valueExt when 'valueExt: comparison>
    (cache: BuildCache<'valueExt>)
    (
      ctx0: Ballerina.DSL.Next.Types.TypeChecker.Model.TypeCheckContext<'valueExt>,
      st0: Ballerina.DSL.Next.Types.TypeChecker.Model.TypeCheckState<'valueExt>
    ) : ProjectCache<'valueExt> =
    Caching.abstract_build_cache cache (ctx0, st0)

  let memcache<'valueExt when 'valueExt: comparison>
    (
      ctx0: Ballerina.DSL.Next.Types.TypeChecker.Model.TypeCheckContext<'valueExt>,
      st0: Ballerina.DSL.Next.Types.TypeChecker.Model.TypeCheckState<'valueExt>
    ) : ProjectCache<'valueExt> =
    MemCache.memcache (ctx0, st0)

  let hardDriveCache<'valueExt when 'valueExt: comparison>
    (
      ctx0: Ballerina.DSL.Next.Types.TypeChecker.Model.TypeCheckContext<'valueExt>,
      st0: Ballerina.DSL.Next.Types.TypeChecker.Model.TypeCheckState<'valueExt>
    ) : ProjectCache<'valueExt> =
    HddCache.hardDriveCache (ctx0, st0)

  let hardDriveCacheWithTypeEvalConfig<'valueExt when 'valueExt: comparison>
    (typeEvalConfig: Option<Ballerina.DSL.Next.Types.TypeChecker.Model.TypeEvalConfig<'valueExt>>)
    (
      ctx0: Ballerina.DSL.Next.Types.TypeChecker.Model.TypeCheckContext<'valueExt>,
      st0: Ballerina.DSL.Next.Types.TypeChecker.Model.TypeCheckState<'valueExt>
    ) : ProjectCache<'valueExt> =
    HddCache.hardDriveCacheWithTypeEvalConfig typeEvalConfig (ctx0, st0)

  let tryLoadTypeEvalConfig<'valueExt when 'valueExt: comparison> () =
    HddCache.tryLoadTypeEvalConfig<'valueExt> ()

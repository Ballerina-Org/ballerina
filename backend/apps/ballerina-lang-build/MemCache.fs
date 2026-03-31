namespace Ballerina.DSL.Next.Runners

module MemCache =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Expr

  let memcache<'valueExt when 'valueExt: comparison>
    (ctx0: TypeCheckContext<'valueExt>, st0: TypeCheckState<'valueExt>)
    : ProjectModel.ProjectCache<'valueExt> =
    let memcache: Caching.BuildCache<'valueExt> =
      let mutable cache
        : Map<
            ProjectModel.FileName,
            ProjectModel.Checksum *
            List<ProjectModel.Checksum> *
            Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt> *
            TypeValue<'valueExt> *
            TypeCheckContext<'valueExt> *
            TypeCheckState<'valueExt>
           > =
        Map.empty

      { TryGet =
          fun file ->
            cache
            |> Map.tryFindWithError file "build cache" (fun () -> file.Path) Location.Unknown
            |> Sum.toOption
        Set = fun k v -> cache <- cache |> Map.add k v }

    Caching.abstract_build_cache memcache (ctx0, st0)

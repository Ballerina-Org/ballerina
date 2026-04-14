namespace Ballerina.DSL.Next.Runners

module Caching =
  open Ballerina
  open System
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Expr
  open Ballerina.Collections.NonEmptyList

  type Fun<'a, 'b> = 'a -> 'b

  type BuildCache<'valueExt when 'valueExt: comparison> =
    { TryGet:
        ProjectModel.FileName
          -> Option<
            ProjectModel.Checksum *
            List<ProjectModel.Checksum> *
            TypeCheckedExpr<'valueExt> *
            TypeValue<'valueExt> *
            TypeCheckContext<'valueExt> *
            TypeCheckState<'valueExt>
           >
      Set:
        ProjectModel.FileName
          -> Fun<
            ProjectModel.Checksum *
            List<ProjectModel.Checksum> *
            TypeCheckedExpr<'valueExt> *
            TypeValue<'valueExt> *
            TypeCheckContext<'valueExt> *
            TypeCheckState<'valueExt>,
            Unit
           > }

  let abstract_build_cache<'valueExt when 'valueExt: comparison>
    (cache: BuildCache<'valueExt>)
    (ctx0: TypeCheckContext<'valueExt>, st0: TypeCheckState<'valueExt>)
    : ProjectModel.ProjectCache<'valueExt> =
    let processFile
      (typeCheck:
        ProjectModel.FileBuildConfiguration * int
          -> State<
            TypeCheckedExpr<'valueExt> * TypeValue<'valueExt>,
            Unit,
            TypeCheckContext<'valueExt> * TypeCheckState<'valueExt>,
            Errors<Location>
           >)
      (prevChecksums: List<ProjectModel.Checksum>)
      (ctx: TypeCheckContext<'valueExt>)
      (st: TypeCheckState<'valueExt>)
      (file: ProjectModel.FileBuildConfiguration, index: int)
      : Sum<
          (TypeCheckedExpr<'valueExt> * TypeValue<'valueExt>) *
          TypeCheckContext<'valueExt> *
          TypeCheckState<'valueExt>,
          Errors<Location>
         >
      =
      let cached_entry = cache.TryGet file.FileName

      let sw = System.Diagnostics.Stopwatch.StartNew()

      match cached_entry with
      | Some(checksum, checksums, expr, typeValue, ctx', st') when
        checksum = file.Checksum && prevChecksums = checksums
        ->
        sum {
          sw.Stop()

          Console.WriteLine
            $"Cache hit  for {file.FileName.Path} in {sw.ElapsedMilliseconds} ms"

          return (expr, typeValue), ctx', st'
        }
      | _ ->
        sum {
          let! (expr, typeValue), st_ctx' =
            typeCheck (file, index)
            |> State.Run((), (ctx, st))
            |> sum.MapError fst

          let ctx', st' = st_ctx' |> Option.defaultValue (ctx, st)

          do
            cache.Set
              file.FileName
              (file.Checksum, prevChecksums, expr, typeValue, ctx', st')

          sw.Stop()

          Console.WriteLine
            $"Cache miss for {file.FileName.Path} in {sw.ElapsedMilliseconds} ms"

          return (expr, typeValue), ctx', st'
        }

    { Fold =
        fun
            (files: NonEmptyList<ProjectModel.FileBuildConfiguration>)
            (typeCheck:
              ProjectModel.FileBuildConfiguration * int
                -> State<
                  TypeCheckedExpr<'valueExt> * TypeValue<'valueExt>,
                  Unit,
                  TypeCheckContext<'valueExt> * TypeCheckState<'valueExt>,
                  Errors<Location>
                 >) ->
          let filesWithOrder =
            files |> NonEmptyList.mapi (fun i file -> file, i)

          filesWithOrder
          |> NonEmptyList.fold
            (fun
                 (acc:
                   Sum<
                     NonEmptyList<
                       TypeCheckedExpr<'valueExt> * TypeValue<'valueExt>
                      > *
                     List<ProjectModel.Checksum> *
                     TypeCheckContext<'valueExt> *
                     TypeCheckState<'valueExt>,
                     Errors<Location>
                    >)
                 ((file, index): ProjectModel.FileBuildConfiguration * int) ->
              sum {
                let! prevExprs, prevChecksums, ctx, st = acc

                let! exprWithType, ctx', st' =
                  processFile typeCheck prevChecksums ctx st (file, index)

                NonEmptyList.OfList(
                  exprWithType,
                  NonEmptyList.ToList prevExprs
                ),
                file.Checksum :: prevChecksums,
                ctx',
                st'
              })
            (fun (file, index) ->
              sum {
                let! exprWithType, ctx', st' =
                  processFile typeCheck [] ctx0 st0 (file, index)

                NonEmptyList.One exprWithType, [ file.Checksum ], ctx', st'
              })
          |> sum.Map(fun (exprs, _, ctx, st) -> exprs, ctx, st)
      FoldStreaming =
        fun
            (files: NonEmptyList<ProjectModel.FileBuildConfiguration>)
            (typeCheck:
              ProjectModel.FileBuildConfiguration * int
                -> State<
                  TypeCheckedExpr<'valueExt> * TypeValue<'valueExt>,
                  Unit,
                  TypeCheckContext<'valueExt> * TypeCheckState<'valueExt>,
                  Errors<Location>
                 >)
            (onFileProcessed:
              ProjectModel.FileBuildConfiguration
                -> TypeCheckContext<'valueExt>
                -> TypeCheckState<'valueExt>
                -> unit) ->

          let processFileWithErrorCallback
            (prevChecksums: List<ProjectModel.Checksum>)
            (ctx: TypeCheckContext<'valueExt>)
            (st: TypeCheckState<'valueExt>)
            (file: ProjectModel.FileBuildConfiguration, index: int)
            =
            let cached_entry = cache.TryGet file.FileName
            let sw = System.Diagnostics.Stopwatch.StartNew()

            match cached_entry with
            | Some(checksum, checksums, expr, typeValue, ctx', st') when
              checksum = file.Checksum && prevChecksums = checksums
              ->
              sw.Stop()

              Console.WriteLine
                $"Cache hit  for {file.FileName.Path} in {sw.ElapsedMilliseconds} ms"

              onFileProcessed file ctx' st'
              Sum.Left((expr, typeValue), ctx', st')
            | _ ->
              // Create a mutable hints accumulator that survives state monad rollback.
              // The accumulator is placed in TypeCheckContext ('c parameter of the inner
              // type-checker state monad), which is NOT saved/restored by catch handlers.
              let hintsAccumulator = HintsAccumulator<'valueExt>()
              let ctxWithAccumulator =
                { ctx with HintsAccumulator = Some (hintsAccumulator :> obj) }

              let runResult =
                typeCheck (file, index)
                |> State.Run((), (ctxWithAccumulator, st))

              match runResult with
              | Sum.Left((expr, typeValue), stateOpt) ->
                let ctx', st' = stateOpt |> Option.defaultValue (ctx, st)

                do
                  cache.Set
                    file.FileName
                    (file.Checksum, prevChecksums, expr, typeValue, ctx', st')

                sw.Stop()

                Console.WriteLine
                  $"Cache miss for {file.FileName.Path} in {sw.ElapsedMilliseconds} ms"

                onFileProcessed file ctx' st'
                Sum.Left((expr, typeValue), ctx', st')
              | Sum.Right(errors, _stateOpt) ->
                // On error, recover partial hints from the accumulator.
                // The state monad's catch handlers roll back TypeCheckState,
                // but the accumulator (in the read-only context) survives.
                let st' =
                  { st with
                      DotAccessHints = hintsAccumulator.DotAccessHints
                      ScopeAccessHints = hintsAccumulator.ScopeAccessHints }
                sw.Stop()

                Console.WriteLine
                  $"Cache miss for {file.FileName.Path} in {sw.ElapsedMilliseconds} ms (error)"

                onFileProcessed file ctx st'
                Sum.Right(errors)

          let filesWithOrder =
            files |> NonEmptyList.mapi (fun i file -> file, i)

          filesWithOrder
          |> NonEmptyList.fold
            (fun
                 (acc:
                   Sum<
                     NonEmptyList<
                       TypeCheckedExpr<'valueExt> * TypeValue<'valueExt>
                      > *
                     List<ProjectModel.Checksum> *
                     TypeCheckContext<'valueExt> *
                     TypeCheckState<'valueExt>,
                     Errors<Location>
                    >)
                 ((file, index): ProjectModel.FileBuildConfiguration * int) ->
              sum {
                let! prevExprs, prevChecksums, ctx, st = acc

                let! exprWithType, ctx', st' =
                  processFileWithErrorCallback prevChecksums ctx st (file, index)

                NonEmptyList.OfList(
                  exprWithType,
                  NonEmptyList.ToList prevExprs
                ),
                file.Checksum :: prevChecksums,
                ctx',
                st'
              })
            (fun (file, index) ->
              sum {
                let! exprWithType, ctx', st' =
                  processFileWithErrorCallback [] ctx0 st0 (file, index)

                NonEmptyList.One exprWithType, [ file.Checksum ], ctx', st'
              })
          |> sum.Map(fun (exprs, _, ctx, st) -> exprs, ctx, st) }

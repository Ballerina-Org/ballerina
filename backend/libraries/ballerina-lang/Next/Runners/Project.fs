namespace Ballerina.DSL.Next.Runners

[<AutoOpen>]
module Project =
  open Ballerina.StdLib.String
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Collections.Option
  open Ballerina.LocalizedErrors
  open System
  open Ballerina.StdLib.Object
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Unification
  open Ballerina.DSL.Next.Types.TypeChecker.AdHocPolymorphicOperators
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Eval
  open Ballerina.DSL.Next.Types.TypeChecker.LiftOtherSteps
  open Ballerina.DSL.Next.Types.TypeChecker.Primitive
  open Ballerina.DSL.Next.Types.TypeChecker.Lookup
  open Ballerina.DSL.Next.Types.TypeChecker.Lambda
  open Ballerina.DSL.Next.Types.TypeChecker.Apply
  open Ballerina.DSL.Next.Types.TypeChecker.If
  open Ballerina.DSL.Next.Types.TypeChecker.Let
  open Ballerina.DSL.Next.Types.TypeChecker.RecordCons
  open Ballerina.DSL.Next.Types.TypeChecker.RecordWith
  open Ballerina.DSL.Next.Types.TypeChecker.RecordDes
  open Ballerina.DSL.Next.Types.TypeChecker.UnionDes
  open Ballerina.DSL.Next.Types.TypeChecker.SumDes
  open Ballerina.DSL.Next.Types.TypeChecker.SumCons
  open Ballerina.DSL.Next.Types.TypeChecker.TupleDes
  open Ballerina.DSL.Next.Types.TypeChecker.TupleCons
  open Ballerina.DSL.Next.Types.TypeChecker.TypeLambda
  open Ballerina.DSL.Next.Types.TypeChecker.TypeLet
  open Ballerina.DSL.Next.Types.TypeChecker.Expr
  open Ballerina.Fun
  open Ballerina.StdLib.OrderPreservingMap
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Next.Extensions
  open Ballerina.DSL.Next.StdLib.Extensions
  open Ballerina.Parser
  open Ballerina.DSL.Next.Syntax


  type ProjectBuildConfiguration = { Files: List<FileBuildConfiguration> }

  and FileName = { Path: string }
  and Checksum = { Value: string }

  and FileBuildConfiguration =
    { FileName: FileName
      Content: Unit -> string
      Checksum: Checksum }

  and ProjectCache<'valueExt> =
    { Fold:
        List<FileBuildConfiguration>
          -> (FileBuildConfiguration * int
            -> State<Expr<TypeValue, ResolvedIdentifier, 'valueExt>, Unit, TypeCheckContext * TypeCheckState, Errors>)
          -> Sum<List<Expr<TypeValue, ResolvedIdentifier, 'valueExt>> * TypeCheckContext * TypeCheckState, Errors> }


  // added to appease the autoformatter Gods
  type Fun<'a, 'b> = 'a -> 'b

  type BuildCache<'valueExt> =
    { TryGet:
        FileName * List<FileName>
          -> Option<
            Checksum *
            List<Checksum> *
            Sum<Expr<TypeValue, ResolvedIdentifier, 'valueExt> * (TypeCheckContext * TypeCheckState) option, Errors>
           >
      Set:
        FileName * List<FileName>
          -> Fun<
            Checksum *
            List<Checksum> *
            Sum<Expr<TypeValue, ResolvedIdentifier, 'valueExt> * (TypeCheckContext * TypeCheckState) option, Errors>,
            Unit
           > }

  let abstract_build_cache<'valueExt>
    (cache: BuildCache<'valueExt>)
    (ctx0: TypeCheckContext, st0: TypeCheckState)
    : ProjectCache<'valueExt> =
    { Fold =
        fun files typeCheck ->
          files
          |> Seq.mapi (fun i file -> (file, i))
          |> Seq.fold
            (fun
                 (acc:
                   Sum<
                     List<FileName * Checksum * Expr<TypeValue, ResolvedIdentifier, 'valueExt>> *
                     TypeCheckContext *
                     TypeCheckState,
                     Errors
                    >)
                 ((file, index): FileBuildConfiguration * int) ->
              sum {
                let! prev_files, ctx, st = acc
                let prev_filenames = prev_files |> List.map (fun (n, _, _) -> n)
                let prev_checksums = prev_files |> List.map (fun (_, v, _) -> v)

                let cached_entry = cache.TryGet(file.FileName, prev_filenames)

                match cached_entry with
                | Some(checksum, checksums, result) when checksum = file.Checksum && prev_checksums = checksums ->
                  do Console.WriteLine $"Cache hit for {file.FileName.Path}"
                  let! expr, st_ctx' = result
                  let ctx', st' = st_ctx' |> Option.defaultValue (ctx, st)
                  return (file.FileName, file.Checksum, expr) :: prev_files, ctx', st'
                | _ ->
                  let! expr, st_ctx' = typeCheck (file, index) |> State.Run((), (ctx, st)) |> sum.MapError fst
                  let ctx', st' = st_ctx' |> Option.defaultValue (ctx, st)

                  do
                    cache.Set
                      (file.FileName, prev_filenames)
                      (file.Checksum, prev_checksums, Left(expr, Some(ctx', st')))

                  // do Console.WriteLine $"Cache miss for {file.FileName.Path}"
                  // do Console.WriteLine $"Updated cache size: {cache.Count}"
                  // do Console.WriteLine $"Updated cache keys: {cache.Keys.AsFSharpString}"

                  return (file.FileName, file.Checksum, expr) :: prev_files, ctx', st'
              })
            (Left([], ctx0, st0))
          |> sum.Map((fun (l, c, s) -> l |> List.map (fun (_, _, v) -> v), c, s)) }

  let memcache<'valueExt> (ctx0: TypeCheckContext, st0: TypeCheckState) : ProjectCache<'valueExt> =
    let memcache: BuildCache<'valueExt> =
      let mutable cache
        : Map<
            FileName * List<FileName>,
            Checksum *
            List<Checksum> *
            Sum<Expr<TypeValue, ResolvedIdentifier, 'valueExt> * (TypeCheckContext * TypeCheckState) option, Errors>
           > =
        Map.empty

      { TryGet =
          fun (file, prev_filenames) ->
            cache
            |> Map.tryFindWithError (file, prev_filenames) "build cache" file.Path Location.Unknown
            |> Sum.toOption
        Set = fun k v -> cache <- cache |> Map.add k v }

    abstract_build_cache memcache (ctx0, st0)

  type ProjectBuildConfiguration with
    static member BuildCached<'valueExt>
      (cache: ProjectCache<'valueExt>)
      (project: ProjectBuildConfiguration)
      : Sum<List<Expr<TypeValue, ResolvedIdentifier, 'valueExt>> * TypeCheckContext * TypeCheckState, Errors> =
      sum {
        let! expressions, finalContext, finalState =
          cache.Fold project.Files (fun (file, index) ->
            state {
              let! ctx, st = state.GetState()

              let initialLocation = Location.Initial file.FileName.Path
              let parserStopwatch = System.Diagnostics.Stopwatch.StartNew()

              let! (ParserResult(actual, _)) =
                tokens
                |> Parser.Run(file.Content() |> Seq.toList, initialLocation)
                |> sum.MapError fst
                |> state.OfSum

              do parserStopwatch.Stop()

              do parserStopwatch.Start()

              let! ParserResult(program, _) =
                Parser.Expr.program ()
                |> Parser.Run(actual, initialLocation)
                |> sum.MapError fst
                |> state.OfSum

              do parserStopwatch.Stop()

              do Console.WriteLine $"Parsed {file.FileName.Path}\nin {parserStopwatch.ElapsedMilliseconds} ms"

              let typeCheckerStopwatch = System.Diagnostics.Stopwatch.StartNew()

              let! (typeCheckedExpr, typeValue, _, ctx'), st' =
                Expr.TypeCheck None program
                |> State.Run(ctx, st)
                |> sum.MapError fst
                |> state.OfSum

              do
                Console.WriteLine
                  $"Typechecked {file.FileName.Path}:\n{typeValue}\nin {typeCheckerStopwatch.ElapsedMilliseconds} ms"

              let st' = st' |> Option.defaultValue st
              do! state.SetState(replaceWith (ctx', st'))

              do typeCheckerStopwatch.Stop()

              if index <> project.Files.Length - 1 then
                do!
                  typeCheckedExpr
                  |> Expr.AsTerminatedByConstantUnit
                  |> sum.MapError(Errors.FromErrors Location.Unknown)
                  |> state.OfSum

              return typeCheckedExpr
            })

        return expressions |> List.rev, finalContext, finalState
      }

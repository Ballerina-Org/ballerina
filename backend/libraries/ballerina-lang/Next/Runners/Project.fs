namespace Ballerina.DSL.Next.Runners

[<AutoOpen>]
module Project =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Errors
  open System
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Expr
  open Ballerina.Parser
  open Ballerina.DSL.Next.Syntax
  open System.IO
  open System.Text.Json
  open System.Text.Json.Serialization
  open Ballerina.StdLib.String
  open Ballerina.Collections.NonEmptyList

  type ProjectBuildConfiguration =
    { Files: NonEmptyList<FileBuildConfiguration> }

  and FileName = { Path: string }

  and Checksum =
    { Value: string }

    static member Compute(s: string) =
      use md5 = System.Security.Cryptography.MD5.Create()
      { Checksum.Value = BitConverter.ToString(md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(s))) }


  and FileBuildConfiguration =
    { FileName: FileName
      Content: Unit -> string
      Checksum: Checksum }

    static member FromFile(fileName: string, fileContent: string) =
      { FileName = { Path = fileName }
        Content = fun () -> fileContent
        Checksum = Checksum.Compute fileContent }

  and ProjectCache<'valueExt when 'valueExt: comparison> =
    { Fold:
        NonEmptyList<FileBuildConfiguration>
          -> (FileBuildConfiguration * int
            -> State<
              Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt> * TypeValue<'valueExt>,
              Unit,
              TypeCheckContext<'valueExt> * TypeCheckState<'valueExt>,
              Errors<Location>
             >)
          -> Sum<
            NonEmptyList<Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt> * TypeValue<'valueExt>> *
            TypeCheckContext<'valueExt> *
            TypeCheckState<'valueExt>,
            Errors<Location>
           > }


  // added to appease the autoformatter Gods
  type Fun<'a, 'b> = 'a -> 'b

  type BuildCache<'valueExt when 'valueExt: comparison> =
    { TryGet:
        FileName
          -> Option<
            Checksum *
            List<Checksum> *
            Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt> *
            TypeValue<'valueExt> *
            TypeCheckContext<'valueExt> *
            TypeCheckState<'valueExt>
           >
      Set:
        FileName
          -> Fun<
            Checksum *
            List<Checksum> *
            Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt> *
            TypeValue<'valueExt> *
            TypeCheckContext<'valueExt> *
            TypeCheckState<'valueExt>,
            Unit
           > }

  let abstract_build_cache<'valueExt when 'valueExt: comparison>
    (cache: BuildCache<'valueExt>)
    (ctx0: TypeCheckContext<'valueExt>, st0: TypeCheckState<'valueExt>)
    : ProjectCache<'valueExt> =
    let processFile
      (typeCheck:
        FileBuildConfiguration * int
          -> State<
            Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt> * TypeValue<'valueExt>,
            Unit,
            TypeCheckContext<'valueExt> * TypeCheckState<'valueExt>,
            Errors<Location>
           >)
      (prevChecksums: List<Checksum>)
      (ctx: TypeCheckContext<'valueExt>)
      (st: TypeCheckState<'valueExt>)
      (file: FileBuildConfiguration, index: int)
      : Sum<
          (Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt> * TypeValue<'valueExt>) *
          TypeCheckContext<'valueExt> *
          TypeCheckState<'valueExt>,
          Errors<Location>
         >
      =
      let cached_entry = cache.TryGet file.FileName

      match cached_entry with
      | Some(checksum, checksums, expr, typeValue, ctx', st') when checksum = file.Checksum && prevChecksums = checksums ->
        Console.WriteLine $"Cache hit for {file.FileName.Path}"
        Left((expr, typeValue), ctx', st')
      | _ ->
        sum {
          let! (expr, typeValue), st_ctx' = typeCheck (file, index) |> State.Run((), (ctx, st)) |> sum.MapError fst
          let ctx', st' = st_ctx' |> Option.defaultValue (ctx, st)
          do cache.Set file.FileName (file.Checksum, prevChecksums, expr, typeValue, ctx', st')

          // do Console.WriteLine $"Cache miss for {file.FileName.Path}"
          // do Console.WriteLine $"Updated cache size: {cache.Count}"
          // do Console.WriteLine $"Updated cache keys: {cache.Keys.AsFSharpString}"

          (expr, typeValue), ctx', st'
        }

    { Fold =
        fun
            (files: NonEmptyList<FileBuildConfiguration>)
            (typeCheck:
              FileBuildConfiguration * int
                -> State<
                  Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt> * TypeValue<'valueExt>,
                  Unit,
                  TypeCheckContext<'valueExt> * TypeCheckState<'valueExt>,
                  Errors<Location>
                 >) ->
          files
          |> NonEmptyList.mapi (fun i file -> file, i)
          |> NonEmptyList.fold
            (fun
                 (acc:
                   Sum<
                     NonEmptyList<Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt> * TypeValue<'valueExt>> *
                     List<Checksum> *
                     TypeCheckContext<'valueExt> *
                     TypeCheckState<'valueExt>,
                     Errors<Location>
                    >)
                 ((file, index): FileBuildConfiguration * int) ->
              sum {
                let! prevExprs, prevChecksums, ctx, st = acc
                let! exprWithType, ctx', st' = processFile typeCheck prevChecksums ctx st (file, index)

                NonEmptyList.OfList(exprWithType, NonEmptyList.ToList prevExprs),
                file.Checksum :: prevChecksums,
                ctx',
                st'
              })
            (fun (file, index) ->
              sum {
                let! exprWithType, ctx', st' = processFile typeCheck [] ctx0 st0 (file, index)
                NonEmptyList.One exprWithType, [ file.Checksum ], ctx', st'
              })
          |> sum.Map(fun (exprs, _, ctx, st) -> exprs, ctx, st) }

  let memcache<'valueExt when 'valueExt: comparison>
    (ctx0: TypeCheckContext<'valueExt>, st0: TypeCheckState<'valueExt>)
    : ProjectCache<'valueExt> =
    let memcache: BuildCache<'valueExt> =
      let mutable cache
        : Map<
            FileName,
            Checksum *
            List<Checksum> *
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

    abstract_build_cache memcache (ctx0, st0)

  type private BuildCacheDto<'valueExt when 'valueExt: comparison> =
    { Checksum: Checksum
      PrevFilesChecksums: Checksum array
      Expr: Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt>
      TypeValue: TypeValue<'valueExt>
      Context: TypeCheckContext<'valueExt>
      State: TypeCheckState<'valueExt> }

    member this.ToDomain() =
      (this.Checksum, this.PrevFilesChecksums |> List.ofArray, this.Expr, this.TypeValue, this.Context, this.State)

    // Type check context and state are huge when serialized (~22k lines rn), so they are skipped
    static member FromDomain(checksum, prevFilesChecksums, expr, typeValue, context, state) =
      { Checksum = checksum
        PrevFilesChecksums = prevFilesChecksums |> Array.ofList
        Expr = expr
        TypeValue = typeValue
        Context = context
        State = state }

  let hardDriveCache<'valueExt when 'valueExt: equality and 'valueExt: comparison>
    (ctx0: TypeCheckContext<'valueExt>, st0: TypeCheckState<'valueExt>)
    : ProjectCache<'valueExt> =
    let options = JsonFSharpOptions.Default().ToJsonSerializerOptions()
    let cacheFolder = "build-cache"

    let buildCachePath original =
      Path.Combine(cacheFolder, Path.ChangeExtension(original, ".json"))

    let hddCache: BuildCache<'valueExt> =
      { TryGet =
          fun { Path = path } ->
            let path = buildCachePath path

            if File.Exists path then
              use content = File.OpenRead path

              try
                let dto = JsonSerializer.Deserialize<BuildCacheDto<'valueExt>>(content, options)
                Some <| dto.ToDomain()
              with ex ->
                Console.WriteLine
                  "Warning: unable to read build cache.
It could be because the cache structure is outdated, and is expected in such case."

                Console.WriteLine ex.Message.ReasonablyClamped
                None
            else
              None

        Set =
          fun { Path = path } data ->
            let path = buildCachePath path
            let dto = BuildCacheDto<_>.FromDomain data

            try
              Directory.CreateDirectory cacheFolder |> ignore
              use writer = File.Create path
              JsonSerializer.Serialize(writer, dto, options)
            with ex ->
              Console.WriteLine $"Warning: unable to write build cache: {ex.Message.ReasonablyClamped}" }

    abstract_build_cache hddCache (ctx0, st0)

  type ProjectBuildConfiguration with
    static member ParseFile<'valueExt when 'valueExt: comparison>
      (file: FileBuildConfiguration)
      : Sum<
          ParserResult<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors<Location>>,
          Errors<Location>
         >
      =
      sum {
        let initialLocation = Location.Initial file.FileName.Path
        let parserStopwatch = System.Diagnostics.Stopwatch.StartNew()

        let! ParserResult(actual, _) =
          tokens
          |> Parser.Run(file.Content() |> Seq.toList, initialLocation)
          |> sum.MapError fst

        do parserStopwatch.Stop()

        do parserStopwatch.Start()

        let! (parserResult:
          ParserResult<Expr<TypeExpr<'valueExt>, Identifier, 'valueExt>, LocalizedToken, Location, Errors<Location>>) =
          Parser.Expr.program ()
          |> Parser.Run(actual, initialLocation)
          |> sum.MapError fst

        do parserStopwatch.Stop()
        do Console.WriteLine $"Parsed {file.FileName.Path}\nin {parserStopwatch.ElapsedMilliseconds} ms"

        // match parserResult with
        // | ParserResult(program, _) -> do Console.WriteLine $"Parsed program is\n{program}\n"

        parserResult
      }

    static member BuildCached<'valueExt when 'valueExt: comparison>
      (cache: ProjectCache<'valueExt>)
      (project: ProjectBuildConfiguration)
      : Sum<
          NonEmptyList<Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt>> *
          TypeValue<'valueExt> *
          TypeCheckContext<'valueExt> *
          TypeCheckState<'valueExt>,
          Errors<Location>
         >
      =

      sum {
        let! expressionsWithTypes, finalContext, finalState =
          cache.Fold project.Files (fun (file, index) ->
            state {
              let! ParserResult(program, _) = file |> ProjectBuildConfiguration.ParseFile |> state.OfSum

              let! ctx, st = state.GetState()
              let typeCheckerStopwatch = System.Diagnostics.Stopwatch.StartNew()

              let! (typeCheckedExpr, typeValue, _, ctx'), st' =
                Expr.TypeCheck () None program
                |> State.Run(ctx, st)
                |> sum.MapError fst
                |> state.OfSum

              do
                Console.WriteLine
                  $"Typechecked {file.FileName.Path}:\n{typeValue}\nin {typeCheckerStopwatch.ElapsedMilliseconds} ms"

              let st' = st' |> Option.defaultValue st
              do! state.SetState(replaceWith (ctx', st'))

              do typeCheckerStopwatch.Stop()

              if index <> (NonEmptyList.ToList project.Files).Length - 1 then
                do!
                  typeCheckedExpr
                  |> Expr.AsTerminatedByConstantUnit
                  |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))
                  |> state.OfSum

              typeCheckedExpr, typeValue
            })

        let expressionsWithTypesReversed = expressionsWithTypes |> NonEmptyList.rev
        let expressions = expressionsWithTypesReversed |> NonEmptyList.map fst

        let lastTypeValue = expressionsWithTypes.Head |> snd

        expressions, lastTypeValue, finalContext, finalState
      }

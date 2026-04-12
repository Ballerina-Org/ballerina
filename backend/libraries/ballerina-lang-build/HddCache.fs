namespace Ballerina.DSL.Next.Runners

module HddCache =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Expr
  open Ballerina.DSL.Next.StdLib.DB.Model
  open Ballerina.DSL.Next.StdLib.Extensions
  open Ballerina.DSL.Next.StdLib.Email.Extension
  open Ballerina.DSL.Next.StdLib.String.Extension
  open Ballerina.Serialization.MessagePack
  open System.IO

  [<CLIMutable>]
  type BuildCacheDto<'valueExt when 'valueExt: comparison> =
    { Checksum: ProjectModel.Checksum
      PrevFilesChecksums: ProjectModel.Checksum array
      Expr: TypeCheckedExpr<'valueExt>
      TypeValue: TypeValue<'valueExt>
      Context: TypeCheckContext<'valueExt>
      State: TypeCheckState<'valueExt> }

    member this.ToDomain() =
      (this.Checksum,
       this.PrevFilesChecksums |> List.ofArray,
       this.Expr,
       this.TypeValue,
       this.Context,
       this.State)

    static member FromDomain
      (checksum, prevFilesChecksums, expr, typeValue, context, state)
      =
      { Checksum = checksum
        PrevFilesChecksums = prevFilesChecksums |> Array.ofList
        Expr = expr
        TypeValue = typeValue
        Context = context
        State = state }

  [<CLIMutable>]
  type BuildCacheContainerDto<'valueExt when 'valueExt: comparison> =
    { Entries: (string * byte array) array
      QueryTypeSymbol: Option<TypeSymbol>
      ListTypeSymbol: Option<TypeSymbol> }

  type PersistedCacheData<'valueExt when 'valueExt: comparison> =
    Map<
      ProjectModel.FileName,
      ProjectModel.Checksum *
      List<ProjectModel.Checksum> *
      TypeCheckedExpr<'valueExt> *
      TypeValue<'valueExt> *
      TypeCheckContext<'valueExt> *
      TypeCheckState<'valueExt>
     > *
    Option<TypeSymbol> *
    Option<TypeSymbol>

  let private loadPersistedCacheData<'valueExt when 'valueExt: comparison>
    (serializer: MessagePackSerializerAdapter)
    (cacheFilePath: string)
    : PersistedCacheData<'valueExt> =
    let decodeEntries (entries: (string * byte array) array) =
      entries
      |> Array.fold
        (fun acc (filePath, payload) ->
          match
            acc, serializer.Deserialize<BuildCacheDto<'valueExt>>(payload)
          with
          | Some cache, Left dto ->
            let fileName: ProjectModel.FileName = { Path = filePath }
            Some(cache |> Map.add fileName (dto.ToDomain()))
          | _ -> None)
        (Some Map.empty)
      |> Option.defaultValue Map.empty

    try
      if File.Exists cacheFilePath then
        let bytes = File.ReadAllBytes cacheFilePath

        match
          serializer.Deserialize<BuildCacheContainerDto<'valueExt>>(bytes)
        with
        | Left container ->
          decodeEntries container.Entries,
          container.QueryTypeSymbol,
          container.ListTypeSymbol
        | Right _ ->
          // Legacy cache format has no TypeCheckingConfig and can lead to symbol mismatches.
          // Force a clean rebuild so the next persisted cache includes the config.
          Map.empty, None, None
      else
        Map.empty, None, None
    with _ ->
      Map.empty, None, None

  let private tryDeleteCacheFile (cacheFilePath: string) =
    try
      if File.Exists cacheFilePath then
        File.Delete cacheFilePath
    with _ ->
      ()

  let private tryLoadTypeCheckingConfig<'valueExt when 'valueExt: comparison>
    (cacheFilePath: string)
    : Option<TypeCheckingConfig<'valueExt>> =
    let serializer = MessagePackSerializerAdapter()

    let _, queryTypeSymbol, listTypeSymbol =
      loadPersistedCacheData<'valueExt> serializer cacheFilePath

    let dbQueryId = Identifier.FullyQualified([ "DB" ], "Query")
    let dbQueryResolvedId = dbQueryId |> TypeCheckScope.Empty.Resolve
    let listId = Identifier.LocalScope "List" |> TypeCheckScope.Empty.Resolve

    match queryTypeSymbol, listTypeSymbol with
    | Some queryTypeSymbol, Some listTypeSymbol ->
      Some
        { QueryTypeSymbol = queryTypeSymbol
          ListTypeSymbol = listTypeSymbol
          // This config is only used to bootstrap stdExtensions with stable symbols.
          MkQueryType =
            fun s qr ->
              TypeValue.Imported
                { Id = dbQueryResolvedId
                  Sym = queryTypeSymbol
                  Parameters = []
                  Arguments = [ TypeValue.Schema s; TypeValue.QueryRow qr ] }
          MkListType =
            fun inner ->
              TypeValue.Imported
                { Id = listId
                  Sym = listTypeSymbol
                  Parameters = []
                  Arguments = [ inner ] } }
    | _ ->
      // If symbols are missing while cache file exists, assume format mismatch and invalidate.
      tryDeleteCacheFile cacheFilePath
      None

  let private hardDriveCacheWithTypeCheckingConfig<'valueExt
    when 'valueExt: comparison>
    (typeCheckingConfig: Option<TypeCheckingConfig<'valueExt>>)
    (ctx0: TypeCheckContext<'valueExt>, st0: TypeCheckState<'valueExt>)
    : ProjectModel.ProjectCache<'valueExt> =
    let serializer = MessagePackSerializerAdapter()
    let cacheFolder = ".build-cache"
    let cacheFilePath = Path.Combine(cacheFolder, "build-cache.msgpack")

    let loadCache () =
      let cache, _, _ =
        loadPersistedCacheData<'valueExt> serializer cacheFilePath

      cache

    let persistCache (cache: Map<ProjectModel.FileName, _>) =
      try
        let entries: (string * byte array) array =
          cache
          |> Map.toSeq
          |> Seq.choose (fun (fileName, data) ->
            match
              serializer.Serialize(BuildCacheDto<'valueExt>.FromDomain data)
            with
            | Left payload -> Some(fileName.Path, payload)
            | Right _ -> None)
          |> Array.ofSeq

        let container: BuildCacheContainerDto<'valueExt> =
          { Entries = entries
            QueryTypeSymbol =
              typeCheckingConfig |> Option.map (fun cfg -> cfg.QueryTypeSymbol)
            ListTypeSymbol =
              typeCheckingConfig |> Option.map (fun cfg -> cfg.ListTypeSymbol) }

        match serializer.Serialize(container) with
        | Left bytes ->
          Directory.CreateDirectory cacheFolder |> ignore
          File.WriteAllBytes(cacheFilePath, bytes)
        | Right _ -> ()
      with _ ->
        ()

    let mutable cache
      : Map<
          ProjectModel.FileName,
          ProjectModel.Checksum *
          List<ProjectModel.Checksum> *
          TypeCheckedExpr<'valueExt> *
          TypeValue<'valueExt> *
          TypeCheckContext<'valueExt> *
          TypeCheckState<'valueExt>
         > =
      loadCache ()

    let hddCache: Caching.BuildCache<'valueExt> =
      { TryGet =
          fun file ->
            cache
            |> Map.tryFindWithError
              file
              "build cache"
              (fun () -> file.Path)
              Location.Unknown
            |> Sum.toOption

        Set =
          fun key value ->
            cache <- cache |> Map.add key value
            persistCache cache }

    Caching.abstract_build_cache hddCache (ctx0, st0)

  let hddcacheWithStdExtensions<'runtimeContext, 'db when 'db: comparison>
    (stringOps: StringTypeClass<ValueExt<'runtimeContext, 'db, unit>>)
    (emailOps: EmailTypeClass<'runtimeContext>)
    (dbOps:
      DBTypeClass<'runtimeContext, 'db, ValueExt<'runtimeContext, 'db, unit>>)
    (updateTypeCheckContext:
      TypeCheckContext<ValueExt<'runtimeContext, 'db, unit>>
        -> TypeCheckContext<ValueExt<'runtimeContext, 'db, unit>>)
    (updateTypeCheckState:
      TypeCheckState<ValueExt<'runtimeContext, 'db, unit>>
        -> TypeCheckState<ValueExt<'runtimeContext, 'db, unit>>)
    =
    let cacheFilePath = Path.Combine(".build-cache", "build-cache.msgpack")

    let deserializedTypeCheckingConfig =
      tryLoadTypeCheckingConfig<ValueExt<'runtimeContext, 'db, unit>>
        cacheFilePath

    let extensions, languageContext, effectiveTypeCheckingConfig =
      match deserializedTypeCheckingConfig with
      | Some typeCheckingConfig ->
        stdExtensions<'runtimeContext, 'db>
          stringOps
          emailOps
          dbOps
          typeCheckingConfig
      | None ->
        bootstrapStdExtensions<'runtimeContext, 'db> stringOps emailOps dbOps

    let languageContext =
      { languageContext with
          TypeCheckContext =
            languageContext.TypeCheckContext |> updateTypeCheckContext
          TypeCheckState =
            languageContext.TypeCheckState |> updateTypeCheckState }

    let cache =
      hardDriveCacheWithTypeCheckingConfig
        (Some effectiveTypeCheckingConfig)
        (languageContext.TypeCheckContext, languageContext.TypeCheckState)

    extensions, languageContext, effectiveTypeCheckingConfig, cache

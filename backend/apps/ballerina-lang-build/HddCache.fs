namespace Ballerina.DSL.Next.Runners

module HddCache =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker.Expr
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
      (this.Checksum, this.PrevFilesChecksums |> List.ofArray, this.Expr, this.TypeValue, this.Context, this.State)

    static member FromDomain(checksum, prevFilesChecksums, expr, typeValue, context, state) =
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
          match acc, serializer.Deserialize<BuildCacheDto<'valueExt>>(payload) with
          | Some cache, Left dto ->
            let fileName: ProjectModel.FileName = { Path = filePath }
            Some(cache |> Map.add fileName (dto.ToDomain()))
          | _ -> None)
        (Some Map.empty)
      |> Option.defaultValue Map.empty

    try
      if File.Exists cacheFilePath then
        let bytes = File.ReadAllBytes cacheFilePath

        match serializer.Deserialize<BuildCacheContainerDto<'valueExt>>(bytes) with
        | Left container -> decodeEntries container.Entries, container.QueryTypeSymbol, container.ListTypeSymbol
        | Right _ ->
          // Legacy cache format has no TypeEvalConfig and can lead to symbol mismatches.
          // Force a clean rebuild so the next persisted cache includes the config.
          Map.empty, None, None
      else
        Map.empty, None, None
    with _ ->
      Map.empty, None, None

  let tryLoadTypeEvalConfig<'valueExt when 'valueExt: comparison> () : Option<TypeEvalConfig<'valueExt>> =
    let serializer = MessagePackSerializerAdapter()
    let cacheFilePath = Path.Combine(".build-cache", "build-cache.msgpack")

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
    | _ -> None

  let hardDriveCacheWithTypeEvalConfig<'valueExt when 'valueExt: comparison>
    (typeEvalConfig: Option<TypeEvalConfig<'valueExt>>)
    (ctx0: TypeCheckContext<'valueExt>, st0: TypeCheckState<'valueExt>)
    : ProjectModel.ProjectCache<'valueExt> =
    let serializer = MessagePackSerializerAdapter()
    let cacheFolder = ".build-cache"
    let cacheFilePath = Path.Combine(cacheFolder, "build-cache.msgpack")

    let loadCache () =
      let cache, _, _ = loadPersistedCacheData<'valueExt> serializer cacheFilePath
      cache

    let persistCache (cache: Map<ProjectModel.FileName, _>) =
      try
        let entries: (string * byte array) array =
          cache
          |> Map.toSeq
          |> Seq.choose (fun (fileName, data) ->
            match serializer.Serialize(BuildCacheDto<'valueExt>.FromDomain data) with
            | Left payload -> Some(fileName.Path, payload)
            | Right _ -> None)
          |> Array.ofSeq

        let container: BuildCacheContainerDto<'valueExt> =
          { Entries = entries
            QueryTypeSymbol = typeEvalConfig |> Option.map (fun cfg -> cfg.QueryTypeSymbol)
            ListTypeSymbol = typeEvalConfig |> Option.map (fun cfg -> cfg.ListTypeSymbol) }

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
            |> Map.tryFindWithError file "build cache" (fun () -> file.Path) Location.Unknown
            |> Sum.toOption

        Set =
          fun key value ->
            cache <- cache |> Map.add key value
            persistCache cache }

    Caching.abstract_build_cache hddCache (ctx0, st0)

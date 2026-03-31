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
      Expr: Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt>
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

  let hardDriveCache<'valueExt when 'valueExt: comparison>
    (ctx0: TypeCheckContext<'valueExt>, st0: TypeCheckState<'valueExt>)
    : ProjectModel.ProjectCache<'valueExt> =
    let serializer = MessagePackSerializerAdapter()
    let cacheFolder = ".build-cache"
    let cacheFilePath = Path.Combine(cacheFolder, "build-cache.msgpack")

    let loadCache () =
      try
        if File.Exists cacheFilePath then
          match serializer.Deserialize<(string * byte array) array>(File.ReadAllBytes cacheFilePath) with
          | Left entries ->
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
          | Right _ -> Map.empty
        else
          Map.empty
      with _ ->
        Map.empty

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

        match serializer.Serialize(entries) with
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
          Expr<TypeValue<'valueExt>, ResolvedIdentifier, 'valueExt> *
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

namespace Ballerina.VirtualFolders

open System
open System.IO
open Ballerina.VirtualFolders
open Ballerina.VirtualFolders.Model

module Operations =
  let private normalizeRelativeToRoot (segments: string list) (folderName: string) =
    match segments with
    | name :: rest when name = folderName -> rest
    | _ -> segments

  let mkFolder (parentPath: VirtualPath) (name: string) (children: VfsNode list) : VfsNode =
    Folder
      { Name = name
        Path = parentPath @ [ name ]
        Children = children
        Metadata = None }

  let mkFile (parentPath: VirtualPath) (name: string) (content: FileContent) : VfsNode =
    File
      { Name = name
        Path = parentPath @ [ name ]
        Content = content
        Metadata = BlobMetadata.From(FileContent.Empty content) }

  let private sameName (wanted: string) (c: VfsNode) =
    match c with
    | Folder f -> f.Name = wanted
    | File x -> x.Name = wanted

  let withFileSuffix (suffix: string) (path: VirtualPath) : VirtualPath =
    match List.rev path with
    | [] -> []
    | last :: restRev ->
      if String.IsNullOrEmpty suffix then
        List.rev (last :: restRev)
      else
        let baseName = Path.GetFileNameWithoutExtension last
        let ext = Path.GetExtension last

        if baseName.EndsWith(suffix, StringComparison.Ordinal) then
          List.rev (last :: restRev)
        else
          let newName = baseName + suffix + ext
          List.rev (newName :: restRev)

  let rec tryFind (path: VirtualPath) (node: VfsNode) : VfsNode option =
    match path, node with
    | [], n -> Some n
    | segments, Folder f ->
      match normalizeRelativeToRoot segments f.Name with
      | [] -> Some node
      | name :: rest ->
        f.Children
        |> List.tryPick (function
          | File fi when fi.Name = name && rest.IsEmpty -> Some(File fi)
          | Folder cf when cf.Name = name ->
            if rest.IsEmpty then
              Some(Folder cf)
            else
              tryFind rest (Folder cf)
          | _ -> None)
    | _ -> None

  let tryGetParentIfFile (path: VirtualPath) (root: VfsNode) : VirtualPath option =
    match tryFind path root with
    | Some(File _) when path.Length > 0 -> Some(List.take (path.Length - 1) path)
    | _ -> None

  let countFiles (content: VfsNode) : int =
    let rec (!) (node: VfsNode) : int =
      match node with
      | File _ -> 1
      | Folder f -> f.Children |> List.map (!) |> List.sum

    (!) content

  let rec delete (path: VirtualPath) (tree: VfsNode) : VfsNode =
    match path, tree with
    | [], _ -> tree
    | [ name ], Folder f ->
      let filtered = f.Children |> List.filter (sameName name >> not)
      Folder { f with Children = filtered }

    | name :: rest, Folder f ->
      let updatedChildren =
        f.Children
        |> List.map (fun c ->
          match c with
          | Folder cf when cf.Name = name -> delete rest (Folder cf)
          | other -> other)

      Folder { f with Children = updatedChildren }

    | _ -> tree


  let insert (fullPath: VirtualPath) (content: FileContent) (folder: VfsNode) : VfsNode =

    let rec (!) (segments: string list) (f: FolderNode) =
      match segments with
      | [] -> f
      | [ file ] ->
        let siblings =
          f.Children
          |> List.filter (function
            | File n when n.Name = file -> false
            | _ -> true)

        { f with
            Children =
              File
                { Name = file
                  Path = f.Path @ [ file ]
                  Content = content
                  Metadata = BlobMetadata.From content }
              :: siblings }

      | dir :: rest ->
        let existing, others =
          f.Children
          |> List.fold
            (fun (hit, acc) c ->
              match c, hit with
              | Folder d, None when d.Name = dir -> (Some d, acc)
              | _ -> (hit, c :: acc))
            (None, [])

        let child =
          defaultArg
            existing
            { Name = dir
              Path = f.Path @ [ dir ]
              Children = []
              Metadata = None }

        let updated = ! rest child

        { f with
            Children = Folder updated :: List.rev others }

    match folder with
    | Folder f ->
      let normalized =
        match fullPath with
        | name :: rest when name = f.Name -> rest
        | _ -> fullPath

      ! normalized f |> Folder
    | _ -> failwith "never"

  let insertNode (fullPath: VirtualPath) (node: VfsNode) (folder: VfsNode) : VfsNode =

    let rec (!) (segments: string list) (f: FolderNode) =
      match segments with
      | [] -> f
      | [ file ] ->
        let siblings =
          f.Children
          |> List.filter (function
            | File n when n.Name = file -> false
            | _ -> true)

        { f with Children = node :: siblings }

      | dir :: rest ->
        let existing, others =
          f.Children
          |> List.fold
            (fun (hit, acc) c ->
              match c, hit with
              | Folder d, None when d.Name = dir -> (Some d, acc)
              | _ -> (hit, c :: acc))
            (None, [])

        let child =
          defaultArg
            existing
            { Name = dir
              Path = f.Path @ [ dir ]
              Children = []
              Metadata = None }

        let updated = ! rest child

        { f with
            Children = Folder updated :: List.rev others }

    match folder with
    | Folder f -> ! (normalizeRelativeToRoot fullPath f.Name) f |> Folder
    | _ -> failwith "never"

  let updateFile (path: VirtualPath) (f: FileContent -> FileContent) (tree: VfsNode) : VfsNode =
    match tryFind path tree with
    | Some(File x) -> insert path (f x.Content) tree
    | _ -> tree

  let readFile (path: VirtualPath) (tree: VfsNode) : FileContent option =
    match tryFind path tree with
    | Some(File x) -> Some x.Content
    | _ -> None

  let rec tryFindFileByName (name: string) (tree: VfsNode) : FileNode option =
    match tree with
    | File f when Path.GetFileNameWithoutExtension(f.Name) = IO.Path.GetFileNameWithoutExtension(name) -> Some f
    | File _ -> None
    | Folder folder -> folder.Children |> List.tryPick (tryFindFileByName name)

  let getWellKnownFile (c: VfsNode) (wkf: WellKnowFile) =
    match wkf with
    | WellKnowFile.Schema -> tryFindFileByName "schema" c
    | WellKnowFile.Merged -> tryFindFileByName "merged" c
    | WellKnowFile.Seeds -> tryFindFileByName "seeds" c
    | WellKnowFile.Codegen -> tryFindFileByName codegenFileName c

  let moveFileIntoOwnFolder (root: VfsNode) (file: FileNode) : VfsNode =

    let parentPath =
      match List.rev file.Path with
      | _ :: rest -> List.rev rest
      | [] -> []

    let stem =
      let s = Path.GetFileNameWithoutExtension file.Name
      if String.IsNullOrWhiteSpace s then file.Name else s

    let targetPath = parentPath @ [ stem; file.Name ]

    root |> delete file.Path |> insert targetPath file.Content

module FolderNode =

  let private normalizeFilePath (name: string) (path: VirtualPath) : VirtualPath =
    match path with
    | [] -> [ name ]
    | ps when List.last ps = name -> ps
    | ps -> ps @ [ name ]

  let flatten (node: FolderNode) : seq<string * VirtualPath * FileContent> =
    let rec walk (c: VfsNode) =
      seq {
        match c with
        | File x ->
          let p' = normalizeFilePath x.Name x.Path
          yield (x.Name, p', x.Content)
        | Folder f ->
          for ch in f.Children do
            yield! walk ch
      }

    seq {
      for ch in node.Children do
        yield! walk ch
    }

  let unflatten (rootName: string) (files: seq<string * VirtualPath * FileContent>) : VfsNode =
    let root =
      Folder
        { Name = rootName
          Path = []
          Children = []
          Metadata = None }

    files
    |> Seq.fold
      (fun acc (name, path, json) ->
        let fullPath = normalizeFilePath name path
        Operations.insert fullPath json acc)
      root

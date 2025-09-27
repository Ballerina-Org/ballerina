namespace Ballerina.Data.VirtualFolders

open FSharp.Data
open System.Text

module VirtualFolders =
  let init () =
    { Name = "root"
      Path = []
      IsLeaf = true
      Children =
        topLevelKeys
        |> List.map (fun key ->
          File
            { Name = key
              Path = [ key ]
              Content = JsonValue.Record [| key, JsonValue.Record [||] |]
              Size = 0 }) }

  let utf8SizeBytes (j: JsonValue) =
    let s = j.ToString(JsonSaveOptions.DisableFormatting)
    Encoding.UTF8.GetByteCount s

  let countFiles (content: Content) : int =
    let rec (!) (node: Content) : int =
      match node with
      | File _ -> 1
      | Folder f -> f.Children |> List.map (!) |> List.sum

    (!) content

  let mkFolder (parentPath: Path) (name: string) (children: Content list) : Content =
    Folder
      { Name = name
        Path = parentPath @ [ name ]
        Children = children
        IsLeaf = children |> List.forall _.IsFile }

  let mkFile (parentPath: Path) (name: string) (content: JsonValue) : Content =
    File
      { Name = name
        Path = parentPath @ [ name ]
        Content = content
        Size = utf8SizeBytes content }

  let private sameName (wanted: string) (c: Content) =
    match c with
    | Folder f -> f.Name = wanted
    | File x -> x.Name = wanted

  let rec tryFind (path: Path) (tree: Content) : Content option =
    match path, tree with
    | [], _ -> Some tree
    | name :: rest, Folder f ->
      f.Children
      |> List.tryPick (fun child ->
        match child with
        | Folder cf when cf.Name = name -> if rest.IsEmpty then Some child else tryFind rest child
        | File n when n.Name = name && rest.IsEmpty -> Some child
        | _ -> None)
    | _ -> None

  let rec delete (path: Path) (tree: Content) : Content =
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

  let insert (fullPath: Path) (json: JsonValue) (target: Content) : Content =
    let rec (!) segments folder =
      match segments with
      | [] -> folder
      | [ file ] ->
        let children =
          folder.Children
          |> List.filter (function
            | File f when f.Name = file -> false
            | _ -> true)

        let newFile = mkFile (folder.Path @ [ file ]) file json

        { folder with
            Children = newFile :: children }

      | dir :: rest ->
        let matching, others =
          folder.Children
          |> List.partition (function
            | Folder d when d.Name = dir -> true
            | _ -> false)

        let child =
          match matching with
          | Folder d :: _ -> d
          | _ ->
            { Name = dir
              Path = folder.Path @ [ dir ]
              Children = []
              IsLeaf = true }

        let updated = (!) rest child

        { folder with
            Children = Folder updated :: others }

    match target with
    | Folder f -> (!) fullPath f |> Folder
    | _ -> failwith "Cannot insert into a file"


  let updateFile (path: Path) (f: JsonValue -> JsonValue) (tree: Content) : Content =
    match tryFind path tree with
    | Some(File x) -> insert path (f x.Content) tree
    | _ -> tree

  let readFile (path: Path) (tree: Content) : JsonValue option =
    match tryFind path tree with
    | Some(File x) -> Some x.Content
    | _ -> None

  let mapRoot (f: Content -> Content) (root: Content) : Content =
    match f root with
    | Folder newRoot -> Folder newRoot
    | File _ -> root

  let rec tryFindFileByName (name: string) (tree: Content) : JsonValue option =
    match tree with
    | File f when f.Name = name -> Some f.Content
    | File _ -> None
    | Folder folder -> folder.Children |> List.tryPick (fun child -> tryFindFileByName name child)

  let getWellKnownFile (c: Content) (wkf: WellKnowFile) =
    match wkf with
    | Merged -> tryFindFileByName "merged.json" c
    | Seeds -> tryFindFileByName "seeds.json" c
    | Config -> tryFindFileByName "codegen.json" c

module FolderNode =

  let private normalizeFilePath (name: string) (path: Path) : Path =
    match path with
    | [] -> [ name ]
    | ps when List.last ps = name -> ps
    | ps -> ps @ [ name ]

  let flatten (node: FolderNode) : seq<string * Path * JsonValue> =
    let rec walk (c: Content) =
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

  let unflatten (rootName: string) (files: seq<string * Path * JsonValue>) : Content =
    let root =
      Folder
        { Name = rootName
          Path = []
          Children = []
          IsLeaf = true }

    files
    |> Seq.fold
      (fun acc (name, path, json) ->
        let fullPath = normalizeFilePath name path
        VirtualFolders.insert fullPath json acc)
      root

namespace Ballerina.Data.VirtualFolders

open FSharp.Data

[<AutoOpen>]
module Model =
  let topLevelKeys =
    [ "types"; "forms"; "apis"; "launchers"; "typesV2"; "schema"; "codegen" ] //seeds & merged ?

  type Path = string list

  type JsonAtPath = { Path: Path; Content: JsonValue }

  type FolderNode =
    { Name: string
      IsLeaf: bool
      Path: Path
      Children: Content list }

  and FileNode =
    { Name: string
      Size: int
      Path: Path
      Content: JsonValue }

  and Content =
    | Folder of FolderNode
    | File of FileNode

  type WellKnowFile =
    | Merged
    | Config
    | Seeds

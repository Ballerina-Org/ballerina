namespace Ballerina.VirtualFolders

open FSharp.Data
open Ballerina.VirtualFolders.Model

[<AutoOpen>]
module Extractors =
  type WellKnowFile =
    | Merged
    | Schema
    | Seeds
    | Codegen

  let topLevelKeys = [ "types"; "forms"; "apis"; "launchers"; "schema" ]

  let codegenFileName = "go-config"

  type JsonAtPath =
    { Path: VirtualPath
      Content: JsonValue }

  type NodeType =
    | FileNode
    | FolderNode

  type NodeType with
    static member Parse: string -> NodeType Option =
      function
      | "dir" -> Some FolderNode
      | "file" -> Some FileNode
      | _unsupported -> None

  let init (contentType: FileContent) =
    { Name = "root"
      Path = []
      Children =
        topLevelKeys
        |> List.map (fun key ->
          File
            { Name = key
              Path = [ key ]
              Content = FileContent.Emptify key contentType
              Metadata = BlobMetadata.From(FileContent.Emptify key contentType) })
        |> List.append
          [ File
              { Name = codegenFileName
                Path = [ codegenFileName ]
                Content = FileContent.Emptify "codegen" contentType
                Metadata = BlobMetadata.From(FileContent.Emptify "codegen" contentType) } ]
      Metadata = None }

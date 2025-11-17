namespace Ballerina.VirtualFolders.Interactions
(*
modes of engagement for user/system behavior (UI/workspace state)
*)

open System.IO
open Ballerina.Collections.NonEmptyList
open Ballerina.Collections.Sum
open Ballerina.Errors
open Ballerina.VirtualFolders
open Ballerina.VirtualFolders.Model
open Ballerina.VirtualFolders.Operations
open Ballerina.VirtualFolders.Patterns

type FileSplit =
  | Full
  | Partial of sections: string list
  | NoSplit

type WorkspaceVariant =
  | Compose
  | Explore of split: FileSplit * transientPath: VirtualPath Transient

type WorkspaceVariant with
  static member WithPath (pathOpt: VirtualPath option) (variant: WorkspaceVariant) : WorkspaceVariant =
    match variant, pathOpt with
    | Compose, _ -> Compose
    | Explore(split, _), Some path -> Explore(split, Transient.some path)
    | Explore(split, _), None -> Explore(split, Transient.none)

module Compose =
  let getFiles (vfs: FolderNode) =
    let skip = [ "merged"; "seeds"; codegenFileName ]

    FolderNode.flatten vfs
    |> Seq.filter (fun (name, _, _) -> not (List.contains (Path.GetFileNameWithoutExtension name) skip))
    |> Seq.map (fun (_, _, json) -> json)

module Explore =
  let getFiles (vfs: FolderNode, path: string list) =
    sum {
      let! inputFile =
        tryFind path (Folder vfs)
        |> sum.OfOption(Errors.Singleton $"Cannot find file: path")

      let! file =
        VfsNode.AsFile inputFile
        |> sum.MapError(fun e ->
          { Errors =
              e.Errors
              |> NonEmptyList.map (fun e ->
                { Message = e.Message
                  Priority = e.Priority }) })

      return file.Content |> List.singleton
    }

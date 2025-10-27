﻿namespace Ballerina.VirtualFolders

open System
open System.IO
open Ballerina.StdLib.String
open Ballerina.Collections.Sum
open Ballerina.Collections.NonEmptyList
open FSharp.Data
open Ballerina.StdLib.Json.Patterns
open Ballerina.Errors
open Ballerina.VirtualFolders.Patterns
open Ballerina.VirtualFolders.Model

module Json =
  type VirtualFolders() =

    static member ParseContentPath(jsonValue: JsonValue) : Sum<JsonAtPath, Errors> =
      sum {
        let! jsonMap = JsonValue.AsRecordMap jsonValue

        let! path =
          jsonMap
          |> Map.tryFindWithError "path" "virtual folders" "'path' field is required"

        let! path = JsonValue.AsString path
        let path = path.Split(string Path.AltDirectorySeparatorChar) |> List.ofArray

        let! content =
          jsonMap
          |> Map.tryFindWithError
            "content"
            "virtual folders"
            "virtual folders: 'content' field is required in file metadata"

        return { Path = path; Content = content }
      }

    static member ParseJson(jsonValue: JsonValue) : Sum<VfsNode, Errors> =
      sum {
        let! jsonMap = JsonValue.AsRecordMap jsonValue

        let! name =
          jsonMap
          |> Map.tryFindWithError "name" "virtual folders" "'name' field is required"

        let! name = JsonValue.AsString name

        let! metadata =
          jsonMap
          |> Map.tryFindWithError "metadata" "virtual folders" "virtual folders: 'metadata' field is required"

        let! metadata = JsonValue.AsRecord metadata

        let! kind =
          metadata
          |> Map.ofArray
          |> Map.tryFindWithError "kind" "virtual folders" "virtual folders: 'kind' field is required in metadata"

        let! kind = JsonValue.AsString kind

        let! kind =
          NodeType.Parse kind
          |> sum.OfOption(Errors.Singleton $"Unsupported ({kind} Node type")

        let! path =
          metadata
          |> Map.ofArray
          |> Map.tryFindWithError "path" "virtual folders" "virtual folders: 'path' field is required in metadata"

        let! path = JsonValue.AsString path
        let path = path.Split(string Path.AltDirectorySeparatorChar) |> List.ofArray

        let children = jsonMap |> Map.tryFind "children"

        match children, kind with
        | None, NodeType.FolderNode ->
          return
            Folder
              { Name = name
                Path = path
                Children = []
                Metadata = None }
        | None, NodeType.FileNode ->
          let! content =
            metadata
            |> Map.ofArray
            |> Map.tryFindWithError
              "content"
              "virtual folders"
              "virtual folders: 'content' field is required in file metadata"

          // let! size =
          //   metadata
          //   |> Map.ofArray
          //   |> Map.tryFindWithError
          //     "size"
          //     "virtual folders"
          //     "virtual folders: 'size' field is required in file metadata "
          //
          // let! size = size |> JsonValue.AsNumber

          return
            VfsNode.File
              { Name = name
                Path = path
                Content = Json content
                Metadata = None }
        | Some _, NodeType.FileNode ->
          return! sum.Throw(Errors.Singleton "virtual folders: When there are children it cannot be a file")
        | Some c, NodeType.FolderNode ->
          return!
            sum {
              let! c = JsonValue.AsArray c
              let! c = c |> Array.map VirtualFolders.ParseJson |> sum.All

              return
                Folder
                  { Name = name
                    Path = path
                    Children = c
                    Metadata = None }
            }
      }

    static member ToJson(content: VfsNode) : JsonValue =
      match content with
      | File f ->
        sum {
          let! content = FileContent.AsJson f.Content

          let fields =
            [| "name", JsonValue.String f.Name
               "metadata",
               JsonValue.Record
                 [| "kind", JsonValue.String "file"
                    "content", content
                    "path",
                    JsonValue.String(
                      f.Path
                      |> Array.ofList
                      |> fun parts -> String.Join(string Path.AltDirectorySeparatorChar, parts)
                    )
                    "size",
                    f.Metadata
                    |> Option.bind _.Size
                    |> Option.map decimal
                    |> Option.defaultValue 0.M
                    |> JsonValue.Number |] |]

          return JsonValue.Record fields
        }

        |> function
          | Left value -> value
          | Right error -> failwith (error.Errors |> NonEmptyList.ToSeq |> Seq.map _.Message |> String.JoinSeq '\n')
      | Folder f ->
        let children =
          f.Children |> List.map VirtualFolders.ToJson |> List.toArray |> JsonValue.Array

        JsonValue.Record
          [| "name", JsonValue.String f.Name
             "metadata",
             JsonValue.Record
               [| "isLeaf", JsonValue.Boolean(f.Children |> List.exists _.IsFolder |> not)
                  "kind", JsonValue.String "dir"
                  "path",
                  JsonValue.String(
                    f.Path
                    |> Array.ofList
                    |> fun parts -> String.Join(string Path.AltDirectorySeparatorChar, parts)
                  ) |]
             "children", children |]

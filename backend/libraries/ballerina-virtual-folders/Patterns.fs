namespace Ballerina.VirtualFolders

module Patterns =
  open FSharp.Data
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.VirtualFolders.Model

  type VfsNode with
    static member AsFolder(fn: VfsNode) =
      match fn with
      | Folder f -> sum.Return f
      | File _ -> sum.Throw(Errors.Singleton "Expected folder as a content, got file")

    static member AsFile(fn: VfsNode) =
      match fn with
      | File f -> sum.Return f
      | Folder _ -> sum.Throw(Errors.Singleton "Expected file as a content, got folder")

  type FileContent with
    static member AsJson(fc: FileContent) =
      match fc with
      | Json json -> sum.Return json
      | content -> sum.Throw(Errors.Singleton $"Expected JSON as a content, got {content}")

  type FileContent with
    static member AsJsonString(fc: FileContent, ?opt: JsonSaveOptions) =
      let opt = defaultArg opt JsonSaveOptions.DisableFormatting

      match fc with
      | Json json -> sum.Return(json.ToString(opt))
      | content -> sum.Throw(Errors.Singleton $"Expected JSON as a content, got {content}")

    static member TryGetJson(fc: FileContent) =
      match fc with
      | Json json -> json |> Some
      | _content -> None

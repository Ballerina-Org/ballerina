namespace Ballerina.Data.VirtualFolders

open System
open System.IO
open Ballerina.Collections.Sum
open Ballerina.DSL.Next.Types
open Ballerina.DSL.Next.Types.Json
open Ballerina.Data.Schema.Model
open Ballerina.Data.Spec.Model
open Ballerina.Reader.WithError
open FSharp.Data
open Ballerina.StdLib.Json.Patterns
open Ballerina.Errors
open Ballerina.Data.Spec.Json
open Ballerina.Data.Json.Schema

module Json =
  type VirtualFolders() =

    static member ParseV2(root: FolderNode) : Sum<V2Format, Errors> =
      sum {
        let! merged =
          VirtualFolders.getWellKnownFile (Folder root) Merged
          |> sum.OfOption(Errors.Singleton "Attempt to get merged spec failed")

        let! r = JsonValue.AsRecord merged
        let! typesV2Json = r |> Map.ofArray |> Map.tryFindWithError "typesV2" "api spec" "typesV2"
        let! schemaJson = r |> Map.ofArray |> Map.tryFindWithError "schema" "api spec" "schema"
        let! types = V2Format.FromJsonTypesV2 typesV2Json
        let! schema = Schema<TypeExpr>.FromJson schemaJson |> Reader.Run TypeExpr.FromJson
        return { TypesV2 = types; Schema = schema }
      }

    static member ToJson(content: Content) : JsonValue =
      match content with
      | File f ->
        let fields =
          [| "name", JsonValue.String f.Name
             "metadata",
             JsonValue.Record
               [| "kind", JsonValue.String "file"
                  "content", f.Content
                  "path",
                  JsonValue.String(
                    f.Path
                    |> Array.ofList
                    |> fun parts -> String.Join(string Path.AltDirectorySeparatorChar, parts)
                  )
                  "size", JsonValue.Number(decimal f.Size) |] |]

        JsonValue.Record fields
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

    static member ParseJson(jsonValue: JsonValue) : Sum<Content, Errors> =
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
          match kind with
          | "dir" -> Some Node.Folder
          | "file" -> Some Node.File
          | _unsupported -> None
          |> sum.OfOption(Errors.Singleton $"Unsupported ({kind} Node type")

        let! path =
          metadata
          |> Map.ofArray
          |> Map.tryFindWithError "path" "virtual folders" "virtual folders: 'path' field is required in metadata"

        let! path = JsonValue.AsString path
        let path = path.Split(string Path.AltDirectorySeparatorChar) |> List.ofArray

        let children = jsonMap |> Map.tryFind "children"

        match children, kind with
        | None, Node.Folder ->
          return
            Folder
              { Name = name
                Path = path
                Children = [] }
        | None, Node.File ->
          let! content =
            metadata
            |> Map.ofArray
            |> Map.tryFindWithError
              "content"
              "virtual folders"
              "virtual folders: 'content' field is required in file metadata"

          let! size =
            metadata
            |> Map.ofArray
            |> Map.tryFindWithError
              "size"
              "virtual folders"
              "virtual folders: 'size' field is required in file metadata "

          let! size = size |> JsonValue.AsNumber

          return
            Content.File
              { Name = name
                Path = path
                Content = content
                Size = int size }
        | Some _, Node.File ->
          return! sum.Throw(Errors.Singleton "virtual folders: When there are children it cannot be a file")
        | Some c, Node.Folder ->
          return!
            sum {
              let! c = JsonValue.AsArray c
              let! c = c |> Array.map VirtualFolders.ParseJson |> sum.All

              return
                Folder
                  { Name = name
                    Path = path
                    Children = c }
            }
      }

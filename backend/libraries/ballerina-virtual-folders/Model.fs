namespace Ballerina.VirtualFolders

module Model =

  open System.Text
  open FSharp.Data

  type VirtualPath = string list

  type Language =
    | Fs of code: string
    | Ballerina of code: string

  type FileContent =
    | Json of JsonValue
    | Text of string
    | Binary of byte[]
    | Markdown of string
    | Language of Language

  type VfsNode =
    | Folder of FolderNode
    | File of FileNode

  and FolderNode =
    { Name: string
      Path: VirtualPath
      Children: VfsNode list
      Metadata: BlobMetadata option }

  and FileNode =
    { Name: string
      Path: VirtualPath
      Content: FileContent
      Metadata: BlobMetadata option }

  and BlobMetadata =
    { ContentType: string option
      Encoding: string option
      CreatedAt: System.DateTimeOffset option
      ModifiedAt: System.DateTimeOffset option
      ETag: string option
      Size: int64 option
      Tags: Map<string, string> }

  type FileContent with
    static member Utf8SizeBytes(j: JsonValue) =
      j.ToString JsonSaveOptions.DisableFormatting
      |> Encoding.UTF8.GetByteCount
      |> int64

    static member Mime =
      function
      | Json _ -> "application/json"
      | Text _ -> "text/plain"
      | Markdown _ -> "text/markdown"
      | Binary _ -> "application/octet-stream"
      | Language(Fs _code) -> "text/x-fsharp"
      | Language(Ballerina _code) -> "text/ballerina"

    static member Size =
      function
      | Json content -> FileContent.Utf8SizeBytes content
      | Text s -> Encoding.UTF8.GetByteCount s |> int64
      | Markdown s -> Encoding.UTF8.GetByteCount s |> int64
      | Binary bytes -> bytes.Length |> int64
      | Language(Fs code) -> Encoding.UTF8.GetByteCount code |> int64
      | Language(Ballerina code) -> Encoding.UTF8.GetByteCount code |> int64

    static member Emptify(init: string) : FileContent -> FileContent =
      function
      | Json _ -> JsonValue.Record [| init, JsonValue.Record [||] |] |> Json
      | Text _ -> Text init
      | Markdown _ -> Markdown init
      | Binary _ -> Binary [||]
      | Language(Fs _) -> Language(Fs init)
      | Language(Ballerina _) -> Language(Ballerina init)

    static member Empty: FileContent -> FileContent =
      function
      | Json _ -> JsonValue.Record [||] |> Json
      | Text _ -> Text ""
      | Markdown _ -> Markdown ""
      | Binary _ -> Binary [||]
      | Language(Fs _) -> Language(Fs "")
      | Language(Ballerina _) -> Language(Ballerina "")

  type BlobMetadata with
    static member From(content: FileContent) =
      Some
        { ContentType = FileContent.Mime content |> Some
          Encoding = None
          CreatedAt = None
          ModifiedAt = None
          ETag = None
          Size = FileContent.Size content |> Some
          Tags = Map.empty }

  type VfsNode with
    static member RecalculateSizes(node: VfsNode) =
      match node with
      | File f ->
        File
          { f with
              Metadata = BlobMetadata.From(f.Content) }
      | Folder folder ->
        Folder
          { folder with
              Children = folder.Children |> List.map VfsNode.RecalculateSizes }

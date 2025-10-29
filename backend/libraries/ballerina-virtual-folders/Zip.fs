namespace Ballerina.VirtualFolders

open Ballerina.Errors
open Ballerina.Collections.Sum
open FSharp.Data

module Zip =

  open System.IO
  open System.IO.Compression
  open System.Text
  open FSharp.Data
  open Ballerina.VirtualFolders.Model
  open Ballerina.VirtualFolders.Patterns

  let private normalizeFolderPath (path: string) =
    if path.EndsWith("/") then path else path + "/"

  let rec private addContentToZip (archive: ZipArchive) (basePath: string) (content: VfsNode) =
    match content with
    | File file ->
      let entryPath = Path.Combine(basePath, $"{file.Name}.json").Replace("\\", "/")
      let entry = archive.CreateEntry(entryPath, CompressionLevel.Optimal)
      use stream = entry.Open()
      use writer = new StreamWriter(stream, Encoding.UTF8)
      writer.Write(file.Content.ToString())
    | Folder folder ->
      let folderPath =
        Path.Combine(basePath, folder.Name).Replace("\\", "/") |> normalizeFolderPath

      archive.CreateEntry(folderPath) |> ignore
      folder.Children |> List.iter (addContentToZip archive folderPath)

  //FIXME: use stream
  let buildZipFromFolder (root: FolderNode) : byte[] =
    use ms = new MemoryStream()

    do
      use archive = new ZipArchive(ms, ZipArchiveMode.Create, false)

      let rec addContent prefix content =
        match content with
        | File f ->
          let entryPath = if prefix = "" then f.Name else $"{prefix}/{f.Name}"
          let entry = archive.CreateEntry(entryPath)
          use entryStream = entry.Open()
          use writer = new StreamWriter(entryStream, Encoding.UTF8)
          let json = FileContent.TryGetJson f.Content
          writer.Write(json.Value)
          writer.Flush()

        | Folder folder ->
          let newPrefix =
            if prefix = "" then
              folder.Name
            else
              $"{prefix}/{folder.Name}"

          folder.Children |> List.iter (addContent newPrefix)

      root.Children |> List.iter (addContent "")

    ms.ToArray()

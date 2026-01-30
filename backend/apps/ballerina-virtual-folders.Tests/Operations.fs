module Ballerina.Data.Tests.VirtualFolders.Operations

open Ballerina
open Ballerina.Collections.Sum
open Ballerina.VirtualFolders
open Ballerina.VirtualFolders.Operations
open Ballerina.VirtualFolders.Model
open Ballerina.VirtualFolders.Patterns
open NUnit.Framework
open FSharp.Data

let toStringOption (j: JsonValue) =
  match j with
  | JsonValue.String s -> Some s
  | _ -> None

[<Test>]
let ``Update and read file`` () =
  let Content_R = JsonValue.String "R" |> Json
  let Content_X = JsonValue.String "X" |> Json
  let Content_Y = JsonValue.String "Y" |> Json

  let root =
    { Name = "Root"
      Path = []
      Metadata = None
      Children =
        [ File
            { Name = "R"
              Metadata = None
              Path = [ "R" ]
              Content = Content_R }

          Folder
            { Name = "F1"
              Path = [ "F1" ]
              Metadata = None
              Children =
                [ Folder
                    { Name = "F12"
                      Path = [ "F1"; "F12" ]
                      Metadata = None
                      Children =
                        [ File
                            { Name = "X"
                              Path = [ "F1"; "F12"; "X" ]
                              Metadata = None
                              Content = Content_X } ] }
                  File
                    { Name = "Y"
                      Metadata = None
                      Path = [ "F1"; "Y" ]
                      Content = Content_Y }

                  ] } ] }

  let state =
    updateFile [ "F1"; "F12"; "X" ] (fun _ -> JsonValue.String "X+" |> Json) (Folder root)

  let actual = readFile [ "F1"; "F12"; "X" ] state
  let expected = Some(JsonValue.String "X+" |> Json)

  Assert.That(actual, Is.EqualTo expected, "")

[<Test>]
let ``Delete file`` () =

  let root =
    { Name = "Root"
      Path = []
      Metadata = None
      Children =
        [ Folder
            { Name = "F1"
              Path = [ "F1" ]
              Metadata = None
              Children =
                [ Folder
                    { Name = "F12"
                      Path = [ "F1"; "F12" ]
                      Metadata = None
                      Children =
                        [ File
                            { Name = "X"
                              Metadata = None
                              Path = [ "F1"; "F12"; "X" ]
                              Content = Json JsonValue.Null } ] }
                  File
                    { Name = "Y"
                      Metadata = None
                      Path = [ "F1"; "Y" ]
                      Content = Json JsonValue.Null } ] } ] }
    |> Folder

  let state = delete [ "F1"; "F12"; "X" ] root
  let actual = readFile [ "F1"; "F12"; "X" ] state
  let countAfter = countFiles state

  Assert.That(actual.IsNone, Is.True, "File deleted from the hierarchy")
  Assert.That(countAfter, Is.EqualTo 1, "Files count decreased from 2 to 1")

[<Test>]
let ``Insert file`` () =

  let root =
    { Name = "Root"
      Path = []
      Metadata = None
      Children =
        [ Folder
            { Name = "F1"
              Path = [ "F1" ]
              Metadata = None
              Children =
                [ Folder
                    { Name = "F12"
                      Path = [ "F1"; "F12" ]
                      Metadata = None
                      Children = [] }
                  File
                    { Name = "Y"
                      Metadata = None
                      Path = [ "F1"; "Y" ]
                      Content = Json JsonValue.Null } ] } ] }
    |> Folder

  let state = insert [ "F1"; "F12"; "X" ] (Json JsonValue.Null) root
  let actual = readFile [ "F1"; "F12"; "X" ] state
  let countAfter = countFiles state

  Assert.That(actual.IsSome, Is.True, "File exists the hierarchy")
  Assert.That(countAfter, Is.EqualTo 2, "Files count increased from 1 to 2")

[<Test>]
let ``Try find file and folder`` () =
  let initial =
    mkFolder
      []
      "root"
      [ mkFolder
          [ "root" ]
          "a"
          [ mkFolder [ "root"; "a" ] "b1" []
            mkFolder [ "root"; "a" ] "b2" [ mkFile [ "root"; "a"; "b2" ] "x" (FileContent.Text "x1") ]
            mkFolder [ "root"; "a" ] "b3" [ mkFile [ "root"; "a"; "b3" ] "x" (FileContent.Text "x2") ] ] ]

  sum {
    let file = tryFind [ "root"; "a"; "b2"; "x" ] initial
    let folder = tryFind [ "root"; "a"; "b1" ] initial
    return file, folder
  }
  |> function
    | Right e -> Assert.Fail $"unexpected error: {e}"
    | Left(file, folder) ->
      Assert.That(file.IsSome, Is.True)
      Assert.That(folder.IsSome, Is.True)

[<Test>]
let ``Find file by name when there is more than one file`` () =
  let initial =
    mkFolder
      []
      "root"
      [ mkFolder
          [ "root" ]
          "a"
          [ mkFolder [ "root"; "a" ] "b1" []
            mkFolder [ "root"; "a" ] "b2" [ mkFile [ "root"; "a"; "b2" ] "x" (FileContent.Text "x1") ]
            mkFolder [ "root"; "a" ] "b3" [ mkFile [ "root"; "a"; "b3" ] "x" (FileContent.Text "x2") ] ] ]

  sum {
    let! folder = VfsNode.AsFolder initial

    let flat =
      FolderNode.flatten folder
      |> Seq.filter (fun (name, _, _) -> name = "x")
      |> Seq.toArray

    let found = tryFindFileByName "x" initial
    return found, flat
  }
  |> function
    | Right e -> Assert.Fail $"unexpected error: {e}"
    | Left(v, flat) ->
      Assert.That(v.IsSome, Is.True)
      Assert.That(flat.Length, Is.EqualTo 2)
      Assert.That(v.Value.Content, Is.EqualTo(FileContent.Text "x1"))

[<Test>]
let ``Move file to own folder`` () =
  let initial =
    mkFolder
      []
      "root"
      [ mkFolder
          [ "root" ]
          "a"
          [ mkFolder [ "root"; "a" ] "b1" []
            mkFolder [ "root"; "a" ] "b2" [ mkFile [ "root"; "a"; "b2" ] "x" (FileContent.Text "x1") ]
            mkFolder [ "root"; "a" ] "b3" [ mkFile [ "root"; "a"; "b3" ] "x" (FileContent.Text "x2") ] ] ]

  sum {
    let found = tryFindFileByName "x" initial
    return moveFileIntoOwnFolder initial found.Value
  }
  |> function
    | Right e -> Assert.Fail $"unexpected error: {e}"
    | Left vfs ->
      let found = tryFindFileByName "x" vfs
      Assert.That(found.IsSome, Is.True)
      Assert.That(found.Value.Path, Is.EqualTo [ "root"; "a"; "b2"; "x"; "x" ])

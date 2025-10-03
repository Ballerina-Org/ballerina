module Ballerina.Data.Tests.VirtualFolders.Operations


open Ballerina.Data.VirtualFolders
open NUnit.Framework
open FSharp.Data

let toStringOption (j: JsonValue) =
  match j with
  | JsonValue.String s -> Some s
  | _ -> None

[<Test>]
let ``Update and read file`` () =
  let Content_R = JsonValue.String "R"
  let Content_X = JsonValue.String "X"
  let Content_Y = JsonValue.String "Y"

  let root =
    { Name = "Root"
      Path = []
      Children =
        [ File
            { Name = "R"
              Size = 1
              Path = [ "R" ]
              Content = Content_R }

          Folder
            { Name = "F1"
              Path = [ "F1" ]
              Children =
                [ Folder
                    { Name = "F12"
                      Path = [ "F1"; "F12" ]
                      Children =
                        [ File
                            { Name = "X"
                              Size = 1
                              Path = [ "F1"; "F12"; "X" ]
                              Content = Content_X } ] }
                  File
                    { Name = "Y"
                      Size = 1
                      Path = [ "F1"; "Y" ]
                      Content = Content_Y }

                  ] } ] }

  let state =
    VirtualFolders.updateFile [ "F1"; "F12"; "X" ] (fun _ -> JsonValue.String "X+") (Folder root)

  let actual = VirtualFolders.readFile [ "F1"; "F12"; "X" ] state
  let expected = Some(JsonValue.String "X+")

  Assert.That(actual, Is.EqualTo expected, "")

[<Test>]
let ``Delete file`` () =

  let root =
    { Name = "Root"
      Path = []
      Children =
        [ Folder
            { Name = "F1"
              Path = [ "F1" ]
              Children =
                [ Folder
                    { Name = "F12"
                      Path = [ "F1"; "F12" ]
                      Children =
                        [ File
                            { Name = "X"
                              Size = 1
                              Path = [ "F1"; "F12"; "X" ]
                              Content = JsonValue.Null } ] }
                  File
                    { Name = "Y"
                      Size = 1
                      Path = [ "F1"; "Y" ]
                      Content = JsonValue.Null } ] } ] }
    |> Folder

  let state = VirtualFolders.delete [ "F1"; "F12"; "X" ] root
  let actual = VirtualFolders.readFile [ "F1"; "F12"; "X" ] state
  let countAfter = VirtualFolders.countFiles state

  Assert.That(actual.IsNone, Is.True, "File deleted from the hierarchy")
  Assert.That(countAfter, Is.EqualTo 1, "Files count decreased from 2 to 1")

[<Test>]
let ``Insert file`` () =

  let root =
    { Name = "Root"
      Path = []
      Children =
        [ Folder
            { Name = "F1"
              Path = [ "F1" ]
              Children =
                [ Folder
                    { Name = "F12"
                      Path = [ "F1"; "F12" ]
                      Children = [] }
                  File
                    { Name = "Y"
                      Size = 1
                      Path = [ "F1"; "Y" ]
                      Content = JsonValue.Null } ] } ] }
    |> Folder

  let state = VirtualFolders.insert [ "F1"; "F12"; "X" ] JsonValue.Null root
  let actual = VirtualFolders.readFile [ "F1"; "F12"; "X" ] state
  let countAfter = VirtualFolders.countFiles state

  Assert.That(actual.IsSome, Is.True, "File exists the hierarchy")
  Assert.That(countAfter, Is.EqualTo 2, "Files count increased from 1 to 2")

[<Test>]
let ``Find file`` () =

  let root =
    { Name = "Root"
      Path = []
      Children =
        [ Folder
            { Name = "F1"
              Path = [ "F1" ]
              Children =
                [ Folder
                    { Name = "F12"
                      Path = [ "F1"; "F12" ]
                      Children = [] }
                  File
                    { Name = "Y"
                      Size = 1
                      Path = [ "F1"; "Y" ]
                      Content = JsonValue.Null } ] } ] }
    |> Folder

  let file = VirtualFolders.tryFindFileByName "Y" root

  Assert.That(file.IsSome, Is.True, "File exists")

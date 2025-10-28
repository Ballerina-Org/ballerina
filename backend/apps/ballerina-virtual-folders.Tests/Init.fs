module Ballerina.Data.Tests.VirtualFolders.Init

open System.Linq
open Ballerina.VirtualFolders
open Ballerina.VirtualFolders.Model
open NUnit.Framework
open FSharp.Data

let json = JsonValue.Record [| "someKey", JsonValue.String "someValue" |] |> Json



[<Test>]
let ``Init structure works`` () =
  let root =
    { Name = "Root"
      Path = []
      Metadata = None
      Children =
        [ File
            { Name = "alone"
              Path = [ "alone" ]
              Content = json
              Metadata = None }

          Folder
            { Name = "A"
              Path = [ "A" ]
              Metadata = None
              Children =
                [ Folder
                    { Name = "A.1"
                      Path = [ "A"; "A.1" ]
                      Metadata = None
                      Children =
                        [ Folder
                            { Name = "A.1.1"
                              Path = [ "A"; "A.1"; "A.1.1" ]
                              Metadata = None
                              Children =
                                [ Folder
                                    { Name = "A.1.2"
                                      Path = [ "A"; "A.1"; "A.1.2" ]
                                      Metadata = None
                                      Children =
                                        [ Folder
                                            { Name = "A.1.2.3"
                                              Path = [ "A"; "A.1"; "A.1.2"; "A.1.2.3" ]
                                              Metadata = None
                                              Children = [] }
                                          File
                                            { Name = "file_x"
                                              Path = [ "A"; "A.1"; "A.1.2"; "A.1.2.3"; "file_x" ]
                                              Metadata = None
                                              Content = json } ] } ] } ] }
                  File
                    { Name = "file1-in-a"
                      Metadata = None
                      Path = [ "A"; "file1-in-a" ]
                      Content = json }
                  File
                    { Name = "file2-in-a"
                      Metadata = None
                      Path = [ "A"; "file2-in-a" ]
                      Content = json } ] }

          Folder
            { Name = "B"
              Path = [ "B" ]
              Metadata = None
              Children =
                [ Folder
                    { Name = "B.1"
                      Path = [ "B"; "B.1" ]
                      Metadata = None
                      Children = [] }
                  File
                    { Name = "file1-in-b"
                      Metadata = None
                      Path = [ "B"; "file1-in-b" ]
                      Content = json } ] }
          File
            { Name = "alone2"
              Metadata = None
              Path = [ "alone2" ]
              Content = json } ] }

  let flat = FolderNode.flatten root |> Seq.toArray
  let test = flat |> Array.find (fun (name, _, _) -> name = "file_x")
  let _, path, _ = test
  Assert.That(flat.Count(), Is.EqualTo 6)


  Assert.That(path.Length, Is.EqualTo 5, "path length including the file name")

  let _unflat = FolderNode.unflatten "Root" (flat |> Seq.ofArray)
  Assert.Pass()

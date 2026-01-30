module Ballerina.Data.Tests.VirtualFolders.Builder

open Ballerina
open Ballerina.Collections.Sum
open Ballerina.State.WithError
open Ballerina.VirtualFolders
open Ballerina.VirtualFolders.Patterns
open Ballerina.VirtualFolders.CE
open Ballerina.VirtualFolders.Operations
open Ballerina.VirtualFolders.Model
open NUnit.Framework
open FSharp.Data
open Ballerina.Errors

let root =
  Folder
    { Name = "root"
      Children = []
      Path = []
      Metadata = None }

let ``Virtual Folders CE custom operators`` () =
  let initialRoot: VfsNode = mkFolder [] "root" []

  let initialState =
    { VfsState.Root = initialRoot
      Path = [] }

  CE.folder {
    insert [ "hello.json" ] (JsonValue.String "hi" |> Json)
    insert [ "x.json" ] (JsonValue.String "x" |> Json)
    updateFile [ "hello.json" ] (fun _ -> JsonValue.String "updated" |> Json)
    delete [ "x.json" ]
  }
  |> State.Run((), initialState)
  |> function
    | Sum.Left(_, Some s) ->
      match s.Root with
      | VfsNode.Folder final -> Assert.That(final.Children.Length, Is.EqualTo 1)
      | _ -> Assert.Fail("Expected folder")
    | _ -> Assert.Fail("Unexpected computation result")

[<Test>]
let ``Virtual Folders CE with nested paths and operations`` () =
  let initial: VfsNode = mkFolder [] "root" []

  folder {
    insert [ "nested"; "deep"; "file.json" ] (JsonValue.String "deep value" |> Json)
    insert [ "nested"; "other.json" ] (JsonValue.String "other" |> Json)
    updateFile [ "nested"; "deep"; "file.json" ] (fun _ -> JsonValue.String "updated" |> Json)
    delete [ "nested"; "other.json" ]
  }

  |> State.Run((), { VfsState.Root = initial; Path = [] })
  |> function
    | Sum.Left(_, Some s) ->
      match tryFind [ "nested"; "deep"; "file.json" ] s.Root with
      | Some(File f) -> Assert.That(f.Content, Is.EqualTo(JsonValue.String "updated" |> Json))
      | _ -> Assert.Fail("Expected updated file not found")
    | _ -> Assert.Fail("Unexpected computation result")

[<Test>]
let ``Insert, update and delete nested paths with FolderBuilder`` () =
  let initial =
    mkFolder [] "root" [ mkFolder [ "root" ] "a" [ mkFolder [ "root"; "a" ] "b" [] ] ]

  let program =
    folder {
      move [ "a"; "b" ]
      insert [ "c.json" ] (JsonValue.String "value" |> Json)
      root
      updateFile [ "a"; "b"; "c.json" ] (fun _ -> JsonValue.String "updated" |> Json)
      insert [ "a"; "b"; "other.json" ] (JsonValue.String "other" |> Json)
      delete [ "a"; "b"; "other.json" ]
    }

  match State.Run ((), { VfsState.Root = initial; Path = [] }) program with
  | Sum.Left(_, Some s) ->
    match tryFind [ "a"; "b"; "c.json" ] s.Root with
    | Some(File f) -> Assert.That(f.Content, Is.EqualTo(JsonValue.String "updated" |> Json))
    | _ -> Assert.Fail("Expected file c.json not found")
  | _x -> Assert.Fail("Invalid result")

[<Test>]
let ``Insert using suffix path`` () =
  let initial = mkFolder [] "root" [ mkFolder [ "root" ] "some" [] ]

  let program =
    folder {
      move [ "some" ]
      insert [ "path.json" ] (JsonValue.String "v" |> Json)
      insert (withFileSuffix "_copy" [ "path.json" ]) (JsonValue.String "v2" |> Json)
    }

  match State.Run ((), { VfsState.Root = initial; Path = [] }) program with
  | Sum.Left(_, Some s) ->
    match tryFind [ "some"; "path.json" ] s.Root, tryFind [ "some"; "path_copy.json" ] s.Root with
    | Some(File f1), Some(File f2) ->
      Assert.That(f1.Content, Is.EqualTo(JsonValue.String "v" |> Json))
      Assert.That(f2.Content, Is.EqualTo(JsonValue.String "v2" |> Json))
    | _ -> Assert.Fail("Expected both original and suffixed files not found")
  | _ -> Assert.Fail("Invalid result")

[<Test>]
let ``Insert multiple top-level branches`` () =
  let initial = mkFolder [] "root" []

  let program =
    folder {
      insert [ "user.json" ] (JsonValue.String "user" |> Json)
      insert [ "endpoint.json" ] (JsonValue.String "endpoint" |> Json)
    }

  match State.Run ((), { VfsState.Root = initial; Path = [] }) program with
  | Sum.Left(_, Some s) ->
    let count = countFiles s.Root
    Assert.That(count, Is.EqualTo 2)
  | _ -> Assert.Fail("Invalid result")

[<Test>]
let ``Jump into subfolder and back to root`` () =
  let initial =
    mkFolder [] "root" [ mkFolder [ "root" ] "a" [ mkFolder [ "root"; "a" ] "b" [] ] ]

  let program =
    folder {
      move [ "a"; "b" ]
      insert [ "hello.json" ] (JsonValue.String "in b" |> Json)
      root
      insert [ "at-root.json" ] (JsonValue.String "in root" |> Json)
    }

  match State.Run ((), { VfsState.Root = initial; Path = [] }) program with
  | Sum.Left(_, Some s) ->
    let inRoot = tryFind [ "at-root.json" ] s.Root
    let inB = tryFind [ "a"; "b"; "hello.json" ] s.Root

    match inRoot, inB with
    | Some(File f1), Some(File f2) ->
      Assert.That(f1.Content, Is.EqualTo(JsonValue.String "in root" |> Json))
      Assert.That(f2.Content, Is.EqualTo(JsonValue.String "in b" |> Json))
    | _ -> Assert.Fail("Files not inserted correctly")
  | _ -> Assert.Fail("Unexpected result")

[<Test>]
let ``Go down into subfolder and insert file`` () =
  let initial =
    mkFolder [] "root" [ mkFolder [ "root" ] "a" [ mkFolder [ "root"; "a" ] "b" [] ] ]

  let program =
    folder {
      down "a"
      down "b"
      insert [ "hello.json" ] (JsonValue.String "in b" |> Json)
    }

  match State.Run ((), { VfsState.Root = initial; Path = [] }) program with
  | Sum.Left(_, Some s) ->
    match tryFind [ "a"; "b"; "hello.json" ] s.Root with
    | Some(File f) -> Assert.That(f.Content, Is.EqualTo(JsonValue.String "in b" |> Json))
    | _ -> Assert.Fail("File not inserted in 'a/b'")
  | _ -> Assert.Fail("Unexpected result")

[<Test>]
let ``Go down and then up`` () =
  let initial =
    mkFolder [] "root" [ mkFolder [ "root" ] "a" [ mkFolder [ "root"; "a" ] "b" [] ] ]

  let program =
    folder {
      down "a"
      down "b"
      up
      insert [ "after-up.json" ] (JsonValue.String "in a" |> Json)
    }

  match State.Run ((), { VfsState.Root = initial; Path = [] }) program with
  | Sum.Left(_, Some s) ->
    match tryFind [ "a"; "after-up.json" ] s.Root with
    | Some(File f) -> Assert.That(f.Content, Is.EqualTo(JsonValue.String "in a" |> Json))
    | _ -> Assert.Fail("File not inserted correctly after going up")
  | _ -> Assert.Fail("Unexpected result")

[<Test>]
let ``Up from root fails gracefully`` () =
  let initial = mkFolder [] "root" []

  let program = folder { up }

  match State.Run ((), { VfsState.Root = initial; Path = [] }) program with
  | Sum.Right(err, _) -> Assert.That(Errors.ToString(err, "\n"), Does.Contain("Already at root"))
  | _ -> Assert.Fail("Expected an error when calling up from root")

[<Test>]
let ``Move relative down and up`` () =
  let initial =
    mkFolder [] "root" [ mkFolder [ "root" ] "a" [ mkFolder [ "root"; "a" ] "b" []; mkFolder [ "root"; "a" ] "c" [] ] ]

  let program =
    folder {
      move [ "a"; "b" ]
      insert [ "in-b.json" ] (JsonValue.String "in b" |> Json)
      move [ ".." ]
      move [ "c" ]
      insert [ "in-c.json" ] (JsonValue.String "in c" |> Json)
    }

  match State.Run ((), { VfsState.Root = initial; Path = [] }) program with
  | Sum.Left(_, Some s) ->
    let inB = tryFind [ "a"; "b"; "in-b.json" ] s.Root
    let inC = tryFind [ "a"; "c"; "in-c.json" ] s.Root

    match inB, inC with
    | Some(File f1), Some(File f2) ->
      Assert.That(f1.Content, Is.EqualTo(JsonValue.String "in b" |> Json))
      Assert.That(f2.Content, Is.EqualTo(JsonValue.String "in c" |> Json))
    | _ -> Assert.Fail("Files not inserted correctly after relative moves")
  | _err -> Assert.Fail("Unexpected result")

[<Test>]
let ``Move to non-existing folder throws error`` () =
  let initial = mkFolder [] "root" [ mkFolder [ "root" ] "a" [] ]

  let program = folder { move [ "a"; "nonexistent" ] }

  match State.Run ((), { VfsState.Root = initial; Path = [] }) program with
  | Sum.Right(err, _) -> Assert.That(Errors.ToString(err, "\n"), Does.Contain("Path not found"))
  | _ -> Assert.Fail("Expected path-not-found error")

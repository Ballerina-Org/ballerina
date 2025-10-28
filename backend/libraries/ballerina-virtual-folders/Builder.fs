namespace Ballerina.VirtualFolders

open Ballerina.VirtualFolders.Model

module CE =
  open Ballerina.Errors
  open Ballerina.State.WithError
  open Ballerina.VirtualFolders.Operations

  type VfsState = { Root: VfsNode; Path: VirtualPath }

  type FolderBuilder() =

    member _.Return(x: 'a) : State<'a, unit, VfsState, Errors> = state.Return x

    member _.Bind
      (p: State<'a, unit, VfsState, Errors>, f: 'a -> State<'b, unit, VfsState, Errors>)
      : State<'b, unit, VfsState, Errors> =
      state.Bind(p, f)

    member _.Zero() : State<unit, unit, VfsState, Errors> = state.Zero()

    member _.Yield(x: 'a) : State<'a, unit, VfsState, Errors> = state.Yield x

    member _.YieldFrom(p: State<'a, unit, VfsState, Errors>) : State<'a, unit, VfsState, Errors> = p

    member _.ReturnFrom(p: State<'a, unit, VfsState, Errors>) : State<'a, unit, VfsState, Errors> = p

    member _.Delay(f: unit -> State<'a, unit, VfsState, Errors>) : State<'a, unit, VfsState, Errors> = state.Delay f

    [<CustomOperation("insert")>]
    member _.Insert
      (p: State<unit, unit, VfsState, Errors>, path: VirtualPath, content: FileContent)
      : State<unit, unit, VfsState, Errors> =
      state {
        do! p
        let! s = state.GetState()
        let fullPath = s.Path @ path
        let updatedRoot = insert fullPath content s.Root
        do! state.SetState(fun _ -> { s with Root = updatedRoot })
      }

    [<CustomOperation("delete")>]
    member _.Delete(p: State<unit, unit, VfsState, Errors>, path: VirtualPath) : State<unit, unit, VfsState, Errors> =
      state {
        do! p
        let! s = state.GetState()
        let fullPath = s.Path @ path
        let updatedRoot = delete fullPath s.Root
        do! state.SetState(fun _ -> { s with Root = updatedRoot })
      }

    [<CustomOperation("updateFile")>]
    member _.UpdateFile
      (p: State<unit, unit, VfsState, Errors>, path: VirtualPath, f: FileContent -> FileContent)
      : State<unit, unit, VfsState, Errors> =
      state {
        do! p
        let! s = state.GetState()
        let fullPath = s.Path @ path
        let updatedRoot = updateFile fullPath f s.Root
        do! state.SetState(fun _ -> { s with Root = updatedRoot })
      }

    [<CustomOperation("read")>]
    member _.Read
      (p: State<unit, unit, VfsState, Errors>, path: VirtualPath)
      : State<FileContent option, unit, VfsState, Errors> =
      state {
        do! p
        let! s = state.GetState()
        return readFile (s.Path @ path) s.Root
      }

    [<CustomOperation("root")>]
    member _.Root(p: State<unit, unit, VfsState, Errors>) : State<unit, unit, VfsState, Errors> =
      state {
        do! p
        let! s = state.GetState()
        do! state.SetState(fun _ -> { s with Path = [] })
      }

    [<CustomOperation("down")>]
    member _.Down(p, subdir) =
      state {
        do! p
        let! s = state.GetState()
        let next = s.Path @ [ subdir ]

        match tryFind next s.Root with
        | Some(Folder _) -> do! state.SetState(fun _ -> { s with Path = next })
        | Some _ -> return! state.Throw(Errors.Singleton $"'{subdir}' is not a folder")
        | None -> return! state.Throw(Errors.Singleton $"Folder '{subdir}' not found")
      }

    [<CustomOperation("up")>]
    member _.Up(p) =
      state {
        do! p
        let! s = state.GetState()

        match s.Path with
        | [] -> return! state.Throw(Errors.Singleton "Already at root folder")
        | path ->
          let parent = path |> List.take (path.Length - 1)
          do! state.SetState(fun _ -> { s with Path = parent })
      }

    [<CustomOperation("move")>]
    member _.Move(p, path: VirtualPath) =
      state {
        do! p
        let! s = state.GetState()

        let normalized =
          path
          |> List.fold
            (fun (acc: string list) seg ->
              match seg with
              | ".." when acc.Length > 0 -> acc |> List.take (acc.Length - 1)
              | ".." -> acc
              | name -> acc @ [ name ])
            s.Path

        match tryFind normalized s.Root with
        | Some(Folder _) -> do! state.SetState(fun _ -> { s with Path = normalized })
        | Some _ -> return! state.Throw(Errors.Singleton "Target is not a folder")
        | None -> return! state.Throw(Errors.Singleton $"""Path not found: {String.concat "/" normalized}""")
      }

  let folder = FolderBuilder()

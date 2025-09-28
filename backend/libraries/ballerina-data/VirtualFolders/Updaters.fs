namespace Ballerina.Data.VirtualFolders

module ToUpdaters =
  let file (root: Content) (path: string list) =
    fun delta -> root |> VirtualFolders.updateFile path (fun _ -> delta)

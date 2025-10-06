namespace Ballerina.Data.VirtualFolders

module Patterns =
  open Ballerina.Collections.Sum
  open Ballerina.Errors

  type Content with
    static member AsFolder(fn: Content) =
      match fn with
      | Folder f -> sum.Return f
      | _ -> sum.Throw(Errors.Singleton "Expected folder as a content, got file")

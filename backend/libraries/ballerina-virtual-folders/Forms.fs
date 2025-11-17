namespace Ballerina.VirtualFolders

open Ballerina.VirtualFolders.Operations
open FSharp.Data
open Ballerina.Collections.Sum
open Ballerina.DSL.FormEngine.Model
open Ballerina.Errors
open Ballerina.VirtualFolders
open Ballerina.VirtualFolders.Interactions
open Ballerina.VirtualFolders.Model
open Ballerina.VirtualFolders.Patterns
open Ballerina.StdLib.Json.Patterns

module Forms =
  let keys (name: string) (variant: WorkspaceVariant) (path: VirtualPath option) (node: VfsNode) =
    match variant with
    | Explore _ ->
      sum {
        let! path = path |> sum.OfOption(Errors.Singleton("Path is required in explore mode"))
        let! v1 = tryFind path node |> sum.OfOption(Errors.Singleton("Can't find types file"))
        let! v1 = VfsNode.AsFile v1
        let! v1 = FileContent.AsJson v1.Content

        return JsonValue.AsRecordKeys(v1.GetProperty name)
      }
    | Compose -> sum.Throw(Errors.Singleton("Not implemented v2 files retrieval for compose spec"))

  let parse
    (variant: WorkspaceVariant)
    (path: VirtualPath option)
    (node: VfsNode)
    (validator: Validator<_, _>)
    : Sum<JsonValue * ParsedFormsContext<_, _> * string list, Errors> =
    match variant with
    | Explore _ -> Updaters.explore path validator id node
    | Compose -> Updaters.compose validator id node

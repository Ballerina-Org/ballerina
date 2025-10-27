namespace Ballerina.VirtualFolders

open Ballerina
open Ballerina.DSL.FormEngine.Model
open Ballerina.VirtualFolders.Interactions
open FSharp.Data
open Ballerina.Collections.Sum
open Ballerina.Errors
open Ballerina.VirtualFolders.Model
open Ballerina.VirtualFolders.Patterns

module Updaters =
  let private validate (workspace: WorkspaceVariant) (validator: Validator<'ExprExt, 'ValExt>) (root: VfsNode) =

    sum {
      let! init = Validator.init root

      let validate =
        validator.Validate init.CodegenConfig init.InitialContext init.LangSpecificConfig

      match workspace with
      | Explore(_fileSplit, path) ->
        let! path =
          Transient.value path
          |> sum.OfOption(Errors.Singleton "explore (no transient path")

        let! node = VfsNode.AsFolder root
        let! files = Explore.getFiles (node, path)
        return! validate files
      | Compose ->
        let! node = VfsNode.AsFolder root
        let files = Compose.getFiles node
        return! validate files
    }

  let compose
    (validator: Validator<'ExprExt, 'ValExt>)
    (u: U<VfsNode>)
    : VfsNode -> Sum<JsonValue * ParsedFormsContext<'ExprExt, 'ValExt>, Errors> =

    fun node ->
      sum {
        let! result = validate Compose validator (u node)
        return result
      }

  let explore
    (path: VirtualPath)
    (validator: Validator<'ExprExt, 'ValExt>)
    (u: U<VfsNode>)
    : VfsNode -> Sum<JsonValue * ParsedFormsContext<'ExprExt, 'ValExt>, Errors> =
    fun node ->
      sum {
        let! result = validate (Explore(FileSplit.Partial [], Transient.some path)) validator (u node)
        return result
      }

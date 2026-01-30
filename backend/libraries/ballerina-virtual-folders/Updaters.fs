namespace Ballerina.VirtualFolders

open Ballerina
open Ballerina.DSL.FormEngine.Model
open Ballerina.VirtualFolders.Interactions
open FSharp.Data
open Ballerina
open Ballerina.Collections.Sum
open Ballerina.Errors
open Ballerina.VirtualFolders.Model
open Ballerina.VirtualFolders.Patterns

module Updaters =
  let private validate
    (workspace: WorkspaceVariant)
    (validator: Validator<'ExprExt, 'ValExt>)
    (root: VfsNode)
    : Sum<JsonValue * ParsedFormsContext<'ExprExt, 'ValExt> * string list, Errors<unit>> =

    sum {
      let! init = Validator.init root

      let validate =
        validator.Validate init.CodegenConfig init.InitialContext init.LangSpecificConfig

      match workspace with
      | Explore(_fileSplit, path) ->
        let! path =
          Transient.value path
          |> sum.OfOption(Errors.Singleton () (fun () -> "explore (no transient path)"))

        let! node = VfsNode.AsFolder root
        let! files = Explore.getFiles (node, path)
        return! validate files |> sum.Map(fun (json, model) -> json, model, init.InjectedTypes)
      | Compose ->
        let! node = VfsNode.AsFolder root
        let files = Compose.getFiles node
        return! validate files |> sum.Map(fun (json, model) -> json, model, init.InjectedTypes)
    }

  let compose
    (validator: Validator<'ExprExt, 'ValExt>)
    (u: U<VfsNode>)
    : VfsNode -> Sum<JsonValue * ParsedFormsContext<'ExprExt, 'ValExt> * string list, Errors<unit>> =

    fun node -> validate Compose validator (u node)

  let explore
    (path: VirtualPath option)
    (validator: Validator<'ExprExt, 'ValExt>)
    (u: U<VfsNode>)
    : VfsNode -> Sum<JsonValue * ParsedFormsContext<'ExprExt, 'ValExt> * string list, Errors<unit>> =
    fun node ->
      sum {
        let! path =
          path
          |> sum.OfOption(Errors.Singleton () (fun () -> "Path is required in 'explore' variant"))

        return! validate (Explore(FileSplit.Partial [], Transient.some path)) validator (u node)
      }

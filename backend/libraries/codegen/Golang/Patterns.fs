namespace Codegen.Golang

module Patterns =
  open Ballerina.State.WithError
  open Codegen.Golang

  let updateImports (imports: Set<GoImport>) : State<unit, _, GoCodeGenState, _> =
    imports |> Set.union |> GoCodeGenState.Updaters.UsedImports |> state.SetState

  let updateImportsFromOptionalImport (imports: Option<GoImport>) : State<unit, _, GoCodeGenState, _> =
    imports |> Option.toList |> Set.ofList |> updateImports

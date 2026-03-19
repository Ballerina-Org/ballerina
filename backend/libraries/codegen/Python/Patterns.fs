namespace Codegen.Python

module Patterns =
  open Codegen.Python.Model
  open Ballerina.State.WithError
  open Ballerina.Errors

  let updateImports (imports: Set<Import>) : State<unit, 'config, PythonCodeGenState, Errors<unit>> =
    imports
    |> Set.union
    |> PythonCodeGenState.Updaters.UsedImports
    |> state.SetState

namespace Ballerina.DSL.FormEngine.Parser

module Patterns =

  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Errors

  type SumBuilder with
    member sum.TryFindField name fields =
      fields
      |> Seq.tryFind (fst >> (=) name)
      |> Option.map snd
      |> Sum.fromOption (fun () -> Errors.Singleton $"Error: cannot find field '{name}'")

  type StateBuilder with
    member state.TryFindField name fields =
      fields |> sum.TryFindField name |> state.OfSum

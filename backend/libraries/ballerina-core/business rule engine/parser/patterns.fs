namespace Ballerina.DSL.Parser

module Patterns =

  open Ballerina.Collections.Sum
  open Ballerina.State.WithError
  open Ballerina.Errors
  open Ballerina.DSL.Expr.Types.Model

  type ContextActions<'context> =
    { TryFindType: 'context -> string -> Sum<TypeBinding, Errors> }

  type SumBuilder with
    member sum.TryFindField name fields =
      fields
      |> Seq.tryFind (fst >> (=) name)
      |> Option.map snd
      |> Sum.fromOption (fun () -> Errors.Singleton $"Error: cannot find field '{name}'")

  type StateBuilder with
    member state.TryFindField name fields =
      fields |> sum.TryFindField name |> state.OfSum

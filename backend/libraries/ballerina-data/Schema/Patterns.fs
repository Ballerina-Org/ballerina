namespace Ballerina.Data.Schema

module Patterns =

  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Data.Schema.Model
  open Ballerina.Errors

  type UpdaterPathStep with
    static member AsField step : Sum<string, Errors<unit>> =
      match step with
      | Field name -> sum.Return name
      | pathStep -> sum.Throw(Errors.Singleton () (fun () -> $"Expected Field, got {pathStep}"))

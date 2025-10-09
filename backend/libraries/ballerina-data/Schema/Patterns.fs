namespace Ballerina.Data.Schema

module Patterns =

  open Ballerina.Collections.Sum
  open Ballerina.Data.Schema.Model
  open Ballerina.Errors

  type UpdaterPathStep with
    static member AsField step : Sum<string, Errors> =
      match step with
      | Field name -> sum.Return name
      | pathStep -> sum.Throw(Errors.Singleton $"Expected Field, got {pathStep}")

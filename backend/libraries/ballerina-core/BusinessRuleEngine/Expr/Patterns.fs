namespace Ballerina.DSL.Expr

module Patterns =
  open System
  open Model
  open Ballerina.Fun
  open Ballerina.Collections.Option
  open Ballerina.Collections.Map
  open Ballerina.Collections.Sum
  open Ballerina.Errors

  type Expr with
    static member AsLambda(e: Expr) =
      match e with
      | Expr.Value(Value.Lambda(v, b)) -> sum { return (v, b) }
      | _ -> sum.Throw(Errors.Singleton $"Error: expected lambda, found {e.ToString()}")

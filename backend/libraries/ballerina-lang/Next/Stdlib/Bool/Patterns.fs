namespace Ballerina.DSL.Next.StdLib.Bool

[<AutoOpen>]
module Patterns =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types

  type BoolOperations<'ext> with
    static member AsAnd(op: BoolOperations<'ext>) : Sum<Option<bool>, Errors<Unit>> =
      match op with
      | BoolOperations.And v -> v.v1 |> sum.Return
      | _ ->
        (fun () -> $"Error: Expected BoolOperations.And, found {op}")
        |> Errors.Singleton()
        |> sum.Throw

    static member AsOr(op: BoolOperations<'ext>) : Sum<Option<bool>, Errors<Unit>> =
      match op with
      | BoolOperations.Or v -> v.v1 |> sum.Return
      | _ ->
        (fun () -> $"Error: Expected BoolOperations.Or, found {op}")
        |> Errors.Singleton()
        |> sum.Throw

    static member AsNot(op: BoolOperations<'ext>) : Sum<Unit, Errors<Unit>> =
      match op with
      | BoolOperations.Not v -> v.v1 |> sum.Return
      | _ ->
        (fun () -> $"Error: Expected BoolOperations.Not, found {op}")
        |> Errors.Singleton()
        |> sum.Throw

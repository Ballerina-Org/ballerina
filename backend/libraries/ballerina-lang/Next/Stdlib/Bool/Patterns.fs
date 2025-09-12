namespace Ballerina.DSL.Next.StdLib.Bool

[<AutoOpen>]
module Patterns =
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types

  type BoolOperations<'ext> with
    static member AsAnd(op: BoolOperations<'ext>) : Sum<Option<bool>, Errors> =
      match op with
      | BoolOperations.And v -> v.v1 |> sum.Return
      | _ -> failwith "Expected And operation"

    static member AsOr(op: BoolOperations<'ext>) : Sum<Option<bool>, Errors> =
      match op with
      | BoolOperations.Or v -> v.v1 |> sum.Return
      | _ -> failwith "Expected Or operation"

    static member AsNot(op: BoolOperations<'ext>) : Sum<Unit, Errors> =
      match op with
      | BoolOperations.Not v -> v.v1 |> sum.Return
      | _ -> failwith "Expected Not operation"

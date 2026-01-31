namespace Ballerina.DSL.Next.StdLib.String

[<AutoOpen>]
module Patterns =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types

  type StringOperations<'ext> with
    static member AsLength(op: StringOperations<'ext>) : Sum<Unit, Errors<Unit>> =
      match op with
      | StringOperations.Length -> sum.Return()
      | _ -> sum.Throw <| Errors.Singleton () (fun () -> "Expected Length operation")

    static member AsConcat(op: StringOperations<'ext>) : Sum<Option<string>, Errors<Unit>> =
      match op with
      | StringOperations.Concat v -> v.v1 |> sum.Return
      | _ -> sum.Throw <| Errors.Singleton () (fun () -> "Expected Concat operation")

    static member AsEqual(op: StringOperations<'ext>) : Sum<Option<string>, Errors<Unit>> =
      match op with
      | StringOperations.Equal v -> v.v1 |> sum.Return
      | _ -> sum.Throw <| Errors.Singleton () (fun () -> "Expected Equal operation")

    static member AsNotEqual(op: StringOperations<'ext>) : Sum<Option<string>, Errors<Unit>> =
      match op with
      | StringOperations.NotEqual v -> v.v1 |> sum.Return
      | _ -> sum.Throw <| Errors.Singleton () (fun () -> "Expected NotEqual operation")

    static member AsGreaterThan(op: StringOperations<'ext>) : Sum<Option<string>, Errors<Unit>> =
      match op with
      | StringOperations.GreaterThan v -> v.v1 |> sum.Return
      | _ -> sum.Throw <| Errors.Singleton () (fun () -> "Expected GreaterThan operation")

    static member AsGreaterThanOrEqual(op: StringOperations<'ext>) : Sum<Option<string>, Errors<Unit>> =
      match op with
      | StringOperations.GreaterThanOrEqual v -> v.v1 |> sum.Return
      | _ ->
        sum.Throw
        <| Errors.Singleton () (fun () -> "Expected GreaterThanOrEqual operation")

    static member AsLessThan(op: StringOperations<'ext>) : Sum<Option<string>, Errors<Unit>> =
      match op with
      | StringOperations.LessThan v -> v.v1 |> sum.Return
      | _ -> sum.Throw <| Errors.Singleton () (fun () -> "Expected LessThan operation")

    static member AsLessThanOrEqual(op: StringOperations<'ext>) : Sum<Option<string>, Errors<Unit>> =
      match op with
      | StringOperations.LessThanOrEqual v -> v.v1 |> sum.Return
      | _ ->
        sum.Throw
        <| Errors.Singleton () (fun () -> "Expected LessThanOrEqual operation")

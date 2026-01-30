namespace Ballerina.DSL.Next.StdLib.DateTime

[<AutoOpen>]
module Patterns =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types
  open System

  type DateTimeOperations<'ext> with

    static member AsDiff(op: DateTimeOperations<'ext>) : Sum<Option<DateTime>, Errors<Unit>> =
      match op with
      | DateTimeOperations.Diff v -> v.v1 |> sum.Return
      | _ -> sum.Throw(Errors.Singleton () (fun () -> "Expected Diff operation"))

    static member AsEqual(op: DateTimeOperations<'ext>) : Sum<Option<DateTime>, Errors<Unit>> =
      match op with
      | DateTimeOperations.Equal v -> v.v1 |> sum.Return
      | _ -> sum.Throw(Errors.Singleton () (fun () -> "Expected Equal operation"))

    static member AsNotEqual(op: DateTimeOperations<'ext>) : Sum<Option<DateTime>, Errors<Unit>> =
      match op with
      | DateTimeOperations.NotEqual v -> v.v1 |> sum.Return
      | _ -> sum.Throw(Errors.Singleton () (fun () -> "Expected NotEqual operation"))

    static member AsGreaterThan(op: DateTimeOperations<'ext>) : Sum<Option<DateTime>, Errors<Unit>> =
      match op with
      | DateTimeOperations.GreaterThan v -> v.v1 |> sum.Return
      | _ -> sum.Throw(Errors.Singleton () (fun () -> "Expected GreaterThan operation"))

    static member AsGreaterThanOrEqual(op: DateTimeOperations<'ext>) : Sum<Option<DateTime>, Errors<Unit>> =
      match op with
      | DateTimeOperations.GreaterThanOrEqual v -> v.v1 |> sum.Return
      | _ -> sum.Throw(Errors.Singleton () (fun () -> "Expected GreaterThanOrEqual operation"))

    static member AsLessThan(op: DateTimeOperations<'ext>) : Sum<Option<DateTime>, Errors<Unit>> =
      match op with
      | DateTimeOperations.LessThan v -> v.v1 |> sum.Return
      | _ -> sum.Throw(Errors.Singleton () (fun () -> "Expected LessThan operation"))

    static member AsLessThanOrEqual(op: DateTimeOperations<'ext>) : Sum<Option<DateTime>, Errors<Unit>> =
      match op with
      | DateTimeOperations.LessThanOrEqual v -> v.v1 |> sum.Return
      | _ -> sum.Throw(Errors.Singleton () (fun () -> "Expected LessThanOrEqual operation"))

namespace Ballerina.DSL.Next.StdLib.DateOnly

open Ballerina
open Ballerina.Collections.Sum
open System
open Ballerina.Errors

[<AutoOpen>]
module Patterns =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types
  open System

  type DateOnlyOperations<'ext> with

    static member AsDiff(op: DateOnlyOperations<'ext>) : Sum<Option<DateOnly>, Errors<Unit>> =
      match op with
      | DateOnlyOperations.Diff v -> v.v1 |> sum.Return
      | _ ->
        (fun () -> $"Error: Expected DateOnlyOperations.Diff, found {op}")
        |> Errors.Singleton()
        |> sum.Throw

    static member AsEqual(op: DateOnlyOperations<'ext>) : Sum<Option<DateOnly>, Errors<Unit>> =
      match op with
      | DateOnlyOperations.Equal v -> v.v1 |> sum.Return
      | _ ->
        (fun () -> $"Error: Expected DateOnlyOperations.Equal, found {op}")
        |> Errors.Singleton()
        |> sum.Throw

    static member AsNotEqual(op: DateOnlyOperations<'ext>) : Sum<Option<DateOnly>, Errors<Unit>> =
      match op with
      | DateOnlyOperations.NotEqual v -> v.v1 |> sum.Return
      | _ ->
        (fun () -> $"Error: Expected DateOnlyOperations.NotEqual, found {op}")
        |> Errors.Singleton()
        |> sum.Throw

    static member AsGreaterThan(op: DateOnlyOperations<'ext>) : Sum<Option<DateOnly>, Errors<Unit>> =
      match op with
      | DateOnlyOperations.GreaterThan v -> v.v1 |> sum.Return
      | _ ->
        (fun () -> $"Error: Expected DateOnlyOperations.GreaterThan, found {op}")
        |> Errors.Singleton()
        |> sum.Throw

    static member AsGreaterThanOrEqual(op: DateOnlyOperations<'ext>) : Sum<Option<DateOnly>, Errors<Unit>> =
      match op with
      | DateOnlyOperations.GreaterThanOrEqual v -> v.v1 |> sum.Return
      | _ ->
        (fun () -> $"Error: Expected DateOnlyOperations.GreaterThanOrEqual, found {op}")
        |> Errors.Singleton()
        |> sum.Throw

    static member AsLessThan(op: DateOnlyOperations<'ext>) : Sum<Option<DateOnly>, Errors<Unit>> =
      match op with
      | DateOnlyOperations.LessThan v -> v.v1 |> sum.Return
      | _ ->
        (fun () -> $"Error: Expected DateOnlyOperations.LessThan, found {op}")
        |> Errors.Singleton()
        |> sum.Throw

    static member AsLessThanOrEqual(op: DateOnlyOperations<'ext>) : Sum<Option<DateOnly>, Errors<Unit>> =
      match op with
      | DateOnlyOperations.LessThanOrEqual v -> v.v1 |> sum.Return
      | _ ->
        (fun () -> $"Error: Expected DateOnlyOperations.LessThanOrEqual, found {op}")
        |> Errors.Singleton()
        |> sum.Throw

    static member AsToDateTime(op: DateOnlyOperations<'ext>) : Sum<Option<DateOnly>, Errors<Unit>> =
      match op with
      | DateOnlyOperations.ToDateTime v -> v.v1 |> sum.Return
      | _ ->
        (fun () -> $"Error: Expected DateOnlyOperations.ToDateTime, found {op}")
        |> Errors.Singleton()
        |> sum.Throw

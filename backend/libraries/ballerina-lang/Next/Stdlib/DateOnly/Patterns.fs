namespace Ballerina.DSL.Next.StdLib.DateOnly

[<AutoOpen>]
module Patterns =
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types
  open System

  type DateOnlyOperations<'ext> with

    static member AsEqual(op: DateOnlyOperations<'ext>) : Sum<Option<DateOnly>, Errors> =
      match op with
      | DateOnlyOperations.Equal v -> v.v1 |> sum.Return
      | _ -> failwith "Expected Equal operation"

    static member AsNotEqual(op: DateOnlyOperations<'ext>) : Sum<Option<DateOnly>, Errors> =
      match op with
      | DateOnlyOperations.NotEqual v -> v.v1 |> sum.Return
      | _ -> failwith "Expected NotEqual operation"

    static member AsGreaterThan(op: DateOnlyOperations<'ext>) : Sum<Option<DateOnly>, Errors> =
      match op with
      | DateOnlyOperations.GreaterThan v -> v.v1 |> sum.Return
      | _ -> failwith "Expected GreaterThan operation"

    static member AsGreaterThanOrEqual(op: DateOnlyOperations<'ext>) : Sum<Option<DateOnly>, Errors> =
      match op with
      | DateOnlyOperations.GreaterThanOrEqual v -> v.v1 |> sum.Return
      | _ -> failwith "Expected GreaterThanOrEqual operation"

    static member AsLessThan(op: DateOnlyOperations<'ext>) : Sum<Option<DateOnly>, Errors> =
      match op with
      | DateOnlyOperations.LessThan v -> v.v1 |> sum.Return
      | _ -> failwith "Expected LessThan operation"

    static member AsLessThanOrEqual(op: DateOnlyOperations<'ext>) : Sum<Option<DateOnly>, Errors> =
      match op with
      | DateOnlyOperations.LessThanOrEqual v -> v.v1 |> sum.Return
      | _ -> failwith "Expected LessThanOrEqual operation"

    static member AsToDateTime(op: DateOnlyOperations<'ext>) : Sum<Option<int * int * int>, Errors> =
      match op with
      | DateOnlyOperations.ToDateTime v -> v.v1 |> sum.Return
      | _ -> failwith "Expected ToDateTime operation"

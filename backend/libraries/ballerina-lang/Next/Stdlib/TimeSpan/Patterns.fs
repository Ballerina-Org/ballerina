namespace Ballerina.DSL.Next.StdLib.TimeSpan

[<AutoOpen>]
module Patterns =
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types
  open System

  type TimeSpanOperations<'ext> with
    static member AsPlus(op: TimeSpanOperations<'ext>) : Sum<Option<TimeSpan>, Errors> =
      match op with
      | TimeSpanOperations.Plus v -> v.v1 |> sum.Return
      | _ -> failwith "Expected Plus operation"

    static member AsMinus(op: TimeSpanOperations<'ext>) : Sum<unit, Errors> =
      match op with
      | TimeSpanOperations.Minus v -> v.v1 |> sum.Return
      | _ -> failwith "Expected Minus operation"

    static member AsEqual(op: TimeSpanOperations<'ext>) : Sum<Option<TimeSpan>, Errors> =
      match op with
      | TimeSpanOperations.Equal v -> v.v1 |> sum.Return
      | _ -> failwith "Expected Equal operation"

    static member AsNotEqual(op: TimeSpanOperations<'ext>) : Sum<Option<TimeSpan>, Errors> =
      match op with
      | TimeSpanOperations.NotEqual v -> v.v1 |> sum.Return
      | _ -> failwith "Expected NotEqual operation"

    static member AsGreaterThan(op: TimeSpanOperations<'ext>) : Sum<Option<TimeSpan>, Errors> =
      match op with
      | TimeSpanOperations.GreaterThan v -> v.v1 |> sum.Return
      | _ -> failwith "Expected GreaterThan operation"

    static member AsGreaterThanOrEqual(op: TimeSpanOperations<'ext>) : Sum<Option<TimeSpan>, Errors> =
      match op with
      | TimeSpanOperations.GreaterThanOrEqual v -> v.v1 |> sum.Return
      | _ -> failwith "Expected GreaterThanOrEqual operation"

    static member AsLessThan(op: TimeSpanOperations<'ext>) : Sum<Option<TimeSpan>, Errors> =
      match op with
      | TimeSpanOperations.LessThan v -> v.v1 |> sum.Return
      | _ -> failwith "Expected LessThan operation"

    static member AsLessThanOrEqual(op: TimeSpanOperations<'ext>) : Sum<Option<TimeSpan>, Errors> =
      match op with
      | TimeSpanOperations.LessThanOrEqual v -> v.v1 |> sum.Return
      | _ -> failwith "Expected LessThanOrEqual operation"

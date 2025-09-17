namespace Ballerina.DSL.Next.StdLib.DateTime

[<AutoOpen>]
module Patterns =
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types
  open System

  type DateTimeOperations<'ext> with

    static member AsEqual(op: DateTimeOperations<'ext>) : Sum<Option<DateTime>, Errors> =
      match op with
      | DateTimeOperations.Equal v -> v.v1 |> sum.Return
      | _ -> failwith "Expected Equal operation"

    static member AsNotEqual(op: DateTimeOperations<'ext>) : Sum<Option<DateTime>, Errors> =
      match op with
      | DateTimeOperations.NotEqual v -> v.v1 |> sum.Return
      | _ -> failwith "Expected NotEqual operation"

    static member AsGreaterThan(op: DateTimeOperations<'ext>) : Sum<Option<DateTime>, Errors> =
      match op with
      | DateTimeOperations.GreaterThan v -> v.v1 |> sum.Return
      | _ -> failwith "Expected GreaterThan operation"

    static member AsGreaterThanOrEqual(op: DateTimeOperations<'ext>) : Sum<Option<DateTime>, Errors> =
      match op with
      | DateTimeOperations.GreaterThanOrEqual v -> v.v1 |> sum.Return
      | _ -> failwith "Expected GreaterThanOrEqual operation"

    static member AsLessThan(op: DateTimeOperations<'ext>) : Sum<Option<DateTime>, Errors> =
      match op with
      | DateTimeOperations.LessThan v -> v.v1 |> sum.Return
      | _ -> failwith "Expected LessThan operation"

    static member AsLessThanOrEqual(op: DateTimeOperations<'ext>) : Sum<Option<DateTime>, Errors> =
      match op with
      | DateTimeOperations.LessThanOrEqual v -> v.v1 |> sum.Return
      | _ -> failwith "Expected LessThanOrEqual operation"

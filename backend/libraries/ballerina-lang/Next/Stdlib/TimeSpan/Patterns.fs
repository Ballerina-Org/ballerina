namespace Ballerina.DSL.Next.StdLib.TimeSpan

[<AutoOpen>]
module Patterns =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types
  open System

  type TimeSpanOperations<'ext> with
    static member AsPlus
      (op: TimeSpanOperations<'ext>)
      : Sum<Option<TimeSpan>, Errors<Unit>> =
      match op with
      | TimeSpanOperations.Plus v -> v.v1 |> sum.Return
      | _ ->
        sum.Throw <| Errors.Singleton () (fun () -> "Expected Plus operation")

    static member AsMinus
      (op: TimeSpanOperations<'ext>)
      : Sum<unit, Errors<Unit>> =
      match op with
      | TimeSpanOperations.Minus v -> v.v1 |> sum.Return
      | _ ->
        sum.Throw <| Errors.Singleton () (fun () -> "Expected Minus operation")

    static member AsEqual
      (op: TimeSpanOperations<'ext>)
      : Sum<Option<TimeSpan>, Errors<Unit>> =
      match op with
      | TimeSpanOperations.Equal v -> v.v1 |> sum.Return
      | _ ->
        sum.Throw <| Errors.Singleton () (fun () -> "Expected Equal operation")

    static member AsNotEqual
      (op: TimeSpanOperations<'ext>)
      : Sum<Option<TimeSpan>, Errors<Unit>> =
      match op with
      | TimeSpanOperations.NotEqual v -> v.v1 |> sum.Return
      | _ ->
        sum.Throw
        <| Errors.Singleton () (fun () -> "Expected NotEqual operation")

    static member AsGreaterThan
      (op: TimeSpanOperations<'ext>)
      : Sum<Option<TimeSpan>, Errors<Unit>> =
      match op with
      | TimeSpanOperations.GreaterThan v -> v.v1 |> sum.Return
      | _ ->
        sum.Throw
        <| Errors.Singleton () (fun () -> "Expected GreaterThan operation")

    static member AsGreaterThanOrEqual
      (op: TimeSpanOperations<'ext>)
      : Sum<Option<TimeSpan>, Errors<Unit>> =
      match op with
      | TimeSpanOperations.GreaterThanOrEqual v -> v.v1 |> sum.Return
      | _ ->
        sum.Throw
        <| Errors.Singleton () (fun () ->
          "Expected GreaterThanOrEqual operation")

    static member AsLessThan
      (op: TimeSpanOperations<'ext>)
      : Sum<Option<TimeSpan>, Errors<Unit>> =
      match op with
      | TimeSpanOperations.LessThan v -> v.v1 |> sum.Return
      | _ ->
        sum.Throw
        <| Errors.Singleton () (fun () -> "Expected LessThan operation")

    static member AsLessThanOrEqual
      (op: TimeSpanOperations<'ext>)
      : Sum<Option<TimeSpan>, Errors<Unit>> =
      match op with
      | TimeSpanOperations.LessThanOrEqual v -> v.v1 |> sum.Return
      | _ ->
        sum.Throw
        <| Errors.Singleton () (fun () -> "Expected LessThanOrEqual operation")

    static member AsFromSeconds
      (op: TimeSpanOperations<'ext>)
      : Sum<Unit, Errors<Unit>> =
      match op with
      | TimeSpanOperations.FromSeconds -> sum.Return()
      | _ ->
        sum.Throw
        <| Errors.Singleton () (fun () -> "Expected FromSeconds operation")

    static member AsFromMinutes
      (op: TimeSpanOperations<'ext>)
      : Sum<Unit, Errors<Unit>> =
      match op with
      | TimeSpanOperations.FromMinutes -> sum.Return()
      | _ ->
        sum.Throw
        <| Errors.Singleton () (fun () -> "Expected FromMinutes operation")

    static member AsFromHours
      (op: TimeSpanOperations<'ext>)
      : Sum<Unit, Errors<Unit>> =
      match op with
      | TimeSpanOperations.FromHours -> sum.Return()
      | _ ->
        sum.Throw
        <| Errors.Singleton () (fun () -> "Expected FromHours operation")

    static member AsFromDays
      (op: TimeSpanOperations<'ext>)
      : Sum<Unit, Errors<Unit>> =
      match op with
      | TimeSpanOperations.FromDays -> sum.Return()
      | _ ->
        sum.Throw
        <| Errors.Singleton () (fun () -> "Expected FromDays operation")

    static member AsToString
      (op: TimeSpanOperations<'ext>)
      : Sum<Unit, Errors<Unit>> =
      match op with
      | TimeSpanOperations.TimeSpan_ToString -> sum.Return()
      | _ ->
        sum.Throw
        <| Errors.Singleton () (fun () ->
          "Expected TimeSpan_ToString operation")

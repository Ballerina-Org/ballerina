namespace Ballerina.DSL.Next.StdLib.Decimal

[<AutoOpen>]
module Patterns =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types

  type DecimalOperations<'ext> with
    static member AsString(op: DecimalOperations<'ext>) : Sum<Unit, Errors<Unit>> =
      match op with
      | DecimalOperations.String -> () |> sum.Return
      | _ -> sum.Throw <| Errors.Singleton () (fun () -> "Expected ToString operation")

    static member AsPlus(op: DecimalOperations<'ext>) : Sum<Option<decimal>, Errors<Unit>> =
      match op with
      | DecimalOperations.Plus v -> v.v1 |> sum.Return
      | _ -> sum.Throw <| Errors.Singleton () (fun () -> "Expected Plus operation")

    static member AsMinus(op: DecimalOperations<'ext>) : Sum<Option<decimal>, Errors<Unit>> =
      match op with
      | DecimalOperations.Minus v -> v.v1 |> sum.Return
      | _ -> sum.Throw <| Errors.Singleton () (fun () -> "Expected Minus operation")

    static member AsDivide(op: DecimalOperations<'ext>) : Sum<Option<decimal>, Errors<Unit>> =
      match op with
      | DecimalOperations.Divide v -> v.v1 |> sum.Return
      | _ -> sum.Throw <| Errors.Singleton () (fun () -> "Expected Divide operation")

    static member AsTimes(op: DecimalOperations<'ext>) : Sum<Option<decimal>, Errors<Unit>> =
      match op with
      | DecimalOperations.Times v -> v.v1 |> sum.Return
      | _ -> sum.Throw <| Errors.Singleton () (fun () -> "Expected Times operation")

    static member AsPower(op: DecimalOperations<'ext>) : Sum<Option<decimal>, Errors<Unit>> =
      match op with
      | DecimalOperations.Power v -> v.v1 |> sum.Return
      | _ -> sum.Throw <| Errors.Singleton () (fun () -> "Expected Power operation")

    static member AsMod(op: DecimalOperations<'ext>) : Sum<Option<decimal>, Errors<Unit>> =
      match op with
      | DecimalOperations.Mod v -> v.v1 |> sum.Return
      | _ -> sum.Throw <| Errors.Singleton () (fun () -> "Expected Mod operation")

    static member AsEqual(op: DecimalOperations<'ext>) : Sum<Option<decimal>, Errors<Unit>> =
      match op with
      | DecimalOperations.Equal v -> v.v1 |> sum.Return
      | _ -> sum.Throw <| Errors.Singleton () (fun () -> "Expected Equal operation")

    static member AsNotEqual(op: DecimalOperations<'ext>) : Sum<Option<decimal>, Errors<Unit>> =
      match op with
      | DecimalOperations.NotEqual v -> v.v1 |> sum.Return
      | _ -> sum.Throw <| Errors.Singleton () (fun () -> "Expected NotEqual operation")

    static member AsGreaterThan(op: DecimalOperations<'ext>) : Sum<Option<decimal>, Errors<Unit>> =
      match op with
      | DecimalOperations.GreaterThan v -> v.v1 |> sum.Return
      | _ -> sum.Throw <| Errors.Singleton () (fun () -> "Expected GreaterThan operation")

    static member AsGreaterThanOrEqual(op: DecimalOperations<'ext>) : Sum<Option<decimal>, Errors<Unit>> =
      match op with
      | DecimalOperations.GreaterThanOrEqual v -> v.v1 |> sum.Return
      | _ ->
        sum.Throw
        <| Errors.Singleton () (fun () -> "Expected GreaterThanOrEqual operation")

    static member AsLessThan(op: DecimalOperations<'ext>) : Sum<Option<decimal>, Errors<Unit>> =
      match op with
      | DecimalOperations.LessThan v -> v.v1 |> sum.Return
      | _ -> sum.Throw <| Errors.Singleton () (fun () -> "Expected LessThan operation")

    static member AsLessThanOrEqual(op: DecimalOperations<'ext>) : Sum<Option<decimal>, Errors<Unit>> =
      match op with
      | DecimalOperations.LessThanOrEqual v -> v.v1 |> sum.Return
      | _ ->
        sum.Throw
        <| Errors.Singleton () (fun () -> "Expected LessThanOrEqual operation")

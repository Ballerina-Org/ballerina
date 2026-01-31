namespace Ballerina.DSL.Next.StdLib.Int32

[<AutoOpen>]
module Patterns =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types

  type Int32Operations<'ext> with
    static member AsPlus(op: Int32Operations<'ext>) : Sum<Option<int32>, Errors<Unit>> =
      match op with
      | Int32Operations.Plus v -> v.v1 |> sum.Return
      | _ -> sum.Throw(Errors.Singleton () (fun () -> "Expected Plus operation"))

    static member AsTimes(op: Int32Operations<'ext>) : Sum<Option<int32>, Errors<Unit>> =
      match op with
      | Int32Operations.Times v -> v.v1 |> sum.Return
      | _ -> sum.Throw(Errors.Singleton () (fun () -> "Expected Times operation"))

    static member AsMinus(op: Int32Operations<'ext>) : Sum<Option<int32>, Errors<Unit>> =
      match op with
      | Int32Operations.Minus v -> v.v1 |> sum.Return
      | _ -> sum.Throw(Errors.Singleton () (fun () -> "Expected Minus operation"))

    static member AsDivide(op: Int32Operations<'ext>) : Sum<Option<int32>, Errors<Unit>> =
      match op with
      | Int32Operations.Divide v -> v.v1 |> sum.Return
      | _ -> sum.Throw(Errors.Singleton () (fun () -> "Expected Divide operation"))

    static member AsPower(op: Int32Operations<'ext>) : Sum<Option<int32>, Errors<Unit>> =
      match op with
      | Int32Operations.Power v -> v.v1 |> sum.Return
      | _ -> sum.Throw(Errors.Singleton () (fun () -> "Expected Power operation"))

    static member AsMod(op: Int32Operations<'ext>) : Sum<Option<int32>, Errors<Unit>> =
      match op with
      | Int32Operations.Mod v -> v.v1 |> sum.Return
      | _ -> sum.Throw(Errors.Singleton () (fun () -> "Expected Mod operation"))

    static member AsEqual(op: Int32Operations<'ext>) : Sum<Option<int32>, Errors<Unit>> =
      match op with
      | Int32Operations.Equal v -> v.v1 |> sum.Return
      | _ -> sum.Throw(Errors.Singleton () (fun () -> "Expected Equal operation"))

    static member AsNotEqual(op: Int32Operations<'ext>) : Sum<Option<int32>, Errors<Unit>> =
      match op with
      | Int32Operations.NotEqual v -> v.v1 |> sum.Return
      | _ -> sum.Throw(Errors.Singleton () (fun () -> "Expected NotEqual operation"))

    static member AsGreaterThan(op: Int32Operations<'ext>) : Sum<Option<int32>, Errors<Unit>> =
      match op with
      | Int32Operations.GreaterThan v -> v.v1 |> sum.Return
      | _ -> sum.Throw(Errors.Singleton () (fun () -> "Expected GreaterThan operation"))

    static member AsGreaterThanOrEqual(op: Int32Operations<'ext>) : Sum<Option<int32>, Errors<Unit>> =
      match op with
      | Int32Operations.GreaterThanOrEqual v -> v.v1 |> sum.Return
      | _ -> sum.Throw(Errors.Singleton () (fun () -> "Expected GreaterThanOrEqual operation"))

    static member AsLessThan(op: Int32Operations<'ext>) : Sum<Option<int32>, Errors<Unit>> =
      match op with
      | Int32Operations.LessThan v -> v.v1 |> sum.Return
      | _ -> sum.Throw(Errors.Singleton () (fun () -> "Expected LessThan operation"))

    static member AsLessThanOrEqual(op: Int32Operations<'ext>) : Sum<Option<int32>, Errors<Unit>> =
      match op with
      | Int32Operations.LessThanOrEqual v -> v.v1 |> sum.Return
      | _ -> sum.Throw(Errors.Singleton () (fun () -> "Expected LessThanOrEqual operation"))

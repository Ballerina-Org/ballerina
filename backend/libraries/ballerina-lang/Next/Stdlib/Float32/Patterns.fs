namespace Ballerina.DSL.Next.StdLib.Float32

[<AutoOpen>]
module Patterns =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types

  type Float32Operations<'ext> with
    static member AsPlus(op: Float32Operations<'ext>) : Sum<Option<float32>, Errors<Unit>> =
      match op with
      | Float32Operations.Plus v -> v.v1 |> sum.Return
      | _ ->
        (fun () -> $"Error: Expected Float32Operations.Plus, found {op}")
        |> Errors.Singleton()
        |> sum.Throw

    static member AsMinus(op: Float32Operations<'ext>) : Sum<Option<float32>, Errors<Unit>> =
      match op with
      | Float32Operations.Minus v -> v.v1 |> sum.Return
      | _ ->
        (fun () -> $"Error: Expected Float32Operations.Minus, found {op}")
        |> Errors.Singleton()
        |> sum.Throw

    static member AsDivide(op: Float32Operations<'ext>) : Sum<Option<float32>, Errors<Unit>> =
      match op with
      | Float32Operations.Divide v -> v.v1 |> sum.Return
      | _ ->
        (fun () -> $"Error: Expected Float32Operations.Divide, found {op}")
        |> Errors.Singleton()
        |> sum.Throw

    static member AsPower(op: Float32Operations<'ext>) : Sum<Option<float32>, Errors<Unit>> =
      match op with
      | Float32Operations.Power v -> v.v1 |> sum.Return
      | _ ->
        (fun () -> $"Error: Expected Float32Operations.Power, found {op}")
        |> Errors.Singleton()
        |> sum.Throw

    static member AsMod(op: Float32Operations<'ext>) : Sum<Option<float32>, Errors<Unit>> =
      match op with
      | Float32Operations.Mod v -> v.v1 |> sum.Return
      | _ ->
        (fun () -> $"Error: Expected Float32Operations.Mod, found {op}")
        |> Errors.Singleton()
        |> sum.Throw

    static member AsEqual(op: Float32Operations<'ext>) : Sum<Option<float32>, Errors<Unit>> =
      match op with
      | Float32Operations.Equal v -> v.v1 |> sum.Return
      | _ ->
        (fun () -> $"Error: Expected Float32Operations.Equal, found {op}")
        |> Errors.Singleton()
        |> sum.Throw

    static member AsNotEqual(op: Float32Operations<'ext>) : Sum<Option<float32>, Errors<Unit>> =
      match op with
      | Float32Operations.NotEqual v -> v.v1 |> sum.Return
      | _ ->
        (fun () -> $"Error: Expected Float32Operations.NotEqual, found {op}")
        |> Errors.Singleton()
        |> sum.Throw

    static member AsGreaterThan(op: Float32Operations<'ext>) : Sum<Option<float32>, Errors<Unit>> =
      match op with
      | Float32Operations.GreaterThan v -> v.v1 |> sum.Return
      | _ ->
        (fun () -> $"Error: Expected Float32Operations.GreaterThan, found {op}")
        |> Errors.Singleton()
        |> sum.Throw

    static member AsGreaterThanOrEqual(op: Float32Operations<'ext>) : Sum<Option<float32>, Errors<Unit>> =
      match op with
      | Float32Operations.GreaterThanOrEqual v -> v.v1 |> sum.Return
      | _ ->
        (fun () -> $"Error: Expected Float32Operations.GreaterThanOrEqual, found {op}")
        |> Errors.Singleton()
        |> sum.Throw

    static member AsLessThan(op: Float32Operations<'ext>) : Sum<Option<float32>, Errors<Unit>> =
      match op with
      | Float32Operations.LessThan v -> v.v1 |> sum.Return
      | _ ->
        (fun () -> $"Error: Expected Float32Operations.LessThan, found {op}")
        |> Errors.Singleton()
        |> sum.Throw

    static member AsLessThanOrEqual(op: Float32Operations<'ext>) : Sum<Option<float32>, Errors<Unit>> =
      match op with
      | Float32Operations.LessThanOrEqual v -> v.v1 |> sum.Return
      | _ ->
        (fun () -> $"Error: Expected Float32Operations.LessThanOrEqual, found {op}")
        |> Errors.Singleton()
        |> sum.Throw

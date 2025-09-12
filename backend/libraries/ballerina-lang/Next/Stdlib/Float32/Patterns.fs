namespace Ballerina.DSL.Next.StdLib.Float32

[<AutoOpen>]
module Patterns =
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types

  type Float32Operations<'ext> with
    static member AsPlus(op: Float32Operations<'ext>) : Sum<Option<float32>, Errors> =
      match op with
      | Float32Operations.Plus v -> v.v1 |> sum.Return
      | _ -> failwith "Expected Plus operation"

    static member AsMinus(op: Float32Operations<'ext>) : Sum<unit, Errors> =
      match op with
      | Float32Operations.Minus v -> v.v1 |> sum.Return
      | _ -> failwith "Expected Minus operation"

    static member AsDivide(op: Float32Operations<'ext>) : Sum<Option<float32>, Errors> =
      match op with
      | Float32Operations.Divide v -> v.v1 |> sum.Return
      | _ -> failwith "Expected Divide operation"

    static member AsPower(op: Float32Operations<'ext>) : Sum<Option<float32>, Errors> =
      match op with
      | Float32Operations.Power v -> v.v1 |> sum.Return
      | _ -> failwith "Expected Power operation"

    static member AsMod(op: Float32Operations<'ext>) : Sum<Option<float32>, Errors> =
      match op with
      | Float32Operations.Mod v -> v.v1 |> sum.Return
      | _ -> failwith "Expected Mod operation"

    static member AsEqual(op: Float32Operations<'ext>) : Sum<Option<float32>, Errors> =
      match op with
      | Float32Operations.Equal v -> v.v1 |> sum.Return
      | _ -> failwith "Expected Equal operation"

    static member AsNotEqual(op: Float32Operations<'ext>) : Sum<Option<float32>, Errors> =
      match op with
      | Float32Operations.NotEqual v -> v.v1 |> sum.Return
      | _ -> failwith "Expected NotEqual operation"

    static member AsGreaterThan(op: Float32Operations<'ext>) : Sum<Option<float32>, Errors> =
      match op with
      | Float32Operations.GreaterThan v -> v.v1 |> sum.Return
      | _ -> failwith "Expected GreaterThan operation"

    static member AsGreaterThanOrEqual(op: Float32Operations<'ext>) : Sum<Option<float32>, Errors> =
      match op with
      | Float32Operations.GreaterThanOrEqual v -> v.v1 |> sum.Return
      | _ -> failwith "Expected GreaterThanOrEqual operation"

    static member AsLessThan(op: Float32Operations<'ext>) : Sum<Option<float32>, Errors> =
      match op with
      | Float32Operations.LessThan v -> v.v1 |> sum.Return
      | _ -> failwith "Expected LessThan operation"

    static member AsLessThanOrEqual(op: Float32Operations<'ext>) : Sum<Option<float32>, Errors> =
      match op with
      | Float32Operations.LessThanOrEqual v -> v.v1 |> sum.Return
      | _ -> failwith "Expected LessThanOrEqual operation"

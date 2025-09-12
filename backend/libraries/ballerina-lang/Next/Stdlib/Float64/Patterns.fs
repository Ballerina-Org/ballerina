namespace Ballerina.DSL.Next.StdLib.Float64

[<AutoOpen>]
module Patterns =
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types

  type Float64Operations<'ext> with
    static member AsPlus(op: Float64Operations<'ext>) : Sum<Option<float>, Errors> =
      match op with
      | Float64Operations.Plus v -> v.v1 |> sum.Return
      | _ -> failwith "Expected Plus operation"

    static member AsMinus(op: Float64Operations<'ext>) : Sum<unit, Errors> =
      match op with
      | Float64Operations.Minus v -> v.v1 |> sum.Return
      | _ -> failwith "Expected Minus operation"

    static member AsDivide(op: Float64Operations<'ext>) : Sum<Option<float>, Errors> =
      match op with
      | Float64Operations.Divide v -> v.v1 |> sum.Return
      | _ -> failwith "Expected Divide operation"

    static member AsPower(op: Float64Operations<'ext>) : Sum<Option<float>, Errors> =
      match op with
      | Float64Operations.Power v -> v.v1 |> sum.Return
      | _ -> failwith "Expected Power operation"

    static member AsMod(op: Float64Operations<'ext>) : Sum<Option<float>, Errors> =
      match op with
      | Float64Operations.Mod v -> v.v1 |> sum.Return
      | _ -> failwith "Expected Mod operation"

    static member AsEqual(op: Float64Operations<'ext>) : Sum<Option<float>, Errors> =
      match op with
      | Float64Operations.Equal v -> v.v1 |> sum.Return
      | _ -> failwith "Expected Equal operation"

    static member AsNotEqual(op: Float64Operations<'ext>) : Sum<Option<float>, Errors> =
      match op with
      | Float64Operations.NotEqual v -> v.v1 |> sum.Return
      | _ -> failwith "Expected NotEqual operation"

    static member AsGreaterThan(op: Float64Operations<'ext>) : Sum<Option<float>, Errors> =
      match op with
      | Float64Operations.GreaterThan v -> v.v1 |> sum.Return
      | _ -> failwith "Expected GreaterThan operation"

    static member AsGreaterThanOrEqual(op: Float64Operations<'ext>) : Sum<Option<float>, Errors> =
      match op with
      | Float64Operations.GreaterThanOrEqual v -> v.v1 |> sum.Return
      | _ -> failwith "Expected GreaterThanOrEqual operation"

    static member AsLessThan(op: Float64Operations<'ext>) : Sum<Option<float>, Errors> =
      match op with
      | Float64Operations.LessThan v -> v.v1 |> sum.Return
      | _ -> failwith "Expected LessThan operation"

    static member AsLessThanOrEqual(op: Float64Operations<'ext>) : Sum<Option<float>, Errors> =
      match op with
      | Float64Operations.LessThanOrEqual v -> v.v1 |> sum.Return
      | _ -> failwith "Expected LessThanOrEqual operation"

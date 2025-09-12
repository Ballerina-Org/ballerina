namespace Ballerina.DSL.Next.StdLib.Int64

[<AutoOpen>]
module Patterns =
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types

  type Int64Operations<'ext> with
    static member AsPlus(op: Int64Operations<'ext>) : Sum<Option<int64>, Errors> =
      match op with
      | Int64Operations.Plus v -> v.v1 |> sum.Return
      | _ -> failwith "Expected Plus operation"

    static member AsMinus(op: Int64Operations<'ext>) : Sum<unit, Errors> =
      match op with
      | Int64Operations.Minus v -> v.v1 |> sum.Return
      | _ -> failwith "Expected Minus operation"

    static member AsDivide(op: Int64Operations<'ext>) : Sum<Option<int64>, Errors> =
      match op with
      | Int64Operations.Divide v -> v.v1 |> sum.Return
      | _ -> failwith "Expected Divide operation"

    static member AsPower(op: Int64Operations<'ext>) : Sum<Option<int64>, Errors> =
      match op with
      | Int64Operations.Power v -> v.v1 |> sum.Return
      | _ -> failwith "Expected Power operation"

    static member AsMod(op: Int64Operations<'ext>) : Sum<Option<int64>, Errors> =
      match op with
      | Int64Operations.Mod v -> v.v1 |> sum.Return
      | _ -> failwith "Expected Mod operation"

    static member AsEqual(op: Int64Operations<'ext>) : Sum<Option<int64>, Errors> =
      match op with
      | Int64Operations.Equal v -> v.v1 |> sum.Return
      | _ -> failwith "Expected Equal operation"

    static member AsNotEqual(op: Int64Operations<'ext>) : Sum<Option<int64>, Errors> =
      match op with
      | Int64Operations.NotEqual v -> v.v1 |> sum.Return
      | _ -> failwith "Expected NotEqual operation"

    static member AsGreaterThan(op: Int64Operations<'ext>) : Sum<Option<int64>, Errors> =
      match op with
      | Int64Operations.GreaterThan v -> v.v1 |> sum.Return
      | _ -> failwith "Expected GreaterThan operation"

    static member AsGreaterThanOrEqual(op: Int64Operations<'ext>) : Sum<Option<int64>, Errors> =
      match op with
      | Int64Operations.GreaterThanOrEqual v -> v.v1 |> sum.Return
      | _ -> failwith "Expected GreaterThanOrEqual operation"

    static member AsLessThan(op: Int64Operations<'ext>) : Sum<Option<int64>, Errors> =
      match op with
      | Int64Operations.LessThan v -> v.v1 |> sum.Return
      | _ -> failwith "Expected LessThan operation"

    static member AsLessThanOrEqual(op: Int64Operations<'ext>) : Sum<Option<int64>, Errors> =
      match op with
      | Int64Operations.LessThanOrEqual v -> v.v1 |> sum.Return
      | _ -> failwith "Expected LessThanOrEqual operation"

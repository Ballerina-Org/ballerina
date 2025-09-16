namespace Ballerina.DSL.Next.StdLib.String

[<AutoOpen>]
module Patterns =
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types

  type StringOperations<'ext> with
    static member AsConcat(op: StringOperations<'ext>) : Sum<Option<string>, Errors> =
      match op with
      | StringOperations.Concat v -> v.v1 |> sum.Return
      | _ -> failwith "Expected Concat operation"

    static member AsEqual(op: StringOperations<'ext>) : Sum<Option<string>, Errors> =
      match op with
      | StringOperations.Equal v -> v.v1 |> sum.Return
      | _ -> failwith "Expected Equal operation"

    static member AsNotEqual(op: StringOperations<'ext>) : Sum<Option<string>, Errors> =
      match op with
      | StringOperations.NotEqual v -> v.v1 |> sum.Return
      | _ -> failwith "Expected NotEqual operation"

    static member AsGreaterThan(op: StringOperations<'ext>) : Sum<Option<string>, Errors> =
      match op with
      | StringOperations.GreaterThan v -> v.v1 |> sum.Return
      | _ -> failwith "Expected GreaterThan operation"

    static member AsGreaterThanOrEqual(op: StringOperations<'ext>) : Sum<Option<string>, Errors> =
      match op with
      | StringOperations.GreaterThanOrEqual v -> v.v1 |> sum.Return
      | _ -> failwith "Expected GreaterThanOrEqual operation"

    static member AsLessThan(op: StringOperations<'ext>) : Sum<Option<string>, Errors> =
      match op with
      | StringOperations.LessThan v -> v.v1 |> sum.Return
      | _ -> failwith "Expected LessThan operation"

    static member AsLessThanOrEqual(op: StringOperations<'ext>) : Sum<Option<string>, Errors> =
      match op with
      | StringOperations.LessThanOrEqual v -> v.v1 |> sum.Return
      | _ -> failwith "Expected LessThanOrEqual operation"

namespace Ballerina.DSL.Next.StdLib.Guid

[<AutoOpen>]
module Patterns =
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types
  open System

  type GuidOperations<'ext> with
    static member AsEqual(op: GuidOperations<'ext>) : Sum<Option<Guid>, Errors> =
      match op with
      | GuidOperations.Equal v -> v.v1 |> sum.Return
      | _ -> failwith "Expected Equal operation"

    static member AsNotEqual(op: GuidOperations<'ext>) : Sum<Option<Guid>, Errors> =
      match op with
      | GuidOperations.NotEqual v -> v.v1 |> sum.Return
      | _ -> failwith "Expected NotEqual operation"

namespace Ballerina.DSL.Next.StdLib.Guid

[<AutoOpen>]
module Patterns =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types
  open System

  type GuidOperations<'ext> with
    static member AsEqual(op: GuidOperations<'ext>) : Sum<Option<Guid>, Errors<Unit>> =
      match op with
      | GuidOperations.Equal v -> v.v1 |> sum.Return
      | _ -> sum.Throw(Errors.Singleton () (fun () -> "Expected Equal operation"))

    static member AsNotEqual(op: GuidOperations<'ext>) : Sum<Option<Guid>, Errors<Unit>> =
      match op with
      | GuidOperations.NotEqual v -> v.v1 |> sum.Return
      | _ -> sum.Throw(Errors.Singleton () (fun () -> "Expected NotEqual operation"))

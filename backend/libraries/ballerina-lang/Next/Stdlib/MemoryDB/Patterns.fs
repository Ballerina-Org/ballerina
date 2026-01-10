namespace Ballerina.DSL.Next.StdLib.MemoryDB

[<AutoOpen>]
module Patterns =
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types
  open System

  type MemoryDBValues<'ext> with
    static member AsRun(op: MemoryDBValues<'ext>) : Sum<Unit, Errors> =
      match op with
      | MemoryDBValues.Run -> () |> sum.Return
      | _ -> Errors.Singleton("Expected Run operation") |> sum.Throw

    static member AsTypeAppliedRun(op: MemoryDBValues<'ext>) : Sum<Schema<'ext>, Errors> =
      match op with
      | MemoryDBValues.TypeAppliedRun s -> s |> sum.Return
      | _ -> Errors.Singleton("Expected TypeAppliedRun operation") |> sum.Throw

    static member AsCreate(op: MemoryDBValues<'ext>) : Sum<Option<Schema<'ext> * SchemaEntity<'ext>>, Errors> =
      match op with
      | MemoryDBValues.Create v -> v.EntityRef |> sum.Return
      | _ -> Errors.Singleton("Expected Create operation") |> sum.Throw

    static member AsUpdate(op: MemoryDBValues<'ext>) : Sum<Option<Schema<'ext> * SchemaEntity<'ext>>, Errors> =
      match op with
      | MemoryDBValues.Update v -> v.EntityRef |> sum.Return
      | _ -> Errors.Singleton("Expected Update operation") |> sum.Throw

    static member AsDelete(op: MemoryDBValues<'ext>) : Sum<Option<Schema<'ext> * SchemaEntity<'ext>>, Errors> =
      match op with
      | MemoryDBValues.Delete v -> v.EntityRef |> sum.Return
      | _ -> Errors.Singleton("Expected Delete operation") |> sum.Throw

    static member AsGetById(op: MemoryDBValues<'ext>) : Sum<Option<Schema<'ext> * SchemaEntity<'ext>>, Errors> =
      match op with
      | MemoryDBValues.GetById v -> v.EntityRef |> sum.Return
      | _ -> Errors.Singleton("Expected GetById operation") |> sum.Throw

    static member AsEntityRef(op: MemoryDBValues<'ext>) : Sum<Schema<'ext> * SchemaEntity<'ext>, Errors> =
      match op with
      | MemoryDBValues.EntityRef(s, e) -> (s, e) |> sum.Return
      | _ -> Errors.Singleton("Expected EntityRef operation") |> sum.Throw

namespace Ballerina.DSL.Next.StdLib.MemoryDB

[<AutoOpen>]
module Patterns =
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types
  open System

  type MemoryDBValues<'ext when 'ext: comparison> with
    static member AsRun(op: MemoryDBValues<'ext>) : Sum<Unit, Errors> =
      match op with
      | MemoryDBValues.Run -> () |> sum.Return
      | _ -> Errors.Singleton("Expected Run operation") |> sum.Throw

    static member AsTypeAppliedRun(op: MemoryDBValues<'ext>) : Sum<Schema<'ext> * MutableMemoryDB<'ext>, Errors> =
      match op with
      | MemoryDBValues.TypeAppliedRun(s, db) -> (s, db) |> sum.Return
      | _ -> Errors.Singleton("Expected TypeAppliedRun operation") |> sum.Throw

    static member AsEvalProperty(op: MemoryDBValues<'ext>) : Sum<MemoryDBEvalProperty<'ext>, Errors> =
      match op with
      | MemoryDBValues.EvalProperty v -> v |> sum.Return
      | _ -> Errors.Singleton("Expected EvalProperty operation") |> sum.Throw

    static member AsStripProperty(op: MemoryDBValues<'ext>) : Sum<MemoryDBEvalProperty<'ext>, Errors> =
      match op with
      | MemoryDBValues.StripProperty v -> v |> sum.Return
      | _ -> Errors.Singleton("Expected StripProperty operation") |> sum.Throw

    static member AsCreate
      (op: MemoryDBValues<'ext>)
      : Sum<Option<Schema<'ext> * MutableMemoryDB<'ext> * SchemaEntity<'ext>>, Errors> =
      match op with
      | MemoryDBValues.Create v -> v.EntityRef |> sum.Return
      | _ -> Errors.Singleton("Expected Create operation") |> sum.Throw

    static member AsUpdate
      (op: MemoryDBValues<'ext>)
      : Sum<Option<Schema<'ext> * MutableMemoryDB<'ext> * SchemaEntity<'ext>>, Errors> =
      match op with
      | MemoryDBValues.Update v -> v.EntityRef |> sum.Return
      | _ -> Errors.Singleton($"Expected Update operation") |> sum.Throw

    static member AsDelete
      (op: MemoryDBValues<'ext>)
      : Sum<Option<Schema<'ext> * MutableMemoryDB<'ext> * SchemaEntity<'ext>>, Errors> =
      match op with
      | MemoryDBValues.Delete v -> v.EntityRef |> sum.Return
      | _ -> Errors.Singleton("Expected Delete operation") |> sum.Throw

    static member AsGetById
      (op: MemoryDBValues<'ext>)
      : Sum<Option<Schema<'ext> * MutableMemoryDB<'ext> * SchemaEntity<'ext>>, Errors> =
      match op with
      | MemoryDBValues.GetById v -> v.EntityRef |> sum.Return
      | _ -> Errors.Singleton("Expected GetById operation") |> sum.Throw

    static member AsEntityRef
      (op: MemoryDBValues<'ext>)
      : Sum<Schema<'ext> * MutableMemoryDB<'ext> * SchemaEntity<'ext>, Errors> =
      match op with
      | MemoryDBValues.EntityRef(s, db, e) -> (s, db, e) |> sum.Return
      | _ -> Errors.Singleton("Expected EntityRef operation") |> sum.Throw

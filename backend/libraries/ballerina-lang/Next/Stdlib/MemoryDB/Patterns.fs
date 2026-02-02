namespace Ballerina.DSL.Next.StdLib.MemoryDB

[<AutoOpen>]
module Patterns =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types
  open System

  type MemoryDBRelation<'ext when 'ext: comparison> with
    static member Empty: MemoryDBRelation<'ext> =
      { All = Set.empty
        FromTo = Map.empty
        ToFrom = Map.empty }

  type MemoryDBValues<'ext when 'ext: comparison> with
    static member AsRun(op: MemoryDBValues<'ext>) : Sum<Unit, Errors<Unit>> =
      match op with
      | MemoryDBValues.Run -> () |> sum.Return
      | _ -> Errors.Singleton () (fun () -> "Expected Run operation") |> sum.Throw

    static member AsTypeAppliedRun(op: MemoryDBValues<'ext>) : Sum<Schema<'ext> * MutableMemoryDB<'ext>, Errors<Unit>> =
      match op with
      | MemoryDBValues.TypeAppliedRun(s, db) -> (s, db) |> sum.Return
      | _ -> Errors.Singleton () (fun () -> "Expected TypeAppliedRun operation") |> sum.Throw

    static member AsEvalProperty(op: MemoryDBValues<'ext>) : Sum<MemoryDBEvalProperty<'ext>, Errors<Unit>> =
      match op with
      | MemoryDBValues.EvalProperty v -> v |> sum.Return
      | _ -> Errors.Singleton () (fun () -> "Expected EvalProperty operation") |> sum.Throw

    static member AsStripProperty(op: MemoryDBValues<'ext>) : Sum<MemoryDBEvalProperty<'ext>, Errors<Unit>> =
      match op with
      | MemoryDBValues.StripProperty v -> v |> sum.Return
      | _ -> Errors.Singleton () (fun () -> "Expected StripProperty operation") |> sum.Throw

    static member AsCreate(op: MemoryDBValues<'ext>) : Sum<Option<EntityRef<'ext>>, Errors<Unit>> =
      match op with
      | MemoryDBValues.Create v -> v.EntityRef |> sum.Return
      | _ -> Errors.Singleton () (fun () -> "Expected Create operation") |> sum.Throw

    static member AsUpdate(op: MemoryDBValues<'ext>) : Sum<Option<EntityRef<'ext>>, Errors<Unit>> =
      match op with
      | MemoryDBValues.Update v -> v.EntityRef |> sum.Return
      | _ -> Errors.Singleton () (fun () -> "Expected Update operation") |> sum.Throw

    static member AsUpsert(op: MemoryDBValues<'ext>) : Sum<Option<EntityRef<'ext>>, Errors<Unit>> =
      match op with
      | MemoryDBValues.Upsert v -> v.EntityRef |> sum.Return
      | _ -> Errors.Singleton () (fun () -> "Expected Upsert operation") |> sum.Throw

    static member AsUpsertMany(op: MemoryDBValues<'ext>) : Sum<Option<EntityRef<'ext>>, Errors<Unit>> =
      match op with
      | MemoryDBValues.UpsertMany v -> v.EntityRef |> sum.Return
      | _ -> Errors.Singleton () (fun () -> "Expected UpsertMany operation") |> sum.Throw

    static member AsUpdateMany(op: MemoryDBValues<'ext>) : Sum<Option<EntityRef<'ext>>, Errors<Unit>> =
      match op with
      | MemoryDBValues.UpdateMany v -> v.EntityRef |> sum.Return
      | _ -> Errors.Singleton () (fun () -> "Expected UpdateMany operation") |> sum.Throw

    static member AsDelete(op: MemoryDBValues<'ext>) : Sum<Option<EntityRef<'ext>>, Errors<Unit>> =
      match op with
      | MemoryDBValues.Delete v -> v.EntityRef |> sum.Return
      | _ -> Errors.Singleton () (fun () -> "Expected Delete operation") |> sum.Throw

    static member AsDeleteMany(op: MemoryDBValues<'ext>) : Sum<Option<EntityRef<'ext>>, Errors<Unit>> =
      match op with
      | MemoryDBValues.DeleteMany v -> v.EntityRef |> sum.Return
      | _ -> Errors.Singleton () (fun () -> "Expected DeleteMany operation") |> sum.Throw

    static member AsGetById(op: MemoryDBValues<'ext>) : Sum<Option<EntityRef<'ext>>, Errors<Unit>> =
      match op with
      | MemoryDBValues.GetById v -> v.EntityRef |> sum.Return
      | _ -> Errors.Singleton () (fun () -> "Expected GetById operation") |> sum.Throw

    static member AsGetMany(op: MemoryDBValues<'ext>) : Sum<Option<EntityRef<'ext>>, Errors<Unit>> =
      match op with
      | MemoryDBValues.GetMany v -> v.EntityRef |> sum.Return
      | _ -> Errors.Singleton () (fun () -> "Expected GetMany operation") |> sum.Throw

    static member AsEntityRef(op: MemoryDBValues<'ext>) : Sum<EntityRef<'ext>, Errors<Unit>> =
      match op with
      | MemoryDBValues.EntityRef e -> e |> sum.Return
      | _ -> Errors.Singleton () (fun () -> "Expected EntityRef operation") |> sum.Throw

    static member AsRelationRef(op: MemoryDBValues<'ext>) : Sum<RelationRef<'ext>, Errors<Unit>> =
      match op with
      | MemoryDBValues.RelationRef(s, db, r, f, t, sv) -> (s, db, r, f, t, sv) |> sum.Return
      | _ -> Errors.Singleton () (fun () -> "Expected RelationRef operation") |> sum.Throw

    static member AsRelationLookupRef
      (op: MemoryDBValues<'ext>)
      : Sum<
          Schema<'ext> *
          MutableMemoryDB<'ext> *
          RelationLookupDirection *
          SchemaRelation<'ext> *
          SchemaEntity<'ext> *
          SchemaEntity<'ext>,
          Errors<Unit>
         >
      =
      match op with
      | MemoryDBValues.RelationLookupRef(s, db, dir, r, f, t) -> (s, db, dir, r, f, t) |> sum.Return
      | _ ->
        Errors.Singleton () (fun () -> "Expected RelationLookupRef operation")
        |> sum.Throw


    static member AsLink(op: MemoryDBValues<'ext>) : Sum<{| RelationRef: Option<RelationRef<'ext>> |}, Errors<Unit>> =
      match op with
      | MemoryDBValues.Link link -> link |> sum.Return
      | _ -> Errors.Singleton () (fun () -> "Expected Link operation") |> sum.Throw

    static member AsUnlink(op: MemoryDBValues<'ext>) : Sum<{| RelationRef: Option<RelationRef<'ext>> |}, Errors<Unit>> =
      match op with
      | MemoryDBValues.Unlink unlink -> unlink |> sum.Return
      | _ -> Errors.Singleton () (fun () -> "Expected Unlink operation") |> sum.Throw

    static member AsLinkMany
      (op: MemoryDBValues<'ext>)
      : Sum<{| RelationRef: Option<RelationRef<'ext>> |}, Errors<Unit>> =
      match op with
      | MemoryDBValues.LinkMany linkMany -> linkMany |> sum.Return
      | _ -> Errors.Singleton () (fun () -> "Expected LinkMany operation") |> sum.Throw

    static member AsUnlinkMany
      (op: MemoryDBValues<'ext>)
      : Sum<{| RelationRef: Option<RelationRef<'ext>> |}, Errors<Unit>> =
      match op with
      | MemoryDBValues.UnlinkMany unlinkMany -> unlinkMany |> sum.Return
      | _ -> Errors.Singleton () (fun () -> "Expected UnlinkMany operation") |> sum.Throw

    static member AsLookupOne
      (op: MemoryDBValues<'ext>)
      : Sum<{| RelationRef: Option<RelationLookupRef<'ext>> |}, Errors<Unit>> =
      match op with
      | MemoryDBValues.LookupOne lookupOne -> lookupOne |> sum.Return
      | _ -> Errors.Singleton () (fun () -> "Expected LookupOne operation") |> sum.Throw

    static member AsLookupOption
      (op: MemoryDBValues<'ext>)
      : Sum<{| RelationRef: Option<RelationLookupRef<'ext>> |}, Errors<Unit>> =
      match op with
      | MemoryDBValues.LookupOption lookupOption -> lookupOption |> sum.Return
      | _ -> Errors.Singleton () (fun () -> "Expected LookupOption operation") |> sum.Throw

    static member AsLookupMany
      (op: MemoryDBValues<'ext>)
      : Sum<
          {| RelationRef: Option<RelationLookupRef<'ext>>
             EntityId: Option<_> |},
          Errors<Unit>
         >
      =
      match op with
      | MemoryDBValues.LookupMany lookupMany -> lookupMany |> sum.Return
      | _ -> Errors.Singleton () (fun () -> "Expected LookupMany operation") |> sum.Throw

    static member AsDBIO(op: MemoryDBValues<'ext>) : Sum<MemoryDBIO<'ext>, Errors<Unit>> =
      match op with
      | MemoryDBValues.DBIO dbio -> dbio |> sum.Return
      | _ -> Errors.Singleton () (fun () -> "Expected DBIO operation") |> sum.Throw

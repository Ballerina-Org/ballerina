namespace Ballerina.DSL.Next.StdLib.DB

[<AutoOpen>]
module Patterns =
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types
  open System

  type DBValues<'runtimeContext, 'db, 'ext when 'ext: comparison> with
    static member AsRun
      (op: DBValues<'runtimeContext, 'db, 'ext>)
      : Sum<Unit, Errors<Unit>> =
      match op with
      | DBValues.Run -> () |> sum.Return
      | _ ->
        Errors.Singleton () (fun () -> "Expected Run operation") |> sum.Throw

    static member AsTypeAppliedRun
      (op: DBValues<'runtimeContext, 'db, 'ext>)
      : Sum<Schema<'ext> * 'db, Errors<Unit>> =
      match op with
      | DBValues.TypeAppliedRun(s, db) -> (s, db) |> sum.Return
      | _ ->
        Errors.Singleton () (fun () -> "Expected TypeAppliedRun operation")
        |> sum.Throw

    static member AsEvalProperty
      (op: DBValues<'runtimeContext, 'db, 'ext>)
      : Sum<DBEvalProperty<'ext>, Errors<Unit>> =
      match op with
      | DBValues.EvalProperty v -> v |> sum.Return
      | _ ->
        Errors.Singleton () (fun () -> "Expected EvalProperty operation")
        |> sum.Throw

    static member AsStripProperty
      (op: DBValues<'runtimeContext, 'db, 'ext>)
      : Sum<DBEvalProperty<'ext>, Errors<Unit>> =
      match op with
      | DBValues.StripProperty v -> v |> sum.Return
      | _ ->
        Errors.Singleton () (fun () -> "Expected StripProperty operation")
        |> sum.Throw

    static member AsCreate
      (op: DBValues<'runtimeContext, 'db, 'ext>)
      : Sum<Option<EntityRef<'db, 'ext>>, Errors<Unit>> =
      match op with
      | DBValues.Create v -> v.EntityRef |> sum.Return
      | _ ->
        Errors.Singleton () (fun () -> "Expected Create operation") |> sum.Throw

    static member AsUpdate
      (op: DBValues<'runtimeContext, 'db, 'ext>)
      : Sum<Option<EntityRef<'db, 'ext>>, Errors<Unit>> =
      match op with
      | DBValues.Update v -> v.EntityRef |> sum.Return
      | _ ->
        Errors.Singleton () (fun () -> "Expected Update operation") |> sum.Throw

    static member AsUpsert
      (op: DBValues<'runtimeContext, 'db, 'ext>)
      : Sum<Option<EntityRef<'db, 'ext>>, Errors<Unit>> =
      match op with
      | DBValues.Upsert v -> v.EntityRef |> sum.Return
      | _ ->
        Errors.Singleton () (fun () -> "Expected Upsert operation") |> sum.Throw

    static member AsUpsertMany
      (op: DBValues<'runtimeContext, 'db, 'ext>)
      : Sum<Option<EntityRef<'db, 'ext>>, Errors<Unit>> =
      match op with
      | DBValues.UpsertMany v -> v.EntityRef |> sum.Return
      | _ ->
        Errors.Singleton () (fun () -> "Expected UpsertMany operation")
        |> sum.Throw

    static member AsUpdateMany
      (op: DBValues<'runtimeContext, 'db, 'ext>)
      : Sum<Option<EntityRef<'db, 'ext>>, Errors<Unit>> =
      match op with
      | DBValues.UpdateMany v -> v.EntityRef |> sum.Return
      | _ ->
        Errors.Singleton () (fun () -> "Expected UpdateMany operation")
        |> sum.Throw

    static member AsDelete
      (op: DBValues<'runtimeContext, 'db, 'ext>)
      : Sum<Option<EntityRef<'db, 'ext>>, Errors<Unit>> =
      match op with
      | DBValues.Delete v -> v.EntityRef |> sum.Return
      | _ ->
        Errors.Singleton () (fun () -> "Expected Delete operation") |> sum.Throw

    static member AsDeleteMany
      (op: DBValues<'runtimeContext, 'db, 'ext>)
      : Sum<Option<EntityRef<'db, 'ext>>, Errors<Unit>> =
      match op with
      | DBValues.DeleteMany v -> v.EntityRef |> sum.Return
      | _ ->
        Errors.Singleton () (fun () -> "Expected DeleteMany operation")
        |> sum.Throw

    static member AsGetById
      (op: DBValues<'runtimeContext, 'db, 'ext>)
      : Sum<Option<EntityRef<'db, 'ext>>, Errors<Unit>> =
      match op with
      | DBValues.GetById v -> v.EntityRef |> sum.Return
      | _ ->
        Errors.Singleton () (fun () -> "Expected GetById operation")
        |> sum.Throw

    static member AsGetMany
      (op: DBValues<'runtimeContext, 'db, 'ext>)
      : Sum<Option<EntityRef<'db, 'ext>>, Errors<Unit>> =
      match op with
      | DBValues.GetMany v -> v.EntityRef |> sum.Return
      | _ ->
        Errors.Singleton () (fun () -> "Expected GetMany operation")
        |> sum.Throw

    static member AsCalculateProps
      (op: DBValues<'runtimeContext, 'db, 'ext>)
      : Sum<Option<EntityRef<'db, 'ext>>, Errors<Unit>> =
      match op with
      | DBValues.CalculateProps v -> v.EntityRef |> sum.Return
      | _ ->
        Errors.Singleton () (fun () -> "Expected CalculateProps operation")
        |> sum.Throw

    static member AsStripProps
      (op: DBValues<'runtimeContext, 'db, 'ext>)
      : Sum<Option<EntityRef<'db, 'ext>>, Errors<Unit>> =
      match op with
      | DBValues.StripProps v -> v.EntityRef |> sum.Return
      | _ ->
        Errors.Singleton () (fun () -> "Expected StripProps operation")
        |> sum.Throw

    static member AsEntityRef
      (op: DBValues<'runtimeContext, 'db, 'ext>)
      : Sum<EntityRef<'db, 'ext>, Errors<Unit>> =
      match op with
      | DBValues.EntityRef e -> e |> sum.Return
      | _ ->
        Errors.Singleton () (fun () -> "Expected EntityRef operation")
        |> sum.Throw

    static member AsRelationRef
      (op: DBValues<'runtimeContext, 'db, 'ext>)
      : Sum<RelationRef<'db, 'ext>, Errors<Unit>> =
      match op with
      | DBValues.RelationRef(s, db, r, f, t, sv) ->
        (s, db, r, f, t, sv) |> sum.Return
      | _ ->
        Errors.Singleton () (fun () -> "Expected RelationRef operation")
        |> sum.Throw

    static member AsRelationLookupRef
      (op: DBValues<'runtimeContext, 'db, 'ext>)
      : Sum<RelationRef<'db, 'ext> * RelationLookupDirection, Errors<Unit>> =
      match op with
      | DBValues.RelationLookupRef(r, d) -> (r, d) |> sum.Return
      | _ ->
        Errors.Singleton () (fun () -> "Expected RelationLookupRef operation")
        |> sum.Throw


    static member AsLink
      (op: DBValues<'runtimeContext, 'db, 'ext>)
      : Sum<{| RelationRef: Option<RelationRef<'db, 'ext>> |}, Errors<Unit>> =
      match op with
      | DBValues.Link link -> link |> sum.Return
      | _ ->
        Errors.Singleton () (fun () -> "Expected Link operation") |> sum.Throw

    static member AsUnlink
      (op: DBValues<'runtimeContext, 'db, 'ext>)
      : Sum<{| RelationRef: Option<RelationRef<'db, 'ext>> |}, Errors<Unit>> =
      match op with
      | DBValues.Unlink unlink -> unlink |> sum.Return
      | _ ->
        Errors.Singleton () (fun () -> "Expected Unlink operation") |> sum.Throw

    static member AsIsLinked
      (op: DBValues<'runtimeContext, 'db, 'ext>)
      : Sum<{| RelationRef: Option<RelationRef<'db, 'ext>> |}, Errors<Unit>> =
      match op with
      | DBValues.IsLinked isLinked -> isLinked |> sum.Return
      | _ ->
        Errors.Singleton () (fun () -> "Expected IsLinked operation")
        |> sum.Throw

    static member AsLinkMany
      (op: DBValues<'runtimeContext, 'db, 'ext>)
      : Sum<{| RelationRef: Option<RelationRef<'db, 'ext>> |}, Errors<Unit>> =
      match op with
      | DBValues.LinkMany linkMany -> linkMany |> sum.Return
      | _ ->
        Errors.Singleton () (fun () -> "Expected LinkMany operation")
        |> sum.Throw

    static member AsUnlinkMany
      (op: DBValues<'runtimeContext, 'db, 'ext>)
      : Sum<{| RelationRef: Option<RelationRef<'db, 'ext>> |}, Errors<Unit>> =
      match op with
      | DBValues.UnlinkMany unlinkMany -> unlinkMany |> sum.Return
      | _ ->
        Errors.Singleton () (fun () -> "Expected UnlinkMany operation")
        |> sum.Throw

    static member AsLookupOne
      (op: DBValues<'runtimeContext, 'db, 'ext>)
      : Sum<
          {| RelationRef:
               Option<RelationRef<'db, 'ext> * RelationLookupDirection> |},
          Errors<Unit>
         >
      =
      match op with
      | DBValues.LookupOne lookupOne -> lookupOne |> sum.Return
      | _ ->
        Errors.Singleton () (fun () -> "Expected LookupOne operation")
        |> sum.Throw

    static member AsLookupOption
      (op: DBValues<'runtimeContext, 'db, 'ext>)
      : Sum<
          {| RelationRef:
               Option<RelationRef<'db, 'ext> * RelationLookupDirection> |},
          Errors<Unit>
         >
      =
      match op with
      | DBValues.LookupOption lookupOption -> lookupOption |> sum.Return
      | _ ->
        Errors.Singleton () (fun () -> "Expected LookupOption operation")
        |> sum.Throw

    static member AsLookupMany
      (op: DBValues<'runtimeContext, 'db, 'ext>)
      : Sum<
          {| RelationRef:
               Option<RelationRef<'db, 'ext> * RelationLookupDirection>
             EntityId: Option<_> |},
          Errors<Unit>
         >
      =
      match op with
      | DBValues.LookupMany lookupMany -> lookupMany |> sum.Return
      | _ ->
        Errors.Singleton () (fun () -> "Expected LookupMany operation")
        |> sum.Throw

    static member AsDBIO
      (op: DBValues<'runtimeContext, 'db, 'ext>)
      : Sum<DBIO<'runtimeContext, 'db, 'ext>, Errors<Unit>> =
      match op with
      | DBValues.DBIO dbio -> dbio |> sum.Return
      | _ ->
        Errors.Singleton () (fun () -> "Expected DBIO operation") |> sum.Throw

    static member AsQueryRun
      (op: DBValues<'runtimeContext, 'db, 'ext>)
      : Sum<Option<ValueQuery<TypeValue<'ext>, 'ext>>, Errors<Unit>> =
      match op with
      | DBValues.QueryRun q -> q.Query |> sum.Return
      | _ ->
        Errors.Singleton () (fun () -> "Expected QueryRun operation")
        |> sum.Throw

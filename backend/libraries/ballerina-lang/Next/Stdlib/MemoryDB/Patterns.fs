namespace Ballerina.DSL.Next.StdLib.MemoryDB

[<AutoOpen>]
module Patterns =
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


    static member AsRelationRef
      (op: MemoryDBValues<'ext>)
      : Sum<Schema<'ext> * MutableMemoryDB<'ext> * SchemaRelation * SchemaEntity<'ext> * SchemaEntity<'ext>, Errors> =
      match op with
      | MemoryDBValues.RelationRef(s, db, r, f, t) -> (s, db, r, f, t) |> sum.Return
      | _ -> Errors.Singleton("Expected RelationRef operation") |> sum.Throw

    static member AsRelationLookupRef
      (op: MemoryDBValues<'ext>)
      : Sum<
          Schema<'ext> *
          MutableMemoryDB<'ext> *
          RelationLookupDirection *
          SchemaRelation *
          SchemaEntity<'ext> *
          SchemaEntity<'ext>,
          Errors
         >
      =
      match op with
      | MemoryDBValues.RelationLookupRef(s, db, dir, r, f, t) -> (s, db, dir, r, f, t) |> sum.Return
      | _ -> Errors.Singleton("Expected RelationLookupRef operation") |> sum.Throw


    static member AsLink
      (op: MemoryDBValues<'ext>)
      : Sum<
          {| RelationRef:
               Option<Schema<'ext> * MutableMemoryDB<'ext> * SchemaRelation * SchemaEntity<'ext> * SchemaEntity<'ext>> |},
          Errors
         >
      =
      match op with
      | MemoryDBValues.Link link -> link |> sum.Return
      | _ -> Errors.Singleton("Expected Link operation") |> sum.Throw


    static member AsUnlink
      (op: MemoryDBValues<'ext>)
      : Sum<
          {| RelationRef:
               Option<Schema<'ext> * MutableMemoryDB<'ext> * SchemaRelation * SchemaEntity<'ext> * SchemaEntity<'ext>> |},
          Errors
         >
      =
      match op with
      | MemoryDBValues.Unlink unlink -> unlink |> sum.Return
      | _ -> Errors.Singleton("Expected Unlink operation") |> sum.Throw


    static member AsLookupOne
      (op: MemoryDBValues<'ext>)
      : Sum<{| RelationRef: Option<RelationLookupRef<'ext>> |}, Errors> =
      match op with
      | MemoryDBValues.LookupOne lookupOne -> lookupOne |> sum.Return
      | _ -> Errors.Singleton("Expected LookupOne operation") |> sum.Throw


    static member AsLookupOption
      (op: MemoryDBValues<'ext>)
      : Sum<{| RelationRef: Option<RelationLookupRef<'ext>> |}, Errors> =
      match op with
      | MemoryDBValues.LookupOption lookupOption -> lookupOption |> sum.Return
      | _ -> Errors.Singleton("Expected LookupOption operation") |> sum.Throw


    static member AsLookupMany
      (op: MemoryDBValues<'ext>)
      : Sum<{| RelationRef: Option<RelationLookupRef<'ext>> |}, Errors> =
      match op with
      | MemoryDBValues.LookupMany lookupMany -> lookupMany |> sum.Return
      | _ -> Errors.Singleton("Expected LookupMany operation") |> sum.Throw

namespace Ballerina.DSL.Next.StdLib.MemoryDB

[<AutoOpen>]
module Model =
  open System
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.Terms
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open Ballerina.LocalizedErrors

  type MemoryDBRelation<'ext when 'ext: comparison> =
    { All: Set<Value<TypeValue<'ext>, 'ext> * Value<TypeValue<'ext>, 'ext>>
      FromTo: Map<Value<TypeValue<'ext>, 'ext>, Set<Value<TypeValue<'ext>, 'ext>>>
      ToFrom: Map<Value<TypeValue<'ext>, 'ext>, Set<Value<TypeValue<'ext>, 'ext>>> }

  type MutableMemoryDB<'ext when 'ext: comparison> =
    { mutable entities: Map<SchemaEntityName, Map<Value<TypeValue<'ext>, 'ext>, Value<TypeValue<'ext>, 'ext>>>
      mutable relations: Map<SchemaRelationName, MemoryDBRelation<'ext>> }

  type MemoryDBEvalProperty<'ext> =
    { PropertyName: LocalIdentifier
      Path: SchemaPath<'ext>
      Body: Expr<TypeValue<'ext>, ResolvedIdentifier, 'ext> }

  [<CustomEquality; CustomComparison>]
  type SchemaAsValue<'ext when 'ext: comparison> =
    { Value: Lazy<Value<TypeValue<'ext>, 'ext>> }

    override x.ToString() = $"SchemaAsValue(Schema:\n{x.Value})"

    override x.Equals(yobj) =
      match yobj with
      | :? SchemaAsValue<'ext> as y -> (x.Value = y.Value)
      | _ -> false

    override x.GetHashCode() = hash x.Value.Value

    interface System.IComparable with
      member x.CompareTo yobj =
        match yobj with
        | :? SchemaAsValue<'ext> as y -> compare x.Value.Value y.Value.Value
        | _ -> invalidArg "yobj" "cannot compare values of different types"


  type EntityRef<'ext when 'ext: comparison> =
    Schema<'ext> * MutableMemoryDB<'ext> * SchemaEntity<'ext> * SchemaAsValue<'ext>

  type RelationRef<'ext when 'ext: comparison> =
    Schema<'ext> *
    MutableMemoryDB<'ext> *
    SchemaRelation<'ext> *
    SchemaEntity<'ext> *
    SchemaEntity<'ext> *
    SchemaAsValue<'ext>

  type RelationLookupRef<'ext when 'ext: comparison> =
    Schema<'ext> *
    MutableMemoryDB<'ext> *
    RelationLookupDirection *
    SchemaRelation<'ext> *
    SchemaEntity<'ext> *
    SchemaEntity<'ext>

  [<CustomEquality; CustomComparison>]
  type MemoryDBIO<'ext when 'ext: comparison> =
    { Schema: Schema<'ext>
      SchemaAsValue: Value<TypeValue<'ext>, 'ext>
      DB: MutableMemoryDB<'ext>
      EvalContext: ExprEvalContextScope<'ext>
      Main: Value<TypeValue<'ext>, 'ext> }


    override x.ToString() = $"MemoryDBIO(Schema:\n{x.Schema})"

    override x.Equals(yobj) =
      match yobj with
      | :? MemoryDBIO<'ext> as y ->
        (x.Schema = y.Schema
         && x.DB = y.DB
         && x.Main = y.Main
         && x.SchemaAsValue = y.SchemaAsValue)
      | _ -> false

    override x.GetHashCode() =
      hash x.Schema ^^^ hash x.DB ^^^ hash x.Main ^^^ hash x.SchemaAsValue

    interface System.IComparable with
      member x.CompareTo yobj =
        match yobj with
        | :? MemoryDBIO<'ext> as y -> compare (x.Schema, x.SchemaAsValue, x.Main) (y.Schema, y.SchemaAsValue, y.Main)
        | _ -> invalidArg "yobj" "cannot compare values of different types"


  type MemoryDBValues<'ext when 'ext: comparison> =
    | EntityRef of EntityRef<'ext>
    | RelationRef of RelationRef<'ext>
    | Link of {| RelationRef: Option<RelationRef<'ext>> |}
    | Unlink of {| RelationRef: Option<RelationRef<'ext>> |}
    | LinkMany of {| RelationRef: Option<RelationRef<'ext>> |}
    | UnlinkMany of {| RelationRef: Option<RelationRef<'ext>> |}
    | LookupOne of {| RelationRef: Option<RelationLookupRef<'ext>> |}
    | LookupOption of {| RelationRef: Option<RelationLookupRef<'ext>> |}
    | LookupMany of
      {| RelationRef: Option<RelationLookupRef<'ext>>
         EntityId: Option<Value<TypeValue<'ext>, 'ext>> |}
    | RelationLookupRef of RelationLookupRef<'ext>
    | EvalProperty of MemoryDBEvalProperty<'ext>
    | StripProperty of MemoryDBEvalProperty<'ext>
    | Create of {| EntityRef: Option<EntityRef<'ext>> |}
    | Update of {| EntityRef: Option<EntityRef<'ext>> |}
    | Upsert of {| EntityRef: Option<EntityRef<'ext>> |}
    | UpsertMany of {| EntityRef: Option<EntityRef<'ext>> |}
    | UpdateMany of {| EntityRef: Option<EntityRef<'ext>> |}
    | DeleteMany of {| EntityRef: Option<EntityRef<'ext>> |}
    | Delete of {| EntityRef: Option<EntityRef<'ext>> |}
    | GetById of {| EntityRef: Option<EntityRef<'ext>> |}
    | GetMany of {| EntityRef: Option<EntityRef<'ext>> |}
    | Run
    | DBIO of MemoryDBIO<'ext>
    | TypeAppliedRun of Schema<'ext> * MutableMemoryDB<'ext>

    override this.ToString() =
      match this with
      | EntityRef(_, _, entity, _) -> $"EntityRef({entity.Name})"
      | RelationRef(_, _, relation, fromEntity, toEntity, _) ->
        $"RelationRef({relation.Name}, from: {fromEntity.Name}, to: {toEntity.Name})"
      | Link link ->
        let relationStr =
          match link.RelationRef with
          | Some(_, _, relation, fromEntity, toEntity, _) ->
            $"RelationRef({relation.Name}, from: {fromEntity.Name}, to: {toEntity.Name})"
          | None -> "None"

        $"Link(Relation: {relationStr})"
      | Unlink unlink ->
        let relationStr =
          match unlink.RelationRef with
          | Some(_, _, relation, fromEntity, toEntity, _) ->
            $"RelationRef({relation.Name}, from: {fromEntity.Name}, to: {toEntity.Name})"
          | None -> "None"

        $"Unlink(Relation: {relationStr})"
      | LinkMany linkMany ->
        let relationStr =
          match linkMany.RelationRef with
          | Some(_, _, relation, fromEntity, toEntity, _) ->
            $"RelationRef({relation.Name}, from: {fromEntity.Name}, to: {toEntity.Name})"
          | None -> "None"

        $"LinkMany(Relation: {relationStr})"
      | UnlinkMany unlinkMany ->
        let relationStr =
          match unlinkMany.RelationRef with
          | Some(_, _, relation, fromEntity, toEntity, _) ->
            $"RelationRef({relation.Name}, from: {fromEntity.Name}, to: {toEntity.Name})"
          | None -> "None"

        $"UnlinkMany(Relation: {relationStr})"
      | LookupOne lookupOne ->
        let relationStr =
          match lookupOne.RelationRef with
          | Some(_, _, _, relation, fromEntity, toEntity) ->
            $"RelationRef({relation.Name}, from: {fromEntity.Name}, to: {toEntity.Name})"
          | None -> "None"

        $"LookupOne(Relation: {relationStr})"
      | LookupOption lookupOption ->
        let relationStr =
          match lookupOption.RelationRef with
          | Some(_, _, _, relation, fromEntity, toEntity) ->
            $"RelationRef({relation.Name}, from: {fromEntity.Name}, to: {toEntity.Name})"
          | None -> "None"

        $"LookupOption(Relation: {relationStr})"
      | LookupMany lookupMany ->
        let relationStr =
          match lookupMany.RelationRef with
          | Some(_, _, _, relation, fromEntity, toEntity) ->
            $"RelationRef({relation.Name}, from: {fromEntity.Name}, to: {toEntity.Name})"
          | None -> "None"

        $"LookupMany(Relation: {relationStr})"
      | RelationLookupRef(_, _, direction, relation, fromEntity, toEntity) ->
        $"RelationLookupRef({relation.Name}, direction: {direction}, from: {fromEntity.Name}, to: {toEntity.Name})"
      | EvalProperty prop -> $"EvalProperty({prop.PropertyName})"
      | StripProperty prop -> $"StripProperty({prop.PropertyName})"
      | Create _ -> "Create"
      | Update _ -> "Update"
      | Upsert _ -> "Upsert"
      | UpsertMany _ -> "UpsertMany"
      | UpdateMany _ -> "UpdateMany"
      | DeleteMany _ -> "DeleteMany"
      | Delete _ -> "Delete"
      | GetById _ -> "GetById"
      | GetMany _ -> "GetMany"
      | Run -> "Run"
      | DBIO dbio -> $"DBIO({dbio})"
      | TypeAppliedRun(_schema, _) -> $"TypeAppliedRun)"

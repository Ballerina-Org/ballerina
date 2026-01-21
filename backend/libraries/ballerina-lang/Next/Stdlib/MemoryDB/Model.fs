namespace Ballerina.DSL.Next.StdLib.MemoryDB

[<AutoOpen>]
module Model =
  open System
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.Terms

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

  type RelationRef<'ext when 'ext: comparison> =
    Schema<'ext> * MutableMemoryDB<'ext> * SchemaRelation * SchemaEntity<'ext> * SchemaEntity<'ext>

  type EntityRef<'ext when 'ext: comparison> = Schema<'ext> * MutableMemoryDB<'ext> * SchemaEntity<'ext>

  type RelationLookupRef<'ext when 'ext: comparison> =
    Schema<'ext> *
    MutableMemoryDB<'ext> *
    RelationLookupDirection *
    SchemaRelation *
    SchemaEntity<'ext> *
    SchemaEntity<'ext>

  type MemoryDBValues<'ext when 'ext: comparison> =
    | EntityRef of Schema<'ext> * MutableMemoryDB<'ext> * SchemaEntity<'ext>
    | RelationRef of Schema<'ext> * MutableMemoryDB<'ext> * SchemaRelation * SchemaEntity<'ext> * SchemaEntity<'ext>
    | Link of {| RelationRef: Option<RelationRef<'ext>> |}
    | Unlink of {| RelationRef: Option<RelationRef<'ext>> |}
    | LookupOne of {| RelationRef: Option<RelationLookupRef<'ext>> |}
    | LookupOption of {| RelationRef: Option<RelationLookupRef<'ext>> |}
    | LookupMany of {| RelationRef: Option<RelationLookupRef<'ext>> |}
    | RelationLookupRef of RelationLookupRef<'ext>
    | EvalProperty of MemoryDBEvalProperty<'ext>
    | StripProperty of MemoryDBEvalProperty<'ext>
    | Create of {| EntityRef: Option<EntityRef<'ext>> |}
    | Update of {| EntityRef: Option<EntityRef<'ext>> |}
    | Delete of {| EntityRef: Option<EntityRef<'ext>> |}
    | GetById of {| EntityRef: Option<EntityRef<'ext>> |}
    | Run
    | TypeAppliedRun of Schema<'ext> * MutableMemoryDB<'ext>

    override this.ToString() =
      match this with
      | EntityRef(_, _, entity) -> $"EntityRef({entity.Name})"
      | RelationRef(_, _, relation, fromEntity, toEntity) ->
        $"RelationRef({relation.Name}, from: {fromEntity.Name}, to: {toEntity.Name})"
      | Link link ->
        let relationStr =
          match link.RelationRef with
          | Some(_, _, relation, fromEntity, toEntity) ->
            $"RelationRef({relation.Name}, from: {fromEntity.Name}, to: {toEntity.Name})"
          | None -> "None"

        $"Link(Relation: {relationStr})"
      | Unlink unlink ->
        let relationStr =
          match unlink.RelationRef with
          | Some(_, _, relation, fromEntity, toEntity) ->
            $"RelationRef({relation.Name}, from: {fromEntity.Name}, to: {toEntity.Name})"
          | None -> "None"

        $"Unlink(Relation: {relationStr})"
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
      | Delete _ -> "Delete"
      | GetById _ -> "GetById"
      | Run -> "Run"
      | TypeAppliedRun(_schema, _) -> $"TypeAppliedRun)"

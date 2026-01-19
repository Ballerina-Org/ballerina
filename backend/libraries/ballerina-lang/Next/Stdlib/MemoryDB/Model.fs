namespace Ballerina.DSL.Next.StdLib.MemoryDB

[<AutoOpen>]
module Model =
  open System
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.Terms

  type MemoryDBRelation<'ext when 'ext: comparison> =
    { All: Set<Value<TypeValue<'ext>, 'ext> * Value<TypeValue<'ext>, 'ext>>
      From: Map<Value<TypeValue<'ext>, 'ext>, Set<Value<TypeValue<'ext>, 'ext>>>
      To: Map<Value<TypeValue<'ext>, 'ext>, Set<Value<TypeValue<'ext>, 'ext>>> }

  type MutableMemoryDB<'ext when 'ext: comparison> =
    { mutable entities: Map<SchemaEntityName, Map<Value<TypeValue<'ext>, 'ext>, Value<TypeValue<'ext>, 'ext>>>
      mutable relations: Map<SchemaRelationName, MemoryDBRelation<'ext>> }

  type MemoryDBEvalProperty<'ext> =
    { PropertyName: LocalIdentifier
      Path: SchemaPath<'ext>
      Body: Expr<TypeValue<'ext>, ResolvedIdentifier, 'ext> }

  type MemoryDBValues<'ext when 'ext: comparison> =
    | EntityRef of Schema<'ext> * MutableMemoryDB<'ext> * SchemaEntity<'ext>
    | RelationRef of Schema<'ext> * MutableMemoryDB<'ext> * SchemaRelation * SchemaEntity<'ext> * SchemaEntity<'ext>
    | Link of
      {| RelationRef:
           Option<Schema<'ext> * MutableMemoryDB<'ext> * SchemaRelation * SchemaEntity<'ext> * SchemaEntity<'ext>> |}
    | Unlink of
      {| RelationRef:
           Option<Schema<'ext> * MutableMemoryDB<'ext> * SchemaRelation * SchemaEntity<'ext> * SchemaEntity<'ext>> |}
    | RelationLookupRef of
      Schema<'ext> *
      MutableMemoryDB<'ext> *
      RelationLookupDirection *
      SchemaRelation *
      SchemaEntity<'ext> *
      SchemaEntity<'ext>
    | EvalProperty of MemoryDBEvalProperty<'ext>
    | StripProperty of MemoryDBEvalProperty<'ext>
    | Create of {| EntityRef: Option<Schema<'ext> * MutableMemoryDB<'ext> * SchemaEntity<'ext>> |}
    | Update of {| EntityRef: Option<Schema<'ext> * MutableMemoryDB<'ext> * SchemaEntity<'ext>> |}
    | Delete of {| EntityRef: Option<Schema<'ext> * MutableMemoryDB<'ext> * SchemaEntity<'ext>> |}
    | GetById of {| EntityRef: Option<Schema<'ext> * MutableMemoryDB<'ext> * SchemaEntity<'ext>> |}
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

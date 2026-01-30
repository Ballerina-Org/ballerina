namespace Ballerina.DSL.Next.StdLib.MemoryDB.Extension

[<AutoOpen>]
module CommonHelpers =
  open Ballerina.Collections.Option
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Types
  open Ballerina.Lenses
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.StdLib.MemoryDB

  /// Extract entity ref from extension value in the first step of operation application
  let extractEntityRefFromValue<'ext when 'ext: comparison>
    (loc0: Location)
    (v: Value<TypeValue<'ext>, 'ext>)
    (valueLens: PartialLens<'ext, MemoryDBValues<'ext>>)
    : Reader<EntityRef<'ext>, ExprEvalContext<'ext>, Errors<Location>> =

    reader {
      let! v, _ =
        v
        |> Value.AsExt
        |> sum.MapError(Errors.MapContext(replaceWith loc0))
        |> reader.OfSum

      let! v =
        v
        |> valueLens.Get
        |> sum.OfOption(Errors.Singleton loc0 (fun () -> "Cannot get value from extension"))
        |> reader.OfSum

      let! v =
        v
        |> MemoryDBValues.AsEntityRef
        |> sum.MapError(Errors.MapContext(replaceWith loc0))
        |> reader.OfSum

      return v
    }

  /// Extract relation ref from extension value in the first step of operation application
  let extractRelationRefFromValue<'ext when 'ext: comparison>
    (loc0: Location)
    (v: Value<TypeValue<'ext>, 'ext>)
    (valueLens: PartialLens<'ext, MemoryDBValues<'ext>>)
    (fieldName: string)
    : Reader<RelationRef<'ext>, ExprEvalContext<'ext>, Errors<Location>> =
    reader {
      let! v =
        v
        |> Value.AsRecord
        |> sum.MapError(Errors.MapContext(replaceWith loc0))
        |> reader.OfSum

      let! v =
        v
        |> Map.tryFind (ResolvedIdentifier.Create fieldName)
        |> sum.OfOption(Errors.Singleton loc0 (fun () -> $"Cannot find '{fieldName}' field in operation"))
        |> reader.OfSum

      let! v, _ =
        v
        |> Value.AsExt
        |> sum.MapError(Errors.MapContext(replaceWith loc0))
        |> reader.OfSum

      let! v =
        v
        |> valueLens.Get
        |> sum.OfOption(Errors.Singleton loc0 (fun () -> "Cannot get value from extension"))
        |> reader.OfSum

      let! v =
        v
        |> MemoryDBValues.AsRelationRef
        |> sum.MapError(Errors.MapContext(replaceWith loc0))
        |> reader.OfSum

      return v
    }

  /// Lookup an existing entity value in the database
  let lookupEntityValue<'ext when 'ext: comparison>
    (db: MutableMemoryDB<'ext>)
    (entity: SchemaEntity<'ext>)
    (entityId: Value<TypeValue<'ext>, 'ext>)
    : Option<Value<TypeValue<'ext>, 'ext>> =
    option {
      let! entityMap = db.entities |> Map.tryFind entity.Name
      let! value = entityMap |> Map.tryFind entityId
      return value
    }

  /// Add an entity value to the database
  let addEntityValue<'ext when 'ext: comparison>
    (db: MutableMemoryDB<'ext>)
    (entity: SchemaEntity<'ext>)
    (entityId: Value<TypeValue<'ext>, 'ext>)
    (value: Value<TypeValue<'ext>, 'ext>)
    : unit =
    db.entities <-
      db.entities
      |> Map.change entity.Name (function
        | Some entities -> Some(entities |> Map.add entityId value)
        | None -> Some(Map.empty |> Map.add entityId value))

  /// Update an entity value in the database
  let updateEntityValue<'ext when 'ext: comparison>
    (db: MutableMemoryDB<'ext>)
    (entity: SchemaEntity<'ext>)
    (entityId: Value<TypeValue<'ext>, 'ext>)
    (value: Value<TypeValue<'ext>, 'ext>)
    : unit =
    db.entities <-
      db.entities
      |> Map.change entity.Name (function
        | Some entities -> Some(entities |> Map.add entityId value)
        | None -> Some(Map.empty |> Map.add entityId value))

  /// Remove an entity value from the database
  let removeEntityValue<'ext when 'ext: comparison>
    (db: MutableMemoryDB<'ext>)
    (entity: SchemaEntity<'ext>)
    (entityId: Value<TypeValue<'ext>, 'ext>)
    : unit =
    db.entities <-
      db.entities
      |> Map.change entity.Name (function
        | Some entities -> Some(entities |> Map.remove entityId)
        | None -> None)

  /// Add a link to a relation in the database
  let addRelationLink<'ext when 'ext: comparison>
    (db: MutableMemoryDB<'ext>)
    (relation: SchemaRelation)
    (fromId: Value<TypeValue<'ext>, 'ext>)
    (toId: Value<TypeValue<'ext>, 'ext>)
    : unit =
    let add_link (rel: MemoryDBRelation<'ext>) : MemoryDBRelation<'ext> =
      { rel with
          All = rel.All |> Set.add (fromId, toId)
          FromTo =
            rel.FromTo
            |> Map.change fromId (function
              | Some toSet -> Some(toSet |> Set.add toId)
              | None -> Some(Set.singleton toId))
          ToFrom =
            rel.ToFrom
            |> Map.change toId (function
              | Some fromSet -> Some(fromSet |> Set.add fromId)
              | None -> Some(Set.singleton fromId)) }

    db.relations <-
      db.relations
      |> Map.change relation.Name (function
        | Some rel -> Some(add_link rel)
        | None -> Some(MemoryDBRelation.Empty |> add_link))

  /// Remove a link from a relation in the database
  let removeRelationLink<'ext when 'ext: comparison>
    (db: MutableMemoryDB<'ext>)
    (relation: SchemaRelation)
    (fromId: Value<TypeValue<'ext>, 'ext>)
    (toId: Value<TypeValue<'ext>, 'ext>)
    : unit =
    let remove_link (rel: MemoryDBRelation<'ext>) : MemoryDBRelation<'ext> =
      { rel with
          All = rel.All |> Set.remove (fromId, toId)
          FromTo =
            rel.FromTo
            |> Map.change fromId (function
              | Some toSet -> Some(toSet |> Set.remove toId)
              | None -> Some(Set.empty))
          ToFrom =
            rel.ToFrom
            |> Map.change toId (function
              | Some fromSet -> Some(fromSet |> Set.remove fromId)
              | None -> Some(Set.empty)) }

    db.relations <-
      db.relations
      |> Map.change relation.Name (function
        | Some rel -> Some(remove_link rel)
        | None -> Some(MemoryDBRelation.Empty |> remove_link))

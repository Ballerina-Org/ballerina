namespace Ballerina.DSL.Next.StdLib.DB

[<AutoOpen>]
module Model =
  open System
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.Terms
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open Ballerina.Collections.Sum

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

  type EntityRef<'db, 'ext when 'ext: comparison> = Schema<'ext> * 'db * SchemaEntity<'ext> * SchemaAsValue<'ext>

  type RelationRef<'db, 'ext when 'ext: comparison> =
    Schema<'ext> * 'db * SchemaRelation<'ext> * SchemaEntity<'ext> * SchemaEntity<'ext> * SchemaAsValue<'ext>

  // type RelationLookupRef<'runtimeContext, 'db, 'ext when 'ext: comparison> =
  //   Schema<'ext> * 'db * RelationLookupDirection * SchemaRelation<'ext> * SchemaEntity<'ext> * SchemaEntity<'ext>

  type CreateArgs<'runtimeContext, 'db, 'ext when 'ext: comparison> =
    { Id: Value<TypeValue<'ext>, 'ext>
      Value: Value<TypeValue<'ext>, 'ext> }

  type UpsertArgs<'runtimeContext, 'db, 'ext when 'ext: comparison> =
    { Id: Value<TypeValue<'ext>, 'ext>
      Previous: Value<TypeValue<'ext>, 'ext>
      Value: Value<TypeValue<'ext>, 'ext> }

  type LinkArgs<'runtimeContext, 'db, 'ext when 'ext: comparison> =
    { FromId: Value<TypeValue<'ext>, 'ext>
      ToId: Value<TypeValue<'ext>, 'ext> }

  type UnlinkArgs<'runtimeContext, 'db, 'ext when 'ext: comparison> =
    { FromId: Value<TypeValue<'ext>, 'ext>
      ToId: Value<TypeValue<'ext>, 'ext> }

  type IsLinkedArgs<'runtimeContext, 'db, 'ext when 'ext: comparison> =
    { FromId: Value<TypeValue<'ext>, 'ext>
      ToId: Value<TypeValue<'ext>, 'ext> }

  type DBTypeClass<'runtimeContext, 'db, 'ext when 'ext: comparison> =
    { DB: 'db
      BeginTransaction: 'db -> Sum<Guid, Errors<Unit>>
      CommitTransaction: 'db -> Guid -> Sum<unit, Errors<Unit>>

      RunQuery:
        ValueQuery<TypeValue<'ext>, 'ext>
          -> Option<int * int>
          -> Reader<List<Value<TypeValue<'ext>, 'ext>>, ExprEvalContext<'runtimeContext, 'ext>, Errors<Unit>>
      Create:
        EntityRef<'db, 'ext>
          -> CreateArgs<'runtimeContext, 'db, 'ext>
          -> Reader<Value<TypeValue<'ext>, 'ext>, ExprEvalContext<'runtimeContext, 'ext>, Errors<Unit>>
      Update:
        EntityRef<'db, 'ext>
          -> UpsertArgs<'runtimeContext, 'db, 'ext>
          -> Reader<Value<TypeValue<'ext>, 'ext>, ExprEvalContext<'runtimeContext, 'ext>, Errors<Unit>>
      Delete:
        EntityRef<'db, 'ext>
          -> Value<TypeValue<'ext>, 'ext>
          -> Reader<unit, ExprEvalContext<'runtimeContext, 'ext>, Errors<Unit>>
      DeleteMany:
        EntityRef<'db, 'ext>
          -> List<Value<TypeValue<'ext>, 'ext>>
          -> Reader<unit, ExprEvalContext<'runtimeContext, 'ext>, Errors<Unit>>
      Link:
        RelationRef<'db, 'ext>
          -> LinkArgs<'runtimeContext, 'db, 'ext>
          -> Reader<unit, ExprEvalContext<'runtimeContext, 'ext>, Errors<Unit>>
      Unlink:
        RelationRef<'db, 'ext>
          -> UnlinkArgs<'runtimeContext, 'db, 'ext>
          -> Reader<unit, ExprEvalContext<'runtimeContext, 'ext>, Errors<Unit>>
      IsLinked:
        RelationRef<'db, 'ext>
          -> IsLinkedArgs<'runtimeContext, 'db, 'ext>
          -> Reader<bool, ExprEvalContext<'runtimeContext, 'ext>, Errors<Unit>>
      GetById:
        EntityRef<'db, 'ext>
          -> Value<TypeValue<'ext>, 'ext>
          -> Reader<Value<TypeValue<'ext>, 'ext>, ExprEvalContext<'runtimeContext, 'ext>, Errors<Unit>>
      GetMany:
        EntityRef<'db, 'ext>
          -> int * int
          -> Reader<List<Value<TypeValue<'ext>, 'ext>>, ExprEvalContext<'runtimeContext, 'ext>, Errors<Unit>>
      LookupMaybe:
        RelationRef<'db, 'ext>
          -> Value<TypeValue<'ext>, 'ext>
          -> RelationLookupDirection
          -> Reader<Option<Value<TypeValue<'ext>, 'ext>>, ExprEvalContext<'runtimeContext, 'ext>, Errors<Unit>>
      LookupOne:
        RelationRef<'db, 'ext>
          -> Value<TypeValue<'ext>, 'ext>
          -> RelationLookupDirection
          -> Reader<Value<TypeValue<'ext>, 'ext>, ExprEvalContext<'runtimeContext, 'ext>, Errors<Unit>>
      LookupMany:
        RelationRef<'db, 'ext>
          -> Value<TypeValue<'ext>, 'ext>
          -> RelationLookupDirection
          -> int * int
          -> Reader<List<Value<TypeValue<'ext>, 'ext>>, ExprEvalContext<'runtimeContext, 'ext>, Errors<Unit>> }

  let db_nonsense () =
    { DB = ()
      BeginTransaction = fun _ -> Left Guid.Empty
      CommitTransaction = fun _ _ -> Left()
      RunQuery = fun _ _ -> reader.Return []
      Create = fun _ args -> reader.Return <| Value.Tuple [ args.Id; args.Value ]
      Update = fun _ args -> reader.Return <| Value.Tuple [ args.Id; args.Previous; args.Value ]
      Delete = fun _ _ -> reader.Return()
      DeleteMany = fun _ _ -> reader.Return()
      Link = fun _ _ -> reader.Return()
      Unlink = fun _ _ -> reader.Return()
      IsLinked = fun _ _ -> reader.Return false
      GetById = fun _ _ -> reader.Throw <| Errors.Singleton () (fun () -> "No such entity")
      GetMany = fun _ _ -> reader.Return []
      LookupMaybe = fun _ _ _ -> reader.Return None
      LookupOne = fun _ _ _ -> reader.Throw <| Errors.Singleton () (fun () -> "No such relation")
      LookupMany = fun _ _ _ _ -> reader.Return [] }

  type DBEvalProperty<'ext> =
    { PropertyName: LocalIdentifier
      Path: SchemaPath<'ext>
      Body: Expr<TypeValue<'ext>, ResolvedIdentifier, 'ext> }

  [<CustomEquality; CustomComparison>]
  type DBIO<'runtimeContext, 'db, 'ext when 'ext: comparison> =
    { Schema: Schema<'ext>
      SchemaAsValue: Value<TypeValue<'ext>, 'ext>
      DB: 'db
      EvalContext: ExprEvalContextScope<'ext>
      Main: Value<TypeValue<'ext>, 'ext> }

    override x.ToString() = $"DBIO(Schema:\n{x.Schema})"

    override x.Equals(yobj) =
      match yobj with
      | :? DBIO<'runtimeContext, 'db, 'ext> as y ->
        (x.Schema = y.Schema
         //  && x.DB = y.DB
         && x.Main = y.Main
         && x.SchemaAsValue = y.SchemaAsValue)
      | _ -> false

    override x.GetHashCode() =
      hash x.Schema ^^^ hash x.Main ^^^ hash x.SchemaAsValue

    interface System.IComparable with
      member x.CompareTo yobj =
        match yobj with
        | :? DBIO<'runtimeContext, 'db, 'ext> as y ->
          compare (x.Schema, x.SchemaAsValue, x.Main) (y.Schema, y.SchemaAsValue, y.Main)
        | _ -> invalidArg "yobj" "cannot compare values of different types"

  type DBValues<'runtimeContext, 'db, 'ext when 'ext: comparison> =
    | EntityRef of EntityRef<'db, 'ext>
    | RelationRef of RelationRef<'db, 'ext>
    | Link of {| RelationRef: Option<RelationRef<'db, 'ext>> |}
    | Unlink of {| RelationRef: Option<RelationRef<'db, 'ext>> |}
    | IsLinked of {| RelationRef: Option<RelationRef<'db, 'ext>> |}
    | LinkMany of {| RelationRef: Option<RelationRef<'db, 'ext>> |}
    | UnlinkMany of {| RelationRef: Option<RelationRef<'db, 'ext>> |}
    | LookupOne of {| RelationRef: Option<RelationRef<'db, 'ext> * RelationLookupDirection> |}
    | LookupOption of {| RelationRef: Option<RelationRef<'db, 'ext> * RelationLookupDirection> |}
    | LookupMany of
      {| RelationRef: Option<RelationRef<'db, 'ext> * RelationLookupDirection>
         EntityId: Option<Value<TypeValue<'ext>, 'ext>> |}
    | RelationLookupRef of RelationRef<'db, 'ext> * RelationLookupDirection
    | EvalProperty of DBEvalProperty<'ext>
    | StripProperty of DBEvalProperty<'ext>
    | Create of {| EntityRef: Option<EntityRef<'db, 'ext>> |}
    | Update of {| EntityRef: Option<EntityRef<'db, 'ext>> |}
    | Upsert of {| EntityRef: Option<EntityRef<'db, 'ext>> |}
    | UpsertMany of {| EntityRef: Option<EntityRef<'db, 'ext>> |}
    | UpdateMany of {| EntityRef: Option<EntityRef<'db, 'ext>> |}
    | DeleteMany of {| EntityRef: Option<EntityRef<'db, 'ext>> |}
    | Delete of {| EntityRef: Option<EntityRef<'db, 'ext>> |}
    | GetById of {| EntityRef: Option<EntityRef<'db, 'ext>> |}
    | GetMany of {| EntityRef: Option<EntityRef<'db, 'ext>> |}
    | Run
    | DBIO of DBIO<'runtimeContext, 'db, 'ext>
    | TypeAppliedRun of Schema<'ext> * 'db
    | QueryRun of {| Query: Option<ValueQuery<TypeValue<'ext>, 'ext>> |}

    override this.ToString() =
      match this with
      | EntityRef(_, _, entity, _) -> $"{entity.Name.Name}"
      | RelationRef(_, _, relation, _fromEntity, _toEntity, _) -> $"{relation.Name.Name}"
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
      | IsLinked isLinked ->
        let relationStr =
          match isLinked.RelationRef with
          | Some(_, _, relation, fromEntity, toEntity, _) ->
            $"RelationRef({relation.Name}, from: {fromEntity.Name}, to: {toEntity.Name})"
          | None -> "None"

        $"IsLinked(Relation: {relationStr})"
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
          | Some((_, _, relation, fromEntity, toEntity, _), _) ->
            $"RelationRef({relation.Name}, from: {fromEntity.Name}, to: {toEntity.Name})"
          | None -> "None"

        $"LookupOne(Relation: {relationStr})"
      | LookupOption lookupOption ->
        let relationStr =
          match lookupOption.RelationRef with
          | Some((_, _, relation, fromEntity, toEntity, _), _) ->
            $"RelationRef({relation.Name}, from: {fromEntity.Name}, to: {toEntity.Name})"
          | None -> "None"

        $"LookupOption(Relation: {relationStr})"
      | LookupMany lookupMany ->
        let relationStr =
          match lookupMany.RelationRef with
          | Some((_, _, relation, fromEntity, toEntity, _), _) ->
            $"RelationRef({relation.Name}, from: {fromEntity.Name}, to: {toEntity.Name})"
          | None -> "None"

        $"LookupMany(Relation: {relationStr})"
      | RelationLookupRef((_, _, relation, _fromEntity, _toEntity, _), direction) -> $"{relation.Name}[{direction}]"
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
      | TypeAppliedRun(_schema, _) -> $"TypeAppliedRun"
      | QueryRun v -> $"runQuery ({v.Query})"

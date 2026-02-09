namespace Ballerina.DSL.Next.StdLib.MemoryDB.Extension

[<AutoOpen>]
module Lookups =
  open Ballerina.StdLib.String
  open Ballerina.Collections.Option
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.Lenses
  open Ballerina.DSL.Next.Extensions
  open FSharp.Data
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Next.Types.TypeChecker
  open System
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina
  open Ballerina.DSL.Next.StdLib.MemoryDB

  let MemoryDBLookupsExtensions<'ext when 'ext: comparison>
    (listSet: List<Value<TypeValue<'ext>, 'ext>> -> 'ext)
    (valueLens: PartialLens<'ext, MemoryDBValues<'ext>>)
    : OperationsExtension<'ext, MemoryDBValues<'ext>> =

    let actual_lookup
      loc0
      (
        _schema: Schema<'ext>,
        _db: MutableMemoryDB<'ext>,
        _dir,
        _relation: SchemaRelation<'ext>,
        _from: SchemaEntity<'ext>,
        _to: SchemaEntity<'ext>
      )
      v
      =
      reader {
        let! relation_ref =
          _db.relations
          |> Map.tryFind _relation.Name
          |> sum.OfOption(Errors.Singleton loc0 (fun () -> "Relation not found"))
          |> reader.OfSum

        let source_entity_ref, target_entity_ref, source_to_targets =
          match _dir with
          | FromTo -> _from, _to, relation_ref.FromTo
          | ToFrom -> _to, _from, relation_ref.ToFrom

        let source_id = v

        let! sources =
          _db.entities
          |> Map.tryFind source_entity_ref.Name
          |> sum.OfOption(Errors.Singleton loc0 (fun () -> "Source entity not found"))
          |> reader.OfSum

        let! targets =
          _db.entities
          |> Map.tryFind target_entity_ref.Name
          |> sum.OfOption(Errors.Singleton loc0 (fun () -> "Target entity not found"))
          |> reader.OfSum

        do!
          sources
          |> Map.tryFind source_id
          |> sum.OfOption(Errors.Singleton loc0 (fun () -> "Source ID not found"))
          |> reader.OfSum
          |> reader.Ignore

        let target_ids =
          source_to_targets |> Map.tryFind source_id |> Option.defaultValue Set.empty

        return!
          target_ids
          |> Set.toSeq
          |> Seq.map (fun target_id ->
            reader {
              let! target_v =
                targets
                |> Map.tryFind target_id
                |> sum.OfOption(Errors.Singleton loc0 (fun () -> "Target ID not found"))
                |> reader.OfSum

              return Value.Tuple [ target_id; target_v ]
            })
          |> reader.All
      }

    let memoryDBLookupOneId =
      Identifier.FullyQualified([ "MemoryDB" ], "lookupOne")
      |> TypeCheckScope.Empty.Resolve

    let memoryDBLookupOneType =
      TypeValue.CreateLambda(
        TypeParameter.Create("schema", Kind.Schema),
        TypeExpr.Lambda(
          TypeParameter.Create("from_id", Kind.Star),
          TypeExpr.Lambda(
            TypeParameter.Create("to_id", Kind.Star),
            TypeExpr.Lambda(
              TypeParameter.Create("to_with_props", Kind.Star),
              TypeExpr.Arrow(
                TypeExpr.Apply(
                  TypeExpr.Apply(
                    TypeExpr.Apply(
                      TypeExpr.Apply(
                        TypeExpr.Lookup("SchemaLookupOne" |> Identifier.LocalScope),
                        TypeExpr.Lookup("schema" |> Identifier.LocalScope)
                      ),
                      TypeExpr.Lookup("from_id" |> Identifier.LocalScope)
                    ),
                    TypeExpr.Lookup("to_id" |> Identifier.LocalScope)
                  ),
                  TypeExpr.Lookup("to_with_props" |> Identifier.LocalScope)
                ),
                TypeExpr.Arrow(
                  TypeExpr.Lookup("from_id" |> Identifier.LocalScope),
                  TypeExpr.Sum
                    [ TypeExpr.Primitive PrimitiveType.Unit
                      TypeExpr.Tuple[TypeExpr.Lookup("to_id" |> Identifier.LocalScope)
                                     TypeExpr.Lookup("to_with_props" |> Identifier.LocalScope)] ]
                )
              )
            )
          )
        )
      )

    let memoryDBLookupOneKind =
      Kind.Arrow(Kind.Schema, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))))


    let LookupOneOperation: OperationExtension<_, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBLookupOneType, memoryDBLookupOneKind, MemoryDBValues.LookupOne {| RelationRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | MemoryDBValues.LookupOne v -> Some(MemoryDBValues.LookupOne v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> MemoryDBValues.AsLookupOne
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op.RelationRef with
              | None -> // the closure is empty - first step in the application
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
                  |> MemoryDBValues.AsRelationLookupRef
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                return
                  (MemoryDBValues.LookupOne {| RelationRef = Some v |} |> valueLens.Set, Some memoryDBLookupOneId)
                  |> Ext
              | Some relation_ref -> // the closure has the first operand - second step in the application
                let! target_values = actual_lookup loc0 relation_ref v |> reader.Catch

                match target_values with
                | Right(_e: Errors<Location>) ->
                  return Value.Sum({ Case = 1; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
                | Left target_values ->
                  match target_values |> List.tryHead with
                  | Some v -> return Value.Sum({ Case = 2; Count = 2 }, v)
                  | _ -> return Value.Sum({ Case = 1; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
            } }

    let memoryDBLookupOptionId =
      Identifier.FullyQualified([ "MemoryDB" ], "lookupOption")
      |> TypeCheckScope.Empty.Resolve

    let memoryDBLookupOptionType =
      TypeValue.CreateLambda(
        TypeParameter.Create("schema", Kind.Schema),
        TypeExpr.Lambda(
          TypeParameter.Create("from_id", Kind.Star),
          TypeExpr.Lambda(
            TypeParameter.Create("to_id", Kind.Star),
            TypeExpr.Lambda(
              TypeParameter.Create("to_with_props", Kind.Star),
              TypeExpr.Arrow(
                TypeExpr.Apply(
                  TypeExpr.Apply(
                    TypeExpr.Apply(
                      TypeExpr.Apply(
                        TypeExpr.Lookup("SchemaLookupOption" |> Identifier.LocalScope),
                        TypeExpr.Lookup("schema" |> Identifier.LocalScope)
                      ),
                      TypeExpr.Lookup("from_id" |> Identifier.LocalScope)
                    ),
                    TypeExpr.Lookup("to_id" |> Identifier.LocalScope)
                  ),
                  TypeExpr.Lookup("to_with_props" |> Identifier.LocalScope)
                ),
                TypeExpr.Arrow(
                  TypeExpr.Lookup("from_id" |> Identifier.LocalScope),
                  TypeExpr.Sum
                    [ TypeExpr.Primitive PrimitiveType.Unit
                      TypeExpr.Sum
                        [ TypeExpr.Primitive PrimitiveType.Unit
                          TypeExpr.Tuple[TypeExpr.Lookup("to_id" |> Identifier.LocalScope)
                                         TypeExpr.Lookup("to_with_props" |> Identifier.LocalScope)] ] ]
                )
              )
            )
          )
        )
      )

    let memoryDBLookupOptionKind =
      Kind.Arrow(Kind.Schema, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))))

    let LookupOptionOperation: OperationExtension<_, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBLookupOptionType, memoryDBLookupOptionKind, MemoryDBValues.LookupOption {| RelationRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | MemoryDBValues.LookupOption v -> Some(MemoryDBValues.LookupOption v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> MemoryDBValues.AsLookupOption
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op.RelationRef with
              | None -> // the closure is empty - first step in the application
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
                  |> MemoryDBValues.AsRelationLookupRef
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                return
                  (MemoryDBValues.LookupOption {| RelationRef = Some v |} |> valueLens.Set, Some memoryDBLookupOptionId)
                  |> Ext
              | Some relation_ref -> // the closure has the first operand - second step in the application
                let! target_values = actual_lookup loc0 relation_ref v |> reader.Catch

                match target_values with
                | Right(_e: Errors<Location>) ->
                  return Value.Sum({ Case = 1; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
                | Left target_values ->
                  match target_values |> List.tryHead with
                  | Some v -> return Value.Sum({ Case = 2; Count = 2 }, Value.Sum({ Case = 2; Count = 2 }, v))
                  | _ ->
                    return
                      Value.Sum(
                        { Case = 2; Count = 2 },
                        Value.Sum({ Case = 1; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
                      )
            } }

    let memoryDBLookupManyId =
      Identifier.FullyQualified([ "MemoryDB" ], "lookupMany")
      |> TypeCheckScope.Empty.Resolve

    let memoryDBLookupManyType =
      TypeValue.CreateLambda(
        TypeParameter.Create("schema", Kind.Schema),
        TypeExpr.Lambda(
          TypeParameter.Create("from_id", Kind.Star),
          TypeExpr.Lambda(
            TypeParameter.Create("to_id", Kind.Star),
            TypeExpr.Lambda(
              TypeParameter.Create("to_with_props", Kind.Star),
              TypeExpr.Arrow(
                TypeExpr.Apply(
                  TypeExpr.Apply(
                    TypeExpr.Apply(
                      TypeExpr.Apply(
                        TypeExpr.Lookup("SchemaLookupMany" |> Identifier.LocalScope),
                        TypeExpr.Lookup("schema" |> Identifier.LocalScope)
                      ),
                      TypeExpr.Lookup("from_id" |> Identifier.LocalScope)
                    ),
                    TypeExpr.Lookup("to_id" |> Identifier.LocalScope)
                  ),
                  TypeExpr.Lookup("to_with_props" |> Identifier.LocalScope)
                ),
                TypeExpr.Arrow(
                  TypeExpr.Lookup("from_id" |> Identifier.LocalScope),
                  TypeExpr.Arrow(
                    TypeExpr.Tuple
                      [ TypeExpr.Primitive PrimitiveType.Int32
                        TypeExpr.Primitive PrimitiveType.Int32 ],
                    TypeExpr.Sum
                      [ TypeExpr.Primitive PrimitiveType.Unit
                        TypeExpr.Apply(
                          TypeExpr.Lookup("List" |> Identifier.LocalScope),
                          TypeExpr.Tuple[TypeExpr.Lookup("to_id" |> Identifier.LocalScope)
                                         TypeExpr.Lookup("to_with_props" |> Identifier.LocalScope)]
                        ) ]
                  )
                )
              )
            )
          )
        )
      )

    let memoryDBLookupManyKind =
      Kind.Arrow(Kind.Schema, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))))

    let LookupManyOperation: OperationExtension<_, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBLookupManyType,
              memoryDBLookupManyKind,
              MemoryDBValues.LookupMany
                {| RelationRef = None
                   EntityId = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | MemoryDBValues.LookupMany v -> Some(MemoryDBValues.LookupMany v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> MemoryDBValues.AsLookupMany
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op.RelationRef with
              | None -> // the closure is empty - first step in the application
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
                  |> MemoryDBValues.AsRelationLookupRef
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                return
                  (MemoryDBValues.LookupMany
                    {| RelationRef = Some v
                       EntityId = None |}
                   |> valueLens.Set,
                   Some memoryDBLookupManyId)
                  |> Ext
              | Some relation_ref -> // the closure has the first operand - second step in the application
                match op.EntityId with
                | None -> // second step in the application
                  return
                    (MemoryDBValues.LookupMany
                      {| RelationRef = Some relation_ref
                         EntityId = Some v |}
                     |> valueLens.Set,
                     Some memoryDBLookupManyId)
                    |> Ext
                | Some entityId ->
                  let! target_values = actual_lookup loc0 relation_ref entityId |> reader.Catch

                  let! v =
                    v
                    |> Value.AsTuple
                    |> sum.MapError(Errors.MapContext(replaceWith loc0))
                    |> reader.OfSum

                  match v with
                  | [ Value.Primitive(PrimitiveValue.Int32 _offset); Value.Primitive(PrimitiveValue.Int32 _limit) ] ->
                    match target_values with
                    | Right(_e: Errors<Location>) ->
                      return Value.Sum({ Case = 1; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
                    | Left target_values ->
                      let target_values =
                        target_values |> List.skip (int _offset) |> List.truncate (int _limit)

                      return Value.Sum({ Case = 2; Count = 2 }, (target_values |> listSet, None) |> Ext)
                  | _ ->
                    return!
                      Errors.Singleton loc0 (fun () -> "Expected a tuple of two Int32 values for offset and limit")
                      |> reader.Throw

            } }

    { TypeVars = []
      Operations =
        [ (memoryDBLookupOptionId, LookupOptionOperation)
          (memoryDBLookupOneId, LookupOneOperation)
          (memoryDBLookupManyId, LookupManyOperation) ]
        |> Map.ofList }

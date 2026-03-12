namespace Ballerina.DSL.Next.StdLib.DB.Extension

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
  open Ballerina.DSL.Next.StdLib.DB

  let DBLookupsExtensions<'runtimeContext, 'db, 'ext when 'ext: comparison>
    (db_ops: DBTypeClass<'runtimeContext, 'db, 'ext>)
    (listSet: List<Value<TypeValue<'ext>, 'ext>> -> 'ext)
    (valueLens: PartialLens<'ext, DBValues<'runtimeContext, 'db, 'ext>>)
    =

    let memoryDBLookupOneId =
      Identifier.FullyQualified([ "DB" ], "lookupOne") |> TypeCheckScope.Empty.Resolve

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


    let LookupOneOperation: OperationExtension<'runtimeContext, _, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBLookupOneType, memoryDBLookupOneKind, DBValues.LookupOne {| RelationRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | DBValues.LookupOne v -> Some(DBValues.LookupOne v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DBValues.AsLookupOne
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
                  |> DBValues.AsRelationLookupRef
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                return
                  (DBValues.LookupOne {| RelationRef = Some v |} |> valueLens.Set, Some memoryDBLookupOneId)
                  |> Ext
              | Some(relation_ref, direction) -> // the closure has the first operand - second step in the application
                let! target_values =
                  db_ops.LookupOne relation_ref v direction
                  |> reader.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.Catch

                match target_values with
                | Right(_e: Errors<_>) -> return Value.Sum({ Case = 1; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
                | Left v -> return Value.Sum({ Case = 2; Count = 2 }, v)
            } }

    let memoryDBLookupOptionId =
      Identifier.FullyQualified([ "DB" ], "lookupOption")
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

    let LookupOptionOperation: OperationExtension<'runtimeContext, _, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBLookupOptionType, memoryDBLookupOptionKind, DBValues.LookupOption {| RelationRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | DBValues.LookupOption v -> Some(DBValues.LookupOption v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DBValues.AsLookupOption
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
                  |> DBValues.AsRelationLookupRef
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                return
                  (DBValues.LookupOption {| RelationRef = Some v |} |> valueLens.Set, Some memoryDBLookupOptionId)
                  |> Ext
              | Some(relation_ref, direction) -> // the closure has the first operand - second step in the application
                let! target_values =
                  db_ops.LookupMaybe relation_ref v direction
                  |> reader.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.Catch

                match target_values with
                | Right(_e: Errors<_>) -> return Value.Sum({ Case = 1; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
                | Left(Some v) -> return Value.Sum({ Case = 2; Count = 2 }, Value.Sum({ Case = 2; Count = 2 }, v))
                | Left(None) ->
                  return
                    Value.Sum(
                      { Case = 2; Count = 2 },
                      Value.Sum({ Case = 1; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
                    )
            } }

    let memoryDBLookupManyId =
      Identifier.FullyQualified([ "DB" ], "lookupMany")
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

    let LookupManyOperation: OperationExtension<'runtimeContext, _, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBLookupManyType,
              memoryDBLookupManyKind,
              DBValues.LookupMany
                {| RelationRef = None
                   EntityId = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | DBValues.LookupMany v -> Some(DBValues.LookupMany v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DBValues.AsLookupMany
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
                  |> DBValues.AsRelationLookupRef
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                return
                  (DBValues.LookupMany
                    {| RelationRef = Some v
                       EntityId = None |}
                   |> valueLens.Set,
                   Some memoryDBLookupManyId)
                  |> Ext
              | Some(relation_ref, direction) -> // the closure has the first operand - second step in the application
                match op.EntityId with
                | None -> // second step in the application
                  return
                    (DBValues.LookupMany
                      {| RelationRef = Some(relation_ref, direction)
                         EntityId = Some v |}
                     |> valueLens.Set,
                     Some memoryDBLookupManyId)
                    |> Ext
                | Some entityId ->
                  let! v =
                    v
                    |> Value.AsTuple
                    |> sum.MapError(Errors.MapContext(replaceWith loc0))
                    |> reader.OfSum

                  match v with
                  | [ Value.Primitive(PrimitiveValue.Int32 _offset); Value.Primitive(PrimitiveValue.Int32 _limit) ] ->
                    let! target_values =
                      db_ops.LookupMany relation_ref entityId direction (_offset, _limit)
                      |> reader.MapError(Errors.MapContext(replaceWith loc0))
                      |> reader.Catch

                    match target_values with
                    | Right(_e: Errors<_>) ->
                      return Value.Sum({ Case = 1; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
                    | Left target_values ->
                      return Value.Sum({ Case = 2; Count = 2 }, (target_values |> listSet, None) |> Ext)
                  | _ ->
                    return!
                      Errors.Singleton loc0 (fun () -> "Expected a tuple of two Int32 values for offset and limit")
                      |> reader.Throw

            } }

    [ (memoryDBLookupOptionId, LookupOptionOperation)
      (memoryDBLookupOneId, LookupOneOperation)
      (memoryDBLookupManyId, LookupManyOperation) ]

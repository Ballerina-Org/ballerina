namespace Ballerina.DSL.Next.StdLib.DB.Extension

[<AutoOpen>]
module Move =
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
  open Ballerina.DSL.Next.Types.TypeChecker
  open Ballerina.DSL.Next.StdLib.DB

  let DBMoveBeforeExtension<'runtimeContext, 'db, 'ext when 'ext: comparison>
    (db_ops: DBTypeClass<'runtimeContext, 'db, 'ext>)
    (_listSet: List<Value<TypeValue<'ext>, 'ext>> -> 'ext)
    (valueLens: PartialLens<'ext, DBValues<'runtimeContext, 'db, 'ext>>)
    =
    let memoryDBMoveBeforeId =
      Identifier.FullyQualified([ "DB" ], "moveBefore")
      |> TypeCheckScope.Empty.Resolve

    let memoryDBMoveBeforeType =
      TypeValue.CreateLambda(
        TypeParameter.Create("schema", Kind.Schema),
        TypeExpr.Lambda(
          TypeParameter.Create("from", Kind.Star),
          TypeExpr.Lambda(
            TypeParameter.Create("from_with_props", Kind.Star),
            TypeExpr.Lambda(
              TypeParameter.Create("from_id", Kind.Star),
              TypeExpr.Lambda(
                TypeParameter.Create("to", Kind.Star),
                TypeExpr.Lambda(
                  TypeParameter.Create("to_with_props", Kind.Star),
                  TypeExpr.Lambda(
                    TypeParameter.Create("to_id", Kind.Star),
                    TypeExpr.Arrow(
                      TypeExpr.Apply(
                        TypeExpr.Apply(
                          TypeExpr.Apply(
                            TypeExpr.Apply(
                              TypeExpr.Apply(
                                TypeExpr.Apply(
                                  TypeExpr.Apply(
                                    TypeExpr.Lookup(
                                      "SchemaRelation" |> Identifier.LocalScope
                                    ),
                                    TypeExpr.Lookup(
                                      "schema" |> Identifier.LocalScope
                                    )

                                  ),
                                  TypeExpr.Lookup(
                                    "from" |> Identifier.LocalScope
                                  )
                                ),
                                TypeExpr.Lookup(
                                  "from_with_props" |> Identifier.LocalScope
                                )
                              ),
                              TypeExpr.Lookup(
                                "from_id" |> Identifier.LocalScope
                              )
                            ),
                            TypeExpr.Lookup("to" |> Identifier.LocalScope)
                          ),
                          TypeExpr.Lookup(
                            "to_with_props" |> Identifier.LocalScope
                          )
                        ),
                        TypeExpr.Lookup("to_id" |> Identifier.LocalScope)
                      ),
                      TypeExpr.Arrow(
                        TypeExpr.Tuple
                          [ TypeExpr.Lookup("from_id" |> Identifier.LocalScope)
                            TypeExpr.Lookup("to_id" |> Identifier.LocalScope)
                            TypeExpr.Lookup("to_id" |> Identifier.LocalScope) ],
                        TypeExpr.Sum
                          [ TypeExpr.Primitive PrimitiveType.Unit
                            TypeExpr.Primitive PrimitiveType.Unit ]
                      )
                    )
                  )
                )
              )
            )
          )
        )
      )

    let memoryDBMoveBeforeKind = standardSchemaRelationOperationKind

    let MoveBeforeOperation: OperationExtension<'runtimeContext, _, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBMoveBeforeType,
              memoryDBMoveBeforeKind,
              DBValues.MoveBefore {| RelationRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | DBValues.MoveBefore v -> Some(DBValues.MoveBefore v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DBValues.AsMoveBefore
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op.RelationRef with
              | None ->
                let! v = extractRelationRefFromValue loc0 v valueLens "Relation"

                return
                  (DBValues.MoveBefore({| RelationRef = Some v |})
                   |> valueLens.Set,
                   Some memoryDBMoveBeforeId)
                  |> Ext
              | Some relation_ref ->
                let! v =
                  v
                  |> Value.AsTuple
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                match v with
                | [ _fromId; _sourceId; _targetId ] ->
                  do!
                    db_ops.MoveBefore
                      relation_ref
                      { FromId = _fromId
                        SourceId = _sourceId
                        TargetId = _targetId }
                    |> reader.MapError(Errors.MapContext(replaceWith loc0))

                  return
                    Value.Sum(
                      { Case = 2; Count = 2 },
                      Value.Primitive PrimitiveValue.Unit
                    )
                | _ ->
                  return!
                    sum.Throw(
                      Errors.Singleton loc0 (fun () ->
                        "Expected a tuple with 3 elements (fromId, sourceId, targetId) when moving before in relation")
                    )
                    |> reader.OfSum
            } }

    memoryDBMoveBeforeId, MoveBeforeOperation


  let DBMoveAfterExtension<'runtimeContext, 'db, 'ext when 'ext: comparison>
    (db_ops: DBTypeClass<'runtimeContext, 'db, 'ext>)
    (_listSet: List<Value<TypeValue<'ext>, 'ext>> -> 'ext)
    (valueLens: PartialLens<'ext, DBValues<'runtimeContext, 'db, 'ext>>)
    =
    let memoryDBMoveAfterId =
      Identifier.FullyQualified([ "DB" ], "moveAfter")
      |> TypeCheckScope.Empty.Resolve

    let memoryDBMoveAfterType =
      TypeValue.CreateLambda(
        TypeParameter.Create("schema", Kind.Schema),
        TypeExpr.Lambda(
          TypeParameter.Create("from", Kind.Star),
          TypeExpr.Lambda(
            TypeParameter.Create("from_with_props", Kind.Star),
            TypeExpr.Lambda(
              TypeParameter.Create("from_id", Kind.Star),
              TypeExpr.Lambda(
                TypeParameter.Create("to", Kind.Star),
                TypeExpr.Lambda(
                  TypeParameter.Create("to_with_props", Kind.Star),
                  TypeExpr.Lambda(
                    TypeParameter.Create("to_id", Kind.Star),
                    TypeExpr.Arrow(
                      TypeExpr.Apply(
                        TypeExpr.Apply(
                          TypeExpr.Apply(
                            TypeExpr.Apply(
                              TypeExpr.Apply(
                                TypeExpr.Apply(
                                  TypeExpr.Apply(
                                    TypeExpr.Lookup(
                                      "SchemaRelation" |> Identifier.LocalScope
                                    ),
                                    TypeExpr.Lookup(
                                      "schema" |> Identifier.LocalScope
                                    )

                                  ),
                                  TypeExpr.Lookup(
                                    "from" |> Identifier.LocalScope
                                  )
                                ),
                                TypeExpr.Lookup(
                                  "from_with_props" |> Identifier.LocalScope
                                )
                              ),
                              TypeExpr.Lookup(
                                "from_id" |> Identifier.LocalScope
                              )
                            ),
                            TypeExpr.Lookup("to" |> Identifier.LocalScope)
                          ),
                          TypeExpr.Lookup(
                            "to_with_props" |> Identifier.LocalScope
                          )
                        ),
                        TypeExpr.Lookup("to_id" |> Identifier.LocalScope)
                      ),
                      TypeExpr.Arrow(
                        TypeExpr.Tuple
                          [ TypeExpr.Lookup("from_id" |> Identifier.LocalScope)
                            TypeExpr.Lookup("to_id" |> Identifier.LocalScope)
                            TypeExpr.Lookup("to_id" |> Identifier.LocalScope) ],
                        TypeExpr.Sum
                          [ TypeExpr.Primitive PrimitiveType.Unit
                            TypeExpr.Primitive PrimitiveType.Unit ]
                      )
                    )
                  )
                )
              )
            )
          )
        )
      )

    let memoryDBMoveAfterKind = standardSchemaRelationOperationKind

    let MoveAfterOperation: OperationExtension<'runtimeContext, _, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBMoveAfterType,
              memoryDBMoveAfterKind,
              DBValues.MoveAfter {| RelationRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | DBValues.MoveAfter v -> Some(DBValues.MoveAfter v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DBValues.AsMoveAfter
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op.RelationRef with
              | None ->
                let! v = extractRelationRefFromValue loc0 v valueLens "Relation"

                return
                  (DBValues.MoveAfter({| RelationRef = Some v |})
                   |> valueLens.Set,
                   Some memoryDBMoveAfterId)
                  |> Ext
              | Some relation_ref ->
                let! v =
                  v
                  |> Value.AsTuple
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                match v with
                | [ _fromId; _sourceId; _targetId ] ->
                  do!
                    db_ops.MoveAfter
                      relation_ref
                      { FromId = _fromId
                        SourceId = _sourceId
                        TargetId = _targetId }
                    |> reader.MapError(Errors.MapContext(replaceWith loc0))

                  return
                    Value.Sum(
                      { Case = 2; Count = 2 },
                      Value.Primitive PrimitiveValue.Unit
                    )
                | _ ->
                  return!
                    sum.Throw(
                      Errors.Singleton loc0 (fun () ->
                        "Expected a tuple with 3 elements (fromId, sourceId, targetId) when moving after in relation")
                    )
                    |> reader.OfSum
            } }

    memoryDBMoveAfterId, MoveAfterOperation


  let DBMoveBeforeReverseExtension<'runtimeContext, 'db, 'ext when 'ext: comparison>
    (db_ops: DBTypeClass<'runtimeContext, 'db, 'ext>)
    (_listSet: List<Value<TypeValue<'ext>, 'ext>> -> 'ext)
    (valueLens: PartialLens<'ext, DBValues<'runtimeContext, 'db, 'ext>>)
    =
    let memoryDBMoveBeforeReverseId =
      Identifier.FullyQualified([ "DB" ], "moveBeforeReverse")
      |> TypeCheckScope.Empty.Resolve

    let memoryDBMoveBeforeReverseType =
      TypeValue.CreateLambda(
        TypeParameter.Create("schema", Kind.Schema),
        TypeExpr.Lambda(
          TypeParameter.Create("from", Kind.Star),
          TypeExpr.Lambda(
            TypeParameter.Create("from_with_props", Kind.Star),
            TypeExpr.Lambda(
              TypeParameter.Create("from_id", Kind.Star),
              TypeExpr.Lambda(
                TypeParameter.Create("to", Kind.Star),
                TypeExpr.Lambda(
                  TypeParameter.Create("to_with_props", Kind.Star),
                  TypeExpr.Lambda(
                    TypeParameter.Create("to_id", Kind.Star),
                    TypeExpr.Arrow(
                      TypeExpr.Apply(
                        TypeExpr.Apply(
                          TypeExpr.Apply(
                            TypeExpr.Apply(
                              TypeExpr.Apply(
                                TypeExpr.Apply(
                                  TypeExpr.Apply(
                                    TypeExpr.Lookup(
                                      "SchemaRelation" |> Identifier.LocalScope
                                    ),
                                    TypeExpr.Lookup(
                                      "schema" |> Identifier.LocalScope
                                    )

                                  ),
                                  TypeExpr.Lookup(
                                    "from" |> Identifier.LocalScope
                                  )
                                ),
                                TypeExpr.Lookup(
                                  "from_with_props" |> Identifier.LocalScope
                                )
                              ),
                              TypeExpr.Lookup(
                                "from_id" |> Identifier.LocalScope
                              )
                            ),
                            TypeExpr.Lookup("to" |> Identifier.LocalScope)
                          ),
                          TypeExpr.Lookup(
                            "to_with_props" |> Identifier.LocalScope
                          )
                        ),
                        TypeExpr.Lookup("to_id" |> Identifier.LocalScope)
                      ),
                      TypeExpr.Arrow(
                        TypeExpr.Tuple
                          [ TypeExpr.Lookup("to_id" |> Identifier.LocalScope)
                            TypeExpr.Lookup("from_id" |> Identifier.LocalScope)
                            TypeExpr.Lookup("from_id" |> Identifier.LocalScope) ],
                        TypeExpr.Sum
                          [ TypeExpr.Primitive PrimitiveType.Unit
                            TypeExpr.Primitive PrimitiveType.Unit ]
                      )
                    )
                  )
                )
              )
            )
          )
        )
      )

    let memoryDBMoveBeforeReverseKind = standardSchemaRelationOperationKind

    let MoveBeforeReverseOperation: OperationExtension<'runtimeContext, _, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBMoveBeforeReverseType,
              memoryDBMoveBeforeReverseKind,
              DBValues.MoveBeforeReverse {| RelationRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | DBValues.MoveBeforeReverse v -> Some(DBValues.MoveBeforeReverse v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DBValues.AsMoveBeforeReverse
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op.RelationRef with
              | None ->
                let! v = extractRelationRefFromValue loc0 v valueLens "Relation"

                return
                  (DBValues.MoveBeforeReverse({| RelationRef = Some v |})
                   |> valueLens.Set,
                   Some memoryDBMoveBeforeReverseId)
                  |> Ext
              | Some relation_ref ->
                let! v =
                  v
                  |> Value.AsTuple
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                match v with
                | [ _toId; _sourceId; _targetId ] ->
                  do!
                    db_ops.MoveBeforeReverse
                      relation_ref
                      { FromId = _toId
                        SourceId = _sourceId
                        TargetId = _targetId }
                    |> reader.MapError(Errors.MapContext(replaceWith loc0))

                  return
                    Value.Sum(
                      { Case = 2; Count = 2 },
                      Value.Primitive PrimitiveValue.Unit
                    )
                | _ ->
                  return!
                    sum.Throw(
                      Errors.Singleton loc0 (fun () ->
                        "Expected a tuple with 3 elements (toId, sourceId, targetId) when moving before (reverse) in relation")
                    )
                    |> reader.OfSum
            } }

    memoryDBMoveBeforeReverseId, MoveBeforeReverseOperation


  let DBMoveAfterReverseExtension<'runtimeContext, 'db, 'ext when 'ext: comparison>
    (db_ops: DBTypeClass<'runtimeContext, 'db, 'ext>)
    (_listSet: List<Value<TypeValue<'ext>, 'ext>> -> 'ext)
    (valueLens: PartialLens<'ext, DBValues<'runtimeContext, 'db, 'ext>>)
    =
    let memoryDBMoveAfterReverseId =
      Identifier.FullyQualified([ "DB" ], "moveAfterReverse")
      |> TypeCheckScope.Empty.Resolve

    let memoryDBMoveAfterReverseType =
      TypeValue.CreateLambda(
        TypeParameter.Create("schema", Kind.Schema),
        TypeExpr.Lambda(
          TypeParameter.Create("from", Kind.Star),
          TypeExpr.Lambda(
            TypeParameter.Create("from_with_props", Kind.Star),
            TypeExpr.Lambda(
              TypeParameter.Create("from_id", Kind.Star),
              TypeExpr.Lambda(
                TypeParameter.Create("to", Kind.Star),
                TypeExpr.Lambda(
                  TypeParameter.Create("to_with_props", Kind.Star),
                  TypeExpr.Lambda(
                    TypeParameter.Create("to_id", Kind.Star),
                    TypeExpr.Arrow(
                      TypeExpr.Apply(
                        TypeExpr.Apply(
                          TypeExpr.Apply(
                            TypeExpr.Apply(
                              TypeExpr.Apply(
                                TypeExpr.Apply(
                                  TypeExpr.Apply(
                                    TypeExpr.Lookup(
                                      "SchemaRelation" |> Identifier.LocalScope
                                    ),
                                    TypeExpr.Lookup(
                                      "schema" |> Identifier.LocalScope
                                    )

                                  ),
                                  TypeExpr.Lookup(
                                    "from" |> Identifier.LocalScope
                                  )
                                ),
                                TypeExpr.Lookup(
                                  "from_with_props" |> Identifier.LocalScope
                                )
                              ),
                              TypeExpr.Lookup(
                                "from_id" |> Identifier.LocalScope
                              )
                            ),
                            TypeExpr.Lookup("to" |> Identifier.LocalScope)
                          ),
                          TypeExpr.Lookup(
                            "to_with_props" |> Identifier.LocalScope
                          )
                        ),
                        TypeExpr.Lookup("to_id" |> Identifier.LocalScope)
                      ),
                      TypeExpr.Arrow(
                        TypeExpr.Tuple
                          [ TypeExpr.Lookup("to_id" |> Identifier.LocalScope)
                            TypeExpr.Lookup("from_id" |> Identifier.LocalScope)
                            TypeExpr.Lookup("from_id" |> Identifier.LocalScope) ],
                        TypeExpr.Sum
                          [ TypeExpr.Primitive PrimitiveType.Unit
                            TypeExpr.Primitive PrimitiveType.Unit ]
                      )
                    )
                  )
                )
              )
            )
          )
        )
      )

    let memoryDBMoveAfterReverseKind = standardSchemaRelationOperationKind

    let MoveAfterReverseOperation: OperationExtension<'runtimeContext, _, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBMoveAfterReverseType,
              memoryDBMoveAfterReverseKind,
              DBValues.MoveAfterReverse {| RelationRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | DBValues.MoveAfterReverse v -> Some(DBValues.MoveAfterReverse v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DBValues.AsMoveAfterReverse
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op.RelationRef with
              | None ->
                let! v = extractRelationRefFromValue loc0 v valueLens "Relation"

                return
                  (DBValues.MoveAfterReverse({| RelationRef = Some v |})
                   |> valueLens.Set,
                   Some memoryDBMoveAfterReverseId)
                  |> Ext
              | Some relation_ref ->
                let! v =
                  v
                  |> Value.AsTuple
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                match v with
                | [ _toId; _sourceId; _targetId ] ->
                  do!
                    db_ops.MoveAfterReverse
                      relation_ref
                      { FromId = _toId
                        SourceId = _sourceId
                        TargetId = _targetId }
                    |> reader.MapError(Errors.MapContext(replaceWith loc0))

                  return
                    Value.Sum(
                      { Case = 2; Count = 2 },
                      Value.Primitive PrimitiveValue.Unit
                    )
                | _ ->
                  return!
                    sum.Throw(
                      Errors.Singleton loc0 (fun () ->
                        "Expected a tuple with 3 elements (toId, sourceId, targetId) when moving after (reverse) in relation")
                    )
                    |> reader.OfSum
            } }

    memoryDBMoveAfterReverseId, MoveAfterReverseOperation

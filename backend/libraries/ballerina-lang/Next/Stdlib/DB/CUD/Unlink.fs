namespace Ballerina.DSL.Next.StdLib.DB.Extension

[<AutoOpen>]
module Unlink =
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

  let onUnlinkingHook
    (_db_ops: DBTypeClass<'runtimeContext, 'db, 'ext>)
    (relation: RelationRef<'db, 'ext>)
    (loc0: Location)
    (_fromId: Value<TypeValue<'ext>, 'ext>)
    (_toId: Value<TypeValue<'ext>, 'ext>)
    =
    reader {
      let _schema, _db, relation, _from, _to, schema_as_value = relation
      let _schema_as_value = schema_as_value.Value.Value

      match relation.Hooks.OnUnlinking with
      | None -> return ()
      | Some(hookExpr: Expr<TypeValue<'ext>, ResolvedIdentifier, 'ext>) ->
        let! run_hook_result =
          Expr.Apply(
            Expr.Apply(
              Expr.Apply(hookExpr, Expr.FromValue(_schema_as_value, TypeValue.CreateUnit(), Kind.Star)),
              Expr.FromValue(_fromId, TypeValue.CreateUnit(), Kind.Star)
            ),
            Expr.FromValue(_toId, TypeValue.CreateUnit(), Kind.Star)
          )
          |> NonEmptyList.One
          |> Expr.Eval

        let! result_case, result_value =
          run_hook_result
          |> Value.AsSum
          |> sum.MapError(Errors.MapContext(replaceWith loc0))
          |> reader.OfSum

        match result_case.Case with
        | 1 -> return ()
        | 2 ->
          return!
            sum.Throw(Errors.Singleton loc0 (fun () -> $"Unlinking hook failed with error {result_value}"))
            |> reader.OfSum
        | _ ->
          return!
            sum.Throw(Errors.Singleton loc0 (fun () -> "Unlinking hook returned invalid result"))
            |> reader.OfSum
    }

  let onUnlinkedHook
    (_db_ops: DBTypeClass<'runtimeContext, 'db, 'ext>)
    (relation: RelationRef<'db, 'ext>)
    (loc0: Location)
    (_fromId: Value<TypeValue<'ext>, 'ext>)
    (_toId: Value<TypeValue<'ext>, 'ext>)
    =
    reader {
      let _schema, _db, relation, _from, _to, schema_as_value = relation
      let _schema_as_value = schema_as_value.Value.Value

      match relation.Hooks.OnUnlinked with
      | None -> return ()
      | Some(hookExpr: Expr<TypeValue<'ext>, ResolvedIdentifier, 'ext>) ->
        let! run_hook_result =
          Expr.Apply(
            Expr.Apply(
              Expr.Apply(hookExpr, Expr.FromValue(_schema_as_value, TypeValue.CreateUnit(), Kind.Star)),
              Expr.FromValue(_fromId, TypeValue.CreateUnit(), Kind.Star)
            ),
            Expr.FromValue(_toId, TypeValue.CreateUnit(), Kind.Star)
          )
          |> NonEmptyList.One
          |> Expr.Eval

        let! result_case, result_value =
          run_hook_result
          |> Value.AsSum
          |> sum.MapError(Errors.MapContext(replaceWith loc0))
          |> reader.OfSum

        match result_case.Case with
        | 1 -> return ()
        | 2 ->
          return!
            sum.Throw(Errors.Singleton loc0 (fun () -> $"Unlinking hook failed with error {result_value}"))
            |> reader.OfSum
        | _ ->
          return!
            sum.Throw(Errors.Singleton loc0 (fun () -> "Unlinking hook returned invalid result"))
            |> reader.OfSum
    }

  let DBUnlinkExtension<'runtimeContext, 'db, 'ext when 'ext: comparison>
    (db_ops: DBTypeClass<'runtimeContext, 'db, 'ext>)
    (_listSet: List<Value<TypeValue<'ext>, 'ext>> -> 'ext)
    (valueLens: PartialLens<'ext, DBValues<'runtimeContext, 'db, 'ext>>)
    =
    let memoryDBUnlinkId =
      Identifier.FullyQualified([ "DB" ], "unlink") |> TypeCheckScope.Empty.Resolve

    let memoryDBUnlinkType =
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
                                    TypeExpr.Lookup("SchemaRelation" |> Identifier.LocalScope),
                                    TypeExpr.Lookup("schema" |> Identifier.LocalScope)

                                  ),
                                  TypeExpr.Lookup("from" |> Identifier.LocalScope)
                                ),
                                TypeExpr.Lookup("from_with_props" |> Identifier.LocalScope)
                              ),
                              TypeExpr.Lookup("from_id" |> Identifier.LocalScope)
                            ),
                            TypeExpr.Lookup("to" |> Identifier.LocalScope)
                          ),
                          TypeExpr.Lookup("to_with_props" |> Identifier.LocalScope)
                        ),
                        TypeExpr.Lookup("to_id" |> Identifier.LocalScope)
                      ),
                      TypeExpr.Arrow(
                        TypeExpr.Tuple
                          [ TypeExpr.Lookup("from_id" |> Identifier.LocalScope)
                            TypeExpr.Lookup("to_id" |> Identifier.LocalScope) ],
                        TypeExpr.Sum [ TypeExpr.Primitive PrimitiveType.Unit; TypeExpr.Primitive PrimitiveType.Unit ]
                      )
                    )
                  )
                )
              )
            )
          )
        )
      )

    let memoryDBUnlinkKind = standardSchemaRelationOperationKind

    let UnlinkOperation: OperationExtension<'runtimeContext, _, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBUnlinkType, memoryDBUnlinkKind, DBValues.Unlink {| RelationRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | DBValues.Unlink v -> Some(DBValues.Unlink v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DBValues.AsUnlink
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op.RelationRef with
              | None -> // the closure is empty - first step in the application
                let! v = extractRelationRefFromValue loc0 v valueLens "Relation"

                return
                  (DBValues.Unlink({| RelationRef = Some v |}) |> valueLens.Set, Some memoryDBUnlinkId)
                  |> Ext
              | Some relation_ref -> // the closure has the first operand - second step in the application

                let! v =
                  v
                  |> Value.AsTuple
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                match v with
                | [ _fromId; _toId ] ->

                  do! onUnlinkingHook db_ops relation_ref loc0 _fromId _toId

                  let! ctx = reader.GetContext()

                  do!
                    db_ops.Unlink relation_ref { FromId = _fromId; ToId = _toId }
                    |> Reader.Run ctx.RuntimeContext
                    |> sum.MapError(Errors.MapContext(replaceWith loc0))
                    |> reader.OfSum

                  do! onUnlinkedHook db_ops relation_ref loc0 _fromId _toId

                  return Value.Sum({ Case = 2; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
                | _ ->
                  return!
                    sum.Throw(
                      Errors.Singleton loc0 (fun () -> "Expected a tuple with 2 elements when unlinking relation")
                    )
                    |> reader.OfSum
            } }

    memoryDBUnlinkId, UnlinkOperation


  let DBUnlinkManyExtension<'runtimeContext, 'db, 'ext when 'ext: comparison>
    (db_ops: DBTypeClass<'runtimeContext, 'db, 'ext>)
    (listLens: PartialLens<'ext, List<Value<TypeValue<'ext>, 'ext>>>)
    (valueLens: PartialLens<'ext, DBValues<'runtimeContext, 'db, 'ext>>)
    =
    let memoryDBUnlinkManyId =
      Identifier.FullyQualified([ "DB" ], "unlinkMany")
      |> TypeCheckScope.Empty.Resolve

    let memoryDBUnlinkManyType =
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
                                    TypeExpr.Lookup("SchemaRelation" |> Identifier.LocalScope),
                                    TypeExpr.Lookup("schema" |> Identifier.LocalScope)

                                  ),
                                  TypeExpr.Lookup("from" |> Identifier.LocalScope)
                                ),
                                TypeExpr.Lookup("from_with_props" |> Identifier.LocalScope)
                              ),
                              TypeExpr.Lookup("from_id" |> Identifier.LocalScope)
                            ),
                            TypeExpr.Lookup("to" |> Identifier.LocalScope)
                          ),
                          TypeExpr.Lookup("to_with_props" |> Identifier.LocalScope)
                        ),
                        TypeExpr.Lookup("to_id" |> Identifier.LocalScope)
                      ),
                      TypeExpr.Arrow(
                        TypeExpr.Apply(
                          TypeExpr.Lookup("List" |> Identifier.LocalScope),
                          TypeExpr.Tuple
                            [ TypeExpr.Lookup("from_id" |> Identifier.LocalScope)
                              TypeExpr.Lookup("to_id" |> Identifier.LocalScope) ]
                        ),
                        TypeExpr.Sum [ TypeExpr.Primitive PrimitiveType.Unit; TypeExpr.Primitive PrimitiveType.Unit ]
                      )
                    )
                  )
                )
              )
            )
          )
        )
      )

    let memoryDBUnlinkManyKind = standardSchemaRelationOperationKind

    let UnlinkManyOperation: OperationExtension<'runtimeContext, _, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBUnlinkManyType, memoryDBUnlinkManyKind, DBValues.UnlinkMany {| RelationRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | DBValues.UnlinkMany v -> Some(DBValues.UnlinkMany v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DBValues.AsUnlinkMany
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op.RelationRef with
              | None -> // the closure is empty - first step in the application
                let! v = extractRelationRefFromValue loc0 v valueLens "Relation"

                return
                  (DBValues.UnlinkMany({| RelationRef = Some v |}) |> valueLens.Set, Some memoryDBUnlinkManyId)
                  |> Ext
              | Some relation_ref -> // the closure has the first operand - second step in the application

                let! vs, _ =
                  v
                  |> Value.AsExt
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                let! (vs: List<Value<TypeValue<'ext>, 'ext>>) =
                  vs
                  |> listLens.Get
                  |> sum.OfOption(Errors.Singleton loc0 (fun () -> "Cannot get value from extension"))
                  |> reader.OfSum

                let! (results: List<Value<TypeValue<'ext>, 'ext>>) =
                  vs
                  |> Seq.map (fun v ->
                    reader {
                      let! v =
                        v
                        |> Value.AsTuple
                        |> sum.MapError(Errors.MapContext(replaceWith loc0))
                        |> reader.OfSum

                      match v with
                      | [ _fromId; _toId ] ->

                        do! onUnlinkingHook db_ops relation_ref loc0 _fromId _toId

                        let! ctx = reader.GetContext()

                        do!
                          db_ops.Unlink relation_ref { FromId = _fromId; ToId = _toId }
                          |> Reader.Run ctx.RuntimeContext
                          |> sum.MapError(Errors.MapContext(replaceWith loc0))
                          |> reader.OfSum

                        do! onUnlinkedHook db_ops relation_ref loc0 _fromId _toId
                        return Value.Sum({ Case = 2; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
                      | _ ->
                        return!
                          sum.Throw(
                            Errors.Singleton loc0 (fun () -> "Expected a tuple with 2 elements when linking relation")
                          )
                          |> reader.OfSum
                    })
                  |> reader.All

                return Ext(results |> listLens.Set, None)
            } }

    memoryDBUnlinkManyId, UnlinkManyOperation

namespace Ballerina.DSL.Next.StdLib.MemoryDB.Extension

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
  open Ballerina.DSL.Next.StdLib.MemoryDB

  let onUnlinkingHook relation loc0 _schema_as_value _fromId _toId =
    reader {
      match relation.OnUnlinking with
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

  let onUnlinkedHook relation loc0 _schema_as_value _fromId _toId =
    reader {
      match relation.OnUnlinked with
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

  let MemoryDBUnlinkExtension<'ext when 'ext: comparison>
    (_listSet: List<Value<TypeValue<'ext>, 'ext>> -> 'ext)
    (valueLens: PartialLens<'ext, MemoryDBValues<'ext>>)
    =
    let memoryDBUnlinkId =
      Identifier.FullyQualified([ "MemoryDB" ], "unlink")
      |> TypeCheckScope.Empty.Resolve

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

    let UnlinkOperation: OperationExtension<_, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBUnlinkType, memoryDBUnlinkKind, MemoryDBValues.Unlink {| RelationRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | MemoryDBValues.Unlink v -> Some(MemoryDBValues.Unlink v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> MemoryDBValues.AsUnlink
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op.RelationRef with
              | None -> // the closure is empty - first step in the application
                let! v = extractRelationRefFromValue loc0 v valueLens "Relation"

                return
                  (MemoryDBValues.Unlink({| RelationRef = Some v |}) |> valueLens.Set, Some memoryDBUnlinkId)
                  |> Ext
              | Some(_schema, _db, _relation, _from, _to, _schema_as_value) -> // the closure has the first operand - second step in the application

                let! v =
                  v
                  |> Value.AsTuple
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                match v with
                | [ _fromId; _toId ] ->

                  do! onUnlinkingHook _relation loc0 _schema_as_value.Value.Value _fromId _toId

                  do removeRelationLink _db _relation _fromId _toId

                  do! onUnlinkedHook _relation loc0 _schema_as_value.Value.Value _fromId _toId

                  return Value.Sum({ Case = 2; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
                | _ ->
                  return!
                    sum.Throw(
                      Errors.Singleton loc0 (fun () -> "Expected a tuple with 2 elements when unlinking relation")
                    )
                    |> reader.OfSum
            } }

    memoryDBUnlinkId, UnlinkOperation


  let MemoryDBUnlinkManyExtension<'ext when 'ext: comparison>
    (listLens: PartialLens<'ext, List<Value<TypeValue<'ext>, 'ext>>>)
    (valueLens: PartialLens<'ext, MemoryDBValues<'ext>>)
    =
    let memoryDBUnlinkManyId =
      Identifier.FullyQualified([ "MemoryDB" ], "unlinkMany")
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

    let UnlinkManyOperation: OperationExtension<_, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBUnlinkManyType, memoryDBUnlinkManyKind, MemoryDBValues.UnlinkMany {| RelationRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | MemoryDBValues.UnlinkMany v -> Some(MemoryDBValues.UnlinkMany v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> MemoryDBValues.AsUnlinkMany
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op.RelationRef with
              | None -> // the closure is empty - first step in the application
                let! v = extractRelationRefFromValue loc0 v valueLens "Relation"

                return
                  (MemoryDBValues.UnlinkMany({| RelationRef = Some v |}) |> valueLens.Set, Some memoryDBUnlinkManyId)
                  |> Ext
              | Some(_schema, _db, _relation, _from, _to, schema_as_value) -> // the closure has the first operand - second step in the application

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

                        do! onUnlinkingHook _relation loc0 schema_as_value.Value.Value _fromId _toId

                        do removeRelationLink _db _relation _fromId _toId

                        do! onUnlinkedHook _relation loc0 schema_as_value.Value.Value _fromId _toId
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

namespace Ballerina.DSL.Next.StdLib.DB.Extension

[<AutoOpen>]
module IsLinked =
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

  let DBIsLinkedExtension<'runtimeContext, 'db, 'ext when 'ext: comparison>
    (db_ops: DBTypeClass<'runtimeContext, 'db, 'ext>)
    (_listSet: List<Value<TypeValue<'ext>, 'ext>> -> 'ext)
    (valueLens: PartialLens<'ext, DBValues<'runtimeContext, 'db, 'ext>>)
    =
    let memoryDBIsLinkedId =
      Identifier.FullyQualified([ "DB" ], "isLinked") |> TypeCheckScope.Empty.Resolve

    let memoryDBIsLinkedType =
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
                        TypeExpr.Primitive PrimitiveType.Bool
                      )
                    )
                  )
                )
              )
            )
          )
        )
      )

    let memoryDBIsLinkedKind = standardSchemaRelationOperationKind

    let IsLinkedOperation: OperationExtension<'runtimeContext, _, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBIsLinkedType, memoryDBIsLinkedKind, DBValues.IsLinked {| RelationRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | DBValues.IsLinked v -> Some(DBValues.IsLinked v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DBValues.AsIsLinked
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op.RelationRef with
              | None -> // the closure is empty - first step in the application
                let! v = extractRelationRefFromValue loc0 v valueLens "Relation"

                return
                  (DBValues.IsLinked({| RelationRef = Some v |}) |> valueLens.Set, Some memoryDBIsLinkedId)
                  |> Ext
              | Some relation_ref -> // the closure has the first operand - second step in the application

                let! v =
                  v
                  |> Value.AsTuple
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                match v with
                | [ _fromId; _toId ] ->

                  let! is_linked =
                    db_ops.IsLinked relation_ref { FromId = _fromId; ToId = _toId }
                    |> reader.MapError(Errors.MapContext(replaceWith loc0))

                  return Value.Primitive(PrimitiveValue.Bool is_linked)
                | _ ->
                  return!
                    sum.Throw(
                      Errors.Singleton loc0 (fun () -> "Expected a tuple with 2 elements when checking if linked")
                    )
                    |> reader.OfSum
            } }

    memoryDBIsLinkedId, IsLinkedOperation


  let DBLinkManyExtension<'runtimeContext, 'db, 'ext when 'ext: comparison>
    (db_ops: DBTypeClass<'runtimeContext, 'db, 'ext>)
    (listLens: PartialLens<'ext, List<Value<TypeValue<'ext>, 'ext>>>)
    (valueLens: PartialLens<'ext, DBValues<'runtimeContext, 'db, 'ext>>)
    =
    let memoryDBLinkId =
      Identifier.FullyQualified([ "DB" ], "linkMany") |> TypeCheckScope.Empty.Resolve

    let memoryDBLinkType =
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

    let memoryDBLinkKind = standardSchemaRelationOperationKind

    let LinkOperation: OperationExtension<'runtimeContext, _, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBLinkType, memoryDBLinkKind, DBValues.LinkMany {| RelationRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | DBValues.LinkMany v -> Some(DBValues.LinkMany v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DBValues.AsLinkMany
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op.RelationRef with
              | None -> // the closure is empty - first step in the application
                let! v = extractRelationRefFromValue loc0 v valueLens "Relation"

                return
                  (DBValues.LinkMany({| RelationRef = Some v |}) |> valueLens.Set, Some memoryDBLinkId)
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

                        do! onLinkingHook db_ops relation_ref loc0 _fromId _toId

                        do!
                          db_ops.Link relation_ref { FromId = _fromId; ToId = _toId }
                          |> reader.MapError(Errors.MapContext(replaceWith loc0))

                        do! onLinkedHook db_ops relation_ref loc0 _fromId _toId

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

    memoryDBLinkId, LinkOperation

namespace Ballerina.DSL.Next.StdLib.MemoryDB.Extension

[<AutoOpen>]
module Link =
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

  let onLinkingHook relation loc0 _schema_as_value _fromId _toId =
    reader {
      match relation.OnLinking with
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
            sum.Throw(Errors.Singleton loc0 (fun () -> $"Linking hook failed with error {result_value}"))
            |> reader.OfSum
        | _ ->
          return!
            sum.Throw(Errors.Singleton loc0 (fun () -> "Linking hook returned invalid result"))
            |> reader.OfSum
    }

  let onLinkedHook relation loc0 _schema_as_value _fromId _toId =
    reader {
      match relation.OnLinked with
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
            sum.Throw(Errors.Singleton loc0 (fun () -> $"Linked hook failed with error {result_value}"))
            |> reader.OfSum
        | _ ->
          return!
            sum.Throw(Errors.Singleton loc0 (fun () -> "Linked hook returned invalid result"))
            |> reader.OfSum
    }

  let MemoryDBLinkExtension<'ext when 'ext: comparison>
    (_listSet: List<Value<TypeValue<'ext>, 'ext>> -> 'ext)
    (valueLens: PartialLens<'ext, MemoryDBValues<'ext>>)
    =
    let memoryDBLinkId =
      Identifier.FullyQualified([ "MemoryDB" ], "link")
      |> TypeCheckScope.Empty.Resolve

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

    let memoryDBLinkKind = standardSchemaRelationOperationKind

    let LinkOperation: OperationExtension<_, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBLinkType, memoryDBLinkKind, MemoryDBValues.Link {| RelationRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | MemoryDBValues.Link v -> Some(MemoryDBValues.Link v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> MemoryDBValues.AsLink
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op.RelationRef with
              | None -> // the closure is empty - first step in the application
                let! v = extractRelationRefFromValue loc0 v valueLens "Relation"

                return
                  (MemoryDBValues.Link({| RelationRef = Some v |}) |> valueLens.Set, Some memoryDBLinkId)
                  |> Ext
              | Some(_schema, _db, _relation, _from, _to, schema_as_value) -> // the closure has the first operand - second step in the application

                let! v =
                  v
                  |> Value.AsTuple
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                match v with
                | [ _fromId; _toId ] ->

                  do! onLinkingHook _relation loc0 schema_as_value.Value.Value _fromId _toId

                  do addRelationLink _db _relation _fromId _toId

                  do! onLinkedHook _relation loc0 schema_as_value.Value.Value _fromId _toId

                  return Value.Sum({ Case = 2; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
                | _ ->
                  return!
                    sum.Throw(
                      Errors.Singleton loc0 (fun () -> "Expected a tuple with 2 elements when linking relation")
                    )
                    |> reader.OfSum
            } }

    memoryDBLinkId, LinkOperation


  let MemoryDBLinkManyExtension<'ext when 'ext: comparison>
    (listLens: PartialLens<'ext, List<Value<TypeValue<'ext>, 'ext>>>)
    (valueLens: PartialLens<'ext, MemoryDBValues<'ext>>)
    =
    let memoryDBLinkId =
      Identifier.FullyQualified([ "MemoryDB" ], "linkMany")
      |> TypeCheckScope.Empty.Resolve

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

    let LinkOperation: OperationExtension<_, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBLinkType, memoryDBLinkKind, MemoryDBValues.LinkMany {| RelationRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | MemoryDBValues.LinkMany v -> Some(MemoryDBValues.LinkMany v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> MemoryDBValues.AsLinkMany
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op.RelationRef with
              | None -> // the closure is empty - first step in the application
                let! v = extractRelationRefFromValue loc0 v valueLens "Relation"

                return
                  (MemoryDBValues.LinkMany({| RelationRef = Some v |}) |> valueLens.Set, Some memoryDBLinkId)
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

                        do! onLinkingHook _relation loc0 schema_as_value.Value.Value _fromId _toId

                        do addRelationLink _db _relation _fromId _toId

                        do! onLinkedHook _relation loc0 schema_as_value.Value.Value _fromId _toId

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

namespace Ballerina.DSL.Next.StdLib.MemoryDB.Extension

[<AutoOpen>]
module QueryFromRelation =
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

  let MemoryDBQueryFromRelationExtension<'ext when 'ext: comparison>
    (listLens: PartialLens<'ext, List<Value<TypeValue<'ext>, 'ext>>>)
    (valueLens: PartialLens<'ext, MemoryDBValues<'ext>>)
    =

    let memoryDBQueryFromRelationId =
      Identifier.FullyQualified([ "MemoryDB" ], "queryFromRelation")
      |> TypeCheckScope.Empty.Resolve

    let memoryDBQueryFromRelationType =
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
                      TypeExpr.Apply(
                        TypeExpr.Apply(
                          TypeExpr.Lookup("Query" |> Identifier.LocalScope),
                          TypeExpr.Lookup("schema" |> Identifier.LocalScope)
                        ),
                        TypeExpr.Tuple
                          [ TypeExpr.Lookup("from_id" |> Identifier.LocalScope)
                            TypeExpr.Lookup("to_id" |> Identifier.LocalScope) ]
                      )
                    )
                  )
                )
              )
            )
          )
        )
      )

    let memoryDBQueryFromRelationKind = standardSchemaRelationOperationKind

    let queryFromRelationOperation: OperationExtension<_, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBQueryFromRelationType, memoryDBQueryFromRelationKind, MemoryDBValues.QueryFromRelation())
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | MemoryDBValues.QueryFromRelation v -> Some(MemoryDBValues.QueryFromRelation v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              do!
                op
                |> MemoryDBValues.AsQueryFromRelation
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              let! (_, db, schema_relation, _, _, _) = extractRelationRefFromValue loc0 v valueLens "Relation"

              let relation_table =
                db.relations
                |> Map.tryFind schema_relation.Name
                |> Option.map (fun r -> r.All)
                |> Option.defaultValue Set.empty

              let result =
                relation_table
                |> Set.toSeq
                |> Seq.map (fun (fromId, toId) -> Value.Tuple [ fromId; toId ])
                |> Seq.toList

              return Value.Ext(listLens.Set result, None)
            } }

    memoryDBQueryFromRelationId, queryFromRelationOperation

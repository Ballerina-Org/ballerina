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
          <| (memoryDBQueryFromRelationType,
              memoryDBQueryFromRelationKind,
              MemoryDBValues.GetById {| EntityRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | MemoryDBValues.GetById v -> Some(MemoryDBValues.GetById v)
            | _ -> None)
        Apply =
          fun _loc0 _rest (_op, _v) ->
            reader {
              return Value.Primitive(PrimitiveValue.Unit)
            // let! op =
            //   op
            //   |> MemoryDBValues.AsGetById
            //   |> sum.MapError(Errors.MapContext(replaceWith loc0))
            //   |> reader.OfSum

            // match op with
            // | None -> // the closure is empty - first step in the application
            //   let! v, _ =
            //     v
            //     |> Value.AsExt
            //     |> sum.MapError(Errors.MapContext(replaceWith loc0))
            //     |> reader.OfSum

            //   let! v =
            //     v
            //     |> valueLens.Get
            //     |> sum.OfOption(Errors.Singleton loc0 (fun () -> "Cannot get value from extension"))
            //     |> reader.OfSum

            //   let! v =
            //     v
            //     |> MemoryDBValues.AsEntityRef
            //     |> sum.MapError(Errors.MapContext(replaceWith loc0))
            //     |> reader.OfSum

            //   return
            //     (MemoryDBValues.GetById({| EntityRef = Some v |}) |> valueLens.Set, Some memoryDBQueryFromEntityId)
            //     |> Ext
            // | Some(_schema, _db, _entity, _schema_as_value) -> // the closure has the first operand - second step in the application
            //   let v =
            //     option {
            //       let! entity = _db.entities |> Map.tryFind _entity.Name
            //       let! value = entity |> Map.tryFind v
            //       return value
            //     }

            //   match v with
            //   | None -> return Value.Sum({ Case = 1; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
            //   | Some value -> return Value.Sum({ Case = 2; Count = 2 }, value)
            } }

    memoryDBQueryFromRelationId, queryFromRelationOperation

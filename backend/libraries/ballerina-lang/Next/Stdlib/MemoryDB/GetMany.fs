namespace Ballerina.DSL.Next.StdLib.MemoryDB.Extension

[<AutoOpen>]
module GetMany =
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

  let MemoryDBGetManyExtension<'ext when 'ext: comparison>
    (listSet: List<Value<TypeValue<'ext>, 'ext>> -> 'ext)
    (valueLens: PartialLens<'ext, MemoryDBValues<'ext>>)
    : OperationsExtension<'ext, MemoryDBValues<'ext>> =

    let memoryDBGetMany =
      Identifier.FullyQualified([ "MemoryDB" ], "getMany")
      |> TypeCheckScope.Empty.Resolve

    let memoryDBGetManyType =
      TypeValue.CreateLambda(
        TypeParameter.Create("schema", Kind.Schema),
        TypeExpr.Lambda(
          TypeParameter.Create("entity", Kind.Star),
          TypeExpr.Lambda(
            TypeParameter.Create("entity_with_props", Kind.Star),
            TypeExpr.Lambda(
              TypeParameter.Create("entityId", Kind.Star),
              TypeExpr.Arrow(
                TypeExpr.Apply(
                  TypeExpr.Apply(
                    TypeExpr.Apply(
                      TypeExpr.Apply(
                        TypeExpr.Lookup("SchemaEntity" |> Identifier.LocalScope),
                        TypeExpr.Lookup("schema" |> Identifier.LocalScope)

                      ),
                      TypeExpr.Lookup("entity" |> Identifier.LocalScope)
                    ),
                    TypeExpr.Lookup("entity_with_props" |> Identifier.LocalScope)
                  ),
                  TypeExpr.Lookup("entityId" |> Identifier.LocalScope)
                ),
                TypeExpr.Arrow(
                  TypeExpr.Tuple
                    [ TypeExpr.Primitive PrimitiveType.Int32
                      TypeExpr.Primitive PrimitiveType.Int32 ],
                  TypeExpr.Apply(
                    TypeExpr.Lookup("List" |> Identifier.LocalScope),
                    TypeExpr.Lookup("entity_with_props" |> Identifier.LocalScope)
                  )
                )
              )
            )
          )
        )
      )

    let memoryDBGetManyKind =
      Kind.Arrow(Kind.Schema, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))))

    let getManyOperation: OperationExtension<_, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBGetManyType, memoryDBGetManyKind, MemoryDBValues.GetMany {| EntityRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | MemoryDBValues.GetMany v -> Some(MemoryDBValues.GetMany v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> MemoryDBValues.AsGetMany
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
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
                  |> MemoryDBValues.AsEntityRef
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                return
                  (MemoryDBValues.GetMany({| EntityRef = Some v |}) |> valueLens.Set, Some memoryDBGetMany)
                  |> Ext
              | Some(_schema, _db, _entity, _schema_as_value) -> // the closure has the first operand - second step in the application

                let! v =
                  v
                  |> Value.AsTuple
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                match v with
                | [ Value.Primitive(PrimitiveValue.Int32 _offset); Value.Primitive(PrimitiveValue.Int32 _limit) ] ->
                  let results =
                    option {
                      let! allValues = _db.entities |> Map.tryFind _entity.Name

                      let pagedValues =
                        allValues |> Map.values |> Seq.skip _offset |> Seq.truncate _limit |> Seq.toList

                      return listSet pagedValues
                    }

                  match results with
                  | Some res -> return (res, None) |> Ext
                  | None ->
                    return!
                      Errors.Singleton loc0 (fun () -> "Entity or values not found in MemoryDB")
                      |> reader.Throw
                | _ ->
                  return!
                    Errors.Singleton loc0 (fun () -> "Expected a tuple of two Int32 values for offset and limit")
                    |> reader.Throw
            } }

    { TypeVars = []
      Operations = [ memoryDBGetMany, getManyOperation ] |> Map.ofList }

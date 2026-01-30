namespace Ballerina.DSL.Next.StdLib.MemoryDB.Extension

[<AutoOpen>]
module DeleteMany =
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


  let MemoryDBDeleteManyExtension<'ext when 'ext: comparison>
    (mapLens: PartialLens<'ext, Map<Value<TypeValue<'ext>, 'ext>, Value<TypeValue<'ext>, 'ext>>>)
    (valueLens: PartialLens<'ext, MemoryDBValues<'ext>>)
    =

    let memoryDBDeleteManyId =
      Identifier.FullyQualified([ "MemoryDB" ], "deleteMany")
      |> TypeCheckScope.Empty.Resolve

    let memoryDBDeleteManyType =
      TypeValue.CreateLambda(
        TypeParameter.Create("schema", Kind.Schema),
        TypeExpr.Lambda(
          TypeParameter.Create("entity", Kind.Star),
          TypeExpr.Lambda(
            TypeParameter.Create("entity_with_props", Kind.Star),
            TypeExpr.Lambda(
              TypeParameter.Create("entityId", Kind.Star),
              TypeExpr.Arrow(
                createSchemaEntityTypeApplication "schema" "entity" "entity_with_props" "entityId",
                TypeExpr.Arrow(
                  TypeExpr.Apply(
                    TypeExpr.Apply(
                      TypeExpr.Lookup("Map" |> Identifier.LocalScope),
                      TypeExpr.Lookup("entityId" |> Identifier.LocalScope)
                    ),
                    TypeExpr.Primitive PrimitiveType.Unit
                  ),
                  TypeExpr.Apply(
                    TypeExpr.Apply(
                      TypeExpr.Lookup("Map" |> Identifier.LocalScope),
                      TypeExpr.Lookup("entityId" |> Identifier.LocalScope)
                    ),
                    TypeExpr.Primitive PrimitiveType.Bool
                  )
                )
              )
            )
          )
        )
      )

    let memoryDBDeleteManyKind = standardSchemaOperationKind

    let DeleteManyOperation: OperationExtension<_, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBDeleteManyType, memoryDBDeleteManyKind, MemoryDBValues.DeleteMany {| EntityRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | MemoryDBValues.DeleteMany v -> Some(MemoryDBValues.DeleteMany v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> MemoryDBValues.AsDeleteMany
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                let! v = extractEntityRefFromValue loc0 v valueLens

                return
                  (MemoryDBValues.DeleteMany({| EntityRef = Some v |}) |> valueLens.Set, Some memoryDBDeleteManyId)
                  |> Ext
              | Some(_schema, _db, _entity, _schema_as_value) -> // the closure has the first operand - second step in the application

                let! vs, _ =
                  v
                  |> Value.AsExt
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                let! vs =
                  vs
                  |> mapLens.Get
                  |> sum.OfOption(Errors.Singleton loc0 (fun () -> "Cannot get value from extension"))
                  |> reader.OfSum

                let! (res: Map<Value<TypeValue<'ext>, 'ext>, Value<TypeValue<'ext>, 'ext>>) =
                  vs
                  |> Map.map (fun _entityId _unit ->
                    reader {
                      let existingValue = lookupEntityValue _db _entity _entityId

                      match existingValue with
                      | None -> return Value.Primitive(PrimitiveValue.Bool false)
                      | Some _ ->
                        do removeEntityValue _db _entity _entityId

                        return Value.Primitive(PrimitiveValue.Bool true)
                    })
                  |> reader.AllMap

                return (res |> mapLens.Set, None) |> Ext
            } }

    memoryDBDeleteManyId, DeleteManyOperation

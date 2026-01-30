namespace Ballerina.DSL.Next.StdLib.MemoryDB.Extension

[<AutoOpen>]
module UpsertMany =
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

  let MemoryDBUpsertManyExtension<'ext when 'ext: comparison>
    (
      calculateProps:
        Value<TypeValue<'ext>, 'ext>
          -> SchemaEntity<'ext>
          -> Reader<Value<TypeValue<'ext>, 'ext>, ExprEvalContext<'ext>, Errors<Location>>,
      stripProps
    )
    (mapLens: PartialLens<'ext, Map<Value<TypeValue<'ext>, 'ext>, Value<TypeValue<'ext>, 'ext>>>)
    (valueLens: PartialLens<'ext, MemoryDBValues<'ext>>)
    =

    let memoryDBUpsertManyId =
      Identifier.FullyQualified([ "MemoryDB" ], "upsertMany")
      |> TypeCheckScope.Empty.Resolve

    let memoryDBUpsertManyType =
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
                    TypeExpr.Tuple
                      [ TypeExpr.Lookup("entity" |> Identifier.LocalScope)
                        TypeExpr.Arrow(
                          TypeExpr.Lookup("entity_with_props" |> Identifier.LocalScope),
                          TypeExpr.Arrow(
                            TypeExpr.Lookup("entity" |> Identifier.LocalScope),
                            TypeExpr.Lookup("entity" |> Identifier.LocalScope)
                          )
                        ) ]
                  ),
                  TypeExpr.Apply(
                    TypeExpr.Apply(
                      TypeExpr.Lookup("Map" |> Identifier.LocalScope),
                      TypeExpr.Lookup("entityId" |> Identifier.LocalScope)
                    ),
                    TypeExpr.Sum
                      [ TypeExpr.Primitive PrimitiveType.Unit
                        TypeExpr.Lookup("entity_with_props" |> Identifier.LocalScope) ]
                  )
                )
              )
            )
          )
        )
      )

    let memoryDBUpsertManyKind = standardSchemaOperationKind

    let UpsertManyOperation: OperationExtension<_, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBUpsertManyType, memoryDBUpsertManyKind, MemoryDBValues.UpsertMany {| EntityRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | MemoryDBValues.UpsertMany v -> Some(MemoryDBValues.UpsertMany v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> MemoryDBValues.AsUpsertMany
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                let! v = extractEntityRefFromValue loc0 v valueLens

                return
                  (MemoryDBValues.UpsertMany({| EntityRef = Some v |}) |> valueLens.Set, Some memoryDBUpsertManyId)
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
                  |> Map.map (fun _entityId v ->
                    reader {
                      let! v =
                        v
                        |> Value.AsTuple
                        |> sum.MapError(Errors.MapContext(replaceWith loc0))
                        |> reader.OfSum

                      match v with
                      | [ valueToInsert: Value<TypeValue<'ext>, 'ext>; updateFunc ] ->
                        let existingValue =
                          option {
                            let! entity = _db.entities |> Map.tryFind _entity.Name
                            let! value = entity |> Map.tryFind _entityId
                            return value
                          }

                        match existingValue with
                        | None ->
                          let! valueWithProps = calculateProps valueToInsert _entity

                          let! valueWithProps =
                            onCreatingHook
                              calculateProps
                              _entity
                              _schema_as_value.Value.Value
                              loc0
                              _entityId
                              valueToInsert
                              valueWithProps

                          do addEntityValue _db _entity _entityId valueWithProps

                          do!
                            onCreatedHook
                              calculateProps
                              _entity
                              _schema_as_value.Value.Value
                              loc0
                              _entityId
                              valueToInsert
                              valueWithProps

                          return Value.Sum({ Case = 2; Count = 2 }, valueWithProps)
                        | Some existingValue ->
                          let! existingValueWithoutProps = stripProps existingValue _entity

                          let! updatedValue =
                            Expr.Apply(
                              Expr.Apply(
                                Expr.FromValue(updateFunc, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star),
                                Expr.FromValue(existingValue, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                              ),
                              Expr.FromValue(
                                existingValueWithoutProps,
                                TypeValue.CreatePrimitive PrimitiveType.Unit,
                                Kind.Star
                              )
                            )
                            |> NonEmptyList.One
                            |> Expr.Eval

                          let! valueWithProps = calculateProps updatedValue _entity

                          let! valueWithProps =
                            onUpdatingHook
                              calculateProps
                              _entity
                              _schema_as_value.Value.Value
                              loc0
                              _entityId
                              existingValue
                              updatedValue
                              valueWithProps

                          do updateEntityValue _db _entity _entityId valueWithProps

                          do!
                            onUpdatedHook
                              _entity
                              _schema_as_value.Value.Value
                              loc0
                              _entityId
                              existingValue
                              valueWithProps

                          return Value.Sum({ Case = 2; Count = 2 }, valueWithProps)
                      | _ ->
                        return!
                          sum.Throw(
                            Errors.Singleton loc0 (fun () -> "Expected a tuple with 2 elements when upserting into DB")
                          )
                          |> reader.OfSum

                    })
                  |> reader.AllMap

                return (res |> mapLens.Set, None) |> Ext
            } }

    memoryDBUpsertManyId, UpsertManyOperation

namespace Ballerina.DSL.Next.StdLib.DB.Extension

[<AutoOpen>]
module UpdateMany =
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


  let DBUpdateManyExtension<'runtimeContext, 'db, 'ext when 'ext: comparison>
    (db_ops: DBTypeClass<'runtimeContext, 'db, 'ext>)
    (
      calculateProps:
        DBTypeClass<'runtimeContext, 'db, 'ext>
          -> Value<TypeValue<'ext>, 'ext>
          -> EntityRef<'db, 'ext>
          -> Reader<Value<TypeValue<'ext>, 'ext>, ExprEvalContext<'runtimeContext, 'ext>, Errors<Location>>,
      stripProps:
        DBTypeClass<'runtimeContext, 'db, 'ext>
          -> Value<TypeValue<'ext>, 'ext>
          -> EntityRef<'db, 'ext>
          -> Reader<Value<TypeValue<'ext>, 'ext>, ExprEvalContext<'runtimeContext, 'ext>, Errors<Location>>
    )
    (mapLens: PartialLens<'ext, Map<Value<TypeValue<'ext>, 'ext>, Value<TypeValue<'ext>, 'ext>>>)
    (valueLens: PartialLens<'ext, DBValues<'runtimeContext, 'db, 'ext>>)
    =

    let memoryDBUpdateManyId =
      Identifier.FullyQualified([ "DB" ], "updateMany")
      |> TypeCheckScope.Empty.Resolve

    let memoryDBUpdateManyType =
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
                    TypeExpr.Arrow(
                      TypeExpr.Lookup("entity_with_props" |> Identifier.LocalScope),
                      TypeExpr.Arrow(
                        TypeExpr.Lookup("entity" |> Identifier.LocalScope),
                        TypeExpr.Lookup("entity" |> Identifier.LocalScope)
                      )
                    )
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

    let memoryDBUpdateManyKind = standardSchemaOperationKind

    let UpdateManyOperation: OperationExtension<'runtimeContext, _, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBUpdateManyType, memoryDBUpdateManyKind, DBValues.UpdateMany {| EntityRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | DBValues.UpdateMany v -> Some(DBValues.UpdateMany v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DBValues.AsUpdateMany
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                let! v = extractEntityRefFromValue loc0 v valueLens

                return
                  (DBValues.UpdateMany({| EntityRef = Some v |}) |> valueLens.Set, Some memoryDBUpdateManyId)
                  |> Ext
              | Some entity_ref -> // the closure has the first operand - second step in the application

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
                  |> Map.map (fun _entityId updateFunc ->
                    reader {
                      let! existingValue =
                        db_ops.GetById entity_ref _entityId
                        |> reader.MapError(Errors.MapContext(replaceWith loc0))
                        |> reader.Catch
                        |> reader.Map Sum.toOption

                      match existingValue with
                      | None -> return Value.Sum({ Case = 1; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
                      | Some existingValue ->
                        let actual_update =
                          reader {
                            let! existingValueWithoutProps = stripProps db_ops existingValue entity_ref

                            let! updatedValue =
                              Expr.Apply(
                                Expr.Apply(
                                  Expr.FromValue(updateFunc, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star),
                                  Expr.FromValue(
                                    existingValue,
                                    TypeValue.CreatePrimitive PrimitiveType.Unit,
                                    Kind.Star
                                  )
                                ),
                                Expr.FromValue(
                                  existingValueWithoutProps,
                                  TypeValue.CreatePrimitive PrimitiveType.Unit,
                                  Kind.Star
                                )
                              )
                              |> NonEmptyList.One
                              |> Expr.Eval

                            let! valueWithProps = calculateProps db_ops updatedValue entity_ref

                            let! valueWithProps =
                              onUpdatingHook
                                db_ops
                                calculateProps
                                entity_ref
                                loc0
                                _entityId
                                existingValue
                                updatedValue
                                valueWithProps

                            let! _ =
                              db_ops.Update
                                entity_ref
                                { Id = _entityId
                                  Previous = existingValue
                                  Value = valueWithProps }
                              |> reader.MapError(Errors.MapContext(replaceWith loc0))

                            do! onUpdatedHook db_ops entity_ref loc0 _entityId existingValue valueWithProps

                            return Value.Sum({ Case = 2; Count = 2 }, valueWithProps)
                          }
                          |> reader.MapContext(ExprEvalContext.Updaters.RootLevelEval(replaceWith false))

                        return!
                          reader {
                            let _, _, entity, schema_value = entity_ref
                            let! ctx = reader.GetContext()

                            match ctx.RootLevelEval, entity.Hooks.CanUpdate with
                            | true, Some canUpdateHook ->
                              match!
                                Expr.Apply(
                                  canUpdateHook,
                                  Expr.FromValue(schema_value.Value.Value, TypeValue.CreateUnit(), Kind.Star)
                                )
                                |> NonEmptyList.One
                                |> Expr.Eval
                              with
                              | Value.Primitive(PrimitiveValue.Bool canUpdate) when canUpdate -> return! actual_update
                              | _ -> return Value.Sum({ Case = 1; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
                            | _ -> return! actual_update
                          }


                    })
                  |> reader.AllMap

                return (res |> mapLens.Set, None) |> Ext
            } }

    memoryDBUpdateManyId, UpdateManyOperation

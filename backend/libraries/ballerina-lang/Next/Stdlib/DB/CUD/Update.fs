namespace Ballerina.DSL.Next.StdLib.DB.Extension

[<AutoOpen>]
module Update =
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

  let onUpdatingHook<'runtimeContext, 'db, 'ext when 'ext: comparison>
    (db_ops: DBTypeClass<'runtimeContext, 'db, 'ext>)
    (calculateProps:
      DBTypeClass<'runtimeContext, 'db, 'ext>
        -> Value<TypeValue<'ext>, 'ext>
        -> EntityRef<'db, 'ext>
        -> Reader<Value<TypeValue<'ext>, 'ext>, ExprEvalContext<'runtimeContext, 'ext>, Errors<Location>>)
    (entity_ref: EntityRef<'db, 'ext>)
    (loc0: Location)
    (_entityId: Value<TypeValue<'ext>, 'ext>)
    (previousValueWithProps: Value<TypeValue<'ext>, 'ext>)
    (current_value: Value<TypeValue<'ext>, 'ext>)
    (currentValueWithProps: Value<TypeValue<'ext>, 'ext>)

    : Reader<Value<TypeValue<'ext>, 'ext>, ExprEvalContext<'runtimeContext, 'ext>, Errors<Location>> =
    reader {
      let _schema, _db, _entity, _schema_as_value = entity_ref
      let _schema_as_value = _schema_as_value.Value.Value

      match _entity.Hooks.OnUpdating with
      | Some hookExpr ->
        let _doRunHookExpr =
          TypeCheckedExpr.Apply(
            TypeCheckedExpr.Apply(
              TypeCheckedExpr.Apply(
                TypeCheckedExpr.Apply(
                  TypeCheckedExpr.Apply(
                    hookExpr,
                    TypeCheckedExpr.FromValue(_schema_as_value, TypeValue.CreateUnit(), Kind.Star)
                  ),
                  TypeCheckedExpr.FromValue(_entityId, TypeValue.CreateUnit(), Kind.Star)
                ),
                TypeCheckedExpr.FromValue(previousValueWithProps, TypeValue.CreateUnit(), Kind.Star)
              ),
              TypeCheckedExpr.FromValue(current_value, TypeValue.CreateUnit(), Kind.Star)
            ),
            TypeCheckedExpr.FromValue(currentValueWithProps, TypeValue.CreateUnit(), Kind.Star)
          )

        let! run_hook_result = _doRunHookExpr |> NonEmptyList.One |> Expr.Eval

        let! result_case, result_value =
          run_hook_result
          |> Value.AsSum
          |> sum.MapError(Errors.MapContext(replaceWith loc0))
          |> reader.OfSum

        match result_case.Case with
        | 1 -> return currentValueWithProps
        | 2 ->
          return!
            sum.Throw(Errors.Singleton loc0 (fun () -> $"On updating hook returned error {result_value}"))
            |> reader.OfSum
        | 3 ->
          let modified_value_to_insert = result_value
          return! calculateProps db_ops modified_value_to_insert entity_ref
        | _ ->
          return!
            sum.Throw(Errors.Singleton loc0 (fun () -> $"On updating hook returned unexpected value {result_value}"))
            |> reader.OfSum
      | None -> return currentValueWithProps
    }


  let onUpdatedHook<'runtimeContext, 'db, 'ext when 'ext: comparison>
    (_db_ops: DBTypeClass<'runtimeContext, 'db, 'ext>)
    (entity_ref: EntityRef<'db, 'ext>)
    (loc0: Location)
    (_entityId: Value<TypeValue<'ext>, 'ext>)
    (previousValueWithProps: Value<TypeValue<'ext>, 'ext>)
    (currentValueWithProps: Value<TypeValue<'ext>, 'ext>)

    : Reader<Unit, ExprEvalContext<'runtimeContext, 'ext>, Errors<Location>> =
    reader {
      let _schema, _db, _entity, _schema_as_value = entity_ref
      let _schema_as_value = _schema_as_value.Value.Value

      match _entity.Hooks.OnUpdated with
      | Some hookExpr ->
        let _doRunHookExpr =
          TypeCheckedExpr.Apply(
            TypeCheckedExpr.Apply(
              TypeCheckedExpr.Apply(
                TypeCheckedExpr.Apply(
                  hookExpr,
                  TypeCheckedExpr.FromValue(_schema_as_value, TypeValue.CreateUnit(), Kind.Star)
                ),
                TypeCheckedExpr.FromValue(_entityId, TypeValue.CreateUnit(), Kind.Star)
              ),
              TypeCheckedExpr.FromValue(previousValueWithProps, TypeValue.CreateUnit(), Kind.Star)
            ),
            TypeCheckedExpr.FromValue(currentValueWithProps, TypeValue.CreateUnit(), Kind.Star)
          )

        let! run_hook_result = _doRunHookExpr |> NonEmptyList.One |> Expr.Eval

        let! result_case, result_value =
          run_hook_result
          |> Value.AsSum
          |> sum.MapError(Errors.MapContext(replaceWith loc0))
          |> reader.OfSum

        match result_case.Case with
        | 1 -> return ()
        | 2 ->
          return!
            sum.Throw(Errors.Singleton loc0 (fun () -> $"On updated hook returned error {result_value}"))
            |> reader.OfSum
        | _ ->
          return!
            sum.Throw(Errors.Singleton loc0 (fun () -> $"On updated hook returned unexpected value {result_value}"))
            |> reader.OfSum
      | None -> return ()
    }

  let DBUpdateExtension<'runtimeContext, 'db, 'ext when 'ext: comparison>
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
    (_listSet: List<Value<TypeValue<'ext>, 'ext>> -> 'ext)
    (valueLens: PartialLens<'ext, DBValues<'runtimeContext, 'db, 'ext>>)
    =
    let memoryDBUpdateId =
      Identifier.FullyQualified([ "DB" ], "update") |> TypeCheckScope.Empty.Resolve

    let memoryDBUpdateType =
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
                  TypeExpr.Tuple
                    [ TypeExpr.Lookup("entityId" |> Identifier.LocalScope)
                      TypeExpr.Arrow(
                        TypeExpr.Lookup("entity_with_props" |> Identifier.LocalScope),
                        TypeExpr.Arrow(
                          TypeExpr.Lookup("entity" |> Identifier.LocalScope),
                          TypeExpr.Lookup("entity" |> Identifier.LocalScope)
                        )
                      ) ],
                  TypeExpr.Sum
                    [ TypeExpr.Primitive PrimitiveType.Unit
                      TypeExpr.Lookup("entity_with_props" |> Identifier.LocalScope) ]
                )
              )
            )
          )
        )
      )

    let memoryDBUpdateKind = standardSchemaOperationKind

    let UpdateOperation: OperationExtension<'runtimeContext, _, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBUpdateType, memoryDBUpdateKind, DBValues.Update {| EntityRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | DBValues.Update v -> Some(DBValues.Update v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DBValues.AsUpdate
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                let! v = extractEntityRefFromValue loc0 v valueLens

                return
                  (DBValues.Update({| EntityRef = Some v |}) |> valueLens.Set, Some memoryDBUpdateId)
                  |> Ext
              | Some entity_ref -> // the closure has the first operand - second step in the application

                let! v =
                  v
                  |> Value.AsTuple
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                match v with
                | [ _entityId; updateFunc ] ->
                  let! existingValue =
                    db_ops.GetById entity_ref _entityId
                    |> reader.MapError(Errors.MapContext(replaceWith loc0))
                    |> reader.Catch


                  match existingValue with
                  | Right _errors ->
                    // let _, _, entity, _ = entity_ref
                    // do Console.WriteLine $"Error getting {entity.Name.Name} with id {_entityId}: {errors.Errors()}"
                    // do Console.ReadLine() |> ignore
                    return Value.Sum({ Case = 1; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
                  | Left existingValue ->
                    let actual_update =
                      reader {
                        let! existingValueWithoutProps = stripProps db_ops existingValue entity_ref

                        let! updatedValue =
                          TypeCheckedExpr.Apply(
                            TypeCheckedExpr.Apply(
                              TypeCheckedExpr.FromValue(
                                updateFunc,
                                TypeValue.CreatePrimitive PrimitiveType.Unit,
                                Kind.Star
                              ),
                              TypeCheckedExpr.FromValue(
                                existingValue,
                                TypeValue.CreatePrimitive PrimitiveType.Unit,
                                Kind.Star
                              )
                            ),
                            TypeCheckedExpr.FromValue(
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
                          // do Console.WriteLine $"Running canUpdate hook for entity {entity.Name}"
                          // do Console.WriteLine $"Hook itself: {canUpdateHook}"
                          // do Console.WriteLine $"Existing value: {existingValue}"
                          // do Console.ReadLine() |> ignore
                          match!
                            TypeCheckedExpr.Apply(
                              TypeCheckedExpr.Apply(
                                TypeCheckedExpr.Apply(
                                  canUpdateHook,
                                  TypeCheckedExpr.FromValue(schema_value.Value.Value, TypeValue.CreateUnit(), Kind.Star)
                                ),
                                TypeCheckedExpr.FromValue(_entityId, TypeValue.CreateUnit(), Kind.Star)
                              ),
                              TypeCheckedExpr.FromValue(existingValue, TypeValue.CreateUnit(), Kind.Star)
                            )
                            |> NonEmptyList.One
                            |> Expr.Eval
                          with
                          | Value.Primitive(PrimitiveValue.Bool canUpdate) when canUpdate -> return! actual_update
                          | _res ->
                            // do Console.WriteLine $"Unexpected result from canUpdate hook: {res}"
                            // do Console.ReadLine() |> ignore

                            return Value.Sum({ Case = 1; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
                        | _ ->
                          // do Console.WriteLine $"There is no canUpdate hook for entity {entity.Name}, proceeding with update"
                          // do Console.ReadLine() |> ignore
                          return! actual_update
                      }
                | _ ->
                  return!
                    sum.Throw(
                      Errors.Singleton loc0 (fun () -> "Expected a tuple with 2 elements when updating DB entity")
                    )
                    |> reader.OfSum
            } }

    memoryDBUpdateId, UpdateOperation

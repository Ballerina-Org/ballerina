namespace Ballerina.DSL.Next.StdLib.DB.Extension

[<AutoOpen>]
module Delete =
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

  let onDeletingHook<'runtimeContext, 'db, 'ext when 'ext: comparison>
    (_db_ops: DBTypeClass<'runtimeContext, 'db, 'ext>)
    (entity_ref: EntityRef<'db, 'ext>)
    (loc0: Location)
    (_entityId: Value<TypeValue<'ext>, 'ext>)
    (currentValueWithProps: Value<TypeValue<'ext>, 'ext>)

    : Reader<Unit, ExprEvalContext<'runtimeContext, 'ext>, Errors<Location>> =
    reader {
      let _schema, _db, _entity, _schema_as_value = entity_ref
      let _schema_as_value = _schema_as_value.Value.Value

      match _entity.Hooks.OnDeleting with
      | Some hookExpr ->
        let _doRunHookExpr =
          TypeCheckedExpr.UnsafeApplyForUntypedEval(
            TypeCheckedExpr.UnsafeApplyForUntypedEval(
              TypeCheckedExpr.UnsafeApplyForUntypedEval(
                hookExpr,
                TypeCheckedExpr.FromValue(
                  _schema_as_value,
                  TypeValue.CreateUnit(),
                  Kind.Star
                )
              ),
              TypeCheckedExpr.FromValue(
                _entityId,
                TypeValue.CreateUnit(),
                Kind.Star
              )
            ),
            TypeCheckedExpr.FromValue(
              currentValueWithProps,
              TypeValue.CreateUnit(),
              Kind.Star
            )
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
            sum.Throw(
              Errors.Singleton loc0 (fun () ->
                $"On deleting hook returned error {result_value}")
            )
            |> reader.OfSum
        | _ ->
          return!
            sum.Throw(
              Errors.Singleton loc0 (fun () ->
                $"On deleting hook returned unexpected value {result_value}")
            )
            |> reader.OfSum
      | None -> return ()
    }

  let onDeletedHook<'runtimeContext, 'db, 'ext when 'ext: comparison>
    (_db_ops: DBTypeClass<'runtimeContext, 'db, 'ext>)
    (entity_ref: EntityRef<'db, 'ext>)
    (loc0: Location)
    (_entityId: Value<TypeValue<'ext>, 'ext>)
    (currentValueWithProps: Value<TypeValue<'ext>, 'ext>)

    : Reader<Unit, ExprEvalContext<'runtimeContext, 'ext>, Errors<Location>> =
    reader {
      let _schema, _db, _entity, _schema_as_value = entity_ref
      let _schema_as_value = _schema_as_value.Value.Value

      match _entity.Hooks.OnDeleted with
      | Some hookExpr ->
        let _doRunHookExpr =
          TypeCheckedExpr.UnsafeApplyForUntypedEval(
            TypeCheckedExpr.UnsafeApplyForUntypedEval(
              TypeCheckedExpr.UnsafeApplyForUntypedEval(
                hookExpr,
                TypeCheckedExpr.FromValue(
                  _schema_as_value,
                  TypeValue.CreateUnit(),
                  Kind.Star
                )
              ),
              TypeCheckedExpr.FromValue(
                _entityId,
                TypeValue.CreateUnit(),
                Kind.Star
              )
            ),
            TypeCheckedExpr.FromValue(
              currentValueWithProps,
              TypeValue.CreateUnit(),
              Kind.Star
            )
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
            sum.Throw(
              Errors.Singleton loc0 (fun () ->
                $"On deleted hook returned error {result_value}")
            )
            |> reader.OfSum
        | _ ->
          return!
            sum.Throw(
              Errors.Singleton loc0 (fun () ->
                $"On deleted hook returned unexpected value {result_value}")
            )
            |> reader.OfSum
      | None -> return ()
    }

  let DBDeleteExtension<'runtimeContext, 'db, 'ext when 'ext: comparison>
    (db_ops: DBTypeClass<'runtimeContext, 'db, 'ext>)
    (_listSet: List<Value<TypeValue<'ext>, 'ext>> -> 'ext)
    (valueLens: PartialLens<'ext, DBValues<'runtimeContext, 'db, 'ext>>)
    =
    let memoryDBDeleteId =
      Identifier.FullyQualified([ "DB" ], "delete")
      |> TypeCheckScope.Empty.Resolve

    let memoryDBDeleteType: TypeValue<'ext> =
      TypeValue.CreateLambda(
        TypeParameter.Create("schema", Kind.Schema),
        TypeExpr.Lambda(
          TypeParameter.Create("entity", Kind.Star),
          TypeExpr.Lambda(
            TypeParameter.Create("entity_with_props", Kind.Star),
            TypeExpr.Lambda(
              TypeParameter.Create("entityId", Kind.Star),
              TypeExpr.Arrow(
                createSchemaEntityTypeApplication
                  "schema"
                  "entity"
                  "entity_with_props"
                  "entityId",
                TypeExpr.Arrow(
                  TypeExpr.Lookup("entityId" |> Identifier.LocalScope),
                  TypeExpr.Primitive PrimitiveType.Bool
                )
              )
            )
          )
        )
      )

    let memoryDBDeleteKind = standardSchemaOperationKind

    let DeleteOperation
      : OperationExtension<
          'runtimeContext,
          'ext,
          DBValues<'runtimeContext, 'db, 'ext>
         > =
      { PublicIdentifiers =
          Some
          <| (memoryDBDeleteType,
              memoryDBDeleteKind,
              DBValues.Delete {| EntityRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | DBValues.Delete v -> Some(DBValues.Delete v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DBValues.AsDelete
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                let! v = extractEntityRefFromValue loc0 v valueLens

                return
                  (DBValues.Delete({| EntityRef = Some v |}) |> valueLens.Set,
                   Some memoryDBDeleteId)
                  |> Ext
              | Some(entity_ref) -> // the closure has the first operand - second step in the application
                let entityId = v

                let! existingValue =
                  db_ops.GetById entity_ref entityId
                  |> reader.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.Catch
                  |> reader.Map Sum.toOption

                match existingValue with
                | None -> return Value.Primitive(PrimitiveValue.Bool false)
                | Some currentValueWithProps ->
                  let actual_delete =
                    reader {
                      do!
                        onDeletingHook
                          db_ops
                          entity_ref
                          loc0
                          entityId
                          currentValueWithProps

                      let! result =
                        db_ops.Delete entity_ref entityId
                        |> reader.MapError(Errors.MapContext(replaceWith loc0))
                        |> reader.Catch

                      do!
                        onDeletedHook
                          db_ops
                          entity_ref
                          loc0
                          entityId
                          currentValueWithProps

                      return Value.Primitive(PrimitiveValue.Bool(result.IsLeft))
                    }
                    |> reader.MapContext(
                      ExprEvalContext.Updaters.RootLevelEval(replaceWith false)
                    )

                  return!
                    reader {
                      let _, _, entity, schema_value = entity_ref
                      let! ctx = reader.GetContext()

                      match ctx.RootLevelEval, entity.Hooks.CanDelete with
                      | false, _ -> return! actual_delete
                      | _, None -> return! actual_delete
                      | _, Some canDeleteHook ->
                        match!
                          TypeCheckedExpr.UnsafeApplyForUntypedEval(
                            TypeCheckedExpr.UnsafeApplyForUntypedEval(
                              TypeCheckedExpr.UnsafeApplyForUntypedEval(
                                canDeleteHook,
                                TypeCheckedExpr.FromValue(
                                  schema_value.Value.Value,
                                  TypeValue.CreateUnit(),
                                  Kind.Star
                                )
                              ),
                              TypeCheckedExpr.FromValue(
                                entityId,
                                TypeValue.CreateUnit(),
                                Kind.Star
                              )
                            ),
                            TypeCheckedExpr.FromValue(
                              currentValueWithProps,
                              TypeValue.CreateUnit(),
                              Kind.Star
                            )
                          )
                          |> NonEmptyList.One
                          |> Expr.Eval
                        with
                        | Value.Primitive(PrimitiveValue.Bool canDelete) when
                          canDelete
                          ->
                          return! actual_delete
                        | _ ->
                          return Value.Primitive(PrimitiveValue.Bool(false))
                    }

            } }

    memoryDBDeleteId, DeleteOperation

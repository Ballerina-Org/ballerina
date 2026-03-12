namespace Ballerina.DSL.Next.StdLib.DB.Extension

[<AutoOpen>]
module Create =
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


  let onCreatingHook<'runtimeContext, 'db, 'ext when 'ext: comparison>
    (db_ops: DBTypeClass<'runtimeContext, 'db, 'ext>)
    (calculateProps:
      DBTypeClass<'runtimeContext, 'db, 'ext>
        -> Value<TypeValue<'ext>, 'ext>
        -> EntityRef<'db, 'ext>
        -> Reader<Value<TypeValue<'ext>, 'ext>, ExprEvalContext<'runtimeContext, 'ext>, Errors<Location>>)
    (entity_ref: EntityRef<'db, 'ext>)
    (loc0: Location)
    (_entityId: Value<TypeValue<'ext>, 'ext>)
    (v: Value<TypeValue<'ext>, 'ext>)
    (valueWithProps: Value<TypeValue<'ext>, 'ext>)

    : Reader<Value<TypeValue<'ext>, 'ext>, ExprEvalContext<'runtimeContext, 'ext>, Errors<Location>> =
    reader {
      let _schema, _db, _entity, _schema_as_value = entity_ref
      let _schema_as_value = _schema_as_value.Value.Value

      match _entity.Hooks.OnCreating with
      | Some hookExpr ->
        let _doRunHookExpr =
          Expr.Apply(
            Expr.Apply(
              Expr.Apply(
                Expr.Apply(hookExpr, Expr.FromValue(_schema_as_value, TypeValue.CreateUnit(), Kind.Star)),
                Expr.FromValue(_entityId, TypeValue.CreateUnit(), Kind.Star)
              ),
              Expr.FromValue(v, TypeValue.CreateUnit(), Kind.Star)
            ),
            Expr.FromValue(valueWithProps, TypeValue.CreateUnit(), Kind.Star)
          )

        let! run_hook_result = _doRunHookExpr |> NonEmptyList.One |> Expr.Eval

        let! result_case, result_value =
          run_hook_result
          |> Value.AsSum
          |> sum.MapError(Errors.MapContext(replaceWith loc0))
          |> reader.OfSum

        match result_case.Case with
        | 1 -> return valueWithProps
        | 2 ->
          return!
            sum.Throw(Errors.Singleton loc0 (fun () -> $"On creating hook returned error {result_value}"))
            |> reader.OfSum
        | 3 ->
          let modified_value_to_insert = result_value
          return! calculateProps db_ops modified_value_to_insert entity_ref
        | _ ->
          return!
            sum.Throw(Errors.Singleton loc0 (fun () -> $"On creating hook returned unexpected value {result_value}"))
            |> reader.OfSum
      | None -> return valueWithProps
    }

  let onCreatedHook<'runtimeContext, 'db, 'ext when 'ext: comparison>
    (_db_ops: DBTypeClass<'runtimeContext, 'db, 'ext>)
    (_calculateProps:
      DBTypeClass<'runtimeContext, 'db, 'ext>
        -> Value<TypeValue<'ext>, 'ext>
        -> EntityRef<'db, 'ext>
        -> Reader<Value<TypeValue<'ext>, 'ext>, ExprEvalContext<'runtimeContext, 'ext>, Errors<Location>>)
    (entity_ref: EntityRef<'db, 'ext>)
    (loc0: Location)
    (_entityId: Value<TypeValue<'ext>, 'ext>)
    (_v: Value<TypeValue<'ext>, 'ext>)
    (valueWithProps: Value<TypeValue<'ext>, 'ext>)
    : Reader<Unit, ExprEvalContext<'runtimeContext, 'ext>, Errors<Location>> =
    reader {
      let _schema, _db, _entity, _schema_as_value = entity_ref
      let _schema_as_value = _schema_as_value.Value.Value

      match _entity.Hooks.OnCreated with
      | Some hookExpr ->
        let _doRunHookExpr =
          Expr.Apply(
            Expr.Apply(
              Expr.Apply(hookExpr, Expr.FromValue(_schema_as_value, TypeValue.CreateUnit(), Kind.Star)),
              Expr.FromValue(_entityId, TypeValue.CreateUnit(), Kind.Star)
            ),
            Expr.FromValue(valueWithProps, TypeValue.CreateUnit(), Kind.Star)
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
            sum.Throw(Errors.Singleton loc0 (fun () -> $"On creating hook returned error {result_value}"))
            |> reader.OfSum
        | _ ->
          return!
            sum.Throw(Errors.Singleton loc0 (fun () -> $"On creating hook returned unexpected value {result_value}"))
            |> reader.OfSum
      | None -> return ()
    }


  let DBCreateExtension<'runtimeContext, 'db, 'ext when 'ext: comparison>
    (db_ops: DBTypeClass<'runtimeContext, 'db, 'ext>)
    (calculateProps:
      DBTypeClass<'runtimeContext, 'db, 'ext>
        -> Value<TypeValue<'ext>, 'ext>
        -> EntityRef<'db, 'ext>
        -> Reader<Value<TypeValue<'ext>, 'ext>, ExprEvalContext<'runtimeContext, 'ext>, Errors<Location>>)
    (_listSet: List<Value<TypeValue<'ext>, 'ext>> -> 'ext)
    (valueLens: PartialLens<'ext, DBValues<'runtimeContext, 'db, 'ext>>)
    =
    let memoryDBCreateId =
      Identifier.FullyQualified([ "DB" ], "create") |> TypeCheckScope.Empty.Resolve

    let memoryDBCreateType =
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
                      TypeExpr.Lookup("entity" |> Identifier.LocalScope) ],
                  TypeExpr.Sum
                    [ TypeExpr.Primitive PrimitiveType.Unit
                      TypeExpr.Lookup("entity_with_props" |> Identifier.LocalScope) ]
                )
              )
            )
          )
        )
      )

    let memoryDBCreateKind = standardSchemaOperationKind

    let CreateOperation: OperationExtension<'runtimeContext, _, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBCreateType, memoryDBCreateKind, DBValues.Create {| EntityRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | DBValues.Create v -> Some(DBValues.Create v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DBValues.AsCreate
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                let! v = extractEntityRefFromValue loc0 v valueLens

                return
                  (DBValues.Create({| EntityRef = Some v |}) |> valueLens.Set, Some memoryDBCreateId)
                  |> Ext
              | Some(entity_ref) -> // the closure has the first operand - second step in the application
                let! v =
                  v
                  |> Value.AsTuple
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                match v with
                | [ _entityId; v ] ->
                  let actual_creation =
                    reader {
                      let! valueWithProps = calculateProps db_ops v entity_ref

                      let! valueWithProps =
                        onCreatingHook db_ops calculateProps entity_ref loc0 _entityId v valueWithProps

                      let! _ =
                        db_ops.Create
                          entity_ref
                          { Id = _entityId
                            Value = valueWithProps }
                        |> reader.MapError(Errors.MapContext(replaceWith loc0))

                      do! onCreatedHook db_ops calculateProps entity_ref loc0 _entityId v valueWithProps

                      return Value.Sum({ Case = 2; Count = 2 }, valueWithProps)
                    }

                  return!
                    reader {
                      let _, _, entity, schema_value = entity_ref

                      match entity.Hooks.CanCreate with
                      | Some canCreateHook ->
                        match!
                          Expr.Apply(
                            canCreateHook,
                            Expr.FromValue(schema_value.Value.Value, TypeValue.CreateUnit(), Kind.Star)
                          )
                          |> NonEmptyList.One
                          |> Expr.Eval
                        with
                        | Value.Primitive(PrimitiveValue.Bool canCreate) when canCreate -> return! actual_creation
                        | _ -> return Value.Sum({ Case = 1; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
                      | None -> return! actual_creation
                    }
                | _ ->
                  return!
                    sum.Throw(
                      Errors.Singleton loc0 (fun () -> "Expected a tuple with 2 elements when creating DB entity")
                    )
                    |> reader.OfSum
            } }

    memoryDBCreateId, CreateOperation

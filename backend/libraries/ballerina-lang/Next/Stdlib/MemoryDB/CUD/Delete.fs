namespace Ballerina.DSL.Next.StdLib.MemoryDB.Extension

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
  open Ballerina.DSL.Next.StdLib.MemoryDB

  let onDeletingHook<'ext when 'ext: comparison>
    (_entity: SchemaEntity<'ext>)
    (_schema_as_value: Value<TypeValue<'ext>, 'ext>)
    (loc0: Location)
    (_entityId: Value<TypeValue<'ext>, 'ext>)
    (currentValueWithProps: Value<TypeValue<'ext>, 'ext>)

    : Reader<Unit, ExprEvalContext<'ext>, Errors<Location>> =
    reader {
      match _entity.OnDeleting with
      | Some hookExpr ->
        let _doRunHookExpr =
          Expr.Apply(
            Expr.Apply(
              Expr.Apply(hookExpr, Expr.FromValue(_schema_as_value, TypeValue.CreateUnit(), Kind.Star)),
              Expr.FromValue(_entityId, TypeValue.CreateUnit(), Kind.Star)
            ),
            Expr.FromValue(currentValueWithProps, TypeValue.CreateUnit(), Kind.Star)
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
            sum.Throw(Errors.Singleton loc0 (fun () -> $"On deleting hook returned error {result_value}"))
            |> reader.OfSum
        | _ ->
          return!
            sum.Throw(Errors.Singleton loc0 (fun () -> $"On deleting hook returned unexpected value {result_value}"))
            |> reader.OfSum
      | None -> return ()
    }

  let onDeletedHook<'ext when 'ext: comparison>
    (_entity: SchemaEntity<'ext>)
    (_schema_as_value: Value<TypeValue<'ext>, 'ext>)
    (loc0: Location)
    (_entityId: Value<TypeValue<'ext>, 'ext>)
    (currentValueWithProps: Value<TypeValue<'ext>, 'ext>)

    : Reader<Unit, ExprEvalContext<'ext>, Errors<Location>> =
    reader {
      match _entity.OnDeleted with
      | Some hookExpr ->
        let _doRunHookExpr =
          Expr.Apply(
            Expr.Apply(
              Expr.Apply(hookExpr, Expr.FromValue(_schema_as_value, TypeValue.CreateUnit(), Kind.Star)),
              Expr.FromValue(_entityId, TypeValue.CreateUnit(), Kind.Star)
            ),
            Expr.FromValue(currentValueWithProps, TypeValue.CreateUnit(), Kind.Star)
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
            sum.Throw(Errors.Singleton loc0 (fun () -> $"On deleted hook returned error {result_value}"))
            |> reader.OfSum
        | _ ->
          return!
            sum.Throw(Errors.Singleton loc0 (fun () -> $"On deleted hook returned unexpected value {result_value}"))
            |> reader.OfSum
      | None -> return ()
    }

  let MemoryDBDeleteExtension<'ext when 'ext: comparison>
    (_listSet: List<Value<TypeValue<'ext>, 'ext>> -> 'ext)
    (valueLens: PartialLens<'ext, MemoryDBValues<'ext>>)
    =
    let memoryDBDeleteId =
      Identifier.FullyQualified([ "MemoryDB" ], "delete")
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
                createSchemaEntityTypeApplication "schema" "entity" "entity_with_props" "entityId",
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

    let DeleteOperation: OperationExtension<'ext, MemoryDBValues<'ext>> =
      { PublicIdentifiers =
          Some
          <| (memoryDBDeleteType, memoryDBDeleteKind, MemoryDBValues.Delete {| EntityRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | MemoryDBValues.Delete v -> Some(MemoryDBValues.Delete v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> MemoryDBValues.AsDelete
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                let! v = extractEntityRefFromValue loc0 v valueLens

                return
                  (MemoryDBValues.Delete({| EntityRef = Some v |}) |> valueLens.Set, Some memoryDBDeleteId)
                  |> Ext
              | Some(_schema, _db, _entity, _schema_as_value) -> // the closure has the first operand - second step in the application
                let entityId = v

                let existingValue = lookupEntityValue _db _entity entityId

                match existingValue with
                | None -> return Value.Primitive(PrimitiveValue.Bool false)
                | Some currentValueWithProps ->

                  do! onDeletingHook _entity _schema_as_value.Value.Value loc0 entityId currentValueWithProps

                  do removeEntityValue _db _entity entityId

                  do! onDeletedHook _entity _schema_as_value.Value.Value loc0 entityId currentValueWithProps

                  return Value.Primitive(PrimitiveValue.Bool true)
            } }

    memoryDBDeleteId, DeleteOperation

namespace Ballerina.DSL.Next.StdLib.DB.Extension

[<AutoOpen>]
module Upsert =
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


  let DBUpsertExtension<'runtimeContext, 'db, 'ext when 'ext: comparison>
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
    let memoryDBUpsertId =
      Identifier.FullyQualified([ "DB" ], "upsert") |> TypeCheckScope.Empty.Resolve

    let memoryDBUpsertType =
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
                      TypeExpr.Lookup("entity" |> Identifier.LocalScope)
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

    let memoryDBUpsertKind = standardSchemaOperationKind

    let UpsertOperation: OperationExtension<'runtimeContext, _, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBUpsertType, memoryDBUpsertKind, DBValues.Upsert {| EntityRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | DBValues.Upsert v -> Some(DBValues.Upsert v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DBValues.AsUpsert
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                let! v = extractEntityRefFromValue loc0 v valueLens

                return
                  (DBValues.Upsert({| EntityRef = Some v |}) |> valueLens.Set, Some memoryDBUpsertId)
                  |> Ext
              | Some entity_ref -> // the closure has the first operand - second step in the application

                let! v =
                  v
                  |> Value.AsTuple
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                match v with
                | [ _entityId; valueToInsert: Value<TypeValue<'ext>, 'ext>; updateFunc ] ->
                  let! ctx = reader.GetContext()

                  let existingValue =
                    db_ops.GetById entity_ref _entityId
                    |> Reader.Run ctx.RuntimeContext
                    |> Sum.toOption

                  match existingValue with
                  | None ->
                    let! valueWithProps = calculateProps db_ops valueToInsert entity_ref

                    let! valueWithProps =
                      onCreatingHook db_ops calculateProps entity_ref loc0 _entityId valueToInsert valueWithProps

                    let! ctx = reader.GetContext()

                    let! _ =
                      db_ops.Upsert
                        entity_ref
                        { Id = _entityId
                          Value = valueWithProps }
                      |> Reader.Run ctx.RuntimeContext
                      |> sum.MapError(Errors.MapContext(replaceWith loc0))
                      |> reader.OfSum

                    do! onCreatedHook db_ops calculateProps entity_ref loc0 _entityId valueToInsert valueWithProps

                    return Value.Sum({ Case = 2; Count = 2 }, valueWithProps)
                  | Some existingValue ->
                    let! existingValueWithoutProps = stripProps db_ops existingValue entity_ref

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
                      db_ops.Upsert
                        entity_ref
                        { Id = _entityId
                          Value = valueWithProps }
                      |> Reader.Run ctx.RuntimeContext
                      |> sum.MapError(Errors.MapContext(replaceWith loc0))
                      |> reader.OfSum

                    let! _ = onUpdatedHook db_ops entity_ref loc0 _entityId existingValue valueWithProps

                    return Value.Sum({ Case = 2; Count = 2 }, valueWithProps)
                | _ ->
                  return!
                    sum.Throw(
                      Errors.Singleton loc0 (fun () -> "Expected a tuple with 3 elements when upserting into DB")
                    )
                    |> reader.OfSum
            } }

    memoryDBUpsertId, UpsertOperation

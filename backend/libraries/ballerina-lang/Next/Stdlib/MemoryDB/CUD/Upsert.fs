namespace Ballerina.DSL.Next.StdLib.MemoryDB.Extension

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
  open Ballerina.DSL.Next.StdLib.MemoryDB


  let MemoryDBUpsertExtension<'ext when 'ext: comparison>
    (
      calculateProps:
        Value<TypeValue<'ext>, 'ext>
          -> SchemaEntity<'ext>
          -> Reader<Value<TypeValue<'ext>, 'ext>, ExprEvalContext<'ext>, Errors<Location>>,
      stripProps
    )
    (_listSet: List<Value<TypeValue<'ext>, 'ext>> -> 'ext)
    (valueLens: PartialLens<'ext, MemoryDBValues<'ext>>)
    =
    let memoryDBUpsertId =
      Identifier.FullyQualified([ "MemoryDB" ], "upsert")
      |> TypeCheckScope.Empty.Resolve

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

    let UpsertOperation: OperationExtension<_, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBUpsertType, memoryDBUpsertKind, MemoryDBValues.Upsert {| EntityRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | MemoryDBValues.Upsert v -> Some(MemoryDBValues.Upsert v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> MemoryDBValues.AsUpsert
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                let! v = extractEntityRefFromValue loc0 v valueLens

                return
                  (MemoryDBValues.Upsert({| EntityRef = Some v |}) |> valueLens.Set, Some memoryDBUpsertId)
                  |> Ext
              | Some(_schema, _db, _entity, _schema_as_value) -> // the closure has the first operand - second step in the application

                let! v =
                  v
                  |> Value.AsTuple
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                match v with
                | [ _entityId; valueToInsert: Value<TypeValue<'ext>, 'ext>; updateFunc ] ->
                  let existingValue = lookupEntityValue _db _entity _entityId

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

                    do updateEntityValue _db _entity _entityId valueWithProps

                    return Value.Sum({ Case = 2; Count = 2 }, valueWithProps)
                | _ ->
                  return!
                    sum.Throw(
                      Errors.Singleton loc0 (fun () -> "Expected a tuple with 3 elements when upserting into DB")
                    )
                    |> reader.OfSum
            } }

    memoryDBUpsertId, UpsertOperation

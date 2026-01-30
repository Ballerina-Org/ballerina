namespace Ballerina.DSL.Next.StdLib.MemoryDB.Extension

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
  open Ballerina.DSL.Next.StdLib.MemoryDB


  let MemoryDBUpdateExtension<'ext when 'ext: comparison>
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
    let memoryDBUpdateId =
      Identifier.FullyQualified([ "MemoryDB" ], "update")
      |> TypeCheckScope.Empty.Resolve

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

    let UpdateOperation: OperationExtension<_, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBUpdateType, memoryDBUpdateKind, MemoryDBValues.Update {| EntityRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | MemoryDBValues.Update v -> Some(MemoryDBValues.Update v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> MemoryDBValues.AsUpdate
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                let! v = extractEntityRefFromValue loc0 v valueLens

                return
                  (MemoryDBValues.Update({| EntityRef = Some v |}) |> valueLens.Set, Some memoryDBUpdateId)
                  |> Ext
              | Some(_schema, _db, _entity, _schema_as_value) -> // the closure has the first operand - second step in the application

                let! v =
                  v
                  |> Value.AsTuple
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                match v with
                | [ _entityId; updateFunc ] ->
                  let existingValue = lookupEntityValue _db _entity _entityId

                  match existingValue with
                  | None -> return Value.Sum({ Case = 1; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
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
                      Errors.Singleton loc0 (fun () -> "Expected a tuple with 2 elements when updating DB entity")
                    )
                    |> reader.OfSum
            } }

    memoryDBUpdateId, UpdateOperation

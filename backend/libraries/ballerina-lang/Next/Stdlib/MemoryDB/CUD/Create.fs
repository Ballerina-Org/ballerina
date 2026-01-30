namespace Ballerina.DSL.Next.StdLib.MemoryDB.Extension

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
  open Ballerina.DSL.Next.StdLib.MemoryDB


  let MemoryDBCreateExtension<'ext when 'ext: comparison>
    (calculateProps:
      Value<TypeValue<'ext>, 'ext>
        -> SchemaEntity<'ext>
        -> Reader<Value<TypeValue<'ext>, 'ext>, ExprEvalContext<'ext>, Errors<Location>>)
    (_listSet: List<Value<TypeValue<'ext>, 'ext>> -> 'ext)
    (valueLens: PartialLens<'ext, MemoryDBValues<'ext>>)
    =
    let memoryDBCreateId =
      Identifier.FullyQualified([ "MemoryDB" ], "create")
      |> TypeCheckScope.Empty.Resolve

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


    let CreateOperation: OperationExtension<_, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBCreateType, memoryDBCreateKind, MemoryDBValues.Create {| EntityRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | MemoryDBValues.Create v -> Some(MemoryDBValues.Create v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> MemoryDBValues.AsCreate
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                let! v = extractEntityRefFromValue loc0 v valueLens

                return
                  (MemoryDBValues.Create({| EntityRef = Some v |}) |> valueLens.Set, Some memoryDBCreateId)
                  |> Ext
              | Some(_schema, _db, _entity, _schema_as_value) -> // the closure has the first operand - second step in the application
                let! v =
                  v
                  |> Value.AsTuple
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                match v with
                | [ _entityId; v ] ->
                  let! valueWithProps = calculateProps v _entity

                  let! valueWithProps =
                    reader {
                      match _entity.OnCreating with
                      | Some hookExpr ->
                        let _doRunHookExpr =
                          Expr.Apply(
                            Expr.Apply(
                              Expr.Apply(
                                Expr.Apply(
                                  hookExpr,
                                  Expr.FromValue(_schema_as_value.Value.Value, TypeValue.CreateUnit(), Kind.Star)
                                ),
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
                            sum.Throw(
                              Errors.Singleton loc0 (fun () -> $"On creating hook returned error {result_value}")
                            )
                            |> reader.OfSum
                        | 3 ->
                          let modified_value_to_insert = result_value
                          return! calculateProps modified_value_to_insert _entity
                        | _ ->
                          return!
                            sum.Throw(
                              Errors.Singleton loc0 (fun () ->
                                $"On creating hook returned unexpected value {result_value}")
                            )
                            |> reader.OfSum
                      | None -> return valueWithProps
                    }

                  do addEntityValue _db _entity _entityId valueWithProps

                  return Value.Sum({ Case = 2; Count = 2 }, valueWithProps)
                | _ ->
                  return!
                    sum.Throw(
                      Errors.Singleton loc0 (fun () -> "Expected a tuple with 2 elements when creating DB entity")
                    )
                    |> reader.OfSum
            } }

    memoryDBCreateId, CreateOperation

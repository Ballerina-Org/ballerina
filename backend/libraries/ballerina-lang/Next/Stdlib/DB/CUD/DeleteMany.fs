namespace Ballerina.DSL.Next.StdLib.DB.Extension

[<AutoOpen>]
module DeleteMany =
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


  let DBDeleteManyExtension<'runtimeContext, 'db, 'ext when 'ext: comparison>
    (db_ops: DBTypeClass<'runtimeContext, 'db, 'ext>)
    (listLens: PartialLens<'ext, List<Value<TypeValue<'ext>, 'ext>>>)
    (valueLens: PartialLens<'ext, DBValues<'runtimeContext, 'db, 'ext>>)
    =

    let memoryDBDeleteManyId =
      Identifier.FullyQualified([ "DB" ], "deleteMany")
      |> TypeCheckScope.Empty.Resolve

    let memoryDBDeleteManyType =
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
                    TypeExpr.Lookup("List" |> Identifier.LocalScope),
                    TypeExpr.Lookup("entityId" |> Identifier.LocalScope)
                  ),
                  TypeExpr.Primitive PrimitiveType.Unit
                // TypeExpr.Apply(
                //   TypeExpr.Apply(
                //     TypeExpr.Lookup("Map" |> Identifier.LocalScope),
                //     TypeExpr.Lookup("entityId" |> Identifier.LocalScope)
                //   ),
                //   TypeExpr.Primitive PrimitiveType.Bool
                // )
                )
              )
            )
          )
        )
      )

    let memoryDBDeleteManyKind = standardSchemaOperationKind

    let DeleteManyOperation: OperationExtension<'runtimeContext, _, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBDeleteManyType, memoryDBDeleteManyKind, DBValues.DeleteMany {| EntityRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | DBValues.DeleteMany v -> Some(DBValues.DeleteMany v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DBValues.AsDeleteMany
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                let! v = extractEntityRefFromValue loc0 v valueLens

                return
                  (DBValues.DeleteMany({| EntityRef = Some v |}) |> valueLens.Set, Some memoryDBDeleteManyId)
                  |> Ext
              | Some(entity_ref) -> // the closure has the first operand - second step in the application

                let! vs, _ =
                  v
                  |> Value.AsExt
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                let! vs =
                  vs
                  |> listLens.Get
                  |> sum.OfOption(Errors.Singleton loc0 (fun () -> "Cannot get value from extension"))
                  |> reader.OfSum

                let! deletingValuesWithProps =
                  vs
                  |> Seq.map (fun _entityId ->
                    reader {
                      let! existingValue =
                        db_ops.GetById entity_ref _entityId
                        |> reader.MapError(Errors.MapContext(replaceWith loc0))
                        |> reader.Catch
                        |> reader.Map Sum.toOption

                      match existingValue with
                      | None -> return _entityId, None
                      | Some currentValueWithProps ->
                        let _, _, entity, schema_value = entity_ref
                        let! ctx = reader.GetContext()

                        match ctx.RootLevelEval, entity.Hooks.CanDelete with
                        | true, Some canDeleteHook ->
                          match!
                            Expr.Apply(
                              Expr.Apply(
                                Expr.Apply(
                                  canDeleteHook,
                                  Expr.FromValue(schema_value.Value.Value, TypeValue.CreateUnit(), Kind.Star)
                                ),
                                Expr.FromValue(_entityId, TypeValue.CreateUnit(), Kind.Star)
                              ),
                              Expr.FromValue(currentValueWithProps, TypeValue.CreateUnit(), Kind.Star)
                            )
                            |> NonEmptyList.One
                            |> Expr.Eval
                          with
                          | Value.Primitive(PrimitiveValue.Bool canDelete) when canDelete ->
                            do!
                              onDeletingHook db_ops entity_ref loc0 _entityId currentValueWithProps
                              |> reader.MapContext(ExprEvalContext.Updaters.RootLevelEval(replaceWith false))

                            return _entityId, Some currentValueWithProps
                          | _ -> return _entityId, None
                        | _ ->
                          do!
                            onDeletingHook db_ops entity_ref loc0 _entityId currentValueWithProps
                            |> reader.MapContext(ExprEvalContext.Updaters.RootLevelEval(replaceWith false))

                          return _entityId, Some currentValueWithProps
                    })
                  |> reader.All

                let! _ =
                  db_ops.DeleteMany entity_ref vs
                  |> reader.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.MapContext(ExprEvalContext.Updaters.RootLevelEval(replaceWith false))

                do!
                  deletingValuesWithProps
                  |> Seq.map (fun (_entityId, existingValue) ->
                    reader {
                      match existingValue with
                      | None -> return ()
                      | Some currentValueWithProps ->

                        do! onDeletedHook db_ops entity_ref loc0 _entityId currentValueWithProps

                        return ()
                    })
                  |> reader.All
                  |> reader.MapContext(ExprEvalContext.Updaters.RootLevelEval(replaceWith false))
                  |> reader.Ignore

                return Value.Primitive(PrimitiveValue.Unit)
            } }

    memoryDBDeleteManyId, DeleteManyOperation

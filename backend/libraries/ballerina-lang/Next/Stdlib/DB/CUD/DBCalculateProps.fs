namespace Ballerina.DSL.Next.StdLib.DB.Extension

[<AutoOpen>]
module CalculateProps =
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

  let DBCalculatePropertyExtension<'runtimeContext, 'db, 'ext
    when 'ext: comparison>
    (_listSet: List<Value<TypeValue<'ext>, 'ext>> -> 'ext)
    (valueLens: PartialLens<'ext, DBValues<'runtimeContext, 'db, 'ext>>)
    =
    let memoryDBCalculatePropertyId =
      Identifier.FullyQualified([ "DB" ], "@@@calculateSchemaProperty")
      |> TypeCheckScope.Empty.Resolve

    let CalculatePropertyOperation: OperationExtension<'runtimeContext, _, _> =
      { PublicIdentifiers = None
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | DBValues.EvalProperty _ as p -> Some p
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DBValues.AsEvalProperty
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op.Path with
              | (segment_binding, p) :: ps ->
                match p with
                | SchemaPathTypeDecomposition.Field fieldName ->
                  let! vFields =
                    v
                    |> Value.AsRecord
                    |> sum.MapError(Errors.MapContext(replaceWith loc0))
                    |> reader.OfSum

                  let! vField =
                    vFields
                    |> Map.tryFind fieldName
                    |> sum.OfOption(
                      Errors.Singleton loc0 (fun () ->
                        $"Field {fieldName.Name} not found in record")
                    )
                    |> reader.OfSum

                  let! valueWithProps =
                    RunnableExpr.UnsafeApplyForUntypedEval(
                      RunnableExpr.FromValue(
                        (DBValues.EvalProperty { op with Path = ps }
                         |> valueLens.Set,
                         Some memoryDBCalculatePropertyId)
                        |> Ext,
                        TypeValue.CreatePrimitive PrimitiveType.Unit,
                        Kind.Star
                      ),
                      RunnableExpr.FromValue(
                        vField,
                        TypeValue.CreatePrimitive PrimitiveType.Unit,
                        Kind.Star
                      )
                    )
                    |> fun e -> NonEmptyList.OfList(e, _rest)
                    |> Expr.Eval
                    |> reader.MapContext(
                      match segment_binding with
                      | Some id ->
                        ExprEvalContext.Updaters.Values(
                          Map.add
                            (id.Name
                             |> Identifier.LocalScope
                             |> TypeCheckScope.Empty.Resolve)
                            vField
                        )
                      | None -> id
                    )

                  let valueWithProps =
                    Value.Record(vFields |> Map.add fieldName valueWithProps)

                  return valueWithProps
                | SchemaPathTypeDecomposition.Item fieldName ->
                  let! vFields =
                    v
                    |> Value.AsTuple
                    |> sum.MapError(Errors.MapContext(replaceWith loc0))
                    |> reader.OfSum

                  let! vField =
                    vFields
                    |> Seq.tryItem (fieldName.Index - 1)
                    |> sum.OfOption(
                      Errors.Singleton loc0 (fun () ->
                        $"Item {fieldName.Index} not found in tuple")
                    )
                    |> reader.OfSum

                  let! valueWithProps =
                    RunnableExpr.UnsafeApplyForUntypedEval(
                      RunnableExpr.FromValue(
                        (DBValues.EvalProperty { op with Path = ps }
                         |> valueLens.Set,
                         Some memoryDBCalculatePropertyId)
                        |> Ext,
                        TypeValue.CreatePrimitive PrimitiveType.Unit,
                        Kind.Star
                      ),
                      RunnableExpr.FromValue(
                        vField,
                        TypeValue.CreatePrimitive PrimitiveType.Unit,
                        Kind.Star
                      )
                    )
                    |> fun e -> NonEmptyList.OfList(e, _rest)
                    |> Expr.Eval
                    |> reader.MapContext(
                      match segment_binding with
                      | Some id ->
                        ExprEvalContext.Updaters.Values(
                          Map.add
                            (id.Name
                             |> Identifier.LocalScope
                             |> TypeCheckScope.Empty.Resolve)
                            vField
                        )
                      | None -> id
                    )

                  let valueWithProps =
                    Value.Tuple(
                      vFields
                      |> Seq.mapi (fun i v ->
                        if i = fieldName.Index - 1 then valueWithProps else v)
                      |> Seq.toList
                    )

                  return valueWithProps
                | SchemaPathTypeDecomposition.UnionCase expectedCaseId ->
                  let! actualCaseId, vCaseContent =
                    v
                    |> Value.AsUnion
                    |> sum.MapError(Errors.MapContext(replaceWith loc0))
                    |> reader.OfSum

                  if actualCaseId.Name <> expectedCaseId.Name then
                    return v
                  else
                    let! vCaseContentWithProps =
                      RunnableExpr.UnsafeApplyForUntypedEval(
                        RunnableExpr.FromValue(
                          (DBValues.EvalProperty { op with Path = ps }
                           |> valueLens.Set,
                           Some memoryDBCalculatePropertyId)
                          |> Ext,
                          TypeValue.CreatePrimitive PrimitiveType.Unit,
                          Kind.Star
                        ),
                        RunnableExpr.FromValue(
                          vCaseContent,
                          TypeValue.CreatePrimitive PrimitiveType.Unit,
                          Kind.Star
                        )
                      )
                      |> fun e -> NonEmptyList.OfList(e, _rest)
                      |> Expr.Eval
                      |> reader.MapContext(
                        match segment_binding with
                        | Some id ->
                          ExprEvalContext.Updaters.Values(
                            Map.add
                              (id.Name
                               |> Identifier.LocalScope
                               |> TypeCheckScope.Empty.Resolve)
                              vCaseContent
                          )
                        | None -> id
                      )

                    let valueWithProps =
                      Value.UnionCase(actualCaseId, vCaseContentWithProps)

                    return valueWithProps
                | SchemaPathTypeDecomposition.SumCase expectedCaseId ->
                  let! actualCaseId, vCaseContent =
                    v
                    |> Value.AsSum
                    |> sum.MapError(Errors.MapContext(replaceWith loc0))
                    |> reader.OfSum

                  if actualCaseId <> expectedCaseId then
                    return v
                  else
                    let! vCaseContentWithProps =
                      RunnableExpr.UnsafeApplyForUntypedEval(
                        RunnableExpr.FromValue(
                          (DBValues.EvalProperty { op with Path = ps }
                           |> valueLens.Set,
                           Some memoryDBCalculatePropertyId)
                          |> Ext,
                          TypeValue.CreatePrimitive PrimitiveType.Unit,
                          Kind.Star
                        ),
                        RunnableExpr.FromValue(
                          vCaseContent,
                          TypeValue.CreatePrimitive PrimitiveType.Unit,
                          Kind.Star
                        )
                      )
                      |> fun e -> NonEmptyList.OfList(e, _rest)
                      |> Expr.Eval
                      |> reader.MapContext(
                        match segment_binding with
                        | Some id ->
                          ExprEvalContext.Updaters.Values(
                            Map.add
                              (id.Name
                               |> Identifier.LocalScope
                               |> TypeCheckScope.Empty.Resolve)
                              vCaseContent
                          )
                        | None -> id
                      )

                    let valueWithProps =
                      Value.Sum(actualCaseId, vCaseContentWithProps)

                    return valueWithProps
                | SchemaPathTypeDecomposition.Iterator iterator ->
                  // iterator.Mapper(fun item -> evalProperty(ps, item))(v)
                  // replace "item" with the binding name if present
                  let lambda_var_name =
                    match segment_binding with
                    | Some id -> id.Name
                    | None -> "item"

                  let! res =
                    RunnableExpr.UnsafeApplyForUntypedEval(
                      RunnableExpr.UnsafeApplyForUntypedEval(
                        iterator.Mapper,
                        RunnableExpr.UnsafeLambdaForUntypedEval(
                          Var.Create lambda_var_name,
                          TypeValue.CreatePrimitive PrimitiveType.Unit,
                          RunnableExpr.UnsafeApplyForUntypedEval(
                            RunnableExpr.FromValue(
                              (DBValues.EvalProperty { op with Path = ps }
                               |> valueLens.Set,
                               Some memoryDBCalculatePropertyId)
                              |> Ext,
                              TypeValue.CreatePrimitive PrimitiveType.Unit,
                              Kind.Star
                            ),
                            RunnableExpr.UnsafeLookupForUntypedEval(
                              lambda_var_name
                              |> Identifier.LocalScope
                              |> TypeCheckScope.Empty.Resolve
                            )
                          ),
                          TypeValue.CreatePrimitive PrimitiveType.Unit
                        )
                      ),
                      RunnableExpr.FromValue(
                        v,
                        TypeValue.CreatePrimitive PrimitiveType.Unit,
                        Kind.Star
                      )
                    )
                    |> fun e -> NonEmptyList.OfList(e, _rest)
                    |> Expr.Eval

                  return res
              | [] ->
                let! propertyValue =
                  op.Body |> fun e -> NonEmptyList.OfList(e, _rest) |> Expr.Eval

                let! vFields =
                  v
                  |> Value.AsRecord
                  |> sum.MapError(Errors.MapContext(replaceWith loc0))
                  |> reader.OfSum

                return
                  Value.Record(
                    vFields
                    |> Map.add
                      (op.PropertyName.Name |> ResolvedIdentifier.Create)
                      propertyValue
                  )
            } }

    let calculateProps
      (_db_ops: DBTypeClass<'runtimeContext, 'db, 'ext>)
      (v: Value<TypeValue<'ext>, 'ext>)
      (_entity: EntityRef<'db, 'ext>)
      : Reader<
          Value<TypeValue<'ext>, 'ext>,
          ExprEvalContext<'runtimeContext, 'ext>,
          Errors<Location>
         >
      =

      let _, _, _entity_desc, _schema_as_value = _entity

      List.fold
        (fun acc (prop: SchemaEntityProperty<'ext>) ->
          reader {
            let! valueSoFar = acc

            return!
              RunnableExpr.UnsafeApplyForUntypedEval(
                RunnableExpr.FromValue(
                  (DBValues.EvalProperty
                    { PropertyName = prop.PropertyName
                      Path = prop.Path
                      Body = prop.Body }
                   |> valueLens.Set,
                   Some memoryDBCalculatePropertyId)
                  |> Ext,
                  TypeValue.CreatePrimitive PrimitiveType.Unit,
                  Kind.Star
                ),
                RunnableExpr.FromValue(
                  valueSoFar,
                  TypeValue.CreatePrimitive PrimitiveType.Unit,
                  Kind.Star
                )
              )
              |> NonEmptyList.One
              |> Expr.Eval
              |> reader.MapContext(
                ExprEvalContext.Updaters.Values(
                  Map.add
                    ("self"
                     |> Identifier.LocalScope
                     |> TypeCheckScope.Empty.Resolve)
                    v
                  >> Map.add
                    ("schema"
                     |> Identifier.LocalScope
                     |> TypeCheckScope.Empty.Resolve)
                    _schema_as_value.Value.Value
                )
              )
          })
        (reader { return v })
        _entity_desc.Properties

    memoryDBCalculatePropertyId, CalculatePropertyOperation, calculateProps


  let DBCalculatePropsPublicExtension<'runtimeContext, 'db, 'ext
    when 'ext: comparison>
    (db_ops: DBTypeClass<'runtimeContext, 'db, 'ext>)
    (calculateProps:
      DBTypeClass<'runtimeContext, 'db, 'ext>
        -> Value<TypeValue<'ext>, 'ext>
        -> EntityRef<'db, 'ext>
        -> Reader<
          Value<TypeValue<'ext>, 'ext>,
          ExprEvalContext<'runtimeContext, 'ext>,
          Errors<Location>
         >)
    (valueLens: PartialLens<'ext, DBValues<'runtimeContext, 'db, 'ext>>)
    =
    let memoryDBCalculatePropsId =
      Identifier.FullyQualified([ "DB" ], "calculateProps")
      |> TypeCheckScope.Empty.Resolve

    let memoryDBCalculatePropsType =
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
                  TypeExpr.Lookup("entity" |> Identifier.LocalScope),
                  TypeExpr.Lookup(
                    "entity_with_props" |> Identifier.LocalScope
                  )
                )
              )
            )
          )
        )
      )

    let memoryDBCalculatePropsKind = standardSchemaOperationKind

    let calculatePropsOperation: OperationExtension<'runtimeContext, _, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBCalculatePropsType,
              memoryDBCalculatePropsKind,
              DBValues.CalculateProps {| EntityRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | DBValues.CalculateProps v -> Some(DBValues.CalculateProps v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> DBValues.AsCalculateProps
                |> sum.MapError(Errors.MapContext(replaceWith loc0))
                |> reader.OfSum

              match op with
              | None ->
                let! v = extractEntityRefFromValue loc0 v valueLens

                return
                  (DBValues.CalculateProps({| EntityRef = Some v |})
                   |> valueLens.Set,
                   Some memoryDBCalculatePropsId)
                  |> Ext
              | Some entity_ref ->
                return! calculateProps db_ops v entity_ref
            } }

    memoryDBCalculatePropsId, calculatePropsOperation

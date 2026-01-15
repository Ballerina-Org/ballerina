namespace Ballerina.DSL.Next.StdLib.MemoryDB

[<AutoOpen>]
module Extension =
  open Ballerina.StdLib.String
  open Ballerina.Collections.Option
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
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

  let MemoryDBRunExtension<'ext when 'ext: comparison>
    (valueLens: PartialLens<'ext, MemoryDBValues<'ext>>)
    : TypeLambdaExtension<'ext, MemoryDBValues<'ext>> =

    let memoryDBRunId =
      Identifier.FullyQualified([ "MemoryDB" ], "run") |> TypeCheckScope.Empty.Resolve

    let memoryDBRunType =
      TypeValue.CreateLambda(
        TypeParameter.Create("schema", Kind.Schema),
        TypeExpr.Lambda(
          TypeParameter.Create("result", Kind.Star),
          TypeExpr.Arrow(
            TypeExpr.Arrow(
              TypeExpr.Lookup("schema" |> Identifier.LocalScope),
              TypeExpr.Lookup("result" |> Identifier.LocalScope)
            ),
            TypeExpr.Lookup("result" |> Identifier.LocalScope)
          )
        )
      )

    let memoryDBRunKind = Kind.Arrow(Kind.Schema, Kind.Arrow(Kind.Star, Kind.Star))

    let typeApply (typeValue: TypeValue<'ext>) : ExprEvaluator<'ext, Value<TypeValue<'ext>, 'ext>> =
      reader {
        let! schema =
          typeValue
          |> TypeValue.AsSchema
          |> Sum.mapRight (Errors.FromErrors Location.Unknown)
          |> reader.OfSum

        return
          MemoryDBValues.TypeAppliedRun(schema, { entities = Map.empty })
          |> valueLens.Set
          |> Ext
      }

    let evalToTypeApplicable
      (loc0: Location)
      (_rest: List<Expr<TypeValue<'ext>, ResolvedIdentifier, 'ext>>)
      (v: 'ext)
      : ExprEvaluator<'ext, ExtEvalResult<'ext>> =
      reader {
        let! v =
          valueLens.Get v
          |> sum.OfOption((loc0, $"Error: cannot get value from extension") |> Errors.Singleton)
          |> reader.OfSum

        do!
          v
          |> MemoryDBValues.AsRun
          |> sum.MapError(Errors.FromErrors loc0)
          |> reader.OfSum

        return TypeApplicable(fun arg -> typeApply arg)
      }

    let apply
      (_loc0: Location)
      (schema: Schema<'ext>)
      (db: MutableMemoryDB<'ext>)
      (value: Value<TypeValue<'ext>, 'ext>)
      (_rest: List<Expr<TypeValue<'ext>, ResolvedIdentifier, 'ext>>)
      : ExprEvaluator<'ext, Value<TypeValue<'ext>, 'ext>> =
      reader {

        let arg =
          [ "Entities" |> ResolvedIdentifier.Create,
            Value.Record(
              schema.Entities
              |> OrderedMap.toSeq
              |> Seq.map (fun (k, v) ->
                k.Name |> ResolvedIdentifier.Create, MemoryDBValues.EntityRef(schema, db, v) |> valueLens.Set |> Ext)
              |> Map.ofSeq
            ) ]
          |> Map.ofList
          |> Value.Record

        return!
          Expr.Apply(
            Expr.FromValue(value, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star),
            Expr.FromValue(arg, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
          )
          |> fun e -> Expr.Eval(NonEmptyList.OfList(e, _rest))
      }

    let evalToApplicable
      (loc0: Location)
      (_rest: List<Expr<TypeValue<'ext>, ResolvedIdentifier, 'ext>>)
      (v: 'ext)
      : ExprEvaluator<'ext, ExtEvalResult<'ext>> =
      reader {
        let! v =
          valueLens.Get v
          |> sum.OfOption((loc0, $"Error: cannot get value from extension") |> Errors.Singleton)
          |> reader.OfSum

        let! schema, db =
          v
          |> MemoryDBValues.AsTypeAppliedRun
          |> sum.MapError(Errors.FromErrors loc0)
          |> reader.OfSum

        return Applicable(fun arg -> apply loc0 schema db arg _rest)
      }

    { ExtensionType = memoryDBRunId, memoryDBRunType, memoryDBRunKind
      ReferencedTypes = []
      Value = MemoryDBValues.Run
      ValueLens = valueLens
      EvalToTypeApplicable = evalToTypeApplicable
      EvalToApplicable = evalToApplicable }

  let MemoryDBGetByIdExtension<'ext when 'ext: comparison>
    (valueLens: PartialLens<'ext, MemoryDBValues<'ext>>)
    : OperationsExtension<'ext, MemoryDBValues<'ext>> =

    let memoryDBGetById =
      Identifier.FullyQualified([ "MemoryDB" ], "getById")
      |> TypeCheckScope.Empty.Resolve

    let memoryDBGetByIdType =
      TypeValue.CreateLambda(
        TypeParameter.Create("schema", Kind.Schema),
        TypeExpr.Lambda(
          TypeParameter.Create("entity", Kind.Star),
          TypeExpr.Lambda(
            TypeParameter.Create("entity_with_props", Kind.Star),
            TypeExpr.Lambda(
              TypeParameter.Create("entityId", Kind.Star),
              TypeExpr.Arrow(
                TypeExpr.Apply(
                  TypeExpr.Apply(
                    TypeExpr.Apply(
                      TypeExpr.Apply(
                        TypeExpr.Lookup("SchemaEntity" |> Identifier.LocalScope),
                        TypeExpr.Lookup("schema" |> Identifier.LocalScope)

                      ),
                      TypeExpr.Lookup("entity" |> Identifier.LocalScope)
                    ),
                    TypeExpr.Lookup("entity_with_props" |> Identifier.LocalScope)
                  ),
                  TypeExpr.Lookup("entityId" |> Identifier.LocalScope)
                ),
                TypeExpr.Arrow(
                  TypeExpr.Lookup("entityId" |> Identifier.LocalScope),
                  TypeExpr.Sum(
                    [ TypeExpr.Primitive PrimitiveType.Unit
                      TypeExpr.Lookup("entity_with_props" |> Identifier.LocalScope) ]
                  )
                )
              )
            )
          )
        )
      )

    let memoryDBGetByIdKind =
      Kind.Arrow(Kind.Schema, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))))

    let getByIdOperation: OperationExtension<_, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBGetByIdType, memoryDBGetByIdKind, MemoryDBValues.GetById {| EntityRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | MemoryDBValues.GetById v -> Some(MemoryDBValues.GetById v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> MemoryDBValues.AsGetById
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                let! v = v |> Value.AsExt |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                let! v =
                  v
                  |> valueLens.Get
                  |> sum.OfOption(Errors.Singleton(loc0, "Cannot get value from extension"))
                  |> reader.OfSum

                let! v =
                  v
                  |> MemoryDBValues.AsEntityRef
                  |> sum.MapError(Errors.FromErrors loc0)
                  |> reader.OfSum

                return MemoryDBValues.GetById({| EntityRef = Some v |}) |> valueLens.Set |> Ext
              | Some(_schema, _db, _entity) -> // the closure has the first operand - second step in the application
                let v =
                  option {
                    let! entity = _db.entities |> Map.tryFind _entity.Name
                    let! value = entity |> Map.tryFind v
                    return value
                  }

                match v with
                | None -> return Value.Sum({ Case = 1; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
                | Some value -> return Value.Sum({ Case = 2; Count = 2 }, value)
            } }

    { TypeVars = []
      Operations = [ memoryDBGetById, getByIdOperation ] |> Map.ofList }

  let MemoryDBCUDExtension<'ext when 'ext: comparison>
    (valueLens: PartialLens<'ext, MemoryDBValues<'ext>>)
    : OperationsExtension<'ext, MemoryDBValues<'ext>> =

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
                TypeExpr.Apply(
                  TypeExpr.Apply(
                    TypeExpr.Apply(
                      TypeExpr.Apply(
                        TypeExpr.Lookup("SchemaEntity" |> Identifier.LocalScope),
                        TypeExpr.Lookup("schema" |> Identifier.LocalScope)

                      ),
                      TypeExpr.Lookup("entity" |> Identifier.LocalScope)
                    ),
                    TypeExpr.Lookup("entity_with_props" |> Identifier.LocalScope)
                  ),
                  TypeExpr.Lookup("entityId" |> Identifier.LocalScope)
                ),
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

    let memoryDBCreateKind =
      Kind.Arrow(Kind.Schema, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))))

    let memoryDBCalculatePropertyId =
      Identifier.FullyQualified([ "MemoryDB" ], "@@@calculateSchemaProperty")
      |> TypeCheckScope.Empty.Resolve

    let CalculatePropertyOperation: OperationExtension<_, _> =
      { PublicIdentifiers = None
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | MemoryDBValues.EvalProperty _ as p -> Some p
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> MemoryDBValues.AsEvalProperty
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op.Path with
              | (segment_binding, p) :: ps ->
                match p with
                | SchemaPathTypeDecomposition.Field fieldName ->
                  let! vFields = v |> Value.AsRecord |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                  let! vField =
                    vFields
                    |> Map.tryFind fieldName
                    |> sum.OfOption(Errors.Singleton(loc0, $"Field {fieldName.Name} not found in record"))
                    |> reader.OfSum

                  let! valueWithProps =
                    Expr.Apply(
                      Expr.FromValue(
                        MemoryDBValues.EvalProperty { op with Path = ps } |> valueLens.Set |> Ext,
                        TypeValue.CreatePrimitive PrimitiveType.Unit,
                        Kind.Star
                      ),
                      Expr.FromValue(vField, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                    )
                    |> fun e -> NonEmptyList.OfList(e, _rest)
                    |> Expr.Eval
                    |> reader.MapContext(
                      match segment_binding with
                      | Some id ->
                        ExprEvalContext.Updaters.Values(
                          Map.add (id.Name |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve) vField
                        )
                      | None -> id
                    )

                  let valueWithProps = Value.Record(vFields |> Map.add fieldName valueWithProps)
                  return valueWithProps
                | SchemaPathTypeDecomposition.Item fieldName ->
                  let! vFields = v |> Value.AsTuple |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                  let! vField =
                    vFields
                    |> Seq.tryItem (fieldName.Index - 1)
                    |> sum.OfOption(Errors.Singleton(loc0, $"Item {fieldName.Index} not found in tuple"))
                    |> reader.OfSum

                  let! valueWithProps =
                    Expr.Apply(
                      Expr.FromValue(
                        MemoryDBValues.EvalProperty { op with Path = ps } |> valueLens.Set |> Ext,
                        TypeValue.CreatePrimitive PrimitiveType.Unit,
                        Kind.Star
                      ),
                      Expr.FromValue(vField, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                    )
                    |> fun e -> NonEmptyList.OfList(e, _rest)
                    |> Expr.Eval
                    |> reader.MapContext(
                      match segment_binding with
                      | Some id ->
                        ExprEvalContext.Updaters.Values(
                          Map.add (id.Name |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve) vField
                        )
                      | None -> id
                    )

                  let valueWithProps =
                    Value.Tuple(
                      vFields
                      |> Seq.mapi (fun i v -> if i = fieldName.Index - 1 then valueWithProps else v)
                      |> Seq.toList
                    )

                  return valueWithProps
                | SchemaPathTypeDecomposition.UnionCase expectedCaseId ->
                  let! actualCaseId, vCaseContent =
                    v |> Value.AsUnion |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                  if actualCaseId.Name <> expectedCaseId.Name then
                    return v
                  else
                    let! vCaseContentWithProps =
                      Expr.Apply(
                        Expr.FromValue(
                          MemoryDBValues.EvalProperty { op with Path = ps } |> valueLens.Set |> Ext,
                          TypeValue.CreatePrimitive PrimitiveType.Unit,
                          Kind.Star
                        ),
                        Expr.FromValue(vCaseContent, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                      )
                      |> fun e -> NonEmptyList.OfList(e, _rest)
                      |> Expr.Eval
                      |> reader.MapContext(
                        match segment_binding with
                        | Some id ->
                          ExprEvalContext.Updaters.Values(
                            Map.add (id.Name |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve) vCaseContent
                          )
                        | None -> id
                      )

                    let valueWithProps = Value.UnionCase(actualCaseId, vCaseContentWithProps)

                    return valueWithProps
                | SchemaPathTypeDecomposition.SumCase expectedCaseId ->
                  let! actualCaseId, vCaseContent =
                    v |> Value.AsSum |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                  if actualCaseId <> expectedCaseId then
                    return v
                  else
                    let! vCaseContentWithProps =
                      Expr.Apply(
                        Expr.FromValue(
                          MemoryDBValues.EvalProperty { op with Path = ps } |> valueLens.Set |> Ext,
                          TypeValue.CreatePrimitive PrimitiveType.Unit,
                          Kind.Star
                        ),
                        Expr.FromValue(vCaseContent, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                      )
                      |> fun e -> NonEmptyList.OfList(e, _rest)
                      |> Expr.Eval
                      |> reader.MapContext(
                        match segment_binding with
                        | Some id ->
                          ExprEvalContext.Updaters.Values(
                            Map.add (id.Name |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve) vCaseContent
                          )
                        | None -> id
                      )

                    let valueWithProps = Value.Sum(actualCaseId, vCaseContentWithProps)

                    return valueWithProps
                | SchemaPathTypeDecomposition.Iterator iterator ->
                  // iterator.Mapper(fun item -> evalProperty(ps, item))(v)
                  // replace "item" with the binding name if present
                  let lambda_var_name =
                    match segment_binding with
                    | Some id -> id.Name
                    | None -> "item"

                  let! res =
                    Expr.Apply(
                      Expr.Apply(
                        iterator.Mapper,
                        Expr.Lambda(
                          Var.Create lambda_var_name,
                          None,
                          Expr.Apply(
                            Expr.FromValue(
                              MemoryDBValues.EvalProperty { op with Path = ps } |> valueLens.Set |> Ext,
                              TypeValue.CreatePrimitive PrimitiveType.Unit,
                              Kind.Star
                            ),
                            Expr.Lookup(lambda_var_name |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve)
                          )
                        )
                      ),
                      Expr.FromValue(v, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                    )
                    |> fun e -> NonEmptyList.OfList(e, _rest)
                    |> Expr.Eval

                  return res
              | [] ->
                let! propertyValue = op.Body |> fun e -> NonEmptyList.OfList(e, _rest) |> Expr.Eval
                let! vFields = v |> Value.AsRecord |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                return
                  Value.Record(
                    vFields
                    |> Map.add (op.PropertyName.Name |> ResolvedIdentifier.Create) propertyValue
                  )
            } }

    let calculateProps (v: Value<TypeValue<'ext>, 'ext>) (_entity: SchemaEntity<'ext>) =
      List.fold
        (fun acc prop ->
          reader {
            let! valueSoFar = acc

            return!
              Expr.Apply(
                Expr.FromValue(
                  MemoryDBValues.EvalProperty
                    { PropertyName = prop.PropertyName
                      Path = prop.Path
                      Body = prop.Body }
                  |> valueLens.Set
                  |> Ext,
                  TypeValue.CreatePrimitive PrimitiveType.Unit,
                  Kind.Star
                ),
                Expr.FromValue(valueSoFar, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
              )
              |> NonEmptyList.One
              |> Expr.Eval
              |> reader.MapContext(
                ExprEvalContext.Updaters.Values(
                  Map.add ("self" |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve) v
                )
              )
          })
        (reader { return v })
        _entity.Properties


    let memoryDBStripPropertyId =
      Identifier.FullyQualified([ "MemoryDB" ], "@@@stripSchemaProperty")
      |> TypeCheckScope.Empty.Resolve

    let StripPropertyOperation: OperationExtension<_, _> =
      { PublicIdentifiers = None
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | MemoryDBValues.StripProperty _ as p -> Some p
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> MemoryDBValues.AsStripProperty
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op.Path with
              | (segment_binding, p) :: ps ->
                match p with
                | SchemaPathTypeDecomposition.Field fieldName ->
                  let! vFields = v |> Value.AsRecord |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                  let! vField =
                    vFields
                    |> Map.tryFind fieldName
                    |> sum.OfOption(Errors.Singleton(loc0, $"Field {fieldName.Name} not found in record"))
                    |> reader.OfSum

                  let! valueWithProps =
                    Expr.Apply(
                      Expr.FromValue(
                        MemoryDBValues.StripProperty { op with Path = ps } |> valueLens.Set |> Ext,
                        TypeValue.CreatePrimitive PrimitiveType.Unit,
                        Kind.Star
                      ),
                      Expr.FromValue(vField, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                    )
                    |> fun e -> NonEmptyList.OfList(e, _rest)
                    |> Expr.Eval
                    |> reader.MapContext(
                      match segment_binding with
                      | Some id ->
                        ExprEvalContext.Updaters.Values(
                          Map.add (id.Name |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve) vField
                        )
                      | None -> id
                    )

                  let valueWithProps = Value.Record(vFields |> Map.add fieldName valueWithProps)
                  return valueWithProps
                | SchemaPathTypeDecomposition.Item fieldName ->
                  let! vFields = v |> Value.AsTuple |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                  let! vField =
                    vFields
                    |> Seq.tryItem (fieldName.Index - 1)
                    |> sum.OfOption(Errors.Singleton(loc0, $"Item {fieldName.Index} not found in tuple"))
                    |> reader.OfSum

                  let! valueWithProps =
                    Expr.Apply(
                      Expr.FromValue(
                        MemoryDBValues.StripProperty { op with Path = ps } |> valueLens.Set |> Ext,
                        TypeValue.CreatePrimitive PrimitiveType.Unit,
                        Kind.Star
                      ),
                      Expr.FromValue(vField, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                    )
                    |> fun e -> NonEmptyList.OfList(e, _rest)
                    |> Expr.Eval
                    |> reader.MapContext(
                      match segment_binding with
                      | Some id ->
                        ExprEvalContext.Updaters.Values(
                          Map.add (id.Name |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve) vField
                        )
                      | None -> id
                    )

                  let valueWithProps =
                    Value.Tuple(
                      vFields
                      |> Seq.mapi (fun i v -> if i = fieldName.Index - 1 then valueWithProps else v)
                      |> Seq.toList
                    )

                  return valueWithProps
                | SchemaPathTypeDecomposition.UnionCase expectedCaseId ->
                  let! actualCaseId, vCaseContent =
                    v |> Value.AsUnion |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                  if actualCaseId.Name <> expectedCaseId.Name then
                    return v
                  else
                    let! vCaseContentWithProps =
                      Expr.Apply(
                        Expr.FromValue(
                          MemoryDBValues.StripProperty { op with Path = ps } |> valueLens.Set |> Ext,
                          TypeValue.CreatePrimitive PrimitiveType.Unit,
                          Kind.Star
                        ),
                        Expr.FromValue(vCaseContent, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                      )
                      |> fun e -> NonEmptyList.OfList(e, _rest)
                      |> Expr.Eval
                      |> reader.MapContext(
                        match segment_binding with
                        | Some id ->
                          ExprEvalContext.Updaters.Values(
                            Map.add (id.Name |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve) vCaseContent
                          )
                        | None -> id
                      )

                    let valueWithProps = Value.UnionCase(actualCaseId, vCaseContentWithProps)

                    return valueWithProps
                | SchemaPathTypeDecomposition.SumCase expectedCaseId ->
                  let! actualCaseId, vCaseContent =
                    v |> Value.AsSum |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                  if actualCaseId <> expectedCaseId then
                    return v
                  else
                    let! vCaseContentWithProps =
                      Expr.Apply(
                        Expr.FromValue(
                          MemoryDBValues.StripProperty { op with Path = ps } |> valueLens.Set |> Ext,
                          TypeValue.CreatePrimitive PrimitiveType.Unit,
                          Kind.Star
                        ),
                        Expr.FromValue(vCaseContent, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                      )
                      |> fun e -> NonEmptyList.OfList(e, _rest)
                      |> Expr.Eval
                      |> reader.MapContext(
                        match segment_binding with
                        | Some id ->
                          ExprEvalContext.Updaters.Values(
                            Map.add (id.Name |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve) vCaseContent
                          )
                        | None -> id
                      )

                    let valueWithProps = Value.Sum(actualCaseId, vCaseContentWithProps)

                    return valueWithProps
                | SchemaPathTypeDecomposition.Iterator iterator ->
                  // iterator.Mapper(fun item -> evalProperty(ps, item))(v)
                  // replace "item" with the binding name if present
                  let lambda_var_name =
                    match segment_binding with
                    | Some id -> id.Name
                    | None -> "item"

                  let! res =
                    Expr.Apply(
                      Expr.Apply(
                        iterator.Mapper,
                        Expr.Lambda(
                          Var.Create lambda_var_name,
                          None,
                          Expr.Apply(
                            Expr.FromValue(
                              MemoryDBValues.StripProperty { op with Path = ps } |> valueLens.Set |> Ext,
                              TypeValue.CreatePrimitive PrimitiveType.Unit,
                              Kind.Star
                            ),
                            Expr.Lookup(lambda_var_name |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve)
                          )
                        )
                      ),
                      Expr.FromValue(v, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
                    )
                    |> fun e -> NonEmptyList.OfList(e, _rest)
                    |> Expr.Eval

                  return res
              | [] ->

                let! vFields = v |> Value.AsRecord |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                return Value.Record(vFields |> Map.remove (op.PropertyName.Name |> ResolvedIdentifier.Create))
            } }

    let stripProps (v: Value<TypeValue<'ext>, 'ext>) (_entity: SchemaEntity<'ext>) =
      List.fold
        (fun acc prop ->
          reader {
            let! valueSoFar = acc

            return!
              Expr.Apply(
                Expr.FromValue(
                  MemoryDBValues.StripProperty
                    { PropertyName = prop.PropertyName
                      Path = prop.Path
                      Body = prop.Body }
                  |> valueLens.Set
                  |> Ext,
                  TypeValue.CreatePrimitive PrimitiveType.Unit,
                  Kind.Star
                ),
                Expr.FromValue(valueSoFar, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
              )
              |> NonEmptyList.One
              |> Expr.Eval
              |> reader.MapContext(
                ExprEvalContext.Updaters.Values(
                  Map.add ("self" |> Identifier.LocalScope |> TypeCheckScope.Empty.Resolve) v
                )
              )
          })
        (reader { return v })
        _entity.Properties


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
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                let! v = v |> Value.AsExt |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                let! v =
                  v
                  |> valueLens.Get
                  |> sum.OfOption(Errors.Singleton(loc0, "Cannot get value from extension"))
                  |> reader.OfSum

                let! v =
                  v
                  |> MemoryDBValues.AsEntityRef
                  |> sum.MapError(Errors.FromErrors loc0)
                  |> reader.OfSum

                return MemoryDBValues.Create({| EntityRef = Some v |}) |> valueLens.Set |> Ext
              | Some(_schema, _db, _entity) -> // the closure has the first operand - second step in the application
                let! v = v |> Value.AsTuple |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                match v with
                | [ _entityId; v ] ->
                  let! valueWithProps = calculateProps v _entity

                  do
                    _db.entities <-
                      _db.entities
                      |> Map.change _entity.Name (function
                        | Some entities -> Some(entities |> Map.add _entityId valueWithProps)
                        | None -> Some(Map.empty |> Map.add _entityId valueWithProps))

                  return Value.Sum({ Case = 2; Count = 2 }, valueWithProps)
                | _ ->
                  return!
                    sum.Throw(Errors.Singleton(loc0, "Expected a tuple with 2 elements when creating DB entity"))
                    |> reader.OfSum
            } }

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
                TypeExpr.Apply(
                  TypeExpr.Apply(
                    TypeExpr.Apply(
                      TypeExpr.Apply(
                        TypeExpr.Lookup("SchemaEntity" |> Identifier.LocalScope),
                        TypeExpr.Lookup("schema" |> Identifier.LocalScope)

                      ),
                      TypeExpr.Lookup("entity" |> Identifier.LocalScope)
                    ),
                    TypeExpr.Lookup("entity_with_props" |> Identifier.LocalScope)
                  ),
                  TypeExpr.Lookup("entityId" |> Identifier.LocalScope)
                ),
                TypeExpr.Arrow(
                  TypeExpr.Tuple
                    [ TypeExpr.Lookup("entityId" |> Identifier.LocalScope)
                      TypeExpr.Arrow(
                        TypeExpr.Lookup("entity" |> Identifier.LocalScope),
                        TypeExpr.Lookup("entity" |> Identifier.LocalScope)
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

    let memoryDBUpdateKind =
      Kind.Arrow(Kind.Schema, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))))

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
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                let! v = v |> Value.AsExt |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                let! v =
                  v
                  |> valueLens.Get
                  |> sum.OfOption(Errors.Singleton(loc0, "Cannot get value from extension"))
                  |> reader.OfSum

                let! v =
                  v
                  |> MemoryDBValues.AsEntityRef
                  |> sum.MapError(Errors.FromErrors loc0)
                  |> reader.OfSum

                return MemoryDBValues.Update({| EntityRef = Some v |}) |> valueLens.Set |> Ext
              | Some(_schema, _db, _entity) -> // the closure has the first operand - second step in the application

                let! v = v |> Value.AsTuple |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                match v with
                | [ _entityId; updateFunc ] ->
                  let existingValue =
                    option {
                      let! entity = _db.entities |> Map.tryFind _entity.Name
                      let! value = entity |> Map.tryFind _entityId
                      return value
                    }

                  match existingValue with
                  | None -> return Value.Sum({ Case = 1; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
                  | Some existingValue ->
                    let! existingValueWithoutProps = stripProps existingValue _entity

                    let! updatedValue =
                      Expr.Apply(
                        Expr.FromValue(updateFunc, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star),
                        Expr.FromValue(
                          existingValueWithoutProps,
                          TypeValue.CreatePrimitive PrimitiveType.Unit,
                          Kind.Star
                        )
                      )
                      |> NonEmptyList.One
                      |> Expr.Eval

                    let! valueWithProps = calculateProps updatedValue _entity

                    do
                      _db.entities <-
                        _db.entities
                        |> Map.change _entity.Name (function
                          | Some entities -> Some(entities |> Map.add _entityId valueWithProps)
                          | None -> Some(Map.empty |> Map.add _entityId valueWithProps))

                    return Value.Sum({ Case = 2; Count = 2 }, valueWithProps)
                | _ ->
                  return!
                    sum.Throw(Errors.Singleton(loc0, "Expected a tuple with 2 elements when updating DB entity"))
                    |> reader.OfSum
            } }

    let memoryDBDeleteId =
      Identifier.FullyQualified([ "MemoryDB" ], "delete")
      |> TypeCheckScope.Empty.Resolve

    let memoryDBDeleteType =
      TypeValue.CreateLambda(
        TypeParameter.Create("schema", Kind.Schema),
        TypeExpr.Lambda(
          TypeParameter.Create("entity", Kind.Star),
          TypeExpr.Lambda(
            TypeParameter.Create("entity_with_props", Kind.Star),
            TypeExpr.Lambda(
              TypeParameter.Create("entityId", Kind.Star),
              TypeExpr.Arrow(
                TypeExpr.Apply(
                  TypeExpr.Apply(
                    TypeExpr.Apply(
                      TypeExpr.Apply(
                        TypeExpr.Lookup("SchemaEntity" |> Identifier.LocalScope),
                        TypeExpr.Lookup("schema" |> Identifier.LocalScope)

                      ),
                      TypeExpr.Lookup("entity" |> Identifier.LocalScope)
                    ),
                    TypeExpr.Lookup("entity_with_props" |> Identifier.LocalScope)
                  ),
                  TypeExpr.Lookup("entityId" |> Identifier.LocalScope)
                ),
                TypeExpr.Arrow(
                  TypeExpr.Lookup("entityId" |> Identifier.LocalScope),
                  TypeExpr.Primitive PrimitiveType.Bool
                )
              )
            )
          )
        )
      )

    let memoryDBDeleteKind =
      Kind.Arrow(Kind.Schema, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))))

    let DeleteOperation: OperationExtension<_, _> =
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
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op with
              | None -> // the closure is empty - first step in the application
                let! v = v |> Value.AsExt |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                let! v =
                  v
                  |> valueLens.Get
                  |> sum.OfOption(Errors.Singleton(loc0, "Cannot get value from extension"))
                  |> reader.OfSum

                let! v =
                  v
                  |> MemoryDBValues.AsEntityRef
                  |> sum.MapError(Errors.FromErrors loc0)
                  |> reader.OfSum

                return MemoryDBValues.Delete({| EntityRef = Some v |}) |> valueLens.Set |> Ext
              | Some(_schema, _db, _entity) -> // the closure has the first operand - second step in the application
                return Value.Primitive(PrimitiveValue.Bool true)
            } }

    { TypeVars = []
      Operations =
        [ (memoryDBStripPropertyId, StripPropertyOperation)
          (memoryDBCalculatePropertyId, CalculatePropertyOperation)
          (memoryDBCreateId, CreateOperation)
          (memoryDBUpdateId, UpdateOperation)
          (memoryDBDeleteId, DeleteOperation) ]
        |> Map.ofList }

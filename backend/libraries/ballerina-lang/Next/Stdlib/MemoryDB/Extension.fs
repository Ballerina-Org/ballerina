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
          MemoryDBValues.TypeAppliedRun(
            schema,
            { entities = Map.empty
              relations = Map.empty }
          )
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

        let! relation_values =
          schema.Relations
          |> OrderedMap.toSeq
          |> Seq.map (fun (k, v) ->
            reader {
              let! from =
                schema.Entities
                |> OrderedMap.tryFind (v.From.LocalName |> SchemaEntityName.Create)
                |> sum.OfOption(Errors.Singleton(_loc0, $"Entity {v.From.LocalName} not found in schema"))
                |> reader.OfSum

              let! to_ =
                schema.Entities
                |> OrderedMap.tryFind (v.To.LocalName |> SchemaEntityName.Create)
                |> sum.OfOption(Errors.Singleton(_loc0, $"Entity {v.To.LocalName} not found in schema"))
                |> reader.OfSum

              return
                k.Name |> ResolvedIdentifier.Create,
                Value.Record(
                  [ "Relation" |> ResolvedIdentifier.Create,
                    MemoryDBValues.RelationRef(schema, db, v, from, to_) |> valueLens.Set |> Ext
                    "From" |> ResolvedIdentifier.Create,
                    MemoryDBValues.RelationLookupRef(schema, db, RelationLookupDirection.FromTo, v, from, to_)
                    |> valueLens.Set
                    |> Ext
                    "To" |> ResolvedIdentifier.Create,
                    MemoryDBValues.RelationLookupRef(schema, db, RelationLookupDirection.ToFrom, v, from, to_)
                    |> valueLens.Set
                    |> Ext ]
                  |> Map.ofList
                )
            })
          |> reader.All

        let arg =
          [ "Entities" |> ResolvedIdentifier.Create,
            Value.Record(
              schema.Entities
              |> OrderedMap.toSeq
              |> Seq.map (fun (k, v) ->
                k.Name |> ResolvedIdentifier.Create, MemoryDBValues.EntityRef(schema, db, v) |> valueLens.Set |> Ext)
              |> Map.ofSeq
            )
            "Relations" |> ResolvedIdentifier.Create, Value.Record(relation_values |> Map.ofSeq) ]
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
    (listSet: List<Value<TypeValue<'ext>, 'ext>> -> 'ext)
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

    let memoryDBLinkId =
      Identifier.FullyQualified([ "MemoryDB" ], "link")
      |> TypeCheckScope.Empty.Resolve

    let memoryDBLinkType =
      TypeValue.CreateLambda(
        TypeParameter.Create("schema", Kind.Schema),
        TypeExpr.Lambda(
          TypeParameter.Create("from", Kind.Star),
          TypeExpr.Lambda(
            TypeParameter.Create("from_with_props", Kind.Star),
            TypeExpr.Lambda(
              TypeParameter.Create("from_id", Kind.Star),
              TypeExpr.Lambda(
                TypeParameter.Create("to", Kind.Star),
                TypeExpr.Lambda(
                  TypeParameter.Create("to_with_props", Kind.Star),
                  TypeExpr.Lambda(
                    TypeParameter.Create("to_id", Kind.Star),
                    TypeExpr.Arrow(
                      TypeExpr.Apply(
                        TypeExpr.Apply(
                          TypeExpr.Apply(
                            TypeExpr.Apply(
                              TypeExpr.Apply(
                                TypeExpr.Apply(
                                  TypeExpr.Apply(
                                    TypeExpr.Lookup("SchemaRelation" |> Identifier.LocalScope),
                                    TypeExpr.Lookup("schema" |> Identifier.LocalScope)

                                  ),
                                  TypeExpr.Lookup("from" |> Identifier.LocalScope)
                                ),
                                TypeExpr.Lookup("from_with_props" |> Identifier.LocalScope)
                              ),
                              TypeExpr.Lookup("from_id" |> Identifier.LocalScope)
                            ),
                            TypeExpr.Lookup("to" |> Identifier.LocalScope)
                          ),
                          TypeExpr.Lookup("to_with_props" |> Identifier.LocalScope)
                        ),
                        TypeExpr.Lookup("to_id" |> Identifier.LocalScope)
                      ),
                      TypeExpr.Arrow(
                        TypeExpr.Tuple
                          [ TypeExpr.Lookup("from_id" |> Identifier.LocalScope)
                            TypeExpr.Lookup("to_id" |> Identifier.LocalScope) ],
                        TypeExpr.Sum [ TypeExpr.Primitive PrimitiveType.Unit; TypeExpr.Primitive PrimitiveType.Unit ]
                      )
                    )
                  )
                )
              )
            )
          )
        )
      )

    let memoryDBLinkKind =
      Kind.Arrow(
        Kind.Schema,
        Kind.Arrow(
          Kind.Star,
          Kind.Arrow(
            Kind.Star,
            Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))))
          )
        )
      )

    let LinkOperation: OperationExtension<_, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBLinkType, memoryDBLinkKind, MemoryDBValues.Link {| RelationRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | MemoryDBValues.Link v -> Some(MemoryDBValues.Link v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> MemoryDBValues.AsLink
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op.RelationRef with
              | None -> // the closure is empty - first step in the application
                let! v = v |> Value.AsRecord |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                let! v =
                  v
                  |> Map.tryFind (ResolvedIdentifier.Create "Relation")
                  |> sum.OfOption(Errors.Singleton(loc0, "Cannot find 'Relation' field in link operation"))
                  |> reader.OfSum

                let! v = v |> Value.AsExt |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                let! v =
                  v
                  |> valueLens.Get
                  |> sum.OfOption(Errors.Singleton(loc0, "Cannot get value from extension"))
                  |> reader.OfSum

                let! v =
                  v
                  |> MemoryDBValues.AsRelationRef
                  |> sum.MapError(Errors.FromErrors loc0)
                  |> reader.OfSum

                return MemoryDBValues.Link({| RelationRef = Some v |}) |> valueLens.Set |> Ext
              | Some(_schema, _db, _relation, _from, _to) -> // the closure has the first operand - second step in the application

                let! v = v |> Value.AsTuple |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                match v with
                | [ _fromId; _toId ] ->

                  let add_link (relation: MemoryDBRelation<'ext>) : MemoryDBRelation<'ext> =
                    { relation with
                        All = relation.All |> Set.add (_fromId, _toId)
                        FromTo =
                          relation.FromTo
                          |> Map.change _fromId (function
                            | Some toSet -> Some(toSet |> Set.add _toId)
                            | None -> Some(Set.singleton _toId))
                        ToFrom =
                          relation.ToFrom
                          |> Map.change _toId (function
                            | Some fromSet -> Some(fromSet |> Set.add _fromId)
                            | None -> Some(Set.singleton _fromId)) }

                  do
                    _db.relations <-
                      _db.relations
                      |> Map.change _relation.Name (function
                        | Some relations -> Some(relations |> add_link)
                        | None -> Some(MemoryDBRelation.Empty |> add_link))

                  return Value.Sum({ Case = 2; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
                | _ ->
                  return!
                    sum.Throw(Errors.Singleton(loc0, "Expected a tuple with 2 elements when linking relation"))
                    |> reader.OfSum
            } }

    let memoryDBUnlinkId =
      Identifier.FullyQualified([ "MemoryDB" ], "unlink")
      |> TypeCheckScope.Empty.Resolve

    let memoryDBUnlinkType =
      TypeValue.CreateLambda(
        TypeParameter.Create("schema", Kind.Schema),
        TypeExpr.Lambda(
          TypeParameter.Create("from", Kind.Star),
          TypeExpr.Lambda(
            TypeParameter.Create("from_with_props", Kind.Star),
            TypeExpr.Lambda(
              TypeParameter.Create("from_id", Kind.Star),
              TypeExpr.Lambda(
                TypeParameter.Create("to", Kind.Star),
                TypeExpr.Lambda(
                  TypeParameter.Create("to_with_props", Kind.Star),
                  TypeExpr.Lambda(
                    TypeParameter.Create("to_id", Kind.Star),
                    TypeExpr.Arrow(
                      TypeExpr.Apply(
                        TypeExpr.Apply(
                          TypeExpr.Apply(
                            TypeExpr.Apply(
                              TypeExpr.Apply(
                                TypeExpr.Apply(
                                  TypeExpr.Apply(
                                    TypeExpr.Lookup("SchemaRelation" |> Identifier.LocalScope),
                                    TypeExpr.Lookup("schema" |> Identifier.LocalScope)

                                  ),
                                  TypeExpr.Lookup("from" |> Identifier.LocalScope)
                                ),
                                TypeExpr.Lookup("from_with_props" |> Identifier.LocalScope)
                              ),
                              TypeExpr.Lookup("from_id" |> Identifier.LocalScope)
                            ),
                            TypeExpr.Lookup("to" |> Identifier.LocalScope)
                          ),
                          TypeExpr.Lookup("to_with_props" |> Identifier.LocalScope)
                        ),
                        TypeExpr.Lookup("to_id" |> Identifier.LocalScope)
                      ),
                      TypeExpr.Arrow(
                        TypeExpr.Tuple
                          [ TypeExpr.Lookup("from_id" |> Identifier.LocalScope)
                            TypeExpr.Lookup("to_id" |> Identifier.LocalScope) ],
                        TypeExpr.Sum [ TypeExpr.Primitive PrimitiveType.Unit; TypeExpr.Primitive PrimitiveType.Unit ]
                      )
                    )
                  )
                )
              )
            )
          )
        )
      )

    let memoryDBUnlinkKind =
      Kind.Arrow(
        Kind.Schema,
        Kind.Arrow(
          Kind.Star,
          Kind.Arrow(
            Kind.Star,
            Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))))
          )
        )
      )

    let UnlinkOperation: OperationExtension<_, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBUnlinkType, memoryDBUnlinkKind, MemoryDBValues.Unlink {| RelationRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | MemoryDBValues.Unlink v -> Some(MemoryDBValues.Unlink v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> MemoryDBValues.AsUnlink
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op.RelationRef with
              | None -> // the closure is empty - first step in the application
                let! v = v |> Value.AsRecord |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                let! v =
                  v
                  |> Map.tryFind (ResolvedIdentifier.Create "Relation")
                  |> sum.OfOption(Errors.Singleton(loc0, "Cannot find 'Relation' field in unlink operation"))
                  |> reader.OfSum

                let! v = v |> Value.AsExt |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                let! v =
                  v
                  |> valueLens.Get
                  |> sum.OfOption(Errors.Singleton(loc0, "Cannot get value from extension"))
                  |> reader.OfSum

                let! v =
                  v
                  |> MemoryDBValues.AsRelationRef
                  |> sum.MapError(Errors.FromErrors loc0)
                  |> reader.OfSum

                return MemoryDBValues.Unlink({| RelationRef = Some v |}) |> valueLens.Set |> Ext
              | Some(_schema, _db, _relation, _from, _to) -> // the closure has the first operand - second step in the application

                let! v = v |> Value.AsTuple |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                match v with
                | [ _fromId; _toId ] ->

                  let remove_link (relation: MemoryDBRelation<'ext>) : MemoryDBRelation<'ext> =
                    let res =
                      { relation with
                          All = relation.All |> Set.remove (_fromId, _toId)
                          FromTo =
                            relation.FromTo
                            |> Map.change _fromId (function
                              | Some toSet -> Some(toSet |> Set.remove _toId)
                              | None -> Some(Set.empty))
                          ToFrom =
                            relation.ToFrom
                            |> Map.change _toId (function
                              | Some fromSet -> Some(fromSet |> Set.remove _fromId)
                              | None -> Some(Set.empty)) }

                    res

                  do
                    _db.relations <-
                      _db.relations
                      |> Map.change _relation.Name (function
                        | Some relations -> Some(relations |> remove_link)
                        | None -> Some(MemoryDBRelation.Empty |> remove_link))

                  return Value.Sum({ Case = 2; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
                | _ ->
                  return!
                    sum.Throw(Errors.Singleton(loc0, "Expected a tuple with 2 elements when unlinking relation"))
                    |> reader.OfSum
            } }

    let actual_lookup
      loc0
      (
        _schema: Schema<'ext>,
        _db: MutableMemoryDB<'ext>,
        _dir,
        _relation: SchemaRelation,
        _from: SchemaEntity<'ext>,
        _to: SchemaEntity<'ext>
      )
      v
      =
      reader {
        let! relation_ref =
          _db.relations
          |> Map.tryFind _relation.Name
          |> sum.OfOption(Errors.Singleton(loc0, "Relation not found"))
          |> reader.OfSum

        let source_entity_ref, target_entity_ref, source_to_targets =
          match _dir with
          | FromTo -> _from, _to, relation_ref.FromTo
          | ToFrom -> _to, _from, relation_ref.ToFrom

        let source_id = v

        let! sources =
          _db.entities
          |> Map.tryFind source_entity_ref.Name
          |> sum.OfOption(Errors.Singleton(loc0, "Source entity not found"))
          |> reader.OfSum

        let! targets =
          _db.entities
          |> Map.tryFind target_entity_ref.Name
          |> sum.OfOption(Errors.Singleton(loc0, "Target entity not found"))
          |> reader.OfSum

        do!
          sources
          |> Map.tryFind source_id
          |> sum.OfOption(Errors.Singleton(loc0, "Source ID not found"))
          |> reader.OfSum
          |> reader.Ignore

        let target_ids =
          source_to_targets |> Map.tryFind source_id |> Option.defaultValue Set.empty

        return!
          target_ids
          |> Set.toSeq
          |> Seq.map (fun target_id ->
            reader {
              let! target_v =
                targets
                |> Map.tryFind target_id
                |> sum.OfOption(Errors.Singleton(loc0, "Target ID not found"))
                |> reader.OfSum

              return target_v
            })
          |> reader.All
      }

    let memoryDBLookupOneId =
      Identifier.FullyQualified([ "MemoryDB" ], "lookupOne")
      |> TypeCheckScope.Empty.Resolve

    let memoryDBLookupOneType =
      TypeValue.CreateLambda(
        TypeParameter.Create("schema", Kind.Schema),
        TypeExpr.Lambda(
          TypeParameter.Create("from_id", Kind.Star),
          TypeExpr.Lambda(
            TypeParameter.Create("to_with_props", Kind.Star),
            TypeExpr.Arrow(
              TypeExpr.Apply(
                TypeExpr.Apply(
                  TypeExpr.Apply(
                    TypeExpr.Lookup("SchemaLookupOne" |> Identifier.LocalScope),
                    TypeExpr.Lookup("schema" |> Identifier.LocalScope)
                  ),
                  TypeExpr.Lookup("from_id" |> Identifier.LocalScope)
                ),
                TypeExpr.Lookup("to_with_props" |> Identifier.LocalScope)
              ),
              TypeExpr.Arrow(
                TypeExpr.Lookup("from_id" |> Identifier.LocalScope),
                TypeExpr.Sum
                  [ TypeExpr.Primitive PrimitiveType.Unit
                    TypeExpr.Lookup("to_with_props" |> Identifier.LocalScope) ]
              )
            )
          )
        )
      )

    let memoryDBLookupOneKind =
      Kind.Arrow(Kind.Schema, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star)))


    let LookupOneOperation: OperationExtension<_, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBLookupOneType, memoryDBLookupOneKind, MemoryDBValues.LookupOne {| RelationRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | MemoryDBValues.LookupOne v -> Some(MemoryDBValues.LookupOne v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> MemoryDBValues.AsLookupOne
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op.RelationRef with
              | None -> // the closure is empty - first step in the application
                let! v = v |> Value.AsExt |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                let! v =
                  v
                  |> valueLens.Get
                  |> sum.OfOption(Errors.Singleton(loc0, "Cannot get value from extension"))
                  |> reader.OfSum

                let! v =
                  v
                  |> MemoryDBValues.AsRelationLookupRef
                  |> sum.MapError(Errors.FromErrors loc0)
                  |> reader.OfSum

                return MemoryDBValues.LookupOne {| RelationRef = Some v |} |> valueLens.Set |> Ext
              | Some relation_ref -> // the closure has the first operand - second step in the application
                let! target_values = actual_lookup loc0 relation_ref v |> reader.Catch

                match target_values with
                | Right(_e: Errors) -> return Value.Sum({ Case = 1; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
                | Left target_values ->
                  match target_values |> List.tryHead with
                  | Some v -> return Value.Sum({ Case = 2; Count = 2 }, v)
                  | _ -> return Value.Sum({ Case = 1; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
            } }

    let memoryDBLookupOptionId =
      Identifier.FullyQualified([ "MemoryDB" ], "lookupOption")
      |> TypeCheckScope.Empty.Resolve

    let memoryDBLookupOptionType =
      TypeValue.CreateLambda(
        TypeParameter.Create("schema", Kind.Schema),
        TypeExpr.Lambda(
          TypeParameter.Create("from_id", Kind.Star),
          TypeExpr.Lambda(
            TypeParameter.Create("to_with_props", Kind.Star),
            TypeExpr.Arrow(
              TypeExpr.Apply(
                TypeExpr.Apply(
                  TypeExpr.Apply(
                    TypeExpr.Lookup("SchemaLookupOption" |> Identifier.LocalScope),
                    TypeExpr.Lookup("schema" |> Identifier.LocalScope)
                  ),
                  TypeExpr.Lookup("from_id" |> Identifier.LocalScope)
                ),
                TypeExpr.Lookup("to_with_props" |> Identifier.LocalScope)
              ),
              TypeExpr.Arrow(
                TypeExpr.Lookup("from_id" |> Identifier.LocalScope),
                TypeExpr.Sum
                  [ TypeExpr.Primitive PrimitiveType.Unit
                    TypeExpr.Sum
                      [ TypeExpr.Primitive PrimitiveType.Unit
                        TypeExpr.Lookup("to_with_props" |> Identifier.LocalScope) ] ]
              )
            )
          )
        )
      )

    let memoryDBLookupOptionKind =
      Kind.Arrow(Kind.Schema, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star)))

    let LookupOptionOperation: OperationExtension<_, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBLookupOptionType, memoryDBLookupOptionKind, MemoryDBValues.LookupOption {| RelationRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | MemoryDBValues.LookupOption v -> Some(MemoryDBValues.LookupOption v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> MemoryDBValues.AsLookupOption
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op.RelationRef with
              | None -> // the closure is empty - first step in the application
                let! v = v |> Value.AsExt |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                let! v =
                  v
                  |> valueLens.Get
                  |> sum.OfOption(Errors.Singleton(loc0, "Cannot get value from extension"))
                  |> reader.OfSum

                let! v =
                  v
                  |> MemoryDBValues.AsRelationLookupRef
                  |> sum.MapError(Errors.FromErrors loc0)
                  |> reader.OfSum

                return MemoryDBValues.LookupOption {| RelationRef = Some v |} |> valueLens.Set |> Ext
              | Some relation_ref -> // the closure has the first operand - second step in the application
                let! target_values = actual_lookup loc0 relation_ref v |> reader.Catch

                match target_values with
                | Right(_e: Errors) -> return Value.Sum({ Case = 1; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
                | Left target_values ->
                  match target_values |> List.tryHead with
                  | Some v -> return Value.Sum({ Case = 2; Count = 2 }, Value.Sum({ Case = 2; Count = 2 }, v))
                  | _ ->
                    return
                      Value.Sum(
                        { Case = 2; Count = 2 },
                        Value.Sum({ Case = 1; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
                      )
            } }

    let memoryDBLookupManyId =
      Identifier.FullyQualified([ "MemoryDB" ], "lookupMany")
      |> TypeCheckScope.Empty.Resolve

    let memoryDBLookupManyType =
      TypeValue.CreateLambda(
        TypeParameter.Create("schema", Kind.Schema),
        TypeExpr.Lambda(
          TypeParameter.Create("from_id", Kind.Star),
          TypeExpr.Lambda(
            TypeParameter.Create("to_with_props", Kind.Star),
            TypeExpr.Arrow(
              TypeExpr.Apply(
                TypeExpr.Apply(
                  TypeExpr.Apply(
                    TypeExpr.Lookup("SchemaLookupMany" |> Identifier.LocalScope),
                    TypeExpr.Lookup("schema" |> Identifier.LocalScope)
                  ),
                  TypeExpr.Lookup("from_id" |> Identifier.LocalScope)
                ),
                TypeExpr.Lookup("to_with_props" |> Identifier.LocalScope)
              ),
              TypeExpr.Arrow(
                TypeExpr.Lookup("from_id" |> Identifier.LocalScope),
                TypeExpr.Sum
                  [ TypeExpr.Primitive PrimitiveType.Unit
                    TypeExpr.Apply(
                      TypeExpr.Lookup("List" |> Identifier.LocalScope),
                      TypeExpr.Lookup("to_with_props" |> Identifier.LocalScope)
                    ) ]
              )
            )
          )
        )
      )

    let memoryDBLookupManyKind =
      Kind.Arrow(Kind.Schema, Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star)))

    let LookupManyOperation: OperationExtension<_, _> =
      { PublicIdentifiers =
          Some
          <| (memoryDBLookupManyType, memoryDBLookupManyKind, MemoryDBValues.LookupMany {| RelationRef = None |})
        OperationsLens =
          valueLens
          |> PartialLens.BindGet (function
            | MemoryDBValues.LookupMany v -> Some(MemoryDBValues.LookupMany v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              let! op =
                op
                |> MemoryDBValues.AsLookupMany
                |> sum.MapError(Errors.FromErrors loc0)
                |> reader.OfSum

              match op.RelationRef with
              | None -> // the closure is empty - first step in the application
                let! v = v |> Value.AsExt |> sum.MapError(Errors.FromErrors loc0) |> reader.OfSum

                let! v =
                  v
                  |> valueLens.Get
                  |> sum.OfOption(Errors.Singleton(loc0, "Cannot get value from extension"))
                  |> reader.OfSum

                let! v =
                  v
                  |> MemoryDBValues.AsRelationLookupRef
                  |> sum.MapError(Errors.FromErrors loc0)
                  |> reader.OfSum

                return MemoryDBValues.LookupMany {| RelationRef = Some v |} |> valueLens.Set |> Ext
              | Some relation_ref -> // the closure has the first operand - second step in the application
                let! target_values = actual_lookup loc0 relation_ref v |> reader.Catch

                match target_values with
                | Right(_e: Errors) -> return Value.Sum({ Case = 1; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
                | Left target_values -> return Value.Sum({ Case = 2; Count = 2 }, target_values |> listSet |> Ext)

            } }

    { TypeVars = []
      Operations =
        [ (memoryDBStripPropertyId, StripPropertyOperation)
          (memoryDBCalculatePropertyId, CalculatePropertyOperation)
          (memoryDBCreateId, CreateOperation)
          (memoryDBUpdateId, UpdateOperation)
          (memoryDBLinkId, LinkOperation)
          (memoryDBUnlinkId, UnlinkOperation)
          (memoryDBLookupOptionId, LookupOptionOperation)
          (memoryDBLookupOneId, LookupOneOperation)
          (memoryDBLookupManyId, LookupManyOperation)
          (memoryDBDeleteId, DeleteOperation) ]
        |> Map.ofList }

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
                        From =
                          relation.From
                          |> Map.change _fromId (function
                            | Some toSet -> Some(toSet |> Set.add _toId)
                            | None -> Some(Set.singleton _toId))
                        To =
                          relation.To
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
          <| (memoryDBUnlinkType, memoryDBUnlinkKind, MemoryDBValues.Link {| RelationRef = None |})
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
                    { relation with
                        All = relation.All |> Set.remove (_fromId, _toId)
                        From =
                          relation.From
                          |> Map.change _fromId (function
                            | Some toSet -> Some(toSet |> Set.remove _toId)
                            | None -> Some(Set.empty))
                        To =
                          relation.To
                          |> Map.change _toId (function
                            | Some fromSet -> Some(fromSet |> Set.remove _fromId)
                            | None -> Some(Set.empty)) }

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


    { TypeVars = []
      Operations =
        [ (memoryDBStripPropertyId, StripPropertyOperation)
          (memoryDBCalculatePropertyId, CalculatePropertyOperation)
          (memoryDBCreateId, CreateOperation)
          (memoryDBUpdateId, UpdateOperation)
          (memoryDBLinkId, LinkOperation)
          (memoryDBUnlinkId, UnlinkOperation)
          (memoryDBDeleteId, DeleteOperation) ]
        |> Map.ofList }

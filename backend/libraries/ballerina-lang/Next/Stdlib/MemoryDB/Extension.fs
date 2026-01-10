namespace Ballerina.DSL.Next.StdLib.MemoryDB

[<AutoOpen>]
module Extension =
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

  let MemoryDBRunExtension<'ext>
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

        return MemoryDBValues.TypeAppliedRun schema |> valueLens.Set |> Ext
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
                k.Name |> ResolvedIdentifier.Create, MemoryDBValues.EntityRef(schema, v) |> valueLens.Set |> Ext)
              |> Map.ofSeq
            ) ]
          |> Map.ofList
          |> Value.Record

        return!
          Expr.Apply(
            Expr.FromValue(value, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star),
            Expr.FromValue(arg, TypeValue.CreatePrimitive PrimitiveType.Unit, Kind.Star)
          )
          |> Expr.Eval _rest
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

        let! schema =
          v
          |> MemoryDBValues.AsTypeAppliedRun
          |> sum.MapError(Errors.FromErrors loc0)
          |> reader.OfSum

        return Applicable(fun arg -> apply loc0 schema arg _rest)
      }

    { ExtensionType = memoryDBRunId, memoryDBRunType, memoryDBRunKind
      ReferencedTypes = []
      Value = MemoryDBValues.Run
      ValueLens = valueLens
      EvalToTypeApplicable = evalToTypeApplicable
      EvalToApplicable = evalToApplicable }

  let MemoryDBGetByIdExtension<'ext>
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
      { Type = memoryDBGetByIdType
        Kind = memoryDBGetByIdKind
        Operation = MemoryDBValues.GetById {| EntityRef = None |}
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
              | Some(_schema, _entity) -> // the closure has the first operand - second step in the application
                return Value.Sum({ Case = 1; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
            } }

    { TypeVars = []
      Operations = [ memoryDBGetById, getByIdOperation ] |> Map.ofList }

  let MemoryDBCreateExtension<'ext>
    (valueLens: PartialLens<'ext, MemoryDBValues<'ext>>)
    : OperationsExtension<'ext, MemoryDBValues<'ext>> =

    let memoryDBCreateC =
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

    let CreateOperation: OperationExtension<_, _> =
      { Type = memoryDBCreateType
        Kind = memoryDBCreateKind
        Operation = MemoryDBValues.Create {| EntityRef = None |}
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
              | Some(_schema, _entity) -> // the closure has the first operand - second step in the application
                return Value.Sum({ Case = 1; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
            } }

    { TypeVars = []
      Operations = [ memoryDBCreateC, CreateOperation ] |> Map.ofList }

  let MemoryDBUpdateExtension<'ext>
    (valueLens: PartialLens<'ext, MemoryDBValues<'ext>>)
    : OperationsExtension<'ext, MemoryDBValues<'ext>> =

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
      { Type = memoryDBUpdateType
        Kind = memoryDBUpdateKind
        Operation = MemoryDBValues.Update {| EntityRef = None |}
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
              | Some(_schema, _entity) -> // the closure has the first operand - second step in the application
                return Value.Sum({ Case = 1; Count = 2 }, Value.Primitive PrimitiveValue.Unit)
            } }

    { TypeVars = []
      Operations = [ memoryDBUpdateId, UpdateOperation ] |> Map.ofList }

  let MemoryDBDeleteExtension<'ext>
    (valueLens: PartialLens<'ext, MemoryDBValues<'ext>>)
    : OperationsExtension<'ext, MemoryDBValues<'ext>> =

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
      { Type = memoryDBDeleteType
        Kind = memoryDBDeleteKind
        Operation = MemoryDBValues.Delete {| EntityRef = None |}
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

                return MemoryDBValues.Delete({| EntityRef = Some v |}) |> valueLens.Set |> Ext
              | Some(_schema, _entity) -> // the closure has the first operand - second step in the application
                return Value.Primitive(PrimitiveValue.Bool true)
            } }

    { TypeVars = []
      Operations = [ memoryDBDeleteId, DeleteOperation ] |> Map.ofList }

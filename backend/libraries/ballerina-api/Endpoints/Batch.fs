namespace Ballerina.API

module Batch =
  open Microsoft.AspNetCore.Builder
  open Microsoft.AspNetCore.Routing
  open System
  open Ballerina.DSL.Next.Serialization.PocoObjects
  open Ballerina.DSL.Next.StdLib.Extensions
  open Ballerina.Collections.Sum
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina.DSL.Next.Types
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina
  open APIUtils
  open Ballerina.DSL.Next.Serialization.ValueDeserializer
  open Ballerina.Reader.WithError
  open Ballerina.DSL.Next.Terms.FastEval
  open Ballerina.DSL.Next.Terms.Eval
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Next.Extensions
  open Microsoft.AspNetCore.Http
  open Ballerina.DSL.Next.Serialization.ValueSerializer
  open Ballerina.DSL.Next.Types.TypeChecker.Value
  open Ballerina.DSL.Next.Types.TypeChecker
  open Ballerina.DSL.Next.StdLib.DB
  open Ballerina.Data.Delta.Serialization.DeltaDTO
  open Ballerina.Data.Delta.Serialization.DeltaDeserializer
  open Ballerina.Data.Delta.ToUpdater
  open Ballerina.Data.Delta
  open Ballerina.DSL.Next.StdLib.Updater.Model
  open Npgsql

  /// Individual operation descriptors matching the existing endpoint DTOs.
  [<NoComparison; NoEquality>]
  type BatchCreateOp =
    { EntityName: string
      Id: ValueDTO<ValueExtDTO>
      Entity: ValueDTO<ValueExtDTO> }

  [<NoComparison; NoEquality>]
  type BatchUpdateOp =
    { EntityName: string
      Id: ValueDTO<ValueExtDTO>
      Delta: DeltaDTO<ValueExtDTO, DeltaExtDTO> }

  [<NoComparison; NoEquality>]
  type BatchDeleteOp =
    { EntityName: string
      Id: ValueDTO<ValueExtDTO> }

  [<NoComparison; NoEquality>]
  type BatchLinkOp =
    { RelationName: string
      FromId: ValueDTO<ValueExtDTO>
      ToId: ValueDTO<ValueExtDTO> }

  [<NoComparison; NoEquality>]
  type BatchUnlinkOp =
    { RelationName: string
      FromId: ValueDTO<ValueExtDTO>
      ToId: ValueDTO<ValueExtDTO> }

  [<NoComparison; NoEquality>]
  type BatchMoveBeforeOp =
    { RelationName: string
      FromId: ValueDTO<ValueExtDTO>
      SourceId: ValueDTO<ValueExtDTO>
      TargetId: ValueDTO<ValueExtDTO> }

  [<NoComparison; NoEquality>]
  type BatchMoveAfterOp =
    { RelationName: string
      FromId: ValueDTO<ValueExtDTO>
      SourceId: ValueDTO<ValueExtDTO>
      TargetId: ValueDTO<ValueExtDTO> }

  /// Polymorphic operation descriptor — exactly one field should be non-null.
  /// Serialized as a oneOf union via System.Text.Json.
  [<NoComparison; NoEquality>]
  type BatchOperationDTO =
    { Create: BatchCreateOp option
      Update: BatchUpdateOp option
      Delete: BatchDeleteOp option
      Link: BatchLinkOp option
      Unlink: BatchUnlinkOp option
      MoveBefore: BatchMoveBeforeOp option
      MoveAfter: BatchMoveAfterOp option }

  [<NoComparison; NoEquality>]
  type BatchRequestDTO =
    { Operations: BatchOperationDTO[] }

  /// Per-operation result: either the serialized result value or an error.
  [<NoComparison; NoEquality>]
  type BatchOperationResult =
    { Index: int
      Success: bool
      Result: obj
      Error: string option }

  [<NoComparison; NoEquality>]
  type BatchResponseDTO =
    { Success: bool
      Results: BatchOperationResult[]
      Error: string option }

  let private executeCreateOp<'runtimeContext, 'db, 'customExtension
    when 'customExtension: comparison and 'db: comparison>
    (op: BatchCreateOp)
    (dbio:
      DBIO<
        'runtimeContext,
        'db,
        ValueExt<'runtimeContext, 'db, 'customExtension>
       >)
    (languageContext:
      LanguageContext<
        'runtimeContext,
        ValueExt<'runtimeContext, 'db, 'customExtension>,
        ValueExtDTO,
        DeltaExt<'runtimeContext, 'db, 'customExtension>,
        DeltaExtDTO
       >)
    (evalContext:
      ExprEvalContext<
        'runtimeContext,
        ValueExt<'runtimeContext, 'db, 'customExtension>
       >)
    (typeCheckContext:
      TypeCheckContext<ValueExt<'runtimeContext, 'db, 'customExtension>>)
    (typeCheckState:
      TypeCheckState<ValueExt<'runtimeContext, 'db, 'customExtension>>)
    (httpContext: HttpContext)
    (context:
      APIRegistrationFactory<'runtimeContext, 'db, 'customExtension, Guid, string>)
    : Sum<ValueDTO<ValueExtDTO>, APIError<'runtimeContext, 'db, 'customExtension, Location>> =
    sum {
      let! _tableDescriptor =
        dbio.Schema.Entities
        |> OrderedMap.tryFind (op.EntityName |> SchemaEntityName.Create)
        |> Sum.fromOption (fun () ->
          Errors.Singleton Location.Unknown (fun () ->
            $"Entity {op.EntityName} not found."))
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      let! schema =
        dbio.SchemaAsValue
        |> Value.AsRecord
        |> toUknonwLocation
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      let! entities =
        schema
        |> Map.tryFindWithError
          ("Entities"
           |> Identifier.LocalScope
           |> ResolvedIdentifier.FromIdentifier)
          "schema"
          (fun () -> "Entities")
          Location.Unknown
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      let! entities =
        entities
        |> Value.AsRecord
        |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      let! entityDescriptor =
        entities
        |> Map.tryFindWithError
          (op.EntityName
           |> Identifier.LocalScope
           |> ResolvedIdentifier.FromIdentifier)
          "schema"
          (fun () -> "Entities")
          Location.Unknown
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      let! idValue =
        runDTOConverter languageContext (valueFromDTO op.Id)
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      let! entityValue =
        runDTOConverter languageContext (valueFromDTO op.Entity)
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      do!
        typeCheckValue
          idValue
          _tableDescriptor.Id
          languageContext
          typeCheckContext
          typeCheckState

      do!
        typeCheckValue
          entityValue
          _tableDescriptor.TypeOriginal
          languageContext
          typeCheckContext
          typeCheckState

      let doCreateExpr
        : RunnableExpr<ValueExt<'runtimeContext, 'db, 'customExtension>> =
        RunnableExpr.UnsafeApplyForUntypedEval(
          RunnableExpr.UnsafeApplyForUntypedEval(
            RunnableExpr.UnsafeLookupForUntypedEval(
              Identifier.FullyQualified([ "DB" ], "create")
              |> ResolvedIdentifier.FromIdentifier
            ),
            RunnableExpr.FromValue(
              entityDescriptor,
              TypeValue.CreatePrimitive PrimitiveType.Unit,
              Kind.Star
            )
          ),
          RunnableExpr.UnsafeTupleConsForUntypedEval
            [ RunnableExpr.FromValue(
                idValue,
                TypeValue.CreatePrimitive PrimitiveType.Unit,
                Kind.Star
              )
              RunnableExpr.FromValue(
                entityValue,
                TypeValue.CreatePrimitive PrimitiveType.Unit,
                Kind.Star
              ) ]
        )

      let! evalResult =
        Expr.Eval(
          NonEmptyList.prependList
            languageContext.TypeCheckedPreludes
            (NonEmptyList.OfList(doCreateExpr, []))
        )
        |> Reader.Run(
          evalContext |> context.PermissionHookInjector httpContext
        )
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      return!
        runDTOConverter languageContext (valueToDTO evalResult)
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create
    }

  let private executeUpdateOp<'runtimeContext, 'db, 'customExtension
    when 'customExtension: comparison and 'db: comparison>
    (op: BatchUpdateOp)
    (dbio:
      DBIO<
        'runtimeContext,
        'db,
        ValueExt<'runtimeContext, 'db, 'customExtension>
       >)
    (languageContext:
      LanguageContext<
        'runtimeContext,
        ValueExt<'runtimeContext, 'db, 'customExtension>,
        ValueExtDTO,
        DeltaExt<'runtimeContext, 'db, 'customExtension>,
        DeltaExtDTO
       >)
    (evalContext:
      ExprEvalContext<
        'runtimeContext,
        ValueExt<'runtimeContext, 'db, 'customExtension>
       >)
    (typeCheckContext:
      TypeCheckContext<ValueExt<'runtimeContext, 'db, 'customExtension>>)
    (typeCheckState:
      TypeCheckState<ValueExt<'runtimeContext, 'db, 'customExtension>>)
    (httpContext: HttpContext)
    (context:
      APIRegistrationFactory<'runtimeContext, 'db, 'customExtension, Guid, string>)
    : Sum<ValueDTO<ValueExtDTO>, APIError<'runtimeContext, 'db, 'customExtension, Location>> =
    sum {
      let! _tableDescriptor =
        dbio.Schema.Entities
        |> OrderedMap.tryFind (op.EntityName |> SchemaEntityName.Create)
        |> Sum.fromOption (fun () ->
          Errors.Singleton Location.Unknown (fun () ->
            $"Entity {op.EntityName} not found."))
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      let! schema =
        dbio.SchemaAsValue
        |> Value.AsRecord
        |> toUknonwLocation
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      let! entities =
        schema
        |> Map.tryFindWithError
          ("Entities"
           |> Identifier.LocalScope
           |> ResolvedIdentifier.FromIdentifier)
          "schema"
          (fun () -> "Entities")
          Location.Unknown
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      let! entities =
        entities
        |> Value.AsRecord
        |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      let! entityDescriptor =
        entities
        |> Map.tryFindWithError
          (op.EntityName
           |> Identifier.LocalScope
           |> ResolvedIdentifier.FromIdentifier)
          "schema"
          (fun () -> "Entities")
          Location.Unknown
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      let! idValue =
        runDTOConverter languageContext (valueFromDTO op.Id)
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      do!
        typeCheckValue
          idValue
          _tableDescriptor.Id
          languageContext
          typeCheckContext
          typeCheckState

      let! delta =
        deltaFromDTO op.Delta
        |> Reader.Run languageContext.SerializationContext
        |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      let! updaterLambda =
        createUpdaterFromDelta delta
        |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      let doUpdateExpr
        : RunnableExpr<ValueExt<'runtimeContext, 'db, 'customExtension>> =
        RunnableExpr.UnsafeApplyForUntypedEval(
          RunnableExpr.UnsafeApplyForUntypedEval(
            RunnableExpr.UnsafeLookupForUntypedEval(
              Identifier.FullyQualified([ "DB" ], "update")
              |> ResolvedIdentifier.FromIdentifier
            ),
            RunnableExpr.FromValue(
              entityDescriptor,
              TypeValue.CreatePrimitive PrimitiveType.Unit,
              Kind.Star
            )
          ),
          RunnableExpr.UnsafeTupleConsForUntypedEval
            [ RunnableExpr.FromValue(
                idValue,
                TypeValue.CreatePrimitive PrimitiveType.Unit,
                Kind.Star
              )
              RunnableExpr.UnsafeLambdaForUntypedEval(
                Var.Create "_",
                TypeValue.CreatePrimitive PrimitiveType.Unit,
                updaterLambda,
                TypeValue.CreatePrimitive PrimitiveType.Unit
              ) ]
        )

      let! evalResult =
        Expr.Eval(
          NonEmptyList.prependList
            languageContext.TypeCheckedPreludes
            (NonEmptyList.OfList(doUpdateExpr, []))
        )
        |> Reader.Run(
          evalContext |> context.PermissionHookInjector httpContext
        )
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      return!
        runDTOConverter languageContext (valueToDTO evalResult)
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create
    }

  let private executeDeleteOp<'runtimeContext, 'db, 'customExtension
    when 'customExtension: comparison and 'db: comparison>
    (op: BatchDeleteOp)
    (dbio:
      DBIO<
        'runtimeContext,
        'db,
        ValueExt<'runtimeContext, 'db, 'customExtension>
       >)
    (languageContext:
      LanguageContext<
        'runtimeContext,
        ValueExt<'runtimeContext, 'db, 'customExtension>,
        ValueExtDTO,
        DeltaExt<'runtimeContext, 'db, 'customExtension>,
        DeltaExtDTO
       >)
    (evalContext:
      ExprEvalContext<
        'runtimeContext,
        ValueExt<'runtimeContext, 'db, 'customExtension>
       >)
    (typeCheckContext:
      TypeCheckContext<ValueExt<'runtimeContext, 'db, 'customExtension>>)
    (typeCheckState:
      TypeCheckState<ValueExt<'runtimeContext, 'db, 'customExtension>>)
    (httpContext: HttpContext)
    (context:
      APIRegistrationFactory<'runtimeContext, 'db, 'customExtension, Guid, string>)
    : Sum<ValueDTO<ValueExtDTO>, APIError<'runtimeContext, 'db, 'customExtension, Location>> =
    sum {
      let! _tableDescriptor =
        dbio.Schema.Entities
        |> OrderedMap.tryFind (op.EntityName |> SchemaEntityName.Create)
        |> Sum.fromOption (fun () ->
          Errors.Singleton Location.Unknown (fun () ->
            $"Entity {op.EntityName} not found."))
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      let! schema =
        dbio.SchemaAsValue
        |> Value.AsRecord
        |> toUknonwLocation
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      let! entities =
        schema
        |> Map.tryFindWithError
          ("Entities"
           |> Identifier.LocalScope
           |> ResolvedIdentifier.FromIdentifier)
          "schema"
          (fun () -> "Entities")
          Location.Unknown
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      let! entities =
        entities
        |> Value.AsRecord
        |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      let! entityDescriptor =
        entities
        |> Map.tryFindWithError
          (op.EntityName
           |> Identifier.LocalScope
           |> ResolvedIdentifier.FromIdentifier)
          "schema"
          (fun () -> "Entities")
          Location.Unknown
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      let! idValue =
        runDTOConverter languageContext (valueFromDTO op.Id)
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      do!
        typeCheckValue
          idValue
          _tableDescriptor.Id
          languageContext
          typeCheckContext
          typeCheckState

      let doDeleteExpr
        : RunnableExpr<ValueExt<'runtimeContext, 'db, 'customExtension>> =
        RunnableExpr.UnsafeApplyForUntypedEval(
          RunnableExpr.UnsafeApplyForUntypedEval(
            RunnableExpr.UnsafeLookupForUntypedEval(
              Identifier.FullyQualified([ "DB" ], "delete")
              |> ResolvedIdentifier.FromIdentifier
            ),
            RunnableExpr.FromValue(
              entityDescriptor,
              TypeValue.CreatePrimitive PrimitiveType.Unit,
              Kind.Star
            )
          ),
          RunnableExpr.FromValue(
            idValue,
            TypeValue.CreatePrimitive PrimitiveType.Unit,
            Kind.Star
          )
        )

      let! evalResult =
        Expr.Eval(
          NonEmptyList.prependList
            languageContext.TypeCheckedPreludes
            (NonEmptyList.OfList(doDeleteExpr, []))
        )
        |> Reader.Run(
          evalContext |> context.PermissionHookInjector httpContext
        )
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      return!
        runDTOConverter languageContext (valueToDTO evalResult)
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create
    }

  let private executeLinkOp<'runtimeContext, 'db, 'customExtension
    when 'customExtension: comparison and 'db: comparison>
    (op: BatchLinkOp)
    (dbOp: string)
    (dbio:
      DBIO<
        'runtimeContext,
        'db,
        ValueExt<'runtimeContext, 'db, 'customExtension>
       >)
    (languageContext:
      LanguageContext<
        'runtimeContext,
        ValueExt<'runtimeContext, 'db, 'customExtension>,
        ValueExtDTO,
        DeltaExt<'runtimeContext, 'db, 'customExtension>,
        DeltaExtDTO
       >)
    (evalContext:
      ExprEvalContext<
        'runtimeContext,
        ValueExt<'runtimeContext, 'db, 'customExtension>
       >)
    (typeCheckContext:
      TypeCheckContext<ValueExt<'runtimeContext, 'db, 'customExtension>>)
    (typeCheckState:
      TypeCheckState<ValueExt<'runtimeContext, 'db, 'customExtension>>)
    (httpContext: HttpContext)
    (context:
      APIRegistrationFactory<'runtimeContext, 'db, 'customExtension, Guid, string>)
    : Sum<ValueDTO<ValueExtDTO>, APIError<'runtimeContext, 'db, 'customExtension, Location>> =
    sum {
      let! _tableDescriptor =
        dbio.Schema.Relations
        |> OrderedMap.tryFind (op.RelationName |> SchemaRelationName.Create)
        |> Sum.fromOption (fun () ->
          Errors<Location>.Singleton Location.Unknown (fun () ->
            $"Relation {op.RelationName} not found."))
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      let! fromIdValue =
        runDTOConverter languageContext (valueFromDTO op.FromId)
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      let! toIdValue =
        runDTOConverter languageContext (valueFromDTO op.ToId)
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      let fromName = _tableDescriptor.From.ToString()
      let toName = _tableDescriptor.To.ToString()

      let! _fromDescriptor =
        dbio.Schema.Entities
        |> OrderedMap.tryFind (fromName |> SchemaEntityName.Create)
        |> Sum.fromOption (fun () ->
          Errors.Singleton Location.Unknown (fun () ->
            $"Entity {fromName} not found."))
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      let! _toDescriptor =
        dbio.Schema.Entities
        |> OrderedMap.tryFind (toName |> SchemaEntityName.Create)
        |> Sum.fromOption (fun () ->
          Errors.Singleton Location.Unknown (fun () ->
            $"Entity {toName} not found."))
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      do!
        typeCheckValue
          fromIdValue
          _fromDescriptor.Id
          languageContext
          typeCheckContext
          typeCheckState

      do!
        typeCheckValue
          toIdValue
          _toDescriptor.Id
          languageContext
          typeCheckContext
          typeCheckState

      let! schema =
        dbio.SchemaAsValue
        |> Value.AsRecord
        |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      let! relations =
        schema
        |> Map.tryFindWithError
          ("Relations"
           |> Identifier.LocalScope
           |> ResolvedIdentifier.FromIdentifier)
          "schema"
          (fun () -> "Relations")
          Location.Unknown
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      let! relations =
        relations
        |> Value.AsRecord
        |> sum.MapError(
          Errors.MapContext(replaceWith Location.Unknown)
          >> APIError<'runtimeContext, 'db, 'customExtension, Location>.Create
        )

      let! relationDescriptor =
        relations
        |> Map.tryFindWithError
          (op.RelationName
           |> Identifier.LocalScope
           |> ResolvedIdentifier.FromIdentifier)
          "schema"
          (fun () -> op.RelationName)
          Location.Unknown
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      let doLinkExpr
        : RunnableExpr<ValueExt<'runtimeContext, 'db, 'customExtension>> =
        RunnableExpr.UnsafeApplyForUntypedEval(
          RunnableExpr.UnsafeApplyForUntypedEval(
            RunnableExpr.UnsafeLookupForUntypedEval(
              Identifier.FullyQualified([ "DB" ], dbOp)
              |> ResolvedIdentifier.FromIdentifier
            ),
            RunnableExpr.FromValue(
              relationDescriptor,
              TypeValue.CreatePrimitive PrimitiveType.Unit,
              Kind.Star
            )
          ),
          RunnableExpr.UnsafeTupleConsForUntypedEval
            [ RunnableExpr.FromValue(
                fromIdValue,
                TypeValue.CreatePrimitive PrimitiveType.Unit,
                Kind.Star
              )
              RunnableExpr.FromValue(
                toIdValue,
                TypeValue.CreatePrimitive PrimitiveType.Unit,
                Kind.Star
              ) ]
        )

      let! evalResult =
        Expr.Eval(
          NonEmptyList.prependList
            languageContext.TypeCheckedPreludes
            (NonEmptyList.OfList(doLinkExpr, []))
        )
        |> Reader.Run(
          evalContext |> context.PermissionHookInjector httpContext
        )
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      return!
        runDTOConverter languageContext (valueToDTO evalResult)
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create
    }

  let private executeMoveOp<'runtimeContext, 'db, 'customExtension
    when 'customExtension: comparison and 'db: comparison>
    (relationName: string)
    (fromId: ValueDTO<ValueExtDTO>)
    (sourceId: ValueDTO<ValueExtDTO>)
    (targetId: ValueDTO<ValueExtDTO>)
    (dbOp: string)
    (dbio:
      DBIO<
        'runtimeContext,
        'db,
        ValueExt<'runtimeContext, 'db, 'customExtension>
       >)
    (languageContext:
      LanguageContext<
        'runtimeContext,
        ValueExt<'runtimeContext, 'db, 'customExtension>,
        ValueExtDTO,
        DeltaExt<'runtimeContext, 'db, 'customExtension>,
        DeltaExtDTO
       >)
    (evalContext:
      ExprEvalContext<
        'runtimeContext,
        ValueExt<'runtimeContext, 'db, 'customExtension>
       >)
    (_typeCheckContext:
      TypeCheckContext<ValueExt<'runtimeContext, 'db, 'customExtension>>)
    (_typeCheckState:
      TypeCheckState<ValueExt<'runtimeContext, 'db, 'customExtension>>)
    (httpContext: HttpContext)
    (context:
      APIRegistrationFactory<'runtimeContext, 'db, 'customExtension, Guid, string>)
    : Sum<ValueDTO<ValueExtDTO>, APIError<'runtimeContext, 'db, 'customExtension, Location>> =
    sum {
      let! _tableDescriptor =
        dbio.Schema.Relations
        |> OrderedMap.tryFind (relationName |> SchemaRelationName.Create)
        |> Sum.fromOption (fun () ->
          Errors<Location>.Singleton Location.Unknown (fun () ->
            $"Relation {relationName} not found."))
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      let! fromIdValue =
        runDTOConverter languageContext (valueFromDTO fromId)
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      let! sourceIdValue =
        runDTOConverter languageContext (valueFromDTO sourceId)
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      let! targetIdValue =
        runDTOConverter languageContext (valueFromDTO targetId)
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      let! schema =
        dbio.SchemaAsValue
        |> Value.AsRecord
        |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      let! relations =
        schema
        |> Map.tryFindWithError
          ("Relations"
           |> Identifier.LocalScope
           |> ResolvedIdentifier.FromIdentifier)
          "schema"
          (fun () -> "Relations")
          Location.Unknown
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      let! relations =
        relations
        |> Value.AsRecord
        |> sum.MapError(
          Errors.MapContext(replaceWith Location.Unknown)
          >> APIError<'runtimeContext, 'db, 'customExtension, Location>.Create
        )

      let! relationDescriptor =
        relations
        |> Map.tryFindWithError
          (relationName
           |> Identifier.LocalScope
           |> ResolvedIdentifier.FromIdentifier)
          "schema"
          (fun () -> relationName)
          Location.Unknown
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      let doMoveExpr
        : RunnableExpr<ValueExt<'runtimeContext, 'db, 'customExtension>> =
        RunnableExpr.UnsafeApplyForUntypedEval(
          RunnableExpr.UnsafeApplyForUntypedEval(
            RunnableExpr.UnsafeLookupForUntypedEval(
              Identifier.FullyQualified([ "DB" ], dbOp)
              |> ResolvedIdentifier.FromIdentifier
            ),
            RunnableExpr.FromValue(
              relationDescriptor,
              TypeValue.CreatePrimitive PrimitiveType.Unit,
              Kind.Star
            )
          ),
          RunnableExpr.UnsafeTupleConsForUntypedEval
            [ RunnableExpr.FromValue(
                fromIdValue,
                TypeValue.CreatePrimitive PrimitiveType.Unit,
                Kind.Star
              )
              RunnableExpr.FromValue(
                sourceIdValue,
                TypeValue.CreatePrimitive PrimitiveType.Unit,
                Kind.Star
              )
              RunnableExpr.FromValue(
                targetIdValue,
                TypeValue.CreatePrimitive PrimitiveType.Unit,
                Kind.Star
              ) ]
        )

      let! evalResult =
        Expr.Eval(
          NonEmptyList.prependList
            languageContext.TypeCheckedPreludes
            (NonEmptyList.OfList(doMoveExpr, []))
        )
        |> Reader.Run(
          evalContext |> context.PermissionHookInjector httpContext
        )
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

      return!
        runDTOConverter languageContext (valueToDTO evalResult)
        |> sum.MapError
          APIError<'runtimeContext, 'db, 'customExtension, Location>.Create
    }

  let private executeOneOp<'runtimeContext, 'db, 'customExtension
    when 'customExtension: comparison and 'db: comparison>
    (op: BatchOperationDTO)
    (dbio:
      DBIO<
        'runtimeContext,
        'db,
        ValueExt<'runtimeContext, 'db, 'customExtension>
       >)
    (languageContext:
      LanguageContext<
        'runtimeContext,
        ValueExt<'runtimeContext, 'db, 'customExtension>,
        ValueExtDTO,
        DeltaExt<'runtimeContext, 'db, 'customExtension>,
        DeltaExtDTO
       >)
    (evalContext:
      ExprEvalContext<
        'runtimeContext,
        ValueExt<'runtimeContext, 'db, 'customExtension>
       >)
    (typeCheckContext:
      TypeCheckContext<ValueExt<'runtimeContext, 'db, 'customExtension>>)
    (typeCheckState:
      TypeCheckState<ValueExt<'runtimeContext, 'db, 'customExtension>>)
    (httpContext: HttpContext)
    (context:
      APIRegistrationFactory<'runtimeContext, 'db, 'customExtension, Guid, string>)
    : Sum<ValueDTO<ValueExtDTO>, APIError<'runtimeContext, 'db, 'customExtension, Location>> =
    match op with
    | { Create = Some createOp } ->
      executeCreateOp createOp dbio languageContext evalContext typeCheckContext typeCheckState httpContext context
    | { Update = Some updateOp } ->
      executeUpdateOp updateOp dbio languageContext evalContext typeCheckContext typeCheckState httpContext context
    | { Delete = Some deleteOp } ->
      executeDeleteOp deleteOp dbio languageContext evalContext typeCheckContext typeCheckState httpContext context
    | { Link = Some linkOp } ->
      executeLinkOp linkOp "link" dbio languageContext evalContext typeCheckContext typeCheckState httpContext context
    | { Unlink = Some unlinkOp } ->
      executeLinkOp
        { BatchLinkOp.RelationName = unlinkOp.RelationName
          FromId = unlinkOp.FromId
          ToId = unlinkOp.ToId }
        "unlink"
        dbio languageContext evalContext typeCheckContext typeCheckState httpContext context
    | { MoveBefore = Some moveOp } ->
      executeMoveOp
        moveOp.RelationName moveOp.FromId moveOp.SourceId moveOp.TargetId
        "move-before"
        dbio languageContext evalContext typeCheckContext typeCheckState httpContext context
    | { MoveAfter = Some moveOp } ->
      executeMoveOp
        moveOp.RelationName moveOp.FromId moveOp.SourceId moveOp.TargetId
        "move-after"
        dbio languageContext evalContext typeCheckContext typeCheckState httpContext context
    | _ ->
      sum.Throw(
        APIError<'runtimeContext, 'db, 'customExtension, Location>.Create(
          Errors.Singleton Location.Unknown (fun () ->
            "Invalid batch operation: no operation field is set.")
        )
      )

  let batch<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName
    when 'customExtension: comparison and 'db: comparison>
    (_app: IEndpointRouteBuilder)
    (_context:
      APIRegistrationFactory<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName>)
    =
    ()

  /// Concrete batch registration for the standard Guid/string tenant/schema pattern.
  let batchConcrete<'runtimeContext, 'db, 'customExtension
    when 'customExtension: comparison and 'db: comparison>
    (app: IEndpointRouteBuilder)
    (context:
      APIRegistrationFactory<'runtimeContext, 'db, 'customExtension, Guid, string>)
    =
    app.MapPost(
      "/{tenantId}/{schemaName}/batch",
      Func<HttpContext, Guid, string, BatchRequestDTO, IResult>
        (fun httpContext tenantId schemaName payload ->
          let txn: NpgsqlTransaction option ref = ref None
          let conn: NpgsqlConnection option ref = ref None

          let cleanup () =
            txn.Value |> Option.iter (fun t -> try t.Rollback() with _ -> ())
            conn.Value |> Option.iter (fun c -> try c.Dispose() with _ -> ())
            txn.Value <- None
            conn.Value <- None

          try
            let descriptorResult =
              getDbDescriptor tenantId schemaName context

            match descriptorResult with
            | Right apiError ->
              cleanup ()
              let errorMessages =
                apiError.Errors.Errors()
                |> NonEmptyList.ToList
                |> List.map (fun e -> e.Message)
                |> String.concat "; "
              Results.BadRequest(
                { BatchResponseDTO.Success = false
                  Results = [||]
                  Error = Some errorMessages }
              )
            | Left (dbio, languageContext, evalContext, typeCheckContext, typeCheckState, dataSource) ->

            // Open a single transaction for all operations
            match dataSource with
            | Some ds ->
              let c = ds.OpenConnection()
              let t = c.BeginTransaction()
              conn.Value <- Some c
              txn.Value <- Some t
            | None -> ()

            let mutable results: ValueDTO<ValueExtDTO> list = []
            let mutable error: string option = None

            for op in payload.Operations do
              if error.IsNone then
                let opResult =
                  executeOneOp
                    op
                    dbio
                    languageContext
                    evalContext
                    typeCheckContext
                    typeCheckState
                    httpContext
                    context

                match opResult with
                | Left resultDto ->
                  results <- results @ [ resultDto ]
                | Right apiErr ->
                  error <-
                    Some(
                      apiErr.Errors.Errors()
                      |> NonEmptyList.ToList
                      |> List.map (fun e -> e.Message)
                      |> String.concat "; "
                    )

            match error with
            | Some errMsg ->
              // Rollback on failure
              cleanup ()
              Results.BadRequest(
                { BatchResponseDTO.Success = false
                  Results = [||]
                  Error = Some errMsg }
              )
            | None ->
              // Commit on success
              txn.Value |> Option.iter (fun t -> t.Commit())
              conn.Value |> Option.iter (fun c -> c.Dispose())
              txn.Value <- None
              conn.Value <- None

              Results.Ok(
                { BatchResponseDTO.Success = true
                  Results =
                    results
                    |> List.mapi (fun i r ->
                      { Index = i
                        Success = true
                        Result = r
                        Error = None })
                    |> Array.ofList
                  Error = None }
              )
          with ex ->
            cleanup ()
            reraise ()
        )
    )
    |> ignore

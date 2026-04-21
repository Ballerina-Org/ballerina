namespace Ballerina.DSL.Next.StdLib.Coroutine

module Extension =
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Extensions
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.Reader.WithError
  open Ballerina.Lenses

  type CoroutineOperations =
    | Co_Show
    | Co_Until
    | Co_Ignore
    | Co_MapContext
    | Co_MapState
    | Co_GetContext
    | Co_GetState
    | Co_SetState

  let CoroutineExtension<'runtimeContext, 'ext, 'extDTO, 'deltaExt, 'deltaExtDTO
    when 'ext: comparison
    and 'extDTO: not null
    and 'extDTO: not struct
    and 'deltaExtDTO: not null
    and 'deltaExtDTO: not struct>
    (operationLens: PartialLens<'ext, CoroutineOperations>)
    (typeSymbol: Option<TypeSymbol>)
    (viewTypeId: Identifier)
    : TypeExtension<
        'runtimeContext,
        'ext,
        'extDTO,
        'deltaExt,
        'deltaExtDTO,
        Unit,
        Unit,
        CoroutineOperations
       > *
      TypeSymbol *
      (TypeValue<'ext>
        -> TypeValue<'ext>
        -> TypeValue<'ext>
        -> TypeValue<'ext>
        -> TypeValue<'ext>)
    =
    let coId = Identifier.LocalScope "Co"

    let coSymbolId =
      typeSymbol |> Option.defaultWith (fun () -> coId |> TypeSymbol.Create)

    let coResolvedId = coId |> TypeCheckScope.Empty.Resolve

    let schemaVar, schemaKind = TypeVar.Create("schema"), Kind.Schema
    let ctxVar, ctxKind = TypeVar.Create("context"), Kind.Star
    let stVar, stKind = TypeVar.Create("state"), Kind.Star
    let resVar, resKind = TypeVar.Create("result"), Kind.Star

    let make_coType
      (schema: TypeValue<'ext>)
      (ctx: TypeValue<'ext>)
      (st: TypeValue<'ext>)
      (res: TypeValue<'ext>)
      =
      TypeValue.Imported
        { Id = coResolvedId
          Sym = coSymbolId
          Parameters = []
          Arguments = [ schema; ctx; st; res ] }

    // --- Co::show ---
    // show : Λschema::Schema. Λctx::*. Λst::*. Λo::*.
    //   (st -> () + o) -> Frontend::View[schema][ctx][st] -> Co[schema][ctx][st][o]
    let showId =
      Identifier.FullyQualified([ "Co" ], "show")
      |> TypeCheckScope.Empty.Resolve

    let _oVar, oKind = TypeVar.Create("o"), Kind.Star

    let showOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'runtimeContext, 'ext, Unit, Unit, CoroutineOperations> =
      showId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("schema", schemaKind),
            TypeExpr.Lambda(
              TypeParameter.Create("ctx", ctxKind),
              TypeExpr.Lambda(
                TypeParameter.Create("st", stKind),
                TypeExpr.Lambda(
                  TypeParameter.Create("o", oKind),
                  TypeExpr.Arrow(
                    TypeExpr.Arrow(
                      TypeExpr.Lookup(Identifier.LocalScope "st"),
                      TypeExpr.Sum
                        [ TypeExpr.Primitive PrimitiveType.Unit
                          TypeExpr.Lookup(Identifier.LocalScope "o") ]
                    ),
                    TypeExpr.Arrow(
                      TypeExpr.Apply(
                        TypeExpr.Apply(
                          TypeExpr.Apply(
                            TypeExpr.Lookup viewTypeId,
                            TypeExpr.Lookup(Identifier.LocalScope "schema")
                          ),
                          TypeExpr.Lookup(Identifier.LocalScope "ctx")
                        ),
                        TypeExpr.Lookup(Identifier.LocalScope "st")
                      ),
                      TypeExpr.Apply(
                        TypeExpr.Apply(
                          TypeExpr.Apply(
                            TypeExpr.Apply(
                              TypeExpr.Lookup(Identifier.LocalScope "Co"),
                              TypeExpr.Lookup(Identifier.LocalScope "schema")
                            ),
                            TypeExpr.Lookup(Identifier.LocalScope "ctx")
                          ),
                          TypeExpr.Lookup(Identifier.LocalScope "st")
                        ),
                        TypeExpr.Lookup(Identifier.LocalScope "o")
                      )
                    )
                  )
                )
              )
            )
          )
        Kind =
          Kind.Arrow(
            Kind.Schema,
            Kind.Arrow(
              Kind.Star,
              Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))
            )
          )
        Operation = Co_Show
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Co_Show -> Some Co_Show
            | _ -> None)
        Apply =
          fun loc0 _rest (_op, _v) ->
            reader {
              return!
                Errors.Singleton loc0 (fun () -> "Co::show is not yet implemented")
                |> reader.Throw
            } }

    // --- Co::until ---
    // until : Λschema::Schema. Λctx::*. Λst::*. Λres::*.
    //   (st -> () + res) -> Co[schema][ctx][st][()] -> Co[schema][ctx][st][res]
    let untilId =
      Identifier.FullyQualified([ "Co" ], "until")
      |> TypeCheckScope.Empty.Resolve

    let _res2Var, res2Kind = TypeVar.Create("res2"), Kind.Star

    let untilOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'runtimeContext, 'ext, Unit, Unit, CoroutineOperations> =
      untilId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("schema", schemaKind),
            TypeExpr.Lambda(
              TypeParameter.Create("ctx", ctxKind),
              TypeExpr.Lambda(
                TypeParameter.Create("st", stKind),
                TypeExpr.Lambda(
                  TypeParameter.Create("res2", res2Kind),
                  TypeExpr.Arrow(
                    TypeExpr.Arrow(
                      TypeExpr.Lookup(Identifier.LocalScope "st"),
                      TypeExpr.Sum
                        [ TypeExpr.Primitive PrimitiveType.Unit
                          TypeExpr.Lookup(Identifier.LocalScope "res2") ]
                    ),
                    TypeExpr.Arrow(
                      TypeExpr.Apply(
                        TypeExpr.Apply(
                          TypeExpr.Apply(
                            TypeExpr.Apply(
                              TypeExpr.Lookup(Identifier.LocalScope "Co"),
                              TypeExpr.Lookup(Identifier.LocalScope "schema")
                            ),
                            TypeExpr.Lookup(Identifier.LocalScope "ctx")
                          ),
                          TypeExpr.Lookup(Identifier.LocalScope "st")
                        ),
                        TypeExpr.Primitive PrimitiveType.Unit
                      ),
                      TypeExpr.Apply(
                        TypeExpr.Apply(
                          TypeExpr.Apply(
                            TypeExpr.Apply(
                              TypeExpr.Lookup(Identifier.LocalScope "Co"),
                              TypeExpr.Lookup(Identifier.LocalScope "schema")
                            ),
                            TypeExpr.Lookup(Identifier.LocalScope "ctx")
                          ),
                          TypeExpr.Lookup(Identifier.LocalScope "st")
                        ),
                        TypeExpr.Lookup(Identifier.LocalScope "res2")
                      )
                    )
                  )
                )
              )
            )
          )
        Kind =
          Kind.Arrow(
            Kind.Schema,
            Kind.Arrow(
              Kind.Star,
              Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))
            )
          )
        Operation = Co_Until
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Co_Until -> Some Co_Until
            | _ -> None)
        Apply =
          fun loc0 _rest (_op, _v) ->
            reader {
              return!
                Errors.Singleton loc0 (fun () -> "Co::until is not yet implemented")
                |> reader.Throw
            } }

    // --- Co::ignore ---
    // ignore : Λschema::Schema. Λctx::*. Λst::*. Λres::*.
    //   Co[schema][ctx][st][res] -> Co[schema][ctx][st][()]
    let ignoreId =
      Identifier.FullyQualified([ "Co" ], "ignore")
      |> TypeCheckScope.Empty.Resolve

    let ignoreOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'runtimeContext, 'ext, Unit, Unit, CoroutineOperations> =
      ignoreId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("schema", schemaKind),
            TypeExpr.Lambda(
              TypeParameter.Create("ctx", ctxKind),
              TypeExpr.Lambda(
                TypeParameter.Create("st", stKind),
                TypeExpr.Lambda(
                  TypeParameter.Create("res", resKind),
                  TypeExpr.Arrow(
                    TypeExpr.Apply(
                      TypeExpr.Apply(
                        TypeExpr.Apply(
                          TypeExpr.Apply(
                            TypeExpr.Lookup(Identifier.LocalScope "Co"),
                            TypeExpr.Lookup(Identifier.LocalScope "schema")
                          ),
                          TypeExpr.Lookup(Identifier.LocalScope "ctx")
                        ),
                        TypeExpr.Lookup(Identifier.LocalScope "st")
                      ),
                      TypeExpr.Lookup(Identifier.LocalScope "res")
                    ),
                    TypeExpr.Apply(
                      TypeExpr.Apply(
                        TypeExpr.Apply(
                          TypeExpr.Apply(
                            TypeExpr.Lookup(Identifier.LocalScope "Co"),
                            TypeExpr.Lookup(Identifier.LocalScope "schema")
                          ),
                          TypeExpr.Lookup(Identifier.LocalScope "ctx")
                        ),
                        TypeExpr.Lookup(Identifier.LocalScope "st")
                      ),
                      TypeExpr.Primitive PrimitiveType.Unit
                    )
                  )
                )
              )
            )
          )
        Kind =
          Kind.Arrow(
            Kind.Schema,
            Kind.Arrow(
              Kind.Star,
              Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))
            )
          )
        Operation = Co_Ignore
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Co_Ignore -> Some Co_Ignore
            | _ -> None)
        Apply =
          fun loc0 _rest (_op, _v) ->
            reader {
              return!
                Errors.Singleton loc0 (fun () -> "Co::ignore is not yet implemented")
                |> reader.Throw
            } }

    // --- Co::mapContext ---
    // mapContext : Λschema::Schema. Λctx::*. Λctx2::*. Λst::*. Λo::*.
    //   (ctx2 -> ctx) -> Co[schema][ctx][st][o] -> Co[schema][ctx2][st][o]
    let mapContextId =
      Identifier.FullyQualified([ "Co" ], "mapContext")
      |> TypeCheckScope.Empty.Resolve

    let _ctx2Var, ctx2Kind = TypeVar.Create("ctx2"), Kind.Star

    let mapContextOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'runtimeContext, 'ext, Unit, Unit, CoroutineOperations> =
      mapContextId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("schema", schemaKind),
            TypeExpr.Lambda(
              TypeParameter.Create("ctx", ctxKind),
              TypeExpr.Lambda(
                TypeParameter.Create("ctx2", ctx2Kind),
                TypeExpr.Lambda(
                  TypeParameter.Create("st", stKind),
                  TypeExpr.Lambda(
                    TypeParameter.Create("o", oKind),
                    TypeExpr.Arrow(
                      TypeExpr.Arrow(
                        TypeExpr.Lookup(Identifier.LocalScope "ctx2"),
                        TypeExpr.Lookup(Identifier.LocalScope "ctx")
                      ),
                      TypeExpr.Arrow(
                        TypeExpr.Apply(
                          TypeExpr.Apply(
                            TypeExpr.Apply(
                              TypeExpr.Apply(
                                TypeExpr.Lookup(Identifier.LocalScope "Co"),
                                TypeExpr.Lookup(Identifier.LocalScope "schema")
                              ),
                              TypeExpr.Lookup(Identifier.LocalScope "ctx")
                            ),
                            TypeExpr.Lookup(Identifier.LocalScope "st")
                          ),
                          TypeExpr.Lookup(Identifier.LocalScope "o")
                        ),
                        TypeExpr.Apply(
                          TypeExpr.Apply(
                            TypeExpr.Apply(
                              TypeExpr.Apply(
                                TypeExpr.Lookup(Identifier.LocalScope "Co"),
                                TypeExpr.Lookup(Identifier.LocalScope "schema")
                              ),
                              TypeExpr.Lookup(Identifier.LocalScope "ctx2")
                            ),
                            TypeExpr.Lookup(Identifier.LocalScope "st")
                          ),
                          TypeExpr.Lookup(Identifier.LocalScope "o")
                        )
                      )
                    )
                  )
                )
              )
            )
          )
        Kind =
          Kind.Arrow(
            Kind.Schema,
            Kind.Arrow(
              Kind.Star,
              Kind.Arrow(
                Kind.Star,
                Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))
              )
            )
          )
        Operation = Co_MapContext
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Co_MapContext -> Some Co_MapContext
            | _ -> None)
        Apply =
          fun loc0 _rest (_op, _v) ->
            reader {
              return!
                Errors.Singleton loc0 (fun () -> "Co::mapContext is not yet implemented")
                |> reader.Throw
            } }

    // --- Co::mapState ---
    // mapState : Λschema::Schema. Λctx::*. Λst::*. Λst2::*. Λo::*.
    //   (st2 -> st) -> ((st -> st) -> (st2 -> st2)) -> Co[schema][ctx][st][o] -> Co[schema][ctx][st2][o]
    let mapStateId =
      Identifier.FullyQualified([ "Co" ], "mapState")
      |> TypeCheckScope.Empty.Resolve

    let _st2Var, st2Kind = TypeVar.Create("st2"), Kind.Star

    let mapStateOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'runtimeContext, 'ext, Unit, Unit, CoroutineOperations> =
      mapStateId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("schema", schemaKind),
            TypeExpr.Lambda(
              TypeParameter.Create("ctx", ctxKind),
              TypeExpr.Lambda(
                TypeParameter.Create("st", stKind),
                TypeExpr.Lambda(
                  TypeParameter.Create("st2", st2Kind),
                  TypeExpr.Lambda(
                    TypeParameter.Create("o", oKind),
                    TypeExpr.Arrow(
                      TypeExpr.Arrow(
                        TypeExpr.Lookup(Identifier.LocalScope "st2"),
                        TypeExpr.Lookup(Identifier.LocalScope "st")
                      ),
                      TypeExpr.Arrow(
                        TypeExpr.Arrow(
                          TypeExpr.Arrow(
                            TypeExpr.Lookup(Identifier.LocalScope "st"),
                            TypeExpr.Lookup(Identifier.LocalScope "st")
                          ),
                          TypeExpr.Arrow(
                            TypeExpr.Lookup(Identifier.LocalScope "st2"),
                            TypeExpr.Lookup(Identifier.LocalScope "st2")
                          )
                        ),
                        TypeExpr.Arrow(
                          TypeExpr.Apply(
                            TypeExpr.Apply(
                              TypeExpr.Apply(
                                TypeExpr.Apply(
                                  TypeExpr.Lookup(Identifier.LocalScope "Co"),
                                  TypeExpr.Lookup(Identifier.LocalScope "schema")
                                ),
                                TypeExpr.Lookup(Identifier.LocalScope "ctx")
                              ),
                              TypeExpr.Lookup(Identifier.LocalScope "st")
                            ),
                            TypeExpr.Lookup(Identifier.LocalScope "o")
                          ),
                          TypeExpr.Apply(
                            TypeExpr.Apply(
                              TypeExpr.Apply(
                                TypeExpr.Apply(
                                  TypeExpr.Lookup(Identifier.LocalScope "Co"),
                                  TypeExpr.Lookup(Identifier.LocalScope "schema")
                                ),
                                TypeExpr.Lookup(Identifier.LocalScope "ctx")
                              ),
                              TypeExpr.Lookup(Identifier.LocalScope "st2")
                            ),
                            TypeExpr.Lookup(Identifier.LocalScope "o")
                          )
                        )
                      )
                    )
                  )
                )
              )
            )
          )
        Kind =
          Kind.Arrow(
            Kind.Schema,
            Kind.Arrow(
              Kind.Star,
              Kind.Arrow(
                Kind.Star,
                Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))
              )
            )
          )
        Operation = Co_MapState
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Co_MapState -> Some Co_MapState
            | _ -> None)
        Apply =
          fun loc0 _rest (_op, _v) ->
            reader {
              return!
                Errors.Singleton loc0 (fun () -> "Co::mapState is not yet implemented")
                |> reader.Throw
            } }

    // --- Co::getContext ---
    // getContext : Λschema::Schema. Λctx::*. Λst::*.
    //   Co[schema][ctx][st][ctx]
    let getContextId =
      Identifier.FullyQualified([ "Co" ], "getContext")
      |> TypeCheckScope.Empty.Resolve

    let getContextOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'runtimeContext, 'ext, Unit, Unit, CoroutineOperations> =
      getContextId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("schema", schemaKind),
            TypeExpr.Lambda(
              TypeParameter.Create("ctx", ctxKind),
              TypeExpr.Lambda(
                TypeParameter.Create("st", stKind),
                TypeExpr.Apply(
                  TypeExpr.Apply(
                    TypeExpr.Apply(
                      TypeExpr.Apply(
                        TypeExpr.Lookup(Identifier.LocalScope "Co"),
                        TypeExpr.Lookup(Identifier.LocalScope "schema")
                      ),
                      TypeExpr.Lookup(Identifier.LocalScope "ctx")
                    ),
                    TypeExpr.Lookup(Identifier.LocalScope "st")
                  ),
                  TypeExpr.Lookup(Identifier.LocalScope "ctx")
                )
              )
            )
          )
        Kind =
          Kind.Arrow(
            Kind.Schema,
            Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))
          )
        Operation = Co_GetContext
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Co_GetContext -> Some Co_GetContext
            | _ -> None)
        Apply =
          fun loc0 _rest (_op, _v) ->
            reader {
              return!
                Errors.Singleton loc0 (fun () -> "Co::getContext is not yet implemented")
                |> reader.Throw
            } }

    // --- Co::getState ---
    // getState : Λschema::Schema. Λctx::*. Λst::*.
    //   Co[schema][ctx][st][st]
    let getStateId =
      Identifier.FullyQualified([ "Co" ], "getState")
      |> TypeCheckScope.Empty.Resolve

    let getStateOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'runtimeContext, 'ext, Unit, Unit, CoroutineOperations> =
      getStateId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("schema", schemaKind),
            TypeExpr.Lambda(
              TypeParameter.Create("ctx", ctxKind),
              TypeExpr.Lambda(
                TypeParameter.Create("st", stKind),
                TypeExpr.Apply(
                  TypeExpr.Apply(
                    TypeExpr.Apply(
                      TypeExpr.Apply(
                        TypeExpr.Lookup(Identifier.LocalScope "Co"),
                        TypeExpr.Lookup(Identifier.LocalScope "schema")
                      ),
                      TypeExpr.Lookup(Identifier.LocalScope "ctx")
                    ),
                    TypeExpr.Lookup(Identifier.LocalScope "st")
                  ),
                  TypeExpr.Lookup(Identifier.LocalScope "st")
                )
              )
            )
          )
        Kind =
          Kind.Arrow(
            Kind.Schema,
            Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))
          )
        Operation = Co_GetState
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Co_GetState -> Some Co_GetState
            | _ -> None)
        Apply =
          fun loc0 _rest (_op, _v) ->
            reader {
              return!
                Errors.Singleton loc0 (fun () -> "Co::getState is not yet implemented")
                |> reader.Throw
            } }

    // --- Co::setState ---
    // setState : Λschema::Schema. Λctx::*. Λst::*.
    //   (st -> st) -> Co[schema][ctx][st][()]
    let setStateId =
      Identifier.FullyQualified([ "Co" ], "setState")
      |> TypeCheckScope.Empty.Resolve

    let setStateOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'runtimeContext, 'ext, Unit, Unit, CoroutineOperations> =
      setStateId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("schema", schemaKind),
            TypeExpr.Lambda(
              TypeParameter.Create("ctx", ctxKind),
              TypeExpr.Lambda(
                TypeParameter.Create("st", stKind),
                TypeExpr.Arrow(
                  TypeExpr.Arrow(
                    TypeExpr.Lookup(Identifier.LocalScope "st"),
                    TypeExpr.Lookup(Identifier.LocalScope "st")
                  ),
                  TypeExpr.Apply(
                    TypeExpr.Apply(
                      TypeExpr.Apply(
                        TypeExpr.Apply(
                          TypeExpr.Lookup(Identifier.LocalScope "Co"),
                          TypeExpr.Lookup(Identifier.LocalScope "schema")
                        ),
                        TypeExpr.Lookup(Identifier.LocalScope "ctx")
                      ),
                      TypeExpr.Lookup(Identifier.LocalScope "st")
                    ),
                    TypeExpr.Primitive PrimitiveType.Unit
                  )
                )
              )
            )
          )
        Kind =
          Kind.Arrow(
            Kind.Schema,
            Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))
          )
        Operation = Co_SetState
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | Co_SetState -> Some Co_SetState
            | _ -> None)
        Apply =
          fun loc0 _rest (_op, _v) ->
            reader {
              return!
                Errors.Singleton loc0 (fun () -> "Co::setState is not yet implemented")
                |> reader.Throw
            } }

    let coExtension =
      { TypeName = coResolvedId, coSymbolId
        TypeVars =
          [ (schemaVar, schemaKind)
            (ctxVar, ctxKind)
            (stVar, stKind)
            (resVar, resKind) ]
        Cases = Map.empty
        Operations =
          [ showOperation; untilOperation; ignoreOperation
            mapContextOperation; mapStateOperation
            getContextOperation; getStateOperation; setStateOperation ] |> Map.ofList
        Serialization = None
        ExtTypeChecker = None }

    coExtension, coSymbolId, make_coType

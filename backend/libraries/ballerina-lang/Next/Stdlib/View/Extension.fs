namespace Ballerina.DSL.Next.StdLib.View

module Extension =
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Extensions
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.Reader.WithError
  open Ballerina.Lenses

  type ViewPropsOperations<'ext> =
    | View_MapContext of
      {| mapper: Option<Value<TypeValue<'ext>, 'ext>> |}
    | View_MapState of
      {| mapDown: Option<Value<TypeValue<'ext>, 'ext>>
         mapUp: Option<Value<TypeValue<'ext>, 'ext>> |}

  let ViewExtension<'runtimeContext, 'ext, 'extDTO, 'deltaExt, 'deltaExtDTO
    when 'ext: comparison
    and 'extDTO: not null
    and 'extDTO: not struct
    and 'deltaExtDTO: not null
    and 'deltaExtDTO: not struct>
    (operationLens: PartialLens<'ext, ViewPropsOperations<'ext>>)
    (viewTypeSymbol: Option<TypeSymbol>)
    (viewPropsTypeSymbol: Option<TypeSymbol>)
    : TypeExtension<
        'runtimeContext,
        'ext,
        'extDTO,
        'deltaExt,
        'deltaExtDTO,
        Unit,
        Unit,
        Unit
       > *
      TypeExtension<
        'runtimeContext,
        'ext,
        'extDTO,
        'deltaExt,
        'deltaExtDTO,
        Unit,
        Unit,
        ViewPropsOperations<'ext>
       > *
      TypeSymbol *
      TypeSymbol *
      (TypeValue<'ext> -> TypeValue<'ext> -> TypeValue<'ext> -> TypeValue<'ext>) *
      (TypeValue<'ext> -> TypeValue<'ext> -> TypeValue<'ext> -> TypeValue<'ext>)
    =
    // --- View ---
    let viewId = Identifier.FullyQualified([ "Frontend" ], "View")

    let viewSymbolId =
      viewTypeSymbol |> Option.defaultWith (fun () -> viewId |> TypeSymbol.Create)

    let viewResolvedId = viewId |> TypeCheckScope.Empty.Resolve

    let schemaVar, schemaKind = TypeVar.Create("schema"), Kind.Schema
    let ctxVar, ctxKind = TypeVar.Create("context"), Kind.Star
    let stVar, stKind = TypeVar.Create("state"), Kind.Star

    let make_viewType (schema: TypeValue<'ext>) (ctx: TypeValue<'ext>) (st: TypeValue<'ext>) =
      TypeValue.Imported
        { Id = viewResolvedId
          Sym = viewSymbolId
          Parameters = []
          Arguments = [ schema; ctx; st ] }

    let viewExtension =
      { TypeName = viewResolvedId, viewSymbolId
        TypeVars = [ (schemaVar, schemaKind); (ctxVar, ctxKind); (stVar, stKind) ]
        Cases = Map.empty
        Operations = Map.empty
        Serialization = None
        ExtTypeChecker = None }

    // --- View::Props ---
    let viewPropsId = Identifier.FullyQualified([ "View" ], "Props")

    let viewPropsSymbolId =
      viewPropsTypeSymbol |> Option.defaultWith (fun () -> viewPropsId |> TypeSymbol.Create)

    let viewPropsResolvedId = viewPropsId |> TypeCheckScope.Empty.Resolve

    let make_viewPropsType (schema: TypeValue<'ext>) (ctx: TypeValue<'ext>) (st: TypeValue<'ext>) =
      TypeValue.Imported
        { Id = viewPropsResolvedId
          Sym = viewPropsSymbolId
          Parameters = []
          Arguments = [ schema; ctx; st ] }

    // --- View::mapContext ---
    // mapContext : Λschema. Λctx. Λst. Λctx2. (ctx -> ctx2) -> Props[schema][ctx][st] -> Props[schema][ctx2][st]
    let mapContextId =
      Identifier.FullyQualified([ "View" ], "mapContext")
      |> TypeCheckScope.Empty.Resolve

    let _ctx2Var, ctx2Kind = TypeVar.Create("ctx2"), Kind.Star

    let mapContextOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'runtimeContext, 'ext, Unit, Unit, ViewPropsOperations<'ext>> =
      mapContextId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("schema", schemaKind),
            TypeExpr.Lambda(
              TypeParameter.Create("ctx", ctxKind),
              TypeExpr.Lambda(
                TypeParameter.Create("st", stKind),
                TypeExpr.Lambda(
                  TypeParameter.Create("ctx2", ctx2Kind),
                  TypeExpr.Arrow(
                    TypeExpr.Arrow(
                      TypeExpr.Lookup(Identifier.LocalScope "ctx"),
                      TypeExpr.Lookup(Identifier.LocalScope "ctx2")
                    ),
                    TypeExpr.Arrow(
                      TypeExpr.Apply(
                        TypeExpr.Apply(
                          TypeExpr.Apply(
                            TypeExpr.Lookup(Identifier.FullyQualified([ "View" ], "Props")),
                            TypeExpr.Lookup(Identifier.LocalScope "schema")
                          ),
                          TypeExpr.Lookup(Identifier.LocalScope "ctx")
                        ),
                        TypeExpr.Lookup(Identifier.LocalScope "st")
                      ),
                      TypeExpr.Apply(
                        TypeExpr.Apply(
                          TypeExpr.Apply(
                            TypeExpr.Lookup(Identifier.FullyQualified([ "View" ], "Props")),
                            TypeExpr.Lookup(Identifier.LocalScope "schema")
                          ),
                          TypeExpr.Lookup(Identifier.LocalScope "ctx2")
                        ),
                        TypeExpr.Lookup(Identifier.LocalScope "st")
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
        Operation = View_MapContext {| mapper = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | View_MapContext v -> Some(View_MapContext v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              match op with
              | View_MapContext s when s.mapper.IsNone ->
                // 1st arg: capture the mapper function
                return
                  (View_MapContext {| mapper = Some v |} |> operationLens.Set, Some mapContextId)
                  |> Value.Ext
              | View_MapContext s ->
                // 2nd arg: v is the props record, apply mapper to context
                let mapper = s.mapper.Value

                let! fields =
                  v
                  |> Value.AsRecord
                  |> sum.MapError(Errors.MapContext(fun _ -> loc0))
                  |> reader.OfSum

                let contextKey =
                  Identifier.LocalScope "context" |> TypeCheckScope.Empty.Resolve

                let! oldContext =
                  fields
                  |> Map.tryFind contextKey
                  |> sum.OfOption(
                    Errors.Singleton loc0 (fun () -> "View::mapContext: missing 'context' field in props")
                  )
                  |> reader.OfSum

                let! newContext = Expr.EvalApply loc0 [] (mapper, oldContext)

                let newFields = fields |> Map.add contextKey newContext
                return Value.Record newFields
              | _ ->
                return!
                  Errors.Singleton loc0 (fun () -> "View::mapContext: unexpected operation state")
                  |> reader.Throw
            } }

    // --- View::mapState ---
    // mapState : Λschema. Λctx. Λst. Λst2.
    //   (st -> st2) -> (((st2 -> st2) -> ()) -> ((st -> st) -> ())) -> Props[schema][ctx][st] -> Props[schema][ctx][st2]
    let mapStateId =
      Identifier.FullyQualified([ "View" ], "mapState")
      |> TypeCheckScope.Empty.Resolve

    let _st2Var, st2Kind = TypeVar.Create("st2"), Kind.Star

    let mapStateOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'runtimeContext, 'ext, Unit, Unit, ViewPropsOperations<'ext>> =
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
                  TypeExpr.Arrow(
                    TypeExpr.Arrow(
                      TypeExpr.Lookup(Identifier.LocalScope "st"),
                      TypeExpr.Lookup(Identifier.LocalScope "st2")
                    ),
                    TypeExpr.Arrow(
                      TypeExpr.Arrow(
                        TypeExpr.Arrow(
                          TypeExpr.Lookup(Identifier.LocalScope "st2"),
                          TypeExpr.Lookup(Identifier.LocalScope "st2")
                        ),
                        TypeExpr.Arrow(
                          TypeExpr.Lookup(Identifier.LocalScope "st"),
                          TypeExpr.Lookup(Identifier.LocalScope "st")
                        )
                      ),
                      TypeExpr.Arrow(
                        TypeExpr.Apply(
                          TypeExpr.Apply(
                            TypeExpr.Apply(
                              TypeExpr.Lookup(Identifier.FullyQualified([ "View" ], "Props")),
                              TypeExpr.Lookup(Identifier.LocalScope "schema")
                            ),
                            TypeExpr.Lookup(Identifier.LocalScope "ctx")
                          ),
                          TypeExpr.Lookup(Identifier.LocalScope "st")
                        ),
                        TypeExpr.Apply(
                          TypeExpr.Apply(
                            TypeExpr.Apply(
                              TypeExpr.Lookup(Identifier.FullyQualified([ "View" ], "Props")),
                              TypeExpr.Lookup(Identifier.LocalScope "schema")
                            ),
                            TypeExpr.Lookup(Identifier.LocalScope "ctx")
                          ),
                          TypeExpr.Lookup(Identifier.LocalScope "st2")
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
              Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))
            )
          )
        Operation = View_MapState {| mapDown = None; mapUp = None |}
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | View_MapState v -> Some(View_MapState v)
            | _ -> None)
        Apply =
          fun loc0 _rest (op, v) ->
            reader {
              match op with
              | View_MapState s when s.mapDown.IsNone ->
                // 1st arg: capture mapDown
                return
                  (View_MapState {| mapDown = Some v; mapUp = None |} |> operationLens.Set,
                   Some mapStateId)
                  |> Value.Ext
              | View_MapState s when s.mapUp.IsNone ->
                // 2nd arg: capture mapUp
                return
                  (View_MapState {| mapDown = s.mapDown; mapUp = Some v |}
                   |> operationLens.Set,
                   Some mapStateId)
                  |> Value.Ext
              | View_MapState s ->
                // 3rd arg: v is the props record
                let mapDown = s.mapDown.Value
                let mapUp = s.mapUp.Value
                let! fields =
                  v
                  |> Value.AsRecord
                  |> sum.MapError(Errors.MapContext(fun _ -> loc0))
                  |> reader.OfSum

                let stateKey =
                  Identifier.LocalScope "state" |> TypeCheckScope.Empty.Resolve

                let setStateKey =
                  Identifier.LocalScope "setState" |> TypeCheckScope.Empty.Resolve

                let! oldState =
                  fields
                  |> Map.tryFind stateKey
                  |> sum.OfOption(
                    Errors.Singleton loc0 (fun () -> "View::mapState: missing 'state' field in props")
                  )
                  |> reader.OfSum

                let! oldSetState =
                  fields
                  |> Map.tryFind setStateKey
                  |> sum.OfOption(
                    Errors.Singleton loc0 (fun () -> "View::mapState: missing 'setState' field in props")
                  )
                  |> reader.OfSum

                // newState = mapDown(oldState)
                let! newState = Expr.EvalApply loc0 [] (mapDown, oldState)

                // newSetState = fun f -> oldSetState(mapUp(f))
                // Construct a Lambda whose body applies mapUp then oldSetState.
                let updaterVar = Var.Create "__updater__"

                let updaterKey =
                  Identifier.LocalScope "__updater__" |> TypeCheckScope.Empty.Resolve

                let dummyType = TypeValue.CreatePrimitive PrimitiveType.Unit

                let mkFromValue (v: Value<TypeValue<'ext>, 'ext>) : RunnableExpr<'ext> =
                  { RunnableExpr.Location = loc0
                    RunnableExpr.Scope = TypeCheckScope.Empty
                    RunnableExpr.Type = dummyType
                    RunnableExpr.Kind = Kind.Star
                    RunnableExpr.Expr =
                      RunnableExprRec.FromValue
                        { RunnableExprFromValue.Value = v
                          RunnableExprFromValue.ValueType = dummyType
                          RunnableExprFromValue.ValueKind = Kind.Star } }

                let mkLookup id : RunnableExpr<'ext> =
                  { RunnableExpr.Location = loc0
                    RunnableExpr.Scope = TypeCheckScope.Empty
                    RunnableExpr.Type = dummyType
                    RunnableExpr.Kind = Kind.Star
                    RunnableExpr.Expr = RunnableExprRec.Lookup { RunnableExprLookup.Id = id } }

                let mkApply (f: RunnableExpr<'ext>) (arg: RunnableExpr<'ext>) : RunnableExpr<'ext> =
                  { RunnableExpr.Location = loc0
                    RunnableExpr.Scope = TypeCheckScope.Empty
                    RunnableExpr.Type = dummyType
                    RunnableExpr.Kind = Kind.Star
                    RunnableExpr.Expr =
                      RunnableExprRec.Apply
                        { RunnableExprApply.F = f
                          RunnableExprApply.Arg = arg } }

                // Body: oldSetState(mapUp(updater))
                let body =
                  mkApply (mkFromValue oldSetState) (mkApply (mkFromValue mapUp) (mkLookup updaterKey))

                let newSetState =
                  Value.Lambda(
                    updaterVar,
                    body,
                    Map.empty,
                    TypeCheckScope.Empty
                  )

                let newFields =
                  fields
                  |> Map.add stateKey newState
                  |> Map.add setStateKey newSetState

                return Value.Record newFields
              | _ ->
                return!
                  Errors.Singleton loc0 (fun () -> "View::mapState: unexpected operation state")
                  |> reader.Throw
            } }

    let viewPropsExtension =
      { TypeName = viewPropsResolvedId, viewPropsSymbolId
        TypeVars = [ (schemaVar, schemaKind); (ctxVar, ctxKind); (stVar, stKind) ]
        Cases = Map.empty
        Operations =
          [ mapContextOperation; mapStateOperation ] |> Map.ofList
        Serialization = None
        ExtTypeChecker = None }

    viewExtension, viewPropsExtension, viewSymbolId, viewPropsSymbolId, make_viewType, make_viewPropsType

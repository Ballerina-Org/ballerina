namespace Ballerina.DSL.Next.StdLib.View

module Extension =
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Extensions
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.Reader.WithError
  open Ballerina.Lenses

  type ViewPropsOperations =
    | View_MapContext
    | View_MapState

  let ViewExtension<'runtimeContext, 'ext, 'extDTO, 'deltaExt, 'deltaExtDTO
    when 'ext: comparison
    and 'extDTO: not null
    and 'extDTO: not struct
    and 'deltaExtDTO: not null
    and 'deltaExtDTO: not struct>
    (operationLens: PartialLens<'ext, ViewPropsOperations>)
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
        ViewPropsOperations
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
        TypeOperationExtension<'runtimeContext, 'ext, Unit, Unit, ViewPropsOperations> =
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
        Operation = View_MapContext
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | View_MapContext -> Some View_MapContext
            | _ -> None)
        Apply =
          fun loc0 _rest (_op, _v) ->
            reader {
              return!
                Errors.Singleton loc0 (fun () -> "View::mapContext is not yet implemented")
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
        TypeOperationExtension<'runtimeContext, 'ext, Unit, Unit, ViewPropsOperations> =
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
                          TypeExpr.Arrow(
                            TypeExpr.Lookup(Identifier.LocalScope "st2"),
                            TypeExpr.Lookup(Identifier.LocalScope "st2")
                          ),
                          TypeExpr.Primitive PrimitiveType.Unit
                        ),
                        TypeExpr.Arrow(
                          TypeExpr.Arrow(
                            TypeExpr.Lookup(Identifier.LocalScope "st"),
                            TypeExpr.Lookup(Identifier.LocalScope "st")
                          ),
                          TypeExpr.Primitive PrimitiveType.Unit
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
        Operation = View_MapState
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | View_MapState -> Some View_MapState
            | _ -> None)
        Apply =
          fun loc0 _rest (_op, _v) ->
            reader {
              return!
                Errors.Singleton loc0 (fun () -> "View::mapState is not yet implemented")
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

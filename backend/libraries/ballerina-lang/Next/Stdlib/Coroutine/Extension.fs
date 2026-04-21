namespace Ballerina.DSL.Next.StdLib.Coroutine

module Extension =
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Extensions
  open Ballerina.Lenses
  open Ballerina.Reader.WithError
  open Ballerina.Errors
  open Ballerina.DSL.Next.StdLib.Coroutine.Model

  let CoroutineExtension<'runtimeContext, 'ext, 'extDTO, 'deltaExt, 'deltaExtDTO
    when 'ext: comparison
    and 'extDTO: not null
    and 'extDTO: not struct
    and 'deltaExtDTO: not null
    and 'deltaExtDTO: not struct>
    (typeSymbol: Option<TypeSymbol>)
    (operationLens: PartialLens<'ext, CoOperations>)
    : TypeExtension<
        'runtimeContext,
        'ext,
        'extDTO,
        'deltaExt,
        'deltaExtDTO,
        Unit,
        Unit,
        CoOperations
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

    // Co::until :
    //   (st -> () + res) ->
    //   Co[schema][ctx][st][()] ->
    //   Co[schema][ctx][st][res]
    let coUntilId =
      Identifier.FullyQualified([ "Co" ], "until")
      |> TypeCheckScope.Empty.Resolve

    let untilOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'runtimeContext, 'ext, Unit, Unit, CoOperations> =
      coUntilId,
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
                    TypeExpr.Arrow(
                      TypeExpr.Lookup(Identifier.LocalScope "st"),
                      TypeExpr.Sum
                        [ TypeExpr.Primitive PrimitiveType.Unit
                          TypeExpr.Lookup(Identifier.LocalScope "res") ]
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
                        TypeExpr.Lookup(Identifier.LocalScope "res")
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
                Errors.Singleton loc0 (fun _ -> "Co::until is not yet implemented")
                |> reader.Throw
            } }

    // Co::ignore :
    //   Co[schema][ctx][st][res] ->
    //   Co[schema][ctx][st][()]
    let coIgnoreId =
      Identifier.FullyQualified([ "Co" ], "ignore")
      |> TypeCheckScope.Empty.Resolve

    let ignoreOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'runtimeContext, 'ext, Unit, Unit, CoOperations> =
      coIgnoreId,
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
                Errors.Singleton loc0 (fun _ -> "Co::ignore is not yet implemented")
                |> reader.Throw
            } }

    // Co::show :
    //   Λschema. Λi. Λs. Λctx. Λst. Λo.
    //   i -> s -> (s -> () + o) -> View[schema][i][s] -> Co[schema][ctx][st][o]
    let coShowId =
      Identifier.FullyQualified([ "Co" ], "show")
      |> TypeCheckScope.Empty.Resolve

    let _iVar, iKind = TypeVar.Create("i"), Kind.Star
    let _sVar, sKind = TypeVar.Create("s"), Kind.Star
    let _oVar, oKind = TypeVar.Create("o"), Kind.Star

    let showOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'runtimeContext, 'ext, Unit, Unit, CoOperations> =
      coShowId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("schema", schemaKind),
            TypeExpr.Lambda(
              TypeParameter.Create("i", iKind),
              TypeExpr.Lambda(
                TypeParameter.Create("s", sKind),
                TypeExpr.Lambda(
                  TypeParameter.Create("ctx", ctxKind),
                  TypeExpr.Lambda(
                    TypeParameter.Create("st", stKind),
                    TypeExpr.Lambda(
                      TypeParameter.Create("o", oKind),
                      TypeExpr.Arrow(
                        TypeExpr.Lookup(Identifier.LocalScope "i"),
                        TypeExpr.Arrow(
                          TypeExpr.Lookup(Identifier.LocalScope "s"),
                          TypeExpr.Arrow(
                            TypeExpr.Arrow(
                              TypeExpr.Lookup(Identifier.LocalScope "s"),
                              TypeExpr.Sum
                                [ TypeExpr.Primitive PrimitiveType.Unit
                                  TypeExpr.Lookup(Identifier.LocalScope "o") ]
                            ),
                            TypeExpr.Arrow(
                              TypeExpr.Apply(
                                TypeExpr.Apply(
                                  TypeExpr.Apply(
                                    TypeExpr.Lookup(
                                      Identifier.FullyQualified(
                                        [ "Frontend" ],
                                        "View"
                                      )
                                    ),
                                    TypeExpr.Lookup(Identifier.LocalScope "schema")
                                  ),
                                  TypeExpr.Lookup(Identifier.LocalScope "i")
                                ),
                                TypeExpr.Lookup(Identifier.LocalScope "s")
                              ),
                              TypeExpr.Apply(
                                TypeExpr.Apply(
                                  TypeExpr.Apply(
                                    TypeExpr.Apply(
                                      TypeExpr.Lookup(Identifier.LocalScope "Co"),
                                      TypeExpr.Lookup(
                                        Identifier.LocalScope "schema"
                                      )
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
                Kind.Arrow(
                  Kind.Star,
                  Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))
                )
              )
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
                Errors.Singleton loc0 (fun _ -> "Co::show is not yet implemented")
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
          [ untilOperation
            ignoreOperation
            showOperation ]
          |> Map.ofList
        Serialization = None
        ExtTypeChecker = None }

    coExtension, coSymbolId, make_coType

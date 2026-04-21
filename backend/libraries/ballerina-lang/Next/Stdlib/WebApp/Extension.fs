namespace Ballerina.DSL.Next.StdLib.WebApp

module Extension =
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Extensions
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.Reader.WithError
  open Ballerina.Lenses

  type WebAppOperations =
    | WebApp_WithRoute
    | WebApp_WithComponent

  let WebAppExtension<'runtimeContext, 'ext, 'extDTO, 'deltaExt, 'deltaExtDTO
    when 'ext: comparison
    and 'extDTO: not null
    and 'extDTO: not struct
    and 'deltaExtDTO: not null
    and 'deltaExtDTO: not struct>
    (operationLens: PartialLens<'ext, WebAppOperations>)
    (webAppIOTypeSymbol: Option<TypeSymbol>)
    (viewTypeId: Identifier)
    (coTypeId: Identifier)
    (dbIOTypeId: Identifier)
    : TypeExtension<
        'runtimeContext,
        'ext,
        'extDTO,
        'deltaExt,
        'deltaExtDTO,
        Unit,
        Unit,
        WebAppOperations
       > *
      TypeLambdaExtension<'runtimeContext, 'ext, 'extDTO, WebAppOperations> *
      TypeSymbol *
      (TypeValue<'ext> -> TypeValue<'ext>)
    =
    // --- WebAppIO[schema] type ---
    let webAppIOId = Identifier.FullyQualified([ "WebApp" ], "IO")
    let webAppIOResolvedId = webAppIOId |> TypeCheckScope.Empty.Resolve

    let webAppIOSymbolId =
      webAppIOTypeSymbol |> Option.defaultWith (fun () -> webAppIOId |> TypeSymbol.Create)

    let schemaVar, schemaKind = TypeVar.Create("schema"), Kind.Schema
    let _ctxVar, ctxKind = TypeVar.Create("ctx"), Kind.Star
    let _stVar, stKind = TypeVar.Create("st"), Kind.Star

    let make_webAppIOType (schema: TypeValue<'ext>) =
      TypeValue.Imported
        { Id = webAppIOResolvedId
          Sym = webAppIOSymbolId
          Parameters = []
          Arguments = [ schema ] }

    // --- WebApp::withRoute ---
    // withRoute : Λschema::Schema. Λctx::*. Λst::*.
    //   string -> Co[schema][ctx][st][()] -> WebAppIO[schema] -> WebAppIO[schema]
    let withRouteId =
      Identifier.FullyQualified([ "WebApp" ], "withRoute")
      |> TypeCheckScope.Empty.Resolve

    let withRouteOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'runtimeContext, 'ext, Unit, Unit, WebAppOperations> =
      withRouteId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("schema", schemaKind),
            TypeExpr.Lambda(
              TypeParameter.Create("ctx", ctxKind),
              TypeExpr.Lambda(
                TypeParameter.Create("st", stKind),
                TypeExpr.Arrow(
                  TypeExpr.Primitive PrimitiveType.String,
                  TypeExpr.Arrow(
                    TypeExpr.Apply(
                      TypeExpr.Apply(
                        TypeExpr.Apply(
                          TypeExpr.Apply(
                            TypeExpr.Lookup coTypeId,
                            TypeExpr.Lookup(Identifier.LocalScope "schema")
                          ),
                          TypeExpr.Lookup(Identifier.LocalScope "ctx")
                        ),
                        TypeExpr.Lookup(Identifier.LocalScope "st")
                      ),
                      TypeExpr.Primitive PrimitiveType.Unit
                    ),
                    TypeExpr.Arrow(
                      TypeExpr.Apply(
                        TypeExpr.Lookup webAppIOId,
                        TypeExpr.Lookup(Identifier.LocalScope "schema")
                      ),
                      TypeExpr.Apply(
                        TypeExpr.Lookup webAppIOId,
                        TypeExpr.Lookup(Identifier.LocalScope "schema")
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
            Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))
          )
        Operation = WebApp_WithRoute
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | WebApp_WithRoute -> Some WebApp_WithRoute
            | _ -> None)
        Apply =
          fun loc0 _rest (_op, _v) ->
            reader {
              return!
                Errors.Singleton loc0 (fun () -> "WebApp::withRoute is not yet implemented")
                |> reader.Throw
            } }

    // --- WebApp::withComponent ---
    // withComponent : Λschema::Schema. Λctx::*. Λst::*.
    //   string -> Frontend::View[schema][ctx][st] -> WebAppIO[schema] -> WebAppIO[schema]
    let withComponentId =
      Identifier.FullyQualified([ "WebApp" ], "withComponent")
      |> TypeCheckScope.Empty.Resolve

    let withComponentOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'runtimeContext, 'ext, Unit, Unit, WebAppOperations> =
      withComponentId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("schema", schemaKind),
            TypeExpr.Lambda(
              TypeParameter.Create("ctx", ctxKind),
              TypeExpr.Lambda(
                TypeParameter.Create("st", stKind),
                TypeExpr.Arrow(
                  TypeExpr.Primitive PrimitiveType.String,
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
                    TypeExpr.Arrow(
                      TypeExpr.Apply(
                        TypeExpr.Lookup webAppIOId,
                        TypeExpr.Lookup(Identifier.LocalScope "schema")
                      ),
                      TypeExpr.Apply(
                        TypeExpr.Lookup webAppIOId,
                        TypeExpr.Lookup(Identifier.LocalScope "schema")
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
            Kind.Arrow(Kind.Star, Kind.Arrow(Kind.Star, Kind.Star))
          )
        Operation = WebApp_WithComponent
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | WebApp_WithComponent -> Some WebApp_WithComponent
            | _ -> None)
        Apply =
          fun loc0 _rest (_op, _v) ->
            reader {
              return!
                Errors.Singleton loc0 (fun () -> "WebApp::withComponent is not yet implemented")
                |> reader.Throw
            } }

    let webAppIOExtension =
      { TypeName = webAppIOResolvedId, webAppIOSymbolId
        TypeVars = [ (schemaVar, schemaKind) ]
        Cases = Map.empty
        Operations =
          [ withRouteOperation; withComponentOperation ] |> Map.ofList
        Serialization = None
        ExtTypeChecker = None }

    // --- WebApp::run ---
    // run : Λschema::Schema. Λresult::*.
    //   DBIO[schema][result] -> WebAppIO[schema]
    let webAppRunId =
      Identifier.FullyQualified([ "WebApp" ], "run")
      |> TypeCheckScope.Empty.Resolve

    let _resVar, resKind = TypeVar.Create("result"), Kind.Star

    let webAppRunType =
      TypeValue.CreateLambda(
        TypeParameter.Create("schema", schemaKind),
        TypeExpr.Lambda(
          TypeParameter.Create("result", resKind),
          TypeExpr.Arrow(
            TypeExpr.Apply(
              TypeExpr.Apply(
                TypeExpr.Lookup dbIOTypeId,
                TypeExpr.Lookup(Identifier.LocalScope "schema")
              ),
              TypeExpr.Lookup(Identifier.LocalScope "result")
            ),
            TypeExpr.Apply(
              TypeExpr.Lookup webAppIOId,
              TypeExpr.Lookup(Identifier.LocalScope "schema")
            )
          )
        )
      )

    let webAppRunKind =
      Kind.Arrow(Kind.Schema, Kind.Arrow(Kind.Star, Kind.Star))

    let webAppRunExtension: TypeLambdaExtension<'runtimeContext, 'ext, 'extDTO, WebAppOperations> =
      { ExtensionType = webAppRunId, webAppRunType, webAppRunKind
        ExtraBindings = Map.empty
        Value = WebApp_WithRoute // sentinel value — not meaningful, just needs to exist
        ValueLens = operationLens
        EvalToTypeApplicable =
          fun loc0 _rest _v ->
            reader {
              return!
                Errors.Singleton loc0 (fun () -> "WebApp::run is not yet implemented")
                |> reader.Throw
            }
        EvalToApplicable =
          fun loc0 _rest _v ->
            reader {
              return!
                Errors.Singleton loc0 (fun () -> "WebApp::run is not yet implemented")
                |> reader.Throw
            } }

    webAppIOExtension, webAppRunExtension, webAppIOSymbolId, make_webAppIOType

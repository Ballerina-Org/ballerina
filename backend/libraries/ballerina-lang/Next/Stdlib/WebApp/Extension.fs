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
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.StdLib.DB

  type WebAppWithRouteArgs<'ext when 'ext: comparison> =
    { Path: Option<string>
      InitialState: Option<Value<TypeValue<'ext>, 'ext>>
      Coroutine: Option<Value<TypeValue<'ext>, 'ext>> }

  type WebAppWithDbRouteArgs<'ext when 'ext: comparison> =
    { Path: Option<string>
      InitialState: Option<Value<TypeValue<'ext>, 'ext>>
      SchemaFn: Option<Value<TypeValue<'ext>, 'ext>> }

  type WebAppWithComponentArgs<'ext when 'ext: comparison> =
    { Name: Option<string>
      View: Option<Value<TypeValue<'ext>, 'ext>> }

  type WebAppOperations<'ext when 'ext: comparison> =
    | WebApp_Run
    | WebApp_TypeAppliedRun
    | WebApp_WithRoute of WebAppWithRouteArgs<'ext>
    | WebApp_WithDbRoute of WebAppWithDbRouteArgs<'ext>
    | WebApp_WithComponent of WebAppWithComponentArgs<'ext>

  let WebAppExtension<'runtimeContext, 'db, 'ext, 'extDTO, 'deltaExt, 'deltaExtDTO
    when 'ext: comparison
    and 'db: comparison
    and 'extDTO: not null
    and 'extDTO: not struct
    and 'deltaExtDTO: not null
    and 'deltaExtDTO: not struct>
    (operationLens: PartialLens<'ext, WebAppOperations<'ext>>)
    (dbValuesLens: PartialLens<'ext, DBValues<'runtimeContext, 'db, 'ext>>)
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
        WebAppOperations<'ext>
       > *
      TypeLambdaExtension<'runtimeContext, 'ext, 'extDTO, WebAppOperations<'ext>> *
      TypeSymbol *
      (TypeValue<'ext> -> TypeValue<'ext> -> TypeValue<'ext>)
    =
    // --- WebAppIO[schema] type ---
    let webAppIOId = Identifier.FullyQualified([ "WebApp" ], "IO")
    let webAppIOResolvedId = webAppIOId |> TypeCheckScope.Empty.Resolve

    let webAppIOSymbolId =
      webAppIOTypeSymbol |> Option.defaultWith (fun () -> webAppIOId |> TypeSymbol.Create)

    let schemaVar, schemaKind = TypeVar.Create("schema"), Kind.Schema
    let _appCtxVar, appCtxKind = TypeVar.Create("appctx"), Kind.Star
    let _stVar, stKind = TypeVar.Create("st"), Kind.Star

    let make_webAppIOType (schema: TypeValue<'ext>) (appCtx: TypeValue<'ext>) =
      TypeValue.Imported
        { Id = webAppIOResolvedId
          Sym = webAppIOSymbolId
          Parameters = []
          Arguments = [ schema; appCtx ] }

    // --- WebApp::withRoute ---
    // withRoute : Λschema::Schema. Λinitst::*. Λappctx::*.
    //   (string * Co[schema][appctx][initst][()] * initst) -> WebAppIO[schema][appctx] -> WebAppIO[schema][appctx]
    let withRouteId =
      Identifier.FullyQualified([ "WebApp" ], "withRoute")
      |> TypeCheckScope.Empty.Resolve

    let withRouteOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'runtimeContext, 'ext, Unit, Unit, WebAppOperations<'ext>> =
      withRouteId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("schema", schemaKind),
            TypeExpr.Lambda(
              TypeParameter.Create("initst", stKind),
              TypeExpr.Lambda(
                TypeParameter.Create("appctx", appCtxKind),
                TypeExpr.Arrow(
                  TypeExpr.Tuple(
                    [ TypeExpr.Primitive PrimitiveType.String
                      TypeExpr.Apply(
                        TypeExpr.Apply(
                          TypeExpr.Apply(
                            TypeExpr.Apply(
                              TypeExpr.Lookup coTypeId,
                              TypeExpr.Lookup(Identifier.LocalScope "schema")
                            ),
                            TypeExpr.Lookup(Identifier.LocalScope "appctx")
                          ),
                          TypeExpr.Lookup(Identifier.LocalScope "initst")
                        ),
                        TypeExpr.Primitive PrimitiveType.Unit
                      )
                      TypeExpr.Lookup(Identifier.LocalScope "initst") ]
                  ),
                  TypeExpr.Arrow(
                    TypeExpr.Apply(
                      TypeExpr.Apply(
                        TypeExpr.Lookup webAppIOId,
                        TypeExpr.Lookup(Identifier.LocalScope "schema")
                      ),
                      TypeExpr.Lookup(Identifier.LocalScope "appctx")
                    ),
                    TypeExpr.Apply(
                      TypeExpr.Apply(
                        TypeExpr.Lookup webAppIOId,
                        TypeExpr.Lookup(Identifier.LocalScope "schema")
                      ),
                      TypeExpr.Lookup(Identifier.LocalScope "appctx")
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
        Operation = WebApp_WithRoute { Path = None; InitialState = None; Coroutine = None }
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | WebApp_WithRoute args -> Some(WebApp_WithRoute args)
            | _ -> None)
        Apply =
          fun _loc0 _rest (op, v) ->
            reader {
              match op with
              | WebApp_WithRoute { Path = None; InitialState = None; Coroutine = None } ->
                // First arg: tuple3 (path, coroutine, initialState)
                match v with
                | Value.Tuple items when items.Length = 3 ->
                  let path =
                    match items.[0] with
                    | Value.Primitive(PrimitiveValue.String s) -> s
                    | _ -> failwith "WebApp::withRoute expects a string path as first tuple element"
                  let coroutine = items.[1]
                  let initialState = items.[2]
                  return
                    (WebApp_WithRoute { Path = Some path; InitialState = Some initialState; Coroutine = Some coroutine } |> operationLens.Set, Some withRouteId)
                    |> Ext
                | _ ->
                  return!
                    Errors.Singleton _loc0 (fun () -> "WebApp::withRoute expects a (path, coroutine, initialState) tuple")
                    |> reader.Throw
              | WebApp_WithRoute { Path = Some path; InitialState = Some initialState; Coroutine = Some coroutine } ->
                // Second arg: extract WebAppIOData, append route, return updated
                let! dbVals =
                  match v with
                  | Ext(ext, _) ->
                    dbValuesLens.Get ext
                    |> sum.OfOption(Errors.Singleton _loc0 (fun () -> "WebApp::withRoute: expected WebAppIO value"))
                    |> reader.OfSum
                  | _ ->
                    Errors.Singleton _loc0 (fun () -> "WebApp::withRoute: expected an Ext value")
                    |> reader.Throw
                match dbVals with
                | DBValues.WebAppIO data ->
                  let updatedData = { data with Routes = data.Routes @ [ (path, initialState, coroutine) ] }
                  return (DBValues.WebAppIO updatedData |> dbValuesLens.Set, None) |> Ext
                | _ ->
                  return!
                    Errors.Singleton _loc0 (fun () -> "WebApp::withRoute: expected WebAppIO data")
                    |> reader.Throw
              | _ ->
                return!
                  Errors.Singleton _loc0 (fun () -> "WebApp::withRoute: unexpected operation state")
                  |> reader.Throw
            } }

    // --- WebApp::withDbRoute ---
    // withDbRoute : Λschema::Schema. Λst::*. Λappctx::*.
    //   (string * (schema -> Co[schema][appctx][st][()]) * st) -> WebAppIO[schema][appctx] -> WebAppIO[schema][appctx]
    // Like withRoute, but the coroutine is provided as a function of the schema descriptor.
    // The function is stored unapplied; the host (Program.fs) resolves it before frontend generation.
    let withDbRouteId =
      Identifier.FullyQualified([ "WebApp" ], "withDbRoute")
      |> TypeCheckScope.Empty.Resolve

    let withDbRouteOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'runtimeContext, 'ext, Unit, Unit, WebAppOperations<'ext>> =
      withDbRouteId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("schema", schemaKind),
            TypeExpr.Lambda(
              TypeParameter.Create("st", stKind),
              TypeExpr.Lambda(
                TypeParameter.Create("appctx", appCtxKind),
                TypeExpr.Arrow(
                  TypeExpr.Tuple(
                    [ TypeExpr.Primitive PrimitiveType.String
                      TypeExpr.Arrow(
                        TypeExpr.Lookup(Identifier.LocalScope "schema"),
                        TypeExpr.Apply(
                          TypeExpr.Apply(
                            TypeExpr.Apply(
                              TypeExpr.Apply(
                                TypeExpr.Lookup coTypeId,
                                TypeExpr.Lookup(Identifier.LocalScope "schema")
                              ),
                              TypeExpr.Lookup(Identifier.LocalScope "appctx")
                            ),
                            TypeExpr.Lookup(Identifier.LocalScope "st")
                          ),
                          TypeExpr.Primitive PrimitiveType.Unit
                        )
                      )
                      TypeExpr.Lookup(Identifier.LocalScope "st") ]
                  ),
                  TypeExpr.Arrow(
                    TypeExpr.Apply(
                      TypeExpr.Apply(
                        TypeExpr.Lookup webAppIOId,
                        TypeExpr.Lookup(Identifier.LocalScope "schema")
                      ),
                      TypeExpr.Lookup(Identifier.LocalScope "appctx")
                    ),
                    TypeExpr.Apply(
                      TypeExpr.Apply(
                        TypeExpr.Lookup webAppIOId,
                        TypeExpr.Lookup(Identifier.LocalScope "schema")
                      ),
                      TypeExpr.Lookup(Identifier.LocalScope "appctx")
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
        Operation = WebApp_WithDbRoute { Path = None; InitialState = None; SchemaFn = None }
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | WebApp_WithDbRoute args -> Some(WebApp_WithDbRoute args)
            | _ -> None)
        Apply =
          fun _loc0 _rest (op, v) ->
            reader {
              match op with
              | WebApp_WithDbRoute { Path = None; InitialState = None; SchemaFn = None } ->
                // First arg: tuple3 (path, schemaFn, initialState)
                match v with
                | Value.Tuple items when items.Length = 3 ->
                  let path =
                    match items.[0] with
                    | Value.Primitive(PrimitiveValue.String s) -> s
                    | _ -> failwith "WebApp::withDbRoute expects a string path as first tuple element"
                  let schemaFn = items.[1]
                  let initialState = items.[2]
                  return
                    (WebApp_WithDbRoute { Path = Some path; InitialState = Some initialState; SchemaFn = Some schemaFn } |> operationLens.Set, Some withDbRouteId)
                    |> Ext
                | _ ->
                  return!
                    Errors.Singleton _loc0 (fun () -> "WebApp::withDbRoute expects a (path, schemaFn, initialState) tuple")
                    |> reader.Throw
              | WebApp_WithDbRoute { Path = Some path; InitialState = Some initialState; SchemaFn = Some schemaFn } ->
                // Second arg: extract WebAppIOData, append to DbRoutes
                let! dbVals =
                  match v with
                  | Ext(ext, _) ->
                    dbValuesLens.Get ext
                    |> sum.OfOption(Errors.Singleton _loc0 (fun () -> "WebApp::withDbRoute: expected WebAppIO value"))
                    |> reader.OfSum
                  | _ ->
                    Errors.Singleton _loc0 (fun () -> "WebApp::withDbRoute: expected an Ext value")
                    |> reader.Throw
                match dbVals with
                | DBValues.WebAppIO data ->
                  let updatedData = { data with DbRoutes = data.DbRoutes @ [ (path, initialState, schemaFn) ] }
                  return (DBValues.WebAppIO updatedData |> dbValuesLens.Set, None) |> Ext
                | _ ->
                  return!
                    Errors.Singleton _loc0 (fun () -> "WebApp::withDbRoute: expected WebAppIO data")
                    |> reader.Throw
              | _ ->
                return!
                  Errors.Singleton _loc0 (fun () -> "WebApp::withDbRoute: unexpected operation state")
                  |> reader.Throw
            } }

    // --- WebApp::withComponent ---
    // withComponent : Λschema::Schema. Λappctx::*. Λst::*.
    //   string -> Frontend::View[schema][appctx][st] -> WebAppIO[schema][appctx] -> WebAppIO[schema][appctx]
    let withComponentId =
      Identifier.FullyQualified([ "WebApp" ], "withComponent")
      |> TypeCheckScope.Empty.Resolve

    let withComponentOperation
      : ResolvedIdentifier *
        TypeOperationExtension<'runtimeContext, 'ext, Unit, Unit, WebAppOperations<'ext>> =
      withComponentId,
      { Type =
          TypeValue.CreateLambda(
            TypeParameter.Create("schema", schemaKind),
            TypeExpr.Lambda(
              TypeParameter.Create("appctx", appCtxKind),
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
                        TypeExpr.Lookup(Identifier.LocalScope "appctx")
                      ),
                      TypeExpr.Lookup(Identifier.LocalScope "st")
                    ),
                    TypeExpr.Arrow(
                      TypeExpr.Apply(
                        TypeExpr.Apply(
                          TypeExpr.Lookup webAppIOId,
                          TypeExpr.Lookup(Identifier.LocalScope "schema")
                        ),
                        TypeExpr.Lookup(Identifier.LocalScope "appctx")
                      ),
                      TypeExpr.Apply(
                        TypeExpr.Apply(
                          TypeExpr.Lookup webAppIOId,
                          TypeExpr.Lookup(Identifier.LocalScope "schema")
                        ),
                        TypeExpr.Lookup(Identifier.LocalScope "appctx")
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
        Operation = WebApp_WithComponent { Name = None; View = None }
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | WebApp_WithComponent args -> Some(WebApp_WithComponent args)
            | _ -> None)
        Apply =
          fun _loc0 _rest (op, v) ->
            reader {
              match op with
              | WebApp_WithComponent { Name = None; View = None } ->
                // First arg: capture the component name string
                let name =
                  match v with
                  | Value.Primitive(PrimitiveValue.String s) -> s
                  | _ -> failwith "WebApp::withComponent expects a string name"
                return
                  (WebApp_WithComponent { Name = Some name; View = None } |> operationLens.Set, Some withComponentId)
                  |> Ext
              | WebApp_WithComponent { Name = Some name; View = None } ->
                // Second arg: capture the view value
                return
                  (WebApp_WithComponent { Name = Some name; View = Some v } |> operationLens.Set, Some withComponentId)
                  |> Ext
              | WebApp_WithComponent { Name = Some name; View = Some viewValue } ->
                // Third arg: extract WebAppIOData, append component, return updated
                let! dbVals =
                  match v with
                  | Ext(ext, _) ->
                    dbValuesLens.Get ext
                    |> sum.OfOption(Errors.Singleton _loc0 (fun () -> "WebApp::withComponent: expected WebAppIO value"))
                    |> reader.OfSum
                  | _ ->
                    Errors.Singleton _loc0 (fun () -> "WebApp::withComponent: expected an Ext value")
                    |> reader.Throw
                match dbVals with
                | DBValues.WebAppIO data ->
                  let updatedData = { data with Components = data.Components @ [ (name, viewValue) ] }
                  return (DBValues.WebAppIO updatedData |> dbValuesLens.Set, None) |> Ext
                | _ ->
                  return!
                    Errors.Singleton _loc0 (fun () -> "WebApp::withComponent: expected WebAppIO data")
                    |> reader.Throw
              | _ ->
                return!
                  Errors.Singleton _loc0 (fun () -> "WebApp::withComponent: unexpected operation state")
                  |> reader.Throw
            } }

    let webAppIOExtension =
      { TypeName = webAppIOResolvedId, webAppIOSymbolId
        TypeVars = [ (schemaVar, schemaKind); (_appCtxVar, appCtxKind) ]
        Cases = Map.empty
        Operations =
          [ withRouteOperation; withDbRouteOperation; withComponentOperation ] |> Map.ofList
        Serialization = None
        ExtTypeChecker = None }

    // --- WebApp::run ---
    // run : Λschema::Schema. Λappctx::*.
    //   DBIO[schema][()] -> WebAppIO[schema][appctx]
    let webAppRunId =
      Identifier.FullyQualified([ "WebApp" ], "run")
      |> TypeCheckScope.Empty.Resolve

    let webAppRunType =
      TypeValue.CreateLambda(
        TypeParameter.Create("schema", schemaKind),
        TypeExpr.Lambda(
          TypeParameter.Create("appctx", appCtxKind),
          TypeExpr.Arrow(
            TypeExpr.Apply(
              TypeExpr.Apply(
                TypeExpr.Lookup dbIOTypeId,
                TypeExpr.Lookup(Identifier.LocalScope "schema")
              ),
              TypeExpr.Primitive PrimitiveType.Unit
            )
            ,
            TypeExpr.Apply(
              TypeExpr.Apply(
                TypeExpr.Lookup webAppIOId,
                TypeExpr.Lookup(Identifier.LocalScope "schema")
              ),
              TypeExpr.Lookup(Identifier.LocalScope "appctx")
            )
          )
        )
      )

    let webAppRunKind = Kind.Arrow(Kind.Schema, Kind.Arrow(Kind.Star, Kind.Star))

    let webAppRunExtension: TypeLambdaExtension<'runtimeContext, 'ext, 'extDTO, WebAppOperations<'ext>> =
      { ExtensionType = webAppRunId, webAppRunType, webAppRunKind
        ExtraBindings = Map.empty
        Value = WebApp_Run
        ValueLens = operationLens
        EvalToTypeApplicable =
          fun loc0 _rest v ->
            reader {
              let! op =
                operationLens.Get v
                |> sum.OfOption(
                  Errors.Singleton loc0 (fun () -> "WebApp::run: cannot extract operation from extension")
                )
                |> reader.OfSum

              match op with
              | WebApp_Run ->
                // First type application [schema] — transition to TypeAppliedRun
                return
                  TypeApplicable(fun _typeArg ->
                    reader {
                      return (WebApp_TypeAppliedRun |> operationLens.Set, None) |> Ext
                    })
              | _ ->
                return!
                  Errors.Singleton loc0 (fun () -> "WebApp::run: expected WebApp_Run sentinel")
                  |> reader.Throw
            }
        EvalToApplicable =
          fun loc0 _rest v ->
            reader {
              let! op =
                operationLens.Get v
                |> sum.OfOption(
                  Errors.Singleton loc0 (fun () -> "WebApp::run: cannot extract operation from extension")
                )
                |> reader.OfSum

              match op with
              | WebApp_TypeAppliedRun ->
                // Remaining type application [appctx] is erased; now value application with DBIO arg
                return
                  Applicable(fun dbioValue ->
                    reader {
                      // Extract the DBIO from the argument value
                      let! dbVals =
                        match dbioValue with
                        | Ext(ext, _) ->
                          dbValuesLens.Get ext
                          |> sum.OfOption(
                            Errors.Singleton loc0 (fun () -> "WebApp::run: expected DBIO value")
                          )
                          |> reader.OfSum
                        | _ ->
                          Errors.Singleton loc0 (fun () -> "WebApp::run: expected an Ext value")
                          |> reader.Throw

                      match dbVals with
                      | DBValues.DBIO dbio ->
                        let webAppData: WebAppIOData<'runtimeContext, 'db, 'ext> =
                          { DBIO = dbio
                            Routes = []
                            DbRoutes = []
                            Components = [] }
                        return (DBValues.WebAppIO webAppData |> dbValuesLens.Set, None) |> Ext
                      | _ ->
                        return!
                          Errors.Singleton loc0 (fun () -> "WebApp::run: expected DBIO data")
                          |> reader.Throw
                    })
              | _ ->
                return!
                  Errors.Singleton loc0 (fun () -> "WebApp::run: expected WebApp_TypeAppliedRun")
                  |> reader.Throw
            } }

    webAppIOExtension, webAppRunExtension, webAppIOSymbolId, make_webAppIOType

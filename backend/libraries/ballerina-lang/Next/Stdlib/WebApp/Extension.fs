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
      Coroutine: Option<Value<TypeValue<'ext>, 'ext>> }

  type WebAppWithComponentArgs<'ext when 'ext: comparison> =
    { Name: Option<string>
      View: Option<Value<TypeValue<'ext>, 'ext>> }

  type WebAppOperations<'ext when 'ext: comparison> =
    | WebApp_Run
    | WebApp_TypeAppliedRun
    | WebApp_WithRoute of WebAppWithRouteArgs<'ext>
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
        TypeOperationExtension<'runtimeContext, 'ext, Unit, Unit, WebAppOperations<'ext>> =
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
        Operation = WebApp_WithRoute { Path = None; Coroutine = None }
        OperationsLens =
          operationLens
          |> PartialLens.BindGet (function
            | WebApp_WithRoute args -> Some(WebApp_WithRoute args)
            | _ -> None)
        Apply =
          fun _loc0 _rest (op, v) ->
            reader {
              match op with
              | WebApp_WithRoute { Path = None; Coroutine = None } ->
                // First arg: capture the path string
                let path =
                  match v with
                  | Value.Primitive(PrimitiveValue.String s) -> s
                  | _ -> failwith "WebApp::withRoute expects a string path"
                return
                  (WebApp_WithRoute { Path = Some path; Coroutine = None } |> operationLens.Set, Some withRouteId)
                  |> Ext
              | WebApp_WithRoute { Path = Some path; Coroutine = None } ->
                // Second arg: capture the coroutine value
                return
                  (WebApp_WithRoute { Path = Some path; Coroutine = Some v } |> operationLens.Set, Some withRouteId)
                  |> Ext
              | WebApp_WithRoute { Path = Some path; Coroutine = Some coroutine } ->
                // Third arg: extract WebAppIOData, append route, return updated
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
                  let updatedData = { data with Routes = data.Routes @ [ (path, coroutine) ] }
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

    // --- WebApp::withComponent ---
    // withComponent : Λschema::Schema. Λctx::*. Λst::*.
    //   string -> Frontend::View[schema][ctx][st] -> WebAppIO[schema] -> WebAppIO[schema]
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
                // Second type application [result] was erased; now value application with DBIO arg
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

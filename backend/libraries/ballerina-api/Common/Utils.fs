namespace Ballerina.API

module APIUtils =
  open Ballerina.DSL.Next.StdLib.Extensions
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.Reader.WithError
  open Ballerina
  open Ballerina.DSL.Next.Extensions
  open Ballerina.Collections.NonEmptyList
  open Microsoft.AspNetCore.Http
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.Types.TypeChecker.Value
  open Ballerina.DSL.Next.Types.TypeChecker
  open System.Runtime.CompilerServices
  open Ballerina.DSL.Next.SyntheticData.Generator
  open Ballerina.DSL.Next.Serialization.ValueSerializer
  // open SyntheticData
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.StdLib.DB
  open Ballerina.DSL.Next.Terms.Eval
  open Ballerina.Data.Delta
  open Ballerina.DSL.Next.StdLib.Updater.Model

  [<Extension>]
  type HttpContextExtensions() =
    [<Extension>]
    static member TryGetValue(httpContext: HttpContext, param: string) =
      httpContext.Request.Query.TryGetValue param
      |> function
        | true, v -> Left(v.ToString())
        | _ -> Right(Errors.Singleton Location.Unknown (fun _ -> $"Missing mandatory query parameter {param}"))

    [<Extension>]
    static member TryGetSchema(httpContext: HttpContext) = httpContext.TryGetValue "schema"

  let toUknonwLocation (s: Sum<'a, Errors<'context>>) : Sum<'a, Errors<Location>> =
    s |> sum.MapError(Errors.MapContext(replaceWith Location.Unknown))

  let typeCheckValue<'runtimeContext, 'db, 'customExtension when 'customExtension: comparison and 'db: comparison>
    (value:
      Value<
        TypeValue<ValueExt<'runtimeContext, 'db, 'customExtension>>,
        ValueExt<'runtimeContext, 'db, 'customExtension>
       >)
    (typeValue: TypeValue<ValueExt<'runtimeContext, 'db, 'customExtension>>)
    (languageContext:
      LanguageContext<
        'runtimeContext,
        ValueExt<'runtimeContext, 'db, 'customExtension>,
        ValueExtDTO,
        DeltaExt<'runtimeContext, 'db, 'customExtension>,
        DeltaExtDTO
       >)
    (typeCheckContext: TypeCheckContext<ValueExt<'runtimeContext, 'db, 'customExtension>>)
    (typeCheckState: TypeCheckState<ValueExt<'runtimeContext, 'db, 'customExtension>>)
    : Sum<unit, APIError<'runtimeContext, 'db, 'customExtension, Location>> =
    sum {
      let! extensionChecker =
        languageContext.ExtTypeChecker
        |> Sum.fromOption (fun _ ->
          { Errors = Errors.Singleton Location.Unknown (fun _ -> "Undefined extension value type checker.")
            TypeError =
              Some
                { ExpectedType = typeValue
                  LanguageContext = languageContext
                  TypeCheckState = typeCheckState
                  TypeCheckContext = typeCheckContext } })

      do!
        Value.IsInstanceOf extensionChecker (value, typeValue)
        |> reader.MapError(Errors.MapContext(replaceWith Location.Unknown))
        |> Reader.Run(typeCheckContext, typeCheckState)
        |> sum.MapError(fun errors ->
          { Errors = errors
            TypeError =
              Some
                { ExpectedType = typeValue
                  LanguageContext = languageContext
                  TypeCheckState = typeCheckState
                  TypeCheckContext = typeCheckContext } })
    }

  let runDTOConverter<'runtimeContext, 'db, 'customExtension, 'result
    when 'customExtension: comparison and 'db: comparison>
    (languageContext:
      LanguageContext<
        'runtimeContext,
        ValueExt<'runtimeContext, 'db, 'customExtension>,
        ValueExtDTO,
        DeltaExt<'runtimeContext, 'db, 'customExtension>,
        DeltaExtDTO
       >)
    (converter:
      Reader<
        'result,
        DSL.Next.Serialization.SerializationContext<ValueExt<'runtimeContext, 'db, 'customExtension>, ValueExtDTO>,
        Errors<unit>
       >)
    =
    converter
    |> reader.MapError(Errors.MapContext(replaceWith Location.Unknown))
    |> Reader.Run languageContext.SerializationContext.SerializationContext

  let errorsToSerializable (e: Errors<'context>) =
    e.Errors()
    |> NonEmptyList.ToList
    |> List.toArray
    |> Array.map (fun error -> error.ToString())

  // let generateExamples<'runtimeContext, 'db, 'customExtension when 'customExtension: comparison and 'db: comparison>
  //   (apiError: APITypeError<'runtimeContext, 'db, 'customExtension>)
  //   =
  //   sum {
  //     let config = configWithRandom 1 (Some ListConfig.Default)
  //     let importedGenerators = listImportedGenerators ()

  //     let! randomValues =
  //       let generators =
  //         seq {
  //           for _ in [ 1..10 ] do
  //             yield!
  //               Generate
  //                 config
  //                 (apiError.TypeCheckContext, apiError.TypeCheckState)
  //                 importedGenerators
  //                 apiError.ExpectedType
  //         }

  //       generators |> Sum.All |> toUknonwLocation

  //     return!
  //       randomValues
  //       |> List.map (fun value -> runDTOConverter apiError.LanguageContext (valueToDTO value))
  //       |> Sum.All
  //       |> Sum.map List.toArray
  //   }


  let entityDescriptorFromDb<'runtimeContext, 'db, 'customExtension
    when 'customExtension: comparison and 'db: comparison>
    (dbio: DBIO<'runtimeContext, 'db, ValueExt<'runtimeContext, 'db, 'customExtension>>)
    (entityName: string)
    =
    sum {
      let! schema = dbio.SchemaAsValue |> Value.AsRecord

      let! entities =
        schema
        |> Map.tryFindWithError
          ("Entities" |> Identifier.LocalScope |> ResolvedIdentifier.FromIdentifier)
          "schema"
          (fun () -> "Entities")
          ()

      let! entities = entities |> Value.AsRecord

      let! entityDescriptor =
        entities
        |> Map.tryFindWithError
          (entityName |> Identifier.LocalScope |> ResolvedIdentifier.FromIdentifier)
          "schema"
          (fun () -> "Entities")
          ()

      return entityDescriptor
    }

  let lookupDescriptorFromDb<'runtimeContext, 'db, 'customExtension
    when 'customExtension: comparison and 'db: comparison>
    (dbio: DBIO<'runtimeContext, 'db, ValueExt<'runtimeContext, 'db, 'customExtension>>)
    (relationName: string)
    (direction: string)
    =
    sum {
      let! schema = dbio.SchemaAsValue |> Value.AsRecord

      let! relations =
        schema
        |> Map.tryFindWithError
          ("Relations" |> Identifier.LocalScope |> ResolvedIdentifier.FromIdentifier)
          "schema"
          (fun () -> "Relations")
          ()

      let! relations = relations |> Value.AsRecord

      let! relationDescriptor =
        relations
        |> Map.tryFindWithError
          (relationName |> Identifier.LocalScope |> ResolvedIdentifier.FromIdentifier)
          "schema"
          (fun () -> relationName)
          ()

      let! relationDescriptorFields = relationDescriptor |> Value.AsRecord

      let! lookupDescriptor =
        relationDescriptorFields
        |> Map.tryFindWithError
          (direction |> Identifier.LocalScope |> ResolvedIdentifier.FromIdentifier)
          "schema"
          (fun () -> direction)
          ()

      lookupDescriptor
    }

  let createUpdaterFromDelta
    (delta: Delta<ValueExt<'runtimeContext, 'db, 'customExtension>, DeltaExt<'runtimeContext, 'db, 'customExtension>>)
    : Sum<
        Expr<
          TypeValue<ValueExt<'runtimeContext, 'db, 'customExtension>>,
          ResolvedIdentifier,
          ValueExt<'runtimeContext, 'db, 'customExtension>
         >,
        Errors<unit>
       >
    =
    sum {

      let! updater = delta |> Delta.ToUpdater DeltaExt.ToUpdater

      let updaterExtension =
        ValueExt(Choice4Of7(CompositeType(Choice5Of5(UpdaterOperations(Apply { Updater = updater })))))

      return
        Expr.FromValue(
          Value.Ext(
            updaterExtension,
            Identifier.FullyQualified([ "@updater" ], "apply")
            |> ResolvedIdentifier.FromIdentifier
            |> Some
          ),
          TypeValue.CreatePrimitive PrimitiveType.Unit,
          Kind.Star
        )
    }

  let getDbDescriptor<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName
    when 'customExtension: comparison and 'db: comparison>
    (tenantId: 'tenantId)
    (schemaName: 'schemaName)
    (draft: bool)
    (context: APIContext<'runtimeContext, 'db, 'customExtension, 'tenantId, 'schemaName>)
    : Sum<
        DBIO<'runtimeContext, 'db, ValueExt<'runtimeContext, 'db, 'customExtension>> *
        APILanguageContext<'runtimeContext, 'db, 'customExtension> *
        ExprEvalContext<'runtimeContext, ValueExt<'runtimeContext, 'db, 'customExtension>> *
        TypeCheckContext<ValueExt<'runtimeContext, 'db, 'customExtension>> *
        TypeCheckState<ValueExt<'runtimeContext, 'db, 'customExtension>>,
        APIError<'runtimeContext, 'db, 'customExtension, Location>
       >
    =
    context.DbDescriptorFetcher tenantId schemaName draft
    |> sum.Map(fun dbDescriptor ->
      dbDescriptor.DbExtension,
      context.LanguageContext,
      dbDescriptor.EvalContext,
      dbDescriptor.TypeCheckContext,
      dbDescriptor.TypeCheckState)
    |> sum.MapError APIError<'runtimeContext, 'db, 'customExtension, Location>.Create

  let apiResponseFromSum<'runtimeContext, 'db, 'deltaDb, 'customExtension, 'context, 'a, 'b
    when 'customExtension: comparison and 'context: comparison and 'db: comparison and 'deltaDb: comparison>
    (body: Sum<'a, APIError<'runtimeContext, 'db, 'customExtension, 'context>>)
    (onSuccess: 'a -> 'b)
    : IResult =
    match body with
    | Left result -> onSuccess >> Results.Ok <| result
    | Right { Errors = errors; TypeError = Some _ } ->
      let serializedErrors = errorsToSerializable errors

      Results.BadRequest
        { Errors = serializedErrors
          Examples = [||] }
    // match generateExamples typeError with
    // | Left values ->
    //   Results.BadRequest(
    //     { Errors = serializedErrors
    //       Examples = values }
    //   )
    // | Right errors ->
    //   Results.BadRequest
    //     { Errors = errorsToSerializable errors
    //       Examples = [||] }
    | Right { Errors = errors; TypeError = None } ->
      Results.BadRequest
        { Errors = errorsToSerializable errors
          Examples = [||] }

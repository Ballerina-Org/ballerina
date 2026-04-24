namespace Ballerina.DSL.Next.StdLib.DB.Extension

[<AutoOpen>]
module CUD =
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.Lenses
  open Ballerina.DSL.Next.Extensions
  open Ballerina.DSL.Next.StdLib.DB
  open System

  let DBCUDExtension<'runtimeContext, 'db, 'ext, 'extDTO
    when 'ext: comparison and 'extDTO: not null and 'extDTO: not struct>
    (db_ops: DBTypeClass<'runtimeContext, 'db, 'ext>)
    (listLens: PartialLens<'ext, List<Value<TypeValue<'ext>, 'ext>>>)
    (mapLens: PartialLens<'ext, Map<Value<TypeValue<'ext>, 'ext>, Value<TypeValue<'ext>, 'ext>>>)
    (valueLens: PartialLens<'ext, DBValues<'runtimeContext, 'db, 'ext>>)
    : OperationsExtension<'runtimeContext, 'ext, DBValues<'runtimeContext, 'db, 'ext>> =

    let DBCalculatePropertyId, CalculatePropertyOperation, calculateProps =
      DBCalculatePropertyExtension listLens.Set valueLens

    let DBStripPropertyId, StripPropertyOperation, stripProps =
      DBStripPropertiesExtension listLens.Set valueLens

    let DBCalculatePropsId, CalculatePropsOperation =
      DBCalculatePropsPublicExtension db_ops calculateProps valueLens

    let DBStripPropsId, StripPropsOperation =
      DBStripPropsPublicExtension db_ops stripProps valueLens

    let DBCreateId, CreateOperation =
      DBCreateExtension db_ops calculateProps listLens.Set valueLens

    let DBUpdateId, UpdateOperation =
      DBUpdateExtension db_ops (calculateProps, stripProps) listLens.Set valueLens

    let DBUpsertId, UpsertOperation =
      DBUpsertExtension db_ops (calculateProps, stripProps) listLens.Set valueLens

    let DBDeleteId, DeleteOperation = DBDeleteExtension db_ops listLens.Set valueLens

    let DBUpsertManyId, UpsertManyOperation =
      DBUpsertManyExtension db_ops (calculateProps, stripProps) mapLens valueLens

    let DBUpdateManyId, UpdateManyOperation =
      DBUpdateManyExtension db_ops (calculateProps, stripProps) mapLens valueLens

    let DBDeleteManyId, DeleteManyOperation =
      DBDeleteManyExtension db_ops listLens valueLens

    let DBLinkId, LinkOperation = DBLinkExtension db_ops listLens.Set valueLens

    let DBLinkManyId, LinkManyOperation = DBLinkManyExtension db_ops listLens valueLens

    let DBUnlinkId, UnlinkOperation = DBUnlinkExtension db_ops listLens.Set valueLens

    let DBUnlinkManyId, UnlinkManyOperation =
      DBUnlinkManyExtension db_ops listLens valueLens

    let DBIsLinkedId, IsLinkedOperation =
      DBIsLinkedExtension db_ops listLens.Set valueLens

    let DBMoveBeforeId, MoveBeforeOperation =
      DBMoveBeforeExtension db_ops listLens.Set valueLens

    let DBMoveAfterId, MoveAfterOperation =
      DBMoveAfterExtension db_ops listLens.Set valueLens

    let DBMoveBeforeReverseId, MoveBeforeReverseOperation =
      DBMoveBeforeReverseExtension db_ops listLens.Set valueLens

    let DBMoveAfterReverseId, MoveAfterReverseOperation =
      DBMoveAfterReverseExtension db_ops listLens.Set valueLens

    let lookupsExtensions = DBLookupsExtensions db_ops listLens.Set valueLens

    let DBGetByIdId, GetByIdOperation = DBGetByIdExtension db_ops valueLens

    let DBGetManyId, GetManyOperation = DBGetManyExtension db_ops listLens.Set valueLens

    { TypeVars = []
      Operations =
        [ (DBGetByIdId, GetByIdOperation)
          (DBGetManyId, GetManyOperation)
          (DBStripPropertyId, StripPropertyOperation)
          (DBCalculatePropertyId, CalculatePropertyOperation)
          (DBCalculatePropsId, CalculatePropsOperation)
          (DBStripPropsId, StripPropsOperation)
          (DBCreateId, CreateOperation)
          (DBUpdateId, UpdateOperation)
          (DBUpsertId, UpsertOperation)
          (DBUpsertManyId, UpsertManyOperation)
          (DBUpdateManyId, UpdateManyOperation)
          (DBLinkId, LinkOperation)
          (DBLinkManyId, LinkManyOperation)
          (DBUnlinkId, UnlinkOperation)
          (DBUnlinkManyId, UnlinkManyOperation)
          (DBIsLinkedId, IsLinkedOperation)
          (DBMoveBeforeId, MoveBeforeOperation)
          (DBMoveAfterId, MoveAfterOperation)
          (DBMoveBeforeReverseId, MoveBeforeReverseOperation)
          (DBMoveAfterReverseId, MoveAfterReverseOperation)
          (DBDeleteId, DeleteOperation)
          (DBDeleteManyId, DeleteManyOperation) ]
        @ lookupsExtensions
        |> Map.ofList }

  let registerDBExtensions
    (db_ops: DBTypeClass<'runtimeContext, 'db, 'ext>)
    (list_lens: PartialLens<'ext, List<Value<TypeValue<'ext>, 'ext>>>)
    (map_lens: PartialLens<'ext, Map<Value<TypeValue<'ext>, 'ext>, Value<TypeValue<'ext>, 'ext>>>)
    (value_lens: PartialLens<'ext, DBValues<'runtimeContext, 'db, 'ext>>)
    (typeCheckingConfig: Option<TypeCheckingConfig<'ext>>)
    : (LanguageContext<'runtimeContext, 'ext, 'valueExtDTO, 'deltaExt, 'deltaExtDTO>
        -> LanguageContext<'runtimeContext, 'ext, 'valueExtDTO, 'deltaExt, 'deltaExtDTO>) *
      TypeSymbol *
      (Schema<'ext> -> TypeQueryRow<'ext> -> TypeValue<'ext>)
    =
    let memoryDBCUDExtension =
      DBCUDExtension<'runtimeContext, 'db, 'ext, 'valueExtDTO>

        db_ops
        list_lens
        map_lens
        value_lens

    let memoryDBRunExtension =
      DBRunExtension<'runtimeContext, 'db, 'ext, 'valueExtDTO> db_ops value_lens

    let memoryDBRunQueryExtension, query_sym, mk_query =
      DBRunQueryExtension<'runtimeContext, 'db, 'ext, 'valueExtDTO>
        db_ops
        list_lens.Set
        value_lens
        (typeCheckingConfig |> Option.map (fun cfg -> cfg.QueryTypeSymbol))

    // Collect all DB identifiers for reject-lists
    let cudIds =
      memoryDBCUDExtension.Operations
      |> Map.toSeq
      |> Seq.map fst

    let runId =
      let (id, _, _) = memoryDBRunExtension.ExtensionType
      id

    let runQueryId =
      let (id, _, _) = memoryDBRunQueryExtension.ExtensionType
      id

    let viewRejected =
      seq {
        yield! cudIds
        yield runId
        yield runQueryId
      }
      |> Seq.fold
        (fun acc id ->
          Map.add id "DB operations are not allowed inside views" acc)
        Map.empty

    let coRejected =
      Map.ofList
        [ (runQueryId,
           "Queries are not allowed inside coroutines; use getMany/lookup instead") ]

    (fun languageContext ->
      let lc =
        languageContext
        |> (memoryDBRunExtension |> TypeLambdaExtension.RegisterLanguageContext)
        |> (memoryDBRunQueryExtension |> TypeLambdaExtension.RegisterLanguageContext)
        |> (memoryDBCUDExtension |> OperationsExtension.RegisterLanguageContext)

      { lc with
          TypeCheckContext =
            { lc.TypeCheckContext with
                ViewRejectedIdentifiers =
                  viewRejected
                  |> Map.fold
                    (fun acc k v -> Map.add k v acc)
                    lc.TypeCheckContext.ViewRejectedIdentifiers
                CoRejectedIdentifiers =
                  coRejected
                  |> Map.fold
                    (fun acc k v -> Map.add k v acc)
                    lc.TypeCheckContext.CoRejectedIdentifiers } }),
    query_sym,
    mk_query

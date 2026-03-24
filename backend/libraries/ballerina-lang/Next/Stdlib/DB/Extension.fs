namespace Ballerina.DSL.Next.StdLib.DB.Extension

[<AutoOpen>]
module CUD =
  open Ballerina.DSL.Next.Types
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

    let lookupsExtensions = DBLookupsExtensions db_ops listLens.Set valueLens

    let DBGetByIdId, GetByIdOperation = DBGetByIdExtension db_ops valueLens

    let DBGetManyId, GetManyOperation = DBGetManyExtension db_ops listLens.Set valueLens

    { TypeVars = []
      Operations =
        [ (DBGetByIdId, GetByIdOperation)
          (DBGetManyId, GetManyOperation)
          (DBStripPropertyId, StripPropertyOperation)
          (DBCalculatePropertyId, CalculatePropertyOperation)
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
          (DBDeleteId, DeleteOperation)
          (DBDeleteManyId, DeleteManyOperation) ]
        @ lookupsExtensions
        |> Map.ofList }

  let registerDBExtensions
    (db_ops: DBTypeClass<'runtimeContext, 'db, 'ext>)
    (list_lens: PartialLens<'ext, List<Value<TypeValue<'ext>, 'ext>>>)
    (map_lens: PartialLens<'ext, Map<Value<TypeValue<'ext>, 'ext>, Value<TypeValue<'ext>, 'ext>>>)
    (value_lens: PartialLens<'ext, DBValues<'runtimeContext, 'db, 'ext>>)
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
      DBRunQueryExtension<'runtimeContext, 'db, 'ext, 'valueExtDTO> db_ops list_lens.Set value_lens

    (fun languageContext ->
      languageContext
      |> (memoryDBRunExtension |> TypeLambdaExtension.RegisterLanguageContext)
      |> (memoryDBRunQueryExtension |> TypeLambdaExtension.RegisterLanguageContext)
      |> (memoryDBCUDExtension |> OperationsExtension.RegisterLanguageContext)),
    query_sym,
    mk_query

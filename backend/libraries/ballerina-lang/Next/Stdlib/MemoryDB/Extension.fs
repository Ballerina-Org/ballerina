namespace Ballerina.DSL.Next.StdLib.MemoryDB.Extension

[<AutoOpen>]
module CUD =
  open Ballerina.DSL.Next.Types
  open Ballerina.Lenses
  open Ballerina.DSL.Next.Extensions
  open Ballerina.DSL.Next.StdLib.MemoryDB

  let MemoryDBCUDExtension<'ext when 'ext: comparison>
    (listLens: PartialLens<'ext, List<Value<TypeValue<'ext>, 'ext>>>)
    (mapLens: PartialLens<'ext, Map<Value<TypeValue<'ext>, 'ext>, Value<TypeValue<'ext>, 'ext>>>)
    (valueLens: PartialLens<'ext, MemoryDBValues<'ext>>)
    : OperationsExtension<'ext, MemoryDBValues<'ext>> =

    let memoryDBCalculatePropertyId, CalculatePropertyOperation, calculateProps =
      MemoryDBCalculatePropertyExtension listLens.Set valueLens

    let memoryDBStripPropertyId, StripPropertyOperation, stripProps =
      MemoryDBStripPropertiesExtension listLens.Set valueLens

    let memoryDBCreateId, CreateOperation =
      MemoryDBCreateExtension calculateProps listLens.Set valueLens

    let memoryDBUpdateId, UpdateOperation =
      MemoryDBUpdateExtension (calculateProps, stripProps) listLens.Set valueLens

    let memoryDBUpsertId, UpsertOperation =
      MemoryDBUpsertExtension (calculateProps, stripProps) listLens.Set valueLens

    let memoryDBDeleteId, DeleteOperation =
      MemoryDBDeleteExtension listLens.Set valueLens

    let memoryDBUpsertManyId, UpsertManyOperation =
      MemoryDBUpsertManyExtension (calculateProps, stripProps) mapLens valueLens

    let memoryDBUpdateManyId, UpdateManyOperation =
      MemoryDBUpdateManyExtension (calculateProps, stripProps) mapLens valueLens

    let memoryDBDeleteManyId, DeleteManyOperation =
      MemoryDBDeleteManyExtension mapLens valueLens

    let memoryDBLinkId, LinkOperation = MemoryDBLinkExtension listLens.Set valueLens

    let memoryDBLinkManyId, LinkManyOperation =
      MemoryDBLinkManyExtension listLens valueLens

    let memoryDBUnlinkId, UnlinkOperation =
      MemoryDBUnlinkExtension listLens.Set valueLens

    let memoryDBUnlinkManyId, UnlinkManyOperation =
      MemoryDBUnlinkManyExtension listLens valueLens

    let memoryDBQueryFromEntityId, QueryFromEntityOperation =
      MemoryDBQueryFromEntityExtension valueLens

    let memoryDBQueryFromRelationId, QueryFromRelationOperation =
      MemoryDBQueryFromRelationExtension valueLens

    let memoryDBQuerySelectId, QuerySelectOperation =
      MemoryDBQuerySelectExtension valueLens

    let memoryDBQueryWhereId, QueryWhereOperation =
      MemoryDBQueryWhereExtension valueLens

    let memoryDBQueryCrossId, QueryCrossOperation =
      MemoryDBQueryCrossExtension valueLens

    let memoryDBQueryOrderById, QueryOrderByOperation =
      MemoryDBQueryOrderByExtension valueLens

    let memoryDBQueryExpandId, QueryExpandOperation =
      MemoryDBQueryExpandExtension valueLens

    { TypeVars = []
      Operations =
        [ (memoryDBStripPropertyId, StripPropertyOperation)
          (memoryDBCalculatePropertyId, CalculatePropertyOperation)
          (memoryDBCreateId, CreateOperation)
          (memoryDBUpdateId, UpdateOperation)
          (memoryDBUpsertId, UpsertOperation)
          (memoryDBUpsertManyId, UpsertManyOperation)
          (memoryDBUpdateManyId, UpdateManyOperation)
          (memoryDBLinkId, LinkOperation)
          (memoryDBLinkManyId, LinkManyOperation)
          (memoryDBUnlinkId, UnlinkOperation)
          (memoryDBUnlinkManyId, UnlinkManyOperation)
          (memoryDBDeleteId, DeleteOperation)
          (memoryDBDeleteManyId, DeleteManyOperation)
          (memoryDBQueryFromEntityId, QueryFromEntityOperation)
          (memoryDBQueryFromRelationId, QueryFromRelationOperation)
          (memoryDBQuerySelectId, QuerySelectOperation)
          (memoryDBQueryWhereId, QueryWhereOperation)
          (memoryDBQueryCrossId, QueryCrossOperation)
          (memoryDBQueryOrderById, QueryOrderByOperation)
          (memoryDBQueryExpandId, QueryExpandOperation) ]
        |> Map.ofList }

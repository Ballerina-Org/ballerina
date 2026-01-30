namespace Ballerina.DSL.Next.StdLib.MemoryDB.Extension

[<AutoOpen>]
module CUD =
  open Ballerina.StdLib.String
  open Ballerina.Collections.Option
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.Reader.WithError
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.DSL.Next.Terms
  open Ballerina.DSL.Next.Terms.Patterns
  open Ballerina.DSL.Next.Types
  open Ballerina.DSL.Next.Types.Patterns
  open Ballerina.Lenses
  open Ballerina.DSL.Next.Extensions
  open FSharp.Data
  open Ballerina.Collections.NonEmptyList
  open Ballerina.DSL.Next.Types.TypeChecker
  open System
  open Ballerina.Cat.Collections.OrderedMap
  open Ballerina
  open Ballerina.DSL.Next.StdLib.MemoryDB

  let MemoryDBCUDExtension<'ext when 'ext: comparison>
    (listSet: List<Value<TypeValue<'ext>, 'ext>> -> 'ext)
    (mapLens: PartialLens<'ext, Map<Value<TypeValue<'ext>, 'ext>, Value<TypeValue<'ext>, 'ext>>>)
    (valueLens: PartialLens<'ext, MemoryDBValues<'ext>>)
    : OperationsExtension<'ext, MemoryDBValues<'ext>> *
      (Value<TypeValue<'ext>, 'ext>
        -> SchemaEntity<'ext>
        -> Reader<Value<TypeValue<'ext>, 'ext>, ExprEvalContext<'ext>, Errors<Location>>)
    =

    let memoryDBCalculatePropertyId, CalculatePropertyOperation, calculateProps =
      MemoryDBCalculatePropertyExtension listSet valueLens

    let memoryDBStripPropertyId, StripPropertyOperation, stripProps =
      MemoryDBStripPropertiesExtension listSet valueLens

    let memoryDBCreateId, CreateOperation =
      MemoryDBCreateExtension calculateProps listSet valueLens

    let memoryDBUpdateId, UpdateOperation =
      MemoryDBUpdateExtension (calculateProps, stripProps) listSet valueLens

    let memoryDBUpsertId, UpsertOperation =
      MemoryDBUpsertExtension (calculateProps, stripProps) listSet valueLens


    let memoryDBDeleteId, DeleteOperation = MemoryDBDeleteExtension listSet valueLens

    let memoryDBUpsertManyId, UpsertManyOperation =
      MemoryDBUpsertManyExtension (calculateProps, stripProps) mapLens valueLens

    let memoryDBUpdateManyId, UpdateManyOperation =
      MemoryDBUpdateManyExtension (calculateProps, stripProps) mapLens valueLens

    let memoryDBDeleteManyId, DeleteManyOperation =
      MemoryDBDeleteManyExtension mapLens valueLens

    let memoryDBLinkId, LinkOperation = MemoryDBLinkExtension listSet valueLens

    let memoryDBUnlinkId, UnlinkOperation = MemoryDBUnlinkExtension listSet valueLens


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
          (memoryDBUnlinkId, UnlinkOperation)
          (memoryDBDeleteId, DeleteOperation)
          (memoryDBDeleteManyId, DeleteManyOperation) ]
        |> Map.ofList },
    calculateProps

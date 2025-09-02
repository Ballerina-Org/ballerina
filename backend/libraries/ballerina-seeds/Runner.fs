namespace Ballerina.Seeds

open System
open System.Linq
open Ballerina.Collections.Sum
open Ballerina.Data.Delta.Model
open Ballerina.DSL.Next.Types.Model
open Ballerina.Data.Api.Model
open Ballerina.Data.Db
open Ballerina.Data.Model
open Ballerina.Data.Seeds
open Ballerina.Data.Seeds.Model
open Ballerina.Data.Seeds.Updaters
open Ballerina.Errors
open Ballerina.Reader.WithError
open Ballerina.Seeds
open Ballerina.Data.Delta.ToUpdater

module Runner =
  let seed (schema: Schema<TypeValue>) : Reader<Seeds<TypeValue>, SeedingContext<TypeValue>, Errors> =
    reader {

      let! entities = schema.Entities |> Map.map (fun _k -> EntityDescriptor.seed) |> reader.AllMap

      let! lookups =
        schema.Lookups
        |> Map.map (fun _k -> LookupDescriptor.seed entities)
        |> Map.values
        |> Seq.toList
        |> sum.All
        |> reader.OfSum

      let lookupByKey =
        schema.Lookups
        |> Map.toList
        |> List.zip (Map.keys schema.Lookups |> Seq.toList)
        |> List.mapi (fun i (k, _) -> k, lookups[i])

      let! backwardsLookups =
        lookupByKey
        |> Seq.map (fun (k, lookup) ->
          sum {
            let descriptor = schema.Lookups[k]
            let! backwards = LookupDescriptor.tryFlip descriptor lookup

            match backwards with
            | Some(name, backwards) -> seq { name, backwards }
            | None -> Seq.empty
          })
        |> sum.All
        |> sum.Map(Seq.collect id)
        |> reader.OfSum

      let allLookups = lookupByKey |> Seq.append backwardsLookups |> Map.ofSeq

      return
        { Entities = entities
          Lookups = allLookups }
    }

  let createApi
    (ctx: SeedingContext<TypeValue>)
    (schema: Schema<TypeValue>)
    (specs: Specs<TypeValue>)
    : Sum<Api * (unit -> Seeds<TypeValue>), Errors> =
    sum {
      let! init = seed schema |> Reader.Run ctx
      let mutable seedsDb = init
      let mutable _specsDb = specs

      let getSeedsDb () = seedsDb

      let entitiesApi =
        { Get =
            fun entityName id ->
              sum {
                let! entities = seedsDb.Entities |> Map.tryFindWithError entityName "entities" entityName
                return! entities |> Map.tryFindWithError id "entities" $"{id} in {entityName}"
              }
          GetMany =
            fun entityName (from, count) ->
              sum {

                let! entities = seedsDb.Entities |> Map.tryFindWithError entityName "entities" entityName

                let values =
                  entities |> Map.toList |> List.map snd |> List.skip from |> List.take count

                return
                  {| Values = values
                     HasMore = from + count < Map.count entities |}
              }
          Create =
            fun entityName value ->
              sum {
                let! entities = seedsDb.Entities |> Map.tryFindWithError entityName "entities" entityName
                let id = Guid.CreateVersion7()
                let entities = entities |> Map.add id value
                seedsDb <- seedsDb |> Seeds.Updaters.Entities(Map.add entityName entities)
                return id
              }
          Update =
            fun entityName (id, delta) ->
              sum {
                let! entities = seedsDb.Entities |> Map.tryFindWithError entityName "entities" entityName
                let! entity = entities |> Map.tryFindWithError id "entity" $"{id} in {entityName}"
                let! updater = delta |> Delta.ToUpdater(TypeValue.Primitive PrimitiveType.Unit)
                let! updated = updater entity
                let entities = entities |> Map.add id updated
                seedsDb <- seedsDb |> Seeds.Updaters.Entities(Map.add entityName entities)
                return ()
              }
          Delete =
            fun entityName id ->
              sum {
                let! entities = seedsDb.Entities |> Map.tryFindWithError entityName "entities" entityName
                let! _entity = entities |> Map.tryFindWithError id "entity" $"{id} in {entityName}"
                let entities = entities |> Map.remove id
                seedsDb <- seedsDb |> Seeds.Updaters.Entities(Map.add entityName entities)
                return ()
              } }

      let link: string -> Guid * Guid -> Sum<unit, Errors> =
        fun lookupName (sourceId, targetId) ->
          sum {
            let! lookupDescriptor =
              schema.Lookups
              |> Map.tryFindWithError lookupName "lookup descriptors" lookupName

            let! lookup = seedsDb.Lookups |> Map.tryFindWithError lookupName "lookups" lookupName

            let! sourceLookup =
              lookup
              |> Map.tryFind sourceId
              |> Sum.fromOption (fun () -> Errors.Singleton "source lookup for link cannot be found")

            let sourceLookup = sourceLookup |> Set.add targetId
            let lookup = lookup |> Map.add sourceId sourceLookup


            match lookupDescriptor.Backward with
            | None ->
              seedsDb <- seedsDb |> Seeds.Updaters.Lookups(Map.add lookupName lookup)
              return ()
            | Some(backwardLookupName, _) ->

              let! backwardLookup =
                seedsDb.Lookups
                |> Map.tryFindWithError backwardLookupName "lookups" backwardLookupName

              let! targetLookup = backwardLookup |> Map.tryFindWithError sourceId lookupName $"{targetId}"
              let targetLookup = targetLookup |> Set.add sourceId
              let backwardLookup = backwardLookup |> Map.add targetId targetLookup
              seedsDb <- seedsDb |> Seeds.Updaters.Lookups(Map.add backwardLookupName backwardLookup)
              return ()

          }

      let unlink: string -> Guid * Guid -> Sum<unit, Errors> =
        fun lookupName (sourceId, targetId) ->
          sum {

            let! lookupDescriptor =
              schema.Lookups
              |> Map.tryFindWithError lookupName "lookup descriptors" lookupName

            let! lookup = seedsDb.Lookups |> Map.tryFindWithError lookupName "lookups" lookupName
            let! sourceLookup = lookup |> Map.tryFindWithError sourceId lookupName $"{sourceId}"
            let sourceLookup = sourceLookup |> Set.remove targetId
            let lookup = lookup |> Map.add sourceId sourceLookup

            return!
              sum {
                match lookupDescriptor.Backward with
                | None ->
                  seedsDb <- seedsDb |> Seeds.Updaters.Lookups(Map.add lookupName lookup)
                  return ()
                | Some(backwardLookupName, _) ->
                  let! backwardLookup =
                    seedsDb.Lookups
                    |> Map.tryFindWithError backwardLookupName "lookups" backwardLookupName

                  let! targetLookup = backwardLookup |> Map.tryFindWithError sourceId lookupName $"{targetId}"
                  let targetLookup = targetLookup |> Set.remove sourceId
                  let backwardLookup = backwardLookup |> Map.add targetId targetLookup
                  seedsDb <- seedsDb |> Seeds.Updaters.Lookups(Map.add backwardLookupName backwardLookup)
                  return ()
              }
          }

      let lookupsApi =
        { GetMany =
            fun lookupName (id, (from, count)) ->
              sum {
                let! _lookupDescriptor =
                  schema.Lookups
                  |> Map.tryFindWithError lookupName "lookup descriptors" lookupName

                let! lookup = seedsDb.Lookups |> Map.tryFindWithError lookupName "lookups" lookupName
                let! lookup = lookup |> Map.tryFindWithError id "lookups with id" $"{id}"

                let entities =
                  seedsDb.Entities
                  |> Map.values
                  |> _.ToArray()
                  |> Seq.map Map.toSeq
                  |> Seq.concat
                  |> Map.ofSeq

                let targetIds = lookup |> Set.toSeq |> Seq.skip from |> Seq.take count

                let! targetValues =
                  targetIds
                  |> Seq.map (fun targetId -> entities |> Map.tryFindWithError targetId "target" $"{targetId}")
                  |> sum.All

                return
                  {| Values = targetValues
                     HasMore = from + count < List.length targetValues |}
              }
          Create =
            fun lookupName (sourceId, newTarget) ->
              sum {
                let! lookupDescriptor =
                  schema.Lookups
                  |> Map.tryFindWithError lookupName "lookup descriptors" lookupName

                let! entities =
                  seedsDb.Entities
                  |> Map.tryFindWithError lookupDescriptor.Source "entities" lookupDescriptor.Source

                let targetId = Guid.CreateVersion7()
                let entities = entities |> Map.add targetId newTarget
                //let sourceLookup = sourceLookup |> Set.add targetId
                //let lookup = lookup |> Map.add sourceId sourceLookup
                do! link lookupName (sourceId, targetId)
                seedsDb <- seedsDb |> Seeds.Updaters.Entities(Map.add lookupDescriptor.Source entities)
                return targetId
              }
          Delete =
            fun lookupName (sourceId, targetId) ->
              sum {
                let! lookupDescriptor =
                  schema.Lookups
                  |> Map.tryFindWithError lookupName "lookup descriptors" lookupName

                let! entities =
                  seedsDb.Entities
                  |> Map.tryFindWithError lookupDescriptor.Source "entities" lookupDescriptor.Source

                let entities = entities |> Map.remove targetId
                // let sourceLookup = sourceLookup |> Set.remove targetId
                //let lookup = lookup |> Map.add sourceId sourceLookup
                do! unlink lookupName (sourceId, targetId)
                seedsDb <- seedsDb |> Seeds.Updaters.Entities(Map.add lookupDescriptor.Source entities)
                return ()
              }
          Link = link
          Unlink = unlink }

      // let specApi = {
      //   Get = failwith "todo"
      //   GetMany =
      //     fun (SpecName name) ->
      //       sum {
      //         let! specs = specsDb |> Map.tryFindWithError name "specs" name
      //         return specs |> Map.toList
      //       }
      //   Create = failwith "todo"
      //   Delete = failwith "todo"
      //   Update = failwith "todo"
      //   Version = failwith "todo"
      // }

      return
        { Entities = entitiesApi
          Lookups = lookupsApi },
        //Specs = specApi },
        getSeedsDb
    }

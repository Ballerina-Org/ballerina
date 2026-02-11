namespace Ballerina.Data.Store.InMemory.Backend

open System
open System.Linq
open System.Collections.Concurrent
open Ballerina
open Ballerina
open Ballerina.Collections.Sum
open Ballerina.DSL.Next.StdLib.Extensions
open Ballerina.DSL.Next.Types.TypeChecker.Model
open Ballerina.DSL.Next.Types.Model
open Ballerina.Data.Schema.Model
open Ballerina.Data.Schema.Json
open Ballerina.Data.Spec.Model
open Ballerina.Data.Store.Api.Model
open Ballerina.Data.Store.Model
open Ballerina.LocalizedErrors
open Ballerina.Errors
open Ballerina.Data.Delta.Model
open Ballerina.Data.Delta.ToUpdater
open Ballerina.State.WithError
open Ballerina.Data.Store.Updaters
open Ballerina.Data.TypeEval
open Ballerina.VirtualFolders.Interactions
open Ballerina.VirtualFolders.Model
open Ballerina.DSL.Next.Terms.Model

type ConcurrentStore<'ext when 'ext: comparison> =
  { Tenants:
      ConcurrentDictionary<TenantId, ConcurrentDictionary<SpecName, Spec<TypeValue<ValueExt<'ext>>, ValueExt<'ext>>>> }

module ConcurrentStore =
  let initStore (tenantIds: TenantId list) : ConcurrentStore<'ext> =
    let tenants =
      ConcurrentDictionary<TenantId, ConcurrentDictionary<SpecName, Spec<TypeValue<ValueExt<'ext>>, ValueExt<'ext>>>>()

    for tid in tenantIds do
      tenants[tid] <- ConcurrentDictionary<SpecName, Spec<TypeValue<ValueExt<'ext>>, ValueExt<'ext>>>()

    { Tenants = tenants }

  let makeSpecApi
    (store: ConcurrentDictionary<SpecName, Spec<TypeValue<ValueExt<'ext>>, ValueExt<'ext>>>)
    (specName: SpecName)
    (path: VirtualPath option)
    (onDeltaExt:
      DeltaExt<'ext>
        -> Value<TypeValue<ValueExt<'ext>>, ValueExt<'ext>>
        -> Sum<Value<TypeValue<ValueExt<'ext>>, ValueExt<'ext>>, Errors.Errors<unit>>)
    : SpecDataApi<ValueExt<'ext>, DeltaExt<'ext>> =

    let storeUpdater (u: U<SpecData<TypeValue<ValueExt<'ext>>, ValueExt<'ext>>>) =
      sum {
        match store.TryGetValue specName with
        | true, current ->
          let next = { current with Seeds = u current.Seeds }

          match store.TryUpdate(specName, next, current) with
          | true -> return next.Seeds
          | false ->
            return!
              sum.Throw(Errors.Singleton () (fun () -> $"Updating seeds in the store failed for {specName.SpecName}"))
        | _ ->
          return!
            sum.Throw(Errors.Singleton () (fun () -> $"Updating seeds in the store failed for {specName.SpecName}"))
      }

    let entitiesApi =
      { Get =
          fun entity id ->
            sum {
              match store.TryGetValue(specName) with
              | false, _ -> return! sum.Throw(Errors.Singleton () (fun () -> $"Store doesn't contain spec {specName}"))
              | true, spec ->
                let seeds = spec.Seeds

                let! entities =
                  seeds.Entities
                  |> Map.tryFindWithError entity "entities" (fun () -> entity.EntityName) ()

                return!
                  entities
                  |> Map.tryFindWithError id "entities" (fun () -> $"{id} in {entity}") ()
            }
        GetMany =
          fun entity (from, count) ->
            sum {
              match store.TryGetValue(specName) with
              | false, _ -> return! sum.Throw(Errors.Singleton () (fun () -> $"Store doesn't contain spec {specName}"))
              | true, spec ->
                let seeds = spec.Seeds

                let! entities =
                  seeds.Entities
                  |> Map.tryFindWithError entity "entities" (fun () -> entity.EntityName) ()

                let values = entities |> Map.toList |> List.skip from |> List.truncate count

                return
                  {| Values = values
                     HasMore = from + count < Map.count entities |}
            }
        Create =
          fun entity value ->
            sum {
              match store.TryGetValue(specName) with
              | false, _ -> return! sum.Throw(Errors.Singleton () (fun () -> $"Store doesn't contain spec {specName}"))
              | true, spec ->
                let seeds = spec.Seeds

                let! entities =
                  seeds.Entities
                  |> Map.tryFindWithError entity "entities" (fun () -> entity.EntityName) ()

                let id = Guid.CreateVersion7()
                let entities = entities |> Map.add id value

                seeds
                |> SpecData.Updaters.Entities(Map.add entity entities)
                |> replaceWith
                |> storeUpdater

                return id
            }
        Update =
          fun entity (id, delta) ->
            sum {
              match store.TryGetValue(specName) with
              | false, _ -> return! sum.Throw(Errors.Singleton () (fun () -> $"Store doesn't contain spec {specName}"))
              | true, spec ->
                let seeds = spec.Seeds

                let! entities =
                  seeds.Entities
                  |> Map.tryFindWithError entity "entities" (fun () -> entity.EntityName) ()

                let! entityValue = entities |> Map.tryFindWithError id "entity" (fun () -> $"{id} in {entity}") ()

                let! updater = delta |> Delta.ToUpdater onDeltaExt

                let! updated = updater entityValue

                let entities = entities |> Map.add id updated

                seeds
                |> SpecData.Updaters.Entities(Map.add entity entities)
                |> replaceWith
                |> storeUpdater

                return ()
            }
        Delete =
          fun entityName id ->
            sum {
              match store.TryGetValue(specName) with
              | false, _ -> return! sum.Throw(Errors.Singleton () (fun () -> $"Store doesn't contain spec {specName}"))
              | true, spec ->
                let seeds = spec.Seeds

                let! entities =
                  seeds.Entities
                  |> Map.tryFindWithError entityName "entities" (fun () -> entityName.EntityName) ()

                let! _entity =
                  entities
                  |> Map.tryFindWithError id "entity" (fun () -> $"{id} in {entityName}") ()

                let entities = entities |> Map.remove id

                seeds
                |> SpecData.Updaters.Entities(Map.add entityName entities)
                |> replaceWith
                |> storeUpdater

                return ()
            } }

    let link: LookupName -> Guid * Guid -> Sum<unit, Errors<unit>> =
      fun lookup (sourceId, targetId) ->
        sum {
          match store.TryGetValue(specName) with
          | false, _ -> return! sum.Throw(Errors.Singleton () (fun () -> $"Store doesn't contain spec {specName}"))
          | true, spec ->
            let seeds = spec.Seeds

            let! v2 =
              (WorkspaceVariant.WithPath path spec.WorkspaceVariant, spec.Folders)
              ||> Ballerina.Data.Schema.Model.Schema.FromJsonVirtualFolder
              |> sum.MapError(Errors.MapContext(replaceWith ()))


            let! lookupDescriptor =
              v2.Lookups
              |> Map.tryFindWithError lookup "lookup descriptors" (fun () -> lookup.LookupName) ()

            let! lookups =
              seeds.Lookups
              |> Map.tryFindWithError lookup "lookups" (fun () -> lookup.LookupName) ()

            let! sourceLookup =
              lookups
              |> Map.tryFind sourceId
              |> Sum.fromOption (fun () -> Errors.Singleton () (fun () -> "source lookup for link cannot be found"))

            let sourceLookup = sourceLookup |> Set.add targetId
            let lookups = lookups |> Map.add sourceId sourceLookup


            match lookupDescriptor.Backward with
            | None ->
              seeds
              |> SpecData.Updaters.Lookups(Map.add lookup lookups)
              |> replaceWith
              |> storeUpdater

              return ()
            | Some(backwardLookupName, _) ->

              let! backwardLookup =
                seeds.Lookups
                |> Map.tryFindWithError backwardLookupName "lookups" (fun () -> backwardLookupName.LookupName) ()

              let! targetLookup =
                backwardLookup
                |> Map.tryFindWithError sourceId lookup.LookupName (fun () -> $"{targetId}") ()

              let targetLookup = targetLookup |> Set.add sourceId
              let backwardLookup = backwardLookup |> Map.add targetId targetLookup

              seeds
              |> SpecData.Updaters.Lookups(Map.add backwardLookupName backwardLookup)
              |> replaceWith
              |> storeUpdater

              return ()
        }

    let unlink: LookupName -> Guid * Guid -> Sum<unit, Errors<unit>> =
      fun lookupName (sourceId, targetId) ->
        sum {
          match store.TryGetValue(specName) with
          | false, _ -> return! sum.Throw(Errors.Singleton () (fun () -> $"Store doesn't contain spec {specName}"))
          | true, spec ->
            let! v2 =
              (WorkspaceVariant.WithPath path spec.WorkspaceVariant, spec.Folders)
              ||> Ballerina.Data.Schema.Model.Schema.FromJsonVirtualFolder
              |> sum.MapError(Errors.MapContext(replaceWith ()))

            let seeds = spec.Seeds

            let! lookupDescriptor =
              v2.Lookups
              |> Map.tryFindWithError lookupName "lookup descriptors" (fun () -> lookupName.LookupName) ()

            let! lookups =
              seeds.Lookups
              |> Map.tryFindWithError lookupName "lookups" (fun () -> lookupName.LookupName) ()

            let! sourceLookup =
              lookups
              |> Map.tryFindWithError sourceId lookupName.LookupName (fun () -> $"{sourceId}") ()

            let sourceLookup = sourceLookup |> Set.remove targetId
            let lookups = lookups |> Map.add sourceId sourceLookup

            return!
              sum {
                let seeds = store[specName].Seeds

                match lookupDescriptor.Backward with
                | None ->
                  seeds
                  |> SpecData.Updaters.Lookups(Map.add lookupName lookups)
                  |> replaceWith
                  |> storeUpdater

                  return ()
                | Some(backwardLookupName, _) ->
                  let! backwardLookup =
                    seeds.Lookups
                    |> Map.tryFindWithError backwardLookupName "lookups" (fun () -> backwardLookupName.LookupName) ()

                  let! targetLookup =
                    backwardLookup
                    |> Map.tryFindWithError targetId lookupName.LookupName (fun () -> $"{targetId}") ()

                  let targetLookup = targetLookup |> Set.remove sourceId
                  let backwardLookup = backwardLookup |> Map.add targetId targetLookup

                  let! _seeds =
                    seeds
                    |> SpecData.Updaters.Lookups(Map.add backwardLookupName backwardLookup)
                    |> SpecData.Updaters.Lookups(Map.add lookupName lookups)
                    |> replaceWith
                    |> storeUpdater

                  return ()
              }
        }

    let unlinkFrom: LookupName -> Guid -> Sum<unit, Errors<unit>> =
      fun lookupName sourceId ->
        sum {
          match store.TryGetValue(specName) with
          | false, _ -> return! sum.Throw(Errors.Singleton () (fun () -> $"Store doesn't contain spec {specName}"))
          | true, spec ->

            let! v2 =
              (WorkspaceVariant.WithPath path spec.WorkspaceVariant, spec.Folders)
              ||> Ballerina.Data.Schema.Model.Schema.FromJsonVirtualFolder
              |> sum.MapError(Errors.MapContext(replaceWith ()))

            let seeds = spec.Seeds

            let! lookupDescriptor =
              v2.Lookups
              |> Map.tryFindWithError lookupName "lookup descriptors" (fun () -> lookupName.LookupName) ()

            let! lookups =
              seeds.Lookups
              |> Map.tryFindWithError lookupName "lookups" (fun () -> lookupName.LookupName) ()

            let! sourceLookup =
              lookups
              |> Map.tryFindWithError sourceId lookupName.LookupName (fun () -> $"{sourceId}") ()

            let lookups = lookups |> Map.add sourceId Set.empty

            return!
              sum {
                let seeds = spec.Seeds

                match lookupDescriptor.Backward with
                | None ->
                  let! _updated =
                    seeds
                    |> SpecData.Updaters.Lookups(Map.add lookupName lookups)
                    |> replaceWith
                    |> storeUpdater

                  return ()
                | Some(backwardLookupName, _) ->
                  let! backwardLookup =
                    seeds.Lookups
                    |> Map.tryFindWithError backwardLookupName "lookups" (fun () -> backwardLookupName.LookupName) ()

                  let! _ =
                    sourceLookup
                    |> Seq.map (fun targetId ->
                      sum {
                        let! targetLookup =
                          backwardLookup
                          |> Map.tryFindWithError targetId lookupName.LookupName (fun () -> $"{targetId}") ()

                        let targetLookup = targetLookup |> Set.remove sourceId
                        let backwardLookup = backwardLookup |> Map.add targetId targetLookup

                        let! _updated =
                          seeds
                          |> SpecData.Updaters.Lookups(Map.add backwardLookupName backwardLookup)
                          |> SpecData.Updaters.Lookups(Map.add lookupName lookups)
                          |> replaceWith
                          |> storeUpdater

                        return ()
                      })
                    |> sum.All

                  return ()

              }
        }

    let lookupsApi =
      { GetMany =
          fun lookupName (id, (from, count)) ->
            sum {
              let specState = store[specName]
              let seeds = specState.Seeds

              let! lookupDescriptor =
                seeds.Lookups
                |> Map.tryFindWithError lookupName "lookup descriptors" (fun () -> lookupName.LookupName) ()

              let lookup = lookupDescriptor |> Map.tryFind id //"lookups with id" $"{id}"

              match lookup with
              | None -> return {| Values = []; HasMore = false |}
              | Some set when set.IsEmpty -> return {| Values = []; HasMore = false |}
              | Some lookup ->
                let entities =
                  seeds.Entities
                  |> Map.values
                  |> _.ToArray()
                  |> Seq.map Map.toSeq
                  |> Seq.concat
                  |> Map.ofSeq

                let targetIds = lookup |> Set.toSeq |> Seq.skip from |> Seq.take count

                let! targetValues =
                  targetIds
                  |> Seq.map (fun targetId ->
                    entities |> Map.tryFindWithError targetId "target" (fun () -> $"{targetId}") ())
                  |> sum.All

                return
                  {| Values = targetValues
                     HasMore = from + count < List.length targetValues |}
            }
        Create =
          fun lookupName (sourceId, newTarget) ->
            sum {
              let specState = store[specName]

              let! v2 =
                (WorkspaceVariant.WithPath path specState.WorkspaceVariant, specState.Folders)
                ||> Ballerina.Data.Schema.Model.Schema.FromJsonVirtualFolder
                |> sum.MapError(Errors.MapContext(replaceWith ()))


              let seeds = specState.Seeds

              let! lookupDescriptor =
                v2.Lookups
                |> Map.tryFindWithError lookupName "lookup descriptors" (fun () -> lookupName.LookupName) ()

              let! entities =
                seeds.Entities
                |> Map.tryFindWithError
                  lookupDescriptor.Source
                  "entities"
                  (fun () -> lookupDescriptor.Source.EntityName)
                  ()

              let targetId = Guid.CreateVersion7()
              let entities = entities |> Map.add targetId newTarget

              do! link lookupName (sourceId, targetId)

              let! _updated =
                seeds
                |> SpecData.Updaters.Entities(Map.add lookupDescriptor.Source entities)
                |> replaceWith
                |> storeUpdater

              return targetId
            }
        Delete =
          fun lookup (sourceId, targetId) ->
            sum {
              do! unlink lookup (sourceId, targetId)
              let specState = store[specName]

              let! v2 =
                (WorkspaceVariant.WithPath path specState.WorkspaceVariant, specState.Folders)
                ||> Ballerina.Data.Schema.Model.Schema.FromJsonVirtualFolder
                |> sum.MapError(Errors.MapContext(replaceWith ()))

              let seeds = specState.Seeds

              let! lookupDescriptor =
                v2.Lookups
                |> Map.tryFindWithError lookup "lookup descriptors" (fun () -> lookup.LookupName) ()

              let! targetEntities =
                seeds.Entities
                |> Map.tryFindWithError
                  lookupDescriptor.Target
                  "entities"
                  (fun () -> lookupDescriptor.Target.EntityName)
                  ()

              let targetEntities = targetEntities |> Map.remove targetId

              let! lookups =
                seeds.Lookups
                |> Map.tryFindWithError lookup "lookups" (fun () -> lookup.LookupName) ()

              let! sourceLookup =
                lookups
                |> Map.tryFindWithError sourceId lookup.LookupName (fun () -> $"{sourceId}") ()

              let sourceLookup = sourceLookup |> Set.remove targetId
              let lookups = lookups |> Map.add sourceId sourceLookup

              let! _updated =
                seeds
                |> SpecData.Updaters.Entities(Map.add lookupDescriptor.Target targetEntities)
                |> SpecData.Updaters.Lookups(Map.add lookup lookups)
                |> replaceWith
                |> storeUpdater

              return ()
            }
        Link = link
        Unlink = unlink
        UnlinkFrom = unlinkFrom }

    { Entities = entitiesApi
      Lookups = lookupsApi }

  let private error (msg: string) =
    Errors.Singleton () (fun () -> msg) |> sum.Throw

  let makeSpecsApi
    (store: ConcurrentDictionary<SpecName, Spec<TypeValue<ValueExt<'ext>>, ValueExt<'ext>>>)
    : SpecApi<TypeValue<ValueExt<'ext>>, ValueExt<'ext>> =
    { Get =
        fun specName ->
          sum {
            match store.TryGetValue specName with
            | true, value -> return value
            | false, _ -> return! error $"SpecApi: {specName.SpecName} does not exists in store"
          }
      Create =
        fun name spec ->
          sum {
            match store.TryGetValue name with
            | false, _ ->
              let success =
                store.TryAdd(
                  name,
                  spec

                )

              match success with
              | true -> return ()
              | false ->
                return! error $"SpecApi: '{name}' already exists in store — likely added concurrently while requesting"
            | true, _ -> return! error $"SpecApi: {name} already exists in store"
          }
      Delete =
        fun specName ->
          sum {
            match store.TryGetValue specName with
            | true, _ ->
              let success, _ = store.TryRemove specName

              match success with
              | true -> return ()
              | false ->
                return!
                  error
                    $"SpecApi Delete: '{specName.SpecName}' found but could not be removed — possible concurrent modification"
            | false, _ -> return! error $"SpecApi Delete: '{specName.SpecName}' does not exist in store"
          }
      Update =
        fun (name: SpecName) (spec: Spec<TypeValue<ValueExt<'ext>>, ValueExt<'ext>>) ->
          sum {
            match store.TryGetValue name with
            | true, current ->
              let updated = spec
              let success = store.TryUpdate(name, updated, current)

              match success with
              | true -> return ()
              | false -> return! error $"SpecApi Update: '{name}' was modified concurrently and could not be updated."
            | false, _ -> return! error $"SpecApi Update: '{name}' does not exist in store"
          }
      List = fun () -> sum { return store.Keys |> Seq.toList } }

  let seed
    (store: ConcurrentDictionary<SpecName, Spec<TypeValue<ValueExt<'ext>>, ValueExt<'ext>>>)
    (seeder:
      Schema<TypeValue<ValueExt<'ext>>, ResolvedIdentifier, ValueExt<'ext>>
        -> Sum<SpecData<TypeValue<ValueExt<'ext>>, ValueExt<'ext>>, Errors<unit>>)
    (name: SpecName)
    (path: VirtualPath option)
    : State<
        SpecData<TypeValue<ValueExt<'ext>>, ValueExt<'ext>>,
        TypeCheckContext<ValueExt<'ext>>,
        TypeCheckState<ValueExt<'ext>>,
        Errors<unit>
       >
    =

    state {
      match store.ContainsKey name with
      | true ->
        let specState = store[name]

        let specStateWithVariant =
          { specState with
              WorkspaceVariant = WorkspaceVariant.WithPath path specState.WorkspaceVariant }

        let! v2 =
          (specStateWithVariant.WorkspaceVariant, specStateWithVariant.Folders)
          ||> Ballerina.Data.Schema.Model.Schema.FromJsonVirtualFolder
          |> sum.MapError(Errors.MapContext(replaceWith ()))
          |> state.OfSum

        let! schemaValues =
          v2
          |> Ballerina.Data.Schema.Model.Schema.SchemaEval()
          |> state.MapError(Errors.MapContext(replaceWith ()))

        let! seeds = seeder schemaValues |> state.OfSum
        let updated = { specState with Seeds = seeds }
        let result = store.TryUpdate(name, updated, specState)

        if result then
          return seeds
        else
          return! state.Throw(Errors.Singleton () (fun () -> $"Updating spec {name} has failed"))
      | _ -> return! state.Throw(Errors.Singleton () (fun () -> $"Can't create seeds for un-existing spec: {name}"))
    }

  let seedWith
    (store: ConcurrentDictionary<SpecName, Spec<TypeValue<ValueExt<'ext>>, ValueExt<'ext>>>)
    (name: SpecName)
    (seeds: SpecData<TypeValue<ValueExt<'ext>>, ValueExt<'ext>>)
    : Sum<unit, Errors<unit>> =

    sum {

      match store.ContainsKey name with
      | true ->
        let specState = store[name]

        let updated = { specState with Seeds = seeds }

        let result = store.TryUpdate(name, updated, specState)

        if result then
          return ()
        else
          return! sum.Throw(Errors.Singleton () (fun () -> $"SeedsWith failed. Cannot update the spec {name}"))
      | false ->
        return! sum.Throw(Errors.Singleton () (fun () -> $"SeedsWith failed. Spec {name} not present in the store"))
    }

  let getSeeds
    (store: ConcurrentDictionary<SpecName, Spec<TypeValue<ValueExt<'ext>>, ValueExt<'ext>>>)
    (specName: SpecName)
    =
    sum {
      match store.TryGetValue specName with
      | false, _ -> return! error $"SpecApi GetSeeds: '{specName.SpecName}' does not exist in store"
      | true, spec -> return spec.Seeds
    }

namespace Ballerina.Data.Store.InMemory

open Ballerina
open Ballerina.Collections.Sum
open Ballerina.DSL.Next.StdLib.Extensions
open Ballerina.DSL.Next.StdLib.MutableMemoryDB
open Ballerina.Data.Store.InMemory.Backend
open Ballerina.Data.Store.Model
open Ballerina.LocalizedErrors
open Ballerina.Errors
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Types.Model

module Store =
  let create
    (initialTenantIds: TenantId list)
    (onDeltaExt:
      DeltaExt<'runtimeContext, 'db, 'ext>
        -> Value<TypeValue<ValueExt<'runtimeContext, 'db, 'ext>>, ValueExt<'runtimeContext, 'db, 'ext>>
        -> Sum<
          Value<TypeValue<ValueExt<'runtimeContext, 'db, 'ext>>, ValueExt<'runtimeContext, 'db, 'ext>>,
          Errors<Unit>
         >)
    _db_query_sym
    _make_db_query_type
    : Store<'runtimeContext, 'db, 'ext> =

    let store = ConcurrentStore.initStore initialTenantIds

    { Specs =
        { GetDataApi =
            fun tenantId specName path ->
              sum {
                let! _tenantExists =
                  store.Tenants.Keys
                  |> Seq.tryFind ((=) tenantId)
                  |> sum.OfOption(
                    Errors.Singleton () (fun () ->
                      $"Can't use SpecData API. Spec store is not setup for the tenant {tenantId}")
                  )

                return ConcurrentStore.makeSpecApi store.Tenants[tenantId] specName path onDeltaExt
              }
          GetSpecApi =
            fun tenantId ->
              sum {
                let! _tenantExists =
                  store.Tenants.Keys
                  |> Seq.tryFind ((=) tenantId)
                  |> sum.OfOption(
                    Errors.Singleton () (fun () ->
                      $"Can't use Spec API. Spec store is not setup for the tenant {tenantId}")
                  )

                return ConcurrentStore.makeSpecsApi store.Tenants[tenantId]
              } }
      Workspace =
        { SeedSpecEval =
            fun tenantId specName seeder path ->
              let specState = store.Tenants[tenantId]
              ConcurrentStore.seed specState seeder specName path _db_query_sym _make_db_query_type
          SeedSpec =
            fun (tenantId, specName, seeds) -> ConcurrentStore.seedWith (store.Tenants[tenantId]) specName seeds
          GetSeeds = fun tenantId -> ConcurrentStore.getSeeds (store.Tenants[tenantId]) }
      Tenants = { ListTenants = fun () -> store.Tenants.Keys |> Seq.toList } }

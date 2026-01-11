namespace Ballerina.Data.Store.InMemory

open Ballerina.Collections.Sum
open Ballerina.DSL.Next.StdLib.Extensions
open Ballerina.Data.Delta.Extensions
open Ballerina.Data.Store.InMemory.Backend
open Ballerina.Data.Store.Model
open Ballerina.LocalizedErrors
open Ballerina.DSL.Next.Terms.Model
open Ballerina.DSL.Next.Types.Model

module Store =
  let create
    (initialTenantIds: TenantId list)
    (onDeltaExt:
      DeltaExt
        -> Value<TypeValue<ValueExt>, ValueExt>
        -> Sum<Value<TypeValue<ValueExt>, ValueExt>, Ballerina.Errors.Errors>)
    : Store =

    let store = ConcurrentStore.initStore initialTenantIds

    { Specs =
        { GetDataApi =
            fun tenantId specName path ->
              sum {
                let! _tenantExists =
                  store.Tenants.Keys
                  |> Seq.tryFind ((=) tenantId)
                  |> sum.OfOption(
                    Errors.Singleton(
                      Location.Unknown,
                      $"Can't use SpecData API. Spec store is not setup for the tenant {tenantId}"
                    )
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
                    Errors.Singleton(
                      Location.Unknown,
                      $"Can't use Spec API. Spec store is not setup for the tenant {tenantId}"
                    )
                  )

                return ConcurrentStore.makeSpecsApi store.Tenants[tenantId]
              } }
      Workspace =
        { SeedSpecEval =
            fun tenantId specName seeder path ->
              let specState = store.Tenants[tenantId]
              ConcurrentStore.seed specState seeder specName path
          SeedSpec =
            fun (tenantId, specName, seeds) -> ConcurrentStore.seedWith (store.Tenants[tenantId]) specName seeds
          GetSeeds = fun tenantId -> ConcurrentStore.getSeeds (store.Tenants[tenantId]) }
      Tenants = { ListTenants = fun () -> store.Tenants.Keys |> Seq.toList } }

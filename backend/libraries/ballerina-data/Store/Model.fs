namespace Ballerina.Data.Store

module Model =

  open System
  open Ballerina.Collections.Sum
  open Ballerina.LocalizedErrors
  open Ballerina.State.WithError
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.Data.Spec.Model
  open Ballerina.Data.Store.Api.Model
  open Ballerina.Data.Schema.Model
  open Ballerina.DSL.Next.StdLib.Extensions
  open Ballerina.DSL.Next.Types
  open Ballerina.Data.Delta.Extensions
  open Ballerina.VirtualFolders.Model

  type TenantId = TenantId of Guid

  type Seeder =
    Schema<TypeValue<ValueExt>, ResolvedIdentifier, ValueExt> -> Sum<SpecData<TypeValue<ValueExt>, ValueExt>, Errors>

  type TenantStore = { ListTenants: unit -> TenantId list }

  type SpecsStore =
    { GetSpecApi: TenantId -> Sum<SpecApi<TypeValue<ValueExt>, ValueExt>, Errors>
      GetDataApi: TenantId -> SpecName -> VirtualPath option -> Sum<SpecDataApi<ValueExt, DeltaExt>, Errors> }

  type Workspace =
    { SeedSpecEval:
        TenantId
          -> SpecName
          -> Seeder
          -> VirtualPath option
          -> State<SpecData<TypeValue<ValueExt>, ValueExt>, TypeCheckContext<ValueExt>, TypeCheckState<ValueExt>, Errors>
      SeedSpec: TenantId * SpecName * SpecData<TypeValue<ValueExt>, ValueExt> -> Sum<unit, Errors>
      GetSeeds: TenantId -> SpecName -> Sum<SpecData<TypeValue<ValueExt>, ValueExt>, Errors> }

  and Store =
    { Specs: SpecsStore
      Tenants: TenantStore
      Workspace: Workspace }

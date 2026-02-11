namespace Ballerina.Data.Store

module Model =

  open System
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.State.WithError
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.Data.Spec.Model
  open Ballerina.Data.Store.Api.Model
  open Ballerina.Data.Schema.Model
  open Ballerina.DSL.Next.StdLib.Extensions
  open Ballerina.DSL.Next.Types
  open Ballerina.VirtualFolders.Model

  type TenantId = TenantId of Guid

  type Seeder<'ext when 'ext: comparison> =
    Schema<TypeValue<ValueExt<'ext>>, ResolvedIdentifier, ValueExt<'ext>>
      -> Sum<SpecData<TypeValue<ValueExt<'ext>>, ValueExt<'ext>>, Errors<unit>>

  type TenantStore = { ListTenants: unit -> TenantId list }

  type SpecsStore<'ext when 'ext: comparison> =
    { GetSpecApi: TenantId -> Sum<SpecApi<TypeValue<ValueExt<'ext>>, ValueExt<'ext>>, Errors<unit>>
      GetDataApi:
        TenantId -> SpecName -> VirtualPath option -> Sum<SpecDataApi<ValueExt<'ext>, DeltaExt<'ext>>, Errors<unit>> }

  type Workspace<'ext when 'ext: comparison> =
    { SeedSpecEval:
        TenantId
          -> SpecName
          -> Seeder<'ext>
          -> VirtualPath option
          -> State<
            SpecData<TypeValue<ValueExt<'ext>>, ValueExt<'ext>>,
            TypeCheckContext<ValueExt<'ext>>,
            TypeCheckState<ValueExt<'ext>>,
            Errors<unit>
           >
      SeedSpec: TenantId * SpecName * SpecData<TypeValue<ValueExt<'ext>>, ValueExt<'ext>> -> Sum<unit, Errors<unit>>
      GetSeeds: TenantId -> SpecName -> Sum<SpecData<TypeValue<ValueExt<'ext>>, ValueExt<'ext>>, Errors<unit>> }

  and Store<'ext when 'ext: comparison> =
    { Specs: SpecsStore<'ext>
      Tenants: TenantStore
      Workspace: Workspace<'ext> }

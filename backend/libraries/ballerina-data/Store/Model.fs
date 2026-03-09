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
  open Ballerina.DSL.Next.StdLib.DB

  type TenantId = TenantId of Guid

  type Seeder<'runtimeContext, 'db, 'ext when 'db: comparison and 'ext: comparison> =
    Schema<TypeValue<ValueExt<'runtimeContext, 'db, 'ext>>, ResolvedIdentifier, ValueExt<'runtimeContext, 'db, 'ext>>
      -> Sum<
        SpecData<TypeValue<ValueExt<'runtimeContext, 'db, 'ext>>, ValueExt<'runtimeContext, 'db, 'ext>>,
        Errors<unit>
       >

  type TenantStore = { ListTenants: unit -> TenantId list }

  type SpecsStore<'runtimeContext, 'db, 'ext when 'db: comparison and 'ext: comparison> =
    { GetSpecApi:
        TenantId
          -> Sum<
            SpecApi<TypeValue<ValueExt<'runtimeContext, 'db, 'ext>>, ValueExt<'runtimeContext, 'db, 'ext>>,
            Errors<unit>
           >
      GetDataApi:
        TenantId
          -> SpecName
          -> VirtualPath option
          -> Sum<SpecDataApi<ValueExt<'runtimeContext, 'db, 'ext>, DeltaExt<'runtimeContext, 'db, 'ext>>, Errors<unit>> }

  type Workspace<'runtimeContext, 'db, 'ext when 'db: comparison and 'ext: comparison> =
    { SeedSpecEval:
        TenantId
          -> SpecName
          -> Seeder<'runtimeContext, 'db, 'ext>
          -> VirtualPath option
          -> State<
            SpecData<TypeValue<ValueExt<'runtimeContext, 'db, 'ext>>, ValueExt<'runtimeContext, 'db, 'ext>>,
            TypeCheckContext<ValueExt<'runtimeContext, 'db, 'ext>>,
            TypeCheckState<ValueExt<'runtimeContext, 'db, 'ext>>,
            Errors<unit>
           >
      SeedSpec:
        TenantId *
        SpecName *
        SpecData<TypeValue<ValueExt<'runtimeContext, 'db, 'ext>>, ValueExt<'runtimeContext, 'db, 'ext>>
          -> Sum<unit, Errors<unit>>
      GetSeeds:
        TenantId
          -> SpecName
          -> Sum<
            SpecData<TypeValue<ValueExt<'runtimeContext, 'db, 'ext>>, ValueExt<'runtimeContext, 'db, 'ext>>,
            Errors<unit>
           > }

  and Store<'runtimeContext, 'db, 'ext when 'db: comparison and 'ext: comparison> =
    { Specs: SpecsStore<'runtimeContext, 'db, 'ext>
      Tenants: TenantStore
      Workspace: Workspace<'runtimeContext, 'db, 'ext> }

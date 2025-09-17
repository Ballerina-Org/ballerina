namespace Ballerina.Data.Store

open Ballerina.DSL.Next.Types

module Model =

  open System
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.State.WithError
  open Ballerina.DSL.Next.Types.Eval
  open Ballerina.Data.Spec.Model
  open Ballerina.Data.Spec.Api.Model
  open Ballerina.Data.Store.Api.Model
  open Ballerina.Data.Schema.Model
  open FSharp.Data

  type TenantId = TenantId of Guid

  type Seeds<'T, 'valueExtension> = SpecData<'T, 'valueExtension>

  type Seeder<'T, 'valueExtension> = Schema<'T> -> Sum<Seeds<'T, 'valueExtension>, Errors>

  type SpecState<'T, 'valueExtension> =
    { Spec: Spec
      Data: SpecData<'T, 'valueExtension>
      Bridge: JsonValue option
      Config: Config }

  type TenantStore = { ListTenants: unit -> TenantId list }

  type SpecsStore<'valueExtension> =
    { GetSpecApi: TenantId -> SpecApi
      GetDataApi: TenantId -> SpecName -> SpecDataApi<'valueExtension> }

  type Workspace<'T, 'valueExtension> =
    { SeedSpecEval:
        TenantId * Spec * Seeder<TypeValue, 'valueExtension> * TypeExprEvalState
          -> State<unit, TypeExprEvalContext, TypeExprEvalState, Errors>
      SeedSpec: TenantId * Spec * SpecData<'T, 'valueExtension> -> Sum<unit, Errors>
      GetSeeds: TenantId -> SpecName -> Sum<SpecData<'T, 'valueExtension>, Errors> }

  and Store<'T, 'valueExtension> =
    { Specs: SpecsStore<'valueExtension>
      Tenants: TenantStore
      Workspace: Workspace<'T, 'valueExtension> }

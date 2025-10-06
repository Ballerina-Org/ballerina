namespace Ballerina.Data.Store

module Model =

  open System
  open Ballerina.Collections.Sum
  open Ballerina.LocalizedErrors
  open Ballerina.State.WithError
  open Ballerina.DSL.Next.Types.Eval
  open Ballerina.Data.Spec.Model
  open Ballerina.Data.Store.Api.Model
  open Ballerina.Data.Schema.Model
  open Ballerina.DSL.Next.StdLib.Extensions
  open Ballerina.DSL.Next.Types

  type TenantId = TenantId of Guid

  type Seeds = SpecData<TypeValue, ValueExt>

  type Seeder = Schema<TypeValue> -> Sum<Seeds, Errors>

  type TenantStore = { ListTenants: unit -> TenantId list }

  type SpecsStore =
    { GetSpecApi: TenantId -> Sum<SpecApi<TypeValue, ValueExt>, Errors>
      GetDataApi: TenantId -> SpecName -> Sum<SpecDataApi<ValueExt>, Errors> }

  type Workspace =
    { SeedSpecEval:
        TenantId
          -> SpecName
          -> Seeder
          -> TypeExprEvalState
          -> State<Seeds, TypeExprEvalContext, TypeExprEvalState, Errors>
      SeedSpec: TenantId * SpecName * SpecData<TypeValue, ValueExt> -> Sum<unit, Errors>
      GetSeeds: TenantId -> SpecName -> Sum<Seeds, Errors> }

  and Store =
    { Specs: SpecsStore
      Tenants: TenantStore
      Workspace: Workspace }

namespace Ballerina.Data.Store

open Ballerina.VirtualFolders.Model

module Model =

  open System
  open Ballerina.Collections.Sum
  open Ballerina.LocalizedErrors
  open Ballerina.State.WithError
  open Ballerina.DSL.Next.Types.TypeChecker.Eval
  open Ballerina.DSL.Next.Types.TypeChecker.Model
  open Ballerina.DSL.Next.Types.TypeChecker
  open Ballerina.Data.Spec.Model
  open Ballerina.Data.Store.Api.Model
  open Ballerina.Data.Schema.Model
  open Ballerina.DSL.Next.StdLib.Extensions
  open Ballerina.DSL.Next.Types

  type TenantId = TenantId of Guid

  type Seeder = Schema<TypeValue, ResolvedIdentifier, ValueExt> -> Sum<SpecData<TypeValue, ValueExt>, Errors>

  type TenantStore = { ListTenants: unit -> TenantId list }

  type SpecsStore =
    { GetSpecApi: TenantId -> Sum<SpecApi<TypeValue, ValueExt>, Errors>
      GetDataApi: TenantId -> SpecName -> VirtualPath option -> Sum<SpecDataApi<ValueExt>, Errors> }

  type Workspace =
    { SeedSpecEval:
        TenantId
          -> SpecName
          -> Seeder
          -> VirtualPath option
          -> State<SpecData<TypeValue, ValueExt>, TypeExprEvalContext, TypeExprEvalState, Errors>
      SeedSpec: TenantId * SpecName * SpecData<TypeValue, ValueExt> -> Sum<unit, Errors>
      GetSeeds: TenantId -> SpecName -> Sum<SpecData<TypeValue, ValueExt>, Errors> }

  and Store =
    { Specs: SpecsStore
      Tenants: TenantStore
      Workspace: Workspace }

namespace Ballerina.Data.Api

module Model =

  open System
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.Data.Delta.Model
  open Ballerina.DSL.Next.Types.Model
  open Ballerina.Data.Spec

  type Value = Ballerina.DSL.Next.Terms.Model.Value<TypeValue>

  type EntitiesApi =
    { Get: string -> Guid -> Sum<Value, Errors>
      GetMany: string -> int * int -> Sum<{| Values: List<Value>; HasMore: bool |}, Errors>
      Create: string -> Value -> Sum<Guid, Errors>
      Update: string -> Guid * Delta -> Sum<Unit, Errors>
      Delete: string -> Guid -> Sum<Unit, Errors> }

  type LookupsApi =
    { GetMany: string -> Guid * (int * int) -> Sum<{| Values: List<Value>; HasMore: bool |}, Errors>
      Create: string -> Guid * Value -> Sum<Guid, Errors>
      Delete: string -> Guid * Guid -> Sum<Unit, Errors>
      Link: string -> Guid * Guid -> Sum<Unit, Errors>
      Unlink: string -> Guid * Guid -> Sum<Unit, Errors> }

  type SpecApi =
    { Get: SpecName -> Guid -> Sum<List<Value>, Errors>
      GetMany: SpecName -> Sum<List<Guid * Value>, Errors>
      Create: SpecName -> Value -> Sum<Guid, Errors>
      Delete: SpecName -> Guid -> Sum<Unit, Errors>
      Update: SpecName -> Guid -> SpecName -> Sum<Unit, Errors>
      Version: SpecName -> Guid -> Sum<unit, Errors> }

  type Api =
    { Entities: EntitiesApi
      Lookups: LookupsApi }
//Specs: SpecApi }

namespace Ballerina.Data.Store.Api

module Model =

  open System
  open Ballerina.Collections.Sum
  open Ballerina.Errors
  open Ballerina.Data.Delta.Model
  open Ballerina.DSL.Next.Types.Model

  type Value<'valueExtension> = Ballerina.DSL.Next.Terms.Model.Value<TypeValue, 'valueExtension>

  type EntitiesApi<'valueExtension> =
    { Get: string -> Guid -> Sum<Value<'valueExtension>, Errors>
      GetMany:
        string
          -> int * int
          -> Sum<
            {| Values: List<Guid * Value<'valueExtension>>
               HasMore: bool |},
            Errors
           >
      Create: string -> Value<'valueExtension> -> Sum<Guid, Errors>
      Update: string -> Guid * Delta<'valueExtension> -> Sum<Unit, Errors>
      Delete: string -> Guid -> Sum<Unit, Errors> }

  type LookupsApi<'valueExtension> =
    { GetMany:
        string
          -> Guid * (int * int)
          -> Sum<
            {| Values: List<Value<'valueExtension>>
               HasMore: bool |},
            Errors
           >
      Create: string -> Guid * Value<'valueExtension> -> Sum<Guid, Errors>
      Delete: string -> Guid * Guid -> Sum<Unit, Errors>
      Link: string -> Guid * Guid -> Sum<Unit, Errors>
      Unlink: string -> Guid * Guid -> Sum<Unit, Errors> }

  type SpecDataApi<'valueExtension> =
    { Entities: EntitiesApi<'valueExtension>
      Lookups: LookupsApi<'valueExtension> }

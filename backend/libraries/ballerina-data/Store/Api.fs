namespace Ballerina.Data.Store.Api

module Model =

  open System
  open Ballerina.Collections.Sum
  open Ballerina.LocalizedErrors
  open Ballerina.Data.Delta.Model
  open Ballerina.Data.Spec.Model
  open Ballerina.Data.Schema.Model
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Types.Model

  type SpecApi<'T, 'valueExtension> =
    { Get: SpecName -> Sum<Spec<'T, 'valueExtension>, Errors>
      Create: SpecName -> Spec<'T, 'valueExtension> -> Sum<unit, Errors>
      Delete: SpecName -> Sum<Unit, Errors>
      Update: SpecName -> Spec<'T, 'valueExtension> -> Sum<Unit, Errors>
      List: unit -> Sum<SpecName list, Errors> }

  type EntitiesApi<'valueExtension, 'deltaExtension> =
    { Get: EntityName -> Guid -> Sum<Value<TypeValue<'valueExtension>, 'valueExtension>, Errors>
      GetMany:
        EntityName
          -> int * int
          -> Sum<
            {| Values: List<Guid * Value<TypeValue<'valueExtension>, 'valueExtension>>
               HasMore: bool |},
            Errors
           >
      Create: EntityName -> Value<TypeValue<'valueExtension>, 'valueExtension> -> Sum<Guid, Errors>
      Update: EntityName -> Guid * Delta<'valueExtension, 'deltaExtension> -> Sum<Unit, Errors>
      Delete: EntityName -> Guid -> Sum<Unit, Errors> }

  type LookupsApi<'valueExtension> =
    { GetMany:
        LookupName
          -> Guid * (int * int)
          -> Sum<
            {| Values: List<Value<TypeValue<'valueExtension>, 'valueExtension>>
               HasMore: bool |},
            Errors
           >
      Create: LookupName -> Guid * Value<TypeValue<'valueExtension>, 'valueExtension> -> Sum<Guid, Errors>
      Delete: LookupName -> Guid * Guid -> Sum<Unit, Errors>
      Link: LookupName -> Guid * Guid -> Sum<Unit, Errors>
      Unlink: LookupName -> Guid * Guid -> Sum<Unit, Errors>
      UnlinkFrom: LookupName -> Guid -> Sum<Unit, Errors> }

  type SpecDataApi<'valueExtension, 'deltaExtension> =
    { Entities: EntitiesApi<'valueExtension, 'deltaExtension>
      Lookups: LookupsApi<'valueExtension> }

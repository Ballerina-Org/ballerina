namespace Ballerina.Data.Store.Api

module Model =

  open System
  open Ballerina
  open Ballerina.Collections.Sum
  open Ballerina.LocalizedErrors
  open Ballerina.Errors
  open Ballerina.Data.Delta.Model
  open Ballerina.Data.Spec.Model
  open Ballerina.Data.Schema.Model
  open Ballerina.DSL.Next.Terms.Model
  open Ballerina.DSL.Next.Types.Model

  type SpecApi<'T, 'valueExtension> =
    { Get: SpecName -> Sum<Spec<'T, 'valueExtension>, Errors<unit>>
      Create: SpecName -> Spec<'T, 'valueExtension> -> Sum<unit, Errors<unit>>
      Delete: SpecName -> Sum<Unit, Errors<unit>>
      Update: SpecName -> Spec<'T, 'valueExtension> -> Sum<Unit, Errors<unit>>
      List: unit -> Sum<SpecName list, Errors<unit>> }

  type EntitiesApi<'valueExtension, 'deltaExtension> =
    { Get: EntityName -> Guid -> Sum<Value<TypeValue<'valueExtension>, 'valueExtension>, Errors<unit>>
      GetMany:
        EntityName
          -> int * int
          -> Sum<
            {| Values: List<Guid * Value<TypeValue<'valueExtension>, 'valueExtension>>
               HasMore: bool |},
            Errors<unit>
           >
      Create: EntityName -> Value<TypeValue<'valueExtension>, 'valueExtension> -> Sum<Guid, Errors<unit>>
      Update: EntityName -> Guid * Delta<'valueExtension, 'deltaExtension> -> Sum<Unit, Errors<unit>>
      Delete: EntityName -> Guid -> Sum<Unit, Errors<unit>> }

  type LookupsApi<'valueExtension> =
    { GetMany:
        LookupName
          -> Guid * (int * int)
          -> Sum<
            {| Values: List<Value<TypeValue<'valueExtension>, 'valueExtension>>
               HasMore: bool |},
            Errors<unit>
           >
      Create: LookupName -> Guid * Value<TypeValue<'valueExtension>, 'valueExtension> -> Sum<Guid, Errors<unit>>
      Delete: LookupName -> Guid * Guid -> Sum<Unit, Errors<unit>>
      Link: LookupName -> Guid * Guid -> Sum<Unit, Errors<unit>>
      Unlink: LookupName -> Guid * Guid -> Sum<Unit, Errors<unit>>
      UnlinkFrom: LookupName -> Guid -> Sum<Unit, Errors<unit>> }

  type SpecDataApi<'valueExtension, 'deltaExtension> =
    { Entities: EntitiesApi<'valueExtension, 'deltaExtension>
      Lookups: LookupsApi<'valueExtension> }

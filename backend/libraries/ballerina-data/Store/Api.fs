namespace Ballerina.Data.Store.Api

open Ballerina.Data.Schema.Model

module Model =

  open System
  open Ballerina.Collections.Sum
  open Ballerina.LocalizedErrors
  open Ballerina.Data.Delta.Model
  open Ballerina.Data.Delta.ToUpdater
  open Ballerina.Data.Spec.Model
  open Ballerina.Data.VirtualFolders

  //FIXME: folders stores v2 types that are used for seeds
  //ensure the deserialized data (seeds, schema) are always in sync (taken from) serialized data (folders)
  type Spec<'T, 'valueExtension> =
    { Seeds: SpecData<'T, 'valueExtension>
      Folders: FolderNode }

  type SpecApi<'T, 'valueExtension> =
    { Get: SpecName -> Sum<Spec<'T, 'valueExtension>, Errors>
      Create: SpecName -> Spec<'T, 'valueExtension> -> Sum<unit, Errors>
      Delete: SpecName -> Sum<Unit, Errors>
      Update: SpecName -> Spec<'T, 'valueExtension> -> Sum<Unit, Errors>
      List: unit -> Sum<SpecName list, Errors> }

  type EntitiesApi<'valueExtension> =
    { Get: EntityName -> Guid -> Sum<Value<'valueExtension>, Errors>
      GetMany:
        EntityName
          -> int * int
          -> Sum<
            {| Values: List<Guid * Value<'valueExtension>>
               HasMore: bool |},
            Errors
           >
      Create: EntityName -> Value<'valueExtension> -> Sum<Guid, Errors>
      Update: EntityName -> Guid * Delta<'valueExtension> -> Sum<Unit, Errors>
      Delete: EntityName -> Guid -> Sum<Unit, Errors> }

  type LookupsApi<'valueExtension> =
    { GetMany:
        LookupName
          -> Guid * (int * int)
          -> Sum<
            {| Values: List<Value<'valueExtension>>
               HasMore: bool |},
            Errors
           >
      Create: LookupName -> Guid * Value<'valueExtension> -> Sum<Guid, Errors>
      Delete: LookupName -> Guid * Guid -> Sum<Unit, Errors>
      Link: LookupName -> Guid * Guid -> Sum<Unit, Errors>
      Unlink: LookupName -> Guid * Guid -> Sum<Unit, Errors> }

  type SpecDataApi<'valueExtension> =
    { Entities: EntitiesApi<'valueExtension>
      Lookups: LookupsApi<'valueExtension> }

namespace Ballerina.Data.Spec

open System
open Ballerina.DSL.Next.Types
open Ballerina.Data.Schema.Model
open Ballerina.VirtualFolders.Interactions
open Ballerina.VirtualFolders.Model

module Model =
  type SpecName = { SpecName: string }

  type SpecData<'T, 'valueExtension> =
    { Entities: Map<EntityName, Map<Guid, Value<'T, 'valueExtension>>>
      Lookups: Map<LookupName, Map<Guid, Set<Guid>>> }

  type Spec<'T, 'valueExtension> =
    { Seeds: SpecData<'T, 'valueExtension>
      WorkspaceVariant: WorkspaceVariant
      Folders: FolderNode }

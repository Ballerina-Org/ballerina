namespace Ballerina.API.MemoryDB

module Model =
  open System
  open Ballerina.DSL.Next.StdLib.Extensions
  open Ballerina.DSL.Next.StdLib.FileDB
  open Ballerina.DSL.Next.StdLib.MutableMemoryDB

  type SchemaFileDefinition = { Path: string; Content: string }

  type SchemaVersion =
    { Id: Guid
      Definition: List<SchemaFileDefinition>
      Version: int64
      PublishedAt: DateTime }

    static member Empty =
      { Id = Guid.Empty
        Definition = []
        Version = Int64.MinValue
        PublishedAt = DateTime.MinValue }

  type Schema =
    { Id: Guid
      Name: string
      Tenant: Guid
      Draft: Option<SchemaVersion>
      Publications: List<SchemaVersion> }

  type SchemaDTO =
    { Id: Guid
      Name: string
      Tenant: Guid
      Draft: SchemaVersion | null
      Publications: SchemaVersion[] }

    static member FromSchema(schema: Schema) : SchemaDTO =
      { Id = schema.Id
        Name = schema.Name
        Tenant = schema.Tenant
        Draft =
          match schema.Draft with
          | None -> null
          | Some draft -> draft
        Publications = schema.Publications |> List.toArray }

  type FileDbValueExtension =
    ValueExt<
      FileDBRuntimeContext,
      MutableMemoryDB<FileDBRuntimeContext, unit>,
      unit
     >

  type FileDbDeltaExtension =
    DeltaExt<
      FileDBRuntimeContext,
      MutableMemoryDB<FileDBRuntimeContext, unit>,
      unit
     >

  type SchemaFileConfig =
    { SchemaDirectory: string
      SchemaExtension: string }

  type SchemaId =
    { TenantId: Guid
      SchemaName: string
      IsDraft: bool }

namespace Ballerina.DSL.Next.StdLib

module FileDbManager =
  open System
  open Ballerina.Collections.Sum
  open System.IO
  open Ballerina.Serialization.MessagePack
  open Ballerina.Errors

  type FileContentManager<'content> =
    { Directory: string
      Extension: string
      TenantId: Guid
      SchemaName: string }

    static member Create(directory, extension, tenantId, schemaName) =
      { Directory = directory
        Extension = extension
        TenantId = tenantId
        SchemaName = schemaName }

    member this.GetFileName =
      $"{this.Directory}/{this.TenantId}-{this.SchemaName}.{this.Extension}"

    member private this.FileExists(fileName: string) =
      Directory.Exists this.Directory && File.Exists fileName

    member this.TryReadContent() =
      sum {
        let fileName = this.GetFileName

        if this.FileExists fileName then
          let bytes = File.ReadAllBytes fileName

          let serializer: MessagePackSerializerAdapter =
            new MessagePackSerializerAdapter()

          let! content = serializer.Deserialize<'content> bytes
          return Some content
        else
          return None
      }

    member this.GetContent() =
      sum {
        match! this.TryReadContent() with
        | None ->
          return!
            sum.Throw(
              Errors.Singleton () (fun _ ->
                $"File not found for {this.SchemaName} in tenant {this.TenantId}")
            )
        | Some content -> return content
      }

    member this.WriteContent(content: 'content) =
      sum {
        let fileName = this.GetFileName
        let serializer = new MessagePackSerializerAdapter()
        let! serializedSchema = serializer.Serialize content

        if Directory.Exists this.Directory |> not then
          Directory.CreateDirectory this.Directory |> ignore

        File.WriteAllBytes(fileName, serializedSchema)
      }

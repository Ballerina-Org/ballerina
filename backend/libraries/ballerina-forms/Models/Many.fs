namespace Ballerina.DSL.FormEngine.Models

module Many =
  type CodegenConfigManyDef =
    { GeneratedTypeName: string
      ChunkTypeName: string
      ItemTypeName: string
      RequiredImport: Option<string>
      DeltaTypeName: string
      SupportedRenderers: ManySupportedRenderers
      DefaultConstructor: string
      MappingFunction: string }

  and ManySupportedRenderers =
    { LinkedUnlinkedRenderers: Set<string>
      AllRenderers: Set<string> }

namespace Ballerina.DSL.Codegen.Python.LanguageConstructs

module Model =


  type PythonCodeGenState =
    { UsedImports: Set<Import> }

    static member Updaters =
      {| UsedImports = fun u -> fun s -> { s with UsedImports = u s.UsedImports } |}

  and Import = { Source: string; Target: string }

  type PythonCodeGenConfig =
    { Int: CodegenConfigTypeDef
      Float: CodegenConfigTypeDef
      Bool: CodegenConfigTypeDef
      String: CodegenConfigTypeDef
      Date: CodegenConfigTypeDef
      DateTime: CodegenConfigTypeDef
      Guid: CodegenConfigTypeDef
      Unit: CodegenConfigUnitDef
      Option: CodegenConfigOptionDef
      Set: CodegenConfigSetDef
      List: CodegenConfigListDef
      Map: CodegenConfigMapDef
      Sum: CodegenConfigSumDef
      Tuple: CodegenConfigTupleDef }

  and CodegenConfigUnitDef =
    { GeneratedTypeName: string
      RequiredImport: Option<Import> }

  and CodegenConfigListDef =
    { GeneratedTypeName: string
      RequiredImport: Option<Import> }

  and CodegenConfigOneDef =
    { GeneratedTypeName: string
      RequiredImport: Option<Import> }

  and CodegenConfigMapDef =
    { GeneratedTypeName: string
      RequiredImport: Option<Import> }

  and CodegenConfigSumDef =
    { GeneratedTypeName: string
      RequiredImport: Option<Import> }

  and CodegenConfigTypeDef =
    { GeneratedTypeName: string
      RequiredImport: Option<Import> }

  and CodegenConfigOptionDef =
    { GeneratedTypeName: string
      RequiredImport: Option<Import> }

  and CodegenConfigSetDef =
    { GeneratedTypeName: string
      RequiredImport: Option<Import> }

  and CodegenConfigTupleDef =
    { GeneratedTypeName: string
      RequiredImport: Option<Import> }

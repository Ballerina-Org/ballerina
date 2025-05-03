namespace Ballerina.DSL.Codegen.Python.LanguageConstructs

module Model =


  type PythonCodeGenState =
    { UsedImports: Set<Import> }

    static member Updaters =
      {| UsedImports = fun u -> fun s -> { s with UsedImports = u s.UsedImports } |}

  and Import = Import of string

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
      RequiredImport: Option<string> }

  and CodegenConfigOneDef =
    { GeneratedTypeName: string
      RequiredImport: Option<string> }

  and CodegenConfigMapDef =
    { GeneratedTypeName: string
      RequiredImport: Option<string> }

  and CodegenConfigSumDef =
    { GeneratedTypeName: string
      RequiredImport: Option<string> }

  and CodegenConfigTypeDef =
    { GeneratedTypeName: string
      RequiredImport: Option<string> }

  and CodegenConfigOptionDef =
    { GeneratedTypeName: string
      RequiredImport: Option<string> }

  and CodegenConfigSetDef =
    { GeneratedTypeName: string
      RequiredImport: Option<string> }

  and CodegenConfigTupleDef =
    { GeneratedTypeName: string
      RequiredImport: Option<string> }

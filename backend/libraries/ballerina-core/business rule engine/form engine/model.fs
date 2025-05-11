namespace Ballerina.DSL.FormEngine

module Model =
  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Types.Model
  open System
  open FSharp.Data

  type CodeGenConfig =
    { Int: CodegenConfigTypeDef
      Bool: CodegenConfigTypeDef
      String: CodegenConfigTypeDef
      Date: CodegenConfigTypeDef
      Guid: CodegenConfigTypeDef
      Unit: CodegenConfigUnitDef
      Option: CodegenConfigOptionDef
      Set: CodegenConfigSetDef
      List: CodegenConfigListDef
      Table: CodegenConfigTableDef
      One: CodegenConfigOneDef
      Many: CodegenConfigManyDef
      Map: CodegenConfigMapDef
      Sum: CodegenConfigSumDef
      Tuple: List<TupleCodegenConfigTypeDef>
      Union: CodegenConfigUnionDef
      Record: CodegenConfigRecordDef
      Custom: Map<string, CodegenConfigCustomDef>
      Generic: List<GenericTypeDef>
      IdentifierAllowedRegex: string
      DeltaBase: CodegenConfigInterfaceDef
      EntityNotFoundError: CodegenConfigErrorDef
      OneNotFoundError: CodegenConfigErrorDef
      LookupStreamNotFoundError: CodegenConfigErrorDef
      ManyNotFoundError: CodegenConfigErrorDef
      TableNotFoundError: CodegenConfigErrorDef
      EntityNameAndDeltaTypeMismatchError: CodegenConfigErrorDef
      EnumNotFoundError: CodegenConfigErrorDef
      InvalidEnumValueCombinationError: CodegenConfigErrorDef
      StreamNotFoundError: CodegenConfigErrorDef
      ContainerRenderers: Set<string> }

  and GenericType =
    | Option
    | List
    | Set
    | Map

  and GenericTypeDef =
    {| Type: string
       SupportedRenderers: Set<string> |}

  and CodegenConfigInterfaceDef =
    { GeneratedTypeName: string
      RequiredImport: Option<string> }

  and CodegenConfigErrorDef =
    { GeneratedTypeName: string
      Constructor: string
      RequiredImport: Option<string> }

  and TupleCodegenConfigTypeDef =
    { Ariety: int
      GeneratedTypeName: string
      DeltaTypeName: string
      SupportedRenderers: Set<string>
      Constructor: string
      RequiredImport: Option<string> }

  and CodegenConfigUnionDef = { SupportedRenderers: Set<string> }

  and CodegenConfigUnitDef =
    { GeneratedTypeName: string
      DeltaTypeName: string
      RequiredImport: Option<string>
      DefaultConstructor: string
      SupportedRenderers: Set<string> }

  and CodegenConfigListDef =
    { GeneratedTypeName: string
      RequiredImport: Option<string>
      DeltaTypeName: string
      SupportedRenderers: Set<string>
      DefaultConstructor: string
      MappingFunction: string }

  and CodegenConfigOneDef =
    { GeneratedTypeName: string
      RequiredImport: Option<string>
      DeltaTypeName: string
      SupportedRenderers: Set<string>
      DefaultConstructor: string
      MappingFunction: string }

  and CodegenConfigManyDef =
    { GeneratedTypeName: string
      RequiredImport: Option<string>
      DeltaTypeName: string
      SupportedRenderers: Set<string>
      DefaultConstructor: string
      MappingFunction: string }

  and CodegenConfigTableDef =
    { GeneratedTypeName: string
      RequiredImport: Option<string>
      DeltaTypeName: string
      SupportedRenderers: Set<string>
      DefaultConstructor: string
      MappingFunction: string }

  and CodegenConfigMapDef =
    { GeneratedTypeName: string
      RequiredImport: Option<string>
      DeltaTypeName: string
      DefaultConstructor: string
      SupportedRenderers: Set<string> }

  and CodegenConfigSumDef =
    { GeneratedTypeName: string
      RequiredImport: Option<string>
      DeltaTypeName: string
      LeftConstructor: string
      RightConstructor: string
      SupportedRenderers: Set<string> }

  and CodegenConfigTypeDef =
    { GeneratedTypeName: string
      DeltaTypeName: string
      DefaultValue: string
      RequiredImport: Option<string>
      SupportedRenderers: Set<string> }

  and CodegenConfigCustomDef =
    { GeneratedTypeName: string
      DefaultConstructor: string
      Const: bool
      DeltaTypeName: string
      RequiredImport: Option<string>
      SupportedRenderers: Set<string> }

  and CodegenConfigOptionDef =
    { GeneratedTypeName: string
      RequiredImport: Option<string>
      DefaultConstructor: string
      DeltaTypeName: string
      SupportedRenderers:
        {| Enum: Set<string>
           Stream: Set<string>
           Plain: Set<string> |} }

  and CodegenConfigSetDef =
    { GeneratedTypeName: string
      RequiredImport: Option<string>
      DefaultConstructor: string
      DeltaTypeName: string
      SupportedRenderers:
        {| Enum: Set<string>
           Stream: Set<string> |} }

  and CodegenConfigRecordDef =
    { SupportedRenderers: Map<string, Set<string>> }

  type CrudMethod =
    | Create
    | Delete
    | Get
    | GetMany
    | GetManyUnlinked
    | Update
    | Default

  type FormLauncherId =
    { LauncherName: string
      LauncherId: Guid }

  and FormLauncher =
    { LauncherName: string
      LauncherId: Guid
      Form: FormConfigId
      Mode: FormLauncherMode }

    static member Name(l: FormLauncher) : string = l.LauncherName

    static member Id(l: FormLauncher) : FormLauncherId =
      { LauncherName = l.LauncherName
        LauncherId = l.LauncherId }

  and FormLauncherApis =
    { EntityApi: EntityApiId
      ConfigEntityApi: EntityApiId }

  and FormLauncherMode =
    | Create of FormLauncherApis
    | Edit of FormLauncherApis
    | Passthrough of {| ConfigType: TypeId |}
    | PassthroughTable of
      {| ConfigType: TypeId
         TableApi: TableApiId |}


  and EnumApiId = { EnumName: string }

  and EnumApi =
    { EnumName: string
      TypeId: TypeId
      UnderlyingEnum: TypeId }

    static member Id(e: EnumApi) = { EnumName = e.EnumName }

    static member Create(n, t, c) : EnumApi =
      { EnumName = n
        TypeId = t
        UnderlyingEnum = c }

    static member Type(a: EnumApi) : TypeId = a.TypeId

  and StreamApiId = { StreamName: string }

  and StreamApi =
    { StreamName: string
      TypeId: TypeId }

    static member Id(e: StreamApi) = { StreamName = e.StreamName }

    static member Create(n, t) : StreamApi = { StreamName = n; TypeId = t }

    static member Type(a: StreamApi) : TypeId = a.TypeId

  and EntityApiId = { EntityName: string }

  and EntityApi =
    { EntityName: string
      TypeId: TypeId }

    static member Id(e: EntityApi) = { EntityName = e.EntityName }

    static member Create(n, t) : EntityApi = { EntityName = n; TypeId = t }

    static member Type(a: EntityApi) : TypeId = a.TypeId

  and TableApiId = { TableName: string }

  and TableApi =
    { TableName: string
      TypeId: TypeId }

    static member Id(e: TableApi) = { TableName = e.TableName }

    static member Create(n, t) : TableApi = { TableName = n; TypeId = t }

  and LookupApi =
    { EntityName: string
      Enums: Map<string, EnumApi>
      Streams: Map<string, StreamApi>
      Ones: Map<string, EntityApi * Set<CrudMethod>>
      Manys: Map<string, TableApi * Set<CrudMethod>> }

  and FormApis =
    { Enums: Map<string, EnumApi>
      Streams: Map<string, StreamApi>
      Entities: Map<string, EntityApi * Set<CrudMethod>>
      Tables: Map<string, TableApi>
      Lookups: Map<string, LookupApi> }

    static member Empty =
      { Enums = Map.empty
        Streams = Map.empty
        Entities = Map.empty
        Tables = Map.empty
        Lookups = Map.empty }

    static member Updaters =
      {| Enums = fun u s -> { s with FormApis.Enums = u (s.Enums) }
         Streams =
          fun u s ->
            { s with
                FormApis.Streams = u (s.Streams) }
         Entities =
          fun u s ->
            { s with
                FormApis.Entities = u (s.Entities) }
         Tables =
          fun u s ->
            { s with
                FormApis.Tables = u (s.Tables) }
         Lookups =
          fun u s ->
            { s with
                FormApis.Lookups = u (s.Lookups) } |}

  and FormConfigId = { FormName: string; FormId: Guid }

  and FormConfig =
    { FormName: string
      FormId: Guid
      Body: FormBody
      ContainerRenderer: Option<string> }

    static member Name f = f.FormName

    static member Id f =
      { FormName = f.FormName
        FormId = f.FormId }

  and FormBody =
    | Record of
      {| Renderer: Option<string>
         Fields: FormFields
         RecordType: ExprType |}
    | Union of
      {| Renderer: Renderer
         Cases: Map<string, Renderer>
         UnionType: ExprType |}
    | Table of
      {| Renderer: string
         Details: Option<NestedRenderer>
         //  Preview: Option<FormBody>
         Columns: Map<string, Column>
         VisibleColumns: FormGroup
         RowType: ExprType |}

    static member FormDeclarationType(self: FormBody) =
      match self with
      | Record f -> f.RecordType
      | Union c -> c.UnionType
      | Table t -> t.RowType

    static member ProcessedType(self: FormBody) =
      match self with
      | Record f -> f.RecordType
      | Union c -> c.UnionType
      | Table t -> t.RowType |> ExprType.TableType

  and Column =
    { FieldConfig: FieldConfig
      IsFilterable: bool
      IsSortable: bool }

  and FormFields =
    { Fields: Map<string, FieldConfig>
      Tabs: FormTabs }

  and FormTabs = { FormTabs: Map<string, FormColumns> }

  and FormColumns =
    { FormColumns: Map<string, FormGroups> }

  and FormGroups = { FormGroups: Map<string, FormGroup> }

  and FormGroup =
    | Computed of Expr
    | Inlined of List<FieldConfigId>

  and FieldConfigId = { FieldName: string; FieldId: Guid }

  and FieldConfig =
    { FieldName: string
      FieldId: Guid
      Label: Option<string>
      Tooltip: Option<string>
      Details: Option<string>
      Renderer: Renderer
      Visible: Expr
      Disabled: Option<Expr> }

    static member Id(f: FieldConfig) : FieldConfigId =
      { FieldName = f.FieldName
        FieldId = f.FieldId }

    static member Name(f: FieldConfig) = f.FieldName

  and Renderer =
    | Multiple of
      {| First:
           {| Name: string
              NestedRenderer: NestedRenderer |}
         Rest: Map<string, NestedRenderer> |}
    | PrimitiveRenderer of PrimitiveRenderer
    | MapRenderer of
      {| Map: Renderer
         Key: NestedRenderer
         Value: NestedRenderer |}
    | TupleRenderer of
      {| Tuple: Renderer
         Elements: List<NestedRenderer> |}
    | OptionRenderer of
      {| Option: Renderer
         Some: NestedRenderer
         None: NestedRenderer |}
    | ListRenderer of
      {| List: Renderer
         Element: NestedRenderer |}
    | OneRenderer of
      {| One: Renderer
         Details: NestedRenderer
         Preview: Option<NestedRenderer>
         OneApiId: Choice<TableApiId, TypeId * string> |}
    | ManyRenderer of
      {| Many: Renderer
         Details: NestedRenderer
         Preview: Option<NestedRenderer>
         ManyApiId: TypeId * string |}
    // | TableRenderer of
    //   {| Table: Renderer
    //      Row: NestedRenderer
    //      Children: RendererChildren |}
    | SumRenderer of
      {| Sum: Renderer
         Left: NestedRenderer
         Right: NestedRenderer |}
    | EnumRenderer of EnumApiId * Renderer
    | StreamRenderer of StreamRendererApi * Renderer
    | FormRenderer of FormConfigId * ExprType //* RendererChildren
    | TableFormRenderer of FormConfigId * ExprType * TableApiId //* RendererChildren
    // | ManyFormRenderer of FormConfigId * ExprType * TypeId * string //* RendererChildren
    | InlineFormRenderer of
      {| Body: FormBody
         ContainerRenderer: Option<string> |}
  // | UnionRenderer of
  //   {| Union: Renderer
  //      Cases: Map<CaseName, Renderer>
  //   //Children: RendererChildren
  //   |}

  and StreamRendererApi =
    | Stream of StreamApiId
    | LookupStream of {| Type: TypeId; Stream: StreamApiId |}

  and NestedRenderer =
    { Label: Option<string>
      Tooltip: Option<string>
      Details: Option<string>
      Renderer: Renderer }

  and PrimitiveRendererId =
    { PrimitiveRendererName: string
      PrimitiveRendererId: Guid }

  and PrimitiveRenderer =
    { PrimitiveRendererName: string
      PrimitiveRendererId: Guid
      Type: ExprType
    // Children: RendererChildren
    }

    static member ToPrimitiveRendererId(r: PrimitiveRenderer) =
      { PrimitiveRendererName = r.PrimitiveRendererName
        PrimitiveRendererId = r.PrimitiveRendererId }

  // and RendererChildren = { Fields: Map<string, FieldConfig> }

  type FormPredicateValidationHistoryItem =
    { Form: FormConfigId
      GlobalType: ExprType
      RootType: ExprType }

  type ValidationState =
    { PredicateValidationHistory: Set<FormPredicateValidationHistoryItem> }

    static member Updaters =
      {| PredicateValidationHistory =
          fun u s ->
            { s with
                PredicateValidationHistory = u (s.PredicateValidationHistory) } |}

  type GeneratedLanguageSpecificConfig =
    { EnumValueFieldName: string
      StreamIdFieldName: string
      StreamDisplayValueFieldName: string }

  type ParsedFormsContext =
    { Types: Map<string, TypeBinding>
      Apis: FormApis
      Forms: Map<string, FormConfig>
      GenericRenderers:
        List<
          {| Type: ExprType
             SupportedRenderers: Set<string> |}
         >
      Launchers: Map<string, FormLauncher> }

    static member Empty: ParsedFormsContext =
      { Types = Map.empty
        Apis = FormApis.Empty
        Forms = Map.empty
        GenericRenderers = []
        Launchers = Map.empty }

    static member Updaters =
      {| Types =
          fun u ->
            fun s ->
              { s with
                  ParsedFormsContext.Types = u (s.Types) }
         Apis =
          fun u ->
            fun s ->
              { s with
                  ParsedFormsContext.Apis = u (s.Apis) }
         Forms =
          fun u ->
            fun s ->
              { s with
                  ParsedFormsContext.Forms = u (s.Forms) }
         GenericRenderers =
          fun u ->
            fun s ->
              { s with
                  ParsedFormsContext.GenericRenderers = u (s.GenericRenderers) }
         Launchers =
          fun u ->
            fun s ->
              { s with
                  ParsedFormsContext.Launchers = u (s.Launchers) } |}

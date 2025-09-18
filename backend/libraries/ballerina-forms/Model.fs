namespace Ballerina.DSL.FormEngine

module Model =
  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Types.Model
  open System
  open Ballerina.State.WithError

  open Models.Many

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
      ReadOnly: CodegenConfigReadOnlyDef
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
      ContainerRenderers: Set<string>
      GenerateReplace: Set<string> }

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


  and CodegenConfigReadOnlyDef =
    { GeneratedTypeName: string
      RequiredImport: Option<string>
      DeltaTypeName: string
      SupportedRenderers: Set<string>
      DefaultConstructor: string }

  and CodegenConfigTableDef =
    { GeneratedTypeName: string
      RequiredImport: Option<string>
      DeltaTypeName: string
      SupportedRenderers: Set<string>
      DefaultConstructor: string
      MappingFunction: string
      FilteringConfig: CodegenConfigTableFilteringTypesDef }

  and CodegenConfigTableFilteringTypesDef =
    { SortingTypeName: string
      FilteringOperators: CodegenConfigTableFilteringOperatorsDef }

  and CodegenConfigTableFilteringOperatorsDef =
    { EqualsTo: string
      NotEqualsTo: string
      GreaterThan: string
      SmallerThan: string
      GreaterThanOrEqualsTo: string
      SmallerThanOrEqualsTo: string
      StartsWith: string
      Contains: string
      IsNull: string
      IsNotNull: string }

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

  type TableMethod =
    | Add
    | Remove
    | Duplicate
    | Move


  type CrudMethod =
    | Create
    | Delete
    | Get
    | GetAll
    | GetManyLinked
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
    | Passthrough of {| ConfigType: ExprTypeId |}
    | PassthroughTable of
      {| ConfigType: ExprTypeId
         TableApi: TableApiId |}


  and EnumApiId = { EnumName: string }

  and EnumApi =
    { EnumName: string
      TypeId: ExprTypeId
      UnderlyingEnum: ExprTypeId }

    static member Id(e: EnumApi) = { EnumName = e.EnumName }

    static member Create(n, t, c) : EnumApi =
      { EnumName = n
        TypeId = t
        UnderlyingEnum = c }

    static member Type(a: EnumApi) : ExprTypeId = a.TypeId

  and StreamApiId = { StreamName: string }

  and StreamApi =
    { StreamName: string
      TypeId: ExprTypeId }

    static member Id(e: StreamApi) = { StreamName = e.StreamName }

    static member Create(n, t) : StreamApi = { StreamName = n; TypeId = t }

    static member Type(a: StreamApi) : ExprTypeId = a.TypeId

  and EntityApiId = { EntityName: string }

  and EntityApi =
    { EntityName: string
      TypeId: ExprTypeId }

    static member Id(e: EntityApi) = { EntityName = e.EntityName }

    static member Create(n, t) : EntityApi = { EntityName = n; TypeId = t }

    static member Type(a: EntityApi) : ExprTypeId = a.TypeId

  and TableApiId = { TableName: string }

  and FieldName = string

  and TableFilteringOperator =
    | EqualsTo
    | NotEqualsTo
    | GreaterThan
    | SmallerThan
    | GreaterThanOrEqualsTo
    | SmallerThanOrEqualsTo
    | StartsWith
    | Contains
    | IsNull
    | IsNotNull

  and TableFilter<'ExprExtension, 'ValueExtension> =
    { Operators: List<TableFilteringOperator>
      Type: ExprType
      Display: NestedRenderer<'ExprExtension, 'ValueExtension> }

  and TableFilters<'ExprExtension, 'ValueExtension> = Map<FieldName, TableFilter<'ExprExtension, 'ValueExtension>>

  and TableApi<'ExprExtension, 'ValueExtension> =
    { TableName: string
      TypeId: ExprTypeId
      Filters: TableFilters<'ExprExtension, 'ValueExtension>
      Sorting: Set<FieldName> }

    static member Id(e: TableApi<'ExprExtension, 'ValueExtension>) = { TableName = e.TableName }

    static member Create(n, t) : TableApi<'ExprExtension, 'ValueExtension> =
      { TableName = n
        TypeId = t
        Filters = Map.empty
        Sorting = Set.empty }

  and LookupApi<'ExprExtension, 'ValueExtension> =
    { EntityName: string
      Enums: Map<string, EnumApi>
      Streams: Map<string, StreamApi>
      Ones: Map<string, EntityApi * Set<CrudMethod>>
      Manys: Map<string, TableApi<'ExprExtension, 'ValueExtension> * Set<CrudMethod>> }

  and FormApis<'ExprExtension, 'ValueExtension> =
    { Enums: Map<string, EnumApi>
      Streams: Map<string, StreamApi>
      Entities: Map<string, EntityApi * Set<CrudMethod>>
      Tables: Map<string, TableApi<'ExprExtension, 'ValueExtension> * Set<TableMethod>>
      Lookups: Map<string, LookupApi<'ExprExtension, 'ValueExtension>> }

    static member Empty: FormApis<'ExprExtension, 'ValueExtension> =
      { Enums = Map.empty
        Streams = Map.empty
        Entities = Map.empty
        Tables = Map.empty
        Lookups = Map.empty }

    static member Updaters =
      {| Enums = fun u (s: FormApis<'ExprExtension, 'ValueExtension>) -> { s with FormApis.Enums = u (s.Enums) }
         Streams =
          fun u (s: FormApis<'ExprExtension, 'ValueExtension>) ->
            { s with
                FormApis.Streams = u (s.Streams) }
         Entities =
          fun u (s: FormApis<'ExprExtension, 'ValueExtension>) ->
            { s with
                FormApis.Entities = u (s.Entities) }
         Tables =
          fun u (s: FormApis<'ExprExtension, 'ValueExtension>) ->
            { s with
                FormApis.Tables = u (s.Tables) }
         Lookups =
          fun u (s: FormApis<'ExprExtension, 'ValueExtension>) ->
            { s with
                FormApis.Lookups = u (s.Lookups) } |}

  and FormConfigId = { FormName: string; FormId: Guid }

  and FormConfig<'ExprExtension, 'ValueExtension> =
    { FormName: string
      FormId: Guid
      Body: FormBody<'ExprExtension, 'ValueExtension>
      ContainerRenderer: Option<string> }

    static member Name(f: FormConfig<'ExprExtension, 'ValueExtension>) = f.FormName

    static member Id(f: FormConfig<'ExprExtension, 'ValueExtension>) =
      { FormName = f.FormName
        FormId = f.FormId }

  and FormBody<'ExprExtension, 'ValueExtension> =
    | Record of
      {| Renderer: Option<string>
         Fields: FormFields<'ExprExtension, 'ValueExtension>
         RecordType: ExprType |}
    | Union of
      {| Renderer: Renderer<'ExprExtension, 'ValueExtension>
         Cases: Map<string, NestedRenderer<'ExprExtension, 'ValueExtension>>
         UnionType: ExprType |}
    | Table of
      {| Renderer: string
         Details: Option<NestedRenderer<'ExprExtension, 'ValueExtension>>
         HighlightedFilters: List<FieldName>
         //  Preview: Option<FormBody>
         Columns: Map<string, Column<'ExprExtension, 'ValueExtension>>
         VisibleColumns: FormGroup<'ExprExtension, 'ValueExtension>
         MethodLabels: Map<TableMethod, string>
         RowType: ExprType |}
    | Annotated of
      {| Type: ExprType
         Renderer: NestedRenderer<'ExprExtension, 'ValueExtension> |}

    static member FormDeclarationType(self: FormBody<'ExprExtension, 'ValueExtension>) =
      match self with
      | Record f -> f.RecordType
      | Union c -> c.UnionType
      | Table t -> t.RowType
      | Annotated t -> t.Type

    static member ProcessedType(self: FormBody<'ExprExtension, 'ValueExtension>) =
      match self with
      | Record f -> f.RecordType
      | Union c -> c.UnionType
      | Table t -> t.RowType |> ExprType.TableType
      | Annotated t -> t.Type

  and Column<'ExprExtension, 'ValueExtension> =
    { FieldConfig: FieldConfig<'ExprExtension, 'ValueExtension> }

  and FormFields<'ExprExtension, 'ValueExtension> =
    { Fields: Map<string, FieldConfig<'ExprExtension, 'ValueExtension>>
      Disabled: FormGroup<'ExprExtension, 'ValueExtension>
      Tabs: FormTabs<'ExprExtension, 'ValueExtension> }

  and FormTabs<'ExprExtension, 'ValueExtension> =
    { FormTabs: Map<string, FormColumns<'ExprExtension, 'ValueExtension>> }

  and FormColumns<'ExprExtension, 'ValueExtension> =
    { FormColumns: Map<string, FormGroups<'ExprExtension, 'ValueExtension>> }

  and FormGroups<'ExprExtension, 'ValueExtension> =
    { FormGroups: Map<string, FormGroup<'ExprExtension, 'ValueExtension>> }

  and FormGroup<'ExprExtension, 'ValueExtension> =
    | Computed of Expr<'ExprExtension, 'ValueExtension>
    | Inlined of List<FieldConfigId>

  and FieldConfigId = { FieldName: string; FieldId: Guid }

  and FieldConfig<'ExprExtension, 'ValueExtension> =
    { FieldName: string
      FieldId: Guid
      Label: Option<string>
      Tooltip: Option<string>
      Details: Option<string>
      Renderer: Renderer<'ExprExtension, 'ValueExtension>
      Visible: Expr<'ExprExtension, 'ValueExtension>
      Disabled: Option<Expr<'ExprExtension, 'ValueExtension>> }

    static member Id(f: FieldConfig<'ExprExtension, 'ValueExtension>) : FieldConfigId =
      { FieldName = f.FieldName
        FieldId = f.FieldId }

    static member Name(f: FieldConfig<'ExprExtension, 'ValueExtension>) = f.FieldName

  and Renderer<'ExprExtension, 'ValueExtension> =
    | Multiple of
      {| First:
           {| Name: string
              NestedRenderer: NestedRenderer<'ExprExtension, 'ValueExtension> |}
         Rest: Map<string, NestedRenderer<'ExprExtension, 'ValueExtension>>
         Label: string option |}
    | PrimitiveRenderer of PrimitiveRenderer
    | MapRenderer of
      {| Label: string option
         Map: Renderer<'ExprExtension, 'ValueExtension>
         Key: NestedRenderer<'ExprExtension, 'ValueExtension>
         Value: NestedRenderer<'ExprExtension, 'ValueExtension> |}
    | TupleRenderer of
      {| Label: string option
         Tuple: Renderer<'ExprExtension, 'ValueExtension>
         Elements: List<NestedRenderer<'ExprExtension, 'ValueExtension>> |}
    | OptionRenderer of
      {| Label: string option
         Option: Renderer<'ExprExtension, 'ValueExtension>
         Some: NestedRenderer<'ExprExtension, 'ValueExtension>
         None: NestedRenderer<'ExprExtension, 'ValueExtension> |}
    | ListRenderer of
      {| Label: string option
         List: Renderer<'ExprExtension, 'ValueExtension>
         Element: NestedRenderer<'ExprExtension, 'ValueExtension>
         MethodLabels: Map<TableMethod, string> |}
    | OneRenderer of
      {| Label: string option
         One: Renderer<'ExprExtension, 'ValueExtension>
         Details: NestedRenderer<'ExprExtension, 'ValueExtension>
         Preview: Option<NestedRenderer<'ExprExtension, 'ValueExtension>>
         OneApiId: ExprTypeId * string |}

    | ManyRenderer of ManyRenderer<'ExprExtension, 'ValueExtension>

    | ReadOnlyRenderer of
      {| Label: string option
         ReadOnly: Renderer<'ExprExtension, 'ValueExtension>
         Value: NestedRenderer<'ExprExtension, 'ValueExtension> |}
    // | TableRenderer of
    //   {| Table: Renderer
    //      Row: NestedRenderer
    //      Children: RendererChildren |}
    | SumRenderer of
      {| Label: string option
         Sum: Renderer<'ExprExtension, 'ValueExtension>
         Left: NestedRenderer<'ExprExtension, 'ValueExtension>
         Right: NestedRenderer<'ExprExtension, 'ValueExtension> |}
    | EnumRenderer of EnumApiId * Renderer<'ExprExtension, 'ValueExtension>
    | StreamRenderer of StreamRendererApi * Renderer<'ExprExtension, 'ValueExtension>
    | FormRenderer of FormConfigId * ExprType //* RendererChildren
    | TableFormRenderer of FormConfigId * ExprType * TableApiId //* RendererChildren
    // | ManyFormRenderer of FormConfigId * ExprType * TypeId * string //* RendererChildren
    | InlineFormRenderer of
      {| Body: FormBody<'ExprExtension, 'ValueExtension>
         ContainerRenderer: Option<string> |}
  // | UnionRenderer of
  //   {| Union: Renderer
  //      Cases: Map<CaseName, Renderer>
  //   //Children: RendererChildren
  //   |}

  and ManyRenderer<'ExprExtension, 'ValueExtension> =
    | ManyLinkedUnlinkedRenderer of
      {| Label: string option
         Many: Renderer<'ExprExtension, 'ValueExtension>
         Linked: NestedRenderer<'ExprExtension, 'ValueExtension>
         Unlinked: Option<NestedRenderer<'ExprExtension, 'ValueExtension>>
         ManyApiId: Option<ExprTypeId * string> |}
    | ManyAllRenderer of
      {| Label: string option
         Many: Renderer<'ExprExtension, 'ValueExtension>
         Element: NestedRenderer<'ExprExtension, 'ValueExtension>
         ManyApiId: Option<ExprTypeId * string> |}


  and StreamRendererApi =
    | Stream of StreamApiId
    | LookupStream of
      {| Type: ExprTypeId
         Stream: StreamApiId |}

  and NestedRenderer<'ExprExtension, 'ValueExtension> =
    { Label: Option<string>
      Tooltip: Option<string>
      Details: Option<string>
      Renderer: Renderer<'ExprExtension, 'ValueExtension> }

  and PrimitiveRendererId =
    { PrimitiveRendererName: string
      PrimitiveRendererId: Guid }

  and PrimitiveRenderer =
    { PrimitiveRendererName: string
      PrimitiveRendererId: Guid
      Type: ExprType
      Label: string option
    // Children: RendererChildren
    }

    static member ToPrimitiveRendererId(r: PrimitiveRenderer) =
      { PrimitiveRendererName = r.PrimitiveRendererName
        PrimitiveRendererId = r.PrimitiveRendererId }

  // and RendererChildren = { Fields: Map<string, FieldConfig> }

  type FormParserPrimitivesExtension<'ExprExtension, 'ValueExtension> =
    { ConstBool: bool -> Expr<'ExprExtension, 'ValueExtension> }

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

  type ParsedFormsContext<'ExprExtension, 'ValueExtension> =
    { Types: TypeContext
      Apis: FormApis<'ExprExtension, 'ValueExtension>
      Forms: Map<string, FormConfig<'ExprExtension, 'ValueExtension>>
      GenericRenderers:
        List<
          {| Type: ExprType
             SupportedRenderers: Set<string> |}
         >
      Launchers: Map<string, FormLauncher> }

    static member Empty: ParsedFormsContext<'ExprExtension, 'ValueExtension> =
      { Types = Map.empty
        Apis = FormApis<'ExprExtension, 'ValueExtension>.Empty
        Forms = Map.empty
        GenericRenderers = []
        Launchers = Map.empty }

    static member Updaters =
      {| Types =
          fun u ->
            fun (s: ParsedFormsContext<'ExprExtension, 'ValueExtension>) ->
              { s with
                  ParsedFormsContext.Types = u (s.Types) }
         Apis =
          fun u ->
            fun (s: ParsedFormsContext<'ExprExtension, 'ValueExtension>) ->
              { s with
                  ParsedFormsContext.Apis = u (s.Apis) }
         Forms =
          fun u ->
            fun (s: ParsedFormsContext<'ExprExtension, 'ValueExtension>) ->
              { s with
                  ParsedFormsContext.Forms = u (s.Forms) }
         GenericRenderers =
          fun u ->
            fun (s: ParsedFormsContext<'ExprExtension, 'ValueExtension>) ->
              { s with
                  ParsedFormsContext.GenericRenderers = u (s.GenericRenderers) }
         Launchers =
          fun u ->
            fun (s: ParsedFormsContext<'ExprExtension, 'ValueExtension>) ->
              { s with
                  ParsedFormsContext.Launchers = u (s.Launchers) } |}

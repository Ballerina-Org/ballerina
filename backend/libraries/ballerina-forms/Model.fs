namespace Ballerina.DSL.FormEngine

module Model =
  open Ballerina.DSL.Expr.Model
  open Ballerina.DSL.Expr.Types.Model
  open System
  open Ballerina.Collections.Sum
  open Ballerina.Errors

  type RendererName = RendererName of string
  type FormName = FormName of string
  type LauncherName = LauncherName of string
  type Label = Label of string
  type LanguageStreamType = LanguageStreamType of string
  type GoImport = GoImport of string

  type Serializer = { Name: string; Import: GoImport }

  type Deserializer = { Name: string; Import: GoImport }

  type Serialization =
    { Serializer: Serializer
      Deserializer: Deserializer }

  type EnumRendererType =
    | Option
    | Set

  type StreamRendererType =
    | Option
    | Set

  type CodegenConfigManyDef =
    { GeneratedTypeName: string
      ChunkTypeName: string
      ItemTypeName: string
      RequiredImport: Option<GoImport>
      DeltaTypeName: string
      SupportedRenderers: ManySupportedRenderers
      MappingFunction: string }

  and ManySupportedRenderers =
    { LinkedUnlinkedRenderers: Set<RendererName>
      AllRenderers: Set<RendererName> }



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
      ContainerRenderers: Set<RendererName>
      GenerateReplace: Set<string>
      LanguageStreamType: LanguageStreamType }

  and GenericTypeDef =
    {| Type: string
       SupportedRenderers: Set<RendererName> |}

  and CodegenConfigInterfaceDef =
    { GeneratedTypeName: string
      RequiredImport: Option<GoImport> }

  and CodegenConfigErrorDef =
    { GeneratedTypeName: string
      Constructor: string
      RequiredImport: Option<GoImport> }

  and TupleCodegenConfigTypeDef =
    { Ariety: int
      GeneratedTypeName: string
      DeltaTypeName: string
      SupportedRenderers: Set<RendererName>
      Constructor: string
      RequiredImport: Option<GoImport>
      Serialization: Serialization }

    static member FindArity
      (config: List<TupleCodegenConfigTypeDef>)
      (arity: int)
      : Sum<TupleCodegenConfigTypeDef, Errors> =
      config
      |> List.tryFind (fun c -> c.Ariety = arity)
      |> Sum.fromOption (fun () -> Errors.Singleton $"Error: missing tuple config for arity {arity}")


  and CodegenConfigUnionDef =
    { SupportedRenderers: Set<RendererName> }

  and CodegenConfigUnitDef =
    { GeneratedTypeName: string
      DeltaTypeName: string
      RequiredImport: Option<GoImport>
      SupportedRenderers: Set<RendererName>
      Serialization: Serialization }

  and CodegenConfigListDef =
    { GeneratedTypeName: string
      RequiredImport: Option<GoImport>
      DeltaTypeName: string
      SupportedRenderers: Set<RendererName>
      MappingFunction: string
      Serialization: Serialization }

  and CodegenConfigOneDef =
    { GeneratedTypeName: string
      RequiredImport: Option<GoImport>
      DeltaTypeName: string
      SupportedRenderers: Set<RendererName>
      MappingFunction: string }


  and CodegenConfigReadOnlyDef =
    { GeneratedTypeName: string
      RequiredImport: Option<GoImport>
      DeltaTypeName: string
      SupportedRenderers: Set<RendererName> }

  and CodegenConfigTableDef =
    { GeneratedTypeName: string
      RequiredImport: Option<GoImport>
      DeltaTypeName: string
      SupportedRenderers: Set<RendererName>
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
      RequiredImport: Option<GoImport>
      DeltaTypeName: string
      SupportedRenderers: Set<RendererName> }

  and CodegenConfigSumDef =
    { GeneratedTypeName: string
      RequiredImport: Option<GoImport>
      DeltaTypeName: string
      LeftConstructor: string
      RightConstructor: string
      SupportedRenderers: Set<RendererName>
      Serialization: Serialization }

  and CodegenConfigTypeDef =
    { GeneratedTypeName: string
      DeltaTypeName: string
      RequiredImport: Option<GoImport>
      SupportedRenderers: Set<RendererName>
      Serialization: Serialization }

  and CodegenConfigCustomDef =
    { GeneratedTypeName: string
      DeltaTypeName: string
      RequiredImport: Option<GoImport>
      SupportedRenderers: Set<RendererName> }

  and CodegenConfigOptionDef =
    { GeneratedTypeName: string
      RequiredImport: Option<GoImport>
      DeltaTypeName: string
      SupportedRenderers:
        {| Enum: Set<RendererName>
           Stream: Set<RendererName>
           Plain: Set<RendererName> |}
      Serialization: Serialization }

  and CodegenConfigSetDef =
    { GeneratedTypeName: string
      RequiredImport: Option<GoImport>
      DeltaTypeName: string
      SupportedRenderers:
        {| Enum: Set<RendererName>
           Stream: Set<RendererName> |} }

  and CodegenConfigRecordDef =
    { SupportedRenderers: Map<RendererName, Set<string>> }

  type TableMethod =
    | Add
    | Remove
    | RemoveAll
    | Duplicate
    | Move


  type CrudMethod =
    | Get
    | GetManyUnlinked

  type FormLauncherId =
    { LauncherName: LauncherName
      LauncherId: Guid }

  and FormLauncher =
    { LauncherName: LauncherName
      LauncherId: Guid
      Form: FormConfigId
      Mode: FormLauncherMode }

    static member Name(l: FormLauncher) : LauncherName = l.LauncherName

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

  and FormConfigId = { FormName: FormName; FormId: Guid }

  and FormConfig<'ExprExtension, 'ValueExtension> =
    { FormName: FormName
      FormId: Guid
      Body: FormBody<'ExprExtension, 'ValueExtension> }

    static member Name(f: FormConfig<'ExprExtension, 'ValueExtension>) = f.FormName

    static member Id(f: FormConfig<'ExprExtension, 'ValueExtension>) =
      { FormName = f.FormName
        FormId = f.FormId }

  and FormBody<'ExprExtension, 'ValueExtension> =
    | Annotated of
      {| Renderer: Renderer<'ExprExtension, 'ValueExtension>
         TypeId: ExprTypeId |}
    | Table of
      {| Renderer: RendererName
         Details: Option<NestedRenderer<'ExprExtension, 'ValueExtension>>
         HighlightedFilters: List<FieldName>
         //  Preview: Option<FormBody>
         Columns: Map<string, Column<'ExprExtension, 'ValueExtension>>
         VisibleColumns: FormGroup<'ExprExtension, 'ValueExtension>
         DisabledColumns: FormGroup<'ExprExtension, 'ValueExtension>
         MethodLabels: Map<TableMethod, Label>
         RowTypeId: ExprTypeId |}

  and FormBody<'ExprExtension, 'ValueExtension> with
    static member FormDeclarationType
      (types: TypeContext)
      (self: FormBody<'ExprExtension, 'ValueExtension>)
      : Sum<ExprType, Errors> =
      let lookupType (id: ExprTypeId) =
        let name = id.VarName

        types
        |> Map.tryFindWithError<string, TypeBinding> name "type" name
        |> Sum.map (fun tb -> tb.Type)

      match self with
      | Annotated f -> lookupType f.TypeId
      | Table t -> lookupType t.RowTypeId

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
      Label: Option<Label>
      Tooltip: Option<string>
      Details: Option<string>
      Renderer: Renderer<'ExprExtension, 'ValueExtension>
      Visible: Expr<'ExprExtension, 'ValueExtension>
      Disabled: Option<Expr<'ExprExtension, 'ValueExtension>> }

    static member Id(f: FieldConfig<'ExprExtension, 'ValueExtension>) : FieldConfigId =
      { FieldName = f.FieldName
        FieldId = f.FieldId }

    static member Name(f: FieldConfig<'ExprExtension, 'ValueExtension>) = f.FieldName

  and RecordRenderer<'ExprExtension, 'ValueExtension> =
    { Renderer: Option<RendererName>
      Fields: FormFields<'ExprExtension, 'ValueExtension> }

  and MapRenderer<'ExprExtension, 'ValueExtension> =
    { Label: Label option
      Map: RendererName
      Key: NestedRenderer<'ExprExtension, 'ValueExtension>
      Value: NestedRenderer<'ExprExtension, 'ValueExtension> }

  and SumRenderer<'ExprExtension, 'ValueExtension> =
    { Label: Label option
      Sum: RendererName
      Left: NestedRenderer<'ExprExtension, 'ValueExtension>
      Right: NestedRenderer<'ExprExtension, 'ValueExtension> }

  and ListRenderer<'ExprExtension, 'ValueExtension> =
    { Label: Label option
      List: RendererName
      Element: NestedRenderer<'ExprExtension, 'ValueExtension>
      MethodLabels: Map<TableMethod, Label> }

  and OptionRenderer<'ExprExtension, 'ValueExtension> =
    { Label: Label option
      Option: RendererName
      Some: NestedRenderer<'ExprExtension, 'ValueExtension>
      None: NestedRenderer<'ExprExtension, 'ValueExtension> }

  and OneRenderer<'ExprExtension, 'ValueExtension> =
    { Label: Label option
      One: RendererName
      Details: NestedRenderer<'ExprExtension, 'ValueExtension>
      Preview: Option<NestedRenderer<'ExprExtension, 'ValueExtension>>
      OneApiId: ExprTypeId * string }

  and TupleRenderer<'ExprExtension, 'ValueExtension> =
    { Label: Label option
      Tuple: RendererName
      Elements: List<NestedRenderer<'ExprExtension, 'ValueExtension>> }

  and ReadOnlyRenderer<'ExprExtension, 'ValueExtension> =
    { Label: Label option
      ReadOnly: RendererName
      Value: NestedRenderer<'ExprExtension, 'ValueExtension> }

  and UnionRenderer<'ExprExtension, 'ValueExtension> =
    { Renderer: RendererName
      Cases: Map<string, NestedRenderer<'ExprExtension, 'ValueExtension>> }

  and Renderer<'ExprExtension, 'ValueExtension> =
    | PrimitiveRenderer of PrimitiveRenderer
    | MapRenderer of MapRenderer<'ExprExtension, 'ValueExtension>
    | TupleRenderer of TupleRenderer<'ExprExtension, 'ValueExtension>
    | OptionRenderer of OptionRenderer<'ExprExtension, 'ValueExtension>
    | ListRenderer of ListRenderer<'ExprExtension, 'ValueExtension>
    | OneRenderer of OneRenderer<'ExprExtension, 'ValueExtension>

    | ManyRenderer of ManyRenderer<'ExprExtension, 'ValueExtension>

    | ReadOnlyRenderer of ReadOnlyRenderer<'ExprExtension, 'ValueExtension>
    | SumRenderer of SumRenderer<'ExprExtension, 'ValueExtension>
    | EnumRenderer of EnumApiId * Label option * EnumRendererType * ExprTypeId * RendererName
    | StreamRenderer of StreamRendererApi * Label option * StreamRendererType * ExprTypeId * RendererName
    | FormRenderer of FormConfigId * ExprTypeId //* RendererChildren
    | TableFormRenderer of FormConfigId * ExprType * TableApiId //* RendererChildren
    | InlineFormRenderer of FormBody<'ExprExtension, 'ValueExtension>
    | RecordRenderer of RecordRenderer<'ExprExtension, 'ValueExtension>
    | UnionRenderer of UnionRenderer<'ExprExtension, 'ValueExtension>

  and ManyRenderer<'ExprExtension, 'ValueExtension> =
    | ManyLinkedUnlinkedRenderer of
      {| Label: Label option
         Many: RendererName
         Linked: NestedRenderer<'ExprExtension, 'ValueExtension>
         Unlinked: Option<NestedRenderer<'ExprExtension, 'ValueExtension>>
         ManyApiId: Option<ExprTypeId * string> |}
    | ManyAllRenderer of
      {| Label: Label option
         Many: RendererName
         Element: NestedRenderer<'ExprExtension, 'ValueExtension>
         ManyApiId: Option<ExprTypeId * string> |}


  and StreamRendererApi =
    | Stream of StreamApiId
    | LookupStream of
      {| Type: ExprTypeId
         Stream: StreamApiId |}

  and NestedRenderer<'ExprExtension, 'ValueExtension> =
    { Label: Option<Label>
      Tooltip: Option<string>
      Details: Option<string>
      Renderer: Renderer<'ExprExtension, 'ValueExtension> }

  and PrimitiveRendererId =
    { PrimitiveRendererName: RendererName
      PrimitiveRendererId: Guid }

  and PrimitiveRenderer =
    { PrimitiveRendererName: RendererName
      PrimitiveRendererId: Guid
      Type: ExprType
      Label: Label option
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
      Forms: Map<FormName, FormConfig<'ExprExtension, 'ValueExtension>>
      SupportedRecordRenderers: Map<RendererName, Set<string>>
      LanguageStreamType: LanguageStreamType
      GenericRenderers:
        List<
          {| Type: ExprType
             SupportedRenderers: Set<RendererName> |}
         >
      Launchers: Map<LauncherName, FormLauncher> }

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

import { List, Map } from "immutable";

import {
  DispatchParsedType,
  DispatchTypeName,
  FilterType,
  SumNType,
} from "./domains/specification/domains/types/state";
import { unit, Unit } from "../../../../fun/domains/unit/state";
import {
  PredicateValue,
  EnumReference,
  ValueOrErrors,
  DispatchInjectedPrimitives,
  BasicFun,
  Guid,
  ApiErrors,
  Specification,
  Synchronized,
  simpleUpdater,
  ValueInfiniteStreamState,
  MapRepo,
  DispatchInjectables,
  RecordAbstractRendererView,
  DispatchInjectablesTypes,
  TableApis,
  SpecificationApis,
  LookupTypeAbstractRendererView,
  ValueFilter,
  ValueTable,
} from "../../../../../main";

import {
  DispatchApiConverters,
  concreteRendererToKind,
  dispatchDefaultState,
  dispatchDefaultValue,
  dispatchFromAPIRawValue,
  dispatchToAPIRawValue,
  tryGetConcreteRenderer,
  ConcreteRenderers,
  getDefaultRecordRenderer,
} from "../built-ins/state";
import { SearchableInfiniteStreamAbstractRendererState } from "../runner/domains/abstract-renderers/searchable-infinite-stream/state";
import { Renderer } from "./domains/specification/domains/forms/domains/renderer/state";

export type DispatchParsedPassthroughLauncher<T> = {
  kind: "passthrough";
  formName: string;
  renderer: Renderer<T>;
  parseEntityFromApi: (_: any) => ValueOrErrors<PredicateValue, string>;
  parseGlobalConfigurationFromApi: (
    _: any,
  ) => ValueOrErrors<PredicateValue, string>;
  parseValueToApi: (
    value: PredicateValue,
    type: DispatchParsedType<T>,
    state: any,
  ) => ValueOrErrors<any, string>;
  type: DispatchParsedType<T>;
};

export type DispatchParsedEditLauncher<T> = {
  kind: "edit";
  formName: string;
  renderer: Renderer<T>;
  fromApiParser: (_: any) => ValueOrErrors<PredicateValue, string>;
  toApiParser: (
    value: PredicateValue,
    type: DispatchParsedType<T>,
    formState: any,
  ) => ValueOrErrors<any, string>;
  parseGlobalConfigurationFromApi: (
    _: any,
  ) => ValueOrErrors<PredicateValue, string>;
  api: string;
  configApi: string;
  type: DispatchParsedType<T>;
};

export type DispatchParsedCreateLauncher<T> = {
  kind: "create";
  formName: string;
  renderer: Renderer<T>;
  fromApiParser: (_: any) => ValueOrErrors<PredicateValue, string>;
  toApiParser: (
    value: PredicateValue,
    type: DispatchParsedType<T>,
    formState: any,
  ) => ValueOrErrors<any, string>;
  parseGlobalConfigurationFromApi: (
    _: any,
  ) => ValueOrErrors<PredicateValue, string>;
  api: string;
  configApi: string;
  type: DispatchParsedType<T>;
};

export type DispatchParsedLauncher<T> =
  | DispatchParsedPassthroughLauncher<T>
  | DispatchParsedEditLauncher<T>
  | DispatchParsedCreateLauncher<T>;

export type DispatchParsedLaunchers<T> = {
  passthrough: Map<string, DispatchParsedPassthroughLauncher<T>>;
  edit: Map<string, DispatchParsedEditLauncher<T>>;
  create: Map<string, DispatchParsedCreateLauncher<T>>;
};

export type IdWrapperProps = {
  domNodeId: string;
  children: React.ReactNode;
};

export type ErrorRendererProps = {
  message: string;
};

export type DispatcherContext<
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContexts,
  ExtraContext,
> = {
  injectedPrimitives: DispatchInjectedPrimitives<T> | undefined;
  apiConverters: DispatchApiConverters<T>;
  specApis: SpecificationApis<T>;
  getConcreteRendererKind: (viewName: string) => ValueOrErrors<string, string>;
  getConcreteRenderer: ReturnType<
    typeof tryGetConcreteRenderer<
      T,
      Flags,
      CustomPresentationContexts,
      ExtraContext
    >
  >;
  lookupTypeRenderer: () => LookupTypeAbstractRendererView<
    CustomPresentationContexts,
    Flags,
    ExtraContext
  >;
  getDefaultRecordRenderer: (
    isNested: boolean,
  ) => RecordAbstractRendererView<
    CustomPresentationContexts,
    Flags,
    ExtraContext
  >;
  concreteRenderers: ConcreteRenderers<
    T,
    Flags,
    CustomPresentationContexts,
    ExtraContext
  >;
  defaultValue: (
    t: DispatchParsedType<T>,
    renderer: Renderer<T>,
  ) => ValueOrErrors<PredicateValue, string>;
  defaultState: (
    infiniteStreamSources: DispatchInfiniteStreamSources,
    lookupSources: DispatchLookupSources | undefined,
    tableApiSources: DispatchTableApiSources | undefined,
  ) => (
    t: DispatchParsedType<T>,
    renderer: Renderer<T>,
  ) => ValueOrErrors<any, string>;
  forms: Map<string, Renderer<T>>;
  types: Map<DispatchTypeName, DispatchParsedType<T>>;
  IdProvider: (props: IdWrapperProps) => React.ReactNode;
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode;
  parseFromApiByType: (
    type: DispatchParsedType<T>,
  ) => (raw: any) => ValueOrErrors<PredicateValue, string>;
  parseToApiByType: (
    type: DispatchParsedType<T>,
    value: PredicateValue,
    state: any,
  ) => ValueOrErrors<any, string>;
};

export type DeserializedDispatchSpecification<
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
  CustomPresentationContexts = Unit,
  ExtraContext = Unit,
> = {
  launchers: DispatchParsedLaunchers<T>;
  dispatcherContext: DispatcherContext<
    T,
    Flags,
    CustomPresentationContexts,
    ExtraContext
  >;
  parseValueToApi: (
    value: PredicateValue,
    type: DispatchParsedType<T>,
    state: any,
  ) => ValueOrErrors<any, string>;
  parseEntityFromApiByTypeLookupName: (
    typeLookupName: string,
    _: any,
  ) => ValueOrErrors<PredicateValue, string>;
  getTypeByLookupName: (
    typeLookupName: string,
  ) => ValueOrErrors<DispatchParsedType<T>, string>;
};

export type DispatchSpecificationDeserializationResult<
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
  CustomPresentationContexts = Unit,
  ExtraContext = Unit,
> = ValueOrErrors<
  DeserializedDispatchSpecification<
    T,
    Flags,
    CustomPresentationContexts,
    ExtraContext
  >,
  string
>;

export type DispatchEnumName = string;
export type DispatchEnumOptionsSources = BasicFun<
  DispatchEnumName,
  ValueOrErrors<BasicFun<Unit, Promise<Array<EnumReference>>>, string>
>;
export type DispatchStreamName = string;
export type DispatchInfiniteStreamSources = BasicFun<
  DispatchStreamName,
  ValueOrErrors<
    SearchableInfiniteStreamAbstractRendererState["customFormState"]["getChunk"],
    string
  >
>;
export type DispatchTableFiltersAndSorting = {
  filters: Map<string, List<ValueFilter>>;
  sorting: Map<string, "Ascending" | "Descending" | undefined>;
};
export type TableGetManyParams = {
  chunkSize: number;
  from: number;
  filtersAndSorting: string;
};
export type DispatchTableApiName = string;
export type DispatchTableApiSource = {
  get: BasicFun<Guid, Promise<any>>;
  getMany: BasicFun<
    BasicFun<any, ValueOrErrors<PredicateValue, string>>,
    BasicFun<TableGetManyParams, Promise<ValueTable>>
  >;
  getDefaultFiltersAndSorting: BasicFun<
    Map<string, SumNType<any>>,
    BasicFun<
      BasicFun<
        DispatchParsedType<any>,
        BasicFun<any, ValueOrErrors<PredicateValue, string>>
      >,
      BasicFun<Unit, Promise<DispatchTableFiltersAndSorting>>
    >
  >;
};

export type DispatchTableApiSources = BasicFun<
  string,
  ValueOrErrors<DispatchTableApiSource, string>
>;

export type DispatchApiName = string;
export type DispatchOneSource = {
  get: BasicFun<Guid, Promise<any>> | undefined;
  getManyUnlinked:
    | BasicFun<
        BasicFun<any, ValueOrErrors<PredicateValue, string>>,
        BasicFun<
          Guid,
          BasicFun<Map<string, string>, ValueInfiniteStreamState["getChunk"]>
        >
      >
    | undefined;
};

export type DispatchLookupSources = (typeName: string) => ValueOrErrors<
  {
    one?: BasicFun<DispatchApiName, ValueOrErrors<DispatchOneSource, string>>;
  },
  string
>;

export type DispatchConfigName = string;
export type DispatchGlobalConfigurationSources = BasicFun<
  DispatchConfigName,
  Promise<any>
>;
export type DispatchEntityName = string;
export type DispatchEntityApis = {
  create: BasicFun<DispatchEntityName, BasicFun<any, Promise<Unit>>>;
  default: BasicFun<DispatchEntityName, BasicFun<Unit, Promise<any>>>;
  update: BasicFun<
    DispatchEntityName,
    (id: Guid, entity: any) => Promise<ApiErrors>
  >;
  get: BasicFun<DispatchEntityName, BasicFun<Guid, Promise<any>>>;
};

export const parseDispatchFormsToLaunchers =
  <
    T extends DispatchInjectablesTypes<T>,
    Flags,
    CustomPresentationContexts,
    ExtraContext,
  >(
    injectedPrimitives: DispatchInjectedPrimitives<T> | undefined,
    apiConverters: DispatchApiConverters<T>,
    lookupTypeRenderer: () => LookupTypeAbstractRendererView<
      CustomPresentationContexts,
      Flags,
      ExtraContext
    >,
    defaultRecordRenderer: () => RecordAbstractRendererView<
      CustomPresentationContexts,
      Flags,
      ExtraContext
    >,
    defaultNestedRecordRenderer: () => RecordAbstractRendererView<
      CustomPresentationContexts,
      Flags,
      ExtraContext
    >,
    concreteRenderers: ConcreteRenderers<
      T,
      Flags,
      CustomPresentationContexts,
      ExtraContext
    >,
    IdProvider: (props: IdWrapperProps) => React.ReactNode,
    ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
  ) =>
  (
    specification: Specification<T>,
  ): DispatchSpecificationDeserializationResult<
    T,
    Flags,
    CustomPresentationContexts,
    ExtraContext
  > =>
    ValueOrErrors.Operations.All(
      List<
        ValueOrErrors<[string, DispatchParsedPassthroughLauncher<T>], string>
      >(
        specification.launchers.passthrough
          .entrySeq()
          .toArray()
          .map(([launcherName, launcher]) =>
            MapRepo.Operations.tryFindWithError(
              launcher.form,
              specification.forms,
              () =>
                `cannot find form "${launcher.form}" when parsing launchers`,
            ).Then((parsedForm) =>
              MapRepo.Operations.tryFindWithError(
                launcher.configType,
                specification.types,
                () =>
                  `cannot find global config type "${launcher.configType}" when parsing launchers`,
              ).Then((globalConfigType) =>
                ValueOrErrors.Default.return([
                  launcherName,
                  {
                    kind: "passthrough",
                    renderer: parsedForm,
                    type: parsedForm.type,
                    parseEntityFromApi: (raw: any) =>
                      dispatchFromAPIRawValue(
                        parsedForm.type,
                        specification.types,
                        apiConverters,
                        injectedPrimitives,
                      )(raw),
                    parseGlobalConfigurationFromApi: (raw: any) =>
                      dispatchFromAPIRawValue(
                        globalConfigType,
                        specification.types,
                        apiConverters,
                        injectedPrimitives,
                      )(raw),
                    parseValueToApi: (
                      value: PredicateValue,
                      type: DispatchParsedType<T>,
                      state: any,
                    ) =>
                      dispatchToAPIRawValue(
                        type,
                        specification.types,
                        apiConverters,
                        injectedPrimitives,
                      )(value, state),
                    formName: launcher.form,
                  },
                ]),
              ),
            ),
          ),
      ),
    )
      .MapErrors((errors) =>
        errors.map(
          (error) => `${error}\n...When parsing passthrough launchers`,
        ),
      )
      .Then((passthroughLaunchers) =>
        ValueOrErrors.Operations.All(
          List<
            ValueOrErrors<[string, DispatchParsedCreateLauncher<T>], string>
          >(
            specification.launchers.create
              .entrySeq()
              .toArray()
              .map(([launcherName, launcher]) =>
                MapRepo.Operations.tryFindWithError(
                  launcher.form,
                  specification.forms,
                  () =>
                    `cannot find form "${launcher.form}" when parsing launchers`,
                ).Then((parsedForm) =>
                  MapRepo.Operations.tryFindWithError(
                    launcher.configApi,
                    specification.apis.entities,
                    () =>
                      `cannot find global config api "${launcher.configApi}" when parsing launchers`,
                  ).Then((globalConfigApi) =>
                    ValueOrErrors.Default.return([
                      launcherName,
                      {
                        kind: "create",
                        renderer: parsedForm,
                        type: parsedForm.type,
                        fromApiParser: (raw: any) =>
                          dispatchFromAPIRawValue(
                            parsedForm.type,
                            specification.types,
                            apiConverters,
                            injectedPrimitives,
                          )(raw),
                        parseGlobalConfigurationFromApi: (raw: any) =>
                          dispatchFromAPIRawValue(
                            // TODO: use tryFindWithError
                            specification.types.get(globalConfigApi.type)!,
                            specification.types,
                            apiConverters,
                            injectedPrimitives,
                          )(raw),
                        toApiParser: (
                          value: PredicateValue,
                          type: DispatchParsedType<T>,
                          formState: any,
                        ) =>
                          dispatchToAPIRawValue(
                            type,
                            specification.types,
                            apiConverters,
                            injectedPrimitives,
                          )(value, formState),
                        formName: launcher.form,
                        api: launcher.api,
                        configApi: launcher.configApi,
                      },
                    ]),
                  ),
                ),
              ),
          ),
        )
          .MapErrors((errors) =>
            errors.map((error) => `${error}\n...When parsing create launchers`),
          )
          .Then((createLaunchers) =>
            ValueOrErrors.Operations.All(
              List<
                ValueOrErrors<[string, DispatchParsedEditLauncher<T>], string>
              >(
                specification.launchers.edit
                  .entrySeq()
                  .toArray()
                  .map(([launcherName, launcher]) =>
                    MapRepo.Operations.tryFindWithError(
                      launcher.form,
                      specification.forms,
                      () =>
                        `cannot find form "${launcher.form}" when parsing launchers`,
                    ).Then((parsedForm) =>
                      MapRepo.Operations.tryFindWithError(
                        launcher.configApi,
                        specification.apis.entities,
                        () =>
                          `cannot find global config api "${launcher.configApi}" when parsing launchers`,
                      ).Then((globalConfigApi) =>
                        ValueOrErrors.Default.return([
                          launcherName,
                          {
                            kind: "edit",
                            renderer: parsedForm,
                            type: parsedForm.type,
                            fromApiParser: (raw: any) =>
                              dispatchFromAPIRawValue(
                                parsedForm.type,
                                specification.types,
                                apiConverters,
                                injectedPrimitives,
                              )(raw),
                            parseGlobalConfigurationFromApi: (raw: any) =>
                              dispatchFromAPIRawValue(
                                // TODO: use tryFindWithError
                                specification.types.get(globalConfigApi.type)!,
                                specification.types,
                                apiConverters,
                                injectedPrimitives,
                              )(raw),
                            toApiParser: (
                              value: PredicateValue,
                              type: DispatchParsedType<T>,
                              formState: any,
                            ) =>
                              dispatchToAPIRawValue(
                                type,
                                specification.types,
                                apiConverters,
                                injectedPrimitives,
                              )(value, formState),
                            formName: launcher.form,
                            api: launcher.api,
                            configApi: launcher.configApi,
                          },
                        ]),
                      ),
                    ),
                  ),
              ),
            )
              .MapErrors((errors) =>
                errors.map(
                  (error) => `${error}\n...When parsing edit launchers`,
                ),
              )
              .Then((editLaunchers) =>
                ValueOrErrors.Default.return({
                  launchers: {
                    passthrough: Map(passthroughLaunchers),
                    edit: Map(editLaunchers),
                    create: Map(createLaunchers),
                  },
                  dispatcherContext: {
                    specApis: specification.apis,
                    forms: specification.forms,
                    injectedPrimitives,
                    apiConverters,
                    concreteRenderers,
                    lookupTypeRenderer,
                    getConcreteRendererKind:
                      concreteRendererToKind(concreteRenderers),
                    getConcreteRenderer:
                      tryGetConcreteRenderer(concreteRenderers),
                    getDefaultRecordRenderer: (isNested: boolean) =>
                      getDefaultRecordRenderer(
                        isNested,
                        defaultRecordRenderer,
                        defaultNestedRecordRenderer,
                      ),
                    defaultValue: dispatchDefaultValue(
                      injectedPrimitives,
                      specification.types,
                      specification.forms,
                    ),
                    defaultState: (
                      infiniteStreamSources: DispatchInfiniteStreamSources,
                      lookupSources: DispatchLookupSources | undefined,
                      tableApiSources: DispatchTableApiSources | undefined,
                    ) =>
                      dispatchDefaultState(
                        infiniteStreamSources,
                        injectedPrimitives,
                        specification.types,
                        specification.forms,
                        apiConverters,
                        lookupSources,
                        tableApiSources,
                        specification.apis,
                      ),
                    types: specification.types,
                    parseFromApiByType: (type: DispatchParsedType<T>) =>
                      dispatchFromAPIRawValue(
                        type,
                        specification.types,
                        apiConverters,
                        injectedPrimitives,
                      ),
                    IdProvider,
                    ErrorRenderer,
                    parseToApiByType: (
                      type: DispatchParsedType<T>,
                      value: PredicateValue,
                      state: any,
                    ) =>
                      dispatchToAPIRawValue(
                        type,
                        specification.types,
                        apiConverters,
                        injectedPrimitives,
                      )(value, state),
                  },
                  parseEntityFromApiByTypeLookupName: (
                    typeLookupName: string,
                    raw: any,
                  ) =>
                    MapRepo.Operations.tryFindWithError(
                      typeLookupName,
                      specification.types,
                      () =>
                        `cannot find type "${typeLookupName}" when parsing launchers`,
                    ).Then((type) =>
                      dispatchFromAPIRawValue(
                        type,
                        specification.types,
                        apiConverters,
                        injectedPrimitives,
                      )(raw),
                    ),
                  parseValueToApi: (
                    value: PredicateValue,
                    type: DispatchParsedType<T>,
                    state: any,
                  ) =>
                    dispatchToAPIRawValue(
                      type,
                      specification.types,
                      apiConverters,
                      injectedPrimitives,
                    )(value, state),
                  getTypeByLookupName: (typeLookupName: string) =>
                    MapRepo.Operations.tryFindWithError(
                      typeLookupName,
                      specification.types,
                      () => `cannot find type "${typeLookupName}" in types`,
                    ),
                }),
              ),
          ),
      )
      .MapErrors((errors) =>
        errors.map((error) => `${error}\n...When parsing launchers`),
      );

export type DispatchFormsParserContext<
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
  CustomPresentationContexts = Unit,
  ExtraContext = Unit,
> = {
  lookupTypeRenderer: () => LookupTypeAbstractRendererView<
    CustomPresentationContexts,
    Flags,
    ExtraContext
  >;
  defaultRecordConcreteRenderer: () => RecordAbstractRendererView<
    CustomPresentationContexts,
    Flags,
    ExtraContext
  >;
  defaultNestedRecordConcreteRenderer: () => RecordAbstractRendererView<
    CustomPresentationContexts,
    Flags,
    ExtraContext
  >;
  concreteRenderers: ConcreteRenderers<
    T,
    Flags,
    CustomPresentationContexts,
    ExtraContext
  >;
  IdWrapper: (props: IdWrapperProps) => React.ReactNode;
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode;
  fieldTypeConverters: DispatchApiConverters<T>;
  getFormsConfig: BasicFun<void, Promise<any>>;
  injectedPrimitives?: DispatchInjectables<T>;
};

export type DispatchFormsParserState<
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContexts,
  ExtraContext,
> = {
  deserializedSpecification: Synchronized<
    Unit,
    DispatchSpecificationDeserializationResult<
      T,
      Flags,
      CustomPresentationContexts,
      ExtraContext
    >
  >;
};

export const DispatchFormsParserState = <
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
  CustomPresentationContexts = Unit,
  ExtraContext = Unit,
>() => {
  return {
    Default: (): DispatchFormsParserState<
      T,
      Flags,
      CustomPresentationContexts,
      ExtraContext
    > => ({
      deserializedSpecification: Synchronized.Default(unit),
    }),
    Updaters: {
      ...simpleUpdater<
        DispatchFormsParserState<
          T,
          Flags,
          CustomPresentationContexts,
          ExtraContext
        >
      >()("deserializedSpecification"),
    },
  };
};

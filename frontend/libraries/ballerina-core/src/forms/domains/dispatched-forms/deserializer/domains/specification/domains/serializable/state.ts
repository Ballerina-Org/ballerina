import { List, Map } from "immutable";

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
  DispatchParsedType,
  DispatchTypeName,
  Unit,
  DispatchSpecificationDeserializationResult,
  DispatchInfiniteStreamSources,
  DispatchLookupSources,
  DispatchTableApiSources,
  Renderer,
  IdWrapperProps,
  ErrorRendererProps,
} from "../../../../../../../../../main";

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
} from "../../../../../built-ins/state";

import React from "react";

export type DispatchParsedPassthroughLauncherSerializable<
  T extends DispatchInjectablesTypes<T>,
> = {
  kind: "passthrough";
  formName: string;
  renderer: Renderer<T>;
  commonCtx: {
    specification: Specification<T>;
  };
  parseEntityFromApi: {
    ctx: {
      parsedForm: Renderer<T>;
    };
    // TODO: probably the fn is not needed? - once we have the ctx, we can just define the function inline
    // fn: (props: {
    //   parsedForm: Renderer<T>;
    //   specification: Specification<T>;
    //   injectedPrimitives: DispatchInjectedPrimitives<T> | undefined;
    //   apiConverters: DispatchApiConverters<T>;
    // }) => (_: any) => ValueOrErrors<PredicateValue, string>;
  };
  parseGlobalConfigurationFromApi: {
    ctx: {
      parsedForm: Renderer<T>;
      globalConfigType: DispatchParsedType<T>;
    };
    // fn: (props: {
    //   globalConfigType: DispatchParsedType<T>;
    //   specification: Specification<T>;
    //   injectedPrimitives: DispatchInjectedPrimitives<T> | undefined;
    //   apiConverters: DispatchApiConverters<T>;
    // }) => (_: any) => ValueOrErrors<PredicateValue, string>;
  };
  parseValueToApi: {
    ctx: {};
    // fn: (props: {
    //   specification: Specification<T>;
    //   injectedPrimitives: DispatchInjectedPrimitives<T> | undefined;
    //   apiConverters: DispatchApiConverters<T>;
    // }) => (
    //   value: PredicateValue,
    //   type: DispatchParsedType<T>,
    //   state: any,
    // ) => ValueOrErrors<any, string>;
  };
  type: DispatchParsedType<T>;
};

export type DispatchParsedEntityLauncherSerializable<
  T extends DispatchInjectablesTypes<T>,
> = {
  kind: "create" | "edit";
  formName: string;
  renderer: Renderer<T>;
  commonCtx: {
    specification: Specification<T>;
  };
  fromApiParser: {
    ctx: {
      parsedForm: Renderer<T>;
    };
  };
  toApiParser: {
    ctx: {};
  };
  parseGlobalConfigurationFromApi: {
    ctx: {
      globalConfigType: DispatchParsedType<T>;
    };
  };
  api: string;
  configApi: string;
  type: DispatchParsedType<T>;
};

export type DispatchParsedLaunchersSerializable<
  T extends DispatchInjectablesTypes<T>,
> = {
  passthrough: Map<string, DispatchParsedPassthroughLauncherSerializable<T>>;
  edit: Map<string, DispatchParsedEntityLauncherSerializable<T>>;
  create: Map<string, DispatchParsedEntityLauncherSerializable<T>>;
};

export type DispatcherContextSerializable<
  T extends DispatchInjectablesTypes<T>,
  Flags,
  CustomPresentationContext,
  ExtraContext,
> = {
  injectedPrimitives: DispatchInjectedPrimitives<T> | undefined;
  apiConverters: DispatchApiConverters<T>;
  specApis: SpecificationApis<T>;
  concreteRenderers: ConcreteRenderers<
    T,
    Flags,
    CustomPresentationContext,
    ExtraContext
  >;
  forms: Map<string, Renderer<T>>;
  types: Map<DispatchTypeName, DispatchParsedType<T>>;
};

export type DeserializedDispatchSpecificationSerializable<
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
> = {
  launchers: DispatchParsedLaunchersSerializable<T>;
  // dispatcherContext: DispatcherContextSerializable<
  //   T,
  //   Flags,
  //   CustomPresentationContext,
  //   ExtraContext
  // >;
};

export type DispatchSpecificationDeserializationResultSerializable<
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
  CustomPresentationContext = Unit,
  ExtraContext = Unit,
> = ValueOrErrors<
  DeserializedDispatchSpecificationSerializable<
    T,
    Flags,
    CustomPresentationContext,
    ExtraContext
  >,
  string
>;
export const hydrateDeserializedDispatchForms =
  <
    T extends DispatchInjectablesTypes<T>,
    Flags,
    CustomPresentationContext,
    ExtraContext,
  >(
    injectedPrimitives: DispatchInjectedPrimitives<T> | undefined,
    apiConverters: DispatchApiConverters<T>,
    lookupTypeRenderer: () => LookupTypeAbstractRendererView<
      CustomPresentationContext,
      Flags,
      ExtraContext
    >,
    defaultRecordRenderer: () => RecordAbstractRendererView<
      CustomPresentationContext,
      Flags,
      ExtraContext
    >,
    defaultNestedRecordRenderer: () => RecordAbstractRendererView<
      CustomPresentationContext,
      Flags,
      ExtraContext
    >,
    concreteRenderers: ConcreteRenderers<
      T,
      Flags,
      CustomPresentationContext,
      ExtraContext
    >,
    IdProvider: (props: IdWrapperProps) => React.ReactNode,
    ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
  ) =>
  (
    deserializedDispatchForms: DispatchSpecificationDeserializationResultSerializable<
      T,
      Flags,
      CustomPresentationContext,
      ExtraContext
    >,
    specification: Specification<T>,
  ): DispatchSpecificationDeserializationResult<
    T,
    Flags,
    CustomPresentationContext,
    ExtraContext
  > =>
    deserializedDispatchForms.kind === "errors"
      ? ValueOrErrors.Default.throw(deserializedDispatchForms.errors)
      : ValueOrErrors.Default.return({
          launchers: {
            passthrough:
              deserializedDispatchForms.value.launchers.passthrough.map(
                (launcher) => ({
                  ...launcher,
                  parseEntityFromApi: (raw: any) =>
                    dispatchFromAPIRawValue(
                      launcher.parseEntityFromApi.ctx.parsedForm.type,
                      specification.types,
                      apiConverters,
                      injectedPrimitives,
                    )(raw),
                  parseGlobalConfigurationFromApi: (raw: any) =>
                    dispatchFromAPIRawValue(
                      launcher.parseGlobalConfigurationFromApi.ctx
                        .globalConfigType,
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
                }),
              ),
            edit: deserializedDispatchForms.value.launchers.edit.map(
              (launcher) => ({
                ...launcher,
                kind: "edit",
                fromApiParser: (raw: any) =>
                  dispatchFromAPIRawValue(
                    launcher.fromApiParser.ctx.parsedForm.type,
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
                parseGlobalConfigurationFromApi: (raw: any) =>
                  dispatchFromAPIRawValue(
                    launcher.parseGlobalConfigurationFromApi.ctx
                      .globalConfigType,
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
              }),
            ),
            create: deserializedDispatchForms.value.launchers.create.map(
              (launcher) => ({
                ...launcher,
                kind: "create",
                fromApiParser: (raw: any) =>
                  dispatchFromAPIRawValue(
                    launcher.fromApiParser.ctx.parsedForm.type,
                    specification.types,
                    apiConverters,
                    injectedPrimitives,
                  )(raw),
                parseGlobalConfigurationFromApi: (raw: any) =>
                  dispatchFromAPIRawValue(
                    launcher.parseGlobalConfigurationFromApi.ctx
                      .globalConfigType,
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
              }),
            ),
          },
          dispatcherContext: {
            specApis: specification.apis,
            forms: specification.forms,
            types: specification.types,
            injectedPrimitives,
            apiConverters,
            concreteRenderers,
            lookupTypeRenderer,
            getConcreteRendererKind: concreteRendererToKind(concreteRenderers),
            getConcreteRenderer: tryGetConcreteRenderer(concreteRenderers),
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
            IdProvider,
            ErrorRenderer,
            parseFromApiByType: (type: DispatchParsedType<T>) =>
              dispatchFromAPIRawValue(
                type,
                specification.types,
                apiConverters,
                injectedPrimitives,
              ),
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
          getTypeByLookupName: (typeLookupName: string) =>
            MapRepo.Operations.tryFindWithError(
              typeLookupName,
              specification.types,
              () => `cannot find type "${typeLookupName}" in types`,
            ),
        });

export const parseDispatchFormsToLaunchersSerializable =
  <
    T extends DispatchInjectablesTypes<T>,
    Flags,
    CustomPresentationContext,
    ExtraContext,
  >(
    injectedPrimitives: DispatchInjectedPrimitives<T> | undefined,
    apiConverters: DispatchApiConverters<T>,
    lookupTypeRenderer: () => LookupTypeAbstractRendererView<
      CustomPresentationContext,
      Flags,
      ExtraContext
    >,
    defaultRecordRenderer: () => RecordAbstractRendererView<
      CustomPresentationContext,
      Flags,
      ExtraContext
    >,
    defaultNestedRecordRenderer: () => RecordAbstractRendererView<
      CustomPresentationContext,
      Flags,
      ExtraContext
    >,
    concreteRenderers: ConcreteRenderers<
      T,
      Flags,
      CustomPresentationContext,
      ExtraContext
    >,
    IdProvider: (props: IdWrapperProps) => React.ReactNode,
    ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
  ) =>
  (
    specification: Specification<T>,
  ): DispatchSpecificationDeserializationResultSerializable<
    T,
    Flags,
    CustomPresentationContext,
    ExtraContext
  > =>
    ValueOrErrors.Operations.All(
      List<
        ValueOrErrors<
          [string, DispatchParsedPassthroughLauncherSerializable<T>],
          string
        >
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
                    commonCtx: {
                      specification,
                    },
                    parseEntityFromApi: {
                      ctx: {
                        parsedForm,
                      },
                      // fn: (props) => (raw: any) =>
                      //   dispatchFromAPIRawValue(
                      //     props.parsedForm.type,
                      //     props.specification.types,
                      //     props.apiConverters,
                      //     props.injectedPrimitives,
                      //   )(raw),
                    },
                    parseGlobalConfigurationFromApi: {
                      ctx: {
                        parsedForm,
                        globalConfigType,
                      },
                      // fn: (props) => (raw: any) =>
                      //   dispatchFromAPIRawValue(
                      //     props.globalConfigType,
                      //     props.specification.types,
                      //     props.apiConverters,
                      //     props.injectedPrimitives,
                      //   )(raw),
                    },
                    parseValueToApi: {
                      ctx: {},
                      // fn:
                      //   (props) =>
                      //   (
                      //     value: PredicateValue,
                      //     type: DispatchParsedType<T>,
                      //     state: any,
                      //   ) =>
                      //     dispatchToAPIRawValue(
                      //       type,
                      //       props.specification.types,
                      //       props.apiConverters,
                      //       props.injectedPrimitives,
                      //     )(value, state),
                    },
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
            ValueOrErrors<
              [string, DispatchParsedEntityLauncherSerializable<T>],
              string
            >
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
                    MapRepo.Operations.tryFindWithError(
                      globalConfigApi.type,
                      specification.types,
                      () =>
                        `cannot find global config type "${globalConfigApi.type}" when parsing launchers`,
                    ).Then((globalConfigType) =>
                      ValueOrErrors.Default.return([
                        launcherName,
                        {
                          kind: "create",
                          renderer: parsedForm,
                          type: parsedForm.type,
                          commonCtx: {
                            specification,
                          },
                          fromApiParser: {
                            ctx: {
                              parsedForm,
                            },
                          },
                          parseGlobalConfigurationFromApi: {
                            ctx: { globalConfigType },
                          },
                          toApiParser: {
                            ctx: {},
                          },
                          formName: launcher.form,
                          api: launcher.api,
                          configApi: launcher.configApi,
                        },
                      ]),
                    ),
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
                ValueOrErrors<
                  [string, DispatchParsedEntityLauncherSerializable<T>],
                  string
                >
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
                        MapRepo.Operations.tryFindWithError(
                          globalConfigApi.type,
                          specification.types,
                          () =>
                            `cannot find global config type "${globalConfigApi.type}" when parsing launchers`,
                        ).Then((globalConfigType) =>
                          ValueOrErrors.Default.return([
                            launcherName,
                            {
                              kind: "edit",
                              renderer: parsedForm,
                              type: parsedForm.type,
                              commonCtx: {
                                specification,
                              },
                              fromApiParser: {
                                ctx: {
                                  parsedForm,
                                },
                              },
                              parseGlobalConfigurationFromApi: {
                                ctx: { globalConfigType },
                              },
                              toApiParser: {
                                ctx: {},
                              },
                              formName: launcher.form,
                              api: launcher.api,
                              configApi: launcher.configApi,
                            },
                          ]),
                        ),
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
                }),
              ),
          ),
      )
      .MapErrors((errors) =>
        errors.map((error) => `${error}\n...When parsing launchers`),
      );

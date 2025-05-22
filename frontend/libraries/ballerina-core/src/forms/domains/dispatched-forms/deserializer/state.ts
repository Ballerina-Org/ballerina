import { List, Map } from "immutable";

import {
  DispatchParsedType,
  DispatchTypeName,
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
  Injectables,
  Synchronized,
  simpleUpdater,
  ValueInfiniteStreamState,
  MapRepo,
  OneAbstractRendererState,
  ValueOption,
  DispatchInjectables,
} from "../../../../../main";

import {
  DispatchApiConverters,
  ConcreteRendererKinds,
  concreteRendererToKind,
  dispatchDefaultState,
  dispatchDefaultValue,
  dispatchFromAPIRawValue,
  dispatchToAPIRawValue,
  tryGetConcreteRenderer,
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

export type DispatchParsedLauncher<T> = DispatchParsedPassthroughLauncher<T>;

export type DispatchParsedLaunchers<T> = {
  passthrough: Map<string, DispatchParsedPassthroughLauncher<T>>;
};

export type IdWrapperProps = {
  domNodeId: string;
  children: React.ReactNode;
};

export type ErrorRendererProps = {
  message: string;
};

export type DispatcherContext<
  T extends { [key in keyof T]: { type: any; state: any } },
> = {
  injectedPrimitives: DispatchInjectedPrimitives<T> | undefined;
  apiConverters: DispatchApiConverters<T>;
  infiniteStreamSources: DispatchInfiniteStreamSources;
  enumOptionsSources: DispatchEnumOptionsSources;
  entityApis: DispatchEntityApis;
  getConcreteRendererKind: (viewName: string) => ValueOrErrors<string, string>;
  getConcreteRenderer: (
    kind: keyof ConcreteRendererKinds<T>,
    name?: string,
    isNested?: boolean,
  ) => ValueOrErrors<any, string>;
  defaultValue: (
    t: DispatchParsedType<T>,
    renderer: Renderer<T>,
  ) => ValueOrErrors<PredicateValue, string>;
  defaultState: (
    t: DispatchParsedType<T>,
    renderer: Renderer<T>,
  ) => ValueOrErrors<any, string>;
  forms: Map<string, Renderer<T>>;
  types: Map<DispatchTypeName, DispatchParsedType<T>>;
  IdProvider: (props: IdWrapperProps) => React.ReactNode;
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode;
  tableApiSources?: DispatchTableApiSources;
  lookupSources?: DispatchLookupSources;
  parseFromApiByType: (
    type: DispatchParsedType<T>,
  ) => (raw: any) => ValueOrErrors<PredicateValue, string>;
};

export type DispatchSpecificationDeserializationResult<
  T extends { [key in keyof T]: { type: any; state: any } },
> = ValueOrErrors<
  {
    launchers: DispatchParsedLaunchers<T>;
    dispatcherContext: DispatcherContext<T>;
  },
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
export type DispatchTableApiName = string;
export type DispatchTableApiSource = {
  get: BasicFun<Guid, Promise<any>>;
  getMany: BasicFun<
    BasicFun<any, ValueOrErrors<PredicateValue, string>>,
    BasicFun<Map<string, string>, ValueInfiniteStreamState["getChunk"]>
  >;
};

export type DispatchTableApiSources = BasicFun<
  string,
  ValueOrErrors<DispatchTableApiSource, string>
>;

export type DispatchApiName = string;
export type DispatchOneSource = {
  get: BasicFun<Guid, Promise<any>>;
  getManyUnlinked: BasicFun<
    BasicFun<any, ValueOrErrors<PredicateValue, string>>,
    BasicFun<
      Guid,
      BasicFun<Map<string, string>, ValueInfiniteStreamState["getChunk"]>
    >
  >;
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
  <T extends { [key in keyof T]: { type: any; state: any } }>(
    injectedPrimitives: DispatchInjectedPrimitives<T> | undefined,
    apiConverters: DispatchApiConverters<T>,
    defaultRecordRenderer: any,
    defaultNestedRecordRenderer: any,
    concreteRenderers: Record<keyof ConcreteRendererKinds<T>, any>,
    infiniteStreamSources: DispatchInfiniteStreamSources,
    enumOptionsSources: DispatchEnumOptionsSources,
    entityApis: DispatchEntityApis,
    IdProvider: (props: IdWrapperProps) => React.ReactNode,
    ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
    tableApiSources?: DispatchTableApiSources,
    lookupSources?: DispatchLookupSources,
  ) =>
  (
    specification: Specification<T>,
  ): DispatchSpecificationDeserializationResult<T> =>
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
      .Then((passthroughLaunchers) =>
        ValueOrErrors.Default.return({
          launchers: {
            passthrough: Map(passthroughLaunchers),
          },
          dispatcherContext: {
            forms: specification.forms,
            injectedPrimitives,
            apiConverters,
            infiniteStreamSources,
            lookupSources,
            enumOptionsSources,
            tableApiSources,
            entityApis,
            getConcreteRendererKind: concreteRendererToKind(concreteRenderers),
            getConcreteRenderer: tryGetConcreteRenderer(
              concreteRenderers,
              defaultRecordRenderer,
              defaultNestedRecordRenderer,
            ),
            defaultValue: dispatchDefaultValue(
              injectedPrimitives,
              specification.types,
              specification.forms,
            ),
            defaultState: dispatchDefaultState(
              infiniteStreamSources,
              injectedPrimitives,
              specification.types,
              specification.forms,
              apiConverters,
              lookupSources,
              tableApiSources,
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
          },
        }),
      )
      .MapErrors((errors) =>
        errors.map((error) => `${error}\n...When parsing launchers`),
      );

export type DispatchFormsParserContext<
  T extends { [key in keyof T]: { type: any; state: any } },
> = {
  defaultRecordConcreteRenderer: any;
  defaultNestedRecordConcreteRenderer: any;
  concreteRenderers: Record<keyof ConcreteRendererKinds<T>, any>;
  IdWrapper: (props: IdWrapperProps) => React.ReactNode;
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode;
  fieldTypeConverters: DispatchApiConverters<T>;
  infiniteStreamSources: DispatchInfiniteStreamSources;
  lookupSources?: DispatchLookupSources;
  enumOptionsSources: DispatchEnumOptionsSources;
  entityApis: DispatchEntityApis;
  getFormsConfig: BasicFun<void, Promise<any>>;
  injectedPrimitives?: DispatchInjectables<T>;
  tableApiSources?: DispatchTableApiSources;
};
export type DispatchFormsParserState<
  T extends { [key in keyof T]: { type: any; state: any } },
> = {
  deserializedSpecification: Synchronized<
    Unit,
    DispatchSpecificationDeserializationResult<T>
  >;
};
export const DispatchFormsParserState = <
  T extends { [key in keyof T]: { type: any; state: any } },
>() => {
  return {
    Default: (): DispatchFormsParserState<T> => ({
      deserializedSpecification: Synchronized.Default(unit),
    }),
    Updaters: {
      ...simpleUpdater<DispatchFormsParserState<T>>()(
        "deserializedSpecification",
      ),
    },
  };
};

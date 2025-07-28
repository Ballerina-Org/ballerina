import { Map, List, OrderedMap } from "immutable";
import {
  CollectionReference,
  EnumReference,
} from "../../collection/domains/reference/state";
import { CollectionSelection } from "../../collection/domains/selection/state";
import { BasicFun } from "../../../../fun/state";
import { ValueOrErrors } from "../../../../collections/domains/valueOrErrors/state";

import {
  PredicateValue,
  replaceWith,
  Sum,
  Unit,
  ValueRecord,
  unit,
  ValueTuple,
  DispatchInfiniteStreamSources,
  DispatchInjectedPrimitives,
  RecordAbstractRendererState,
  TableAbstractRendererState,
  MapRepo,
  ValueTable,
  DispatchCommonFormState,
  UnionAbstractRendererState,
  OneAbstractRendererState,
  DispatchLookupSources,
  DispatchTableApiSources,
  ValueOption,
  BasicUpdater,
  DispatchDelta,
  Option,
  CommonAbstractRendererState,
  EnumMultiselectAbstractRendererView,
  RecordAbstractRendererView,
  SearchableInfiniteStreamMultiselectAbstractRendererView,
  OneAbstractRendererView,
  ReadOnlyAbstractRendererView,
  TableAbstractRendererView,
  UnionAbstractRendererView,
  DispatchInjectablesTypes,
  SpecificationApis,
} from "../../../../../main";
import {
  DispatchParsedType,
  DispatchTypeName,
} from "../deserializer/domains/specification/domains/types/state";
import {
  UnitAbstractRendererState,
  UnitAbstractRendererView,
} from "../runner/domains/abstract-renderers/unit/state";
import {
  StringAbstractRendererState,
  StringAbstractRendererView,
} from "../runner/domains/abstract-renderers/string/state";
import {
  NumberAbstractRendererState,
  NumberAbstractRendererView,
} from "../runner/domains/abstract-renderers/number/state";
import {
  BoolAbstractRendererState,
  BoolAbstractRendererView,
} from "../runner/domains/abstract-renderers/boolean/state";
import {
  DateAbstractRendererState,
  DateAbstractRendererView,
} from "../runner/domains/abstract-renderers/date/state";
import {
  Base64FileAbstractRendererState,
  Base64FileAbstractRendererView,
} from "../runner/domains/abstract-renderers/base-64-file/state";
import {
  SecretAbstractRendererState,
  SecretAbstractRendererView,
} from "../runner/domains/abstract-renderers/secret/state";
import { ReadOnlyAbstractRendererState } from "../runner/domains/abstract-renderers/readOnly/state";
import {
  MapAbstractRendererState,
  MapAbstractRendererView,
} from "../runner/domains/abstract-renderers/map/state";
import {
  TupleAbstractRendererState,
  TupleAbstractRendererView,
} from "../runner/domains/abstract-renderers/tuple/state";
import {
  SumAbstractRendererState,
  SumAbstractRendererView,
} from "../runner/domains/abstract-renderers/sum/state";
import {
  EnumAbstractRendererState,
  EnumAbstractRendererView,
} from "../runner/domains/abstract-renderers/enum/state";
import {
  ListAbstractRendererState,
  ListAbstractRendererView,
} from "../runner/domains/abstract-renderers/list/state";
import {
  SearchableInfiniteStreamAbstractRendererState,
  SearchableInfiniteStreamAbstractRendererView,
} from "../runner/domains/abstract-renderers/searchable-infinite-stream/state";
import { Renderer } from "../deserializer/domains/specification/domains/forms/domains/renderer/state";
import { LookupRenderer } from "../deserializer/domains/specification/domains/forms/domains/renderer/domains/lookup/state";

const sortObjectKeys = (obj: Record<string, any>) =>
  Object.keys(obj)
    .sort()
    .reduce((sortedObj, key) => {
      sortedObj[key] = obj[key]!;
      return sortedObj;
    }, {} as any);

const simpleMapKeyToIdentifer = (key: any): string => {
  if (typeof key == "object") return JSON.stringify(sortObjectKeys(key));
  return JSON.stringify(key);
};

export type VoidCallbackWithOptionalFlags<Flags = Unit> = (
  flags: Flags | undefined,
) => void;

export type ValueCallbackWithOptionalFlags<Value, Flags = Unit> = (
  value: Value,
  flags: Flags | undefined,
) => void;

export type DispatchOnChange<Value, Flags = Unit> = (
  updater: Option<BasicUpdater<Value>>,
  delta: DispatchDelta<Flags>,
) => void;

type ApiConverter<T> = {
  fromAPIRawValue: BasicFun<any, T>;
  toAPIRawValue: BasicFun<[T, boolean], any>;
};

export type DispatchApiConverters<T extends DispatchInjectablesTypes<T>> = {
  [key in keyof T]: ApiConverter<T[key]["type"]>;
} & BuiltInApiConverters;

type RawUnion = {
  caseName: string;
  fields: Record<string, any>;
};

type Table = {
  data: Map<string, Record<string, any>>;
  hasMoreValues: boolean;
  from: number;
  to: number;
};

export const DispatchGenericTypes = [
  "SingleSelection",
  "MultiSelection",
  "List",
  "Map",
  "Union",
  "Tuple",
  "Option",
  "Sum",
  "KeyOf",
  "Table",
  "One",
  "ReadOnly",
] as const;
export type DispatchGenericType = (typeof DispatchGenericTypes)[number];

type BuiltInApiConverters = {
  string: ApiConverter<string>;
  number: ApiConverter<number>;
  boolean: ApiConverter<boolean>;
  base64File: ApiConverter<string>;
  secret: ApiConverter<string>;
  Date: ApiConverter<Date>;
  union: ApiConverter<RawUnion>;
  SingleSelection: ApiConverter<
    CollectionSelection<CollectionReference | EnumReference>
  >;
  MultiSelection: ApiConverter<
    OrderedMap<string, CollectionReference | EnumReference>
  >;
  List: ApiConverter<List<any>>;
  Map: ApiConverter<List<[any, any]>>;
  Tuple: ApiConverter<List<any>>;
  Sum: ApiConverter<Sum<any, any>>;
  SumUnitDate: ApiConverter<Sum<Unit, Date>>;
  Table: ApiConverter<Table>;
  One: ApiConverter<ValueOption>;
  ReadOnly: ApiConverter<any>;
};

export type ConcreteRenderers<
  T extends DispatchInjectablesTypes<T>,
  Flags = Unit,
  CustomPresentationContexts = Unit,
  ExtraContext = Unit,
> = {
  unit: {
    [_: string]: () =>
      | UnitAbstractRendererView<
          CustomPresentationContexts,
          Flags,
          ExtraContext
        >
      | React.MemoExoticComponent<
          UnitAbstractRendererView<
            CustomPresentationContexts,
            Flags,
            ExtraContext
          >
        >;
  };
  boolean: {
    [_: string]: () =>
      | BoolAbstractRendererView<
          CustomPresentationContexts,
          Flags,
          ExtraContext
        >
      | React.MemoExoticComponent<
          BoolAbstractRendererView<
            CustomPresentationContexts,
            Flags,
            ExtraContext
          >
        >;
  };
  number: {
    [_: string]: () =>
      | NumberAbstractRendererView<
          CustomPresentationContexts,
          Flags,
          ExtraContext
        >
      | React.MemoExoticComponent<
          NumberAbstractRendererView<
            CustomPresentationContexts,
            Flags,
            ExtraContext
          >
        >;
  };
  string: {
    [_: string]: () =>
      | StringAbstractRendererView<
          CustomPresentationContexts,
          Flags,
          ExtraContext
        >
      | React.MemoExoticComponent<
          StringAbstractRendererView<
            CustomPresentationContexts,
            Flags,
            ExtraContext
          >
        >;
  };
  base64File: {
    [_: string]: () =>
      | Base64FileAbstractRendererView<
          CustomPresentationContexts,
          Flags,
          ExtraContext
        >
      | React.MemoExoticComponent<
          Base64FileAbstractRendererView<
            CustomPresentationContexts,
            Flags,
            ExtraContext
          >
        >;
  };
  secret: {
    [_: string]: () =>
      | SecretAbstractRendererView<
          CustomPresentationContexts,
          Flags,
          ExtraContext
        >
      | React.MemoExoticComponent<
          SecretAbstractRendererView<
            CustomPresentationContexts,
            Flags,
            ExtraContext
          >
        >;
  };
  date: {
    [_: string]: () =>
      | DateAbstractRendererView<
          CustomPresentationContexts,
          Flags,
          ExtraContext
        >
      | React.MemoExoticComponent<
          DateAbstractRendererView<
            CustomPresentationContexts,
            Flags,
            ExtraContext
          >
        >;
  };
  enumSingleSelection: {
    [_: string]: () =>
      | EnumAbstractRendererView<
          CustomPresentationContexts,
          Flags,
          ExtraContext
        >
      | React.MemoExoticComponent<
          EnumAbstractRendererView<
            CustomPresentationContexts,
            Flags,
            ExtraContext
          >
        >;
  };
  enumMultiSelection: {
    [_: string]: () =>
      | EnumMultiselectAbstractRendererView<
          CustomPresentationContexts,
          Flags,
          ExtraContext
        >
      | React.MemoExoticComponent<
          EnumMultiselectAbstractRendererView<
            CustomPresentationContexts,
            Flags,
            ExtraContext
          >
        >;
  };
  streamSingleSelection: {
    [_: string]: () =>
      | SearchableInfiniteStreamAbstractRendererView<
          CustomPresentationContexts,
          Flags,
          ExtraContext
        >
      | React.MemoExoticComponent<
          SearchableInfiniteStreamAbstractRendererView<
            CustomPresentationContexts,
            Flags,
            ExtraContext
          >
        >;
  };
  streamMultiSelection: {
    [_: string]: () =>
      | SearchableInfiniteStreamMultiselectAbstractRendererView<
          CustomPresentationContexts,
          Flags,
          ExtraContext
        >
      | React.MemoExoticComponent<
          SearchableInfiniteStreamMultiselectAbstractRendererView<
            CustomPresentationContexts,
            Flags,
            ExtraContext
          >
        >;
  };
  list: {
    [_: string]: () =>
      | ListAbstractRendererView<
          CustomPresentationContexts,
          Flags,
          ExtraContext
        >
      | React.MemoExoticComponent<
          ListAbstractRendererView<
            CustomPresentationContexts,
            Flags,
            ExtraContext
          >
        >;
  };
  map: {
    [_: string]: () =>
      | MapAbstractRendererView<CustomPresentationContexts, Flags, ExtraContext>
      | React.MemoExoticComponent<
          MapAbstractRendererView<
            CustomPresentationContexts,
            Flags,
            ExtraContext
          >
        >;
  };
  tuple: {
    [_: string]: () =>
      | TupleAbstractRendererView<
          CustomPresentationContexts,
          Flags,
          ExtraContext
        >
      | React.MemoExoticComponent<
          TupleAbstractRendererView<
            CustomPresentationContexts,
            Flags,
            ExtraContext
          >
        >;
  };
  sum: {
    [_: string]: () =>
      | SumAbstractRendererView<CustomPresentationContexts, Flags, ExtraContext>
      | React.MemoExoticComponent<
          SumAbstractRendererView<
            CustomPresentationContexts,
            Flags,
            ExtraContext
          >
        >;
  };
  sumUnitDate: {
    [_: string]: () =>
      | SumAbstractRendererView<CustomPresentationContexts, Flags, ExtraContext>
      | React.MemoExoticComponent<
          SumAbstractRendererView<
            CustomPresentationContexts,
            Flags,
            ExtraContext
          >
        >;
  };
  record: {
    [_: string]: () =>
      | RecordAbstractRendererView<
          CustomPresentationContexts,
          Flags,
          ExtraContext
        >
      | React.MemoExoticComponent<
          RecordAbstractRendererView<
            CustomPresentationContexts,
            Flags,
            ExtraContext
          >
        >;
  };
  table: {
    [_: string]: () =>
      | TableAbstractRendererView<
          CustomPresentationContexts,
          Flags,
          ExtraContext
        >
      | React.MemoExoticComponent<
          TableAbstractRendererView<
            CustomPresentationContexts,
            Flags,
            ExtraContext
          >
        >;
  };
  union: {
    [_: string]: () =>
      | UnionAbstractRendererView<
          CustomPresentationContexts,
          Flags,
          ExtraContext
        >
      | React.MemoExoticComponent<
          UnionAbstractRendererView<
            CustomPresentationContexts,
            Flags,
            ExtraContext
          >
        >;
  };
  one: {
    [_: string]: () =>
      | OneAbstractRendererView<CustomPresentationContexts, Flags, ExtraContext>
      | React.MemoExoticComponent<
          OneAbstractRendererView<
            CustomPresentationContexts,
            Flags,
            ExtraContext
          >
        >;
  };
  readOnly: {
    [_: string]: () =>
      | ReadOnlyAbstractRendererView<
          CustomPresentationContexts,
          Flags,
          ExtraContext
        >
      | React.MemoExoticComponent<
          ReadOnlyAbstractRendererView<
            CustomPresentationContexts,
            Flags,
            ExtraContext
          >
        >;
  };
} & {
  [key in keyof T]: { [_: string]: () => T[key]["view"] };
};

export type ConcreteRenderer<T> =
  | Base64FileAbstractRendererView<any, any>
  | BoolAbstractRendererView<any, any>
  | DateAbstractRendererView<any, any>
  | EnumAbstractRendererView<any, any>
  | EnumMultiselectAbstractRendererView<any, any>
  | ListAbstractRendererView<any, any>
  | MapAbstractRendererView<any, any>
  | NumberAbstractRendererView<any, any>
  | OneAbstractRendererView<any, any>
  | ReadOnlyAbstractRendererView<any, any>
  | RecordAbstractRendererView<any, any>
  | SearchableInfiniteStreamAbstractRendererView<any, any>
  | SearchableInfiniteStreamMultiselectAbstractRendererView<any, any>
  | SecretAbstractRendererView<any, any>
  | StringAbstractRendererView<any, any>
  | SumAbstractRendererView<any, any>
  | TableAbstractRendererView<any, any>
  | TupleAbstractRendererView<any, any>
  | UnionAbstractRendererView<any, any>
  | UnitAbstractRendererView<any, any>;

export const concreteRendererToKind =
  <
    T extends DispatchInjectablesTypes<T>,
    Flags,
    CustomPresentationContexts,
    ExtraContext = Unit,
  >(
    concreteRenderers: ConcreteRenderers<
      T,
      Flags,
      CustomPresentationContexts,
      ExtraContext
    >,
  ) =>
  (name: string): ValueOrErrors<string, string> => {
    const viewTypes = Object.keys(concreteRenderers);
    for (const viewType of viewTypes) {
      if (name in concreteRenderers[viewType as keyof ConcreteRenderers<T>]) {
        return ValueOrErrors.Default.return(viewType);
      }
    }
    return ValueOrErrors.Default.throwOne(
      `cannot find view ${name} in concrete renderers`,
    );
  };

export const tryGetConcreteRenderer =
  <
    T extends DispatchInjectablesTypes<T>,
    Flags,
    CustomPresentationContexts,
    ExtraContext,
  >(
    concreteRenderers: ConcreteRenderers<
      T,
      Flags,
      CustomPresentationContexts,
      ExtraContext
    >,
  ) =>
  <
    K extends keyof ConcreteRenderers<
      T,
      Flags,
      CustomPresentationContexts,
      ExtraContext
    >,
  >(
    kind: K,
    name: keyof ConcreteRenderers<
      T,
      Flags,
      CustomPresentationContexts,
      ExtraContext
    >[K],
  ): ValueOrErrors<
    ReturnType<
      ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >[K][keyof ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContexts,
        ExtraContext
      >[K]]
    >,
    string
  > => {
    if (!concreteRenderers[kind]) {
      return ValueOrErrors.Default.throwOne(
        `cannot find concrete renderer kind "${kind as string}" in  concrete renderers`,
      );
    }
    if (concreteRenderers[kind][name]) {
      return ValueOrErrors.Default.return(concreteRenderers[kind][name]());
    }
    return ValueOrErrors.Default.throwOne(
      `cannot find concrete renderer "${name as string}" in kind "${kind as string}"`,
    );
  };

export const getDefaultRecordRenderer = <
  CustomPresentationContexts,
  Flags,
  ExtraContext,
>(
  isNested: boolean,
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
): RecordAbstractRendererView<
  CustomPresentationContexts,
  Flags,
  ExtraContext
> => (isNested ? defaultNestedRecordRenderer() : defaultRecordRenderer());

export const dispatchDefaultState =
  <T extends DispatchInjectablesTypes<T>>(
    infiniteStreamSources: DispatchInfiniteStreamSources,
    injectedPrimitives: DispatchInjectedPrimitives<T> | undefined,
    types: Map<DispatchTypeName, DispatchParsedType<T>>,
    forms: Map<string, Renderer<T>>,
    converters: DispatchApiConverters<T>,
    lookupSources: DispatchLookupSources | undefined,
    tableApiSources: DispatchTableApiSources | undefined,
    specApis: SpecificationApis,
  ) =>
  (
    t: DispatchParsedType<T>,
    renderer: Renderer<T>,
  ): ValueOrErrors<any, string> => {
    const result: ValueOrErrors<any, string> = (() => {
      if (renderer == undefined) {
        return ValueOrErrors.Default.return(undefined);
      }

      if (renderer.kind == "lookupType-lookupRenderer") {
        return DispatchParsedType.Operations.ResolveLookupType(
          renderer.type.name,
          types,
        ).Then((resolvedType) =>
          LookupRenderer.Operations.ResolveRenderer(renderer, forms).Then(
            (resolvedRenderer) =>
              dispatchDefaultState(
                infiniteStreamSources,
                injectedPrimitives,
                types,
                forms,
                converters,
                lookupSources,
                tableApiSources,
                specApis,
              )(resolvedType, resolvedRenderer),
          ),
        );
      }

      if (renderer.kind == "lookupType-inlinedRenderer") {
        return DispatchParsedType.Operations.ResolveLookupType(
          renderer.type.name,
          types,
        ).Then((resolvedType) =>
          dispatchDefaultState(
            infiniteStreamSources,
            injectedPrimitives,
            types,
            forms,
            converters,
            lookupSources,
            tableApiSources,
            specApis,
          )(resolvedType, renderer.inlinedRenderer),
        );
      }

      if (renderer.kind == "inlinedType-lookupRenderer") {
        return LookupRenderer.Operations.ResolveRenderer(renderer, forms).Then(
          (resolvedRenderer) =>
            dispatchDefaultState(
              infiniteStreamSources,
              injectedPrimitives,
              types,
              forms,
              converters,
              lookupSources,
              tableApiSources,
              specApis,
            )(renderer.type, resolvedRenderer),
        );
      }

      if (t.kind == "primitive")
        return t.name == "unit"
          ? ValueOrErrors.Default.return(UnitAbstractRendererState.Default())
          : t.name == "boolean"
            ? ValueOrErrors.Default.return(BoolAbstractRendererState.Default())
            : t.name == "number"
              ? ValueOrErrors.Default.return(
                  NumberAbstractRendererState.Default(),
                )
              : t.name == "string"
                ? ValueOrErrors.Default.return(
                    StringAbstractRendererState.Default(),
                  )
                : t.name == "base64File"
                  ? ValueOrErrors.Default.return(
                      Base64FileAbstractRendererState.Default(),
                    )
                  : t.name == "secret"
                    ? ValueOrErrors.Default.return(
                        SecretAbstractRendererState.Default(),
                      )
                    : t.name == "Date"
                      ? ValueOrErrors.Default.return(
                          DateAbstractRendererState.Default(),
                        )
                      : injectedPrimitives?.get(t.name as keyof T) != undefined
                        ? ValueOrErrors.Default.return({
                            commonFormState: DispatchCommonFormState.Default(),
                            ...injectedPrimitives?.get(t.name as keyof T)!
                              .defaultState,
                          })
                        : ValueOrErrors.Default.throwOne(
                            `could not resolve defaultState for primitive renderer kind "${
                              t.name as string
                            }"`,
                          );

      if (t.kind == "singleSelection")
        return renderer.kind == "enumRenderer"
          ? ValueOrErrors.Default.return(EnumAbstractRendererState().Default())
          : renderer.kind == "streamRenderer"
            ? infiniteStreamSources(renderer.stream).Then((streamSource) =>
                ValueOrErrors.Default.return(
                  SearchableInfiniteStreamAbstractRendererState.Default(
                    streamSource,
                  ),
                ),
              )
            : ValueOrErrors.Default.throwOne(
                `received non singleSelection renderer kind "${renderer.kind}" when resolving defaultState for singleSelection`,
              );

      if (t.kind == "multiSelection")
        return renderer.kind == "enumRenderer"
          ? ValueOrErrors.Default.return(EnumAbstractRendererState().Default())
          : renderer.kind == "streamRenderer"
            ? infiniteStreamSources(renderer.stream).Then((streamSource) =>
                ValueOrErrors.Default.return(
                  SearchableInfiniteStreamAbstractRendererState.Default(
                    streamSource,
                  ),
                ),
              )
            : ValueOrErrors.Default.throwOne(
                `received non multiSelection renderer kind "${renderer.kind}" when resolving defaultState for multiSelection`,
              );

      if (t.kind == "list")
        return renderer.kind == "listRenderer"
          ? ValueOrErrors.Default.return(
              ListAbstractRendererState.Default.zero(),
            )
          : ValueOrErrors.Default.throwOne(
              `received non list renderer kind "${renderer.kind}" when resolving defaultState for list`,
            );

      if (t.kind == "map")
        return renderer.kind == "mapRenderer"
          ? ValueOrErrors.Default.return(
              MapAbstractRendererState.Default.zero(),
            )
          : ValueOrErrors.Default.throwOne(
              `received non map renderer kind "${renderer.kind}" when resolving defaultState for map`,
            );

      if (t.kind == "tuple")
        return renderer.kind == "tupleRenderer"
          ? ValueOrErrors.Operations.All(
              List<
                ValueOrErrors<[number, CommonAbstractRendererState], string>
              >(
                t.args.map((_, index) =>
                  dispatchDefaultState(
                    infiniteStreamSources,
                    injectedPrimitives,
                    types,
                    forms,
                    converters,
                    lookupSources,
                    tableApiSources,
                    specApis,
                  )(_, renderer.itemRenderers[index].renderer).Then(
                    (itemState) =>
                      ValueOrErrors.Default.return([index, itemState]),
                  ),
                ),
              ),
            ).Then((itemStates) =>
              ValueOrErrors.Default.return(
                TupleAbstractRendererState.Default(Map(itemStates)),
              ),
            )
          : ValueOrErrors.Default.throwOne(
              `received non tuple renderer kind "${renderer.kind}" when resolving defaultState for tuple`,
            );

      if (t.kind == "sum")
        return renderer.kind == "sumRenderer"
          ? dispatchDefaultState(
              infiniteStreamSources,
              injectedPrimitives,
              types,
              forms,
              converters,
              lookupSources,
              tableApiSources,
              specApis,
            )(t.args[0], renderer.leftRenderer.renderer).Then((left) =>
              renderer.rightRenderer == undefined
                ? ValueOrErrors.Default.throwOne(
                    `rightRenderer is undefined when resolving defaultState sum view ${renderer.concreteRenderer}`,
                  )
                : dispatchDefaultState(
                    infiniteStreamSources,
                    injectedPrimitives,
                    types,
                    forms,
                    converters,
                    lookupSources,
                    tableApiSources,
                    specApis,
                  )(t.args[1], renderer.rightRenderer.renderer).Then((right) =>
                    ValueOrErrors.Default.return(
                      SumAbstractRendererState.Default({
                        left,
                        right,
                      }),
                    ),
                  ),
            )
          : renderer.kind == "sumUnitDateRenderer"
            ? ValueOrErrors.Default.return(
                SumAbstractRendererState.Default({
                  left: UnitAbstractRendererState.Default(),
                  right: DateAbstractRendererState.Default(),
                }),
              )
            : ValueOrErrors.Default.throwOne(
                `renderer kind "${renderer.kind}" not supported for sum`,
              );

      if (t.kind == "one")
        return renderer.kind != "oneRenderer"
          ? ValueOrErrors.Default.throwOne(
              `received non one renderer kind "${renderer.kind}" when resolving defaultState for one`,
            )
          : specApis.lookups != undefined &&
              specApis.lookups.get(renderer.api[0]) != undefined &&
              specApis.lookups.get(renderer.api[0])?.one != undefined &&
              specApis.lookups.get(renderer.api[0])?.one.get(renderer.api[1]) !=
                undefined &&
              specApis.lookups.get(renderer.api[0])?.one.get(renderer.api[1])
                ?.methods.getManyUnlinked
            ? lookupSources == undefined
              ? ValueOrErrors.Default.throwOne(
                  `lookup sources referenced but no lookup sources are provided`,
                )
              : lookupSources(renderer.api[0]) == undefined
                ? ValueOrErrors.Default.throwOne(
                    `cannot find lookup source for ${renderer.api[0]}`,
                  )
                : lookupSources(renderer.api[0]).Then((lookupSource) =>
                    lookupSource.one == undefined
                      ? ValueOrErrors.Default.throwOne(
                          `one source not provided for ${renderer.api[0]}`,
                        )
                      : lookupSource.one!(renderer.api[1]) // safe because we check for undefined above but type system doesn't know that
                          .Then((oneSource) =>
                            oneSource.getManyUnlinked == undefined
                              ? ValueOrErrors.Default.throwOne(
                                  `getManyUnlinked not provided for ${renderer.api[0]}-${renderer.api[1]}`,
                                )
                              : MapRepo.Operations.tryFindWithError(
                                  t.arg.name,
                                  types,
                                  () =>
                                    `cannot find lookup type ${JSON.stringify(
                                      t.arg.name,
                                    )} in ${JSON.stringify(t)}`,
                                ).Then((lookupType) =>
                                  ValueOrErrors.Default.return(
                                    OneAbstractRendererState.Default(
                                      oneSource.getManyUnlinked!(
                                        // safe because we check for undefined above but type system doesn't know that
                                        dispatchFromAPIRawValue(
                                          lookupType,
                                          types,
                                          converters,
                                          injectedPrimitives,
                                        ),
                                      ),
                                    ),
                                  ),
                                ),
                          ),
                  )
            : ValueOrErrors.Default.return(
                OneAbstractRendererState.Default(undefined),
              );

      if (t.kind == "readOnly")
        return renderer.kind != "readOnlyRenderer"
          ? ValueOrErrors.Default.throwOne(
              `received non readOnly renderer kind "${renderer.kind}" when resolving defaultState for readOnly`,
            )
          : dispatchDefaultState(
              infiniteStreamSources,
              injectedPrimitives,
              types,
              forms,
              converters,
              lookupSources,
              tableApiSources,
              specApis,
            )(t.arg, renderer.childRenderer.renderer).Then((childState) =>
              ValueOrErrors.Default.return(
                ReadOnlyAbstractRendererState.Default.childFormState(
                  childState,
                ),
              ),
            );

      if (t.kind == "record")
        return renderer.kind == "recordRenderer"
          ? ValueOrErrors.Operations.All(
              List<
                ValueOrErrors<[string, CommonAbstractRendererState], string>
              >(
                renderer.fields
                  .entrySeq()
                  .map(([fieldName, fieldRenderer]) =>
                    MapRepo.Operations.tryFindWithError(
                      fieldName,
                      t.fields,
                      () =>
                        `field ${fieldName} not found in renderer ${JSON.stringify(
                          renderer.fields,
                        )} fields`,
                    ).Then((fieldType) =>
                      dispatchDefaultState(
                        infiniteStreamSources,
                        injectedPrimitives,
                        types,
                        forms,
                        converters,
                        lookupSources,
                        tableApiSources,
                        specApis,
                      )(fieldType, fieldRenderer.renderer).Then((value) =>
                        ValueOrErrors.Default.return([fieldName, value]),
                      ),
                    ),
                  ),
              ),
            ).Then((res) =>
              ValueOrErrors.Default.return(
                RecordAbstractRendererState.Default.fieldState(Map(res)),
              ),
            )
          : ValueOrErrors.Default.throwOne(
              `received non record renderer kind "${renderer.kind}" when resolving defaultValue for record`,
            );

      if (t.kind == "union") {
        return renderer.kind == "unionRenderer"
          ? ValueOrErrors.Operations.All(
              List<ValueOrErrors<[string, any], string>>(
                renderer.cases
                  .entrySeq()
                  .map(([caseName, caseRenderer]) =>
                    MapRepo.Operations.tryFindWithError(
                      caseName,
                      t.args,
                      () =>
                        `case ${caseName} not found in type ${JSON.stringify(
                          t,
                        )}`,
                    ).Then((caseType) =>
                      dispatchDefaultState(
                        infiniteStreamSources,
                        injectedPrimitives,
                        types,
                        forms,
                        converters,
                        lookupSources,
                        tableApiSources,
                        specApis,
                      )(caseType, caseRenderer).Then((caseState) =>
                        ValueOrErrors.Default.return([caseName, caseState]),
                      ),
                    ),
                  ),
              ),
            ).Then((caseStates) =>
              ValueOrErrors.Default.return(
                UnionAbstractRendererState.Default(Map(caseStates)),
              ),
            )
          : ValueOrErrors.Default.throwOne(
              `received non union renderer kind "${renderer.kind}" when resolving defaultState for union`,
            );
      }

      if (t.kind == "table") {
        return renderer.kind == "tableRenderer"
          ? ValueOrErrors.Default.return(TableAbstractRendererState.Default())
          : ValueOrErrors.Default.throwOne(
              `received non table renderer kind "${renderer.kind}" when resolving defaultState for table`,
            );
      }

      return ValueOrErrors.Default.throwOne(
        `type of kind "${JSON.stringify(t)}" not supported by defaultState`,
      );
    })();
    return result.MapErrors((errors) =>
      errors.map(
        (error) =>
          `${error}\n...When resolving defaultState for ${t.kind} and renderer kind ${renderer.kind}`,
      ),
    );
  };

export const dispatchDefaultValue =
  <T extends DispatchInjectablesTypes<T>>(
    injectedPrimitives: DispatchInjectedPrimitives<T> | undefined,
    types: Map<DispatchTypeName, DispatchParsedType<T>>,
    forms: Map<string, Renderer<T>>,
  ) =>
  (
    t: DispatchParsedType<any>,
    renderer: Renderer<T>,
  ): ValueOrErrors<PredicateValue, string> => {
    const result: ValueOrErrors<PredicateValue, string> = (() => {
      if (renderer == undefined) {
        return ValueOrErrors.Default.return(PredicateValue.Default.unit());
      }
      if (renderer.kind == "lookupType-lookupRenderer")
        return DispatchParsedType.Operations.ResolveLookupType(
          renderer.type.name,
          types,
        ).Then((resolvedType) =>
          LookupRenderer.Operations.ResolveRenderer(renderer, forms).Then(
            (resolvedRenderer) =>
              dispatchDefaultValue(
                injectedPrimitives,
                types,
                forms,
              )(resolvedType, resolvedRenderer),
          ),
        );

      if (renderer.kind == "lookupType-inlinedRenderer")
        return DispatchParsedType.Operations.ResolveLookupType(
          renderer.type.name,
          types,
        ).Then((resolvedType) =>
          dispatchDefaultValue(
            injectedPrimitives,
            types,
            forms,
          )(resolvedType, renderer.inlinedRenderer),
        );

      if (renderer.kind == "inlinedType-lookupRenderer")
        return LookupRenderer.Operations.ResolveRenderer(renderer, forms).Then(
          (resolvedRenderer) =>
            dispatchDefaultValue(
              injectedPrimitives,
              types,
              forms,
            )(renderer.type, resolvedRenderer),
        );

      if (t.kind == "primitive")
        return t.name == "unit"
          ? ValueOrErrors.Default.return(PredicateValue.Default.unit())
          : t.name == "boolean"
            ? ValueOrErrors.Default.return(PredicateValue.Default.boolean())
            : t.name == "number"
              ? ValueOrErrors.Default.return(PredicateValue.Default.number())
              : t.name == "string"
                ? ValueOrErrors.Default.return(PredicateValue.Default.string())
                : t.name == "base64File"
                  ? ValueOrErrors.Default.return(
                      PredicateValue.Default.string(),
                    )
                  : t.name == "secret"
                    ? ValueOrErrors.Default.return(
                        PredicateValue.Default.string(),
                      )
                    : t.name == "Date"
                      ? ValueOrErrors.Default.return(
                          PredicateValue.Default.date(),
                        )
                      : injectedPrimitives?.get(t.name as keyof T) != undefined
                        ? ValueOrErrors.Default.return(
                            injectedPrimitives?.get(t.name as keyof T)!
                              .defaultValue,
                          )
                        : ValueOrErrors.Default.throwOne(
                            `could not resolve defaultValue for primitive renderer type "${
                              t.name as string
                            }"`,
                          );

      if (t.kind == "singleSelection")
        return renderer.kind == "enumRenderer" ||
          renderer.kind == "streamRenderer"
          ? ValueOrErrors.Default.return(
              PredicateValue.Default.option(
                false,
                PredicateValue.Default.unit(),
              ),
            )
          : ValueOrErrors.Default.throwOne(
              `received non singleSelection renderer kind "${renderer.kind}" when resolving defaultValue for singleSelection`,
            );

      if (t.kind == "multiSelection")
        return renderer.kind == "enumRenderer" ||
          renderer.kind == "streamRenderer"
          ? ValueOrErrors.Default.return(
              PredicateValue.Default.record(OrderedMap()),
            )
          : ValueOrErrors.Default.throwOne(
              `received non multiSelection renderer kind "${renderer.kind}" when resolving defaultValue for multiSelection`,
            );

      if (t.kind == "list")
        return renderer.kind == "listRenderer"
          ? ValueOrErrors.Default.return(PredicateValue.Default.tuple(List()))
          : ValueOrErrors.Default.throwOne(
              `received non list renderer kind "${renderer.kind}" when resolving defaultValue for list`,
            );

      if (t.kind == "map")
        return renderer.kind == "mapRenderer"
          ? ValueOrErrors.Default.return(PredicateValue.Default.tuple(List()))
          : ValueOrErrors.Default.throwOne(
              `received non map renderer kind "${renderer.kind}" when resolving defaultValue for map`,
            );

      if (t.kind == "tuple")
        return renderer.kind == "tupleRenderer"
          ? ValueOrErrors.Operations.All(
              List<ValueOrErrors<PredicateValue, string>>(
                t.args.map((_, index) =>
                  dispatchDefaultValue(
                    injectedPrimitives,
                    types,
                    forms,
                  )(_, renderer.itemRenderers[index].renderer),
                ),
              ),
            ).Then((values) =>
              ValueOrErrors.Default.return(
                PredicateValue.Default.tuple(List(values)),
              ),
            )
          : ValueOrErrors.Default.throwOne(
              `received non tuple renderer kind "${renderer.kind}" when resolving defaultValue for tuple`,
            );

      if (t.kind == "sum")
        return renderer.kind == "sumRenderer"
          ? dispatchDefaultValue(
              injectedPrimitives,
              types,
              forms,
            )(t.args[1], renderer.rightRenderer.renderer).Then((right) =>
              ValueOrErrors.Default.return(
                PredicateValue.Default.sum(Sum.Default.right(right)),
              ),
            )
          : renderer.kind == "sumUnitDateRenderer"
            ? ValueOrErrors.Default.return(
                PredicateValue.Default.sum(
                  Sum.Default.left(PredicateValue.Default.unit()),
                ),
              )
            : ValueOrErrors.Default.throwOne(
                `received non sum renderer kind "${renderer.kind}" when resolving defaultValue for sum`,
              );

      if (t.kind == "one") {
        return renderer.kind == "oneRenderer"
          ? ValueOrErrors.Default.return(
              PredicateValue.Default.option(
                false,
                PredicateValue.Default.unit(),
              ),
            )
          : ValueOrErrors.Default.throwOne(
              `received non one renderer kind "${renderer.kind}" when resolving defaultValue for one`,
            );
      }

      if (t.kind == "readOnly") {
        return renderer.kind == "readOnlyRenderer"
          ? dispatchDefaultValue(
              injectedPrimitives,
              types,
              forms,
            )(t.arg, renderer.childRenderer.renderer).Then((childValue) =>
              ValueOrErrors.Default.return(
                PredicateValue.Default.readonly(childValue),
              ),
            )
          : ValueOrErrors.Default.throwOne(
              `received non readOnly renderer kind "${renderer.kind}" when resolving defaultValue for readOnly`,
            );
      }

      if (t.kind == "record")
        return renderer.kind == "recordRenderer"
          ? ValueOrErrors.Operations.All(
              List<ValueOrErrors<[string, PredicateValue], string>>(
                renderer.fields
                  .entrySeq()
                  .map(([fieldName, fieldRenderer]) =>
                    MapRepo.Operations.tryFindWithError(
                      fieldName,
                      t.fields,
                      () =>
                        `field ${fieldName} not found in type ${JSON.stringify(
                          t,
                        )} fields`,
                    ).Then((fieldType) =>
                      dispatchDefaultValue(
                        injectedPrimitives,
                        types,
                        forms,
                      )(fieldType, fieldRenderer.renderer).Then((value) =>
                        ValueOrErrors.Default.return([fieldName, value]),
                      ),
                    ),
                  ),
              ),
            ).Then((res) =>
              ValueOrErrors.Default.return(
                PredicateValue.Default.record(OrderedMap(res)),
              ),
            )
          : ValueOrErrors.Default.throwOne(
              `received non record renderer kind "${renderer.kind}" when resolving defaultValue for record`,
            );

      if (t.kind == "union") {
        return renderer.kind != "unionRenderer"
          ? ValueOrErrors.Default.throwOne(
              `received non union renderer kind "${renderer.kind}" when resolving defaultValue for union`,
            )
          : MapRepo.Operations.tryFirstWithError(
              t.args,
              () => `union type ${JSON.stringify(t, null, 2)} has no cases`,
            ).Then((firstCaseType) =>
              MapRepo.Operations.tryFirstWithError(
                renderer.cases,
                () => `union renderer has no cases`,
              ).Then((firstCaseRenderer) =>
                dispatchDefaultValue(
                  injectedPrimitives,
                  types,
                  forms,
                )(firstCaseType, firstCaseRenderer),
              ),
            );
      }

      if (t.kind == "table") {
        return renderer.kind == "tableRenderer"
          ? ValueOrErrors.Default.return(
              PredicateValue.Default.table(0, 0, Map(), false),
            )
          : ValueOrErrors.Default.throwOne(
              `received non table renderer kind "${renderer.kind}" when resolving defaultValue for table`,
            );
      }

      return ValueOrErrors.Default.throwOne(
        `type ${t} not supported by defaultValue`,
      );
    })();
    return result.MapErrors((errors) =>
      errors.map(
        (error) =>
          `${error}\n...When resolving defaultValue for type "${JSON.stringify(t, null, 2)}" and renderer kind "${renderer.kind}"`,
      ),
    );
  };

export const dispatchFromAPIRawValue =
  <T extends DispatchInjectablesTypes<T>>(
    t: DispatchParsedType<T>,
    types: Map<DispatchTypeName, DispatchParsedType<T>>,
    converters: DispatchApiConverters<T>,
    injectedPrimitives?: DispatchInjectedPrimitives<T>,
  ) =>
  (raw: any): ValueOrErrors<PredicateValue, string> => {
    const result: ValueOrErrors<PredicateValue, string> = (() => {
      if (t.kind == "primitive") {
        // unit is a special kind of primitive
        if (t.name == "unit") {
          return ValueOrErrors.Default.return(PredicateValue.Default.unit());
        }

        if (
          !PredicateValue.Operations.IsPrimitive(raw) &&
          !injectedPrimitives?.keySeq().contains(t.name as keyof T)
        ) {
          return ValueOrErrors.Default.throwOne(
            `primitive expected but got ${JSON.stringify(raw)}`,
          );
        }
        return ValueOrErrors.Default.return(
          converters[t.name].fromAPIRawValue(raw),
        );
      }
      if (t.kind == "union") {
        const result = converters["union"].fromAPIRawValue(raw);
        const caseType = t.args.get(result.caseName);
        if (caseType == undefined)
          return ValueOrErrors.Default.throwOne(
            `union case ${result.caseName} not found in type ${JSON.stringify(
              t,
            )}`,
          );

        return dispatchFromAPIRawValue(
          caseType,
          types,
          converters,
          injectedPrimitives,
        )(result.fields).Then((value) =>
          ValueOrErrors.Default.return(
            PredicateValue.Default.unionCase(result.caseName, value),
          ),
        );
      }

      if (t.kind == "singleSelection") {
        const result = converters["SingleSelection"].fromAPIRawValue(raw);
        const isSome = result.kind == "l";
        const value = isSome
          ? PredicateValue.Default.record(OrderedMap(result.value))
          : PredicateValue.Default.unit();

        return ValueOrErrors.Default.return(
          PredicateValue.Default.option(isSome, value),
        );
      }
      if (t.kind == "multiSelection") {
        const result = converters["MultiSelection"].fromAPIRawValue(raw);
        const values = result.map((_) =>
          PredicateValue.Default.record(OrderedMap(_)),
        );
        return ValueOrErrors.Default.return(
          PredicateValue.Default.record(OrderedMap(values)),
        );
      }
      if (t.kind == "list") {
        const result = converters["List"].fromAPIRawValue(raw);
        return ValueOrErrors.Operations.All(
          result.map((_) =>
            dispatchFromAPIRawValue(
              t.args[0],
              types,
              converters,
              injectedPrimitives,
            )(_),
          ),
        ).Then((values) =>
          ValueOrErrors.Default.return(PredicateValue.Default.tuple(values)),
        );
      }
      if (t.kind == "map" && t.args.length == 2) {
        const result = converters["Map"].fromAPIRawValue(raw);

        return ValueOrErrors.Operations.All(
          List<ValueOrErrors<PredicateValue, string>>(
            result.map((_) =>
              dispatchFromAPIRawValue(
                t.args[0],
                types,
                converters,
                injectedPrimitives,
              )(_[0]).Then((key) =>
                dispatchFromAPIRawValue(
                  t.args[1],
                  types,
                  converters,
                  injectedPrimitives,
                )(_[1]).Then((value) =>
                  ValueOrErrors.Default.return(
                    PredicateValue.Default.tuple(List([key, value])),
                  ),
                ),
              ),
            ),
          ),
        ).Then((values) =>
          ValueOrErrors.Default.return(
            PredicateValue.Default.tuple(List(values)),
          ),
        );
      }

      if (t.kind == "tuple") {
        const result = converters["Tuple"].fromAPIRawValue(raw);
        return ValueOrErrors.Operations.All(
          List<ValueOrErrors<PredicateValue, string>>(
            result.map((_, index) =>
              dispatchFromAPIRawValue(
                t.args[index],
                types,
                converters,
                injectedPrimitives,
              )(_),
            ),
          ),
        ).Then((values) =>
          ValueOrErrors.Default.return(
            PredicateValue.Default.tuple(List(values)),
          ),
        );
      }

      if (t.kind == "sum" && t.args.length === 2) {
        const result = converters["Sum"].fromAPIRawValue(raw);

        return dispatchFromAPIRawValue(
          result.kind == "l" ? t.args[0] : t.args[1],
          types,
          converters,
          injectedPrimitives,
        )(result.value).Then((value) =>
          ValueOrErrors.Default.return(
            PredicateValue.Default.sum(
              Sum.Updaters.map2(replaceWith(value), replaceWith(value))(result),
            ),
          ),
        );
      }

      if (t.kind == "lookup")
        return MapRepo.Operations.tryFindWithError(
          t.name, // TODO -- double check this is correct instead of typeName, and maybe remove typeName
          types,
          () => `type ${t.name} not found in types`,
        ).Then((type) =>
          dispatchFromAPIRawValue(
            type,
            types,
            converters,
            injectedPrimitives,
          )(raw),
        );

      // TODO -- this can be more functional
      if (t.kind == "table") {
        // move to the converter
        if (typeof raw != "object") {
          return ValueOrErrors.Default.throwOne(
            `object expected but got ${JSON.stringify(raw)}`,
          );
        }
        const converterResult = converters["Table"].fromAPIRawValue(raw);
        const argType = t.arg;

        return DispatchParsedType.Operations.ResolveLookupType(
          argType.name,
          types,
        ).Then((resolvedType) =>
          ValueOrErrors.Operations.All(
            List<ValueOrErrors<[string, ValueRecord], string>>(
              converterResult.data
                .toArray()
                .map(([key, record]) =>
                  dispatchFromAPIRawValue(
                    resolvedType,
                    types,
                    converters,
                    injectedPrimitives,
                  )(record).Then((value) =>
                    PredicateValue.Operations.IsRecord(value)
                      ? ValueOrErrors.Default.return([key, value])
                      : ValueOrErrors.Default.throwOne(
                          `record expected but got ${PredicateValue.Operations.GetKind(
                            value,
                          )}`,
                        ),
                  ),
                ),
            ),
          ).Then((values) =>
            ValueOrErrors.Default.return(
              ValueTable.Default.fromParsed(
                converterResult.from,
                converterResult.to,
                converterResult.hasMoreValues,
                OrderedMap(values),
              ),
            ),
          ),
        );
      }

      if (t.kind == "one") {
        const result = converters["One"].fromAPIRawValue(raw);
        if (!result.isSome) {
          return ValueOrErrors.Default.return(result);
        }
        return dispatchFromAPIRawValue(
          t.arg,
          types,
          converters,
          injectedPrimitives,
        )(result.value).Then((value) =>
          ValueOrErrors.Default.return(
            PredicateValue.Default.option(true, value),
          ),
        );
      }

      if (t.kind == "readOnly") {
        const readOnlyResult = converters["ReadOnly"].fromAPIRawValue(raw);
        return dispatchFromAPIRawValue(
          t.arg,
          types,
          converters,
          injectedPrimitives,
        )(readOnlyResult.ReadOnly).Then((value) =>
          ValueOrErrors.Default.return(PredicateValue.Default.readonly(value)),
        );
      }

      // TODO -- this can be more functional
      if (t.kind == "record") {
        if (typeof raw != "object") {
          return ValueOrErrors.Default.throwOne(
            `object expected but got ${JSON.stringify(raw)}`,
          );
        }
        let result: OrderedMap<string, PredicateValue> = OrderedMap();
        let errors: List<string> = List();
        t.fields.forEach((fieldType, fieldName) => {
          const fieldValue = raw[fieldName];
          if (fieldValue !== null && fieldValue === undefined) {
            return;
          }
          const parsedValue = dispatchFromAPIRawValue(
            fieldType,
            types,
            converters,
            injectedPrimitives,
          )(fieldValue);
          if (parsedValue.kind == "errors") {
            errors = errors.concat(parsedValue.errors);
          } else {
            result = result.set(fieldName, parsedValue.value);
          }
        });
        if (errors.size > 0) {
          return ValueOrErrors.Default.throw(errors);
        }
        return ValueOrErrors.Default.return(
          PredicateValue.Default.record(result),
        );
      }

      return ValueOrErrors.Default.throwOne(
        `unsupported type ${JSON.stringify(t)} for raw: `,
      );
    })();
    return result.MapErrors((errors) =>
      errors.map(
        (error) =>
          `${error}\n...When converting type ${JSON.stringify(
            t,
            null,
            2,
          )} and value ${JSON.stringify(raw, null, 2)} from API raw value`,
      ),
    );
  };

export const dispatchToAPIRawValue =
  <T extends DispatchInjectablesTypes<T>>(
    t: DispatchParsedType<T>,
    types: Map<DispatchTypeName, DispatchParsedType<T>>,
    converters: DispatchApiConverters<T>,
    injectedPrimitives?: DispatchInjectedPrimitives<T>,
  ) =>
  (raw: PredicateValue, formState: any): ValueOrErrors<any, string> => {
    const result: ValueOrErrors<any, string> = (() => {
      if (t.kind == "primitive") {
        if (t.name == "unit") {
          return ValueOrErrors.Default.return(unit);
        }
        return ValueOrErrors.Operations.Return(
          converters[t.name as string | keyof T].toAPIRawValue([
            raw,
            formState?.commonFormState?.modifiedByUser ?? false,
          ]),
        );
      }

      if (t.kind == "union") {
        if (!PredicateValue.Operations.IsUnionCase(raw)) {
          return ValueOrErrors.Default.throwOne(
            `Union case expected but got ${JSON.stringify(raw)}\n...when converting union to API raw value`,
          );
        }
        const caseName = raw.caseName;
        if (
          caseName == undefined ||
          !PredicateValue.Operations.IsString(caseName)
        ) {
          return ValueOrErrors.Default.throwOne(
            `caseName expected but got ${JSON.stringify(raw)}`,
          );
        }
        const caseType = t.args.get(caseName);
        if (caseType == undefined) {
          return ValueOrErrors.Default.throwOne(
            `union case ${caseName} not found in type ${JSON.stringify(t)}`,
          );
        }

        return dispatchToAPIRawValue(
          caseType,
          types,
          converters,
          injectedPrimitives,
        )(raw.fields, formState);
      }

      if (t.kind == "singleSelection") {
        if (!PredicateValue.Operations.IsOption(raw)) {
          return ValueOrErrors.Default.throwOne(
            `Option expected but got ${JSON.stringify(raw)}`,
          );
        }

        if (raw.isSome) {
          if (!PredicateValue.Operations.IsRecord(raw.value)) {
            return ValueOrErrors.Default.throwOne(
              `Record expected but got ${JSON.stringify(raw.value)}`,
            );
          }
          const rawValue = raw.value.fields.toJS();
          if (
            !CollectionReference.Operations.IsCollectionReference(rawValue) &&
            !EnumReference.Operations.IsEnumReference(rawValue)
          ) {
            return ValueOrErrors.Default.throwOne(
              `CollectionReference or EnumReference expected but got ${rawValue}`,
            );
          }

          return ValueOrErrors.Operations.Return(
            converters["SingleSelection"].toAPIRawValue([
              Sum.Default.left(rawValue),
              formState?.commonFormState?.modifiedByUser ?? false,
            ]),
          );
        } else {
          return ValueOrErrors.Operations.Return(
            converters["SingleSelection"].toAPIRawValue([
              Sum.Default.right("no selection"),
              formState?.commonFormState?.modifiedByUser ?? false,
            ]),
          );
        }
      }

      if (t.kind == "multiSelection") {
        if (!PredicateValue.Operations.IsRecord(raw)) {
          return ValueOrErrors.Default.throwOne(
            `Record expected but got multi selection of ${JSON.stringify(raw)}`,
          );
        }

        const rawValue: Map<
          string,
          ValueOrErrors<CollectionReference | EnumReference, string>
        > = raw.fields.map((value) => {
          if (!PredicateValue.Operations.IsRecord(value)) {
            return ValueOrErrors.Default.throwOne(
              `Record expected but got ${JSON.stringify(value)}`,
            );
          }
          const fieldsObject = value.fields.toJS();

          if (
            !CollectionReference.Operations.IsCollectionReference(
              fieldsObject,
            ) &&
            !EnumReference.Operations.IsEnumReference(fieldsObject)
          ) {
            return ValueOrErrors.Default.throwOne(
              `CollectionReference or EnumReference expected but got ${JSON.stringify(
                fieldsObject,
              )}`,
            );
          }
          return ValueOrErrors.Default.return(fieldsObject);
        });

        return ValueOrErrors.Operations.All(rawValue.valueSeq().toList()).Then(
          (values) =>
            ValueOrErrors.Default.return(
              converters["MultiSelection"].toAPIRawValue([
                OrderedMap<string, EnumReference | CollectionReference>(
                  values
                    .map((v): [string, EnumReference | CollectionReference] => {
                      if (
                        CollectionReference.Operations.IsCollectionReference(v)
                      ) {
                        return [v.Id, v];
                      }
                      return [v.Value, v];
                    })
                    .toArray(),
                ),
                formState?.commonFormState?.modifiedByUser ?? false,
              ]),
            ),
        );
      }
      if (t.kind == "list") {
        if (!PredicateValue.Operations.IsTuple(raw)) {
          return ValueOrErrors.Default.throwOne(
            `Tuple expected but got list of ${JSON.stringify(raw)}`,
          );
        }
        return ValueOrErrors.Operations.All(
          List(
            raw.values.map((value, index) =>
              dispatchToAPIRawValue(
                t.args[0],
                types,
                converters,
                injectedPrimitives,
              )(value, formState?.elementFormStates?.get(index)),
            ),
          ),
        ).Then((values) =>
          ValueOrErrors.Default.return(
            converters["List"].toAPIRawValue([
              values,
              formState?.commonFormState?.modifiedByUser ?? false,
            ]),
          ),
        );
      }
      if (t.kind == "map" && t.args.length == 2) {
        const keyValues = (raw as ValueTuple).values.map((keyValue, index) => {
          return dispatchToAPIRawValue(
            t.args[0],
            types,
            converters,
            injectedPrimitives,
          )(
            (keyValue as ValueTuple).values.get(0)!,
            formState?.elementFormStates?.get(index)?.KeyFormState,
          )
            .Then((possiblyUndefinedKey) => {
              if (
                possiblyUndefinedKey == undefined ||
                possiblyUndefinedKey == null ||
                possiblyUndefinedKey == "" ||
                (typeof possiblyUndefinedKey == "object" &&
                  (Object.keys(possiblyUndefinedKey).length == 0 ||
                    ("IsSome" in possiblyUndefinedKey &&
                      !possiblyUndefinedKey.IsSome)))
              ) {
                return ValueOrErrors.Default.throwOne(
                  `A mapped key is undefined for type ${JSON.stringify(
                    t.args[0],
                  )}`,
                );
              } else {
                return ValueOrErrors.Default.return(possiblyUndefinedKey);
              }
            })
            .Then((key) =>
              dispatchToAPIRawValue(
                t.args[1],
                types,
                converters,
                injectedPrimitives,
              )(
                (keyValue as ValueTuple).values.get(1)!,
                formState?.elementFormStates?.get(index)?.ValueFormState,
              ).Then((value) =>
                ValueOrErrors.Default.return([key, value] as [any, any]),
              ),
            );
        });

        return ValueOrErrors.Operations.All(List(keyValues)).Then((values) => {
          if (
            values.map((kv) => JSON.stringify(kv[0])).toSet().size !=
            values.size
          ) {
            return ValueOrErrors.Default.throwOne(
              "Keys in the map are not unique",
            );
          }
          return ValueOrErrors.Operations.Return(
            converters["Map"].toAPIRawValue([
              values,
              formState?.commonFormState?.modifiedByUser ?? false,
            ]),
          );
        });
      }

      if (t.kind == "sum" && t.args.length === 2) {
        if (!PredicateValue.Operations.IsSum(raw)) {
          return ValueOrErrors.Default.throwOne(
            `Sum expected but got ${JSON.stringify(raw)}`,
          );
        }

        return dispatchToAPIRawValue(
          raw.value.kind == "l" ? t.args[0] : t.args[1],
          types,
          converters,
          injectedPrimitives,
        )(
          raw.value.value,
          raw.value.kind == "l"
            ? formState?.commonFormState?.left
            : formState?.commonFormState?.right,
        ).Then((value) =>
          ValueOrErrors.Default.return(
            converters["Sum"].toAPIRawValue([
              raw.value.kind == "l"
                ? Sum.Default.left(value)
                : Sum.Default.right(value),
              formState?.commonFormState?.modifiedByUser ?? false,
            ]),
          ),
        );
      }

      if (t.kind == "tuple") {
        if (!PredicateValue.Operations.IsTuple(raw)) {
          return ValueOrErrors.Default.throwOne(
            `Tuple expected but got ${JSON.stringify(raw)}`,
          );
        }
        return ValueOrErrors.Operations.All(
          List(
            raw.values.map((value, index) => {
              return dispatchToAPIRawValue(
                t.args[index],
                types,
                converters,
                injectedPrimitives,
              )(value, formState?.itemFormStates?.get(index));
            }),
          ),
        ).Then((values) =>
          ValueOrErrors.Default.return(
            converters["Tuple"].toAPIRawValue([
              values,
              formState?.commonFormState?.modifiedByUser ?? false,
            ]),
          ),
        );
      }

      if (t.kind == "lookup")
        return dispatchToAPIRawValue(
          types.get(t.name)!,
          types,
          converters,
          injectedPrimitives,
        )(raw, formState);

      if (t.kind == "record") {
        if (!PredicateValue.Operations.IsRecord(raw)) {
          return ValueOrErrors.Default.throwOne(
            `Record expected but got ${JSON.stringify(raw)}`,
          );
        }
        const res = [] as any;
        t.fields.forEach((fieldType, fieldName) => {
          const rawField = raw.fields.get(fieldName);
          if (rawField == undefined) {
            return;
          }
          res.push([
            fieldName,
            dispatchToAPIRawValue(
              fieldType,
              types,
              converters,
              injectedPrimitives,
            )(
              raw.fields.get(fieldName)!,
              formState?.fieldStates?.get(fieldName),
            ),
          ]);
        });
        const errors: ValueOrErrors<
          List<any>,
          string
        > = ValueOrErrors.Operations.All(
          List(
            res.map(
              ([_, value]: [_: string, value: ValueOrErrors<any, string>]) =>
                value,
            ),
          ),
        );
        if (errors.kind == "errors") return errors;

        return ValueOrErrors.Operations.Return(
          res.reduce(
            (acc: any, [fieldName, value]: [fieldName: string, value: any]) => {
              acc[fieldName] = value.value;
              return acc;
            },
            {} as any,
          ),
        );
      }

      if (t.kind == "one") {
        if (!PredicateValue.Operations.IsOption(raw)) {
          return ValueOrErrors.Default.throwOne(
            `Option expected but got ${JSON.stringify(raw)}\n...when converting one to API raw value`,
          );
        }

        if (!raw.isSome) {
          return ValueOrErrors.Default.return(
            converters["One"].toAPIRawValue([
              raw,
              formState?.commonFormState?.modifiedByUser ?? false,
            ]),
          );
        }

        return dispatchToAPIRawValue(
          t.arg,
          types,
          converters,
          injectedPrimitives,
        )(raw.value, formState?.commonFormState?.modifiedByUser ?? false).Then(
          (value) => {
            return ValueOrErrors.Default.return(
              converters["One"].toAPIRawValue([
                PredicateValue.Default.option(true, value),
                formState?.commonFormState?.modifiedByUser ?? false,
              ]),
            );
          },
        );
      }

      if (t.kind == "readOnly") {
        if (!PredicateValue.Operations.IsReadOnly(raw)) {
          return ValueOrErrors.Default.throwOne(
            `ReadOnly expected but got ${JSON.stringify(raw)}`,
          );
        }
        return dispatchToAPIRawValue(
          t.arg,
          types,
          converters,
          injectedPrimitives,
        )(raw.ReadOnly, formState).Then((childValue) =>
          ValueOrErrors.Default.return(
            converters["ReadOnly"].toAPIRawValue([
              { ReadOnly: childValue },
              formState?.commonFormState?.modifiedByUser ?? false,
            ]),
          ),
        );
      }

      if (t.kind == "table") {
        if (!PredicateValue.Operations.IsTable(raw)) {
          return ValueOrErrors.Default.throwOne(
            `Table expected but got ${JSON.stringify(raw)}`,
          );
        }

        return ValueOrErrors.Default.return(
          converters["Table"].toAPIRawValue([
            raw,
            formState?.commonFormState?.modifiedByUser ?? false,
          ]),
        );
      }

      return ValueOrErrors.Default.throwOne(
        `Unsupported type ${JSON.stringify(t)}`,
      );
    })();
    return result.MapErrors((errors) =>
      errors.map(
        (error) =>
          `${error}\n...When converting type ${JSON.stringify(
            t,
            null,
            2,
          )} and value ${JSON.stringify(raw, null, 2)} to API raw value`,
      ),
    );
  };

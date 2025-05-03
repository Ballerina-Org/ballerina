import { Map, List, Set, OrderedMap } from "immutable";
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
  InjectedPrimitives,
  RecordAbstractRendererState,
  AbstractTableRendererState,
  MapRepo,
  ValueTable,
  DispatchCommonFormState,
} from "../../../../../main";
import {
  DispatchParsedType,
  DispatchTypeName,
} from "../deserializer/domains/specification/domains/types/state";
import { UnitAbstractRendererState } from "../dispatcher/domains/abstract-renderers/unit/state";
import { StringAbstractRendererState } from "../dispatcher/domains/abstract-renderers/string/state";
import { NumberAbstractRendererState } from "../dispatcher/domains/abstract-renderers/number/state";
import { BoolAbstractRendererState } from "../dispatcher/domains/abstract-renderers/boolean/state";
import { DateAbstractRendererState } from "../dispatcher/domains/abstract-renderers/date/state";
import { Base64FileAbstractRendererState } from "../dispatcher/domains/abstract-renderers/base-64-file/state";
import { SecretAbstractRendererState } from "../dispatcher/domains/abstract-renderers/secret/state";
import { MapAbstractRendererState } from "../dispatcher/domains/abstract-renderers/map/state";
import { TupleAbstractRendererState } from "../dispatcher/domains/abstract-renderers/tuple/state";
import { SumAbstractRendererState } from "../dispatcher/domains/abstract-renderers/sum/state";
import { EnumAbstractRendererState } from "../dispatcher/domains/abstract-renderers/enum/state";
import { ListAbstractRendererState } from "../dispatcher/domains/abstract-renderers/list/state";
import { SearchableInfiniteStreamAbstractRendererState } from "../dispatcher/domains/abstract-renderers/searchable-infinite-stream/state";
import { Form } from "../deserializer/domains/specification/domains/form/state";
import { BaseRenderer } from "../deserializer/domains/specification/domains/form/domains/renderers/domains/baseRenderer/state";

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

type ApiConverter<T> = {
  fromAPIRawValue: BasicFun<any, T>;
  toAPIRawValue: BasicFun<[T, boolean], any>;
};
export type DispatchApiConverters<
  T extends { [key in keyof T]: { type: any; state: any } },
> = { [key in keyof T]: ApiConverter<T[key]["type"]> } & BuiltInApiConverters;

type UnionCase = {
  caseName: string;
  fields: Record<string, any>;
};

type Table = {
  data: Map<string, Record<string, any>>;
  hasMoreValues: boolean;
  from: number;
  to: number;
};

type BuiltInApiConverters = {
  string: ApiConverter<string>;
  number: ApiConverter<number>;
  boolean: ApiConverter<boolean>;
  base64File: ApiConverter<string>;
  secret: ApiConverter<string>;
  Date: ApiConverter<Date>;
  union: ApiConverter<UnionCase>;
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
};

export type ConcreteRendererKinds = {
  unit: Set<string>;
  boolean: Set<string>;
  number: Set<string>;
  string: Set<string>;
  base64File: Set<string>;
  secret: Set<string>;
  date: Set<string>;
  enumSingleSelection: Set<string>;
  enumMultiSelection: Set<string>;
  streamSingleSelection: Set<string>;
  streamMultiSelection: Set<string>;
  list: Set<string>;
  map: Set<string>;
  tuple: Set<string>;
  sum: Set<string>;
  sumUnitDate: Set<string>;
  record: Set<string>;
  table: Set<string>;
};

export const concreteRendererToKind =
  (concreteRenderers: Record<string, any>) =>
  (name: string): ValueOrErrors<string, string> => {
    const viewTypes = Object.keys(concreteRenderers);
    for (const viewType of viewTypes) {
      if (name in concreteRenderers[viewType]) {
        return ValueOrErrors.Default.return(viewType);
      }
    }
    return ValueOrErrors.Default.throwOne(
      `cannot find view ${name} in formViews`,
    );
  };

// TODO -- JSX instead of any
export const tryGetConcreteRenderer =
  (
    concreteRenderers: Record<string, any>,
    defaultRecordRenderer: any,
    defaultNestedRecordRenderer: any,
  ) =>
  (
    kind: keyof ConcreteRendererKinds,
    name?: string,
    isNested?: boolean, // valid only for record kind
  ): ValueOrErrors<JSX.Element, string> => {
    if (kind == "record" && name == undefined) {
      if (isNested) {
        return ValueOrErrors.Default.return(defaultNestedRecordRenderer);
      }
      return ValueOrErrors.Default.return(defaultRecordRenderer);
    }
    if (name == undefined) {
      return ValueOrErrors.Default.throwOne(
        `concrete renderer name is undefined for kind "${kind}"`,
      );
    }
    if (!concreteRenderers[kind]) {
      return ValueOrErrors.Default.throwOne(
        `cannot find concrete renderer kind "${kind}" in formViews`,
      );
    }
    if (concreteRenderers[kind][name]) {
      return ValueOrErrors.Default.return(concreteRenderers[kind][name]());
    }
    return ValueOrErrors.Default.throwOne(
      `cannot find concrete renderer "${name}" in kind "${kind}"`,
    );
  };

export const dispatchDefaultState =
  <T>(
    infiniteStreamSources: DispatchInfiniteStreamSources,
    injectedPrimitives: InjectedPrimitives<T> | undefined,
    types: Map<DispatchTypeName, DispatchParsedType<T>>,
    forms: Map<string, Form<T>>,
  ) =>
  (
    t: DispatchParsedType<any>,
    renderer: BaseRenderer<any> | Form<any>,
  ): ValueOrErrors<any, string> => {
    const result: ValueOrErrors<any, string> = (() => {
      if (renderer == undefined) {
        return ValueOrErrors.Default.return(undefined);
      }
      if (t.kind == "primitive")
        return renderer.kind != "basePrimitiveRenderer"
          ? ValueOrErrors.Default.throwOne(
              `received non primitive renderer kind "${renderer.kind}" when resolving defaultState for primitive`,
            )
          : t.name == "unit"
            ? ValueOrErrors.Default.return(UnitAbstractRendererState.Default())
            : t.name == "boolean"
              ? ValueOrErrors.Default.return(
                  BoolAbstractRendererState.Default(),
                )
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
                        : injectedPrimitives?.injectedPrimitives.get(
                              t.name as keyof T,
                            ) != undefined
                          ? ValueOrErrors.Default.return({
                              commonFormState:
                                DispatchCommonFormState.Default(),
                              ...injectedPrimitives.injectedPrimitives.get(
                                t.name as keyof T,
                              )!.defaultState,
                            })
                          : ValueOrErrors.Default.throwOne(
                              `could not resolve defaultState for primitive renderer kind "${
                                t.name as string
                              }"`,
                            );

      if (t.kind == "singleSelection")
        return renderer.kind == "baseEnumRenderer"
          ? ValueOrErrors.Default.return(EnumAbstractRendererState().Default())
          : renderer.kind == "baseStreamRenderer"
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
        return renderer.kind == "baseEnumRenderer"
          ? ValueOrErrors.Default.return(EnumAbstractRendererState().Default())
          : renderer.kind == "baseStreamRenderer"
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
        return renderer.kind == "baseListRenderer"
          ? ValueOrErrors.Default.return(
              ListAbstractRendererState.Default.zero(),
            )
          : ValueOrErrors.Default.throwOne(
              `received non list renderer kind "${renderer.kind}" when resolving defaultState for list`,
            );

      if (t.kind == "map")
        return renderer.kind == "baseMapRenderer"
          ? ValueOrErrors.Default.return(
              MapAbstractRendererState().Default.zero(),
            )
          : ValueOrErrors.Default.throwOne(
              `received non map renderer kind "${renderer.kind}" when resolving defaultState for map`,
            );

      if (t.kind == "tuple")
        return renderer.kind == "baseTupleRenderer"
          ? ValueOrErrors.Operations.All(
              List<
                ValueOrErrors<
                  [number, { commonFormState: DispatchCommonFormState }],
                  string
                >
              >(
                t.args.map((_, index) =>
                  dispatchDefaultState(
                    infiniteStreamSources,
                    injectedPrimitives,
                    types,
                    forms,
                  )(_, renderer.itemRenderers[index]).Then((itemState) =>
                    ValueOrErrors.Default.return([index, itemState]),
                  ),
                ),
              ),
            ).Then((itemStates) =>
              ValueOrErrors.Default.return(
                TupleAbstractRendererState<{
                  commonFormState: DispatchCommonFormState;
                }>().Default(Map(itemStates)),
              ),
            )
          : ValueOrErrors.Default.throwOne(
              `received non tuple renderer kind "${renderer.kind}" when resolving defaultState for tuple`,
            );

      if (t.kind == "sum")
        return renderer.kind == "baseSumRenderer"
          ? dispatchDefaultState(
              infiniteStreamSources,
              injectedPrimitives,
              types,
              forms,
            )(t.args[0], renderer.leftRenderer).Then((left) =>
              renderer.rightRenderer == undefined
                ? ValueOrErrors.Default.throwOne(
                    `rightRenderer is undefined when resolving defaultState sum view ${renderer.concreteRendererName}`,
                  )
                : dispatchDefaultState(
                    infiniteStreamSources,
                    injectedPrimitives,
                    types,
                    forms,
                  )(t.args[1], renderer.rightRenderer).Then((right) =>
                    ValueOrErrors.Default.return(
                      SumAbstractRendererState().Default({
                        left,
                        right,
                      }),
                    ),
                  ),
            )
          : renderer.kind == "baseSumUnitDateRenderer"
            ? ValueOrErrors.Default.return(
                SumAbstractRendererState().Default({
                  left: UnitAbstractRendererState.Default(),
                  right: DateAbstractRendererState.Default(),
                }),
              )
            : ValueOrErrors.Default.throwOne(
                `renderer kind "${renderer.kind}" not supported for sum`,
              );

      if (t.kind == "record")
        return renderer.kind == "recordForm"
          ? ValueOrErrors.Operations.All(
              List<ValueOrErrors<[string, PredicateValue], string>>(
                t.fields
                  .entrySeq()
                  .map(([fieldName, field]) =>
                    dispatchDefaultState(
                      infiniteStreamSources,
                      injectedPrimitives,
                      types,
                      forms,
                    )(field, renderer.fields.get(fieldName)!).Then((value) =>
                      ValueOrErrors.Default.return([fieldName, value] as const),
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

      if (t.kind == "table") {
        return renderer.kind == "tableForm" ||
          renderer.kind == "baseTableRenderer"
          ? ValueOrErrors.Default.return(AbstractTableRendererState.Default())
          : ValueOrErrors.Default.throwOne(
              `received non table renderer kind "${renderer.kind}" when resolving defaultState for table`,
            );
      }

      if (t.kind == "lookup") {
        if (renderer.kind != "baseLookupRenderer") {
          return ValueOrErrors.Default.throwOne(
            `received non lookup renderer kind "${renderer.kind}" when resolving defaultState for lookup`,
          );
        }
        const lookupType = types.get(t.name);

        if (lookupType == undefined) {
          return ValueOrErrors.Default.throwOne(
            `lookup type ${t.name} not found in types`,
          );
        }
        const formRenderer = forms.get(renderer.lookupRendererName);
        if (formRenderer == undefined) {
          return ValueOrErrors.Default.throwOne(
            `lookup form renderer ${renderer.lookupRendererName} not found in forms`,
          );
        }

        return dispatchDefaultState(
          infiniteStreamSources,
          injectedPrimitives,
          types,
          forms,
        )(lookupType, formRenderer);
      }

      return ValueOrErrors.Default.throwOne(
        `type of kind "${t.kind}" not supported by defaultState`,
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
  <T>(
    injectedPrimitives: InjectedPrimitives<T> | undefined,
    types: Map<DispatchTypeName, DispatchParsedType<T>>,
    forms: Map<string, Form<T>>,
  ) =>
  (
    t: DispatchParsedType<any>,
    renderer: BaseRenderer<any> | Form<any>,
  ): ValueOrErrors<PredicateValue, string> => {
    const result: ValueOrErrors<PredicateValue, string> = (() => {
      if (renderer == undefined) {
        return ValueOrErrors.Default.return(PredicateValue.Default.unit());
      }
      if (t.kind == "primitive")
        return renderer.kind != "basePrimitiveRenderer"
          ? ValueOrErrors.Default.throwOne(
              `received non primitive renderer kind "${renderer.kind}" when resolving defaultValue for primitive`,
            )
          : t.name == "unit"
            ? ValueOrErrors.Default.return(PredicateValue.Default.unit())
            : t.name == "boolean"
              ? ValueOrErrors.Default.return(PredicateValue.Default.boolean())
              : t.name == "number"
                ? ValueOrErrors.Default.return(PredicateValue.Default.number())
                : t.name == "string"
                  ? ValueOrErrors.Default.return(
                      PredicateValue.Default.string(),
                    )
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
                        : injectedPrimitives?.injectedPrimitives.get(
                              t.name as keyof T,
                            ) != undefined
                          ? ValueOrErrors.Default.return(
                              injectedPrimitives.injectedPrimitives.get(
                                t.name as keyof T,
                              )!.defaultValue,
                            )
                          : ValueOrErrors.Default.throwOne(
                              `could not resolve defaultValue for primitive renderer type "${
                                t.name as string
                              }"`,
                            );

      if (t.kind == "singleSelection")
        return renderer.kind == "baseEnumRenderer" ||
          renderer.kind == "baseStreamRenderer"
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
        return renderer.kind == "baseEnumRenderer" ||
          renderer.kind == "baseStreamRenderer"
          ? ValueOrErrors.Default.return(PredicateValue.Default.record(Map()))
          : ValueOrErrors.Default.throwOne(
              `received non multiSelection renderer kind "${renderer.kind}" when resolving defaultValue for multiSelection`,
            );

      if (t.kind == "list")
        return renderer.kind == "baseListRenderer"
          ? ValueOrErrors.Default.return(PredicateValue.Default.tuple(List()))
          : ValueOrErrors.Default.throwOne(
              `received non list renderer kind "${renderer.kind}" when resolving defaultValue for list`,
            );

      if (t.kind == "map")
        return renderer.kind == "baseMapRenderer"
          ? ValueOrErrors.Default.return(PredicateValue.Default.tuple(List()))
          : ValueOrErrors.Default.throwOne(
              `received non map renderer kind "${renderer.kind}" when resolving defaultValue for map`,
            );

      if (t.kind == "tuple")
        return renderer.kind == "baseTupleRenderer"
          ? ValueOrErrors.Operations.All(
              List<ValueOrErrors<PredicateValue, string>>(
                t.args.map((_, index) =>
                  dispatchDefaultValue(
                    injectedPrimitives,
                    types,
                    forms,
                  )(_, renderer.itemRenderers[index]),
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
        return renderer.kind == "baseSumRenderer"
          ? dispatchDefaultValue(
              injectedPrimitives,
              types,
              forms,
            )(t.args[0], renderer.leftRenderer).Then((left) =>
              ValueOrErrors.Default.return(
                PredicateValue.Default.sum(Sum.Default.left(left)),
              ),
            )
          : renderer.kind == "baseSumUnitDateRenderer"
            ? ValueOrErrors.Default.return(
                PredicateValue.Default.sum(
                  Sum.Default.left(PredicateValue.Default.unit()),
                ),
              )
            : ValueOrErrors.Default.throwOne(
                `received non sum renderer kind "${renderer.kind}" when resolving defaultValue for sum`,
              );

      if (t.kind == "record")
        return renderer.kind == "recordForm"
          ? ValueOrErrors.Operations.All(
              List<ValueOrErrors<[string, PredicateValue], string>>(
                t.fields
                  .entrySeq()
                  .map(([fieldName, field]) =>
                    dispatchDefaultValue(
                      injectedPrimitives,
                      types,
                      forms,
                    )(field, renderer.fields.get(fieldName)!).Then((value) =>
                      ValueOrErrors.Default.return([fieldName, value] as const),
                    ),
                  ),
              ),
            ).Then((res) =>
              ValueOrErrors.Default.return(
                PredicateValue.Default.record(Map(res)),
              ),
            )
          : ValueOrErrors.Default.throwOne(
              `received non record renderer kind "${renderer.kind}" when resolving defaultValue for record`,
            );

      if (t.kind == "lookup") {
        if (renderer.kind != "baseLookupRenderer") {
          return ValueOrErrors.Default.throwOne(
            `received non lookup renderer kind "${renderer.kind}" when resolving defaultValue for lookup`,
          );
        }
        const lookupType = types.get(t.name);
        if (lookupType == undefined) {
          return ValueOrErrors.Default.throwOne(
            `lookup type ${t.name} not found in types`,
          );
        }
        const formRenderer = forms.get(renderer.lookupRendererName);
        if (formRenderer == undefined) {
          return ValueOrErrors.Default.throwOne(
            `lookup form renderer ${renderer.lookupRendererName} not found in forms`,
          );
        }
        return dispatchDefaultValue(
          injectedPrimitives,
          types,
          forms,
        )(lookupType, formRenderer);
      }

      return ValueOrErrors.Default.throwOne(
        `type of kind ${t.kind} not supported by defaultValue`,
      );
    })();
    return result.MapErrors((errors) =>
      errors.map(
        (error) =>
          `${error}\n...When resolving defaultValue for type of kind "${t.kind}" for "${t.typeName}" and renderer kind "${renderer.kind}"`,
      ),
    );
  };

export const dispatchFromAPIRawValue =
  <T extends { [key in keyof T]: { type: any; state: any } }>(
    t: DispatchParsedType<T>,
    types: Map<DispatchTypeName, DispatchParsedType<T>>,
    converters: DispatchApiConverters<T>,
    injectedPrimitives?: InjectedPrimitives<T>,
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
          !injectedPrimitives?.injectedPrimitives
            .keySeq()
            .contains(t.name as keyof T)
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
        const result = converters[t.kind].fromAPIRawValue(raw);
        const caseType = t.args.get(result.caseName);

        if (caseType == undefined)
          return ValueOrErrors.Default.throwOne(
            `union case ${result.caseName} not found in type ${JSON.stringify(
              t,
            )}`,
          );

        const fieldsType = caseType.fields;

        // TODO  -- assumption here that the fields type is a record

        return dispatchFromAPIRawValue(
          fieldsType,
          types,
          converters,
          injectedPrimitives,
        )(result.fields).Then((fields) => {
          return ValueOrErrors.Default.return(
            PredicateValue.Default.unionCase(
              result.caseName,
              fields as ValueRecord,
            ),
          );
        });
      }

      if (t.kind == "singleSelection") {
        const result = converters["SingleSelection"].fromAPIRawValue(raw);
        const isSome = result.kind == "l";
        const value = isSome
          ? PredicateValue.Default.record(Map(result.value))
          : PredicateValue.Default.unit();

        return ValueOrErrors.Default.return(
          PredicateValue.Default.option(isSome, value),
        );
      }
      if (t.kind == "multiSelection") {
        const result = converters["MultiSelection"].fromAPIRawValue(raw);
        const values = result.map((_) => PredicateValue.Default.record(Map(_)));
        return ValueOrErrors.Default.return(
          PredicateValue.Default.record(Map(values)),
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
        const lookupType = t.args[0];
        if (lookupType.kind != "lookup") {
          return ValueOrErrors.Default.throwOne(
            `expected lookup type for table arg, got ${JSON.stringify(
              lookupType,
            )}`,
          );
        }
        return MapRepo.Operations.tryFindWithError(
          lookupType.name, // TODO check this
          types,
          () => `type ${lookupType.name} not found in types`,
        ).Then((type) =>
          ValueOrErrors.Operations.All(
            List<ValueOrErrors<[string, ValueRecord], string>>(
              converterResult.data
                .toArray()
                .map(([key, record]) =>
                  dispatchFromAPIRawValue(
                    type,
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

      // TODO -- this can be more functional
      if (t.kind == "record") {
        if (typeof raw != "object") {
          return ValueOrErrors.Default.throwOne(
            `object expected but got ${JSON.stringify(raw)}`,
          );
        }
        let result: Map<string, PredicateValue> = Map();
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
  <T extends { [key in keyof T]: { type: any; state: any } }>(
    t: DispatchParsedType<T>,
    types: Map<DispatchTypeName, DispatchParsedType<T>>,
    converters: DispatchApiConverters<T>,
    injectedPrimitives?: InjectedPrimitives<T>,
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
        if (!PredicateValue.Operations.IsRecord(raw)) {
          return ValueOrErrors.Default.throwOne(
            `Option expected but got ${JSON.stringify(raw)}`,
          );
        }
        const caseName = raw.fields.get("caseName");
        if (
          caseName == undefined ||
          !PredicateValue.Operations.IsString(caseName)
        ) {
          return ValueOrErrors.Default.throwOne(
            `caseName expected but got ${JSON.stringify(raw)}`,
          );
        }
        const fields = raw.fields.get("fields");
        if (
          fields == undefined ||
          !PredicateValue.Operations.IsRecord(fields)
        ) {
          return ValueOrErrors.Default.throwOne(
            `fields expected but got ${JSON.stringify(raw)}`,
          );
        }
        const rawUnionCase = {
          caseName,
          fields: fields.fields.toJS(),
        };
        return ValueOrErrors.Operations.Return(
          converters["union"].toAPIRawValue([
            rawUnionCase,
            formState?.commonFormState?.modifiedByUser ?? false,
          ]),
        );
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

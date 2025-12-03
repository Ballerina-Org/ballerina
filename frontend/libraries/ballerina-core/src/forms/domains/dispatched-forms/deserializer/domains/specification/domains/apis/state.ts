import { List, Map, Set } from "immutable";
import {
  ConcreteRenderers,
  DispatchInjectablesTypes,
  DispatchInjectedPrimitives,
  EntityApi,
  isObject,
  isString,
  Renderer,
  ValueOrErrors,
} from "../../../../../../../../../main";
import {
  DispatchIsObject,
  DispatchParsedType,
  DispatchTypeName,
  FilterType,
  SerializedType,
  SumNType,
} from "../types/state";

export type SerializedEntityApi = {
  type?: any;
  methods?: any;
};

export type SerializedLookupApi = {
  enums?: unknown;
  streams?: unknown;
  one?: unknown;
  many?: unknown;
  tables?: unknown;
};

export type SpecificationApis<T> = {
  entities: Map<string, EntityApi>; // TODO move entity apis out
  enums?: EnumApis;
  streams?: StreamApis;
  tables?: TableApis<T>;
  lookups?: LookupApis;
};

export type EnumApiName = string;
export type EnumApis = Map<EnumApiName, DispatchTypeName>;
export const EnumApis = {
  Operations: {
    Deserialize: (
      serializedApiEnums?: unknown,
    ): ValueOrErrors<undefined | EnumApis, string> => {
      return serializedApiEnums === undefined
        ? ValueOrErrors.Default.return(undefined)
        : !isObject(serializedApiEnums)
          ? ValueOrErrors.Default.throwOne(
              `serializedApiEnums is not an object`,
            )
          : ValueOrErrors.Operations.All(
              List<ValueOrErrors<[EnumApiName, DispatchTypeName], string>>(
                Object.entries(serializedApiEnums).map(([key, value]) =>
                  !isString(key)
                    ? ValueOrErrors.Default.throwOne(`key is not a string`)
                    : !isString(value)
                      ? ValueOrErrors.Default.throwOne(`value is not a string`)
                      : ValueOrErrors.Default.return([key, value]),
                ),
              ),
            )
              .Then((entries) =>
                ValueOrErrors.Default.return(
                  Map<EnumApiName, DispatchTypeName>(entries),
                ),
              )
              .MapErrors((errors) =>
                errors.map(
                  (error) => `${error}\n...When deserializing enum apis`,
                ),
              );
    },
  },
};

export type StreamApiName = string;
export type StreamApis = Map<StreamApiName, DispatchTypeName>;
export const StreamApis = {
  Operations: {
    Deserialize: (
      serializedApiStreams?: unknown,
    ): ValueOrErrors<undefined | StreamApis, string> => {
      return serializedApiStreams === undefined
        ? ValueOrErrors.Default.return(undefined)
        : !isObject(serializedApiStreams)
          ? ValueOrErrors.Default.throwOne(
              `serializedApiStreams is not an object`,
            )
          : ValueOrErrors.Operations.All(
              List<ValueOrErrors<[StreamApiName, DispatchTypeName], string>>(
                Object.entries(serializedApiStreams).map(([key, value]) =>
                  !isString(key)
                    ? ValueOrErrors.Default.throwOne(`key is not a string`)
                    : !isString(value)
                      ? ValueOrErrors.Default.throwOne(`value is not a string`)
                      : ValueOrErrors.Default.return([key, value]),
                ),
              ),
            )
              .Then((entries) =>
                ValueOrErrors.Default.return(
                  Map<StreamApiName, DispatchTypeName>(entries),
                ),
              )
              .MapErrors((errors) =>
                errors.map(
                  (error) => `${error}\n...When deserializing stream apis`,
                ),
              );
    },
  },
};

type SerializedTableApi = {
  type: unknown;
  methods: unknown;
  highlightedFilters?: unknown;
  filtering?: unknown;
  sorting?: unknown;
};

const TableMethods = {
  add: "add",
  duplicate: "duplicate",
  remove: "remove",
  removeAll: "removeAll",
  move: "move",
} as const;

type ColumnName = string;

export type ColumnFilters<T> = {
  displayType: DispatchParsedType<T>;
  displayRenderer: Renderer<T>;
  filters: SumNType<T>;
  label?: string;
  tooltip?: string;
  details?: string;
};
export type TableMethod = (typeof TableMethods)[keyof typeof TableMethods];
export type TableApiName = string;
export type TableApiFiltering<T> = Map<ColumnName, ColumnFilters<T>>;
export type TableApis<T> = Map<
  TableApiName,
  {
    type: DispatchTypeName;
    methods: Array<TableMethod>;
    highlightedFilters?: List<ColumnName>;
    filtering?: TableApiFiltering<T>;
    sorting?: Array<ColumnName>;
  }
>;
export const TableApis = {
  Operations: {
    IsValidTables: (
      value: unknown,
    ): value is Record<string, SerializedTableApi> =>
      typeof value == "object" &&
      value != null &&
      Object.entries(value).every(
        ([key, value]) =>
          typeof key == "string" && typeof value == "object" && value != null,
      ),
    IsValidTableApi: (value: unknown): value is SerializedTableApi =>
      typeof value == "object" && value != null && "type" in value,
    IsMethod: (value: unknown): value is TableMethod => {
      return (
        isString(value) &&
        Object.values(TableMethods).includes(value as TableMethod)
      );
    },
    IsValidSorting: (value: unknown): value is Array<ColumnName> => {
      return Array.isArray(value) && value.every((value) => isString(value));
    },
    IsMethodsArray: (values: unknown): values is Array<TableMethod> => {
      return (
        Array.isArray(values) &&
        values.every((value) => TableApis.Operations.IsMethod(value))
      );
    },
    IsValidHighlightedFilters: (value: unknown): value is Array<ColumnName> => {
      return (
        value === undefined ||
        (Array.isArray(value) && value.every((value) => isString(value)))
      );
    },
    DeserializeFiltering: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContext,
      ExtraContext,
    >(
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >,
      types: Map<DispatchTypeName, DispatchParsedType<T>>,
      serializedTypes: Record<string, SerializedType<T>>,
      serializedFiltering?: unknown,
      injectedPrimitives?: DispatchInjectedPrimitives<T>,
    ): ValueOrErrors<undefined | TableApiFiltering<T>, string> => {
      return serializedFiltering
        ? ValueOrErrors.Operations.All(
            List<ValueOrErrors<[ColumnName, ColumnFilters<T>], string>>(
              Object.entries(serializedFiltering).map(([key, value]) =>
                !isString(key)
                  ? ValueOrErrors.Default.throwOne<
                      [ColumnName, ColumnFilters<T>],
                      string
                    >(`key is not a string`)
                  : !isObject(value)
                    ? ValueOrErrors.Default.throwOne<
                        [ColumnName, ColumnFilters<T>],
                        string
                      >(`value is not an object`)
                    : !("display" in value)
                      ? ValueOrErrors.Default.throwOne<
                          [ColumnName, ColumnFilters<T>],
                          string
                        >(`display property is missing from value`)
                      : !isObject(value.display)
                        ? ValueOrErrors.Default.throwOne<
                            [ColumnName, ColumnFilters<T>],
                            string
                          >(`display is not an object`)
                        : !("renderer" in value.display)
                          ? ValueOrErrors.Default.throwOne<
                              [ColumnName, ColumnFilters<T>],
                              string
                            >(`renderer property is missing from display`)
                          : !("type" in value)
                            ? ValueOrErrors.Default.throwOne<
                                [ColumnName, ColumnFilters<T>],
                                string
                              >(`type property is missing from value`)
                            : !("operators" in value)
                              ? ValueOrErrors.Default.throwOne<
                                  [ColumnName, ColumnFilters<T>],
                                  string
                                >(`operators property is missing from value`)
                              : !Array.isArray(value.operators)
                                ? ValueOrErrors.Default.throwOne<
                                    [ColumnName, ColumnFilters<T>],
                                    string
                                  >(`operators is not an array`)
                                : DispatchParsedType.Operations.ParseRawType(
                                    "filter",
                                    value.type as SerializedType<T>,
                                    serializedTypes,
                                    Map(),
                                    injectedPrimitives,
                                  ).Then((type) =>
                                    Renderer.Operations.Deserialize(
                                      type[0],
                                      // checked above that this is an object with renderer property
                                      type[0].kind == "primitive"
                                        ? (
                                            value.display as {
                                              renderer: string;
                                            }
                                          ).renderer
                                        : value.display,
                                      concreteRenderers,
                                      types,
                                      undefined,
                                      {},
                                      Map(),
                                    ).Then(([renderer, _]) =>
                                      ValueOrErrors.Operations.All<
                                        FilterType<T>,
                                        string
                                      >(
                                        List<
                                          ValueOrErrors<FilterType<T>, string>
                                        >(
                                          // checked above that this is an array
                                          (value.operators as unknown[]).map(
                                            (operator: unknown) =>
                                              !isString(operator)
                                                ? ValueOrErrors.Default.throwOne<
                                                    FilterType<T>,
                                                    string
                                                  >(`operator is not a string`)
                                                : DispatchParsedType.Operations.ParseRawFilterType(
                                                    operator,
                                                    type[0],
                                                  ),
                                          ),
                                        ),
                                      ).Then((filters) =>
                                        ValueOrErrors.Default.return([
                                          key,
                                          {
                                            displayType: type[0],
                                            displayRenderer: renderer,
                                            filters:
                                              DispatchParsedType.Default.sumN(
                                                filters.toArray(),
                                              ),
                                            label:
                                              typeof value.display ===
                                                "object" &&
                                              value.display !== null &&
                                              "label" in value.display &&
                                              isString(value.display.label)
                                                ? value.display.label
                                                : undefined,
                                            tooltip:
                                              typeof value.display ===
                                                "object" &&
                                              value.display !== null &&
                                              "tooltip" in value.display &&
                                              isString(value.display.tooltip)
                                                ? value.display.tooltip
                                                : undefined,
                                            details:
                                              typeof value.display ===
                                                "object" &&
                                              value.display !== null &&
                                              "details" in value.display &&
                                              isString(value.display.details)
                                                ? value.display.details
                                                : undefined,
                                          },
                                        ] as const),
                                      ),
                                    ),
                                  ),
              ),
            ),
          ).Then((filters) => ValueOrErrors.Default.return(Map(filters)))
        : ValueOrErrors.Default.return(undefined);
    },
    Deserialize: <
      T extends DispatchInjectablesTypes<T>,
      Flags,
      CustomPresentationContext,
      ExtraContext,
    >(
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >,
      types: Map<DispatchTypeName, DispatchParsedType<T>>,
      serializedTypes: Record<string, SerializedType<T>>,
      serializedApiTables?: unknown,
      injectedPrimitives?: DispatchInjectedPrimitives<T>,
    ): ValueOrErrors<undefined | TableApis<T>, string> => {
      if (serializedApiTables === undefined) {
        return ValueOrErrors.Default.return(undefined);
      }

      const isValidTables =
        TableApis.Operations.IsValidTables(serializedApiTables);
      if (!isValidTables) {
        return ValueOrErrors.Default.throwOne(
          `serializedApiTables is not a valid tables api object`,
        );
      }

      return ValueOrErrors.Operations.All(
        List<
          ValueOrErrors<
            [
              TableApiName,
              {
                type: DispatchTypeName;
                methods: Array<TableMethod>;
                highlightedFilters?: List<ColumnName>;
                filtering?: TableApiFiltering<T>;
              },
            ],
            string
          >
        >(
          Object.entries(serializedApiTables).map(([key, value], index) =>
            !TableApis.Operations.IsValidTableApi(value)
              ? ValueOrErrors.Default.throwOne(
                  `${key} is not a valid table api object`,
                )
              : TableApis.Operations.DeserializeFiltering(
                  concreteRenderers,
                  types,
                  serializedTypes,
                  value.filtering,
                  injectedPrimitives,
                ).Then((filtering) => {
                  if (typeof value.type !== "string") {
                    return ValueOrErrors.Default.throwOne(
                      `type is not a string`,
                    );
                  }

                  const finalMethods = value.methods ?? [];
                  const finalSorting = value.sorting ?? [];

                  if (!TableApis.Operations.IsValidSorting(finalSorting)) {
                    return ValueOrErrors.Default.throwOne(
                      `sorting is not an array of strings`,
                    );
                  }

                  if (!TableApis.Operations.IsMethodsArray(finalMethods)) {
                    return ValueOrErrors.Default.throwOne(
                      `methods is not an array of valid table methods`,
                    );
                  }

                  if (
                    !TableApis.Operations.IsValidHighlightedFilters(
                      value.highlightedFilters,
                    )
                  ) {
                    return ValueOrErrors.Default.throwOne(
                      `highlightedFilters is not an array of strings`,
                    );
                  }

                  return ValueOrErrors.Default.return([
                    key,
                    {
                      type: value.type,
                      methods: finalMethods,
                      highlightedFilters: value.highlightedFilters
                        ? List(value.highlightedFilters)
                        : undefined,
                      filtering,
                      sorting: finalSorting,
                    },
                  ]);
                }),
          ),
        ),
      )
        .Then((entries) =>
          ValueOrErrors.Default.return(
            Map<
              TableApiName,
              {
                type: DispatchTypeName;
                methods: Array<TableMethod>;
                highlightedFilters?: List<ColumnName>;
                filtering?: TableApiFiltering<T>;
              }
            >(entries),
          ),
        )
        .MapErrors((errors) =>
          errors.map((error) => `${error}\n...When deserializing table apis`),
        );
    },
  },
};

export type LookupApiName = string;
export type LookupApis = Map<LookupApiName, { one: LookupApiOne }>;
export type LookupApiOne = Map<
  string,
  {
    type: DispatchTypeName;
    methods: {
      get: boolean;
      update: boolean;
      getManyUnlinked: boolean;
      create: boolean;
      delete: boolean;
    };
  }
>;

// TODO add many deserialization
export const LookupApis = {
  Operations: {
    isOneApi: (
      _: unknown,
    ): _ is {
      one: {
        [key: string]: {
          type: DispatchTypeName;
          methods: Array<string>;
        };
      };
    } =>
      DispatchIsObject(_) &&
      "one" in _ &&
      DispatchIsObject(_.one) &&
      Object.values(_.one).every(
        (value) =>
          isObject(value) &&
          "type" in value &&
          isString(value.type) &&
          "methods" in value &&
          Array.isArray(value.methods) &&
          value.methods.every((method) => isString(method)),
      ),
    DeserializeOne: (
      serializedOneApi: unknown,
    ): ValueOrErrors<LookupApiOne, string> =>
      !LookupApis.Operations.isOneApi(serializedOneApi)
        ? ValueOrErrors.Default.throwOne<LookupApiOne, string>(
            `serializedLookupApi is not a valid lookup api`,
          )
        : ValueOrErrors.Default.return<LookupApiOne, string>(
            Map(
              Object.entries(serializedOneApi.one).map(([key, value]) => [
                key,
                {
                  type: value.type,
                  methods: {
                    get: value.methods.includes("get"),
                    update: value.methods.includes("update"),
                    getManyUnlinked: value.methods.includes("getManyUnlinked"),
                    create: value.methods.includes("create"),
                    delete: value.methods.includes("delete"),
                  },
                },
              ]),
            ),
          ).MapErrors((errors) =>
            errors.map((error) => `${error}\n...When deserializing lookup api`),
          ),
    Deserialize: (
      serializedApiLookups?: unknown,
    ): ValueOrErrors<undefined | LookupApis, string> => {
      return serializedApiLookups === undefined
        ? ValueOrErrors.Default.return(undefined)
        : !isObject(serializedApiLookups)
          ? ValueOrErrors.Default.throwOne(
              `serializedApiLookups is not an object`,
            )
          : ValueOrErrors.Operations.All(
              List<
                ValueOrErrors<[LookupApiName, { one: LookupApiOne }], string>
              >(
                Object.entries(serializedApiLookups).map(([key, value]) =>
                  LookupApis.Operations.DeserializeOne(value).Then((one) =>
                    ValueOrErrors.Default.return([key, { one }]),
                  ),
                ),
              ),
            )
              .Then((entries) =>
                ValueOrErrors.Default.return(
                  Map<LookupApiName, { one: LookupApiOne }>(entries),
                ),
              )
              .MapErrors((errors) =>
                errors.map(
                  (error) => `${error}\n...When deserializing lookup apis`,
                ),
              );
    },
  },
};

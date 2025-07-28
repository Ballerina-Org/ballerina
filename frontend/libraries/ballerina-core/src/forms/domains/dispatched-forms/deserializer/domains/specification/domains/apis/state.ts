import { List, Map } from "immutable";
import {
  EntityApi,
  isObject,
  isString,
  ValueOrErrors,
} from "../../../../../../../../../main";
import { DispatchIsObject, DispatchTypeName } from "../types/state";

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

export type SpecificationApis = {
  entities: Map<string, EntityApi>; // TODO move entity apis out
  enums?: EnumApis;
  streams?: StreamApis;
  tables?: TableApis;
  lookups?: LookupApis;
};

export type EnumApiName = string;
export type EnumApis = Map<EnumApiName, DispatchTypeName>;
export const EnumApis = {
  Operations: {
    Deserialize: (
      serializedApiEnums?: unknown,
    ): ValueOrErrors<undefined | EnumApis, string> =>
      serializedApiEnums === undefined
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
              ),
  },
};

export type StreamApiName = string;
export type StreamApis = Map<StreamApiName, DispatchTypeName>;
export const StreamApis = {
  Operations: {
    Deserialize: (
      serializedApiStreams?: unknown,
    ): ValueOrErrors<undefined | StreamApis, string> =>
      serializedApiStreams === undefined
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
              ),
  },
};

const TableMethods = {
  add: "add",
  duplicate: "duplicate",
  remove: "remove",
  move: "move",
} as const;
export type TableMethod = (typeof TableMethods)[keyof typeof TableMethods];
export type TableApiName = string;
export type TableApis = Map<
  TableApiName,
  { type: DispatchTypeName; methods: Array<TableMethod> }
>;
export const TableApis = {
  Operations: {
    IsMethod: (value: unknown): value is TableMethod => {
      return (
        isString(value) &&
        Object.values(TableMethods).includes(value as TableMethod)
      );
    },
    IsMethodsArray: (values: unknown[]): values is Array<TableMethod> => {
      return values.every((value) => TableApis.Operations.IsMethod(value));
    },
    Deserialize: (
      serializedApiTables?: unknown,
    ): ValueOrErrors<undefined | TableApis, string> =>
      serializedApiTables === undefined
        ? ValueOrErrors.Default.return(undefined)
        : !isObject(serializedApiTables)
          ? ValueOrErrors.Default.throwOne(
              `serializedApiTables is not an object`,
            )
          : ValueOrErrors.Operations.All(
              List<
                ValueOrErrors<
                  [
                    TableApiName,
                    { type: DispatchTypeName; methods: Array<TableMethod> },
                  ],
                  string
                >
              >(
                Object.entries(serializedApiTables).map(([key, value]) =>
                  !isString(key)
                    ? ValueOrErrors.Default.throwOne(`key is not a string`)
                    : !isObject(value)
                      ? ValueOrErrors.Default.throwOne(`value is not an object`)
                      : !("type" in value)
                        ? ValueOrErrors.Default.throwOne(
                            `type is missing from value`,
                          )
                        : !isString(value.type)
                          ? ValueOrErrors.Default.throwOne(
                              `type is not a string`,
                            )
                          : !("methods" in value)
                            ? ValueOrErrors.Default.return([
                                key,
                                { ...value, methods: [] } as {
                                  type: DispatchTypeName;
                                  methods: Array<TableMethod>;
                                },
                              ])
                            : !Array.isArray(value.methods)
                              ? ValueOrErrors.Default.throwOne(
                                  `methods is not an array`,
                                )
                              : !TableApis.Operations.IsMethodsArray(
                                    value.methods,
                                  )
                                ? ValueOrErrors.Default.throwOne(
                                    `methods is not an array of valid table methods`,
                                  )
                                : ValueOrErrors.Default.return([
                                    key,
                                    {
                                      // TODO: type assertion is safe but would like typescript to know that
                                      type: value.type as DispatchTypeName,
                                      methods: value.methods,
                                    },
                                  ]),
                ),
              ),
            )
              .Then((entries) =>
                ValueOrErrors.Default.return(
                  Map<
                    TableApiName,
                    { type: DispatchTypeName; methods: Array<TableMethod> }
                  >(entries),
                ),
              )
              .MapErrors((errors) =>
                errors.map(
                  (error) => `${error}\n...When deserializing table apis`,
                ),
              ),
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
    ): ValueOrErrors<undefined | LookupApis, string> =>
      serializedApiLookups === undefined
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
              ),
  },
};

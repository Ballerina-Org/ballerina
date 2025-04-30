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

export type TableApiName = string;
export type TableApis = Map<TableApiName, { type: DispatchTypeName }>;
export const TableApis = {
  Operations: {
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
                  [TableApiName, { type: DispatchTypeName }],
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
                          : // TODO: type assertion is safe but would like typescript to know that
                            ValueOrErrors.Default.return([
                              key,
                              value as { type: DispatchTypeName },
                            ]),
                ),
              ),
            )
              .Then((entries) =>
                ValueOrErrors.Default.return(
                  Map<TableApiName, { type: DispatchTypeName }>(entries),
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
export type LookupApis = Map<LookupApiName, LookupApi>;
export type LookupApi = {
  enums?: EnumApis;
  streams?: StreamApis;
  tables?: TableApis;
  lookups?: LookupApis;
  one?: Map<
    string,
    {
      type: DispatchTypeName;
      methods: {
        update: boolean;
        getManyUnlinked: boolean;
        create: boolean;
        delete: boolean;
      };
    }
  >;
  many?: Map<
    string,
    {
      type: DispatchTypeName;
      methods: {
        update: boolean;
        getManyUnlinked: boolean;
        create: boolean;
        delete: boolean;
      };
    }
  >;
};

// TODO add one and many deserialization
export const LookupApis = {
  Operations: {
    isLookupApi: (
      _: unknown,
    ): _ is {
      enums?: unknown;
      streams?: unknown;
      tables?: unknown;
      lookups?: unknown;
      one?: unknown;
      many?: unknown;
    } => DispatchIsObject(_),
    DeserializeOne: (
      serializedLookupApi: unknown,
    ): ValueOrErrors<LookupApi, string> =>
      !LookupApis.Operations.isLookupApi(serializedLookupApi)
        ? ValueOrErrors.Default.throwOne(`serializedLookupApi is not an object`)
        : EnumApis.Operations.Deserialize(serializedLookupApi.enums)
            .Then((enums) =>
              StreamApis.Operations.Deserialize(
                serializedLookupApi.streams,
              ).Then((streams) =>
                TableApis.Operations.Deserialize(
                  serializedLookupApi.tables,
                ).Then((tables) =>
                  LookupApis.Operations.Deserialize(
                    serializedLookupApi.lookups,
                  ).Then((lookups) =>
                    ValueOrErrors.Default.return({
                      enums,
                      streams,
                      tables,
                      lookups,
                    }),
                  ),
                ),
              ),
            )
            .MapErrors((errors) =>
              errors.map(
                (error) => `${error}\n...When deserializing lookup api`,
              ),
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
              List<ValueOrErrors<[LookupApiName, LookupApi], string>>(
                Object.entries(serializedApiLookups).map(([key, value]) =>
                  ValueOrErrors.Default.return([key, value]),
                ),
              ),
            )
              .Then((entries) =>
                ValueOrErrors.Default.return(
                  Map<LookupApiName, LookupApi>(entries),
                ),
              )
              .MapErrors((errors) =>
                errors.map(
                  (error) => `${error}\n...When deserializing lookup apis`,
                ),
              ),
  },
};

import { Set, Map, List } from "immutable";
import {
  InjectedPrimitives,
  DispatchIsObject,
  DispatchTypeName,
  EntityApi,
  CreateLauncher,
  EditLauncher,
  PassthroughLauncher,
  SerializedEntityApi,
  Launcher,
} from "../../../../../../../main";
import { ValueOrErrors } from "../../../../../../collections/domains/valueOrErrors/state";
import { Form } from "./domains/form/state";
import { DispatchParsedType, SerializedType } from "./domains/types/state";
import { DispatchApiConverters } from "../../../built-ins/state";

const INITIAL_CONFIG: ValidatedSerializedSpecification = {
  types: {},
  forms: {},
  apis: {
    enumOptions: {},
    searchableStreams: {},
    entities: {},
  },
  launchers: {},
};

export type SerializedSpecification = {
  types?: unknown;
  apis?: unknown;
  forms?: unknown;
  launchers?: unknown;
};

export type ValidatedSerializedSpecification = {
  types: object;
  apis: {
    enumOptions: object;
    searchableStreams: object;
    entities: object;
  };
  forms: object;
  launchers: Record<
    string,
    | {
        kind: "create";
        form: string;
        api: string;
        configApi: string;
      }
    | {
        kind: "edit";
        form: string;
        api: string;
        configApi: string;
      }
    | {
        kind: "passthrough";
        form: string;
        configType: string;
      }
  >;
};

export type Specification<T> = {
  types: Map<DispatchTypeName, DispatchParsedType<T>>;
  apis: {
    enums: Map<string, DispatchTypeName>;
    streams: Map<string, DispatchTypeName>;
    entities: Map<string, EntityApi>;
  };
  forms: Map<string, Form<T>>;
  launchers: {
    create: Map<string, CreateLauncher>;
    edit: Map<string, EditLauncher>;
    passthrough: Map<string, PassthroughLauncher>;
  };
};

export const Specification = {
  Operations: {
    hasTypes: (_: unknown): _ is { types: object } =>
      DispatchIsObject(_) && "types" in _ && DispatchIsObject(_.types),
    hasForms: (_: unknown): _ is { forms: object } =>
      DispatchIsObject(_) && "forms" in _ && DispatchIsObject(_.forms),
    hasApis: (
      _: unknown,
    ): _ is {
      apis: {
        enumOptions: object;
        searchableStreams: object;
        entities: object;
      };
    } =>
      DispatchIsObject(_) &&
      "apis" in _ &&
      DispatchIsObject(_.apis) &&
      "enumOptions" in _.apis &&
      DispatchIsObject(_.apis.enumOptions) &&
      "searchableStreams" in _.apis &&
      DispatchIsObject(_.apis.searchableStreams) &&
      "entities" in _.apis &&
      DispatchIsObject(_.apis.entities),
    hasLaunchers: (
      _: unknown,
    ): _ is {
      launchers: Record<
        string,
        | {
            kind: "create";
            form: string;
            api: string;
            configApi: string;
          }
        | {
            kind: "edit";
            form: string;
            api: string;
            configApi: string;
          }
        | {
            kind: "passthrough";
            form: string;
            configType: string;
          }
      >;
    } =>
      DispatchIsObject(_) && "launchers" in _ && DispatchIsObject(_.launchers),
    // TODO, doesn't need to be its own operation
    DeserializeForms: <T>(
      forms: object,
      types: Map<DispatchTypeName, DispatchParsedType<T>>,
      fieldViews?: any,
    ): ValueOrErrors<Map<string, Form<T>>, string> =>
      ValueOrErrors.Operations.All(
        List<ValueOrErrors<[string, Form<T>], string>>(
          Object.entries(forms).map(([formName, form]) =>
            Form<T>()
              .Operations.Deserialize(types, formName, form, fieldViews)
              .Then((form) => ValueOrErrors.Default.return([formName, form])),
          ),
        ),
      ).Then((forms) => ValueOrErrors.Default.return(Map(forms))),
    tryAsValidSpecification:
      <T extends { [key in keyof T]: { type: any; state: any } }>(
        apiConverters: DispatchApiConverters<T>,
        injectedPrimitives?: InjectedPrimitives<T>,
      ) =>
      (
        serializedSpecification: SerializedSpecification,
      ): ValueOrErrors<ValidatedSerializedSpecification, string> => {
        if (!Specification.Operations.hasTypes(serializedSpecification))
          return ValueOrErrors.Default.throwOne(
            "the specification is missing the required types attribute",
          );

        if (!Specification.Operations.hasForms(serializedSpecification))
          return ValueOrErrors.Default.throwOne(
            "the specification is missing the required forms attribute",
          );

        if (!Specification.Operations.hasApis(serializedSpecification))
          return ValueOrErrors.Default.throwOne(
            "the specification is missing the required apis attribute or its enumOptions, searchableStreams, or entities attribute",
          );

        if (!Specification.Operations.hasLaunchers(serializedSpecification))
          return ValueOrErrors.Default.throwOne(
            "the specification is missing the required launchers attribute",
          );

        // This error check must stay in the frontend, as it depends on injected api converters that the form config is unaware of
        if (
          injectedPrimitives?.injectedPrimitives
            .keySeq()
            .toArray()
            .some(
              (injectedPrimitiveName) =>
                !Object.keys(apiConverters).includes(
                  injectedPrimitiveName as string,
                ),
            )
        )
          return ValueOrErrors.Default.throwOne(
            `the formsConfig does not contain an Api Converter for all injected primitives`,
          );

        return ValueOrErrors.Default.return(serializedSpecification);
      },
    Merge: <T extends { [key in keyof T]: { type: any; state: any } }>(
      apiConverters: DispatchApiConverters<T>,
      specifications: SerializedSpecification | SerializedSpecification[],
      injectedPrimitives?: InjectedPrimitives<T>,
    ): ValueOrErrors<ValidatedSerializedSpecification, string> => {
      if (!Array.isArray(specifications)) {
        return Specification.Operations.tryAsValidSpecification(
          apiConverters,
          injectedPrimitives,
        )(specifications);
      }

      if (!specifications || specifications.length === 0)
        return ValueOrErrors.Default.throwOne("no forms configs provided");

      return ValueOrErrors.Operations.All(
        List<ValueOrErrors<ValidatedSerializedSpecification, string>>(
          specifications.map((specification) =>
            Specification.Operations.tryAsValidSpecification(
              apiConverters,
              injectedPrimitives,
            )(specification),
          ),
        ),
      ).Then((validatedSpecifications) => {
        return ValueOrErrors.Default.return(
          validatedSpecifications.reduce(
            (
              acc: ValidatedSerializedSpecification,
              current: ValidatedSerializedSpecification,
            ) => {
              return {
                types: Object.assign(acc.types, current.types),
                forms: Object.assign(acc.forms, current.forms),
                apis: {
                  enumOptions: Object.assign(
                    acc.apis.enumOptions,
                    current.apis.enumOptions,
                  ),
                  searchableStreams: Object.assign(
                    acc.apis.searchableStreams,
                    current.apis.searchableStreams,
                  ),
                  entities: Object.assign(
                    acc.apis.entities,
                    current.apis.entities,
                  ),
                },
                launchers: Object.assign(acc.launchers, current.launchers),
              };
            },
            { ...INITIAL_CONFIG },
          ),
        );
      });
    },
    Deserialize:
      <T extends { [key in keyof T]: { type: any; state: any } }>(
        apiConverters: DispatchApiConverters<T>,
        fieldViews: any,
        injectedPrimitives?: InjectedPrimitives<T>,
      ) =>
      (
        serializedSpecifications:
          | SerializedSpecification
          | SerializedSpecification[],
      ): ValueOrErrors<Specification<T>, string> => {
        return Specification.Operations.Merge(
          apiConverters,
          serializedSpecifications,
          injectedPrimitives,
        )
          .Then((mergedValidatedSpecification) => {
            return ValueOrErrors.Operations.All(
              List<ValueOrErrors<DispatchParsedType<T>, string>>(
                Object.entries(mergedValidatedSpecification.types)
                  // Skip keyof types in first pass as they depend on other types
                  .filter(([_, rawType]) => !SerializedType.isKeyOf(rawType))
                  .map(
                    ([rawTypeName, rawType]: [
                      rawTypeName: string,
                      rawType: SerializedType<T>,
                    ]) =>
                      DispatchParsedType.Operations.ParseRawType(
                        rawTypeName,
                        rawType,
                        Set(Object.keys(mergedValidatedSpecification.types)),
                        injectedPrimitives,
                      ),
                  ),
              ),
            )
              .MapErrors((errors) =>
                errors.map(
                  (error) => `${error}\n...When parsing unextended types`,
                ),
              )
              .Then((unextendedTypes) =>
                DispatchParsedType.Operations.ExtendDispatchParsedTypes(
                  unextendedTypes.reduce(
                    (acc, type) => acc.set(type.typeName, type),
                    Map<DispatchTypeName, DispatchParsedType<T>>(),
                  ),
                ),
              )
              .MapErrors((errors) =>
                errors.map((error) => `${error}\n...When extending types`),
              )
              .Then((extendedTypes) =>
                ValueOrErrors.Operations.All(
                  List<ValueOrErrors<DispatchParsedType<T>, string>>(
                    Object.entries(mergedValidatedSpecification.types)
                      .filter(([_, rawType]) => SerializedType.isKeyOf(rawType))
                      .map(
                        ([rawTypeName, rawType]: [
                          rawTypeName: string,
                          rawType: SerializedType<T>,
                        ]) =>
                          DispatchParsedType.Operations.ParseRawKeyOf(
                            rawTypeName,
                            rawType,
                            extendedTypes,
                          ),
                      ),
                  ),
                )
                  .MapErrors((errors) =>
                    errors.map(
                      (error) => `${error}\n...When parsing keyOf types`,
                    ),
                  )
                  .Then((parsedKeyOfTypes) =>
                    ValueOrErrors.Default.return(
                      extendedTypes.merge(
                        parsedKeyOfTypes.reduce(
                          (acc, type) => acc.set(type.typeName, type),
                          Map<DispatchTypeName, DispatchParsedType<T>>(),
                        ),
                      ),
                    ),
                  ),
              )

              .Then((allTypes) => {
                return Specification.Operations.DeserializeForms<T>(
                  mergedValidatedSpecification.forms,
                  allTypes,
                  fieldViews,
                ).Then((forms) => {
                  let enums: Map<string, DispatchTypeName> = Map();
                  Object.entries(
                    mergedValidatedSpecification.apis.enumOptions,
                  ).forEach(
                    ([enumOptionName, enumOption]) =>
                      (enums = enums.set(enumOptionName, enumOption)),
                  );

                  let streams: Map<string, DispatchTypeName> = Map();
                  Object.entries(
                    mergedValidatedSpecification.apis.searchableStreams,
                  ).forEach(
                    ([searchableStreamName, searchableStream]) =>
                      (streams = streams.set(
                        searchableStreamName,
                        searchableStream,
                      )),
                  );

                  let entities: Map<string, EntityApi> = Map();
                  Object.entries(
                    mergedValidatedSpecification.apis.entities,
                  ).forEach(
                    ([entityApiName, entityApi]: [
                      entiyApiName: string,
                      entityApi: SerializedEntityApi,
                    ]) => {
                      entities = entities.set(entityApiName, {
                        type: entityApi.type,
                        methods: {
                          create: entityApi.methods.includes("create"),
                          get: entityApi.methods.includes("get"),
                          update: entityApi.methods.includes("update"),
                          default: entityApi.methods.includes("default"),
                        },
                      });
                    },
                  );

                  let launchers: Specification<T>["launchers"] = {
                    create: Map<string, CreateLauncher>(),
                    edit: Map<string, EditLauncher>(),
                    passthrough: Map<string, PassthroughLauncher>(),
                  };

                  Object.keys(
                    mergedValidatedSpecification["launchers"],
                  ).forEach((launcherName: any) => {
                    const launcher: Launcher =
                      mergedValidatedSpecification.launchers[launcherName][
                        "kind"
                      ] == "create" ||
                      mergedValidatedSpecification.launchers[launcherName][
                        "kind"
                      ] == "edit"
                        ? {
                            name: launcherName,
                            kind: mergedValidatedSpecification.launchers[
                              launcherName
                            ]["kind"],
                            form: mergedValidatedSpecification.launchers[
                              launcherName
                            ]["form"],
                            api: mergedValidatedSpecification.launchers[
                              launcherName
                            ]["api"],
                            configApi:
                              mergedValidatedSpecification.launchers[
                                launcherName
                              ]["configApi"],
                          }
                        : {
                            name: launcherName,
                            kind: mergedValidatedSpecification.launchers[
                              launcherName
                            ]["kind"],
                            form: mergedValidatedSpecification.launchers[
                              launcherName
                            ]["form"],
                            configType:
                              mergedValidatedSpecification.launchers[
                                launcherName
                              ]["configType"],
                          };
                    if (launcher.kind == "create")
                      launchers.create = launchers.create.set(
                        launcherName,
                        launcher,
                      );
                    else if (launcher.kind == "edit")
                      launchers.edit = launchers.edit.set(
                        launcherName,
                        launcher,
                      );
                    else if (launcher.kind == "passthrough")
                      launchers.passthrough = launchers.passthrough.set(
                        launcherName,
                        launcher,
                      );
                  });

                  return ValueOrErrors.Default.return({
                    types: allTypes,
                    forms,
                    apis: {
                      enums,
                      streams,
                      entities,
                    },
                    launchers,
                  });
                });
              });
          })
          .MapErrors((errors) =>
            errors.map(
              (error) => `${error}\n...When deserializing specification`,
            ),
          );
      },
  },
};

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
  EnumApis,
  StreamApis,
  SpecificationApis,
  TableApis,
  LookupApis,
  MapRepo,
} from "../../../../../../../main";
import { ValueOrErrors } from "../../../../../../collections/domains/valueOrErrors/state";
import { DispatchParsedType, SerializedType } from "./domains/types/state";
import { ConcreteRendererKinds, DispatchApiConverters } from "../../../built-ins/state";
import { Renderer } from "./domains/forms/domains/renderer/state";

// TODO -- either add the lookups to merge, or remove the front end merger
const INITIAL_CONFIG = {
  types: {},
  forms: {},
  apis: {
    lookups: {},
    enumOptions: {},
    searchableStreams: {},
    entities: {},
    tables: {},
  },
  launchers: {},
};

export type SerializedSpecification = {
  types?: unknown;
  apis?: unknown;
  forms?: unknown;
  launchers?: unknown;
};

export type Specification<T> = {
  types: Map<DispatchTypeName, DispatchParsedType<T>>;
  apis: SpecificationApis;
  forms: Map<string, Renderer<T>>;
  launchers: {
    create: Map<string, CreateLauncher>;
    edit: Map<string, EditLauncher>;
    passthrough: Map<string, PassthroughLauncher>;
  };
};

export const Specification = {
  Operations: {
    hasTypes: (
      _: unknown,
    ): _ is { types: Record<string, SerializedType<any>> } =>
      DispatchIsObject(_) && "types" in _ && DispatchIsObject(_.types),
    hasForms: (_: unknown): _ is { forms: object } =>
      DispatchIsObject(_) && "forms" in _ && DispatchIsObject(_.forms),
    hasApis: (
      _: unknown,
    ): _ is {
      apis: {
        entities: object;
        enumOptions?: unknown;
        searchableStreams?: unknown;
        tables?: unknown;
        lookups?: unknown;
      };
    } =>
      DispatchIsObject(_) &&
      "apis" in _ &&
      DispatchIsObject(_.apis) &&
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
    DeserializeForms: <T>(
      forms: object,
      types: Map<DispatchTypeName, DispatchParsedType<T>>,
      concreteRenderers: Record<keyof ConcreteRendererKinds, any>,
    ): ValueOrErrors<Map<string, Renderer<T>>, string> =>
      ValueOrErrors.Operations.All(
        List<ValueOrErrors<[string, Renderer<T>], string>>(
          Object.entries(forms).map(([formName, form]) =>
            !DispatchIsObject(form) ||
            !("type" in form) ||
            typeof form.type != "string"
              ? ValueOrErrors.Default.throwOne(
                  `form ${formName} is missing the required type attribute`,
                )
              : MapRepo.Operations.tryFindWithError(
                  form.type,
                  types,
                  () => `form type ${form.type} not found in types`,
                ).Then((formType) =>
                  Renderer.Operations.Deserialize(
                    formType,
                    form,
                    concreteRenderers,
                    types,
                  )
                    .MapErrors((errors) =>
                      errors.map(
                        (error) =>
                          `${error}\n...When deserializing form ${formName}`,
                      ),
                    )
                    .Then((form) =>
                      ValueOrErrors.Default.return([formName, form]),
                    ),
                ),
          ),
        ),
      ).Then((forms) => ValueOrErrors.Default.return(Map(forms))),
    Deserialize:
      <T extends { [key in keyof T]: { type: any; state: any } }>(
        apiConverters: DispatchApiConverters<T>,
        concreteRenderers: Record<keyof ConcreteRendererKinds, any>,
        injectedPrimitives?: InjectedPrimitives<T>,
      ) =>
      (
        serializedSpecifications:
          | SerializedSpecification
          | SerializedSpecification[],
      ): ValueOrErrors<Specification<T>, string> =>
        injectedPrimitives?.injectedPrimitives
          .keySeq()
          .toArray()
          .some(
            (injectedPrimitiveName) =>
              !Object.keys(apiConverters).includes(
                injectedPrimitiveName as string,
              ),
          )
          ? ValueOrErrors.Default.throwOne(
              `the formsConfig does not contain an Api Converter for all injected primitives`,
            )
          : !Specification.Operations.hasTypes(serializedSpecifications)
          ? ValueOrErrors.Default.throwOne<Specification<T>, string>(
              "types are missing from the specification",
            )
          : ValueOrErrors.Operations.All(
              List<ValueOrErrors<DispatchParsedType<T>, string>>(
                Object.entries(serializedSpecifications.types)
                  // Skip keyof and union types in first pass as they depend on other types, which may be extended
                  .filter(
                    ([_, rawType]) =>
                      !SerializedType.isKeyOf(rawType) &&
                      !SerializedType.isUnion(rawType),
                  )
                  .map(
                    ([rawTypeName, rawType]: [
                      rawTypeName: string,
                      rawType: SerializedType<T>,
                    ]) =>
                      DispatchParsedType.Operations.ParseRawType(
                        rawTypeName,
                        rawType,
                        Set(Object.keys(serializedSpecifications.types)),
                        serializedSpecifications.types,
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
                    Object.entries(serializedSpecifications.types)
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
                  )
                  .Then((parsedAndExtendedTypes) =>
                    ValueOrErrors.Operations.All(
                      List<ValueOrErrors<DispatchParsedType<T>, string>>(
                        Object.entries(serializedSpecifications.types)
                          // Skip keyof and union types in first pass as they depend on other types, which may be extended
                          .filter(([_, rawType]) =>
                            SerializedType.isUnion(rawType),
                          )
                          .map(
                            ([rawTypeName, rawType]: [
                              rawTypeName: string,
                              rawType: SerializedType<T>,
                            ]) =>
                              DispatchParsedType.Operations.ParseRawType(
                                rawTypeName,
                                rawType,
                                Set(
                                  Object.keys(serializedSpecifications.types),
                                ),
                                serializedSpecifications.types,
                                injectedPrimitives,
                              ),
                          ),
                      ),
                    )
                      .Then((parsedUnionTypes) =>
                        ValueOrErrors.Default.return(
                          parsedAndExtendedTypes.merge(
                            parsedUnionTypes.reduce(
                              (acc, type) => acc.set(type.typeName, type),
                              Map<DispatchTypeName, DispatchParsedType<T>>(),
                            ),
                          ),
                        ),
                      )
                      .Then((unresolvedTypes) =>
                        DispatchParsedType.Operations.EvaluateLookupTypes(
                          unresolvedTypes,
                        ),
                      )
                      .Then((allTypes) =>
                        !Specification.Operations.hasForms(
                          serializedSpecifications,
                        )
                          ? ValueOrErrors.Default.throwOne<
                              Specification<T>,
                              string
                            >("forms are missing from the specification")
                          : Specification.Operations.DeserializeForms<T>(
                              serializedSpecifications.forms,
                              allTypes,
                              concreteRenderers,
                            ).Then((forms) =>
                              !Specification.Operations.hasApis(
                                serializedSpecifications,
                              )
                                ? ValueOrErrors.Default.throwOne<
                                    Specification<T>,
                                    string
                                  >("apis are missing from the specification")
                                : // TODO move all apis serialization to the apis state file
                                  EnumApis.Operations.Deserialize(
                                    serializedSpecifications.apis.enumOptions,
                                  ).Then((enums) =>
                                    StreamApis.Operations.Deserialize(
                                      serializedSpecifications.apis
                                        .searchableStreams,
                                    ).Then((streams) =>
                                      TableApis.Operations.Deserialize(
                                        serializedSpecifications.apis.tables,
                                      ).Then((tables) =>
                                        LookupApis.Operations.Deserialize(
                                          serializedSpecifications.apis.lookups,
                                        ).Then((lookups) => {
                                          let entities: Map<string, EntityApi> =
                                            Map();
                                          Object.entries(
                                            serializedSpecifications.apis
                                              .entities,
                                          ).forEach(
                                            ([entityApiName, entityApi]: [
                                              entiyApiName: string,
                                              entityApi: SerializedEntityApi,
                                            ]) => {
                                              entities = entities.set(
                                                entityApiName,
                                                {
                                                  type: entityApi.type,
                                                  methods: {
                                                    create:
                                                      entityApi.methods.includes(
                                                        "create",
                                                      ),
                                                    get: entityApi.methods.includes(
                                                      "get",
                                                    ),
                                                    update:
                                                      entityApi.methods.includes(
                                                        "update",
                                                      ),
                                                    default:
                                                      entityApi.methods.includes(
                                                        "default",
                                                      ),
                                                  },
                                                },
                                              );
                                            },
                                          );

                                          let launchers: Specification<T>["launchers"] =
                                            {
                                              create: Map<
                                                string,
                                                CreateLauncher
                                              >(),
                                              edit: Map<string, EditLauncher>(),
                                              passthrough: Map<
                                                string,
                                                PassthroughLauncher
                                              >(),
                                            };

                                          if (
                                            !Specification.Operations.hasLaunchers(
                                              serializedSpecifications,
                                            )
                                          )
                                            return ValueOrErrors.Default.throwOne<
                                              Specification<T>,
                                              string
                                            >(
                                              "launchers are missing from the specification",
                                            );

                                          Object.keys(
                                            serializedSpecifications[
                                              "launchers"
                                            ],
                                          ).forEach((launcherName: any) => {
                                            const launcher: Launcher =
                                              serializedSpecifications
                                                .launchers[launcherName][
                                                "kind"
                                              ] == "create" ||
                                              serializedSpecifications
                                                .launchers[launcherName][
                                                "kind"
                                              ] == "edit"
                                                ? {
                                                    name: launcherName,
                                                    kind: serializedSpecifications
                                                      .launchers[launcherName][
                                                      "kind"
                                                    ],
                                                    form: serializedSpecifications
                                                      .launchers[launcherName][
                                                      "form"
                                                    ],
                                                    api: serializedSpecifications
                                                      .launchers[launcherName][
                                                      "api"
                                                    ],
                                                    configApi:
                                                      serializedSpecifications
                                                        .launchers[
                                                        launcherName
                                                      ]["configApi"],
                                                  }
                                                : {
                                                    name: launcherName,
                                                    kind: serializedSpecifications
                                                      .launchers[launcherName][
                                                      "kind"
                                                    ],
                                                    form: serializedSpecifications
                                                      .launchers[launcherName][
                                                      "form"
                                                    ],
                                                    configType:
                                                      serializedSpecifications
                                                        .launchers[
                                                        launcherName
                                                      ]["configType"],
                                                  };
                                            if (launcher.kind == "create")
                                              launchers.create =
                                                launchers.create.set(
                                                  launcherName,
                                                  launcher,
                                                );
                                            else if (launcher.kind == "edit")
                                              launchers.edit =
                                                launchers.edit.set(
                                                  launcherName,
                                                  launcher,
                                                );
                                            else if (
                                              launcher.kind == "passthrough"
                                            )
                                              launchers.passthrough =
                                                launchers.passthrough.set(
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
                                              tables,
                                              lookups,
                                            },
                                            launchers,
                                          });
                                        }),
                                      ),
                                    ),
                                  ),
                            ),
                      )
                      .MapErrors((errors) =>
                        errors.map(
                          (error) =>
                            `${error}\n...When deserializing specification`,
                        ),
                      ),
                  ),
              ),
  },
};

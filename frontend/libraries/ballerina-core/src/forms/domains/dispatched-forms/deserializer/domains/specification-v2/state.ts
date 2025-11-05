import { Set, Map, List } from "immutable";
import {
  DispatchInjectedPrimitives,
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
  DispatchInjectablesTypes,
  Unit,
} from "../../../../../../../main";
import { ValueOrErrors } from "../../../../../../collections/domains/valueOrErrors/state";
import { DispatchParsedType, SerializedType } from "./domains/types/state";
import {
  ConcreteRenderers,
  DispatchApiConverters,
} from "../../../built-ins/state";
import { Renderer } from "./domains/forms/domains/renderer/state";

export type SerializedSpecificationV2 = {
  types?: unknown;
  apis?: unknown;
  forms?: unknown;
  launchers?: unknown;
};

export type SerializedLauncher = {
  kind: "create" | "edit" | "passthrough";
  form: string;
  api?: string;
  configApi?: string;
  configType?: string;
};

export type SpecificationV2<T> = {
  types: Map<DispatchTypeName, DispatchParsedType<T>>;
  apis: SpecificationApis<T>;
  forms: Map<string, Renderer<T>>;
  launchers: Launchers;
};

export type Launchers = {
  create: Map<string, CreateLauncher>;
  edit: Map<string, EditLauncher>;
  passthrough: Map<string, PassthroughLauncher>;
};

export const SpecificationV2 = {
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
    DeserializeSpecTypes: <T>(
      launcherTypes: List<DispatchTypeName>,
      serializedTypes: Record<string, SerializedType<T>>,
      injectedPrimitives?: DispatchInjectedPrimitives<T>,
    ): ValueOrErrors<Map<DispatchTypeName, DispatchParsedType<T>>, string> => {
      const serializedTypeNames = Set(Object.keys(serializedTypes));
      return ValueOrErrors.Operations.All(
        List<ValueOrErrors<[DispatchTypeName, DispatchParsedType<T>], string>>(
          launcherTypes
            .reduce((acc, rawTypeName) => {
              const res = DispatchParsedType.Operations.ParseRawType(
                rawTypeName,
                serializedTypes[rawTypeName],
                serializedTypeNames,
                serializedTypes,
                acc,
                injectedPrimitives,
              );
              return res.kind == "errors"
                ? acc.set(rawTypeName, res)
                : res.value[1].set(
                    rawTypeName,
                    ValueOrErrors.Default.return<DispatchParsedType<T>, string>(
                      res.value[0],
                    ),
                  );
            }, Map<DispatchTypeName, ValueOrErrors<DispatchParsedType<T>, string>>())
            .entrySeq()
            .map(([name, type]) =>
              type.Then((type) =>
                ValueOrErrors.Default.return<
                  [string, DispatchParsedType<T>],
                  string
                >([name, type]),
              ),
            ),
        ),
      ).Then((parsedTypes) =>
        ValueOrErrors.Default.return(
          Map<DispatchTypeName, DispatchParsedType<T>>(parsedTypes),
        ),
      );
    },
    DeserializeLaunchers: <T>(
      desiredLaunchers: string[],
      serializedLaunchers: Record<string, SerializedLauncher>,
    ): ValueOrErrors<Launchers, string> => {
      let launchers: SpecificationV2<T>["launchers"] = {
        create: Map<string, CreateLauncher>(),
        edit: Map<string, EditLauncher>(),
        passthrough: Map<string, PassthroughLauncher>(),
      };
      Object.entries(serializedLaunchers)
        .filter(([launcherName, _]) => desiredLaunchers.includes(launcherName))
        .forEach(([launcherName, serializedLauncher]) => {
          const launcher =
            serializedLauncher["kind"] == "create" ||
            serializedLauncher["kind"] == "edit"
              ? {
                  name: launcherName,
                  kind: serializedLauncher["kind"],
                  form: serializedLauncher["form"],
                  api: serializedLauncher["api"],
                  configApi: serializedLauncher["configApi"],
                }
              : {
                  name: launcherName,
                  kind: serializedLauncher["kind"],
                  form: serializedLauncher["form"],
                  configType: serializedLauncher["configType"],
                };
          if (launcher.kind == "create")
            launchers.create = launchers.create.set(
              launcherName,
              launcher as CreateLauncher,
            );
          else if (launcher.kind == "edit")
            launchers.edit = launchers.edit.set(
              launcherName,
              launcher as EditLauncher,
            );
          else if (launcher.kind == "passthrough")
            launchers.passthrough = launchers.passthrough.set(
              launcherName,
              launcher as PassthroughLauncher,
            );
        });
      return ValueOrErrors.Default.return(launchers);
    },
    DeserializeForms: <
      T extends DispatchInjectablesTypes<T>,
      Flags = Unit,
      CustomPresentationContext = Unit,
      ExtraContext = Unit,
    >(
      forms: object,
      types: Map<DispatchTypeName, DispatchParsedType<T>>,
      concreteRenderers: ConcreteRenderers<
        T,
        Flags,
        CustomPresentationContext,
        ExtraContext
      >,
      launcherForms: List<string>,
    ): ValueOrErrors<Map<string, Renderer<T>>, string> => {
      return Object.entries(forms)
        .filter(([formName]) => launcherForms.includes(formName))
        .reduce<
          ValueOrErrors<
            [Map<string, Renderer<T>>, Map<string, Renderer<T>>],
            string
          >
        >(
          (acc, [formName, form]) =>
            acc.Then(([formsMap, accumulatedAlreadyParsedForms]) =>
              !DispatchIsObject(form) ||
              !("type" in form) ||
              typeof form.type != "string"
                ? ValueOrErrors.Default.throwOne<
                    [Map<string, Renderer<T>>, Map<string, Renderer<T>>],
                    string
                  >(`form ${formName} is missing the required type attribute`)
                : accumulatedAlreadyParsedForms.has(formName)
                  ? ValueOrErrors.Default.return<
                      [Map<string, Renderer<T>>, Map<string, Renderer<T>>],
                      string
                    >([
                      formsMap.set(
                        formName,
                        accumulatedAlreadyParsedForms.get(formName)!,
                      ),
                      accumulatedAlreadyParsedForms,
                    ])
                  : MapRepo.Operations.tryFindWithError(
                      form.type,
                      types,
                      () => `form type ${form.type} not found in types`,
                    ).Then((formType) =>
                      Renderer.Operations.Deserialize(
                        "columns" in form
                          ? DispatchParsedType.Default.table(
                              DispatchParsedType.Default.lookup(
                                form.type as string,
                              ),
                            )
                          : formType,
                        form,
                        concreteRenderers as unknown as ConcreteRenderers<
                          T,
                          Flags,
                          CustomPresentationContext
                        >,
                        types,
                        undefined,
                        forms,
                        accumulatedAlreadyParsedForms,
                      )
                        .MapErrors((errors) =>
                          errors.map(
                            (error) =>
                              `${error}\n...When deserializing form ${formName}`,
                          ),
                        )
                        .Then(([deserializedForm, newAlreadyParsedForms]) =>
                          ValueOrErrors.Default.return<
                            [
                              Map<string, Renderer<T>>,
                              Map<string, Renderer<T>>,
                            ],
                            string
                          >([
                            formsMap.set(formName, deserializedForm),
                            newAlreadyParsedForms,
                          ]),
                        ),
                    ),
            ),
          ValueOrErrors.Default.return<
            [Map<string, Renderer<T>>, Map<string, Renderer<T>>],
            string
          >([Map<string, Renderer<T>>(), Map<string, Renderer<T>>()]),
        )
        .Then(([formsMap]) =>
          ValueOrErrors.Default.return<Map<string, Renderer<T>>, string>(
            formsMap,
          ),
        );
    },
    GetFormType: <T>(form: object): ValueOrErrors<string, string> => {
      if (!("type" in form) || typeof form.type != "string")
        return ValueOrErrors.Default.throwOne(
          `form is missing the required type attribute`,
        );
      return ValueOrErrors.Default.return(form.type);
    },
    TryFindForm: <T>(
      formName: string,
      forms: object,
    ): ValueOrErrors<object, string> => {
      if (!(formName in forms))
        return ValueOrErrors.Default.throwOne(
          `form ${formName} not found in forms`,
        );
      return ValueOrErrors.Default.return(Reflect.get(forms, formName));
    },
    GetLauncherFormNames: <T>(launchers: Launchers): List<string> => {
      return launchers.create
        .valueSeq()
        .map((launcher) => launcher.form)
        .concat(launchers.edit.valueSeq().map((launcher) => launcher.form))
        .concat(
          launchers.passthrough.valueSeq().map((launcher) => launcher.form),
        )
        .toList();
    },
    GetCreateAndEditConfigEntityTypes: <T>(
      launchers: Launchers,
      apis: object,
    ): List<ValueOrErrors<string, string>> => {
      const entities = Reflect.get(apis, "entities");
      return launchers.create
        .valueSeq()
        .map((launcher) => {
          const entityType = Reflect.get(entities, launcher.configApi)?.type;
          return entityType
            ? ValueOrErrors.Default.return<string, string>(entityType)
            : ValueOrErrors.Default.throwOne<string, string>(
                `entity type ${launcher.configApi} not found in entities`,
              );
        })
        .concat(
          launchers.edit.valueSeq().map((launcher) => {
            const entityType = Reflect.get(entities, launcher.configApi)?.type;
            return entityType
              ? ValueOrErrors.Default.return<string, string>(entityType)
              : ValueOrErrors.Default.throwOne<string, string>(
                  `entity type ${launcher.configApi} not found in entities`,
                );
          }),
        )
        .toList();
    },

    GetLauncherConfigTypes: <T>(launchers: Launchers): List<string> => {
      return launchers.passthrough
        .valueSeq()
        .map((launcher) => launcher.configType)
        .toList();
    },
    GetLaucherFormTypes: <T>(
      launchers: Launchers,
      forms: object,
      apis: object,
    ): ValueOrErrors<List<string>, string> => {
      return ValueOrErrors.Operations.All(
        launchers.create
          .valueSeq()
          .map((launcher) =>
            SpecificationV2.Operations.TryFindForm(launcher.form, forms).Then(
              (form) => SpecificationV2.Operations.GetFormType(form),
            ),
          )
          .concat(
            launchers.edit
              .valueSeq()
              .map((launcher) =>
                SpecificationV2.Operations.TryFindForm(
                  launcher.form,
                  forms,
                ).Then((form) => SpecificationV2.Operations.GetFormType(form)),
              ),
          )
          .concat(
            launchers.passthrough
              .valueSeq()
              .map((launcher) =>
                SpecificationV2.Operations.TryFindForm(
                  launcher.form,
                  forms,
                ).Then((form) => SpecificationV2.Operations.GetFormType(form)),
              ),
          )
          .concat(
            launchers.passthrough
              .valueSeq()
              .map((launcher) =>
                ValueOrErrors.Default.return<string, string>(
                  launcher.configType,
                ),
              ),
          )
          .concat(
            SpecificationV2.Operations.GetCreateAndEditConfigEntityTypes(
              launchers,
              apis,
            ),
          )
          .toList(),
      );
    },
    Deserialize:
      <
        T extends DispatchInjectablesTypes<T>,
        Flags = Unit,
        CustomPresentationContext = Unit,
        ExtraContext = Unit,
      >(
        apiConverters: DispatchApiConverters<T>,
        concreteRenderers: ConcreteRenderers<
          T,
          Flags,
          CustomPresentationContext,
          ExtraContext
        >,
        injectedPrimitives?: DispatchInjectedPrimitives<T>,
      ) =>
      (
        serializedSpecificationV2s:
          | SerializedSpecificationV2
          | SerializedSpecificationV2[],
        launcherNames: string[],
      ): ValueOrErrors<SpecificationV2<T>, string> =>
        injectedPrimitives
          ?.keySeq()
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
          : !SpecificationV2.Operations.hasLaunchers(serializedSpecificationV2s)
            ? ValueOrErrors.Default.throwOne<SpecificationV2<T>, string>(
                "launchers are missing from the specificationV2",
              )
            : SpecificationV2.Operations.DeserializeLaunchers(
                launcherNames,
                serializedSpecificationV2s.launchers,
              )
                .Then((launchers) =>
                  !SpecificationV2.Operations.hasTypes(
                    serializedSpecificationV2s,
                  )
                    ? ValueOrErrors.Default.throwOne<
                        SpecificationV2<T>,
                        string
                      >("types are missing from the specificationV2")
                    : !SpecificationV2.Operations.hasForms(
                          serializedSpecificationV2s,
                        )
                      ? ValueOrErrors.Default.throwOne<
                          SpecificationV2<T>,
                          string
                        >("forms are missing from the specificationV2")
                      : !SpecificationV2.Operations.hasApis(
                            serializedSpecificationV2s,
                          )
                        ? ValueOrErrors.Default.throwOne<
                            SpecificationV2<T>,
                            string
                          >("apis are missing from the specificationV2")
                        : SpecificationV2.Operations.GetLaucherFormTypes(
                            launchers,
                            serializedSpecificationV2s.forms,
                            serializedSpecificationV2s.apis,
                          )
                            .Then((formTypes) =>
                              SpecificationV2.Operations.DeserializeSpecTypes(
                                formTypes,
                                serializedSpecificationV2s.types,
                                injectedPrimitives,
                              ),
                            )
                            .Then((allTypes) =>
                              SpecificationV2.Operations.DeserializeForms<
                                T,
                                Flags,
                                CustomPresentationContext,
                                ExtraContext
                              >(
                                serializedSpecificationV2s.forms,
                                allTypes,
                                concreteRenderers,
                                SpecificationV2.Operations.GetLauncherFormNames(
                                  launchers,
                                ),
                              ).Then((forms) =>
                                EnumApis.Operations.Deserialize(
                                  serializedSpecificationV2s.apis.enumOptions,
                                ).Then((enums) =>
                                  StreamApis.Operations.Deserialize(
                                    serializedSpecificationV2s.apis
                                      .searchableStreams,
                                  ).Then((streams) =>
                                    TableApis.Operations.Deserialize(
                                      concreteRenderers,
                                      allTypes,
                                      serializedSpecificationV2s.apis.tables,
                                      injectedPrimitives,
                                    ).Then((tables) =>
                                      LookupApis.Operations.Deserialize(
                                        serializedSpecificationV2s.apis.lookups,
                                      ).Then((lookups) => {
                                        let entities: Map<string, EntityApi> =
                                          Map();
                                        Object.entries(
                                          serializedSpecificationV2s.apis
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

                                        console.debug("forms", forms);

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
                            ),
                )
                .MapErrors((errors) =>
                  errors.map(
                    (error) =>
                      `${error}\n...When deserializing specificationV2`,
                  ),
                ),
  },
};

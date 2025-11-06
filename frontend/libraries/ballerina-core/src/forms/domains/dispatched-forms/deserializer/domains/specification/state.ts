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

export type SerializedSpecification = {
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

export type Specification<T> = {
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
    DeserializeSpecTypes: <T>(
      launcherTypes: List<DispatchTypeName>,
      serializedTypes: Record<string, SerializedType<T>>,
      serializedTypeNames: Set<DispatchTypeName>,
      injectedPrimitives?: DispatchInjectedPrimitives<T>,
    ): ValueOrErrors<Map<DispatchTypeName, DispatchParsedType<T>>, string> => {
      return ValueOrErrors.Operations.All(
        List<ValueOrErrors<[DispatchTypeName, DispatchParsedType<T>], string>>(
          launcherTypes
            .reduce((acc, rawTypeName) => {
              if (acc.has(rawTypeName)) {
                return acc;
              }
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
      desiredLaunchers: string[] | undefined,
      serializedLaunchers: Record<string, SerializedLauncher>,
    ): ValueOrErrors<Launchers, string> => {
      let launchers: Specification<T>["launchers"] = {
        create: Map<string, CreateLauncher>(),
        edit: Map<string, EditLauncher>(),
        passthrough: Map<string, PassthroughLauncher>(),
      };
      Object.entries(serializedLaunchers)
        .filter(([launcherName, _]) =>
          desiredLaunchers ? desiredLaunchers.includes(launcherName) : true,
        )
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
        .Then(([forms, accumulatedAlreadyParsedForms]) =>
          ValueOrErrors.Default.return<Map<string, Renderer<T>>, string>(
            accumulatedAlreadyParsedForms.concat(forms),
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
      const forms: string[] = [];
      launchers.create.forEach((launcher) => forms.push(launcher.form));
      launchers.edit.forEach((launcher) => forms.push(launcher.form));
      launchers.passthrough.forEach((launcher) => forms.push(launcher.form));
      return List(forms);
    },
    GetCreateAndEditConfigEntityTypes: <T>(
      launchers: Launchers,
      apis: object,
    ): List<ValueOrErrors<string, string>> => {
      const entities = Reflect.get(apis, "entities");
      const results: ValueOrErrors<string, string>[] = [];
      launchers.create.forEach((launcher) => {
        const entityType = Reflect.get(entities, launcher.configApi)?.type;
        results.push(
          entityType
            ? ValueOrErrors.Default.return<string, string>(entityType)
            : ValueOrErrors.Default.throwOne<string, string>(
                `entity type ${launcher.configApi} not found in entities`,
              ),
        );
      });
      launchers.edit.forEach((launcher) => {
        const entityType = Reflect.get(entities, launcher.configApi)?.type;
        results.push(
          entityType
            ? ValueOrErrors.Default.return<string, string>(entityType)
            : ValueOrErrors.Default.throwOne<string, string>(
                `entity type ${launcher.configApi} not found in entities`,
              ),
        );
      });
      return List(results);
    },

    GetLauncherConfigTypes: <T>(launchers: Launchers): List<string> => {
      return launchers.passthrough
        .map((launcher) => launcher.configType)
        .toList();
    },
    GetLaucherFormTypes: <T>(
      launchers: Launchers,
      forms: object,
      apis: object,
    ): ValueOrErrors<List<string>, string> => {
      const results: ValueOrErrors<string, string>[] = [];
      launchers.create.forEach((launcher) => {
        results.push(
          Specification.Operations.TryFindForm(launcher.form, forms).Then(
            (form) => Specification.Operations.GetFormType(form),
          ),
        );
      });
      launchers.edit.forEach((launcher) => {
        results.push(
          Specification.Operations.TryFindForm(launcher.form, forms).Then(
            (form) => Specification.Operations.GetFormType(form),
          ),
        );
      });
      launchers.passthrough.forEach((launcher) => {
        results.push(
          Specification.Operations.TryFindForm(launcher.form, forms).Then(
            (form) => Specification.Operations.GetFormType(form),
          ),
        );
        results.push(
          ValueOrErrors.Default.return<string, string>(launcher.configType),
        );
      });
      results.push(
        ...Specification.Operations.GetCreateAndEditConfigEntityTypes(
          launchers,
          apis,
        ).toArray(),
      );
      return ValueOrErrors.Operations.All(List(results));
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
          | SerializedSpecification
          | SerializedSpecification[],
        launcherNames?: string[],
      ): ValueOrErrors<Specification<T>, string> =>
        injectedPrimitives
          ?.keySeq()
          .some(
            (injectedPrimitiveName) =>
              !(injectedPrimitiveName in apiConverters),
          )
          ? ValueOrErrors.Default.throwOne(
              `the formsConfig does not contain an Api Converter for all injected primitives`,
            )
          : !Specification.Operations.hasLaunchers(serializedSpecificationV2s)
            ? ValueOrErrors.Default.throwOne<Specification<T>, string>(
                "launchers are missing from the specificationV2",
              )
            : Specification.Operations.DeserializeLaunchers(
                launcherNames,
                serializedSpecificationV2s.launchers,
              )
                .Then((launchers) =>
                  !Specification.Operations.hasTypes(serializedSpecificationV2s)
                    ? ValueOrErrors.Default.throwOne<Specification<T>, string>(
                        "types are missing from the specificationV2",
                      )
                    : !Specification.Operations.hasForms(
                          serializedSpecificationV2s,
                        )
                      ? ValueOrErrors.Default.throwOne<
                          Specification<T>,
                          string
                        >("forms are missing from the specificationV2")
                      : !Specification.Operations.hasApis(
                            serializedSpecificationV2s,
                          )
                        ? ValueOrErrors.Default.throwOne<
                            Specification<T>,
                            string
                          >("apis are missing from the specificationV2")
                        : Specification.Operations.GetLaucherFormTypes(
                            launchers,
                            serializedSpecificationV2s.forms,
                            serializedSpecificationV2s.apis,
                          ).Then((formTypes) =>
                            ValueOrErrors.Operations.Return(
                              Set(
                                Object.keys(serializedSpecificationV2s.types),
                              ),
                            ).Then((serializedTypeNames) =>
                              Specification.Operations.DeserializeSpecTypes(
                                formTypes,
                                serializedSpecificationV2s.types,
                                serializedTypeNames,
                                injectedPrimitives,
                              ).Then((allTypes) =>
                                Specification.Operations.DeserializeForms<
                                  T,
                                  Flags,
                                  CustomPresentationContext,
                                  ExtraContext
                                >(
                                  serializedSpecificationV2s.forms,
                                  allTypes,
                                  concreteRenderers,
                                  Specification.Operations.GetLauncherFormNames(
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
                                        serializedTypeNames,
                                        serializedSpecificationV2s.types,
                                        serializedSpecificationV2s.apis.tables,
                                        injectedPrimitives,
                                      ).Then((tables) =>
                                        LookupApis.Operations.Deserialize(
                                          serializedSpecificationV2s.apis
                                            .lookups,
                                        ).Then((lookups) => {
                                          const entityEntries: [
                                            string,
                                            EntityApi,
                                          ][] = Object.entries(
                                            serializedSpecificationV2s.apis
                                              .entities,
                                          ).map(
                                            ([entityApiName, entityApi]: [
                                              entiyApiName: string,
                                              entityApi: SerializedEntityApi,
                                            ]) => {
                                              const methods = entityApi.methods;
                                              return [
                                                entityApiName,
                                                {
                                                  type: entityApi.type,
                                                  methods: {
                                                    create: methods.includes(
                                                      "create",
                                                    ),
                                                    get: methods.includes("get"),
                                                    update: methods.includes(
                                                      "update",
                                                    ),
                                                    default: methods.includes(
                                                      "default",
                                                    ),
                                                  },
                                                },
                                              ] as [string, EntityApi];
                                            },
                                          );
                                          const entities: Map<
                                            string,
                                            EntityApi
                                          > = Map(entityEntries);

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

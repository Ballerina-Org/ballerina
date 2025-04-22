import { Set, Map, OrderedMap, List } from "immutable";
import {
  ApiConverters,
  BuiltIns,
  FieldName,
  FormsConfigMerger,
  InjectedPrimitives,
  isObject,
  ParsedType,
  RawFieldType,
  RawType,
  TypeName,
  PredicateFormLayout,
  FormLayout,
  PredicateValue,
  Unit,
  Updater,
  Delta,
  ValueOption,
  ValueRecord,
  DeltaOption,
  DeltaRecord,
  CollectionReference,
  EnumReference,
  Option,
  DeltaPrimitive,
  ValuePrimitive,
  unit,
  replaceWith,
  ValueTuple,
  DeltaTuple,
  ListRepo,
  DeltaUnion,
  ValueSum,
  Sum,
  DeltaSum,
} from "../../../../../../main";
import { ValueOrErrors } from "../../../../../collections/domains/valueOrErrors/state";
import { ParsedRenderer } from "../renderer/state";
import { fail } from "assert";

export type RawForm = {
  type?: any;
  fields?: any;
  tabs?: any;
  header?: any;
};
export const RawForm = {
  hasType: (_: any): _ is { type: any } => isObject(_) && "type" in _,
  hasFields: (_: any): _ is { fields: any } => isObject(_) && "fields" in _,
  hasTabs: (_: any): _ is { tabs: any } => isObject(_) && "tabs" in _,
  hasHeader: (_: any): _ is { header: any } => isObject(_) && "header" in _,
};
export type ParsedFormConfig<T> = {
  name: string;
  type: ParsedType<T>;
  fields: Map<FieldName, ParsedRenderer<T>>;
  tabs: PredicateFormLayout;
  header?: string;
};

export type BaseLauncher = {
  name: string;
  form: string;
};

export type CreateLauncher = {
  kind: "create";
  api: string;
  configApi: string;
} & BaseLauncher;

export type EditLauncher = {
  kind: "edit";
  api: string;
  configApi: string;
} & BaseLauncher;

export type PassthroughLauncher = {
  kind: "passthrough";
  configType: string;
} & BaseLauncher;

export type Launcher = CreateLauncher | EditLauncher | PassthroughLauncher;

export type RawEntityApi = {
  type?: any;
  methods?: any;
};
export type EntityApi = {
  type: TypeName;
  methods: { create: boolean; get: boolean; update: boolean; default: boolean };
};
export type GlobalConfigurationApi = {
  type: TypeName;
  methods: { get: boolean };
};

export type RawFormJSON = {
  types?: any;
  apis?: any;
  forms?: any;
  launchers?: any;
};
export const RawFormJSON = {
  hasTypes: (_: any): _ is { types: object } =>
    isObject(_) && "types" in _ && isObject(_.types),
  hasForms: (_: any): _ is { forms: object } =>
    isObject(_) && "forms" in _ && isObject(_.forms),
  hasApis: (
    _: any,
  ): _ is {
    apis: {
      enumOptions: object;
      searchableStreams: object;
      entities: { globalConfiguration: object };
      globalConfiguration: object;
    };
  } =>
    isObject(_) &&
    "apis" in _ &&
    isObject(_.apis) &&
    "enumOptions" in _.apis &&
    isObject(_.apis.enumOptions) &&
    "searchableStreams" in _.apis,
  hasLaunchers: (_: any): _ is { launchers: any } =>
    isObject(_) && "launchers" in _,
};
export type ParsedFormJSON<T> = {
  types: Map<TypeName, ParsedType<T>>;
  apis: {
    enums: Map<string, TypeName>;
    streams: Map<string, TypeName>;
    entities: Map<string, EntityApi>;
  };
  forms: Map<string, ParsedFormConfig<T>>;
  launchers: {
    create: Map<string, CreateLauncher>;
    edit: Map<string, EditLauncher>;
    passthrough: Map<string, PassthroughLauncher>;
  };
};

export type FormValidationError = string;

export type FormConfigValidationAndParseResult<T> = ValueOrErrors<
  ParsedFormJSON<T>,
  FormValidationError
>;

export const FormsConfig = {
  Default: {
    validateAndParseFormConfig:
      <T extends { [key in keyof T]: { type: any; state: any } }>(
        builtIns: BuiltIns,
        apiConverters: ApiConverters<T>,
        injectedPrimitives?: InjectedPrimitives<T>,
      ) =>
      (fc: any): FormConfigValidationAndParseResult<T> => {
        let errors: List<FormValidationError> = List();
        const formsConfig = Array.isArray(fc)
          ? FormsConfigMerger.Default.merge(fc)
          : fc;

        if (
          !RawFormJSON.hasTypes(formsConfig) ||
          !RawFormJSON.hasForms(formsConfig) ||
          !RawFormJSON.hasApis(formsConfig) ||
          !RawFormJSON.hasLaunchers(formsConfig)
        ) {
          return ValueOrErrors.Default.throw(
            List(["the formsConfig is missing required top level fields"]),
          );
        }

        // This error check must stay in the frontend, as it depends on injected types
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
          return ValueOrErrors.Default.throw(
            List([
              `the formsConfig does not contain an Api Converter for all injected primitives`,
            ]),
          );
        let keyOfTypes: Map<TypeName, RawType<T>> = Map();

        let parsedTypes: Map<TypeName, ParsedType<T>> = Map();
        const rawTypesFromConfig = formsConfig.types;
        const rawTypeNames = Set(Object.keys(rawTypesFromConfig));
        Object.entries(rawTypesFromConfig).forEach(([rawTypeName, rawType]) => {
          if (RawType.isKeyOf<T>(rawType)) {
            keyOfTypes = keyOfTypes.set(rawTypeName, rawType);
            return;
          }
          if (RawType.isExtendedType<T>(rawType)) {
            parsedTypes = parsedTypes.set(
              rawTypeName,
              ParsedType.Default.lookup(rawType.extends[0]),
            );
            return;
          }

          if (RawFieldType.isUnion(rawType)) {
            const parsingResult = ParsedType.Operations.ParseRawFieldType(
              rawTypeName,
              rawType,
              rawTypeNames,
              injectedPrimitives,
            );
            if (parsingResult.kind == "errors") {
              errors = errors.concat(parsingResult.errors.toArray());
              return;
            }
            parsedTypes = parsedTypes.set(rawTypeName, parsingResult.value);
            return;
          }

          if (!RawType.hasFields(rawType)) {
            errors = errors.push(
              `missing 'fields' in type ${rawTypeName}: expected object`,
            );
            return;
          }

          const parsedType: ParsedType<T> = {
            kind: "record",
            value: rawTypeName,
            fields: Map(),
          };
          Object.entries(rawType.fields).forEach(
            ([rawFieldName, rawFieldType]: [
              rawFieldName: any,
              rawFieldType: any,
            ]) => {
              if (
                RawFieldType.isMaybeLookup(rawFieldType) &&
                !RawFieldType.isPrimitive(rawFieldType, injectedPrimitives) &&
                (injectedPrimitives?.injectedPrimitives.has(
                  rawFieldType as keyof T,
                ) ||
                  builtIns.primitives.has(rawFieldType))
              ) {
                // This validation must be done at runtime, as we need to know the injectedPrimitives and field names
                errors = errors.push(
                  `field ${rawFieldName} in type ${rawTypeName}: fields, injectedPrimitive and builtIns cannot have the same name`,
                );
                return;
              }

              const parsedFieldType = ParsedType.Operations.ParseRawFieldType(
                rawFieldName,
                rawFieldType,
                rawTypeNames,
                injectedPrimitives,
              );
              if (parsedFieldType.kind == "errors") {
                errors = errors.concat(parsedFieldType.errors.toArray());
                return;
              }

              parsedType.fields = parsedType.fields.set(
                rawFieldName,
                parsedFieldType.value,
              );
            },
          );
          parsedTypes = parsedTypes.set(rawTypeName, parsedType);
        });

        const keyOfTypesResult = ValueOrErrors.Operations.All(
          List<ValueOrErrors<[string, ParsedType<T>], string>>(
            keyOfTypes.entrySeq().map(([rawTypeName, rawType]) => {
              return ParsedType.Operations.ParseRawKeyOf(
                rawTypeName,
                rawType,
                parsedTypes,
              ).Then((parsedType) =>
                ValueOrErrors.Default.return([rawTypeName, parsedType]),
              );
            }),
          ),
        );
        if (keyOfTypesResult.kind == "errors") {
          errors = errors.concat(keyOfTypesResult.errors.toArray());
          return ValueOrErrors.Default.throw(errors);
        }
        keyOfTypesResult.value.forEach(([rawTypeName, parsedType]) => {
          parsedTypes = parsedTypes.set(rawTypeName, parsedType);
        });

        let enums: Map<string, TypeName> = Map();
        Object.entries(formsConfig.apis.enumOptions).forEach(
          ([enumOptionName, enumOption]) =>
            (enums = enums.set(enumOptionName, enumOption)),
        );

        let streams: Map<string, TypeName> = Map();
        Object.entries(formsConfig.apis.searchableStreams).forEach(
          ([searchableStreamName, searchableStream]) =>
            (streams = streams.set(searchableStreamName, searchableStream)),
        );

        let entities: Map<string, EntityApi> = Map();
        Object.entries(formsConfig.apis.entities).forEach(
          ([entityApiName, entityApi]: [
            entiyApiName: string,
            entityApi: RawEntityApi,
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

        let forms: Map<string, ParsedFormConfig<T>> = Map();
        Object.entries(formsConfig.forms).forEach(
          ([formName, form]: [formName: string, form: RawForm]) => {
            if (
              !RawForm.hasType(form) ||
              !RawForm.hasFields(form) ||
              !RawForm.hasTabs(form)
            ) {
              errors = errors.push(
                `form ${formName} is missing the required type, fields or tabs attribute`,
              );
              return;
            }
            const formType = parsedTypes.get(form.type)!;
            if (formType.kind != "record") {
              errors = errors.push(
                `form ${formName} references non-record type ${form.type}`,
              );
              return;
            }

            const parsedForm: ParsedFormConfig<T> = {
              name: formName,
              fields: Map(),
              tabs: Map(),
              type: parsedTypes.get(form.type)!,
              header: RawForm.hasHeader(form) ? form.header : undefined,
            };

            Object.entries(form.fields).forEach(
              ([fieldName, field]: [fieldName: string, field: any]) => {
                const fieldType = formType.fields.get(fieldName)!;

                const bwcompatiblefield =
                  fieldType.kind == "application" &&
                  fieldType.value == "List" &&
                  typeof field.elementRenderer == "string"
                    ? {
                        renderer: field.renderer,
                        label: field?.label,
                        visible: field.visible,
                        disabled: field?.disabled,
                        description: field?.description,
                        elementRenderer: {
                          renderer: field.elementRenderer,
                          label: field?.elementLabel,
                          tooltip: field?.elementTooltip,
                          visible: field.visible,
                        },
                      }
                    : field;

                return (parsedForm.fields = parsedForm.fields.set(
                  fieldName,
                  ParsedRenderer.Operations.ParseRenderer(
                    fieldType,
                    bwcompatiblefield,
                    parsedTypes,
                  ),
                ));
              },
            );

            const parsedTabs = FormLayout.Operations.ParseLayout(
              form,
              formName,
            );
            if (parsedTabs.kind == "errors") {
              errors = errors.concat(parsedTabs.errors.toArray());
              return ValueOrErrors.Default.throw(errors);
            }
            parsedForm.tabs = parsedTabs.value;

            forms = forms.set(formName, parsedForm);
          },
        );

        let launchers: ParsedFormJSON<T>["launchers"] = {
          create: Map<string, CreateLauncher>(),
          edit: Map<string, EditLauncher>(),
          passthrough: Map<string, PassthroughLauncher>(),
        };

        Object.keys(formsConfig["launchers"]).forEach((launcherName: any) => {
          const launcher: Launcher =
            formsConfig.launchers[launcherName]["kind"] == "create" ||
            formsConfig.launchers[launcherName]["kind"] == "edit"
              ? {
                  name: launcherName,
                  kind: formsConfig.launchers[launcherName]["kind"],
                  form: formsConfig.launchers[launcherName]["form"],
                  api: formsConfig.launchers[launcherName]["api"],
                  configApi: formsConfig.launchers[launcherName]["configApi"],
                }
              : {
                  name: launcherName,
                  kind: formsConfig.launchers[launcherName]["kind"],
                  form: formsConfig.launchers[launcherName]["form"],
                  configType: formsConfig.launchers[launcherName]["configType"],
                };
          if (launcher.kind == "create")
            launchers.create = launchers.create.set(launcherName, launcher);
          else if (launcher.kind == "edit")
            launchers.edit = launchers.edit.set(launcherName, launcher);
          else if (launcher.kind == "passthrough")
            launchers.passthrough = launchers.passthrough.set(
              launcherName,
              launcher,
            );
        });

        if (errors.size > 0) {
          console.error("parsing errors");
          console.error(errors);
          return ValueOrErrors.Default.throw(errors);
        }

        console.debug({ parsedTypes: parsedTypes.toJSON(), fc });
        console.debug(
          "parsed types",
          `${JSON.stringify(parsedTypes.toJSON())}`,
        );

        return ValueOrErrors.Default.return({
          types: parsedTypes,
          forms,
          apis: {
            enums,
            streams,
            entities,
          },
          launchers,
        });
      },
  },
};

type FailingCheckApproval = (newApprovalValue: boolean) => {
  // function to call when clicking on the approval button
  OptimisticUpdate: Updater<PredicateValue>; // applied immediately in the FE
  BackendDeltaPATCH: Delta; // sent to the BE
};

type FailingCheckOp = {
  FailingCheck: PredicateValue;
  ToggleApproval: Option<FailingCheckApproval>;
};

type CollectFailingChecks<T = Unit> = (
  typesMap: Map<TypeName, ParsedType<T>>,
  t: ParsedType<T>,
) => (v: PredicateValue) => ValueOrErrors<
  // these are all fields somewhere in the root
  Array<FailingCheckOp>,
  [msg: string, _: any]
>;

const traverse: CollectFailingChecks = (typesMap, t) => {
  switch (t.kind) {
    case "unionCase":
      const traverseUnionCase = traverse(typesMap, t.fields);
      return (v: PredicateValue) =>
        !PredicateValue.Operations.IsUnionCase(v)
          ? ValueOrErrors.Default.throwOne(["not a UnionCase", v])
          : traverseUnionCase(v.fields).Map((valueFailingChecks) =>
              valueFailingChecks.map<FailingCheckOp>((fOp) => ({
                FailingCheck: fOp.FailingCheck,
                ToggleApproval: Option.Updaters.map2<
                  FailingCheckApproval,
                  FailingCheckApproval
                >((toggleApproval) => (a) => {
                  const innerApproval = toggleApproval(a);
                  return {
                    OptimisticUpdate: ValueOption.Updaters.value(
                      innerApproval.OptimisticUpdate,
                    ) as Updater<PredicateValue>,
                    BackendDeltaPATCH: {
                      kind: "UnionCase",
                      caseName: [v.caseName, innerApproval.BackendDeltaPATCH],
                    } as DeltaUnion,
                  };
                })(fOp.ToggleApproval),
              })),
            );

    case "lookup":
      const lookupType = typesMap.get(t.name)!;
      if (!lookupType) {
        return (_) =>
          ValueOrErrors.Default.throwOne([
            t.name,
            "cannot find lookup type name",
          ]);
      }

      const traverseLookupValue = traverse(typesMap, lookupType);
      return (v) =>
        !PredicateValue.Operations.IsVarLookup(v)
          ? ValueOrErrors.Default.throwOne(["not a ValueLookup", v])
          : v.varName === "FailingChecks"
          ? ValueOrErrors.Default.return([
              {
                FailingCheck: v,
                ToggleApproval: Option.Default.none<FailingCheckApproval>(),
              },
            ] as FailingCheckOp[])
          : traverseLookupValue(v);

    case "primitive":
      return (_) => ValueOrErrors.Default.return([]);

    case "option":
      const traverseOptionValue = traverse(typesMap, t.value);
      return (v: PredicateValue) =>
        !PredicateValue.Operations.IsOption(v)
          ? ValueOrErrors.Default.throwOne(["not a ValueOption", v])
          : !v.isSome
          ? ValueOrErrors.Default.return([])
          : traverseOptionValue(v.value).Map((valueFailingChecks) =>
              valueFailingChecks.map<FailingCheckOp>((fOp) => ({
                FailingCheck: fOp.FailingCheck,
                ToggleApproval: Option.Updaters.map2<
                  FailingCheckApproval,
                  FailingCheckApproval
                >((toggleApproval) => (a) => {
                  const innerApproval = toggleApproval(a);
                  return {
                    OptimisticUpdate: ValueOption.Updaters.value(
                      innerApproval.OptimisticUpdate,
                    ) as Updater<PredicateValue>,
                    BackendDeltaPATCH: {
                      kind: "OptionValue",
                      value: innerApproval.BackendDeltaPATCH,
                    } as DeltaOption,
                  };
                })(fOp.ToggleApproval),
              })),
            );
    case "record":
      const traverseRecordFields = t.fields.map((f) => traverse(typesMap, f));
      return (v: PredicateValue) =>
        !PredicateValue.Operations.IsRecord(v)
          ? ValueOrErrors.Default.throwOne(["not a ValueRecord", v])
          : ValueOrErrors.Operations.All(
              List(
                traverseRecordFields.entrySeq().map(([k, traverseField]) =>
                  traverseField(v.fields.get(k)!).Map((fieldFailingChecks) =>
                    fieldFailingChecks.map<FailingCheckOp>((fOp) => ({
                      FailingCheck: fOp.FailingCheck,
                      ToggleApproval: Option.Updaters.map2<
                        FailingCheckApproval,
                        FailingCheckApproval
                      >((toggleApproval) => (a) => {
                        const innerApproval = toggleApproval(a);
                        return {
                          OptimisticUpdate: ValueRecord.Updaters.set(
                            k,
                            innerApproval.OptimisticUpdate(v.fields.get(k)!),
                          ) as Updater<PredicateValue>,
                          BackendDeltaPATCH: {
                            kind: "RecordField",
                            field: [k, innerApproval.BackendDeltaPATCH],
                            recordType: t,
                          } as DeltaRecord,
                        };
                      })(fOp.ToggleApproval),
                    })),
                  ),
                ),
              ),
            ).Map(
              (listFailingChecks) =>
                listFailingChecks.flatten().toArray() as Array<FailingCheckOp>,
            );

    case "application":
      switch (t.value) {
        case "SingleSelection":
          const traverseSingleSelection = traverse(typesMap, t.args[0]);
          return (v) =>
            !PredicateValue.Operations.IsOption(v)
              ? ValueOrErrors.Default.throwOne(["not a ValueOption", v])
              : !v.isSome
              ? ValueOrErrors.Default.return([])
              : !CollectionReference.Operations.IsCollectionReference(
                  v.value,
                ) && !EnumReference.Operations.IsEnumReference(v.value)
              ? ValueOrErrors.Default.throwOne([
                  "not a CollectionReference or EnumReference",
                  v.value,
                ])
              : traverseSingleSelection(v.value).Map((valueFailingChecks) =>
                  valueFailingChecks.map<FailingCheckOp>((fOp) => ({
                    FailingCheck: fOp.FailingCheck,
                    ToggleApproval: Option.Updaters.map2<
                      FailingCheckApproval,
                      FailingCheckApproval
                    >((toggleApproval) => (a) => {
                      const innerApproval = toggleApproval(a);
                      return {
                        OptimisticUpdate: ValueOption.Updaters.value(
                          innerApproval.OptimisticUpdate,
                        ) as Updater<PredicateValue>,
                        BackendDeltaPATCH: {
                          kind: "OptionValue",
                          value: innerApproval.BackendDeltaPATCH,
                        } as DeltaOption,
                      };
                    })(fOp.ToggleApproval),
                  })),
                );

        case "MultiSelection":
          // multi selection only has 1 arg type, which is the same for all the selcted elements
          const traverseMultiSelectionField = traverse(typesMap, t.args[0]);
          return (v: PredicateValue) =>
            !PredicateValue.Operations.IsRecord(v)
              ? ValueOrErrors.Default.throwOne([
                  "not a ValueRecord (from MultiSelection)",
                  v,
                ])
              : ValueOrErrors.Operations.All(
                  List(
                    v.fields.entrySeq().map(([k, field]) =>
                      traverseMultiSelectionField(field).Map(
                        (fieldFailingChecks) =>
                          fieldFailingChecks.map<FailingCheckOp>((fOp) => ({
                            FailingCheck: fOp.FailingCheck,
                            ToggleApproval: Option.Updaters.map2<
                              FailingCheckApproval,
                              FailingCheckApproval
                            >((toggleApproval) => (a) => {
                              const innerApproval = toggleApproval(a);
                              return {
                                OptimisticUpdate: ValueRecord.Updaters.set(
                                  k,
                                  innerApproval.OptimisticUpdate(
                                    v.fields.get(k)!,
                                  ),
                                ) as Updater<PredicateValue>,
                                BackendDeltaPATCH: {
                                  kind: "RecordField",
                                  field: [k, innerApproval.BackendDeltaPATCH],
                                  recordType: t,
                                } as DeltaRecord,
                              };
                            })(fOp.ToggleApproval),
                          })),
                      ),
                    ),
                  ),
                ).Map(
                  (listFailingChecks) =>
                    listFailingChecks
                      .flatten()
                      .toArray() as Array<FailingCheckOp>,
                );

        case "Map":
          const traverseKey = traverse(typesMap, t.args[0]);
          const traverseValue = traverse(typesMap, t.args[1]);
          return (v: PredicateValue) =>
            !PredicateValue.Operations.IsRecord(v)
              ? ValueOrErrors.Default.throwOne([
                  "not a ValueRecord (from MultiSelection)",
                  v,
                ])
              : ValueOrErrors.Operations.All(
                  List(
                    v.fields.entrySeq().map(([k, field]) =>
                      ValueOrErrors.Operations.All(
                        List(
                          [traverseKey, traverseValue].map((traverseField) =>
                            traverseField(field).Map((fieldFailingChecks) =>
                              fieldFailingChecks.map<FailingCheckOp>((fOp) => ({
                                FailingCheck: fOp.FailingCheck,
                                ToggleApproval: Option.Updaters.map2<
                                  FailingCheckApproval,
                                  FailingCheckApproval
                                >((toggleApproval) => (a) => {
                                  const innerApproval = toggleApproval(a);
                                  return {
                                    OptimisticUpdate: ValueRecord.Updaters.set(
                                      k,
                                      innerApproval.OptimisticUpdate(
                                        v.fields.get(k)!,
                                      ),
                                    ) as Updater<PredicateValue>,
                                    BackendDeltaPATCH: {
                                      kind: "RecordField",
                                      field: [
                                        k,
                                        innerApproval.BackendDeltaPATCH,
                                      ],
                                      recordType: t,
                                    } as DeltaRecord,
                                  };
                                })(fOp.ToggleApproval),
                              })),
                            ),
                          ),
                        ),
                      ).Map(
                        (listFailingChecks) =>
                          listFailingChecks
                            .flatten()
                            .toArray() as Array<FailingCheckOp>,
                      ),
                    ),
                  ),
                ).Map(
                  (listFailingChecks) =>
                    listFailingChecks
                      .flatten()
                      .toArray() as Array<FailingCheckOp>,
                );
        case "Sum":
          return (v) =>
            !PredicateValue.Operations.IsSum(v)
              ? ValueOrErrors.Default.throwOne(["not a ValueSum", v])
              : (v.value.kind === "l"
                  ? traverse(typesMap, t.args[0])
                  : traverse(typesMap, t.args[1]))(v.value.value).Map(
                  (valueFailingChecks) =>
                    valueFailingChecks.map<FailingCheckOp>((fOp) => ({
                      FailingCheck: fOp.FailingCheck,
                      ToggleApproval: Option.Updaters.map2<
                        FailingCheckApproval,
                        FailingCheckApproval
                      >((toggleApproval) => (a) => {
                        const innerApproval = toggleApproval(a);
                        return {
                          OptimisticUpdate: ValueSum.Updaters.value(
                            Sum.Updaters.map2(
                              innerApproval.OptimisticUpdate,
                              innerApproval.OptimisticUpdate,
                            ),
                          ) as Updater<PredicateValue>,
                          BackendDeltaPATCH: {
                            kind: v.value.kind === "l" ? "SumLeft" : "SumRight",
                            value: innerApproval.BackendDeltaPATCH,
                          } as DeltaSum,
                        };
                      })(fOp.ToggleApproval),
                    })),
                );

        case "Option":
          const traverseOptionValue = traverse(typesMap, t.args[0]); // TODO: check this
          return (v: PredicateValue) =>
            !PredicateValue.Operations.IsOption(v)
              ? ValueOrErrors.Default.throwOne(["not a ValueOption", v])
              : !v.isSome
              ? ValueOrErrors.Default.return([])
              : traverseOptionValue(v.value).Map((valueFailingChecks) =>
                  valueFailingChecks.map<FailingCheckOp>((fOp) => ({
                    FailingCheck: fOp.FailingCheck,
                    ToggleApproval: Option.Updaters.map2<
                      FailingCheckApproval,
                      FailingCheckApproval
                    >((toggleApproval) => (a) => {
                      const innerApproval = toggleApproval(a);
                      return {
                        OptimisticUpdate: ValueOption.Updaters.value(
                          innerApproval.OptimisticUpdate,
                        ) as Updater<PredicateValue>,
                        BackendDeltaPATCH: {
                          kind: "OptionValue",
                          value: innerApproval.BackendDeltaPATCH,
                        } as DeltaOption,
                      };
                    })(fOp.ToggleApproval),
                  })),
                );
        case "Tuple":
          const traverseTupleFields = t.args.flatMap((f) =>
            traverse(typesMap, f),
          );
          return (v) =>
            !PredicateValue.Operations.IsTuple(v)
              ? ValueOrErrors.Default.throwOne(["not a ValueTuple", v])
              : (() => {
                  const approvalPredicateValueIndex = v.values.findIndex(
                    (_) =>
                      PredicateValue.Operations.IsVarLookup(_) &&
                      _.varName === "Approval",
                  );

                  return ValueOrErrors.Operations.All(
                    List(
                      traverseTupleFields.flatMap((traverseField, idx) =>
                        idx === approvalPredicateValueIndex
                          ? []
                          : traverseField(v.values.get(idx)!).Map(
                              (fieldFailingChecks) =>
                                fieldFailingChecks.map<FailingCheckOp>(
                                  (fOp) => ({
                                    FailingCheck: fOp.FailingCheck,
                                    ToggleApproval: Option.Updaters.map2<
                                      FailingCheckApproval,
                                      FailingCheckApproval
                                    >((toggleApproval) => (a) => {
                                      const innerApproval = toggleApproval(a);
                                      return {
                                        OptimisticUpdate:
                                          ValueTuple.Updaters.values(
                                            ListRepo.Updaters.update(
                                              idx,
                                              innerApproval.OptimisticUpdate,
                                            ).thenMany(
                                              approvalPredicateValueIndex < 0
                                                ? []
                                                : // this tuple contains an approval field
                                                  // -> we need to update it when approving this field
                                                  [
                                                    ListRepo.Updaters.update(
                                                      approvalPredicateValueIndex,
                                                      Updater<PredicateValue>(
                                                        (_) => a,
                                                      ),
                                                    ),
                                                  ],
                                            ),
                                          ) as Updater<PredicateValue>,
                                        BackendDeltaPATCH: {
                                          kind: "TupleCase",
                                          item: [
                                            idx,
                                            innerApproval.BackendDeltaPATCH,
                                          ],
                                          tupleType: t,
                                        } as DeltaTuple,
                                      };
                                    })(fOp.ToggleApproval),
                                  }),
                                ),
                            ),
                      ),
                    ),
                  ).Map(
                    (listFailingChecks) =>
                      listFailingChecks
                        .flatten()
                        .toArray() as Array<FailingCheckOp>,
                  );
                })();
        case "Union":
        case "KeyOf":
          return (_) => ValueOrErrors.Default.return([]);
        case "List":
          const traverseListField = traverse(typesMap, t.args[0]);
          return (v) =>
            !PredicateValue.Operations.IsTuple(v)
              ? ValueOrErrors.Default.throwOne(["not a ValueTuple", v])
              : (() => {
                  const approvalPredicateValueIndex = v.values.findIndex(
                    (_) =>
                      PredicateValue.Operations.IsVarLookup(_) &&
                      _.varName === "Approval",
                  );

                  return ValueOrErrors.Operations.All(
                    List(
                      v.values.map((v, idx) =>
                        traverseListField(v).Map((fieldFailingChecks) =>
                          fieldFailingChecks.map<FailingCheckOp>((fOp) => ({
                            FailingCheck: fOp.FailingCheck,
                            ToggleApproval: Option.Updaters.map2<
                              FailingCheckApproval,
                              FailingCheckApproval
                            >((toggleApproval) => (a) => {
                              const innerApproval = toggleApproval(a);
                              return {
                                OptimisticUpdate: ValueTuple.Updaters.values(
                                  ListRepo.Updaters.update(
                                    idx,
                                    innerApproval.OptimisticUpdate,
                                  ),
                                ) as Updater<PredicateValue>,
                                BackendDeltaPATCH: {
                                  kind: "TupleCase",
                                  item: [idx, innerApproval.BackendDeltaPATCH],
                                  tupleType: t,
                                } as DeltaTuple,
                              };
                            })(fOp.ToggleApproval),
                          })),
                        ),
                      ),
                    ),
                  ).Map(
                    (listFailingChecks) =>
                      listFailingChecks
                        .flatten()
                        .toArray() as Array<FailingCheckOp>,
                  );
                })();
        default:
          return (_) => ValueOrErrors.Default.return([]);
      }
    case "union":
      // return empty array?
      // and use the following in the application/union case?
      const traverseUnionFields = t.args.map((f) => traverse(typesMap, f));
      return (v) =>
        !PredicateValue.Operations.IsRecord(v)
          ? ValueOrErrors.Default.throwOne([
              "not a ValueRecord (from union)",
              v,
            ])
          : ValueOrErrors.Operations.All(
              List(
                traverseUnionFields.entrySeq().flatMap(([k, traverseField]) =>
                  !v.fields.has(k)
                    ? []
                    : [
                        traverseField(v.fields.get(k)!).Map(
                          (fieldFailingChecks) =>
                            fieldFailingChecks.map<FailingCheckOp>((fOp) => ({
                              FailingCheck: fOp.FailingCheck,
                              ToggleApproval: Option.Updaters.map2<
                                FailingCheckApproval,
                                FailingCheckApproval
                              >((toggleApproval) => (a) => {
                                const innerApproval = toggleApproval(a);
                                return {
                                  OptimisticUpdate: ValueRecord.Updaters.set(
                                    k,
                                    innerApproval.OptimisticUpdate(
                                      v.fields.get(k)!,
                                    ),
                                  ) as Updater<PredicateValue>,
                                  BackendDeltaPATCH: {
                                    kind: "RecordField",
                                    field: [k, innerApproval.BackendDeltaPATCH],
                                    recordType: t,
                                  } as DeltaRecord,
                                };
                              })(fOp.ToggleApproval),
                            })),
                        ),
                      ],
                ),
              ),
            ).Map(
              (listFailingChecks) =>
                listFailingChecks.flatten().toArray() as Array<FailingCheckOp>,
            );
    default:
      return (_) => ValueOrErrors.Default.throwOne(["unknown type", t]);
  }
};

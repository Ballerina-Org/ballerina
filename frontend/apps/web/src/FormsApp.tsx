import { useEffect, useState } from "react";
import "./App.css";
import {
  unit,
  FormParsingResult,
  FormsParserState,
  FormRunnerState,
  FormsParserTemplate,
  PromiseRepo,
  FormRunnerTemplate,
  Sum,
  PredicateValue,
  replaceWith,
  Updater,
  DeltaTransfer,
  Delta,
  DeltaCustom,
  ValueOrErrors,
  ParsedType,
  ValueRecord,
} from "ballerina-core";
import { List, Set, Map, OrderedMap } from "immutable";
// import { PersonView } from "./domains/person/views/main-view";
import {
  PersonContainerFormView,
  PersonNestedContainerFormView,
  CreatePersonSubmitButtonWrapper,
  EditPersonSubmitButtonWrapper,
} from "./domains/person/domains/from-config/views/wrappers";
import { PersonFromConfigApis, Person } from "playground-core";
import { PersonFieldViews } from "./domains/person-from-config/views/field-views";
// import { PersonForm } from "./domains/person/template";
import { fieldTypeConverters } from "./domains/person/apis/field-converters";
import {
  categoryForm,
  CategoryState,
  PersonFormInjectedTypes,
} from "./domains/person-from-config/injected-forms/category";
import PersonConfig from "../public/SampleSpecs/person-config.json";
import { PassthroughFormContainerWrapper } from "./domains/passthrough-forms/views/wrappers";
import { UsersSetupFromConfigApis } from "playground-core";
const ShowFormsParsingErrors = (parsedFormsConfig: FormParsingResult) => (
  <div style={{ border: "red" }}>
    {parsedFormsConfig.kind == "r" && JSON.stringify(parsedFormsConfig.value)}
  </div>
);

const InstantiedPersonFormsParserTemplate =
  FormsParserTemplate<PersonFormInjectedTypes>();

export const FormsApp = (props: {}) => {
  const [configFormsParser, setConfigFormsParser] = useState(
    FormsParserState.Default(),
  );
  const [formToShow, setFormToShow] = useState(1);
  const numForms = 3;
  const [personCreateFormState, setPersonCreateFormState] = useState(
    FormRunnerState.Default(),
  );
  const [personEditFormState, setPersonEditFormState] = useState(
    FormRunnerState.Default(),
  );
  const [personPassthroughFormState, setPersonPassthroughFormState] = useState(
    FormRunnerState.Default(),
  );
  const [personAddressConfigFormState, setPersonAddressConfigFormState] =
    useState(FormRunnerState.Default());
  const [usersSetupFormState, setUsersSetupFormState] = useState(
    FormRunnerState.Default(),
  );
  const [usersSetupConfigFormState, setUsersSetupConfigFormState] = useState(
    FormRunnerState.Default(),
  );
  const [usersSetupEntity, setUsersSetupEntity] = useState<
    Sum<PredicateValue, "not initialized">
  >(Sum.Default.right("not initialized"));

  const [usersSetupConfigEntity, setUsersSetupConfigEntity] = useState<
    Sum<PredicateValue, "not initialized">
  >(Sum.Default.right("not initialized"));
  const [personState, setPersonState] = useState(Person.Default.mocked());
  const [formErrors, setFormErrors] = useState<List<string>>(List());
  const [formSuccess, setFormSuccess] = useState(false);
  // const [personFormState, setPersonFormState] = useState(PersonFormState.Default(""))
  // const [personConfigState, setPersonConfigState] = useState(PersonConfig.Default())

  const [renderParserState, renderForms] = [true, true];
  const logState = true;

  if (
    personCreateFormState.form.kind == "l" &&
    personCreateFormState.form.value.entity.sync.kind == "loaded"
  ) {
    console.log(
      "entity",
      personCreateFormState.form.value.entity.sync.value.fields.toJS(),
    );
    console.log(
      "visibilities",
      personCreateFormState.form.value.customFormState.predicateEvaluations.value.visiblityPredicateEvaluations.fields.toJS(),
    );
  }

  if (
    personEditFormState.form.kind == "l" &&
    personEditFormState.form.value.entity.sync.kind == "loaded"
  ) {
    console.log(
      "entity",
      personEditFormState.form.value.entity.sync.value.fields.toJS(),
    );
    console.log(
      "visibilities",
      personEditFormState.form.value.customFormState.predicateEvaluations.value.visiblityPredicateEvaluations.fields.toJS(),
    );
  }

  logState && formToShow % numForms == 0
    ? console.log({
        parser: configFormsParser,
        runner: personCreateFormState,
      })
    : logState && formToShow % numForms == 1
      ? console.log({
          parser: configFormsParser,
          runner: personEditFormState,
        })
      : logState && formToShow % numForms == 2
        ? console.log({
            parser: configFormsParser,
            runner: personPassthroughFormState,
          })
        : logState && formToShow % numForms == 3
          ? console.log({
              parser: configFormsParser,
              usersSetupFormState: usersSetupFormState,
              usersSetupConfigFormState: usersSetupConfigFormState,
              usersSetupEntity:
                usersSetupEntity.kind == "l"
                  ? (usersSetupEntity.value as ValueRecord).fields.toJS()
                  : null,
              usersSetupConfigEntity:
                usersSetupConfigEntity.kind == "l"
                  ? (usersSetupConfigEntity.value as ValueRecord).fields.toJS()
                  : null,
            })
          : undefined;

  // Passthrough form only -- Person
  const [personEntity, setPersonEntity] = useState<
    Sum<PredicateValue, "not initialized">
  >(Sum.Default.right("not initialized"));
  const [globalConfiguration, setGlobalConfiguration] = useState<
    Sum<PredicateValue, "not initialized">
  >(Sum.Default.right("not initialized"));
  const [entityPath, setEntityPath] = useState<any>(null);

  const parseCustomDelta =
    <T,>(
      toRawObject: (
        value: PredicateValue,
        state: any,
        type: ParsedType<any>,
      ) => ValueOrErrors<any, string>,
      fromDelta: (delta: Delta) => ValueOrErrors<DeltaTransfer<T>, string>,
    ) =>
    (deltaCustom: DeltaCustom): ValueOrErrors<[T, string], string> => {
      if (deltaCustom.value.kind == "CategoryReplace") {
        return toRawObject(
          deltaCustom.value.replace,
          deltaCustom.value.state,
          deltaCustom.value.type,
        ).Then((value) => {
          return ValueOrErrors.Default.return([
            {
              kind: "CategoryReplace",
              replace: value,
            },
            "[CategoryReplace]",
          ] as [T, string]);
        });
      }
      return ValueOrErrors.Default.throwOne(
        `Unsupported delta kind: ${deltaCustom.value.kind}`,
      );
    };

  const onPersonEntityChange = (updater: Updater<any>, delta: Delta): void => {
    if (personPassthroughFormState.form.kind == "r") return;
    setTimeout(() => {}, 500);
    const newEntity = updater(personEntity.value);
    console.log("patching entity", newEntity);
    setPersonEntity(replaceWith(Sum.Default.left(newEntity)));
    if (
      configFormsParser.formsConfig.sync.kind == "loaded" &&
      configFormsParser.formsConfig.sync.value.kind == "l"
    ) {
      const toApiRawParser =
        configFormsParser.formsConfig.sync.value.value.passthrough.get(
          "person-transparent",
        )!().toApiParser;
      setEntityPath(
        DeltaTransfer.Default.FromDelta(
          toApiRawParser,
          parseCustomDelta,
        )(delta),
      );
    }
  };

  const onAddressFieldsChange = (updater: Updater<any>, delta: Delta): void => {
    if (personPassthroughFormState.form.kind == "r") return;
    setTimeout(() => {}, 500);
    const newEntity = updater(globalConfiguration.value);
    console.log("patching entity", newEntity);
    setGlobalConfiguration(replaceWith(Sum.Default.left(newEntity)));
    if (
      configFormsParser.formsConfig.sync.kind == "loaded" &&
      configFormsParser.formsConfig.sync.value.kind == "l"
    ) {
      const toApiRawParser =
        configFormsParser.formsConfig.sync.value.value.passthrough.get(
          "addresses-config",
        )!().toApiParser;
      setEntityPath(
        DeltaTransfer.Default.FromDelta(
          toApiRawParser,
          parseCustomDelta,
        )(delta),
      );
    }
  };

  useEffect(() => {
    if (formToShow % numForms == 2) {
      PersonFromConfigApis.entityApis
        .get("person")("")
        .then((raw) => {
          if (
            configFormsParser.formsConfig.sync.kind == "loaded" &&
            configFormsParser.formsConfig.sync.value.kind == "l"
          ) {
            const parsed =
              configFormsParser.formsConfig.sync.value.value.passthrough.get(
                "person-transparent",
              )!().fromApiParser(raw);
            if (parsed.kind == "errors") {
              console.error(parsed.errors);
            } else {
              setPersonEntity(Sum.Default.left(parsed.value));
            }
          }
        });
      PersonFromConfigApis.entityApis
        .get("globalConfiguration")("")
        .then((raw) => {
          if (
            configFormsParser.formsConfig.sync.kind == "loaded" &&
            configFormsParser.formsConfig.sync.value.kind == "l"
          ) {
            const parsed =
              configFormsParser.formsConfig.sync.value.value.passthrough.get(
                "person-transparent",
              )!().parseGlobalConfiguration(raw);
            if (parsed.kind == "errors") {
              console.error(parsed.errors);
            } else {
              setGlobalConfiguration(Sum.Default.left(parsed.value));
            }
          }
        });
    }
  }, [personPassthroughFormState.form.kind, formToShow]);

  const onUsersSetupConfigEntityChange = (
    updater: Updater<any>,
    delta: Delta,
  ): void => {
    if (usersSetupConfigFormState.form.kind == "r") return;
    setTimeout(() => {}, 500);
    const newEntity = updater(usersSetupConfigEntity.value);
    console.log("patching entity", newEntity);
    setUsersSetupConfigEntity(replaceWith(Sum.Default.left(newEntity)));
    if (
      configFormsParser.formsConfig.sync.kind == "loaded" &&
      configFormsParser.formsConfig.sync.value.kind == "l"
    ) {
      const toApiRawParser =
        configFormsParser.formsConfig.sync.value.value.passthrough.get(
          "UsersSetupConfig",
        )!().toApiParser;
      setEntityPath(
        DeltaTransfer.Default.FromDelta(
          toApiRawParser,
          parseCustomDelta,
        )(delta),
      );
    }
  };

  // Passthrough form only -- UsersSetup
  useEffect(() => {
    if (formToShow % numForms == 3) {
      UsersSetupFromConfigApis.entityApis
        .get("UsersSetupApi")("")
        .then((raw) => {
          if (
            configFormsParser.formsConfig.sync.kind == "loaded" &&
            configFormsParser.formsConfig.sync.value.kind == "l"
          ) {
            const parsed =
              configFormsParser.formsConfig.sync.value.value.passthrough.get(
                "UsersSetup",
              )!().fromApiParser(raw);
            if (parsed.kind == "errors") {
              console.error(parsed.errors);
            } else {
              setUsersSetupEntity(Sum.Default.left(parsed.value));
            }
          }
        });
      UsersSetupFromConfigApis.entityApis
        .get("UsersSetupConfigApi")("")
        .then((raw) => {
          if (
            configFormsParser.formsConfig.sync.kind == "loaded" &&
            configFormsParser.formsConfig.sync.value.kind == "l"
          ) {
            const parsed =
              configFormsParser.formsConfig.sync.value.value.passthrough.get(
                "UsersSetupConfig",
              )!().fromApiParser(raw);
            if (parsed.kind == "errors") {
              console.error(parsed.errors);
            } else {
              setUsersSetupConfigEntity(Sum.Default.left(parsed.value));
            }
          }
        });
    }
  }, [
    usersSetupFormState.form.kind,
    usersSetupConfigFormState.form.kind,
    formToShow,
  ]);

  if (
    configFormsParser.formsConfig.sync.kind == "loaded" &&
    configFormsParser.formsConfig.sync.value.kind == "r"
  ) {
    return (
      <ol>
        {configFormsParser.formsConfig.sync.value.value.map((_: string) => (
          <li>{_}</li>
        ))}
      </ol>
    );
  }

  return (
    <div className="App">
      <h1>Ballerina 🩰</h1>
      <div className="card">
        <table>
          <tbody>
            <tr>
              <td>
                {/* { JSON.stringify(personFormState.address.elementFormStates.toArray()) } */}
                {/* <PersonForm
									context={{
										...personFormState,
										value: personState,
										formState: personFormState,
										person: personState,
										columns: [["name", "surname", "gender", "birthday"],
										["subscribeToNewsletter", "interests"],
										["departments", "address"]],
										visibleFields: Person.Operations.VisibleFields,
										disabledFields: Person.Operations.VisibleFields,
										flags: Set(["BC"]),
										showAllErrors: false,
									}}
									setState={_ => setPersonFormState(_)}
									view={PersonView}
									foreignMutations={{
										onChange: (_, path) => {
											setPersonState(_)
											console.log(path.toArray())
										}
									}}
								/> */}
                {/* {JSON.stringify(personConfigState)} */}
              </td>
            </tr>
            <tr>
              <td>
                {renderParserState && JSON.stringify(configFormsParser)}
                <button
                  onClick={() => {
                    setFormErrors(List());
                    setFormSuccess(false);
                    setFormToShow(formToShow + 1);
                  }}
                >
                  Show next form
                </button>

                <InstantiedPersonFormsParserTemplate
                  context={{
                    ...configFormsParser,
                    containerFormView: PersonContainerFormView,
                    fieldTypeConverters: fieldTypeConverters,
                    nestedContainerFormView: PersonNestedContainerFormView,
                    fieldViews: PersonFieldViews,
                    infiniteStreamSources: PersonFromConfigApis.streamApis,
                    enumOptionsSources: PersonFromConfigApis.enumApis,
                    entityApis: PersonFromConfigApis.entityApis,
                    getFormsConfig: () =>
                      PromiseRepo.Default.mock(() => PersonConfig),
                    injectedPrimitives: Map([
                      [
                        "injectedCategory",
                        {
                          fieldView: categoryForm,
                          defaultValue: {
                            kind: "custom",
                            value: {
                              kind: "adult",
                              extraSpecial: false,
                            },
                          },
                          defaultState: CategoryState.Default(),
                        },
                      ],
                    ]),
                  }}
                  setState={setConfigFormsParser}
                  view={unit}
                  foreignMutations={unit}
                />
                {renderForms && formToShow % numForms == 0 ? (
                  <>
                    <h3>Create person</h3>
                    {formErrors.size > 0 && (
                      <div style={{ border: "2px solid red" }}>
                        <p style={{ color: "red" }}>Errors</p>
                        <ul>
                          {formErrors.map((_, i) => (
                            <li key={i}>{_}</li>
                          ))}
                        </ul>
                      </div>
                    )}
                    {formSuccess && (
                      <div style={{ border: "2px solid green" }}>
                        Form successfully submitted
                      </div>
                    )}
                    <FormRunnerTemplate
                      context={{
                        ...configFormsParser,
                        ...personCreateFormState,
                        formRef: {
                          formName: "create-person",
                          kind: "create",
                          submitButtonWrapper: CreatePersonSubmitButtonWrapper,
                          apiHandlers: {
                            onDefaultSuccess: (_) => {
                              console.log(
                                `Success getting default person ${JSON.stringify(
                                  _,
                                )}`,
                              );
                            },
                            onDefaultError: (_) => {
                              setFormSuccess(false);
                              setFormErrors(
                                List(["Error getting default person"]),
                              );
                              console.log(
                                `Error getting default person ${JSON.stringify(
                                  _,
                                )}`,
                              );
                            },
                            onCreateError: (_) => {
                              console.log(_);
                              setFormSuccess(false);
                              setFormErrors(List(["Error creating person"]));
                              console.log(
                                `Error submitting new person ${JSON.stringify(
                                  _,
                                )}`,
                              );
                            },
                            onCreateSuccess: (_) => {
                              setFormSuccess(true);
                              setFormErrors(List());
                              console.log(
                                `Success creating person ${JSON.stringify(
                                  _.entity.sync.value,
                                )}`,
                              );
                            },
                          },
                        },
                        showFormParsingErrors: ShowFormsParsingErrors,
                        extraContext: {
                          flags: Set(["BC", "X"]),
                        },
                      }}
                      setState={setPersonCreateFormState}
                      view={unit}
                      foreignMutations={unit}
                    />
                  </>
                ) : renderForms && formToShow % numForms == 1 ? (
                  <>
                    <h3>Edit person</h3>
                    {formErrors.size > 0 && (
                      <div style={{ border: "2px solid red" }}>
                        <p style={{ color: "red" }}>Errors</p>
                        <ul>
                          {formErrors.map((_, i) => (
                            <li key={i}>{_}</li>
                          ))}
                        </ul>
                      </div>
                    )}
                    {formSuccess && (
                      <div style={{ border: "2px solid green" }}>
                        Form successfully submitted
                      </div>
                    )}

                    <FormRunnerTemplate
                      context={{
                        ...configFormsParser,
                        ...personEditFormState,
                        formRef: {
                          formName: "edit-person",
                          entityId: "abcd-1234",
                          kind: "edit",
                          submitButtonWrapper: EditPersonSubmitButtonWrapper,
                          apiHandlers: {
                            onGetError: (_) => {
                              setFormSuccess(false);
                              setFormErrors(List(["Error getting person"]));
                              console.log(
                                `Error getting person ${JSON.stringify(_)}`,
                              );
                            },
                            onGetSuccess: (_) => {
                              console.log(
                                `Success getting person ${JSON.stringify(_)}`,
                              );
                            },
                            onUpdateError: (_) => {
                              console.log(_);
                              setFormSuccess(false);
                              setFormErrors(List(["Error updating person"]));
                              console.log({ type: "error", msg: _ });
                            },
                            onUpdateSuccess: (_) => {
                              setFormSuccess(true);
                              setFormErrors(List());
                              console.log({ type: "success", data: _ });
                            },
                          },
                        },
                        showFormParsingErrors: ShowFormsParsingErrors,
                        extraContext: {
                          flags: Set(["BC", "X"]),
                        },
                      }}
                      setState={setPersonEditFormState}
                      view={unit}
                      foreignMutations={unit}
                    />
                  </>
                ) : renderForms && formToShow % numForms == 2 ? (
                  <>
                    <h3>Passthrough form</h3>

                    {entityPath && entityPath.kind == "value" && (
                      <pre
                        style={{
                          display: "inline-block",
                          verticalAlign: "top",
                          textAlign: "left",
                        }}
                      >
                        {JSON.stringify(entityPath.value, null, 2)}
                      </pre>
                    )}
                    {entityPath && entityPath.kind == "errors" && (
                      <p>
                        DeltaErrors:{" "}
                        {JSON.stringify(entityPath.errors, null, 2)}
                      </p>
                    )}
                    {globalConfiguration.kind == "l" && (
                      <div
                        style={{
                          border: "2px solid lightblue",
                          display: "inline-block",
                          verticalAlign: "top",
                        }}
                      >
                        <p>Addresses config</p>
                        <FormRunnerTemplate
                          context={{
                            ...configFormsParser,
                            ...personAddressConfigFormState,
                            formRef: {
                              formName: "addresses-config",
                              kind: "passthrough",
                              containerWrapper: PassthroughFormContainerWrapper,
                              entity: Sum.Default.left(
                                PredicateValue.Default.record(
                                  OrderedMap([
                                    [
                                      "ActiveFields",
                                      (
                                        globalConfiguration.value as ValueRecord
                                      ).fields.get("ActiveFields")!,
                                    ],
                                  ]),
                                ),
                              ),
                              globalConfiguration,
                              onEntityChange: onAddressFieldsChange,
                            },
                            showFormParsingErrors: ShowFormsParsingErrors,
                            extraContext: {
                              flags: Set(["BC", "X"]),
                            },
                          }}
                          setState={setPersonAddressConfigFormState}
                          view={unit}
                          foreignMutations={unit}
                        />
                      </div>
                    )}
                    <FormRunnerTemplate
                      context={{
                        ...configFormsParser,
                        ...personPassthroughFormState,
                        formRef: {
                          formName: "person-transparent",
                          kind: "passthrough",
                          containerWrapper: PassthroughFormContainerWrapper,
                          entity: personEntity,
                          globalConfiguration,
                          onEntityChange: onPersonEntityChange,
                        },
                        showFormParsingErrors: ShowFormsParsingErrors,
                        extraContext: {
                          flags: Set(["BC", "X"]),
                        },
                      }}
                      setState={setPersonPassthroughFormState}
                      view={unit}
                      foreignMutations={unit}
                    />
                  </>
                ) : undefined}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  );
};

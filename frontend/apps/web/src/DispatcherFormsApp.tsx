import { useEffect, useState } from "react";
import "./App.css";
import {
  unit,
  PromiseRepo,
  Sum,
  PredicateValue,
  replaceWith,
  Updater,
  DeltaTransfer,
  ValueOrErrors,
  DispatchFormsParserTemplate,
  DispatchFormsParserState,
  DispatchFormRunnerTemplate,
  DispatchDeltaTransfer,
  DispatchDeltaCustom,
  DispatchDelta,
  DispatchSpecificationDeserializationResult,
  DispatchFormRunnerState,
  DispatchParsedType,
  ValueRecord,
} from "ballerina-core";
import { Set, Map, OrderedMap } from "immutable";
import {
  PersonContainerFormView,
  PersonNestedContainerFormView,
} from "./domains/person/domains/from-config/views/wrappers";
import {
  DispatchPersonFromConfigApis,
  PersonFromConfigApis,
} from "playground-core";
import { PersonFieldViews } from "./domains/person-from-config/views/field-views";
import { fieldTypeConverters } from "./domains/person/apis/field-converters";
import {
  categoryForm,
  CategoryState,
  PersonFormInjectedTypes,
} from "./domains/person-from-config/injected-forms/category";
// import SPEC from "../../../../backend/apps/automatic-tests/input-forms/simple-union-example-lookups.json";
import SPEC from "../../../../backend/apps/ballerina-runtime/input-forms/person-config.json";
import {
  DispatchPersonContainerFormView,
  DispatchPersonNestedContainerFormView,
} from "./domains/dispatched-passthrough-form/views/wrappers";
import {
  dispatchCategoryForm,
  DispatchCategoryForm,
  DispatchCategoryState,
} from "./domains/dispatched-passthrough-form/injected-forms/category";
import { PersonConcreteRenderers } from "./domains/dispatched-passthrough-form/views/field-views";
import { DispatchFieldTypeConverters } from "./domains/dispatched-passthrough-form/apis/field-converters";

const ShowFormsParsingErrors = (
  parsedFormsConfig: DispatchSpecificationDeserializationResult<PersonFormInjectedTypes>,
) => (
  <div style={{ border: "red" }}>
    {parsedFormsConfig.kind == "errors" &&
      JSON.stringify(parsedFormsConfig.errors)}
  </div>
);

const InstantiedPersonFormsParserTemplate =
  DispatchFormsParserTemplate<PersonFormInjectedTypes>();

const InstantiedPersonDispatchFormRunnerTemplate =
  DispatchFormRunnerTemplate<PersonFormInjectedTypes>();

export const DispatcherFormsApp = (props: {}) => {
  const [specificationDeserializer, setSpecificationDeserializer] = useState(
    DispatchFormsParserState<PersonFormInjectedTypes>().Default(),
  );

  const [personPassthroughFormState, setPersonPassthroughFormState] = useState(
    DispatchFormRunnerState<PersonFormInjectedTypes>().Default(),
  );
  const [personAddressConfigFormState, setPersonAddressConfigFormState] =
    useState(DispatchFormRunnerState<PersonFormInjectedTypes>().Default());

  const [personEntity, setPersonEntity] = useState<
    Sum<ValueOrErrors<PredicateValue, string>, "not initialized">
  >(Sum.Default.right("not initialized"));
  const [config, setConfig] = useState<
    Sum<ValueOrErrors<PredicateValue, string>, "not initialized">
  >(Sum.Default.right("not initialized"));

  // TODO replace with delta transfer
  const [entityPath, setEntityPath] = useState<any>(null);

  const parseCustomDelta =
    <T,>(
      toRawObject: (
        value: PredicateValue,
        type: DispatchParsedType<T>,
        state: any,
      ) => ValueOrErrors<any, string>,
      fromDelta: (
        delta: DispatchDelta,
      ) => ValueOrErrors<DeltaTransfer<T>, string>,
    ) =>
    (deltaCustom: DispatchDeltaCustom): ValueOrErrors<[T, string], string> => {
      if (deltaCustom.value.kind == "CategoryReplace") {
        return toRawObject(
          deltaCustom.value.replace,
          deltaCustom.value.type,
          deltaCustom.value.state,
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
  const onPersonEntityChange = (
    updater: Updater<any>,
    delta: DispatchDelta,
  ): void => {
    if (personEntity.kind == "r" || personEntity.value.kind == "errors") {
      return;
    }

    const newEntity = updater(personEntity.value.value);
    console.log("patching entity", newEntity);
    setPersonEntity(
      replaceWith(Sum.Default.left(ValueOrErrors.Default.return(newEntity))),
    );
    if (
      specificationDeserializer.deserializedSpecification.sync.kind ==
        "loaded" &&
      specificationDeserializer.deserializedSpecification.sync.value.kind ==
        "value"
    ) {
      const toApiRawParser =
        specificationDeserializer.deserializedSpecification.sync.value.value.launchers.passthrough.get(
          "person-transparent",
        )!.parseValueToApi;
      setEntityPath(
        DispatchDeltaTransfer.Default.FromDelta(
          toApiRawParser as any, //TODO - fix type issue if worth it
          parseCustomDelta,
        )(delta),
      );
    }
  };

  // const onAddressFieldsChange = (
  //   updater: Updater<any>,
  //   delta: DispatchDelta,
  // ): void => {
  //   if (globalConfiguration.kind == "r" || globalConfiguration.value.kind == "errors") {
  //     return;
  //   }
  //   const newEntity = updater(globalConfiguration.value);
  //   console.log("patching entity", newEntity);
  //   setGlobalConfiguration(replaceWith(Sum.Default.left(newEntity)));
  //   if (
  //     specificationDeserializer.deserializedSpecification.sync.kind ==
  //       "loaded" &&
  //     specificationDeserializer.deserializedSpecification.sync.value.kind ==
  //       "value"
  //   ) {
  //     const toApiRawParser =
  //       specificationDeserializer.deserializedSpecification.sync.value.value.launchers.passthrough.get(
  //         "addresses-config",
  //       )!.parseEntityFromApi;
  //     setEntityPath(
  //       DispatchDeltaTransfer.Default.FromDelta(
  //         toApiRawParser,
  //         parseCustomDelta,
  //       )(delta),
  //     );
  //   }
  // };

  useEffect(() => {
    DispatchPersonFromConfigApis.entityApis
      .get("person")("")
      .then((raw) => {
        if (
          specificationDeserializer.deserializedSpecification.sync.kind ==
            "loaded" &&
          specificationDeserializer.deserializedSpecification.sync.value.kind ==
            "value"
        ) {
          const parsed =
            specificationDeserializer.deserializedSpecification.sync.value.value.launchers.passthrough
              .get("person-transparent")!
              .parseEntityFromApi(raw);
          if (parsed.kind == "errors") {
            console.error("parsed entity errors", parsed.errors);
          } else {
            setPersonEntity(Sum.Default.left(parsed));
          }
        }
      });
    DispatchPersonFromConfigApis.entityApis
      .get("globalConfiguration")("")
      .then((raw) => {
        if (
          specificationDeserializer.deserializedSpecification.sync.kind ==
            "loaded" &&
          specificationDeserializer.deserializedSpecification.sync.value.kind ==
            "value"
        ) {
          const parsed =
            specificationDeserializer.deserializedSpecification.sync.value.value.launchers.passthrough
              .get("person-transparent")!
              .parseGlobalConfigurationFromApi(raw);
          if (parsed.kind == "errors") {
            console.error("parsed global configuration errors", parsed.errors);
          } else {
            setConfig(Sum.Default.left(parsed));
          }
        }
      });
  }, [specificationDeserializer.deserializedSpecification.sync.kind]);

  // console.debug("formParser", JSON.stringify(specificationDeserializer, null, 2));
  // console.debug("personPassthroughFormState", JSON.stringify(personPassthroughFormState, null, 2));
  // console.debug("personEntity", JSON.stringify(personEntity, null, 2));
  // console.debug("globalConfiguration", JSON.stringify(globalConfiguration, null, 2));

  if (
    specificationDeserializer.deserializedSpecification.sync.kind == "loaded" &&
    specificationDeserializer.deserializedSpecification.sync.value.kind ==
      "errors"
  ) {
    return (
      <ol>
        {specificationDeserializer.deserializedSpecification.sync.value.errors.map(
          (_: string, index: number) => (
            <li key={index}>{_}</li>
          ),
        )}
      </ol>
    );
  }

  // console.debug("personEntity", JSON.stringify(personEntity, null, 2));

  return (
    <div className="App">
      <h1>Ballerina 🩰</h1>
      <div className="card">
        <table>
          <tbody>
            <tr>
              <td>
                <InstantiedPersonFormsParserTemplate
                  context={{
                    ...specificationDeserializer,
                    defaultRecordConcreteRenderer:
                      DispatchPersonContainerFormView,
                    fieldTypeConverters: DispatchFieldTypeConverters,
                    defaultNestedRecordConcreteRenderer:
                      DispatchPersonNestedContainerFormView,
                    concreteRenderers: PersonConcreteRenderers,
                    infiniteStreamSources:
                      DispatchPersonFromConfigApis.streamApis,
                    enumOptionsSources: DispatchPersonFromConfigApis.enumApis,
                    entityApis: DispatchPersonFromConfigApis.entityApis,
                    getFormsConfig: () => PromiseRepo.Default.mock(() => SPEC),
                    injectedPrimitives: Map([
                      [
                        "injectedCategory",
                        {
                          fieldView: dispatchCategoryForm,
                          defaultValue: {
                            kind: "custom",
                            value: {
                              kind: "adult",
                              extraSpecial: false,
                            },
                          },
                          defaultState: DispatchCategoryState.Default(),
                        },
                      ],
                    ]),
                  }}
                  setState={setSpecificationDeserializer}
                  view={unit}
                  foreignMutations={unit}
                />
                <h3> Dispatcher Passthrough form</h3>

                <h3>Person</h3>
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
                    DeltaErrors: {JSON.stringify(entityPath.errors, null, 2)}
                  </p>
                )}
                <InstantiedPersonDispatchFormRunnerTemplate
                  context={{
                    ...specificationDeserializer,
                    ...personPassthroughFormState,
                    formRef: {
                      formName: "person-transparent",
                      kind: "passthrough",
                      entity: personEntity,
                      config,
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
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  );
};

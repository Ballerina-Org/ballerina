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
  IdWrapperProps,
  ErrorRendererProps,
  DispatchInjectedPrimitive,
} from "ballerina-core";
import { Set, Map, OrderedMap } from "immutable";
import {
  DispatchPersonFromConfigApis,
  UsersSetupFromConfigApis,
} from "playground-core";
import { PersonFormInjectedTypes } from "./domains/person-from-config/injected-forms/category";
import SPEC from "../public/SampleSpecs/example-tables.json";
import {
  DispatchPersonContainerFormView,
  DispatchPersonNestedContainerFormView,
} from "./domains/dispatched-passthrough-form/views/wrappers";
import {
  CategoryAbstractRenderer,
  DispatchCategoryState,
} from "./domains/dispatched-passthrough-form/injected-forms/category";
import { PersonConcreteRenderers } from "./domains/dispatched-passthrough-form/views/concrete-renderers";
import { DispatchFieldTypeConverters } from "./domains/dispatched-passthrough-form/apis/field-converters";

const ShowFormsParsingErrors = (
  parsedFormsConfig: DispatchSpecificationDeserializationResult<PersonFormInjectedTypes>,
) => (
  <div style={{ border: "red" }}>
    {parsedFormsConfig.kind == "errors" &&
      JSON.stringify(parsedFormsConfig.errors)}
  </div>
);

const IdWrapper = ({ children }: IdWrapperProps) => <>{children}</>;

const ErrorRenderer = ({ message }: ErrorRendererProps) => (
  <div style={{ border: "red" }}>
    <p>{message}</p>
  </div>
);

const InstantiedPersonFormsParserTemplate =
  DispatchFormsParserTemplate<PersonFormInjectedTypes>();

const InstantiedPersonDispatchFormRunnerTemplate =
  DispatchFormRunnerTemplate<PersonFormInjectedTypes>();

export const DispatcherFormsAppTables = (props: {}) => {
  const [specificationDeserializer, setSpecificationDeserializer] = useState(
    DispatchFormsParserState<PersonFormInjectedTypes>().Default(),
  );

  const [tablesRunnerState, setTablesRunnerState] = useState(
    DispatchFormRunnerState<PersonFormInjectedTypes>().Default(),
  );

  const [entity, setEntity] = useState<
    Sum<ValueOrErrors<PredicateValue, string>, "not initialized">
  >(Sum.Default.right("not initialized"));

  const [delta, setDelta] = useState<any>(null);

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

  const onEntityChange = (
    updater: Updater<any>,
    delta: DispatchDelta,
  ): void => {
    if (entity.kind == "r" || entity.value.kind == "errors") {
      return;
    }

    const newEntity = updater(entity.value.value);
    console.log("patching entity", newEntity);
    setEntity(
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
          "UsersSetupLauncher",
        )!.parseValueToApi;
      setDelta(
        DispatchDeltaTransfer.Default.FromDelta(
          toApiRawParser as any, //TODO - fix type issue if worth it
          parseCustomDelta,
        )(delta),
      );
    }
  };

  useEffect(() => {
    UsersSetupFromConfigApis.entityApis
      .get("UsersSetupApi")("")
      .then((raw) => {
        if (
          specificationDeserializer.deserializedSpecification.sync.kind ==
            "loaded" &&
          specificationDeserializer.deserializedSpecification.sync.value.kind ==
            "value"
        ) {
          const parsed =
            specificationDeserializer.deserializedSpecification.sync.value.value.launchers.passthrough
              .get("UsersSetupLauncher")!
              .parseEntityFromApi(raw);
          if (parsed.kind == "errors") {
            console.error("parsed entity errors", parsed.errors);
          } else {
            setEntity(Sum.Default.left(parsed));
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

  console.log("formState", JSON.stringify(tablesRunnerState, null, 2));

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
                      DispatchPersonFromConfigApis.streamApis, // TODO make and test some table cell streams
                    enumOptionsSources: UsersSetupFromConfigApis.enumApis,
                    lookupSources: UsersSetupFromConfigApis.lookupSources,
                    entityApis: DispatchPersonFromConfigApis.entityApis,
                    getFormsConfig: () => PromiseRepo.Default.mock(() => SPEC),
                    tableApiSources: UsersSetupFromConfigApis.tableApiSources,
                    IdWrapper,
                    ErrorRenderer,
                    injectedPrimitives: [
                      DispatchInjectedPrimitive.Default(
                        "injectedCategory",
                        CategoryAbstractRenderer,
                        {
                          kind: "custom",
                          value: {
                            kind: "adult",
                            extraSpecial: false,
                          },
                        },
                        DispatchCategoryState.Default(),
                      ),
                    ],
                  }}
                  setState={setSpecificationDeserializer}
                  view={unit}
                  foreignMutations={unit}
                />
                <h3> Dispatcher Table Passthrough form</h3>
                {/* {delta && delta.kind == "value" && (
                  <pre
                    style={{
                      display: "inline-block",
                      verticalAlign: "top",
                      textAlign: "left",
                    }}
                  >
                    {JSON.stringify(delta.value, null, 2)}
                  </pre>
                )} */}
                {delta && delta.kind == "errors" && (
                  <p>DeltaErrors: {JSON.stringify(delta.errors, null, 2)}</p>
                )}
                <InstantiedPersonDispatchFormRunnerTemplate
                  context={{
                    ...specificationDeserializer,
                    ...tablesRunnerState,
                    launcherRef: {
                      name: "UsersSetupLauncher",
                      kind: "passthrough",
                      entity: entity,
                      config: Sum.Default.left(
                        ValueOrErrors.Default.return(
                          PredicateValue.Default.record(OrderedMap()),
                        ),
                      ),
                      onEntityChange: onEntityChange,
                    },
                    showFormParsingErrors: ShowFormsParsingErrors,
                    extraContext: {
                      flags: Set(["BC", "X"]),
                    },
                    remoteEntityVersionIdentifier: "",
                  }}
                  setState={setTablesRunnerState}
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

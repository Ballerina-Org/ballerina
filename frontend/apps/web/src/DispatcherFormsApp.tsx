import { useEffect, useState } from "react";
import "./App.css";
import {
  unit,
  PromiseRepo,
  Sum,
  PredicateValue,
  replaceWith,
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
  DispatchOnChange,
  AggregatedFlags,
} from "ballerina-core";
import { Set, OrderedMap } from "immutable";
import { DispatchPersonFromConfigApis } from "playground-core";
// import SPEC from "../../../../backend/apps/automatic-tests/input-forms/simple-union-example-lookups.json";
import SPEC from "../public/SampleSpecs/dispatch-person-config.json";
import {
  DispatchPersonContainerFormView,
  DispatchPersonLookupTypeRenderer,
  DispatchPersonNestedContainerFormView,
} from "./domains/dispatched-passthrough-form/views/wrappers";
import {
  CategoryAbstractRenderer,
  DispatchCategoryState,
  DispatchPassthroughFormInjectedTypes,
} from "./domains/dispatched-passthrough-form/injected-forms/category";
import {
  DispatchPassthroughFormConcreteRenderers,
  DispatchPassthroughFormCustomPresentationContext,
  DispatchPassthroughFormFlags,
  DispatchPassthroughFormExtraContext,
} from "./domains/dispatched-passthrough-form/views/concrete-renderers";
import { DispatchFieldTypeConverters } from "./domains/dispatched-passthrough-form/apis/field-converters";
import { v4 } from "uuid";

const ShowFormsParsingErrors = (
  parsedFormsConfig: DispatchSpecificationDeserializationResult<
    DispatchPassthroughFormInjectedTypes,
    DispatchPassthroughFormFlags,
    DispatchPassthroughFormCustomPresentationContext,
    DispatchPassthroughFormExtraContext
  >,
) => (
  <div style={{ display: "flex", border: "red" }}>
    {parsedFormsConfig.kind == "errors" &&
      JSON.stringify(parsedFormsConfig.errors)}
  </div>
);

const IdWrapper = ({ domNodeId, children }: IdWrapperProps) => (
  <div id={domNodeId}>{children}</div>
);

const ErrorRenderer = ({ message }: ErrorRendererProps) => (
  <div
    style={{
      display: "flex",
      border: "2px dashed red",
      maxWidth: "200px",
      maxHeight: "50px",
      overflowY: "scroll",
      padding: "10px",
    }}
  >
    <pre
      style={{
        whiteSpace: "pre-wrap",
        maxWidth: "200px",
        lineBreak: "anywhere",
      }}
    >{`Error: ${message}`}</pre>
  </div>
);

const InstantiedPersonFormsParserTemplate = DispatchFormsParserTemplate<
  DispatchPassthroughFormInjectedTypes,
  DispatchPassthroughFormFlags,
  DispatchPassthroughFormCustomPresentationContext,
  DispatchPassthroughFormExtraContext
>();

const InstantiedPersonDispatchFormRunnerTemplate = DispatchFormRunnerTemplate<
  DispatchPassthroughFormInjectedTypes,
  DispatchPassthroughFormFlags,
  DispatchPassthroughFormCustomPresentationContext,
  DispatchPassthroughFormExtraContext
>();

export const DispatcherFormsApp = (props: {}) => {
  const [specificationDeserializer, setSpecificationDeserializer] = useState(
    DispatchFormsParserState<
      DispatchPassthroughFormInjectedTypes,
      DispatchPassthroughFormFlags,
      DispatchPassthroughFormCustomPresentationContext,
      DispatchPassthroughFormExtraContext
    >().Default(),
  );

  const [personPassthroughFormState, setPersonPassthroughFormState] = useState(
    DispatchFormRunnerState<
      DispatchPassthroughFormInjectedTypes,
      DispatchPassthroughFormFlags
    >().Default(),
  );
  const [personConfigState, setPersonConfigState] = useState(
    DispatchFormRunnerState<
      DispatchPassthroughFormInjectedTypes,
      DispatchPassthroughFormFlags
    >().Default(),
  );

  const [personEntity, setPersonEntity] = useState<
    Sum<ValueOrErrors<PredicateValue, string>, "not initialized">
  >(Sum.Default.right("not initialized"));
  const [config, setConfig] = useState<
    Sum<ValueOrErrors<PredicateValue, string>, "not initialized">
  >(Sum.Default.right("not initialized"));

  // TODO replace with delta transfer
  const [entityPath, setEntityPath] = useState<any>(null);

  const [remoteEntityVersionIdentifier, setRemoteEntityVersionIdentifier] =
    useState(v4());
  const [
    remoteConfigEntityVersionIdentifier,
    setRemoteConfigEntityVersionIdentifier,
  ] = useState(v4());

  const parseCustomDelta =
    <T,>(
      toRawObject: (
        value: PredicateValue,
        type: DispatchParsedType<T>,
        state: any,
      ) => ValueOrErrors<any, string>,
      fromDelta: (
        delta: DispatchDelta<DispatchPassthroughFormFlags>,
      ) => ValueOrErrors<DeltaTransfer<T>, string>,
    ) =>
    (
      deltaCustom: DispatchDeltaCustom<DispatchPassthroughFormFlags>,
    ): ValueOrErrors<
      [T, string, AggregatedFlags<DispatchPassthroughFormFlags>],
      string
    > => {
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
            deltaCustom.flags ? [[deltaCustom.flags, "[CategoryReplace]"]] : [],
          ] as [T, string, AggregatedFlags<DispatchPassthroughFormFlags>]);
        });
      }
      return ValueOrErrors.Default.throwOne(
        `Unsupported delta kind: ${deltaCustom.value.kind}`,
      );
    };

  console.debug("personEntity", JSON.stringify(personEntity, null, 2));

  const onPersonConfigChange: DispatchOnChange<
    PredicateValue,
    DispatchPassthroughFormFlags
  > = (updater, delta) => {
    if (config.kind == "r" || config.value.kind == "errors") {
      return;
    }

    const newConfig =
      updater.kind == "r"
        ? updater.value(config.value.value)
        : config.value.value;
    console.log("patching config", newConfig);
    setConfig(
      replaceWith(Sum.Default.left(ValueOrErrors.Default.return(newConfig))),
    );
    if (
      specificationDeserializer.deserializedSpecification.sync.kind ==
        "loaded" &&
      specificationDeserializer.deserializedSpecification.sync.value.kind ==
        "value"
    ) {
      const toApiRawParser =
        specificationDeserializer.deserializedSpecification.sync.value.value.launchers.passthrough.get(
          "person-config",
        )!.parseValueToApi;
      setEntityPath(
        DispatchDeltaTransfer.Default.FromDelta(
          toApiRawParser as any, //TODO - fix type issue if worth it
          parseCustomDelta,
        )(delta),
      );
      setRemoteConfigEntityVersionIdentifier(v4());
    }
  };

  const onPersonEntityChange: DispatchOnChange<
    PredicateValue,
    DispatchPassthroughFormFlags
  > = (updater, delta) => {
    if (personEntity.kind == "r" || personEntity.value.kind == "errors") {
      return;
    }

    const newEntity =
      updater.kind == "r"
        ? updater.value(personEntity.value.value)
        : personEntity.value.value;
    console.log("patching entity", newEntity);
    console.log("delta", JSON.stringify(delta, null, 2));
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
      setRemoteEntityVersionIdentifier(v4());
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
      .get("person-config")("")
      .then((raw) => {
        if (
          specificationDeserializer.deserializedSpecification.sync.kind ==
            "loaded" &&
          specificationDeserializer.deserializedSpecification.sync.value.kind ==
            "value"
        ) {
          const parsed =
            specificationDeserializer.deserializedSpecification.sync.value.value.launchers.passthrough
              .get("person-config")!
              .parseEntityFromApi(raw);
          if (parsed.kind == "errors") {
            console.error("parsed person config errors", parsed.errors);
          } else {
            setConfig(Sum.Default.left(parsed));
          }
        }
      });
  }, [specificationDeserializer.deserializedSpecification.sync.kind]);

  // console.debug(
  //   "personPassthroughFormState",
  //   JSON.stringify(
  //     personPassthroughFormState?.formState?.fieldStates?.get("dependants"),
  //     null,
  //     2,
  //   ),
  // );
  // console.debug(
  //   "personPassthroughFormState common",
  //   JSON.stringify(
  //     personPassthroughFormState?.formState?.commonFormState,
  //     null,
  //     2,
  //   ),
  // );
  // console.debug("personConfig", JSON.stringify(config, null, 2));

  if (
    specificationDeserializer.deserializedSpecification.sync.kind == "loaded" &&
    specificationDeserializer.deserializedSpecification.sync.value.kind ==
      "errors"
  ) {
    return (
      <ol>
        <pre>
          {specificationDeserializer.deserializedSpecification.sync.value.errors.map(
            (_: string, index: number) => (
              <li key={index}>{_}</li>
            ),
          )}
        </pre>
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
                    lookupTypeRenderer: DispatchPersonLookupTypeRenderer,
                    defaultRecordConcreteRenderer:
                      DispatchPersonContainerFormView,
                    fieldTypeConverters: DispatchFieldTypeConverters,
                    defaultNestedRecordConcreteRenderer:
                      DispatchPersonNestedContainerFormView,
                    concreteRenderers: DispatchPassthroughFormConcreteRenderers,
                    infiniteStreamSources:
                      DispatchPersonFromConfigApis.streamApis,
                    enumOptionsSources: DispatchPersonFromConfigApis.enumApis,
                    entityApis: DispatchPersonFromConfigApis.entityApis,
                    tableApiSources:
                      DispatchPersonFromConfigApis.tableApiSources,
                    lookupSources: DispatchPersonFromConfigApis.lookupSources,
                    getFormsConfig: () => PromiseRepo.Default.mock(() => SPEC),
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
                <h3> Dispatcher Passthrough form</h3>

                <h4>Config</h4>
                <div style={{ border: "2px dashed lightblue" }}>
                  <InstantiedPersonDispatchFormRunnerTemplate
                    context={{
                      ...specificationDeserializer,
                      ...personConfigState,
                      launcherRef: {
                        name: "person-config",
                        kind: "passthrough",
                        entity: config,
                        config: Sum.Default.left(
                          ValueOrErrors.Default.return(
                            PredicateValue.Default.record(OrderedMap()),
                          ),
                        ),
                        onEntityChange: onPersonConfigChange,
                      },
                      remoteEntityVersionIdentifier:
                        remoteConfigEntityVersionIdentifier,
                      showFormParsingErrors: ShowFormsParsingErrors,
                      extraContext: {
                        flags: Set(["BC", "X"]),
                      },
                    }}
                    setState={setPersonConfigState}
                    view={unit}
                    foreignMutations={unit}
                  />
                </div>
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
                  <pre>
                    DeltaErrors: {JSON.stringify(entityPath.errors, null, 2)}
                  </pre>
                )}
                <InstantiedPersonDispatchFormRunnerTemplate
                  context={{
                    ...specificationDeserializer,
                    ...personPassthroughFormState,
                    launcherRef: {
                      name: "person-transparent",
                      kind: "passthrough",
                      entity: personEntity,
                      config,
                      onEntityChange: onPersonEntityChange,
                    },
                    remoteEntityVersionIdentifier,
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

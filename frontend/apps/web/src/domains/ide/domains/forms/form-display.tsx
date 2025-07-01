import {
  DeltaTransfer,
  DispatchDelta,
  DispatchDeltaCustom,
  DispatchDeltaTransfer, DispatcherContext,
  DispatchFormRunnerState,
  DispatchFormRunnerTemplate,
  DispatchFormsParserState,
  DispatchFormsParserTemplate,
  DispatchInjectedPrimitive,
  DispatchParsedLauncher,
  DispatchParsedLaunchers,
  DispatchParsedType,
  DispatchSpecificationDeserializationResult,
  ErrorRendererProps,
  IdWrapperProps,
  Option,
  PredicateValue,
  PromiseRepo,
  replaceWith,
  Sum,
  unit,
  Updater,
  ValueOrErrors
} from "ballerina-core";

import {
  PersonFormInjectedTypes,
  PersonFormInjectedTypes as EntityFormInjectedTypes
} from "../../../person-from-config/injected-forms/category.tsx";
import {OrderedMap, Set} from "immutable";
import {useEffect, useState} from "react";
import {v4} from "uuid";
import {
    DispatchEntityContainerFormView,
    DispatchEntityNestedContainerFormView
} from "../../../dispatched-passthrough-form/views/wrappers.tsx";
import {DispatchFieldTypeConverters2, PersonDispatchFieldTypeConverters} from "../../../dispatched-passthrough-form/apis/field-converters.ts";
import {PersonConcreteRenderers} from "../../../dispatched-passthrough-form/views/concrete-renderers.tsx";
import {DispatchFromConfigApis, SpecRunnerIndicator} from "playground-core";
import {
    CategoryAbstractRenderer,
    DispatchCategoryState
} from "../../../dispatched-passthrough-form/injected-forms/category.tsx";
import { IdWrapper, ShowFormsParsingErrors, ErrorRenderer, GetLoadedValue} from "./form-display-peripheral";

const InstantiatedDispatchFormRunnerTemplate =
    DispatchFormRunnerTemplate<EntityFormInjectedTypes>();

const InstantiatedFormsParserTemplate =
    DispatchFormsParserTemplate<EntityFormInjectedTypes>();

export const FormDisplayTemplate = (props: { 
  spec: string, 
  specName: string, 
  step: SpecRunnerIndicator, 
  example: any,
  entityName: string,
  launcherName: string
}) => {
    const [entity, setEntity] = 
      useState<Sum<ValueOrErrors<PredicateValue, string>, "not initialized">>(Sum.Default.right("not initialized"));
    const [config, setConfig] =
      useState<Sum<ValueOrErrors<PredicateValue, string>, "not initialized">>(Sum.Default.right("not initialized"));
    
    const [specificationDeserializer, setSpecificationDeserializer] = useState(DispatchFormsParserState<EntityFormInjectedTypes>().Default());
    const [passthroughFormState, setPassthroughFormState] = useState(DispatchFormRunnerState<EntityFormInjectedTypes>().Default());
    const [personConfigState, setPersonConfigState] = useState(
      DispatchFormRunnerState<EntityFormInjectedTypes>().Default(),
    );
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
    
    
    // TODO replace with delta transfer
    const [entityPath, setEntityPath] = useState<any>(null);
    const [remoteEntityVersionIdentifier, setRemoteEntityVersionIdentifier] = useState(v4());

    const [remoteConfigEntityVersionIdentifier,setRemoteConfigEntityVersionIdentifier] = useState(v4());


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

    const onPersonConfigChange = (
      updater: Updater<any>,
      delta: DispatchDelta,
    ): void => {
      if (config.kind == "r" || config.value.kind == "errors") {
        return;
      }
  
      const newConfig = updater(config.value.value);
      console.log("patching config", newConfig);
      setConfig(
        replaceWith(Sum.Default.left(ValueOrErrors.Default.return(newConfig))),
      );
      // if (
      //   specificationDeserializer.deserializedSpecification.sync.kind ==
      //   "loaded" &&
      //   specificationDeserializer.deserializedSpecification.sync.value.kind ==
      //   "value"
      // ) {
      //   const toApiRawParser =
      //     specificationDeserializer.deserializedSpecification.sync.value.value.launchers.passthrough.get(
      //       "person-config",
      //     )!.parseValueToApi;
      //   setEntityPath(
      //     DispatchDeltaTransfer.Default.FromDelta(
      //       toApiRawParser as any, //TODO - fix type issue if worth it
      //       parseCustomDelta,
      //     )(delta),
      //   );
      //   setRemoteConfigEntityVersionIdentifier(v4());
      // }
    };
    
    useEffect( () => {

              
      const data = props.example
      const entity = data.fields[props.entityName]?.fields;
      const config = data?.fields[`${props.entityName}Config`]?.fields
     
      GetLoadedValue<{
        launchers: DispatchParsedLaunchers<EntityFormInjectedTypes>
        dispatcherContext: DispatcherContext<EntityFormInjectedTypes>
      }>(specificationDeserializer.deserializedSpecification).then(value =>{


        const parsedConfig =
          value.launchers.passthrough
            .get(`${props.entityName}-config`)!
            .parseEntityFromApi(config);
        parsedConfig.kind == "errors"?
          console.error("parsed person config errors", parsedConfig.errors)
          : setConfig(Sum.Default.left(parsedConfig));
          debugger
          const parsed = value.launchers.passthrough
            .get(`${props.entityName}-transparent`)!
            .parseEntityFromApi(entity);

          parsed.kind == "errors" ? 
            console.error("parsed entity errors", parsed.errors)
            : setEntity(Sum.Default.left(parsed));
          
      

            
      });
      
    }, [specificationDeserializer.deserializedSpecification.sync.kind, props.example ]);
  const data = props.example
  console.log(JSON.stringify(data));
  // const entity2 = Sum.Default.left(ValueOrErrors.Default.return(data.fields[props.entityName]?.fields));
  // const config2 = Sum.Default.left(ValueOrErrors.Default.return(data?.fields[`${props.entityName}Config`]?.fields));

    return (<div>
        <div className="App">
            <InstantiatedFormsParserTemplate
                context={{
                    ...specificationDeserializer,
                    defaultRecordConcreteRenderer: DispatchEntityContainerFormView,
                    fieldTypeConverters: DispatchFieldTypeConverters2,
                    defaultNestedRecordConcreteRenderer: DispatchEntityNestedContainerFormView,
                    concreteRenderers: PersonConcreteRenderers,
                    infiniteStreamSources: DispatchFromConfigApis.streamApis,
                    enumOptionsSources: DispatchFromConfigApis.enumApis,
                    entityApis: DispatchFromConfigApis.entityApis,
                    tableApiSources: DispatchFromConfigApis.tableApiSources,
                    lookupSources: DispatchFromConfigApis.lookupSources,
                    getFormsConfig: () => PromiseRepo.Default.mock(() =>
                    {
                      return JSON.parse(props.spec)}),
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
          <h4>Config</h4>
          <div style={{ border: "2px dashed lightblue" }}>
            {/*<InstantiatedDispatchFormRunnerTemplate*/}
            {/*  context={{*/}
            {/*    ...specificationDeserializer,*/}
            {/*    ...personConfigState,*/}
            {/*    launcherRef: {*/}
            {/*      name: props.launcherName,*/}
            {/*      kind: "passthrough",*/}
            {/*      entity: config,*/}
            {/*      config: Sum.Default.left(*/}
            {/*        ValueOrErrors.Default.return(*/}
            {/*          PredicateValue.Default.record(OrderedMap()),*/}
            {/*        ),*/}
            {/*      ),*/}
            {/*      onEntityChange: onPersonConfigChange,*/}
            {/*    },*/}
            {/*    remoteEntityVersionIdentifier:*/}
            {/*    remoteConfigEntityVersionIdentifier,*/}
            {/*    showFormParsingErrors: ShowFormsParsingErrors,*/}
            {/*    extraContext: {},*/}
            {/*  }}*/}
            {/*  setState={setPersonConfigState}*/}
            {/*  view={unit}*/}
            {/*  foreignMutations={unit}*/}
            {/*/>*/}
          </div>
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
            <InstantiatedDispatchFormRunnerTemplate
                context={{
                    ...specificationDeserializer,
                    ...passthroughFormState,
                    launcherRef: {
                        name: `${props.entityName}-transparent`,
                        kind: "passthrough",
                        entity: entity,
                        config: config,
                        onEntityChange: onEntityChange,
                    },
                    remoteEntityVersionIdentifier,
                    showFormParsingErrors: ShowFormsParsingErrors,
                    extraContext: {
                        flags: Set(["BC", "X"]),
                    },
                }}
                setState={setPassthroughFormState}
                view={unit}
                foreignMutations={unit}
            />
        </div>
    </div>)
}
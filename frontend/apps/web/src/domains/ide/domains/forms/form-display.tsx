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

import {PersonFormInjectedTypes as EntityFormInjectedTypes} from "../../../person-from-config/injected-forms/category.tsx";
import {Set} from "immutable";
import {useEffect, useState} from "react";
import {v4} from "uuid";
import {
    DispatchEntityContainerFormView,
    DispatchEntityNestedContainerFormView
} from "../../../dispatched-passthrough-form/views/wrappers.tsx";
import {DispatchFieldTypeConverters, PersonDispatchFieldTypeConverters} from "../../../dispatched-passthrough-form/apis/field-converters.ts";
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
    
    const [localValue, setLocalValue] = useState<string>(props.spec);
    const [prevValue, setPrevValue] = useState<string>(props.spec);
    useEffect(() => {

      if (props.spec !== localValue) {
        setPrevValue(localValue);
        setLocalValue(props.spec);
      }
    }, [props.spec]);
    
    const [specificationDeserializer, setSpecificationDeserializer] = useState(DispatchFormsParserState<EntityFormInjectedTypes>().Default());
    const [passthroughFormState, setPassthroughFormState] = useState(DispatchFormRunnerState<EntityFormInjectedTypes>().Default());
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
    
    useEffect( () => {

              
      const data = props.example
      const fields = data.fields[props.entityName]?.fields;
    
      GetLoadedValue<{
        launchers: DispatchParsedLaunchers<EntityFormInjectedTypes>
        dispatcherContext: DispatcherContext<EntityFormInjectedTypes>
      }>(specificationDeserializer.deserializedSpecification).then(value =>{
          
          const parsed = value.launchers.passthrough
            .get(`${props.entityName}-transparent`)!
            .parseEntityFromApi(fields);

          parsed.kind == "errors" ? console.error("parsed entity errors", parsed.errors): setEntity(Sum.Default.left(parsed));
      });
 
          const config = data?.fields[`${props.entityName}Config`]
          if (
            specificationDeserializer.deserializedSpecification.sync.kind ==
            "loaded" &&
            specificationDeserializer.deserializedSpecification.sync.value.kind ==
            "value"
          ) {
            debugger
            const parsed =
              specificationDeserializer.deserializedSpecification.sync.value.value.launchers.passthrough
                .get(`${props.entityName}-config`)!
                .parseEntityFromApi(config?.fields);
            if (parsed.kind == "errors") {
              console.error("parsed person config errors", parsed.errors);
            } else {
              setConfig(Sum.Default.left(parsed));
            }
          }
      
    }, [specificationDeserializer.deserializedSpecification.sync.kind, prevValue, props.example ]);
    
    return (<div>
        <div className="App">
            <InstantiatedFormsParserTemplate
                context={{
                    ...specificationDeserializer,
                    defaultRecordConcreteRenderer: DispatchEntityContainerFormView,
                    fieldTypeConverters: DispatchFieldTypeConverters,
                    defaultNestedRecordConcreteRenderer: DispatchEntityNestedContainerFormView,
                    concreteRenderers: PersonConcreteRenderers,
                    infiniteStreamSources: DispatchFromConfigApis.streamApis,
                    enumOptionsSources: DispatchFromConfigApis.enumApis,
                    entityApis: DispatchFromConfigApis.entityApis,
                    tableApiSources: DispatchFromConfigApis.tableApiSources,
                    lookupSources: DispatchFromConfigApis.lookupSources,
                    getFormsConfig: () => PromiseRepo.Default.mock(() =>
                    {
                      return JSON.parse(localValue)}),
                    parentSpecification: { current: localValue, prev: prevValue },  
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
            <InstantiatedDispatchFormRunnerTemplate
                context={{
                    ...specificationDeserializer,
                    ...passthroughFormState,
                    launcherRef: {
                        name: props.launcherName,
                        kind: "passthrough",
                        entity: entity,
                        config,
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
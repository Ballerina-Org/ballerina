import { useEffect, useState } from "react";
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
    AggregatedFlags, LookupApiOne,
} from "ballerina-core";
import { Set, OrderedMap } from "immutable";

import {
    DispatchEntityContainerFormView,
    DispatchLookupTypeRenderer,
    DispatchEntityNestedContainerFormView,
} from "../../dispatched-passthrough-form/views/wrappers";
import {
    CategoryAbstractRenderer,
    DispatchCategoryState,
    DispatchPassthroughFormInjectedTypes,
} from "../../dispatched-passthrough-form/injected-forms/category";
import {
    DispatchPassthroughFormConcreteRenderers,
    DispatchPassthroughFormCustomPresentationContext,
    DispatchPassthroughFormFlags,
    DispatchPassthroughFormExtraContext,
} from "../../dispatched-passthrough-form/views/concrete-renderers";
import { DispatchFieldTypeConverters } from "../../dispatched-passthrough-form/apis/field-converters";
import { v4 } from "uuid";
import {DispatchFromConfigApis, IdeFormProps} from "playground-core";
// import {getSeedEntity, updateEntity, UnmockingApisEntities, UnmockingApisTables, UnmockingApisStreams, UnmockingApisEnums, UnmockingApisLookups} from "playground-core";
import {getSeedEntity, getLookup, GetLookupResponse, updateEntity, UnmockingApisLookups, findByDispatchType} from "playground-core";
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

const InstantiatedFormsParserTemplate = DispatchFormsParserTemplate<
    DispatchPassthroughFormInjectedTypes,
    DispatchPassthroughFormFlags,
    DispatchPassthroughFormCustomPresentationContext,
    DispatchPassthroughFormExtraContext
>();

const InstantiatedDispatchFormRunnerTemplate = DispatchFormRunnerTemplate<
    DispatchPassthroughFormInjectedTypes,
    DispatchPassthroughFormFlags,
    DispatchPassthroughFormCustomPresentationContext,
    DispatchPassthroughFormExtraContext
>();

export const DispatcherFormsApp = (props: IdeFormProps) => {
    const [specificationDeserializer, setSpecificationDeserializer] = useState(
        DispatchFormsParserState<
            DispatchPassthroughFormInjectedTypes,
            DispatchPassthroughFormFlags,
            DispatchPassthroughFormCustomPresentationContext,
            DispatchPassthroughFormExtraContext
        >().Default(),
    );

    const [passthroughFormState, setPassthroughFormState] = useState(
        DispatchFormRunnerState<
            DispatchPassthroughFormInjectedTypes,
            DispatchPassthroughFormFlags
        >().Default(),
    );
    const [entityConfigState, setEntityConfigState] = useState(
        DispatchFormRunnerState<
            DispatchPassthroughFormInjectedTypes,
            DispatchPassthroughFormFlags
        >().Default(),
    );

    const [entity, setEntity] = useState<
        Sum<ValueOrErrors<PredicateValue, string>, "not initialized">
    >(Sum.Default.right("not initialized"));

    const [entityId, setEntityId] = useState<
        Sum<string, "not initialized">
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

    const onEntityConfigChange: DispatchOnChange<
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

    const onEntityChange: DispatchOnChange<
        PredicateValue,
        DispatchPassthroughFormFlags
    > = (updater, delta) => {
        debugger
        if (entity.kind == "r" || entity.value.kind == "errors") {
            return;
        }

        const newEntity =
            updater.kind == "r"
                ? updater.value(entity.value.value)
                : entity.value.value;
        console.log("patching entity", newEntity);
        console.log("delta", JSON.stringify(delta, null, 2));
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
            const path = DispatchDeltaTransfer.Default.FromDelta(
                toApiRawParser as any, //TODO - fix type issue if worth it
                parseCustomDelta,
            )(delta);

            setEntityPath(path);
            setRemoteEntityVersionIdentifier(v4());
        }
    };

    useEffect(() => {
        if (
            specificationDeserializer.deserializedSpecification.sync.kind ==
            "loaded" &&
            specificationDeserializer.deserializedSpecification.sync.value.kind ==
            "value" 
        ) {
            const spec = specificationDeserializer.deserializedSpecification.sync.value.value
            const {lookupSources, specApis} = specificationDeserializer.deserializedSpecification.sync.value.value.dispatcherContext

            getSeedEntity(props.specName, props.entityName)
                .then(async (raw) => {
                   
                    if (raw.kind == "value") {
                        
                        const id = raw.value[0].id
                        debugger
                        const parsed =
                            spec.launchers.passthrough
                                .get("person-transparent")!
                                .parseEntityFromApi(raw.value[0].value);



                        if (parsed.kind == "errors") {
                            console.error("parsed entity errors", parsed.errors);
                        } else {
                            const entity = parsed.value;
                            const e = entity as any;
                            if(specApis.lookups && lookupSources){
                                debugger
                                const oneFields = findByDispatchType(specApis.lookups, props.typeName)
                                const fields =
                                    oneFields
                                        .flatMap(x => {
                                    const s = lookupSources(x.key);
                                    return s.kind === "value" ? [{ api: x.key, sources: s.value }] : [];
                                });
                          
                                const fetched =
                                    await Promise.all(
                                        fields.map(
                                async field =>
                                            ({ 
                                                key: field.api.replace(/Api$/, ""),
                                                value: await getLookup(props.specName, field.api.replace(/Api$/,""),  id)
                                            })
                                        ));
                                const ones =  [...fetched, { key: "Id", value: ValueOrErrors.Default.return(id) }];
                                
                                const pairs = 
                                    ones
                                    .flatMap(x => {
                                        const tmp =  x.value.kind === "value" ? x.key == "Id" ? x.value.value : PredicateValue.Default.option(true,PredicateValue.Default.record(x.value.value.values[0])) : null;
                                        debugger
                                        return x.value.kind === "value" ? [[x.key, tmp] as const] : []
                                    });

                                const updated = {
                                    ...e,
                                    fields: e.fields.merge(Object.fromEntries(pairs)),
                                };
                                debugger
                                //setEntity(Sum.Default.left(parsed));
                                setEntity(Sum.Default.left(ValueOrErrors.Default.return(updated)));// pv));
                                setEntityId(id);
                                
    
                            }
                            

                        }
                    }
                });
        }
        //UnmockingApisEntities.entityApis
        DispatchFromConfigApis.entityApis
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


    return (
        <div className="App">
            {/*<h1>Ballerina 🩰</h1>*/}
            <div className="card">
                <table>
                    <tbody>
                    <tr>
                        <td>
                            <InstantiatedFormsParserTemplate
                                context={{
                                    ...specificationDeserializer,
                                    lookupTypeRenderer: DispatchLookupTypeRenderer,
                                    defaultRecordConcreteRenderer:
                                    DispatchEntityContainerFormView,
                                    fieldTypeConverters: DispatchFieldTypeConverters,
                                    defaultNestedRecordConcreteRenderer:
                                    DispatchEntityNestedContainerFormView,
                                    concreteRenderers: DispatchPassthroughFormConcreteRenderers,
                                    infiniteStreamSources:
                                    DispatchFromConfigApis.streamApis, //UnmockingApisStreams.streamApis,
                                    enumOptionsSources:DispatchFromConfigApis.enumApis, // UnmockingApisEnums.enumApis,
                                    entityApis: DispatchFromConfigApis.entityApis, //UnmockingApisEntities.entityApis,
                                    tableApiSources:
                                    DispatchFromConfigApis.tableApiSources, // UnmockingApisTables.tableApiSources,
                                    lookupSources: UnmockingApisLookups.lookupSources,
                                    getFormsConfig: () => PromiseRepo.Default.mock(() => props.spec),
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
                            {/*<h3> Dispatcher Passthrough form</h3>*/}
                            
                            {/*<h4>Config</h4>*/}
                            {/*<div style={{ border: "2px dashed lightblue" }}>*/}
                            {/*    <InstantiatedDispatchFormRunnerTemplate*/}
                            {/*        context={{*/}
                            {/*            ...specificationDeserializer,*/}
                            {/*            ...entityConfigState,*/}
                            {/*            launcherRef: {*/}
                            {/*                name: "person-config",*/}
                            {/*                kind: "passthrough",*/}
                            {/*                entity: config,*/}
                            {/*                config: Sum.Default.left(*/}
                            {/*                    ValueOrErrors.Default.return(*/}
                            {/*                        PredicateValue.Default.record(OrderedMap()),*/}
                            {/*                    ),*/}
                            {/*                ),*/}
                            {/*                onEntityChange: onEntityConfigChange,*/}
                            {/*            },*/}
                            {/*            remoteEntityVersionIdentifier:*/}
                            {/*            remoteConfigEntityVersionIdentifier,*/}
                            {/*            showFormParsingErrors: ShowFormsParsingErrors,*/}
                            {/*            extraContext: {*/}
                            {/*                flags: Set(["BC", "X"]),*/}
                            {/*            },*/}
                            {/*        }}*/}
                            {/*        setState={setEntityConfigState}*/}
                            {/*        view={unit}*/}
                            {/*        foreignMutations={unit}*/}
                            {/*    />*/}
                            {/*</div>*/}
                            {/*<h3>Person</h3>*/}
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
                            <InstantiatedDispatchFormRunnerTemplate
                                context={{
                                    ...specificationDeserializer,
                                    ...passthroughFormState,
                                    launcherRef: {
                                        name: "person-transparent",
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
                        </td>
                    </tr>
                    </tbody>
                </table>
            </div>
        </div>
    );
};

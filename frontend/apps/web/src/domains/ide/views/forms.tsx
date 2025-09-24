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
import {
    DispatchFromConfigApis,
    Ide,
    IdeEntityApis,
    IdeFormProps,
    UnmockingApisEnums,
    UnmockingApisLookups
} from "playground-core";
import { UnmockingApisStreams, getSeed} from "playground-core";
// import {getSeedEntity, getLookup,  updateEntity, UnmockingApisLookups} from "playground-core";
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
import {  isCollection, isKeyed } from "immutable";
import {FormsSeedEntity} from "playground-core/ide/domains/seeds/state.ts";

type AnyObject = Record<string, unknown>;

const isPlainObject = (x: unknown): x is AnyObject =>
    !!x && typeof x === "object" && (x as object).constructor === Object;

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
                    props.launcherName,
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
        console.log("re-rendering");
        console.log(props.spec)

        if (
            specificationDeserializer.deserializedSpecification.sync.kind ==
            "loaded" &&
            specificationDeserializer.deserializedSpecification.sync.value.kind ==
            "value" 
        ) {
            const spec = specificationDeserializer.deserializedSpecification.sync.value.value

            getSeed(props.specName, props.entityName)
                .then(async (raw) => {
             
                    if (raw.kind == "value") {
                        const res: FormsSeedEntity = raw.value;
     
     
                        const parsed =
                            spec.launchers.passthrough
                                .get(props.launcherName)!
                                .parseEntityFromApi(raw.value.value);

                        if (parsed.kind == "errors") {
                            console.error("parsed entity errors", parsed.errors);
                        } else {
                            const entity = parsed.value;
                            
                            const e = entity as any;

                            const updated = {
                                ...e,
                                fields: e.fields.merge(Object.fromEntries([["Id",  res.id]])),
                            };
                            //setEntity(Sum.Default.left(parsed));
                            debugger
                            setEntity(Sum.Default.left(ValueOrErrors.Default.return(updated)));
                            setEntityId(res.id as any);
                        }
                    }
                    else {
                        props.setState(Ide.Updaters.CommonUI.formsError(raw.errors));
                    }
                });
        }
        //UnmockingApisEntities.entityApis
        //DispatchFromConfigApis.entityApis
        IdeEntityApis
            .get(props.launcherConfigName)!("")
            .then((raw) => {
                if (
                    specificationDeserializer.deserializedSpecification.sync.kind ==
                    "loaded" &&
                    specificationDeserializer.deserializedSpecification.sync.value.kind ==
                    "value"
                ) {
                    debugger
                    const parsed =
                        specificationDeserializer.deserializedSpecification.sync.value.value.launchers.passthrough
                            .get(props.launcherName)!
                            .parseEntityFromApi(raw.value.value);
                    if (parsed.kind == "errors") {
                        debugger
                        console.error("parsed person config errors", parsed.errors);
                    } else {
                        setConfig(Sum.Default.left(parsed));
                    }
                }
            });
    }, [specificationDeserializer.deserializedSpecification.sync.kind, props.spec]);
    

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
                                    defaultRecordConcreteRenderer: DispatchEntityContainerFormView,
                                    fieldTypeConverters: DispatchFieldTypeConverters,
                                    defaultNestedRecordConcreteRenderer:DispatchEntityNestedContainerFormView,
                                    concreteRenderers: DispatchPassthroughFormConcreteRenderers,
                                    
                                    infiniteStreamSources:UnmockingApisStreams.streamApis,
                                    enumOptionsSources:UnmockingApisEnums.enumApis,
                                    entityApis: DispatchFromConfigApis.entityApis, //UnmockingApisEntities.entityApis,
                                    tableApiSources: DispatchFromConfigApis.tableApiSources, // UnmockingApisTables.tableApiSources,
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
                {/*            {entityPath && entityPath.kind == "value" && (*/}
                {/*  <pre*/}
                {/*    style={{*/}
                {/*      display: "inline-block",*/}
                {/*      verticalAlign: "top",*/}
                {/*      textAlign: "left",*/}
                {/*    }}*/}
                {/*  >*/}
                {/*    {JSON.stringify(entityPath.value, null, 2)}*/}
                {/*  </pre>*/}
                {/*)} */}
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

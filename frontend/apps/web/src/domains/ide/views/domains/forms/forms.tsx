import React, { useEffect, useState } from "react";
import { getTheme } from "@blp-private-npm/ui";
import { ThemeProvider } from "@mui/material";
import {
    unit,
    PromiseRepo,
    Sum,
    PredicateValue,
    replaceWith,
    DeltaTransfer,
    ValueOrErrors,
    DispatchFormsParserState,
    DispatchDeltaTransfer,
    DispatchDeltaCustom,
    DispatchDelta,
    DispatchFormRunnerState,
    DispatchParsedType,
    DispatchInjectedPrimitive,
    DispatchOnChange,
    AggregatedFlags, LookupApiOne, DispatchDeltaTransferComparand, DispatchDeltaTransferV2, ConcreteRenderers,
    dispatchToAPIRawValue, dispatchToAPIRawValueV2, IdeTypeConverters,
} from "ballerina-core";
import {List, Set } from "immutable";
import {FormsSeedEntity} from "playground-core/ide/domains/types/seeds";

import {
    DispatchEntityContainerFormView,
    DispatchLookupTypeRenderer,
    DispatchEntityNestedContainerFormView,
} from "./wrappers";
import {
    CategoryAbstractRenderer,
    DispatchCategoryState,
    DispatchPassthroughFormInjectedTypes,
} from "../../../../dispatched-passthrough-form/injected-forms/category";
import {
    DispatchPassthroughFormConcreteRenderers as TailwindRenderers,
    DispatchPassthroughFormCustomPresentationContext,
    //DispatchPassthroughFormFlags,
    //DispatchPassthroughFormExtraContext,
} from "../../../../dispatched-passthrough-form/views/tailwind-renderers";
import {
    DispatchPassthroughFormConcreteRenderers as NoneRenderers,

} from "../../../../dispatched-passthrough-form/views/concrete-renderers.tsx";
import { v4 } from "uuid";
import {
    DispatchFromConfigApis, expand,
    IdePhase,
    IdeEntityApis, LockedPhase, sendDelta,
    UnmockingApisEnums,
    UnmockingApisLookups, Ide, Forms
} from "playground-core";
import { UnmockingApisStreams, getSeed} from "playground-core";
import {IdeRenderers} from "./domains/loader.tsx";
import {
    IdeErrorRenderer, IdeIdWrapper,
    IdeShowFormsParsingErrors,
    InstantiatedDispatchFormRunnerTemplate,
    InstantiatedFormsParserTemplate, parseCustomDelta
} from "./forms-essentials.tsx";
import {IdeFlags} from "./domains/common/ide-flags.ts";
import {FieldExtraContext} from "./domains/common/field-extra-context.ts";
import {Namespace} from "./domains/common/namespace.ts";

export type IT = DispatchPassthroughFormInjectedTypes
export type FL = IdeFlags
export type PC = DispatchPassthroughFormCustomPresentationContext
export type EC = FieldExtraContext

export const DispatcherFormsApp = (props: Forms) => {
    const theme = getTheme(true,"mocked");
    const [specificationDeserializer, setSpecificationDeserializer] = useState(
        DispatchFormsParserState<IT,FL, PC, EC>().Default(),
    );

    const [passthroughFormState, setPassthroughFormState] = useState(
        DispatchFormRunnerState<IT,FL, PC, EC>().Default.passthrough(),
    );

    const [entity, setEntity] = 
        useState<Sum<ValueOrErrors<PredicateValue, string>, "not initialized">>(Sum.Default.right("not initialized"));
    const [entityName, setEntityName] = 
        useState<Sum<string, "not initialized">>(Sum.Default.right("not initialized"));
    const [entityId, setEntityId] = 
        useState<Sum<string, "not initialized">>(Sum.Default.right("not initialized"));
    const [config, setConfig] = useState<
        Sum<ValueOrErrors<PredicateValue, string>, "not initialized">>(Sum.Default.right("not initialized"));
    const [entityPath, setEntityPath] = 
        useState<any>(null);

    const [remoteEntityVersionIdentifier, setRemoteEntityVersionIdentifier] = useState(v4());
    const [
        remoteConfigEntityVersionIdentifier,
        setRemoteConfigEntityVersionIdentifier,
    ] = useState(v4());

    const onEntityChange: DispatchOnChange<PredicateValue, FL
    > = async (updater, delta) => {

        if (entity.kind == "r" || entity.value.kind == "errors") return;

        const newEntity =
            updater.kind == "r"
                ? updater.value(entity.value.value)
                : entity.value.value;

        setEntity(
            replaceWith(Sum.Default.left(ValueOrErrors.Default.return(newEntity))),
        );
     
        if (
            specificationDeserializer.deserializedSpecification.sync.kind ==
            "loaded" &&
            specificationDeserializer.deserializedSpecification.sync.value.kind ==
            "value" && 
            entityName.kind == "l" && entityId.kind == "l"
        ) {
            
            const toApiRawParser =
                specificationDeserializer.deserializedSpecification.sync.value.value.launchers.passthrough.get(
                    props.launcher,
                )!.parseValueToApi;
            
            const path: ValueOrErrors<
                [
                    DispatchDeltaTransferV2,
                    DispatchDeltaTransferComparand,
                    AggregatedFlags<any>,
                ],
                string
            >  = DispatchDeltaTransferV2.Default.FromDelta(
                toApiRawParser as any, 
               // dispatchToAPIRawValueV2 as any,
                parseCustomDelta,
            )(delta);
            
            if(path.kind == "value") {
                if (props.deltas.kind == "l") {
               
                    props.setState(
                       Ide.Updaters.Core.phase.locked(
                           LockedPhase.Updaters.Core.startDeltas()
                               .then(LockedPhase.Updaters.Core.addDelta(path.value))
                       ));
                } else {
                    props.setState(
                        Ide.Updaters.Core.phase.locked(
                            LockedPhase.Updaters.Core.addDelta(path.value)));
                }
            }
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

            expand(props.specName, props.launcher, props.path)
                .then(async (raw) => {
                    if (raw.kind == "value") {
                        const res: FormsSeedEntity = raw.value;
                        const parsed =
                            spec.launchers.passthrough
                                .get(props.launcher)!
                                .parseEntityFromApi(raw.value.value);
                 
                        if (parsed.kind == "errors") {
                            console.error("parsed entity errors 2", parsed.errors);
                            props.setState(Ide.Updaters.Core.phase.locked(LockedPhase.Updaters.Core.errors(replaceWith(parsed.errors))));
                        } else {
                            const entity = parsed.value;
                            const e = entity as any
                            const updated = {
                                ...e,
                                fields: e.fields.merge(Object.fromEntries([["Id",  res.id]])),
                            };
                            setEntity(Sum.Default.left(ValueOrErrors.Default.return(updated)));
                            setEntityName(Sum.Default.left(res.entityName));
                            setEntityId(Sum.Default.left(res.id as any));
                        }
                    }
                    else {
                        props.setState(Ide.Updaters.Core.phase.locked(LockedPhase.Updaters.Core.errors(replaceWith(raw.errors))));
                    }
                });
        }
        IdeEntityApis
            .get("GlobalConfiguration")!("")
            .then((raw) => {
                if (
                    specificationDeserializer.deserializedSpecification.sync.kind ==
                    "loaded" &&
                    specificationDeserializer.deserializedSpecification.sync.value.kind ==
                    "value"
                ) {
                    const parsed =
                        specificationDeserializer.deserializedSpecification.sync.value.value.launchers.passthrough
                            .get(props.launcher)!
                            .parseEntityFromApi(raw.value.value);
                    if (parsed.kind == "errors") {
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
        <div className="App h-full pb-12">
                        {props.deltas.kind == 'r' && props.showDeltas &&

                            <div className="stats bg-base-100 border-base-300 border w-full">
                                <div className="stat">
                                    <div className="stat-title">Current deltas</div>
                                    <div className="stat-desc">↗︎ {JSON.stringify(props.deltas.value.left).length} (~size)</div>
                                    <div className="stat-value">{props.deltas.value.left.size}</div>
                                    <div className="stat-actions">
                                        <button className="btn btn-xs btn-success"
                                                onClick={async() =>{
                                                    if(props.deltas.kind == "r") {
                                                        const result = await sendDelta(props.specName, entityName.value, entityId.value, props.deltas.value, props.path, props.launcher);
                                                        if(result.kind == "value")
                                                            props.setState(Ide.Updaters.Core.phase.locked(LockedPhase.Updaters.Core.drainDeltas()));
                                                        else props.setState(Ide.Updaters.Core.phase.locked(LockedPhase.Updaters.Core.errors(replaceWith(result.errors))));
                                                    }
                                                }}
                                        >Drain</button>
                                    </div>
                                </div>

                                <div className="stat">
                                    <div className="stat-title">Deltas drained</div>
                                    <div className="stat-value">{props.deltas.value.right.size}</div>
                                </div>
                            </div>
                        }
                            <InstantiatedFormsParserTemplate
                                context={{
                                    ...specificationDeserializer,
                                    lookupTypeRenderer: DispatchLookupTypeRenderer,
                                    defaultRecordConcreteRenderer: DispatchEntityContainerFormView,
                                    fieldTypeConverters: IdeTypeConverters,
                                    defaultNestedRecordConcreteRenderer:DispatchEntityNestedContainerFormView,
                                    concreteRenderers: 
                                        props.ui.kind == 'ui-kit' ? IdeRenderers : TailwindRenderers,
                                    getFormsConfig: () => PromiseRepo.Default.mock(() => props.spec),
                                    IdWrapper:IdeIdWrapper,
                                    ErrorRenderer: IdeErrorRenderer,
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
                            {entityPath && entityPath.kind == "errors" && (
                                <pre>
                    DeltaErrors: {JSON.stringify(entityPath.errors, null, 2)}
                  </pre>
                            )}
                            <ThemeProvider theme={theme}>
                       
                            <InstantiatedDispatchFormRunnerTemplate
                                context={{
                                    ...specificationDeserializer,
                                    ...passthroughFormState,
                                    launcherRef: {
                                        name: props.launcher,
                                        kind: "passthrough",
                                        entity: entity,
                                        config,
                                        onEntityChange: onEntityChange,
                                        apiSources: {
                                            infiniteStreamSources: UnmockingApisStreams.streamApis,
                                            enumOptionsSources: UnmockingApisEnums.enumApis,
                                            tableApiSources:
                                            DispatchFromConfigApis.tableApiSources,
                                            lookupSources:UnmockingApisLookups.lookupSources,
                                        },
                                    },
                                    remoteEntityVersionIdentifier,
                                    showFormParsingErrors: IdeShowFormsParsingErrors,
                                    extraContext: {
                                        isSuperAdmin: false, 
                                        locale: ""!, // LocalizationState.Default(null, t, i18n),
                                        namespace: Namespace.TranslationNamespaceSetupGuide,
                                        headers: {},
                                        docId: "",
                                        foreignMutations: unit as any, 
                                        downloadExampleAccountingCsv: unit as any,
                                        customLocks: Set(),
                                    },
                                    globallyDisabled: false,
                                    globallyReadOnly: false,
                                }}
                                setState={setPassthroughFormState}
                                view={unit}
                                foreignMutations={unit}
                            />
                            </ThemeProvider>

        </div>
    );
};

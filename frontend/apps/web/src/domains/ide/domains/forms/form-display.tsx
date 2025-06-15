import {
    DeltaTransfer,
    DispatchDelta, DispatchDeltaCustom,
    DispatchDeltaTransfer,
    DispatchFormRunnerState,
    DispatchFormRunnerTemplate,
    DispatchFormsParserState,
    DispatchFormsParserTemplate, DispatchInjectedPrimitive, DispatchParsedType,
    DispatchSpecificationDeserializationResult, ErrorRendererProps, IdWrapperProps, Option,
    PredicateValue, PromiseRepo,
    replaceWith,
    Sum,
    unit,
    Updater,
    ValueOrErrors
} from "ballerina-core";
import {PersonFormInjectedTypes} from "../../../person-from-config/injected-forms/category.tsx";
import {Set} from "immutable";
import {useEffect, useState} from "react";
import {v4} from "uuid";
import {
    DispatchPersonContainerFormView,
    DispatchPersonNestedContainerFormView
} from "../../../dispatched-passthrough-form/views/wrappers.tsx";
import {DispatchFieldTypeConverters} from "../../../dispatched-passthrough-form/apis/field-converters.ts";
import {PersonConcreteRenderers} from "../../../dispatched-passthrough-form/views/concrete-renderers.tsx";
import {DispatchPersonFromConfigApis, EditorStep} from "playground-core";
import {
    CategoryAbstractRenderer,
    DispatchCategoryState
} from "../../../dispatched-passthrough-form/injected-forms/category.tsx";

const InstantiedPersonDispatchFormRunnerTemplate =
    DispatchFormRunnerTemplate<PersonFormInjectedTypes>();

const InstantiedPersonFormsParserTemplate =
    DispatchFormsParserTemplate<PersonFormInjectedTypes>();
const ShowFormsParsingErrors = (
    parsedFormsConfig: DispatchSpecificationDeserializationResult<PersonFormInjectedTypes>,
) => (
    <div style={{ display: "flex", border: "red" }}>
        {parsedFormsConfig.kind == "errors" &&
            JSON.stringify(parsedFormsConfig.errors)}
    </div>
);

export const FormDisplayTemplate = (props: {spec: Option<string>, step: EditorStep}) => {
    const [personEntity, setPersonEntity] = useState<
        Sum<ValueOrErrors<PredicateValue, string>, "not initialized">
    >(Sum.Default.right("not initialized"));
    
    const [specificationDeserializer, setSpecificationDeserializer] = useState(
        DispatchFormsParserState<PersonFormInjectedTypes>().Default(),
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
    
    const [personPassthroughFormState, setPersonPassthroughFormState] = useState(
        DispatchFormRunnerState<PersonFormInjectedTypes>().Default(),
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
            setRemoteEntityVersionIdentifier(v4());
        }
    };
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
    return (<div>
        <div>{props.step.kind}</div>
        {props.spec.kind == "r" && <div className="App">
        <InstantiedPersonFormsParserTemplate
            context={{
                ...specificationDeserializer,
                defaultRecordConcreteRenderer: DispatchPersonContainerFormView,
                fieldTypeConverters: DispatchFieldTypeConverters,
                defaultNestedRecordConcreteRenderer: DispatchPersonNestedContainerFormView,
                concreteRenderers: PersonConcreteRenderers,
                infiniteStreamSources:  DispatchPersonFromConfigApis.streamApis,
                enumOptionsSources: DispatchPersonFromConfigApis.enumApis,
                entityApis: DispatchPersonFromConfigApis.entityApis,
                tableApiSources: DispatchPersonFromConfigApis.tableApiSources,
                lookupSources: DispatchPersonFromConfigApis.lookupSources,
                getFormsConfig: () => PromiseRepo.Default.mock(() => JSON.parse(props.spec.value as string)),
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
    </div>}</div>)
}

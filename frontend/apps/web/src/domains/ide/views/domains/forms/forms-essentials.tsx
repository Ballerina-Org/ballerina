import React from "react";
import {
    DispatchFormsParserTemplate,
    DispatchFormRunnerTemplate,
    DispatchSpecificationDeserializationResult,
    IdWrapperProps,
    ErrorRendererProps, PredicateValue, DispatchParsedType, ValueOrErrors, DispatchDelta, DeltaTransfer,
    DispatchDeltaCustom, AggregatedFlags,
} from "ballerina-core";

import {
    DispatchPassthroughFormInjectedTypes,
} from "../../../../dispatched-passthrough-form/injected-forms/category";
import {
    DispatchPassthroughFormConcreteRenderers,
    DispatchPassthroughFormCustomPresentationContext,
    //DispatchPassthroughFormFlags,
    //DispatchPassthroughFormExtraContext,
} from "../../../../dispatched-passthrough-form/views/tailwind-renderers";
import { IdeFlags } from "./domains/common/ide-flags";
import {FieldExtraContext} from "./domains/common/field-extra-context.ts";

export const IdeShowFormsParsingErrors = (
    parsedFormsConfig: DispatchSpecificationDeserializationResult<
        DispatchPassthroughFormInjectedTypes,
        IdeFlags, // DispatchPassthroughFormFlags,
        DispatchPassthroughFormCustomPresentationContext,
        FieldExtraContext
    >,
) => (
    <div style={{ display: "flex", border: "red" }}>
        {parsedFormsConfig.kind == "errors" &&
            JSON.stringify(parsedFormsConfig.errors)}
    </div>
);

export const IdeIdWrapper = ({ domNodeId, children }: IdWrapperProps) => (
    <div id={domNodeId}>{children}</div>
);

export const IdeErrorRenderer = ({ message }: ErrorRendererProps) => (
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

export const InstantiatedFormsParserTemplate = DispatchFormsParserTemplate<
    DispatchPassthroughFormInjectedTypes,
    IdeFlags, //DispatchPassthroughFormFlags,
    DispatchPassthroughFormCustomPresentationContext,
    FieldExtraContext //DispatchPassthroughFormExtraContext
>();

export const InstantiatedDispatchFormRunnerTemplate = DispatchFormRunnerTemplate<
    DispatchPassthroughFormInjectedTypes,
    IdeFlags, //DispatchPassthroughFormFlags,
    DispatchPassthroughFormCustomPresentationContext,
    FieldExtraContext //DispatchPassthroughFormExtraContext
>();

export const parseCustomDelta =
    <T,>(
        toRawObject: (
            value: PredicateValue,
            type: DispatchParsedType<T>,
            state: any,
        ) => ValueOrErrors<any, string>,
        fromDelta: (
            delta: DispatchDelta<IdeFlags>, //DispatchPassthroughFormFlags>,
        ) => ValueOrErrors<DeltaTransfer<T>, string>,
    ) =>
        (
            deltaCustom: DispatchDeltaCustom<IdeFlags>, //DispatchPassthroughFormFlags>,
        ): ValueOrErrors<
            [T, string, AggregatedFlags<IdeFlags>], //DispatchPassthroughFormFlags>],
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
                    ] as [T, string, AggregatedFlags<IdeFlags>]) //DispatchPassthroughFormFlags>]);
                });
            }
            return ValueOrErrors.Default.throwOne(
                `Unsupported delta kind: ${deltaCustom.value.kind}`,
            );
        };


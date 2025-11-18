import {
    BaseComparisonTablePropsV3,
    BaseComparisonTableV3,
    BaseDropdownOptionPreviewSection,
    BaseFieldMetadata,
    BaseFieldMetadataCustomAiProps,
    BaseFieldMetadataProps,
    BaseFieldPopoverAllTodos,
    BaseFieldPopoverAutomations,
    BaseFieldPopoverSections,
    BaseFieldStatus,
    BaseFieldStatusDot,
    // BaseFieldStatusDot,
    BaseNodeWithModal,
    BaseTypography,
} from "@blp-private-npm/ui";
import {
    DispatchCommonFormState,
    DispatchDelta,
    DispatchDeltaRecord,
    DispatchDeltaTuple,
    DispatchParsedType,
    Option,
    PredicateValue,
    TupleAbstractRendererView,
    unit,
    ValueOrErrors,
    ValueRecord,
} from "ballerina-core";
import { List, OrderedMap } from "immutable";
import React, { Fragment } from "react";
import { IdeConcreteRenderers, RendererPropsDomain } from "../common/concrete-renderers";


const parseApproval = (
    tuples: List<PredicateValue>,
    idx: number | undefined
): boolean | undefined => {
    const approval = idx != undefined ? tuples.get(idx) : undefined;
    if (approval != undefined && PredicateValue.Operations.IsBoolean(approval)) {
        return approval;
    }
};

export const TupleFieldViews = {
    tuple2: () => (props) => (
        <>
            {Array.from({ length: 2 }, (_, index) => (
                <Fragment key={index}>
                    {props.embeddedItemTemplates(index)(undefined)({
                        ...RendererPropsDomain(
                            props
                        ).Operations.AugmentingCustomPresentationContext({
                            sum: undefined,
                        }),
                        view: unit,
                    })}
                </Fragment>
            ))}
        </>
    ),
    companyAndBankAccount2: () => (props) => (
        <>
            {Array.from({ length: 2 }, (_, index) => (
                <Fragment key={index}>
                    {props.embeddedItemTemplates(index)(undefined)({
                        ...RendererPropsDomain(
                            props
                        ).Operations.AugmentingCustomPresentationContext({
                            sum: undefined,
                        }),
                        view: unit,
                    })}
                </Fragment>
            ))}
        </>
    ),
    previewSections2: () => (props) => (
        <>
            {Array.from({ length: 2 }, (_, index) => (
                <BaseDropdownOptionPreviewSection key={index}>
                    {props.embeddedItemTemplates(index)(undefined)({
                        ...RendererPropsDomain(
                            props
                        ).Operations.AugmentingCustomPresentationContext({
                            sum: undefined,
                        }),
                        view: unit,
                    })}
                </BaseDropdownOptionPreviewSection>
            ))}
        </>
    ),
    tuple3: () => (props) => (
        <>
            {Array.from({ length: 3 }, (_, index) =>
                props.embeddedItemTemplates(index)(undefined)({
                    ...RendererPropsDomain(
                        props
                    ).Operations.AugmentingCustomPresentationContext({
                        sum: undefined,
                    }),
                    view: unit,
                })
            )}
        </>
    ),
    tuple4: () => (props) => (
        <>
            {Array.from({ length: 4 }, (_, index) =>
                props.embeddedItemTemplates(index)(undefined)({
                    ...RendererPropsDomain(
                        props
                    ).Operations.AugmentingCustomPresentationContext({
                        sum: undefined,
                    }),
                    view: unit,
                })
            )}
        </>
    ),
    tuple5: () => (props) => (
        <>
            {Array.from({ length: 5 }, (_, index) =>
                props.embeddedItemTemplates(index)(undefined)({
                    ...RendererPropsDomain(
                        props
                    ).Operations.AugmentingCustomPresentationContext({
                        sum: undefined,
                    }),
                    view: unit,
                })
            )}
        </>
    ),
    tuple6: () => (props) => (
        <>
            {Array.from({ length: 6 }, (_, index) =>
                props.embeddedItemTemplates(index)(undefined)({
                    ...RendererPropsDomain(
                        props
                    ).Operations.AugmentingCustomPresentationContext({
                        sum: undefined,
                    }),
                    view: unit,
                })
            )}
        </>
    ),
    tuple7: () => (props) => (
        <>
            {Array.from({ length: 7 }, (_, index) =>
                props.embeddedItemTemplates(index)(undefined)({
                    ...RendererPropsDomain(
                        props
                    ).Operations.AugmentingCustomPresentationContext({
                        sum: undefined,
                    }),
                    view: unit,
                })
            )}
        </>
    ),
    tuple8: () => (props) => (
        <>
            {Array.from({ length: 8 }, (_, index) =>
                props.embeddedItemTemplates(index)(undefined)({
                    ...RendererPropsDomain(
                        props
                    ).Operations.AugmentingCustomPresentationContext({
                        sum: undefined,
                    }),
                    view: unit,
                })
            )}
        </>
    ),
    tuple9: () => (props) => (
        <>
            {Array.from({ length: 9 }, (_, index) =>
                props.embeddedItemTemplates(index)(undefined)({
                    ...RendererPropsDomain(
                        props
                    ).Operations.AugmentingCustomPresentationContext({
                        sum: undefined,
                    }),
                    view: unit,
                })
            )}
        </>
    ),
    
    comparisonValueOnlyContainer8: () => () => <></>,
    tabWithMetadata5: () => (props) => {
        // TODO: render metadata components
        return (
            <>
                {props.embeddedItemTemplates(0)(undefined)({
                    ...RendererPropsDomain(
                        props
                    ).Operations.AugmentingCustomPresentationContext({
                        sum: undefined,
                    }),
                    view: unit,
                })}
            </>
        );
    },
    valueOnlyContainer5: () => () => <></>,
    valueOnlyContainer6: () => () => <></>,
} satisfies IdeConcreteRenderers["tuple"];

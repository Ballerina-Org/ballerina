import {
    BaseBooleanCheckboxSwitcher,
    BaseBooleanCheckboxSwitcherProps,
    BaseBooleanSwitcher,
    BaseBooleanSwitcherProps,
    BaseCheckboxWithLabel,
    BaseFieldWithoutMetadata,
} from "@blp-private-npm/ui";

import { BoolAbstractRendererView, PredicateValue } from "ballerina-core";
import React from "react";
import { CustomPresentationContexts } from "../common/custom-presentation-contexts";
import {IdeFlags} from "../common/ide-flags.ts";
import { FieldExtraContext } from "../common/field-extra-context.ts";
import {
    translateForCustomDataDrivenTranslations,
    translateForDataDrivenTranslationsWithContext
} from "../common/translate.ts";
import {IdeConcreteRenderers} from "../common/concrete-renderers.ts";


type BoolComponentProps = React.ComponentProps<
    BoolAbstractRendererView<CustomPresentationContexts, IdeFlags, FieldExtraContext>
>;
const withTooltip =
    (Component: React.ComponentType<BoolComponentProps>) =>
        (props: BoolComponentProps) => {
            const ddTranslationsWithCtx = translateForDataDrivenTranslationsWithContext(
                props.context.extraContext.locale,
                props.context.extraContext.namespace
            );

            return (
                <Component
                    {...props}
                    context={{
                        ...props.context,
                        customPresentationContext: {
                            ...props.context.customPresentationContext,
                            tooltip: ddTranslationsWithCtx(
                                props.context.label,
                                props.context.labelContext,
                                props.context.domNodeId
                            ),
                        },
                    }}
                />
            );
        };

const BoolIsDiscountComponent = (props: BoolComponentProps) => {
    const ddTranslations = translateForCustomDataDrivenTranslations(
        props.context.extraContext.locale,
        props.context.extraContext.namespace
    );

    const booleanSwitcherProps = {
        variant: props.context?.customPresentationContext?.isCell
            ? "cell"
            : "outlined",
        disabled: props.context.disabled,
        readOnly: props.context.readOnly,
        value: PredicateValue.Operations.IsUnit(props.context.value)
            ? false
            : props.context.value,
        onChange: (v) => props.foreignMutations.setNewValue(v, undefined),
        customAiApplied: Boolean(
            props.context?.customPresentationContext?.customAiApplied
        ),
        startAdornment: props.context?.customPresentationContext?.customAiApplied,
        isWrapped: props.context?.customPresentationContext?.isWrapped,
        endAdornment: props.context?.customPresentationContext?.endAdornment,
        tooltip: props.context.customPresentationContext?.tooltip,
        compact: true,
        onState: {
            label: ddTranslations("discount"),
            icon: "minus",
            activeColor: "success",
        },
        offState: {
            label: ddTranslations("surcharge"),
            icon: "plus",
            activeColor: "error",
        },
    } satisfies BaseBooleanSwitcherProps;

    return <BaseBooleanSwitcher {...booleanSwitcherProps} />;
};

const BoolComponent = (props: BoolComponentProps) => {
    const booleanSwitcherProps = {
        variant: props.context?.customPresentationContext?.isCell
            ? "cell"
            : "outlined",
        disabled: props.context.disabled,
        readOnly: props.context.readOnly,
        value: PredicateValue.Operations.IsUnit(props.context.value)
            ? false
            : props.context.value,
        onChange: (v) => props.foreignMutations.setNewValue(v, undefined),
        customAiApplied: Boolean(
            props.context?.customPresentationContext?.customAiApplied
        ),
        startAdornment: props.context?.customPresentationContext?.customAiApplied,
        isWrapped: props.context?.customPresentationContext?.isWrapped,
        endAdornment: props.context?.customPresentationContext?.endAdornment,
        tooltip: props.context.customPresentationContext?.tooltip,
    } satisfies BaseBooleanCheckboxSwitcherProps;

    return <BaseBooleanCheckboxSwitcher {...booleanSwitcherProps} />;
};

export const BooleanFieldViews = {
    boolean: () => (props) => <BoolComponent {...props} />,
    booleanCell: () => (props) => (
        <BoolComponent
            {...props}
            context={{
                ...props.context,
                customPresentationContext: {
                    ...props.context.customPresentationContext,
                    isCell: true,
                },
            }}
        />
    ),
    booleanWithLabel: () => (props) => {
        const ddTranslationsWithCtx = translateForDataDrivenTranslationsWithContext(
            props.context.extraContext.locale,
            props.context.extraContext.namespace
        );

        return (
            <BaseCheckboxWithLabel
                label={ddTranslationsWithCtx(
                    props.context.label,
                    props.context.labelContext,
                    props.context.domNodeId
                )}
                searchedValue={
                    props.context?.customPresentationContext?.highlightLabelValue
                }
                checkbox={{
                    disabled: props.context.disabled || props.context.readOnly,
                    value: PredicateValue.Operations.IsUnit(props.context.value)
                        ? false
                        : props.context.value,
                    onClick: (v) => {
                        props.foreignMutations.setNewValue(!!v, undefined);
                    },
                }}
            />
        );
    },
    booleanLabelInTooltip: () => (props) => {
        const ComponentWithTooltip = withTooltip(BoolComponent);
        return <ComponentWithTooltip {...props} />;
    },
    booleanNotEditable: () => (props) => {
        return (
            <BoolComponent
                {...{ ...props, context: { ...props.context, readOnly: true } }}
            />
        );
    },
    booleanNotEditableCell: () => (props) => {
        return (
            <BoolComponent
                {...{
                    ...props,
                    context: {
                        ...props.context,
                        readOnly: true,
                        customPresentationContext: {
                            ...props.context.customPresentationContext,
                            isCell: true,
                        },
                    },
                }}
            />
        );
    },
    booleanWithMenuContext: () => (props) => {
        return (
            <BaseFieldWithoutMetadata
                disabled={props.context.disabled || props.context.readOnly}
                domNodeId={props.context.domNodeId}
            >
                {({ showMenu, endAdornment }) => (
                    <BoolComponent
                        {...props}
                        context={{
                            ...props.context,
                            customPresentationContext: {
                                ...props.context.customPresentationContext,
                                isWrapped: showMenu,
                                endAdornment,
                            },
                        }}
                    />
                )}
            </BaseFieldWithoutMetadata>
        );
    },
    booleanIsDiscount: () => (props) => {
        return <BoolIsDiscountComponent {...props} />;
    },
    booleanIsDiscountWithLabelTooltip: () => (props) => {
        const ComponentWithTooltip = withTooltip(BoolIsDiscountComponent);
        return <ComponentWithTooltip {...props} />;
    },
} satisfies IdeConcreteRenderers["boolean"];

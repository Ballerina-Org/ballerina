import {
    BaseFieldWithoutMetadata,
    BaseLabelV3,
    BaseNumberInputV3,
    BaseReadOnlyNumberInputV3,
} from "@blp-private-npm/ui";
import { NumberAbstractRendererView } from "ballerina-core";
import debounce from "lodash.debounce";
import React, { useEffect, useRef } from "react";
import { IdeConcreteRenderers } from "../common/concrete-renderers";
import { CustomPresentationContexts } from "../common/custom-presentation-contexts";
import { IdeFlags } from "../common/ide-flags";
import {FieldExtraContext} from "../common/field-extra-context.ts";
import {THROTTLE_TIME} from "../common/dates.ts";
import {translateForDataDrivenTranslationsWithContext} from "../common/translate.ts";
import {useStateWithEffectAndCondition} from "../common/use-state.ts";

const NumberInputComponent = (
    props: React.ComponentProps<
        NumberAbstractRendererView<
            CustomPresentationContexts,
            IdeFlags,
            FieldExtraContext
        >
    >
) => {
    return props.context.readOnly ? (
        <ReadOnlyNumberInputComponent {...props} />
) : (
        <EditableNumberInputComponent {...props} />
);
};

const EditableNumberInputComponent = (
    props: React.ComponentProps<
        NumberAbstractRendererView<
            CustomPresentationContexts,
            IdeFlags,
            FieldExtraContext
        >
    >
) => {
    const isLocalDebouncerRunningRef = useRef(false);
    const [state, setState] = useStateWithEffectAndCondition(
        props.context.value,
        !isLocalDebouncerRunningRef.current
    );

    const debouncedRef = useRef(
        debounce(
            (value, flags) => {
                props.foreignMutations.setNewValue(value, flags);
                isLocalDebouncerRunningRef.current = false;
            },
            THROTTLE_TIME,
            {
                trailing: true,
                leading: false,
            }
        )
    );

    useEffect(() => {
        if (!isLocalDebouncerRunningRef.current) {
            debouncedRef.current = debounce(
                (value, flags) => {
                    props.foreignMutations.setNewValue(value, flags);
                    isLocalDebouncerRunningRef.current = false;
                },
                THROTTLE_TIME,
                {
                    trailing: true,
                    leading: false,
                }
            );
        }
    }, [props.foreignMutations]);

    //const mutations = useViewerMutationsContext();

    const handleClick = props.context.customPresentationContext
        ?.disableItemSelectionCallback
        ? undefined
        : (e: React.MouseEvent<HTMLElement, MouseEvent>) => {
            e.stopPropagation();
            // mutations.registerCellFloatCallback(
            //     (value) => {
            //         setState(value);
            //         props.foreignMutations.setNewValue(value, undefined);
            //     },
            //     "", // FIXME: registeredFieldName
            //     props.context.extraContext.foreignMutations.snackbar,
            //     props.context.extraContext.locale
            // );
        };

    const { locale } = props.context.extraContext;

    return (
        <BaseNumberInputV3
        
            variant={
            props.context.customPresentationContext?.isCell ? "text" : "outlined"
        }
    kind="int"
    shape={
        props.context.customPresentationContext?.isCell
            ? "rectangular"
            : "square"
    }
    fullWidth
    copyable={
        props.context.customPresentationContext?.disableCopyable ? false : true
    }
    value={state}
    onClick={handleClick}
    handleChange={(v) => {
        setState(() => v ?? 0);
        isLocalDebouncerRunningRef.current = true;
        debouncedRef.current(v ?? 0, undefined);
    }}
    clearCallback={
        props.context.customPresentationContext?.disableClearable
            ? undefined
            : () => {
                setState(0);
                props.foreignMutations.setNewValue(0, undefined);
            }
    }
    disabled={props.context.disabled}
    formatter={new Intl.NumberFormat("de-DE", {
        minimumFractionDigits: 0,
        maximumFractionDigits: 0,
    })}
    handleBlur={() => {
        if (state !== props.context.value) {
            debouncedRef.current.cancel();
            isLocalDebouncerRunningRef.current = false;
            props.foreignMutations.setNewValue(state, undefined);
        }
    }}
    />
);
};

const ReadOnlyNumberInputComponent = (
    props: React.ComponentProps<
        NumberAbstractRendererView<
            CustomPresentationContexts,
            IdeFlags,
            FieldExtraContext
        >
    >
) => {
    const { locale } = props.context.extraContext;

    return (
        <BaseReadOnlyNumberInputV3
            variant={"outlined"}
    shape={"square"}
    value={props.context.value}
    formatter={locale.floatFormatter}
    fullWidth
    copyable={
        props.context.customPresentationContext?.disableCopyable ? false : true
    }
    disabled={props.context.disabled}
    />
);
};

export const NumberFieldViews = {
    number: () => (props) => {
        return <NumberInputComponent {...props} />;
    },
    numberWithMenuContext: () => (props) => {
        return (
            <BaseFieldWithoutMetadata
                disabled={props.context.disabled}
        domNodeId={props.context.domNodeId}
            >
            {({ showMenu, endAdornment }) => (
            <NumberInputComponent
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
    numberWithLabel: () => (props) => {
        const ddTranslationsWithCtx = translateForDataDrivenTranslationsWithContext(
            props.context.extraContext.locale,
            props.context.extraContext.namespace
        );

        return (
            <BaseLabelV3
                dense
        label={ddTranslationsWithCtx(
                props.context.label,
            props.context.labelContext,
            props.context.domNodeId
    )}
        highlightedValue={
                props.context.customPresentationContext?.highlightLabelValue
            }
            >
            <NumberInputComponent {...props} />
        </BaseLabelV3>
    );
    },
    numberCell: () => (props) => {
        return (
            <NumberInputComponent
                {...props}
        context={{
        ...props.context,
                customPresentationContext: {
            ...props.context.customPresentationContext,
                    isCell: true,
            },
        }}
        />
    );
    },
    numberNotEditable: () => (props) => {
        const { locale } = props.context.extraContext;

        return (
            <BaseReadOnlyNumberInputV3
                variant={"outlined"}
        shape={"square"}
        value={props.context.value}
        formatter={locale.floatFormatter}
        fullWidth
        copyable={
            props.context.customPresentationContext?.disableCopyable
                ? false
                : true
        }
        disabled={props.context.disabled}
        />
    );
    },
} satisfies IdeConcreteRenderers["number"];

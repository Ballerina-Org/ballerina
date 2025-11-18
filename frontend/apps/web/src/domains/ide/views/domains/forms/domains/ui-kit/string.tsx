import {
    BaseFieldWithoutMetadata,
    BaseInputV3,
    BaseLabelV3,
    BaseTypography,
} from "@blp-private-npm/ui";
import { styled } from "@mui/material";
import { StringAbstractRendererView } from "ballerina-core";
import debounce from 'lodash.debounce';
import React, { useEffect, useRef } from "react";
import { FieldExtraContext } from "../common/field-extra-context";
import { CustomPresentationContexts } from "../common/custom-presentation-contexts";
import {THROTTLE_TIME} from "../common/dates.ts";
import {useStateWithEffectAndCondition} from "../common/use-state.ts";
import {IdeFlags} from "../common/ide-flags.ts";
import {IdeConcreteRenderers} from "../common/concrete-renderers.ts";


// import { WeekSelector } from "@/components2/organisms/forms/dashboard-field-views/string/index";
// import { useStateWithEffectAndCondition } from "@/library/hooks/useStateWithEffectAndCondition";
// import { useViewerMutationsContext } from "@/web2/domains/document-viewer-v3/context";
//
// import { ReadonlyGuidFieldViews } from "./readonly-guid";
//import { ReadonlyStringFieldViews } from "./readonly-string";

const StyledContentWrapper = styled("div")(() => ({
    width: "100%",
}));

const StringInputComponent = (
    props: React.ComponentProps<
        StringAbstractRendererView<
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

    const handleClick = props.context?.customPresentationContext
        ?.disableItemSelectionCallback
        ? () => {}
        : (e: React.MouseEvent<HTMLElement, MouseEvent>) => {
            e.stopPropagation();
            // mutations.registerCellTextCallback(
            //     (text:any) => {
            //         setState(text);
            //         props.foreignMutations.setNewValue(text, undefined);
            //     },
            //     "" // FIXME: registeredFieldName
            // );
        };

    const customAiApplied =
        props.context?.customPresentationContext?.customAiApplied;

    return (
        <BaseInputV3
            variant={
                props.context?.customPresentationContext?.isCell ? "cell" : "outlined"
            }
            shape={
                props.context?.customPresentationContext?.isCell
                    ? "rectangular"
                    : "square"
            }
            kind={props.context.readOnly ? "readOnly" : "editable"}
            fullWidth
            copyable={
                props.context.customPresentationContext?.disableCopyable ? false : true
            }
            value={state}
            onClick={handleClick}
            handleChange={(v) => {
                setState(() => v);
                isLocalDebouncerRunningRef.current = true;
                debouncedRef.current(v, undefined);
            }}
            clearCallback={
                props.context.customPresentationContext?.disableClearable
                    ? undefined
                    : () => {
                        setState("");
                        props.foreignMutations.setNewValue("", undefined);
                    }
            }
            disabled={props.context.disabled}
            startAdornment={customAiApplied ? customAiApplied : undefined}
            customAiApplied={Boolean(customAiApplied)}
            isWrapped={props.context?.customPresentationContext?.isWrapped}
            endAdornment={props.context?.customPresentationContext?.endAdornment}
            placeholder={props.context?.customPresentationContext?.placeholder}
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



export const StringFieldViews = {
    string: () => (props) => <StringInputComponent {...props} />,

} satisfies IdeConcreteRenderers["string"];

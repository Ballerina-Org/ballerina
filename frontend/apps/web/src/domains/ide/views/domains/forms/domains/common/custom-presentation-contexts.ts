import React, {ReactNode} from "react";
import {PredicateValue, SimpleCallback, VoidCallbackWithOptionalFlags} from "ballerina-core";
import { IdeFlags } from "./ide-flags";

export type CustomPresentationContexts = {
    customAiApplied?: ReactNode;
    endAdornment?: ReactNode;
    isWrapped?: boolean;
    highlightLabelValue?: string;
    hideLabel?: boolean;
    disableItemSelectionCallback?: boolean;
    detailsDirection?: "row" | "column";
    isLast?: boolean;
    placeholder?: string;
    inPortalContext?: boolean;
    detailsPortalRef?: HTMLElement | null;
    positionsPortalContainerRef?: (
        id: string,
        node: HTMLDivElement | null
    ) => void;
    positionDetailsPortalTabNames?: string[];
    containerActions?: {
        add?: VoidCallbackWithOptionalFlags<IdeFlags>;
        insert?: VoidCallbackWithOptionalFlags<IdeFlags>;
        remove?: VoidCallbackWithOptionalFlags<IdeFlags>;
        duplicate?: VoidCallbackWithOptionalFlags<IdeFlags>;
        move?: (elementIndex: number, to: number, flags: IdeFlags | undefined) => void;
    };
    tuple?: {
        automationsIndex?: number;
        valueIndex?: number;
        failingChecksIndex?: number;
        failingChecksStatusIndex?: number;
        failingFilterGroupChecksIndex?: number;
        automationIndex?: number;
        statusComponentIndex?: number;
        approvalIndex?: number;
        customAIOutputIndex?: number;
        evidenceIndex?: number;
        evidenceValue?: PredicateValue;
        isEvidenceReadOnly?: boolean;
        comparedValueIndex?: number;
        contentContainerStyle?: React.CSSProperties;
        nodeContainerStyle?: React.CSSProperties;
    };
    disableCopyable?: boolean;
    disableClearable?: boolean;
    isCell?: boolean;
    tooltip?: string;
    fieldStyle?: React.CSSProperties;
    table?: {
        columnsSize?: number;
        rowsSize?: number;
        emptyLastColumn?: boolean;
        isFirstRow?: boolean;
        isLastRow?: boolean;
        subtotalHeader?: boolean;
        selectCurrentRow?: SimpleCallback<void>;
    };
    sum?: {
        clearCallback?: SimpleCallback<void>;
    };
    list?: {
        elementLabel?: string;
    };
};
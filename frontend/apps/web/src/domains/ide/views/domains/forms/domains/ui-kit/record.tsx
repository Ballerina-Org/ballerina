import {
    BaseAccordion,
    BaseAccordionDetails,
    BaseAccordionItem,
    BaseAccordionSummary,
    BaseAttentionBoxV3,
    BaseButtonGroupV3,
    BaseButtonV3,
    BaseCardFormSingleField,
    BaseCellContainer,
    BaseColumnWrapper,
    BaseConfigurationModalLayoutV3,
    BaseContainerWithBottomActionsPropsV3,
    BaseContainerWithBottomActionsV3,
    BaseDivider,
    BaseDragAndDropAreaV3,
    BaseDropdownOptionPreview,
    BaseDropdownOptionPreviewSection,
    BaseExpandableTabs,
    BaseFormColumns,
    BaseIconButtonV3,
    BaseLabelV3,
    BaseNoContent,
    BaseSectionWrapper,
    BaseSplitPaneV3,
    BaseTableGrid,
    BaseTableHeadV3,
    BaseTypography,
    BaseVirtualized,
    BaseVirtualizedList,
    DividerRenderItem,
    DropzoneFile,
    KeyValueRenderItem,
    StyledBaseRow as BaseTableRow,
    TabV3,
} from "@blp-private-npm/ui";
import {
    BaseSplitResponsivePane,
    BaseSplitResponsivePaneProps,
} from "@blp-private-npm/ui";
import { Stack, styled, useTheme } from "@mui/material";

import {
    AsyncState,
    CalculatedColumnLayout,
    CalculatedTabLayout,
    PredicateValue,
    RecordAbstractRendererView,
    unit,
} from "ballerina-core";
import { Map } from "immutable";
import {
    ComponentProps,
    Fragment,
    useCallback,
    useEffect,
    useRef,
    useState,
} from "react";
import { createPortal } from "react-dom";
import { v4 as uuidv4 } from "uuid";
import {IdeConcreteRenderers, RendererPropsDomain} from "../common/concrete-renderers.ts";
import {translateForDataDrivenTranslationsWithContext} from "../common/translate.ts";


const CardTabWrapper = styled("div")(({ theme: { web3 } }) => ({
    margin: `${web3.spacing.s} ${web3.spacing.m} ${web3.spacing.s} ${web3.spacing.m}`,
}));
CardTabWrapper.displayName = "CardTabWrapper";

const DetailsWrapper = styled("div")({
    gridColumn: "1 / -1",
    gridRow: 2,
});
DetailsWrapper.displayName = "DetailsWrapper";

const NotesWrapper = styled("div")({
    gridColumn: "1 / -1",
    gridRow: 3,
});
NotesWrapper.displayName = "NotesWrapper";

const DependantRecordsWrapper = styled("div")(({ theme: { web3 } }) => ({
    display: "flex",
    flexDirection: "column",
    gap: web3.spacing.xs,
    width: "100%",
}));
DependantRecordsWrapper.displayName = "DependantRecordsWrapper";



const PaymentSectionWrapper = styled("div")({
    display: "flex",
    flexDirection: "column",
});
PaymentSectionWrapper.displayName = "PaymentSectionWrapper";
type RecordSplitResponsivePaneProps = BaseSplitResponsivePaneProps;

const RecordSplitResponsivePane = (props: RecordSplitResponsivePaneProps) => {
    const { web3 } = useTheme();
    return (
        <BaseSplitResponsivePane
            {...props}
            style={{
                padding: `${web3.spacing.s} ${web3.spacing.xs} ${web3.spacing.s} ${web3.spacing.m}`,
            }}
        />
    );
};

export default RecordSplitResponsivePane;

export const RecordFieldViews = {
    containerRecord: () => (props) => {
        const ddTranslationsWithCtx = translateForDataDrivenTranslationsWithContext(
            props.context.extraContext.locale,
            props.context.extraContext.namespace
        );

        return (
            <Fragment>
                <RecordSplitResponsivePane
                    children={
                        <Fragment>
                            {props.context.layout.valueSeq().map((tab) =>
                                tab.columns.valueSeq().map((col) =>
                                    col.groups.valueSeq().map((fields, idx) =>
                                        fields
                                            .filter((fieldName) =>
                                                props.VisibleFieldKeys.has(fieldName)
                                            )
                                            .map((fieldName, fieldIdx) => {
                                                const extendedProps = {
                                                    ...props,
                                                    context: {
                                                        ...props.context,
                                                        disabled:
                                                            props.context.disabled ||
                                                            props.DisabledFieldKeys.has(fieldName),
                                                    },
                                                };
                                                return props.FieldLabels.get(fieldName) ? (
                                                    <KeyValueRenderItem
                                                        key={fieldIdx}
                                                        keyNode={
                                                            <BaseTypography variant="body" ellipsis>
                                                                {ddTranslationsWithCtx(
                                                                    props.FieldLabels.get(fieldName),
                                                                    props.context.labelContext,
                                                                    fieldName
                                                                )}
                                                            </BaseTypography>
                                                        }
                                                        valueNode={props.EmbeddedFields.get(fieldName)?.(
                                                            undefined
                                                        )({
                                                            ...RendererPropsDomain(
                                                                extendedProps
                                                            ).Operations.AugmentingCustomPresentationContext({
                                                                sum: undefined,
                                                            }),
                                                            view: unit,
                                                        })}
                                                    />
                                                ) : (
                                                    <Fragment key={fieldIdx}>
                                                        {props.EmbeddedFields.get(fieldName)?.(undefined)({
                                                            ...RendererPropsDomain(
                                                                extendedProps
                                                            ).Operations.AugmentingCustomPresentationContext({
                                                                sum: undefined,
                                                                isLast: idx === fields.length - 1,
                                                            }),
                                                            view: unit,
                                                        })}
                                                    </Fragment>
                                                );
                                            })
                                    )
                                )
                            )}
                        </Fragment>
                    }
                />
            </Fragment>
        );
    },
    dashboardConfig: () => (props) => {
        const ddTranslationsWithCtx = translateForDataDrivenTranslationsWithContext(
            props.context.extraContext.locale,
            props.context.extraContext.namespace
        );
        const [highlightLabelValue, setSearchInputValue] = useState("");
        return (
            <BaseConfigurationModalLayoutV3
                hideSidebar={false}
                searchInputProps={{
                    value: highlightLabelValue,
                    handleChange: setSearchInputValue,
                    clearCallback: () => setSearchInputValue(""),
                }}
                configurations={props.context.layout
                    .entrySeq()
                    .map(([key, tab]) => ({
                        id: key,
                        title: ddTranslationsWithCtx(key, props.context.labelContext, key),
                        sections: tab.columns
                            .entrySeq()
                            .map(([colName, colLayout]) => ({
                                id: colName,
                                title: ddTranslationsWithCtx(
                                    colName,
                                    props.context.labelContext,
                                    colName
                                ),
                                icon: "chevronRight" as const,
                                fields: colLayout.groups.valueSeq().map((group: string[]) =>
                                    group
                                        .filter((fieldName: string) =>
                                            props.VisibleFieldKeys.has(fieldName)
                                        )
                                        .map((fieldName: string) => {
                                            const extendedProps = {
                                                ...props,
                                                context: {
                                                    ...props.context,
                                                    label: fieldName,
                                                    disabled:
                                                        props.context.disabled ||
                                                        props.DisabledFieldKeys.has(fieldName),
                                                },
                                            };
                                            return (
                                                <Fragment key={fieldName}>
                                                    {props.EmbeddedFields.get(fieldName)?.(undefined)({
                                                        ...RendererPropsDomain(
                                                            extendedProps
                                                        ).Operations.AugmentingCustomPresentationContext({
                                                            sum: undefined,
                                                            highlightLabelValue: highlightLabelValue,
                                                        }),
                                                        view: unit,
                                                    })}
                                                </Fragment>
                                            );
                                        })
                                ),
                            }))
                            .toArray(),
                    }))
                    .toArray()}
            />
        );
    },

} satisfies IdeConcreteRenderers["record"];

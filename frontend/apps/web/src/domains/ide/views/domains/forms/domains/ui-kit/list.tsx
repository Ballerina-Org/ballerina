import {
    AttentionBoxVariant,
    BaseAttentionBoxV3,
    BaseButtonV3,
    BaseInfoBanner,
    BaseInputWithDelete,
} from "@blp-private-npm/ui";
import {
    DispatchParsedType,
    ListAbstractRendererView,
    Option,
    PredicateValue,
    unit,
    ValueTuple,
    ValueUnit,
} from "ballerina-core";
import { List } from "immutable";
import React, { Fragment, useState } from "react";
import { v4 as uuidv4 } from "uuid";

import { styled } from "@mui/material";
import {FL, PC} from "../../forms.tsx";
import {IdeConcreteRenderers, RendererPropsDomain} from "../common/concrete-renderers.ts";
import { CustomPresentationContexts } from "../common/custom-presentation-contexts.ts";
import { IdeFlags } from "../common/ide-flags.ts";
import { FieldExtraContext } from "../common/field-extra-context.ts";
import {translateForCustomDataDrivenTranslations} from "../common/translate.ts";
import {mapTypeToFlags} from "../common/map-type-to-flag.ts";
import {SketchType} from "../common/sketch.ts";

const DefaultListWrapper = styled("div")(({ theme: { web3 } }) => ({
    flex: 1,
    display: "flex",
    flexDirection: "column",
    gap: web3.spacing.s,
}));

DefaultListWrapper.displayName = "DefaultListWrapper";

const ErrorList = (
    props: React.ComponentProps<
        ListAbstractRendererView<
            CustomPresentationContexts & PC,
            IdeFlags,
            FieldExtraContext
        >
    > & {
        size?: "m" | "s";
        noErrorsView?: React.ReactNode;
    }
) => {
    const ddTranslations = translateForCustomDataDrivenTranslations(
        props.context.extraContext.locale,
        props.context.extraContext.namespace
    );

    const value = props.context.value;

    if (PredicateValue.Operations.IsUnit(value)) {
        console.error(`Non partial list renderer called with unit value`);
        return <></>;
    }

    const noErrorsView = props.noErrorsView ?? (
        <BaseAttentionBoxV3
            text={ddTranslations("noErrorItems")}
            variant={"info"}
            size={props.size ?? "m"}
            fullWidth
        />
    );
    return (
        <>
            {value.values.size > 0
                ? value.values
                    .valueSeq()
                    .map((_, elementIndex) => {
                        return (
                            <Fragment key={elementIndex}>
                                {props.embeddedElementTemplate(elementIndex)(undefined)({
                                    ...RendererPropsDomain(
                                        props
                                    ).Operations.AugmentingCustomPresentationContext({
                                        sum: undefined,
                                    }),
                                    view: unit,
                                })}
                            </Fragment>
                        );
                    })
                    .toArray()
                : noErrorsView}
        </>
    );
};

export const ListFieldViews = {
    list:
        (): ListAbstractRendererView<
            CustomPresentationContexts & PC,
            FL,
            FieldExtraContext
        > =>
            (props) => {
                const ddTranslations = translateForCustomDataDrivenTranslations(
                    props.context.extraContext.locale,
                    props.context.extraContext.namespace
                );

                const addLabel = ddTranslations("addNewListElement");
                const deleteLabel = ddTranslations("removeNewListElement");

                const value = props.context.value;

                if (PredicateValue.Operations.IsUnit(value)) {
                    console.error(`Non partial list renderer called with unit value`);
                    return <></>;
                }

                const flags = mapTypeToFlags({
                    ancestors: props.context.lookupTypeAncestorNames,
                });

                const isReadonly = props.context.readOnly;

                return (
                    <DefaultListWrapper>
                        {value.values.map((_: any, elementIndex: number) => {
                            return props.foreignMutations.add && !isReadonly ? (
                                <BaseInputWithDelete
                                    key={elementIndex}
                                    onDelete={() => {
                                        props.foreignMutations.remove?.(elementIndex, flags);
                                    }}
                                    showDelete
                                    deleteButtonContent={deleteLabel}
                                >
                                    {props.embeddedElementTemplate(elementIndex)(undefined)({
                                        ...RendererPropsDomain(
                                            props
                                        ).Operations.AugmentingCustomPresentationContext({
                                            sum: undefined,
                                        }),
                                        view: unit,
                                    })}
                                </BaseInputWithDelete>
                            ) : (
                                <div style={{ position: "relative" }}>
                                    {props.embeddedElementTemplate(elementIndex)(undefined)({
                                        ...RendererPropsDomain(
                                            props
                                        ).Operations.AugmentingCustomPresentationContext({
                                            sum: undefined,
                                        }),
                                        view: unit,
                                    })}
                                </div>
                            );
                        })}

                        {props.foreignMutations.add && !isReadonly && (
                            <BaseButtonV3
                                id={uuidv4()}
                                variant="text"
                                shape="square"
                                colorVariant="secondary"
                                onClick={() => {
                                    props.foreignMutations.add!(flags)();
                                }}
                                StartIcon="plus"
                                fullWidth
                            >
                                {addLabel}
                            </BaseButtonV3>
                        )}
                    </DefaultListWrapper>
                );
            },
    errorList: () => (props:any) => <ErrorList {...props} />,
    cardErrorList: () => (props:any) => {
        const ddTranslations = translateForCustomDataDrivenTranslations(
            props.context.extraContext.locale,
            props.context.extraContext.namespace
        );

        return (
            <ErrorList
                {...props}
                noErrorsView={
                    <BaseInfoBanner
                        hideIcon
                        size="m"
                        type="text"
                        text={ddTranslations("noErrors")}
                        imageSrc={SketchType.Operations.getSketchPath("happy-baloons")}
                        variant="rounded"
                    />
                }
            />
        );
    },
    errorListSmall: () => (props) => <ErrorList {...props} size="s" />,
    failingCheckList: () => (props) => {
        const value = props.context.value;

        if (PredicateValue.Operations.IsUnit(value)) {
            console.error(`Non partial list renderer called with unit value`);
            return <></>;
        }

        const checks = value.values
            .filter((check) => PredicateValue.Operations.IsRecord(check))
            .map((check) => {
                const message = check.fields.get("FailingCheckMessage");
                const isResolvedWithApproving = check.fields.get(
                    "IsResolvedWithApproving"
                );
                const isFailing = check.fields.get("IsFailing");
                return {
                    message: !message
                        ? ""
                        : PredicateValue.Operations.IsString(message)
                            ? message
                            : message.toString(),
                    status: isResolvedWithApproving
                        ? "success"
                        : isFailing
                            ? "warning"
                            : "info",
                };
            })
            .toArray();

        return (
            <>
                {checks.map((el, k) => (
                    <BaseAttentionBoxV3
                        key={k}
                        text={el.message}
                        variant={el.status as AttentionBoxVariant}
                    />
                ))}
            </>
        );
    },

} satisfies IdeConcreteRenderers["list"];

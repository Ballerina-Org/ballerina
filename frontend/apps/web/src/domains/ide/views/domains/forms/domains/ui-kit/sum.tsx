import { BaseButtonV3, BaseInfoBannerWithModal } from "@blp-private-npm/ui";
import { styled } from "@mui/material";
import {
  DispatchCommonFormState,
  DispatchDeltaSum,
  DispatchParsedType,
  Option,
  PredicateValue,
  replaceWith,
  Sum,
  unit,
  ValueSum,
} from "ballerina-core";
import { OrderedMap } from "immutable";
import React from "react";
import { v4 as uuidv4 } from "uuid";
import {IdeConcreteRenderers} from "../common/concrete-renderers.ts";
import {IdeFlags} from "../common/ide-flags.ts";
import {translateForCustomDataDrivenTranslations} from "../common/translate.ts";
import {SketchType} from "../common/sketch.ts";


//import { SketchType } from "@/web2/domains/sketches/state";

const StyledCustomAiBannerWrapper = styled("div")(({ theme: { web3 } }) => ({
  display: "flex",
  flexDirection: "column",
  width: "100%",
  padding: `${web3.spacing.m} ${web3.spacing.s} ${web3.spacing.xs} ${web3.spacing.m}`,
  position: "relative",
  height: "100%",
  boxSizing: "border-box",
}));

export const SumFieldViews = {
  sum: () => (props) => {
    if (PredicateValue.Operations.IsUnit(props.context.value)) {
      return <>nth</>;
    }

    return (
      <>
        {props.context.value.value.kind == "l"
          ? props.embeddedLeftTemplate?.(undefined)({
              ...props,
              view: unit,
            })
          : props.embeddedRightTemplate?.(undefined)({
              ...props,
              context: {
                ...props.context,
                customPresentationContext: {
                  ...props.context.customPresentationContext,
                  sum: {
                    ...props.context.customPresentationContext?.sum,
                    clearCallback: props.context.readOnly
                      ? undefined
                      : () => {
                          props.foreignMutations.onChange(
                            Option.Default.some(
                              replaceWith(
                                PredicateValue.Default.sum(
                                  Sum.Default.left(
                                    PredicateValue.Default.unit()
                                  )
                                )
                              )
                            ),
                            {
                              kind: "SumReplace",
                              replace: PredicateValue.Default.sum(
                                Sum.Default.left(PredicateValue.Default.unit())
                              ),
                              state: {
                                commonFormState: props.context.commonFormState,
                                customFormState: props.context.customFormState,
                              },
                              type: props.context.type,
                              flags: undefined,
                              sourceAncestorLookupTypeNames:
                                props.context.lookupTypeAncestorNames,
                            }
                          );
                        },
                  },
                },
              },
              view: unit,
            })}
      </>
    );
  },
  switchableSum: () => (props) => {
    if (PredicateValue.Operations.IsUnit(props.context.value)) {
      return <></>;
    }

    return <>WIP</>;
  },
  onlyRight: () => (props) => {
    return (
      <>
        {props.embeddedRightTemplate?.(undefined)({
          ...props,
          context: {
            ...props.context,
            customPresentationContext: {
              ...props.context.customPresentationContext,
              sum: {
                ...props.context.customPresentationContext?.sum,
                clearCallback: props.context.readOnly
                  ? undefined
                  : () => {
                      props.foreignMutations.onChange(
                        Option.Default.some(
                          replaceWith(
                            PredicateValue.Default.sum(
                              Sum.Default.left(PredicateValue.Default.unit())
                            )
                          )
                        ),
                        {
                          kind: "SumReplace",
                          replace: PredicateValue.Default.sum(
                            Sum.Default.left(PredicateValue.Default.unit())
                          ),
                          state: {
                            commonFormState: props.context.commonFormState,
                            customFormState: props.context.customFormState,
                          },
                          type: props.context.type,
                          flags: undefined,
                          sourceAncestorLookupTypeNames:
                            props.context.lookupTypeAncestorNames,
                        }
                      );
                    },
              },
            },
          },
          view: unit,
        })}
      </>
    );
  },
  readonlySum: () => (props) => {
    if (PredicateValue.Operations.IsUnit(props.context.value)) {
      return <></>;
    }

    return (
      <>
        {props.context.value.value.kind == "l"
          ? props.embeddedLeftTemplate?.(undefined)({
              ...props,
              view: unit,
            })
          : props.embeddedRightTemplate?.(undefined)({
              ...props,
              view: unit,
            })}
      </>
    );
  },
  customAiBanner: () => (props) => {
    const translate = translateForCustomDataDrivenTranslations(
      props.context.extraContext.locale,
      props.context.extraContext.namespace
    );

    const hasCustomAi =
      PredicateValue.Operations.IsSum(props.context.value) &&
      props.context.value.value.kind === "r";

    return (
      <>
        {hasCustomAi && (
          <StyledCustomAiBannerWrapper>
            <BaseInfoBannerWithModal
              text={translate("customAi.card.banner.content")}
              sketch={SketchType.Operations.getSketchPath("robot")!}
              buttonText={translate("customAi.card.banner.button")}
              modalContent={
                <>
                  {props.embeddedRightTemplate?.(undefined)({
                    ...props,
                    view: unit,
                  })}
                </>
              }
            />
          </StyledCustomAiBannerWrapper>
        )}
      </>
    );
  },
  localStateFormRight: () => (props) => {
    // we need to manually build the default right value to provide to the updater
    // NOTE: it would be possible to avoid this and make this component reusable
    // if we want to do this, all we need to do would be to update the abstract renderer
    // to get the default value constructor from its context and pass it to the
    // concrete renderer context

    const defaultRightValue = ValueSum.Default(
      Sum.Default.right<PredicateValue, PredicateValue>(
        PredicateValue.Default.record(
          OrderedMap<string, PredicateValue>([
            ["IsDiscount", PredicateValue.Default.boolean()],
            [
              "ValueKind",
              PredicateValue.Default.option(
                false,
                PredicateValue.Default.unit()
              ),
            ],
            ["Value", PredicateValue.Default.number()],
          ])
        )
      )
    );

    const buildDelta = (
      replace: PredicateValue,
      commonFormState: DispatchCommonFormState,
      type: DispatchParsedType<Sum<PredicateValue, PredicateValue>>,
      lookupTypeAncestorNames: string[]
    ) => {
      const delta: DispatchDeltaSum<IdeFlags> = {
        kind: "SumReplace",
        replace,
        state: commonFormState,
        type: type,
        flags: {
          kind: "localAndRemote",
          customLocks: [],
          lockedCards: ["PositionsCard"],
        },
        sourceAncestorLookupTypeNames: lookupTypeAncestorNames,
      };

      return delta;
    };

    const ddTranslations = translateForCustomDataDrivenTranslations(
      props.context.extraContext.locale,
      props.context.extraContext.namespace
    );

    const [localSumState, setLocalSumState] = React.useState(defaultRightValue);

    const valueKind =
      PredicateValue.Operations.IsSum(localSumState) &&
      PredicateValue.Operations.IsRecord(localSumState.value.value)
        ? localSumState.value.value.fields.get("ValueKind")
        : undefined;

    const valueKindOptionSelected =
      valueKind &&
      PredicateValue.Operations.IsOption(valueKind) &&
      !PredicateValue.Operations.IsUnit(valueKind.value);

    const onSubmit = () => {
      props.foreignMutations.onChange(
        Option.Default.some(replaceWith(defaultRightValue)),
        buildDelta(
          localSumState,
          props.context.commonFormState,
          props.context.type,
          props.context.lookupTypeAncestorNames
        )
      );
    };

    return (
      <>
        {props.embeddedRightTemplate?.(undefined)({
          ...props,
          context: {
            ...props.context,
            value: localSumState,
          },
          foreignMutations: {
            ...props.foreignMutations,
            onChange: (upd, _) => {
              if (upd.kind == "l") {
                return;
              }

              setLocalSumState((prev) =>
                PredicateValue.Operations.IsUnit(prev.value.value)
                  ? defaultRightValue
                  : upd.value(prev)
              );
            },
          },
          view: unit,
        })}
        <BaseButtonV3
          id={uuidv4()}
          StartIcon="plusCircle"
          variant="text"
          colorVariant="success"
          onClick={(_) => {
            onSubmit();
            setLocalSumState(defaultRightValue);
          }}
          tooltip={{
            message: !valueKindOptionSelected
              ? ddTranslations("SelectFromOneOfDropdownOption")
              : ddTranslations("AddEntryTooltip"),
            tooltipPlacement: "top",
          }}
          disabled={!valueKindOptionSelected}
        >
          {ddTranslations("AddEntry")}
        </BaseButtonV3>
      </>
    );
  },
} satisfies IdeConcreteRenderers["sum"];

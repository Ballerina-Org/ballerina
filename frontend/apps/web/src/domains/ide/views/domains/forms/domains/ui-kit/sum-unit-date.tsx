import {
  BaseFieldWithoutMetadata,
  BaseInputV3,
  DatePicker,
} from "@blp-private-npm/ui";
import {
  DateAbstractRendererState,
  DispatchParsedType,
  Maybe,
  Option,
  PredicateValue,
  replaceWith,
  Sum,
  SumAbstractRendererState,
  SumAbstractRendererView,
  UnitAbstractRendererState,
} from "ballerina-core";
import React from "react";

import {
    ListElementCustomPresentationContext
} from "../../../../../../dispatched-passthrough-form/views/tailwind-renderers.tsx";
import {IdeFlags} from "../common/ide-flags.ts";
import { CustomPresentationContexts } from "../common/custom-presentation-contexts.ts";
import { FieldExtraContext } from "../common/field-extra-context.ts";
import {DATE_FORMAT} from "../common/dates.ts";
import {IdeConcreteRenderers} from "../common/concrete-renderers.ts";

const ReadOnlySumUnitDateComponent = (
  props: React.ComponentProps<
    SumAbstractRendererView<
      CustomPresentationContexts & { listElement: ListElementCustomPresentationContext },
      IdeFlags,
      FieldExtraContext
    >
  >
) => {
  const formatISODate = useFormatISODate();

  if (PredicateValue.Operations.IsUnit(props.context.value)) {
    return <></>;
  }

  const formattedDate =
    props.context.value.value.kind == "l"
      ? ""
      : formatISODate(
          (props.context.value.value.value as Date).toISOString(),
          DATE_FORMAT.DATE
        );

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
      kind="readOnly"
      fullWidth
      copyable={
        props.context.customPresentationContext?.disableCopyable ? false : true
      }
      value={formattedDate}
      onClick={() => {}}
      startAdornment={props.context?.customPresentationContext?.customAiApplied}
      customAiApplied={Boolean(
        props.context?.customPresentationContext?.customAiApplied
      )}
    />
  );
};

const MaybeDatePickerComponent = (
  props: React.ComponentProps<
    SumAbstractRendererView<
      CustomPresentationContexts & {  listElement: ListElementCustomPresentationContext;},
      IdeFlags,
      FieldExtraContext
    >
  > & {
    pickerKind?: React.ComponentProps<typeof DatePicker>["kind"];
  }
) => {
  if (PredicateValue.Operations.IsUnit(props.context.value)) {
    return <></>;
  }

  const dateString =
    props.context.value.value.kind == "l"
      ? ""
      : props.context.customFormState.right.commonFormState.modifiedByUser
        ? (props.context.customFormState.right as DateAbstractRendererState)
            .customFormState.possiblyInvalidInput
        : (props.context.value.value.value as Date).toISOString();

  const setNewValue = (_: Maybe<string>) => {
    props.setState(
      SumAbstractRendererState.Updaters.Core.customFormState((__) => ({
        ...__,
        right: DateAbstractRendererState.Updaters.Core.customFormState.children
          .possiblyInvalidInput(replaceWith(_))
          .then(
            DateAbstractRendererState.Updaters.Core.commonFormState((___) => ({
              ...___,
              modifiedByUser: true,
            }))
          )(__.right as DateAbstractRendererState),
      }))
    );
    const newValue = _ == undefined ? _ : new Date(_);
    setTimeout(() => {
      if (newValue == undefined) {
        return;
      }
      props.foreignMutations.onChange(
        newValue == undefined || isNaN(newValue.getTime())
          ? Option.Default.none()
          : Option.Default.some(
              replaceWith(
                PredicateValue.Default.sum(Sum.Default.right(newValue))
              )
            ),
        {
          kind: "SumReplace",
          replace: PredicateValue.Default.sum(Sum.Default.right(newValue)),
          state: {
            commonFormState: props.context.commonFormState,
            customFormState: props.context.customFormState,
          },
          type: DispatchParsedType.Default.sum([
            DispatchParsedType.Default.primitive("unit"),
            DispatchParsedType.Default.primitive("Date"),
          ]),
          flags: undefined,
          sourceAncestorLookupTypeNames: props.context.lookupTypeAncestorNames,
        }
      );
    }, 0);
  };

  const clearValue = () => {
    props.setState(
      SumAbstractRendererState.Updaters.Core.customFormState((__) => ({
        ...__,
        left: UnitAbstractRendererState.Updaters.Core.commonFormState(
          (___) => ({
            ...___,
            modifiedByUser: true,
          })
        )(__.left as UnitAbstractRendererState),
      }))
    );
    setTimeout(() => {
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
          type: DispatchParsedType.Default.sum([
            DispatchParsedType.Default.primitive("unit"),
            DispatchParsedType.Default.primitive("Date"),
          ]),
          flags: undefined,
          sourceAncestorLookupTypeNames: props.context.lookupTypeAncestorNames,
        }
      );
    }, 0);
  };

  if (props.context.readOnly) {
    return <ReadOnlySumUnitDateComponent {...props} />;
  }

  return (
    <DatePicker
      kind={props.pickerKind ?? "date"}
      language={props.context.extraContext.locale.language}
      variant={
        props.context?.customPresentationContext?.isCell ? "cell" : "outlined"
      }
      shape={
        props.context?.customPresentationContext?.isCell
          ? "rectangular"
          : "square"
      }
      value={dateString}
      disabled={props.context.disabled}
      onChange={(date) => {
        const isoDate = date?.toISO();
        if (isoDate) {
          setNewValue(isoDate);
        } else {
          clearValue();
        }
      }}
      startAdornment={props.context?.customPresentationContext?.customAiApplied}
      customAiApplied={Boolean(
        props.context?.customPresentationContext?.customAiApplied
      )}
      isWrapped={props.context?.customPresentationContext?.isWrapped}
      endAdornment={props.context?.customPresentationContext?.endAdornment}
      clearable={!props.context?.customPresentationContext?.disableClearable}
      placeholder={props.context?.customPresentationContext?.placeholder}
    />
  );
};

const MaybeStringComponent = (
  props: React.ComponentProps<
    SumAbstractRendererView<
      CustomPresentationContexts,
      IdeFlags,
      FieldExtraContext
    >
  >
) => {
  const setNewValue = (_: Maybe<string>) => {
    props.setState(
      SumAbstractRendererState.Updaters.Core.customFormState((__) => ({
        ...__,
      }))
    );
  };

  const clearValue = () => {
    props.setState(
      SumAbstractRendererState.Updaters.Core.customFormState((_) => ({
        ..._,
        left: UnitAbstractRendererState.Updaters.Core.commonFormState((__) => ({
          ...__,
          modifiedByUser: true,
        }))(_.left as UnitAbstractRendererState),
      }))
    );
  };

  return (
    <BaseInputV3
      variant="outlined"
      shape="square"
      kind={props.context.readOnly ? "readOnly" : "editable"}
      fullWidth
      copyable={
        props.context.customPresentationContext?.disableCopyable ? false : true
      }
      value={""}
      handleChange={setNewValue}
      clearCallback={
        props.context.customPresentationContext?.disableClearable
          ? undefined
          : clearValue
      }
    />
  );
};

export const SumUnitDateFieldViews = {
  maybeDate: () => (props) => {
    return <MaybeDatePickerComponent {...props} />;
  },
  maybeDateTime: () => (props) => {
    return <MaybeDatePickerComponent {...props} pickerKind="dateTime" />;
  },
  maybeDateWithMenuContext: () => (props) => {
    return (
      <BaseFieldWithoutMetadata
        disabled={props.context.disabled}
        domNodeId={props.context.domNodeId}
      >
        {({ showMenu, endAdornment }) => (
          <MaybeDatePickerComponent
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
  maybeDateCell: () => (props) => {
    return (
      <MaybeDatePickerComponent
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
  maybeString: () => (props) => {
    return <MaybeStringComponent {...props} />;
  },
  maybeStringWithMenuContext: () => (props) => {
    return (
      <BaseFieldWithoutMetadata
        disabled={props.context.disabled}
        domNodeId={props.context.domNodeId}
      >
        {({ showMenu, endAdornment }) => (
          <MaybeStringComponent
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
  maybeDateNotEditable: () => (props) => {
    return <ReadOnlySumUnitDateComponent {...props} />;
  },
  maybeDateNotEditableCell: () => (props) => {
    return <ReadOnlySumUnitDateComponent {...props} />;
  },
  maybeStringNotEditable: () => (props) => {
    return (
      <BaseInputV3
        variant={"outlined"}
        shape={"square"}
        kind="readOnly"
        fullWidth
        copyable={
          props.context.customPresentationContext?.disableCopyable
            ? false
            : true
        }
        value={
          PredicateValue.Operations.IsUnit(props.context.value)
            ? ""
            : props.context.value.value.kind == "l"
              ? ""
              : props.context.value.value.value.toString()
        }
        onClick={() => {}}
        startAdornment={
          props.context?.customPresentationContext?.customAiApplied
        }
        customAiApplied={Boolean(
          props.context?.customPresentationContext?.customAiApplied
        )}
      />
    );
  },
  maybeDateNotClearable: () => (props) => {
    return (
      <MaybeDatePickerComponent
        {...props}
        context={{
          ...props.context,
          customPresentationContext: {
            ...props.context.customPresentationContext,
            disableClearable: true,
          },
        }}
      />
    );
  },
} satisfies IdeConcreteRenderers["sumUnitDate"];

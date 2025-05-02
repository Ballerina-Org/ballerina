import React from "react";
import {
  BasicUpdater,
  CommonFormState,
  DispatchDelta,
  PredicateValue,
  Sum,
  Updater,
  Value,
  ValueSum,
} from "../../../../../../../../main";
import { Template } from "../../../../../../../../main";
import { FormLabel } from "../../../../../singleton/domains/form-label/state";
import { DispatchParsedType } from "../../../../deserializer/domains/specification/domains/types/state";
import { DispatchOnChange } from "../../../state";
import { SumAbstractRendererState, SumAbstractRendererView } from "./state";

export const SumAbstractRenderer = <
  LeftFormState extends { commonFormState: CommonFormState },
  RightFormState extends { commonFormState: CommonFormState },
  Context extends FormLabel & {
    disabled: boolean;
    type: DispatchParsedType<any>;
    identifiers: { withLauncher: string; withoutLauncher: string };
  },
  ForeignMutationsExpected,
>(
  leftTemplate?: Template<
    Value<PredicateValue> &
      LeftFormState & { disabled: boolean; extraContext: any },
    LeftFormState,
    {
      onChange: DispatchOnChange<PredicateValue>;
    }
  >,
  rightTemplate?: Template<
    Value<PredicateValue> &
      RightFormState & { disabled: boolean; extraContext: any },
    RightFormState,
    {
      onChange: DispatchOnChange<PredicateValue>;
    }
  >,
) => {
  const embeddedLeftTemplate = leftTemplate
    ?.mapContext((_: any): any => ({
      ..._.customFormState.left,
      disabled: _.disabled,
      value: _.value.value.value,
      bindings: _.bindings,
      extraContext: _.extraContext,
      identifiers: {
        withLauncher: _.identifiers.withLauncher.concat(`[left]`),
        withoutLauncher: _.identifiers.withoutLauncher.concat(`[left]`),
      },
    }))
    ?.mapState(
      (
        upd: BasicUpdater<LeftFormState>,
      ): Updater<SumAbstractRendererState<LeftFormState, RightFormState>> =>
        SumAbstractRendererState<
          LeftFormState,
          RightFormState
        >().Updaters.Core.customFormState((_) => ({
          ..._,
          left: upd(_.left),
        })),
    )
    .mapForeignMutationsFromProps<
      ForeignMutationsExpected & {
        onChange: DispatchOnChange<ValueSum>;
      }
    >(
      (
        props,
      ): {
        onChange: DispatchOnChange<PredicateValue>;
      } => ({
        onChange: (elementUpdater, nestedDelta) => {
          const delta: DispatchDelta = {
            kind: "SumLeft",
            value: nestedDelta,
          };
          props.foreignMutations.onChange(
            (_) => ({
              ..._,
              value: Sum.Updaters.left<PredicateValue, PredicateValue>(
                elementUpdater,
              )(_.value),
            }),
            delta,
          );
          props.setState((_) => ({
            ..._,
            commonFormState: {
              ..._.commonFormState,
              modifiedByUser: true,
            },
            customFormState: {
              ..._.customFormState,
              left: {
                ..._.customFormState.left,
                commonFormState: {
                  ..._.customFormState.left.commonFormState,
                  modifiedByUser: true,
                },
              },
            },
          }));
        },
      }),
    );

  const embeddedRightTemplate = rightTemplate
    ?.mapContext((_: any): any => ({
      ..._.customFormState.right,
      disabled: _.disabled,
      value: _.value.value.value,
      bindings: _.bindings,
      extraContext: _.extraContext,
      identifiers: {
        withLauncher: _.identifiers.withLauncher.concat(`[right]`),
        withoutLauncher: _.identifiers.withoutLauncher.concat(`[right]`),
      },
    }))
    .mapState(
      (
        upd: BasicUpdater<RightFormState>,
      ): Updater<SumAbstractRendererState<LeftFormState, RightFormState>> =>
        SumAbstractRendererState<
          LeftFormState,
          RightFormState
        >().Updaters.Core.customFormState((_) => ({
          ..._,
          right: upd(_.right),
        })),
    )
    .mapForeignMutationsFromProps<
      ForeignMutationsExpected & {
        onChange: DispatchOnChange<ValueSum>;
      }
    >(
      (
        props,
      ): ForeignMutationsExpected & {
        onChange: DispatchOnChange<PredicateValue>;
      } => ({
        ...props.foreignMutations,
        onChange: (elementUpdater, nestedDelta) => {
          const delta: DispatchDelta = {
            kind: "SumRight",
            value: nestedDelta,
          };
          props.foreignMutations.onChange(
            (_) => ({
              ..._,
              value: Sum.Updaters.right<PredicateValue, PredicateValue>(
                elementUpdater,
              )(_.value),
            }),
            delta,
          );
          props.setState((_) => ({
            ..._,
            commonFormState: {
              ..._.commonFormState,
              modifiedByUser: true,
            },
            customFormState: {
              ..._.customFormState,
              right: {
                ..._.customFormState.right,
                commonFormState: {
                  ..._.customFormState.right.commonFormState,
                  modifiedByUser: true,
                },
              },
            },
          }));
        },
      }),
    );

  return Template.Default<
    Context &
      Value<ValueSum> & {
        disabled: boolean;
        extraContext: any;
        identifiers: { withLauncher: string; withoutLauncher: string };
      },
    SumAbstractRendererState<LeftFormState, RightFormState>,
    ForeignMutationsExpected & {
      onChange: DispatchOnChange<ValueSum>;
    },
    SumAbstractRendererView<
      LeftFormState,
      RightFormState,
      Context,
      ForeignMutationsExpected
    >
  >((props) => {
    return (
      <span
        className={`${props.context.identifiers.withLauncher} ${props.context.identifiers.withoutLauncher}`}
      >
        <props.view
          {...props}
          context={{ ...props.context }}
          foreignMutations={{
            ...props.foreignMutations,
          }}
          embeddedLeftTemplate={embeddedLeftTemplate}
          embeddedRightTemplate={embeddedRightTemplate}
        />
      </span>
    );
  }).any([]);
};

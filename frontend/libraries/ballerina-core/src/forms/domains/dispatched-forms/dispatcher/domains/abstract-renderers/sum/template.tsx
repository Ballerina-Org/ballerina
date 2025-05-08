import {
  CommonAbstractRendererReadonlyContext,
  CommonAbstractRendererState,
  CommonFormState,
  DispatchCommonFormState,
  DispatchDelta,
  PredicateValue,
  replaceWith,
  Sum,
  Value,
  ValueSum,
} from "../../../../../../../../main";
import { Template } from "../../../../../../../../main";
import { FormLabel } from "../../../../../singleton/domains/form-label/state";
import {
  DispatchParsedType,
  SumType,
} from "../../../../deserializer/domains/specification/domains/types/state";
import { DispatchOnChange } from "../../../state";
import {
  SumAbstractRendererReadonlyContext,
  SumAbstractRendererState,
  SumAbstractRendererView,
} from "./state";

export const SumAbstractRenderer = <
  LeftFormState extends CommonAbstractRendererState,
  RightFormState extends CommonAbstractRendererState,
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
    ?.mapContext(
      (
        _: SumAbstractRendererReadonlyContext &
          SumAbstractRendererState<LeftFormState, RightFormState>,
      ): CommonAbstractRendererReadonlyContext<
        DispatchParsedType<any>,
        PredicateValue
      > &
        LeftFormState => ({
        ..._.customFormState.left,
        disabled: _.disabled,
        value: _.value.value.value,
        bindings: _.bindings,
        extraContext: _.extraContext,
        identifiers: {
          withLauncher: _.identifiers.withLauncher.concat(`[left]`),
          withoutLauncher: _.identifiers.withoutLauncher.concat(`[left]`),
        },
        type: _.type.args[0],
      }),
    )
    ?.mapState(
      SumAbstractRendererState<LeftFormState, RightFormState>().Updaters.Core
        .customFormState.children.left,
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
          props.setState(
            SumAbstractRendererState<LeftFormState, RightFormState>()
              .Updaters.Core.commonFormState(
                DispatchCommonFormState.Updaters.modifiedByUser(
                  replaceWith(true),
                ),
              )
              .then(
                SumAbstractRendererState<
                  LeftFormState,
                  RightFormState
                >().Updaters.Core.customFormState.children.left((_) => ({
                  ..._,
                  commonFormState:
                    DispatchCommonFormState.Updaters.modifiedByUser(
                      replaceWith(true),
                    )(_.commonFormState),
                })),
              ),
          );
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
      SumAbstractRendererState<LeftFormState, RightFormState>().Updaters.Core
        .customFormState.children.right,
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
          props.setState(
            SumAbstractRendererState<LeftFormState, RightFormState>()
              .Updaters.Core.commonFormState(
                DispatchCommonFormState.Updaters.modifiedByUser(
                  replaceWith(true),
                ),
              )
              .then(
                SumAbstractRendererState<
                  LeftFormState,
                  RightFormState
                >().Updaters.Core.customFormState.children.right((_) => ({
                  ..._,
                  commonFormState:
                    DispatchCommonFormState.Updaters.modifiedByUser(
                      replaceWith(true),
                    )(_.commonFormState),
                })),
              ),
          );
        },
      }),
    );

  return Template.Default<
    SumAbstractRendererReadonlyContext,
    SumAbstractRendererState<LeftFormState, RightFormState>,
    ForeignMutationsExpected & {
      onChange: DispatchOnChange<ValueSum>;
    },
    SumAbstractRendererView<
      LeftFormState,
      RightFormState,
      SumAbstractRendererReadonlyContext,
      ForeignMutationsExpected
    >
  >((props) => {
    if (!PredicateValue.Operations.IsSum(props.context.value)) {
      console.error(
        `Sum expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering sum field\n...${
          props.context.identifiers.withLauncher
        }`,
      );
      return (
        <p>
          {props.context.label && `${props.context.label}: `}RENDER ERROR: Sum
          value expected for sum but got something else
        </p>
      );
    }

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
          embeddedLeftTemplate={
            props.context.value.value.kind == "l"
              ? embeddedLeftTemplate
              : undefined
          }
          embeddedRightTemplate={
            props.context.value.value.kind == "r"
              ? embeddedRightTemplate
              : undefined
          }
        />
      </span>
    );
  }).any([]);
};

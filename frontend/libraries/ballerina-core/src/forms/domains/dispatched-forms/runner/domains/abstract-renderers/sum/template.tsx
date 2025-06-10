import {
  CommonAbstractRendererReadonlyContext,
  CommonAbstractRendererState,
  DispatchCommonFormState,
  DispatchDelta,
  IdWrapperProps,
  PredicateValue,
  replaceWith,
  Sum,
  Value,
  ValueSum,
  DispatchOnChange,
  ErrorRendererProps,
  getLeafIdentifierFromIdentifier,
} from "../../../../../../../../main";
import { Template } from "../../../../../../../../main";
import { DispatchParsedType } from "../../../../deserializer/domains/specification/domains/types/state";
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
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
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
        ..._,
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
            isWholeEntityMutation: false,
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
      ..._,
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
            isWholeEntityMutation: false,
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
        <ErrorRenderer
          message={`${getLeafIdentifierFromIdentifier(
            props.context.identifiers.withoutLauncher,
          )}: Sum value expected for sum but got ${JSON.stringify(
            props.context.value,
          )}`}
        />
      );
    }

    return (
      <>
        <IdProvider domNodeId={props.context.identifiers.withoutLauncher}>
          <props.view
            {...props}
            context={{
              ...props.context,
              domNodeId: props.context.identifiers.withoutLauncher,
            }}
            foreignMutations={{
              ...props.foreignMutations,
            }}
            embeddedLeftTemplate={embeddedLeftTemplate}
            embeddedRightTemplate={embeddedRightTemplate}
          />
        </IdProvider>
      </>
    );
  }).any([]);
};

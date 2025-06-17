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
  Option,
  Unit,
  VoidCallbackWithOptionalFlags,
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
  Flags = Unit,
>(
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
  leftTemplate?: Template<
    Value<PredicateValue> &
      LeftFormState & { disabled: boolean; extraContext: any },
    LeftFormState,
    {
      onChange: DispatchOnChange<PredicateValue, Flags>;
    }
  >,
  rightTemplate?: Template<
    Value<PredicateValue> &
      RightFormState & { disabled: boolean; extraContext: any },
    RightFormState,
    {
      onChange: DispatchOnChange<PredicateValue, Flags>;
    }
  >,
) => {
  const embeddedLeftTemplate = leftTemplate ?
  (flags: Flags | undefined) => leftTemplate
    .mapContext(
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
    .mapState(
      SumAbstractRendererState<LeftFormState, RightFormState>().Updaters.Core
        .customFormState.children.left,
    )
    .mapForeignMutationsFromProps<
      ForeignMutationsExpected & {
        onChange: DispatchOnChange<ValueSum, Flags>;
      }
    >(
      (
        props,
      ): {
        onChange: DispatchOnChange<PredicateValue, Flags>;
      } => ({
        onChange: (elementUpdater, nestedDelta) => {
          const delta: DispatchDelta<Flags> = {
            kind: "SumLeft",
            value: nestedDelta,
            flags: flags,
          };
          props.foreignMutations.onChange(
            elementUpdater.kind == "l"
              ? Option.Default.none()
              : Option.Default.some((_) => ({
                  ..._,
                  value: Sum.Updaters.left<PredicateValue, PredicateValue>(
                    elementUpdater.value,
                  )(_.value),
                })),
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
    ) : undefined

  const embeddedRightTemplate = rightTemplate ? (flags: Flags | undefined) => rightTemplate
    .mapContext((_: any): any => ({
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
        onChange: DispatchOnChange<ValueSum, Flags>;
      }
    >(
      (
        props,
      ): ForeignMutationsExpected & {
        onChange: DispatchOnChange<PredicateValue, Flags>;
      } => ({
        ...props.foreignMutations,
        onChange: (elementUpdater, nestedDelta) => {
          const delta: DispatchDelta<Flags> = {
            kind: "SumRight",
            value: nestedDelta,
            flags,
          };
          props.foreignMutations.onChange(
            elementUpdater.kind == "l"
              ? Option.Default.none()
              : Option.Default.some((_) => ({
                  ..._,
                  value: Sum.Updaters.right<PredicateValue, PredicateValue>(
                    elementUpdater.value,
                  )(_.value),
                })),
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
    ) : undefined

  return Template.Default<
    SumAbstractRendererReadonlyContext,
    SumAbstractRendererState<LeftFormState, RightFormState>,
    ForeignMutationsExpected & {
      onChange: DispatchOnChange<ValueSum, Flags>;
    },
    SumAbstractRendererView<
      LeftFormState,
      RightFormState,
      SumAbstractRendererReadonlyContext,
      ForeignMutationsExpected,
      Flags
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

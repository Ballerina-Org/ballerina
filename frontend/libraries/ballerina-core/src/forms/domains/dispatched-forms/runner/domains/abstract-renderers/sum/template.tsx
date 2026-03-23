import {
  CommonAbstractRendererReadonlyContext,
  CommonAbstractRendererState,
  DispatchCommonFormState,
  IdWrapperProps,
  PredicateValue,
  replaceWith,
  Sum,
  DispatchOnChange,
  ErrorRendererProps,
  Option,
  Unit,
  CommonAbstractRendererForeignMutationsExpected,
  NestedRenderer,
  ValueOrErrors,
} from "../../../../../../../../main";
import { Template } from "../../../../../../../../main";
import { DispatchParsedType } from "../../../../deserializer/domains/specification/domains/types/state";
import {
  SumAbstractRendererForeignMutationsExpected,
  SumAbstractRendererReadonlyContext,
  SumAbstractRendererState,
  SumAbstractRendererView,
} from "./state";
import { DispatchDelta } from "../../deltas/dispatch-delta/state";

export const SumAbstractRenderer = <
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
>(
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
  defaultLeftValue: ValueOrErrors<PredicateValue, string>,
  defaultRightValue: ValueOrErrors<PredicateValue, string>,
  leftTemplate?: Template<
    CommonAbstractRendererReadonlyContext<
      DispatchParsedType<any>,
      PredicateValue,
      CustomPresentationContext,
      ExtraContext
    > &
      CommonAbstractRendererState,
    CommonAbstractRendererState,
    CommonAbstractRendererForeignMutationsExpected<Flags>
  >,
  rightTemplate?: Template<
    CommonAbstractRendererReadonlyContext<
      DispatchParsedType<any>,
      PredicateValue,
      CustomPresentationContext,
      ExtraContext
    > &
      CommonAbstractRendererState,
    CommonAbstractRendererState,
    CommonAbstractRendererForeignMutationsExpected<Flags>
  >,
  LeftRenderer?: NestedRenderer<any>,
  RightRenderer?: NestedRenderer<any>,
) => {
  const embeddedLeftTemplate =
    leftTemplate && LeftRenderer
      ? (flags: Flags | undefined) =>
          leftTemplate
            .mapContext(
              (
                _: SumAbstractRendererReadonlyContext<
                  CustomPresentationContext,
                  ExtraContext
                > &
                  SumAbstractRendererState,
              ) => {
                const labelContext =
                  CommonAbstractRendererState.Operations.GetLabelContext(
                    _.labelContext,
                    LeftRenderer,
                  );
                return {
                  ..._.customFormState.left,
                  disabled: _.disabled || _.globallyDisabled,
                  globallyDisabled: _.globallyDisabled,
                  locked: _.locked,
                  value: PredicateValue.Operations.IsUnit(_.value)
                    ? _.value
                    : _.value.value.value,
                  bindings: _.bindings,
                  readOnly: _.readOnly || _.globallyReadOnly,
                  globallyReadOnly: _.globallyReadOnly,
                  extraContext: _.extraContext,
                  type: _.type.args[0],
                  customPresentationContext: _.customPresentationContext,
                  remoteEntityVersionIdentifier:
                    _.remoteEntityVersionIdentifier,
                  domNodeAncestorPath: _.domNodeAncestorPath + "[Value]",
                  legacy_domNodeAncestorPath:
                    _.legacy_domNodeAncestorPath + "[sum][left]",
                  predictionAncestorPath: _.predictionAncestorPath + "[Value]",
                  layoutAncestorPath: _.layoutAncestorPath + "[sum][left]",
                  typeAncestors: [_.type as DispatchParsedType<any>].concat(
                    _.typeAncestors,
                  ),
                  lookupTypeAncestorNames: _.lookupTypeAncestorNames,
                  preprocessedSpecContext: _.preprocessedSpecContext,
                  labelContext,
                  usePreprocessor: _.usePreprocessor,
                  preventOneInitialization: _.preventOneInitialization,
                };
              },
            )
            .mapState(
              SumAbstractRendererState.Updaters.Core.customFormState.children
                .left,
            )
            .mapForeignMutationsFromProps<
              SumAbstractRendererForeignMutationsExpected<Flags>
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
                    sourceAncestorLookupTypeNames:
                      nestedDelta.sourceAncestorLookupTypeNames,
                  };
                  props.foreignMutations.onChange(
                    elementUpdater.kind == "l"
                      ? Option.Default.none()
                      : Option.Default.some((_) => ({
                          ..._,
                          value: Sum.Updaters.left<
                            PredicateValue,
                            PredicateValue
                          >(elementUpdater.value)(_.value),
                        })),
                    delta,
                  );
                  props.setState(
                    SumAbstractRendererState.Updaters.Core.commonFormState(
                      DispatchCommonFormState.Updaters.modifiedByUser(
                        replaceWith(true),
                      ),
                    ).then(
                      SumAbstractRendererState.Updaters.Core.customFormState.children.left(
                        (_) => ({
                          ..._,
                          commonFormState:
                            DispatchCommonFormState.Updaters.modifiedByUser(
                              replaceWith(true),
                            )(_.commonFormState),
                        }),
                      ),
                    ),
                  );
                },
              }),
            )
      : undefined;

  const embeddedRightTemplate =
    rightTemplate && RightRenderer
      ? (flags: Flags | undefined) =>
          rightTemplate
            .mapContext(
              (
                _: SumAbstractRendererReadonlyContext<
                  CustomPresentationContext,
                  ExtraContext
                > &
                  SumAbstractRendererState,
              ) => {
                const labelContext =
                  CommonAbstractRendererState.Operations.GetLabelContext(
                    _.labelContext,
                    RightRenderer,
                  );
                return {
                  ..._.customFormState.right,
                  disabled: _.disabled || _.globallyDisabled,
                  globallyDisabled: _.globallyDisabled,
                  locked: _.locked,
                  value: PredicateValue.Operations.IsUnit(_.value)
                    ? _.value
                    : _.value.value.value,
                  bindings: _.bindings,
                  readOnly: _.readOnly || _.globallyReadOnly,
                  globallyReadOnly: _.globallyReadOnly,
                  extraContext: _.extraContext,
                  type: _.type.args[1],
                  customPresentationContext: _.customPresentationContext,
                  remoteEntityVersionIdentifier:
                    _.remoteEntityVersionIdentifier,
                  domNodeAncestorPath: _.domNodeAncestorPath + "[Value]",
                  legacy_domNodeAncestorPath:
                    _.legacy_domNodeAncestorPath + "[sum][right]",
                  predictionAncestorPath: _.predictionAncestorPath + "[Value]",
                  layoutAncestorPath: _.layoutAncestorPath + "[sum][right]",
                  typeAncestors: [_.type as DispatchParsedType<any>].concat(
                    _.typeAncestors,
                  ),
                  lookupTypeAncestorNames: _.lookupTypeAncestorNames,
                  preprocessedSpecContext: _.preprocessedSpecContext,
                  labelContext,
                  usePreprocessor: _.usePreprocessor,
                  preventOneInitialization: _.preventOneInitialization,
                };
              },
            )
            .mapState(
              SumAbstractRendererState.Updaters.Core.customFormState.children
                .right,
            )
            .mapForeignMutationsFromProps<
              SumAbstractRendererForeignMutationsExpected<Flags>
            >((props) => ({
              onChange: (elementUpdater, nestedDelta) => {
                const delta: DispatchDelta<Flags> = {
                  kind: "SumRight",
                  value: nestedDelta,
                  flags,
                  sourceAncestorLookupTypeNames:
                    nestedDelta.sourceAncestorLookupTypeNames,
                };
                props.foreignMutations.onChange(
                  elementUpdater.kind == "l"
                    ? Option.Default.none()
                    : Option.Default.some((_) => ({
                        ..._,
                        value: Sum.Updaters.right<
                          PredicateValue,
                          PredicateValue
                        >(elementUpdater.value)(_.value),
                      })),
                  delta,
                );
                props.setState(
                  SumAbstractRendererState.Updaters.Core.commonFormState(
                    DispatchCommonFormState.Updaters.modifiedByUser(
                      replaceWith(true),
                    ),
                  ).then(
                    SumAbstractRendererState.Updaters.Core.customFormState.children.right(
                      (_) => ({
                        ..._,
                        commonFormState:
                          DispatchCommonFormState.Updaters.modifiedByUser(
                            replaceWith(true),
                          )(_.commonFormState),
                      }),
                    ),
                  ),
                );
              },
            }))
      : undefined;

  return Template.Default<
    SumAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    > &
      CommonAbstractRendererState,
    SumAbstractRendererState,
    SumAbstractRendererForeignMutationsExpected<Flags>,
    SumAbstractRendererView<CustomPresentationContext, Flags, ExtraContext>
  >((props) => {
    const domNodeId = props.context.domNodeAncestorPath;
    const legacy_domNodeId = props.context.legacy_domNodeAncestorPath + "[sum]";

    if (
      !PredicateValue.Operations.IsSum(props.context.value) &&
      !PredicateValue.Operations.IsUnit(props.context.value)
    ) {
      console.error(
        `Sum or unit value expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering \n...${domNodeId}`,
      );
      return (
        <ErrorRenderer
          message={`${domNodeId}: Sum or unit value expected but got ${JSON.stringify(
            props.context.value,
          )}`}
        />
      );
    }

    return (
      <>
        <IdProvider
          domNodeId={
            props.context.usePreprocessor ? domNodeId : legacy_domNodeId
          }
        >
          <props.view
            {...props}
            context={{
              ...props.context,
              domNodeId,
              legacy_domNodeId,
            }}
            foreignMutations={{
              ...props.foreignMutations,
              toLeft: (value, flags) => {
                const leftValue = PredicateValue.Default.sum(
                  Sum.Default.left(value),
                );
                props.foreignMutations.onChange(
                  Option.Default.some(replaceWith(leftValue)),
                  {
                    kind: "SumReplace",
                    replace: leftValue,
                    flags: flags,
                    sourceAncestorLookupTypeNames:
                      props.context.lookupTypeAncestorNames,
                    state: {
                      commonFormState: props.context.commonFormState,
                      customFormState: props.context.customFormState,
                    },
                    type: props.context.type.args[0],
                  },
                );
              },
              toRight: (value, flags) => {
                const rightValue = PredicateValue.Default.sum(
                  Sum.Default.right(value),
                );
                props.foreignMutations.onChange(
                  Option.Default.some(replaceWith(rightValue)),
                  {
                    kind: "SumReplace",
                    replace: rightValue,
                    flags: flags,
                    sourceAncestorLookupTypeNames:
                      props.context.lookupTypeAncestorNames,
                    state: {
                      commonFormState: props.context.commonFormState,
                      customFormState: props.context.customFormState,
                    },
                    type: props.context.type.args[1],
                  },
                );
              },
            }}
            embeddedLeftTemplate={embeddedLeftTemplate}
            embeddedRightTemplate={embeddedRightTemplate}
            defaultLeftValue={
              defaultLeftValue.kind == "value"
                ? () => defaultLeftValue.value
                : undefined
            }
            defaultRightValue={
              defaultRightValue.kind == "value"
                ? () => defaultRightValue.value
                : undefined
            }
          />
        </IdProvider>
      </>
    );
  }).any([]);
};

import {
  CommonAbstractRendererReadonlyContext,
  CommonAbstractRendererState,
  DispatchCommonFormState,
  DispatchDelta,
  IdWrapperProps,
  PredicateValue,
  replaceWith,
  Sum,
  DispatchOnChange,
  ErrorRendererProps,
  Option,
  Unit,
  CommonAbstractRendererForeignMutationsExpected,
} from "../../../../../../../../main";
import { Template } from "../../../../../../../../main";
import { DispatchParsedType } from "../../../../deserializer/domains/specification/domains/types/state";
import {
  SumAbstractRendererForeignMutationsExpected,
  SumAbstractRendererReadonlyContext,
  SumAbstractRendererState,
  SumAbstractRendererView,
} from "./state";

export const SumAbstractRenderer = <
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
>(
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
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
) => {
  const embeddedLeftTemplate = leftTemplate
    ? (flags: Flags | undefined) =>
        leftTemplate
          .mapContext(
            (
              _: SumAbstractRendererReadonlyContext<
                CustomPresentationContext,
                ExtraContext
              > &
                SumAbstractRendererState,
            ) => ({
              ..._.customFormState.left,
              disabled: _.disabled,
              locked: _.locked,
              value: _.value.value.value,
              bindings: _.bindings,
              extraContext: _.extraContext,
              type: _.type.args[0],
              customPresentationContext: _.customPresentationContext,
              remoteEntityVersionIdentifier: _.remoteEntityVersionIdentifier,
              domNodeAncestorPath: _.domNodeAncestorPath + "[sum][left]",
              typeAncestors: [_.type as DispatchParsedType<any>].concat(
                _.typeAncestors,
              ),
            }),
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

  const embeddedRightTemplate = rightTemplate
    ? (flags: Flags | undefined) =>
        rightTemplate
          .mapContext(
            (
              _: SumAbstractRendererReadonlyContext<
                CustomPresentationContext,
                ExtraContext
              > &
                SumAbstractRendererState,
            ) => ({
              ..._.customFormState.right,
              disabled: _.disabled,
              locked: _.locked,
              value: _.value.value.value,
              bindings: _.bindings,
              extraContext: _.extraContext,
              type: _.type.args[1],
              customPresentationContext: _.customPresentationContext,
              remoteEntityVersionIdentifier: _.remoteEntityVersionIdentifier,
              domNodeAncestorPath: _.domNodeAncestorPath + "[sum][right]",
              typeAncestors: [_.type as DispatchParsedType<any>].concat(
                _.typeAncestors,
              ),
            }),
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
    const domNodeId = props.context.domNodeAncestorPath + "[sum]";

    if (!PredicateValue.Operations.IsSum(props.context.value)) {
      console.error(
        `Sum expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering \n...${domNodeId}`,
      );
      return (
        <ErrorRenderer
          message={`${domNodeId}: Sum value expected but got ${JSON.stringify(
            props.context.value,
          )}`}
        />
      );
    }

    return (
      <>
        <IdProvider domNodeId={domNodeId}>
          <props.view
            {...props}
            context={{
              ...props.context,
              domNodeId,
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

import {
  BoolAbstractRendererForeignMutationsExpected,
  BoolAbstractRendererReadonlyContext,
  BoolAbstractRendererView,
} from "./state";
import { Template } from "../../../../../../../template/state";
import {
  DispatchDelta,
  IdWrapperProps,
  PredicateValue,
  ErrorRendererProps,
  Option,
  Unit,
  StringSerializedType,
} from "../../../../../../../../main";
import { replaceWith } from "../../../../../../../../main";
import { BoolAbstractRendererState } from "./state";

export const BoolAbstractRenderer = <
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
>(
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) => {
  return Template.Default<
    BoolAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    > &
      BoolAbstractRendererState,
    BoolAbstractRendererState,
    BoolAbstractRendererForeignMutationsExpected<Flags>,
    BoolAbstractRendererView<CustomPresentationContext, Flags, ExtraContext>
  >((props) => {
    const domNodeId = props.context.domNodeAncestorPath + "[boolean]";

    if (!PredicateValue.Operations.IsBoolean(props.context.value)) {
      console.error(
        `Boolean expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering \n...${domNodeId}`,
      );
      return (
        <ErrorRenderer
          message={`${domNodeId}: Boolean expected but got ${JSON.stringify(
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
              setNewValue: (value, flags) => {
                const delta: DispatchDelta<Flags> = {
                  kind: "BoolReplace",
                  replace: value,
                  state: {
                    commonFormState: props.context.commonFormState,
                    customFormState: props.context.customFormState,
                  },
                  type: props.context.type,
                  flags,
                };
                props.foreignMutations.onChange(
                  Option.Default.some(replaceWith(value)),
                  delta,
                );
              },
            }}
          />
        </IdProvider>
      </>
    );
  }).any([]);
};

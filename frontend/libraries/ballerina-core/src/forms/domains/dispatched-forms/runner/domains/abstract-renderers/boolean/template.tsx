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
import { useRegistryValueAtPath } from "../registry-store";

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
    const value = useRegistryValueAtPath(props.context.path);
    if (!value) {
      return <></>;
    }
    if (
      !PredicateValue.Operations.IsBoolean(value) &&
      !PredicateValue.Operations.IsUnit(value)
    ) {
      console.error(
        `Boolean or unit value expected but got: ${JSON.stringify(
          value,
        )}\n...When rendering \n...${domNodeId}`,
      );
      return (
        <ErrorRenderer
          message={`${domNodeId}: Boolean or unit value expected but got ${JSON.stringify(
            value,
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
              value,
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
                  sourceAncestorLookupTypeNames:
                    props.context.lookupTypeAncestorNames,
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

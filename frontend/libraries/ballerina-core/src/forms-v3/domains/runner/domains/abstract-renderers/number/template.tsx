import {
  DispatchDelta,
  IdWrapperProps,
  PredicateValue,
  replaceWith,
  Template,
  ErrorRendererProps,
  Option,
  Unit,
  StringSerializedType,
} from "../../../../../../../../main";
import {
  NumberAbstractRendererForeignMutationsExpected,
  NumberAbstractRendererReadonlyContext,
  NumberAbstractRendererState,
  NumberAbstractRendererView,
} from "./state";
import { useRegistryValueAtPath } from "../registry-store";

export const NumberAbstractRenderer = <
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
>(
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) => {
  return Template.Default<
    NumberAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    > &
      NumberAbstractRendererState,
    NumberAbstractRendererState,
    NumberAbstractRendererForeignMutationsExpected<Flags>,
    NumberAbstractRendererView<CustomPresentationContext, Flags, ExtraContext>
  >((props) => {
    const domNodeId = props.context.domNodeAncestorPath + "[number]";

    const value = useRegistryValueAtPath(props.context.path);
    if (!value) {
      return <></>;
    }
    if (!PredicateValue.Operations.IsNumber(value)) {
      console.error(
        `Number expected but got: ${JSON.stringify(
          value,
        )}\n...When rendering \n...${domNodeId}`,
      );
      return (
        <ErrorRenderer
          message={`${domNodeId}: Number value expected but got ${JSON.stringify(
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
                  kind: "NumberReplace",
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

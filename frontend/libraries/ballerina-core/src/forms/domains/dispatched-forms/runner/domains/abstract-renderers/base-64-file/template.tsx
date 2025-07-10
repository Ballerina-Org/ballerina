import {
  DispatchDelta,
  IdWrapperProps,
  PredicateValue,
  ErrorRendererProps,
  Option,
  Unit,
  StringSerializedType,
} from "../../../../../../../../main";
import { replaceWith, Template } from "../../../../../../../../main";
import {
  Base64FileAbstractRendererForeignMutationsExpected,
  Base64FileAbstractRendererReadonlyContext,
  Base64FileAbstractRendererState,
  Base64FileAbstractRendererView,
} from "./state";

export const Base64FileAbstractRenderer = <
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
>(
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) => {
  return Template.Default<
    Base64FileAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    > &
      Base64FileAbstractRendererState,
    Base64FileAbstractRendererState,
    Base64FileAbstractRendererForeignMutationsExpected<Flags>,
    Base64FileAbstractRendererView<
      CustomPresentationContext,
      Flags,
      ExtraContext
    >
  >((props) => {
    const domNodeId = props.context.domNodeAncestorPath + "[base64File]";

    if (!PredicateValue.Operations.IsString(props.context.value)) {
      console.error(
        `String expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering \n...${domNodeId}`,
      );
      return (
        <ErrorRenderer
          message={`${domNodeId}: String expected but got ${JSON.stringify(
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
                  kind: "StringReplace",
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

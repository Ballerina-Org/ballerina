import { Template } from "../../../../../../../template/state";
import {
  DispatchDelta,
  IdWrapperProps,
  PredicateValue,
  replaceWith,
  ErrorRendererProps,
  Option,
  Unit,
  StringSerializedType,
} from "../../../../../../../../main";
import {
  DateAbstractRendererForeignMutationsExpected,
  DateAbstractRendererReadonlyContext,
  DateAbstractRendererState,
  DateAbstractRendererView,
} from "./state";

export const DateAbstractRenderer = <
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
>(
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
  SerializedType: StringSerializedType,
) => {
  return Template.Default<
    DateAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    > &
      DateAbstractRendererState,
    DateAbstractRendererState,
    DateAbstractRendererForeignMutationsExpected<Flags>,
    DateAbstractRendererView<CustomPresentationContext, Flags, ExtraContext>
  >((props) => {
    const completeSerializedTypeHierarchy = [SerializedType].concat(
      props.context.serializedTypeHierarchy,
    );

    const domNodeId = props.context.domNodeAncestorPath + "[date]";

    if (!PredicateValue.Operations.IsDate(props.context.value)) {
      console.error(
        `Date expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering \n...${domNodeId}`,
      );
      return (
        <ErrorRenderer
          message={`${domNodeId}: Date value expected but got ${JSON.stringify(
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
              completeSerializedTypeHierarchy,
            }}
            foreignMutations={{
              ...props.foreignMutations,
              setNewValue: (value, flags) => {
                props.setState(
                  DateAbstractRendererState.Updaters.Core.customFormState.children.possiblyInvalidInput(
                    replaceWith(value),
                  ),
                );
                const newValue = value == undefined ? value : new Date(value);

                if (!(newValue == undefined || isNaN(newValue.getTime()))) {
                  const delta: DispatchDelta<Flags> = {
                    kind: "TimeReplace",
                    replace: newValue.toISOString(),
                    state: {
                      commonFormState: props.context.commonFormState,
                      customFormState: props.context.customFormState,
                    },
                    type: props.context.type,
                    flags,
                  };
                  setTimeout(() => {
                    props.foreignMutations.onChange(
                      Option.Default.some(replaceWith(newValue)),
                      delta,
                    );
                  }, 0);
                }
              },
            }}
          />
        </IdProvider>
      </>
    );
  }).any([]);
};

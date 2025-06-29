import { Template } from "../../../../../../../template/state";
import {
  DispatchDelta,
  IdWrapperProps,
  PredicateValue,
  replaceWith,
  ErrorRendererProps,
  getLeafIdentifierFromIdentifier,
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
>(
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
  SerializedType: StringSerializedType,
) => {
  return Template.Default<
    DateAbstractRendererReadonlyContext<CustomPresentationContext> &
      DateAbstractRendererState,
    DateAbstractRendererState,
    DateAbstractRendererForeignMutationsExpected<Flags>,
    DateAbstractRendererView<CustomPresentationContext, Flags>
  >((props) => {
    if (!PredicateValue.Operations.IsDate(props.context.value)) {
      console.error(
        `Date expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering date field\n...${
          props.context.identifiers.withLauncher
        }`,
      );
      return (
        <ErrorRenderer
          message={`${getLeafIdentifierFromIdentifier(
            props.context.identifiers.withoutLauncher,
          )}: Date value expected for date but got ${JSON.stringify(
            props.context.value,
          )}`}
        />
      );
    }

    const serializedTypeHierarchy = [SerializedType].concat(
      props.context.serializedTypeHierarchy,
    );

    return (
      <>
        <IdProvider domNodeId={props.context.identifiers.withoutLauncher}>
          <props.view
            {...props}
            context={{
              ...props.context,
              serializedTypeHierarchy,
              domNodeId: props.context.identifiers.withoutLauncher,
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

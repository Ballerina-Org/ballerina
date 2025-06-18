import {
  DispatchDelta,
  IdWrapperProps,
  PredicateValue,
  ErrorRendererProps,
  getLeafIdentifierFromIdentifier,
  Option,
  Unit,
} from "../../../../../../../../main";
import { replaceWith, Template } from "../../../../../../../../main";
import {
  Base64FileAbstractRendererForeignMutationsExpected,
  Base64FileAbstractRendererReadonlyContext,
  Base64FileAbstractRendererState,
  Base64FileAbstractRendererView,
} from "./state";

export const Base64FileAbstractRenderer = <
  CustomContext = Unit,
  Flags = Unit,
>(
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) => {
  return Template.Default<
    Base64FileAbstractRendererReadonlyContext<CustomContext> &
      Base64FileAbstractRendererState,
    Base64FileAbstractRendererState,
    Base64FileAbstractRendererForeignMutationsExpected<Flags>,
    Base64FileAbstractRendererView<CustomContext, Flags>
  >((props) => {
    if (!PredicateValue.Operations.IsString(props.context.value)) {
      console.error(
        `String expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering base64 field\n...${
          props.context.identifiers.withLauncher
        }`,
      );
      return (
        <ErrorRenderer
          message={`${getLeafIdentifierFromIdentifier(
            props.context.identifiers.withoutLauncher,
          )}: String expected for base64 but got ${JSON.stringify(
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

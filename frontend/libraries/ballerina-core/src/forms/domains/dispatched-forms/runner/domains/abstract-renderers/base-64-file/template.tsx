import {
  DispatchDelta,
  FormLabel,
  IdWrapperProps,
  PredicateValue,
  Value,
  DispatchOnChange,
  ErrorRendererProps,
  getLeafIdentifierFromIdentifier,
} from "../../../../../../../../main";
import { replaceWith, Template } from "../../../../../../../../main";
import { DispatchParsedType } from "../../../../deserializer/domains/specification/domains/types/state";
import {
  Base64FileAbstractRendererState,
  Base64FileAbstractRendererView,
} from "./state";

export const Base64FileAbstractRenderer = <
  Context extends FormLabel,
  ForeignMutationsExpected,
>(
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) => {
  return Template.Default<
    Context &
      Value<string> & {
        disabled: boolean;
        type: DispatchParsedType<any>;
        identifiers: { withLauncher: string; withoutLauncher: string };
      },
    Base64FileAbstractRendererState,
    ForeignMutationsExpected & { onChange: DispatchOnChange<string> },
    Base64FileAbstractRendererView<Context, ForeignMutationsExpected>
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
        <IdProvider
          id={`${props.context.identifiers.withLauncher} ${props.context.identifiers.withoutLauncher}`}
        />
        <props.view
          {...props}
          foreignMutations={{
            ...props.foreignMutations,
            setNewValue: (_) => {
              const delta: DispatchDelta = {
                kind: "StringReplace",
                replace: _,
                state: {
                  commonFormState: props.context.commonFormState,
                  customFormState: props.context.customFormState,
                },
                type: props.context.type,
                isWholeEntityMutation: false,
              };
              props.foreignMutations.onChange(replaceWith(_), delta);
            },
          }}
        />
      </>
    );
  }).any([]);
};

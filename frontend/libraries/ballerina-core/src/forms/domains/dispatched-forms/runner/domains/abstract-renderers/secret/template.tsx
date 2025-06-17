import { List } from "immutable";
import {
  SecretAbstractRendererState,
  SecretAbstractRendererView,
} from "./state";
import {
  DispatchDelta,
  FormLabel,
  IdWrapperProps,
  PredicateValue,
  replaceWith,
  Template,
  Value,
  DispatchOnChange,
  ErrorRendererProps,
  getLeafIdentifierFromIdentifier,
  Option,
  Unit,
} from "../../../../../../../../main";
import { DispatchParsedType } from "../../../../deserializer/domains/specification/domains/types/state";

export const SecretAbstractRenderer = <
  Context extends FormLabel & {
    identifiers: { withLauncher: string; withoutLauncher: string };
  },
  ForeignMutationsExpected,
  Flags = Unit,
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
    SecretAbstractRendererState,
    ForeignMutationsExpected & { onChange: DispatchOnChange<string, Flags> },
    SecretAbstractRendererView<Context, ForeignMutationsExpected, Flags>
  >((props) => {
    if (!PredicateValue.Operations.IsString(props.context.value)) {
      console.error(
        `String expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering secret field\n...${
          props.context.identifiers.withLauncher
        }`,
      );
      return (
        <ErrorRenderer
          message={`${getLeafIdentifierFromIdentifier(
            props.context.identifiers.withoutLauncher,
          )}: String value expected for secret but got ${JSON.stringify(
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
              setNewValue: (_, flags) => {
                const delta: DispatchDelta<Flags> = {
                  kind: "StringReplace",
                  replace: _,
                  state: {
                    commonFormState: props.context.commonFormState,
                    customFormState: props.context.customFormState,
                  },
                  type: props.context.type,
                  flags,
                };
                props.foreignMutations.onChange(
                  Option.Default.some(replaceWith(_)),
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

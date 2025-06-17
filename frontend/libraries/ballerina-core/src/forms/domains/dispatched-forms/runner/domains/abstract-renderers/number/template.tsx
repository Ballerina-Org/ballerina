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
import {
  NumberAbstractRendererState,
  NumberAbstractRendererView,
} from "./state";
import { DispatchParsedType } from "../../../../deserializer/domains/specification/domains/types/state";

export const NumberAbstractRenderer = <
  Context extends FormLabel,
  ForeignMutationsExpected,
  Flags = Unit,
>(
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) => {
  return Template.Default<
    Context &
      Value<number> & {
        disabled: boolean;
        type: DispatchParsedType<any>;
        identifiers: { withLauncher: string; withoutLauncher: string };
      },
    NumberAbstractRendererState,
    ForeignMutationsExpected & { onChange: DispatchOnChange<number, Flags> },
    NumberAbstractRendererView<Context, ForeignMutationsExpected, Flags>
  >((props) => {
    if (!PredicateValue.Operations.IsNumber(props.context.value)) {
      console.error(
        `Number expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering number field\n...${
          props.context.identifiers.withLauncher
        }`,
      );
      return (
        <ErrorRenderer
          message={`${getLeafIdentifierFromIdentifier(
            props.context.identifiers.withoutLauncher,
          )}: Number value expected for number but got ${JSON.stringify(
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
                  kind: "NumberReplace",
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

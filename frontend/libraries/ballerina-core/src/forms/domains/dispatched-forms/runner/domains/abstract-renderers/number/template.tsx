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
} from "../../../../../../../../main";
import {
  NumberAbstractRendererState,
  NumberAbstractRendererView,
} from "./state";
import { DispatchParsedType } from "../../../../deserializer/domains/specification/domains/types/state";

export const NumberAbstractRenderer = <
  Context extends FormLabel,
  ForeignMutationsExpected,
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
    ForeignMutationsExpected & { onChange: DispatchOnChange<number> },
    NumberAbstractRendererView<Context, ForeignMutationsExpected>
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
        <IdProvider
          id={`${props.context.identifiers.withLauncher} ${props.context.identifiers.withoutLauncher}`}
        />
        <props.view
          {...props}
          foreignMutations={{
            ...props.foreignMutations,
            setNewValue: (_) => {
              const delta: DispatchDelta = {
                kind: "NumberReplace",
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

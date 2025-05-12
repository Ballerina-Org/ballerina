import {
  DispatchDelta,
  FormLabel,
  IdWrapperProps,
  PredicateValue,
  replaceWith,
  Template,
  Value,
  DispatchOnChange,
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
  IdWrapper: (props: IdWrapperProps) => React.ReactNode,
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
        <p>
          {props.context.label && `${props.context.label}: `}RENDER ERROR:
          Number value expected for number but got something else
        </p>
      );
    }
    return (
      <IdWrapper
        id={`${props.context.identifiers.withLauncher} ${props.context.identifiers.withoutLauncher}`}
      >
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
              };
              props.foreignMutations.onChange(replaceWith(_), delta);
            },
          }}
        />
      </IdWrapper>
    );
  }).any([]);
};

import { BoolAbstractRendererView } from "./state";
import { Template } from "../../../../../../../template/state";
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
import { replaceWith } from "../../../../../../../../main";
import { DispatchParsedType } from "../../../../deserializer/domains/specification/domains/types/state";
import { BoolAbstractRendererState } from "./state";

export const BoolAbstractRenderer = <
  Context extends FormLabel,
  ForeignMutationsExpected,
>(
  IdWrapper: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) => {
  return Template.Default<
    Context &
      Value<boolean> & {
        disabled: boolean;
        type: DispatchParsedType<any>;
        identifiers: { withLauncher: string; withoutLauncher: string };
      },
    BoolAbstractRendererState,
    ForeignMutationsExpected & { onChange: DispatchOnChange<boolean> },
    BoolAbstractRendererView<Context, ForeignMutationsExpected>
  >((props) => {
    if (!PredicateValue.Operations.IsBoolean(props.context.value)) {
      console.error(
        `Boolean expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering boolean field\n...${
          props.context.identifiers.withLauncher
        }`,
      );
      return (
        <ErrorRenderer
          message={`${getLeafIdentifierFromIdentifier(
            props.context.identifiers.withoutLauncher,
          )}: Boolean expected for boolean but got ${JSON.stringify(
            props.context.value,
          )}`}
        />
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
                kind: "BoolReplace",
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

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
  Option,
  Unit,
} from "../../../../../../../../main";
import { replaceWith } from "../../../../../../../../main";
import { DispatchParsedType } from "../../../../deserializer/domains/specification/domains/types/state";
import { BoolAbstractRendererState } from "./state";

export const BoolAbstractRenderer = <
  Context extends FormLabel,
  ForeignMutationsExpected,
  Flags = Unit,
>(
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
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
    ForeignMutationsExpected & { onChange: DispatchOnChange<boolean, Flags> },
    BoolAbstractRendererView<Context, ForeignMutationsExpected, Flags>
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
                  kind: "BoolReplace",
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

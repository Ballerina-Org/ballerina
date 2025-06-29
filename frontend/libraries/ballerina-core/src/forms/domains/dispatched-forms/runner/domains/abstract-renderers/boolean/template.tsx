import {
  BoolAbstractRendererForeignMutationsExpected,
  BoolAbstractRendererReadonlyContext,
  BoolAbstractRendererView,
} from "./state";
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
  StringSerializedType,
} from "../../../../../../../../main";
import { replaceWith } from "../../../../../../../../main";
import { BoolAbstractRendererState } from "./state";

export const BoolAbstractRenderer = <
  CustomPresentationContext = Unit,
  Flags = Unit,
>(
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
  SerializedType: StringSerializedType,
) => {
  return Template.Default<
    BoolAbstractRendererReadonlyContext<CustomPresentationContext> &
      BoolAbstractRendererState,
    BoolAbstractRendererState,
    BoolAbstractRendererForeignMutationsExpected<Flags>,
    BoolAbstractRendererView<CustomPresentationContext, Flags>
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

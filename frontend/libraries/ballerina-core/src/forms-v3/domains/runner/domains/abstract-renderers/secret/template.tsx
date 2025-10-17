import {
  SecretAbstractRendererForeignMutationsExpected,
  SecretAbstractRendererReadonlyContext,
  SecretAbstractRendererState,
  SecretAbstractRendererView,
} from "./state";
import {
  DispatchDelta,
  IdWrapperProps,
  PredicateValue,
  replaceWith,
  Template,
  ErrorRendererProps,
  Option,
  Unit,
  useRegistryValueAtPath,
} from "../../../../../../../../main";

export const SecretAbstractRenderer = <
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
>(
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) => {
  return Template.Default<
    SecretAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    >,
    SecretAbstractRendererState,
    SecretAbstractRendererForeignMutationsExpected<Flags>,
    SecretAbstractRendererView<CustomPresentationContext, Flags, ExtraContext>
  >((props) => {
    const domNodeId = props.context.domNodeAncestorPath + "[secret]";
    const value = useRegistryValueAtPath(props.context.path);
    if (!value) {
      return <></>;
    }
    if (!PredicateValue.Operations.IsString(value)) {
      console.error(
        `String expected but got: ${JSON.stringify(
          value,
        )}\n...When rendering \n...${domNodeId}`,
      );
      return (
        <ErrorRenderer
          message={`${domNodeId}: String value expected but got ${JSON.stringify(
            value,
          )}`}
        />
      );
    }

    return (
      <>
        <IdProvider domNodeId={domNodeId}>
          <props.view
            {...props}
            context={{
              ...props.context,
              domNodeId,
              value,
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
                  sourceAncestorLookupTypeNames:
                    props.context.lookupTypeAncestorNames,
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

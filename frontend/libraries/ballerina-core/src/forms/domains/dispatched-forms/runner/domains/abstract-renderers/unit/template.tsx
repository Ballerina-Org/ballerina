import {
  UnitAbstractRendererReadonlyContext,
  UnitAbstractRendererState,
  UnitAbstractRendererView,
  UnitAbstractRendererForeignMutationsExpected,
} from "./state";
import {
  DispatchDelta,
  ErrorRendererProps,
  IdWrapperProps,
  PredicateValue,
  Template,
  Unit,
  Option,
  replaceWith,
  StringSerializedType,
} from "../../../../../../../../main";

export const UnitAbstractRenderer = <
  CustomPresentationContext = Unit,
  Flags = Unit,
  ExtraContext = Unit,
>(
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) =>
  Template.Default<
    UnitAbstractRendererReadonlyContext<
      CustomPresentationContext,
      ExtraContext
    > &
      UnitAbstractRendererState,
    UnitAbstractRendererState,
    UnitAbstractRendererForeignMutationsExpected<Flags>,
    UnitAbstractRendererView<CustomPresentationContext, Flags, ExtraContext>
  >((props) => {
    const domNodeId = props.context.domNodeAncestorPath + "[unit]";

    if (!PredicateValue.Operations.IsUnit(props.context.value)) {
      console.error(
        `Unit expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering \n...${domNodeId}`,
      );
      return (
        <ErrorRenderer
          message={`${domNodeId}: Unit value expected but got ${JSON.stringify(
            props.context.value,
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
            }}
            foreignMutations={{
              ...props.foreignMutations,
              set: (flags: Flags | undefined) => {
                const delta: DispatchDelta<Flags> = {
                  kind: "UnitReplace",
                  replace: PredicateValue.Default.unit(),
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
                  Option.Default.some(
                    replaceWith(PredicateValue.Default.unit()),
                  ),
                  delta,
                );
              },
            }}
          />
        </IdProvider>
      </>
    );
  }).any([]);

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
} from "../../../../../../../../main";
import { useRegistryValueAtPath } from "../registry-store";

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

    const value = useRegistryValueAtPath(props.context.path);
    if (!value) {
      return <></>;
    }
    if (!PredicateValue.Operations.IsUnit(value)) {
      console.error(
        `Unit expected but got: ${JSON.stringify(
          value,
        )}\n...When rendering \n...${domNodeId}`,
      );
      return (
        <ErrorRenderer
          message={`${domNodeId}: Unit value expected but got ${JSON.stringify(
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

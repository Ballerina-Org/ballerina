import {
  UnitAbstractRendererReadonlyContext,
  UnitAbstractRendererState,
  UnitAbstractRendererView,
  UnitAbstractRendererForeignMutationsExpected,
} from "./state";
import {
  DispatchDelta,
  ErrorRendererProps,
  getLeafIdentifierFromIdentifier,
  IdWrapperProps,
  PredicateValue,
  Template,
  Unit,
  Option,
  replaceWith,
} from "../../../../../../../../main";

export const UnitAbstractRenderer = <CustomContext = Unit, Flags = Unit>(
  IdProvider: (props: IdWrapperProps) => React.ReactNode,
  ErrorRenderer: (props: ErrorRendererProps) => React.ReactNode,
) =>
  Template.Default<
    UnitAbstractRendererReadonlyContext<CustomContext> &
      UnitAbstractRendererState,
    UnitAbstractRendererState,
    UnitAbstractRendererForeignMutationsExpected<Flags>,
    UnitAbstractRendererView<CustomContext, Flags>
  >((props) => {
    if (!PredicateValue.Operations.IsUnit(props.context.value)) {
      console.error(
        `Unit expected but got: ${JSON.stringify(
          props.context.value,
        )}\n...When rendering unit field with label: ${JSON.stringify(
          props.context?.label,
        )}`,
      );
      return (
        <ErrorRenderer
          message={`${getLeafIdentifierFromIdentifier(
            props.context.identifiers.withoutLauncher,
          )}: Unit value expected but got ${JSON.stringify(
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
